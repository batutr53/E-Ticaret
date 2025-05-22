using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Utils
{
    public class FileHelper
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

        public static async Task<string?> FileLoaderAsync(IFormFile formFile, string filePath = "/img/")
        {
            if (formFile == null || formFile.Length == 0)
                return null;

            var extension = Path.GetExtension(formFile.FileName).ToLower();
            if (!AllowedImageExtensions.Contains(extension))
                return null;

            // Benzersiz dosya adı üretme (Her zaman jpg olarak kaydediyoruz eğer sıkıştırılırsa)
            string uniqueFileName = $"{Guid.NewGuid()}.jpg";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, uniqueFileName);

            if (formFile.Length > MaxFileSizeInBytes)
            {
                // >5MB ise: otomatik sıkıştır!
                using var inputStream = formFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                // Genişlik büyükse yeniden boyutlandır (ör: 1280 px’e düşür)
                int maxWidth = 1280;
                if (image.Width > maxWidth)
                {
                    double ratio = (double)maxWidth / image.Width;
                    image.Mutate(x => x.Resize(maxWidth, (int)(image.Height * ratio)));
                }

                // Kaliteyi düşürerek kaydet (Jpeg 70)
                var encoder = new JpegEncoder { Quality = 70 };
                using var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, encoder);

                // Hâlâ 5MB üzerindeyse kaliteyi kademeli azalt
                int quality = 70;
                while (ms.Length > MaxFileSizeInBytes && quality > 30)
                {
                    ms.SetLength(0);
                    quality -= 10;
                    encoder = new JpegEncoder { Quality = quality };
                    await image.SaveAsJpegAsync(ms, encoder);
                }

                await File.WriteAllBytesAsync(fullPath, ms.ToArray());
            }
            else
            {
                // 5MB ve altı: doğrudan kaydet
                using var stream = new FileStream(fullPath, FileMode.Create);
                await formFile.CopyToAsync(stream);
            }

            // Yolun başına filePath ekleyerek döndür
            return $"{filePath.TrimEnd('/')}/{uniqueFileName}".Replace("/img/", "");
        }

        public static void FileRemover(string fileName, string filePath = "/img/")
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'), fileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
