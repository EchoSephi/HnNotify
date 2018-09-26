using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HnNotify.Data;
using HnNotify.Service;

namespace HnNotify {
    public class Startup {
        private string _connectionStr;
        public IConfiguration Configuration { get; }

        public Startup (IHostingEnvironment env) {
            var builder = new ConfigurationBuilder ()
                .SetBasePath (env.ContentRootPath)
                .AddJsonFile ("appsettings.json", optional : true, reloadOnChange : true)
                .AddJsonFile ($"appsettings.{env.EnvironmentName}.json", optional : true)
                .AddEnvironmentVariables ();
            Configuration = builder.Build ();

            // _connectionStr = Configuration["ConnectionStrings:Dev"];
            _connectionStr = UStore.GetUStore (Configuration["ConnectionStrings:Dev"]);
        }

        public void ConfigureServices (IServiceCollection services) {
            var connectionStr = _connectionStr;

            services.AddDbContext<HNContext> (options => options.UseSqlServer (connectionStr));
            services.AddScoped<IMembersService, MembersService> ();
            services.AddScoped<INotifyService, NotifyService> ();
            services.AddScoped<INotifyItemService, NotifyItemService> ();

            services.AddSingleton<IConfiguration> (Configuration);
            services.Configure<CookiePolicyOptions> (options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddCors (options => options.AddPolicy ("CorsDomain",
                p => p.WithOrigins (
                    "http://localhost:4200",
                    "http://127.0.0.1:8887",
                    "https://notify-bot.line.me"
                ).AllowAnyMethod ().AllowAnyHeader ()));

            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseExceptionHandler ("/Home/Error");
                app.UseHsts ();
            }

            // app.UseHttpsRedirection ();
            app.UseStaticFiles ();
            app.UseCookiePolicy ();
            app.UseCors ("CorsDomain");
            app.UseMvc (routes => {
                routes.MapRoute (
                    name: "default",
                    template: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}