using MassTransit.Dispatch.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PUBLIC.API.Helpers
{
    /// <summary>
    ///
    /// </summary>
    public class HealthCheckMassTransit : IHealthCheck
    {
        private readonly DispatchClient _dispatchClient;

        /// <summary>
        ///
        /// </summary>
        /// <param name="dispatchClient"></param>
        public HealthCheckMassTransit(DispatchClient dispatchClient)
        {
            _dispatchClient = dispatchClient;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dispatchClient.SendHealthMessageAsync();
                return HealthCheckResult.Healthy();
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy();
            }
        }
    }
}