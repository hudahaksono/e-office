using PDFEditor.Helper;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PDFEditor
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Init.Setup();
            //ContohPenggunaan();
        }

        public static void ContohPenggunaan()
        {
            var pathPdf = @"D:\Computer\Downloads\test_landscape.pdf";
            //var pathPdf = @"C:\Users\sagadio\Desktop\test.pdf";
            var destPathPdf = @"D:\Computer\Downloads\demo1.pdf";
            var pathImage = @"C:\Users\sagadio\Desktop\bg.png";


            var pdfSream = new FileStream(pathPdf, FileMode.Open, FileAccess.Read);
            //var imgStream = new FileStream(pathImage, FileMode.Open, FileAccess.Read);

            var pdfBuilder = new PDFBuilder();

            #region STREAM
            //var result = pdfBuilder.Build(
            //    pdfSream,
            //    imgStream,
            //    page: 2,
            //    appendPositionImage: PDFBuilder.Position.MIDDLE);

            //var result = pdfBuilder.Build(
            //    pdfSream,
            //    imgStream,
            //    page: 0,
            //    appendPositionImage: PDFBuilder.Position.TOP,
            //    imageWidth: 200,
            //    imageHeight: 50);

            //var result = pdfBuilder.Build(
            //    pdfSream,
            //    imgStream,
            //    page: 0,
            //    x: 0,
            //    y: 500);

            //var result = pdfBuilder.Build(
            //    pdfSream,
            //    imgStream,
            //    page: 0,
            //    x: 0,
            //    y: 500,
            //    imageWidth: 200,
            //    imageHeight: 50);
            #endregion

            #region File Path
            //var result = pdfBuilder.Build(
            //    pathPdf,
            //    destPathPdf,
            //    pathImage,
            //    page: 2,
            //    appendPositionImage: PDFBuilder.Position.MIDDLE);

            //var result = pdfBuilder.Build(
            //    pathPdf,
            //    destPathPdf,
            //    pathImage,
            //    page: 0,
            //    appendPositionImage: PDFBuilder.Position.TOP,
            //    imageWidth: 200,
            //    imageHeight: 50);

            //var result = pdfBuilder.Build(
            //    pathPdf,
            //    destPathPdf,
            //    pathImage,
            //    page: 0,
            //    x: 0,
            //    y: 500);

            //var result = pdfBuilder.Build(
            //    pathPdf,
            //    destPathPdf,
            //    pathImage,
            //    page: 0,
            //    x: 0,
            //    y: 500,
            //    imageWidth: 200,
            //    imageHeight: 50);
            #endregion

            #region TEMPLATE ===> return nya stream
            var result = pdfBuilder.Build(
                streamPdf: pdfSream,
                template: PDFBuilder.Template.FOOTER,
                pageTte: 2);
            #endregion

            // untuk development ====> ini membuat stream jd file
            //if (result.Success)
            //{
            //    if (File.Exists(destPathPdf))
            //    {
            //        File.Delete(destPathPdf);
            //    }
            //    using (var fileStream = new FileStream(destPathPdf, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
            //    {
            //        result.Output.Seek(0, SeekOrigin.Begin);
            //        result.Output.CopyTo(fileStream);
            //    }
            //}

            Console.WriteLine(result.Success);
        }
    }
}
