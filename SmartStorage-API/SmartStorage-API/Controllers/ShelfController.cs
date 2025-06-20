using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        #region Propriedades

        private IShelfService _shelfService;

        #endregion

        #region Construtores

        public ShelfController(IShelfService shelfService)
        {
            _shelfService = shelfService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        public ActionResult<List<ShelfDTO>> FindAllShelf()
        {
            return Ok(_shelfService.FindAllShelf());
        }

        [HttpGet("{id}")]
        public IActionResult FindShelfById(int id)
        {
            try
            {
                if (id.Equals(0))
                    return BadRequest("O campo Id é obrigatório.");

                return Ok(_shelfService.FindShelfById(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }  
        }

        [HttpGet("allocation")]
        public ActionResult<List<ShelfDTO>> GetProductsInShelves()
        {
            return Ok(_shelfService.FindAllProductsInShelves());
        }

        [HttpGet("allocation/{enterId}")]
        public ActionResult<List<ShelfDTO>> FindProductInShelfById(int enterId)
        {
            if (enterId.Equals(0))
                return BadRequest("O ID da entrada é obrigatório");

            var enter = _shelfService.FindProductInShelfById(enterId);

            if (enter is null)
                return NotFound("Entrada não encontrada com o ID informado");

            return Ok(enter);
        }

        [HttpPost]
        public IActionResult CreateNewShelf([FromBody] NewShelfDTO newShelf)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newShelf.shelfName))
                    throw new Exception("O campo Nome da Prateleira é obrigatório.");

                return Ok(_shelfService.CreateNewShelf(newShelf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{shelfId}")]
        public IActionResult UpdateShelf(int shelfId, [FromBody] NewShelfDTO shelf)
        {
            try
            {
                if (shelfId.Equals(0))
                    return BadRequest("O campo Id é obrigatório.");

                if (string.IsNullOrWhiteSpace(shelf.shelfName))
                    throw new Exception("O campo Nome da Prateleira é obrigatório.");

                return Ok(_shelfService.UpdateShelf(shelfId, shelf.shelfName));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{shelfId}")]
        public IActionResult DeleteShelf(int shelfId)
        {
            try
            {
                if (shelfId.Equals(0))
                    throw new Exception("O campo ID da Prateleira é obrigatório.");

                return Ok(_shelfService.DeleteShelf(shelfId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("allocation")]
        public IActionResult AllocateProductToShelf([FromBody] AllocateProductToShelfDTO newAllocation)
        {
            try
            {
                if (newAllocation.ProductId.Equals(0))
                    throw new Exception("O campo ID do Produto é obrigatório.");

                if (newAllocation.ShelfId.Equals(0))
                    throw new Exception("O campo ID da Prateleira é obrigatório.");

                if (newAllocation.ProductPrice.Equals(0))
                    throw new Exception("O campo Preço do Produto é obrigatório.");

                return Ok(_shelfService.AllocateProductToShelf(newAllocation));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("allocation/{enterId}")]
        public IActionResult UndoAllocate(int enterId)
        {
            try
            {
                if (enterId.Equals(0))
                    throw new Exception("O campo ID da entrada é obrigatório.");

                return Ok(_shelfService.UndoAllocate(enterId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
