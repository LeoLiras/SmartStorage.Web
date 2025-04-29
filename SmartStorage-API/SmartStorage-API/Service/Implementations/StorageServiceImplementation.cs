using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;
using SmartStorage_API.DTO;
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

        public Product CreateNewProduct(Product product)
        {
            try
            {
                _context.Add(product);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

            return product;
        }

        public List<Product> FindAllProducts()
        {
            return _context.Products.OrderBy(q => q.Name).ToList();
        }

        public List<SaleDTO> FindAllSales()
        {
            var querySale = from sale in _context.Sales
                            join enter in _context.Enters on sale.IdEnter equals enter.Id
                            join product in _context.Products on enter.IdProduct equals product.Id
                            join shelf in _context.Shelves on enter.IdShelf equals shelf.Id
                            select new SaleDTO
                            {
                                productName = product.Name,
                                shelfName = shelf.Name,
                                saleQntd = sale.Qntd,
                                saleData = sale.DateSale,
                                enterPrice = enter.Price,
                            };

            return querySale.OrderBy(q => q.productName).ToList();
        }

        public List<ShelfDTO> FindAllShelves()
        {
            var queryEnter = from enter in _context.Enters
                             join product in _context.Products on enter.IdProduct equals product.Id
                             join shelf in _context.Shelves on enter.IdShelf equals shelf.Id
                             select new ShelfDTO
                             {
                                 productName = product.Name,
                                 shelfName = shelf.Name,
                                 qntd = enter.Qntd,
                                 allocateData = shelf.DataRegister,
                                 price = enter.Price,
                             };

            return queryEnter.OrderBy(q => q.productName).ToList();
        }

        public Product FindProductById(int id)
        {
            return _context.Products.SingleOrDefault(x => x.Id.Equals(id));
        }
    }
}
