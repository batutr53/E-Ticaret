namespace E_Ticaret.Core.DTO
{
    public class PaymentResponseDTO
    {
        public string? TxnNo { get; set; }                 // TRPOS Tarafından Oluşturulan İşlem Numarası
        public string? TxnType { get; set; }              // İşlem Tipi
        public string? ResultCode { get; set; }           // Ödeme İşleminin Sonucu
        public string? ResultDetail { get; set; }         // Ödeme İşlem Sonucuna Ait Açıklama
        public string? Amount { get; set; }               // İşlem Tutarı
        public string? NetAmount { get; set; }            // Komisyonlar düşüldükten sonra kalan müşteri hak ediş tutarı
        public string? CurrencyAmount { get; set; }      // İşlem Tutarı döviz karşılığı (opsiyonel)
        public string? NetSumCommissionAmount { get; set; } // İşlemin toplam komisyon tutarı (opsiyonel)
        public string? Currency { get; set; }             // İşlem Para Birimi
        public string? AmountTL { get; set; }            // İşlem Tutarı TL Karşılığı (opsiyonel)
        public string? StoreType { get; set; }            // Ödeme Yöntemi
        public string? Oid { get; set; }                  // Sipariş Numarası
        public string? Hash { get; set; }                 // İşlem doğruluğunu kontrol etmek için hash değeri
    }
}
