using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace E_Ticaret.Tests;

public class ContentAndImageTests
{
    [Theory]
    [InlineData("<h3>Aranjman &ccedil;i&ccedil;ek g&ouml;nder</h3>")]
    [InlineData("<h3>Aranjman &amp;ccedil;i&amp;ccedil;ek g&amp;ouml;nder</h3>")]
    [InlineData("&lt;h3&gt;Aranjman &amp;ccedil;i&amp;ccedil;ek g&amp;ouml;nder&lt;/h3&gt;")]
    public void TurkishSeoTextRemainsReadableAndIdempotent(string input)
    {
        var result = RichTextHelper.Normalize(input);
        Assert.Contains("<h3>Aranjman çiçek gönder</h3>", result);
        Assert.Equal(result, RichTextHelper.Normalize(result));
    }

    [Fact]
    public void RemovesPastedArtifactsAndUnsafeHtmlWithoutDestroyingStructure()
    {
        var result = RichTextHelper.Normalize("<h3 data-pm-slice='1 1 []'>Başlık</h3><p style='position:fixed'>Özel <strong>çiçek</strong><script>alert(1)</script><a href='javascript:alert(1)'>Bağlantı</a></p>");
        Assert.Contains("<h3>Başlık</h3>", result);
        Assert.Contains("<strong>çiçek</strong>", result);
        Assert.DoesNotContain("data-pm", result);
        Assert.DoesNotContain("position:fixed", result);
        Assert.DoesNotContain("<script", result);
        Assert.DoesNotContain("javascript:", result);
    }

    [Theory]
    [InlineData(800, 400)]
    [InlineData(400, 800)]
    [InlineData(600, 600)]
    public async Task ProductPhotosShareCanvasAndStayCenteredWithoutCropping(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, Color.Red);
        using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        using var output = Image.Load<Rgba32>(await ProductImageProcessor.NormalizeAsync(stream));
        Assert.Equal(1000, output.Width);
        Assert.Equal(1300, output.Height);
        Assert.True(output[500, 650].R > 240 && output[500, 650].G < 15);
        var top = output[500, 0];
        var bottom = output[500, 1299];
        Assert.Equal(top.R, bottom.R);
        Assert.InRange(Math.Abs(top.G - bottom.G), 0, 5);
    }

    [Fact]
    public async Task TiffDecoderIsNotAvailableEvenForDisguisedUploads()
    {
        using var image = new Image<Rgba32>(10, 10, Color.Red);
        using var tiff = new MemoryStream();
        await image.SaveAsTiffAsync(tiff);
        tiff.Position = 0;
        await Assert.ThrowsAsync<UnknownImageFormatException>(() => ProductImageProcessor.NormalizeAsync(tiff));
    }
}
