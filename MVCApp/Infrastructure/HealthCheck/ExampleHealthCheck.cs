using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MVCApp.Infrastructure.HealthCheck
{
    public class ExampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            bool isHealthy = true;
            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Seems all is fine",  new Dictionary<string, object>{ {"foo", "bar"} }));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Oops. Something is wrong"));
        }
    }
}
