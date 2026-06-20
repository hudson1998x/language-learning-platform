using System;
using System.Reflection;
using System.Reflection.Emit;

namespace LLE.Kernel.Builders;

public static class RepositoryProxyBuilder
{
    private static readonly AssemblyBuilder _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
        new AssemblyName("LLE.Kernel.DynamicProxies"),
        AssemblyBuilderAccess.Run);

    private static readonly ModuleBuilder _moduleBuilder =
        _assemblyBuilder.DefineDynamicModule("LLE.Kernel.DynamicProxies.Module");

    public static T BuildProxyRepository<T>() => (T)BuildProxyRepository(typeof(T));

    public static object BuildProxyRepository(Type type)
    {
        var typeBuilder = _moduleBuilder.DefineType(
            $"{type.Name}_Proxy_{Guid.NewGuid():N}",
            TypeAttributes.Public | TypeAttributes.Class,
            typeof(object),
            type.IsInterface ? new[] { type } : Array.Empty<Type>());

        // If we're proxying a concrete class rather than an interface, inherit from it instead.
        if (!type.IsInterface)
        {
            typeBuilder = _moduleBuilder.DefineType(
                $"{type.Name}_Proxy_{Guid.NewGuid():N}",
                TypeAttributes.Public | TypeAttributes.Class,
                type);
        }

        foreach (var property in type.GetProperties())
        {
            CreatePropertyOnProxy(typeBuilder, property);
        }

        foreach (var method in type.GetMethods())
        {
            // Skip property accessor methods (get_X / set_X) - they're handled by CreatePropertyOnProxy.
            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                continue;
            }

            CreateMethodOnProxy(typeBuilder, method);
        }

        var proxyType = typeBuilder.CreateType();

        return Activator.CreateInstance(proxyType)!;
    }

    private static void CreatePropertyOnProxy(TypeBuilder typeBuilder, PropertyInfo property)
    {
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
                new[] { property.PropertyType });

            var il = setMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }

    private static void CreateMethodOnProxy(TypeBuilder typeBuilder, MethodInfo method)
    {
        // Stub - implement interception/dispatch logic here.
        throw new NotImplementedException();
    }
}