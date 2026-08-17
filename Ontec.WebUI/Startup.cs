using Dapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Ontec.Core.Application;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Cache;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Infrastructure.Repositories;
using Ontec.Infrastructure;
using Ontec.Infrastructure.Persistence.Configurations;
using Ontec.Infrastructure.Persistence.Repositories.BulkUpload;
using Ontec.Infrastructure.Persistence.Repositories.Common;
using Ontec.Infrastructure.Persistence.Repositories.Wallet;
using Ontec.Infrastructure.Services;
using Ontec.WebUI.Config;
using Ontec.WebUI.Filters;
using Ontec.WebUI.Infrastructure;
using Serilog;
using System.Globalization;
using System.Text;

namespace Ontec.WebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private AppSettings AppSettings { get; set; }
        private JwtTokenConfig JwtTokenConfig { get; set; }
        private MasterApiSetting MasterApiSetting { get; set; }
        private CacheConfig CacheConfig { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var cultureInfo = new CultureInfo("en-US");

            //var cultureInfo = new CultureInfo("en-ZA");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            //CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            //CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            //For JWT start here 
            var jwtTokenConfig = Configuration.GetSection(nameof(JwtTokenConfig)).Get<JwtTokenConfig>();
            services.AddSingleton(jwtTokenConfig);

            var masterApi = Configuration.GetSection(nameof(MasterApiSetting)).Get<MasterApiSetting>();
            services.AddSingleton(masterApi);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtTokenConfig.Issuer,
                    ValidAudience = jwtTokenConfig.Audience,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
            services.AddSingleton<IJwtAuthManager, JwtAuthManager>();
            services.AddHostedService<JwtRefreshTokenCache>();
            //JWT Ends Here 
            services.AddApplication();
            ConfigurationAppSetting(services);
            CacheDepedenciesConfig.ConfigureDependencies(services);


            services.AddInfrastructure();

            //----------------------Bulk Upload---------------------//
            services.AddSingleton<IBulkUploadQueue, BulkUploadQueue>();
            services.AddHostedService<BulkUploadBackgroundService>();

            services.AddScoped<IBulkUploadRepository, BulkUploadRepository>();
            services.AddScoped<IUserProvisioningService, UserProvisioningService>();
            services.AddScoped<IBulkUploadProcessor, BulkUploadProcessor>();
            services.AddHostedService<BulkUploadEmailNotifierService>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            //----------------------Bulk Upload---------------------//
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 5242880000; // If don't set default value is : 128 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            services.AddScoped<IWorkContext, WorkContext>();
            services.AddScoped<ICompanyHelper,CompanyHelper>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IAuditTrail, AuditTrailService>();
            // services.AddScoped<IEncryptionandDecryption, EncryptionandDecryption>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ILiveUserService, LiveUserService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddControllersWithViews();


            //added for sequrity headers
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });
            
            services.AddHttpContextAccessor();
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilterAttribute>();
                options.Filters.Add<ReuestHeaderFilterAttribute>();

            }).AddFluentValidation(x => x.AutomaticValidationEnabled = false);
            // Configure FluentValidation to use custom property name resolver
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                    });
                options.AddPolicy("hubWeb",
                builder => builder
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .AllowCredentials()
                     .WithOrigins("http://localhost:3000/"));
            });
            services.AddEndpointsApiExplorer();
            services.ConfigureSwagger();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSignalR(options =>
            {
                options.AddFilter<OntecHubFilter>();
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);


            services.AddSpaStaticFiles(config =>
            config.RootPath = "ClientApp/dist");
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    foreach (var description in provider.ApiVersionDescriptions)
            //    {
            //        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            //    }
            //    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            //});

            app.UseSwaggerUI(c =>
            {
                //foreach (var description in provider.ApiVersionDescriptions)
                //{
                //c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                //}
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                c.DefaultModelsExpandDepth(-1); // Hide models
                c.DefaultModelExpandDepth(1);
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
            });


            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging(configure =>
            {
                configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} ({UserId}) responsed {StatusCode} in {ElapsedL:0.0000}ms";
            });

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.Add("Referrer-Policy", "no-referrer, strict-origin-when-cross--origin");
                ctx.Response.Headers.Add("Permission-policy", "geolocation=(self), microphone=()");

                await next();
            });
            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
            app.UseCors("hubWeb");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHub<OntecHub>("/ontec");
            });
            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    var liveUserService = scope.ServiceProvider.GetRequiredService<ILiveUserService>();
            //    var dbSetting = scope.ServiceProvider.GetRequiredService<DatabaseSetting>();

            //    using var connection = new NpgsqlConnection(AppSettings.DatabaseSetting.OntechDbConnectionString);
            //    var connectionIds = connection.Query<string>("SELECT connection_id FROM user_notification_connections").ToList();

            //    liveUserService.InitializeConnections(connectionIds);
            //}

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var liveUserService = scope.ServiceProvider.GetRequiredService<ILiveUserService>();
                var dbSetting = scope.ServiceProvider.GetRequiredService<DatabaseSetting>();

                using var connection = new NpgsqlConnection(AppSettings.DatabaseSetting.OntechDbConnectionString);

                var connectionData = connection.Query<ConnectionData>(
                    "SELECT connection_id AS ConnectionId, user_id AS UserId, client_type AS ClientType FROM public.user_notification_connections"
                ).ToList();

                liveUserService.InitializeConnections(connectionData);
            }
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer(Configuration["SpaBaseUrl"] ?? "http://localhost:3000");
                }
            });
        }
        private void ConfigurationAppSetting(IServiceCollection services)
        {
            AppSettings = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            CacheConfig = Configuration.GetSection(nameof(CacheConfig)).Get<CacheConfig>();
            services.AddSingleton(AppSettings.DatabaseSetting);
            services.AddSingleton(CacheConfig);

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<CacheConfig>(Configuration.GetSection("CacheConfig"));

        }
    }
}
