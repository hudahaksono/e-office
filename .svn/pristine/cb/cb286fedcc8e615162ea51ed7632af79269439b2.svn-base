using System.Drawing.Drawing2D;
using System.IO;

namespace PDFEditor.Helper
{
    static class ImageHelper
    {

        public static System.Drawing.Image ResizeImage(string path, float Width, float Height, bool transparent = false)
        {
            var imgStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            System.Drawing.Image imgPhoto = System.Drawing.Image.FromStream(imgStream);
            imgStream.Close();

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent;
            float nPercentW;
            float nPercentH;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            System.Drawing.Bitmap bmPhoto = new System.Drawing.Bitmap(destWidth, destHeight);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(bmPhoto);
            if (transparent)
                grPhoto.Clear(System.Drawing.Color.Transparent);
            else
                grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.CompositingMode = CompositingMode.SourceCopy;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;

            grPhoto.DrawImage(imgPhoto,
                new System.Drawing.Rectangle(0, 0, destWidth, destHeight));

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static System.Drawing.Image ResizeImage(Stream imgStream, float Width, float Height, bool transparent = false)
        {
            //var imgStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            imgStream.Position = 0;
            System.Drawing.Image imgPhoto = System.Drawing.Image.FromStream(imgStream);
            imgStream.Close();

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent;
            float nPercentW;
            float nPercentH;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            System.Drawing.Bitmap bmPhoto = new System.Drawing.Bitmap(destWidth, destHeight);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(bmPhoto);
            if (transparent)
                grPhoto.Clear(System.Drawing.Color.Transparent);
            else
                grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.CompositingMode = CompositingMode.SourceCopy;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;

            grPhoto.DrawImage(imgPhoto,
                new System.Drawing.Rectangle(0, 0, destWidth, destHeight));

            grPhoto.Dispose();
            return bmPhoto;
        }

    }
}
