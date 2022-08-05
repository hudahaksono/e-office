using iText.Html2pdf;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Surat.Models;
using Surat.Models.Entities;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Surat.Codes.Functions;

namespace Surat.Codes.TataNaskah
{
    public class SuratKeputusan
    {
        DataMasterModel dataMasterModel = new DataMasterModel();
        NaskahDinasModel nd = new NaskahDinasModel();
        SuratModel mdl = new SuratModel();
        internal MemoryStream prepare(DraftSurat data, userIdentity usr, string pathx)
        {

            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument pdfDocument = new PdfDocument(pw);
            Rectangle pageSize = new Rectangle(width: 597, height: 935);
            Document doc = new Document(pdfDocument, new PageSize(pageSize));
            doc.SetMargins(40, 20, 60, 20);

            Table _table;
            Cell _cell;

            #region call data

            var sm = new SuratModel();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            var dokumen = data;
            var kopsurat = nd.getKopDetail(dokumen.KopSurat);

            #endregion

            if (dokumen.TandaTanganBasah)
            {
                pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, new FooterEvent());
            }

            KopSuratAlamat kop = new KopSuratAlamat();
            KopSK SK = new KopSK();
            if (dokumen.ProfilePengirim == "H0000001")
            {
                pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, new AlamatBottom());
                SK.garuda(doc, pathx, kopsurat, kantorid, unitkerjaid);
            }
            else
            {
                bool usesatker = true;
                if (dokumen.AtasNama == "H0000001")
                {
                    usesatker = false;
                }
                SK.generate(doc, pathx, kopsurat, kantorid, unitkerjaid, usesatker: usesatker);
            }



            FontProgram fontProgramBokos = FontProgramFactory.CreateFont(pathx + "\\pdf\\fonts\\BOOKOS.TTF");
            PdfFont fontBokos = PdfFontFactory.CreateFont(fontProgramBokos, PdfEncodings.IDENTITY_H, true);

            dokumen.Pengirim = dataMasterModel.GetProfileNameFromId(dokumen.ProfilePengirim);
            string keputusanNama = "";
            if(dokumen.ProfilePengirim == "H0000001" || dokumen.AtasNama == "H0000001")
            {
                keputusanNama = "MENTERI AGRARIA DAN TATA RUANG/\nKEPALA BADAN PERTANAHAN NASIONAL";
            }
            if(dokumen.ProfilePengirim == "H4000000" || dokumen.AtasNama == "H4000000")
            {
                keputusanNama = "DIREKTUR JENDERAL SURVEI DAN\nPEMETAAN PERTANAHAN DAN RUANG";
            }
            else
            {
                if (!string.IsNullOrEmpty(dokumen.AtasNama))
                {
                    keputusanNama = dataMasterModel.GetProfileNameFromId(dokumen.AtasNama).ToUpper();
                }
                else
                {
                    keputusanNama = dokumen.Pengirim.ToUpper();
                }
            }
            if (keputusanNama.Contains(" DAN ") && dokumen.ProfilePengirim != "H0000001" && dokumen.AtasNama != "H0000001" && dokumen.ProfilePengirim != "H4000000" && dokumen.AtasNama != "H4000000")
            {
                keputusanNama = keputusanNama.Replace(" DAN ", " DAN\n");
            } 
            

