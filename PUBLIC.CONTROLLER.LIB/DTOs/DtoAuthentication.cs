namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class DtoAuthentication
    {
        /// <summary>
        /// 
        /// </summary>
        public class MdlLogonUser
        {
            /// <summary>
            /// 
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Password { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class MdlUserLoggedOn
        {
            /// <summary>
            /// 
            /// </summary>
            public string Token { get; set; }
        } 
        /// <summary>
        /// 
        /// </summary>
        public class MdlAuthenticated
        {
            /// <summary>
            /// 
            /// </summary>
            public string AccessToken { get; set; }
        }
    }
}
