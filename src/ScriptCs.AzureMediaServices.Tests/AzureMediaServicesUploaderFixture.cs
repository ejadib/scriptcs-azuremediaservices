namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
    using System.IO.Fakes;
    using System.Threading.Tasks.Fakes;

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
                var collection = new StubAssetBaseCollection
                                     {
                                         CreateStringAssetCreationOptions =
                                             (name, options) => null
                                     };

                var context = new ShimCloudMediaContext { AssetsGet = () => collection };
                Func<CloudMediaContext> createContext = () => context;
                
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
                var collection = new StubAssetBaseCollection
                                     {
                                         CreateStringAssetCreationOptions =
                                             (name, options) => asset
                                     };
                var context = new ShimCloudMediaContext { AssetsGet = () => collection };
                
                Func<CloudMediaContext> createContext = () => context;

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

        [TestMethod]
        public void WhenStartIsCallednThenUploadProcessBegins()
        {
            bool uploadTaskStarted = false;

            var stubAsset = new StubIAsset { NameGet = () => "test" };
            var stubAssetFile = new StubIAssetFile();
            var stubAccessPolicy = new StubIAccessPolicy();
            var stubLocator = new StubILocator();
            var stubUploadTask = new StubTask(() => uploadTaskStarted = true);

            using (ShimsContext.Create())
            {
                var stubAssets = new StubAssetBaseCollection
                                     {
                                         CreateStringAssetCreationOptions =
                                             (name, options) => stubAsset
                                     };

                var stubAsetsFiles = new StubAssetFileBaseCollection() { CreateString = path => stubAssetFile };

                var accessPolicies = new ShimAccessPolicyBaseCollection
                                                 {
                                                     CreateStringTimeSpanAccessPermissions
                                                         =
                                                         (name,
                                                          timesSpan,
                                                          accessPermissions) =>
                                                         stubAccessPolicy
                                                 };

                var locators = new ShimLocatorBaseCollection()
                                   {
                                       CreateSasLocatorIAssetIAccessPolicy =
                                           (asset, acccessPolicy) => stubLocator
                                   };

                ShimPath.GetFileNameString = fileName => string.Empty;

                ShimBlobTransferClient.Constructor = client => { };

                stubAssetFile.UploadAsyncStringBlobTransferClientILocatorCancellationToken =
                    (filePath, blobTransferClient, locator, cancellationToken) => stubUploadTask;

                var context = new ShimCloudMediaContext
                                  {
                                      AssetsGet = () => stubAssets, 
                                      FilesGet = () => stubAsetsFiles,
                                      AccessPoliciesGet = () => accessPolicies,
                                      LocatorsGet = () => locators,
                                  };

                Func<CloudMediaContext> createContext = () => context;

                var uploader = new AzureMediaServicesUploader("myVideo", @"C:\videos\myvideo.mp4", createContext);

                uploader.Start();
            }

            Assert.IsTrue(uploadTaskStarted);
        }
        // TODO: OnError
        // TODO: OnCancelled
    }
}
