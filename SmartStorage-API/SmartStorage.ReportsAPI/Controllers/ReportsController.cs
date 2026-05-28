using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.ReportsAPI.Repository.IRepository;
using SmartStorage.Shared.Auth;

namespace SmartStorage.ReportsAPI.Controllers
{
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiVersion($"{Utils.Utils.apiVersion}")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        #region Properties

        private readonly IReportRepository _reportRepository;

        #endregion

        #region Constructors

        public ReportsController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        #endregion

        #region Methods
        [Authorize]
        [HttpGet("export-excel")]
        public async Task<ActionResult> GenerateExcel()
        {
            try
            {
                var report = await _reportRepository.GenerateExcel();

                return File(
                    report,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Vendas {DateTime.Now.Month}-{DateTime.Now.Year}"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("export-pdf")]
        public async Task<ActionResult> GeneratePdf()
        {
            try
            {
                return File(
                    await _reportRepository.GeneratePdf(),
                    "application/pdf",
                    $"Vendas {DateTime.Now.Month}-{DateTime.Now.Year}"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
