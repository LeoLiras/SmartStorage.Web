using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Hypermedia.Filters;
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
        [TypeFilter(typeof(HyperMediaFilter))]
        public ActionResult<List<SaleVO>> FindAllSales()
        {
            return Ok(_saleService.FindAllSales());
        }

        [HttpGet("{saleId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
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
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewSale([FromBody] SaleVO newSale)
        {
            try
            {
                if (newSale.ProductId.Equals(0))
                    throw new Exception("O campo ID do produto é obrigatório.");

                if (newSale.Qntd.Equals(0))
                    throw new Exception("O campo Quantidade da Venda é obrigatório.");

                return Ok(_saleService.CreateNewSale(newSale.IdEnter, newSale.Qntd, newSale.DateSale));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{saleId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
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
        [TypeFilter(typeof(HyperMediaFilter))]
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

        [HttpPost("analyse/ai")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public async Task<IActionResult> AnalyseAI([FromBody] string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("O texto da requisição é obrigatório.");

                var result = await _saleService.AnalyseAI(text);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export-excel")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public ActionResult GenerateExcel()
        {
            var report = _saleService.GenerateExcel();

            return File(
                report,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Vendas {DateTime.Now.Month}-{DateTime.Now.Year}"
            );
        }

        [HttpGet("export-pdf")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public async Task<ActionResult> GeneratePdf()
        {
            return File(
                await _saleService.GeneratePdf(),
                "application/pdf",
                $"Vendas {DateTime.Now.Month}-{DateTime.Now.Year}"
            );
        }

        #endregion
    }
}
