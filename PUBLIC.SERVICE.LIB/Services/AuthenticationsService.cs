using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Platform.Api.Client.Api;
using Platform.Api.Client.Model;
using Platform.Framework;
using PUBLIC.SERVICE.LIB.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PUBLIC.SERVICE.LIB.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationsService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly ApiKeys _apiKeys;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="apiKeys"></param>
        public AuthenticationsService(IConfiguration config, ILogger<AuthenticationsService> logger, ApiKeys apiKeys) 
        {
            _logger = logger;
            _config = config;
            _apiKeys = apiKeys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestAPIKey"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<string> AuthenticateAsync(string requestAPIKey, IdmAccountValidateRequest body)
        {
            string token = null;

            var apiKeys = _apiKeys.GetApiKeys();
            var apiKey = apiKeys.Where(t => t.Key.ToLower() == requestAPIKey.ToLower() && t.State.Equals(1)).FirstOrDefault();

            if (apiKey == null)
            {
                _logger.LogInformation($"AuthenticationController/Authenticate: apiKey '{requestAPIKey}' not found or not active (state=1)");
                throw new UnauthorizedAccessException();
            }

            // Get the token config from appsettings.json
            string issuer = _config["Token:Issuer"];
            if (!int.TryParse(_config["Token:ExpiryDurationMins"], out int expiryDuration))
            {
                expiryDuration = 30;
            }
            DateTime expiry = DateTime.UtcNow.Add(TimeSpan.FromMinutes(expiryDuration));

            List<Claim> claims = new List<Claim>();

            // Check the user credentials and return a 400 if they are invalid
            try
            {
                var accountsApi = new AccountsApi(_config.GetSection("AppSettings:PlatformRestApiUrl").Value);
                var response = await accountsApi.PlatformAccountsValidatePostAsyncWithHttpInfo(body);
                var refreshToken = response.Cookies.Any(a => a.Name.Equals("RefreshToken")) ? response.Cookies.Where(a => a.Name.Equals("RefreshToken")).First().Value.ToString() : "";

                var handler = new JwtSecurityTokenHandler();
                var argusToken = handler.ReadJwtToken(response.Data.Token);
                foreach (var claim in argusToken.Claims)
                    claims.Add(claim);

                claims.Add(new Claim("IDMAuthToken", response.Data.Token));
            }
            catch (System.Exception)
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
            token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            _logger.LogInformation($"AuthenticationController/Authenticate: Finished. Token: {token}");

            return token;
        }
    }
}
