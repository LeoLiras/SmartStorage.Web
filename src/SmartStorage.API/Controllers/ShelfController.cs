using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.Shared.Validation;
using SmartStorage_API.Hypermedia.Filters;
using SmartStorage_API.Service;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiController]
    [Authorize]
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
        [TypeFilter(typeof(HyperMediaFilter))]
        public ActionResult<List<ShelfVO>> FindAllShelf()
        {
            return Ok(_shelfService.FindAllShelf());
        }

        [HttpGet("{id}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult FindShelfById(int id)
        {
            if (id.Equals(0))
                throw new BusinessException("O campo Id da Prateleira é obrigatório.");

            return Ok(_shelfService.FindShelfById(id));
        }

        [HttpGet("allocation")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public ActionResult<List<ShelfVO>> GetProductsInShelves([FromQuery] int? page, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            if (page is null)
                return Ok(_shelfService.FindAllProductsInShelves());

            var (items, total) = _shelfService.FindProductsInShelvesPage(page.Value, pageSize, search);

            Response.Headers[Pagination.TotalCountHeader] = total.ToString();

            return Ok(items);
        }

        [HttpGet("allocation/{enterId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public ActionResult<List<ShelfVO>> FindProductInShelfById(int enterId)
        {
            if (enterId.Equals(0))
                return BadRequest("O ID da entrada é obrigatório");

            return Ok(_shelfService.FindProductInShelfById(enterId));
        }

        [HttpPost]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewShelf([FromBody] ShelfVO newShelf)
        {
            if (string.IsNullOrWhiteSpace(newShelf.Name))
                throw new BusinessException("O campo Nome da Prateleira é obrigatório.");

            return Ok(_shelfService.CreateNewShelf(newShelf));
        }

        [HttpPut("{shelfId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UpdateShelf(int shelfId, [FromBody] ShelfVO shelf)
        {
            if (shelfId.Equals(0))
                return BadRequest("O campo Id é obrigatório.");

            if (string.IsNullOrWhiteSpace(shelf.Name))
                throw new BusinessException("O campo Nome da Prateleira é obrigatório.");

            return Ok(_shelfService.UpdateShelf(shelfId, shelf));
        }

        [HttpDelete("{shelfId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult DeleteShelf(int shelfId)
        {
            if (shelfId.Equals(0))
                throw new BusinessException("O campo ID da Prateleira é obrigatório.");

            return Ok(_shelfService.DeleteShelf(shelfId));
        }

        [HttpPost("allocation")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult AllocateProductToShelf([FromBody] EnterVO newAllocation)
        {
            if (newAllocation.ProductId.Equals(0))
                throw new BusinessException("O campo ID do Produto é obrigatório.");

            if (newAllocation.ShelfId.Equals(0))
                throw new BusinessException("O campo ID da Prateleira é obrigatório.");

            if (newAllocation.ProductPrice <= 0)
                throw new BusinessException("O preço deve ser maior que zero.");

            if (newAllocation.ProductQuantity.Equals(0))
                throw new BusinessException("O campo Quantidade do Produto é obrigatório.");

            return Ok(_shelfService.AllocateProductToShelf(newAllocation));
        }

        [HttpPost("allocation/batch")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult AllocateProductsToShelves([FromBody] AllocationBatchVO batch)
        {
            if (batch is null)
                throw new BusinessException("Os dados do lote são obrigatórios.");

            return Ok(_shelfService.AllocateProductsToShelves(batch.Items));
        }

        [HttpPut("allocation/{enterId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UndoAllocate(int enterId)
        {
            if (enterId.Equals(0))
                throw new BusinessException("O campo ID da entrada é obrigatório.");

            return Ok(_shelfService.UndoAllocate(enterId));
        }

        [HttpPost("allocation/{enterId}/transfer")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult TransferProductToShelf(int enterId, [FromBody] ShelfTransferVO transfer)
        {
            if (enterId.Equals(0))
                throw new BusinessException("O campo ID da entrada é obrigatório.");

            if (transfer is null)
                throw new BusinessException("Os dados da transferência são obrigatórios.");

            return Ok(_shelfService.TransferProductToShelf(enterId, transfer.ShelfId));
        }

        [HttpPost("{shelfId}/inventory")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CountShelfInventory(int shelfId, [FromBody] InventoryCountVO count)
        {
            if (shelfId.Equals(0))
                throw new BusinessException("O campo ID da Prateleira é obrigatório.");

            if (count is null)
                throw new BusinessException("Os dados do inventário são obrigatórios.");

            return Ok(_shelfService.CountShelfInventory(shelfId, count));
        }

        #endregion
    }
}
