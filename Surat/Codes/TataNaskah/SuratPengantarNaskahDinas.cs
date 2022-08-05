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
    public class SuratPengantarNaskahDinas
    {
        DataMasterModel dataMasterModel = new DataMasterModel();
        NaskahDinasModel nd = new NaskahDinasModel();
        SuratModel mdl = new SuratModel();
        internal MemoryStream prepare(DraftSurat data, userIdentity usr, string pathx)
        {
            var dokumen = data;
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument pdfDocument = new PdfDocument(pw);
            Document doc;
            if (dokumen.ProfilePengirim == "H0000001")
            {
                Rectangle pageSize = new Rectangle(width: 597, height: 935);

                doc = new Document(pdfDocument, new PageSize(pageSize));
            }
            else
            {
                doc = new Document(pdfDocument, PageSize.A4);
            }
            doc.SetMargins(40, 20, 60, 20);

            Table _table;
            Cell _cell;

            #region call data

            var sm = new SuratModel();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            var kopsurat = sm.getKopDetail(dokumen.KopSurat);

            #endregion

            if (dokumen.TandaTanganBasah)
            {
                pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, new FooterEvent());
            }

            KopSuratAlamat kop = new KopSuratAlamat();
            if (dokumen.ProfilePengirim == "H0000001")
            {
                pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, new AlamatBottom());
                kop.garuda(doc, pathx, kopsurat, kantorid, unitkerjaid);
            }
            else
            {
                kop.generate(doc, pathx, kopsurat, kantorid, unitkerjaid);
            }



            FontProgram fontProgramBokos = FontProgramFactory.CreateFont(pathx + "\\pdf\\fonts\\BOOKOS.TTF");
            PdfFont fontBokos = PdfFontFactory.CreateFont(fontProgramBokos, PdfEncodings.IDENTITY_H, true);

            _table = new Table(1).SetWidth(470).SetPaddingLeft(20).SetMargin(0).SetHorizontalAlignment(HorizontalAlignment.CENTER); //table buat badan
            _cell = new Cell().Add(new Paragraph("").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            _cell.SetPaddingTop(6f).SetMargin(0f);
            _table.AddCell(_cell);


            Style styledalem = new Style()
                .SetFont(fontBokos)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(0)
                .SetPaddingBottom(0);

            dokumen.Pengirim = dataMasterModel.GetProfileNameFromId(dokumen.ProfilePengirim);
            var setPengirim = string.IsNullOrEmpty(dokumen.AtasNama) ? dokumen.ProfilePengirim : dokumen.AtasNama;
            var lokasiditetapkan = nd.getLokasiKantor(setPengirim);
           


            Table tabledalem = new Table(new float[] { 23, 300, 400 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.CENTER).SetBorder(Border.NO_BORDER).SetMarginTop(10);
            Cell celldalem = new Cell(1, 1).Add(new Paragraph("Yth.").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            tabledalem.AddCell(celldalem);
            Table tabletujuan = kop.DrawTujuan(dokumen.Tujuan, pathx);
            celldalem = new Cell(1, 1).Add(tabletujuan).SetPadding(0).SetMargin(0).SetBorder(Border.NO_BORDER);
            tabledalem.AddCell(celldalem);
            kop.LokasiTanggal(
               tabledalem,
               celldalem,
               styledalem,
               lokasiditetapkan,
               dokumen.TanggalSurat,
               pathx: pathx
               );



            _cell = new Cell().Add(tabledalem).SetBorder(Border.NO_BORDER).SetMargin(0).SetPadding(0);
            _table.AddCell(_cell);
            //_cell = new Cell().Add(new Paragraph("di Tempat").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            //_table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER);
            _cell.SetMargin(0).SetPaddingBottom(15);
            _table.AddCell(_cell);



            _cell = new Cell().Add(new Paragraph("SURAT PENGANTAR").SetMultipliedLeading(1.0f).SetFontSize(11).SetFont(fontBokos).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
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


            string isisurat;
            isisurat = HttpUtility.UrlDecode(dokumen.IsiSurat);
            isisurat = isisurat.Replace("<div", "<p");
            isisurat = isisurat.Replace("</div>", "</p>");
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
            _cell.SetPaddingBottom(7f);
            _table.AddCell(_cell);

            Table bottomtable = new Table(new float[] { 300, 423 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.CENTER).SetBorder(Border.NO_BORDER).SetMarginTop(10);
            Cell cellbottom = new Cell(1, 1).Add(new Paragraph("Yth.").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            kop.TandaTerima(
                 bottomtable,
                 cellbottom,
                 styledalem
                 );

            kop.PenandatanganSection(
                     bottomtable, cellbottom, styledalem, dokumen, tipekantorid.ToString()
                 );


            _cell = new Cell().Add(bottomtable).SetBorder(Border.NO_BORDER).SetMargin(0).SetPadding(0);
            _table.AddCell(_cell);
                        

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


            byte[] byteArray = kop.polish(ms.ToArray(),fontBokos, dokumen.DraftCode, pathx);

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;


            return mss;
        }
    }
}