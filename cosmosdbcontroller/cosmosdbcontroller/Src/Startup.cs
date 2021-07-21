namespace CosmosDbController
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

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
            services.Configure<HostOptions>(
                o => o.ShutdownTimeout =
                TimeSpan.FromSeconds(int.Parse(this.Configuration["BackgroundserviceShutdownTimeout"])));
            services.AddControllers();
            services.Configure<CosmosDbOptions>(Configuration.GetSection(CosmosDbOptions.CosmosDb));
            services.AddSingleton(
                serviceProvider =>
                {
                    var cosmosDbOptions = serviceProvider.GetService<IOptions<CosmosDbOptions>>().Value;
                    return new CosmosClient(cosmosDbOptions.EndpointUri, cosmosDbOptions.PrimaryKey,
                        new CosmosClientOptions()
                        {
                            ApplicationName = "SimpleCosmosDbCRUDController"
                        });
                });
            services.AddSingleton<CosmosDbInitializer>();
            services.AddSingleton<CosmosDbInitializerModel>();
            services.AddSingleton<CosmosDbBackgroundService>();
            services.AddHostedService(
                serviceProvider => serviceProvider.GetService<CosmosDbBackgroundService>());
            services.AddSingleton<CosmosDbrepository>();
            services.AddMemoryCache(options =>
            {
                options.CompactionPercentage = 0.2;
                options.SizeLimit = int.Parse(this.Configuration["CacheSizeLimit"]);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
