using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace cosmosdbcontroller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitializeDataSource();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void InitializeDataSource()
        {
            // todo: created database and container if they don't exist
        }
    }
}
