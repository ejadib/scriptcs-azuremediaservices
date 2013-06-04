namespace ScriptCs.AzureMediaServices
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.MediaServices.Client;

    public class AzureMediaServicesUploader
    {
        private readonly Func<CloudMediaContext> createContext;

        private IAsset asset;

        private Action<int> onProgress;

        private Action<string> onCompleted;

        private Action<Exception> onError;

        private ILocator locator;

        public AzureMediaServicesUploader(string assetName, string filePath, Func<CloudMediaContext> createContext)
        {
            this.AssetName = assetName;
            this.FilePath = filePath;
            this.createContext = createContext;
        }

        public string AssetName { get; private set; }

        public string FilePath { get; private set; }

        public void On(Action<int> progress = null, Action<string> completed = null, Action<Exception> error = null)
        {
            this.onProgress = progress;
            this.onCompleted = completed;
            this.onError = error;
        }

        public async Task Start()
        {
            var context = this.createContext.Invoke();

            var blobTransferClient = new BlobTransferClient();
            blobTransferClient.TransferProgressChanged += this.OnBlobTransferProgressChanged;
            blobTransferClient.TransferCompleted += this.OnBlobTransferClientOnTransferCompleted;

            this.asset = await CreateEmptyAsset(context, this.AssetName, AssetCreationOptions.None);

            this.locator = await CreateSasLocatorAsync(context, this.asset);

            var fileName = Path.GetFileName(this.FilePath);

            var assetFile = await this.asset.AssetFiles.CreateAsync(fileName, CancellationToken.None);

            await assetFile.UploadAsync(this.FilePath, blobTransferClient, this.locator, CancellationToken.None);
        }

        private static Task<IAsset> CreateEmptyAsset(MediaContextBase context, string assetName, AssetCreationOptions assetCreationOptions)
        {
            return context.Assets.CreateAsync(assetName, assetCreationOptions, CancellationToken.None);
        }

        private static async Task<ILocator> CreateSasLocatorAsync(CloudMediaContext context, IAsset asset)
        {
            var accessPolicy = await context.AccessPolicies.CreateAsync(
                asset.Name, TimeSpan.FromDays(2), AccessPermissions.Write | AccessPermissions.List);

            return await context.Locators.CreateSasLocatorAsync(asset, accessPolicy);
        }

        private void DeleteAccessPolicyAndLocator()
        {
            if (this.locator != null)
            {
                var accessPolicy = this.locator.AccessPolicy;

                this.locator.Delete();
                this.locator = null;
                accessPolicy.Delete();
            }
        }

        private void OnBlobTransferClientOnTransferCompleted(object sender, BlobTransferCompleteEventArgs args)
        {
            this.DeleteAccessPolicyAndLocator();

            if (args.Error != null && this.onError != null)
            {
                this.onError.Invoke(args.Error);
                return;
            }

            if (this.onCompleted != null)
            {
                this.onCompleted.Invoke(this.asset.Id);
            }
        }

        private void OnBlobTransferProgressChanged(object sender, BlobTransferProgressChangedEventArgs e)
        {
            if (this.onProgress != null)
            {
                this.onProgress.Invoke(e.ProgressPercentage);
            }
        }
    }
}