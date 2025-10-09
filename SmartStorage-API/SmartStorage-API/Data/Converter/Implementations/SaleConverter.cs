using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

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

            var enter = _context.Enters.FirstOrDefault(e => e.EntId.Equals(origin.SalEntId));

            if (enter is null)
                return new SaleVO();

            var product = _context.Products.FirstOrDefault(p => p.ProId.Equals(enter.EntProId));

            var shelf = _context.Shelves.FirstOrDefault(s => s.SheId.Equals(enter.EntSheId));

            return new SaleVO
            {
                Id = origin.SalId,
                IdEnter = origin.SalEntId,
                Qntd = origin.SalQntd,
                DateSale = origin.SalDateSale,
                ProductId = product is null ? 0 : product.ProId,
                ProductName = product is null ? string.Empty : product.ProName,
                ShelfName = shelf is null ? string.Empty : shelf.SheName,
                EnterPrice = enter is null? 0.0m : enter.EntPrice,
                SaleTotal = enter is null ? 0.0m : enter.EntPrice * origin.SalQntd
            };
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

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
