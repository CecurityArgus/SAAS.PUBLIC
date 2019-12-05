using Newtonsoft.Json;
using PUBLIC.CONTROLLER.Helpers;
using PUBLIC.CONTROLLER.LIB.DTOs;
using RestSharp;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    class IdmRestApi
    {
        public class UserByIds
        {
            public string UserId { get; set; }
            public string ProviderId { get; set; }
            public string TenantId { get; set; }
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
                throw new CecurityException("PUBLIC_API_99999", $"IDM/GetByList error: Status '{response.StatusCode}', Error: '{response.Content}'", new { response.StatusCode, Message = response.Content });
            }

            return JsonConvert.DeserializeObject<DtoAuthentication.MdlUserLoggedOn>(response.Content);
        }
    }
}
