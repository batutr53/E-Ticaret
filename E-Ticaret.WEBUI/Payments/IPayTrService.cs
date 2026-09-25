using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Payments;

public interface IPayTrService
{
    Task<PayTrTokenResult> CreatePaymentTokenAsync(
        Order order,
        string userIp,
        string successUrl,
        string failureUrl,
        CancellationToken cancellationToken = default);

    Task<PayTrRefundResult> RefundAsync(
        string merchantOid,
        decimal amount,
        CancellationToken cancellationToken = default);

    bool ValidateCallback(PayTrCallbackModel callback);
    string GetPaymentUrl(string token);
}
