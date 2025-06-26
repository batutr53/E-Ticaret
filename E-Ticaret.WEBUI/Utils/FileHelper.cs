using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Utils
{
    public class FileHelper
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        private const long MaxFileSizeInBytes = 500 * 1024; // 500KB
        private const int MaxWidth = 1200; // Maksimum genişlik
        private const int Quality = 75; // Varsayılan kalite

        public static async Task<string?> FileLoaderAsync(IFormFile formFile, string filePath = "/img/")
        {
            if (formFile == null || formFile.Length == 0)
                return null;

            var extension = Path.GetExtension(formFile.FileName).ToLower();
            if (!AllowedImageExtensions.Contains(extension))
                return null;

            // Benzersiz dosya adı üretme (Her zaman jpg olarak kaydediyoruz)
            string uniqueFileName = $"{Guid.NewGuid()}.jpg";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, uniqueFileName);

            try
            {
                using var inputStream = formFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                // Yeniden boyutlandırma
                if (image.Width > MaxWidth || image.Height > MaxWidth)
                {
                    var options = new ResizeOptions
                    {
                        Size = new Size(MaxWidth, MaxWidth),
                        Mode = ResizeMode.Max,
                        Sampler = KnownResamplers.Lanczos3,
                        Compand = true
                    };
                    
                    image.Mutate(x => x.Resize(options));
                }

                // JPEG için optimize ayarları
                var encoder = new JpegEncoder
                {
                    Quality = Quality
                };

                // Önce belleğe yaz
                using var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, encoder);

                // Eğer hala çok büyükse kaliteyi düşür
                int currentQuality = Quality;
                while (ms.Length > MaxFileSizeInBytes && currentQuality > 30)
                {
                    currentQuality -= 5;
                    // Yeni bir encoder örneği oluştur
                    var newEncoder = new JpegEncoder
                    {
                        Quality = currentQuality
                    };
                    ms.SetLength(0);
                    await image.SaveAsJpegAsync(ms, newEncoder);
                }

                // Dosyaya yaz
                await File.WriteAllBytesAsync(fullPath, ms.ToArray());
            }
            catch (Exception ex)
            {
                // Hata durumunda orijinal dosyayı kaydet
                using var stream = new FileStream(fullPath, FileMode.Create);
                await formFile.CopyToAsync(stream);
            }

            // Return just the filename, let the view handle the path
            return uniqueFileName;
        }

        public static void FileRemover(string fileName, string filePath = "/img/")
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'), fileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
