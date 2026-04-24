using Microsoft.Extensions.DependencyInjection;
using Ontec.Core.Domain.BankNotification;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Interface.Content;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Interface.ManagePermission;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Interface.Services;
using Ontec.Core.Domain.Interface.Status;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.UserNotificationConnection;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Infrastructure.Persistence.Property.Repository;
using Ontec.Infrastructure.Persistence.Repositories;
using Ontec.Infrastructure.Persistence.Repositories.CommunicationRepository;
using Ontec.Infrastructure.Persistence.Repositories.Company;
using Ontec.Infrastructure.Persistence.Repositories.Configuration;
using Ontec.Infrastructure.Persistence.Repositories.Consumer;
using Ontec.Infrastructure.Persistence.Repositories.ContentRepository;
using Ontec.Infrastructure.Persistence.Repositories.Document;
using Ontec.Infrastructure.Persistence.Repositories.Estate;
using Ontec.Infrastructure.Persistence.Repositories.GenericRepository;
using Ontec.Infrastructure.Persistence.Repositories.ManagePermission;
using Ontec.Infrastructure.Persistence.Repositories.MasterApiService;
using Ontec.Infrastructure.Persistence.Repositories.MeterRepository;
using Ontec.Infrastructure.Persistence.Repositories.Notification;
using Ontec.Infrastructure.Persistence.Repositories.NotificationRepository;
using Ontec.Infrastructure.Persistence.Repositories.Otp;
using Ontec.Infrastructure.Persistence.Repositories.PropertyUser;
using Ontec.Infrastructure.Persistence.Repositories.Status;
using Ontec.Infrastructure.Persistence.Repositories.TopUp;
using Ontec.Infrastructure.Persistence.Repositories.UserNotificationConnection;
using Ontec.Infrastructure.Persistence.Repositories.UserRepository;
using Ontec.Infrastructure.Persistence.Repositories.Wallet;
using Ontec.Infrastructure.Services;

namespace Ontec.Infrastructure
{
    public static class DepedencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            //Add infrastructure dependencies here.
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IMeterRepository, MeterRepository>();
            services.AddScoped<ICommunicationRepository, CommunicationRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IPropertyUserRepository, PropertyUserRepository>();
            services.AddScoped<IMasterApiConnectService, MasterApiConnectService>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<ITopUpRepository, TopUpRepository>();
            services.AddScoped<ISignalRService, SignalRService>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserNotificationConnection, UserNotificationConnection>();
            services.AddScoped<IConsumerRepository, ConsumerRepository>();
            services.AddScoped<IBankNotificationRepository, BankNotificationRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IContentRepository,ContentRepository>();
            services.AddScoped<IEstateRepository ,EstateRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            return services;
        }
    }
}