            _table = new Table(1).SetWidth(470).SetPaddingLeft(20).SetMargin(0).SetHorizontalAlignment(HorizontalAlignment.CENTER); //table buat badan
            _cell = new Cell().Add(new Paragraph($"KEPUTUSAN {keputusanNama}").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(26f).SetMargin(0f);
            _table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("KEMENTERIAN AGRARIA DAN TATA RUANG/\nBADAN PERTANAHAN NASIONAL").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(0f).SetMargin(0f);
            _table.AddCell(_cell);

            _cell = new Cell().Add(new Paragraph("").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(6f).SetMargin(0f);
            _table.AddCell(_cell);
            Paragraph N = new Paragraph();
            N.Add(new Text("NOMOR : ")).SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER);
            if (string.IsNullOrEmpty(dokumen.NomorSurat))
            {
                N.Add(new Text("____________________________").SetFontColor(ColorConstants.WHITE)).SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER);
            }
            else
            {
                N.Add(new Text(dokumen.NomorSurat)).SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER);
            }
            _cell = new Cell().Add(N).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(0).SetMargin(0f);
            _table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(6f).SetMargin(0f);
            _table.AddCell(_cell);

            var isiperihal = $"<link rel='stylesheet' type='text/css' href='{pathx}/pdf/PdfStyle.css'><div style='text-align: center;'><span style='font-size:11pt;'>TENTANG</br>{HttpUtility.UrlDecode(HttpUtility.UrlDecode(dokumen.Perihal)).ToUpper()}</span><div>";
            foreach (IElement element in HtmlConverter.ConvertToElements(isiperihal))
            {
                _cell = new Cell().Add((IBlockElement)element).SetBorder(Border.NO_BORDER);
                _cell.SetPaddingTop(6f).SetMargin(0f);
                _table.AddCell(_cell);
            }

            //var perihaltext = string.IsNullOrEmpty(dokumen.Perihal) ? "<<Ringkasan Pengenal Surat>>" : HttpUtility.UrlDecode(HttpUtility.UrlDecode(dokumen.Perihal));
            //_cell = new Cell().Add(new Paragraph($"TENTANG\n{perihaltext.ToUpper()}").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            //_cell.SetPaddingTop(6f).SetMargin(0f);
            //_table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(6f).SetMargin(0f);
            _table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph($"{keputusanNama}").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(6f).SetMargin(0f);
            _table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("KEMENTERIAN AGRARIA DAN TATA RUANG/\nBADAN PERTANAHAN NASIONAL").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(0f).SetMargin(0f);
            _table.AddCell(_cell);





            string isisurat;
            isisurat = HttpUtility.UrlDecode(dokumen.IsiSurat);
            isisurat = isisurat.Replace("<br>", "<br/>");
            isisurat = isisurat.Replace("</table><table", "</table><br/><table");
            if (!isisurat.Substring(0, 1).Equals("<p"))
            {
                int pos = isisurat.IndexOf("<p");
                if (pos > -1)
                {
                    isisurat = "<p>" + isisurat.Substring(0, pos) + "</p>" + isisurat.Substring(pos, isisurat.Length - pos);
                }
                else
                {
                    isisurat = "<p>" + isisurat + "</p>";
                }
            }
            isisurat = "<link rel='stylesheet' type='text/css' href='" + pathx + "/pdf/PdfStyle.css'>" + isisurat;

            foreach (IElement element in HtmlConverter.ConvertToElements(isisurat))
            {
                _cell = new Cell().Add((IBlockElement)element);
                _cell.SetMarginBottom(0).SetBorder(Border.NO_BORDER);
                _table.AddCell(_cell);
            }

            _cell = new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingBottom(20f);
            _table.AddCell(_cell);


            Style styledalem = new Style()
                .SetFont(fontBokos)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(0)
                .SetPaddingBottom(0);

            kop.PenandatanganSection(
                     _table, _cell, styledalem, dokumen, tipekantorid.ToString()
                 );


            if (dokumen.Tembusan.Count() > 0)
            {
                _cell = new Cell().Add(new Paragraph("Tembusan:").SetPaddingTop(10f).SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos)).SetBorder(Border.NO_BORDER);
                _table.AddCell(_cell);
                dokumen.listTembusan = kop.DrawTembusan(dokumen.Tembusan, pathx);
                foreach (IElement element in HtmlConverter.ConvertToElements(dokumen.listTembusan))
                {
                    _cell = new Cell().Add((IBlockElement)element);
                    _cell.SetMarginBottom(0).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell);
                }
            }



            doc.Add(_table);

            doc.Close();
            pdfDocument.Close();
            pw.Close();



            byte[] byteArray = kop.polish(ms.ToArray(), fontBokos, dokumen.DraftCode, pathx);

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            return mss;
        }
    }
}