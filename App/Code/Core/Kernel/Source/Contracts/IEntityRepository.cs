namespace LLE.Kernel.Contracts
{
    public interface IEntityRepository<T> : IDatabaseAdapter where T : class
    {
        public Task<T> CreateAsync(T item);
        public Task<T> UpdateAsync(T item);
        public Task<T> DeleteAsync(T item);
        public Task<T> FindByIdAsync(Guid id);
        public Task<List<T>> FindAllAsync();
    }
}