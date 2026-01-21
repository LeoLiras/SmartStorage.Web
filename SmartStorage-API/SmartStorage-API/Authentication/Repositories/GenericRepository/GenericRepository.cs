using Microsoft.EntityFrameworkCore;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Authentication.Repositories.GenericRepository
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity

    {
        protected SmartStorageContext _context;
        private DbSet<T> _dataset;

        public GenericRepository(SmartStorageContext context)
        {
            _context = context;
            _dataset = context.Set<T>();
        }

        public List<T> FindAll()
        {
            return _dataset.ToList();
        }

        public T FindById(long id)
        {
            return _dataset.Find(id);
        }

        public T Create(T item)
        {
            _context.Add(item);
            _context.SaveChanges();
            return item;
        }

        public T Update(T item)
        {
            var existingItem = _dataset.Find(item.Id);
            if (existingItem == null) return null;

            _context.Entry(existingItem).CurrentValues.SetValues(item);
            _context.SaveChanges();
            return item;
        }
        public void Delete(long id)
        {
            var existingItem = _dataset.Find(id);
            if (existingItem == null) return;
            _context.Remove(existingItem);
            _context.SaveChanges();
        }

        public bool Exists(long id)
        {
            return _dataset.Any(e => e.Id == id);
        }

        public List<T> FindWithPagedSearch(string query)
        {
            //return _dataset.FromSqlRaw(query).ToList();
            return [.. _dataset.FromSqlRaw(query)];
        }

        public int GetCount(string query)
        {
            using var connection = _context.Database.GetDbConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = query;

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
    }
}

