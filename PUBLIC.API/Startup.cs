﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EPAIE.REPO.LIB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PUBLIC.API.Helpers;
using PUBLIC.CONTROLLER.LIB.Helpers;

namespace PUBLIC.API
{
    public class Startup
    {
        private readonly ILogger _logger;

        public IConfiguration Configuration { get; }

        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            _logger = logger;
            _logger.LogInformation("PUBLIC Rest API started.");
            _env = env;
        }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EPaieRepositoryContext>(x => x.UseSqlServer(Configuration.GetConnectionString("ePayConnection"), b => b.MigrationsAssembly("EPAIE.API")));
            services.AddScoped<IEPaieRepositoryWrapper, EPaieRepositoryWrapper>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
               .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
               .AddApplicationPart(Assembly.Load(new AssemblyName("PUBLIC.CONTROLLER.LIB")));

            services.AddCors();

            services.AddScoped<Tenants>();

            // Get access to the tenants
            var tenants = services.BuildServiceProvider().GetService<Tenants>();
 
            // Setup the JWT authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Specify what in the JWT needs to be checked 
                        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        // Specify the valid issue from appsettings.json
                        ValidIssuer = Configuration["Token:Issuer"],

                        // Specify the tenant API keys as the valid audiences 
                        ValidAudiences = tenants.Select(t => t.APIKey).ToList(),

                        IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters) =>
                        {
                            Tenant tenant = tenants.Where(t => t.APIKey == kid).FirstOrDefault();
                            List<SecurityKey> keys = new List<SecurityKey>();
                            if (tenant != null)
                            {
                                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tenant.SecretKey));
                                keys.Add(signingKey);
                            }
                            return keys;
                        }
                    };
                });
                /*.AddHMACAuthentication<HMACAuthenticationService>(o =>
                {
                    o.AuthorizedApplications = new List<AuthorizedApplication>();
                    o.AuthorizedApplications.Add(new AuthorizedApplication()
                    {
                        ApplicationId = "PublicAPI",
                        ApplicationSecret = "cc9a5a11-8831-4b99-9e59-bbac907c243c"
                    });

                }); */          

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });

            services.AddSwaggerDocumentation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocumentation(Configuration);
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseSwaggerDocumentation(Configuration);

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseMvc(routes => routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"));

            // Turn on authentication
            app.UseAuthentication();
        }
    }
}
