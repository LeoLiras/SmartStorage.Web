using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    public class ProductServiceImplementation : IProductService
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly ProductConverter _converter;

        #endregion

        #region Construtores

        public ProductServiceImplementation(SmartStorageContext context)
        {
            _context = context;
            _converter = new ProductConverter();
        }

        #endregion

        #region Métodos

        public List<ProductVO> FindAllProducts()
        {
            return _converter.Parse(_context.Products.OrderBy(q => q.Name).ToList());
        }

        public ProductVO FindProductById(int id)
        {
            var product = _context.Products.SingleOrDefault(x => x.Id.Equals(id));

            if (product is null)
                throw new Exception("Produto não encontrado com o ID informado");

            return _converter.Parse(product);
        }

        public ProductVO CreateNewProduct(ProductVO product)
        {
            var productSearch = _context.Products.FirstOrDefault(x => x.Name == product.Name);

            if (productSearch != null)
                throw new Exception("Produto já cadastrado.");

            var emplyeeSearch = _context.Employees.FirstOrDefault(x => x.Id == product.EmployeeId);

            if (emplyeeSearch == null)
                throw new Exception("Funcionario não encontrado com o ID informado.");

            var newProduct = new Product
            {
                Name = product.Name,
                Descricao = product.Descricao,
                DateRegister = DateTime.UtcNow,
                Qntd = product.Qntd,
                EmployeeId = product.EmployeeId
            };

            _context.Add(newProduct);
            _context.SaveChanges();

            return _converter.Parse(newProduct);

        }

        public ProductVO UpdateProduct(int productId, ProductVO product)
        {
            var searchProduct = _context.Products.FirstOrDefault(x => x.Id == productId);

            if (searchProduct == null)
                throw new Exception("Produto não encontrado com o ID informado.");

            var prod = _context.Products.FirstOrDefault(x => x.Name == product.Name && x.Id != productId);

            if (prod != null)
                throw new Exception("Já existe um produto cadastrado com esse nome.");

            var employee = _context.Employees.FirstOrDefault(x => x.Id == product.EmployeeId);

            if (employee == null)
                throw new Exception("Colaborador com o ID informado não encontrado.");

            if (!product.EmployeeId.Equals(0))
                searchProduct.EmployeeId = product.EmployeeId;

            if (!string.IsNullOrWhiteSpace(product.Name))
                searchProduct.Name = product.Name;

            if (!product.EmployeeId.Equals(0))
                searchProduct.Qntd = product.Qntd;

            if (!string.IsNullOrWhiteSpace(product.Descricao))
                searchProduct.Descricao = product.Descricao;

            _context.SaveChanges();

            return _converter.Parse(searchProduct);
        }

        public ProductVO DeleteProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id.Equals(productId));

            if (product is null)
                throw new Exception("Produto não encontrado com o ID informado");

            var enters = _context.Enters.Where(e => e.IdProduct.Equals(productId)).ToList();

            if (enters.Count > 0)
            {
                foreach (var enter in enters)
                {
                    var sales = _context.Sales.Where(s => s.IdEnter.Equals(enter.Id)).ToList();

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
