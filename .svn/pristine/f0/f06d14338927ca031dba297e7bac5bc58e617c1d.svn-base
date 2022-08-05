using PDFEditor.Helper;
using PDFEditor.Model;
using PDFEditor.Template;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Properties;
using System;
using System.IO;
using iText.Forms;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas;

namespace PDFEditor
{
    public class PDFBuilder
    {

        public enum Position
        {
            TOP,
            MIDDLE,
            BOTTOM
        }

        public enum Template
        {
            FOOTER
        }

        private string Validatate(string pathPdf, string pathImage, string pathDestination)
        {
            var msg = "";
            #region validation
            if (string.IsNullOrEmpty(pathPdf))
                msg = "Path PDF tidak boleh kosong.";
            if (!File.Exists(pathPdf))
                msg = "File tidak ditemukan.";
            if (!FileHelper.IsPDF(pathPdf))
                msg = "File tidak didukung. File yang didukung hanya PDF.";


            if (string.IsNullOrEmpty(pathImage))
                msg = "Path Image tidak boleh kosong.";
            if (!File.Exists(pathImage))
                msg = "File Image tidak ditemukan";
            if (!FileHelper.IsImage(pathImage))
                msg = "File Image tidak didukung. File Image yang didukung hanya JPG & PNG.";

            
            if (string.IsNullOrEmpty(pathDestination))
                msg = "Path hasil build tidak boleh kosong atau null.";

            
            var isOutputCreated = false;
            if (string.IsNullOrEmpty(msg))
            {
                isOutputCreated = FileHelper.CreateOutput(pathPdf, pathDestination);
            }

            if (!isOutputCreated) msg = "Gagal membuat File Output, Silahkan periksa lokasi directory";
            #endregion validation

            return msg;
        }

        private string Validatate(Stream pathPdf, Stream pathImage)
        {
            var msg = "";
            #region validation
            if (pathPdf == null)
                msg = "Input Pdf Stream tidak boleh null.";
            if (pathPdf.Length == 0)
                msg = "Panjang dari input Pdf stream 0. Periksa kembali inputan anda.";

            if (pathImage == null)
                msg = "Input Image Stream tidak boleh null.";
            if (pathImage.Length == 0)
                msg = "Panjang dari image stream 0. Periksa kembali inputan anda.";
            #endregion validation

            return msg;
        }

        private string Validatate(Stream pathPdf, Template template)
        {
            var msg = "";
            #region validation
            if (pathPdf == null)
                msg = "Input Pdf Stream tidak boleh null.";
            if (pathPdf.Length == 0)
                msg = "Panjang dari input Pdf stream 0. Periksa kembali inputan anda.";

            if (template == null)
                msg = "Template tidak boleh kosong.";
            #endregion validation

            return msg;
        }


        #region TEMPLATE
        public ResultModel Build(Stream streamPdf, Template template, int pageTte)
        {
            return ExecuteProcess(
                streamPdf: streamPdf,
                template: template,
                pageTte: pageTte);
        }
        #endregion


        #region KATA SAMBUNG
        public ResultModel BuildKalimatSambung(Stream streamPdf)
        {
            return ExecuteProcess(
                streamPdf: streamPdf);
        }
        #endregion

