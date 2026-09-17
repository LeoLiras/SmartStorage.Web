using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class ProductConverter : IParser<ProductVO, Product>, IParser<Product, ProductVO>
    {
        private readonly SmartStorageContext _context;

        public ProductConverter(SmartStorageContext context)
        {
            _context = context;
        }

        public Product Parse(ProductVO origin)
        {
            if (origin == null)
                return null;

            return new Product
            {
                ProId = origin.Id,
                ProName = origin.Name,
                ProDescription = origin.Descricao,
                ProDateRegister = origin.DateRegister,
                ProQntd = origin.Qntd,
                ProMinimumStock = origin.MinimumStock,
                ProVolume = origin.Volume,
                ProPrecoInicial = origin.PrecoInicial,
                ProCodigo = origin.Codigo,
                ProFatorConversao = origin.FatorConversao,
                ProCusto = origin.Custo,
                ProEmpId = origin.EmployeeId,
            };
        }

        public ProductVO Parse(Product origin)
        {
            if (origin == null)
                return null;

            var shelvesQuantity = _context.Enters
                .Where(e => e.EntProId == origin.ProId)
                .Sum(e => (int?)e.EntQntd) ?? 0;

            return Parse(origin, shelvesQuantity);
        }

        public List<Product> Parse(List<ProductVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }

        public List<ProductVO> Parse(List<Product> origin)
        {
            if (origin == null)
                return null;

            var productIds = origin.Select(p => p.ProId).ToList();

            var shelvesQuantities = _context.Enters
                .Where(e => productIds.Contains(e.EntProId))
                .GroupBy(e => e.EntProId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(e => e.EntQntd) })
                .ToDictionary(x => x.ProductId, x => x.Quantity);

            return origin.Select(item => Parse(item, shelvesQuantities.GetValueOrDefault(item.ProId))).ToList();
        }

        private static ProductVO Parse(Product origin, int shelvesQuantity)
        {
            return new ProductVO
            {
                Id = origin.ProId,
                Name = origin.ProName,
                Descricao = origin.ProDescription,
                DateRegister = origin.ProDateRegister,
                Qntd = origin.ProQntd,
                ShelvesQntd = shelvesQuantity,
                MinimumStock = origin.ProMinimumStock,
                Volume = origin.ProVolume,
                PrecoInicial = origin.ProPrecoInicial,
                Codigo = origin.ProCodigo,
                FatorConversao = origin.ProFatorConversao,
                Custo = origin.ProCusto,
                EmployeeId = origin.ProEmpId,
                ProImage = origin.ProImage,
            };
        }
    }
}
