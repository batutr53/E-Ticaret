namespace E_Ticaret.WEBUI.Utils
{
    public class FileHelper
    {
        public static async Task<string> FileLoaderASynx(IFormFile formFile, string filePath = "/img/")
        {
            string fileName = "";
            if (formFile != null && formFile.Length > 0)
            {
                fileName = formFile.FileName.ToLower();
                string directory = Directory.GetCurrentDirectory() + "/wwwroot" + filePath;
                using var stream = new FileStream(Path.Combine(directory, fileName), FileMode.Create);
                await formFile.CopyToAsync(stream);
            
            }
            return fileName;
        }

        public static void FileRemover(string fileName ,string filePath = "/img/")
        {
            string directory = Directory.GetCurrentDirectory() + "/wwwroot" + filePath + fileName;
            if (File.Exists(directory))
            {
                File.Delete(directory);
            }
        }
    }
}
