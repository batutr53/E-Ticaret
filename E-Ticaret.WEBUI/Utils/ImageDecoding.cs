using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace E_Ticaret.WEBUI.Utils;

public static class ImageDecoding
{
    // Do not register TIFF (including disguised .jpg uploads) or other unused decoders.
    // ImageSharp 3.1.x has TIFF advisories; the application never reads or writes TIFF.
    private static readonly Configuration AllowedFormats = new(
        new JpegConfigurationModule(), new PngConfigurationModule(),
        new GifConfigurationModule(), new WebpConfigurationModule());

    public static DecoderOptions Options => new() { Configuration = AllowedFormats, MaxFrames = 1 };
}
