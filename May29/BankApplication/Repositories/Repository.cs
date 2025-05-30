using BankApplication.Contexts;
using BankApplication.Interfaces;
namespace BankApplication.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected readonly BankingContext _bankingcontext;

        public Repository(BankingContext bankingContext)
        {
            _bankingcontext = bankingContext;
        }
        public abstract Task<T> GetById(K id);
        public abstract Task<IEnumerable<T>> GetAll();

        public async Task<T> Add(T item)
        {
            _bankingcontext.Add(item);
            await _bankingcontext.SaveChangesAsync();
            return item;
        }

        public async Task<T> Update(K id, T item)
        {
            var update_item = await GetById(id);
            if (update_item == null)
            {
                throw new Exception("item not found");
            }
            _bankingcontext.Entry(update_item).CurrentValues.SetValues(item);
            await _bankingcontext.SaveChangesAsync();
            return item;
        }

        public async Task<T> Delete(K id)
        {
            var delete_item = await GetById(id);
            if (delete_item == null)
            {
                throw new Exception("Item not found.");
            }
            _bankingcontext.Remove(delete_item);
            await _bankingcontext.SaveChangesAsync();
            return delete_item;
        }
    }

}
