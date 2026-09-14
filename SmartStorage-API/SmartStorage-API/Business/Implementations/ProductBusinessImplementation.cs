using SmartStorage.Shared.Enum;
using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Model.Context;
using SmartStorage_API.Repository.Interfaces;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Service.Implementations
{
    public class ProductBusinessImplementation : IProductBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly ProductConverter _converter;

        private readonly IProductStockMovementRepository _movementRepository;

        #endregion

        #region Construtores

        public ProductBusinessImplementation(SmartStorageContext context, IProductStockMovementRepository movementRepository)
        {
            _context = context;
            _converter = new ProductConverter();
            _movementRepository = movementRepository;
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

            if (product.Qntd < 0)
                throw new Exception("A quantidade do produto não pode ser negativa.");

            var newProduct = new Product
            {
                ProName = product.Name,
                ProDescription = product.Descricao,
                ProDateRegister = DateTime.UtcNow,
                ProQntd = 0,
                ProEmpId = product.EmployeeId,
                ProImage = product.ProImage
            };

            _context.Add(newProduct);
            _context.SaveChanges();

            if (product.Qntd > 0)
                _movementRepository.CreateNewStockMovement(
                    newProduct.ProId,
                    shelfId: null,
                    TipoMovimentacao.Entrada,
                    product.Qntd);

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

            searchProduct.ProEmpId = product.EmployeeId;

            if (!string.IsNullOrWhiteSpace(product.Name))
                searchProduct.ProName = product.Name;

            if (!string.IsNullOrWhiteSpace(product.Descricao))
                searchProduct.ProDescription = product.Descricao;

            searchProduct.ProImage = product.ProImage;

            _context.SaveChanges();

            return _converter.Parse(searchProduct);
        }

        public ProductVO AdjustProductStock(int productId, int newQuantity, string reason)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProId == productId);

            if (product is null)
                throw new Exception("Produto não encontrado com o ID informado.");

            if (newQuantity < 0)
                throw new Exception("A quantidade do ajuste não pode ser negativa.");

            var quantityDelta = newQuantity - product.ProQntd;

            if (quantityDelta == 0)
                throw new Exception("A quantidade informada é igual ao saldo atual do depósito.");

            _movementRepository.CreateNewStockMovement(
                productId,
                shelfId: null,
                TipoMovimentacao.Ajuste,
                quantityDelta,
                reason);

            return _converter.Parse(product);
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

            var movements = _context.ProductStockMovements.Where(m => m.PsmProId.Equals(productId)).ToList();

            if (movements.Count > 0)
                _context.ProductStockMovements.RemoveRange(movements);

            _context.Products.Remove(product);
            _context.SaveChanges();

            return _converter.Parse(product);
        }
        #endregion
    }
}
