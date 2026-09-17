using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Service;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        #region Propriedades

        private readonly IInvoiceBusiness _invoiceService;

        #endregion

        #region Construtores

        public InvoicesController(IInvoiceBusiness invoiceService)
        {
            _invoiceService = invoiceService;
        }

        #endregion

        #region Métodos

        [HttpPost("preview")]
        public IActionResult PreviewInvoice([FromBody] InvoiceImportVO import)
        {
            try
            {
                if (import is null)
                    throw new Exception("O XML da nota fiscal é obrigatório.");

                return Ok(_invoiceService.PreviewInvoice(import.Xml));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ImportInvoice([FromBody] InvoiceImportVO import)
        {
            try
            {
                if (import is null)
                    throw new Exception("Os dados da importação são obrigatórios.");

                return Ok(_invoiceService.ImportInvoice(import));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
