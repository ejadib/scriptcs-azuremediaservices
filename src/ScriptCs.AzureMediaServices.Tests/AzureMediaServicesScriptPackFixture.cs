namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ScriptCs.Contracts.Fakes;

    [TestClass]
    public class AzureMediaServicesScriptPackFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenInitializeIsCalledAndSessionIsNotProvidedThenAnExceptionIsThrown()
        {
            var mediaServicesScriptPack = new AzureMediaServicesScriptPack();

            mediaServicesScriptPack.Initialize(null);
        }

        [TestMethod]
        public void WhenInitializeIsCalledThenScriptCsAzureMediaServicesNamespaceIsImported()
        {
            string importedNamespace = null;

            StubIScriptPackSession session = new StubIScriptPackSession()
                {
                    ImportNamespaceString = (ns) => importedNamespace = ns
                };

            var mediaServicesScriptPack = new AzureMediaServicesScriptPack();

            mediaServicesScriptPack.Initialize(session);

            Assert.AreEqual("ScriptCs.AzureMediaServices", importedNamespace);
        }

        [TestMethod]
        public void WhenGetContextIsCalledThenScriptCsPackContextIsReturned()
        {
            var mediaServicesScriptPack = new AzureMediaServicesScriptPack();

            var context = mediaServicesScriptPack.GetContext();

            Assert.IsNotNull(context);
            Assert.IsInstanceOfType(context, typeof(AzureMediaServices));
        }
    }
}
