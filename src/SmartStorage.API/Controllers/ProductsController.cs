using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.Shared.Validation;
using SmartStorage_API.Hypermedia.Filters;
using SmartStorage_API.RabbitMQSender;
using SmartStorage_API.Service;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        #region Propriedades

        private IProductBusiness _productService;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        #endregion

        #region Construtores

        public ProductsController(IProductBusiness productService, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _productService = productService;
            _rabbitMQMessageSender = rabbitMQMessageSender;
        }

        #endregion

        #region Métodos

        [HttpGet]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult FindAllProducts([FromQuery] int? page, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            if (page is null)
                return Ok(_productService.FindAllProducts());

            var (items, total) = _productService.FindProductsPage(page.Value, pageSize, search);

            Response.Headers[Pagination.TotalCountHeader] = total.ToString();

            return Ok(items);
        }

        [HttpGet("{id}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult FindProductById(int id)
        {
            return Ok(_productService.FindProductById(id));
        }

        [HttpPost]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewProduct([FromBody] ProductVO newProduct)
        {
            if (string.IsNullOrWhiteSpace(newProduct.Name))
                throw new BusinessException("O Nome do Produto é obrigatório.");

            _productService.CreateNewProduct(newProduct);

            _rabbitMQMessageSender.SendMessage(newProduct, "sendemailqueue");

            return Ok(newProduct);
        }

        [HttpPut("{productId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductVO product)
        {
            if (productId.Equals(0))
                throw new BusinessException("O campo ID do Produto é obrigatório.");

            return Ok(_productService.UpdateProduct(productId, product));
        }

        [HttpPost("{productId}/adjust-stock")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult AdjustProductStock(int productId, [FromBody] StockAdjustmentVO adjustment)
        {
            if (productId.Equals(0))
                throw new BusinessException("O campo ID do Produto é obrigatório.");

            if (adjustment is null)
                throw new BusinessException("Os dados do ajuste são obrigatórios.");

            return Ok(_productService.AdjustProductStock(productId, adjustment.Quantity, adjustment.Reason));
        }

        #endregion
    }
}
