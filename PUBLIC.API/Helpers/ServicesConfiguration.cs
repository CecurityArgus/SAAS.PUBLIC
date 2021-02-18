using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PUBLIC.CONTROLLER.LIB.Security;
using PUBLIC.SERVICE.LIB.Helpers;
using PUBLIC.SERVICE.LIB.Services;

namespace PUBLIC.API.Helpers
{
    /// <summary>
    ///
    /// </summary>
    public static class ServicesConfiguration
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();            
            //Add custome services
            services.AddTransient<ApiKeys>();
            services.AddTransient<AuthenticationsService>();
            services.AddTransient<UploadService>();
            services.AddTransient<CommonService>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomAuthentication(this IServiceCollection services)
        {
            //HMAC authentication
            /*services.AddAuthentication(HMACAuthenticationDefaults.AuthenticationScheme)
                .AddHMACAuthentication<HMACAuthenticationService>(o =>
                {
                    o.AuthorizedClients = new List<AuthorizedClient>
                    {
                        new AuthorizedClient() { ClientId = "EPAIE.API", ClientSecret = "4854360F-F693-4315-B158-1406001332AA" },
                        new AuthorizedClient() { ClientId = "EFACTURE.API", ClientSecret = "991A7074-0D40-4C85-BB68-22F829AE3BD2" },
                        new AuthorizedClient() { ClientId = "A4SB.EPAIE", ClientSecret = "0C60B38C-9294-4771-BCA4-DDB88738E080" },
                        new AuthorizedClient() { ClientId = "A4SB.EFACTURE", ClientSecret = "E86316FC-7DE9-465B-BBC4-9688D4402149" },
                        new AuthorizedClient() { ClientId = "CHORUSPRO.DISTRIBUTION.SERVICE", ClientSecret = "10A8BF0E-C70B-4B21-BCE4-87BB3CA29AA9" }
                    };
                });*/
            //Bearer authentication
            services.AddAuthentication(BearerAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>(BearerAuthenticationHandler.SchemeName, null);
            //Api-Key authentication
            //services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
                //.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            //Entity framework database context
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "AllowAny",
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                    });
            });
        }
    }
}