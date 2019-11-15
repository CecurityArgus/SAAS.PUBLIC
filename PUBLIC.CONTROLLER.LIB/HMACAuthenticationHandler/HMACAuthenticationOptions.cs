using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace PUBLIC.CONTROLLER.HMACAuthenticationHandler
{
    public class AuthorizedApplication {
        public string ApplicationId { get; set; }

        public string ApplicationSecret { get; set; }
    }

    public class HMACAuthenticationOptions : AuthenticationSchemeOptions
    {
        public  List<AuthorizedApplication> AuthorizedApplications { get; set; }
    }
}