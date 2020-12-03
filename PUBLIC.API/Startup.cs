using MassTransit;
using MassTransit.Dispatch.Client;
using MassTransit.Dispatch.Client.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly DispatchClient _dispatchClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public Startup(IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var environment = env.EnvironmentName;
            var jsonFile = "appsettings.json";
            if (!string.IsNullOrEmpty(environment))
                jsonFile = $"appsettings.{environment.Trim()}.json";

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(jsonFile, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            ProcessData processData = new ProcessData()
            {
                ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                ProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                ProcessVersion = System.Diagnostics.Process.GetCurrentProcess().Id.ToString()
            };

            IBusControl bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri($"rabbitmq://{_configuration.GetSection("MassTransit:RabbitMQ:HostAddress").Value}"), h =>
                {
                    h.Username(_configuration.GetSection("MassTransit:RabbitMQ:Username").Value);
                    h.Password(_configuration.GetSection("MassTransit:RabbitMQ:Password").Value);
                });
            });

            _dispatchClient = new DispatchClient(bus, processData, JsonConvert.SerializeObject(MQErrors.MQErrorMessages));
            _dispatchClient.SendApplicationInfoMessageAsync((int)MQMessages.APP_INF_APPLICATIONSTARTED, new Dictionary<string, object>(), new Dictionary<string, object>()).Wait();

            _logger = logger;
            _logger.LogInformation("PUBLIC Rest API started.");            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //Entity framework database context
            services.AddDatabaseContext(_configuration);

            services.AddSingleton<DispatchClient>(cfg =>
            {
                return _dispatchClient;
            });

            // Add framework services.
            services.AddCorsConfiguration();

            services.AddMvc(options =>
            {
                options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
            })
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            //Add authentication
            services.AddCustomAuthentication();

            //Add swagger documentation
            services.AddSwaggerDocumentation();

            // Setting to override multipart limits
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });

            //Add health checks
            services.AddHealthChecks()
                .AddCheck<HealthCheckMassTransit>("MassTransit context check");

            //Load custom services
            services.AddCustomServices(_configuration);
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

            if (!env.EnvironmentName.ToLower().Equals("production"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocumentation(_configuration);
            }
            else
            {
                //TODO: Enable production exception handling (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
                app.UseExceptionHandler("/Error");

                app.UseHsts();

                //TODO: Use Https Redirection
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
