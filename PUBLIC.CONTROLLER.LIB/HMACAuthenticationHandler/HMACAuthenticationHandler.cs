using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace PUBLIC.CONTROLLER.HMACAuthenticationHandler
{
    public class HMACAuthenticationHandler : AuthenticationHandler<HMACAuthenticationOptions>
    {
        private const string AuthenticationHeaderName = "Authentication";
        private const string TimestampHeaderName = "Timestamp";
        private readonly IHMACAuthenticationService _authenticationService;

        public HMACAuthenticationHandler(
            IOptionsMonitor<HMACAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHMACAuthenticationService authenticationService)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(AuthenticationHeaderName))
            {
                return AuthenticateResult.NoResult();
            }

            /*if (!Request.Headers.ContainsKey(TimestampHeaderName))
            {
                return AuthenticateResult.NoResult();
            }*/

            // get the tokens from the request header
            //var timestampValue = Request.Headers["Timestamp"];
            var authenticationHashValue = Request.Headers["Authentication"];
            
            if (!GetHashTokens(authenticationHashValue, out string requestApplicationId, out string requestTimeStamp, out string requestNonce, out string requestContentBase64String))
            {
                return AuthenticateResult.NoResult();
            }

            bool isValidUser = await _authenticationService.IsValidUserAsync(Options, requestApplicationId, requestTimeStamp, requestNonce, requestContentBase64String);

            if (!isValidUser)
            {
                return AuthenticateResult.Fail("Failed authentication");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, requestApplicationId) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        private static bool GetHashTokens(string authenticationHashValue, out string requestApplicationId, out string requestTimeStamp, out string requestNonce, out string requestContentBase64String)
        {
            requestApplicationId = string.Empty;
            requestTimeStamp = string.Empty;
            requestNonce = string.Empty;
            requestContentBase64String = string.Empty;

            var tokens = authenticationHashValue.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 4)
            {
                return false;
            }

            requestApplicationId = tokens[0];
            requestTimeStamp = tokens[1];
            requestNonce = tokens[2];
            requestContentBase64String = tokens[3];
            return true;
        }
    }
}
