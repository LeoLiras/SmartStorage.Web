using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private IStorageService _storageService;

        public SalesController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        public ActionResult<List<SaleDTO>> GetSales()
        {
            return Ok(_storageService.FindAllSales());
        }

        [HttpPost]
        public IActionResult CreateNewSale([FromBody] SaleDTO newSale)
        {
            if (newSale.productId.Equals(0)) return BadRequest("O campo ID do produto é obrigatório.");

            if (newSale.saleQntd.Equals(0)) return BadRequest("O campo Quantidade da Venda é obrigatório.");

            var searchNewSale = _storageService.CreateNewSale(newSale);

            if (searchNewSale == null) return BadRequest("Falha ao registrar a venda. Verifique se existe a quantidade informada disponível na prateleira.");

            return Ok(searchNewSale);
        }
    }
}
