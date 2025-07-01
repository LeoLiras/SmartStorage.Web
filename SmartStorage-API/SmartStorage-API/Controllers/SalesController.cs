using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        #region Propriedades

        private ISaleBusiness _saleService;

        #endregion

        #region Construtores

        public SalesController(ISaleBusiness saleService)
        {
            _saleService = saleService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        public ActionResult<List<SaleVO>> FindAllSales()
        {
            return Ok(_saleService.FindAllSales());
        }

        [HttpGet("{saleId}")]
        public ActionResult<List<SaleVO>> FindSaleById(int saleId)
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
        public IActionResult CreateNewSale([FromBody] SaleVO newSale)
        {
            try
            {
                if (newSale.ProductId.Equals(0))
                    throw new Exception("O campo ID do produto é obrigatório.");

                if (newSale.Qntd.Equals(0))
                    throw new Exception("O campo Quantidade da Venda é obrigatório.");

                return Ok(_saleService.CreateNewSale(newSale.ProductId, newSale.Qntd));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{saleId}")]
        public IActionResult UpdateSale(int saleId, [FromBody] SaleVO newSale)
        {
            try
            {
                if (saleId.Equals(0))
                    throw new Exception("O campo ID da Venda é obrigatório.");

                if (newSale.Qntd.Equals(0))
                    throw new Exception("O campo Quantidade da Venda é obrigatório.");

                return Ok(_saleService.UpdateSale(saleId, newSale.Qntd));
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
