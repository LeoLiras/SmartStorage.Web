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
        public ActionResult<List<SaleVO>> FindAllSales([FromQuery] int? page, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            if (page is null)
                return Ok(_saleService.FindAllSales());

            var (items, total) = _saleService.FindSalesPage(page.Value, pageSize, search);

            Response.Headers[Pagination.TotalCountHeader] = total.ToString();

            return Ok(items);
        }

        [HttpGet("{saleId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public ActionResult<List<SaleVO>> FindSaleById(int saleId)
        {
            if (saleId.Equals(0))
                throw new BusinessException("O ID da Venda é obrigatório.");

            return Ok(_saleService.FindSaleById(saleId));
        }

        [HttpPost]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewSale([FromBody] SaleVO newSale)
        {
            if (newSale.IdEnter.Equals(0))
                throw new BusinessException("O campo ID da Entrada é obrigatório.");

            if (newSale.Qntd.Equals(0))
                throw new BusinessException("O campo Quantidade da Venda é obrigatório.");

            return Ok(_saleService.CreateNewSale(newSale.IdEnter, newSale.Qntd, newSale.DateSale));
        }

        [HttpPost("batch")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewSales([FromBody] SaleBatchVO newSales)
        {
            if (newSales is null)
                throw new BusinessException("Os dados do carrinho são obrigatórios.");

            if (newSales.DateSale == default)
                throw new BusinessException("O campo Data da Venda é obrigatório.");

            return Ok(_saleService.CreateNewSales(newSales.Items, newSales.DateSale));
        }

        [HttpPut("{saleId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UpdateSale(int saleId, [FromBody] SaleVO newSale)
        {
            if (saleId.Equals(0))
                throw new BusinessException("O campo ID da Venda é obrigatório.");

            if (newSale.Qntd.Equals(0))
                throw new BusinessException("O campo Quantidade da Venda é obrigatório.");

            return Ok(_saleService.UpdateSale(saleId, newSale.Qntd));
        }

        [HttpDelete("{saleId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult DeleteSale(int saleId)
        {
            if (saleId.Equals(0))
                throw new BusinessException("O campo ID da Venda é obrigatório.");

            return Ok(_saleService.DeleteSale(saleId));
        }

        [HttpPost("{saleId}/return")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult ReturnSale(int saleId, [FromBody] SaleReturnVO saleReturn)
        {
            if (saleId.Equals(0))
                throw new BusinessException("O campo ID da Venda é obrigatório.");

            if (saleReturn is null)
                throw new BusinessException("Os dados da devolução são obrigatórios.");

            return Ok(_saleService.ReturnSale(saleId, saleReturn.Quantity));
        }

        #endregion
    }
}
