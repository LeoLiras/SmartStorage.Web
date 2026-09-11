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

        #endregion

        #region Construtores

        public ShelfBusinessImplementation(SmartStorageContext context, IProductStockMovementRepository movementRepository)
        {
            _context = context;
            _converterShelf = new ShelfConverter();
            _converterEnter = new EnterConverter(_context);
            _movementRepository = movementRepository;
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

        public EnterVO FindProductInShelfById(int enterId)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID informado");

            return _converterEnter.Parse(enter);
        }

        public ShelfVO CreateNewShelf(ShelfVO newShelf)
        {
            var shelf = new Shelf
            {
                SheName = newShelf.Name,
                SheDataRegister = DateTime.UtcNow,
            };

            _context.Add(shelf);
            _context.SaveChanges();

            return _converterShelf.Parse(shelf);
        }

        public ShelfVO UpdateShelf(int shelfId, string shelfName)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId == shelfId);

            if (shelf == null)
                throw new Exception("Prateleira não encontrada com o ID informado");

            shelf.SheName = shelfName;

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
            _movementRepository.TransferProductBetweenLocations(
                newAllocation.ProductId,
                fromShelfId: null,
                toShelfId: newAllocation.ShelfId,
                quantity: newAllocation.ProductQuantity,
                shelfPrice: newAllocation.ProductPrice,
                date: newAllocation.DateEnter);

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

        #endregion
    }
}
