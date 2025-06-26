using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using E_Ticaret.WEBUI.Extensions;

namespace E_Ticaret.WEBUI.Helpers
{
    public static class ImageHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private const int MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        /// <summary>
        /// HTTP bağlamına erişmek için IHttpContextAccessor'ı yapılandırır.
        /// </summary>
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Göreceli resim yolunu tam URL'ye dönüştürür.
        /// </summary>
        public static string ToFullImageUrl(string relativeImagePath)
        {
            if (string.IsNullOrWhiteSpace(relativeImagePath))
                return "/img/no-image.png";

            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request == null)
                return relativeImagePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{(relativeImagePath.StartsWith("/") ? "" : "/")}{relativeImagePath.TrimStart('/')}";
        }

        /// <summary>
        /// Resmi işler ve kaydeder. WebP formatına dönüştürme ve yeniden boyutlandırma desteği içerir.
        /// </summary>
        /// <param name="file">Yüklenecek dosya</param>
        /// <param name="savePath">Kaydedilecek dizin (wwwroot'dan sonraki kısım, örn: "img/products")</param>
        /// <param name="convertToWebP">WebP formatına dönüştürülsün mü?</param>
        /// <param name="maxWidth">Maksimum genişlik (piksel)</param>
        /// <param name="quality">Kalite (1-100)</param>
        /// <returns>Kaydedilen dosyanın göreceli yolu (wwwroot'dan itibaren)</returns>
        public static async Task<string> ProcessAndSaveImageAsync(IFormFile file, string savePath, bool convertToWebP = false, int? maxWidth = null, int quality = 75)
        {
            // Giriş kontrolleri
            if (file == null || file.Length == 0)
                return null;

            // Dosya uzantısı kontrolü
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Geçersiz dosya uzantısı. Sadece JPG, JPEG, PNG, GIF ve WebP formatları desteklenmektedir.");
            }

            // Dosya boyutu kontrolü
            if (file.Length > MaxFileSize)
            {
                throw new InvalidOperationException($"Dosya boyutu çok büyük. Maksimum izin verilen boyut: {MaxFileSize / 1024 / 1024}MB");
            }

            // Güvenli dosya adı oluştur
            var fileName = Path.GetFileNameWithoutExtension(file.FileName).ToUrlFriendly();
            var extension = convertToWebP ? ".webp" : fileExtension;
            var safeFileName = $"{fileName}{extension}";

            // wwwroot dizinini temel alarak tam yolu oluştur
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullSavePath = Path.Combine(webRootPath, savePath.TrimStart('\\', '/'));
            var fullFilePath = Path.Combine(fullSavePath, safeFileName);

            try
            {
                // Dizin yoksa oluştur
                Directory.CreateDirectory(fullSavePath);

                // Resmi yükle ve işle
                using var image = await Image.LoadAsync(file.OpenReadStream());
                
                // Yeniden boyutlandırma
                if (maxWidth.HasValue && image.Width > maxWidth.Value)
                {
                    var options = new ResizeOptions
                    {
                        Size = new Size(maxWidth.Value, 0),
                        Mode = ResizeMode.Max,
                        Position = AnchorPositionMode.Center,
                        Compand = true
                    };
                    image.Mutate(x => x.Resize(options));
                }

                // Kalite ayarı (1-100 arasında olmalı)
                quality = Math.Clamp(quality, 1, 100);

                // WebP'ye dönüştürme
                if (convertToWebP)
                {
                    var encoder = new WebpEncoder
                    {
                        Quality = quality,
                        Method = WebpEncodingMethod.Level4,
                        FileFormat = WebpFileFormatType.Lossy
                    };
                    await image.SaveAsWebpAsync(fullFilePath, encoder);
                }
                // Orijinal formatta kaydet (JPEG veya diğer formatlar)
                else
                {
                    if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                    {
                        var encoder = new JpegEncoder { Quality = quality };
                        await image.SaveAsJpegAsync(fullFilePath, encoder);
                    }
                    else
                    {
                        // Diğer formatlar için orijinal haliyle kaydet
                        await using var fileStream = new FileStream(fullFilePath, FileMode.Create);
                        await file.CopyToAsync(fileStream);
                    }
                }

                // Göreceli yolu döndür (wwwroot'dan sonrası)
                return Path.Combine(savePath, safeFileName).Replace("\\", "/").TrimStart('/');
            }
            catch (Exception ex)
            {
                // Hata durumunda oluşturulan dosyayı sil
                if (System.IO.File.Exists(fullFilePath))
                {
                    try { System.IO.File.Delete(fullFilePath); } catch { }
                }
                throw new InvalidOperationException("Resim işlenirken bir hata oluştu.", ex);
            }
        }
    }
}
