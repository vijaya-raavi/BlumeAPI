using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using System.Reflection;

namespace Ontec.Core.Application
{
    public static class DepedencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //Add application dependencies here.
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IEncryptionandDecryption, EncryptionandDecryption>();
            services.AddScoped<ITransactionFeesService, TransactionFeesCalculation>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IVendRequestHelper, VendRequestHelper>();
            services.AddScoped<IBankNotificationValidator, BankNotificationValidator>();
            services.AddScoped<IPushNotification,PushNotification>();
            services.AddScoped<IUserBlockService, Userblock>();
            

            return services;
        }
    }
}
