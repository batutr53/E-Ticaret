using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace E_Ticaret.WEBUI.Utils;

public static class ProductImageProcessor
{
    public static async Task<byte[]> NormalizeAsync(Stream input)
    {
        var options = ImageDecoding.Options;
        var info = await Image.IdentifyAsync(options, input);
        if (info == null || (long)info.Width * info.Height > 40_000_000)
            throw new InvalidDataException("Görsel en fazla 40 megapiksel olabilir.");
        input.Position = 0;
        using var image = await Image.LoadAsync<Rgba32>(options, input);
        image.Mutate(x => x.AutoOrient().Resize(new ResizeOptions
        {
            Size = new Size(1000, 1300), Mode = ResizeMode.Pad,
            Position = AnchorPositionMode.Center, PadColor = Color.White
        }).BackgroundColor(Color.White));
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;
        using var output = new MemoryStream();
        await image.SaveAsJpegAsync(output, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 88 });
        return output.ToArray();
    }
}
