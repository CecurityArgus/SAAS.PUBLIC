using Microsoft.Extensions.Configuration;
using PUBLIC.CONTROLLER.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    public class ApiKeys
    {
        private readonly IConfiguration _config;
        private readonly string _platformRestApiUrl;

        public ApiKeys(IConfiguration config)
        {
            _config = config;
            _platformRestApiUrl = _config["AppSettings:PlatformRestApiUrl"];
        }
        public List<ApiKey> GetApiKeys() 
        {
            try
            {
                var apiKeys = new List<ApiKey>();
                PlatformRestApi platformRestApi = new PlatformRestApi(_platformRestApiUrl, "");

                apiKeys = platformRestApi.GetApiKeys();

                return apiKeys;
            }
            catch (CecurityException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new CecurityException("PUBLIC_API_00000", exception.Message);
            }
        }
    }
}
