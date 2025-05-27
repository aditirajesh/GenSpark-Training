using FirstAPI.Interfaces;
namespace FirstAPI.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {

        protected List<T> _items = new List<T>();
        public abstract K GenerateID();
        public abstract T GetByID(K id);
        public abstract ICollection<T> GetAll();

        public T Add(T item)
        {
            var id = GenerateID();
            var property = typeof(T).GetProperty("Id");
            if (property != null)
            {
                property.SetValue(this, item);
            }

            if (_items.Contains(item))
            {
                throw new Exception("Duplicate entity found");
            }
            _items.Add(item);
            return item;
        }

        public T Delete(K id)
        {
            var delete_item = GetByID(id);
            if (delete_item == null)
            {
                throw new Exception("Item not found");
            }
            _items.Remove(delete_item);
            return delete_item;
        }
    }
}
