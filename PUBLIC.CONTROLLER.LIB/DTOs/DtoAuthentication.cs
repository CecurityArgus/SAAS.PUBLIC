using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    public class DtoAuthentication
    {
        public class MdlUser
        {
            public string id { get; set; }
            public string tenantID { get; set; }
            public string providerID { get; set; }
            public string loginName { get; set; }
            public string name { get; set; }
            public string surName { get; set; }
            public string emailAddr { get; set; }
        }
        public class MdlLogonUser
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }

        public class MdlUserLoggedOn
        {
            public string Token { get; set; }
        }
    }
}
