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
        // TODO: Build
    }

    private static void CreateRepositoryMethods(TypeBuilder typeBuilder, Type repositoryType)
    {
        // TODO: Build
    }
}