using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Services
{
    public interface IImageService
    {
        Task<string> ConvertToWebPAsync(Stream imageStream, string outputPath, int quality = 75, int? maxWidth = null);
        string GetWebPImagePath(string originalPath);
    }

    public class ImageService : IImageService
    {
        public async Task<string> ConvertToWebPAsync(Stream imageStream, string outputPath, int quality = 75, int? maxWidth = null)
        {
            // Çıktı dizinini oluştur
            var directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // WebP formatında kaydet
            using (var image = await Image.LoadAsync(imageStream))
            {
                // İsteğe bağlı olarak yeniden boyutlandır
                if (maxWidth.HasValue && image.Width > maxWidth.Value)
                {
                    var options = new ResizeOptions
                    {
                        Size = new Size(maxWidth.Value, 0),
                        Mode = ResizeMode.Max
                    };
                    image.Mutate(x => x.Resize(options));
                }

                var encoder = new WebpEncoder
                {
                    Quality = quality,
                    Method = WebpEncodingMethod.Level4,
                    FileFormat = WebpFileFormatType.Lossy
                };

                await using var output = new FileStream(outputPath, FileMode.Create);
                await image.SaveAsWebpAsync(output, encoder);
            }

            return outputPath;
        }

        public string GetWebPImagePath(string originalPath)
        {
            if (string.IsNullOrEmpty(originalPath))
                return originalPath;

            var extension = Path.GetExtension(originalPath);
            if (string.IsNullOrEmpty(extension))
                return originalPath;

            return originalPath.Replace(extension, ".webp");
        }
    }
}
