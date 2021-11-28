using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BindoWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //2021.11.21 �R���g���[���Ŏg��appsettings��p�ӂ��Ă���
            var webSettings = new WebSettings();
            webSettings.EndpointUri = Configuration["EndPointUri"];
            webSettings.PrimaryKey = Configuration["PrimaryKey"];
            webSettings.DatabaseId = Configuration["DatabaseId"];
            webSettings.ContainerId = Configuration["ContainerId"];
            webSettings.MaxBingoCard = int.Parse(Configuration["MaxBingoCard"]);
            if (String.IsNullOrWhiteSpace(Configuration["RandomSeed"]))
            {
                webSettings.RandomSeed = null;
            }
            else
            {
                webSettings.RandomSeed = int.Parse(Configuration["RandomSeed"]);
            }
            services.AddSingleton(webSettings);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
