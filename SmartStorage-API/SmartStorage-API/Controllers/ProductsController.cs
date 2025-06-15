using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        #region Propriedades

        private IProductService _productService;

        #endregion

        #region Construtores

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        public IActionResult FindAllProducts()
        {
            return Ok(_productService.FindAllProducts());
        }

        [HttpGet("{id}")]
        public IActionResult FindProductById(int id)
        {
            var product = _productService.FindProductById(id);

            if (product == null) return NotFound("Produto não encontrado.");

            return Ok(product);
        }

        [HttpPost]
        public IActionResult CreateNewProduct([FromBody] ProductDTO newProduct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newProduct.productName))
                    throw new Exception("O Nome do Produto é obrigatório.");

                return Ok(_productService.CreateNewProduct(newProduct));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{productId}")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductDTO product)
        {
            try
            {
                if (productId.Equals(0)) return BadRequest("O campo ID do Produto é obrigatório.");

                return Ok(_productService.UpdateProduct(productId, product));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{productId}")]
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
