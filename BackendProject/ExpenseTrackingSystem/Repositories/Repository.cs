using ExpenseTrackingSystem.Contexts;
using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;

using Microsoft.EntityFrameworkCore;
namespace ExpenseTrackingSystem.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {

        protected readonly ExpenseContext _expenseContext;

        public Repository(ExpenseContext ExpenseContext)
        {
            _expenseContext = ExpenseContext;
        }
        public abstract Task<T> GetByID(K id);
        public abstract Task<IEnumerable<T>> GetAll();

        public virtual async Task<T> Add(T item)
        {
            _expenseContext.Add(item);
            await _expenseContext.SaveChangesAsync();
            return item;
        }

        public async Task<T> Delete(K id)
        {
            var delete_item = await GetByID(id);
            if (delete_item == null)
            {
                throw new Exception("Item not found.");
            }
            _expenseContext.Remove(delete_item);
            await _expenseContext.SaveChangesAsync();
            return delete_item;
        }
        public async Task<T> Update(K id, T item)
        {
            var exists = await _expenseContext.Set<T>().FindAsync(id);
            if (exists == null)
            {
                throw new EntityNotFoundException("Item not found");
            }

            _expenseContext.Set<T>().Update(item);
            await _expenseContext.SaveChangesAsync();
            return item;
        }

    }
}