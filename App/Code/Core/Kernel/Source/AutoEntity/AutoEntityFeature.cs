using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Dto;
using LLE.Kernel.Exceptions;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;

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
                var uc = UserContext.FromHttpContext(context);
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.CreateAsync(entity, uc, DataOptions.Default)
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
                var uc = UserContext.FromHttpContext(context);
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.UpdateAsync(entity, uc, DataOptions.Default)
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
                var uc = UserContext.FromHttpContext(context);
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.DeleteAsync(entity, uc, DataOptions.Default)
                };
            }
        });
        FeatureRegistry.Add(new Feature<object, ApiResponse<T>>()
        {
            FeatureName = $"delete{type.Name}ById",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/deleteById/{{id}}",
            Method = HttpMethod.Delete,
            Handler = async (_, context) =>
            {
                if (!context.Request.RouteValues.TryGetValue("id", out var id))
                    throw new MalformedUrlException("Invalid/Missing parameter id from the URL");
                if (id is null)
                    throw new MalformedUrlException("Invalid/Missing parameter id from the URL");

                var objectId = Guid.Parse(id.ToString()!);
                var uc = UserContext.FromHttpContext(context);
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.DeleteAsync(objectId, uc, DataOptions.Default)
                };
            }
        });
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<T>>>()
        {
            FeatureName = $"listAll{type.Name}",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/list",
            Method = HttpMethod.Get,
            Handler = async (_, context) =>
            {
                var uc = UserContext.FromHttpContext(context);

                SortOption? sortBy = null;
                if (context.Request.Query.TryGetValue("sortBy", out var sortField) && !string.IsNullOrWhiteSpace(sortField))
                {
                    var ascending = !context.Request.Query.TryGetValue("sortAsc", out var sortAsc) || sortAsc != "false";
                    sortBy = new SortOption { Field = sortField!, Ascending = ascending };
                }

                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new ApiResponse<List<T>>
                {
                    Success = true,
                    Data = await repository.FindAllAsync(uc, DataOptions.Default, sortBy)
                };
            }
        });
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<T>>>()
        {
            FeatureName = $"list{type.Name}Paged",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/list/{{pageNum}}/{{size}}",
            Method = HttpMethod.Get,
            Handler = async (_, context) =>
            {
                if (!context.Request.RouteValues.TryGetValue("pageNum", out var pageNum) || pageNum is null)
                    throw new MalformedUrlException("Invalid/Missing parameter pageNum from the URL");

                if (!context.Request.RouteValues.TryGetValue("size", out var size) || size is null)
                    throw new MalformedUrlException("Invalid/Missing parameter size from the URL");

                var uc = UserContext.FromHttpContext(context);

                SortOption? sortBy = null;
                if (context.Request.Query.TryGetValue("sortBy", out var sortField) && !string.IsNullOrWhiteSpace(sortField))
                {
                    var ascending = !context.Request.Query.TryGetValue("sortAsc", out var sortAsc) || sortAsc != "false";
                    sortBy = new SortOption { Field = sortField!, Ascending = ascending };
                }

                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                var pagination = new Pagination { PageNo = int.Parse(pageNum.ToString()!), Limit = int.Parse(size.ToString()!) };
                return new ApiResponse<List<T>>
                {
                    Success = true,
                    Data = await repository.FindAllAsync(uc, DataOptions.Default, sortBy, pagination)
                };
            }
        });
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<T>>>()
        {
            FeatureName = $"list{type.Name}PagedSorted",
            FeatureGroup = type.Name.ToLower(),
            Route = $"/api/{type.Name.ToLower()}/list/{{pageNum}}/{{size}}/{{sortField}}/{{sortDir}}",
            Method = HttpMethod.Get,
            Handler = async (_, context) =>
            {
                if (!context.Request.RouteValues.TryGetValue("pageNum", out var pageNum) || pageNum is null)
                    throw new MalformedUrlException("Invalid/Missing parameter pageNum from the URL");

                if (!context.Request.RouteValues.TryGetValue("size", out var size) || size is null)
                    throw new MalformedUrlException("Invalid/Missing parameter size from the URL");

                if (!context.Request.RouteValues.TryGetValue("sortField", out var sortField) || sortField is null)
                    throw new MalformedUrlException("Invalid/Missing parameter sortField from the URL");

                if (!context.Request.RouteValues.TryGetValue("sortDir", out var sortDir) || sortDir is null)
                    throw new MalformedUrlException("Invalid/Missing parameter sortDir from the URL");

                var uc = UserContext.FromHttpContext(context);
                var sortBy = new SortOption { Field = sortField.ToString()!, Ascending = sortDir.ToString() != "desc" };
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                var pagination = new Pagination { PageNo = int.Parse(pageNum.ToString()!), Limit = int.Parse(size.ToString()!) };
                return new ApiResponse<List<T>>
                {
                    Success = true,
                    Data = await repository.FindAllAsync(uc, DataOptions.Default, sortBy, pagination)
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
                
                var uc = UserContext.FromHttpContext(context);
                var repository = (T1) RepositoryCatalog.GetRepository(typeof(T1));
                return new()
                {
                    Success = true,
                    Data = await repository.FindByIdAsync(objectId, uc, DataOptions.Default)
                };
            }
        });
    }
}