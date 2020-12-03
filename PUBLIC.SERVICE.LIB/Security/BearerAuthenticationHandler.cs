using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PUBLIC.SERVICE.LIB.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace PUBLIC.CONTROLLER.LIB.Security
{
    /// <summary>
    /// class to handle bearer authentication.
    /// </summary>
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// scheme name for authentication handler.
        /// </summary>
        public const string SchemeName = "Bearer";

        private readonly IConfiguration _configuration;
        private readonly ApiKeys _apiKeys;

        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="configuration"></param>
        public BearerAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration, ApiKeys apiKeys) : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _apiKeys = apiKeys;
        }

        /// <summary>
        /// Verify that require authorization header exists.
        /// </summary>        
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                if (!authHeader.Scheme.Equals(SchemeName))
                    throw new Exception();

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();

                IPrincipal principal = await Task.Run(() => tokenHandler.ValidateToken(authHeader.Parameter, validationParameters, out SecurityToken validatedToken));

                var ticket = new AuthenticationTicket((ClaimsPrincipal)principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception)
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                // Specify what in the JWT needs to be checked 
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                // Specify the valid issue from appsettings.json
                ValidIssuer = _configuration["Token:Issuer"],
                // Specify the tenant API keys as the valid audiences 
                ValidAudiences = _apiKeys.GetApiKeys().Select(a => a.Key).ToList(),

                IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
                {
                    ApiKey apiKey = _apiKeys.GetApiKeys().Where(t => t.Key == kid && t.State.Equals(1)).FirstOrDefault();
                    List<SecurityKey> keys = new List<SecurityKey>();
                    if (apiKey != null)
                    {
                        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey.Secret));
                        keys.Add(signingKey);
                    }
                    return keys;
                }
            };
        }
    }
}