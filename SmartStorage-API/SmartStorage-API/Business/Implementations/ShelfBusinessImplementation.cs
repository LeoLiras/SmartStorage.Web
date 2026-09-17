using SmartStorage.Shared.Enum;
using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Repository.Interfaces;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service.Implementations
{
    public class ShelfBusinessImplementation : IShelfBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly ShelfConverter _converterShelf;

        private readonly EnterConverter _converterEnter;

        private readonly IProductStockMovementRepository _movementRepository;

        private readonly IStockAlertBusiness _stockAlert;

        #endregion

        #region Construtores

        public ShelfBusinessImplementation(SmartStorageContext context, IProductStockMovementRepository movementRepository, IStockAlertBusiness stockAlert)
        {
            _context = context;
            _converterShelf = new ShelfConverter(_context);
            _converterEnter = new EnterConverter(_context);
            _movementRepository = movementRepository;
            _stockAlert = stockAlert;
        }

        #endregion

        #region Métodos

        public List<ShelfVO> FindAllShelf()
        {
            return _converterShelf.Parse(_context.Shelves.OrderBy(x => x.SheName).ToList());
        }

        public ShelfVO FindShelfById(int id)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == id);

            if (shelf is null)
                throw new Exception("Prateleira não encontrada com o ID Informado");

            return _converterShelf.Parse(shelf);
        }

        public List<EnterVO> FindAllProductsInShelves()
        {
            return _converterEnter.Parse(_context.Enters.OrderBy(e => e.EntId).ToList());
        }

        public (List<EnterVO> Items, int Total) FindProductsInShelvesPage(int page, int pageSize, string search)
        {
            if (page < 1)
                throw new Exception("A página deve ser maior que zero.");

            if (pageSize < 1 || pageSize > Pagination.MaxPageSize)
                throw new Exception($"O tamanho da página deve estar entre 1 e {Pagination.MaxPageSize}.");

            var query = _context.Enters.Where(e => e.EntQntd > 0);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Product.ProName.Contains(search.Trim()));

            var total = query.Count();

            var enters = query
                .OrderBy(e => e.Shelf.SheName)
                .ThenBy(e => e.Product.ProName)
                .ThenBy(e => e.EntId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (_converterEnter.Parse(enters), total);
        }

        public EnterVO FindProductInShelfById(int enterId)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID informado");

            return _converterEnter.Parse(enter);
        }

        public ShelfVO CreateNewShelf(ShelfVO newShelf)
        {
            ValidateShelfVolume(newShelf.Volume);

            var shelf = new Shelf
            {
                SheName = newShelf.Name,
                SheDataRegister = DateTime.UtcNow,
                SheVolume = newShelf.Volume,
            };

            _context.Add(shelf);
            _context.SaveChanges();

            return _converterShelf.Parse(shelf);
        }

        public ShelfVO UpdateShelf(int shelfId, ShelfVO updatedShelf)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == shelfId);

            if (shelf == null)
                throw new Exception("Prateleira não encontrada com o ID informado");

            ValidateShelfVolume(updatedShelf.Volume);

            shelf.SheName = updatedShelf.Name;

            shelf.SheVolume = updatedShelf.Volume;

            _context.SaveChanges();

            return _converterShelf.Parse(shelf);
        }

        public ShelfVO DeleteShelf(int shelfId)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId.Equals(shelfId));

            if (shelf is null)
                throw new Exception("Prateleira não encontrada com o ID informado");

            var enters = _context.Enters.Where(e => e.EntSheId.Equals(shelfId)).ToList();

            if (enters.Count > 0)
                throw new Exception("Não é possível excluír a prateleira pois há entradas de produtos associadas a ela");

            _context.Shelves.Remove(shelf);
            _context.SaveChanges();

            return _converterShelf.Parse(shelf);
        }

        public EnterVO AllocateProductToShelf(EnterVO newAllocation)
        {
            EnsureSingleShelf(newAllocation.ProductId, newAllocation.ShelfId);

            EnsureShelfFits(newAllocation.ProductId, newAllocation.ShelfId, newAllocation.ProductQuantity);

            var totalBefore = _movementRepository.FindProductTotalBalance(newAllocation.ProductId);

            _movementRepository.TransferProductBetweenLocations(
                newAllocation.ProductId,
                fromShelfId: null,
                toShelfId: newAllocation.ShelfId,
                quantity: newAllocation.ProductQuantity,
                shelfPrice: newAllocation.ProductPrice);

            _stockAlert.NotifyIfBelowMinimum(newAllocation.ProductId, totalBefore, "Alocação");

            var enter = _context.Enters.First(e => e.EntProId == newAllocation.ProductId && e.EntSheId == newAllocation.ShelfId);

            return _converterEnter.Parse(enter);
        }

        public List<EnterVO> AllocateProductsToShelves(List<AllocationBatchItemVO> items)
        {
            if (items is null || items.Count == 0)
                throw new Exception("Selecione ao menos um produto para alocar.");

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();

            var shelfIds = items.Select(i => i.ShelfId).Distinct().ToList();

            var products = _context.Products
                .Where(p => productIds.Contains(p.ProId))
                .ToDictionary(p => p.ProId);

            var shelves = _context.Shelves
                .Where(s => shelfIds.Contains(s.SheId))
                .ToDictionary(s => s.SheId);

            var pendingVolumeByShelf = new Dictionary<int, decimal>();

            var allocatedProducts = new HashSet<int>();

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];

                products.TryGetValue(item.ProductId, out var product);

                shelves.TryGetValue(item.ShelfId, out var shelf);

                var itemName = product is null
                    ? $"Item {index + 1}"
                    : shelf is null
                        ? $"Item {index + 1} ({product.ProName})"
                        : $"Item {index + 1} ({product.ProName} na {shelf.SheName})";

                try
                {
                    if (product is null)
                        throw new Exception("produto não encontrado com o ID informado.");

                    if (shelf is null)
                        throw new Exception("prateleira não encontrada com o ID informado.");

                    if (!allocatedProducts.Add(product.ProId))
                        throw new Exception("o produto aparece mais de uma vez no lote.");

                    if (item.Qntd <= 0)
                        throw new Exception("a quantidade deve ser maior que zero.");

                    if (item.Price <= 0)
                        throw new Exception("o preço deve ser maior que zero.");

                    if (item.Qntd > product.ProQntd)
                        throw new Exception($"saldo insuficiente no depósito: há {product.ProQntd} e o lote pede {item.Qntd}.");

                    EnsureSingleShelf(product.ProId, shelf.SheId);

                    var pendingVolume = pendingVolumeByShelf.GetValueOrDefault(shelf.SheId);

                    EnsureShelfFits(product.ProId, shelf.SheId, item.Qntd, pendingVolume);

                    pendingVolumeByShelf[shelf.SheId] = pendingVolume + item.Qntd * product.ProVolume.Value;
                }
                catch (Exception ex)
                {
                    throw new Exception($"{itemName}: {ex.Message}");
                }
            }

            var totalsBefore = productIds.ToDictionary(
                productId => productId,
                productId => _movementRepository.FindProductTotalBalance(productId));

            var movementDate = DateTime.Now;

            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var item in items)
                    _movementRepository.TransferProductBetweenLocations(
                        item.ProductId,
                        fromShelfId: null,
                        toShelfId: item.ShelfId,
                        quantity: item.Qntd,
                        shelfPrice: item.Price,
                        date: movementDate);

                transaction.Commit();
            }

            foreach (var (productId, totalBefore) in totalsBefore)
                _stockAlert.NotifyIfBelowMinimum(productId, totalBefore, "Alocação");

            var enters = _context.Enters
                .Where(e => productIds.Contains(e.EntProId) && shelfIds.Contains(e.EntSheId))
                .ToList();

            return _converterEnter.Parse(items
                .Select(i => enters.First(e => e.EntProId == i.ProductId && e.EntSheId == i.ShelfId))
                .ToList());
        }

        public EnterVO UndoAllocate(int enterId)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID informado");

            if (enter.EntQntd > 0)
                _movementRepository.TransferProductBetweenLocations(
                    enter.EntProId,
                    fromShelfId: enter.EntSheId,
                    toShelfId: null,
                    quantity: enter.EntQntd);

            return _converterEnter.Parse(enter);
        }

        public EnterVO TransferProductToShelf(int enterId, int toShelfId)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID informado");

            if (enter.EntQntd <= 0)
                throw new Exception("Não há saldo nesta prateleira para transferir.");

            if (!_context.Shelves.Any(s => s.SheId == toShelfId))
                throw new Exception("Prateleira de destino não encontrada.");

            if (toShelfId != enter.EntSheId)
                EnsureShelfFits(enter.EntProId, toShelfId, enter.EntQntd);

            var destinationExists = _context.Enters.Any(e => e.EntProId == enter.EntProId && e.EntSheId == toShelfId);

            _movementRepository.TransferProductBetweenLocations(
                enter.EntProId,
                fromShelfId: enter.EntSheId,
                toShelfId: toShelfId,
                quantity: enter.EntQntd,
                type: TipoMovimentacao.Transferencia,
                shelfPrice: destinationExists ? null : enter.EntPrice);

            var destination = _context.Enters.First(e => e.EntProId == enter.EntProId && e.EntSheId == toShelfId);

            return _converterEnter.Parse(destination);
        }

        public List<EnterVO> CountShelfInventory(int shelfId, InventoryCountVO count)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == shelfId)
                ?? throw new Exception("Prateleira não encontrada com o ID informado");

            var reason = count?.Reason?.Trim();

            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 5 || reason.Length > 180)
                throw new Exception("Informe o motivo do inventário, entre 5 e 180 caracteres.");

            if (count.Items is null || count.Items.Count == 0)
                throw new Exception("Informe ao menos uma quantidade contada.");

            var enterIds = count.Items.Select(i => i.EnterId).Distinct().ToList();

            var enters = _context.Enters
                .Where(e => enterIds.Contains(e.EntId))
                .ToDictionary(e => e.EntId);

            var productNames = _context.Products
                .Where(p => enters.Values.Select(e => e.EntProId).Contains(p.ProId))
                .ToDictionary(p => p.ProId, p => p.ProName);

            var countedEnters = new HashSet<int>();

            var adjustments = new List<(Enter Enter, int Delta)>();

            for (var index = 0; index < count.Items.Count; index++)
            {
                var item = count.Items[index];

                enters.TryGetValue(item.EnterId, out var enter);

                var itemName = enter is null
                    ? $"Item {index + 1}"
                    : $"Item {index + 1} ({productNames.GetValueOrDefault(enter.EntProId)})";

                try
                {
                    if (enter is null)
                        throw new Exception("entrada não encontrada com o ID informado.");

                    if (enter.EntSheId != shelfId)
                        throw new Exception($"o produto não está na {shelf.SheName}.");

                    if (!countedEnters.Add(enter.EntId))
                        throw new Exception("o produto aparece mais de uma vez na contagem.");

                    if (item.CountedQntd < 0)
                        throw new Exception("a quantidade contada não pode ser negativa.");
                }
                catch (Exception ex)
                {
                    throw new Exception($"{itemName}: {ex.Message}");
                }

                if (item.CountedQntd != enter.EntQntd)
                    adjustments.Add((enter, item.CountedQntd - enter.EntQntd));
            }

            if (adjustments.Count == 0)
                throw new Exception("Nenhuma quantidade contada difere do saldo do sistema.");

            var totalsBefore = adjustments
                .Select(a => a.Enter.EntProId)
                .Distinct()
                .ToDictionary(productId => productId, productId => _movementRepository.FindProductTotalBalance(productId));

            var movementDate = DateTime.Now;

            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (var (enter, delta) in adjustments)
                    _movementRepository.CreateNewStockMovement(
                        enter.EntProId,
                        shelfId,
                        TipoMovimentacao.Ajuste,
                        delta,
                        $"Inventário da {shelf.SheName}: {reason}",
                        date: movementDate);

                transaction.Commit();
            }

            foreach (var (productId, totalBefore) in totalsBefore)
                _stockAlert.NotifyIfBelowMinimum(productId, totalBefore, "Inventário");

            return _converterEnter.Parse(adjustments.Select(a => a.Enter).ToList());
        }

        private void EnsureSingleShelf(int productId, int shelfId)
        {
            var otherShelf = _context.Enters
                .Where(e => e.EntProId == productId && e.EntSheId != shelfId && e.EntQntd > 0)
                .Select(e => e.Shelf.SheName)
                .FirstOrDefault();

            if (otherShelf is not null)
                throw new Exception($"O produto já está alocado na {otherShelf}. Um produto só pode ficar em uma prateleira: aloque nela ou transfira o saldo antes.");
        }

        private void EnsureShelfFits(int productId, int shelfId, int quantity, decimal pendingVolume = 0)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProId == productId)
                ?? throw new Exception("Produto não encontrado na base de dados");

            if (product.ProVolume is null)
                throw new Exception("Cadastre o volume do produto antes de alocá-lo em uma prateleira.");

            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == shelfId)
                ?? throw new Exception("Prateleira não encontrada na base de dados");

            if (shelf.SheVolume is null)
                throw new Exception($"Cadastre o volume da {shelf.SheName} antes de alocar produtos nela.");

            var usable = shelf.SheVolume.Value * ShelfVO.UsableFraction;

            var free = usable - _converterShelf.UsedVolumeOf(shelfId) - pendingVolume;

            var needed = quantity * product.ProVolume.Value;

            var batchNote = pendingVolume > 0 ? $", já descontados {Liters(pendingVolume)} L de outros itens do lote" : string.Empty;

            if (needed > free)
                throw new Exception($"A {shelf.SheName} não comporta a alocação: são necessários {Liters(needed)} L e restam {Liters(Math.Max(free, 0))} L dos {Liters(usable)} L úteis (90% do volume{batchNote}).");
        }

        private static void ValidateShelfVolume(decimal? volume)
        {
            if (volume <= 0)
                throw new Exception("O volume da prateleira deve ser maior que zero.");
        }

        private static string Liters(decimal value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        }

        #endregion
    }
}
