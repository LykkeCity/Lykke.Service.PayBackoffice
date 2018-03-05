using Lykke.Service.Assets.Client.Models;

namespace Core.Assets
{
    public static class AssetsExtensions
    {
        public static int GetDisplayAccuracy(this Asset asset)
        {
            return asset.DisplayAccuracy ?? asset.Accuracy;
        }
    }
}
