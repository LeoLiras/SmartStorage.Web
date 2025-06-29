using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;

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
                Id = origin.Id,
                Name = origin.Name,
                Descricao = origin.Descricao,
                DateRegister = origin.DateRegister,
                Qntd = origin.Qntd,
                EmployeeId = origin.EmployeeId,
            };
        }

        public ProductVO Parse(Product origin)
        {
            if (origin == null)
                return null;

            return new ProductVO
            {
                Id = origin.Id,
                Name = origin.Name,
                Descricao = origin.Descricao,
                DateRegister = origin.DateRegister,
                Qntd = origin.Qntd,
                EmployeeId = origin.EmployeeId,
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
