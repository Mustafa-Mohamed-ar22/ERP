public record OpenShiftRequest(Guid WarehouseId, decimal OpeningCashBalance, string? Notes);
public record CashMovementRequest(string MovementType, decimal Amount, string Reason);
public record CloseShiftRequest(decimal CountedClosingCash, string? Notes);

public record CashierShiftResponse(
    Guid Id, string ShiftNumber, Guid WarehouseId, string WarehouseName, Guid CashierUserId,
    DateTime OpenedAt, DateTime? ClosedAt, decimal OpeningCashBalance,
    decimal? ExpectedClosingCash, decimal? CountedClosingCash, decimal? DiscrepancyAmount, string Status);

public record ShiftClosingReportResponse(
    Guid ShiftId, string ShiftNumber, DateTime OpenedAt, DateTime? ClosedAt,
    decimal OpeningCashBalance, int CompletedOrderCount, int VoidedOrderCount,
    decimal TotalCashSales, decimal TotalCardSales, decimal TotalOtherSales,
    decimal TotalCashIn, decimal TotalCashOut, decimal ExpectedClosingCash,
    decimal? CountedClosingCash, decimal? DiscrepancyAmount);

public record CashierOrderLineRequest(Guid ProductId, decimal Quantity, decimal UnitPrice, decimal DiscountAmount);
public record CashierPaymentRequest(string Method, decimal Amount, string? ReferenceNumber);
public record CreateCashierOrderRequest(
    Guid? CustomerId, string? WalkInCustomerName, string? WalkInCustomerPhone, decimal DiscountAmount, decimal TaxAmount,
    List<CashierOrderLineRequest> Lines, List<CashierPaymentRequest> Payments);

public record VoidCashierOrderRequest(string Reason);

public record CashierOrderLineResponse(Guid Id, Guid ProductId, string ProductSku, string ProductName, decimal Quantity, decimal UnitPrice, decimal DiscountAmount, decimal LineTotal);
public record CashierPaymentResponse(Guid Id, string Method, decimal Amount, string? ReferenceNumber);
public record CashierOrderResponse(
    Guid Id, string OrderNumber, Guid CashierShiftId, Guid WarehouseId, Guid? CustomerId, string? WalkInCustomerName, string? WalkInCustomerPhone,
    DateTime OrderDate, string Status, decimal SubTotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount, decimal ChangeDue,
    Guid? InvoiceId, string? InvoiceNumber, List<CashierOrderLineResponse> Lines, List<CashierPaymentResponse> Payments, List<string> Warnings);
public record CashierInvoiceLineResponse(Guid ProductId, string ProductName, string Sku, decimal Quantity, decimal UnitPrice, decimal DiscountAmount, decimal LineTotal);
public record CashierInvoiceResponse(
    Guid Id, string InvoiceNumber, Guid CashierOrderId, Guid? CustomerId, string? WalkInCustomerName, string? WalkInCustomerPhone,
    DateTime InvoiceDate, string Status, decimal SubTotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount,
    decimal AmountPaid, decimal ChangeDue, List<CashierInvoiceLineResponse> Lines);