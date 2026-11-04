using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Payments;

public sealed class PayTrCallbackModel
{
    [FromForm(Name = "merchant_oid")]
    public string MerchantOid { get; set; } = string.Empty;

    [FromForm(Name = "status")]
    public string Status { get; set; } = string.Empty;

    [FromForm(Name = "total_amount")]
    public string TotalAmount { get; set; } = string.Empty;

    [FromForm(Name = "hash")]
    public string Hash { get; set; } = string.Empty;

    [FromForm(Name = "failed_reason_code")]
    public string? FailedReasonCode { get; set; }

    [FromForm(Name = "failed_reason_msg")]
    public string? FailedReasonMessage { get; set; }

    [FromForm(Name = "test_mode")]
    public string? TestMode { get; set; }

    [FromForm(Name = "payment_type")]
    public string? PaymentType { get; set; }

    [FromForm(Name = "currency")]
    public string? Currency { get; set; }

    [FromForm(Name = "payment_amount")]
    public string? PaymentAmount { get; set; }
}

public sealed record PayTrTokenResult(bool IsSuccess, string? Token, string? ErrorMessage);

public sealed record PayTrRefundResult(bool IsSuccess, string? ErrorCode, string? ErrorMessage, bool IsUncertain = false);
