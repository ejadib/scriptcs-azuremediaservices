namespace ScriptCs.AzureMediaServices
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.MediaServices.Client;

    public class AzureMediaServicesClient
    {
        private readonly string accountName;
        private readonly string accountKey;

        public AzureMediaServicesClient(string accountName, string accountKey)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentNullException("accountName");
            }

            if (string.IsNullOrWhiteSpace(accountKey))
            {
                throw new ArgumentNullException("accountKey");
            }

            this.accountName = accountName;
            this.accountKey = accountKey;
        }

        public IQueryable<IAsset> GetAssets(Expression<Func<IAsset, bool>> predicate = null)
        {
            var context = this.CreateContext();

            return predicate != null ? context.Assets.Where(predicate) : context.Assets;
        }

        public IAsset GetAsset(string assetId)
        {
            return this.GetAssets(asset => asset.Id == assetId).FirstOrDefault();
        }

        public void DeleteAsset(string assetId)
        {
            var asset = this.GetAsset(assetId);

            if (asset != null)
            {
                asset.Delete();
            }
        }

        public IQueryable<IMediaProcessor> GetMediaProcessors()
        {
            var context = this.CreateContext();

            return context.MediaProcessors;
        }

        public IQueryable<IJob> GetJobsByState(JobState state)
        {
            var context = this.CreateContext();

            return context.Jobs.Where(job => job.State == state);
        }

        public IJob GetJob(string jobId)
        {
            var context = this.CreateContext();

            return context.Jobs.Where(job => job.Id == jobId).FirstOrDefault();
        }

        public AzureMediaServicesUploader CreateUploader(string assetName, string filePath)
        {
            return new AzureMediaServicesUploader(assetName, filePath, this.CreateContext);
        }

        private CloudMediaContext CreateContext()
        {
            return new CloudMediaContext(this.accountName, this.accountKey);
        }
    }
}
