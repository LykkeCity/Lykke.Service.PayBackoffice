namespace Core.InternalTransfers
{
    public interface IInternalTransferRequest
    {
        string AccountFrom { get; }
        string AssetId { get;}
        string AccountTo { get; }
        double Amount { get; }
        string Comment { get; }
    }
}
