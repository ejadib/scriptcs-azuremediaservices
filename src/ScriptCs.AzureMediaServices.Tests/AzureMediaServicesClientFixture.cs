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
        public void WhenGetAssetsIsCalledThenAssetsCollectionIsFiltered()
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

                        FakesDelegates.Func<IQueryable<IAsset>> func = () => assets.Where(predicate);

                        return ShimsContext.ExecuteWithoutShims(func);
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
    }
}
