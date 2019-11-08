using Newtonsoft.Json;
using PUBLIC.CONTROLLER.LIB.DTOs;
using RestSharp;
using System;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    class PlatformRestApi
    {
        readonly RestClient Client = null;
        
        readonly string authToken;

        public string RestApiURL { get; set; }

        public PlatformRestApi(string restApiURL, string authToken)
        {
            RestApiURL = restApiURL;
            this.Client = new RestClient(restApiURL);
            this.authToken = authToken;
        }

        public void GetAuthorizedSubscriptions()
        {
            var client = new RestClient(this.RestApiURL);
            var request = new RestRequest("/Platform/PlatformSubscriptions/GetAuthorizedSubscriptions", Method.GET, DataFormat.Json);

            request.AddHeader("Authorization", authToken);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {               
                throw new Exception(response.Content);
            }
        }
    }
}
