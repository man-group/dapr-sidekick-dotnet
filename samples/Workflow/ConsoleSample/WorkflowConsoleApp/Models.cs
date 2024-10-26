#pragma warning disable SA1649 // File name should match first type name
namespace WorkflowConsoleApp
{
    public record OrderPayload(string Name, double TotalCost, int Quantity = 1);

    public record InventoryRequest(string RequestId, string ItemName, int Quantity);

    public record InventoryResult(bool Success, InventoryItem OrderPayload);

    public record PaymentRequest(string RequestId, string ItemName, int Amount, double Currency);

    public record OrderResult(bool Processed);

    public record InventoryItem(string Name, double PerItemCost, int Quantity);

    public enum ApprovalResult
    {
        Unspecified = 0,
        Approved = 1,
        Rejected = 2,
    }
}
#pragma warning restore SA1649 // File name should match first type name
