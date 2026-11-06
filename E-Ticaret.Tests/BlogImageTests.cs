using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace E_Ticaret.Tests;

public class BlogImageTests
{
    private sealed class EnvironmentStub(string root) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = root;
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Fact]
    public async Task UploadReencodesToJpegPreservesProportionsAndIgnoresOriginalFilename()
    {
        var root = Path.Combine(Path.GetTempPath(), "dtyc-blog-image-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var image = new Image<Rgba32>(2400, 1200, Color.ForestGreen);
            await using var stream = new MemoryStream(); await image.SaveAsPngAsync(stream); stream.Position = 0;
            var file = new FormFile(stream, 0, stream.Length, "file", "../../outside.svg");
            var name = await new BlogImageStorage(new EnvironmentStub(root)).SaveAsync(file);
            Assert.Matches("^[a-f0-9]{32}\\.jpg$", name);
            var path = Path.Combine(root, "img", "blog", name);
            using var stored = await Image.LoadAsync(path);
            Assert.Equal(1800, stored.Width); Assert.Equal(900, stored.Height);
            Assert.Null(stored.Metadata.ExifProfile); Assert.Null(stored.Metadata.IccProfile);
            Assert.Single(Directory.GetFiles(Path.Combine(root, "img", "blog")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
