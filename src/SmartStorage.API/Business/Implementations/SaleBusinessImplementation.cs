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

        private readonly IStockAlertBusiness _stockAlert;

        #endregion

        #region Construtores

        public SaleBusinessImplementation(SmartStorageContext context, IProductStockMovementRepository movementRepository, IStockAlertBusiness stockAlert)
        {
            _context = context;
            _converter = new SaleConverter(_context);
            _movementRepository = movementRepository;
            _stockAlert = stockAlert;
        }

        #endregion

        #region Métodos

        public List<SaleVO> FindAllSales()
        {
            return _converter.Parse(_context.Sales.Where(s => s.SalQntd > s.SalReturnedQntd).OrderBy(s => s.SalId).ToList());
        }

        public (List<SaleVO> Items, int Total) FindSalesPage(int page, int pageSize, string search)
        {
            if (page < 1)
                throw new Exception("A página deve ser maior que zero.");

            if (pageSize < 1 || pageSize > Pagination.MaxPageSize)
                throw new Exception($"O tamanho da página deve estar entre 1 e {Pagination.MaxPageSize}.");

            var query = _context.Sales.Where(s => s.SalQntd > s.SalReturnedQntd);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Enter.Product.ProName.Contains(search.Trim()));

            var total = query.Count();

            var sales = query
                .OrderByDescending(s => s.SalDateSale)
                .ThenByDescending(s => s.SalId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (_converter.Parse(sales), total);
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
                SalPrice = enter.EntPrice,
            };

            var totalBefore = _movementRepository.FindProductTotalBalance(enter.EntProId);

            _context.Sales.Add(sale);

            _movementRepository.CreateNewStockMovement(
                enter.EntProId,
                enter.EntSheId,
                TipoMovimentacao.Venda,
                -saleQntd);

            _stockAlert.NotifyIfBelowMinimum(enter.EntProId, totalBefore, "Venda");

            return _converter.Parse(sale);
        }

        public List<SaleVO> CreateNewSales(List<SaleBatchItemVO> items, DateTime dateSale)
        {
            if (items is null || items.Count == 0)
                throw new Exception("O carrinho está vazio.");

            var enterIds = items.Select(i => i.IdEnter).Distinct().ToList();

            var enters = _context.Enters
                .Where(e => enterIds.Contains(e.EntId))
                .ToDictionary(e => e.EntId);

            var names = _context.Enters
                .Where(e => enterIds.Contains(e.EntId))
                .Select(e => new { e.EntId, Description = e.Product.ProName + " na " + e.Shelf.SheName })
                .ToDictionary(e => e.EntId, e => e.Description);

            var requestedByEnter = new Dictionary<int, int>();

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];

                var itemName = names.TryGetValue(item.IdEnter, out var description)
                    ? $"Item {index + 1} ({description})"
                    : $"Item {index + 1}";

                if (!enters.TryGetValue(item.IdEnter, out var enter))
                    throw new Exception($"{itemName}: entrada não encontrada com o ID informado.");

                if (item.Qntd <= 0)
                    throw new Exception($"{itemName}: a quantidade da venda deve ser maior que zero.");

                var requested = requestedByEnter.GetValueOrDefault(enter.EntId) + item.Qntd;

                if (requested > enter.EntQntd)
                    throw new Exception($"{itemName}: saldo insuficiente na prateleira: há {enter.EntQntd} e o carrinho pede {requested}.");

                requestedByEnter[enter.EntId] = requested;
            }

            var totalsBefore = enters.Values
                .Select(e => e.EntProId)
                .Distinct()
                .ToDictionary(productId => productId, productId => _movementRepository.FindProductTotalBalance(productId));

            var movementDate = DateTime.Now;

            var sales = new List<Sale>();

            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var item in items)
                {
                    var enter = enters[item.IdEnter];

                    var sale = new Sale
                    {
                        SalEntId = enter.EntId,
                        SalQntd = item.Qntd,
                        SalDateSale = dateSale,
                        SalPrice = enter.EntPrice,
                    };

                    _context.Sales.Add(sale);

                    _movementRepository.CreateNewStockMovement(
                        enter.EntProId,
                        enter.EntSheId,
                        TipoMovimentacao.Venda,
                        -item.Qntd,
                        date: movementDate);

                    sales.Add(sale);
                }

                transaction.Commit();
            }

            foreach (var (productId, totalBefore) in totalsBefore)
                _stockAlert.NotifyIfBelowMinimum(productId, totalBefore, "Venda");

            return _converter.Parse(sales);
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

            var totalBefore = _movementRepository.FindProductTotalBalance(enter.EntProId);

            sale.SalQntd = saleQntd;

            if (quantityDelta == 0)
                _context.SaveChanges();
            else
                _movementRepository.CreateNewStockMovement(
                    enter.EntProId,
                    enter.EntSheId,
                    TipoMovimentacao.Venda,
                    quantityDelta);

            if (quantityDelta < 0)
                _stockAlert.NotifyIfBelowMinimum(enter.EntProId, totalBefore, "Venda");

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
