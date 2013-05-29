﻿namespace ScriptCs.AzureMediaServices
{
    using System;

    using Microsoft.WindowsAzure.MediaServices.Client;

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

        public void Start()
        {
            var context = this.createContext();

            this.asset = CreateEmptyAsset(context, this.AssetName, AssetCreationOptions.None);

            var blobTransferClient = new BlobTransferClient();
            blobTransferClient.TransferProgressChanged += this.OnBlobTransferProgressChanged;
            blobTransferClient.TransferCompleted += this.OnBlobTransferClientOnTransferCompleted;
        }

        private static IAsset CreateEmptyAsset(CloudMediaContext context, string assetName, AssetCreationOptions assetCreationOptions)
        {
            return context.Assets.Create(assetName, assetCreationOptions);
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