using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PUBLIC.CONTROLLER.Helpers;
using PUBLIC.CONTROLLER.LIB.DTOs;
using PUBLIC.CONTROLLER.LIB.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace PUBLIC.CONTROLLER.LIB.Controllers
{
    [Route("Public/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly ApiKeys _apiKeys;

        public AuthenticationController(IConfiguration config, ILogger<AuthenticationController> logger, ApiKeys apiKeys)
        {
            _config = config;
            _logger = logger;
            _apiKeys = apiKeys;
        }

        #region CONTROLLERS

        [HttpPost("Authenticate")]
        [ProducesResponseType(200, Type = typeof(DtoAuthentication.MdlAuthenticated))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(400, Type = typeof(CecurityError))]
        public IActionResult Authenticate(DtoAuthentication.MdlLogonUser dtoLogonUser)
        {
            _logger.LogInformation("AuthenticationController/Authenticate: Started");

            try
            {
                // Check the API key is correct
                string requestAPIKey = Request.Headers["X-ApiKey"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(requestAPIKey))
                    throw new CecurityException("PUBLIC_API_00251", "No APIKey provided !");

                var apiKeys = _apiKeys.GetApiKeys();
                var apiKey = apiKeys.Where(t => t.Key.ToLower() == requestAPIKey.ToLower() && t.State.Equals(1)).FirstOrDefault();

                if (apiKey == null)
                {
                    _logger.LogInformation($"AuthenticationController/Authenticate: apiKey '{requestAPIKey}' not found or not active (state=1)");
                    return Unauthorized();
                }

                // Get the token config from appsettings.json
                string issuer = _config["Token:Issuer"];
                if (!int.TryParse(_config["Token:ExpiryDurationMins"], out int expiryDuration))
                {
                    expiryDuration = 30;
                }
                DateTime expiry = DateTime.UtcNow.Add(TimeSpan.FromMinutes(expiryDuration));

                JwtSecurityToken argusToken = new JwtSecurityToken();
                var argusAuthToken = "";
                List<Claim> claims = new List<Claim>();

                // Check the user credentials and return a 400 if they are invalid
                try
                {
                    IdmRestApi idmRestApi = new IdmRestApi(_config.GetSection("AppSettings:IdmRestApiUrl").Value);
                    var dtoUserLoggedOn = idmRestApi.LogonUser(dtoLogonUser);
                    argusAuthToken = dtoUserLoggedOn.Token;
                    var handler = new JwtSecurityTokenHandler();
                    argusToken = handler.ReadJwtToken(dtoUserLoggedOn.Token);
                    foreach (var claim in argusToken.Claims)
                        claims.Add(claim);
                    
                    claims.Add(new Claim("IDMAuthToken", dtoUserLoggedOn.Token));
                }
                catch(Exception)
                {
                    throw;
                }
                // Create and sign the token                
                var jwtSecurityToken = new JwtSecurityToken(                    
                    issuer: issuer,
                    audience: requestAPIKey,
                    claims: claims.ToArray(),
                    expires: expiry,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey.Secret)),
                        SecurityAlgorithms.HmacSha256
                    )
                );
                jwtSecurityToken.Header.Add("kid", requestAPIKey);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                _logger.LogInformation($"AuthenticationController/Authenticate: Finished. Token: {token}");

                // return the token
                return Ok(new { AccessToken = token });                
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"AuthenticationController/Authenticate: Error: {exception.Message}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"AuthenticationController/Authenticate: Error: {exception.Message}");

                throw new CecurityException("PUBLIC_API_00250", exception.Message);
            }
        }

        #endregion
    }
}
