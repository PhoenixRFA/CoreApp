using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MVCApp.Infrastructure.HealthCheck
{
    public class StartupHostedServiceHealthCheck : IHealthCheck
    {
        private volatile bool _startupTaskCompleted = false;

        public string Name => "slow_dependency_check";

        public bool StartupTaskCompleted
        {
            get => _startupTaskCompleted;
            set => _startupTaskCompleted = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (StartupTaskCompleted)
            {
                return Task.FromResult(HealthCheckResult.Healthy("The startup task is completed"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("The startup task is still running completed"));
        }
    }
}
