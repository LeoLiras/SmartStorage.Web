using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.AIAPI.Repository.Interfaces;
using SmartStorage.Shared.VO.AiService;

namespace SmartStorage.AIAPI.Controllers
{
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiVersion($"{Utils.Utils.apiVersion}")]
    [ApiController]
    public class AiController : ControllerBase
    {
        #region Properties

        private IAiRepository _aiRepository;

        #endregion

        #region Constructors

        public AiController(IAiRepository aiRepository)
        {
            _aiRepository = aiRepository;
        }

        #endregion

        #region Methods

        [HttpPost("analyse-sales")]
        //[TypeFilter(typeof(HyperMediaFilter))]
        public async Task<IActionResult> AnalyseSalesWithAI([FromBody] AiRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.aiQuestion))
                    throw new Exception("O texto da requisição é obrigatório.");

                var result = await _aiRepository.CallAISales(request.aiQuestion);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
