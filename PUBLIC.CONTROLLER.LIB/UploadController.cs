using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PUBLIC.CONTROLLER.LIB.DTOs;
using PUBLIC.CONTROLLER.LIB.Helpers;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace PUBLIC.CONTROLLER.LIB.Controllers
{
    [Route("Public/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public UploadController(IConfiguration config, ILogger<AuthenticationController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("BeginOfTransfer")]
        [ProducesResponseType(200, Type = typeof(DtoUpload.BeginOfTransferRegistered))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BeginOfTransfer(DtoUpload.RegisterBeginOfTransfer dtoRegisterBeginOfTransfer)
        {
            try
            {
                var authToken = Request.Headers["Authorization"];
                PlatformRestApi platformRestApi = new PlatformRestApi(_config.GetSection("AppSettings:PlatformRestApiUrl").Value, authToken);
                platformRestApi.GetAuthorizedSubscriptions();

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Authenticate error: {exception.Message}");
                throw;
            }
        }
    }
}
