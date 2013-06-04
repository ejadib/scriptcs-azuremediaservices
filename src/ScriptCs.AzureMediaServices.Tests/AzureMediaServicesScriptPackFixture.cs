namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
    using System.Collections.Generic;

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
        public void WhenInitializeIsCalledThenScriptCsAzureMediaServicesAndWindowsMediaServicesClientNamespacesAreImported()
        {
            var importedNamespace = new List<string>();

            var session = new StubIScriptPackSession
            {
                ImportNamespaceString = ns => importedNamespace.Add(ns)
            };

            var mediaServicesScriptPack = new AzureMediaServicesScriptPack();

            mediaServicesScriptPack.Initialize(session);

            Assert.IsTrue(importedNamespace.Contains("ScriptCs.AzureMediaServices"));
            Assert.IsTrue(importedNamespace.Contains("Microsoft.WindowsAzure.MediaServices.Client"));
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
