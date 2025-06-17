using SmartStorage_API.DTO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    public class ShelfServiceImplementation : IShelfService
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        #endregion

        #region Construtores

        public ShelfServiceImplementation(SmartStorageContext context)
        {
            _context = context;
        }

        #endregion

        #region Métodos

        public List<Shelf> FindAllShelf()
        {
            return _context.Shelves.OrderBy(x => x.Name).ToList();
        }

        public Shelf FindShelfById(int id)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.Id == id);

            if (shelf is null)
                throw new Exception("Prateleira não encontrada com o ID Informado");

            return shelf;
        }

        public List<ShelfDTO> FindAllProductsInShelves()
        {
            var queryEnter = from enter in _context.Enters
                             join product in _context.Products on enter.IdProduct equals product.Id
                             join shelf in _context.Shelves on enter.IdShelf equals shelf.Id
                             select new ShelfDTO
                             {
                                 enterId = enter.Id,
                                 productName = product.Name,
                                 productId = product.Id,
                                 shelfName = shelf.Name,
                                 qntd = enter.Qntd,
                                 allocateData = shelf.DataRegister,
                                 price = enter.Price,
                             };

            return queryEnter.OrderBy(q => q.productName).ToList();
        }

        public Shelf CreateNewShelf(NewShelfDTO newShelf)
        {
            var shelf = new Shelf
            {
                Name = newShelf.shelfName,
                DataRegister = DateTime.UtcNow,
            };

            _context.Add(shelf);
            _context.SaveChanges();

            return shelf;
        }

        public Shelf UpdateShelf(int shelfId, string shelfName)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.Id == shelfId);

            if (shelf == null)
                throw new Exception("Prateleira não encontrada com o ID informado");

            shelf.Name = shelfName;

            _context.SaveChanges();

            return shelf;
        }

        public Shelf DeleteShelf(int shelfId)
        {
            var shelf = _context.Shelves.FirstOrDefault(s => s.Id.Equals(shelfId));

            if (shelf is null)
                throw new Exception("Prateleira não encontrada com o ID informado");

            var enters = _context.Enters.Where(e => e.IdShelf.Equals(shelfId)).ToList();

            if (enters.Count > 0)
                throw new Exception("Não é possível excluír a prateleira pois há entradas de produtos associadas a ela");

            _context.Shelves.Remove(shelf);

            _context.SaveChanges();

            return shelf;
        }

        public Enter AllocateProductToShelf(AllocateProductToShelfDTO newAllocation)
        {
            var product = _context.Products.Where(p => p.Id == newAllocation.ProductId).FirstOrDefault();

            if (product is null)
                throw new Exception("Produto não encontrado na base de dados");

            if (product.Qntd < newAllocation.ProductQuantity)
                throw new Exception("Quantidade indisponível para alocação e venda.");

            product.Qntd -= newAllocation.ProductQuantity;

            var enter = _context.Enters.Where(e => e.IdProduct == newAllocation.ProductId && e.IdShelf == newAllocation.ShelfId).FirstOrDefault();

            if (enter is null)
            {
                var shelf = _context.Shelves.Where(s => s.Id == newAllocation.ShelfId).FirstOrDefault();

                if (shelf is null)
                    throw new Exception("Prateleira não encontrada na base de dados");

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
            else
            {
                enter.Qntd += newAllocation.ProductQuantity;
                enter.Price = newAllocation.ProductPrice;

                _context.SaveChanges();

                return enter;
            }
        }

        public Enter UndoAllocate(int enterId)
        {
            var enter = _context.Enters.FirstOrDefault(e => e.Id.Equals(enterId));

            if (enter is null)
                throw new Exception("Entrada não encontrada com o ID informado");

            var product = _context.Products.FirstOrDefault(p => p.Id.Equals(enter.IdProduct));

            if (product is null)
                throw new Exception("Produto não encontrado com o ID da entrada informada");

            product.Qntd += enter.Qntd;
            enter.Qntd = 0;

            _context.SaveChanges();

            return enter;
        }

        #endregion
    }
}
