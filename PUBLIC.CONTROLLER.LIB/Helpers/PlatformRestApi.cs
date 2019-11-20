using Newtonsoft.Json;
using PUBLIC.CONTROLLER.Helpers;
using RestSharp;
using System;
using System.Collections.Generic;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    public class PlatformRestApi
    {
        public class MdlSolutionSpecific
        {
            public string Name { get; set; }
            public int Version { get; set; }
            public string Params { get; set; }
        }

        public class MdlConnectorParams
        {
            public string Domain { get; set; }
            public string DocumentType { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
            public string CustomerName { get; set; }
            public string CustomerKey { get; set; }
        }
        public class MdlConnector
        {
            public string Name { get; set; }
            public int Version { get; set; }
            public string Params { get; set; }
        }

        public class MdlOption
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public class MdlModule
        {
            public string Name { get; set; }
            public int Version { get; set; }
            public string Params { get; set; }
        }

        public class MdlSolutionParams
        {
            public MdlSolutionSpecific Solution { get; set; }
            public List<MdlConnector> Connectors { get; set; }
            public List<MdlOption> Options { get; set; }
            public List<MdlModule> Modules { get; set; }
        }

        public class MdlSolution
        {
            public string Id { get; set; }
            public string SolutionName { get; set; }
            public string Params { get; set; }
        }

        public class MdlSubscription
        {
            public string Id { get; set; }
            public int ReferenceType { get; set; }
            public string ReferenceId { get; set; }
            public string SolutionId { get; set; }
            public string SolutionParams { get; set; }
            public MdlSolution Solution { get; set; }
        }

        readonly RestClient Client = null;

        public string RestApiURL { get; set; }

        public PlatformRestApi(string restApiURL, string authToken)
        {
            RestApiURL = restApiURL;
            this.Client = new RestClient(restApiURL);

            if (!String.IsNullOrEmpty(authToken))
            {
                Client.AddDefaultHeader("Authorization", authToken);
            }
        }

        public List<MdlSubscription> GetAuthorizedSubscriptions()
        {
            List<MdlSubscription> subscriptions = null;

            var request = new RestRequest("/Platform/PlatformSubscriptions/GetAuthorizedSubscriptions", Method.GET, DataFormat.Json);

            var response = Client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {               
                throw new CecurityException("PUBLIC_API_99999", $"GetAuthorizedSubscriptions error: Status '{response.StatusCode}', Error: '{response.Content}'", new { StatusCode = response.StatusCode, Message = response.Content });
            }
            else
            {
                subscriptions = JsonConvert.DeserializeObject<List<MdlSubscription>>(response.Content);
            }

            return subscriptions;
        }

        public MdlSubscription GetSubscriptionById(string subscriptionId)
        {
            MdlSubscription subscriptions = null;

            var request = new RestRequest("/Platform/PlatformSubscriptions/{id}", Method.GET, DataFormat.Json);

            request.AddUrlSegment("id", subscriptionId);

            var response = Client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new CecurityException("PUBLIC_API_99999", $"PlatformSubscriptions error: Status '{response.StatusCode}', Error: '{response.Content}'", new { StatusCode = response.StatusCode, Message = response.Content });
            }
            else
            {
                subscriptions = JsonConvert.DeserializeObject<MdlSubscription>(response.Content);
            }

            return subscriptions;
        }
      
    }
}
