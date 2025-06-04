using FirstAPI.Contexts;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FirstAPI.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {

        protected readonly ClinicContext _clinicContext;

        public Repository(ClinicContext clinicContext)
        {
            _clinicContext = clinicContext;
        }
        public abstract Task<T> GetByID(K id);
        public abstract Task<IEnumerable<T>> GetAll();

        public virtual async Task<T> Add(T item)
        {
            _clinicContext.Add(item);
            await _clinicContext.SaveChangesAsync();
            return item;
        }

        public async Task<T> Delete(K id)
        {
            var delete_item = await GetByID(id);
            if (delete_item != null)
            {
                _clinicContext.Remove(delete_item);
                await _clinicContext.SaveChangesAsync();
                return delete_item;
            }
            throw new Exception("Item not found");

        }
        public async Task<T> Update(K id, T item)
        {
            var existingItem = await _clinicContext.Set<T>().FindAsync(id);
            if (existingItem == null)
            {
                throw new Exception("Item not found");
            }

            _clinicContext.Entry(existingItem).CurrentValues.SetValues(item);
            await _clinicContext.SaveChangesAsync();
            return existingItem;
        }

    }
}
