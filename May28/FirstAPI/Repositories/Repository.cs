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

        public async Task<T> Add(T item)
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
            var update_item = await GetByID(id);
            if (update_item != null)
            {
                _clinicContext.Entry(update_item).CurrentValues.SetValues(item);
                await _clinicContext.SaveChangesAsync();
                return update_item;
            }
            throw new Exception("Item not found");

        }
    }
}
