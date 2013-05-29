namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
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

                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IAsset>((assets, predicate) =>
                {
                    return null;
                });

                client.GetAssets((asset) => asset.Name == "EZ");

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
                Expression<Func<IAsset, bool>> filter = (asset) => asset.Name == "EZ";

                Expression<Func<IAsset, bool>> providedPredicate = null;

                ShimCloudMediaContext.ConstructorStringString = (context, accountName, accountKey) => { };
                ShimQueryable.WhereOf1IQueryableOfM0ExpressionOfFuncOfM0Boolean<IAsset>((assets, predicate) =>
                    {
                        providedPredicate = predicate;
                        return null;
                    });

                client.GetAssets(filter);

                Assert.AreSame(filter, providedPredicate);
            }
        }
    }
}
