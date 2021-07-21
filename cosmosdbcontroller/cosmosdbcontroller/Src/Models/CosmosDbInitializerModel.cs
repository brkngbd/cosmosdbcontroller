namespace CosmosDbController
{
    using System.Threading.Tasks;

    /// <summary>
    ///   Used to verify if CosmosDb is ready and wait for its initialization
    /// </summary>
    public class CosmosDbInitializerModel
    {
        /// <summary>Gets the initialize task.</summary>
        /// <value>The initialize task.</value>
        public Task InitTask { get; private set; }

        /// <summary>Sets the task data.</summary>
        /// <param name="task">The task.</param>
        public void SetTaskData(Task task)
        {
            this.InitTask = task;
        }
    }
}
