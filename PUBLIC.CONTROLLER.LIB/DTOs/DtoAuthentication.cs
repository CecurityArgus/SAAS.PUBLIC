using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    public class DtoAuthentication
    {
        public class LogonUser
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }

        public class UserLoggedOn
        {
            public string Token { get; set; }
        }
    }
}
