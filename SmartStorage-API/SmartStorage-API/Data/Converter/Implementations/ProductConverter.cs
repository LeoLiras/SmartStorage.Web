using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_Shared.Model;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class ProductConverter : IParser<ProductVO, Product>, IParser<Product, ProductVO>
    {
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
                ProEmpId = origin.EmployeeId,
            };
        }

        public ProductVO Parse(Product origin)
        {
            if (origin == null)
                return null;

            return new ProductVO
            {
                Id = origin.ProId,
                Name = origin.ProName,
                Descricao = origin.ProDescription,
                DateRegister = origin.ProDateRegister,
                Qntd = origin.ProQntd,
                EmployeeId = origin.ProEmpId,
            };
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

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
