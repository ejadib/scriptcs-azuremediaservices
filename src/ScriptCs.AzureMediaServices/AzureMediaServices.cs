namespace ScriptCs.AzureMediaServices
{
    using ScriptCs.Contracts;

    public class AzureMediaServices : IScriptPackContext
    {
        public AzureMediaServicesClient CreateClient(string accountName, string accountKey)
        {
            return new AzureMediaServicesClient(accountName, accountKey);
        }
    }
}
