using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Service;
using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        private IStorageService _storageService;

        public StorageController(ILogger<StorageController> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        [HttpGet("products")]
        public IActionResult GetProducts()
        {
            return Ok(_storageService.FindAllProducts());
        }

        [HttpGet("products/{id}")]
        public IActionResult GetProductsById(int id)
        {
            var product = _storageService.FindProductById(id);

            if (product == null) return NotFound();

            return Ok(product);
        }

        [HttpPost("products")]
        public IActionResult CreateNewProduct([FromBody] Product product)
        {
            if (product == null) return BadRequest();

            return Ok(_storageService.CreateNewProduct(product));
        }

        [HttpGet("sales")]
        public ActionResult<List<SaleDTO>> GetSales()
        {
            return Ok(_storageService.FindAllSales());
        }

        [HttpPost("sales")]
        public IActionResult CreateNewSale([FromBody] NewSaleDTO newSale)
        {
            if (newSale.ProductId == null || newSale.ProductId.Equals(0) || newSale.ProductQuantity == null || newSale.ProductQuantity.Equals(0)) return BadRequest();

            var searchNewSale = _storageService.CreateNewSale(newSale);

            if(searchNewSale == null) return BadRequest();

            return Ok(searchNewSale);
        }

        [HttpGet("shelves")]
        public ActionResult<List<ShelfDTO>> GetShelves()
        {
            return Ok(_storageService.FindAllShelves());
        }
        
    }
}
