namespace ScriptCs.AzureMediaServices.Tests
{
    using System;

    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;
    using Microsoft.WindowsAzure.MediaServices.Client.Fakes;

    [TestClass]
    public class AzureMediaServicesUploaderFixture
    {
        [TestMethod]
        public void WhenBlobTransferProgressChangedEventIsRaisedThenProgressCallbackIsCalled()
        {
            const int Percentage = 10;
            var progressCalled = false;
            var providedPercentage = -1;
            EventHandler<BlobTransferProgressChangedEventArgs> blobTransferProgressHandler = null;

            using (ShimsContext.Create())
            {
                var context = new ShimCloudMediaContext();
                Func<CloudMediaContext> createContext = () => context;

                var collection = new StubAssetBaseCollection();

                context.AssetsGet = () => collection;

                collection.CreateStringAssetCreationOptions = (name, options) => null;

                ShimBlobTransferClient.AllInstances.TransferProgressChangedAddEventHandlerOfBlobTransferProgressChangedEventArgs = (client, handler) =>
                        {
                            blobTransferProgressHandler = handler;
                        };

                var uploader = new AzureMediaServicesUploader("myVideo", @"C:\videos\myvideo.mp4", createContext);

                Action<int> onProgress = progressPercentage =>
                {
                    progressCalled = true;
                    providedPercentage = progressPercentage;
                };

                uploader.On(progress: onProgress);

                uploader.Start();
            }

            var args = new BlobTransferProgressChangedEventArgs(0, 0, 0, Percentage, 0, new Uri("http://myvideo"), @"C:\videos\myvideo.mp4", null);

            blobTransferProgressHandler.Invoke(null, args);

            Assert.IsTrue(progressCalled);
            Assert.AreEqual(Percentage, providedPercentage);
        }

        [TestMethod]
        public void WhenBlobTransferCompleteEventIsRaisedThenCompletedCallbackIsCalled()
        {
            string providedAssetId = null;

            EventHandler<BlobTransferCompleteEventArgs> blobTransferCompletedHandler = null;

            var asset = new StubIAsset() { IdGet = () => "myId" };

            using (ShimsContext.Create())
            {
                var context = new ShimCloudMediaContext();
                Func<CloudMediaContext> createContext = () => context;

                var collection = new StubAssetBaseCollection();

                context.AssetsGet = () => collection;

                collection.CreateStringAssetCreationOptions = (name, options) => asset;

                ShimBlobTransferClient.AllInstances.TransferCompletedAddEventHandlerOfBlobTransferCompleteEventArgs = (client, handler) =>
                        {
                            blobTransferCompletedHandler = handler;
                        };

                var uploader = new AzureMediaServicesUploader("myVideo", @"C:\videos\myvideo.mp4", createContext);

                Action<string> onCompleted = assetId =>
                {
                    providedAssetId = assetId;
                };

                uploader.On(completed: onCompleted);

                uploader.Start();
            }

            var args = new BlobTransferCompleteEventArgs(null, true, null, @"C:\videos\myvideo.mp4", new Uri("http://myvideo"), BlobTransferType.Upload);

            blobTransferCompletedHandler.Invoke(null, args);

            Assert.AreEqual(asset.IdGet(), providedAssetId);
        }

        // TODO: OnError
        // TODO: OnCancelled
    }
}
