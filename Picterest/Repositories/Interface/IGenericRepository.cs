namespace Picterest.Repositories.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByPkAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        void DeleteAsync(T entity);
    }
}
