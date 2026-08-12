using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Ontec.WebUI.Filters;

namespace Ontec.WebUI.Config
{
    public static class SwaggerConfig
    {
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddMemoryCache();
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Version = "v1",
            //        Title = "Ontec API",
            //        Description = "Ontec",
            //    });
            //    var securityScheme = new OpenApiSecurityScheme
            //    {
            //        Name = "JWT Authentication",
            //        Description = "Enter JWT Bearer token **__only__**",
            //        In = ParameterLocation.Header,
            //        Type = SecuritySchemeType.Http,
            //        Scheme = "bearer",//This should in lower case
            //        BearerFormat = "JWT",
            //        Reference = new OpenApiReference
            //        {
            //            Id = JwtBearerDefaults.AuthenticationScheme,
            //            Type = ReferenceType.SecurityScheme
            //        }
            //    };
            //    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            //    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {securityScheme,new string[]{ } }
            //    });
            //    //c.OperationFilter<SwaggerFileOperationFilter>();
            //    c.OperationFilter<SwaggerDefaultValue>();
            //});

            services.AddSwaggerGen(c =>
            {

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Blume API",
                    Description = "Blume",
                });
                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "Blume API",
                    Description = "Blume",
                });
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    // Try to get the API version model associated with the action descriptor
                    var actionApiVersionModel = apiDesc.ActionDescriptor.GetApiVersionModel();

                    if (actionApiVersionModel == null)
                    {
                        return true; // Include actions without explicit versioning by default, if desired
                    }

                    // Check if any declared API version matches the current document name (e.g., "v1", "v2")
                    return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.MajorVersion}" == docName);
                });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **__only__**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",//This should in lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme,new string[]{ } }
                });
                //c.OperationFilter<SwaggerFileOperationFilter>();
                c.OperationFilter<SwaggerDefaultValue>();
            });
            return services;
            return services;
        }
    }
}
