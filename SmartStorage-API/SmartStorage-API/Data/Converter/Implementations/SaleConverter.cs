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
                Id = origin.Id,
                IdEnter = origin.IdEnter,
                Qntd = origin.Qntd,
                DateSale = origin.DateSale
            };
        }

        public SaleVO Parse(Sale origin)
        {
            if (origin == null)
                return null;

            var enter = _context.Enters.FirstOrDefault(e => e.Id.Equals(origin.IdEnter));

            if (enter is null)
                return new SaleVO();

            var product = _context.Products.FirstOrDefault(p => p.Id.Equals(enter.IdProduct));

            var shelf = _context.Shelves.FirstOrDefault(s => s.Id.Equals(enter.IdShelf));

            return new SaleVO
            {
                Id = origin.Id,
                IdEnter = origin.IdEnter,
                Qntd = origin.Qntd,
                DateSale = origin.DateSale,
                ProductId = product is null ? 0 : product.Id,
                ProductName = product is null ? string.Empty : product.Name,
                ShelfName = shelf is null ? string.Empty : shelf.Name,
                EnterPrice = enter is null? 0.0m : enter.Price,
                SaleTotal = enter is null ? 0.0m : enter.Price * origin.Qntd
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
