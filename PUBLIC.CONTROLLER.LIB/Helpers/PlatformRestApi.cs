using Newtonsoft.Json;
using PUBLIC.CONTROLLER.LIB.DTOs;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public class MdlMailTemplate
        {
            public string Id { get; set; }
            public string MailCode { get; set; }
            public string LanguageNeutralName { get; set; }
            public string FromAddr { get; set; }
            public string DisplayName { get; set; }
            public string ReplyToAddr { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public DateTime LastModified { get; set; }
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
                throw new Exception(response.Content);
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
                throw new Exception(response.Content);
            }
            else
            {
                subscriptions = JsonConvert.DeserializeObject<MdlSubscription>(response.Content);
            }

            return subscriptions;
        }

        public MdlMailTemplate GetMailTemplateByMailCodeAndReference(string mailCode, string languageNeutralName, string referenceType, string referenceId)
        {
            var request = new RestRequest("/Platform/PlatformMailTemplates/GetByMailCodeAndReference", Method.GET, DataFormat.Json);

            request.AddQueryParameter("mailCode", mailCode);
            request.AddQueryParameter("languageNeutralName", languageNeutralName);
            request.AddQueryParameter("referenceType", referenceType);
            request.AddQueryParameter("referenceId", referenceId);

            var response = Client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(response.Content);
            }

            return JsonConvert.DeserializeObject<MdlMailTemplate>(response.Content);
        }
    }
}
