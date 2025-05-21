using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        #region Propriedades

        private ISaleService _saleService;

        #endregion

        #region Construtores

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        public ActionResult<List<SaleDTO>> FindAllSales()
        {
            return Ok(_saleService.FindAllSales());
        }

        [HttpPost]
        public IActionResult CreateNewSale([FromBody] SaleDTO newSale)
        {
            if (newSale.productId.Equals(0)) return BadRequest("O campo ID do produto é obrigatório.");

            if (newSale.saleQntd.Equals(0)) return BadRequest("O campo Quantidade da Venda é obrigatório.");

            var searchNewSale = _saleService.CreateNewSale(newSale);

            if (searchNewSale == null) return BadRequest("Falha ao registrar a venda. Verifique se existe a quantidade informada disponível na prateleira.");

            return Ok(searchNewSale);
        }

        #endregion
    }
}
