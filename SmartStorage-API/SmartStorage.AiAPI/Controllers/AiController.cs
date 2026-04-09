using Microsoft.AspNetCore.Mvc;
using SmartStorage.AIAPI.Repository.Interfaces;

namespace SmartStorage.AIAPI.Controllers
{
    [Route("api/storage/[controller]/v{version:apiVersion}")]
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
        public async Task<IActionResult> AnalyseSalesWithAI([FromBody] string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("O texto da requisição é obrigatório.");

                var result = await _aiRepository.CallAISales(text);

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
