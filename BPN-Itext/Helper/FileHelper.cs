using System.IO;

namespace PDFEditor.Helper
{
    public static class FileHelper
    {
        public static byte[] ReadFileToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static bool CreateOutput(string input, string output)
        {
            var sourceDir = Path.GetDirectoryName(output);
            if (!Directory.Exists(sourceDir))
            {
                Directory.CreateDirectory(sourceDir);
            }

            if (Directory.Exists(sourceDir) && File.Exists(input))
            {
                File.Copy(input, output, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPDF(string file)
        {
            return Path.GetExtension(file).ToLower().Equals(".pdf");
        }

        public static bool IsImage(string file)
        {
            var ext = Path.GetExtension(file).ToLower();
            return 
                ext.Equals(".jpg") ||
                ext.Equals(".jpeg") ||
                ext.Equals(".png");
        }

    }
}
