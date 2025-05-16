using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        private IStorageService _storageService;

        public ShelfController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
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
            if (newAllocation.ProductId.Equals(0)) return BadRequest("O campo ID do Produto é obrigatório.");

            if (newAllocation.ShelfId.Equals(0)) return BadRequest("O campo ID da Prateleira é obrigatório.");

            if (newAllocation.ProductPrice.Equals(0.0)) return BadRequest("O campo Preço do Produto é obrigatório.");

            var searchNewAllocation = _storageService.AllocateProductToShelf(newAllocation);

            if (searchNewAllocation == null) return BadRequest("Erro alocando produto a prateleira.");

            return Ok(searchNewAllocation);
        }
    }
}
