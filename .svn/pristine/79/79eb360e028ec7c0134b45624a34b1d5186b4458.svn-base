using QRCoder;
using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class MeetingController : Controller
    {
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.MeetingModel meetingmodel = new Models.MeetingModel();
        Functions functions = new Functions();


        #region Rapat Online

        public ActionResult RapatOnline()
        {
            var find = new FindRapatOnline();
            return View(find);
        }

        public ActionResult ListRapatOnline()
        {
            var find = new FindRapatOnline();
            return View(find);
        }

        public ActionResult RekapPresensi()
        {
            var data = new RekapPresensiRapat();
            data.list =  meetingmodel.GetRekapPresensiRapat("E45E3BD39A03ED7FE0530C1D140A2F27");
            return View(data);
        }

        public ActionResult DaftarRapatOnline(int? pageNum, FindRapatOnline f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string unitkerjaid = usr.UnitKerjaId;

            string metadata = f.Metadata;

            var result = meetingmodel.GetRapatOnline("", unitkerjaid, metadata, "", from, to);

            int custIndex = from;
            Dictionary<int, RapatOnline> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarRapatOnline", dict);
                }
                else
                {
                    return RedirectToAction("RapatOnline", "Meeting");
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

        public ActionResult DaftarListRapatOnline(int? pageNum, FindRapatOnline f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string unitkerjaid = usr.UnitKerjaId;
            string nip = usr.PegawaiId;

            string metadata = f.Metadata;

            var result = meetingmodel.GetRapatOnline("", unitkerjaid, metadata, nip, from, to);

            int custIndex = from;
            Dictionary<int, RapatOnline> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarListRapatOnline", dict);
                }
                else
                {
                    return RedirectToAction("ListRapatOnline", "Meeting");
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

        public ContentResult JumlahRapatOnlineSaya()
        {
            string result = "";
            var usr = functions.claimUser();
            string nip = usr.PegawaiId;
            string unitkerjaid = usr.UnitKerjaId;

            int jumlah = meetingmodel.JumlahRapatOnlineSaya(unitkerjaid, nip);

            result = string.Format("{0:#,##0}", jumlah);

            return Content(result);
        }

        public ActionResult EntriDataRapatOnline(string id)
        {
            var data = new RapatOnline();
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(id))
            {
                var list = meetingmodel.GetRapatOnline(id, "", "", "", 0, 1);
                data = list[0];
            }

            string unitkerjaid = usr.UnitKerjaId;

            ViewBag.UnitKerjaId = unitkerjaid;

            data.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);

            return View(data);
        }

        public ActionResult ViewDataRapatOnline(string id)
        {
            var data = new RapatOnline();

            if (!string.IsNullOrEmpty(id))
            {
                var list = meetingmodel.GetRapatOnline(id, "", "", "", 0, 1);
                data = list[0];
            }

            return View(data);
        }

        [HttpPost]
        public JsonResult SimpanRapatOnline(RapatOnline data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            data.UnitKerjaId = usr.UnitKerjaId;

            tr = meetingmodel.SimpanRapatOnline(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UbahKodeRapat(RapatOnline data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            
            tr = meetingmodel.UbahKodeRapat(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusRapatOnline()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            try
            {
                string id = Request.Form["id"].ToString();
                if (!string.IsNullOrEmpty(id))
                {
                    string userid = usr.UserId;
                    result = meetingmodel.HapusRapatOnline(id, userid);
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
        public JsonResult GetKodeRapat(string id) //Arya :: 2020-08-01
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = meetingmodel.GetKodeRapat(id);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

      
        #endregion


        #region Peserta Rapat

        public ActionResult ListPesertaRapat(int? draw, int? start, int? length)
        {
            List<PesertaRapatOnline> result = new List<PesertaRapatOnline>();
            decimal? total = 0;

            string rapatonlineid = Request.Form["rapatonlineid"].ToString();

            if (!String.IsNullOrEmpty(rapatonlineid))
            {
                result = meetingmodel.GetListPesertaRapat(rapatonlineid);
                if (result.Count > 0)
                {
                    total = result.Count;
                }
            }

            return Json(new { data = result, draw = draw, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanPesertaRapat(RapatOnline data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = meetingmodel.SimpanPesertaRapat(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HapusPesertaRapat()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    result = meetingmodel.HapusPesertaRapat(id);
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


        #endregion


        #region File Lampiran

        public ActionResult DaftarLampiranRapatOnline(string rapatonlineid)
        {
            decimal? total = 0;

            List<LampiranRapatOnline> result = meetingmodel.GetLampiranRapatOnlineForTable(rapatonlineid);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanLampiranRapatOnline(RapatOnline data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            string pegawaiid = usr.PegawaiId;
            data.NipPengupload = pegawaiid;

            string ekstensi = (!string.IsNullOrEmpty(data.Ekstensi)) ? data.Ekstensi.Replace(".", "") : "link";
            data.Ekstensi = ekstensi;

            var mfile = Request.Files["file"];
            if (mfile != null)
            {
                byte[] byteFile = null;
                using (var binaryReader = new BinaryReader(mfile.InputStream))
                {
                    byteFile = binaryReader.ReadBytes(mfile.ContentLength);
                }
                if (byteFile.Length > 0)
                {
                    data.ObjectFile = byteFile;
                }
            }

            tr = meetingmodel.SimpanLampiranRapatOnline(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusLampiranRapatOnlinen()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string lampiranrapatonlineid = Request.Form["lampiranrapatonlineid"].ToString();
                if (!string.IsNullOrEmpty(lampiranrapatonlineid))
                {
                    result = meetingmodel.HapusLampiranRapatOnline(lampiranrapatonlineid);
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

        public ActionResult GetFileLampiranById(string lampiranrapatonlineid)
        {
            string namafile = "FileLampiran";
            string ekstensi = "pdf";

            byte[] byteArray = meetingmodel.GetFileLampiranById(lampiranrapatonlineid, out namafile, out ekstensi);

            var mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, "application/pdf");
            if (ekstensi == "jpg")
            {
                docfile = new FileStreamResult(mss, "image/jpeg");
            }
            else if (ekstensi == "png")
            {
                docfile = new FileStreamResult(mss, "image/png");
            }
            else if (ekstensi == "xls")
            {
                docfile = new FileStreamResult(mss, "application/vnd.ms-excel");
            }
            else if (ekstensi == "xlsx")
            {
                docfile = new FileStreamResult(mss, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            else if (ekstensi == "doc")
            {
                docfile = new FileStreamResult(mss, "application/msword");
            }
            else if (ekstensi == "docx")
            {
                docfile = new FileStreamResult(mss, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
            else if (ekstensi == "txt")
            {
                docfile = new FileStreamResult(mss, "text/plain");
            }
            else if (ekstensi == "zip")
            {
                docfile = new FileStreamResult(mss, "application/x-zip-compressed");
            }
            else if (ekstensi == "wav")
            {
                docfile = new FileStreamResult(mss, "audio/wav");
            }
            else if (ekstensi == "mp3")
            {
                docfile = new FileStreamResult(mss, "audio/mpeg");
            }


            docfile.FileDownloadName = String.Concat(namafile, "." + ekstensi);

            return docfile;
        }

        #endregion

        #region Data Absensi

        public ActionResult DataAbsensi(string rapatid)
        {
            var data = new RapatOnline();

            if (!string.IsNullOrEmpty(rapatid))
            {
                var list = meetingmodel.GetRapatOnline(rapatid, "", "", "", 0, 1);
                data = list[0];
                data.listPresensi = meetingmodel.GetRekapPresensiRapatInduk(rapatid,2);
            }
            else
            {
                return RedirectToAction("RapatOnline", "Meeting");
            }
            return View(data);
        }

        public JsonResult GetLokasiKantor(string rapatid)
        {
            LokasiKantor data = new LokasiKantor();
            List<LokasiKantor> list = meetingmodel.GetLokasiKantorAbsen(rapatid);

            int jumlahKantah = 0;
            int jumlahKanwil = 0;
            int jumlahKantahHadir = 0;
            int jumlahKanwilHadir = 0;
            foreach (var kantor in list)
            {
                if (kantor.Nama.Contains("Kantor Wilayah"))
                {
                    jumlahKanwil += 1;
                    if (kantor.Ct > 0)
                    {
                        jumlahKanwilHadir += 1;
                    }
                } else if (kantor.Nama.Contains("Kantor Pertanahan"))
                {
                    jumlahKantah += 1;
                    if (kantor.Ct > 0)
                    {
                        jumlahKantahHadir += 1;
                    }
                }
            }

            return Json(new { data = list, count = list.Count, jmlKantah = jumlahKantah, jmlKanwil = jumlahKanwil, jmlKantahHadir = jumlahKantahHadir, jmlKanwilHadir = jumlahKanwilHadir}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAbsenData(string rapatid)
        {
            var record = meetingmodel.GetAbsenData(rapatid);

            return Json(record, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region "Absensi QR Reader"
        public ActionResult PresensiPeserta()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }
        public ActionResult PendaftaranPeserta()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }
        public ActionResult ValidasiPeserta()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        [HttpPost]
        public JsonResult OnlineAttendanceMeeting(string mCd, string pLong, string pLat)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            mCd = string.IsNullOrEmpty(mCd)?string.Empty:HttpUtility.UrlDecode(mCd).ToUpper();

            if (usr == null)
            {
                tr.Status = false;
                tr.Pesan = "Harap Login Terlebih Dahulu";
            }
            else if (string.IsNullOrEmpty(mCd) || mCd.Length != 6)
            {
                tr.Status = false;
                tr.Pesan = "Kode Rapat Daring Salah";
            }
            else if (string.IsNullOrEmpty(pLong) || string.IsNullOrEmpty(pLat))
            {
                tr.Status = false;
                tr.Pesan = "Lokasi Perangkat Tidak Ditemukan";
            }
            else
            {
                tr = meetingmodel.MeetingAttend(mCd, usr.UserId, pLong, pLat);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        private string createQR(string str, bool useLogo)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            byte[] byteGambar = null;
            if (useLogo)
            {
                Image logo = Image.FromFile(Server.MapPath("~/Reports/logoqr_small.png"));
                var destRect = new Rectangle(0, 0, 150, 150);
                var destImage = new Bitmap(150, 150);
                destImage.SetResolution(logo.HorizontalResolution, logo.VerticalResolution);
                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(logo, destRect, 0, 0, logo.Width, logo.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
                logo = destImage;

                int left = (qrCodeImage.Width / 2) - ((int)logo.Width / 2);
                int top = (qrCodeImage.Height / 2) - ((int)logo.Height / 2);
                Graphics g = Graphics.FromImage(qrCodeImage);
                g.DrawImage(logo, new Point(left, top));
            }
            try
            {
                using (var stream = new MemoryStream())
                {
                    qrCodeImage.Save(stream, ImageFormat.Bmp);
                    byteGambar = stream.ToArray();
                }
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            return "data:image/png;base64, " + Convert.ToBase64String(byteGambar);
        }

        [HttpPost]
        public ActionResult DetailRapat(string mCd, bool sQc = true)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var data = meetingmodel.GetRapatOnlineDetail("", mCd.ToUpper());
            data.NamaPeserta = string.Concat(usr.PegawaiId," - ",usr.NamaPegawai);
            if(sQc)
                data.QRCode = createQR(string.Concat(usr.PegawaiId,"|",data.RapatOnlineId), false);

            return new JsonResult()
            {
                Data = data,
                MaxJsonLength = Int32.MaxValue
            };
            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarPresensiRapat(int? draw, int? start, int? length, string rId)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<AbsensiRapatOnline>();
            decimal? total = 0;
            var usr = functions.claimUser();
            string userid = usr.UserId;

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = meetingmodel.GetListAbsensi(rId, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TambahPeserta(string mCd)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            if (usr == null)
            {
                tr.Status = false;
                tr.Pesan = "Harap Login Terlebih Dahulu";
            }
            else if (string.IsNullOrEmpty(mCd) || mCd.Length != 6)
            {
                tr.Status = false;
                tr.Pesan = "Kode Rapat Salah";
            }
            else
            {
                string myProfileId = functions.MyProfiles(usr.PegawaiId, usr.KantorId).Replace("'", "");
                string[] arrProfileId = myProfileId.Split(",".ToCharArray());
                if (arrProfileId.Length > 0)
                {
                    myProfileId = arrProfileId[0];
                }
                string namajabatan = dataMasterModel.GetProfileNameFromId(myProfileId);
                tr = meetingmodel.PendaftaranPeserta(mCd.ToUpper(), usr.PegawaiId, usr.NamaPegawai, myProfileId, namajabatan);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ValidasiPeserta(string pCd, string pLong, string pLat)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            if (usr == null)
            {
                tr.Status = false;
                tr.Pesan = "Harap Login Terlebih Dahulu";
            }
            else if (string.IsNullOrEmpty(pCd))
            {
                tr.Status = false;
                tr.Pesan = "Kode Peserta Salah";
            }
            else if (string.IsNullOrEmpty(pLong) || string.IsNullOrEmpty(pLat))
            {
                tr.Status = false;
                tr.Pesan = "Lokasi Perangkat Tidak Ditemukan";
            }
            else
            {
                var _cd = pCd.Split('|');
                if(_cd.Length < 2)
                {
                    tr.Status = false;
                    tr.Pesan = "Format Kode Peserta Salah";
                }
                else
                {
                    tr = meetingmodel.ValidasiPeserta(_cd[1], _cd[0], usr.UserId, pLong, pLat);
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetRekapPresensiRapat(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var data = meetingmodel.GetRekapPresensiRapat(id);
            return new JsonResult()
            {
                Data = data,
                MaxJsonLength = Int32.MaxValue
            };
        }
        #endregion
    }
}