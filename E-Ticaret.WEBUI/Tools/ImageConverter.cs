using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Tools
{
    public static class ImageConverter
    {
        private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public static async Task<int> ConvertDirectoryToWebPAsync(
            string sourceDirectory, 
            string targetDirectory = null,
            bool deleteOriginal = false,
            int? maxWidth = null,
            int quality = 75)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
                throw new ArgumentNullException(nameof(sourceDirectory));

            // Eğer hedef dizin belirtilmemişse, kaynak dizinin içine "webp" adında bir alt dizin oluştur
            targetDirectory ??= Path.Combine(sourceDirectory, "webp");

            // Hedef dizini oluştur
            Directory.CreateDirectory(targetDirectory);

            // Tüm desteklenen dosyaları bul
            var files = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            if (!files.Any())
            {
                Console.WriteLine("Dönüştürülecek dosya bulunamadı.");
                return 0;
            }

            Console.WriteLine($"{files.Count} adet dosya dönüştürülecek...\n");

            int convertedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            foreach (var file in files)
            {
                try
                {
                    // İlgili alt dizin yapısını koru
                    var relativePath = Path.GetRelativePath(sourceDirectory, file);
                    var targetSubDir = Path.Combine(targetDirectory, Path.GetDirectoryName(relativePath));
                    Directory.CreateDirectory(targetSubDir);

                    var targetFile = Path.Combine(
                        targetSubDir,
                        Path.GetFileNameWithoutExtension(file) + ".webp");

                    // Eğer hedef dosya zaten varsa ve daha yeni değilse atla
                    if (File.Exists(targetFile) && 
                        File.GetLastWriteTimeUtc(file) <= File.GetLastWriteTimeUtc(targetFile))
                    {
                        Console.WriteLine($"[ATLANDI] {Path.GetFileName(file)}");
                        skippedCount++;
                        continue;
                    }

                    Console.Write($"Dönüştürülüyor: {Path.GetFileName(file)}... ");

                    // Resmi yükle ve işle
                    using var image = await Image.LoadAsync(file);

                    // Yeniden boyutlandır
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

                    // WebP olarak kaydet
                    var encoder = new WebpEncoder
                    {
                        Quality = quality,
                        Method = WebpEncodingMethod.Level4,
                        FileFormat = WebpFileFormatType.Lossy
                    };

                    using var output = File.Create(targetFile);
                    await image.SaveAsWebpAsync(output, encoder);

                    Console.WriteLine("Tamam");
                    convertedCount++;

                    // Orijinal dosyayı sil (eğer istenirse)
                    if (deleteOriginal)
                    {
                        try
                        {
                            File.Delete(file);
                            Console.WriteLine($"  Orijinal dosya silindi: {Path.GetFileName(file)}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Orijinal dosya silinemedi: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nHATA: {file} - {ex.Message}");
                    errorCount++;
                }
            }

            Console.WriteLine("\nDönüştürme tamamlandı!");
            Console.WriteLine($"Toplam: {files.Count}, Başarılı: {convertedCount}, Atlandı: {skippedCount}, Hata: {errorCount}");

            return convertedCount;
        }
    }
}
