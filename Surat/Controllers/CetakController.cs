﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Surat.Codes;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class CetakController : Controller
    {
        Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.KontentModel kontentm = new Models.KontentModel();
        Functions functions = new Functions();

        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);


        public ActionResult LaporanSuratMasuk()
        {
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        public ActionResult LaporanSuratKeluar()
        {
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }


        public ActionResult ViewPdf_LembarDisposisi(string suratid)
        {
            var result = new { Status = false, Message = "" };

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;

            if (pegawaiid == "342440001")
            {
                return ViewPdf_LembarDisposisi_Menteri(suratid, "H0000001");
                //return ViewPdf_LembarDisposisi_Setjen(suratid, "H0000001");
            }

            List<Models.Entities.Profile> profileDisposisi = dataMasterModel.GetProfileDisposisiByProfileId(pegawaiid, kantorid, false);
            if (profileDisposisi.Count > 0)
            {
                string profileid = profileDisposisi[0].ProfileId;

                if (profileid == "H0000003")
                {
                    return ViewPdf_LembarDisposisi_Setjen(suratid, profileid);
                }
                else if (profileid == "H0000001" || profileid == "H0000002")
                {
                    return ViewPdf_LembarDisposisi_Menteri(suratid, profileid);
                }
                else
                {
                    return ViewPdf_LembarDisposisi_Satker(suratid, profileid);
                }
            }
            else
            {
                string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");
                return ViewPdf_LembarDisposisi_Satker(suratid, myProfileId);
            }

            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewPdf_LembarDisposisi_Setjen(string suratid, string pejabatdisposisi)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(pejabatdisposisi);

            string namapejabatdisposisi = "";
            if (listPegawai.Count > 0)
            {
                namapejabatdisposisi = listPegawai[0].NamaLengkap;
            }

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
            surat.NomorSurat = string.IsNullOrEmpty(surat.NomorSurat) ? surat.NomorSurat : HttpUtility.UrlDecode(surat.NomorSurat);
            surat.NamaPengirim = string.IsNullOrEmpty(surat.NamaPengirim) ? surat.NamaPengirim : HttpUtility.UrlDecode(surat.NamaPengirim);
            surat.NamaPegawai = string.IsNullOrEmpty(surat.NamaPegawai) ? surat.NamaPegawai : HttpUtility.UrlDecode(surat.NamaPegawai);

            //List<Models.Entities.TipeEselon> listTipeEselon = persuratanmodel.GetTipeEselonOnDisposisiSurat(suratid);


            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontBold = PdfUtil.GetArial(10f, Font.BOLD);
            Phrase phrase = new Phrase();

            iTextSharp.text.Font cellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10f, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font cellfontBold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10f, iTextSharp.text.Font.BOLD);


            // KOP SURAT
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            // Cell NAMA KANTOR
            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL", Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSuratUnderline("SEKRETARIS JENDERAL", Element.ALIGN_LEFT, 12f, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);


            // Cell LOGO
            cell = new PdfPCell();
            cell.Border = 0;
            //cell.Rowspan = 3;
            Image image = Image.GetInstance(Server.MapPath("~/Reports/logobpn.png"));
            image.ScaleAbsolute(80, 78);
            cell.AddElement(image);
            table.AddCell(cell);

            columnWidths = new float[] { 620f, 150f };
            table.SetWidths(columnWidths);

            doc.Add(table);


            // Horizontal Line
            //doc.Add(objPdf.HorizontalLine());


            // TITLE
            string titlenamadokumen = "LEMBAR DISPOSISI";
            doc.Add(objPdf.AddTitleSuratUnderline(titlenamadokumen, Element.ALIGN_CENTER, 12f, 0f, 0f, 0f));
            // Eof TITLE


            //----- BODY --------------------------------------------------

            // line separator
            doc.Add(objPdf.AddLineSeparator(20f));


            doc.Add(objPdf.AddParagraph(10f, 10f, 0f, Font.BOLD, 0, 0, 0, 0, Element.ALIGN_LEFT, "AGENDA NO : " + surat.NomorAgendaSurat, ""));

            // line separator
            doc.Add(objPdf.AddLineSeparator(5f));


            // Surat Dari dan Tanggal Surat -----------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Surat Dari", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.PengirimSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 240f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Surat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 100f, 10f, 210f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);

            // Nomor Surat dan Tanggal Terima -----------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Nomor Surat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.NomorSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 240f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Diterima", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalTerimaCetak, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 100f, 10f, 210f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);

            // Hal dan Resume Surat -----------------------

            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Hal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 10f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 564f, };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Resume", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.IsiSingkatSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 10f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 564f, };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);





            //table = new PdfPTable(2);
            //table.WidthPercentage = 100;

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Hal :" + surat.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            ////tableIn.AddCell(objPdf.CreateCellTable(surat.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 644f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Resume Surat :" + surat.IsiSingkatSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            ////tableIn.AddCell(objPdf.CreateCellTable(surat.IsiSingkatSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 644f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //columnWidths = new float[] { 322f, 322f };
            //table.SetWidths(columnWidths);
            //doc.Add(table);





            // --------------- Middle Header -------------------

            table = new PdfPTable(1);
            table.WidthPercentage = 100;
            table.AddCell(objPdf.CreateCellTable("Diteruskan Kepada", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.AddCell(objPdf.CreateCellTable("Pejabat Pimpinan Tingkat Madya:", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            table.AddCell(objPdf.CreateCellTable("Pejabat Pimpinan Tingkat Pratama:", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // ----------- LIST OF DISPOSISI ---------------------------

            PdfPCell checkBoxcell = new PdfPCell();
            checkBoxcell.VerticalAlignment = Element.ALIGN_TOP;
            checkBoxcell.HorizontalAlignment = Element.ALIGN_CENTER;
            checkBoxcell.Border = 0;
            checkBoxcell.PaddingTop = 4f;
            //checkBoxcell.FixedHeight = 20f;
            image = Image.GetInstance(Server.MapPath("~/Reports/checkbox.jpg"));
            image.ScaleAbsolute(8, 8);
            checkBoxcell.AddElement(image);

            // 1 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Inspektur Jenderal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Biro Perencanaan dan Kerja Sama", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 2 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Tata Ruang (Dirjen I)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Biro Organisasi dan Kepegawaian", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 3 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Survei dan Pemetaan Pertanahan dan Ruang (Dirjen II)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Biro Keuangan dan BMN", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 4 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Penetapan Hak dan Pendaftaran Tanah (Dirjen III)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Biro Hukum", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 5 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Penataan Agraria (Dirjen IV)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Biro Hubungan Masyarakat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 6 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Pengadaan Tanah dan Pengembangan Pertanahan (Dirjen V)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Biro Umum dan Layanan Pengadaan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 7 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Pengendalian dan Penertiban Tanah dan Ruang (Dirjen VI)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Pusat Data dan Informasi Pertanahan, Tata Ruang dan LPPB", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 8 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Direktur Jenderal Penanganan Sengketa dan Konflik Pertanahan (Dirjen VII)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Pusat Pengembangan Sumber Daya Manusia", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 9 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            table.AddCell(objPdf.CreateCellTable("Staf Ahli", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Pusat Pengembangan dan Standarisasi Kebijakan Agraria, Tata Ruang, dan Pertanahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 10 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Ahli Hukum Agraria dan Masyarakat Adat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Ketua STPN", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 11 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Ahli Bidang Reformasi Birokrasi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Kanwil BPN Provinsi: ...", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 12 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Ahli Bidang Partisipasi Masyarakat dan Pemerintah Daerah", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Kepala Kantor Pertanahan Kab.Kota: ...", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 13 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Ahli Bidang Pengembangan Kawasan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Sekretariat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);






            // 13a ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Ahli Bidang Teknologi Informasi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;


            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 14 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            table.AddCell(objPdf.CreateCellTable("Staff Khusus", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            table.AddCell(objPdf.CreateCellTable("DISPOSISI", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);



            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 15 ------------------------------

            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Khusus Bidang Penanganan Sengketa dan Konflik Tanah dan Ruang", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Hadiri/Wakili & Lapor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Koordinasi & Selesaikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 161f, 161f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 16 ------------------------------

            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Khusus Bidang Pengembangan Teknologi Informasi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Dampingi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Tindak Lanjuti", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 161f, 161f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 17 ------------------------------

            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Khusus Bidang Manajemen Data", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Dijadwalkan/Catat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Surat Jawaban", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 161f, 161f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 18 ------------------------------

            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Khusus Bidang Kelembagaan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Menghadap Saya", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Untuk jadi Perhatian", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 161f, 161f };
            table.SetWidths(columnWidths);
            doc.Add(table);





            // 18a ------------------------------


            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Staf Khusus Bidang Hukum Adat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 305f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);


            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Pelajari dan Saran Pendapat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Monitor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 161f, 161f };
            table.SetWidths(columnWidths);
            doc.Add(table);




            // 19 ------------------------------

            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            cell = new PdfPCell(new Phrase("Tenaga Ahli Menteri (TAM)", cellfontBold));
            cell.Padding = 5f;
            cell.Rowspan = 3;
            table.AddCell(cell);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Laporan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Perbaikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Bahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("File", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 30f, 290f };
            tableIn.SetWidths(columnWidths);
            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 161f, 161f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // 19 ------------------------------

            //table = new PdfPTable(3);
            //table.WidthPercentage = 100;
            //table.AddCell(checkBoxcell);
            //table.AddCell(objPdf.CreateCellTable("Tenaga Ahli Menteri (TAM)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));

            //cell = new PdfPCell(new Phrase("", cellfontBold));
            //cell.Padding = 5f;
            //cell.Rowspan = 3;
            //table.AddCell(cell);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("Pelajari & Saran Pendapat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 30f, 290f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("Monitor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 30f, 290f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("Siapkan Laporan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 30f, 290f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("Perbaikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 30f, 290f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("Siapkan Bahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 30f, 290f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("File", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 30f, 290f };
            //tableIn.SetWidths(columnWidths);
            //table.AddCell(tableIn);

            //columnWidths = new float[] { 322f, 161f, 161f };
            //table.SetWidths(columnWidths);
            //doc.Add(table);

            /*

            // 14 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.DefaultCell.Padding = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));


            // Sub Perintah Disposisi
            tableSub = new PdfPTable(2);
            tableSub.WidthPercentage = 100;

            cell = new PdfPCell(new Phrase("DISPOSISI", cellfontBold));
            cell.Padding = 5f;
            cell.Colspan = 2;
            tableSub.AddCell(cell);

            //----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Hadiri/Wakili & Lapor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Koordinasi & Selesaikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Dampingi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Tindak Lanjuti", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Dijadwalkan/Catat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Surat Jawaban", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            columnWidths = new float[] { 160f, 160f };
            tableSub.SetWidths(columnWidths);

            table.AddCell(tableSub);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 15 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.DefaultCell.Padding = 0;

            table.AddCell(objPdf.CreateCellTable("Tenaga Ahli Menteri (TAM)", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));

            // Sub Perintah Disposisi
            tableSub = new PdfPTable(2);
            tableSub.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Menghadap Saya", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Untuk Jadi Perhatian", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            columnWidths = new float[] { 160f, 160f };
            tableSub.SetWidths(columnWidths);

            table.AddCell(tableSub);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // 16 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.DefaultCell.Padding = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));

            // Sub Perintah Disposisi
            tableSub = new PdfPTable(2);
            tableSub.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Pelajari & Saran Pendapat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Monitor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Laporan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Perbaiki", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Bahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("File", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            columnWidths = new float[] { 160f, 160f };
            tableSub.SetWidths(columnWidths);

            table.AddCell(tableSub);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);
            */


            // -------------- Catatan -------------------
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            // Catatan
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Catatan:", Element.ALIGN_LEFT, Element.ALIGN_TOP, 55f, 10f, Font.BOLD, true, ""));

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(namapejabatdisposisi, Element.ALIGN_RIGHT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, true, ""));

            tableSub.AddCell(tableIn);

            table.AddCell(tableSub);


            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("LembarDisposisi", ".pdf");

            return docfile;
        }

        public ActionResult ViewPdf_LembarDisposisi_Menteri(string suratid, string pejabatdisposisi)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(pejabatdisposisi);

            string namapejabatdisposisi = "";
            if (listPegawai.Count > 0)
            {
                namapejabatdisposisi = listPegawai[0].Nama;
            }

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            //Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
            Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratIdWithProfileId(suratid, pejabatdisposisi);
            surat.NomorSurat = string.IsNullOrEmpty(surat.NomorSurat) ? surat.NomorSurat : HttpUtility.UrlDecode(surat.NomorSurat);
            surat.NamaPengirim = string.IsNullOrEmpty(surat.NamaPengirim) ? surat.NamaPengirim : HttpUtility.UrlDecode(surat.NamaPengirim);
            surat.NamaPegawai = string.IsNullOrEmpty(surat.NamaPegawai) ? surat.NamaPegawai : HttpUtility.UrlDecode(surat.NamaPegawai);

            //List<Models.Entities.TipeEselon> listTipeEselon = persuratanmodel.GetTipeEselonOnDisposisiSurat(suratid);


            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.LEGAL, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontBold = PdfUtil.GetArial(10f, Font.BOLD);
            Phrase phrase = new Phrase();


            // KOP SURAT
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            string wakil = (pejabatdisposisi == "H0000002") ? "WAKIL " : "";

            cell.AddElement(objPdf.AddTitleSurat(wakil + "MENTERI AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(wakil + "KEPALA BADAN PERTANAHAN NASIONAL", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("LEMBAR EDARAN", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 5f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            //----- BODY --------------------------------------------------



            // TITLE INFO
            table = new PdfPTable(4);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("NOMOR AGENDA", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("TANGGAL SURAT", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("DITERIMA TANGGAL", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 9f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("TINGKAT SURAT", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 256f, 128f, 128f, 132f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // CONTENT INFO
            table = new PdfPTable(4);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(surat.NomorAgendaSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalTerima, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            string tingkatsurat = string.IsNullOrEmpty(surat.Redaksi) ? "" : surat.Redaksi.ToUpper();
            if (tingkatsurat.Equals("ASLI")) tingkatsurat = "BIASA";
            if (tingkatsurat.Equals("PENANGGUNG JAWAB")) tingkatsurat = "BIASA";
            tableIn.AddCell(objPdf.CreateCellTable(tingkatsurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 12f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 256f, 128f, 128f, 132f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // CONTENT INFO 2
            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Asal Surat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.PengirimSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 60f, 10f, 250f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Nomor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.NomorSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 10f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 60f, 10f, 250f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Perihal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 60f, 10f, 250f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL, true, ""));
            tableSub.AddCell(tableIn);

            table.AddCell(tableSub);

            // Blank Cell
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 512f, 132f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // DISPOSISI
            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("DITERUSKAN KEPADA:", Element.ALIGN_LEFT, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("DISPOSISI", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            string labelundangan = (surat.TipeSurat == "Surat Undangan") ? "UNDANGAN" : "";

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(labelundangan, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 256f, 256f, 132f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // CONTENT DISPOSISI --------------
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Padding = 0;

            // JABATAN DISPOSISI
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;
            tableSub.DefaultCell.Padding = 0;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("WAKIL MENTERI/WAKIL KEPALA", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("SEKRETARIS JENDERAL", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("INSPEKTUR JENDERAL", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("DIREKTORAT JENDERAL", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("1", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Tata Ruang", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("2", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Survei & Pemetaan Pertanahan & Ruang", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("3", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Penetapan Hak & Pendaftaran Tanah", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("4", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Penataan Agraria", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("5", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Pengadaan Tanah & Pengembangan Pertanahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 9f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("6", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Pengendalian & Penertiban Tanah & Ruang", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("7", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Penanganan Sengketa & Konflik Pertanahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("STAF AHLI", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("1", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Hukum Agraria & Masyarakat Adat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("2", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Reformasi Birokrasi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("3", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Partisipasi Masyarakat & Pemerintah Daerah", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("4", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Pengembangan Kawasan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("5", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Teknologi Informasi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("STAF KHUSUS", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("TENAGA AHLI", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("PUSAT", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("1", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Pengembangan Sumber Daya Manusia", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("2", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Pengembangan & Standarisasi Kebijakan ATR & P", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 9f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("3", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Data dan Informasi Pertanahan, T R & LP2B", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 18f, 218f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("LAINNYA", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Pengelola Pengaduan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);


            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("PMO JABODETABEKPUNJUR", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 236f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            // ADD JABATAN DISPOSISI
            table.AddCell(tableSub);


            // PERINTAH DISPOSISI
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;
            tableSub.DefaultCell.Padding = 0;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("1", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Edarkan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("2", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Mohon dihadiri/diwakili", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("3", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Bicarakan dengan saya", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("4", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Dibahas Bersama", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("5", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Teliti dan Tanggapi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("6", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Draft/Bahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 40f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("7", Element.ALIGN_CENTER, Element.ALIGN_TOP, 40f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan jawaban sesuai Peraturan Perundangan-undangan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 40f, 10f, Font.NORMAL, false, "")); // Perundang-undangan
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("8", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan laporan/Laporkan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("9", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Dapat Disetujui", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("10", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Ditolak", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("11", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Perbaiki", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("12", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Koordinasikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("13", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Untuk Diketahui", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("14", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Untuk Menjadi Perhatian", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("15", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Dijadwalkan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("16", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Dampingi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 40f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("17", Element.ALIGN_CENTER, Element.ALIGN_TOP, 40f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("Tindak lanjuti sesuai Peraturan\nPerundangan-undangan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 40f, 10f, Font.NORMAL, false, "")); // Perundang-undangan
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("18", Element.ALIGN_CENTER, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            tableIn.AddCell(objPdf.CreateCellTable("File", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 20f, 22f, 214f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            // Blank Cell in Perintah Disposisi (paling bawah)
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            columnWidths = new float[] { 256f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            // ADD PERINTAH DISPOSISI
            table.AddCell(tableSub);



            // Blank Cell (UNDANGAN CONTENT)

            string tanggalundangan =  (surat.TipeSurat == "Surat Undangan") ? surat.InfoTanggalUndangan : "";

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(tanggalundangan, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 256f, 256f, 132f };
            table.SetWidths(columnWidths);
            doc.Add(table);

            //string labeltanggalundangan = (surat.TanggalUndangan == "Surat Undangan") ? "UNDANGAN" : "";

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(labeltanggalundangan, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //table.AddCell(tableIn);

            //columnWidths = new float[] { 256f, 256f, 132f };
            //table.SetWidths(columnWidths);
            //doc.Add(table);

            //tableSub = new PdfPTable(1);
            //tableSub.WidthPercentage = 100;
            //tableSub.DefaultCell.Border = 0;
            //tableSub.DefaultCell.Padding = 0;

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Hari,tanggal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // Senin - Kamis
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // 19-21 November 2019
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Waktu", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // 08.30 - Selesai // Jember
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Tempat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //// ADD UNDANGAN CONTENT
            //table.AddCell(tableSub);






            // -------------- Catatan -------------------
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("CATATAN/ARAHAN", Element.ALIGN_LEFT, Element.ALIGN_TOP, 150f, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(namapejabatdisposisi, Element.ALIGN_RIGHT, Element.ALIGN_BOTTOM, 40f, 10f, Font.ITALIC, true, "")); // namapejabatdisposisi
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE, 40f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 300f, 88f, 22f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0; 
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_RIGHT, Element.ALIGN_MIDDLE, 10f, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 644f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);


            table.AddCell(tableSub);
            doc.Add(table);

            //Catatan
            //tableSub = new PdfPTable(1);
            //tableSub.WidthPercentage = 100;
            //tableSub.DefaultCell.Border = 0;

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("CATATAN/ARAHAN", Element.ALIGN_LEFT, Element.ALIGN_TOP, 150f, 10f, Font.BOLD, true, ""));
            //tableSub.AddCell(tableIn);


            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(namapejabatdisposisi, Element.ALIGN_RIGHT, Element.ALIGN_TOP, 30f, 10f, Font.NORMAL, true, "")); // namapejabatdisposisi
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);                                                                                                                                      
                                                                                                                                                   

            //tableSub.AddCell(tableIn);

            //table.AddCell(tableSub);


            //columnWidths = new float[] { 644f };
            //table.SetWidths(columnWidths);
            //doc.Add(table);


            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("LembarDisposisi", ".pdf");

            return docfile;
        }

        public ActionResult ViewPdf_LembarDisposisi_Satker(string suratid, string pejabatdisposisi)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(pejabatdisposisi, pegawaiid);

            string namajabatan = "";
            string namapejabatdisposisi = "";
            if (listPegawai.Count > 0)
            {
                foreach(var l in listPegawai)
                {
                    if (l.PegawaiId == pegawaiid)
                    {
                        namapejabatdisposisi =l.NamaLengkap;
                        namajabatan = l.Jabatan;
                    }
                }

                if (string.IsNullOrEmpty(namajabatan))
                {
                    namapejabatdisposisi = listPegawai[0].NamaLengkap;
                    namajabatan = listPegawai[0].Jabatan;
                }
            }

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
            surat.NomorSurat = string.IsNullOrEmpty(surat.NomorSurat) ? surat.NomorSurat : HttpUtility.UrlDecode(surat.NomorSurat);
            surat.NamaPengirim = string.IsNullOrEmpty(surat.NamaPengirim) ? surat.NamaPengirim : HttpUtility.UrlDecode(surat.NamaPengirim);
            surat.NamaPegawai = string.IsNullOrEmpty(surat.NamaPegawai) ? surat.NamaPegawai : HttpUtility.UrlDecode(surat.NamaPegawai);

            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontBold = PdfUtil.GetArial(10f, Font.BOLD);
            Phrase phrase = new Phrase();


            // KOP SURAT
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL REPUBLIK INDONESIA", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namajabatan.ToUpper(), Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("LEMBAR EDARAN", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 5f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            //----- BODY --------------------------------------------------



            // TITLE INFO
            table = new PdfPTable(4);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("NOMOR AGENDA", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("TANGGAL SURAT", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("DITERIMA TANGGAL", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 9f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("TINGKAT SURAT", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 252f, 126f, 126f, 140f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // CONTENT INFO
            table = new PdfPTable(4);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(surat.NomorAgendaSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalTerimaCetak, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 9f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            string sifatsurat = string.IsNullOrEmpty(surat.SifatSurat) ? "" : surat.SifatSurat.ToUpper();
            tableIn.AddCell(objPdf.CreateCellTable(sifatsurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 12f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 252f, 126f, 126f, 140f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // CONTENT INFO 2
            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Asal Surat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.PengirimSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 60f, 10f, 250f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Nomor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.NomorSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 60f, 10f, 250f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Perihal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 60f, 10f, 250f };
            tableIn.SetWidths(columnWidths);
            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL, true, ""));
            tableSub.AddCell(tableIn);

            table.AddCell(tableSub);

            // Blank Cell
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 504f, 140f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // DISPOSISI
            table = new PdfPTable(3);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("DITERUSKAN KEPADA:", Element.ALIGN_LEFT, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("DISPOSISI", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 252f, 252f, 140f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            PdfPCell checkBoxcell = new PdfPCell();
            checkBoxcell.VerticalAlignment = Element.ALIGN_TOP;
            checkBoxcell.HorizontalAlignment = Element.ALIGN_CENTER;
            //checkBoxcell.Border = 0;
            checkBoxcell.PaddingTop = 7f;
            checkBoxcell.PaddingLeft = 5f;
            //checkBoxcell.FixedHeight = 20f;
            Image image = Image.GetInstance(Server.MapPath("~/Reports/checkbox.jpg"));
            image.ScaleAbsolute(8, 8);
            checkBoxcell.AddElement(image);



            // CONTENT DISPOSISI --------------
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Padding = 0;

            // JABATAN DISPOSISI
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;
            tableSub.DefaultCell.Padding = 0;

            //List<Models.Entities.Profile> listProfileDisposisi = dataMasterModel.GetListProfileDisposisi(pegawaiid, kantorid, true);
            //if (listProfileDisposisi.Count == 0)
            //{
            //    // Bila bukan login TU, ambil jabatan2 di bawahnya user login ybs
            //    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            //    listProfileDisposisi = dataMasterModel.GetProfileDisposisiByMyProfiles(myProfiles, true);
            //}

            //foreach (Models.Entities.Profile profileSurat in listProfileDisposisi)
            //{
            //    tableIn = new PdfPTable(2);
            //    tableIn.WidthPercentage = 100;
            //    tableIn.DefaultCell.Border = 0;
            //    tableIn.AddCell(checkBoxcell);
            //    tableIn.AddCell(objPdf.CreateCellTable(profileSurat.NamaProfile, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //    columnWidths = new float[] { 20f, 232f };
            //    tableIn.SetWidths(columnWidths);
            //    tableSub.AddCell(tableIn);
            //}

            // ambil data disposisi, per surat per unitkerja
            List<Models.Entities.DisposisiSurat> listDisposisiSurat = persuratanmodel.GetListDisposisiSurat(suratid, unitkerjaid, "");
            if (listDisposisiSurat.Count == 0)
            {
                // bila data di atas tidak ada, ambil data disposisi, per surat tanpa unitkerja
                listDisposisiSurat = persuratanmodel.GetListDisposisiSurat(suratid, "", "");
            }

            foreach (Models.Entities.DisposisiSurat disposisiSurat in listDisposisiSurat)
            {
                tableIn = new PdfPTable(2);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                tableIn.AddCell(checkBoxcell);
                tableIn.AddCell(objPdf.CreateCellTable(disposisiSurat.NamaPegawai + " (" + disposisiSurat.NamaJabatan + ")", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
                columnWidths = new float[] { 20f, 232f };
                tableIn.SetWidths(columnWidths);
                tableSub.AddCell(tableIn);
            }


            // ADD JABATAN DISPOSISI
            table.AddCell(tableSub);


            // PERINTAH DISPOSISI
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;
            tableSub.DefaultCell.Padding = 0;

            List<Models.Entities.PerintahDisposisi> listPerintahDisposisi = persuratanmodel.GetPerintahDisposisi();
            foreach (Models.Entities.PerintahDisposisi perintahDisposisi in listPerintahDisposisi)
            {
                tableIn = new PdfPTable(2);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                tableIn.AddCell(checkBoxcell);
                tableIn.AddCell(objPdf.CreateCellTable(perintahDisposisi.NamaPerintahDisposisi, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
                columnWidths = new float[] { 20f, 232f };
                tableIn.SetWidths(columnWidths);
                tableSub.AddCell(tableIn);
            }

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(checkBoxcell);
            //tableIn.AddCell(objPdf.CreateCellTable("Edarkan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 232f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("2", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Mohon dihadiri/diwakili", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("3", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Bicarakan dengan saya", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("4", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Dibahas Bersama", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("5", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Teliti dan Tanggapi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(2);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("6", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Siapkan Draft/Bahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("7", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Siapkan jawaban sesuai Peraturan Perundangan-undangan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, "")); // Perundang-undangan
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("8", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Siapkan laporan/Laporkan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("9", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Dapat Disetujui", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("10", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Ditolak", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("11", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Perbaiki", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("12", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Koordinasikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("13", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Untuk Diketahui", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("14", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Untuk Menjadi Perhatian", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("15", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Dijadwalkan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("16", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Dampingi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("17", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("Tindak lanjuti sesuai Peraturan Perundangan-undangan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, "")); // Perundang-undangan
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(3);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("18", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //tableIn.AddCell(objPdf.CreateCellTable("File", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 20f, 25f, 207f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //// Blank Cell in Perintah Disposisi (paling bawah)
            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, false, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            // ADD PERINTAH DISPOSISI
            table.AddCell(tableSub);


            // Blank Cell (UNDANGAN CONTENT)
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("", Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(tableIn);

            //tableSub = new PdfPTable(1);
            //tableSub.WidthPercentage = 100;
            //tableSub.DefaultCell.Border = 0;
            //tableSub.DefaultCell.Padding = 0;

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Hari,tanggal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // Senin - Kamis
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // 19-21 November 2019
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Waktu", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // 08.30 - Selesai // Jember
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable("Tempat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //tableIn = new PdfPTable(1);
            //tableIn.WidthPercentage = 100;
            //tableIn.DefaultCell.Border = 0;
            //tableIn.AddCell(objPdf.CreateCellTable(" ", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //columnWidths = new float[] { 252f };
            //tableIn.SetWidths(columnWidths);
            //tableSub.AddCell(tableIn);

            //// ADD UNDANGAN CONTENT
            //table.AddCell(tableSub);

            columnWidths = new float[] { 252f, 252f, 140f };
            table.SetWidths(columnWidths);
            doc.Add(table);




            // -------------- Catatan -------------------
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            // Catatan
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("CATATAN/ARAHAN", Element.ALIGN_LEFT, Element.ALIGN_TOP, 80f, 10f, Font.BOLD, true, ""));

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(namapejabatdisposisi, Element.ALIGN_RIGHT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, true, "")); // namapejabatdisposisi

            tableSub.AddCell(tableIn);

            table.AddCell(tableSub);


            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("LembarDisposisi", ".pdf");

            return docfile;
        }

        public ActionResult ViewPdf_LembarDisposisi_Satker_OLD(string suratid, string pejabatdisposisi)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(pejabatdisposisi);

            string namapejabatdisposisi = "";
            if (listPegawai.Count > 0)
            {
                namapejabatdisposisi = listPegawai[0].NamaLengkap;
            }

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);


            //List<Models.Entities.TipeEselon> listTipeEselon = persuratanmodel.GetTipeEselonOnDisposisiSurat(suratid);


            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontBold = PdfUtil.GetArial(10f, Font.BOLD);
            Phrase phrase = new Phrase();


            // KOP SURAT
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            // Cell NAMA KANTOR
            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL", Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            string namakantor = dataMasterModel.GetUnitKerjaFromProfileId(pejabatdisposisi).ToUpper();
            //string namakantor = kantor.NamaKantor.ToUpper();
            //if (namakantor == "KANTOR BADAN PERTANAHAN NASIONAL")
            //{
            //    namakantor = "KANTOR PUSAT";
            //}

            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSuratUnderline(namakantor, Element.ALIGN_LEFT, 12f, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);


            // Cell LOGO
            cell = new PdfPCell();
            cell.Border = 0;
            //cell.Rowspan = 3;
            Image image = Image.GetInstance(Server.MapPath("~/Reports/logobpn.png"));
            image.ScaleAbsolute(80, 78);
            cell.AddElement(image);
            table.AddCell(cell);

            columnWidths = new float[] { 620f, 150f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Horizontal Line
            //doc.Add(objPdf.HorizontalLine());


            // TITLE
            string titlenamadokumen = "LEMBAR DISPOSISI";
            doc.Add(objPdf.AddTitleSuratUnderline(titlenamadokumen, Element.ALIGN_CENTER, 12f, 10f, 0f, 0f));
            // Eof TITLE


            //----- BODY

            // line separator
            doc.Add(objPdf.AddLineSeparator(20f));


            doc.Add(objPdf.AddParagraph(10f, 10f, 0f, Font.BOLD, 0, 0, 0, 0, Element.ALIGN_LEFT, "AGENDA NO : " + surat.NomorAgendaSurat, ""));

            // line separator
            doc.Add(objPdf.AddLineSeparator(15f));


            // Surat Dari dan Tanggal Surat -----------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Surat Dari", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.PengirimSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 240f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Surat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 100f, 10f, 210f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);

            // Nomor Surat dan Tanggal Terima -----------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Nomor Surat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.NomorSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 240f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Diterima", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.InfoTanggalTerimaCetak, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 100f, 10f, 210f };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // Perihal
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(3);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Perihal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            tableIn.AddCell(objPdf.CreateCellTable(surat.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 70f, 10f, 564 };
            tableIn.SetWidths(columnWidths);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            // --------------- Middle Header -------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.AddCell(objPdf.CreateCellTable("DITERUSKAN KEPADA", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            table.AddCell(objPdf.CreateCellTable("DISPOSISI", Element.ALIGN_LEFT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, false, ""));
            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);




            // DISPOSISI --------------------------

            PdfPCell checkBoxcell = new PdfPCell();
            checkBoxcell.VerticalAlignment = Element.ALIGN_TOP;
            checkBoxcell.HorizontalAlignment = Element.ALIGN_CENTER;
            checkBoxcell.Border = 0;
            checkBoxcell.PaddingTop = 4f;
            //checkBoxcell.FixedHeight = 20f;
            image = Image.GetInstance(Server.MapPath("~/Reports/checkbox.jpg"));
            image.ScaleAbsolute(8, 8);
            checkBoxcell.AddElement(image);

            //List<Models.Entities.DisposisiSurat> listDisposisiSurat = persuratanmodel.GetListDisposisiSurat(suratid, "");
            //List<Models.Entities.Profile> listProfileDisposisi = dataMasterModel.GetProfileDisposisi(profileidtu, false);
            List<Models.Entities.Profile> listProfileDisposisi = dataMasterModel.GetListProfileDisposisi(pegawaiid, kantorid, true);


            // 16 ------------------------------

            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.DefaultCell.Padding = 0;



            // Sub Diteruskan Kepada ------------------------------------
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;

            // Ambil dari database
            //foreach (Models.Entities.DisposisiSurat disposisiSurat in listDisposisiSurat)
            foreach (Models.Entities.Profile profileSurat in listProfileDisposisi)
            {
                tableIn = new PdfPTable(2);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;

                tableIn.AddCell(checkBoxcell);
                tableIn.AddCell(objPdf.CreateCellTable(profileSurat.NamaProfile, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "")); // disposisiSurat.NamaJabatan
                columnWidths = new float[] { 15f, 305f };
                tableIn.SetWidths(columnWidths);

                tableSub.AddCell(tableIn);
            }


            columnWidths = new float[] { 322f };
            tableSub.SetWidths(columnWidths);

            table.AddCell(tableSub);



            // Sub Perintah Disposisi -----------------------------------
            tableSub = new PdfPTable(2);
            tableSub.WidthPercentage = 100;

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Hadiri/Wakili & Lapor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Koordinasi & Selesaikan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Dampingi", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Tindak Lanjuti", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Dijadwalkan/Catat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Surat Jawaban", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Menghadap Saya", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Untuk Jadi Perhatian", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Pelajari & Saran Pendapat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Monitor", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Laporan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Perbaiki", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            // -----

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("Siapkan Bahan", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(2);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            tableIn.AddCell(checkBoxcell);
            tableIn.AddCell(objPdf.CreateCellTable("File", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 15f, 145f };
            tableIn.SetWidths(columnWidths);

            tableSub.AddCell(tableIn);


            // -----


            columnWidths = new float[] { 160f, 160f };
            tableSub.SetWidths(columnWidths);

            table.AddCell(tableSub);

            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);
            doc.Add(table);





            // Catatan
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            // Catatan
            tableSub = new PdfPTable(1);
            tableSub.WidthPercentage = 100;
            tableSub.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Catatan:", Element.ALIGN_LEFT, Element.ALIGN_TOP, 140f, 10f, Font.BOLD, true, ""));

            tableSub.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable(namapejabatdisposisi, Element.ALIGN_RIGHT, Element.ALIGN_TOP, 20f, 10f, Font.BOLD, true, ""));

            tableSub.AddCell(tableIn);

            table.AddCell(tableSub);


            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);



            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("LembarDisposisi", ".pdf");

            return docfile;
        }

        public ActionResult ViewPdf_SuratPengantar(string pengantarsuratid)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            string profileid = myProfiles.Replace("'", "");

            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(profileid);

            string namapejabat = "";
            string nippejabat = "";
            string namajabatan = dataMasterModel.GetProfileNameFromId(profileid);
            if (listPegawai.Count > 0)
            {
                namapejabat = listPegawai[0].NamaLengkap;
                nippejabat = listPegawai[0].PegawaiId;
            }
            /*
            string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
            Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);
            if (pegawaiTU != null)
            {
                namapejabat = pegawaiTU.NamaLengkap;
                nippejabat = pegawaiTU.PegawaiId;
                namajabatan = pegawaiTU.Jabatan;
            }
            */
            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            Models.Entities.PengantarSurat pengantarsurat = persuratanmodel.GetSuratPengantar(pengantarsuratid, "", "", 1, 1)[0];
            List<Models.Entities.DetilPengantar> listDetilPengantar = persuratanmodel.GetDetilPengantar(pengantarsuratid);

            string namakantor = kantor.NamaKantor.ToUpper();
            string alamatkantor = kantor.Alamat.ToUpper();
            string teleponkantor = kantor.Telepon;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
                namakantor = dataMasterModel.GetUnitKerjaFromProfileId(profileid).ToUpper();
            }


            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontnormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10f, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font fontbold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10f, iTextSharp.text.Font.BOLD);
            Phrase phrase = new Phrase();


            // KOP SURAT
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;


            // Cell LOGO
            cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Reports/logobpn.png"));
            image.ScaleAbsolute(80, 78);
            cell.AddElement(image);
            table.AddCell(cell);


            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            // Cell NAMA KANTOR
            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namakantor, Element.ALIGN_CENTER, 11f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(alamatkantor, Element.ALIGN_CENTER, 9f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("TELEPON: " + teleponkantor, Element.ALIGN_CENTER, 9f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);


            columnWidths = new float[] { 150f, 620f };
            table.SetWidths(columnWidths);

            doc.Add(table);


            // Horizontal Line
            doc.Add(objPdf.HorizontalLine());


            //// TITLE
            //doc.Add(objPdf.AddTitleSurat("SURAT PENGANTAR", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            //doc.Add(objPdf.AddTitleSurat("Nomor " + pengantarsurat.Nomor, Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            //// Eof TITLE


            //----- BODY

            // line separator
            doc.Add(objPdf.AddLineSeparator(20f));


            // Nomor Surat Pengantar
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("NO.SURAT PENGANTAR", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(pengantarsurat.Nomor, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 150f, 10f, 480f };
            table.SetWidths(columnWidths);

            doc.Add(table);


            // Waktu
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("TANGGAL", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(pengantarsurat.TanggalDari, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 150f, 10f, 480f };
            table.SetWidths(columnWidths);

            doc.Add(table);


            // Tujuan Surat
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("KEPADA YTH", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(pengantarsurat.Tujuan, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 150f, 10f, 480f };
            table.SetWidths(columnWidths);

            doc.Add(table);



            // line separator
            doc.Add(objPdf.AddLineSeparator(15f));



            table = new PdfPTable(7);
            table.WidthPercentage = 100;
            table.AddCell(objPdf.CreateCell("NO", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("ASAL SURAT", Element.ALIGN_MIDDLE, Font.BOLD));
            //table.AddCell(objPdf.CreateCell("NO AGENDA", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("PERIHAL", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("NO & TGL SURAT", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("SIFAT SURAT", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("ASLI/\nTEMBUSAN", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("KET", Element.ALIGN_MIDDLE, Font.BOLD));
            columnWidths = new float[] { 30f, 102f, 102f, 102f, 102f, 101f, 101f };
            table.SetWidths(columnWidths);
            doc.Add(table);

            foreach (Models.Entities.DetilPengantar item in listDetilPengantar)
            {
                table = new PdfPTable(7);
                table.WidthPercentage = 100;
                table.AddCell(objPdf.CreateCell(string.Format("{0:#,##0}", item.RNumber), Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.Pengirim, Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                //table.AddCell(objPdf.CreateCell(item.NomorAgenda, Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.NomorSurat + "\n" + item.TanggalSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.SifatSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.Redaksi, Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.KeteranganSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                columnWidths = new float[] { 30f, 102f, 102f, 102f, 102f, 101f, 101f };
                table.SetWidths(columnWidths);
                doc.Add(table);
            }


            // line separator
            doc.Add(objPdf.AddLineSeparator(20f));



            // FOOTER
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;


            // TTD PENERIMA
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("Diterima tanggal " + pengantarsurat.InfoTanggalTerima, Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.NORMAL, 0f, 30f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("Penerima", Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.NORMAL, 0f, 50f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(pengantarsurat.NamaPenerima, Element.ALIGN_LEFT, 12f, iTextSharp.text.Font.NORMAL, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);


            // TTD PENGIRIM
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 40f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("Pengirim", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namajabatan + ",", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 70f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namapejabat, Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("NIP. " + nippejabat, Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);


            columnWidths = new float[] { 322f, 322f };
            table.SetWidths(columnWidths);

            doc.Add(table);







            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("SuratPengantar", ".pdf");

            ViewBag.FileSP = docfile;

            return docfile;
        }

        public ActionResult ViewPdf_LaporanSurat(string sortby, string sorttype, string tanggaldari, string tanggalsampai, string bulansurat, string kategori)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                satkerid = unitkerjaid;
            }

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            if (@OtorisasiUser.IsRoleAdministrator() == true)
            {
                myProfiles = "";
            }

            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            List<Models.Entities.Surat> data = persuratanmodel.GetLaporanSurat(satkerid, myProfiles, "", kategori, "", sortby, sorttype, tanggaldari, tanggalsampai, bulansurat, 0, 0, bypegawaiid:pegawaiid, switchby:true);

            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontBold = PdfUtil.GetArial(10f, Font.BOLD);
            Phrase phrase = new Phrase();


            // KOP SURAT
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL REPUBLIK INDONESIA", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namapegawai.ToUpper(), Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            string title = "LAPORAN SURAT " + kategori.ToUpper();
            if (bulansurat == "")
            {
                title += " (" + tanggaldari + " - " + tanggalsampai + ")";
            }
            else
            {
                title += " (" + bulansurat + ")";
            }
            cell.AddElement(objPdf.AddTitleSurat(title, Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 5f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            //----- BODY --------------------------------------------------



            // Header
            table = new PdfPTable(10);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("#", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Nomor Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 9f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Nomor Agenda", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Asal Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Tujuan Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Perihal", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Sifat Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Terima", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Status", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 2, 7, 4, 7, 10, 10, 14, 4, 5, 3 };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // Content
            string str = "";
            foreach (var dt in data)
            {
                table = new PdfPTable(10);
                table.WidthPercentage = 100;

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = dt.RNumber.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.TanggalSurat) ? "" : dt.TanggalSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.NomorSurat) ? "" : dt.NomorSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.NomorAgenda) ? "" : dt.NomorAgenda.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.PengirimSurat) ? "" : dt.PengirimSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.PenerimaSurat) ? "" : dt.PenerimaSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.Perihal) ? "" : dt.Perihal.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.SifatSurat) ? "" : dt.SifatSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.TanggalTerima) ? "" : dt.TanggalTerima.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = (dt.StatusArsip == 1) ? "Selesai" : "Aktif";
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                columnWidths = new float[] { 2, 7, 4, 7, 10, 10, 14, 4, 5, 3 };
                table.SetWidths(columnWidths);
                doc.Add(table);
            }

            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("LaporanSuratMasuk", ".pdf");

            return docfile;
        }

        public ActionResult ViewPdf_LaporanSuratKeluar(string sortby, string sorttype, string tanggaldari, string tanggalsampai, string bulansurat)
        {
            var result = new { Status = false, Message = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            Models.Entities.Kantor kantor = dataMasterModel.GetKantor(kantorid);
            List<Models.Entities.SuratOutbox> data = persuratanmodel.GetLaporanSuratOutbox(pegawaiid, sortby, sorttype, tanggaldari, tanggalsampai, bulansurat, 0, 0);

            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();


            Chunk chunk = new Chunk();
            PdfPTable table = new PdfPTable(1);
            PdfPTable tableSub = new PdfPTable(1);
            PdfPTable tableIn = new PdfPTable(1);
            PdfPCell cell = new PdfPCell();
            Paragraph paragraph = new Paragraph();
            float[] columnWidths = new float[] { 0f, 0f };

            iTextSharp.text.Font fontBold = PdfUtil.GetArial(10f, Font.BOLD);
            Phrase phrase = new Phrase();


            // KOP SURAT
            table = new PdfPTable(1);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL REPUBLIK INDONESIA", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namapegawai.ToUpper(), Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            string title = "LAPORAN SURAT KELUAR";
            if (bulansurat == "")
            {
                title += " (" + tanggaldari + " - " + tanggalsampai + ")";
            }
            else
            {
                title += " (" + bulansurat + ")";
            }
            cell.AddElement(objPdf.AddTitleSurat(title, Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.BOLD, 0f, 5f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 644f };
            table.SetWidths(columnWidths);
            doc.Add(table);


            //----- BODY --------------------------------------------------



            // Header
            table = new PdfPTable(6);
            table.WidthPercentage = 100;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("#", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Tanggal Kirim", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Nomor Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 9f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Tujuan Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Perihal", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(objPdf.CreateCellTable("Sifat Surat", Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0, 10f, Font.BOLD, true, ""));
            table.AddCell(tableIn);

            columnWidths = new float[] { 1, 7, 4, 8, 14, 4 };
            table.SetWidths(columnWidths);
            doc.Add(table);



            // Content
            string str = "";
            foreach (var dt in data)
            {
                table = new PdfPTable(6);
                table.WidthPercentage = 100;

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = dt.RNumber.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_RIGHT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.TanggalKirim) ? "" : dt.TanggalKirim.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_CENTER, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.NomorSurat) ? "" : dt.NomorSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.PenerimaSurat) ? "" : dt.PenerimaSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.Perihal) ? "" : dt.Perihal.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                tableIn = new PdfPTable(1);
                tableIn.WidthPercentage = 100;
                tableIn.DefaultCell.Border = 0;
                str = string.IsNullOrEmpty(dt.SifatSurat) ? "" : dt.SifatSurat.ToString();
                tableIn.AddCell(objPdf.CreateCellTable(str, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                table.AddCell(tableIn);

                columnWidths = new float[] { 1, 7, 4, 8, 14, 4 };
                table.SetWidths(columnWidths);
                doc.Add(table);
            }

            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("LaporanSuratKeluar", ".pdf");

            return docfile;
        }
    }
}