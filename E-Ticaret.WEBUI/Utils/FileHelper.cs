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

            if (formFile.Length > MaxFileSizeInBytes)
                return null;

            // Benzersiz dosya adı üretme
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, uniqueFileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await formFile.CopyToAsync(stream);

            return uniqueFileName;
        }

        public static void FileRemover(string fileName, string filePath = "/img/")
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'), fileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }

}
