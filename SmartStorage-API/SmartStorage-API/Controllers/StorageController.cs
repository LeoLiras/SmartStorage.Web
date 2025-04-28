using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Service;
using SmartStorage_API.DTO;

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

        [HttpGet]
        [Route("products")]
        public IActionResult GetProducts()
        {
            return Ok(_storageService.FindAllProducts());
        }

        [HttpGet]
        [Route("sales")]
        public ActionResult<List<SaleDTO>> GetSales()
        {
            return Ok(_storageService.FindAllSales());
        }

        [HttpGet]
        [Route("shelves")]
        public ActionResult<List<ShelfDTO>> GetShelves()
        {
            return Ok(_storageService.FindAllShelves());
        }
    }
}
