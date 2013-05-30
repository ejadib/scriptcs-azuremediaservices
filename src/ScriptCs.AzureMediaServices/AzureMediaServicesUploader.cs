namespace ScriptCs.AzureMediaServices
{
    using System;
    using System.IO;
    using System.Threading;

    using Microsoft.WindowsAzure.MediaServices.Client;
    using System.Threading.Tasks;

    public class AzureMediaServicesUploader
    {
        private readonly Func<CloudMediaContext> createContext;

        private IAsset asset;

        private Action<int> onProgress;

        private Action<string> onCompleted;

        public AzureMediaServicesUploader(string assetName, string filePath, Func<CloudMediaContext> createContext)
        {
            this.AssetName = assetName;
            this.FilePath = filePath;
            this.createContext = createContext;
        }

        public string AssetName { get; private set; }

        public string FilePath { get; private set; }

        public void On(Action<int> progress = null, Action<string> completed = null)
        {
            this.onProgress = progress;
            this.onCompleted = completed;
        }

        public Task Start()
        {
            var context = this.createContext();

            var blobTransferClient = new BlobTransferClient();
            blobTransferClient.TransferProgressChanged += this.OnBlobTransferProgressChanged;
            blobTransferClient.TransferCompleted += this.OnBlobTransferClientOnTransferCompleted;

            this.asset = CreateEmptyAsset(context, this.AssetName, AssetCreationOptions.None);

            var locator = CreateSasLocator(context, this.asset);

            var fileName = Path.GetFileName(this.FilePath);

            var assetFile = this.asset.AssetFiles.Create(fileName);

            return assetFile.UploadAsync(this.FilePath, blobTransferClient, locator, CancellationToken.None);
        }

        private static IAsset CreateEmptyAsset(MediaContextBase context, string assetName, AssetCreationOptions assetCreationOptions)
        {
            return context.Assets.Create(assetName, assetCreationOptions);
        }

        private static ILocator CreateSasLocator(CloudMediaContext context, IAsset asset)
        {
            var accessPolicy = context.AccessPolicies.Create(
                asset.Name, TimeSpan.FromDays(2), AccessPermissions.Write | AccessPermissions.List);

            return context.Locators.CreateSasLocator(asset, accessPolicy);
        }

        private void OnBlobTransferClientOnTransferCompleted(object sender, BlobTransferCompleteEventArgs args)
        {
            this.onCompleted(this.asset.Id);
        }

        private void OnBlobTransferProgressChanged(object sender, BlobTransferProgressChangedEventArgs e)
        {
            this.onProgress(e.ProgressPercentage);
        }
    }
}