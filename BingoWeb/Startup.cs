using BingoWeb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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

            services.AddMemoryCache();//2021.12.4 APIコントローラでIMemoryCacheを使用する

            //2021.11.21 コントローラで使うappsettingsを用意しておく
            var webSettings = new WebSettings();
            webSettings.Cachekey = Configuration["Cachekey"];
            webSettings.CacheSpanSeconds = Configuration["CacheSpanSeconds"];
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
            if(!String.IsNullOrWhiteSpace(Configuration["DebugLog"]) && bool.Parse(Configuration["DebugLog"]))
            {
                webSettings.DebugLog = true;
            }
            else
            {
                webSettings.DebugLog = false;
            }
            webSettings.ApplicationName = Configuration["ApplicationName"];
            webSettings.ContainerDeletable = bool.Parse(Configuration["ContainerDeletable"]);
            services.AddSingleton(webSettings);

            var cosmosClient= new CosmosClient(webSettings.EndpointUri, webSettings.PrimaryKey, new CosmosClientOptions() { ApplicationName = webSettings.ApplicationName });
            services.AddSingleton(cosmosClient);

            var cosmosCall = new CosmosCall(webSettings,cosmosClient);
            services.AddSingleton(cosmosCall);

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
