using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Platform.Framework;
using PUBLIC.CONTROLLER.LIB.DTOs;
using PUBLIC.SERVICE.LIB.Helpers;
using PUBLIC.SERVICE.LIB.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static PUBLIC.SERVICE.LIB.Helpers.MQErrors;

namespace PUBLIC.CONTROLLER.LIB.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("Public/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly AuthenticationsService _authenticationsService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticationsService"></param>
        public AuthenticationController(ILogger<AuthenticationController> logger, AuthenticationsService authenticationsService)
        {
            _logger = logger;
            _authenticationsService = authenticationsService;
        }

        #region CONTROLLERS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("Authenticate")]
        [ProducesResponseType(200, Type = typeof(DtoAuthentication.MdlAuthenticated))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(400, Type = typeof(CecurityException))]
        public async Task<IActionResult> Authenticate(DtoAuthentication.MdlLogonUser body)
        {
            _logger.LogInformation("AuthenticationController/Authenticate: Started");

            try
            {
                // Check the API key is correct
                string requestAPIKey = Request.Headers["X-ApiKey"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(requestAPIKey))
                    throw new CecurityException((int)MQMessages.APP_ERR_NO_APIKEY_PROVIDED, "No APIKey provided !", null);

                var response = await _authenticationsService.AuthenticateAsync(requestAPIKey, new Platform.Api.Client.Model.IdmAccountValidateRequest() { LoginName = body.Username, Password = body.Password });

                _logger.LogInformation($"AuthenticationController/Authenticate: Finished. Token: {response}");

                // return the token
                return Ok(new { AccessToken = response });                
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"AuthenticationController/Authenticate: Error: {exception.Message}");

                throw;
            }
            catch (UnauthorizedAccessException uEx)
            {
                return Unauthorized(uEx.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"AuthenticationController/Authenticate: Error: {exception.Message}");

                throw new CecurityException((int)MQMessages.APP_ERR_AUTHENTICATE_FAILED, exception.Message, exception);
            }
        }

        #endregion
    }
}
