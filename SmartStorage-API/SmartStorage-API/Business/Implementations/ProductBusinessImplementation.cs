using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    public class ProductBusinessImplementation : IProductBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly ProductConverter _converter;

        #endregion

        #region Construtores

        public ProductBusinessImplementation(SmartStorageContext context)
        {
            _context = context;
            _converter = new ProductConverter();
        }

        #endregion

        #region Métodos

        public List<ProductVO> FindAllProducts()
        {
            return _converter.Parse(_context.Products.OrderBy(q => q.ProName).ToList());
        }

        public ProductVO FindProductById(int id)
        {
            var product = _context.Products.SingleOrDefault(x => x.ProId.Equals(id));

            if (product is null)
                throw new Exception("Produto não encontrado com o ID informado");

            return _converter.Parse(product);
        }

        public ProductVO CreateNewProduct(ProductVO product)
        {
            var productSearch = _context.Products.FirstOrDefault(x => x.ProName == product.Name);

            if (productSearch != null)
                throw new Exception("Produto já cadastrado.");

            var emplyeeSearch = _context.Employees.FirstOrDefault(x => x.EmpId == product.EmployeeId);

            if (emplyeeSearch == null)
                throw new Exception("Funcionario não encontrado com o ID informado.");

            var newProduct = new Product
            {
                ProName = product.Name,
                ProDescription = product.Descricao,
                ProDateRegister = DateTime.UtcNow,
                ProQntd = product.Qntd,
                ProEmpId = product.EmployeeId
            };

            _context.Add(newProduct);
            _context.SaveChanges();

            return _converter.Parse(newProduct);

        }

        public ProductVO UpdateProduct(int productId, ProductVO product)
        {
            var searchProduct = _context.Products.FirstOrDefault(x => x.ProId == productId);

            if (searchProduct == null)
                throw new Exception("Produto não encontrado com o ID informado.");

            var prod = _context.Products.FirstOrDefault(x => x.ProName == product.Name && x.ProId != productId);

            if (prod != null)
                throw new Exception("Já existe um produto cadastrado com esse nome.");

            var employee = _context.Employees.FirstOrDefault(x => x.EmpId == product.EmployeeId);

            if (employee == null)
                throw new Exception("Colaborador com o ID informado não encontrado.");

            if (!product.EmployeeId.Equals(0))
                searchProduct.ProEmpId = product.EmployeeId;

            if (!string.IsNullOrWhiteSpace(product.Name))
                searchProduct.ProName = product.Name;

            if (!product.EmployeeId.Equals(0))
                searchProduct.ProQntd = product.Qntd;

            if (!string.IsNullOrWhiteSpace(product.Descricao))
                searchProduct.ProDescription = product.Descricao;

            _context.SaveChanges();

            return _converter.Parse(searchProduct);
        }

        public ProductVO DeleteProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProId.Equals(productId));

            if (product is null)
                throw new Exception("Produto não encontrado com o ID informado");

            var enters = _context.Enters.Where(e => e.EntProId.Equals(productId)).ToList();

            if (enters.Count > 0)
            {
                foreach (var enter in enters)
                {
                    var sales = _context.Sales.Where(s => s.SalEntId.Equals(enter.EntId)).ToList();

                    if (sales.Count > 0)
                        _context.Sales.RemoveRange(sales);

                    _context.Remove(enter);
                }
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return _converter.Parse(product);
        }
        #endregion
    }
}
