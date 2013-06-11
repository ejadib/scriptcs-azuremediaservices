namespace ScriptCs.AzureMediaServices
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.MediaServices.Client;

    using ScriptCs.AzureMediaServices.Properties;

    public class AzureMediaServicesEncoder
    {
        private readonly Func<CloudMediaContext> createContext;

        private Action<string> onFinished;

        private Action<string> onProcessing;

        private Action<string> onError;

        public AzureMediaServicesEncoder(string jobName, IAsset inputAsset, string configuration, string outputAssetName, Func<CloudMediaContext> createContext)
        {
            this.JobName = jobName;
            this.InputAsset = inputAsset;
            this.Configuration = configuration;
            this.OutputAssetName = outputAssetName;
            this.createContext = createContext;
        }

        public string JobName { get; private set; }

        public IAsset InputAsset { get; private set; }

        public string OutputAssetName { get; private set; }

        public string Configuration { get; private set; }

        public void On(Action<string> processing = null, Action<string> finished = null, Action<string> error = null)
        {
            this.onProcessing = processing;
            this.onFinished = finished;
            this.onError = error;
        }

        public async Task Start()
        {
            var context = this.createContext.Invoke();

            var job = context.Jobs.Create(this.JobName);

            var mediaProcessor = GetLatestMediaProcessorByName(context, Resources.MediaServicesEncoderMediaProcessor);

            var task = job.Tasks.AddNew(
                "Encoding task", mediaProcessor, this.Configuration, TaskOptions.ProtectedConfiguration);

            task.InputAssets.Add(this.InputAsset);

            task.OutputAssets.AddNew(this.OutputAssetName, AssetCreationOptions.None);

            job.StateChanged += this.OnJobStateChanged;

            await job.SubmitAsync();

            await job.GetExecutionProgressTask(CancellationToken.None);
        }

        private static IMediaProcessor GetLatestMediaProcessorByName(CloudMediaContext context, string mediaProcessorName)
        {
            // The possible strings that can be passed into the 
            // method for the mediaProcessor parameter:
            //   Windows Azure Media Encoder
            //   Windows Azure Media Packager
            //   Windows Azure Media Encryptor
            //   Storage Decryption
            var processor = context.MediaProcessors.Where(p => p.Name == mediaProcessorName)
                .ToList()
                .OrderBy(p => new Version(p.Version)).LastOrDefault();

            if (processor == null)
            {
                throw new ArgumentException(string.Format("Unknown media processor {0}", mediaProcessorName));
            }

            return processor;
        }

        private static string GetErrorMessage(IJob job)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("\nThe job stopped due an error.");
            builder.AppendLine("***************************");
            builder.AppendLine("Job ID: " + job.Id);
            builder.AppendLine("Job Name: " + job.Name);
            builder.AppendLine("Job State: " + job.State.ToString());
            builder.AppendLine("Job started (server UTC time): " + job.StartTime.ToString());
            builder.Append("Error Details: \n");

            foreach (ITask task in job.Tasks)
            {
                foreach (ErrorDetail detail in task.ErrorDetails)
                {
                    builder.AppendLine("  Task Id: " + task.Id);
                    builder.AppendLine("    Error Code: " + detail.Code);
                    builder.AppendLine("    Error Message: " + detail.Message + "\n");
                }
            }

            builder.AppendLine("***************************\n");

            return builder.ToString();
        }

        private void OnJobStateChanged(object sender, JobStateChangedEventArgs e)
        {
            switch (e.CurrentState)
            {
                case JobState.Finished:
                    if (this.onFinished != null)
                    {
                        var job = (IJob)sender;
                        this.onFinished.Invoke(job.Id);
                    }

                    break;
                case JobState.Canceling:
                case JobState.Queued:
                case JobState.Scheduled:
                    break;
                case JobState.Processing:
                    if (this.onProcessing != null)
                    {
                        var job = (IJob)sender;
                        this.onProcessing.Invoke(job.Id);   
                    }

                    break;
                case JobState.Canceled:
                    break;
                case JobState.Error:
                    if (this.onError != null)
                    {
                        var job = (IJob)sender;
                        string errorMessage = GetErrorMessage(job);

                        this.onError.Invoke(errorMessage);  
                    }

                    break;
            }
        }
    }
}