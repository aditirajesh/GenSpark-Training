using System;
namespace FirstAPI.Interfaces
{
   public interface IRepository<K, T> where T : class
    {
        T GetByID(K id);
        T Add(T item);
        T Delete(K id);
        ICollection<T> GetAll();
        K GenerateID();
    } 
}
