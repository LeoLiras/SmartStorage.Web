using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class SaleConverter : IParser<SaleVO, Sale>, IParser<Sale, SaleVO>
    {

        private readonly SmartStorageContext _context;

        public SaleConverter(SmartStorageContext context)
        {
            _context = context;
        }

        public Sale Parse(SaleVO origin)
        {
            if (origin == null)
                return null;

            return new Sale
            {
                SalId = origin.Id,
                SalEntId = origin.IdEnter,
                SalQntd = origin.Qntd,
                SalDateSale = origin.DateSale
            };
        }

        public SaleVO Parse(Sale origin)
        {
            if (origin == null)
                return null;

            var enter = EntersOf(new[] { origin.SalEntId }).GetValueOrDefault(origin.SalEntId);

            return enter is null ? new SaleVO() : Parse(origin, enter);
        }

        public List<Sale> Parse(List<SaleVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }

        public List<SaleVO> Parse(List<Sale> origin)
        {
            if (origin == null)
                return null;

            var enters = EntersOf(origin.Select(s => s.SalEntId).Distinct().ToList());

            return origin
                .Select(item => enters.TryGetValue(item.SalEntId, out var enter) ? Parse(item, enter) : new SaleVO())
                .ToList();
        }

        private record EnterNames(int ProductId, string ProductName, string ShelfName);

        private Dictionary<int, EnterNames> EntersOf(IReadOnlyCollection<int> enterIds)
        {
            return _context.Enters
                .Where(e => enterIds.Contains(e.EntId))
                .Select(e => new { e.EntId, Names = new EnterNames(e.EntProId, e.Product.ProName, e.Shelf.SheName) })
                .ToDictionary(e => e.EntId, e => e.Names);
        }

        private static SaleVO Parse(Sale origin, EnterNames enter)
        {
            return new SaleVO
            {
                Id = origin.SalId,
                IdEnter = origin.SalEntId,
                Qntd = origin.SalQntd,
                ReturnedQntd = origin.SalReturnedQntd,
                DateSale = origin.SalDateSale,
                ProductId = enter.ProductId,
                ProductName = enter.ProductName ?? string.Empty,
                ShelfName = enter.ShelfName ?? string.Empty,
                SalePrice = origin.SalPrice,
                SaleTotal = origin.SalPrice * (origin.SalQntd - origin.SalReturnedQntd)
            };
        }
    }
}
