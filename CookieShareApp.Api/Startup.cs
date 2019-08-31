using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieShareApp.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private DirectoryInfo GetKyRingDirectoryInfo()
        {
            string applicationBasePath = System.AppContext.BaseDirectory;
            DirectoryInfo directoryInof = new DirectoryInfo(applicationBasePath);
            string keyRingPath = Configuration.GetSection("AppKeys").GetValue<string>("keyRingPath");
            do
            {
                directoryInof = directoryInof.Parent;

                DirectoryInfo keyRingDirectoryInfo = new DirectoryInfo($"{directoryInof.FullName}{keyRingPath}");
                if (keyRingDirectoryInfo.Exists)
                {
                    return keyRingDirectoryInfo;
                }

            }
            while (directoryInof.Parent != null);
            throw new Exception($"key ring path not foun");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
            .PersistKeysToFileSystem(GetKyRingDirectoryInfo())
           .SetApplicationName("SharedCookieApp");

            services.Configure<CookiePolicyOptions>(options =>
            {

                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication("Identity.Application")
                .AddCookie("Identity.Application", option => {
                    
                    option.Cookie.Name = ".AspNet.SharedCookie";
                });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
