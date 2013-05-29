namespace ScriptCs.AzureMediaServices
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.WindowsAzure.MediaServices.Client;

    public class AzureMediaServicesClient
    {
        private readonly string accountName;
        private readonly string accountKey;

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

        public IQueryable<IAsset> GetAssets(Expression<Func<IAsset, bool>> predicate = null)
        {
            var context = new CloudMediaContext(this.accountName, this.accountKey);

            return predicate != null ? context.Assets.Where(predicate) : context.Assets;
        }

        public IAsset GetAsset(string assetId)
        {
            return this.GetAssets(assets => assets.Id == assetId).FirstOrDefault();
        }
    }
}
