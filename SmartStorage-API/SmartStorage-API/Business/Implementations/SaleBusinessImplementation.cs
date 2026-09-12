using SmartStorage.Shared.Enum;
using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Repository.Interfaces;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service.Implementations
{
    public class SaleBusinessImplementation : ISaleBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly SaleConverter _converter;

        private readonly IProductStockMovementRepository _movementRepository;

        #endregion

        #region Construtores

        public SaleBusinessImplementation(SmartStorageContext context, IProductStockMovementRepository movementRepository)
        {
            _context = context;
            _converter = new SaleConverter(_context);
            _movementRepository = movementRepository;
        }

        #endregion

        #region Métodos

        public List<SaleVO> FindAllSales()
        {
            return _converter.Parse(_context.Sales.OrderBy(s => s.SalId).ToList());
        }

        public SaleVO FindSaleById(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            return _converter.Parse(sale);
        }

        public SaleVO CreateNewSale(int enterId, int saleQntd, DateTime dateSale)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID do Produto informado.");

            if (saleQntd <= 0)
                throw new Exception("A quantidade da venda deve ser maior que zero.");

            var sale = new Sale
            {
                SalEntId = enter.EntId,
                SalQntd = saleQntd,
                SalDateSale = dateSale,
            };

            _context.Sales.Add(sale);

            _movementRepository.CreateNewStockMovement(
                enter.EntProId,
                enter.EntSheId,
                TipoMovimentacao.Venda,
                -saleQntd);

            return _converter.Parse(sale);
        }

        public SaleVO UpdateSale(int saleId, int saleQntd)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId == saleId);

            if (sale == null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(sale.SalEntId));

            if (enter == null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado");

            if (saleQntd <= 0)
                throw new Exception("A quantidade da venda deve ser maior que zero.");

            var quantityDelta = sale.SalQntd - saleQntd;

            sale.SalQntd = saleQntd;

            if (quantityDelta == 0)
                _context.SaveChanges();
            else
                _movementRepository.CreateNewStockMovement(
                    enter.EntProId,
                    enter.EntSheId,
                    TipoMovimentacao.Venda,
                    quantityDelta);

            return _converter.Parse(sale);
        }

        public SaleVO DeleteSale(int saleId)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId.Equals(saleId));

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(sale.SalEntId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado.");

            _context.Sales.Remove(sale);

            if (sale.SalQntd > 0)
                _movementRepository.CreateNewStockMovement(
                    enter.EntProId,
                    enter.EntSheId,
                    TipoMovimentacao.Venda,
                    sale.SalQntd);
            else
                _context.SaveChanges();

            return _converter.Parse(sale);
        }

        #endregion
    }
}
