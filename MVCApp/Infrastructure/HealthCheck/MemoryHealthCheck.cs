using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MVCApp.Infrastructure.HealthCheck
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly IOptionsSnapshot<MemoryCheckOptions> _options;

        public string Name => "memory_check";

        public MemoryHealthCheck(IOptionsSnapshot<MemoryCheckOptions> options)
        {
            _options = options;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            MemoryCheckOptions opts = _options.Value;
            
            // Include GC information in the reported diagnostics.
            long allocated = GC.GetTotalMemory(false);
            var data = new Dictionary<string, object>
            {
                { "AllocatedBytes", allocated },
                { "Gen0Collections", GC.GetGeneration(0) },
                { "Gen1Collections", GC.GetGeneration(1) },
                { "Gen2Collections", GC.GetGeneration(2) }
            };
            HealthStatus status = (allocated < opts.Threshold) ? HealthStatus.Healthy : context.Registration.FailureStatus;

            return Task.FromResult(new HealthCheckResult(status, $"Reports degraded status if allocated bytes >= {opts.Threshold} bytes.", data: data));
        }
    }

    public class MemoryCheckOptions
    {
        /// <summary> Failure threshold (in bytes) </summary>
        public long Threshold { get; set; } = 1024L * 1024L * 1024L;
    }

    public static class GCInfoHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddMemoryHealthCheck(this IHealthChecksBuilder builder, string name, HealthStatus? failureStatus = null, IEnumerable<string> tags = null, long? thresholdInBytes = null)
        {
            builder.AddCheck<MemoryHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags ?? new string[0]);

            if (thresholdInBytes.HasValue)
            {
                builder.Services.Configure<MemoryCheckOptions>(opts =>
                {
                    opts.Threshold = thresholdInBytes.Value;
                });
            }

            return builder;
        }
    }
}
