namespace ExpenseTrackingSystem.Interfaces
{
    public interface IRepository<K, T> where T : class
    {
        Task<T> Add(T item);
        Task<T> Update(K id, T item);
        Task<T> Delete(K id);
        Task<T> GetByID(K id);
        Task<IEnumerable<T>> GetAll();
    }
}