using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerServiceDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started");

            // Logging child property Name of structured object structured
            // to a separate column according to configuration in AddictionalColumns in appsettings.json
            var structured = new Structured
            {
                Name = "Structured Subproperty Value"
            };
            _logger.LogInformation("{@Structured} {@Scalar}", structured, "Scalar Value");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}. CustomProperty1: {CustomProperty1}",
                    DateTimeOffset.Now, "Value");
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Worker stopping ...");
        }
    }
}
