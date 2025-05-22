using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Helpers
{
    public static class ImageHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string ToFullImageUrl(string relativeImagePath)
        {
            if (string.IsNullOrWhiteSpace(relativeImagePath))
                relativeImagePath = "/img/no-image.png";

            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request == null)
                return relativeImagePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{(relativeImagePath.StartsWith("/") ? "" : "/")}{relativeImagePath}";
        }

        private const int MaxFileSize = 5 * 1024 * 1024; // 2MB
        public static async Task<string> ProcessAndSaveImageAsync(IFormFile file, string savePath)
        {
            if (file == null || file.Length == 0)
                return null;

            byte[] fileBytes;
            using (var inputStream = file.OpenReadStream())
            {
                if (file.Length <= MaxFileSize)
                {
                    using var ms = new MemoryStream();
                    await inputStream.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }
                else
                {
                    using var image = await Image.LoadAsync(inputStream);
                    int maxWidth = 1024;
                    if (image.Width > maxWidth)
                    {
                        var ratio = (double)maxWidth / image.Width;
                        image.Mutate(x => x.Resize(maxWidth, (int)(image.Height * ratio)));
                    }

                    byte[] outputBytes = null;
                    int quality = 75;
                    do
                    {
                        using var msOut = new MemoryStream();
                        var encoder = new JpegEncoder { Quality = quality }; // her seferinde yeni encoder
                        await image.SaveAsJpegAsync(msOut, encoder);
                        outputBytes = msOut.ToArray();

                        if (outputBytes.Length <= MaxFileSize || quality <= 30)
                            break;

                        quality -= 10;
                    }
                    while (true);

                    fileBytes = outputBytes;
                }
            }

            // Dosyayı kaydet
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            await File.WriteAllBytesAsync(savePath, fileBytes);
            return savePath;
        }
    }
}
