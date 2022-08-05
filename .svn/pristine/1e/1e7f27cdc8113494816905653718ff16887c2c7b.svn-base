using iText.Kernel.Events;
//using iText.Kernel.Geom;
using iText.Kernel.Utils;
using iText.Layout;
using PDFEditor;
using Surat.Models;
using Surat.Models.Entities;
using Surat.Codes;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using iTextSharp.text;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using PDFEditor;
using Newtonsoft.Json;
using static iTextSharp.text.Font;



namespace Surat.Controllers
{
    public class KearsipanController : Controller
    {
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        KearsipanModel km = new KearsipanModel();
        NaskahDinasModel nd = new NaskahDinasModel();
        SuratModel mdl = new SuratModel();
        TandaTanganElektronikModel TTEM = new TandaTanganElektronikModel();
        Functions functions = new Functions();



        // GET: Kearsipan

        public ActionResult MenuKearsipan()
        {
            return View();
        }

        /// MENU MASTER ARSIP /// - 12 April 2022 FIX

        public async Task<ActionResult> DaftarSemuaArsip(int? pageNum, Models.Entities.MasterArsip f)
        {


            if (!(OtorisasiUser.IsProfileAdminKearsipan() || OtorisasiUser.isTU()))
            {
                return RedirectToAction("Index", "Home");
            }

            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 10;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string metadata = HttpUtility.UrlEncode(f.MetaData);
            string golonganarsip = HttpUtility.UrlEncode(f.GolonganArsip);
            string tahun = f.Tahun;
            string unitkerja = functions.claimUser().UnitKerjaId;

            List<Models.Entities.MasterArsip> result = km.GetListSemuaArsip(metadata, golonganarsip, tahun, from, to, unitkerja);

            foreach (var dt in result)
            {
                dt.NomorSK = string.IsNullOrEmpty(dt.NomorSK) ? dt.NomorSK : HttpUtility.UrlDecode(dt.NomorSK);
                dt.JenisArsip = string.IsNullOrEmpty(dt.JenisArsip) ? dt.JenisArsip : HttpUtility.UrlDecode(dt.JenisArsip);
                dt.JumlahBerkas = string.IsNullOrEmpty(dt.JumlahBerkas) ? dt.JumlahBerkas : HttpUtility.UrlDecode(dt.JumlahBerkas);
                dt.Perkembangan = string.IsNullOrEmpty(dt.Perkembangan) ? dt.Perkembangan : HttpUtility.UrlDecode(dt.Perkembangan);
                dt.Gedung = string.IsNullOrEmpty(dt.Gedung) ? dt.Gedung : HttpUtility.UrlDecode(dt.Gedung);
                dt.Lantai = string.IsNullOrEmpty(dt.Lantai) ? dt.Lantai : HttpUtility.UrlDecode(dt.Lantai);
                dt.Rak = string.IsNullOrEmpty(dt.Rak) ? dt.Rak : HttpUtility.UrlDecode(dt.Rak);
                dt.NomorBoks = string.IsNullOrEmpty(dt.NomorBoks) ? dt.NomorBoks : HttpUtility.UrlDecode(dt.NomorBoks);
                dt.Keterangan = string.IsNullOrEmpty(dt.Keterangan) ? dt.Keterangan : HttpUtility.UrlDecode(dt.Keterangan);
            }
            int custIndex = from;
            Dictionary<int, Models.Entities.MasterArsip> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSemuaArsip", dict);
                }
                else
                {
                    return RedirectToAction("DaftarArsip", "Kearsipan");
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
        public async Task<ActionResult> DaftarSemuaArsipExport(int? pageNum, Models.Entities.MasterArsip f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 10;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string metadata = HttpUtility.UrlEncode(f.MetaData);
            string golonganarsip = HttpUtility.UrlEncode(f.GolonganArsip);
            string tahun = f.Tahun;
            string unitkerja = functions.claimUser().UnitKerjaId;

            List<Models.Entities.MasterArsip> result = km.GetListSemuaArsipExport(metadata, golonganarsip, tahun, from, to, unitkerja);

            foreach (var dt in result)
            {
                dt.NomorSK = string.IsNullOrEmpty(dt.NomorSK) ? dt.NomorSK : HttpUtility.UrlDecode(dt.NomorSK);
                dt.JenisArsip = string.IsNullOrEmpty(dt.JenisArsip) ? dt.JenisArsip : HttpUtility.UrlDecode(dt.JenisArsip);
                dt.JumlahBerkas = string.IsNullOrEmpty(dt.JumlahBerkas) ? dt.JumlahBerkas : HttpUtility.UrlDecode(dt.JumlahBerkas);
                dt.Perkembangan = string.IsNullOrEmpty(dt.Perkembangan) ? dt.Perkembangan : HttpUtility.UrlDecode(dt.Perkembangan);
                dt.Gedung = string.IsNullOrEmpty(dt.Gedung) ? dt.Gedung : HttpUtility.UrlDecode(dt.Gedung);
                dt.Lantai = string.IsNullOrEmpty(dt.Lantai) ? dt.Lantai : HttpUtility.UrlDecode(dt.Lantai);
                dt.Rak = string.IsNullOrEmpty(dt.Rak) ? dt.Rak : HttpUtility.UrlDecode(dt.Rak);
                dt.NomorBoks = string.IsNullOrEmpty(dt.NomorBoks) ? dt.NomorBoks : HttpUtility.UrlDecode(dt.NomorBoks);
                dt.Keterangan = string.IsNullOrEmpty(dt.Keterangan) ? dt.Keterangan : HttpUtility.UrlDecode(dt.Keterangan);
            }
            int custIndex = from;
            Dictionary<int, Models.Entities.MasterArsip> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSemuaArsipExport", dict);
                }
                else
                {
                    return RedirectToAction("DaftarArsip", "Kearsipan");
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
        public async Task<ActionResult> DaftarArsip()
        {

            Models.Entities.MasterArsip data = new Models.Entities.MasterArsip();

            if (!(OtorisasiUser.IsProfileAdminKearsipan() || OtorisasiUser.isTU()))
            {
                return RedirectToAction("Index", "Home");
            }

            string unitkerja = functions.claimUser().UnitKerjaId;

            data.ListGolonganMasterArsip = km.GetGolonganMasterArsip();
            data.ListGolonganMasterArsip2 = km.GetGolonganMasterArsip2(unitkerja);
            var data1 = data.ListGolonganMasterArsip;
            foreach (var dt in data1)
            {
                dt.NamaGolongan = Server.UrlDecode(dt.NamaGolongan);
            }
            var data3 = data.ListGolonganMasterArsip2;
            foreach (var dt in data3)
            {
                dt.NamaJenis = Server.UrlDecode(dt.NamaJenis);
                dt.ValueJenisArsip = Server.UrlDecode(dt.ValueJenisArsip);
            }

            return View(data);
        }
        public async Task<ActionResult> MasterArsip()
        {
            Models.Entities.MasterArsip data = new Models.Entities.MasterArsip();

            if (!(OtorisasiUser.IsProfileAdminKearsipan() || OtorisasiUser.isTU()))
            {
                return RedirectToAction("Index", "Home");
            }
            string unitkerja = functions.claimUser().UnitKerjaId;
            data.ListGolonganMasterArsip = km.GetGolonganMasterArsip();
            data.ListGolonganMasterArsip2 = km.GetGolonganMasterArsip2(unitkerja);
            var data1 = data.ListGolonganMasterArsip;
            foreach (var dt in data1)
            {
                dt.NamaGolongan = Server.UrlDecode(dt.NamaGolongan);
            }
            var data3 = data.ListGolonganMasterArsip2;
            foreach (var dt in data3)
            {
                dt.NamaJenis = Server.UrlDecode(dt.NamaJenis);
                dt.ValueJenisArsip = Server.UrlDecode(dt.ValueJenisArsip);
            }
            data.ListKlasifikasiMasterArsip = km.GetKlasifikasiMasterArsip();
            data.NomorSK = Server.UrlDecode(data.NomorSK);
            data.JenisArsip = Server.UrlDecode(data.JenisArsip);
            data.JumlahBerkas = Server.UrlDecode(data.JumlahBerkas);
            data.Perkembangan = Server.UrlDecode(data.Perkembangan);
            data.Gedung = Server.UrlDecode(data.Gedung);
            data.Lantai = Server.UrlDecode(data.Lantai);
            data.Rak = Server.UrlDecode(data.Rak);
            data.NomorBoks = Server.UrlDecode(data.NomorBoks);
            data.Keterangan = Server.UrlDecode(data.Keterangan);
            data.GolonganArsip = Server.UrlDecode(data.GolonganArsip);

            data.ListMasterArsipAll = km.GetMasterArsip();
            data.ListKeteranganDetailArsip = km.GetKeteranganDetailArsip();


            return View(data);

        }
        public async Task<ActionResult> ListMasterArsip(int? start, int? length, Models.Entities.MasterArsip f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            decimal? total = 0;

            string Tahun = f.Tahun;
            string GolonganArsip = HttpUtility.UrlEncode(f.GolonganArsip);
            string MetaData = f.MetaData;
            string unitkerja = functions.claimUser().UnitKerjaId;


            var result = km.GetListMasterArsip(Tahun, GolonganArsip, MetaData, from, to, unitkerja);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.NomorSK = string.IsNullOrEmpty(dt.NomorSK) ? dt.NomorSK : HttpUtility.UrlDecode(dt.NomorSK);
                    dt.JenisArsip = string.IsNullOrEmpty(dt.JenisArsip) ? dt.JenisArsip : HttpUtility.UrlDecode(dt.JenisArsip);
                    dt.JumlahBerkas = string.IsNullOrEmpty(dt.JumlahBerkas) ? dt.JumlahBerkas : HttpUtility.UrlDecode(dt.JumlahBerkas);
                    dt.Perkembangan = string.IsNullOrEmpty(dt.Perkembangan) ? dt.Perkembangan : HttpUtility.UrlDecode(dt.Perkembangan);
                    dt.Gedung = string.IsNullOrEmpty(dt.Gedung) ? dt.Gedung : HttpUtility.UrlDecode(dt.Gedung);
                    dt.Lantai = string.IsNullOrEmpty(dt.Lantai) ? dt.Lantai : HttpUtility.UrlDecode(dt.Lantai);
                    dt.Rak = string.IsNullOrEmpty(dt.Rak) ? dt.Rak : HttpUtility.UrlDecode(dt.Rak);
                    dt.NomorBoks = string.IsNullOrEmpty(dt.NomorBoks) ? dt.NomorBoks : HttpUtility.UrlDecode(dt.NomorBoks);
                    dt.Keterangan = string.IsNullOrEmpty(dt.Keterangan) ? dt.Keterangan : HttpUtility.UrlDecode(dt.Keterangan);
                    dt.GolonganArsip = string.IsNullOrEmpty(dt.GolonganArsip) ? dt.GolonganArsip : HttpUtility.UrlDecode(dt.GolonganArsip);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult TambahMasterArsip(List<MasterArsip> masterArsips)
        {
            Models.Entities.TransactionResult mr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            if (masterArsips == null)
            {
                masterArsips = new List<MasterArsip>();
            }
            foreach (MasterArsip masterArsip in masterArsips)
            {
                masterArsip.StatusHapus = "0";
                masterArsip.TanggalInput = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
                masterArsip.UserInput = functions.claimUser().NamaPegawai;
                masterArsip.UnitKerjaId = functions.claimUser().UnitKerjaId;
                masterArsip.NomorSK = Server.UrlEncode(masterArsip.NomorSK);
                masterArsip.Gedung = Server.UrlEncode(masterArsip.Gedung);
                masterArsip.Lantai = Server.UrlEncode(masterArsip.Lantai);
                masterArsip.Rak = Server.UrlEncode(masterArsip.Rak);
                masterArsip.NomorBoks = Server.UrlEncode(masterArsip.NomorBoks);
                masterArsip.GolonganArsip = Server.UrlEncode(masterArsip.GolonganArsip);
                mr = km.InsertMasterArsip(masterArsip);
            }
            return Json(mr, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HapusMasterArsip(Models.Entities.MasterArsip data, string id)
        {
            data.TanggalHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
            data.UserHapus = functions.claimUser().UserId;
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {

                    var usr = functions.claimUser();
                    result = km.DeleteMasterArsip(data, id);
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
        public JsonResult TambahMasterArsipDetail(List<MasterArsipDetail> arsipDetails, string id)
        {
            Models.Entities.TransactionResult mr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (arsipDetails == null)
            {
                arsipDetails = new List<MasterArsipDetail>();
            }

            foreach (MasterArsipDetail arsipDetail in arsipDetails)
            {
                arsipDetail.StatusHapus = "0";
                arsipDetail.TanggalInput = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
                arsipDetail.UserInput = functions.claimUser().NamaPegawai;
                arsipDetail.NomorSK = Server.UrlEncode(arsipDetail.NomorSK);
                arsipDetail.JenisArsip = Server.UrlEncode(arsipDetail.JenisArsip);
                arsipDetail.JumlahBerkas = Server.UrlEncode(arsipDetail.JumlahBerkas);
                arsipDetail.Perkembangan = Server.UrlEncode(arsipDetail.Perkembangan);
                arsipDetail.Keterangan = Server.UrlEncode(arsipDetail.Keterangan);
                arsipDetail.NomorUrut = km.GetFirstNum(arsipDetail.JenisArsip);
                mr = km.InsertMasterArsipDetail(arsipDetail, id);
            }


            return Json(mr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult HapusMasterArsipDetail(Models.Entities.MasterArsipDetail data, string id)
        {
            data.TanggalHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
            data.UserHapus = functions.claimUser().UserId;
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    var usr = functions.claimUser();
                    result = km.DeleteMasterArsipDetail(data, id);
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
        public ActionResult ListMasterArsipDetail(int? start, int? length, string nomorsk, Models.Entities.MasterArsipDetail f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            decimal? total = 0;

            nomorsk = Server.UrlEncode(nomorsk);
            var result = km.GetListMasterArsipDetail(nomorsk, from, to);


            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.JenisArsip = string.IsNullOrEmpty(dt.JenisArsip) ? dt.JenisArsip : HttpUtility.UrlDecode(dt.JenisArsip);
                    dt.Tahun = string.IsNullOrEmpty(dt.Tahun) ? dt.Tahun : HttpUtility.UrlDecode(dt.Tahun);
                    dt.JumlahBerkas = string.IsNullOrEmpty(dt.JumlahBerkas) ? dt.JumlahBerkas : HttpUtility.UrlDecode(dt.JumlahBerkas);
                    dt.Perkembangan = string.IsNullOrEmpty(dt.Perkembangan) ? dt.Perkembangan : HttpUtility.UrlDecode(dt.Perkembangan);
                    dt.Keterangan = string.IsNullOrEmpty(dt.Keterangan) ? dt.Keterangan : HttpUtility.UrlDecode(dt.Keterangan);
                    dt.NomorSK = string.IsNullOrEmpty(dt.NomorSK) ? dt.NomorSK : HttpUtility.UrlDecode(dt.NomorSK);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        /// MENU JENIS NASKAH DINAS /// - 12 April 2022 FIX
        public ActionResult ListGolonganArsip(int? start, int? length, Models.Entities.GolonganMasterArsip f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            decimal? total = 0;

            var result = km.GetListGolonganArsip(from, to);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.NamaGolongan = string.IsNullOrEmpty(dt.NamaGolongan) ? dt.NamaGolongan : HttpUtility.UrlDecode(dt.NamaGolongan);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult TambahGolonganMasterArsip(Models.Entities.GolonganMasterArsip data, string id)
        {
            Models.Entities.TransactionResult mr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            data.StatusHapus = "0";
            data.NamaGolongan = Server.UrlEncode(data.NamaGolongan);
            data.UserInput = functions.claimUser().NamaPegawai;
            mr = km.InsertGolonganMasterArsip(data, id);
            return Json(mr, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HapusGolonganMasterArsip(Models.Entities.GolonganMasterArsip data, string id)
        {
            data.TglHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
            data.UserHapus = functions.claimUser().UserId;
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {

                    var usr = functions.claimUser();
                    result = km.DeleteGolonganMasterArsip(data, id);
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
        public ActionResult GolonganMasterArsip()
        {
            if (!(OtorisasiUser.IsProfileAdminKearsipan() || OtorisasiUser.isTU()))
            {
                return RedirectToAction("Index", "Home");
            }
            //List<Models.Entities.JenisMasterArsip> data = km.GetJenisMasterArsip();
            Models.Entities.GolonganMasterArsip data = new Models.Entities.GolonganMasterArsip();
            data.NamaGolongan = Server.UrlDecode(data.NamaGolongan);
            data.ListGolonganMasterArsip = km.GetGolonganMasterArsip();
            return View(data);


        }

        /// MENU KLASIFIKASI ARSIP /// - 12 April 2022 FIX
        public ActionResult KlasifikasiMasterArsip()
        {
            if (!(OtorisasiUser.IsProfileAdminKearsipan() || OtorisasiUser.isTU()))
            {
                return RedirectToAction("Index", "Home");
            }

            Models.Entities.KlasifikasiMasterArsip data = new Models.Entities.KlasifikasiMasterArsip();
            data.KodeKlasifikasi = Server.UrlDecode(data.KodeKlasifikasi);
            data.JenisArsip = Server.UrlDecode(data.JenisArsip);
            data.Keterangan = Server.UrlDecode(data.Keterangan);
            data.ListKlasifikasiMasterArsip = km.GetKlasifikasiMasterArsip();
            return View(data);

        }
        public ActionResult ListKlasifikasiMasterArsip(int? start, int? length, Models.Entities.KlasifikasiMasterArsip f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            decimal? total = 0;

            string ListArsip = f.Kategori;
            string KeteranganLokasi = f.KeteranganLokasi;
            string MetaData = f.MetaData;
            Models.Entities.KlasifikasiMasterArsip data = new Models.Entities.KlasifikasiMasterArsip();

            var result = km.GetListKlasifikasiMasterArsip(ListArsip, KeteranganLokasi, MetaData, from, to);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.JenisArsip = string.IsNullOrEmpty(dt.JenisArsip) ? dt.JenisArsip : HttpUtility.UrlDecode(dt.JenisArsip);
                    dt.KodeKlasifikasi = string.IsNullOrEmpty(dt.KodeKlasifikasi) ? dt.KodeKlasifikasi : HttpUtility.UrlDecode(dt.KodeKlasifikasi);
                    dt.Keterangan = string.IsNullOrEmpty(dt.Keterangan) ? dt.Keterangan : HttpUtility.UrlDecode(dt.Keterangan);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult TambahKlasifikasiMasterArsip(Models.Entities.KlasifikasiMasterArsip data, string id)
        {
            Models.Entities.TransactionResult mr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            data.StatusHapus = "0";
            data.UserInput = functions.claimUser().NamaPegawai;
            data.KodeKlasifikasi = Server.UrlEncode(data.KodeKlasifikasi);
            data.JenisArsip = Server.UrlEncode(data.JenisArsip);
            data.Keterangan = Server.UrlEncode(data.Keterangan);
            mr = km.InsertKlasifikasiMasterArsip(data, id);
            return Json(mr, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HapusKlasifikasiMasterArsip(Models.Entities.KlasifikasiMasterArsip data, string id)
        {
            data.TanggalHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
            data.UserHapus = functions.claimUser().UserId;
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {

                    var usr = functions.claimUser();
                    result = km.DeleteKlasifikasiMasterArsip(data, id);
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
        public JsonResult TambahGolonganKlasifikasiMasterArsip(List<GolonganKlasifikasi> golonganKlasifikasis, string id, string kodeklasifikasi)
        {
            Models.Entities.TransactionResult mr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (golonganKlasifikasis == null)
            {
                golonganKlasifikasis = new List<GolonganKlasifikasi>();
            }

            foreach (GolonganKlasifikasi golonganKlasifikasi in golonganKlasifikasis)
            {
                golonganKlasifikasi.StatusHapus = "0";
                golonganKlasifikasi.UserInput = functions.claimUser().NamaPegawai;
                golonganKlasifikasi.NamaJenisArsip = Server.UrlEncode(golonganKlasifikasi.NamaJenisArsip);
                golonganKlasifikasi.Keterangan = Server.UrlEncode(golonganKlasifikasi.Keterangan);
                mr = km.InsertGolonganKlasifikasiMasterArsip(golonganKlasifikasi, id, kodeklasifikasi);
            }


            return Json(mr, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HapusGolonganKlasifikasiMasterArsip(Models.Entities.GolonganKlasifikasi data, string id)
        {
            data.TanggalHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
            data.UserHapus = functions.claimUser().NamaPegawai;
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {

                    var usr = functions.claimUser();
                    result = km.DeleteGolonganKlasifikasiMasterArsip(data, id);
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
        public ActionResult ListMasa(int? start, int? length, string kodeklasifikasi, Models.Entities.GolonganKlasifikasi f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            decimal? total = 0;


            var result = km.GetListMasa(kodeklasifikasi, from, to);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.KodeKlasifikasi = string.IsNullOrEmpty(dt.KodeKlasifikasi) ? dt.KodeKlasifikasi : HttpUtility.UrlDecode(dt.KodeKlasifikasi);
                    dt.NamaJenisArsip = string.IsNullOrEmpty(dt.NamaJenisArsip) ? dt.NamaJenisArsip : HttpUtility.UrlDecode(dt.NamaJenisArsip);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        /// UPLOAD FILE ARSIP
        [HttpPost]
        public JsonResult UploadLampiranArsip(Models.Entities.LampiranArsip data, int DetailID, List<HttpPostedFileBase> fileUploadStream)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "", ReturnValue2 = "" };
            var kontentm = new KontentModel();
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            string profileidtu = usr.ProfileIdTU;
            string userid = usr.UserId;
            string pegawaiid = usr.PegawaiId;
            string namapegawaipengirim = usr.NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            data.UserInput = userid;
            data.IdMasterArsipDetail = DetailID;
            data.KantorId = kantorid;
            data.UnitKerjaId = unitkerjaid;
            data.TanggalUpload = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");


            var dataSessionLampiran = new List<SessionLampiranArsip>();
            int urutLampiran = 1;
            foreach (HttpPostedFileBase file in fileUploadStream)
            {
                if (file != null)
                {
                    var datafile = new SessionLampiranArsip();
                    if (file.FileName.Length > 100)
                    {
                        tr.Pesan = "Nama File Maksimal 100 karakter";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    datafile.NamaFile = urutLampiran.ToString() + "|" + file.FileName;
                    MemoryStream ms1 = new MemoryStream();
                    file.InputStream.CopyTo(ms1);
                    datafile.ObjectFile = ms1.ToArray();
                    datafile.LampiranArsipId = mdl.GetUID();
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
                    tr.Pesan = "File Arsip wajib diupload";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }

            #region Simpan File Fisik
            foreach (var lampiranArsip in dataSessionLampiran)
            {
                if (lampiranArsip.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranArsip.LampiranArsipId;

                    Stream stream = new MemoryStream(lampiranArsip.ObjectFile);

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("BerkasKearsipan"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranArsip.NamaFile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            //tr = km.UploadLampiranArsip(data, kantorid, unitkerjaid, myProfileId, profileidtu, namapegawaipengirim, dataSessionLampiran);
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal Menyimpan Arsip, ada file arsip yang bermasalah\nHarap cek ulang file anda.";
                            tr.ReturnValue = reqresult.ReasonPhrase;
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            #endregion

            tr = km.UploadLampiranArsip(data, kantorid, unitkerjaid, myProfileId, profileidtu, namapegawaipengirim, dataSessionLampiran);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarLampiranArsip(int? IdMasterArsipDetail)
        {
            var usr = functions.claimUser();
            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
            var result = km.GetListLampiranArsip(IdMasterArsipDetail, satkerid);

            int custIndex = 1;
            Dictionary<int, LampiranArsip> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarLampiranArsip", dict);
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
        public ActionResult HapusLampiranArsip(int? IdMasterArsipDetail)
        {
            var usr = functions.claimUser();
            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);
            var result = km.GetListLampiranArsip(IdMasterArsipDetail, satkerid);

            int custIndex = 1;
            Dictionary<int, LampiranArsip> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("HapusLampiranArsip", dict);
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

        public async Task<ActionResult> GetFileArsip(string id, string kantorid)
        {
            Models.Entities.TransactionResult result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                if (string.IsNullOrEmpty(kantorid))
                {
                    kantorid = functions.claimUser().KantorId;
                }
                string tipe = "BerkasKearsipan";
                //string versi = km.CekVersi(id).ToString();
                string ext = ".pdf";

                if (kantorid.Length < 32)
                {
                    kantorid = dataMasterModel.GetKantorIdFromUnitKerjaId(kantorid);
                }

                var file = km.getFileLampiran(id);
                var filePath = file.Path.Split('|');
                string serviceurl = "ServiceEofficeUrl";
                string versi = "0";
                //serviceurl = "ServiceBaseUrl";
                if (filePath.Length == 2)
                {
                    tipe = filePath[0];
                    id = filePath[1];
                    file.PengenalFile = string.Concat(file.PengenalFile, ".pdf");
                    serviceurl = "ServiceEofficeUrl";
                    versi = "0";
                }

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(ext), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = file.PengenalFile;//String.Concat(tipe, ".pdf");

                            result.Status = true;
                            result.StreamResult = docfile;

                            return docfile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //result = new { Status = false, Message = ex.Message };
                    result.Pesan = ex.Message;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusFileLampiran(Models.Entities.LampiranArsip data)
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                var usr = functions.claimUser();
                data.UserHapus = usr.NamaPegawai;
                data.TanggalHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
                string id = Request.Form["id"].ToString();
                string namafile = Request.Form["namafile"].ToString();
                if (!String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(namafile))
                {
                    result = km.DeleteLampiranArsip(id, namafile, data);
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

        public async Task<MemoryStream> GetStreamFileArsip(string id, string kantorid)
        {
            MemoryStream msresult = new MemoryStream();
            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                if (string.IsNullOrEmpty(kantorid))
                {
                    kantorid = functions.claimUser().KantorId;
                }
                string tipe = "BerkasKearsipan";
                //string versi = km.CekVersi(id).ToString();
                string ext = ".pdf";

                if (kantorid.Length < 32)
                {
                    kantorid = dataMasterModel.GetKantorIdFromUnitKerjaId(kantorid);
                }

                var file = km.getFileLampiran(id);
                var filePath = file.Path.Split('|');
                string serviceurl = "ServiceEofficeUrl";
                string versi = "0";
                //serviceurl = "ServiceBaseUrl";
                if (filePath.Length == 2)
                {
                    tipe = filePath[0];
                    id = filePath[1];
                    file.PengenalFile = string.Concat(file.PengenalFile, ".pdf");
                    serviceurl = "ServiceEofficeUrl";
                    versi = "0";
                }

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(ext), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = file.PengenalFile;//String.Concat(tipe, ".pdf");
                            strm.CopyTo(msresult);
                            return msresult;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return msresult;
                }
            }

            return msresult;
        }


        public FileStreamResult GetWaterMarkPdf(string id, string kantorid)
        {



            // File Sumber
            //string originalFile = Server.MapPath("~/Contents/Test1.pdf");

            // AMBIL FILE DARI SERVER
            //id = "E04922E844D6BF88E0530B1D140A4F1E";
            //kantorid = "980FECFC746D8C80E0400B0A9214067D";
            MemoryStream filems = GetStreamFileArsip(id, kantorid).Result;

            //// File Tujuan
            //string destinationpath = Server.MapPath("~/Contents/TesthWaterMark.pdf");

            // Baca file dari lokasi file
            //PdfReader reader = new PdfReader(originalFile);

            //baca file dari memorystream
            filems.Position = 0;
            PdfReader reader = new PdfReader(filems);

            //menentukan ukuran dan gaya font
            Font font = new Font(FontFamily.TIMES_ROMAN, 13);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var pdfStamper = new PdfStamper(reader, memoryStream, '\0'))
                {
                    // Mendapatkan jumlah halaman dari Dokumen yang Ada
                    int pageCount = reader.NumberOfPages;

                    // Buat dua Layer Baru untuk Watermark
                    PdfLayer layer = new PdfLayer("WatermarkLayer", pdfStamper.Writer);
                    PdfLayer layer2 = new PdfLayer("WatermarkLayer2", pdfStamper.Writer);

                    // Ulangi setiap Halaman

                    string layerwarkmarktxt = "- ARSIP ASLI -"; // tentukan teks 
                    string Layer2warkmarktxt = "ARSIP KEMENTERIAN ATR/BPN";
                    for (int i = 1; i <= pageCount; i++)
                    {

                        // Mendapatkan Ukuran Halaman
                        Rectangle rect = reader.GetPageSize(i);

                        // Dapatkan objek ContentByte
                        PdfContentByte cb = pdfStamper.GetOverContent(i);


                        // Beri tahu cb bahwa perintah berikutnya harus "diikat" ke lapisan baru ini
                        // Mulai Lapisan

                        cb.BeginLayer(layer);


                        PdfGState gState = new PdfGState();

                        gState.FillOpacity = 0.1f; // tentukan tingkat opacity
                        cb.SetGState(gState);

                        // atur ukuran dan gaya font untuk teks tanda air lapisan
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10);


                        List<string> watermarkList = new List<string>();
                        float singleWaterMarkWidth = cb.GetEffectiveStringWidth(layerwarkmarktxt, false);

                        float fontHeight = 10;


                        //Mengerjakan Watermark untuk Satu Baris pada Halaman berdasarkan Lebar Halaman
                        float currentWaterMarkWidth = 0;
                        while (currentWaterMarkWidth + singleWaterMarkWidth < rect.Width)
                        {
                            watermarkList.Add(layerwarkmarktxt);
                            currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        }


                        //Isi Halaman dengan Garis Tanda Air
                        float currentYPos = rect.Height;

                        //cb.BeginText();
                        while (currentYPos > 0)
                        {
                            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), rect.Width / 2, currentYPos, 0, PdfWriter.RUN_DIRECTION_RTL, 1);

                            currentYPos -= fontHeight;
                        }


                        cb.EndLayer();

                        // End First Layer

                        //**********************//

                        // Start Layer 2

                        // Beri tahu cb bahwa perintah berikutnya harus "diikat" ke lapisan baru ini
                        cb.BeginLayer(layer2);
                        Font f = new Font(FontFamily.HELVETICA, 50);

                        gState = new PdfGState();
                        gState.FillOpacity = 0.1f;
                        cb.SetGState(gState);

                        cb.SetColorFill(BaseColor.RED);

                        //cb.BeginText();
                        ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", Layer2warkmarktxt), f), rect.Width / 2, rect.Height / 2, 45f, PdfWriter.RUN_DIRECTION_RTL, 1);

                        // Close the layer
                        cb.EndLayer();

                        // Akhir Layer 2
                    }




                }

                // Save file to destination location if required
                //if (System.IO.File.Exists(destinationpath))
                //{
                //    System.IO.File.Delete(destinationpath);
                //}
                //System.IO.File.WriteAllBytes(destinationpath, memoryStream.ToArray());
                var newms = new MemoryStream(memoryStream.ToArray());
                var docfile = new FileStreamResult(newms, "application/pdf");
                return docfile;

            }

            // send file to browse to open it from destination location.
            var none = new FileStreamResult(filems, "application/pdf");
            return none;
        }


    }
}