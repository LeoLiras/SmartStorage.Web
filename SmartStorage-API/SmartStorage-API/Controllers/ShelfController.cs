using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        #region Propriedades

        private IShelfBusiness _shelfService;

        #endregion

        #region Construtores

        public ShelfController(IShelfBusiness shelfService)
        {
            _shelfService = shelfService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        public ActionResult<List<ShelfVO>> FindAllShelf()
        {
            return Ok(_shelfService.FindAllShelf());
        }

        [HttpGet("{id}")]
        public IActionResult FindShelfById(int id)
        {
            try
            {
                if (id.Equals(0))
                    throw new Exception("O campo Id da Prateleira é obrigatório.");

                return Ok(_shelfService.FindShelfById(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("allocation")]
        public ActionResult<List<ShelfVO>> GetProductsInShelves()
        {
            return Ok(_shelfService.FindAllProductsInShelves());
        }

        [HttpGet("allocation/{enterId}")]
        public ActionResult<List<ShelfVO>> FindProductInShelfById(int enterId)
        {
            try
            {
                if (enterId.Equals(0))
                    return BadRequest("O ID da entrada é obrigatório");

                return Ok(_shelfService.FindProductInShelfById(enterId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateNewShelf([FromBody] ShelfVO newShelf)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newShelf.Name))
                    throw new Exception("O campo Nome da Prateleira é obrigatório.");

                return Ok(_shelfService.CreateNewShelf(newShelf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{shelfId}")]
        public IActionResult UpdateShelf(int shelfId, [FromBody] ShelfVO shelf)
        {
            try
            {
                if (shelfId.Equals(0))
                    return BadRequest("O campo Id é obrigatório.");

                if (string.IsNullOrWhiteSpace(shelf.Name))
                    throw new Exception("O campo Nome da Prateleira é obrigatório.");

                return Ok(_shelfService.UpdateShelf(shelfId, shelf.Name));
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
        public IActionResult AllocateProductToShelf([FromBody] EnterVO newAllocation)
        {
            try
            {
                if (newAllocation.ProductId.Equals(0))
                    throw new Exception("O campo ID do Produto é obrigatório.");

                if (newAllocation.ShelfId.Equals(0))
                    throw new Exception("O campo ID da Prateleira é obrigatório.");

                if (newAllocation.ProductPrice.Equals(0))
                    throw new Exception("O campo Preço do Produto é obrigatório.");

                if (newAllocation.ProductQuantity.Equals(0))
                    throw new Exception("O campo Quantidade do Produto é obrigatório.");

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
