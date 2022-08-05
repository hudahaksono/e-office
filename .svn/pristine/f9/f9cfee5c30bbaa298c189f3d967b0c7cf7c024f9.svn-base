using iText.Html2pdf;
using iText.IO.Font;
using iText.IO.Image;
using iText.IO.Source;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Extgstate;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Surat.Models;
using Surat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Surat.Codes.TataNaskah
{
    public class KopSuratAlamat
    {
        DataMasterModel dataMasterModel = new DataMasterModel();
        internal void generate(Document doc, string pathx, KopSurat kopsurat, string kantorid, string unitkerjaid)
        {

            #region kop
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            if (string.IsNullOrEmpty(kopsurat.UnitKerjaId))
            {
                Kantor kantor = dataMasterModel.GetKantor(kantorid);
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                kopsurat.NamaKantor_L1 = (tipekantorid == 1) ? dataMasterModel.GetNamaUnitKerjaById(unitkerjaid).ToUpper() : kantor.NamaKantor.ToUpper();
                string L2 = "";
                if (tipekantorid == 3)
                {
                    string induk = dataMasterModel.GetKantorIdIndukFromKantorId(kantorid);
                    Kantor kantorInduk = dataMasterModel.GetKantor(induk);
                    L2 = kantorInduk.NamaKantor.Substring(14);
                } else if (tipekantorid == 2)
                {
                    L2 = kantor.NamaKantor.Substring(14);
                }
                
                kopsurat.NamaKantor_L2 = L2.ToUpper();
                kopsurat.Alamat = myTI.ToTitleCase(kantor.Alamat.ToLower());
                kopsurat.Telepon = kantor.Telepon;
                kopsurat.Email = kantor.Email;
                kopsurat.FontSize = 11;
            } 

            float[] columnWidths = { 85, 370, 68 };
            Table _table = new Table(UnitValue.CreatePercentArray(columnWidths)).SetPadding(0).SetMarginTop(-20); //table buat kop

            PdfFont fontkop = PdfFontFactory.CreateFont("Times-Bold");
            Style stylekop = new Style()
                .SetFont(fontkop)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingBottom(0f)
                .SetMarginBottom(0f);

            Cell _cell = new Cell(2, 0).Add(new Image(ImageDataFactory.Create(pathx + "/logobpn.png")).ScaleToFit(80, 78).SetHorizontalAlignment(HorizontalAlignment.RIGHT));
            _cell.SetPadding(0f).SetMargin(0f)
                .SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("KEMENTERIAN AGRARIA DAN TATA RUANG/\nBADAN PERTANAHAN NASIONAL").AddStyle(stylekop).SetMultipliedLeading(1.0f)
                .SetFontSize(17));
            _cell.SetPaddingTop(6f).SetMargin(0f)
                .SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);
            _cell = new Cell(2, 0).Add(new Paragraph("").AddStyle(stylekop));
            _cell.SetPadding(0f).SetMargin(0f)
                .SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);
            if (tipekantorid == 3)
            {
                _cell = new Cell().Add(new Paragraph((string.IsNullOrEmpty(kopsurat.NamaKantor_L1) ? "" : kopsurat.NamaKantor_L1.ToUpper()) + "\n" + (string.IsNullOrEmpty(kopsurat.NamaKantor_L2) ? "" : kopsurat.NamaKantor_L2.ToUpper())).AddStyle(stylekop).SetMultipliedLeading(1.0f)
                .SetFontSize(12));
            }
            else
            {
                _cell = new Cell().Add(new Paragraph((string.IsNullOrEmpty(kopsurat.NamaKantor_L1) ? "" : kopsurat.NamaKantor_L1.ToUpper()) + "\n" + (string.IsNullOrEmpty(kopsurat.NamaKantor_L2) ? "" : kopsurat.NamaKantor_L2.ToUpper())).AddStyle(stylekop).SetMultipliedLeading(1.0f)
                    .SetFontSize(15));

            }
            _cell.SetPadding(-1f).SetMargin(0f)
                .SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);


            FontProgram fontProgramArial = FontProgramFactory.CreateFont(pathx + "\\pdf\\fonts\\ARIALN.TTF");
            PdfFont fontArial = PdfFontFactory.CreateFont(fontProgramArial, PdfEncodings.WINANSI, true);

            FontProgram fontProgramAriali = FontProgramFactory.CreateFont(pathx + "\\pdf/fonts/ARIALNI.TTF");
            PdfFont fontAriali = PdfFontFactory.CreateFont(fontProgramAriali, PdfEncodings.WINANSI, true);

            Paragraph P = new Paragraph();
            P.Add(new Text(kopsurat.Alamat).SetFont(fontArial).SetFontSize(11));
            string telepon = string.IsNullOrEmpty(kopsurat.Telepon) ? "" : " Telepon: " + kopsurat.Telepon;
            P.Add(new Text(telepon).SetFont(fontArial).SetFontSize(11));
            string email = string.IsNullOrEmpty(kopsurat.Email) ? "" : " email: " + kopsurat.Email;
            P.Add(new Text(email).SetFont(fontAriali).SetFontSize(11));
            _cell = new Cell(0, 3).Add(P).SetTextAlignment(TextAlignment.CENTER);
            _cell.SetPaddingTop(0).SetPaddingBottom(0).SetMargin(0f).SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(0.6f));
            _table.AddCell(_cell);
            doc.Add(_table);

            #endregion
        }

        internal void garuda(Document doc, string pathx, KopSurat kopsurat, string kantorid, string unitkerjaid)
        {

            #region kop
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            if (string.IsNullOrEmpty(kopsurat.UnitKerjaId))
            {
                Kantor kantor = dataMasterModel.GetKantor(kantorid);
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                kopsurat.NamaKantor_L1 = (tipekantorid == 1) ? dataMasterModel.GetNamaUnitKerjaById(unitkerjaid).ToUpper() : kantor.NamaKantor.ToUpper();
                string L2 = "";
                if (tipekantorid == 3)
                {
                    string induk = dataMasterModel.GetKantorIdIndukFromKantorId(kantorid);
                    Kantor kantorInduk = dataMasterModel.GetKantor(induk);
                    L2 = kantorInduk.NamaKantor.Substring(14);
                }
                else if (tipekantorid == 2)
                {
                    L2 = kantor.NamaKantor.Substring(14);
                }

                kopsurat.NamaKantor_L2 = L2.ToUpper();
                kopsurat.Alamat = myTI.ToTitleCase(kantor.Alamat.ToLower());
                kopsurat.Telepon = kantor.Telepon;
                kopsurat.Email = kantor.Email;
                kopsurat.FontSize = 11;
            }

            float[] columnWidths = { 523 };
            Table _table = new Table(columnWidths).SetPadding(0).SetMarginTop(-20).UseAllAvailableWidth(); //table buat kop

            PdfFont fontkop = PdfFontFactory.CreateFont("Times-Bold");
            Style stylekop = new Style()
                .SetFont(fontkop)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingBottom(0f)
                .SetMarginBottom(0f);

            Cell _cell = new Cell().Add(new Image(ImageDataFactory.Create(pathx + "/garudaemas.png")).ScaleToFit(80, 78).SetHorizontalAlignment(HorizontalAlignment.CENTER));
            _cell.SetPadding(0f).SetMargin(0f).SetBorder(Border.NO_BORDER).SetPaddingBottom(10);
            _table.AddCell(_cell);
            _cell = new Cell().Add(new Paragraph("MENTERI AGRARIA DAN TATA RUANG/\nKEPALA BADAN PERTANAHAN NASIONAL").AddStyle(stylekop).SetMultipliedLeading(1.0f)
                .SetFontSize(17));
            _cell.SetBorder(Border.NO_BORDER).SetPaddingBottom(10);
            _table.AddCell(_cell);
            doc.Add(_table);


            #endregion
        }
        //Surat Pengantar Tanda Terima SK
        internal void TandaTerima(Table _table, Cell _cell, Style style, bool isyth = false, string pathx = null)
        {
            Table leftCol = new Table(new float[] { 200, 10, 233 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER);

            Cell leftCell = new Cell().Add(new Paragraph("Diterima Tanggal").AddStyle(style).SetMultipliedLeading(1.0f)).SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("Penerima").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("Nama").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("Tanda Tangan").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("No. Telephone").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(" ").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);



            //add left col
            _cell = new Cell().Add(leftCol).SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);
        }
        //Surat Pengantar Tanda Terima SK
        internal void LokasiTanggal(Table _table, Cell _cell, Style style, string lokasi, string tanggalsurat = null, bool isyth = false, string pathx = null)
        {

            Table tableHeader = new Table(new float[] { 523 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetMarginTop(10);
            Cell cellHeader;
            Table leftCol = new Table(new float[] { 80, 10, 233 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER);
            //Cell leftCell = new Cell().Add(new Paragraph("Yth.").AddStyle(style).SetMultipliedLeading(1.0f)).SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            //leftCol.AddCell(leftCell);


            //add left col
            //cellHeader = new Cell().Add(leftCol).SetBorder(Border.NO_BORDER);
            //tableHeader.AddCell(cellHeader);


            if (!string.IsNullOrEmpty(tanggalsurat))
            {
                try
                {
                    DateTime dtraw = DateTime.ParseExact(tanggalsurat, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    tanggalsurat = dtraw.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"));
                }
                catch
                {

                }
            }
            Paragraph N = new Paragraph();
            N.Add(new Text($"{new CultureInfo("en-US").TextInfo.ToTitleCase(lokasi.ToLower())}, ")).SetMultipliedLeading(1.0f).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER);
            if (string.IsNullOrEmpty(tanggalsurat))
            {
                N.Add(new Text("_________________").SetFontColor(ColorConstants.WHITE)).SetMultipliedLeading(1.0f).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER);
            }
            else
            {
                N.Add(new Text(tanggalsurat)).SetMultipliedLeading(1.0f).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER);
            }

            cellHeader = new Cell().Add(N.AddStyle(style).SetMultipliedLeading(1.0f).SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(-10f)).SetBorder(Border.NO_BORDER);
            tableHeader.AddCell(cellHeader);


            _cell = new Cell().Add(tableHeader).SetPadding(0).SetMargin(0).SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);

        }
        internal void generateHeaderKorespondensi(Table _table, Cell _cell, Style style, string lokasi, string sifat, string lampirantxt, string prihal, string nomorsurat = null, string tanggalsurat = null, bool isyth = false, string pathx = null)
        {
            Table tableHeader = new Table(new float[] { 323, 200 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetMarginTop(10);
            Cell cellHeader;

            Table leftCol = new Table(new float[] { 80, 10, 400 }).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER);
            
            Cell leftCell = new Cell().Add(new Paragraph("Nomor").AddStyle(style).SetMultipliedLeading(1.0f)).SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(string.IsNullOrEmpty(nomorsurat) ? "" : nomorsurat).AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("Sifat").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(string.IsNullOrEmpty(sifat) ? "<<Biasa>>" : sifat).AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("Lampiran").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(string.IsNullOrEmpty(lampirantxt) ? "-" : lampirantxt).AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph("Hal").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            leftCol.AddCell(leftCell);
            leftCell = new Cell().Add(new Paragraph(":").AddStyle(style).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            leftCol.AddCell(leftCell);

            var isiperihal = $"<link rel='stylesheet' type='text/css' href='{pathx}/pdf/PdfStyle.css'><span style='font-size:11pt;display:inline-block;'>{HttpUtility.UrlDecode(HttpUtility.UrlDecode(prihal))}</span>";
            foreach (IElement element in HtmlConverter.ConvertToElements(isiperihal))
            {
                leftCell = new Cell().Add((IBlockElement)element);
                leftCell.AddStyle(style).SetBorder(Border.NO_BORDER);
                leftCol.AddCell(leftCell);
            }

            //add left col
            cellHeader = new Cell().Add(leftCol).SetBorder(Border.NO_BORDER);
            tableHeader.AddCell(cellHeader);

            _cell = new Cell().Add(tableHeader).SetBorder(Border.NO_BORDER).SetPadding(0).SetMargin(0);
            _table.AddCell(_cell);
        }

        internal byte[] LokasiTanggal(byte[] byteArray, PdfFont fontBokos, string tanggal, string lokasi, string pathx)
        {


            IRandomAccessSource source = new RandomAccessSourceFactory().CreateSource(byteArray);
            MemoryStream baos = new MemoryStream();
            var reader = new PdfReader(source, new ReaderProperties());

            var writer = new PdfWriter(baos);

            PdfDocument pdfDoc = new PdfDocument(reader, writer);
            PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
            Document doc2 = new Document(pdfDoc);
            int numberOfPages = pdfDoc.GetNumberOfPages();
            var wi = pdfDoc.GetPage(1).GetPageSize().GetWidth();
            var hi = pdfDoc.GetPage(1).GetPageSize().GetHeight();
            for (int page = 1; page == 1; page++)
            {
                if (page == 1)
                {
                    PdfCanvas pdfCanvas = new PdfCanvas(pdfDoc, page);
                    Rectangle rectangle = new Rectangle(130, hi - 169, 400, 30);
                    Canvas can = new Canvas(pdfCanvas, rectangle);
                    Table t = new Table(new float[] { 400 });

                    string extra = string.IsNullOrEmpty(tanggal) ? "____________" : tanggal;
                    lokasi = new CultureInfo("en-US").TextInfo.ToTitleCase(lokasi.ToLower());

                    string cont = $"<p id='katasambung'>{lokasi}, {extra}</p>";
                    string keterangan = "<link rel='stylesheet' type='text/css' href='" + pathx + "/pdf/PdfStyle.css'>" + cont;
                    foreach (IElement element in HtmlConverter.ConvertToElements(keterangan))
                    {
                        Cell c = new Cell().Add((IBlockElement)element).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);
                        t.AddCell(c);
                    }
                    can.Add(t);
                    can.Close();
                }

            }

            writer.SetCloseStream(false);
            doc2.Close();

            byte[] byteResult = baos.ToArray();

            return byteResult;
        }

        internal void PenandatanganSection(
            Table _table, Cell _cell, Style styledalem, DraftSurat dokumen, string kantorid
            )
        {
            var tabledalem = new Table(new float[] { 250, 273 }).UseAllAvailableWidth();
            if (dokumen.TipeSurat == "Surat Pengantar")
            {
                tabledalem = new Table(new float[] { 90, 273 }).UseAllAvailableWidth();
            }
            var celldalem = new Cell(10, 1).Add(new Paragraph("").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
            tabledalem.AddCell(celldalem);

            if (!string.IsNullOrEmpty(dokumen.TanggalSurat))
            {
                try
                {
                    DateTime dtraw = DateTime.ParseExact(dokumen.TanggalSurat, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    dokumen.TanggalSurat = dtraw.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"));
                }
                catch
                {
                    
                }
            }
            var dataTTE = new NaskahDinasModel().GetUserTtd(dokumen.DraftCode).Find(x => x.Tipe == "1");
            var penandatangan = dataMasterModel.GetPegawaiByProfileId(dokumen.ProfilePengirim).Find(x => x.PegawaiId == dataTTE.PegawaiId);
            dokumen.Pengirim = string.IsNullOrEmpty(dokumen.Pengirim)? dataMasterModel.GetProfileNameFromId(dokumen.ProfilePengirim) : dokumen.Pengirim;
            bool plt = new NaskahDinasModel().CheckIsPLT(penandatangan.PegawaiId, dokumen.ProfilePengirim);
            bool plh = new NaskahDinasModel().CheckIsPLH(penandatangan.PegawaiId, dokumen.ProfilePengirim);
            if (plt)
            {
                dokumen.Pengirim = "Plt. " + dokumen.Pengirim;
            }
            if (plh)
            {
                dokumen.Pengirim = "Plh. " + dokumen.Pengirim;
            }

            string namajabatan = "";
            var anjbt = "";
            string nama = "";
            
            if (dokumen.TanpaGelar)
            {
                nama = penandatangan.Nama;
            }
            else
            {
                nama = penandatangan.NamaLengkap;
            }
            string nip = penandatangan.PegawaiId;

            if (!string.IsNullOrEmpty(dokumen.AtasNama))
            {
                anjbt = new DataMasterModel().GetProfileNameFromId(dokumen.AtasNama);
                if (anjbt.ToUpper() == "MENTERI AGRARIA DAN TATA RUANG/KEPALA BADAN PERTANAHAN NASIONAL")
                {
                    anjbt = "Menteri Agraria dan Tata Ruang/\nKepala Badan Pertanahan Nasional";
                }
                else if (anjbt.Contains("Kepala Kantor Wilayah BPN"))
                {
                    anjbt = anjbt.Replace("Kepala Kantor Wilayah BPN", "Kepala Kantor Wilayah\n Badan Pertanahan Nasional");
                }
                else if (anjbt.Contains("Kepala Kantor Pertanahan"))
                {
                    anjbt = anjbt.Replace("Kepala Kantor Pertanahan", "Kepala Kantor Pertanahan\n");
                }


            }

            if (!string.IsNullOrEmpty(dokumen.JabatanAdhoc))
            {
                namajabatan = string.IsNullOrEmpty(dokumen.JabatanAdhoc.Trim()) ? dokumen.Pengirim : dokumen.JabatanAdhoc;
            }
            else
            {
                if (dokumen.Pengirim.ToUpper() == "MENTERI AGRARIA DAN TATA RUANG/KEPALA BADAN PERTANAHAN NASIONAL")
                {
                    namajabatan = "Menteri Agraria dan Tata Ruang/\nKepala Badan Pertanahan Nasional";
                }
                else if(dokumen.Pengirim == "Kepala Kantor Pertanahan")
                {
                    namajabatan = "Kepala Kantor Pertanahan\n" + dataMasterModel.GetNamaUnitKerjaById(dokumen.UnitKerjaId);
                }
                else if (dokumen.Pengirim.Contains("Kepala Kantor Pertanahan"))
                {
                    namajabatan = dokumen.Pengirim.Replace("Kepala Kantor Pertanahan", "Kepala Kantor Pertanahan\n");
                }
                else if (dokumen.Pengirim.Contains("Kepala Kantor Wilayah BPN"))
                {
                    namajabatan = dokumen.Pengirim.Replace("Kepala Kantor Wilayah BPN", "Kepala Kantor Wilayah\n Badan Pertanahan Nasional\n");
                }
                else
                {
                    namajabatan = dokumen.Pengirim;
                }
            }

            string[] ex = { "Surat Keputusan", "Surat Pernyataan", "Pengumuman", "Surat Edaran", "Surat Keterangan", "Surat Tugas", "Surat Perintah" };
            if (ex.Contains(dokumen.TipeSurat))
            {
                var setPengirim = string.IsNullOrEmpty(dokumen.AtasNama) ? dokumen.ProfilePengirim : dokumen.AtasNama;
                var lokasiditetapkan = new NaskahDinasModel().getLokasiKantor(setPengirim);

                if (dokumen.TipeSurat != "Surat Pernyataan" && dokumen.TipeSurat != "Pengumuman" && dokumen.TipeSurat != "Surat Keterangan" && dokumen.TipeSurat != "Surat Tugas" && dokumen.TipeSurat != "Surat Perintah")
                {
                    celldalem = new Cell(1, 1).Add(new Paragraph($"Ditetapkan di {new CultureInfo("en-US").TextInfo.ToTitleCase(lokasiditetapkan.ToLower())}").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
                    tabledalem.AddCell(celldalem);
                }
                if (dokumen.TipeSurat == "Pengumuman")
                {
                    celldalem = new Cell(1, 1).Add(new Paragraph($"Dikeluarkan di {new CultureInfo("en-US").TextInfo.ToTitleCase(lokasiditetapkan.ToLower())}").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
                    tabledalem.AddCell(celldalem);
                }

                var tanggalsuratTXT = "";
                if (!string.IsNullOrEmpty(dokumen.TanggalSurat))
                {
                    try
                    {
                        DateTime dtraw = DateTime.ParseExact(dokumen.TanggalSurat, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        tanggalsuratTXT = dtraw.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"));
                    }
                    catch
                    {
                        tanggalsuratTXT = dokumen.TanggalSurat;
                    }
                }
                else
                {
                    tanggalsuratTXT = "...............";
                }

                if (dokumen.TipeSurat == "Surat Keputusan" || dokumen.TipeSurat == "Pengumuman" || dokumen.TipeSurat == "Surat Edaran")
                {
                    celldalem = new Cell(1, 1).Add(new Paragraph($"pada tanggal {tanggalsuratTXT}").AddStyle(styledalem).SetMultipliedLeading(1.0f)).SetBorder(Border.NO_BORDER);
                    tabledalem.AddCell(celldalem);
                }
                else
                {
                    celldalem = new Cell(1, 1).Add(new Paragraph($"{new CultureInfo("en-US").TextInfo.ToTitleCase(lokasiditetapkan.ToLower())}, {tanggalsuratTXT}").AddStyle(styledalem).SetMultipliedLeading(1.0f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
                    tabledalem.AddCell(celldalem);
                }

                celldalem = new Cell(1, 1).Add(new Paragraph("").AddStyle(styledalem).SetMultipliedLeading(1.0f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
                tabledalem.AddCell(celldalem);

                if (dokumen.TipeSurat == "Surat Keputusan" || dokumen.TipeSurat == "Surat Edaran")
                {
                    namajabatan = namajabatan.ToUpper();
                    nama = nama.ToUpper();
                    if (!string.IsNullOrEmpty(dokumen.AtasNama))
                    {
                        anjbt = anjbt.ToUpper();
                    }
                }
            }
            Color SignColor = new DeviceCmyk(0, 0, 0, 35);

            celldalem = new Cell(1, 1).Add(new Paragraph(string.IsNullOrEmpty(dokumen.AtasNama) ? $"{namajabatan}," : $"a.n. {anjbt}\n{namajabatan},").AddStyle(styledalem).SetMultipliedLeading(1.0f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            tabledalem.AddCell(celldalem);
            celldalem = new Cell(1, 1).Add(new Paragraph(dokumen.DraftCode).AddStyle(styledalem).SetMultipliedLeading(.5f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER).SetFontColor(ColorConstants.WHITE);
            tabledalem.AddCell(celldalem);
            celldalem = new Cell(1, 1).Add(new Paragraph("Ditandatangani Secara\nElektronik").AddStyle(styledalem).SetMultipliedLeading(1f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER).SetFontColor(SignColor);
            tabledalem.AddCell(celldalem);
            celldalem = new Cell(1, 1).Add(new Paragraph("").AddStyle(styledalem).SetMultipliedLeading(.5f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER).SetFontColor(ColorConstants.WHITE);
            tabledalem.AddCell(celldalem);
            //if (!dokumen.TandaTanganBasah && !string.IsNullOrEmpty(dokumen.NomorSurat) && !string.IsNullOrEmpty(dokumen.TanggalSurat))
            //{
            //    tabledalem.AddCell(celldalem);
            //}
            //else
            //{
            //    tabledalem.AddCell(celldalem.SetPaddingBottom(23).SetPaddingTop(23));
            //}

            celldalem = new Cell(1, 1).Add(new Paragraph(nama).AddStyle(styledalem).SetMultipliedLeading(1.0f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
            tabledalem.AddCell(celldalem);
            if (nip.Length > 12)
            {
                celldalem = new Cell(1, 1).Add(new Paragraph("NIP " + nip).AddStyle(styledalem).SetMultipliedLeading(1.0f).SetTextAlignment(TextAlignment.CENTER)).SetBorder(Border.NO_BORDER);
                tabledalem.AddCell(celldalem);
            }

            _cell = new Cell(1, 1).Add(tabledalem).SetBorder(Border.NO_BORDER);
            _table.AddCell(_cell);
        }

        internal string DrawTembusan(List<string> listTembusan, string pathx)
        {
            string Ftembusan = "";
            if (listTembusan.Count() < 1)
            {
                return Ftembusan;
            } else
            {
                Ftembusan = "<link rel='stylesheet' type='text/css' href='" + pathx + "/pdf/PdfStyle.css'>";
                if (listTembusan.Count() == 1)
                {
                    string[] lstr = listTembusan[0].Split('%');
                    string lokasi = string.Empty;
                    if (lstr[1] != "freetxt")
                    {
                        lokasi = new NaskahDinasModel().getLokasiKantor(lstr[1]);
                    }                    
                    lstr[0] = lstr[0].Contains("Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional") ? "Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional" : lstr[0];
                    Ftembusan = $"<link rel='stylesheet' type='text/css' href='{pathx}/pdf/PdfStyle.css'><span style='font-size:11pt;'>{lstr[0].Replace("(PLT)","").Trim()}, di {(string.IsNullOrEmpty(lokasi) ? "Tempat." : $"{new CultureInfo("en-US").TextInfo.ToTitleCase(lokasi.ToLower())}.")}</span>";
                }
                else
                {
                    Ftembusan += "<ol>";
                    int n = 1;
                    int l = listTembusan.Count();
                    foreach (var str in listTembusan)
                    {
                        string[] lstr = str.Split('%');
                        string lokasi = string.Empty;
                        if (lstr[1] != "freetxt")
                        {
                            lokasi = new NaskahDinasModel().getLokasiKantor(lstr[1]);
                        }
                        string strEnd = (n == l) ? "." : ";";
                        lstr[0] = lstr[0].Contains("Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional") ? "Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional" : lstr[0];
                        Ftembusan += (string.IsNullOrEmpty(Ftembusan)) ? "<li>" : "</li><li>";
                        Ftembusan += string.Concat(lstr[0].Replace("(PLT)", "").Trim(), $", di {(string.IsNullOrEmpty(lokasi) ? $"Tempat{strEnd}" : $"{new CultureInfo("en-US").TextInfo.ToTitleCase(lokasi.ToLower())}{strEnd}")}");
                        n++;
                    }
                }

                return Ftembusan;
            }
        }

        internal Table DrawTujuan(List<string> listTujuan, string pathx) {
            Table tabletujuan = new Table(1);
            Cell celltujuan;
            string Ftujuan = "";
            if (listTujuan.Count() > 0)
            {
                Ftujuan += "<link rel='stylesheet' type='text/css' href='" + pathx + "/pdf/PdfStyle.css'>";
                if (listTujuan.Count() == 1)
                {
                    string[] lstr = listTujuan[0].Split('%');
                    if(lstr[1] != "freetxt" && lstr[0].Contains(" (PLT)"))
                    {
                        lstr[0] = lstr[0].Replace(" (PLT)","");
                    }
                    Ftujuan = $"<link rel='stylesheet' type='text/css' href='{pathx}/pdf/PdfStyle.css'><div style='font-size:11pt;margin-left:-2px;'><span>{lstr[0]}</span><div>";

                    foreach (IElement element in HtmlConverter.ConvertToElements(Ftujuan))
                    {
                        celltujuan = new Cell().Add((IBlockElement)element);
                        celltujuan.SetPadding(1).SetMargin(0).SetBorder(Border.NO_BORDER);
                        tabletujuan.AddCell(celltujuan);
                    }
                }
                else
                {
                    Ftujuan += "<ol style='margin-bottom:0;'>";
                    int cnt = 1;
                    foreach (var str in listTujuan)
                    {
                        string[] lstr = str.Split('%');
                        if (lstr[1] != "freetxt" && lstr[0].Contains(" (PLT)"))
                        {
                            lstr[0] = lstr[0].Replace(" (PLT)", "");
                        }
                        Ftujuan += (string.IsNullOrEmpty(Ftujuan)) ? "<li>" : "</li><li>";
                        Ftujuan += lstr[0];
                        Ftujuan += (cnt < listTujuan.Count()) ? ";" : ".";
                        cnt++;
                    }
                    Ftujuan += "</ol>";

                    foreach (IElement element in HtmlConverter.ConvertToElements(Ftujuan))
                    {
                        celltujuan = new Cell().Add((IBlockElement)element);
                        celltujuan.SetMargin(0).SetBorder(Border.NO_BORDER);
                        tabletujuan.AddCell(celltujuan);
                    }
                }
            }           

            return tabletujuan;
        }

        internal byte[] polish(byte[] byteArray, PdfFont fontBokos, string id, string pathx, bool watermark = false)
        {
            IRandomAccessSource source = new RandomAccessSourceFactory().CreateSource(byteArray);
            MemoryStream baos = new MemoryStream();
            var reader = new PdfReader(source, new ReaderProperties());

            var writer = new PdfWriter(baos);

            PdfDocument pdfDoc = new PdfDocument(reader, writer);
            PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
            Document doc2 = new Document(pdfDoc);
            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();           

            int numberOfPages = pdfDoc.GetNumberOfPages();
            var wi = pdfDoc.GetPage(1).GetPageSize().GetWidth();
            var hi = pdfDoc.GetPage(1).GetPageSize().GetHeight();
            int numpg = 100;
            bool akhirSurat = false;
            PdfExtGState gs1 = new PdfExtGState().SetFillOpacity(0.5f);

            for (int page = 1; page <= numberOfPages; page++)
            {
                pdfDoc.GetPage(page).SetIgnorePageRotationForContent(true);
                string check = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                if (check.Contains(id))
                {
                    akhirSurat = true;
                    numpg = page;
                }
                #region katasambung
                var nextPage = page + 1;
                if (nextPage <= numberOfPages && page < numpg && !akhirSurat)
                {
                    string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(nextPage));
                    var text = pageContent.Substring(0, 30);
                    text = text.Replace("\n", " ");
                    string[] split = text.Trim().Split(' ');

                    PdfCanvas pdfCanvas = new PdfCanvas(pdfDoc, page);
                    Rectangle rectangle = new Rectangle(330, 30, 200, 30);
                    Canvas can = new Canvas(pdfCanvas, rectangle);
                    Table t = new Table(new float[] {200});

                    string textsambung;
                    try
                    {
                        string txt = split[0];
                        txt = txt.Remove(txt.Length - 1);
                        int num = int.Parse(txt);
                        textsambung = $"{num}. {split[1]}";
                    }
                    catch
                    {
                        try
                        {
                            if (split[0].Trim().Count() == 2 && (split[0].Substring(1, split[0].Length - 1) == "." || split[0].Substring(1, split[0].Length - 1) == ")"))
                            {
                                textsambung = $"{split[0]} {split[1]}";
                            } else
                            {
                                textsambung = split[0];
                            }
                        }
                        catch
                        {
                            textsambung = split[0];
                        }                        
                    }

                    if(textsambung != id)
                    {
                        string cont = $"<span id='katasambung'>{textsambung}...</span>";
                        string keterangan = "<link rel='stylesheet' type='text/css' href='" + pathx + "/pdf/PdfStyle.css'>" + cont;
                        foreach (IElement element in HtmlConverter.ConvertToElements(keterangan))
                        {
                            Cell c = new Cell().Add((IBlockElement)element).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);
                            t.AddCell(c);
                        }
                        can.Add(t);
                    }
                    
                    can.Close();

                }
                #endregion

                #region nomorhalaman
                if (page > 1 && page <= numpg )
                {
                    PdfCanvas pdfCanvas = new PdfCanvas(pdfDoc, page);
                    Rectangle rectangle = new Rectangle(290, hi-42 , 570, 30);
                    Canvas can = new Canvas(pdfCanvas, rectangle);
                    Table t = new Table(1).SetBorder(Border.NO_BORDER);

                    string cont = $"<span id='nomorhal'>-{page.ToString()}-</span>";
                    string keterangan = "<link rel='stylesheet' type='text/css' href='" + pathx + "/pdf/PdfStyle.css'>" + cont;
                    foreach (IElement element in HtmlConverter.ConvertToElements(keterangan))
                    {
                        Cell c = new Cell().SetBorder(Border.NO_BORDER).Add((IBlockElement)element);
                        t.AddCell(c);
                    }
                    can.Add(t);
                    can.Close();
                }

                #endregion
            }

            writer.SetCloseStream(false);
            doc2.Close();

            byte[] byteResult = baos.ToArray();

            return byteResult;
        }
    }
    public class FooterEvent : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            int pageNum = docEvent.GetDocument().GetPageNumber(page);
            FontProgram fontProgramfree = FontProgramFactory.CreateFont(Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\FREESCPT.TTF");
            PdfFont fontfree = PdfFontFactory.CreateFont(fontProgramfree, PdfEncodings.IDENTITY_H, true);
            Color mycolor = new DeviceRgb(22, 218, 249);


            if (pageNum > 1) return;
            Rectangle rootarea = new Rectangle(235, -7, 500, 50);
            Canvas canvas = new Canvas(page, rootarea);
            canvas.Add(new Paragraph("Melayani, Profesional, Terpercaya").SetFont(fontfree).SetFontSize(14).SetFontColor(mycolor)).Close();

        }

    }

    public class AlamatBottom : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            int pageNum = docEvent.GetDocument().GetPageNumber(page);


            if (pageNum > 1) return;
            Rectangle rootarea = new Rectangle(35, -11, 523, 50);
            Canvas canvas = new Canvas(page, rootarea);

            FontProgram fontProgramArial = FontProgramFactory.CreateFont(Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\ARIALN.TTF");
            PdfFont fontArial = PdfFontFactory.CreateFont(fontProgramArial, PdfEncodings.WINANSI, true);

            FontProgram fontProgramAriali = FontProgramFactory.CreateFont(Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\ARIALNI.TTF");
            PdfFont fontAriali = PdfFontFactory.CreateFont(fontProgramAriali, PdfEncodings.WINANSI, true);

            Paragraph P = new Paragraph();
            P.Add(new Text("Jalan Sisingamangaraja Nomor 2 Jakarta Selatan 12014 Kotak Pos 1403 Telepon: 7228901, 7393939 email : ").SetFont(fontArial).SetFontSize(11));
            P.Add(new Text("surat@atrbpn.go.id").SetFont(fontAriali).SetFontSize(11));

            canvas.Add(P.SetFontSize(14)).Close();

        }
    }
}