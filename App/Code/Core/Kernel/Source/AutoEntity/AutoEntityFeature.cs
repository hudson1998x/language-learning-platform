using LLE.Kernel.Contracts;
using LLE.Kernel.Dto;
using LLE.Kernel.Exceptions;
using LLE.Kernel.Registry;

namespace LLE.Kernel.AutoEntity;

public static class AutoEntityFeature
{
    public static void AutoFeature<T, T1>() where T1 : IEntityRepository<T> where T : class 
    {
        var type = typeof(T);
        
        FeatureRegistry.Add(new Feature<T, ApiResponse<T>>
        {
            FeatureName = $"create{type.Name}",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/create",
            Method = HttpMethod.Put,
            Handler = async (entity, context) =>
            {
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.CreateAsync(entity)
                };
            }
        });
        FeatureRegistry.Add(new Feature<T, ApiResponse<T>>()
        {
            FeatureName = $"update{type.Name}",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/update",
            Method = HttpMethod.Patch,
            Handler = async (entity, context) =>
            {
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.UpdateAsync(entity)
                };
            }
        });
        FeatureRegistry.Add(new Feature<T, ApiResponse<T>>()
        {
            FeatureName = $"delete{type.Name}",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/delete",
            Method = HttpMethod.Delete,
            Handler = async (entity, context) =>
            {
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.DeleteAsync(entity)
                };
            }
        });
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<T>>>()
        {
            FeatureName = $"listAll{type.Name}",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/list",
            Method = HttpMethod.Get,
            Handler = async (entity, context) =>
            {
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.FindAllAsync()
                };
            }
        });
        FeatureRegistry.Add(new Feature<object, ApiResponse<T>>()
        {
            FeatureName = $"load{type.Name}",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/{{id}}",
            Method = HttpMethod.Get,
            Handler = async (_, context) =>
            {
                if (!context.Request.RouteValues.TryGetValue("id", out var id))
                {
                    throw new MalformedUrlException("Invalid/Missing parameter id from the URL");
                }

                if (id is null)
                {
                    throw new MalformedUrlException("Invalid/Missing parameter id from the URL");
                }
                
                var objectId = Guid.Parse(id.ToString()!);
                
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.FindByIdAsync(objectId)
                };
            }
        });
    }
}