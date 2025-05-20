namespace E_Ticaret.WEBUI.Helpers
{
    public class TelegramHelper
    {
        private readonly string _botToken;
        private readonly string _chatId;
        private readonly string _adminPanelBaseUrl;

        public TelegramHelper(IConfiguration configuration)
        {
            _botToken = configuration["TelegramBot:BotToken"];
            _chatId = configuration["TelegramBot:ChatId"];
            _adminPanelBaseUrl = configuration["TelegramBot:AdminPanelBaseUrl"];
        }

        public async Task SendTelegramMessage(string message)
        {
            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(message)}";
            using var client = new HttpClient();
            await client.GetAsync(url);
        }

        public async Task SendTelegramOrderMessage(int orderId, string message)
        {
            string orderUrl = $"{_adminPanelBaseUrl}{orderId}";
            string htmlMessage = $"{message} <a href=\"{orderUrl}\">Detay için tıkla</a>";
            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(htmlMessage)}&parse_mode=HTML";
            using var client = new HttpClient();
            await client.GetAsync(url);
        }
    }
}
