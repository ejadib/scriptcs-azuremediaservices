namespace ScriptCs.AzureMediaServices.Tests
{
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ScriptCs.AzureMediaServices.Fakes;

    [TestClass]
    public class AzureMediaServicesFixture
    {
        [TestMethod]
        public void WhenCreateClientIsCalledThenMediaServicesClientIsReturned()
        {
            const string AccountName = "myAccount";
            const string AccountKey = "myKey";

            var mediaServices = new AzureMediaServices();

            Assert.IsNotNull(mediaServices);

            using (ShimsContext.Create())
            {
                string providedAccountName = null;
                string providedAccountKey = null;

                ShimAzureMediaServicesClient.ConstructorStringString = (client, accountName, accountKey) =>
                    {
                        providedAccountName = accountName;
                        providedAccountKey = accountKey;
                    };

                mediaServices.CreateClient(AccountName, AccountKey);

                Assert.AreEqual(AccountName, providedAccountName);
                Assert.AreEqual(AccountKey, providedAccountKey);
            }
        }
    }
}