namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Linq.Fakes;
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;
    using Microsoft.WindowsAzure.MediaServices.Client.Fakes;

    [TestClass]
    public class AzureMediaServicesClientFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAccountNameIsSetToNullThenAnExceptionIsThrown()
        {
            new AzureMediaServicesClient(null, "myAccountKey");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAccountNameIsSetToEmptyThenAnExceptionIsThrown()
        {
            new AzureMediaServicesClient(string.Empty, "myAccountKey");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAccountKeyIsSetToNullThenAnExceptionIsThrown()
        {
            new AzureMediaServicesClient("myAccountName", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAccountKeyIsSetToEmptyThenAnExceptionIsThrown()
        {
            new AzureMediaServicesClient("myAccountName", string.Empty);
        }

        [TestMethod]
        public void WhenGetAssetsIsCalledThenMediaServicesContextIsCreatedWithTheAccountProvided()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var client = new AzureMediaServicesClient(AccountName, AccountKey);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) =>
                {
                    providedAccountName = accountName;
                    providedAccountKey = accountKey;
                };

                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IAsset>((assets, predicate) => null);

                client.GetAssets(asset => asset.Name == "EZ");

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }

        [TestMethod]
        public void WhenGetAssetsIsCalledWithAPredicateThenAssetsCollectionIsFiltered()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                var asset1 = new StubIAsset { NameGet = () => "EZ" };
                var asset2 = new StubIAsset { NameGet = () => "VOD" };

                var sampleAssets = new List<IAsset> { asset1, asset2 };

                var collection = new StubAssetBaseCollection { QueryableGet = sampleAssets.AsQueryable };

                Expression<Func<IAsset, bool>> filter = asset => asset.Name == "EZ";

                Expression<Func<IAsset, bool>> providedPredicate = null;

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;
                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IAsset>((assets, predicate) =>
                    {
                        Assert.AreEqual(2, assets.Count());

                        providedPredicate = predicate;

                        return ShimsContext.ExecuteWithoutShims(() => assets.Where(predicate));
                    });

                var returnedAssets = client.GetAssets(filter);

                Assert.AreSame(filter, providedPredicate);
                Assert.AreEqual(1, returnedAssets.Count());
                Assert.AreEqual(asset1.NameGet(), returnedAssets.First().Name);
            }
        }

        [TestMethod]
        public void WhenGetAssetsIsCalledWithNoPredicateThenAllAssetsAreReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                var asset = new StubIAsset { NameGet = () => "EZ" };

                var assets = new List<IAsset> { asset };

                var collection = new StubAssetBaseCollection { QueryableGet = assets.AsQueryable };

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;

                var returnedAssets = client.GetAssets();

                Assert.AreEqual(1, returnedAssets.Count());
                Assert.AreSame(collection, returnedAssets);
            }
        }

        [TestMethod]
        public void WhenGetAssetIsCalledThenMediaServicesContextIsCreatedWithTheAccountProvided()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var client = new AzureMediaServicesClient(AccountName, AccountKey);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                var sampleAssets = new List<IAsset>().AsQueryable();

                var collection = new StubAssetBaseCollection { QueryableGet = () => sampleAssets };

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) =>
                {
                    providedAccountName = accountName;
                    providedAccountKey = accountKey;
                };

                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;

                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IAsset>((assets, predicate) => sampleAssets);

                client.GetAsset("nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c");

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }

        [TestMethod]
        public void WhenGetAssetIsCalledWithAnExistingIdThenAssetIsReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                const string AssetId = "nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c";
                var asset1 = new StubIAsset { IdGet = () => AssetId };
                var asset2 = new StubIAsset { IdGet = () => "myId" };

                var assets = new List<IAsset> { asset1, asset2 };

                var collection = new StubAssetBaseCollection { QueryableGet = assets.AsQueryable };

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;

                var returnedAsset = client.GetAsset(AssetId);

                Assert.AreSame(asset1, returnedAsset);
            }
        }

        [TestMethod]
        public void WhenGetAssetIsCalledWithANonExistingIdThenNullIsReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                const string AssetId = "nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c";
                var asset = new StubIAsset { IdGet = () => "myId" };

                var assets = new List<IAsset> { asset };

                var collection = new StubAssetBaseCollection { QueryableGet = assets.AsQueryable };

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;

                var returnedAsset = client.GetAsset(AssetId);

                Assert.IsNull(returnedAsset);
            }
        }

        [TestMethod]
        public void WhenGetMediaProcessorsIsCalledThenMediaServicesContextIsCreatedWithTheAccountProvided()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var client = new AzureMediaServicesClient(AccountName, AccountKey);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) =>
                {
                    providedAccountName = accountName;
                    providedAccountKey = accountKey;
                };

                client.GetMediaProcessors();

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }

        [TestMethod]
        public void WhenGetMediaProcessorsIsCalledThenAllMediaProcessorsAreReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                var mediaProcessor = new StubIMediaProcessor { NameGet = () => "My Media Processor" };

                var mediaProcessors = new List<IMediaProcessor> { mediaProcessor };

                var collection = new ShimMediaProcessorBaseCollection();

                collection.Bind(mediaProcessors.AsQueryable());

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.MediaProcessorsGet = context => collection;

                var returnedMediaProcesors = client.GetMediaProcessors();

                Assert.AreEqual(1, returnedMediaProcesors.Count());
                Assert.AreEqual(mediaProcessor.NameGet(), returnedMediaProcesors.First().Name);
            }
        }

        [TestMethod]
        public void WhenCreateUploaderIsCalledThenUploaderIsReturned()
        {
            const string AssetName = "MyVideo";
            const string Path = @"C:\videos\myvideo.mp4";

            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            var uploader = client.CreateUploader(AssetName, Path);

            Assert.AreEqual(AssetName, uploader.AssetName);
            Assert.AreEqual(Path, uploader.FilePath);
        }

        [TestMethod]
        public void WhenDeleteAssetIsCalledThenMediaServicesContextIsCreatedWithTheAccountProvided()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var client = new AzureMediaServicesClient(AccountName, AccountKey);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                var sampleAssets = new List<IAsset>().AsQueryable();

                var collection = new StubAssetBaseCollection { QueryableGet = () => sampleAssets };

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) =>
                {
                    providedAccountName = accountName;
                    providedAccountKey = accountKey;
                };

                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;

                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IAsset>((assets, predicate) => sampleAssets);
                client.DeleteAsset("nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c");

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }

        [TestMethod]
        public void WhenDeleteAssetIsCalledWithAnExistingIdThenDeleteIsCalledOnAsset()
        {
            bool deleteCalled = false;
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                const string AssetId = "nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c";
                var asset1 = new StubIAsset { IdGet = () => AssetId };
                var asset2 = new StubIAsset { IdGet = () => "myId" };

                asset1.Delete = () => { deleteCalled = true; };

                var assets = new List<IAsset> { asset1, asset2 };

                var collection = new StubAssetBaseCollection { QueryableGet = assets.AsQueryable };

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.AssetsGet = context => collection;

                client.DeleteAsset(AssetId);

                Assert.IsTrue(deleteCalled);
            }
        }

        [TestMethod]
        public void WhenGetJobsByStateIsCalledThenMediaServicesContextIsCreatedWithTheAccountProvided()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var client = new AzureMediaServicesClient(AccountName, AccountKey);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                var jobs = new List<IJob>().AsQueryable();

                var collection = new ShimJobBaseCollection();
                collection.Bind(jobs);

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) =>
                {
                    providedAccountName = accountName;
                    providedAccountKey = accountKey;
                };

                ShimCloudMediaContext.AllInstances.JobsGet = context => collection;

                client.GetJobsByState(JobState.Finished);

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }

        [TestMethod]
        public void WhenGetJobsByStateIsCalledThenJobsInTheProvidedStateAreReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                var job1 = new StubIJob() { StateGet = () => JobState.Finished };
                var job2 = new StubIJob() { StateGet = () => JobState.Canceled };

                var jobs = new List<IJob> { job1, job2 };

                var collection = new ShimJobBaseCollection();
                
                collection.Bind(jobs.AsQueryable());

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.JobsGet = context => collection;

                var returnedJobs = client.GetJobsByState(JobState.Finished);

                Assert.AreEqual(1, returnedJobs.Count());
            }
        }

        [TestMethod]
        public void WhenGetJobIsCalledThenMediaServicesContextIsCreatedWithTheAccountProvided()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var client = new AzureMediaServicesClient(AccountName, AccountKey);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                var sampleJobs = new List<IJob>().AsQueryable();

                var collection = new ShimJobBaseCollection();
                collection.Bind(sampleJobs);

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) =>
                {
                    providedAccountName = accountName;
                    providedAccountKey = accountKey;
                };

                ShimCloudMediaContext.AllInstances.JobsGet = context => collection;

                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IJob>((jobs, predicate) => sampleJobs);

                client.GetJob("nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c");

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }

        [TestMethod]
        public void WhenGetJobIsCalledWithAnExistingIdThenJobIsReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                const string JobId = "nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c";
                var job1 = new StubIJob() { IdGet = () => JobId };
                var job2 = new StubIJob() { IdGet = () => "myId" };

                var jobs = new List<IJob> { job1, job2 };

                var collection = new ShimJobBaseCollection();

                collection.Bind(jobs.AsQueryable());

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.JobsGet = context => collection;

                var returnedJob = client.GetJob(JobId);

                Assert.AreSame(job1, returnedJob);
            }
        }

        [TestMethod]
        public void WhenGetJobIsCalledWithANonExistingIdThenNullIsReturned()
        {
            var client = new AzureMediaServicesClient("myAccountName", "myAccountKey");

            using (ShimsContext.Create())
            {
                const string JobId = "nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c";
                var job = new StubIJob() { IdGet = () => "myId" };

                var jobs = new List<IJob> { job };

                var collection = new ShimJobBaseCollection();

                collection.Bind(jobs.AsQueryable());

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimCloudMediaContext.AllInstances.JobsGet = context => collection;

                var returnedJob = client.GetJob(JobId);

                Assert.IsNull(returnedJob);
            }
        }
    }
}
