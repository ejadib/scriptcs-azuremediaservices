namespace ScriptCs.AzureMediaServices
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.MediaServices.Client;

    public class AzureMediaServicesClient
    {
        private string accountName;
        private string accountKey;

        public AzureMediaServicesClient(string accountName, string accountKey)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentNullException("accountName");
            }

            if (string.IsNullOrWhiteSpace(accountKey))
            {
                throw new ArgumentNullException("accountKey");
            }

            this.accountName = accountName;
            this.accountKey = accountKey;
        }

        public IQueryable<IAsset> GetAssets(Expression<Func<IAsset, bool>> predicate)
        {
            CloudMediaContext context = new CloudMediaContext(this.accountName, this.accountKey);
        
            return context.Assets.Where(predicate);
        }
    }
}
