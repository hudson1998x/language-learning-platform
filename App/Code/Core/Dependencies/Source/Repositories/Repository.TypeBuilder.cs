using System.Reflection;
using System.Reflection.Emit;

namespace LLE.Dependencies.Repositories;

public static partial class Repository
{
    private static readonly AssemblyBuilder _builder = AssemblyBuilder.DefineDynamicAssembly(
        new AssemblyName("LLE.Repositories"),
        AssemblyBuilderAccess.Run
    );

    private static readonly ModuleBuilder _moduleBuilder = _builder.DefineDynamicModule("Generated");
    
    private static object CreateRepositoryFromInterface(Type repositoryType)
    {
        var entityType = repositoryType.GetCustomAttribute<RepositoryAttribute>()!.EntityType;
        // create the proxy class.
        var typeBuilder = _moduleBuilder.DefineType(
            repositoryType.Name + "Proxy", 
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed
        );
        
        // add an interface implementation, from this point forward
        // all the repository methods MUST be implemented.
        typeBuilder.AddInterfaceImplementation(typeof(IDatabaseRepository<>).MakeGenericType(entityType));
        typeBuilder.AddInterfaceImplementation(repositoryType);

        // now to build out methods, props etc...
        CreateRepositoryProperties(typeBuilder, repositoryType);
        CreateRepositoryMethods(typeBuilder, repositoryType);
        
        // will throw if missing interface implementations.
        var createdType = typeBuilder.CreateType() ?? throw new InvalidOperationException($"Failed to create proxy class for {repositoryType.Name}");
        
        // pass it back to the caller.
        return Activator.CreateInstance(createdType)!;
    }

    private static void CreateRepositoryProperties(TypeBuilder typeBuilder, Type repositoryType)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        foreach (var property in repositoryType.GetProperties(flags))
        {
            var propertyType = property.PropertyType;

            // backing field: _<PropertyName>
            var fieldBuilder = typeBuilder.DefineField(
                $"_{property.Name}",
                propertyType,
                FieldAttributes.Private
            );

            // define property
            var propertyBuilder = typeBuilder.DefineProperty(
                property.Name,
                PropertyAttributes.None,
                propertyType,
                Type.EmptyTypes
            );

            // GETTER
            if (property.CanRead)
            {
                var getMethod = typeBuilder.DefineMethod(
                    $"get_{property.Name}",
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    propertyType,
                    Type.EmptyTypes
                );

                var il = getMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethod);

                // ensure interface mapping
                typeBuilder.DefineMethodOverride(
                    getMethod,
                    property.GetGetMethod()!
                );
            }

            // SETTER
            if (property.CanWrite)
            {
                var setMethod = typeBuilder.DefineMethod(
                    $"set_{property.Name}",
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    null,
                    new[] { propertyType }
                );

                var il = setMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, fieldBuilder);
                il.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethod);

                typeBuilder.DefineMethodOverride(
                    setMethod,
                    property.GetSetMethod()!
                );
            }
        }
    }

    private static void CreateRepositoryMethods(TypeBuilder typeBuilder, Type repositoryType)
    {
        // TODO: Build
    }
}