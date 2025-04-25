using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;
using System.Runtime.CompilerServices;

namespace SmartStorage_API.Service.Implementations
{
    public class StorageServiceImplementation : IStorageService
    {
        private readonly SmartStorageContext _context;

        public StorageServiceImplementation(SmartStorageContext context)
        {
            _context = context;
        }

        public List<Product> FindAll()
        {
            return _context.Products.OrderBy(q => q.Name).ToList();
        }
    }
}
