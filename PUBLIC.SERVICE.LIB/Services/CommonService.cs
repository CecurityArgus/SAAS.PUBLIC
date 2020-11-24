using Metrics.Api.Client.Api;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.SERVICE.LIB.Services
{
    public class CommonService
    {
        public static void UpdateMtricsForJobId(int messageCode, List<Metrics.Api.Client.Model.NameValuePair> data, IConfiguration config)
        {
            var metricsApi = new MetricsApi(config.GetSection("SMTP:MetricsRestApiUrl").Value);
            metricsApi.PostMetrics(new Metrics.Api.Client.Model.Metric() { Code = messageCode, Data = data });
        }
    }
}
