using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;

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

            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId.Equals(origin.EntSheId));

            var product = _context.Products.FirstOrDefault(p => p.ProId.Equals(origin.EntProId));

            return new EnterVO
            {
                Id = origin.EntId,
                ProductId = origin.EntProId,
                ProductName = product is null ? string.Empty : product.ProName,
                ProductQuantity = origin.EntQntd,
                ProductPrice = origin.EntPrice,
                ShelfId = origin.EntSheId,
                ShelfName = shelf is null ? string.Empty : shelf.SheName,
                DateEnter = origin.EntDateEnter,
            };
        }

        public Enter Parse(EnterVO origin)
        {
            if (origin == null)
                return null;

            return new Enter
            {
                EntId = origin.Id,
                EntProId = origin.ProductId,
                EntQntd = origin.ProductQuantity,
                EntPrice = origin.ProductPrice,
                EntSheId = origin.ShelfId,
                EntDateEnter = origin.DateEnter,
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
