using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class EnterConverter : IParser<EnterVO, Enter>, IParser<Enter, EnterVO>
    {
        private readonly SmartStorageContext _context;

        public EnterConverter(SmartStorageContext context)
        {
            _context = context;
        }

        public EnterVO Parse(Enter origin)
        {
            if (origin == null)
                return null;

            var shelf = _context.Shelves.FirstOrDefault(s => s.Id.Equals(origin.IdShelf));

            var product = _context.Products.FirstOrDefault(p => p.Id.Equals(origin.IdProduct));

            return new EnterVO
            {
                Id = origin.Id,
                ProductId = origin.IdProduct,
                ProductName = product is null ? string.Empty : product.Name,
                ProductQuantity = origin.Qntd,
                ProductPrice = origin.Price,
                ShelfId = origin.IdShelf,
                ShelfName = shelf is null ? string.Empty : shelf.Name,
                DateEnter = origin.DateEnter,
            };
        }

        public Enter Parse(EnterVO origin)
        {
            if (origin == null)
                return null;

            return new Enter
            {
                Id = origin.Id,
                ProductId = origin.ProductId,
                Qntd= origin.ProductQuantity,
                Price = origin.ProductPrice,
                IdShelf = origin.ShelfId,
                DateEnter = origin.DateEnter,
            };
        }

        public List<EnterVO> Parse(List<Enter> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }

        public List<Enter> Parse(List<EnterVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
