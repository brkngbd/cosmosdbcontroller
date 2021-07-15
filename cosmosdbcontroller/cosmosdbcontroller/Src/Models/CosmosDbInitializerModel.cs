namespace CosmosDbController
{
    using System.Threading;
    using System.Threading.Tasks;

    public class CosmosDbInitializerModel
    {
        public Task InitTask { get; private set; }
        public CancellationToken StoppingToken { get; private set; }

        public void SetTaskData(Task task, CancellationToken stoppingToken)
        {
            this.InitTask = task;
            this.StoppingToken = stoppingToken;
        }
    }
}
