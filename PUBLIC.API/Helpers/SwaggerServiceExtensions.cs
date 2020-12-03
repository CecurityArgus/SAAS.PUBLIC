using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PUBLIC.CONTROLLER.LIB.Security;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;

namespace PUBLIC.API.Helpers
{
    /// <summary>
    ///
    /// </summary>
    public static class SwaggerServiceExtensions
    {
        /// <summary>
        ///
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), new OpenApiInfo
                {
                    Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    Title = "public.api",
                    Description = "API endpoints for the SaaS PUBLIC solution",
                    Contact = new OpenApiContact()
                    {
                        Name = "Support",
                        Url = new Uri("https://www.cecurity.com"),
                        Email = "helpdesk@cecurity.com"
                    },
                    TermsOfService = new Uri("https://www.cecurity.com")
                });

                c.CustomSchemaIds(type => type.Name);

#if DEBUG
                c.AddServer(new OpenApiServer() { Url = "/" });
#else
                c.AddServer(new OpenApiServer() { Url = "/public.api" });
#endif
                c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}PUBLIC.CONTROLLER.LIB.xml");

                // Sets the basePath property in the Swagger document generated
                //c.DocumentFilter<BasePathFilter>("/platform.api");

                // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                c.OperationFilter<GeneratePathParamsValidationFilter>();

                c.AddSecurityDefinition(BearerAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
                {
                    Description = @"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = BearerAuthenticationHandler.SchemeName
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = BearerAuthenticationHandler.SchemeName
                        }
                        },
                        new string[] { }
                    }
                });
            });

            return services;
        }

        /// <summary>
        ///
        /// </summary>
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                var virtualDirectory = configuration["Appsettings:VirtualDirectory"];

                c.SwaggerEndpoint(
                    !String.IsNullOrEmpty(virtualDirectory)
                        ? $"/{virtualDirectory}/swagger/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}/swagger.json"
                        : $"/swagger/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}/swagger.json", "public.api");

                c.DocumentTitle = "PUBLIC REST API";

                c.DocExpansion(DocExpansion.None);
            });

            return app;
        }
    }
}