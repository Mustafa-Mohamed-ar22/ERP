public enum StockMovementType
{
    In,                  // e.g. purchase receipt
    Out,                 // e.g. sale shipment
    AdjustmentIncrease,  // manual correction, found extra stock
    AdjustmentDecrease,  // manual correction, damage/loss/shrinkage
    TransferOut,         // leaving the source warehouse — only created by TransferAsync, not directly requestable
    TransferIn           // arriving at the destination warehouse — same
}