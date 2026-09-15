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
            _converter = new ProductConverter(_context);
            _movementRepository = movementRepository;
        }

        #endregion

        #region Métodos

        public List<ProductVO> FindAllProducts()
        {
            return _converter.Parse(_context.Products.OrderBy(q => q.ProName).ToList());
        }

        public (List<ProductVO> Items, int Total) FindProductsPage(int page, int pageSize, string search)
        {
            if (page < 1)
                throw new Exception("A página deve ser maior que zero.");

            if (pageSize < 1 || pageSize > Pagination.MaxPageSize)
                throw new Exception($"O tamanho da página deve estar entre 1 e {Pagination.MaxPageSize}.");

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.ProName.Contains(search.Trim()));

            var total = query.Count();

            var products = query
                .OrderBy(p => p.ProName)
                .ThenBy(p => p.ProId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (_converter.Parse(products), total);
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

            ValidateMinimumStock(product.MinimumStock);

            var newProduct = new Product
            {
                ProName = product.Name,
                ProDescription = product.Descricao,
                ProDateRegister = DateTime.UtcNow,
                ProQntd = 0,
                ProMinimumStock = product.MinimumStock,
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

            ValidateMinimumStock(product.MinimumStock);

            searchProduct.ProEmpId = product.EmployeeId;

            searchProduct.ProMinimumStock = product.MinimumStock;

            if (!string.IsNullOrWhiteSpace(product.Name))
                searchProduct.ProName = product.Name;

            if (!string.IsNullOrWhiteSpace(product.Descricao))
                searchProduct.ProDescription = product.Descricao;

            searchProduct.ProImage = product.ProImage;

            if (product.StockAdjustment is null)
                _context.SaveChanges();
            else
                _movementRepository.CreateNewStockMovement(
                    productId,
                    shelfId: null,
                    TipoMovimentacao.Ajuste,
                    CalculateStockAdjustmentDelta(searchProduct, product.StockAdjustment.Quantity),
                    product.StockAdjustment.Reason);

            return _converter.Parse(searchProduct);
        }

        public ProductVO AdjustProductStock(int productId, int newQuantity, string reason)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProId == productId);

            if (product is null)
                throw new Exception("Produto não encontrado com o ID informado.");

            _movementRepository.CreateNewStockMovement(
                productId,
                shelfId: null,
                TipoMovimentacao.Ajuste,
                CalculateStockAdjustmentDelta(product, newQuantity),
                reason);

            return _converter.Parse(product);
        }

        private static void ValidateMinimumStock(int minimumStock)
        {
            if (minimumStock < 0)
                throw new Exception("O estoque mínimo não pode ser negativo.");
        }

        private static int CalculateStockAdjustmentDelta(Product product, int newQuantity)
        {
            if (newQuantity < 0)
                throw new Exception("A quantidade do ajuste não pode ser negativa.");

            var quantityDelta = newQuantity - product.ProQntd;

            if (quantityDelta == 0)
                throw new Exception("A quantidade informada é igual ao saldo atual do depósito.");

            return quantityDelta;
        }

        #endregion
    }
}
