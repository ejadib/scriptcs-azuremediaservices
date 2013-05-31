namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
    using System.IO.Fakes;
    using System.Net.WebSockets;
    using System.Threading.Tasks;

    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;
    using Microsoft.WindowsAzure.MediaServices.Client.Fakes;

    [TestClass]
    public class AzureMediaServicesUploaderFixture
    {
        [TestMethod]
        public async Task WhenStartIsCallednThenUploadAsyncOnAssetFileIsCalled()
        {
            bool uploadAsyncCalled = false;

            var stubAsset = new StubIAsset { NameGet = () => "test" };
            var stubAssetFile = new StubIAssetFile();
            var stubAccessPolicy = new StubIAccessPolicy();
            var stubLocator = new StubILocator();

            using (ShimsContext.Create())
            {
                var stubAssets = new StubAssetBaseCollection
                                     {
                                         CreateAsyncStringAssetCreationOptionsCancellationToken = 
                                             (name, options, cancellationToken) => Task.FromResult((IAsset)stubAsset)
                                     };

                var stubAssetsFiles = new StubAssetFileBaseCollection { CreateAsyncStringCancellationToken = (path, cancellationToken) => Task.FromResult((IAssetFile)stubAssetFile) };

                stubAsset.AssetFilesGet = () => stubAssetsFiles;

                var accessPolicies = new ShimAccessPolicyBaseCollection
                                                 {
                                                     CreateAsyncStringTimeSpanAccessPermissions
                                                         =
                                                         (name,
                                                          timesSpan,
                                                          accessPermissions) =>
                                                         Task.FromResult((IAccessPolicy)stubAccessPolicy)
                                                 };

                var locators = new ShimLocatorBaseCollection
                                   {
                                       CreateSasLocatorAsyncIAssetIAccessPolicy = 
                                           (asset, acccessPolicy) => Task.FromResult((ILocator)stubLocator)
                                   };

                ShimPath.GetFileNameString = fileName => string.Empty;

                ShimBlobTransferClient.Constructor = client => { };

                stubAssetFile.UploadAsyncStringBlobTransferClientILocatorCancellationToken =
                    (filePath, blobTransferClient, locator, cancellationToken) =>
                        {
                            uploadAsyncCalled = true;

                            return Task.Delay(0);
                        };

                var context = new ShimCloudMediaContext
                                  {
                                      AssetsGet = () => stubAssets, 
                                      AccessPoliciesGet = () => accessPolicies,
                                      LocatorsGet = () => locators,
                                  };

                Func<CloudMediaContext> createContext = () => context;

                var uploader = new AzureMediaServicesUploader("myVideo", @"C:\videos\myvideo.mp4", createContext);

                await uploader.Start();
            }

            Assert.IsTrue(uploadAsyncCalled);
        }

        [TestMethod]
        public void WhenBlobTransferProgressChangedEventIsRaisedThenProgressCallbackIsCalled()
        {
            const int Percentage = 10;
            var progressCalled = false;
            var providedPercentage = -1;
            EventHandler<BlobTransferProgressChangedEventArgs> blobTransferProgressHandler = null;
            var stubAsset = new StubIAsset { NameGet = () => "test" };
            var stubAssetFile = new StubIAssetFile();
            var stubAccessPolicy = new StubIAccessPolicy();
            var stubLocator = new StubILocator();

            using (ShimsContext.Create())
            {
                var stubAssets = new StubAssetBaseCollection
                {
                    CreateStringAssetCreationOptions =
                        (name, options) => stubAsset
                };

                var stubAssetsFiles = new StubAssetFileBaseCollection { CreateString = path => stubAssetFile };

                stubAsset.AssetFilesGet = () => stubAssetsFiles;

                var accessPolicies = new ShimAccessPolicyBaseCollection
                {
                    CreateStringTimeSpanAccessPermissions
                        =
                        (name,
                         timesSpan,
                         accessPermissions) =>
                        stubAccessPolicy
                };

                var locators = new ShimLocatorBaseCollection
                {
                    CreateSasLocatorIAssetIAccessPolicy =
                        (asset, acccessPolicy) => stubLocator
                };

                ShimPath.GetFileNameString = fileName => string.Empty;

                ShimBlobTransferClient.Constructor = client => { };

                stubAssetFile.UploadAsyncStringBlobTransferClientILocatorCancellationToken =
                    (filePath, blobTransferClient, locator, cancellationToken) => Task.Delay(0);

                var context = new ShimCloudMediaContext
                {
                    AssetsGet = () => stubAssets,
                    AccessPoliciesGet = () => accessPolicies,
                    LocatorsGet = () => locators,
                };

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
        public async Task WhenBlobTransferCompleteEventIsRaisedThenCompletedCallbackIsCalled()
        {
            string providedAssetId = null;
            EventHandler<BlobTransferCompleteEventArgs> blobTransferCompletedHandler = null;
            var stubAsset = new StubIAsset { IdGet = () => "myId", NameGet = () => "test" };
            var stubAssetFile = new StubIAssetFile();
            var stubAccessPolicy = new StubIAccessPolicy();
            var stubLocator = new StubILocator() { AccessPolicyGet = () => stubAccessPolicy };

            using (ShimsContext.Create())
            {
                var stubAssets = new StubAssetBaseCollection
                {
                    CreateAsyncStringAssetCreationOptionsCancellationToken = 
                        (name, options, cancellationToken) => Task.FromResult((IAsset)stubAsset)
                };

                var stubAssetsFiles = new StubAssetFileBaseCollection
                                          {
                                              CreateAsyncStringCancellationToken =
                                                  (path, cancellationToken) =>
                                                  Task.FromResult((IAssetFile)stubAssetFile)
                                          };

                stubAsset.AssetFilesGet = () => stubAssetsFiles;

                var accessPolicies = new ShimAccessPolicyBaseCollection
                {
                    CreateAsyncStringTimeSpanAccessPermissions
                        = 
                        (name,
                         timesSpan,
                         accessPermissions) =>
                        Task.FromResult((IAccessPolicy)stubAccessPolicy)
                };

                var locators = new ShimLocatorBaseCollection
                {
                    CreateSasLocatorAsyncIAssetIAccessPolicy = 
                        (asset, acccessPolicy) => Task.FromResult((ILocator)stubLocator)
                };

                ShimPath.GetFileNameString = fileName => string.Empty;

                ShimBlobTransferClient.Constructor = client => { };

                stubAssetFile.UploadAsyncStringBlobTransferClientILocatorCancellationToken =
                    (filePath, blobTransferClient, locator, cancellationToken) => Task.Delay(0);

                var context = new ShimCloudMediaContext
                {
                    AssetsGet = () => stubAssets,
                    AccessPoliciesGet = () => accessPolicies,
                    LocatorsGet = () => locators,
                };

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

                await uploader.Start();
            }

            var args = new BlobTransferCompleteEventArgs(null, true, null, @"C:\videos\myvideo.mp4", new Uri("http://myvideo"), BlobTransferType.Upload);

            blobTransferCompletedHandler.Invoke(null, args);

            Assert.AreEqual(stubAsset.IdGet(), providedAssetId);
        }

        [TestMethod]
        public async Task WhenBlobTransferCompleteEventIsRaisedAndAnErrorOcurredThenErrorCallbackIsCalled()
        {
            EventHandler<BlobTransferCompleteEventArgs> blobTransferCompletedHandler = null;
            var stubAsset = new StubIAsset { IdGet = () => "myId", NameGet = () => "test" };
            var stubAssetFile = new StubIAssetFile();
            var stubAccessPolicy = new StubIAccessPolicy();
            var stubLocator = new StubILocator() { AccessPolicyGet = () => stubAccessPolicy };
            Exception providedError = null;

            using (ShimsContext.Create())
            {
                var stubAssets = new StubAssetBaseCollection
                {
                    CreateAsyncStringAssetCreationOptionsCancellationToken = 
                        (name, options, cancellationToken) => Task.FromResult((IAsset)stubAsset)
                };

                var stubAssetsFiles = new StubAssetFileBaseCollection
                                          {
                                              CreateAsyncStringCancellationToken =
                                                  (path, cancellationToken) =>
                                                  Task.FromResult((IAssetFile)stubAssetFile)
                                          };

                stubAsset.AssetFilesGet = () => stubAssetsFiles;

                var accessPolicies = new ShimAccessPolicyBaseCollection
                {
                    CreateAsyncStringTimeSpanAccessPermissions
                        =
                        (name,
                         timesSpan,
                         accessPermissions) =>
                        Task.FromResult((IAccessPolicy)stubAccessPolicy)
                };

                var locators = new ShimLocatorBaseCollection
                {
                    CreateSasLocatorAsyncIAssetIAccessPolicy = 
                        (asset, acccessPolicy) => Task.FromResult((ILocator)stubLocator)
                };

                ShimPath.GetFileNameString = fileName => string.Empty;

                ShimBlobTransferClient.Constructor = client => { };

                stubAssetFile.UploadAsyncStringBlobTransferClientILocatorCancellationToken =
                    (filePath, blobTransferClient, locator, cancellationToken) => Task.Delay(0);

                var context = new ShimCloudMediaContext
                {
                    AssetsGet = () => stubAssets,
                    AccessPoliciesGet = () => accessPolicies,
                    LocatorsGet = () => locators,
                };

                Func<CloudMediaContext> createContext = () => context;

                ShimBlobTransferClient.AllInstances.TransferCompletedAddEventHandlerOfBlobTransferCompleteEventArgs = (client, handler) =>
                {
                    blobTransferCompletedHandler = handler;
                };

                var uploader = new AzureMediaServicesUploader("myVideo", @"C:\videos\myvideo.mp4", createContext);

                Action<Exception> onError = error =>
                {
                    providedError = error;
                };

                uploader.On(error: onError);

                await uploader.Start();
            }

            var sampleError = new WebSocketException(WebSocketError.ConnectionClosedPrematurely);
            var args = new BlobTransferCompleteEventArgs(sampleError, true, null, @"C:\videos\myvideo.mp4", new Uri("http://myvideo"), BlobTransferType.Upload);

            blobTransferCompletedHandler.Invoke(null, args);

            Assert.AreSame(sampleError, providedError);
        }

        [TestMethod]
        public async Task WhenBlobTransferCompleteEventIsRaisedThenAccessPolicyAndLocatorAreDeleted()
        {
            EventHandler<BlobTransferCompleteEventArgs> blobTransferCompletedHandler = null;
            var stubAsset = new StubIAsset { IdGet = () => "myId", NameGet = () => "test" };
            var stubAssetFile = new StubIAssetFile();
            var stubAccessPolicy = new StubIAccessPolicy();
            var stubLocator = new StubILocator() { AccessPolicyGet = () => stubAccessPolicy };
            bool accessPolicyDeleteCalled = false;
            bool locatorDeletedCalled = false;

            using (ShimsContext.Create())
            {
                var stubAssets = new StubAssetBaseCollection
                {
                    CreateAsyncStringAssetCreationOptionsCancellationToken =
                        (name, options, cancellationToken) => Task.FromResult((IAsset)stubAsset)
                };

                var stubAssetsFiles = new StubAssetFileBaseCollection
                {
                    CreateAsyncStringCancellationToken =
                        (path, cancellationToken) =>
                        Task.FromResult((IAssetFile)stubAssetFile)
                };

                stubAsset.AssetFilesGet = () => stubAssetsFiles;

                var accessPolicies = new ShimAccessPolicyBaseCollection
                {
                    CreateAsyncStringTimeSpanAccessPermissions
                        =
                        (name,
                         timesSpan,
                         accessPermissions) =>
                        Task.FromResult((IAccessPolicy)stubAccessPolicy)
                };

                stubAccessPolicy.Delete = () =>
                    {
                        accessPolicyDeleteCalled = true;
                    };

                var locators = new ShimLocatorBaseCollection
                {
                    CreateSasLocatorAsyncIAssetIAccessPolicy =
                        (asset, acccessPolicy) => Task.FromResult((ILocator)stubLocator),
                };

                stubLocator.Delete = () =>
                {
                    locatorDeletedCalled = true;
                };

                ShimPath.GetFileNameString = fileName => string.Empty;

                ShimBlobTransferClient.Constructor = client => { };

                stubAssetFile.UploadAsyncStringBlobTransferClientILocatorCancellationToken =
                    (filePath, blobTransferClient, locator, cancellationToken) => Task.Delay(0);

                var context = new ShimCloudMediaContext
                {
                    AssetsGet = () => stubAssets,
                    AccessPoliciesGet = () => accessPolicies,
                    LocatorsGet = () => locators,
                };

                Func<CloudMediaContext> createContext = () => context;

                ShimBlobTransferClient.AllInstances.TransferCompletedAddEventHandlerOfBlobTransferCompleteEventArgs = (client, handler) =>
                {
                    blobTransferCompletedHandler = handler;
                };

                var uploader = new AzureMediaServicesUploader("myVideo", @"C:\videos\myvideo.mp4", createContext);

                uploader.On();

                await uploader.Start();
            }

            var args = new BlobTransferCompleteEventArgs(null, true, null, @"C:\videos\myvideo.mp4", new Uri("http://myvideo"), BlobTransferType.Upload);

            blobTransferCompletedHandler.Invoke(null, args);

            Assert.IsTrue(locatorDeletedCalled);
            Assert.IsTrue(accessPolicyDeleteCalled);
        }

        // TODO: OnCancelled
    }
}
