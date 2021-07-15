namespace CosmosDbController
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public class CosmosDbBackgroundService : BackgroundService
    {
        private readonly CosmosDbInitializer cosmosDbInitializer;
        private readonly CosmosDbInitializerModel cosmosDbInitializerModel;
        
        public CosmosDbBackgroundService(CosmosDbInitializer cosmosDbInitializer, CosmosDbInitializerModel cosmosDbInitializerModel)
        {
            this.cosmosDbInitializer = cosmosDbInitializer;
            this.cosmosDbInitializerModel = cosmosDbInitializerModel;
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService">IHostedService</see> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">
        /// Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)">StopAsync(System.Threading.CancellationToken)</see> is called.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents the long running operations.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.cosmosDbInitializerModel.SetTaskData(cosmosDbInitializer.Initialize(), stoppingToken);
            return this.cosmosDbInitializerModel.InitTask;
        }
    }
}
