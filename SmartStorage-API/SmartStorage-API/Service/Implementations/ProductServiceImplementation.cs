using SmartStorage_API.DTO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    public class ProductServiceImplementation : IProductService
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        #endregion

        #region Construtores

        public ProductServiceImplementation(SmartStorageContext context)
        {
            _context = context;
        }

        #endregion

        #region Métodos

        public List<Product> FindAllProducts()
        {
            return _context.Products.OrderBy(q => q.Name).ToList();
        }

        public Product FindProductById(int id)
        {
            return _context.Products.SingleOrDefault(x => x.Id.Equals(id));
        }

        public Product CreateNewProduct(ProductDTO product)
        {
            var productSearch = _context.Products.FirstOrDefault(x => x.Name == product.productName);

            if (productSearch != null) throw new Exception("Produto já cadastrado");

            var emplyeeSearch = _context.Employees.FirstOrDefault(x => x.Id == product.productEmployeeId);

            if (emplyeeSearch == null) throw new Exception("Colaborador não encontrado com o ID informado.");

            var newProduct = new Product
            {
                Name = product.productName,
                Descricao = product.productDescricao,
                DateRegister = DateTime.UtcNow,
                Qntd = product.productQntd,
                EmployeeId = product.productEmployeeId
            };

            _context.Add(newProduct);
            _context.SaveChanges();

            return newProduct;
            
        }

        public Product UpdateProduct(int productId, ProductDTO product)
        {
            var searchProduct = _context.Products.FirstOrDefault(x => x.Id == productId);

            if (searchProduct == null) throw new Exception("Produto com o ID informado não encontrado.");

            if (!string.IsNullOrWhiteSpace(product.productName))
            {
                var prod = _context.Products.FirstOrDefault(x => x.Name == searchProduct.Name && x.Id != productId);

                if (prod != null) 
                    throw new Exception("Já existe um produto cadastrado com esse nome.");

                searchProduct.Name = product.productName;
            }
                
            if (!string.IsNullOrWhiteSpace(product.productDescricao))
                searchProduct.Descricao = product.productDescricao;

            if (!product.productQntd.Equals(0))
                searchProduct.Qntd = product.productQntd;

            if (!product.productEmployeeId.Equals(0))
            {
                var employee = _context.Employees.FirstOrDefault(x => x.Id == product.productEmployeeId);

                if (employee == null) throw new Exception("Colaborador com o ID informado não encontrado.");

                searchProduct.EmployeeId = product.productEmployeeId;
            }

            _context.SaveChanges();

            return searchProduct;
        }
        #endregion
    }
}
