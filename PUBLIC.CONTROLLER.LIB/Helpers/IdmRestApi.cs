using Newtonsoft.Json;
using PUBLIC.CONTROLLER.LIB.DTOs;
using RestSharp;
using System;
using System.Collections.Generic;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    class IdmRestApi
    {
        public class DtoGetByIds
        {
            public List<UserByIds> UserByIds { get; set; }
        }

        public class UserByIds
        {
            public string UserId { get; set; }
            public string ProviderId { get; set; }
            public string TenantId { get; set; }
        }

        public class MdlUser
        {
            public string id { get; set; }
            public string tenantID { get; set; }
            public string providerID { get; set; }
            public string loginName { get; set; }
            public string name { get; set; }
            public string surName { get; set; }
            public string emailAddr { get; set; }
            public string passwordHash { get; set; }
            public string passwordSalt { get; set; }
            public bool changePasswordAtNextLogon { get; set; }
            public int invalidLoginAttempts { get; set; }
            public DateTime lastInvalidLoginAttempt { get; set; }
            public DateTime lastLogin { get; set; }
            public string pwdForgottenRequestId { get; set; }
            public DateTime lastPwdForgottenRequest { get; set; }
            public string accountActivationId { get; set; }
            public DateTime lastAccountActivationRequest { get; set; }
        }

        private RestClient Client = null;
        public string RestApiURL { get; set; }

        public IdmRestApi(string restApiURL)
        {
            RestApiURL = restApiURL;
            this.Client = new RestClient(restApiURL);
        }

        public DtoAuthentication.MdlUserLoggedOn LogonUser(DtoAuthentication.MdlLogonUser dtoLogonUser)
        {
            var client = new RestClient(this.RestApiURL);
            var request = new RestRequest("IdentityManagerAuth/Login", Method.POST, DataFormat.Json);

            request.AddJsonBody(dtoLogonUser);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {               
                throw new Exception(response.Content);
            }

            return JsonConvert.DeserializeObject<DtoAuthentication.MdlUserLoggedOn>(response.Content);
        }

        public DtoAuthentication.MdlUserLoggedOn GetUserByLoginName(DtoAuthentication.MdlLogonUser dtoLogonUser)
        {
            var client = new RestClient(this.RestApiURL);
            var request = new RestRequest("IdentityManagerUser/GetByLoginName", Method.POST, DataFormat.Json);

            request.AddQueryParameter("loginName", "");

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(response.Content);
            }

            return JsonConvert.DeserializeObject<DtoAuthentication.MdlUserLoggedOn>(response.Content);
        }

        public List<MdlUser> IdentityManagerUser_GetByList(DtoGetByIds getByIdsDto)
        {
            var request = new RestRequest("IdentityManagerUser/GetByList", Method.POST, DataFormat.Json);

            request.AddJsonBody(getByIdsDto.UserByIds);

            var response = Client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(response.Content);

            return JsonConvert.DeserializeObject<List<MdlUser>>(response.Content);
        }
    }
}
