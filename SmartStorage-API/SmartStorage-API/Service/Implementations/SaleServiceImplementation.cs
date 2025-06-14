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
                                saleId = sale.Id,
                                saleProductName = product.Name,
                                saleShelfName = shelf.Name,
                                saleSaleQntd = sale.Qntd,
                                saleSaleData = sale.DateSale,
                                saleEnterPrice = enter.Price,
                                saleProductId = product.Id,
                                saleTotal = sale.Qntd * enter.Price,
                                saleEnterId = enter.Id
                            };

            return querySale.OrderBy(q => q.saleProductName).ToList();
        }

        public SaleDTO CreateNewSale(int productId, int saleQntd)
        {
            var query = from enter in _context.Enters
                        join product in _context.Products on enter.IdProduct equals product.Id
                        join shelf in _context.Shelves on enter.IdShelf equals shelf.Id
                        select new SaleDTO
                        {
                            saleProductName = product.Name,
                            saleEnterId = (int)enter.Id,
                            saleProductId = product.Id,
                            saleShelfName = shelf.Name,
                            saleSaleQntd = enter.Qntd,
                            saleEnterPrice = enter.Price,
                        };

            var enterFromSale = query.Where(e => e.saleProductId == productId).FirstOrDefault();

            if (enterFromSale == null) 
                throw new Exception("Não foram encontradas entradas de produto com o ID informado.");

            if (enterFromSale.saleSaleQntd >= saleQntd)
            {
                var enter = _context.Enters.Where(e => e.Id == enterFromSale.saleEnterId).FirstOrDefault();

                if (enter == null)
                    throw new Exception("Objeto de entrada não encontrado.");

                enter.Qntd -= saleQntd;

                var sale = new Sale
                {
                    IdEnter = (int)enterFromSale.saleEnterId,
                    Qntd = saleQntd,
                    DateSale = DateTime.UtcNow,
                };

                _context.Add(sale);
                _context.SaveChanges();

                var newSaleReturn = new SaleDTO
                {
                    saleProductName = enterFromSale.saleProductName,
                    saleProductId = enterFromSale.saleProductId,
                    saleShelfName = enterFromSale.saleShelfName,
                    saleEnterId = (int)enterFromSale.saleEnterId,
                    saleEnterPrice = enterFromSale.saleEnterPrice,
                    saleSaleQntd = saleQntd,
                    saleSaleData = DateTime.UtcNow,
                    saleTotal = saleQntd * enterFromSale.saleEnterPrice
                };

                return newSaleReturn;
            }
            else
            {
                throw new Exception("Quantidade indisponível para venda.");
            }
        }

        public SaleDTO UpdateSale(int saleId, int productId, int saleQntd)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.Id == saleId);

            if (sale == null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.IdProduct == productId);

            if (enter == null)
                throw new Exception("Entrada não encontrada com o ID de produto informado");

            if(saleQntd < sale.Qntd)
                enter.Qntd += saleQntd;
            else
            {
                if (enter.Qntd > (saleQntd - sale.Qntd))
                    enter.Qntd -= saleQntd;
                else
                    throw new Exception("Não há quantidade suficiente na entrada para realizar essa atualização");
            }

            sale.Qntd = saleQntd;

            _context.SaveChanges(); 

            var shelf = _context.Shelves.FirstOrDefault(s => s.Id == enter.IdShelf);

            var newSaleDetails = new SaleDTO
            {
                saleProductId = enter.ProductId,
                saleShelfName = shelf.Name,
                saleEnterId = enter.Id,
                saleEnterPrice = enter.Price,
                saleSaleQntd = saleQntd,
                saleSaleData = DateTime.UtcNow,
                saleTotal = saleQntd * enter.Price,
            };
            
            return newSaleDetails;
        }

        public Sale DeleteSale(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.Id.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.Id.Equals(sale.IdEnter));

            if (enter is null)
                throw new Exception("Entrada não encontrada para essa Venda");

            enter.Qntd += sale.Qntd;

            _context.Remove(sale);

            _context.SaveChanges();

            return sale;
        }

        #endregion
    }
}
