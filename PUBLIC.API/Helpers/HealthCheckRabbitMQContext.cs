using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQDispatch.Client;
using System.Threading;
using System.Threading.Tasks;

namespace PUBLIC.API.Helpers
{
    internal class HealthCheckRabbitMQContext : IHealthCheck
    {
        private readonly IRabbitMqPersistentConnection _rabbitMqPersistentConnection;

        public HealthCheckRabbitMQContext(IRabbitMqPersistentConnection rabbitMqPersistentConnection)
        {
            _rabbitMqPersistentConnection = rabbitMqPersistentConnection;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_rabbitMqPersistentConnection.IsConnected ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
        }
    }
}