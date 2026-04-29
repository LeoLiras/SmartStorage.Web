using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult FindAllProducts()
        {
            return Ok(_productService.FindAllProducts());
        }

        [HttpGet("{id}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult FindProductById(int id)
        {
            try
            {
                return Ok(_productService.FindProductById(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewProduct([FromBody] ProductVO newProduct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newProduct.Name))
                    throw new Exception("O Nome do Produto é obrigatório.");

                _productService.CreateNewProduct(newProduct);

                _rabbitMQMessageSender.SendMessage(newProduct, "sendemailqueue");

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{productId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductVO product)
        {
            try
            {
                if (productId.Equals(0))
                    throw new Exception("O campo ID do Produto é obrigatório.");

                return Ok(_productService.UpdateProduct(productId, product));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{productId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult DeleteProduct(int productId)
        {
            try
            {
                if (productId.Equals(0))
                    throw new Exception("O campo ID do Produto é obrigatório.");

                return Ok(_productService.DeleteProduct(productId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
