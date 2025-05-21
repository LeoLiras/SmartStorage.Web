using SmartStorage_API.DTO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    public class SaleServiceImplementation : ISaleService
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        #endregion

        #region Construtores

        public SaleServiceImplementation(SmartStorageContext context)
        {
            _context = context;
        }

        #endregion

        #region Métodos

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

        #endregion
    }
}
