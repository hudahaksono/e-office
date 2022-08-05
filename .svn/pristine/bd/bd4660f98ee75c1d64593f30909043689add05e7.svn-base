using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using QRCoder;
using Surat.Models;
using Surat.Models.Entities;
using iTextSharp.text;
using Surat.Codes;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using PDFEditor;
using static Surat.Codes.Functions;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class SuratController : Controller
    {
        DataMasterModel dataMasterModel = new DataMasterModel();
        PersuratanModel persuratanmodel = new PersuratanModel();
        KontentModel kontentmodel = new KontentModel();
        SuratModel mdl = new SuratModel();
        Functions functions = new Functions();

        #region Env

        public ActionResult ListSP()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public ActionResult BuatSP()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var find = new FindSurat();
            find.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            return View(find);
        }

        public ActionResult ListDraft()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public ActionResult BuatSuratMasuk()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            if (!OtorisasiUser.IsProfile("PembuatSuratMasuk"))
            {
                return View("Forbidden", "Error");
            }
            var persuratanmodel = new PersuratanModel();
            var usr = functions.claimUser();
            var data = new Models.Entities.Surat();
            data.ListSifatSurat = persuratanmodel.GetSifatSurat();
            data.ListTipeSurat = persuratanmodel.GetTipeSurat();
            data.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            data.ListProfileTujuan = new List<Profile>();
            data.ListTujuanPegawai = new List<Pegawai>();
            data.ListProfiles = new List<Profile>();
            ViewBag.UnitKerjaId = usr.UnitKerjaId;
            return View(data);
        }

        public ActionResult BuatSuratKeluar()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var persuratanmodel = new PersuratanModel();
            var usr = functions.claimUser();
            var data = new Models.Entities.Surat();
            string reff = Request.Form["reff"];
            if (!string.IsNullOrEmpty(reff))
            {
                data.Referensi = reff;
            }
            else
            {
                data.Referensi = "";
            }
            data.ListSifatSurat = persuratanmodel.GetSifatSurat();
            data.ListTipeSurat = persuratanmodel.GetTipeSurat();
            data.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            data.ListProfileTujuan = new List<Profile>();
            data.ListTujuanPegawai = new List<Pegawai>();
            data.ListProfiles = new List<Profile>();
            ViewBag.Massal = usr.KantorId.Equals("980FECFC746D8C80E0400B0A9214067D");
            return View(data);
        }

        public ActionResult BuatSuratInisiatif()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();

            ViewBag.UnitKerjaId = usr.UnitKerjaId;

            var surat = new Models.Entities.Surat();
            surat.ListUnitKerja = dataMasterModel.GetListUnitKerjaInisiatif(usr.PegawaiId);
            surat.ListProfileTujuan = new List<Profile>();
            surat.ListTujuanPegawai = new List<Pegawai>();
            surat.ListProfiles = new List<Profile>();

            return View(surat);
        }

        public ActionResult EditorNaskahDinas(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var data = new DraftSurat();
            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
            if (string.IsNullOrEmpty(id))
            {
                if (OtorisasiUser.PembuatDokumenElektronik())
                {
                    data.Tujuan = new List<string>();
                    data.Tembusan = new List<string>();
                    ViewBag.Judul = "Baru";
                }
                else
                {
                    return View("Index", "Home");
                }
            }
            else
            {
                data = mdl.GetDraftSurat(id, usr.UnitKerjaId);
                data.Perihal = Server.UrlDecode(data.Perihal);
                data.IsiSurat = Server.UrlDecode(data.IsiSurat);
                ViewBag.Judul = data.DraftCode;
                data.Status = string.IsNullOrEmpty(data.Status) ? "P" : data.Status;
            }
            data.isTU = OtorisasiUser.isTU();
            data.ListKodeKopSurat = new List<SelectListItem>();
            var listProfile = new SuratModel().GetProfilesByUnitKerja(usr.UnitKerjaId);
            data.ListProfile = new List<SelectListItem>();
            foreach (var p in listProfile)
            {
                data.ListProfile.Add(new SelectListItem { Text = p.NamaProfile, Value = p.ProfileId });
            }
            if (tipekantorid == 1)
            {
                data.ListKodeKopSurat.Add(new SelectListItem { Text = dataMasterModel.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId });
                data.ListKodeKopSurat.Add(new SelectListItem { Text = "Kementerian ATR/BPN", Value = "02" });
            }
            else if (tipekantorid == 2)
            {
                data.ListKodeKopSurat.Add(new SelectListItem { Text = dataMasterModel.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId });
                data.ListKodeKopSurat.Add(new SelectListItem { Text = "Kementerian ATR/BPN", Value = "02" });
            }
            else
            {
                string induk = dataMasterModel.GetKantorIdIndukFromKantorId(usr.KantorId);
                string unitinduk = dataMasterModel.GetUnitKerjaIdFromKantorId(induk);
                data.ListKodeKopSurat.Add(new SelectListItem { Text = dataMasterModel.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId });
                data.ListKodeKopSurat.Add(new SelectListItem { Text = dataMasterModel.GetNamaUnitKerjaById(unitinduk), Value = unitinduk });
            }
            return View(data);
        }

        #endregion

        public ActionResult DaftarSP(int? draw, int? start, int? length, CariSuratPengantar f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<PengantarSurat>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string unitkerjaid = usr.UnitKerjaId;
            string userid = usr.UserId;
            string sort = Request.Form["orderby"];
            string dir = Request.Form["orderdir"];
            string tipe = Request.Form["tipe"].ToString();
            string sortby = string.Empty;
            switch (sort){
                case "1":
                    sortby = "TDE.TANGGALDIBUAT " + dir;
                    break;
                case "2":
                    sortby = "TDE.NOMORSURAT " + dir;
                    break;
                case "3":
                    sortby = "TDE.TANGGALSURAT " + dir;
                    break;
                case "4":
                    if(tipe == "sudah")
                    {
                        sortby = "TTE.TANGGAL " + dir;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.UnitKerjaId = usr.UnitKerjaId;
                result = mdl.GetSuratPengantar(tipe, sortby , f, from, to);
                var hakAkses = new HakAksesModel();
                bool isChecker = hakAkses.isPriv(usr.PegawaiId, unitkerjaid, "SP_CHECKER");
                bool isApprover = hakAkses.isPriv(usr.PegawaiId, unitkerjaid, "SP_APPROVER");
                if (isChecker || isApprover)
                {
                    foreach (var dt in result)
                    {
                        if(!string.IsNullOrEmpty(dt.Status) && dt.Status.Equals("P"))
                        {
                            if (isChecker)
                            {
                                dt.stCheck = "1";
                            }
                        }else if (!string.IsNullOrEmpty(dt.Status) && dt.Status.Equals("W"))
                        {
                            if (isApprover)
                            {
                                dt.stTTE = "1";
                            }
                        }
                    }
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarSurat(int? draw, int? start, int? length, FindSurat f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<Models.Entities.Surat>();
            DateTime cM = DateTime.Today;
            DateTime cS = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (!string.IsNullOrEmpty(Request.Form["cariMulai"]))
            {
                cM = Convert.ToDateTime(Request.Form["cariMulai"]);
            }
            if (!string.IsNullOrEmpty(Request.Form["cariSampai"]))
            {
                cS = Convert.ToDateTime(Request.Form["cariSampai"]);
            }

            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;
            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;

                result = mdl.GetSuratForSP(f.UnitKerjaIdTujuan, f.ProfileIdTujuan, satkerid, cM, cS, from, to);
                if(result.Count > 0)
                {
                    total = result[0].Total;
                }


            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarDraft(int? draw, int? start, int? length, CariDraftSurat f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<ListDraft>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;
            string sort = Request.Form["orderby"];
            string dir = Request.Form["orderdir"];
            string tipe = Request.Form["tipe"].ToString();
            string sortby = string.Empty;
            switch (sort)
            {
                case "1":
                    sortby = "TDE.TANGGALDIBUAT " + dir;
                    break;
                case "2":
                    sortby = "TDE.NOMORSURAT " + dir;
                    break;
                case "3":
                    sortby = "TDE.TANGGALSURAT " + dir;
                    break;
                case "4":
                    if (tipe == "sudah")
                    {
                        sortby = "TTE.TANGGAL " + dir;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.UnitKerjaId = usr.UnitKerjaId;
                result = mdl.GetListDraft(tipe, usr.UserId, sortby, f, from, to);
                foreach (var dt in result)
                {
                    dt.Perihal = Server.UrlDecode(dt.Perihal);
                    if (!string.IsNullOrEmpty(dt.Status) && dt.Status.Equals("W") && OtorisasiUser.isTU())
                    {
                        dt.stCheck = "1";
                    }
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ViewPdf_SuratPengantar(string pengantarsuratid)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new { Status = false, Message = "" };

            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string pegawaiid = usr.PegawaiId;

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            string profileid = myProfiles.Replace("'", "");

            var listPegawai = dataMasterModel.GetPegawaiByProfileId(profileid);

            string namapejabat = "";
            string nippejabat = "";
            string namajabatan = dataMasterModel.GetProfileNameFromId(profileid);
            if (listPegawai.Count > 0)
            {
                namapejabat = listPegawai[0].NamaLengkap;
                nippejabat = listPegawai[0].PegawaiId;
            }

            var kantor = dataMasterModel.GetKantor(kantorid);
            var pengantarsurat = new PersuratanModel().GetSuratPengantar(pengantarsuratid, "", "", 1, 1)[0];
            var listDetilPengantar = new PersuratanModel().GetDetilPengantar(pengantarsuratid);

            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            var objPdf = new PdfUtil();

            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            var doc = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            var pw = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            var chunk = new Chunk();
            var table = new PdfPTable(1);
            var tableSub = new PdfPTable(1);
            var tableIn = new PdfPTable(1);
            var cell = new PdfPCell();
            var paragraph = new Paragraph();
            var columnWidths = new float[] { 0f, 0f };

            var fontnormal = new Font(Font.FontFamily.TIMES_ROMAN, 10f, Font.NORMAL);
            var fontbold = new Font(Font.FontFamily.TIMES_ROMAN, 10f, Font.BOLD);
            var phrase = new Phrase();

            // KOP SURAT
            var kopsurat = mdl.getKopDetail(unitkerjaid);
            if (string.IsNullOrEmpty(kopsurat.UnitKerjaId))
            {
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                kopsurat.NamaKantor_L1 = (tipekantorid == 1) ? dataMasterModel.GetNamaUnitKerjaById(unitkerjaid).ToUpper() : kantor.NamaKantor.ToUpper();
                kopsurat.NamaKantor_L2 = "";
                kopsurat.Alamat = myTI.ToTitleCase(kantor.Alamat.ToLower());
                kopsurat.Telepon = kantor.Telepon;
                kopsurat.Email = kantor.Email;
                kopsurat.FontSize = 11;
            }
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            // Cell LOGO
            cell = new PdfPCell();
            cell.Border = 0;
            cell.PaddingTop = -30f;
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
            cell.PaddingTop = -25f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 17f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingTop = -8f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL", Element.ALIGN_CENTER, 17f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L1, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L2, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 100f, 540f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            string fullAlamat = string.Concat("<p>", kopsurat.Alamat);
            if (!string.IsNullOrEmpty(kopsurat.Telepon))
            {
                fullAlamat += string.Concat(" Telepon: ", kopsurat.Telepon);
            }
            if (!string.IsNullOrEmpty(kopsurat.Email))
            {
                fullAlamat += string.Concat(" <i>email: ", kopsurat.Email, "</i>");
            }
            fullAlamat += "</p>";

            foreach (IElement element in XMLWorkerHelper.ParseToElementList(fullAlamat, "p { font-family: Arial Narrow; font-size: " + kopsurat.FontSize + "px; text-align: center; }"))
            {
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Padding = 0f;
                cell.AddElement(element);
                table.AddCell(cell);
                doc.Add(table);
            }

            doc.Add(objPdf.HorizontalLine());
            doc.Add(objPdf.AddLineSeparator(10f));

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
            table.AddCell(objPdf.CreateCell("PERIHAL", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("NO & TGL SURAT", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("SIFAT\nSURAT", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("ASLI/\nTEMBUSAN", Element.ALIGN_MIDDLE, Font.BOLD));
            table.AddCell(objPdf.CreateCell("KET", Element.ALIGN_MIDDLE, Font.BOLD));
            columnWidths = new float[] { 30f, 102f, 152f, 112f, 82f, 91f, 71f };
            table.SetWidths(columnWidths);
            doc.Add(table);

            foreach (var item in listDetilPengantar)
            {
                table = new PdfPTable(7);
                table.WidthPercentage = 100;
                table.AddCell(objPdf.CreateCell(string.Format("{0:#,##0}", item.RNumber), Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.Pengirim, Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.Perihal, Element.ALIGN_LEFT, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.NomorSurat + "\n" + item.TanggalSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.SifatSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.Redaksi, Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                table.AddCell(objPdf.CreateCell(item.KeteranganSurat, Element.ALIGN_CENTER, Element.ALIGN_TOP, 5f, 10f, Font.NORMAL));
                columnWidths = new float[] { 30f, 102f, 152f, 112f, 82f, 91f, 71f };
                table.SetWidths(columnWidths);
                doc.Add(table);
            }

            // FOOTER

            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("Diterima", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(pengantarsurat.TanggalTerima, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 100f, 10f, 530f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("Nama", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(pengantarsurat.NamaPenerima, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 100f, 10f, 530f };
            table.SetWidths(columnWidths);

            doc.Add(table);
            
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            
            // TTD PENERIMA
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            table.AddCell(tableIn);
            
            // TTD PENGIRIM
            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat("Pengirim", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.AddElement(objPdf.AddTitleSurat(namajabatan + ",", Element.ALIGN_CENTER, 12f, iTextSharp.text.Font.NORMAL, 0f, 20f, 0f));
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
            
            byte[] byteArray = ms.ToArray();

            var mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var resultAddFooter = new PDFBuilder().Build(
               streamPdf: mss,
               template: PDFBuilder.Template.FOOTER,
               pageTte: 0);

            var docfile = new FileStreamResult(
                resultAddFooter.Output,
                MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = string.Concat("SuratPengantar", ".pdf");

            ViewBag.FileSP = docfile;

            return docfile;
        }

        private string DigitalSignatureUrl()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return ConfigurationManager.AppSettings["DigitalSignatureUrl"].ToString();
        }

        private string UrlDokumenTTE()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return ConfigurationManager.AppSettings["UrlDokumenTTE"].ToString();
        }

        public ActionResult SettingKopSurat(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            id = (string.IsNullOrEmpty(id)) ? usr.UnitKerjaId : id;

            if (!string.IsNullOrEmpty(id))
            {
                var data = mdl.getKopDetail(id);
                if (string.IsNullOrEmpty(data.UnitKerjaId))
                {
                    data.UnitKerjaId = id;
                    TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                    string kantorid = usr.KantorId;
                    var kantor = dataMasterModel.GetKantor(kantorid);
                    string namakantor = kantor.NamaKantor.ToUpper();
                    string namasatker = dataMasterModel.GetNamaUnitKerjaById(id).ToUpper();
                    string alamatkantor = myTI.ToTitleCase(kantor.Alamat.ToLower());
                    string teleponkantor = kantor.Telepon;
                    string email = kantor.Email;
                    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                    if (tipekantorid == 1)
                    {
                        namakantor = namasatker;
                    }
                    data.UnitKerjaName = namasatker;
                    data.NamaKantor_L1 = namakantor;
                    data.NamaKantor_L2 = "";
                    data.Alamat = alamatkantor;
                    data.Telepon = teleponkantor;
                    data.Email = email;
                    data.FontSize = 11;
                }

                return PartialView("KopSetting", data);
            }
            else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "noresults",
                    ContentEncoding = Encoding.UTF8
                };
            }
        }

        [HttpPost]
        public JsonResult SimpanKopSurat(KopSurat data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string UserId = usr.UserId;
            data.UnitKerjaId = string.IsNullOrEmpty(data.UnitKerjaId) ? usr.UnitKerjaId : data.UnitKerjaId;

            tr = mdl.SimpanKopSurat(data, UserId);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuatSuratPengantar(string ud, string pd, DateTime cm, DateTime cs, string ids, string jt, string pt, string pc)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            bool sukses = true;
            string pesan = string.Empty;
            string id = string.Empty;
            string nomorsp = "";
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(ud))
            {
                string userid = usr.UserId;
                string unitkerjaid = usr.UnitKerjaId;

                string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
                try
                {
                    var suratids = new List<string>();
                    if (!string.IsNullOrEmpty(ids))
                    {
                        string[] str = ids.Split(',');
                        foreach(var s in str)
                        {
                            suratids.Add(s);
                        }
                    }
                    
                    TransactionResult tr = mdl.BuatSuratPengantar(ud, pd, cm, cs, suratids, jt, pc, pt, satkerid, unitkerjaid, userid, out nomorsp);

                    if (!tr.Status)
                    {
                        throw new Exception(tr.Pesan);
                    }

                    pesan = tr.Pesan;
                    id = tr.ReturnValue;
                }
                catch (Exception ex)
                {
                    sukses = false;
                    pesan = ex.Message;
                }
            }
            else
            {
                sukses = false;
                pesan = "Data yang diproses tidak ditemukan.";
            }

            return Json(new { Status = sukses, Pesan = pesan, ReturnValue = id, NomorSP = nomorsp }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PembuatanSurat(string id, string tp, string ps)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            var surat = GenerateNotaDinasElektronik(id, usr);
            var data = mdl.GetDraftSurat(id, usr.UnitKerjaId);

            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");

            string dokid = mdl.GetUID();
            tr = new TandaTanganElektronikModel().SimpanPengajuanDariDraft(data, dokid, usr.NamaPegawai, usr.KantorId, "DokumenTTE");
            if (tr.Status)
            {
                var strSurat = surat.FileStream;
                if (!string.IsNullOrEmpty(data.LampiranId))
                {
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    string tipe = "FileLampiranTTE";
                    content.Add(new StringContent(usr.KantorId), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(data.LampiranId), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent("0"), "versionNumber");

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Retrieve"));

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var reqresult = client.SendAsync(reqmessage).Result;
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                            {
                                var strm = reqresult.Content.ReadAsStreamAsync().Result;
                                List<Stream> lst = new List<Stream>();
                                lst.Add(strSurat);
                                lst.Add(strm);
                                strSurat = functions.MergeFiles(lst);
                            }
                            else
                            {
                                tr.Status = false;
                                tr.Pesan = "Lampiran tidak ditemukan";
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tr.Status = false;
                        tr.Pesan = ex.Message;
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                }

                if (string.IsNullOrEmpty(ps))
                {
                    tr = SimpanNaskahDinas(dokid, id, surat.FileStream, usr.KantorId, usr.UserId, usr.NamaPegawai);
                }
                else
                {
                    byte[] byt = new byte[strSurat.Length];
                    Stream strm = strSurat;
                    strm.Read(byt, 0, byt.Length);

                    string pegawai = usr.NamaPegawai;
                    string nip = usr.PegawaiId;
                    string userid = usr.UserId;
                    PenandatanganInfo info = new TandaTanganElektronikModel().getPenandatanganInfo(nip);
                    if (info == null)
                    {
                        tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    string nik = info.nik;
                    string passphrase = ps;
                    string ttdid = info.ttdid;

                    try
                    {
                        if (string.IsNullOrEmpty(dokid))
                        {
                            tr.Pesan = "Dokumen tidak ditemukan";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        if (string.IsNullOrEmpty(pegawai))
                        {
                            tr.Pesan = "Penandatangan tidak ditemukan";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        if (string.IsNullOrEmpty(passphrase))
                        {
                            tr.Pesan = "Harap masukkan Passphrase";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            passphrase = Server.UrlEncode(passphrase);
                        }

                        string tampilan = new TandaTanganElektronikModel().getTipeTTE(dokid, userid);
                        tr = new TandaTanganElektronikController().ProcessSignNaskahDinas(new MemoryStream(byt), nik, usr.UnitKerjaId, passphrase, ttdid, usr.KantorId, dokid, 0, tampilan).Result;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { Status = false, Pesan = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTujuanSurat(string jn)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            List<AsalSurat> result = mdl.GetTujuanSurat(jn);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListSifatSurat()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var list = mdl.GetSifatSurat();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListTipeSurat()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var list = mdl.GetTipeSurat();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPegawaiByJabatanNama(int? draw, int? start, int? length)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<Pegawai>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string unitkerjaid = usr.UnitKerjaId;
            string userid = usr.UserId;
            string profileidtu = usr.ProfileIdTU;
            string metadata = Request.Form["metadata"].ToString();
            string namajabatan = Request.Form["namajabatan"].ToString();
            string namapegawai = Request.Form["namapegawai"].ToString();
            string tipe = Request.Form["tipe"].ToString();
            if (tipe == "1")
            {
                userid = string.Empty;
            }

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = mdl.GetPegawaiByJabatanNama(profileidtu, namajabatan, namapegawai, metadata, userid, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SimpanDraftNaskahDinas(DraftSurat data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            var userIdentity = functions.claimUser();
            string kantorid = userIdentity.KantorId;
            string pNama = userIdentity.NamaPegawai;
            data.UserPembuat = userIdentity.UserId;

            if (string.IsNullOrEmpty(data.listTujuan))
            {
                tr.Pesan = "Tujuan Surat Kosong";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.UserPembuat))
            {
                tr.Pesan = "Kode Pembuatan Kosong";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.KodeArsip))
            {
                tr.Pesan = "Kode Arsip Kosong";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.TipeSurat))
            {
                tr.Pesan = "Tipe Surat Kosong";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.SifatSurat))
            {
                tr.Pesan = "Sifat Surat Kosong";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(data.listTTE) && data.listTTE != "")
            {
                string[] str = data.listTTE.Split('|');
                data.TTE = new List<UserTTE>();
                var usertte = new UserTTE();
                int urut = 1;
                if (!string.IsNullOrEmpty(data.Pass))
                {
                    usertte.PenandatanganId = userIdentity.UserId;
                    usertte.Tipe = "0";
                    usertte.ProfileId = userIdentity.ProfileIdTU;
                    usertte.Urut = urut;
                    bool doParaf = true;
                    foreach (var s in str)
                    {
                        string[] tte = s.Split(',');
                        if (tte[0].ToString() == userIdentity.PegawaiId) { doParaf = false; }
                    }
                    if (doParaf)
                    {
                        data.TTE.Add(usertte);
                        urut += 1;
                    }
                }
                bool ttd = false;
                foreach (var s in str)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] tte = s.Split(',');
                        if (tte.Count() == 3)
                        {
                            usertte = new UserTTE();
                            usertte.PenandatanganId = new TandaTanganElektronikModel().getUserId(tte[0].ToString());
                            usertte.Tipe = tte[1].ToString();
                            usertte.ProfileId = tte[2].ToString();
                            usertte.Urut = urut;
                            data.TTE.Add(usertte);
                            urut += 1;
                            if (tte[1].ToString() == "1")
                            {
                                ttd = true;
                            }
                        }
                    }
                }
                if (data.TTE.Count() == 0)
                {
                    tr.Pesan = "List Penandatangan harus diisi";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                if (!ttd)
                {
                    tr.Pesan = "Penandatangan harus dipilih";
                    return Json(tr , JsonRequestBehavior.AllowGet);
                }
            }

            data.Perihal = data.Perihal;
            data.UnitKerjaId = string.IsNullOrEmpty(data.UnitKerjaId) ? userIdentity.UnitKerjaId : data.UnitKerjaId;
            data.JumlahLampiran = "-";

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && (string.IsNullOrEmpty(data.LampiranId) || data.islampiranChange) )
                {
                    int versi = 0;
                    HttpPostedFileBase mfile = Request.Files[0];
                    if (mfile != null)
                    {
                        if (mfile.ContentType != "application/pdf")
                        {
                            tr.Pesan = "Lampiran harus pdf";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        data.LampiranId = string.IsNullOrEmpty(data.LampiranId)? mdl.GetUID() : data.LampiranId;

                        var reqmessage = new HttpRequestMessage();
                        var content = new MultipartFormDataContent();
                        var tipe = "DokumenTTE";
                        content.Add(new StringContent(kantorid), "kantorId");
                        content.Add(new StringContent(tipe), "tipeDokumen");
                        content.Add(new StringContent(data.LampiranId), "dokumenId");
                        content.Add(new StringContent(".pdf"), "fileExtension");
                        content.Add(new StringContent(versi.ToString()), "versionNumber");
                        content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);
                        reqmessage.Method = HttpMethod.Post;
                        reqmessage.Content = content;
                        reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                        using (var client = new HttpClient())
                        {
                            var reqresult = client.SendAsync(reqmessage).Result;
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                            {
                                tr = mdl.SimpanDraftNaskahDinas(data, pNama, kantorid);
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tr.Pesan = string.Concat("Gagal mengunggah lampiran, \n", reqresult.ReasonPhrase);
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        tr = mdl.SimpanDraftNaskahDinas(data, pNama, kantorid);
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (!string.IsNullOrEmpty(data.LampiranId) && data.islampiranChange) {
                    data.LampiranId = "hapus|" + data.LampiranId;
                    tr = mdl.SimpanDraftNaskahDinas(data, pNama, kantorid);
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tr = mdl.SimpanDraftNaskahDinas(data, pNama, kantorid);
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                tr.Pesan = ex.Message;
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult HapusDraft(string id, string alasan)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var usr = functions.claimUser();
                    result = mdl.HapusDraft(id, usr.UnitKerjaId, usr.UserId, alasan);
                    if (!result.Status)
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PengajuanDraft(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var usr = functions.claimUser();
                    result = mdl.PengajuanDraft(id, usr.UnitKerjaId, usr.UserId, usr.ProfileIdTU);
                    if (!result.Status)
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public TransactionResult SimpanNaskahDinas(string dokid, string id, Stream surat, string kantorid, string userid, string nama)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                var tipe = "DokumenTTE";
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(dokid), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent("0"), "versionNumber");
                content.Add(new StreamContent(surat), "file", string.Concat(id, ".pdf"));
                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                using (var client = new HttpClient())
                {
                    var reqresult = client.SendAsync(reqmessage).Result;
                    if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                    {
                        result.Status = true;
                        result.Pesan = dokid;
                    }
                    else
                    {
                        mdl.HapusDokumen(dokid, userid, nama, string.Concat("Gagal Upload ", reqresult.ReasonPhrase.ToString()));

                        result.Status = false;
                        result.Pesan = reqresult.ReasonPhrase;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Pesan = string.Concat("Gagal Menyiapkan Dokumen \n", ex.Message);
            }
            return result;
        }

        public ActionResult GenerateDokumenElektronik(string id, string tp)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            FileStreamResult result = null;
            tp = string.IsNullOrEmpty(tp) ? "Nota Dinas" : tp;
            if (!string.IsNullOrEmpty(id))
            {
                var usr = functions.claimUser();
                if (tp.Equals("Nota Dinas"))
                {
                    result = GenerateNotaDinasElektronik(id, usr);
                }
                else if (tp.Equals("Surat Undangan"))
                {
                    result = GenerateSuratUndanganElektronik(id, usr);
                }
            }

            ViewBag.FileSP = result;

            return result;
        }

        public FileStreamResult GenerateNotaDinasElektronik(string id, userIdentity usr)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var sm = new SuratModel();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;

            var dokumen = mdl.GetDraftSurat(id, unitkerjaid);

            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            PdfUtil objPdf = new PdfUtil();

            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4, 20, 20, 40, 60);
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
            Phrase phrase = new Phrase();

            // KOP SURAT
            var kopsurat = sm.getKopDetail(unitkerjaid);
            if (string.IsNullOrEmpty(kopsurat.UnitKerjaId))
            {
                Kantor kantor = dataMasterModel.GetKantor(kantorid);
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                kopsurat.NamaKantor_L1 = (tipekantorid == 1) ? dataMasterModel.GetNamaUnitKerjaById(unitkerjaid).ToUpper() : kantor.NamaKantor.ToUpper();
                kopsurat.NamaKantor_L2 = "";
                kopsurat.Alamat = myTI.ToTitleCase(kantor.Alamat.ToLower());
                kopsurat.Telepon = kantor.Telepon;
                kopsurat.Email = kantor.Email;
                kopsurat.FontSize = 11;
            }
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            // Cell LOGO
            cell = new PdfPCell();
            cell.Border = 0;
            cell.PaddingTop = -30f;
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
            cell.PaddingTop = -25f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 17f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingTop = -8f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL", Element.ALIGN_CENTER, 17f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L1, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L2, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 100f, 540f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            string fullAlamat = string.Concat("<p>", kopsurat.Alamat);
            if (!string.IsNullOrEmpty(kopsurat.Telepon))
            {
                fullAlamat += string.Concat(" Telepon: ", kopsurat.Telepon);
            }
            if (!string.IsNullOrEmpty(kopsurat.Email))
            {
                fullAlamat += string.Concat(" <i>email: ", kopsurat.Email, "</i>");
            }
            fullAlamat += "</p>";

            foreach (IElement element in XMLWorkerHelper.ParseToElementList(fullAlamat, "p { font-family: Arial Narrow; font-size: " + kopsurat.FontSize + "px; text-align: center; }"))
            {
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Padding = 0f;
                cell.AddElement(element);
                table.AddCell(cell);
                doc.Add(table);
            }

            doc.Add(objPdf.HorizontalLine());
            doc.Add(objPdf.AddLineSeparator(10f));


            // TITLE
            doc.Add(objPdf.AddTitleSurat(dokumen.TipeSurat.ToUpper(), Element.ALIGN_CENTER, 12f, Font.BOLD, 0f, 0f, 0f, "Bookman Old Style"));
            doc.Add(objPdf.AddTitleSurat("\n", Element.ALIGN_CENTER, 12f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));

            TextField nomorField = new TextField(pw, new Rectangle(200, 705, PageSize.A4.Width - 200, 685), "fnomor");
            nomorField.Text = "Nomor : ";
            nomorField.Alignment = Element.ALIGN_CENTER;
            var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\BOOKOS.TTF";
            nomorField.Font = BaseFont.CreateFont(fontPath, BaseFont.WINANSI, BaseFont.EMBEDDED);
            nomorField.FontSize = 12f;

            pw.AddAnnotation(nomorField.GetTextField());
            // Eof TITLE

            //----- BODY

            // line separator
            doc.Add(objPdf.AddLineSeparator(20f));

            // Penerima Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            foreach (var str in dokumen.Tujuan)
            {
                dokumen.Penerima += (string.IsNullOrEmpty(dokumen.Penerima)) ? "" : "\n";
                dokumen.Penerima += str;
            }
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Yth", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.Penerima) ? "<<Penerima Surat>>" : dokumen.Penerima, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Pengirim Surat

            dokumen.Pengirim = dataMasterModel.GetProfileNameFromId(dokumen.ProfilePengirim);
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Dari", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.Pengirim) ? "<<Pengirim Surat>>" : dokumen.Pengirim, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Tanggal Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Tanggal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.TanggalSurat) ? string.Concat("<<",DateTime.Now.ToString("dd MMMM yyyy"),">>") : dokumen.TanggalSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            float start = 50 + 65 + 18;
            float end = start + 100;
            TextField tanggalField = new TextField(pw, new Rectangle(start, 647.5f, end, 627.5f), "ftanggal");
            tanggalField.Text = "";
            tanggalField.Alignment = Element.ALIGN_LEFT;
            tanggalField.Font = BaseFont.CreateFont(fontPath, BaseFont.WINANSI, BaseFont.EMBEDDED);
            tanggalField.FontSize = 10f;
            pw.AddAnnotation(tanggalField.GetTextField());

            // Sifat Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Sifat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.SifatSurat) ? "<<Biasa>>" : dokumen.SifatSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Lampiran Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Lampiran", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.LampiranId) ? "-" : "1", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Hal Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Hal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.Perihal) ? "<<Ringkasan Pengenal Surat>>" : HttpUtility.UrlDecode(dokumen.Perihal), Element.ALIGN_JUSTIFIED, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.HorizontalLine());
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 540f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // line separator
            doc.Add(objPdf.AddLineSeparator(15f));

            // Isi Surat
            string isisurat = HttpUtility.UrlDecode(dokumen.IsiSurat);
            isisurat = isisurat.Replace("<div", "<p");
            isisurat = isisurat.Replace("</div>", "</p>");
            isisurat = isisurat.Replace("<br>", "<br/>");
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
            string styles = "p { font-family: Bookman Old Style; font-size: 10pt; text-indent: 100px; text-align: justify; }";
            foreach (IElement element in XMLWorkerHelper.ParseToElementList(isisurat, styles))
            {
                table = new PdfPTable(3);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;
                table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Padding = 0f;
                cell.AddElement(element);
                table.AddCell(cell);
                table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                columnWidths = new float[] { 50f, 540f, 50f };
                table.SetWidths(columnWidths);

                doc.Add(table);
            }

            // Penandatangan
            table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));

            tableIn = new PdfPTable(1);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.AddElement(objPdf.AddTitleSurat(dokumen.Pengirim, Element.ALIGN_CENTER, 10f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));
            tableIn.AddCell(cell);

            var penandatangan = dataMasterModel.GetPegawaiByProfileId(dokumen.ProfilePengirim)[0];
            string nama = penandatangan.NamaLengkap;
            cell = new PdfPCell();
            cell.Border = 0;
            cell.PaddingTop = 20f;
            cell.AddElement(objPdf.AddTitleSurat(nama, Element.ALIGN_CENTER, 10f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));
            tableIn.AddCell(cell);

            string nip = penandatangan.PegawaiId;
            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSurat(string.Concat("NIP. ", nip), Element.ALIGN_CENTER, 10f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 240f, 300f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Tembusan Surat
            int n = 1;
            foreach (var str in dokumen.Tembusan)
            {
                dokumen.listTembusan += (string.IsNullOrEmpty(dokumen.listTembusan)) ? "" : "\n";
                dokumen.listTembusan += string.Concat(n.ToString(),". ",str);
                n += 1;
            }
            if (dokumen.Tembusan.Count > 0)
            {
                dokumen.listTembusan = string.Concat("Tembusan : \n", dokumen.listTembusan);
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;
                table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.listTembusan) ? "<<Tembusan Surat>>" : dokumen.listTembusan, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
                columnWidths = new float[] { 640f };
                table.SetWidths(columnWidths);

                doc.Add(table);
            }

            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var resultAddFooter = new PDFBuilder().Build(
               streamPdf: mss,
               template: PDFBuilder.Template.FOOTER,
               pageTte: 0);

            //resultAddFooter = new PDFBuilder().BuildKalimatSambung(resultAddFooter.Output);

            var docfile = new FileStreamResult(
                resultAddFooter.Output,
                MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = string.Concat("DokumenElektronik", ".pdf");

            return docfile;
        }

        public FileStreamResult GenerateSuratUndanganElektronik(string id, userIdentity usr)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var sm = new SuratModel();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;

            var dokumen = mdl.GetDraftSurat(id, unitkerjaid);

            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            PdfUtil objPdf = new PdfUtil();

            if (!Directory.Exists(Server.MapPath("~/Reports")))
                Directory.CreateDirectory(Server.MapPath("~/Reports"));

            Document doc = new Document(PageSize.A4, 20, 20, 40, 60);
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
            Phrase phrase = new Phrase();

            // KOP SURAT
            var kopsurat = sm.getKopDetail(unitkerjaid);
            if (string.IsNullOrEmpty(kopsurat.UnitKerjaId))
            {
                Kantor kantor = dataMasterModel.GetKantor(kantorid);
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                kopsurat.NamaKantor_L1 = (tipekantorid == 1) ? dataMasterModel.GetNamaUnitKerjaById(unitkerjaid).ToUpper() : kantor.NamaKantor.ToUpper();
                kopsurat.NamaKantor_L2 = "";
                kopsurat.Alamat = myTI.ToTitleCase(kantor.Alamat.ToLower());
                kopsurat.Telepon = kantor.Telepon;
                kopsurat.Email = kantor.Email;
                kopsurat.FontSize = 11;
            }
            table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            // Cell LOGO
            cell = new PdfPCell();
            cell.Border = 0;
            cell.PaddingTop = -30f;
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
            cell.PaddingTop = -25f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat("KEMENTERIAN AGRARIA DAN TATA RUANG/", Element.ALIGN_CENTER, 17f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingTop = -8f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat("BADAN PERTANAHAN NASIONAL", Element.ALIGN_CENTER, 17f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L1, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L2, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 100f, 540f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            string fullAlamat = string.Concat("<p>", kopsurat.Alamat);
            if (!string.IsNullOrEmpty(kopsurat.Telepon))
            {
                fullAlamat += string.Concat(" Telepon: ", kopsurat.Telepon);
            }
            if (!string.IsNullOrEmpty(kopsurat.Email))
            {
                fullAlamat += string.Concat(" <i>email: ", kopsurat.Email, "</i>");
            }
            fullAlamat += "</p>";

            foreach (IElement element in XMLWorkerHelper.ParseToElementList(fullAlamat, "p { font-family: Arial Narrow; font-size: " + kopsurat.FontSize + "px; text-align: center; }"))
            {
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Padding = 0f;
                cell.AddElement(element);
                table.AddCell(cell);
                doc.Add(table);
            }

            doc.Add(objPdf.HorizontalLine());
            doc.Add(objPdf.AddLineSeparator(10f));

            // TITLE
            doc.Add(objPdf.AddTitleSurat(dokumen.TipeSurat.ToUpper(), Element.ALIGN_CENTER, 12f, Font.BOLD, 0f, 0f, 0f, "Bookman Old Style"));
            doc.Add(objPdf.AddTitleSurat("\n", Element.ALIGN_CENTER, 12f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));

            TextField nomorField = new TextField(pw, new Rectangle(200, 705, PageSize.A4.Width - 200, 685), "fnomor");
            nomorField.Text = "Nomor : ";
            nomorField.Alignment = Element.ALIGN_CENTER;
            var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\BOOKOS.TTF";
            nomorField.Font = BaseFont.CreateFont(fontPath, BaseFont.WINANSI, BaseFont.EMBEDDED);
            nomorField.FontSize = 12f;

            pw.AddAnnotation(nomorField.GetTextField());
            // Eof TITLE

            //----- BODY

            // line separator
            doc.Add(objPdf.AddLineSeparator(20f));

            // Penerima Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            foreach (var str in dokumen.Tujuan)
            {
                dokumen.Penerima += (string.IsNullOrEmpty(dokumen.Penerima)) ? "" : "\n";
                dokumen.Penerima += str;
            }
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Yth", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.Penerima) ? "<<Penerima Surat>>" : dokumen.Penerima, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Pengirim Surat

            dokumen.Pengirim = dataMasterModel.GetProfileNameFromId(dokumen.ProfilePengirim);
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Dari", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.Pengirim) ? "<<Pengirim Surat>>" : dokumen.Pengirim, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Tanggal Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Tanggal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            //table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.TanggalSurat) ? string.Concat("<<",DateTime.Now.ToString("dd MMMM yyyy"),">>") : dokumen.TanggalSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            float start = 50 + 65 + 18;
            float end = start + 100;
            TextField tanggalField = new TextField(pw, new Rectangle(start, 647.5f, end, 627.5f), "ftanggal");
            tanggalField.Text = "";
            tanggalField.Alignment = Element.ALIGN_LEFT;
            tanggalField.Font = BaseFont.CreateFont(fontPath, BaseFont.WINANSI, BaseFont.EMBEDDED);
            tanggalField.FontSize = 10f;
            pw.AddAnnotation(tanggalField.GetTextField());

            // Sifat Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Sifat", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.SifatSurat) ? "<<Biasa>>" : dokumen.SifatSurat, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Lampiran Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Lampiran", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.LampiranId) ? "-" : "1", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Hal Surat
            table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("Hal", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable(":", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.Perihal) ? "<<Ringkasan Pengenal Surat>>" : HttpUtility.UrlDecode(dokumen.Perihal), Element.ALIGN_JUSTIFIED, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 65f, 15f, 460f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);
            table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.HorizontalLine());
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 540f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // line separator
            doc.Add(objPdf.AddLineSeparator(15f));

            // Isi Surat
            string isisurat = HttpUtility.UrlDecode(dokumen.IsiSurat);
            isisurat = isisurat.Replace("<div", "<p");
            isisurat = isisurat.Replace("</div>", "</p>");
            isisurat = isisurat.Replace("<br>", "<br/>");
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
            string styles = "p { font-family: Bookman Old Style; font-size: 10pt; text-indent: 100px; text-align: justify; }";
            foreach (IElement element in XMLWorkerHelper.ParseToElementList(isisurat, styles))
            {
                table = new PdfPTable(3);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;
                table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Padding = 0f;
                cell.AddElement(element);
                table.AddCell(cell);
                table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
                columnWidths = new float[] { 50f, 540f, 50f };
                table.SetWidths(columnWidths);

                doc.Add(table);
            }

            // Penandatangan
            table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));

            tableIn = new PdfPTable(1);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.AddElement(objPdf.AddTitleSurat(dokumen.Pengirim, Element.ALIGN_CENTER, 10f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));
            tableIn.AddCell(cell);

            var penandatangan = dataMasterModel.GetPegawaiByProfileId(dokumen.ProfilePengirim)[0];
            string nama = penandatangan.NamaLengkap;
            cell = new PdfPCell();
            cell.Border = 0;
            cell.PaddingTop = 20f;
            cell.AddElement(objPdf.AddTitleSurat(nama, Element.ALIGN_CENTER, 10f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));
            tableIn.AddCell(cell);

            string nip = penandatangan.PegawaiId;
            cell = new PdfPCell();
            cell.Border = 0;
            cell.AddElement(objPdf.AddTitleSurat(string.Concat("NIP. ", nip), Element.ALIGN_CENTER, 10f, Font.NORMAL, 0f, 0f, 0f, "Bookman Old Style"));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);
            table.AddCell(objPdf.CreateCellTable("", Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, ""));
            columnWidths = new float[] { 50f, 240f, 300f, 50f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            // Tembusan Surat
            int n = 1;
            foreach (var str in dokumen.Tembusan)
            {
                dokumen.listTembusan += (string.IsNullOrEmpty(dokumen.listTembusan)) ? "" : "\n";
                dokumen.listTembusan += string.Concat(n.ToString(), ". ", str);
                n += 1;
            }
            if (dokumen.Tembusan.Count > 0)
            {
                dokumen.listTembusan = string.Concat("Tembusan : \n", dokumen.listTembusan);
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;
                table.AddCell(objPdf.CreateCellTable(string.IsNullOrEmpty(dokumen.listTembusan) ? "<<Tembusan Surat>>" : dokumen.listTembusan, Element.ALIGN_LEFT, Element.ALIGN_TOP, 0, 10f, Font.NORMAL, true, "Bookman Old Style"));
                columnWidths = new float[] { 640f };
                table.SetWidths(columnWidths);

                doc.Add(table);
            }

            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var resultAddFooter = new PDFBuilder().Build(
               streamPdf: mss,
               template: PDFBuilder.Template.FOOTER,
               pageTte: 0);

            //resultAddFooter = new PDFBuilder().BuildKalimatSambung(resultAddFooter.Output);

            var docfile = new FileStreamResult(
                resultAddFooter.Output,
                MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = string.Concat("DokumenElektronik", ".pdf");

            return docfile;
        }

        [HttpPost]
        public ActionResult CekNomorSurat(string nmr, string pengirim)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "error" };
            var usr = functions.claimUser();
            try
            {
                if (string.IsNullOrEmpty(nmr))
                {
                    result.Pesan = "Nomor Surat wajib diisi";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(pengirim))
                {
                    result.Pesan = "Asal Surat wajib diisi";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (mdl.cekNomorSuratGanda(nmr, pengirim))
                {
                    string kantorid = usr.KantorId;
                    string unitkerjaid = usr.UnitKerjaId;
                    string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
                    if (mdl.cekNomorSuratGanda(nmr, pengirim, satkerid))
                    {
                        result.Pesan = string.Format("Nomor Surat : ", nmr, "\nPengirim : ", pengirim, "\nsudah terdaftar didalam system");
                    }
                    else
                    {
                        result.Pesan = string.Format("Nomor Surat : ", nmr, "\nPengirim : ", pengirim, "\nsudah didaftarkan oleh unit lain");
                        result.ReturnValue = "ganda";
                    }
                }
                else
                {
                    result.Status = true;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSuratMasuk(Models.Entities.Surat data, string daftarTujuan, List<HttpPostedFileBase> fileUploadStream)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "", ReturnValue2 = "" };
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string profileidtu = usr.ProfileIdTU;
            string userid = usr.UserId;
            string pegawaiid = usr.PegawaiId;
            string namapegawaipengirim = usr.NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            data.SuratId = mdl.GetUID();
            data.UserId = userid;
            data.PegawaiId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            // Cek Tujuan Surat
            var dataSessionTujuanSurat = new List<SessionTujuanSurat>();
            try
            {
                dynamic json = JsonConvert.DeserializeObject(daftarTujuan);

                foreach (var js in json)
                {
                    dataSessionTujuanSurat.Add(JsonConvert.DeserializeObject<SessionTujuanSurat>(JsonConvert.SerializeObject(js.Value)));
                }
            }
            catch
            {
                tr.Pesan = "Data yang dikirimkan tidak sesuai";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            data.PenerimaSurat = "";
            foreach(var tj in dataSessionTujuanSurat)
            {
                if (tj.Redaksi.Equals("Asli"))
                {
                    data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat)?"":", ", tj.NamaJabatan);
                }
            }

            string judul = "Surat masuk untuk " + data.PenerimaSurat + " nomor: " + data.NomorSurat;

            var dataSessionLampiran = new List<SessionLampiranSurat>();
            int urutLampiran = 1;
            foreach (HttpPostedFileBase file in fileUploadStream)
            {
                if (file != null)
                {
                    var datafile = new SessionLampiranSurat();
                    if(file.FileName.Length > 100)
                    {
                        tr.Pesan = "Nama File Maksimal 100 karakter";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    datafile.NamaFile = urutLampiran.ToString() + "|" + file.FileName;
                    MemoryStream ms1 = new MemoryStream();
                    file.InputStream.CopyTo(ms1);
                    datafile.ObjectFile = ms1.ToArray();
                    datafile.LampiranSuratId = mdl.GetUID();
                    datafile.Nip = pegawaiid;
                    dataSessionLampiran.Add(datafile);
                    urutLampiran++;
                }
            }
            data.JumlahLampiran = dataSessionLampiran.Count;

            string isFileAttMandatory = ConfigurationManager.AppSettings["IsFileAttMandatory"].ToString();
            if (isFileAttMandatory == "true")
            {
                if (dataSessionLampiran.Count == 0)
                {
                    tr.Pesan = "File Surat wajib diupload";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }

            var listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            string suratid_duplikat = string.Empty;
            if (!string.IsNullOrEmpty(data.NomorSurat))
            {
                Regex sWhitespace = new Regex(@"[^0-9a-zA-Z-./]+");

                string nomorsurat = sWhitespace.Replace(data.NomorSurat, "");
                data.NomorSurat = nomorsurat;
                suratid_duplikat = mdl.GetSuratIdFromNomorSuratDanPengirim(data.NomorSurat, data.PengirimSurat);
            }

            #region Simpan File Fisik
            foreach (var lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId;

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("Surat"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranSurat.NamaFile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            tr = kontentmodel.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal Membuat Surat, ada file lampiran yang bermasalah\nHarap cek ulang lampiran anda.";
                            tr.ReturnValue = reqresult.ReasonPhrase;
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            #endregion

            if (string.IsNullOrEmpty(suratid_duplikat))
            {
                tr = mdl.InsertSuratMasuk(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim, dataSessionTujuanSurat, dataSessionLampiran);
            }
            else
            {
                data.SuratId = suratid_duplikat;
                tr = mdl.MergeSuratMasuk(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim, dataSessionTujuanSurat, dataSessionLampiran);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSuratKeluar(Models.Entities.Surat data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string profileidtu = usr.ProfileIdTU;
            string userid = usr.UserId;
            string pegawaiid = usr.PegawaiId;
            string namapegawaipengirim = usr.NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            data.SuratId = mdl.GetUID();
            data.UserId = userid;
            data.PegawaiId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            var listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            data.JumlahLampiran = data.ListFiles.Count;

            if (string.IsNullOrEmpty(data.NomorSurat))
            {
                tr.Pesan = "Nomor Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            data.PenerimaSurat = "";
            data.ListTujuanSurat = new List<SessionTujuanSurat>();
            if (data.ListTujuan != null)
            {
                foreach (var _tujuan in data.ListTujuan)
                {
                    string jabatan = mdl.GetNamaJabatan(_tujuan.ProfileId);
                    if (_tujuan.Redaksi.Equals("Asli"))
                    {
                        data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", jabatan);
                    }
                    data.ListTujuanSurat.Add(new SessionTujuanSurat()
                    {
                        ProfileId = _tujuan.ProfileId,
                        NIP = _tujuan.NIP,
                        Redaksi = _tujuan.Redaksi,
                        NamaJabatan = Server.UrlEncode(jabatan),
                        NamaPegawai = Server.UrlEncode(mdl.GetNamaPegawai(_tujuan.NIP))
                    });
                }
            }

            if (data.kirimPusat)
            {
                data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", "Seluruh Unit Kerja Pusat");
                data.ListTujuanSurat.AddRange(mdl.getTujuanMassal(1));
            }
            if (data.kirimKanwil)
            {
                data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", "Seluruh Kantor Wilayah");
                data.ListTujuanSurat.AddRange(mdl.getTujuanMassal(2));
            }
            if (data.kirimKantah)
            {
                data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", "Seluruh Kantor Pertanahan");
                data.ListTujuanSurat.AddRange(mdl.getTujuanMassal(3));
            }
            if (data.ListTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            string judul = "Surat keluar untuk " + data.PenerimaSurat + " nomor: " + data.NomorSurat;

            data.NomorSurat = Server.UrlEncode(data.NomorSurat);
            data.PenerimaSurat = data.PenerimaSurat.Length > 500 ? data.PenerimaSurat.Substring(0, 500) : data.PenerimaSurat;

            #region Simpan File Fisik

            foreach (var lampiranSurat in data.ListFiles)
            {
                if (lampiranSurat.ObjectFile != null && lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.FilesId = mdl.GetUID();

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("Surat"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranSurat.PengenalFile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            tr = kontentmodel.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = string.Concat("Gagal Membuat Surat, ada file lampiran yang bermasalah\nHarap cek ulang lampiran anda.\n", reqresult.ReasonPhrase);

                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            #endregion

            tr = mdl.InsertSuratKeluar(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSuratInisiatif(Models.Entities.Surat data, string daftarTujuan, List<HttpPostedFileBase> fileUploadStream)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "", ReturnValue2 = "" };
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string profileidtu = usr.ProfileIdTU;
            string pegawaiid = usr.PegawaiId;
            string namapegawaipengirim = usr.NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            data.UserId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            var dataSessionTujuanSurat = new List<SessionTujuanSurat>();
            try
            {
                dynamic json = JsonConvert.DeserializeObject(daftarTujuan);

                foreach (var js in json)
                {
                    dataSessionTujuanSurat.Add(JsonConvert.DeserializeObject<SessionTujuanSurat>(JsonConvert.SerializeObject(js.Value)));
                }
            }
            catch
            {
                tr.Pesan = "Data yang dikirimkan tidak sesuai";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            var dataSessionLampiran = new List<SessionLampiranSurat>();
            foreach (HttpPostedFileBase file in fileUploadStream)
            {
                if (file != null)
                {
                    var datafile = new SessionLampiranSurat();
                    datafile.NamaFile = file.FileName;
                    MemoryStream ms1 = new MemoryStream();
                    file.InputStream.CopyTo(ms1);
                    datafile.ObjectFile = ms1.ToArray();
                    datafile.LampiranSuratId = mdl.GetUID();
                    datafile.Nip = pegawaiid;
                    datafile.Ext = Path.GetExtension(file.FileName).Replace(".","");
                    dataSessionLampiran.Add(datafile);
                }
            }

            data.JumlahLampiran = dataSessionLampiran.Count;

            string judul = "Surat Inisiatif untuk " + data.PenerimaSurat;

            var listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            foreach (var lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId;

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

                    string namafile = lampiranSurat.NamaFile;
                    string fileExtension = lampiranSurat.Ext;

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    DateTime tglSunting = DateTime.Now;
                    string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("Surat"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranSurat.NamaFile);
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        content.Add(new StringContent(fileExtension), "fileExtension");
                    }

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            tr = kontentmodel.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal Membuat Surat, ada file lampiran yang bermasalah\nHarap cek ulang lampiran anda.";
                            tr.ReturnValue = reqresult.ReasonPhrase;
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            tr = mdl.InsertSuratInisiatif(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim, dataSessionTujuanSurat, dataSessionLampiran);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FileList(int? draw, int? start, int? length)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<Files>();
            decimal? total = 0;
            string metadata = Server.UrlEncode(Request.Form["metadata"].ToString());
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string userid = usr.UserId;
            string profiletu = usr.ProfileIdTU;
            bool isTU = OtorisasiUser.isTU();

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;

                result = mdl.getListDokumenTTE(userid, profiletu, kantorid, isTU, metadata, from, to);
                foreach(var r in result)
                {
                    r.PengenalFile = Server.UrlDecode(r.PengenalFile);
                    r.Keterangan = Server.UrlDecode(Server.UrlDecode(r.Keterangan));
                }

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFilePdf(string id, string kd, string tp, string nm)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string ext = new SuratModel().GetExt(id);
                ext = string.IsNullOrEmpty(ext) ? ".pdf" : ext;
                string kantorid = kd;
                if (string.IsNullOrEmpty(kantorid))
                {
                    kantorid = usr.KantorId;
                }
                string tipe = tp;
                string versi = new KontentModel().CekVersi(id).ToString();

                if (kantorid.Length < 32)
                {
                    kantorid = dataMasterModel.GetKantorIdFromUnitKerjaId(kantorid);
                }

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(ext), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            if (tp.Equals("Surat"))
                            {
                                docfile.FileDownloadName = string.Concat(nm);
                            }
                            else
                            {
                                docfile.FileDownloadName = string.Concat(nm, ".pdf");
                            }
                            result.Status = true;
                            result.StreamResult = docfile;

                            return docfile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Pesan = ex.Message;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BukaSurat(string suratid, string suratinboxid, string nomorsurat, string arah)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            if(usr == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (!string.IsNullOrEmpty(suratinboxid))
            {
                string referensiUnitKerjaId = "";

                ViewBag.UnitKerjaId = usr.UnitKerjaId;

                string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

                var surat = mdl.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid, usr.PegawaiId);
                if(surat == null)
                {
                    if (string.IsNullOrEmpty(arah))
                    {
                        return View("Index", "Home");
                    }
                    else
                    {
                        if (arah.ToLower().Equals("masuk"))
                        {
                            return RedirectToAction("KotakMasuk", "Surat");
                        }
                        else if (arah.ToLower().Equals("keluar"))
                        {
                            return RedirectToAction("KotakTerkirim", "Surat");
                        }
                        else if (arah.ToLower().Equals("inisiatif"))
                        {
                            return RedirectToAction("KotakInisiatif", "Surat");
                        }
                        else
                        {
                            return View("Index", "Home");
                        }
                    }
                }
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
                surat.ListPerintahDisposisi = persuratanmodel.GetPerintahDisposisi();
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListUnitKerjaHistoriSurat = persuratanmodel.GetUnitKerjaSuratHistory(suratid);
                surat.ListProfileTujuan = new List<Profile>();
                surat.ListTujuanPegawai = new List<Pegawai>();
                surat.CatatanSebelumnya = mdl.GetCatatanSebelumnya(suratinboxid,satkerid);
                surat.PerintahDisposisiSebelumnya = persuratanmodel.GetDisposisiSebelumnya(suratinboxid);
                surat.IsBedaUnitKerja = "0";
                if (string.IsNullOrEmpty(arah))
                {
                    surat.LabelTitleJenisSurat = "SURAT MASUK";
                }
                else
                {
                    if (arah.ToLower().Equals("masuk"))
                    {
                        surat.LabelTitleJenisSurat = "SURAT MASUK";
                    }
                    else if (arah.ToLower().Equals("keluar"))
                    {
                        surat.LabelTitleJenisSurat = "SURAT KELUAR";
                    }
                    else if (arah.ToLower().Equals("inisiatif"))
                    {
                        surat.LabelTitleJenisSurat = "SURAT INISIATIF";
                    }
                }

                if (!string.IsNullOrEmpty(surat.ReferensiSurat))
                {
                    // ambil data surat sebagai referensi
                    var referensisurat = persuratanmodel.GetSuratBySuratId(surat.ReferensiSurat, satkerid);
                    surat.ReferensiAsalSurat = referensisurat.PengirimSurat;
                    surat.ReferensiTujuanSurat = referensisurat.PenerimaSurat;
                    surat.ReferensiNomorAgenda = referensisurat.NomorAgenda;
                    surat.ReferensiNomorSurat = referensisurat.NomorSurat;
                    surat.ReferensiTanggalSurat = referensisurat.TanggalSurat;
                    surat.ReferensiPerihal = referensisurat.Perihal;
                    surat.ReferensiKategori = referensisurat.Kategori;
                    surat.ReferensiUnitKerjaId = referensisurat.ReferensiUnitKerjaId;

                    if (referensiUnitKerjaId != usr.UnitKerjaId)
                    {
                        surat.IsBedaUnitKerja = "1";
                        surat.LabelTitleJenisSurat = "SURAT MASUK";
                    }
                }

                if (surat.KeteranganSuratRedaksi.ToLower().Contains("tembusan"))
                {
                    surat.PenanggungJawab = persuratanmodel.GetPenanggungJawab(suratid, satkerid, usr.PegawaiId);
                }

                // Update Flag Buka Surat
                persuratanmodel.BukaSuratInbox(surat.SuratId, surat.SuratInboxId, usr.PegawaiId, usr.NamaPegawai);
                surat.Waktu = persuratanmodel.GetWaktuProses(suratinboxid);
                surat.WaktuTunggak = persuratanmodel.GetWaktuTunggak(suratinboxid);
                surat.ListKantor = dataMasterModel.GetAllKantor();

                return View("ViewSurat", surat);
            }
            else
            {
                if (string.IsNullOrEmpty(arah))
                {
                    return View("Index", "Home");
                }
                else
                {
                    if (arah.ToLower().Equals("masuk"))
                    {
                        return RedirectToAction("KotakMasuk", "Surat");
                    }
                    else if (arah.ToLower().Equals("keluar"))
                    {
                        return RedirectToAction("KotakTerkirim", "Surat");
                    }
                    else if (arah.ToLower().Equals("inisiatif"))
                    {
                        return RedirectToAction("KotakInisiatif", "Surat");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        public async Task<ActionResult> GetFileSuratWithExt(string id, string kantorid, string namafile, string extension)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new { Status = false, Message = "" };
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                if (string.IsNullOrEmpty(kantorid))
                {
                    kantorid = usr.KantorId;
                }
                string tipe = "Surat";
                string versi = kontentmodel.CekVersi(id).ToString();

                var tglSunting = persuratanmodel.getTglSunting(id, tipe);
                string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);
                string ext = new Models.SuratModel().GetExt(id);
                ext = string.IsNullOrEmpty(ext) ? extension : ext;

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(ext), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = namafile;
                            if (extension == ".jpg")
                            {
                                docfile = new FileStreamResult(strm, "image/jpeg");
                            }
                            else if (extension == ".png")
                            {
                                docfile = new FileStreamResult(strm, "image/png");
                            }
                            else if (extension == ".xls")
                            {
                                docfile = new FileStreamResult(strm, "application/vnd.ms-excel");
                            }
                            else if (extension == ".xlsx")
                            {
                                docfile = new FileStreamResult(strm, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                            }
                            else if (extension == ".doc")
                            {
                                docfile = new FileStreamResult(strm, "application/msword");
                            }
                            else if (extension == ".docx")
                            {
                                docfile = new FileStreamResult(strm, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                            }
                            return docfile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new { Status = false, Message = ex.Message };
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSurat(int? start, int? length, FindSurat f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;
            var usr = functions.claimUser();
            string pegawaiid = usr.PegawaiId;
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;

            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
            string nomorsurat = f.NomorSurat;
            string nomoragenda = f.NomorAgenda;
            string perihal = f.Perihal;
            string tanggalsurat = f.TanggalSurat;
            string tipesurat = f.TipeSurat;
            string sifatsurat = f.SifatSurat;
            string sortby = f.SortBy;
            string sorttype = f.SortType;
            string spesificprofileid = f.SpesificProfileId;
            string sumber = f.Sumber_Keterangan;
            string arah = f.Arah;

            var result = mdl.GetDaftarSurat(satkerid, pegawaiid, "1", arah, myProfiles, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, spesificprofileid, from, to, sumber);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                    dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                    dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
                    if (string.IsNullOrEmpty(dt.NomorAgendaSurat))
                    {
                        var tr = mdl.GetNomorAgenda(dt.SuratId, satkerid, unitkerjaid, dt.SuratInboxId);
                        if (tr.Status)
                            dt.NomorAgendaSurat = tr.ReturnValue;
                    }
                }
                total = result[0].Total;
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KirimDisposisi(Models.Entities.Surat data, string daftarTujuan)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string profileidtu = usr.ProfileIdTU;
            string pegawaiid = usr.PegawaiId;
            string namapengirim = usr.NamaPegawai;

            data.UserId = pegawaiid;
            data.NamaPengirim = namapengirim;

            // Cek Tujuan Surat
            var dataSessionTujuanSurat = new List<SessionTujuanSurat>();
            try
            {
                dynamic json = JsonConvert.DeserializeObject(daftarTujuan);

                foreach (var js in json)
                {
                    dataSessionTujuanSurat.Add(JsonConvert.DeserializeObject<SessionTujuanSurat>(JsonConvert.SerializeObject(js.Value)));
                }
            }
            catch
            {
                tr.Pesan = "Data yang dikirimkan tidak sesuai";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            var listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            tr = mdl.KirimDisposisi(data, kantorid, unitkerjaid, profileidtu, pegawaiid, dataSessionTujuanSurat);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult KotakMasuk(string t, string p)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var find = new FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListMyProfiles = dataMasterModel.GetProfilePegawai_Simpeg(usr.PegawaiId, usr.KantorId);
            find.JumlahMyProfiles = find.ListMyProfiles.Count;

            //if (find.JumlahMyProfiles == 1)
            //{
            //    find.SpesificProfileId = string.IsNullOrEmpty(p) ? find.ListMyProfiles[0].ProfileId : p;
            //}
            find.SpesificProfileId = string.IsNullOrEmpty(p) ? find.ListMyProfiles[0].ProfileId : p;
            find.Arah = "Masuk";
            find.Sumber_Keterangan = (string.IsNullOrEmpty(t)) ? "Loket" : t;

            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

            //listTipeSurat           
            ViewBag.lstTipeSurat = persuratanmodel.GetTipeSurat(satkerid, usr.PegawaiId, find.Arah, find.SpesificProfileId);

            //jumlahKotakSurat
            ViewBag.jumlahKotakSurat = persuratanmodel.JumlahSuratBySumber(satkerid, usr.PegawaiId, find.SpesificProfileId, find.Arah);

            return View("KotakSurat",find);
        }

        public JsonResult jumlahKotakSurat(string profileid, string arah)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
            var list = persuratanmodel.JumlahSuratBySumber(satkerid, usr.PegawaiId, profileid, arah);
            return Json(new { Email = list.Email, Loket = list.Loket, Internal = list.Internal}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult KotakTerkirim()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var find = new FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListMyProfiles = dataMasterModel.GetProfilePegawai_Simpeg(usr.PegawaiId, usr.KantorId);
            find.JumlahMyProfiles = find.ListMyProfiles.Count;
            if (find.JumlahMyProfiles == 1)
            {
                find.SpesificProfileId = find.ListMyProfiles[0].ProfileId;
            }
            find.Arah = "Keluar";
            return View("KotakSurat", find);
        }

        public ActionResult KotakInisiatif()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var find = new FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListMyProfiles = dataMasterModel.GetProfilePegawai_Simpeg(usr.PegawaiId, usr.KantorId);
            find.JumlahMyProfiles = find.ListMyProfiles.Count;
            if (find.JumlahMyProfiles == 1)
            {
                find.SpesificProfileId = find.ListMyProfiles[0].ProfileId;
            }
            find.Arah = "Inisiatif";
            return View("KotakSurat", find);
        }

        public ActionResult DaftarLampiranSurat(string suratid)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
            var result = mdl.GetListLampiranSurat(suratid, satkerid);

            int custIndex = 1;
            Dictionary<int, LampiranSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarLampiranSurat", dict);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "noresults",
                    ContentEncoding = System.Text.Encoding.UTF8
                };
            }
        }

        public ActionResult BuatSuratJawaban(string reff)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var persuratanmodel = new PersuratanModel();
            var usr = functions.claimUser();
            if (usr != null && usr.UnitKerjaId.Equals("020116"))
            {
                var data = new Models.Entities.Surat();
                if (!string.IsNullOrEmpty(reff))
                {
                    data.Referensi = reff;
                    string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
                    var referensisurat = persuratanmodel.GetSuratBySuratId(reff, satkerid);
                    data.ReferensiAsalSurat = referensisurat.PengirimSurat;
                    data.ReferensiTujuanSurat = referensisurat.PenerimaSurat;
                    data.ReferensiNomorAgenda = referensisurat.NomorAgenda;
                    data.ReferensiNomorSurat = Server.UrlDecode(referensisurat.NomorSurat);
                    data.ReferensiTanggalSurat = referensisurat.TanggalSurat;
                    data.ReferensiPerihal = referensisurat.Perihal;
                    data.ReferensiKategori = referensisurat.Kategori;
                    data.ReferensiUnitKerjaId = referensisurat.ReferensiUnitKerjaId;

                    data.ListSifatSurat = persuratanmodel.GetSifatSurat();
                    data.ListTipeSurat = persuratanmodel.GetTipeSurat();
                    data.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                    data.ListProfileTujuan = new List<Profile>();
                    data.ListTujuanPegawai = new List<Pegawai>();
                    data.ListProfiles = new List<Profile>();
                    ViewBag.Massal = usr.KantorId.Equals("980FECFC746D8C80E0400B0A9214067D");
                    return View(data);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult InsertSuratJawaban(Models.Entities.Surat data, string daftarTujuan, string tujuanSurat, bool kirimPusat, bool kirimKanwil, bool kirimKantah, List<HttpPostedFileBase> fileUploadStream)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string profileidtu = usr.ProfileIdTU;
            string userid = usr.UserId;
            string pegawaiid = usr.PegawaiId;
            string namapegawaipengirim = usr.NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            data.SuratId = mdl.GetUID();
            data.UserId = userid;
            data.PegawaiId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            var listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            if (string.IsNullOrEmpty(data.NomorSurat))
            {
                tr.Pesan = "Nomor Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            data.ArahSuratKeluar = "Masuk";

            data.PenerimaSurat = "";
            data.ListTujuanSurat = new List<SessionTujuanSurat>();
            try
            {
                dynamic json = JsonConvert.DeserializeObject(daftarTujuan);

                foreach (var js in json)
                {
                    var _tujuan = new SessionTujuanSurat();
                    _tujuan = JsonConvert.DeserializeObject<SessionTujuanSurat>(JsonConvert.SerializeObject(js.Value));
                    if (_tujuan.Redaksi.Equals("Asli"))
                    {
                        data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", _tujuan.NamaJabatan);
                    }
                    data.ListTujuanSurat.Add(new SessionTujuanSurat()
                    {
                        ProfileId = _tujuan.ProfileId,
                        NIP = _tujuan.NIP,
                        Redaksi = _tujuan.Redaksi,
                        NamaJabatan = Server.UrlEncode(mdl.GetNamaJabatan(_tujuan.ProfileId)),
                        NamaPegawai = Server.UrlEncode(mdl.GetNamaPegawai(_tujuan.NIP))
                    });
                }
            }
            catch
            {
                tr.Pesan = "Data yang dikirimkan tidak sesuai";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            if (kirimPusat)
            {
                data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", "Seluruh Unit Kerja Pusat");
                data.ListTujuanSurat.AddRange(mdl.getTujuanMassal(1));
            }
            if (kirimKanwil)
            {
                data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", "Seluruh Kantor Wilayah");
                data.ListTujuanSurat.AddRange(mdl.getTujuanMassal(2));
            }
            if (kirimKantah)
            {
                data.PenerimaSurat = string.Concat(data.PenerimaSurat, string.IsNullOrEmpty(data.PenerimaSurat) ? "" : ", ", "Seluruh Kantor Pertanahan");
                data.ListTujuanSurat.AddRange(mdl.getTujuanMassal(3));
            }

            if (data.ListTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            string judul = "Surat keluar untuk " + data.PenerimaSurat + " nomor: " + data.NomorSurat;

            data.NomorSurat = Server.UrlEncode(data.NomorSurat);
            data.PenerimaSurat = data.PenerimaSurat.Length > 500 ? data.PenerimaSurat.Substring(0, 500) : data.PenerimaSurat;

            data.ListFiles = new List<Files>();
            var dataSessionLampiran = new List<SessionLampiranSurat>();
            int urutLampiran = 1;
            foreach (HttpPostedFileBase file in fileUploadStream)
            {
                if (file != null)
                {
                    var datafile = new SessionLampiranSurat();
                    if (file.FileName.Length > 100)
                    {
                        tr.Pesan = "Nama File Maksimal 100 karakter";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    datafile.NamaFile = urutLampiran.ToString() + "|" + file.FileName;
                    MemoryStream ms1 = new MemoryStream();
                    file.InputStream.CopyTo(ms1);
                    datafile.ObjectFile = ms1.ToArray();
                    datafile.LampiranSuratId = mdl.GetUID();
                    datafile.Nip = pegawaiid;
                    dataSessionLampiran.Add(datafile);
                    data.ListFiles.Add(new Files()
                    {
                        FilesId = datafile.LampiranSuratId,
                        PengenalFile = file.FileName
                    });
                    urutLampiran++;
                }
            }
            data.JumlahLampiran = dataSessionLampiran.Count;

            if(data.JumlahLampiran.Equals(0))
            {
                tr.Pesan = "File Surat Tidak Ditemukan";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            #region Simpan File Fisik
            foreach (var lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId;

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("Surat"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranSurat.NamaFile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            tr = kontentmodel.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = string.Concat("Gagal Membuat Surat, ada file lampiran yang bermasalah\nHarap cek ulang lampiran anda.\n", reqresult.ReasonPhrase);
                            tr.ReturnValue = reqresult.ReasonPhrase;
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            #endregion

            tr = mdl.InsertSuratKeluar(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult KembalikanSurat(SuratKembali data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            try
            {
                data.UserId = usr.UserId;

                result = mdl.KembalikanSurat(data);
                if (!result.Status)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SuratOutbox()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var find = new FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View("SuratTerkirim",find);
        }

        public ActionResult KirimUlangSurat(string suratid, string nomorsurat)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            string suratinboxid = persuratanmodel.GetSuratInboxIdFromSuratId(suratid);
            var usr = functions.claimUser();
            if (!string.IsNullOrEmpty(suratinboxid))
            {
                string pegawaiid = usr.PegawaiId;
                string unitkerjaid = usr.UnitKerjaId;

                ViewBag.UnitKerjaId = unitkerjaid;

                string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

                var surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid);
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
                surat.ListPerintahDisposisi = persuratanmodel.GetPerintahDisposisi();
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListProfileTujuan = new List<Profile>();
                surat.ListTujuanPegawai = new List<Pegawai>();
                surat.ListProfiles = new List<Profile>();
                surat.CatatanSebelumnya = persuratanmodel.GetCatatanSebelumnya(suratinboxid);

                if (string.IsNullOrEmpty(surat.TanggalTerima))
                {
                    surat.TanggalTerima = persuratanmodel.GetServerDate().ToString("dd/MM/yyyy HH:mm");
                }

                ViewBag.IsProfileTataUsaha = "0";

                string ismyprofiletu = dataMasterModel.GetIsMyProfileTU(pegawaiid);
                if (ismyprofiletu == "1")
                {
                    ViewBag.IsProfileTataUsaha = "1";
                }

                return View("KirimKembali", surat);
            }
            else
            {
                return RedirectToAction("SuratOutbox", "Surat");
            }
        }

        #region PengantarSuratMasuk

        public ActionResult PengantarSuratMasuk()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            ViewBag.userid = functions.claimUser().UserId;
            return View();
        }

        public ActionResult BuatPengantarSuratMasuk(string pengantarsuratid)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
            List<UnitKerja> lstUnitkerja = new List<UnitKerja>();
            var lst = new UnitKerja();
            if (tipekantorid == 1)
            {
                lst.UnitKerjaId = "H0000001";
                lst.NamaUnitKerja = "Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional";
                lstUnitkerja.Add(lst);
                lst = new UnitKerja();
                lst.UnitKerjaId = "H0000002";
                lst.NamaUnitKerja = "Wakil Menteri Agraria dan Tata Ruang/Wakil Kepala Badan Pertanahan Nasional";
                lstUnitkerja.Add(lst);
            }
            var units = dataMasterModel.GetListUnitKerjaByKantorId(usr.KantorId, true, false);
            lstUnitkerja.AddRange(units);

            ViewBag.ListUnitkerja = lstUnitkerja;
            ViewBag.ListPenandatangan = mdl.GetProfilesByUnitKerja(usr.UnitKerjaId);
            ViewBag.ListPetugasEntri = mdl.GetPetugasSuratMasukByUnitKerja(usr.UnitKerjaId);
            ViewBag.ListSuratTersimpan = new List<Models.Entities.Surat>();
            var Data = new PengantarSurat();
            if (!string.IsNullOrEmpty(pengantarsuratid))
            {
                Data = mdl.GetNewPengantarSurat(pengantarsuratid);
                if (Data.UnitKerjaId == usr.UnitKerjaId || OtorisasiUser.isTU())
                {
                    List<Models.Entities.Surat> lstSurat = new List<Models.Entities.Surat>();
                    string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
                    foreach (var d in Data.LstSurat.Split('|'))
                    {
                        var findsurat = persuratanmodel.GetSuratBySuratId(d, satkerid);
                        lstSurat.Add(findsurat);
                    }
                    ViewBag.ListSuratTersimpan = lstSurat;
                }
                else
                {
                    RedirectToAction("PengantarSuratMasuk", "Surat");
                }
            }

            return View(Data);
        }

        public JsonResult HapusSuratPengantar(string psid)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var Data = mdl.GetNewPengantarSurat(psid);
            var usrid = functions.claimUser().UserId;
            bool status = false;
            string mssg = "";
            if (Data.UserId == usrid || OtorisasiUser.isTU())
            {
                mssg = mdl.HapusSuratPengantar(psid, functions.claimUser().UnitKerjaId);
                if (mssg == "berhasil")
                {
                    status = true;
                }
            }
            else
            {
                mssg = "Anda Tidak Dapat Melakukan Aksi Tersebut";
            }
            return Json(new { Status = status, pesan = mssg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSuratPengantar(int? draw, int? start, int? length, string srchkey)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string useriunitkerjaid = functions.claimUser().UnitKerjaId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            List<PengantarSurat> result = mdl.GetListPengantar(from, to, unitkerjaid: useriunitkerjaid, nomorpengantar: srchkey);
            decimal? total = 0;
            if (result.Count > 0)
            {
                foreach (var r in result)
                {
                    if (r.ProfileIdTujuan == "H0000001")
                    {
                        r.TujuanSurat = "Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional";
                    }
                    else if (r.ProfileIdTujuan == "H0000002")
                    {
                        r.TujuanSurat = "Wakil Menteri Agraria dan Tata Ruang/Wakil Kepala Badan Pertanahan Nasional";
                    }
                    else
                    {
                        r.TujuanSurat = dataMasterModel.GetNamaUnitKerjaById(r.ProfileIdTujuan);
                    }

                    if (r.UserId == usr.UserId)
                    {
                        r.Status = "Pembuat";
                    }
                    else
                    {
                        r.Status = "None";
                    }

                    r.NamaPembuat = new InternalUser().GetNamaPengguna(r.UserId);
                    var doktte = new NaskahDinasModel().fDokTTEPengantarSuratMasuk(r.PengantarSuratId);
                    if (doktte != null)
                    {
                        r.StatusTTE = !string.IsNullOrEmpty(doktte.DokumenElektronikId) ? $"{doktte.Status}|{doktte.DokumenElektronikId}" : "";
                    }
                }

                total = result[0].Total > 50 ? 50 : result[0].Total;
            }


            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSuratByPengirim(string namapegawai, string ukidPenerima, string tanggal)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            bool ismenteri = false;
            if (ukidPenerima == "H0000001" || ukidPenerima == "H0000002")
            {
                ismenteri = true;
            }
            var profileidTu = mdl.GetProfileidTuFromUnitKerja(ukidPenerima, ismenteri);

            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

            string[] parapengirim = namapegawai.Split('|');
            List<Models.Entities.Surat> result = new List<Models.Entities.Surat>();
            foreach (var pengirim in parapengirim)
            {
                result.AddRange(mdl.GetListSuratForPengantar(pengirim, profileidTu, tanggal));
            }

            foreach (var surat in result)
            {
                var findsurat = persuratanmodel.GetSuratBySuratId(surat.SuratId, satkerid);
                surat.TanggalSurat = !string.IsNullOrEmpty(findsurat.TanggalSurat) ? findsurat.TanggalSurat : "";
                surat.Perihal = !string.IsNullOrEmpty(findsurat.Perihal) ? findsurat.Perihal : "";
                surat.NomorSurat = !string.IsNullOrEmpty(findsurat.NomorSurat) ? findsurat.NomorSurat : "";
                surat.PengirimSurat = !string.IsNullOrEmpty(findsurat.PengirimSurat) ? findsurat.PengirimSurat : "";
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SimpanSuratPengantar(Models.Entities.PengantarSurat ps)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            ps.UserId =usr.UserId;
            ps.UnitKerjaId = usr.UnitKerjaId;
            var result = new TransactionResult() { Status = false, Pesan = "data gagal disimpan" };
            string mssg = string.Empty;
            if (string.IsNullOrEmpty(ps.NomorSurat))
            {
                if (ps.Gnumber)
                {
                    var nomorakhir = new NaskahDinasModel().nomorAkhirPengantar(ps.UnitKerjaId);
                    if (nomorakhir == 0)
                    {
                        mssg = "Terdapat Perbedaan Format pada nomor sebelumnya";
                        return Json(new { Status = result.Status, pesan = mssg }, JsonRequestBehavior.AllowGet);
                    }
                    ps.NomorSurat = $"{nomorakhir + 1}/P-100.5.1/{new Models.NaskahDinasModel().ToRoman(DateTime.Now.Month)}/{DateTime.Now.Year.ToString()}";
                    result = mdl.SimpanSuratPengantar(ps);
                    mssg = result.Pesan;
                }
                else

                {
                    mssg = "Nomor Surat Kosong";
                    return Json(new { Status = result.Status, pesan = mssg }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                result = mdl.SimpanSuratPengantar(ps);
                mssg = result.Pesan;
            }
            return Json(new { Status = result.Status, pesan = mssg, psid = result.ReturnValue }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}