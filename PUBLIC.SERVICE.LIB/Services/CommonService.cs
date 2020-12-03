using Metrics.Api.Client.Api;
using Microsoft.Extensions.Configuration;
using PUBLIC.SERVICE.LIB.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PUBLIC.SERVICE.LIB.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public CommonService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> UpdateMtricsForJobIdAsync(int messageCode, List<Metrics.Api.Client.Model.NameValuePair> data)
        {
            var metricsConfig = new Metrics.Api.Client.Client.Configuration()
            {
                BasePath = _config.GetSection("SMTP:MetricsRestApiUrl").Value
            };
            metricsConfig.Username = HMACApiDefaults.ClientId;
            metricsConfig.Password = HMACApiDefaults.ClientSecret;

            var metricsApi = new MetricsApi(metricsConfig);
            await metricsApi.PostMetricsAsync(new Metrics.Api.Client.Model.Metric() { Code = messageCode, Data = data });

            return "";
        }
    }
}
