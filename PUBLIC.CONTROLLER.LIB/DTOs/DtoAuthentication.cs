namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    public class DtoAuthentication
    {
        public class MdlLogonUser
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }
        public class MdlUserLoggedOn
        {
            public string Token { get; set; }
        } 
        public class MdlAuthenticated
        {
            public string AccessToken { get; set; }
        }
    }
}
