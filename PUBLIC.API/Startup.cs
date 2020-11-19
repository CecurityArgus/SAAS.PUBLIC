using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQDispatch.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PUBLIC.API.Helpers;
using PUBLIC.CONTROLLER.LIB.Security;
using PUBLIC.SERVICE.LIB.Helpers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using static PUBLIC.SERVICE.LIB.Helpers.MQErrors;

namespace PUBLIC.API
{
    public class Startup
    {
        private IConfiguration Conf { get; }
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private RabbitMqPersistentConnection _rabbitMqPersistentConnection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public Startup(IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Conf = builder.Build();

            _logger = logger;
            _logger.LogInformation("PUBLIC Rest API started.");

            var messageDict = new Dictionary<string, object>();
            var referenceDict = new Dictionary<string, object>();
            var factory = new ConnectionFactory { Uri = new Uri(Conf["RabbitMQConnection:Uri"]) };
            _rabbitMqPersistentConnection = new RabbitMqPersistentConnection(factory, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                System.Diagnostics.Process.GetCurrentProcess().ProcessName, System.Diagnostics.Process.GetCurrentProcess().Id.ToString(), JsonConvert.SerializeObject(MQErrorMessages));
            _rabbitMqPersistentConnection.SendApplicationInfoMessage((int)MQMessages.APP_INF_APPLICATIONSTARTED, messageDict, referenceDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ApiKeys>();
            var apiKeys = new ApiKeys(Conf);

            // RabbitMq
            services.AddSingleton<IRabbitMqPersistentConnection, RabbitMqPersistentConnection>(sp =>
            {
                return _rabbitMqPersistentConnection;
            });

            // Add framework services.
            services.AddMvc(options =>
            {
                options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
            })
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAny",
                builder =>
                {
                    // Not a permanent solution, but just trying to isolate the problem
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddAuthentication(BearerAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>(BearerAuthenticationHandler.SchemeName, null);

            services.AddSwaggerDocumentation();

            // Setting to override multipart limits
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });

            services.AddHealthChecks()
                .AddCheck<HealthCheckRabbitMQContext>("RabbitMQ context check");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<SerilogMiddleware>();

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseRouting();

            app.UseCors("AllowAny");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireCors("AllowAny");
            });

            //TODO: Enable production exception handling (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
            app.UseExceptionHandler("/error");

            if (!env.EnvironmentName.ToLower().Equals("production"))
            {
                app.UseSwaggerDocumentation(Conf);
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(report));
                    await context.Response.Body.WriteAsync(bytes);
                }
            });
        }
    }
}
