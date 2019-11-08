using Newtonsoft.Json;
using PUBLIC.CONTROLLER.LIB.DTOs;
using RestSharp;
using System;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    class IdmRestApi
    {
        private RestClient Client = null;

        public string RestApiURL { get; set; }

        public IdmRestApi(string restApiURL)
        {
            RestApiURL = restApiURL;
            this.Client = new RestClient(restApiURL);
        }

        public DtoAuthentication.UserLoggedOn LogonUser(DtoAuthentication.LogonUser dtoLogonUser)
        {
            var client = new RestClient(this.RestApiURL);
            var request = new RestRequest("IdentityManagerAuth/Login", Method.POST, DataFormat.Json);

            request.AddJsonBody(dtoLogonUser);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {               
                throw new Exception(response.Content);
            }

            return JsonConvert.DeserializeObject<DtoAuthentication.UserLoggedOn>(response.Content);
        }
    }
}