        private ResultModel ExecuteProcess(
            string pathFilePdfIn, 
            string pathFilePdfOut, 
            string pathImage, 
            int page, 
            Position? appendPositionImage = null, 
            float? x = null, 
            float? y = null,
            float? imageWidth = null, 
            float? imageHeight = null,
            bool isFullWidth = true)
        {
            ResultModel result = new ResultModel();
            result.Success = true;
            result.Message = "Berhasil";
            result.ErrorCode = null;
            result.ErrorMessage = null;

            var msg = Validatate(pathFilePdfIn, pathImage, pathFilePdfOut);
            if (!string.IsNullOrEmpty(msg))
            {
                result.ErrorCode = 2001;
                result.ErrorMessage = msg;
                result.Success = false;
                result.Message = "Periksa kebali inputan.";
                return result;
            }

            try
            {
                var stampingProperties = new StampingProperties().UseAppendMode();
                var reader = new PdfReader(pathFilePdfIn);
                var writer = new PdfWriter(pathFilePdfOut);

                PdfDocument pdfDoc = new PdfDocument(reader, writer, stampingProperties);
                PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
                Document doc = new Document(pdfDoc);

                try
                {
                    int numberOfPages = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        if (page == 0 || page == i)
                        {
                            
                            var width = 0f;
                            var height = 0f;

                            if (isFullWidth)
                            {
                                width = pdfDoc.GetPage(i).GetPageSize().GetWidth();
                                height = pdfDoc.GetPage(i).GetPageSize().GetHeight();
                            } 
                            else if (imageWidth != null && imageHeight != null)
                            {
                                width = (float)imageWidth;
                                height = (float)imageHeight;
                            }

                            var image = ComponentHelper.ImageResizeAspectRatio(pathImage, width, height);
                            var pEl = ComponentHelper.Paragraph(image);

                            if ((x == null || y == null) && appendPositionImage != null)
                            {
                                y = GetY(
                                imageHeight: image.GetImageHeight(),
                                pageHeight: pdfDoc.GetPage(i).GetPageSize().GetHeight(),
                                position: (Position)appendPositionImage);
                            }

                            if (x == null) x = 0f;
                            if (y == null) y = 0f;

                            doc.ShowTextAligned(pEl, (float)x, (float)y, i, TextAlignment.LEFT, VerticalAlignment.MIDDLE, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.StackTrace;
                    result.Success = false;
                    result.Message = ex.StackTrace;
                    result.ErrorCode = 1001;
                    result.ErrorMessage = "Gagal";
                }
                finally
                {
                    doc.Close();
                    writer.Close();
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                result.Success = false;
                result.Message = e.StackTrace;
                result.ErrorCode = 1001;
                result.ErrorMessage = "Gagal";
            }

            return result;
        }

        private ResultModel ExecuteProcess(
            Stream streamPdf,
            Stream streamImage,
            int page,
            Position? appendPositionImage = null,
            float? x = null,
            float? y = null,
            float? imageWidth = null,
            float? imageHeight = null,
            bool isFullWidth = true)
        {
            ResultModel result = new ResultModel();
            result.Success = true;
            result.Message = "Berhasil";
            result.ErrorCode = null;
            result.ErrorMessage = null;

            var msg = Validatate(streamPdf, streamImage);
            if (!string.IsNullOrEmpty(msg))
            {
                result.ErrorCode = 2001;
                result.ErrorMessage = msg;
                result.Success = false;
                result.Message = "Periksa kebali inputan.";
                return result;
            }

            try
            {
                // reset stream position
                streamPdf.Position = 0;
                streamImage.Position = 0;

                var stampingProperties = new StampingProperties().UseAppendMode();
                var reader = new PdfReader(streamPdf);
                using (var osWriter = new MemoryStream())
                {
                    var writer = new PdfWriter(osWriter);

                    PdfDocument pdfDoc = new PdfDocument(reader, writer, stampingProperties);
                    PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
                    Document doc = new Document(pdfDoc);

                    try
                    {
                        int numberOfPages = pdfDoc.GetNumberOfPages();
                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            if (page == 0 || page == i)
                            {

                                var width = 0f;
                                var height = 0f;

                                if (isFullWidth)
                                {
                                    width = pdfDoc.GetPage(i).GetPageSize().GetWidth();
                                    height = pdfDoc.GetPage(i).GetPageSize().GetHeight();
                                }
                                else if (imageWidth != null && imageHeight != null)
                                {
                                    width = (float)imageWidth;
                                    height = (float)imageHeight;
                                }

                                var image = ComponentHelper.ImageResizeAspectRatio(streamImage, width, height);
                                var pEl = ComponentHelper.Paragraph(image);

                                if ((x == null || y == null) && appendPositionImage != null)
                                {
                                    y = GetY(
                                    imageHeight: image.GetImageHeight(),
                                    pageHeight: pdfDoc.GetPage(i).GetPageSize().GetHeight(),
                                    position: (Position)appendPositionImage);
                                }

                                if (x == null) x = 0f;
                                if (y == null) y = 0f;

                                doc.ShowTextAligned(pEl, (float)x, (float)y, i, TextAlignment.LEFT, VerticalAlignment.MIDDLE, 0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex.StackTrace;
                        result.Success = false;
                        result.Message = ex.StackTrace;
                        result.ErrorCode = 1001;
                        result.ErrorMessage = "Gagal";
                    }
                    finally
                    {
                        writer.SetCloseStream(false);
                        doc.Close();
                        //writer.Close();
                        reader.Close();

                        if (result.Success)
                        {
                            if (osWriter != null && osWriter.Length > 0)
                            {
                                result.Output = new MemoryStream();
                                osWriter.Position = 0;
                                osWriter.CopyTo(result.Output);
                                writer.SetCloseStream(true);
                            }
                            else
                            {
                                result.Success = false;
                                result.Message = "Gagal membuat pdf.";
                                result.ErrorCode = 2001;
                                result.ErrorMessage = "Length Output Stream " + osWriter.Length;
                            }
                        }
                    }
                }
               
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                result.Success = false;
                result.Message = e.StackTrace;
                result.ErrorCode = 1001;
                result.ErrorMessage = "Gagal";
            }

            return result;
        }

        private ResultModel ExecuteProcess(
            Stream streamPdf,
            Template template,
            int pageTte)
        {
            ResultModel result = new ResultModel();
            result.Success = true;
            result.Message = "Berhasil";
            result.ErrorCode = null;
            result.ErrorMessage = null;

            var msg = Validatate(streamPdf, template);
            if (!string.IsNullOrEmpty(msg))
            {
                result.ErrorCode = 2001;
                result.ErrorMessage = msg;
                result.Success = false;
                result.Message = "Periksa kembali file.";
                return result;
            }

            try
            {
                // reset stream position
                streamPdf.Position = 0;

                var stampingProperties = new StampingProperties();//.UseAppendMode();
                var reader = new PdfReader(streamPdf);

                using (var osWriter = new MemoryStream())
                {
                    var writer = new PdfWriter(osWriter);

                    PdfDocument pdfDoc = new PdfDocument(reader, writer, stampingProperties);
                    PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
                    Document doc = new Document(pdfDoc);

                    try
                    {
                        int numberOfPages = pdfDoc.GetNumberOfPages();
                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            pdfDoc.GetPage(i).SetIgnorePageRotationForContent(true);
                            var pdfPage = pdfDoc.GetPage(i);
                            var rect = pdfPage.GetPageSizeWithRotation();

                            float width = rect.GetWidth();
                            float height = rect.GetHeight();

                            SetTemplate(template, doc, i, pageTte == 0 ? numberOfPages : pageTte, rect.GetWidth(), rect.GetHeight());
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex.StackTrace;
                        result.Success = false;
                        result.Message = "Gagal membuat dokumen.";
                        result.ErrorCode = 1001;
                        result.ErrorMessage = ex.StackTrace;
                    }
                    finally
                    {
                        writer.SetCloseStream(false);
                        doc.Close();
                        reader.Close();

                        if (result.Success)
                        {
                            if (osWriter != null && osWriter.Length > 0)
                            {
                                result.Output = new MemoryStream();
                                osWriter.Position = 0;
                                osWriter.CopyTo(result.Output);
                                result.Output.Position = 0;
                                writer.SetCloseStream(true);
                            }
                            else
                            {
                                result.Success = false;
                                result.Message = "Gagal membuat pdf.";
                                result.ErrorCode = 2001;
                                result.ErrorMessage = "Length Output Stream " + osWriter.Length;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                result.Success = false;
                result.Message = e.StackTrace;
                result.ErrorCode = 1001;
                result.ErrorMessage = "Gagal Menyisipkan Catatan Kaki :\n" + e.Message;
            }

            return result;
        }

        private void SetTemplate(Template template, Document doc, int page, int pageTte, float width, float height)
        {
            switch(template)
            {
                case Template.FOOTER:
                    FooterTemplate.SetTemplate(doc, page, pageTte, width, height);
                    break;
            }
        }

        private float GetY(float imageHeight, float pageHeight, Position position)
        {
            float y = 0; 
            switch(position)
            {
                case Position.TOP:
                    y = pageHeight - (imageHeight / 2);
                    break;
                case Position.MIDDLE:
                    y = (pageHeight / 2);
                    break;
                case Position.BOTTOM:
                    y = imageHeight / 2;
                    break;
            }
            return y;
        }
        public ResultModel UpdateForm(
            Stream streamPdf,
            string tanggal,
            string nomor,
            bool setReadOnly)
        {
            ResultModel result = new ResultModel();
            result.Success = true;
            result.Message = "Berhasil";
            result.ErrorCode = null;
            result.ErrorMessage = null;

            try
            {
                // reset stream position
                streamPdf.Position = 0;

                var stampingProperties = new StampingProperties().UseAppendMode();
                var reader = new PdfReader(streamPdf);
                using (var osWriter = new MemoryStream())
                {
                    var writer = new PdfWriter(osWriter);

                    PdfDocument pdfDoc = new PdfDocument(reader, writer, stampingProperties);
                    PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
                    Document doc = new Document(pdfDoc);

                    try
                    {
                        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, false);
                        if(form != null)
                        {
                            form.GetField("fnomor").SetValue(string.Concat("Nomor : ", nomor == null ? "" : nomor)).SetReadOnly(setReadOnly);
                            form.GetField("ftanggal").SetValue(tanggal == null ? "" : tanggal).SetReadOnly(setReadOnly);
                        }
                        else
                        {
                            result.Success = false;
                            result.Message = "Gagal mendapatkan field.";
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex.StackTrace;
                        result.Success = false;
                        result.Message = "Gagal merubah dokumen.";
                        result.ErrorCode = 1001;
                        result.ErrorMessage = ex.StackTrace;
                    }
                    finally
                    {
                        writer.SetCloseStream(false);
                        doc.Close();
                        reader.Close();

                        if (result.Success)
                        {
                            if (osWriter != null && osWriter.Length > 0)
                            {
                                result.Output = new MemoryStream();
                                osWriter.Position = 0;
                                osWriter.CopyTo(result.Output);
                                result.Output.Position = 0;
                                writer.SetCloseStream(true);
                            }
                            else
                            {
                                result.Success = false;
                                result.Message = "Gagal Update Form pdf.";
                                result.ErrorCode = 2001;
                                result.ErrorMessage = "Length Output Stream " + osWriter.Length;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                result.Success = false;
                result.Message = e.StackTrace;
                result.ErrorCode = 1001;
                result.ErrorMessage = "Gagal";
            }

            return result;
        }

        private ResultModel ExecuteProcess(
           Stream streamPdf)
        {
            ResultModel result = new ResultModel();
            result.Success = true;
            result.Message = "Berhasil";
            result.ErrorCode = null;
            result.ErrorMessage = null;

            try
            {
                // reset stream position
                streamPdf.Position = 0;

                var stampingProperties = new StampingProperties().UseAppendMode();
                var reader = new PdfReader(streamPdf);
                using (var osWriter = new MemoryStream())
                {
                    var writer = new PdfWriter(osWriter);

                    PdfDocument pdfDoc = new PdfDocument(reader, writer, stampingProperties);
                    PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
                    Document doc = new Document(pdfDoc);

                    try
                    {
                        int numberOfPages = pdfDoc.GetNumberOfPages();
                        for (int page = 1; page <= numberOfPages; page++)
                        {
                            var widht = pdfDoc.GetPage(1).GetPageSize().GetWidth();
                            pdfDoc.GetPage(page).SetIgnorePageRotationForContent(true);
                            var nextPage = page + 1;
                            if (nextPage <= numberOfPages)
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(nextPage), strategy);
                                var text = pageContent.Substring(0, 30).Replace("\n", "").Replace("\r", "").Replace("\t", "");
                                PdfCanvas canvas = new PdfCanvas(pdfDoc, page);
                                canvas.BeginText()
                                      .SetFontAndSize(ComponentHelper.GetPdfFont(FontHelper.FONT.BookmanOldStyle_Regular), 8)
                                      .MoveText(widht - 200, 20)
                                      .ShowText($"{text}...")
                                      .EndText();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex.StackTrace;
                        result.Success = false;
                        result.Message = "Gagal membuat dokumen.";
                        result.ErrorCode = 1001;
                        result.ErrorMessage = ex.StackTrace;
                    }
                    finally
                    {
                        writer.SetCloseStream(false);
                        doc.Close();
                        reader.Close();

                        if (result.Success)
                        {
                            if (osWriter != null && osWriter.Length > 0)
                            {
                                result.Output = new MemoryStream();
                                osWriter.Position = 0;
                                osWriter.CopyTo(result.Output);
                                writer.SetCloseStream(true);
                            }
                            else
                            {
                                result.Success = false;
                                result.Message = "Gagal membuat pdf.";
                                result.ErrorCode = 2001;
                                result.ErrorMessage = "Length Output Stream " + osWriter.Length;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                result.Success = false;
                result.Message = e.StackTrace;
                result.ErrorCode = 1001;
                result.ErrorMessage = "Gagal";
            }

            return result;
        }
    }
}
