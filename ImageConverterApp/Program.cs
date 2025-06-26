using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Kullanım: dotnet run [kaynak_dizin] [hedef_dizin] [genişlik] [kalite] [orijinali_sil]
// Örnek: dotnet run "C:\Resimler" "C:\Cikti" 1200 80 true

// Varsayılan değerler
string sourceDir = "wwwroot/img";  // Varsayılan kaynak dizin
string targetDir = null;          // Varsayılan olarak kaynak_dizin/webp
int? maxWidth = 1200;             // Varsayılan maksimum genişlik
int quality = 80;                 // Varsayılan kalite (1-100)
bool deleteOriginal = false;      // Varsayılan olarak orijinalleri silme

// Komut satırı argümanlarını işle
if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
    sourceDir = args[0];

if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
    targetDir = args[1];

if (args.Length > 2 && int.TryParse(args[2], out int width) && width > 0)
    maxWidth = width;
if (args.Length > 3 && int.TryParse(args[3], out int q) && q >= 1 && q <= 100)
    quality = q;
if (args.Length > 4 && bool.TryParse(args[4], out bool del))
    deleteOriginal = del;
// Eğer hedef dizin belirtilmemişse, kaynak dizinin yanına "webp" klasörü oluştur
if (string.IsNullOrEmpty(targetDir))
{
    targetDir = Path.Combine(sourceDir, "webp");
}

try
{
    Console.WriteLine("=== Resim Dönüştürücü ===");
    Console.WriteLine($"Kaynak Dizin: {Path.GetFullPath(sourceDir)}");
    Console.WriteLine($"Hedef Dizin: {Path.GetFullPath(targetDir)}");
    Console.WriteLine($"Maksimum Genişlik: {maxWidth?.ToString() ?? "Yok"}");
    Console.WriteLine($"Kalite: {quality}");
    Console.WriteLine($"Orijinalleri Sil: {deleteOriginal}");
    Console.WriteLine(new string('=', 30));

    // Kaynak dizinin varlığını kontrol et
    if (!Directory.Exists(sourceDir))
    {
        Console.WriteLine($"Hata: Kaynak dizin bulunamadı: {sourceDir}");
        return 1;
    }

    // Dönüştürme işlemini başlat
    var result = await ConvertImagesAsync(sourceDir, targetDir, maxWidth, quality, deleteOriginal);
    
    Console.WriteLine("\nİşlem tamamlandı!");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"\nHata oluştu: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"İç Hata: {ex.InnerException.Message}");
    }
    return 1;
}

static async Task<int> ConvertImagesAsync(string sourceDir, string targetDir, int? maxWidth, int quality, bool deleteOriginal)
{
    var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    
    // Tüm desteklenen dosyaları bul (alt dizinler dahil)
    var files = Directory.EnumerateFiles(sourceDir, "*.*", SearchOption.AllDirectrees)
        .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
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
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetSubDir = Path.Combine(targetDir, Path.GetDirectoryName(relativePath));
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
