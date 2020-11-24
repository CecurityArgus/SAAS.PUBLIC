using Epaie.Api.Client.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Platform.Api.Client.Api;
using Platform.Framework;
using PUBLIC.SERVICE.LIB.Security;
using System;
using System.Collections.Generic;
using static PUBLIC.SERVICE.LIB.Helpers.MQErrors;

namespace PUBLIC.SERVICE.LIB.Helpers
{
    public class ApiKeys
    {
        private readonly IConfiguration _config;
        private readonly Platform.Api.Client.Client.Configuration _platformConfig;

        public ApiKeys(IConfiguration config)
        {
            _config = config;
            _platformConfig = new Platform.Api.Client.Client.Configuration
            {
                BasePath = _config["AppSettings:PlatformRestApiUrl"],
                Username = HMACApiDefaults.PlatformClientId,
                Password = HMACApiDefaults.PlatformClientSecret
            };
        }
        public List<ApiKey> GetApiKeys() 
        {
            try
            {
                var apiKeysApi = new PlatformApiKeysApi(_platformConfig);
                var apiKeysResponse = apiKeysApi.PlatformPlatformApiKeysGet();

                var apiKeys = JsonConvert.DeserializeObject<List<ApiKey>>(JsonConvert.SerializeObject(apiKeysResponse));

                return apiKeys;
            }
            catch (ApiException aEx)
            {
                if (aEx.ErrorCode == 401)
                    throw new UnauthorizedAccessException();

                throw new CecurityException((int)MQMessages.APP_ERR_APIKEYS, aEx.Message, aEx.ErrorContent.ToString(), aEx);
            }
            catch (Exception exception)
            {
                throw new CecurityException((int)MQMessages.APP_ERR_APIKEYS, exception.Message, exception);
            }
        }
    }
}
