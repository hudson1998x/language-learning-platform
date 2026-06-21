using System.Reflection;
using System.Reflection.Emit;
using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.DataQL.Tokeniser;
using LLE.Kernel.Security;

namespace LLE.Kernel.Builders;

/// <summary>
/// Builds runtime proxy implementations of repository interfaces using <see cref="System.Reflection.Emit"/>.
/// </summary>
/// <remarks>
/// <para>
/// A "repository interface" is any interface that (directly or transitively) inherits
/// <see cref="IEntityRepository{T}"/> — and therefore <see cref="IDatabaseAdapter"/> — and may additionally
/// declare custom query methods decorated with <see cref="QueryAttribute"/>.
/// </para>
/// <para>
/// For a given repository interface, this builder emits a concrete class at runtime that:
/// <list type="bullet">
///   <item>Implements every method on the interface and its full inherited interface chain.</item>
///   <item>Holds a single <see cref="IDatabaseAdapter"/> field, lazily resolved (and cached) on first
///   use via <see cref="RepositoryProxyHelper.GetOrInitializeAdapter"/> — not in the constructor. This
///   decouples proxy construction from database-module registration order: a proxy can be built before
///   its owning database module has subscribed, as long as the module has registered by the time the
///   first call is actually made.</item>
///   <item>Forwards every call — whether it's an <see cref="IEntityRepository{T}"/> CRUD method or a
///   custom <see cref="QueryAttribute"/> method — to the adapter as an <see cref="AstNode"/>, letting
///   whichever database module owns the adapter interpret the call.</item>
/// </list>
/// </para>
/// <para>
/// Each call to <see cref="BuildProxyRepository(Type)"/> defines a brand-new dynamic type (suffixed with a
/// fresh GUID), so the same repository interface can safely be proxied multiple times without type-name
/// collisions within the dynamic module.
/// </para>
/// </remarks>
public static class RepositoryProxyBuilder
{
    // A single dynamic assembly/module is reused for the lifetime of the process. Run-only access means
    // the generated types are never persisted to disk — they exist purely in memory for this process.
    private static readonly AssemblyBuilder _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
        new AssemblyName("LLE.Kernel.DynamicProxies"),
        AssemblyBuilderAccess.Run);

    private static readonly ModuleBuilder _moduleBuilder =
        _assemblyBuilder.DefineDynamicModule("LLE.Kernel.DynamicProxies.Module");

    /// <summary>
    /// Attributes applied to every emitted interface-implementing method: public, overridable via the
    /// v-table (Virtual), and matching the interface signature exactly by name and type (HideBySig).
    /// </summary>
    private const MethodAttributes ImplAttributes =
        MethodAttributes.Public | MethodAttributes.Virtual |
        MethodAttributes.HideBySig;

    /// <summary>
    /// Builds and instantiates a proxy implementation of repository interface <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The repository interface to implement (must inherit <see cref="IEntityRepository{TEntity}"/>).</typeparam>
    /// <returns>A new instance of the generated proxy, cast to <typeparamref name="T"/>.</returns>
    public static T BuildProxyRepository<T>() => (T)BuildProxyRepository(typeof(T));

    /// <summary>
    /// Builds and instantiates a proxy implementation of the given repository interface type.
    /// </summary>
    /// <param name="type">
    /// The repository interface to implement. Must transitively inherit <see cref="IEntityRepository{T}"/>.
    /// </param>
    /// <returns>A new instance of the generated proxy type, boxed as <see cref="object"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown by <see cref="ResolveEntityType"/> if <paramref name="type"/> does not implement
    /// <see cref="IEntityRepository{T}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown by <see cref="CreateMethodOnProxy"/> if the interface declares a method that is neither
    /// part of <see cref="IEntityRepository{T}"/>/<see cref="IDatabaseAdapter"/> nor decorated with
    /// <see cref="QueryAttribute"/>.
    /// </exception>
    public static object BuildProxyRepository(Type type)
    {
        // Step 1: Work out the entity type T from IEntityRepository<T>. This is needed up front because
        // it gets passed into the proxy's constructor so the adapter-resolution logic knows what entity
        // the repository is for, independent of which specific repository interface is being proxied.
        var entityType = ResolveEntityType(type);

        // Read caching configuration from the [Repository] attribute, if present.
        var repoAttr = type.GetCustomAttribute<RepositoryAttribute>();
        var isCached = repoAttr?.IsCached ?? false;
        var cacheSize = repoAttr?.CacheSize ?? 0;

        // Step 2: Determine the full set of interfaces the proxy type must declare.
        //
        // IMPORTANT: type.GetInterfaces() already returns the *entire transitive closure* of inherited
        // interfaces (e.g. for IUserRepository : IEntityRepository<User>, and IEntityRepository<T> :
        // IDatabaseAdapter, this returns both IEntityRepository<User> AND IDatabaseAdapter — not just the
        // immediate parent). We must declare all of them on the TypeBuilder, or the CLR will refuse to
        // finalize the type: a type that claims to implement an interface must also explicitly implement
        // every interface that interface itself inherits.
        var allInterfaces = type.IsInterface
            ? new[] { type }.Concat(type.GetInterfaces()).Distinct().ToArray()
            : Array.Empty<Type>();

        // Step 3: Start defining the dynamic type. The interface-based branch is the primary, supported
        // path: a public class deriving from object that implements every interface from Step 2.
        var typeBuilder = _moduleBuilder.DefineType(
            $"{type.Name}_Proxy_{Guid.NewGuid():N}",
            TypeAttributes.Public | TypeAttributes.Class,
            typeof(object),
            allInterfaces);

        // NOTE: this branch currently has no effect on the interface case above — it unconditionally
        // reassigns typeBuilder when `type` is a class, subclassing `type` directly instead of
        // implementing interfaces. Class-based repository "interfaces" are not really exercised by the
        // rest of this builder (ResolveEntityType requires GetInterfaces() to find IEntityRepository<T>,
        // which a plain base class won't satisfy on its own), so this path is effectively dead/unused
        // today. Left in place in case base-class repositories are supported in the future, but worth
        // revisiting if it's not actually needed.
        if (!type.IsInterface)
        {
            typeBuilder = _moduleBuilder.DefineType(
                $"{type.Name}_Proxy_{Guid.NewGuid():N}",
                TypeAttributes.Public | TypeAttributes.Class,
                type);
        }

        // Step 4: Declare the fields the proxy needs at call time.
        //
        // Adapter resolution is deliberately DEFERRED rather than done in the constructor. Repository
        // proxies and database modules can be constructed/registered in either order depending on module
        // load order, so resolving eagerly in the constructor made proxy construction brittle: building a
        // repository before its owning database module had subscribed would throw immediately. Instead,
        // _adapter starts out null and is lazily resolved (and cached) by RepositoryProxyHelper on first
        // use, via GetOrInitializeAdapter — by which point all modules have had a chance to register.
        var adapterField = typeBuilder.DefineField(
            "_adapter",
            typeof(IDatabaseAdapter),
            FieldAttributes.Private);

        // _repositoryType / _entityType are captured once at construction time (they're cheap, static
        // facts about the proxy) so GetOrInitializeAdapter has what it needs to resolve _adapter on
        // first use without the constructor having to do that resolution itself.
        var repositoryTypeField = typeBuilder.DefineField(
            "_repositoryType",
            typeof(Type),
            FieldAttributes.Private);

        var entityTypeField = typeBuilder.DefineField(
            "_entityType",
            typeof(Type),
            FieldAttributes.Private);

        FieldBuilder? cacheField = null;
        if (isCached)
        {
            cacheField = typeBuilder.DefineField(
                "_cache",
                typeof(object[]),
                FieldAttributes.Private);
        }

        // Step 5: Emit the parameterless constructor, which now just records the type info needed for
        // later lazy adapter resolution — it no longer resolves the adapter itself.
        CreateConstructor(typeBuilder, repositoryTypeField, entityTypeField, type, entityType);

        // Step 6: Emit auto-property backing fields/accessors for any properties declared on the
        // interface itself (not from GetInterfaces() — properties aren't part of the AstNode dispatch
        // contract the way methods are, so only the leaf interface's own properties are handled here).
        foreach (var property in type.GetProperties())
        {
            CreatePropertyOnProxy(typeBuilder, property);
        }

        // Step 7: Gather every method that needs an implementation. Like Step 2, this must walk the full
        // interface closure rather than relying on type.GetMethods() alone, since methods declared on
        // ancestor interfaces (e.g. CreateAsync on IEntityRepository<T>, ExecuteQuery on IDatabaseAdapter)
        // need their own emitted bodies on the proxy, distinct from methods declared directly on `type`
        // (e.g. a custom [Query] method like GetByEmailAsync on IUserRepository itself).
        var allMethods = type.IsInterface
            ? new[] { type }.Concat(type.GetInterfaces()).SelectMany(i => i.GetMethods()).Distinct()
            : type.GetMethods();

        foreach (var method in allMethods)
        {
            // Property accessors (get_X / set_X) are special-name methods already handled by
            // CreatePropertyOnProxy above — skip them here to avoid emitting duplicate method bodies.
            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                continue;
            }

            CreateMethodOnProxy(typeBuilder, method, adapterField, repositoryTypeField, entityTypeField, entityType, cacheField, isCached, cacheSize);
        }

        // Step 8: Finalize the type. This is where the CLR validates that every interface member has a
        // matching emitted method body — if anything from Step 2's interface list lacks an implementation,
        // CreateType() throws a TypeLoadException here.
        var proxyType = typeBuilder.CreateType();

        // Step 9: Instantiate via the parameterless constructor emitted in Step 5.
        return Activator.CreateInstance(proxyType)!;
    }

    /// <summary>
    /// Finds the closed <c>T</c> in <see cref="IEntityRepository{T}"/> implemented by
    /// <paramref name="repositoryType"/>.
    /// </summary>
    /// <param name="repositoryType">The repository interface to inspect.</param>
    /// <returns>The entity type <c>T</c>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="repositoryType"/> does not implement <see cref="IEntityRepository{T}"/>.
    /// </exception>
    private static Type ResolveEntityType(Type repositoryType)
    {
        foreach (var iface in repositoryType.GetInterfaces())
        {
            // Compare against the open generic definition (IEntityRepository<>) since `iface` here is a
            // closed generic type (e.g. IEntityRepository<User>).
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityRepository<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }

        throw new InvalidOperationException(
            $"Type '{repositoryType.Name}' does not implement IEntityRepository<T>.");
    }

    /// <summary>
    /// Emits the proxy's parameterless constructor. The constructor no longer resolves an
    /// <see cref="IDatabaseAdapter"/> directly — it simply records <paramref name="repositoryType"/> and
    /// <paramref name="entityType"/> so that <see cref="RepositoryProxyHelper.GetOrInitializeAdapter"/> can
    /// lazily resolve (and cache) the adapter the first time any generated method actually needs it.
    /// </summary>
    /// <param name="typeBuilder">The proxy type under construction.</param>
    /// <param name="repositoryTypeField">The field that will hold the repository interface type (e.g. IUserRepository).</param>
    /// <param name="entityTypeField">The field that will hold the entity type T resolved from IEntityRepository&lt;T&gt;.</param>
    /// <param name="repositoryType">The original repository interface type (e.g. IUserRepository).</param>
    /// <param name="entityType">The entity type T resolved from IEntityRepository&lt;T&gt;.</param>
    private static void CreateConstructor(TypeBuilder typeBuilder, FieldBuilder repositoryTypeField,
        FieldBuilder entityTypeField, Type repositoryType, Type entityType)
    {
        var ctorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            Type.EmptyTypes);

        var il = ctorBuilder.GetILGenerator();

        // base.ctor() — call object's parameterless constructor first, as required for any class.
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);

        // this._repositoryType = typeof(repositoryType);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldtoken, repositoryType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Stfld, repositoryTypeField);

        // this._entityType = typeof(entityType);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Stfld, entityTypeField);

        // NOTE: this._adapter is deliberately left at its default value (null) here. It's resolved lazily
        // by RepositoryProxyHelper.GetOrInitializeAdapter on first use — see CreateExecuteQueryMethod and
        // CreateDelegateMethod, both of which call it before touching the adapter.
        il.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Emits a simple auto-property (private backing field + get/set accessors) on the proxy for a
    /// property declared on the repository interface.
    /// </summary>
    /// <param name="typeBuilder">The proxy type under construction.</param>
    /// <param name="property">The property to implement.</param>
    private static void CreatePropertyOnProxy(TypeBuilder typeBuilder, PropertyInfo property)
    {
        // Backing field: _PropertyName
        var fieldBuilder = typeBuilder.DefineField(
            $"_{property.Name}",
            property.PropertyType,
            FieldAttributes.Private);

        var propertyBuilder = typeBuilder.DefineProperty(
            property.Name,
            PropertyAttributes.None,
            property.PropertyType,
            Type.EmptyTypes);

        const MethodAttributes accessorAttributes =
            MethodAttributes.Public | MethodAttributes.SpecialName |
            MethodAttributes.HideBySig | MethodAttributes.Virtual;

        if (property.CanRead)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                $"get_{property.Name}",
                accessorAttributes,
                property.PropertyType,
                Type.EmptyTypes);

            // return this._PropertyName;
            var il = getMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        if (property.CanWrite)
        {
            var setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{property.Name}",
                accessorAttributes,
                null,
                [property.PropertyType]);

            // this._PropertyName = value;
            var il = setMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }

    /// <summary>
    /// Routes a single interface method to the appropriate IL-emission strategy based on where the
    /// method is declared (or whether it carries a <see cref="QueryAttribute"/>).
    /// </summary>
    /// <param name="typeBuilder">The proxy type under construction.</param>
    /// <param name="method">The method to implement.</param>
    /// <param name="adapterField">The field holding the (possibly not-yet-resolved) <see cref="IDatabaseAdapter"/>.</param>
    /// <param name="repositoryTypeField">The field holding the repository interface type, used for lazy adapter resolution.</param>
    /// <param name="entityTypeField">The field holding the entity type, used for lazy adapter resolution.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if <paramref name="method"/> is not declared on <see cref="IDatabaseAdapter"/> or
    /// <see cref="IEntityRepository{T}"/>, and is not decorated with <see cref="QueryAttribute"/>.
    /// </exception>
    private static void CreateMethodOnProxy(TypeBuilder typeBuilder, MethodInfo method,
        FieldBuilder adapterField, FieldBuilder repositoryTypeField, FieldBuilder entityTypeField,
        Type entityType, FieldBuilder? cacheField, bool isCached, int cacheSize)
    {
        // Case 1: IDatabaseAdapter.ExecuteQuery itself — forward the caller-supplied AstNode directly
        // to the adapter, with no synthesized node. This is the one method where the proxy is purely a
        // pass-through rather than a translator.
        if (method.DeclaringType == typeof(IDatabaseAdapter))
        {
            CreateExecuteQueryMethod(typeBuilder, method, adapterField, repositoryTypeField, entityTypeField);
            return;
        }

        // Case 2: a CRUD method inherited from IEntityRepository<T> (CreateAsync, UpdateAsync,
        // DeleteAsync, FindByIdAsync, FindAllAsync). DeclaringType here is the *closed* generic type
        // (e.g. IEntityRepository<User>), so we compare against the open generic definition.
        if (method.DeclaringType!.IsGenericType &&
            method.DeclaringType.GetGenericTypeDefinition() == typeof(IEntityRepository<>))
        {
            CreateCrudMethod(typeBuilder, method, adapterField, repositoryTypeField, entityTypeField, entityType, cacheField, isCached, cacheSize);
            return;
        }

        // Case 3: a custom method declared directly on the repository interface (e.g.
        // IUserRepository.GetByEmailAsync), explicitly opted in via [Query].
        var queryAttr = method.GetCustomAttribute<QueryAttribute>();
        if (queryAttr != null)
        {
            CreateQueryMethod(typeBuilder, method, adapterField, repositoryTypeField, entityTypeField, entityType, queryAttr.Query);
            return;
        }

        // Anything else is an interface member the builder doesn't know how to satisfy — fail loudly at
        // build time rather than emitting a broken/missing method and failing later at CLR load time.
        throw new NotSupportedException(
            $"Method '{method.Name}' on repository '{method.DeclaringType?.Name}' is not supported. " +
            $"Only IEntityRepository<T> members and methods decorated with [Query] are allowed.");
    }

    /// <summary>
    /// Emits a method body for <see cref="IDatabaseAdapter.ExecuteQuery"/> that lazily resolves the
    /// adapter (caching it in <paramref name="adapterField"/>) and forwards its single
    /// <see cref="AstNode"/> argument to it.
    /// </summary>
    /// <remarks>
    /// Equivalent to:
    /// <c>return RepositoryProxyHelper.GetOrInitializeAdapter(ref this._adapter, this._repositoryType, this._entityType).ExecuteQuery(node);</c>
    /// </remarks>
    private static void CreateExecuteQueryMethod(TypeBuilder typeBuilder, MethodInfo method,
        FieldBuilder adapterField, FieldBuilder repositoryTypeField, FieldBuilder entityTypeField)
    {
        var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        var methodBuilder = typeBuilder.DefineMethod(method.Name, ImplAttributes, method.ReturnType, paramTypes);

        var il = methodBuilder.GetILGenerator();

        // Push the by-ref address of this._adapter (NOT its value — Ldflda, not Ldfld) along with
        // this._repositoryType and this._entityType, then call the helper. The helper checks whether the
        // field at that address is still null; if so it resolves an adapter and writes it back through
        // the ref, caching it on the instance for every subsequent call. Either way it returns the
        // now-non-null adapter.
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, adapterField);       // ref this._adapter
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, repositoryTypeField);  // this._repositoryType
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, entityTypeField);      // this._entityType
        il.Emit(OpCodes.Call, GetOrInitializeAdapterMethod); // -> IDatabaseAdapter (resolved + cached)

        il.Emit(OpCodes.Ldarg_1);                              // node (the single AstNode parameter)
        il.Emit(OpCodes.Callvirt, DatabaseAdapterExecuteQuery); // adapter.ExecuteQuery(node)
        il.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Emits a method body for a CRUD method from <see cref="IEntityRepository{T}"/> (CreateAsync,
    /// UpdateAsync, DeleteAsync, FindByIdAsync, FindAllAsync) that lazily resolves the adapter,
    /// constructs the appropriate concrete <see cref="AstNode"/> subclass, and forwards both to
    /// <see cref="RepositoryProxyHelper.ExecuteAsync{T}"/>.
    /// </summary>
    private static void CreateCrudMethod(TypeBuilder typeBuilder, MethodInfo method,
        FieldBuilder adapterField, FieldBuilder repositoryTypeField, FieldBuilder entityTypeField,
        Type entityType, FieldBuilder? cacheField, bool isCached, int cacheSize)
    {
        var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        var methodBuilder = typeBuilder.DefineMethod(method.Name, ImplAttributes, method.ReturnType, paramTypes);
        var il = methodBuilder.GetILGenerator();

        var adapterLocal = il.DeclareLocal(typeof(IDatabaseAdapter));

        // adapter = GetOrInitializeAdapter(ref _adapter, _repositoryType, _entityType)
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, adapterField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, repositoryTypeField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, entityTypeField);
        il.Emit(OpCodes.Call, GetOrInitializeAdapterMethod);
        il.Emit(OpCodes.Stloc, adapterLocal);

        LocalBuilder? cacheLocal = null;
        if (isCached)
        {
            cacheLocal = il.DeclareLocal(typeof(object[]));

            // cache = GetOrInitializeCache(ref _cache, adapter, _entityType, cacheSize)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, cacheField!);
            il.Emit(OpCodes.Ldloc, adapterLocal);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, entityTypeField);
            il.Emit(OpCodes.Ldc_I4, cacheSize);
            il.Emit(OpCodes.Call, GetOrInitializeCacheMethod);
            il.Emit(OpCodes.Stloc, cacheLocal);
        }

        switch (method.Name)
        {
            case "CreateAsync":
            {
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitWriteQueryNode(il, entityType);
                EmitContextAndOptions(il, method);

                if (isCached)
                {
                    il.Emit(OpCodes.Ldloc, cacheLocal!);
                    il.Emit(OpCodes.Ldtoken, entityType);
                    il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                    il.Emit(OpCodes.Ldc_I4, 0); // CacheOp.Insert

                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Call, ExecuteWithCacheOpMethod.MakeGenericMethod(resultType));
                }
                else
                {
                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(resultType));
                }

                il.Emit(OpCodes.Ret);
                return;
            }

            case "UpdateAsync":
            {
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitWriteQueryNodeWithFilter(il, entityType);
                EmitContextAndOptions(il, method);

                if (isCached)
                {
                    il.Emit(OpCodes.Ldloc, cacheLocal!);
                    il.Emit(OpCodes.Ldtoken, entityType);
                    il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                    il.Emit(OpCodes.Ldc_I4, 1); // CacheOp.Update

                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Call, ExecuteWithCacheOpMethod.MakeGenericMethod(resultType));
                }
                else
                {
                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(resultType));
                }

                il.Emit(OpCodes.Ret);
                return;
            }

            case "DeleteAsync":
            {
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitDeleteQueryNode(il, entityType);
                EmitContextAndOptions(il, method);

                if (isCached)
                {
                    il.Emit(OpCodes.Ldloc, cacheLocal!);
                    il.Emit(OpCodes.Ldtoken, entityType);
                    il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                    il.Emit(OpCodes.Ldc_I4, 2); // CacheOp.Remove

                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Call, ExecuteWithCacheOpMethod.MakeGenericMethod(resultType));
                }
                else
                {
                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(resultType));
                }

                il.Emit(OpCodes.Ret);
                return;
            }

            case "FindByIdAsync" when isCached:
            {
                // return Task.FromResult(FindInCache(cache, _entityType, id) as T)
                il.Emit(OpCodes.Ldloc, cacheLocal!);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, entityTypeField);
                il.Emit(OpCodes.Ldarg_1);                           // id
                il.Emit(OpCodes.Call, FindInCacheMethod);
                il.Emit(OpCodes.Isinst, entityType);

                var resultType = method.ReturnType.GetGenericArguments()[0];
                il.Emit(OpCodes.Call, TaskFromResultMethod.MakeGenericMethod(resultType));
                il.Emit(OpCodes.Ret);
                return;
            }

            case "FindByIdAsync":
            {
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitReadQueryNodeWithFilter(il, entityType);
                EmitContextAndOptions(il, method);

                var resultType = method.ReturnType.GetGenericArguments()[0];
                il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(resultType));
                il.Emit(OpCodes.Ret);
                return;
            }

            case "FindAllAsync" when isCached:
            {
                var sortByIdx = FindParameterIndex(method, typeof(SortOption));
                var paginationIdx = FindParameterIndex(method, typeof(Pagination));

                var findAllResultType = method.ReturnType.GetGenericArguments()[0];

                if (sortByIdx < 0 && paginationIdx < 0)
                {
                    // return Task.FromResult(FindAllFromCache<T>(cache))
                    il.Emit(OpCodes.Ldloc, cacheLocal!);
                    il.Emit(OpCodes.Call, FindAllFromCacheMethod.MakeGenericMethod(entityType));

                    il.Emit(OpCodes.Call, TaskFromResultMethod.MakeGenericMethod(findAllResultType));
                    il.Emit(OpCodes.Ret);
                    return;
                }

                // Runtime check: if sortBy != null || pagination != null, use adapter
                var useAdapter = il.DefineLabel();
                var useCache = il.DefineLabel();

                if (sortByIdx >= 0)
                {
                    EmitLoadArg(il, sortByIdx + 1);
                    il.Emit(OpCodes.Brtrue, useAdapter);
                }

                if (paginationIdx >= 0)
                {
                    EmitLoadArg(il, paginationIdx + 1);
                    il.Emit(OpCodes.Brtrue, useAdapter);
                }

                il.Emit(OpCodes.Br, useCache);

                il.MarkLabel(useAdapter);
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitReadQueryNode(il, entityType);
                EmitSortingAndPaginationOnNode(il, method);
                EmitContextAndOptions(il, method);

                il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(findAllResultType));
                il.Emit(OpCodes.Ret);

                il.MarkLabel(useCache);
                il.Emit(OpCodes.Ldloc, cacheLocal!);
                il.Emit(OpCodes.Call, FindAllFromCacheMethod.MakeGenericMethod(entityType));
                il.Emit(OpCodes.Call, TaskFromResultMethod.MakeGenericMethod(findAllResultType));
                il.Emit(OpCodes.Ret);
                return;
            }

            case "FindAllAsync":
            {
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitReadQueryNode(il, entityType);
                EmitSortingAndPaginationOnNode(il, method);
                EmitContextAndOptions(il, method);

                var resultType = method.ReturnType.GetGenericArguments()[0];
                il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(resultType));
                il.Emit(OpCodes.Ret);
                return;
            }

            case "TotalRecords":
            {
                il.Emit(OpCodes.Ldloc, adapterLocal);
                EmitReadQueryNode(il, entityType);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stfld, ReadQueryNodeIsCountField);
                EmitContextAndOptions(il, method);

                il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(typeof(int)));
                il.Emit(OpCodes.Ret);
                return;
            }

            default:
                throw new NotSupportedException(
                    $"Unknown CRUD method '{method.Name}' on IEntityRepository<{entityType.Name}>.");
        }
    }

    private static void EmitWriteQueryNode(ILGenerator il, Type entityType)
    {
        // new WriteQueryNode { TableName = entityType.Name, Payload = arg_1, EntityType = entityType }
        il.Emit(OpCodes.Newobj, WriteQueryNodeConstructor);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldstr, entityType.Name);
        il.Emit(OpCodes.Callvirt, WriteQueryNodeSetTableName);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Callvirt, WriteQueryNodeSetPayload);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Callvirt, AstNodeSetEntityType);
    }

    private static void EmitWriteQueryNodeWithFilter(ILGenerator il, Type entityType)
    {
        var idGetter = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod()
            ?? throw new InvalidOperationException(
                $"Entity type '{entityType.Name}' must have a public 'Id' property.");

        var filterLocal = il.DeclareLocal(typeof(FilterNode));
        var entityLocal = il.DeclareLocal(entityType);

        // var filter = new FilterNode { ColumnName = "Id", Operator = Equals, Value = item.Id }
        il.Emit(OpCodes.Newobj, FilterNodeConstructor);
        il.Emit(OpCodes.Stloc, filterLocal);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldstr, "Id");
        il.Emit(OpCodes.Callvirt, FilterNodeSetColumnName);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Callvirt, FilterNodeSetOperator);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Stloc, entityLocal);
        il.Emit(OpCodes.Callvirt, idGetter);
        il.Emit(OpCodes.Box, typeof(Guid));
        il.Emit(OpCodes.Callvirt, FilterNodeSetValue);

        // new WriteQueryNode { TableName = entityType.Name, Payload = entity, Where = filter, EntityType = entityType }
        il.Emit(OpCodes.Newobj, WriteQueryNodeConstructor);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldstr, entityType.Name);
        il.Emit(OpCodes.Callvirt, WriteQueryNodeSetTableName);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldloc, entityLocal);
        il.Emit(OpCodes.Callvirt, WriteQueryNodeSetPayload);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Callvirt, WriteQueryNodeSetWhere);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Callvirt, AstNodeSetEntityType);
    }

    private static void EmitDeleteQueryNode(ILGenerator il, Type entityType)
    {
        var idGetter = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod()
            ?? throw new InvalidOperationException(
                $"Entity type '{entityType.Name}' must have a public 'Id' property.");

        var filterLocal = il.DeclareLocal(typeof(FilterNode));
        var entityLocal = il.DeclareLocal(entityType);

        // var filter = new FilterNode { ColumnName = "Id", Operator = Equals, Value = item.Id }
        il.Emit(OpCodes.Newobj, FilterNodeConstructor);
        il.Emit(OpCodes.Stloc, filterLocal);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldstr, "Id");
        il.Emit(OpCodes.Callvirt, FilterNodeSetColumnName);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Callvirt, FilterNodeSetOperator);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Stloc, entityLocal);
        il.Emit(OpCodes.Callvirt, idGetter);
        il.Emit(OpCodes.Box, typeof(Guid));
        il.Emit(OpCodes.Callvirt, FilterNodeSetValue);

        // new DeleteQueryNode { TableName = entityType.Name, Where = filter, Payload = entity, EntityType = entityType }
        il.Emit(OpCodes.Newobj, DeleteQueryNodeConstructor);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldstr, entityType.Name);
        il.Emit(OpCodes.Callvirt, DeleteQueryNodeSetTableName);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Stfld, DeleteQueryNodeWhereField);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldloc, entityLocal);
        il.Emit(OpCodes.Callvirt, DeleteQueryNodeSetPayload);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Callvirt, AstNodeSetEntityType);
    }

    private static void EmitReadQueryNodeWithFilter(ILGenerator il, Type entityType)
    {
        var filterLocal = il.DeclareLocal(typeof(FilterNode));

        // var filter = new FilterNode { ColumnName = "Id", Operator = Equals, Value = id }
        il.Emit(OpCodes.Newobj, FilterNodeConstructor);
        il.Emit(OpCodes.Stloc, filterLocal);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldstr, "Id");
        il.Emit(OpCodes.Callvirt, FilterNodeSetColumnName);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Callvirt, FilterNodeSetOperator);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Box, typeof(Guid));
        il.Emit(OpCodes.Callvirt, FilterNodeSetValue);

        // new ReadQueryNode { TableName = entityType.Name, Where = filter, EntityType = entityType }
        il.Emit(OpCodes.Newobj, ReadQueryNodeConstructor);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldstr, entityType.Name);
        il.Emit(OpCodes.Callvirt, ReadQueryNodeSetTableName);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldloc, filterLocal);
        il.Emit(OpCodes.Stfld, ReadQueryNodeWhereField);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Callvirt, AstNodeSetEntityType);
    }

    private static void EmitReadQueryNode(ILGenerator il, Type entityType)
    {
        // new ReadQueryNode { TableName = entityType.Name, EntityType = entityType }
        il.Emit(OpCodes.Newobj, ReadQueryNodeConstructor);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldstr, entityType.Name);
        il.Emit(OpCodes.Callvirt, ReadQueryNodeSetTableName);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Callvirt, AstNodeSetEntityType);
    }

    /// <summary>
    /// Emits a method body for a custom <see cref="QueryAttribute"/> method that lazily resolves
    /// the adapter, calls <see cref="AstParser.Parse"/> to build the AST from the query string,
    /// and forwards to <see cref="RepositoryProxyHelper.ExecuteAsync{T}"/>.
    /// </summary>
    private static void CreateQueryMethod(TypeBuilder typeBuilder, MethodInfo method,
        FieldBuilder adapterField, FieldBuilder repositoryTypeField, FieldBuilder entityTypeField,
        Type entityType, string query)
    {
        var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        var methodBuilder = typeBuilder.DefineMethod(method.Name, ImplAttributes, method.ReturnType, paramTypes);
        var il = methodBuilder.GetILGenerator();

        var adapterLocal = il.DeclareLocal(typeof(IDatabaseAdapter));

        // adapter = GetOrInitializeAdapter(ref _adapter, _repositoryType, _entityType)
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, adapterField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, repositoryTypeField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, entityTypeField);
        il.Emit(OpCodes.Call, GetOrInitializeAdapterMethod);
        il.Emit(OpCodes.Stloc, adapterLocal);

        // Push adapter onto stack first
        il.Emit(OpCodes.Ldloc, adapterLocal);

        // Determine which method parameters map to :paramName tokens in the query
        var paramBindings = FindQueryParameterBindings(query, method);

        if (paramBindings.Count > 0)
        {
            // var dict = new Dictionary<string, object?>();
            var dictLocal = il.DeclareLocal(typeof(Dictionary<string, object?>));
            il.Emit(OpCodes.Newobj, DictionaryStringObjectCtor);
            il.Emit(OpCodes.Stloc, dictLocal);

            foreach (var (paramName, argIndex, paramType) in paramBindings)
            {
                // dict.Add(paramName, arg)
                il.Emit(OpCodes.Ldloc, dictLocal);
                il.Emit(OpCodes.Ldstr, paramName);
                EmitLoadArg(il, argIndex);
                if (paramType.IsValueType)
                    il.Emit(OpCodes.Box, paramType);
                il.Emit(OpCodes.Callvirt, DictionaryStringObjectAdd);
            }

            // node = AstParser.Parse(entityType, query, dict)
            il.Emit(OpCodes.Ldtoken, entityType);
            il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
            il.Emit(OpCodes.Ldstr, query);
            il.Emit(OpCodes.Ldloc, dictLocal);
            il.Emit(OpCodes.Call, AstParserParseWithParamsMethod);
        }
        else
        {
            // node = AstParser.Parse(entityType, query)
            il.Emit(OpCodes.Ldtoken, entityType);
            il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
            il.Emit(OpCodes.Ldstr, query);
            il.Emit(OpCodes.Call, AstParserParseMethod);
        }

        // node.OperationKind = OperationKind.Read
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Callvirt, AstNodeSetOperationKind);

        // node.EntityType = entityType
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldtoken, entityType);
        il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
        il.Emit(OpCodes.Callvirt, AstNodeSetEntityType);

        // stack: [adapter, node]
        // node.OrderBy = sortBy (if present); node.Pagination = pagination (if present)
        EmitSortingAndPaginationOnNode(il, method);
        // stack: [adapter, node]
        EmitContextAndOptions(il, method);

        // stack: [adapter, node, context, options] → ExecuteAsync<TResult>(adapter, node, context, options)
        var resultType = method.ReturnType.IsGenericType
            ? method.ReturnType.GetGenericArguments()[0]
            : typeof(object);

        il.Emit(OpCodes.Call, ExecuteAsyncMethod.MakeGenericMethod(resultType));
        il.Emit(OpCodes.Ret);
    }

    private static void EmitContextAndOptions(ILGenerator il, MethodInfo method)
    {
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            if (paramType == typeof(UserContext) || paramType == typeof(DataOptions))
                EmitLoadArg(il, i + 1);
        }
    }

    private static int FindParameterIndex(MethodInfo method, Type type)
    {
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType == type)
                return i;
        }
        return -1;
    }

    private static void EmitSortingAndPaginationOnNode(ILGenerator il, MethodInfo method)
    {
        var sortByIdx = FindParameterIndex(method, typeof(SortOption));
        var paginationIdx = FindParameterIndex(method, typeof(Pagination));

        if (sortByIdx >= 0)
        {
            il.Emit(OpCodes.Dup);
            EmitLoadArg(il, sortByIdx + 1);
            il.Emit(OpCodes.Stfld, ReadQueryNodeOrderByField);
        }

        if (paginationIdx >= 0)
        {
            il.Emit(OpCodes.Dup);
            EmitLoadArg(il, paginationIdx + 1);
            il.Emit(OpCodes.Stfld, ReadQueryNodePaginationField);
        }
    }

    /// <summary>
    /// Finds all <c>:paramName</c> tokens in <paramref name="query"/> and matches them to
    /// method parameters on <paramref name="method"/> by name. Returns the binding info needed
    /// to emit IL that builds a parameter dictionary at runtime.
    /// </summary>
    private static List<(string paramName, int argIndex, Type paramType)> FindQueryParameterBindings(
        string query, MethodInfo method)
    {
        var tokens = TokenArrayBuilder.Parse(query);
        var methodParams = method.GetParameters();
        var bindings = new List<(string, int, Type)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in tokens)
        {
            if (token.Kind != TokenKind.Parameter)
                continue;

            var name = query.Substring(token.StartPosition, token.Length);

            if (!seen.Add(name))
                continue;

            for (var i = 0; i < methodParams.Length; i++)
            {
                if (methodParams[i].Name is { } paramName &&
                    paramName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    bindings.Add((name, i + 1, methodParams[i].ParameterType));
                    break;
                }
            }
        }

        return bindings;
    }

    /// <summary>
    /// Emits the appropriate <c>ldarg</c> opcode for the given argument index.
    /// </summary>
    private static void EmitLoadArg(ILGenerator il, int index)
    {
        switch (index)
        {
            case 0: il.Emit(OpCodes.Ldarg_0); break;
            case 1: il.Emit(OpCodes.Ldarg_1); break;
            case 2: il.Emit(OpCodes.Ldarg_2); break;
            case 3: il.Emit(OpCodes.Ldarg_3); break;
            default: il.Emit(OpCodes.Ldarg_S, (byte)index); break;
        }
    }

    // --- Cached reflection handles used during IL emission ---
    // Resolved once at static-init time rather than per proxy/method, since reflection lookups are
    // comparatively expensive and these targets never change.

    private static readonly MethodInfo GetTypeFromHandleMethod =
        typeof(Type).GetMethod("GetTypeFromHandle", [typeof(RuntimeTypeHandle)])!;

    private static readonly MethodInfo GetOrInitializeAdapterMethod =
        typeof(RepositoryProxyHelper).GetMethod(
            nameof(RepositoryProxyHelper.GetOrInitializeAdapter),
            BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo DatabaseAdapterExecuteQuery =
        typeof(IDatabaseAdapter).GetMethod(nameof(IDatabaseAdapter.ExecuteQuery))!;

    private static readonly MethodInfo ExecuteAsyncMethod =
        typeof(RepositoryProxyHelper).GetMethod(
            nameof(RepositoryProxyHelper.ExecuteAsync),
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(IDatabaseAdapter), typeof(AstNode), typeof(UserContext), typeof(DataOptions)],
            null)!;

    // --- AstParser ---
    private static readonly MethodInfo AstParserParseMethod =
        typeof(AstParser).GetMethod(
            nameof(AstParser.Parse),
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(Type), typeof(string)],
            null)!;

    private static readonly MethodInfo AstParserParseWithParamsMethod =
        typeof(AstParser).GetMethod(
            nameof(AstParser.Parse),
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(Type), typeof(string), typeof(IReadOnlyDictionary<string, object?>)],
            null)!;

    // --- Dictionary<string, object?> ---
    private static readonly ConstructorInfo DictionaryStringObjectCtor =
        typeof(Dictionary<string, object?>).GetConstructor(Type.EmptyTypes)!;

    private static readonly MethodInfo DictionaryStringObjectAdd =
        typeof(Dictionary<string, object?>).GetMethod("Add", [typeof(string), typeof(object)])!;

    // --- AstNode ---
    private static readonly MethodInfo AstNodeSetOperationKind =
        typeof(AstNode).GetMethod("set_OperationKind")!;

    private static readonly MethodInfo AstNodeSetEntityType =
        typeof(AstNode).GetMethod("set_EntityType")!;

    // --- ReadQueryNode ---
    private static readonly ConstructorInfo ReadQueryNodeConstructor =
        typeof(ReadQueryNode).GetConstructor(Type.EmptyTypes)!;

    private static readonly MethodInfo ReadQueryNodeSetTableName =
        typeof(ReadQueryNode).GetMethod("set_TableName")!;

    private static readonly FieldInfo ReadQueryNodeWhereField =
        typeof(ReadQueryNode).GetField("Where")!;

    private static readonly FieldInfo ReadQueryNodeOrderByField =
        typeof(ReadQueryNode).GetField(nameof(ReadQueryNode.OrderBy))!;

    private static readonly FieldInfo ReadQueryNodePaginationField =
        typeof(ReadQueryNode).GetField(nameof(ReadQueryNode.Pagination))!;

    private static readonly FieldInfo ReadQueryNodeIsCountField =
        typeof(ReadQueryNode).GetField(nameof(ReadQueryNode.IsCount))!;

    // --- WriteQueryNode ---
    private static readonly ConstructorInfo WriteQueryNodeConstructor =
        typeof(WriteQueryNode).GetConstructor(Type.EmptyTypes)!;

    private static readonly MethodInfo WriteQueryNodeSetTableName =
        typeof(WriteQueryNode).GetMethod("set_TableName")!;

    private static readonly MethodInfo WriteQueryNodeSetPayload =
        typeof(WriteQueryNode).GetMethod("set_Payload")!;

    private static readonly MethodInfo WriteQueryNodeSetWhere =
        typeof(WriteQueryNode).GetMethod("set_Where")!;

    // --- DeleteQueryNode ---
    private static readonly ConstructorInfo DeleteQueryNodeConstructor =
        typeof(DeleteQueryNode).GetConstructor(Type.EmptyTypes)!;

    private static readonly MethodInfo DeleteQueryNodeSetTableName =
        typeof(DeleteQueryNode).GetMethod("set_TableName")!;

    private static readonly MethodInfo DeleteQueryNodeSetPayload =
        typeof(DeleteQueryNode).GetMethod("set_Payload")!;

    private static readonly FieldInfo DeleteQueryNodeWhereField =
        typeof(DeleteQueryNode).GetField("Where")!;

    // --- FilterNode ---
    private static readonly ConstructorInfo FilterNodeConstructor =
        typeof(FilterNode).GetConstructor(Type.EmptyTypes)!;

    private static readonly MethodInfo FilterNodeSetColumnName =
        typeof(FilterNode).GetMethod("set_ColumnName")!;

    private static readonly MethodInfo FilterNodeSetOperator =
        typeof(FilterNode).GetMethod("set_Operator")!;

    private static readonly MethodInfo FilterNodeSetValue =
        typeof(FilterNode).GetMethod("set_Value")!;

    // --- Cache helpers ---
    private static readonly MethodInfo GetOrInitializeCacheMethod =
        typeof(RepositoryProxyHelper).GetMethod(
            nameof(RepositoryProxyHelper.GetOrInitializeCache),
            BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo FindInCacheMethod =
        typeof(RepositoryProxyHelper).GetMethod(
            nameof(RepositoryProxyHelper.FindInCache),
            BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo FindAllFromCacheMethod =
        typeof(RepositoryProxyHelper).GetMethod(
            nameof(RepositoryProxyHelper.FindAllFromCache),
            BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo ExecuteWithCacheOpMethod =
        typeof(RepositoryProxyHelper).GetMethod(
            nameof(RepositoryProxyHelper.ExecuteWithCacheOp),
            BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo TaskFromResultMethod =
        typeof(Task).GetMethod(
            nameof(Task.FromResult),
            BindingFlags.Public | BindingFlags.Static)!;
}