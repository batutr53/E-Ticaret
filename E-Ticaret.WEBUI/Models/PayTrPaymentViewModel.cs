namespace E_Ticaret.WEBUI.Models;

public sealed class PayTrPaymentViewModel
{
    public string PaymentUrl { get; init; } = string.Empty;
    public string MerchantOid { get; init; } = string.Empty;
}

public sealed class PaymentResultViewModel
{
    public string MerchantOid { get; init; } = string.Empty;
    public bool IsSuccess { get; init; }
    public bool IsPending { get; init; }
    public string Message { get; init; } = string.Empty;
}
