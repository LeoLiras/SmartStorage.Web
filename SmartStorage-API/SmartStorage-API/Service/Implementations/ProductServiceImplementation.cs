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
            try
            {
                var productSearch = _context.Products.FirstOrDefault(x => x.Name == product.Name);

                if (productSearch != null)
                {
                    productSearch.Qntd += product.Qntd;

                    _context.SaveChanges();

                    return productSearch;
                }
                else
                {
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

                    return newProduct;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
