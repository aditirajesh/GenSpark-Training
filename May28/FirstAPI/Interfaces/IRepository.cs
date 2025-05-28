using System;
namespace FirstAPI.Interfaces
{
   public interface IRepository<K, T> where T : class
    {
        Task<T> GetByID(K id);
        Task<T> Add(T item);
        Task<T> Delete(K id);
        Task<IEnumerable<T>> GetAll();
        Task<T> Update(K id, T item);
    } 
}
