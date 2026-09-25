namespace E_Ticaret.WEBUI.Payments;

public sealed class PayTrOptions
{
    public const string SectionName = "PayTR";

    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string MerchantSalt { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = "https://www.paytr.com/odeme/api/get-token";
    public string PaymentUrl { get; set; } = "https://www.paytr.com/odeme/guvenli";
    public string RefundUrl { get; set; } = "https://www.paytr.com/odeme/iade";
    public int TestMode { get; set; } = 1;
    public int DebugOn { get; set; } = 1;
    public int NoInstallment { get; set; }
    public int MaxInstallment { get; set; }
    public int TimeoutLimit { get; set; } = 30;
    public string Currency { get; set; } = "TL";

    public bool HasCredentials =>
        !string.IsNullOrWhiteSpace(MerchantId) &&
        !string.IsNullOrWhiteSpace(MerchantKey) &&
        !string.IsNullOrWhiteSpace(MerchantSalt);
}
