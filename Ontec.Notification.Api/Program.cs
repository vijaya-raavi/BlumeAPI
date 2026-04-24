using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ontec.Core.Cache;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Infrastructure;
using Ontec.Infrastructure.Persistence.Repositories.Common;
using Ontec.Notification.Core;
namespace Ontec.Notification.Api
{
    public class Program
    {
        public static ConfigurationManager Configuration;
        private static AppSettings AppSettings;
        //private JwtTokenConfig JwtTokenConfig { get; set; }
        private static CacheConfig CacheConfig;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Configuration = builder.Configuration;
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            ConfigurationAppSetting(builder.Services);
            builder.Services.AddInfrastructure();
            builder.Services.AddScoped<IWorkContext, WorkContext>();
            //builder.Services.AddScoped<IEncryptionandDecryption, EncryptionandDecryption>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddNotificationDependencies();

            //builder.Services.AddVersionedApiExplorer(setup =>
            //{
            //    setup.GroupNameFormat = "'v'VVV";
            //    setup.SubstituteApiVersionInUrl = true;
            //});
            builder.Services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.UseApiVersioning();

            app.Run();
        }

        private static void ConfigurationAppSetting(IServiceCollection services)
        {
            AppSettings = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            //CacheConfig = Configuration.GetSection(nameof(CacheConfig)).Get<CacheConfig>();
            services.AddSingleton(AppSettings.DatabaseSetting);
            //services.AddSingleton(CacheConfig);

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            //services.Configure<CacheConfig>(Configuration.GetSection("CacheConfig"));

        }
    }
}
