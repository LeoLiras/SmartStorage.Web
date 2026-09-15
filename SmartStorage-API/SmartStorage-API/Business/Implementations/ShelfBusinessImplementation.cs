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

        private void EnsureShelfFits(int productId, int shelfId, int quantity)
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

            var free = usable - _converterShelf.UsedVolumeOf(shelfId);

            var needed = quantity * product.ProVolume.Value;

            if (needed > free)
                throw new Exception($"A {shelf.SheName} não comporta a alocação: são necessários {Liters(needed)} L e restam {Liters(Math.Max(free, 0))} L dos {Liters(usable)} L úteis (90% do volume).");
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
