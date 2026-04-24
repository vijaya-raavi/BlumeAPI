using Moq;
using Ontec.Core.Domain.BankNotification;
using Ontec.Notification.Core;
using Ontec.Notification.Core.Implementations;
using Ontec.Notification.Core.Models;
using Ontec.Payment.Core.Interfaces;

namespace Ontec.Notification.Tests
{
    public class NotificationProcessorTests
    {
        [Fact]
        public async Task Process_Notification()
        {
            var notificationValidator = new ChecksumValidator();
            var mockNotificationStore = new Mock<IBankNotificationRepository>();
            var mockWalletServiceFactory = new Mock<IWalletServiceFactory>();
            var mockRechargeService = new Mock<IRechargeService>();
            var mockUserService = new Mock<IUserService>();

            mockWalletServiceFactory.Setup(m => m.GetWalletServiceInstanceAsync(It.IsAny<int>())).ReturnsAsync(new Mock<IWalletService>().Object);
            mockUserService.Setup(m => m.GetUserUsingPayerReferenceNumber(It.IsAny<string>())).ReturnsAsync(new UserModel()
            {
               UserId = 123
            });

            var notificationProcessor = new NotificationProcessor(notificationValidator,
                mockNotificationStore.Object,
                mockWalletServiceFactory.Object,
                mockRechargeService.Object,
                mockUserService.Object
                );

            await notificationProcessor.ProcessAsync(new BankNotificationDetails() { 
                TransactionValue = 20,
                IsCreditTransaction = true,
                AccountNumber = "1234567890",
                PayerReferenceNumber = "ABCD 987654321",
                NetUpChecksum = "ab13738a1a11d6b465623208fae841b9c128c885bb979cad0e5667403012f36e" //Generated from https://emn178.github.io/online-tools/sha256.html. By putting raw string as per our criteria. This should match checksum generated from our logic
            });

        }
    }
}