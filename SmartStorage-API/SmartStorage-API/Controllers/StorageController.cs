using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Service;

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
        public IActionResult Get()
        {
            return Ok(_storageService.FindAll());
        }
    }
}
