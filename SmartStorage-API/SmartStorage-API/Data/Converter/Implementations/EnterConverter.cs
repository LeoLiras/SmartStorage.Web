using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

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

            return Parse(
                origin,
                ProductNamesOf(new[] { origin.EntProId }).GetValueOrDefault(origin.EntProId),
                ShelfNamesOf(new[] { origin.EntSheId }).GetValueOrDefault(origin.EntSheId));
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

            var productNames = ProductNamesOf(origin.Select(e => e.EntProId).Distinct().ToList());

            var shelfNames = ShelfNamesOf(origin.Select(e => e.EntSheId).Distinct().ToList());

            return origin
                .Select(item => Parse(item, productNames.GetValueOrDefault(item.EntProId), shelfNames.GetValueOrDefault(item.EntSheId)))
                .ToList();
        }

        private Dictionary<int, string> ProductNamesOf(IReadOnlyCollection<int> productIds)
        {
            return _context.Products
                .Where(p => productIds.Contains(p.ProId))
                .Select(p => new { p.ProId, p.ProName })
                .ToDictionary(p => p.ProId, p => p.ProName);
        }

        private Dictionary<int, string> ShelfNamesOf(IReadOnlyCollection<int> shelfIds)
        {
            return _context.Shelves
                .Where(s => shelfIds.Contains(s.SheId))
                .Select(s => new { s.SheId, s.SheName })
                .ToDictionary(s => s.SheId, s => s.SheName);
        }

        private static EnterVO Parse(Enter origin, string productName, string shelfName)
        {
            return new EnterVO
            {
                Id = origin.EntId,
                ProductId = origin.EntProId,
                ProductName = productName ?? string.Empty,
                ProductQuantity = origin.EntQntd,
                ProductPrice = origin.EntPrice,
                ShelfId = origin.EntSheId,
                ShelfName = shelfName ?? string.Empty,
                DateEnter = origin.EntDateEnter,
            };
        }

        public List<Enter> Parse(List<EnterVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
