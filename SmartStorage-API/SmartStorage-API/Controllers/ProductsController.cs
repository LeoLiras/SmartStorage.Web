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
        public IActionResult CreateNewProduct([FromBody] ProductDTO product)
        {
            if (product.Qntd.Equals(0)) return BadRequest("O campo Quantidade do Produto é obrigatório");

            return Ok(_productService.CreateNewProduct(product));
        }

        #endregion
    }
}
