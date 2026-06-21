using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Security;

namespace LLE.Kernel.Contracts
{
    public interface IEntityRepository<T> : IDatabaseAdapter where T : class
    {
        public Task<T> CreateAsync(T item, UserContext context, DataOptions options);
        public Task<T> UpdateAsync(T item, UserContext context, DataOptions options);
        public Task<T> DeleteAsync(T item, UserContext context, DataOptions options);
        public Task<T?> FindByIdAsync(Guid id, UserContext context, DataOptions options);
        public Task<List<T>> FindAllAsync(UserContext context, DataOptions options, SortOption? sortBy = null, Pagination? pagination = null);
        public Task<int> TotalRecords(UserContext context, DataOptions options);
    }
}
