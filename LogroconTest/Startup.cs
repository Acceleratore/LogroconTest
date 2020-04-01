using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using LogroconTest.Helpers;

namespace LogroconTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private static string GetXmlCommentsPath()
        {
            return string.Format(@"{0}\LogroconTest.XML", AppDomain.CurrentDomain.BaseDirectory);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<Settings>(option =>
            {
                var mainBD = new DBSettings();
                Configuration.GetSection("MainDBSetting").Bind(mainBD);
                option.MainDBConnection = mainBD;

                option.ChachedEmployee = Configuration.GetSection("Chached").GetValue<bool>("Employee");
                option.ChachedPosts    = Configuration.GetSection("Chached").GetValue<bool>("Post");

            });

            services.AddSingleton<ICacheStore>(new CacheStore(Configuration.GetSection("Chached").GetValue<bool>("Employee"), Configuration.GetSection("Chached").GetValue<bool>("Post")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version     = "v1",
                    Title       = "Logrocon API",
                    Description = "ASP.NET Core Web API"
                });

                try
                {
                    c.IncludeXmlComments(GetXmlCommentsPath());
                }
                catch
                {

                }
            });
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
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logrocon API v1");
            });
        }
    }
}
