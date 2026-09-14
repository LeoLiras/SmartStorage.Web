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
            return _converter.Parse(_context.Sales.Where(s => s.SalQntd > s.SalReturnedQntd).OrderBy(s => s.SalId).ToList());
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

            if (saleQntd < sale.SalReturnedQntd)
                throw new Exception($"A quantidade da venda não pode ficar abaixo do que já foi devolvido: {sale.SalReturnedQntd}.");

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

            if (sale.SalReturnedQntd > 0)
                throw new Exception("Não é possível cancelar a venda pois ela já tem devoluções registradas.");

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

        public SaleVO ReturnSale(int saleId, int quantity)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.SalId == saleId);

            if (sale is null)
                throw new Exception("Venda não encontrada com o ID informado");

            if (quantity <= 0)
                throw new Exception("A quantidade a devolver deve ser maior que zero.");

            var remaining = sale.SalQntd - sale.SalReturnedQntd;

            if (quantity > remaining)
                throw new Exception($"A devolução excede o que resta da venda: restam {remaining} e a devolução pede {quantity}.");

            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(sale.SalEntId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID de Venda informado.");

            sale.SalReturnedQntd += quantity;

            _movementRepository.CreateNewStockMovement(
                enter.EntProId,
                shelfId: null,
                TipoMovimentacao.Devolucao,
                quantity,
                reason: $"Devolução da venda {sale.SalId}");

            return _converter.Parse(sale);
        }

        #endregion
    }
}
