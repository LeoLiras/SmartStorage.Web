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
        public IActionResult CreateNewProduct([FromBody] ProductDTO product)
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
        public IActionResult CreateNewSale([FromBody] SaleDTO newSale)
        {
            if (newSale.productId == null || newSale.productId.Equals(0) || newSale.saleQntd == null || newSale.saleQntd.Equals(0)) return BadRequest();

            var searchNewSale = _storageService.CreateNewSale(newSale);

            if(searchNewSale == null) return BadRequest();

            return Ok(searchNewSale);
        }

        [HttpGet("shelf")]
        public ActionResult<List<ShelfDTO>> FindAllShelf()
        {
            return Ok(_storageService.FindAllShelf());
        }

        [HttpGet("allocation")]
        public ActionResult<List<ShelfDTO>> GetProductsInShelves()
        {
            return Ok(_storageService.FindAllProductsInShelves());
        }

        [HttpPost("allocation")]
        public IActionResult AllocateProductToShelf([FromBody] AllocateProductToShelfDTO newAllocation)
        {
            if (newAllocation.ProductId.Equals(0) || newAllocation.ShelfId.Equals(0) || newAllocation.ProductPrice.Equals(0.0)) return BadRequest();

            var searchNewAllocation = _storageService.AllocateProductToShelf(newAllocation);

            if (searchNewAllocation == null) return BadRequest();

            return Ok(searchNewAllocation);
        }

    }
}
