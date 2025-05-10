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
                var productSearch = _context.Products.FirstOrDefault(x => x.Name == product.Name);

                if(productSearch != null)
                {
                    productSearch.Qntd += product.Qntd;
                    _context.SaveChanges();

                    return productSearch;
                }
                else
                {
                    _context.Add(product);
                    _context.SaveChanges();

                    return product;
                }
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Product> FindAllProducts()
        {
            return _context.Products.OrderBy(q => q.Name).ToList();
        }

        public Product FindProductById(int id)
        {
            return _context.Products.SingleOrDefault(x => x.Id.Equals(id));
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

        public SaleDTO CreateNewSale(SaleDTO newSale)
        {
            var query = from enter in _context.Enters
                        join product in _context.Products on enter.IdProduct equals product.Id
                        join shelf in _context.Shelves on enter.IdShelf equals shelf.Id
                        select new SaleDTO
                        {
                            productName = product.Name,
                            enterId = (int)enter.Id,
                            productId = product.Id,
                            shelfName = shelf.Name,
                            saleQntd = enter.Qntd,
                            enterPrice = enter.Price,
                        };

            var enterFromSale = query.Where(e => e.productId == newSale.productId).FirstOrDefault();

            if (enterFromSale == null) return null;

            if (enterFromSale.saleQntd >= newSale.saleQntd)
            {
                var enter = _context.Enters.Where(e => e.Id == enterFromSale.enterId).FirstOrDefault();

                if (enter == null) return null;

                enter.Qntd -= newSale.saleQntd;

                var sale = new Sale
                {
                    IdEnter = (int)enterFromSale.enterId,
                    Qntd = newSale.saleQntd,
                    DateSale = DateTime.UtcNow,
                };

                _context.Add(sale);
                _context.SaveChanges();

                var newSaleReturn = new SaleDTO
                {
                    productName = enterFromSale.productName,
                    productId = enterFromSale.productId,
                    shelfName = enterFromSale.shelfName,
                    enterId = (int)enterFromSale.enterId,
                    enterPrice = enterFromSale.enterPrice,
                    saleQntd = newSale.saleQntd,
                    saleData = DateTime.UtcNow,
                    total = newSale.saleQntd * enterFromSale.enterPrice
                };

                return newSaleReturn;
            }
            else
            {
                return null;
            }
        }

        public List<ShelfDTO> FindAllShelves()
        {
            var queryEnter = from enter in _context.Enters
                             join product in _context.Products on enter.IdProduct equals product.Id
                             join shelf in _context.Shelves on enter.IdShelf equals shelf.Id
                             select new ShelfDTO
                             {
                                 productName = product.Name,
                                 productId = product.Id,
                                 shelfName = shelf.Name,
                                 qntd = enter.Qntd,
                                 allocateData = shelf.DataRegister,
                                 price = enter.Price,
                             };

            return queryEnter.OrderBy(q => q.productName).ToList();
        }

        public Enter AllocateProductToShelf(AllocateProductToShelfDTO newAllocation)
        {
            var product = _context.Products.Where(p => p.Id == newAllocation.ProductId).FirstOrDefault();
            
            bool alreadyAllocated = _context.Enters.Any(e => e.IdProduct == newAllocation.ProductId);

            try
            {
                if (product != null)
                {
                    if (product.Qntd >= newAllocation.ProductQuantity)
                    {
                        product.Qntd = product.Qntd - newAllocation.ProductQuantity;

                        var enter = _context.Enters.Where(e => e.IdProduct == newAllocation.ProductId).FirstOrDefault();

                        var shelf = _context.Shelves.Where(s => s.Id == newAllocation.ShelfId).FirstOrDefault();

                        if (enter != null && shelf != null)
                        {
                            if (alreadyAllocated && enter.IdShelf == shelf.Id && enter.Price == newAllocation.ProductPrice)
                            {
                                enter.Qntd += newAllocation.ProductQuantity;
                                _context.SaveChanges();

                                return enter;
                            }
                            else
                            {
                                var newEnterProduct = new Enter
                                {
                                    IdProduct = (int)product.Id,
                                    IdShelf = (int)shelf.Id,
                                    Qntd = newAllocation.ProductQuantity,
                                    DateEnter = DateTimeOffset.UtcNow.UtcDateTime,
                                    Price = newAllocation.ProductPrice
                                };

                                _context.Enters.Add(newEnterProduct);
                                _context.SaveChanges();

                                return newEnterProduct;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }
    }
}
