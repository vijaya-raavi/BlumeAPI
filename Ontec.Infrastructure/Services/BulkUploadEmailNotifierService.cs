using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Core.Domain.Models.Dto.BulkUpload;

namespace Ontec.Infrastructure.Services
{
    public class BulkUploadEmailNotifierService : BackgroundService
    {
        private const int BatchSize = 50;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BulkUploadEmailNotifierService> _logger;
        public BulkUploadEmailNotifierService(
            IServiceScopeFactory scopeFactory,
            ILogger<BulkUploadEmailNotifierService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollInterval);

            do
            {
                try
                {
                    await ProcessOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bulk upload email poll failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task ProcessOnceAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var bulkUploadRepo = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
            var otpService = scope.ServiceProvider.GetRequiredService<IOtpService>();

            var records = (await bulkUploadRepo.GetSuccessfulUnnotifiedRecordsAsync(BatchSize)).ToArray();
            if (records.Length == 0) return;

            var sentIds = new List<long>();

         
            var tasks = records.Select(async record =>
            {
                if (cancellationToken.IsCancellationRequested) return;

                try
                {
                    var staged = JsonSerializer.Deserialize<StagedBulkUser>(record.Payload)!;
                    var user = staged.User;
                    var emailModel = new EmailModelClass
                    {
                        companyId =user.CompanyId,
                        email = user.Email,
                        mobile = user.Mobile,
                        propertyUser = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email : user.FirstName,
                        forEvent = "newbulkUser",
                        body = string.Empty,
                        title = string.Empty,
                        subtitle = string.Empty
                    };

                    await otpService.SendEventMail(emailModel);
                    sentIds.Add(record.RecordId);
                }
                catch (Exception ex)
                {
                    // stays email_sent = false, retried on the next poll
                    _logger.LogError(ex, "Failed to send welcome email for record {RecordId}", record.RecordId);
                }
            });

            await Task.WhenAll(tasks);

            if (sentIds.Count() > 0)
                await bulkUploadRepo.MarkRecordsEmailSentAsync(sentIds);
        }
    }

}
