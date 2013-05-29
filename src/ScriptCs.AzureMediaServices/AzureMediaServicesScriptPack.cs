namespace ScriptCs.AzureMediaServices
{
    using System;
    using ScriptCs.Contracts;

    public class AzureMediaServicesScriptPack : IScriptPack
    {
        public IScriptPackContext GetContext()
        {
            return new AzureMediaServices();
        }

        public void Initialize(IScriptPackSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            session.ImportNamespace("ScriptCs.AzureMediaServices");
        }

        public void Terminate()
        {
        }
    }
}
