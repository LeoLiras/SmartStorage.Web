using Asp.Versioning;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.EmailAPI.Repository.Interfaces;
using SmartStorage_Shared.VO;

namespace SmartStorage.EmailAPI.Controllers
{
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiVersion($"{Utils.Utils.apiVersion}")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        #region Properties

        private IEmailRepository _emailRepository;

        #endregion

        #region Constructors

        public EmailController(IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }

        #endregion

        #region Methods

        [HttpPost("new-product")]
        public async Task<IActionResult> SendNewProjectEmail([FromBody] ProductVO product)
        {
            try
            {
                if (product is null)
                    throw new Exception("É necessário fornecer dados do produto.");

                await _emailRepository.NewProductEmail(product);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
