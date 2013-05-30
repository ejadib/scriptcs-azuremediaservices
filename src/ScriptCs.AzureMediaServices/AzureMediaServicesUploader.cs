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

            var locator = await CreateSasLocatorAsync(context, this.asset);

            var fileName = Path.GetFileName(this.FilePath);

            var assetFile = await this.asset.AssetFiles.CreateAsync(fileName, CancellationToken.None);

            await assetFile.UploadAsync(this.FilePath, blobTransferClient, locator, CancellationToken.None);
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

        private void OnBlobTransferClientOnTransferCompleted(object sender, BlobTransferCompleteEventArgs args)
        {
            if (args.Error != null)
            {
                this.onError(args.Error);
                return;
            }

            this.onCompleted(this.asset.Id);
        }

        private void OnBlobTransferProgressChanged(object sender, BlobTransferProgressChangedEventArgs e)
        {
            this.onProgress(e.ProgressPercentage);
        }
    }
}