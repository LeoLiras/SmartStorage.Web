using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    public class SaleBusinessImplementation : ISaleBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly SaleConverter _converter;

        #endregion

        #region Construtores

        public SaleBusinessImplementation(SmartStorageContext context)
        {
            _context = context;
            _converter = new SaleConverter(_context);
        }

        #endregion

        #region Métodos

        public List<SaleVO> FindAllSales()
        {
            return _converter.Parse(_context.Sales.OrderBy(s => s.Id).ToList());
        }

        public SaleVO FindSaleById(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.Id.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            return _converter.Parse(sale);
        }

        public SaleVO CreateNewSale(int productId, int saleQntd)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.IdProduct.Equals(productId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID do Produto informado.");

            if (enter.Qntd >= saleQntd)
            {
                enter.Qntd -= saleQntd;

                var sale = new Sale
                {
                    IdEnter = enter.Id,
                    Qntd = saleQntd,
                    DateSale = DateTime.UtcNow,
                };

                _context.Sales.Add(sale);
                _context.SaveChanges();

                return _converter.Parse(sale);
            }
            else
            {
                throw new Exception("Quantidade indisponível para venda.");
            }
        }

        public SaleVO UpdateSale(int saleId, int saleQntd)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.Id == saleId);

            if (sale == null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.Id.Equals(sale.IdEnter));

            if (enter == null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado");

            if (saleQntd < sale.Qntd)
                enter.Qntd += saleQntd;
            else
            {
                var rest = (saleQntd - sale.Qntd);

                if (enter.Qntd > rest)
                    enter.Qntd -= rest;
                else
                    throw new Exception("Não há quantidade suficiente na entrada do Produto para realizar essa atualização");
            }

            sale.Qntd = saleQntd;

            _context.SaveChanges();

            var shelf = _context.Shelves.FirstOrDefault(s => s.Id == enter.IdShelf);

            return _converter.Parse(sale);
        }

        public SaleVO DeleteSale(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.Id.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.Id.Equals(sale.IdEnter));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado.");

            enter.Qntd += sale.Qntd;

            _context.Sales.Remove(sale);
            _context.SaveChanges();

            return _converter.Parse(sale);
        }

        #endregion
    }
}
