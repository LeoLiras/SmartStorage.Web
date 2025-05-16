using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private IStorageService _storageService;

        public ProductsController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            return Ok(_storageService.FindAllProducts());
        }

        [HttpGet("{id}")]
        public IActionResult GetProductsById(int id)
        {
            var product = _storageService.FindProductById(id);

            if (product == null) return NotFound("Produto não encontrado.");

            return Ok(product);
        }

        [HttpPost]
        public IActionResult CreateNewProduct([FromBody] ProductDTO product)
        {
            if (product.Qntd.Equals(0)) return BadRequest("O campo Quantidade do Produto é obrigatório");

            return Ok(_storageService.CreateNewProduct(product));
        }
    }
}
