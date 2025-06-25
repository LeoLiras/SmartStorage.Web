using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
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

        [HttpGet("{saleId}")]
        public ActionResult<List<SaleDTO>> FindSaleById(int saleId)
        {
            try
            {
                if (saleId.Equals(0))
                    throw new Exception("O ID da Venda é obrigatório.");

                return Ok(_saleService.FindSaleById(saleId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            } 
        }

        [HttpPost]
        public IActionResult CreateNewSale([FromBody] NewSaleDTO newSale)
        {
            try
            {
                if (newSale.saleProductId.Equals(0)) 
                    throw new Exception("O campo ID do produto é obrigatório.");

                if (newSale.saleSaleQntd.Equals(0)) 
                    throw new Exception("O campo Quantidade da Venda é obrigatório.");

                return Ok(_saleService.CreateNewSale(newSale.saleProductId, newSale.saleSaleQntd));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{saleId}")]
        public IActionResult UpdateSale(int saleId, [FromBody] NewSaleDTO newSale)
        {
            try
            {
                if (saleId.Equals(0))
                    throw new Exception("O campo ID da Venda é obrigatório.");

                if (newSale.saleSaleQntd.Equals(0))
                    throw new Exception("O campo Quantidade da Venda é obrigatório.");

                return Ok(_saleService.UpdateSale(saleId, newSale.saleSaleQntd));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{saleId}")]
        public IActionResult DeleteSale(int saleId)
        {
            try
            {
                if (saleId.Equals(0))
                    throw new Exception("O campo ID da Venda é obrigatório.");

                return Ok(_saleService.DeleteSale(saleId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
