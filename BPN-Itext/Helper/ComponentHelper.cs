using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Drawing.Imaging;
using System.IO;
using static PDFEditor.Helper.FontHelper;

namespace PDFEditor.Helper
{
    static class ComponentHelper
    {
        public static Paragraph Paragraph(ILeafElement element, TextAlignment? textAlignment = null, FONT? eFont = null)
        {
            var p = new Paragraph();
            p.Add(element);
            if (textAlignment != null)
                p.SetTextAlignment(textAlignment);

            if (eFont != null)
            {
                var font = GetPdfFont(eFont);
                if (font != null)
                {
                    Style style = new Style();
                    style.SetFont(font);
                    p.AddStyle(style);
                }
            }

            return p;
        }

        public static Paragraph Paragraph(string text)
        {
            var p = new Paragraph(text);
            return p;
        }

        public static PdfFont GetPdfFont(FONT? eFont)
        {
            if (eFont == null)
                return null;
            var f = FontHelper.GetFont((FONT)eFont);
            if (string.IsNullOrEmpty(f))
            {
                return null;
            }
            FontProgram fontProgram = FontProgramFactory.CreateFont(f);
            PdfFont font = null;
            if (!PdfFontFactory.IsRegistered(f))
            {
                font = PdfFontFactory.CreateFont(fontProgram, PdfEncodings.IDENTITY_H, true);
            }

            return font;
        }

        public static Text Text(string data, 
            TextAlignment textAlign = TextAlignment.LEFT, 
            FONT? eFont = null, 
            float fontSize = 8,
            Color fontColor = null)
        {
            var text = new Text(data)
                .SetTextAlignment(textAlign);

            text.SetFontSize(fontSize);

            if (eFont != null)
            {
                var font = GetPdfFont(eFont);
                if (font != null)
                {
                    Style style = new Style();
                    style.SetFont(font);
                    style.SetFontColor(fontColor);
                    text.AddStyle(style);
                }
            }

            return text;
        }

        public static Table Table(float[] pointColumWidth = null, Cell[] elements = null, bool isKeepTogether = false)
        {
            Table table;

            if (pointColumWidth == null)
                table = new Table(UnitValue.CreatePercentArray(new float[] { 1 })).UseAllAvailableWidth();
            else
                table = new Table(pointColumWidth);

            table.SetKeepTogether(isKeepTogether);

            if (elements != null && elements.Length > 0)
                for (int i = 0; i < elements.Length; i++)
                    table.AddCell(elements[i]);

            table.SetWidth(UnitValue.CreatePercentValue(100));

            return table;
        }

        public static Image ImageIconBsre(float width, float height)
        {
            byte[] data;
            using (Stream stream = new FileStream(Path.Combine(Constant.GetResourceDir(), "icon_tte_bsre.jpg"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    data = ms.ToArray();
                }
            }
            var imageData = ImageDataFactory.Create(data); // Config.IMAGE_DENAH
            Image image = new Image(imageData)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetWidth(width)
            .SetHeight(height);

            return image;
        }

        public static Image ImageQrCode(float width, float height)
        {
            byte[] data;
            using (Stream stream = new FileStream(Path.Combine(Constant.GetResourceDir(), "qrcode.png"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    data = ms.ToArray();
                }
            }
            var imageData = ImageDataFactory.Create(data); // Config.IMAGE_DENAH
            Image image = new Image(imageData)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetWidth(width)
            .SetHeight(height);

            return image;
        }

        public static void HorizontalLine(Document document, int page, float width, float x, float y)
        {
            SolidLine line = new SolidLine(1f);
            line.SetColor(ColorConstants.BLACK);

            LineSeparator ls = new LineSeparator(line);
            ls.SetWidth(width); // widthDoc - 84 - 2f

            var pSeparator = Paragraph("").Add(ls);
            document.ShowTextAligned(
                    pSeparator,
                    x,
                    y,
                    page,
                    TextAlignment.LEFT,
                    VerticalAlignment.TOP,
                    0);
        }

        public static void AddImageBSRE(Document document, int page, float width, float height, float x, float y)
        {
            var iIconBsre = ImageIconBsre(width, height);
            var pIconBsre = Paragraph(iIconBsre);
            document.ShowTextAligned(
                    pIconBsre,
                    x,
                    y,
                    page,
                    TextAlignment.LEFT,
                    VerticalAlignment.TOP,
                    0);
        }

        public static Cell Cell(
            Paragraph paragraph = null, 
            bool border = false, 
            TextAlignment textAlign = TextAlignment.LEFT, 
            VerticalAlignment verticalAlignment = VerticalAlignment.TOP)
        {
            Cell cell = new Cell();

            if (!border)
                cell.SetBorder(Border.NO_BORDER);

            if (paragraph != null)
                cell.Add(paragraph);

            cell.SetTextAlignment(textAlign);
            cell.SetVerticalAlignment(verticalAlignment);

            return cell;
        }

        public static Image ImageResizeAspectRatio(string path, float width, float height)
        {
            var bmp = ImageHelper.ResizeImage(path, width, height, true);
            byte[] dataMs;
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                dataMs = ms.ToArray();
            }

            var imageData = ImageDataFactory.Create(dataMs);
            Image image = new Image(imageData)
                .SetTextAlignment(TextAlignment.CENTER);

            return image;
        }

        public static Image ImageResizeAspectRatio(Stream imgStream, float width, float height)
        {
            var bmp = ImageHelper.ResizeImage(imgStream, width, height, true);
            byte[] dataMs;
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                dataMs = ms.ToArray();
            }

            var imageData = ImageDataFactory.Create(dataMs);
            Image image = new Image(imageData)
                .SetTextAlignment(TextAlignment.CENTER);

            return image;
        }

    }
}
