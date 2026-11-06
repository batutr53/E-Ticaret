using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace E_Ticaret.WEBUI.Utils;

public class BlogImageStorage(IWebHostEnvironment environment)
{
    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length is <= 0 or > 10 * 1024 * 1024)
            throw new InvalidDataException("Görsel en fazla 10 MB olabilir. JPG, PNG veya WebP yükleyin.");
        await using var input = new MemoryStream();
        await file.CopyToAsync(input, cancellationToken);
        input.Position = 0;
        var info = await Image.IdentifyAsync(ImageDecoding.Options, input, cancellationToken);
        if (info == null || (long)info.Width * info.Height > 40_000_000)
            throw new InvalidDataException("Görsel en fazla 40 megapiksel olabilir.");
        input.Position = 0;
        using var image = await Image.LoadAsync<Rgba32>(ImageDecoding.Options, input, cancellationToken);
        image.Mutate(x => x.AutoOrient().Resize(new ResizeOptions { Size = new Size(1800, 1800), Mode = ResizeMode.Max }).BackgroundColor(Color.White));
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;
        var directory = Path.Combine(environment.WebRootPath, "img", "blog");
        Directory.CreateDirectory(directory);
        var filename = Guid.NewGuid().ToString("N") + ".jpg";
        await image.SaveAsJpegAsync(Path.Combine(directory, filename), new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 88 }, cancellationToken);
        return filename;
    }
}
