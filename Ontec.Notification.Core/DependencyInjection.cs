using Microsoft.Extensions.DependencyInjection;
using Ontec.Notification.Core.Implementations;
using Ontec.Notification.Core.Interfaces;
using Ontec.Payment.Core;
using Ontec.Payment.Core.Implementations;
using Ontec.Payment.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Notification.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationDependencies(this IServiceCollection services) {
            services.AddScoped<INotificationProcessor, NotificationProcessor>();
            services.AddScoped<INotificationValidator, ChecksumValidator>();
            services.AddScoped<IWalletServiceFactory, WalletServiceFactory>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRechargeService, RechargeService>();

            return services;
        }
    }
}
