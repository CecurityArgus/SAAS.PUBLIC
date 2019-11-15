using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PUBLIC.CONTROLLER.LIB.DTOs;
using PUBLIC.CONTROLLER.LIB.Helpers;
using System;

namespace PUBLIC.CONTROLLER.LIB.Controllers
{
    [Route("Public/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;

        private readonly ILogger _logger;

        public AuthenticationController(IConfiguration config, ILogger<AuthenticationController> logger)
        {
            _config = config;
            _logger = logger;
        }
        
        [HttpPost("Authenticate")]
        [ProducesResponseType(200, Type = typeof(DtoAuthentication.MdlUserLoggedOn))]
        public IActionResult Authenticate(DtoAuthentication.MdlLogonUser dtoLogonUser)
        {
            try
            {
                IdmRestApi idmRestApi = new IdmRestApi(_config.GetSection("AppSettings:IdmRestApiUrl").Value);
                var dtoUserLoggedOn = idmRestApi.LogonUser(dtoLogonUser);

                return Ok(dtoUserLoggedOn);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Authenticate error: {exception.Message}");
                throw;
            }
        }
    }
}
