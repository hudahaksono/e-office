﻿using System;
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
    public class TandaTanganElektronikController : Controller
    {
        DataMasterModel dataMasterModel = new DataMasterModel();
        TandaTanganElektronikModel mdl = new TandaTanganElektronikModel();
        SuratModel surat = new SuratModel();
        Functions functions = new Functions();
        private static string dev_nik = "0803202100007062";
        private static string dev_pass = "!Bsre1221*";

        #region Env

        public ActionResult PengajuanTTE()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public ActionResult PersetujuanTTE()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public ActionResult ProsesTTE()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var cari = new CariDokumenTTE();
            cari.status = false;
            return View(cari);
        }

        public ActionResult SudahTTE()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var cari = new CariDokumenTTE();
            cari.status = true;
            return View(cari);
        }

        public ActionResult BuatDokumen(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            DokumenTTE data = new DokumenTTE();
            var usr = functions.claimUser();
            ViewBag.listUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true, cek2: true, kid: usr.KantorId);
            ViewBag.selfUnitKerja = usr.UnitKerjaId;
            if (string.IsNullOrEmpty(id))
            {
                if (OtorisasiUser.PembuatDokumenElektronik())
                {
                    ViewBag.Judul = "Baru";
                    return View(data);
                }
                else
                {
                    return View("ProsesTTE");
                }
            }
            else
            {
                if (mdl.CekStatusDokumen(id) == "P"/* mdl.JumlahTTE(id) == 0*/)
                {
                    data = mdl.GetDokumenElektronik(id);
                    data.NomorSurat = Server.UrlDecode(data.NomorSurat);
                    data.Perihal = Server.UrlDecode(data.Perihal);
                    data.TTE = mdl.GetListUserTTE(id);
                    ViewBag.Judul = data.NomorSurat;
                    return View(data);
                }
                return View("PersetujuanTTE");
            }
        }

        public ActionResult BuatDokumenMulti(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            DokumenTTE data = new DokumenTTE();
            if (string.IsNullOrEmpty(id))
            {
                if (OtorisasiUser.PembuatDokumenElektronik())
                {
                    ViewBag.Judul = "Baru";
                    return View(data);
                }
                else
                {
                    return View("ProsesTTE");
                }
            }
            else
            {
                if (mdl.JumlahTTE(id) == 0)
                {
                    data = mdl.GetDokumenElektronik(id);
                    data.NomorSurat = Server.UrlDecode(data.NomorSurat);
                    data.Perihal = Server.UrlDecode(data.Perihal);
                    ViewBag.Judul = data.NomorSurat;
                    return View(data);
                }
                return View("PersetujuanTTE");
            }
        }

        #endregion

        public ActionResult DaftarDokumen(int? draw, int? start, int? length, CariDokumenTTE f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<DokumenTTE>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;
            string sort = Request.Form["orderby"];
            string dir = Request.Form["orderdir"];
            f.Tipe = Request.Form["tipe"].ToString();
            switch (sort)
            {
                case "1":
                    f.SortBy = "TDE.TANGGALDIBUAT " + dir;
                    break;
                case "2":
                    f.SortBy = "TDE.NOMORSURAT " + dir;
                    break;
                case "3":
                    f.SortBy = "TDE.TANGGALSURAT " + dir;
                    break;
                case "4":
                    if (f.Tipe == "sudah")
                    {
                        f.SortBy = "TTE.TANGGAL " + dir;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.KantorId = usr.KantorId;
                string profiletu = usr.ProfileIdTU;
                bool isTU = OtorisasiUser.isTU();
                f.MetaData = string.IsNullOrEmpty(f.MetaData) ? string.Empty : Server.UrlEncode(f.MetaData);
                result = mdl.GetListDokumenTTE(userid, profiletu, isTU, f, from, to);

                if (result.Count > 0)
                {
                    foreach (var r in result)
                    {
                        r.NomorSurat = Server.UrlDecode(r.NomorSurat);
                        r.Perihal = Server.UrlDecode(Server.UrlDecode(r.Perihal));
                        r.isTU = isTU;
                        if (OtorisasiUser.NamaSkema.Equals("surat"))
                        {
                            r.isTU = false;
                        }

                        var stringDate = r.TanggalSurat;
                        var dateTime = Convert.ToDateTime(stringDate);
                        var now = DateTime.Today;
                        if (dateTime < now)
                        {
                            r.isExpired = true;
                        }
                        else
                        {
                            r.isExpired = false;
                        }
                    }
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ContentResult JumlahPersetujuanPDF()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string userid = usr.UserId;
            string profiletu = usr.ProfileIdTU;
            bool isTU = OtorisasiUser.isTU();
            CariDokumenTTE f = new CariDokumenTTE();
            f.Tipe = "persetujuan";
            f.KantorId = usr.KantorId;
            f.status = false;

            string result = "";

            try
            {
                if (isTU)
                {
                    var persetujuanresult = mdl.GetListDokumenTTE(userid, profiletu, isTU, f, 0, 0);
                    var jumlah = persetujuanresult.Count();
                    result = String.Format("{0:#,##0}", jumlah);
                }
                else
                {
                    result = "--";
                }
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ActionResult DaftarSertipikat(int? draw, int? start, int? length, CariExpoSertipikat f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<ExpoSertipikat>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;
            string tipe = Request.Form["tipe"].ToString();

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 20;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.Metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : Server.UrlEncode(f.Metadata);
                result = mdl.GetListExpoSertipikat(userid, tipe, f, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ContentResult JumlahProsesDokumen()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string result = string.Empty;
            string userid = usr.UserId;

            try
            {
                int jumlah = mdl.JumlahProsesDokumen(userid);
                result = string.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }
            return Content(result);
        }


        [HttpPost]
        public JsonResult SimpanPengajuan(DokumenTTE data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string namapegawai = usr.NamaPegawai;
            string userid = usr.UserId;
            string pegawaiid = usr.PegawaiId;
            data.UserPembuat = usr.UserId;
            bool status = false;
            string pesan = "";
            string tid = "DokumenTTE";
            if (string.IsNullOrEmpty(data.NomorSurat))
            {
                return Json(new { Status = false, Pesan = "Nomor Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.UserPembuat))
            {
                return Json(new { Status = false, Pesan = "Kode Pembuat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.TanggalSurat))
            {
                return Json(new { Status = false, Pesan = "Tanggal Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }

            HttpPostedFileBase mfile = Request.Files[0];
            if (mfile == null || mfile.ContentType != "application/pdf")
            {
                //throw new Exception("File harus pdf");
                return Json(new { Status = false, Pesan = "File harus pdf" }, JsonRequestBehavior.AllowGet);
            }
            var strInput = mfile.InputStream;
            if (data.GenerateFooter)
            {
                int posTTE = 0;
                if (data.PosisiTTE.Equals("pertama"))
                {
                    posTTE = 1;
                }
                else if (data.PosisiTTE.Equals("terakhir"))
                {
                    posTTE = 0;
                }
                else if (!string.IsNullOrEmpty(data.PosisiTTE))
                {
                    posTTE = Convert.ToInt32(data.PosisiTTE);
                }
                var InputStr = new PDFBuilder().Build(
                   streamPdf: strInput,
                   template: PDFBuilder.Template.FOOTER,
                   pageTte: posTTE);
                if (InputStr.Output == null)
                {
                    return Json(new { Status = false, Pesan = InputStr.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
                strInput = InputStr.Output;
            }
            if (!string.IsNullOrEmpty(data.listTTE))
            {
                data.Status = OtorisasiUser.isTU() ? "A" : "P";
                string[] str = data.listTTE.Split('|');
                data.TTE = new List<UserTTE>();
                UserTTE usertte = new UserTTE();
                int urut = 1;
                if (!string.IsNullOrEmpty(data.Pass))
                {
                    usertte.PenandatanganId = usr.UserId;
                    usertte.Tipe = "0";
                    usertte.EMeterai = "0";
                    usertte.Urut = urut;
                    bool doParaf = true;
                    foreach (var s in str)
                    {
                        string[] tte = s.Split(',');
                        if (tte[0].ToString() == usr.PegawaiId) { doParaf = false; }
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
                        if (tte.Count() == 2 || tte.Count() == 3)
                        {
                            usertte = new UserTTE();
                            usertte.PenandatanganId = mdl.getUserId(tte[0].ToString());
                            usertte.Tipe = tte[1].ToString();
                            if (usertte.Tipe.Equals("1") && tte.Count().Equals(3))
                            {
                                usertte.EMeterai = tte[2].ToString();
                            }
                            else
                            {
                                usertte.EMeterai = "0";
                            }
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
                    return Json(new { Status = false, Pesan = "List Penandatangan harus diisi" }, JsonRequestBehavior.AllowGet);
                }
                if (!ttd)
                {
                    return Json(new { Status = false, Pesan = "Penandatangan harus dipilih" }, JsonRequestBehavior.AllowGet);
                }
            }
            else data.Status = "P";

            try
            {
                data.DokumenElektronikId = mdl.NewGuID();

                if (string.IsNullOrEmpty(data.DokumenElektronikId))
                {
                    return Json(new { Status = false, Pesan = "Id Pengajuan Tidak Ditemukan" }, JsonRequestBehavior.AllowGet);
                }

                int versi = 0;

                data.NomorSurat = Server.UrlEncode(data.NomorSurat);
                data.Perihal = Server.UrlEncode(data.Perihal);
                mfile = Request.Files[0];

                tr = mdl.SimpanPengajuan(data, namapegawai, kantorid, tid);
                if (!tr.Status)
                {
                    return Json(new { Status = tr.Status, Pesan = tr.Pesan }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(data.Pass))
                {
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "DokumenTTE";
                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(data.DokumenElektronikId), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(strInput), "file", mfile.FileName);
                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(mdl.GetServerDate())].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            status = true;
                            pesan = "Pembuatan Dokumen Berhasil";
                        }
                        else
                        {
                            mdl.HapusDokumen(data.DokumenElektronikId, userid, namapegawai, string.Concat("Gagal Upload ", reqresult.ReasonPhrase.ToString()));
                            status = false;
                            throw new Exception(reqresult.ReasonPhrase);
                        }
                    }
                }
                else
                {
                    byte[] byt = new byte[strInput.Length];
                    strInput.Read(byt, 0, byt.Length);
                    //byte[] byt = new byte[mfile.ContentLength];
                    //mfile.InputStream.Read(byt, 0, byt.Length);

                    string dokid = data.DokumenElektronikId;
                    PenandatanganInfo info = mdl.getPenandatanganInfo(pegawaiid);
                    if (info == null)
                    {
                        tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    string nik = info.nik;
                    string passphrase = data.Pass;
                    //nik = "5204056811830106";//"30122019"; // Dev
                    //passphrase = "281183@_dian";//"#1234qwer*"; // Dev
                    string ttdid = info.ttdid;

                    try
                    {
                        if (string.IsNullOrEmpty(dokid))
                        {
                            tr.Pesan = "Dokumen tidak ditemukan";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        if (String.IsNullOrEmpty(namapegawai))
                        {
                            tr.Pesan = "Penandatangan tidak ditemukan";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        if (String.IsNullOrEmpty(passphrase))
                        {
                            tr.Pesan = "Harap masukkan Passphrase";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            passphrase = Server.UrlEncode(passphrase);
                        }

                        string tampilan = mdl.getTipeTTE(dokid, userid);
                        tr = ProcessSignDocument(usr, new MemoryStream(byt), nik, passphrase, ttdid, kantorid, dokid, versi, tampilan).Result;
                        status = tr.Status;
                        pesan = tr.Pesan;
                        if (!status)
                        {
                            mdl.HapusDokumen(data.DokumenElektronikId, userid, namapegawai, string.Concat("Gagal Signing ", pesan));
                        }
                    }
                    catch (Exception ex)
                    {
                        pesan = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                pesan = ex.Message;
            }

            return Json(new { Status = status, Pesan = pesan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PengajuanTTEDraft(DokumenTTE data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string pNama = usr.NamaPegawai;
            data.UserPembuat = string.IsNullOrEmpty(data.UserPembuat) ? usr.UserId : data.UserPembuat;
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            bool status = false;
            string pesan = "";
            string tid = "DokumenTTE";
            if (string.IsNullOrEmpty(data.NomorSurat))
            {
                return Json(new { Status = false, Pesan = "Nomor Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.UserPembuat))
            {
                return Json(new { Status = false, Pesan = "Kode Pembuat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.TanggalSurat))
            {
                return Json(new { Status = false, Pesan = "Tanggal Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }

            if (OtorisasiUser.isTU())
            {
                data.Status = "A";
                string[] str = data.listTTE.Split('|');
                data.TTE = new List<UserTTE>();
                UserTTE usertte = new UserTTE();
                int urut = 1;
                if (!string.IsNullOrEmpty(data.Pass))
                {
                    usertte.PenandatanganId = usr.UserId;
                    usertte.Tipe = "0";
                    usertte.EMeterai = "0";
                    usertte.Urut = urut;
                    bool doParaf = true;
                    foreach (var s in str)
                    {
                        string[] tte = s.Split(',');
                        if (tte[0].ToString() == usr.PegawaiId) { doParaf = false; }
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
                        if (tte.Count() == 2)
                        {
                            usertte = new UserTTE();
                            usertte.PenandatanganId = mdl.getUserId(tte[0].ToString());
                            usertte.Tipe = tte[1].ToString();
                            if (usertte.Tipe.Equals("1") && tte.Count().Equals(3))
                            {
                                usertte.EMeterai = tte[2].ToString();
                            }
                            else
                            {
                                usertte.EMeterai = "0";
                            }
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
                    return Json(new { Status = false, Pesan = "List Penandatangan harus diisi" }, JsonRequestBehavior.AllowGet);
                }
                if (!ttd)
                {
                    return Json(new { Status = false, Pesan = "Penandatangan harus dipilih" }, JsonRequestBehavior.AllowGet);
                }
            }
            else data.Status = "P";

            try
            {
                string draftcode = data.IsiSurat;
                data.NamaFile = $"Naskah_Dinas_[{draftcode}].pdf";
                data.Ekstensi = ".pdf";


                if (string.IsNullOrEmpty(data.DokumenElektronikId))
                {
                    return Json(new { Status = false, Pesan = "Id Pengajuan Tidak Ditemukan" }, JsonRequestBehavior.AllowGet);
                }


                data.NomorSurat = Server.UrlEncode(data.NomorSurat);
                data.Perihal = Server.UrlEncode(data.Perihal);


                tr = mdl.SimpanPengajuanDraft(data, pNama, kantorid, tid, draftcode);
                if (!tr.Status)
                {
                    return Json(new { Status = tr.Status, Pesan = tr.Pesan }, JsonRequestBehavior.AllowGet);
                }
                var simpan = new NaskahDinasModel().SetDraftStatus(draftcode, "F");
                status = true;
                pesan = "Pembuatan Dokumen Berhasil";
                return Json(new { Status = status, Pesan = pesan }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                pesan = ex.Message;
            }

            return Json(new { Status = status, Pesan = pesan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusDokumen(string id, string alasan)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    var usr = functions.claimUser();
                    result = mdl.HapusDokumen(id, usr.UserId, usr.NamaPegawai, alasan);
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

        private string DigitalSignatureUrl()
        {
            return ConfigurationManager.AppSettings["DigitalSignatureUrl"].ToString();
        }

        private string UrlDokumenTTE()
        {
            return ConfigurationManager.AppSettings["UrlDokumenTTE"].ToString();
        }

        public ActionResult ProsesPenandatangananBanyak(List<string> ids, string pps)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            string pegawai = usr.NamaPegawai;
            string nip = usr.PegawaiId;
            string userid = usr.UserId;
            PenandatanganInfo info = mdl.getPenandatanganInfo(nip);
            if (info == null)
            {
                tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            string nik = info.nik;
            string passphrase = pps;
            //nik = "5204056811830106";//"30122019"; // Dev
            //passphrase = "281183@_dian";//"#1234qwer*"; // Dev
            string ttdid = info.ttdid;
            string kantorid = usr.KantorId;
            bool adagagal = false;
            if (String.IsNullOrEmpty(pegawai))
            {
                tr.Pesan = "Penandatangan tidak ditemukan";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            if (String.IsNullOrEmpty(passphrase))
            {
                tr.Pesan = "Harap masukkan Passphrase";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            else
            {
                passphrase = Server.UrlEncode(passphrase);
            }
            foreach (var dokid in ids)
            {
                try
                {
                    if (string.IsNullOrEmpty(dokid))
                    {
                        tr.Pesan = "Dokumen tidak ditemukan";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }

                    tr = CreatePdf(usr, dokid, kantorid, nik, passphrase, ttdid, userid).Result;
                }
                catch (Exception ex)
                {
                    adagagal = true;
                }
            }
            if (tr.Status)
            {
                tr.Pesan = "Proses TTE Berhasil";
                if (adagagal)
                {
                    tr.Pesan += " Sebagian";
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProsesPenandatanganan(string id, string pps)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            string dokid = id;
            string pegawai = usr.NamaPegawai;
            string nip = usr.PegawaiId;
            string userid = usr.UserId;
            PenandatanganInfo info = mdl.getPenandatanganInfo(nip);
            if (info == null)
            {
                tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            string nik = info.nik;
            string passphrase = pps;
            if (System.Web.Mvc.OtorisasiUser.isTrainBSSN)
            {
                nik = dev_nik;
                passphrase = dev_pass;
            }
            //nik = "5204056811830106";//"30122019"; // Dev
            //passphrase = "281183@_dian";//"#1234qwer*"; // Dev
            string ttdid = info.ttdid;
            string kantorid = usr.KantorId;
            try
            {
                if (string.IsNullOrEmpty(dokid))
                {
                    tr.Pesan = "Dokumen tidak ditemukan";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(pegawai))
                {
                    tr.Pesan = "Penandatangan tidak ditemukan";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(passphrase))
                {
                    tr.Pesan = "Harap masukkan Passphrase";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    passphrase = Server.UrlEncode(passphrase);
                }

                tr = CreatePdf(usr, dokid, kantorid, nik, passphrase, ttdid, userid).Result;
            }
            catch (Exception ex)
            {
                tr.Pesan = ex.Message;
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public async Task<TransactionResult> CreatePdf(userIdentity usr, string dokid, string kantorid, string NIK, string PassPhrase, string TTDId, string userid, bool generateFooter = false)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string tampilan = mdl.getTipeTTE(dokid, userid);
                MemoryStream memoryStream = new MemoryStream();
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string tipe = "DokumenTTE";
                int cekversi = mdl.CekVersi(dokid);
                DateTime tglSunting = mdl.getTglSunting(dokid);
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(dokid), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(cekversi.ToString()), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            byte[] b = new byte[strm.Length];
                            strm.Read(b, 0, (int)strm.Length);
                            memoryStream = new MemoryStream(b);
                        }
                        else
                        {
                            result.Pesan = "Dokumen tidak ditemukan";
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Pesan = ex.Message;
                }

                result = await ProcessSignDocument(usr, new MemoryStream(memoryStream.ToArray()), NIK, PassPhrase, TTDId, kantorid, dokid, cekversi, tampilan, generateFooter);
            }
            catch (Exception ex)
            {
                result.Pesan = String.Concat("Gagal Menyiapkan Dokumen \n", ex.Message);
            }
            return result;
        }

        private async Task<TransactionResult> ProcessSignDocument(userIdentity usr, MemoryStream memoryStream, string NIK, string PassPhrase, string TTDId, string kantoridTTD, string DokId, int versi, string tampilan, bool generateFooter = false)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            string kantorid = usr.KantorId;
            string userid = usr.UserId;
            string nama = usr.NamaPegawai;
            string halaman = mdl.getPosisiTTE(DokId);
            if (OtorisasiUser.isTrainBSSN)
            {
                NIK = dev_nik;
                PassPhrase = dev_pass;
            }

            if (generateFooter)
            {
                int posTTE = 0;
                if (halaman.Equals("pertama"))
                {
                    posTTE = 1;
                }
                else if (halaman.Equals("terakhir"))
                {
                    posTTE = 0;
                }
                else if (!string.IsNullOrEmpty(halaman))
                {
                    posTTE = Convert.ToInt32(halaman);
                }
                var InputStr = new PDFBuilder().Build(
                   streamPdf: memoryStream,
                   template: PDFBuilder.Template.FOOTER,
                   pageTte: posTTE);
                if (InputStr.Output != null)
                {
                    var _output = new byte[InputStr.Output.Length];
                    InputStr.Output.Read(_output, 0, (int)InputStr.Output.Length);
                    memoryStream = new MemoryStream(_output);
                }
                else
                {
                    result.Status = false;
                    result.Pesan = "Gagal membuat catatan kaki, Silahkan coba kembali";
                    return result;
                }
            }

            string pHal = Regex.IsMatch(halaman, @"\d") ? "page" : "halaman";
            var kvp = new[] {
                    new KeyValuePair<string, string>("nik",NIK),
                    new KeyValuePair<string, string>("passphrase", functions.bsreEncode(PassPhrase)),
                    new KeyValuePair<string, string>("tampilan", tampilan), // visible // invisible
                    new KeyValuePair<string, string>("reason", tampilan.Equals("visible")?"Tanda Tangan Surat":"Paraf Pemeriksaan"),
                    new KeyValuePair<string, string>(pHal, halaman),
                    new KeyValuePair<string, string>("image", "true"),
                    new KeyValuePair<string, string>("xAxis", "67"),
                    new KeyValuePair<string, string>("yAxis", "66"),
                    new KeyValuePair<string, string>("width", "136"), // x
                    new KeyValuePair<string, string>("height", "13") // y
            };

            var address = string.Format(DigitalSignatureUrl() + "api/sign/pdf?{0}", string.Join("&", kvp.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))));
            var addressdownload = DigitalSignatureUrl() + "api/sign/download/";
            string logid = mdl.NewGuID();
            try
            {
                var docfileTTD = new MemoryStream();
                if (tampilan == "visible")
                {
                    result = mdl.GetKodeFile(DokId);
                    if (result.Status)
                    {
                        string kode = result.Pesan;
                        string urlkey = string.Concat(UrlDokumenTTE(), "?q=", kode);
                        if (!string.IsNullOrEmpty(kode))
                        {
                            QRCodeGenerator qrGenerator = new QRCodeGenerator();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(urlkey, QRCodeGenerator.ECCLevel.Q);
                            QRCode qrCode = new QRCode(qrCodeData);
                            System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20);
                            System.Drawing.Image logo = System.Drawing.Image.FromFile(Server.MapPath("~/Reports/logoqr_small.png"));
                            int left = (qrCodeImage.Width / 2) - ((int)logo.Width / 2) - 20;
                            int top = (qrCodeImage.Height / 2) - ((int)logo.Height / 2) - 20;
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(qrCodeImage);
                            g.DrawImage(logo, new System.Drawing.Point(left, top));

                            MemoryStream ms = null;
                            try
                            {
                                using (var stream = new MemoryStream())
                                {
                                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                                    docfileTTD = new MemoryStream(stream.ToArray());
                                }
                            }
                            catch (ArgumentNullException ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }

                string userName = ConfigurationManager.AppSettings["AuthUserName"].ToString();
                string userPassword = ConfigurationManager.AppSettings["AuthPassword"].ToString();

                string tipepengirim = "NIK";
                string idpengirim = NIK;
                string aplikasi = "Eoffice";
                string tipedokumen = "DokumenTTE";
                string servis = "SIGN DOKUMEN";
                result = mdl.InsertLog(logid, tipepengirim, idpengirim, aplikasi, tipedokumen, "", servis, address);
                if (!result.Status) { return result; }

                var request = WebRequest.Create(address);

                string auth = Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));
                request.Headers.Add("Authorization", "Basic " + auth);
                request.Method = "POST";
                var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                boundary = "--" + boundary;

                using (var requestStream = request.GetRequestStream())
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", "file.pdf", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", MediaTypeNames.Application.Pdf, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    memoryStream.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "imageTTD", "ttd.png", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", "image/png", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    docfileTTD.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                HttpStatusCode wRespStatusCode = response.StatusCode;
                WebHeaderCollection responseHeader = request.GetResponse().Headers;
                var iddokumen = responseHeader["id_dokumen"];
                string hasil = responseHeader.ToString();
                string status = wRespStatusCode == HttpStatusCode.OK ? "T" : "F";

                result = mdl.UpdateLog(logid, hasil, status);
                if (!result.Status) { return result; }
              
                using (var stream = new MemoryStream())
                {
                    responseStream.CopyTo(stream);
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "DokumenTTE";
                    var namafile = mdl.getFileName(DokId);
                    content.Add(new StringContent(kantoridTTD), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(DokId), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent((versi + 1).ToString()), "versionNumber");
                    content.Add(new StreamContent(new MemoryStream(stream.ToArray())), "file", namafile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(mdl.GetServerDate())].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        result.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                        result.Pesan = "Storage Server - " + reqresult.ReasonPhrase;
                    }

                    if (result.Status)
                    {
                        result = mdl.ProsesTandaTangan(DokId, userid, nama);
                    }
                }
            }
            catch (WebException wex)
            {
                using (var stream = wex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(responseFromServer))
                    {
                        result.Status = false;
                        StatusErrorResponse responseResult = JsonConvert.DeserializeObject<StatusErrorResponse>(responseFromServer);
                        result.Pesan = responseResult.message + " " + responseResult.error.Replace("!!! 2031", "!");
                    }
                    else
                    {
                        result.Status = false;
                        result.Pesan = "Tidak dapat menentukan status validasi user, Server BSSN tidak mengirimkan pesan apapun";
                    }
                }
            }
            catch (Exception ex)
            {
                result = mdl.UpdateLog(logid, ex.Message, "F");
                result.Pesan = ex.Message;
                result.Status = false;
            }

            return result;
        }

        public async Task<ActionResult> getDokumen(string id)
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
                string kantorid = mdl.getKantorid(id);
                string tipe = "DokumenTTE";
                string versi = mdl.CekVersi(id).ToString();
                DateTime tglSunting = mdl.getTglSunting(id);
                string filename = mdl.getFileName(id);
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = string.IsNullOrEmpty(filename) ? string.Concat(tipe, ".pdf") : filename;

                            result.Status = true;
                            result.StreamResult = docfile;

                            return docfile;
                        }
                        else
                        {
                            result.Pesan = "Dokumen tidak ditemukan";
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

        public class tte
        {
            public string tanggal { get; set; }
            public string nama { get; set; }
            public string info { get; set; }
        }
        public class transStream
        {
            public bool Status { get; set; }
            public string Pesan { get; set; }
            public bool TTE { get; set; }
            public List<tte> listTTE { get; set; }
        }

        public ActionResult cekDokumenElektronik(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new transStream();
            result.listTTE = new List<tte>();
            var usr = functions.claimUser();

            var reqmessage = new HttpRequestMessage();
            var content = new MultipartFormDataContent();
            string kantorid = mdl.getKantorid(id);
            string tipe = "DokumenTTE";
            DateTime tglSunting = mdl.getTglSunting(id);
            string versi = mdl.CekVersi(id).ToString();
            string filename = mdl.getFileName(id);
            content.Add(new StringContent(kantorid), "kantorId");
            content.Add(new StringContent(tipe), "tipeDokumen");
            content.Add(new StringContent(id), "dokumenId");
            content.Add(new StringContent(".pdf"), "fileExtension");
            content.Add(new StringContent(versi.ToString()), "versionNumber");

            reqmessage.Method = HttpMethod.Post;
            reqmessage.Content = content;
            reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));
            try
            {
                using (var client = new HttpClient())
                {
                    var reqresult = client.SendAsync(reqmessage).Result;
                    if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                    {
                        var strm = reqresult.Content.ReadAsStreamAsync().Result;
                        var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                        result.Status = true;
                        PdfReader pdfreader = new PdfReader(new ReaderProperties().SetPartialRead(true), strm);
                        AcroFields fields = pdfreader.AcroFields;
                        AcroFields.Item item = fields.GetFieldItem("Signature1");
                        List<string> names = fields.GetSignatureNames();
                        if (names.Count == 0)
                        {
                            //result.Status = false;
                            result.Pesan = "File ini belum memiliki Stempel Digital";
                        }
                        else
                        {
                            result.TTE = true;
                            foreach (string name in names)
                            {
                                PdfPKCS7 pk = fields.VerifySignature(name);
                                var cert = pk.Certificates;

                                if (!pk.Verify())
                                {
                                    //result.Status = false;
                                    result.Pesan = "Stempel Digital tidak valid";
                                    break;
                                }
                                if (cert == null)
                                {
                                    //result.Status = false;
                                    result.Pesan = "Sertifikasi Stempel Digital tidak temukan";
                                    break;
                                }
                                var _nama = pk.SigningCertificate.SubjectDN.GetValueList()[2];
                                var _tanggal = pk.SignDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                                var _info = pk.Reason;
                                result.listTTE.Add(new tte() { tanggal = _tanggal, info = _info, nama = _nama.ToString() });
                            }
                        }
                        pdfreader.Close();
                        //result.Pesan = "Dokumen Digital Ditemukan";
                    }
                    else
                    {
                        result.Status = false;
                        result.Pesan = "Dokumen Digital Tidak Ditemukan";
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                result.Status = false;
                result.Pesan = "Dokumen Digital Tidak Ditemukan";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> cekDokumen(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "0", ReturnValue2 = "terakhir" };
            var usr = functions.claimUser();

            Stream strm = null;
            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = mdl.getKantorid(id);
                string tipe = "DokumenTTE";
                DateTime tglSunting = mdl.getTglSunting(id);
                string versi = mdl.CekVersi(id).ToString();
                string filename = mdl.getFileName(id);

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            int pageNumber = 0;
                            using (StreamReader sr = new StreamReader(strm))
                            {
                                pageNumber = new Regex(@"/Type\s*/Page[^s]").Matches(sr.ReadToEnd()).Count;
                            }
                            result.Status = true;
                            //var _result = validasiKonten(strm);
                            //result.Pesan = _result.Pesan;
                            result.ReturnValue2 = mdl.getPosisiTTE(id);
                            result.ReturnValue = pageNumber.ToString();
                        }
                        else
                        {
                            result.Pesan = "Dokumen tidak ditemukan";
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

        public ActionResult GetDetailDokumen(int? draw, int? start, int? length)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<SejarahDokumenTTE>();
            decimal? total = 0;
            string dokid = Request.Form["dokid"].ToString();

            if (!string.IsNullOrEmpty(dokid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = mdl.GetDetailDokumen(dokid, from, to, true);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ContentResult NamaPenandatangan(string kode)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            string dokid = mdl.getDokid(kode);
            string result = mdl.getNamaPenandatangan(dokid);

            return Content(result);
        }

        public async Task<ActionResult> getTemplate(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = "980FECFC746D8C80E0400B0A9214067D";
                string tipe = "TemplateSurat";
                string versi = "0";
                DateTime tglSunting = mdl.getTglSunting(id);
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".docx"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                            return docfile;
                        }
                        else
                        {
                            result.Pesan = "Dokumen tidak ditemukan";
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

        public async Task<ActionResult> addFooter(string id, string pg)
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
                string kantorid = usr.KantorId;
                string tipe = "DokumenTTE";
                DateTime tglSunting = mdl.getTglSunting(id);
                string versi = mdl.CekVersi(id).ToString();
                string filename = mdl.getFileName(id);
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            byte[] b = new byte[strm.Length];
                            strm.Read(b, 0, (int)strm.Length);
                            MemoryStream memoryStream = new MemoryStream(b);

                            //pdfBuilder.Build(
                            //    );

                            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4, 1, 1, 1, 1);
                            iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, memoryStream);
                            doc.Open();
                            try
                            {
                                Paragraph paragraph = new Paragraph("Getting Started ITextSharp.");

                                string imageURL = Server.MapPath("~/Reports/logoqr_small.png");
                                //System.Drawing.Image logo = System.Drawing.Image.FromFile(Server.MapPath("~/Reports/logoqr_small.png"));

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);

                                //Resize image depend upon your need

                                jpg.ScaleToFit(140f, 120f);

                                //Give space before image

                                jpg.SpacingBefore = 10f;

                                //Give some space after the image

                                jpg.SpacingAfter = 1f;

                                jpg.Alignment = Element.ALIGN_LEFT;

                                doc.Add(paragraph);

                                //doc.Add(jpg);

                                doc.Close();

                                //memoryStream.Flush(); //Always catches me out
                                //memoryStream.Position = 0; //Not sure if this is required

                                return File(memoryStream, "application/pdf", "DownloadName.pdf");

                                var docfile = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                                return docfile;

                            }
                            catch (Exception ex)
                            {
                                string msg = ex.Message;
                            }

                            //string code = "testing";
                            //iTextSharp.text.pdf.BarcodeQRCode qrcode = new iTextSharp.text.pdf.BarcodeQRCode(code, 1, 1, null);
                            //Image img = qrcode.GetImage();
                            //return docfile;
                        }
                        else
                        {
                            result.Pesan = "Dokumen tidak ditemukan";
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

        [HttpPost]
        public JsonResult SetujuiPengajuan(DokumenTTE data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string pegawaiid = usr.PegawaiId;
            string namapegawai = usr.NamaPegawai;
            string userid = usr.UserId;
            data.UserPembuat = usr.UserId;
            bool status = false;
            string pesan = "";
            if (string.IsNullOrEmpty(data.NomorSurat))
            {
                return Json(new { Status = false, Pesan = "Nomor Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.UserPembuat))
            {
                return Json(new { Status = false, Pesan = "Kode Pembuat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.TanggalSurat))
            {
                return Json(new { Status = false, Pesan = "Tanggal Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (DateTime.ParseExact(data.TanggalSurat, "dd/MM/yyyy", CultureInfo.InvariantCulture) < DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture))
            {
                return Json(new { Status = false, Pesan = "Tanggal Surat Sudah Terlewat, Gunakan Tanggal Yang Sesuai" }, JsonRequestBehavior.AllowGet);
            }
            string[] str = data.listTTE.Split('|');
            data.TTE = new List<UserTTE>();
            UserTTE usertte = new UserTTE();
            int urut = 1;
            if (!string.IsNullOrEmpty(data.Pass))
            {
                usertte.PenandatanganId = userid;
                usertte.Tipe = "0";
                usertte.EMeterai = "0";
                usertte.Urut = urut;
                bool doParaf = true;
                foreach (var s in str)
                {
                    string[] tte = s.Split(',');
                    if (tte[0].ToString() == pegawaiid) { doParaf = false; }
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
                    if (tte.Count() == 2)
                    {
                        usertte = new UserTTE();
                        usertte.PenandatanganId = mdl.getUserId(tte[0].ToString());
                        usertte.Tipe = tte[1].ToString();
                        if (usertte.Tipe.Equals("1") && tte.Count().Equals(3))
                        {
                            usertte.EMeterai = tte[2].ToString();
                        }
                        else
                        {
                            usertte.EMeterai = "0";
                        }
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
                return Json(new { Status = false, Pesan = "List Penandatangan harus diisi" }, JsonRequestBehavior.AllowGet);
            }
            if (!ttd)
            {
                return Json(new { Status = false, Pesan = "Penandatangan harus dipilih" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (string.IsNullOrEmpty(data.DokumenElektronikId))
                {
                    return Json(new { Status = false, Pesan = "Id Pengajuan Tidak Ditemukan" }, JsonRequestBehavior.AllowGet);
                }
                data.NomorSurat = Server.UrlEncode(data.NomorSurat);
                data.Perihal = Server.UrlEncode(data.Perihal);

                string dokid = data.DokumenElektronikId;
                tr = mdl.SetujuPengajuan(data, userid, kantorid);
                status = tr.Status;
                pesan = tr.Pesan;
                if (!tr.Status)
                {
                    return Json(new { Status = tr.Status, Pesan = tr.Pesan }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (!string.IsNullOrEmpty(data.Pass))
                    {
                        PenandatanganInfo info = mdl.getPenandatanganInfo(pegawaiid);
                        if (info == null)
                        {
                            tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        string nik = info.nik;
                        string passphrase = data.Pass;
                        //nik = "5204056811830106";//"30122019"; // Dev
                        //passphrase = "281183@_dian";//"#1234qwer*"; // Dev
                        string ttdid = info.ttdid;
                        try
                        {
                            if (string.IsNullOrEmpty(dokid))
                            {
                                tr.Pesan = "Dokumen tidak ditemukan";
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                            if (String.IsNullOrEmpty(namapegawai))
                            {
                                tr.Pesan = "Penandatangan tidak ditemukan";
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                            if (String.IsNullOrEmpty(passphrase))
                            {
                                tr.Pesan = "Harap masukkan Passphrase";
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                passphrase = Server.UrlEncode(passphrase);
                            }

                            tr = CreatePdf(usr, dokid, kantorid, nik, passphrase, ttdid, userid, data.GenerateFooter).Result;
                            status = tr.Status;
                            pesan = tr.Pesan;
                        }
                        catch (Exception ex)
                        {
                            pesan = ex.Message;
                        }
                    }
                    else
                    {
                        tr = AddFooter(dokid, kantorid, data.PosisiTTE, namapegawai).Result;
                    }
                }
            }
            catch (Exception ex)
            {
                pesan = ex.Message;
            }

            return Json(new { Status = status, Pesan = pesan }, JsonRequestBehavior.AllowGet);
        }

        public async Task<TransactionResult> AddFooter(string dokid, string kantorid, string halaman, string namapegawai)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string tipe = "DokumenTTE";
                int cekversi = mdl.CekVersi(dokid);
                DateTime tglSunting = mdl.getTglSunting(dokid);
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(dokid), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(cekversi.ToString()), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(tglSunting)].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            byte[] b = new byte[strm.Length];
                            strm.Read(b, 0, (int)strm.Length);
                            memoryStream = new MemoryStream(b);
                            int posTTE = 0;
                            if (halaman.Equals("pertama"))
                            {
                                posTTE = 1;
                            }
                            else if (halaman.Equals("terakhir"))
                            {
                                posTTE = 0;
                            }
                            else if (!string.IsNullOrEmpty(halaman))
                            {
                                posTTE = Convert.ToInt32(halaman);
                            }
                            var InputStr = new PDFBuilder().Build(
                               streamPdf: memoryStream,
                               template: PDFBuilder.Template.FOOTER,
                               pageTte: posTTE);
                            if (InputStr.Output != null)
                            {
                                var _output = new byte[InputStr.Output.Length];
                                InputStr.Output.Read(_output, 0, (int)InputStr.Output.Length);
                                memoryStream = new MemoryStream(_output);
                            }

                            // Simpan File
                            cekversi += 1;
                            reqmessage = new HttpRequestMessage();
                            content = new MultipartFormDataContent();
                            var namafile = mdl.getFileName(dokid);
                            content.Add(new StringContent(kantorid), "kantorId");
                            content.Add(new StringContent(tipe), "tipeDokumen");
                            content.Add(new StringContent(dokid), "dokumenId");
                            content.Add(new StringContent(".pdf"), "fileExtension");
                            content.Add(new StringContent(cekversi.ToString()), "versionNumber");
                            content.Add(new StreamContent(memoryStream), "file", namafile);

                            reqmessage.Method = HttpMethod.Post;
                            reqmessage.Content = content;
                            reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(mdl.GetServerDate())].ToString(), "Store"));

                            using (var _client = new HttpClient())
                            {
                                var _reqresult = _client.SendAsync(reqmessage).Result;
                                result.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                                result.Pesan = "[Generate Footer] Storage Server - " + reqresult.ReasonPhrase;
                                if (result.Status)
                                {
                                    result = mdl.UpdateVersiDokumen(dokid, cekversi, namapegawai);
                                }
                            }
                        }
                        else
                        {
                            result.Pesan = "[Generate Footer] Dokumen tidak ditemukan";
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Pesan = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.Pesan = string.Concat("[Generate Footer] Gagal Menyiapkan Dokumen \n", ex.Message);
            }
            return result;
        }

        [HttpPost]
        public ActionResult TolakPengajuan(string id, string alasan)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    var usr = functions.claimUser();
                    result = mdl.TolakPengajuan(id, usr.UserId, alasan);
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

        public ActionResult cekStatusTTE()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string pegawaiid = usr.PegawaiId;

            string result = mdl.getInfoTTE(pegawaiid, "EMAIL");

            return Json(result);
        }

        public TransactionResult validasiUpload(HttpPostedFileBase file)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = true, Pesan = "" };

            byte[] pdf = new byte[file.ContentLength];
            file.InputStream.Read(pdf, 0, pdf.Length);
            PdfReader pdfreader = new PdfReader(pdf);
            AcroFields fields = pdfreader.AcroFields;
            AcroFields.Item item = fields.GetFieldItem("Signature1");
            List<string> names = fields.GetSignatureNames();
            if (names.Count == 0)
            {
                result.Status = false;
                result.Pesan = "TTE Tidak ditemukan";
            }
            else
            {
                foreach (string name in names)
                {
                    PdfPKCS7 pk = fields.VerifySignature(name);
                    var cal = pk.SignDate;
                    var pkc = pk.Certificates;

                    if (!pk.Verify())
                    {
                        result.Status = false;
                        result.Pesan = "TTE tidak valid";
                        break;
                    }
                    if (!pk.VerifyTimestampImprint())
                    {
                        result.Status = false;
                        result.Pesan = "Tanggal TTE tidak valid";
                        break;
                    }
                    if (pkc == null)
                    {
                        result.Status = false;
                        result.Pesan = "Sertifikasi tidak temukan";
                        break;
                    }

                }
            }
            return result;
        }

        public TransactionResult validasiKonten(Stream strm)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = true, Pesan = "" };

            MemoryStream _ms = (MemoryStream)strm;
            byte[] pdf = _ms.ToArray();
            PdfReader pdfreader = new PdfReader(pdf);
            AcroFields fields = pdfreader.AcroFields;
            AcroFields.Item item = fields.GetFieldItem("Signature1");
            List<string> names = fields.GetSignatureNames();
            if (names.Count == 0)
            {
                result.Status = false;
                result.Pesan = "TTE Tidak ditemukan";
            }
            else
            {
                foreach (string name in names)
                {
                    PdfPKCS7 pk = fields.VerifySignature(name);
                    var cal = pk.SignDate;
                    var pkc = pk.Certificates;

                    if (!pk.Verify())
                    {
                        result.Status = false;
                        result.Pesan = "TTE tidak valid";
                        break;
                    }
                    if (!pk.VerifyTimestampImprint())
                    {
                        result.Status = false;
                        result.Pesan = "Tanggal TTE tidak valid";
                        break;
                    }
                    if (pkc == null)
                    {
                        result.Status = false;
                        result.Pesan = "Sertifikasi tidak temukan";
                        break;
                    }

                }
            }
            return result;
        }

        public JsonResult ResetPassPhrase()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            try
            {
                string pegawaiid = usr.PegawaiId;

                string NIK = mdl.getInfoTTE(pegawaiid, "NIK");
                var url = string.Format(ConfigurationManager.AppSettings["DigitalSignatureUrl"].ToString() + "api/user/passphrase/forget/" + NIK);
                var request = WebRequest.Create(url);

                string userName = ConfigurationManager.AppSettings["AuthUserName"].ToString();
                string userPassword = ConfigurationManager.AppSettings["AuthPassword"].ToString();
                string auth = Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));

                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + auth);
                request.Method = "POST";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();

                if (!String.IsNullOrEmpty(responseFromServer))
                {
                    result.Status = true;
                    StatusResponse responseResult = JsonConvert.DeserializeObject<StatusResponse>(responseFromServer);
                    result.Pesan = responseResult.message;
                }
                else
                {
                    result.Status = false;
                    result.Pesan = "Tidak dapat menentukan status validasi user, Server BSSN tidak mengirimkan pesan apapun";
                }
            }
            catch (WebException wex)
            {
                using (var stream = wex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    if (!String.IsNullOrEmpty(responseFromServer))
                    {
                        result.Status = false;
                        StatusErrorResponse responseResult = JsonConvert.DeserializeObject<StatusErrorResponse>(responseFromServer);
                        result.Pesan = responseResult.message + " " + responseResult.error;
                    }
                    else
                    {
                        result.Status = false;
                        result.Pesan = "Tidak dapat menentukan status validasi user, Server BSSN tidak mengirimkan pesan apapun";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CekDokumenUpload(DokumenTTE data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            HttpPostedFileBase mfile = Request.Files[0];
            if (mfile == null || mfile.ContentType != "application/pdf")
            {
                throw new Exception("File harus pdf");
            }

            tr = validasiUpload(Request.Files[0]);

            return Json(new { Status = tr.Status, Pesan = tr.Pesan }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult cekPenolakan(string dokid)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            if (!string.IsNullOrEmpty(dokid))
            {
                result = mdl.cekPenolakanDokumen(dokid);
            }
            else
            {
                result.Status = false;
                result.Pesan = "Pengenal Dokumen tidak ditemukan";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewPdf_KopSurat(string l1, string l2, string al, string tl, string em, string fs)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new { Status = false, Message = "" };

            PdfUtil objPdf = new PdfUtil();

            string path = Server.MapPath("~/Reports/myfile.pdf");
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
            cell.AddElement(objPdf.AddTitleSurat(l1, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(l2, Element.ALIGN_CENTER, 15f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            table.AddCell(tableIn);

            columnWidths = new float[] { 100f, 540f };
            table.SetWidths(columnWidths);

            doc.Add(table);

            string fullAlamat = string.Concat("<p>", al);
            if (!string.IsNullOrEmpty(tl))
            {
                fullAlamat += string.Concat(" Telepon: ", tl);
            }
            if (!string.IsNullOrEmpty(em))
            {
                fullAlamat += string.Concat(" <i>email: ", em, "</i>");
            }
            fullAlamat += "</p>";

            foreach (IElement element in XMLWorkerHelper.ParseToElementList(fullAlamat, "p { font-family: Arial Narrow; font-size: " + fs + "px; text-align: center; }"))
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

            doc.Close();

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var resultAddFooter = new PDFBuilder().Build(
               streamPdf: mss,
               template: PDFBuilder.Template.FOOTER,
               pageTte: 0);

            var docfile = new FileStreamResult(
                resultAddFooter.Output,
                MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("KopSurat", ".pdf");

            ViewBag.FileSP = docfile;

            return docfile;
        }

        public async Task<TransactionResult> ProcessSignNaskahDinas(MemoryStream memoryStream, string NIK, string unitkerjaid, string PassPhrase, string TTDId, string kantoridTTD, string DokId, int versi, string tampilan)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string userid = usr.UserId;
            string nama = usr.NamaPegawai;
            string halaman = mdl.getPosisiTTE(DokId);
            TransactionResult result = new TransactionResult() { Status = false, Pesan = "" };
            string pHal = Regex.IsMatch(halaman, @"\d") ? "page" : "halaman";
            var kvp = new[] {
                    new KeyValuePair<string, string>("nik",NIK),
                    new KeyValuePair<string, string>("passphrase", PassPhrase),
                    new KeyValuePair<string, string>("tampilan", tampilan),
                    new KeyValuePair<string, string>(pHal, halaman),
                    new KeyValuePair<string, string>("image", "true"),
                    new KeyValuePair<string, string>("linkQR", ""),
                    new KeyValuePair<string, string>("xAxis", "60"),
                    new KeyValuePair<string, string>("yAxis", "70"),
                    new KeyValuePair<string, string>("width", "115"),
                    new KeyValuePair<string, string>("height", "20")
            };

            var address = string.Format(DigitalSignatureUrl() + "api/sign/pdf?{0}", string.Join("&", kvp.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))));
            var addressdownload = DigitalSignatureUrl() + "api/sign/download/";
            string logid = mdl.NewGuID();
            try
            {
                var docfileTTD = new MemoryStream();
                if (tampilan == "visible")
                {
                    result = mdl.GetKodeFile(DokId);
                    if (result.Status)
                    {
                        string kode = result.Pesan;
                        string urlkey = string.Concat(UrlDokumenTTE(), "?q=", kode);
                        if (!string.IsNullOrEmpty(kode))
                        {
                            QRCodeGenerator qrGenerator = new QRCodeGenerator();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(urlkey, QRCodeGenerator.ECCLevel.Q);
                            QRCode qrCode = new QRCode(qrCodeData);
                            System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20);
                            System.Drawing.Image logo = System.Drawing.Image.FromFile(Server.MapPath("~/Reports/logoqr_small.png"));
                            int left = (qrCodeImage.Width / 2) - ((int)logo.Width / 2) - 20;
                            int top = (qrCodeImage.Height / 2) - ((int)logo.Height / 2) - 20;
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(qrCodeImage);
                            g.DrawImage(logo, new System.Drawing.Point(left, top));

                            MemoryStream ms = null;
                            try
                            {
                                using (var stream = new MemoryStream())
                                {
                                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                                    docfileTTD = new MemoryStream(stream.ToArray());
                                }
                            }
                            catch (ArgumentNullException ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }

                string userName = ConfigurationManager.AppSettings["AuthUserName"].ToString();
                string userPassword = ConfigurationManager.AppSettings["AuthPassword"].ToString();

                string tipepengirim = "NIK";
                string idpengirim = NIK;
                string aplikasi = "Eoffice";
                string tipedokumen = "DokumenTTE";
                string servis = "SIGN DOKUMEN";
                result = mdl.InsertLog(logid, tipepengirim, idpengirim, aplikasi, tipedokumen, "", servis, address);
                if (!result.Status) { return result; }

                var request = WebRequest.Create(address);

                string auth = Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));
                request.Headers.Add("Authorization", "Basic " + auth);
                request.Method = "POST";
                var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                boundary = "--" + boundary;

                using (var requestStream = request.GetRequestStream())
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", "file.pdf", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", MediaTypeNames.Application.Pdf, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    memoryStream.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "imageTTD", "ttd.png", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", "image/png", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    docfileTTD.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                HttpStatusCode wRespStatusCode = response.StatusCode;
                WebHeaderCollection responseHeader = request.GetResponse().Headers;
                var iddokumen = responseHeader["id_dokumen"];
                string hasil = responseHeader.ToString();
                string status = wRespStatusCode == HttpStatusCode.OK ? "T" : "F";

                result = mdl.UpdateLog(logid, hasil, status);
                if (!result.Status) { return result; }

                var requestDownload = WebRequest.Create(addressdownload + iddokumen);
                requestDownload.Headers.Add("Authorization", "Basic " + auth);
                requestDownload.Method = "GET";

                var responseDownload = requestDownload.GetResponse();
                var responseStreamDownload = responseDownload.GetResponseStream();


                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(responseStreamDownload);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                // Display the content.                  
                using (var stream = new MemoryStream())
                {

                    if (tampilan == "visible")
                    {
                        var dt = mdl.GetServerDate();
                        string nomor = surat.getPenomoranSurat(DokId, unitkerjaid, dt);
                        if (!string.IsNullOrEmpty(nomor))
                        {
                            var resUpdateForm = new PDFBuilder().UpdateForm(responseStream, dt.ToString("dd MMMM yyyy"), nomor, true);
                            if (resUpdateForm.Success)
                            {
                                responseStream = resUpdateForm.Output;
                                result = mdl.SimpanFieldSurat(DokId, nomor, dt);
                                if (!result.Status)
                                {
                                    return result;
                                }
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.Pesan = "Penomoran Gagal";
                            return result;
                        }
                    }
                    responseStream.CopyTo(stream);
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "DokumenTTE";
                    var namafile = mdl.getFileName(DokId);
                    content.Add(new StringContent(kantoridTTD), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(DokId), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent((versi + 1).ToString()), "versionNumber");
                    content.Add(new StreamContent(new MemoryStream(stream.ToArray())), "file", namafile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(mdl.GetServerDate())].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        result.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                        result.Pesan = "Storage Server - " + reqresult.ReasonPhrase;
                    }

                    if (result.Status)
                    {
                        result = mdl.ProsesTandaTangan(DokId, userid, nama);
                    }
                }
            }
            catch (WebException wex)
            {
                using (var stream = wex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    if (!String.IsNullOrEmpty(responseFromServer))
                    {
                        result.Status = false;
                        StatusErrorResponse responseResult = JsonConvert.DeserializeObject<StatusErrorResponse>(responseFromServer);
                        result.Pesan = responseResult.message + " " + responseResult.error.Replace("!!! 2031", "!");
                    }
                    else
                    {
                        result.Status = false;
                        result.Pesan = "Tidak dapat menentukan status validasi user, Server BSSN tidak mengirimkan pesan apapun";
                    }
                }
            }
            catch (Exception ex)
            {
                result = mdl.UpdateLog(logid, ex.Message, "F");
                result.Pesan = ex.Message;
                result.Status = false;
            }

            return result;
        }

        public ActionResult GetPegawaiByJabatanNama(int? draw, int? start, int? length)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<Pegawai>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;
            string profileidtu = usr.ProfileIdTU;
            string metadata = Request.Form["metadata"].ToString();
            string namajabatan = Request.Form["namajabatan"].ToString();
            string namapegawai = Request.Form["namapegawai"].ToString();
            string unitkerjaid = string.Empty;
            string uk = string.Empty;
            try
            {
                unitkerjaid = Request.Form["unitkerjaid"].ToString();
                uk = unitkerjaid;
            }
            catch
            {
                unitkerjaid = usr.UnitKerjaId;
            }
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
                result = new SuratModel().GetPegawaiByJabatanNama(profileidtu, namajabatan, namapegawai, metadata, userid, from, to, unitkerja: uk);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult KirimNotif(string did, string nip)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            if (!string.IsNullOrEmpty(did) && !string.IsNullOrEmpty(nip))
            {
                var usr = functions.claimUser();
                try
                {
                    var data = mdl.GetDokumenElektronik(did);
                    new Mobile().KirimNotifikasi(nip, "asn", usr.NamaPegawai, string.Concat("Permintaan TTE Surat Nomor : ", Server.UrlDecode(data.NomorSurat)), "Pengingat TTE");
                    result.Status = true;
                }
                catch (Exception ex)
                {
                    result.Pesan = ex.Message;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PersetujuanAkses()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public ActionResult DaftarDokumenPengajuan(int? draw, int? start, int? length, CariPengajuanAkses f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<PengajuanAkses>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.UnitKerjaId = usr.UnitKerjaId;
                f.PegawaiId = usr.PegawaiId;
                f.MetaData = string.IsNullOrEmpty(f.MetaData) ? string.Empty : Server.UrlEncode(f.MetaData);
                result = mdl.GetListPengajuanAkses(f, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public FileStreamResult GeneratePersetujuanHakAkses(string id, userIdentity usr)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var sm = new SuratModel();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;

            var data = new DataMasterModel().getPengajuanAksesKKP(id);
            data.PegawaiId = usr.PegawaiId;
            data.NamaPegawai = usr.NamaPegawai;
            data.ListAkses = new DataMasterModel().getListHakAkses(id, 0, 0);

            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            PdfUtil objPdf = new PdfUtil();
            var phrase = new Phrase();
            Font tahomaBold10 = FontFactory.GetFont("Tahoma", 10, Font.BOLD, BaseColor.BLACK);
            Font tahomaNormal10 = FontFactory.GetFont("Tahoma", 10, Font.NORMAL, BaseColor.BLACK);
            Font tahomaBold11 = FontFactory.GetFont("Tahoma", 11, Font.BOLD, BaseColor.BLACK);
            Font tahomaNormal11 = FontFactory.GetFont("Tahoma", 11, Font.NORMAL, BaseColor.BLACK);

            string path = Server.MapPath("~/Reports/myfile.pdf");
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
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L1, Element.ALIGN_CENTER, 14f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L2, Element.ALIGN_CENTER, 14f, Font.BOLD, 0f, 0f, 0f));
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

            /* Nomor Pengesahan */
            Paragraph par = new Paragraph();
            Phrase pr1 = new Phrase("PERSETUJUAN\nHAK AKSES KKP\n", tahomaBold11);
            par.Add(pr1);
            //Phrase pr2 = new Phrase("Nomor: " + data.NomorPersetujuan, tahomaNormal11);
            //par.Add(pr2);
            par.Alignment = Rectangle.ALIGN_CENTER;
            doc.Add(par);

            par = new Paragraph(string.Concat("\nPada hari ini tanggal ", data.TanggalPersetujuan, " dinyatakan bahwa disetujui untuk pemberian akses Aplikasi KKP kepada ", data.NamaTarget, " dengan NIP ", data.TargetId, " pada ", (data.ListAkses.Count > 0) ? " Kantor, dengan uraian sebagai berikut :" : "."), tahomaNormal11);
            par.Alignment = Rectangle.ALIGN_JUSTIFIED;
            doc.Add(par);

            if (data.ListAkses.Count > 0)
            {
                table = new PdfPTable(4);
                table.SpacingBefore = 15f;
                table.WidthPercentage = 100;

                cell.PaddingBottom = 5f;
                cell.PaddingLeft = 5f;
                cell.HorizontalAlignment = 2;
                cell.Phrase = new Phrase("#", tahomaBold11);
                table.AddCell(cell);

                cell.HorizontalAlignment = 0;
                cell.Phrase = new Phrase("Kantor", tahomaBold11);
                table.AddCell(cell);

                cell.HorizontalAlignment = 0;
                cell.Phrase = new Phrase("Profile", tahomaBold11);
                table.AddCell(cell);

                cell.HorizontalAlignment = 0;
                cell.Phrase = new Phrase("Tipe", tahomaBold11);
                table.AddCell(cell);

                columnWidths = new float[] { 10, 100, 40, 20 };
                table.SetWidths(columnWidths);
                doc.Add(table);

                if (data.ListAkses.Count > 0)
                {
                    foreach (var dp in data.ListAkses)
                    {
                        table = new PdfPTable(4);
                        table.WidthPercentage = 100;

                        cell.HorizontalAlignment = 2;
                        cell.Phrase = new Phrase(dp.RNumber.ToString(), tahomaNormal10);
                        table.AddCell(cell);

                        cell.HorizontalAlignment = 0;
                        cell.Phrase = new Phrase(dp.Kantor, tahomaNormal10);
                        table.AddCell(cell);

                        cell.HorizontalAlignment = 0;
                        cell.Phrase = new Phrase(dp.Profile, tahomaNormal10);
                        table.AddCell(cell);

                        cell.HorizontalAlignment = 0;
                        cell.Phrase = new Phrase(dp.Tipe, tahomaNormal10);
                        table.AddCell(cell);

                        columnWidths = new float[] { 10, 100, 40, 20 };
                        table.SetWidths(columnWidths);
                        doc.Add(table);
                    }
                }
            }

            table = new PdfPTable(1);
            table.SpacingBefore = 15f;
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(new Phrase("Demikian persetujuan ini dibuat untuk dapat dipergunakan sebagaimana mestinya.", tahomaNormal11));
            table.AddCell(tableIn);

            PdfPTable ttd_table = new PdfPTable(new float[] { 50, 100 });
            //table.SplitLate = false;
            ttd_table.SpacingBefore = 15f;
            ttd_table.SpacingAfter = 15f;
            ttd_table.DefaultCell.Padding = 5f;
            ttd_table.DefaultCell.Border = 0;
            ttd_table.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            ttd_table.WidthPercentage = 100;
            ttd_table.HeaderRows = 2;

            PdfPCell ttd = new PdfPCell();
            ttd.HorizontalAlignment = 1;
            ttd.PaddingBottom = 5;
            ttd.Border = 0;
            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);
            //phrase = new Phrase("Bogor" + ", " + data.TanggalPersetujuan, tahomaNormal11);
            phrase = new Phrase(data.TanggalPersetujuan, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);
            phrase = new Phrase(data.Jabatan, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd.PaddingBottom = 5f;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd.PaddingTop = 40f;
            ttd_table.AddCell(ttd);
            ttd.FixedHeight = 50f;
            Image tte_png = Image.GetInstance(Server.MapPath("~/Reports/TTE.png"));
            tte_png.ScaleToFitHeight = false;
            tte_png.ScalePercent(50f);
            phrase = new Phrase();
            phrase.Font = tahomaNormal10;
            chunk = new Chunk(tte_png, 0, 0, false);
            phrase.Add(chunk);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd.FixedHeight = 0f;
            ttd.PaddingBottom = 0f;
            ttd_table.AddCell(ttd);
            phrase = new Phrase(data.NamaPegawai, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd.PaddingTop = 5f;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd.PaddingTop = 0f;
            ttd_table.AddCell(ttd);
            phrase = new Phrase("NIP. " + data.PegawaiId, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);

            table.AddCell(ttd_table);
            doc.Add(table);

            pw.Flush();
            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            //var resultAddFooter = new PDFBuilder().Build(
            //   streamPdf: mss,
            //   template: PDFBuilder.Template.FOOTER,
            //   pageTte: 0);

            var docfile = new FileStreamResult(
                //resultAddFooter.Output,
                mss,
                System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("DokumenElektronik", ".pdf");

            return docfile;
        }

        [HttpPost]
        public JsonResult PersetujuanAksesKKP(string id, string pps, string alasan, bool resp)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string nip = usr.PegawaiId;
            string kantorid = usr.KantorId;
            try
            {
                if (String.IsNullOrEmpty(nip))
                {
                    tr.Pesan = "Penandatangan tidak ditemukan";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }

                //var BerkasDokumen = new List<DokumenOutput>();

                if (resp)
                {
                    if (string.IsNullOrEmpty(pps))
                    {
                        tr.Pesan = "PassPhrase harus diisi";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    PenandatanganInfo info = mdl.getPenandatanganInfo(nip);
                    if (info == null)
                    {
                        tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    string nik = info.nik;
                    string passphrase = functions.bsreEncode(pps);
                    if (OtorisasiUser.isTrainBSSN)
                    {
                        nik = dev_nik;
                        passphrase = functions.bsreEncode(dev_pass);
                    }
                    string ttdid = info.ttdid;
                    //tr = mBeritaAcara.GenerateBeritaAcara(id, log.KantorId, log.UserId, log.PegawaiId, out BerkasDokumen);
                    if (!string.IsNullOrEmpty(nik))
                    {
                        try
                        {
                            string NomorBA = tr.Pesan;
                            string input = string.Empty;
                            var reqmessage = new HttpRequestMessage();
                            var content = new MultipartFormDataContent();
                            FileStreamResult dokumen = GeneratePersetujuanHakAkses(id, usr);
                            var strDok = dokumen.FileStream;
                            byte[] byt = new byte[strDok.Length];
                            Stream strm = strDok;
                            if (strm == null || byt.Length == 0)
                            {
                                tr.Pesan = "[Pdf] Gagal Membuat File : Hasil Generate Kosong";
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                            strm.Read(byt, 0, byt.Length);
                            string tipe = "AksesKKP";
                            content.Add(new StringContent(usr.KantorId), "kantorId");
                            content.Add(new StringContent(tipe), "tipeDokumen");
                            content.Add(new StringContent(id), "dokumenId");
                            content.Add(new StringContent(".pdf"), "fileExtension");
                            content.Add(new StringContent("1"), "versionNumber");

                            using (MemoryStream outstrm = new MemoryStream())
                            {
                                MemoryStream ms = new MemoryStream(byt.ToArray());

                                var kvp = new[] {
                                    new KeyValuePair<string, string>("nik",nik),
                                    new KeyValuePair<string, string>("passphrase", passphrase),
                                    new KeyValuePair<string, string>("tampilan", "invisible"),
                                    new KeyValuePair<string, string>("halaman", "terakhir"),
                                    new KeyValuePair<string, string>("image", "true"),
                                    new KeyValuePair<string, string>("linkQR", ""),
                                    new KeyValuePair<string, string>("xAxis", "67"), // 
                                    new KeyValuePair<string, string>("yAxis", "66"),
                                    new KeyValuePair<string, string>("width", "136"), // x
                                    new KeyValuePair<string, string>("height", "13") // y
                            };

                                var address = string.Format(DigitalSignatureUrl() + "api/sign/pdf?{0}", string.Join("&", kvp.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))));
                                var addressdownload = DigitalSignatureUrl() + "api/sign/download/";
                                string logid = mdl.NewGuID();
                                try
                                {
                                    var docfileTTD = new MemoryStream();
                                    string userName = ConfigurationManager.AppSettings["AuthUserName"].ToString();
                                    string userPassword = ConfigurationManager.AppSettings["AuthPassword"].ToString();

                                    string tipepengirim = "NIK";
                                    string idpengirim = nik;
                                    string aplikasi = "KKP2";
                                    string tipedokumen = tipe;
                                    string servis = "SIGN DOKUMEN";
                                    tr = mdl.InsertLog(logid, tipepengirim, idpengirim, aplikasi, tipedokumen, "", servis, address);
                                    if (!tr.Status)
                                    {
                                        tr.Pesan = string.Concat("[DB] Membuat Log : ", tr.Pesan);
                                        return Json(tr, JsonRequestBehavior.AllowGet);
                                    }

                                    var request = WebRequest.Create(address);

                                    string auth = Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));
                                    request.Headers.Add("Authorization", "Basic " + auth);
                                    request.Method = "POST";
                                    var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                                    request.ContentType = "multipart/form-data; boundary=" + boundary;
                                    boundary = "--" + boundary;

                                    using (var requestStream = request.GetRequestStream())
                                    {
                                        var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", "file.pdf", Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", MediaTypeNames.Application.Pdf, Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        ms.CopyTo(requestStream);
                                        buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);

                                        buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "imageTTD", "ttd.png", Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", "image/png", Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        docfileTTD.CopyTo(requestStream);
                                        buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);

                                        var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                                        requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                                    }

                                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                    var responseStream = response.GetResponseStream();
                                    HttpStatusCode wRespStatusCode = response.StatusCode;
                                    WebHeaderCollection responseHeader = request.GetResponse().Headers;
                                    var iddokumen = responseHeader["id_dokumen"];
                                    string hasil = responseHeader.ToString();
                                    string status = wRespStatusCode == HttpStatusCode.OK ? "T" : "F";

                                    tr = mdl.UpdateLog(logid, hasil, status);
                                    if (!tr.Status)
                                    {
                                        tr.Pesan = string.Concat("[DB] Merubah Log : ", tr.Pesan);
                                        return Json(tr, JsonRequestBehavior.AllowGet);
                                    }

                                    // Display the content.                  
                                    using (var stream = new MemoryStream())
                                    {
                                        responseStream.CopyTo(stream);
                                        content.Add(new StreamContent(new MemoryStream(stream.ToArray())), "file", string.Concat(tipe, ".pdf"));
                                        reqmessage.Method = HttpMethod.Post;
                                        reqmessage.Content = content;
                                        reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                                        using (var client = new HttpClient())
                                        {
                                            var reqresult = client.SendAsync(reqmessage).Result;
                                            tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                                            if (!tr.Status)
                                            {
                                                tr.Pesan = string.Concat("[Konten] Gagal Menyimpan File : ", reqresult.ReasonPhrase);
                                            }
                                            else
                                            {
                                                tr = dataMasterModel.persetujuanAksesKKP(usr.UserId, usr.PegawaiId, id, resp);
                                                if (!tr.Status)
                                                {
                                                    tr.Pesan = string.Concat("[DB] ", tr.Pesan);
                                                }
                                            }
                                            return Json(tr, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                catch (WebException wex)
                                {
                                    using (var stream = wex.Response.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        string responseFromServer = reader.ReadToEnd();
                                        if (!String.IsNullOrEmpty(responseFromServer))
                                        {
                                            tr.Status = false;
                                            StatusErrorResponse responseResult = JsonConvert.DeserializeObject<StatusErrorResponse>(responseFromServer);
                                            tr.Pesan = string.Concat("[BSSN] Proses TTE Gagal : ", responseResult.message, " ", responseResult.error.Replace("!!! 2031", "!"));
                                        }
                                        else
                                        {
                                            tr.Status = false;
                                            tr.Pesan = "[BSSN] Proses TTE Gagal : Tidak ada respon";
                                        }
                                        return Json(tr, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    tr = mdl.UpdateLog(logid, ex.Message, "F");
                                    tr.Status = false;
                                    tr.Pesan = string.Concat("[BSSN] Proses TTE Gagal : ", ex.Message);
                                    return Json(tr, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            tr.Status = false;
                            tr.Pesan = string.Concat("[Pdf] Gagal Membuat File : ", ex.Message);
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    tr = dataMasterModel.persetujuanAksesKKP(usr.UserId, usr.PegawaiId, id, resp, alasan);
                }
            }
            catch (Exception ex)
            {
                tr.Status = false;
                tr.Pesan = string.Concat("[DB] : ", ex.Message);
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        private string MeteraiUrl()
        {
            return ConfigurationManager.AppSettings["MeteraiUrl"].ToString();
        }
        private string MeteraiUrlLogin()
        {
            return string.Concat("https://backendservice", MeteraiUrl(), "api/users/login");
        }
        private string MeteraiUrlUpload()
        {
            return string.Concat("https://fileupload", MeteraiUrl(), "uploaddoc2");
        }
        private string MeteraiUrlGenerateSN()
        {
            return string.Concat("https://stampv2", MeteraiUrl(), "chanel/stampv2");
        }

        [HttpPost]
        public JsonResult TestLoginMeterai()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                tr = doLoginMeterai(MeteraiUrlLogin(), "enterprisedev@easymail.digital", "Qwerty123!").Result;
            }
            catch (Exception ex)
            {
                tr.Pesan = ex.Message;
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public class loginMeterai
        {
            public string token { get; set; }
        }
        private async Task<TransactionResult> doLoginMeterai(string urlLogin, string vUser, string vPass)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlLogin);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        user = vUser,
                        password = vPass
                    });
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    tr.Status = httpResponse.StatusCode == HttpStatusCode.OK;
                    if (tr.Status)
                    {
                        try
                        {
                            dynamic json = JsonConvert.DeserializeObject(result);
                            tr.Pesan = json["token"];
                        }
                        catch (Exception ex)
                        {
                            tr.Status = false;
                            tr.Pesan = ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return tr;
        }

        private async Task<TransactionResult> ProcessMeteraiDocument(userIdentity usr, MemoryStream memoryStream, string NIK, string PassPhrase, string TTDId, string kantoridTTD, string DokId, int versi, string tampilan)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            string kantorid = usr.KantorId;
            string userid = usr.UserId;
            string nama = usr.NamaPegawai;
            string halaman = mdl.getPosisiTTE(DokId);
            if (OtorisasiUser.isTrainBSSN)
            {
                NIK = dev_nik;
                PassPhrase = dev_pass;
            }
            //var encryption = new EncryptionService();
            //string keyid = encryption.Encrypt(DokId, "Ketut", "E1F53135E559C253");
            TransactionResult result = new TransactionResult() { Status = false, Pesan = "" };
            string pHal = Regex.IsMatch(halaman, @"\d") ? "page" : "halaman";
            var kvp = new[] {
                    new KeyValuePair<string, string>("nik",NIK),
                    new KeyValuePair<string, string>("passphrase", functions.bsreEncode(PassPhrase)),
                    new KeyValuePair<string, string>("tampilan", tampilan), // visible // invisible
                    new KeyValuePair<string, string>("reason", tampilan.Equals("visible")?"Tanda Tangan Surat":"Paraf Pemeriksaan"),
                    new KeyValuePair<string, string>(pHal, halaman),
                    new KeyValuePair<string, string>("image", "true"),
//                    new KeyValuePair<string, string>("linkQR", ""),
                    new KeyValuePair<string, string>("xAxis", "67"), // 
                    new KeyValuePair<string, string>("yAxis", "66"),
                    new KeyValuePair<string, string>("width", "136"), // x
                    new KeyValuePair<string, string>("height", "13") // y
            };

            var address = string.Format(DigitalSignatureUrl() + "api/sign/pdf?{0}", string.Join("&", kvp.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))));
            var addressdownload = DigitalSignatureUrl() + "api/sign/download/";
            string logid = mdl.NewGuID();
            try
            {
                var docfileTTD = new MemoryStream();
                if (tampilan == "visible")
                {
                    result = mdl.GetKodeFile(DokId);
                    if (result.Status)
                    {
                        string kode = result.Pesan;
                        string urlkey = string.Concat(UrlDokumenTTE(), "?q=", kode);
                        if (!string.IsNullOrEmpty(kode))
                        {
                            QRCodeGenerator qrGenerator = new QRCodeGenerator();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(urlkey, QRCodeGenerator.ECCLevel.Q);
                            QRCode qrCode = new QRCode(qrCodeData);
                            System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20);
                            System.Drawing.Image logo = System.Drawing.Image.FromFile(Server.MapPath("~/Reports/logoqr_small.png"));
                            int left = (qrCodeImage.Width / 2) - ((int)logo.Width / 2) - 20;
                            int top = (qrCodeImage.Height / 2) - ((int)logo.Height / 2) - 20;
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(qrCodeImage);
                            g.DrawImage(logo, new System.Drawing.Point(left, top));

                            MemoryStream ms = null;
                            try
                            {
                                using (var stream = new MemoryStream())
                                {
                                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                                    docfileTTD = new MemoryStream(stream.ToArray());
                                }
                            }
                            catch (ArgumentNullException ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }

                string userName = ConfigurationManager.AppSettings["AuthUserName"].ToString();
                string userPassword = ConfigurationManager.AppSettings["AuthPassword"].ToString();

                string tipepengirim = "NIK";
                string idpengirim = NIK;
                string aplikasi = "Eoffice";
                string tipedokumen = "DokumenTTE";
                string servis = "SIGN DOKUMEN";
                result = mdl.InsertLog(logid, tipepengirim, idpengirim, aplikasi, tipedokumen, "", servis, address);
                if (!result.Status) { return result; }

                var request = WebRequest.Create(address);

                string auth = Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));
                request.Headers.Add("Authorization", "Basic " + auth);
                request.Method = "POST";
                var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                boundary = "--" + boundary;

                using (var requestStream = request.GetRequestStream())
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", "file.pdf", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", MediaTypeNames.Application.Pdf, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    memoryStream.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "imageTTD", "ttd.png", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", "image/png", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    docfileTTD.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                HttpStatusCode wRespStatusCode = response.StatusCode;
                WebHeaderCollection responseHeader = request.GetResponse().Headers;
                var iddokumen = responseHeader["id_dokumen"];
                string hasil = responseHeader.ToString();
                string status = wRespStatusCode == HttpStatusCode.OK ? "T" : "F";

                result = mdl.UpdateLog(logid, hasil, status);
                if (!result.Status) { return result; }

                using (var stream = new MemoryStream())
                {
                    responseStream.CopyTo(stream);
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "DokumenTTE";
                    var namafile = mdl.getFileName(DokId);
                    content.Add(new StringContent(kantoridTTD), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(DokId), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent((versi + 1).ToString()), "versionNumber");
                    content.Add(new StreamContent(new MemoryStream(stream.ToArray())), "file", namafile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[mdl.apiUrl(mdl.GetServerDate())].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        result.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                        result.Pesan = "Storage Server - " + reqresult.ReasonPhrase;
                    }

                    if (result.Status)
                    {
                        result = mdl.ProsesTandaTangan(DokId, userid, nama);
                    }
                }
            }
            catch (WebException wex)
            {
                using (var stream = wex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    if (!String.IsNullOrEmpty(responseFromServer))
                    {
                        result.Status = false;
                        StatusErrorResponse responseResult = JsonConvert.DeserializeObject<StatusErrorResponse>(responseFromServer);
                        result.Pesan = responseResult.message + " " + responseResult.error.Replace("!!! 2031", "!");
                    }
                    else
                    {
                        result.Status = false;
                        result.Pesan = "Tidak dapat menentukan status validasi user, Server BSSN tidak mengirimkan pesan apapun";
                    }
                }
            }
            catch (Exception ex)
            {
                result = mdl.UpdateLog(logid, ex.Message, "F");
                result.Pesan = ex.Message;
                result.Status = false;
            }

            return result;
        }

        public ActionResult PersetujuanJabatan()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public ActionResult DaftarPengajuanJabatanPelaksana(int? draw, int? start, int? length, CariPengajuanAkses f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<PengajuanAkses>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.PegawaiId = usr.PegawaiId;
                f.MetaData = string.IsNullOrEmpty(f.MetaData) ? string.Empty : Server.UrlEncode(f.MetaData);
                result = mdl.GetListPengajuanPelaksana(f, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public FileStreamResult GeneratePersetujuanJabatanPelaksana(string id, userIdentity usr)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var sm = new SuratModel();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;

            var data = new DataMasterModel().getPengajuanJabatan(id);
            data.PegawaiId = usr.PegawaiId;
            data.NamaPegawai = usr.NamaPegawai;

            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            PdfUtil objPdf = new PdfUtil();
            var phrase = new Phrase();
            Font tahomaBold10 = FontFactory.GetFont("Tahoma", 10, Font.BOLD, BaseColor.BLACK);
            Font tahomaNormal10 = FontFactory.GetFont("Tahoma", 10, Font.NORMAL, BaseColor.BLACK);
            Font tahomaBold11 = FontFactory.GetFont("Tahoma", 11, Font.BOLD, BaseColor.BLACK);
            Font tahomaNormal11 = FontFactory.GetFont("Tahoma", 11, Font.NORMAL, BaseColor.BLACK);

            string path = Server.MapPath("~/Reports/myfile.pdf");
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
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L1, Element.ALIGN_CENTER, 14f, Font.BOLD, 0f, 0f, 0f));
            tableIn.AddCell(cell);

            cell = new PdfPCell();
            cell.Border = 0;
            cell.Padding = 0f;
            cell.PaddingRight = 80f;
            cell.AddElement(objPdf.AddTitleSurat(kopsurat.NamaKantor_L2, Element.ALIGN_CENTER, 14f, Font.BOLD, 0f, 0f, 0f));
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

            Paragraph par = new Paragraph();
            Phrase pr1 = new Phrase("PERSETUJUAN PENUGASAN\nJABATAN PELAKSANA\n", tahomaBold11);
            par.Add(pr1);
            par.Alignment = Rectangle.ALIGN_CENTER;
            doc.Add(par);

            string _tipejabatan = "Definitif";
            switch (data.StatusPlt)
            {
                case 1:
                    _tipejabatan = "Pelaksana Tugas";
                    break;
                case 2:
                    _tipejabatan = "Pelaksana Harian";
                    break;
            }

            par = new Paragraph(string.Concat("\nPada hari ini tanggal ", data.TanggalPersetujuan, " dinyatakan bahwa disetujui untuk Penugasan Jabatan ", _tipejabatan, " kepada ", data.NamaTarget, " dengan NIP ", data.TargetId, " pada ", data.KantorTarget, " sebagai ", data.Jabatan), tahomaNormal11);
            par.Alignment = Rectangle.ALIGN_JUSTIFIED;
            doc.Add(par);

            table = new PdfPTable(1);
            table.SpacingBefore = 15f;
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;

            tableIn = new PdfPTable(1);
            tableIn.WidthPercentage = 100;
            tableIn.DefaultCell.Border = 0;
            tableIn.AddCell(new Phrase("Demikian persetujuan ini dibuat untuk dapat dipergunakan sebagaimana mestinya.", tahomaNormal11));
            table.AddCell(tableIn);

            PdfPTable ttd_table = new PdfPTable(new float[] { 50, 100 });
            //table.SplitLate = false;
            ttd_table.SpacingBefore = 15f;
            ttd_table.SpacingAfter = 15f;
            ttd_table.DefaultCell.Padding = 5f;
            ttd_table.DefaultCell.Border = 0;
            ttd_table.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            ttd_table.WidthPercentage = 100;
            ttd_table.HeaderRows = 2;

            PdfPCell ttd = new PdfPCell();
            ttd.HorizontalAlignment = 1;
            ttd.PaddingBottom = 5;
            ttd.Border = 0;
            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);
            phrase = new Phrase(data.TanggalPersetujuan, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);
            phrase = new Phrase(data.Jabatan, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd.PaddingBottom = 5f;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd.PaddingTop = 40f;
            ttd_table.AddCell(ttd);
            ttd.FixedHeight = 50f;
            Image tte_png = Image.GetInstance(Server.MapPath("~/Reports/TTE.png"));
            tte_png.ScaleToFitHeight = false;
            tte_png.ScalePercent(50f);
            phrase = new Phrase();
            phrase.Font = tahomaNormal10;
            chunk = new Chunk(tte_png, 0, 0, false);
            phrase.Add(chunk);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd.FixedHeight = 0f;
            ttd.PaddingBottom = 0f;
            ttd_table.AddCell(ttd);
            phrase = new Phrase(data.NamaPegawai, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd.PaddingTop = 5f;
            ttd_table.AddCell(ttd);

            phrase = new Phrase("", tahomaBold11);
            ttd.Phrase = phrase;
            ttd.PaddingTop = 0f;
            ttd_table.AddCell(ttd);
            phrase = new Phrase("NIP. " + data.PegawaiId, tahomaNormal11);
            ttd.Phrase = phrase;
            ttd_table.AddCell(ttd);

            table.AddCell(ttd_table);
            doc.Add(table);

            pw.Flush();
            doc.Close();

            // WRITE IN MEMORYSTREAM

            byte[] byteArray = ms.ToArray();

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(
                mss,
                MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("DokumenElektronik", ".pdf");

            return docfile;
        }

        [HttpPost]
        public JsonResult PersetujuanJabatanPelaksana(string id, string pps, string alasan, bool resp)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string nip = usr.PegawaiId;
            string kantorid = dataMasterModel.getKantorIdFromPengajuanJabatan(id);
            try
            {
                if (String.IsNullOrEmpty(nip))
                {
                    tr.Pesan = "Penandatangan tidak ditemukan";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }

                if (resp)
                {
                    if (string.IsNullOrEmpty(pps))
                    {
                        tr.Pesan = "PassPhrase harus diisi";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    PenandatanganInfo info = mdl.getPenandatanganInfo(nip);
                    if (info == null)
                    {
                        tr.Pesan = "Data NIP anda tidak terdaftar untuk TTE";
                        return Json(tr, JsonRequestBehavior.AllowGet);
                    }
                    string nik = info.nik;
                    string passphrase = functions.bsreEncode(pps);
                    if (OtorisasiUser.isTrainBSSN)
                    {
                        nik = dev_nik;
                        passphrase = functions.bsreEncode(dev_pass);
                    }
                    string ttdid = info.ttdid;
                    if (!string.IsNullOrEmpty(nik))
                    {
                        try
                        {
                            string NomorBA = tr.Pesan;
                            string input = string.Empty;
                            var reqmessage = new HttpRequestMessage();
                            var content = new MultipartFormDataContent();
                            FileStreamResult dokumen = GeneratePersetujuanJabatanPelaksana(id, usr);
                            var strDok = dokumen.FileStream;
                            byte[] byt = new byte[strDok.Length];
                            Stream strm = strDok;
                            if (strm == null || byt.Length == 0)
                            {
                                tr.Pesan = "[Pdf] Gagal Membuat File : Hasil Generate Kosong";
                                return Json(tr, JsonRequestBehavior.AllowGet);
                            }
                            strm.Read(byt, 0, byt.Length);
                            string tipe = "AksesKKP";
                            content.Add(new StringContent(kantorid), "kantorId");
                            content.Add(new StringContent(tipe), "tipeDokumen");
                            content.Add(new StringContent(id), "dokumenId");
                            content.Add(new StringContent(".pdf"), "fileExtension");
                            content.Add(new StringContent("1"), "versionNumber");

                            using (MemoryStream outstrm = new MemoryStream())
                            {
                                MemoryStream ms = new MemoryStream(byt.ToArray());

                                var kvp = new[] {
                                    new KeyValuePair<string, string>("nik",nik),
                                    new KeyValuePair<string, string>("passphrase", passphrase),
                                    new KeyValuePair<string, string>("tampilan", "invisible"),
                                    new KeyValuePair<string, string>("halaman", "terakhir"),
                                    new KeyValuePair<string, string>("image", "true"),
                                    new KeyValuePair<string, string>("linkQR", ""),
                                    new KeyValuePair<string, string>("xAxis", "67"), // 
                                    new KeyValuePair<string, string>("yAxis", "66"),
                                    new KeyValuePair<string, string>("width", "136"), // x
                                    new KeyValuePair<string, string>("height", "13") // y
                            };

                                var address = string.Format(DigitalSignatureUrl() + "api/sign/pdf?{0}", string.Join("&", kvp.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))));
                                var addressdownload = DigitalSignatureUrl() + "api/sign/download/";
                                string logid = mdl.NewGuID();
                                try
                                {
                                    var docfileTTD = new MemoryStream();
                                    string userName = ConfigurationManager.AppSettings["AuthUserName"].ToString();
                                    string userPassword = ConfigurationManager.AppSettings["AuthPassword"].ToString();

                                    string tipepengirim = "NIK";
                                    string idpengirim = nik;
                                    string aplikasi = "KKP2";
                                    string tipedokumen = tipe;
                                    string servis = "SIGN DOKUMEN";
                                    tr = mdl.InsertLog(logid, tipepengirim, idpengirim, aplikasi, tipedokumen, "", servis, address);
                                    if (!tr.Status)
                                    {
                                        tr.Pesan = string.Concat("[DB] Membuat Log : ", tr.Pesan);
                                        return Json(tr, JsonRequestBehavior.AllowGet);
                                    }

                                    var request = WebRequest.Create(address);

                                    string auth = Convert.ToBase64String(Encoding.Default.GetBytes(userName + ":" + userPassword));
                                    request.Headers.Add("Authorization", "Basic " + auth);
                                    request.Method = "POST";
                                    var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                                    request.ContentType = "multipart/form-data; boundary=" + boundary;
                                    boundary = "--" + boundary;

                                    using (var requestStream = request.GetRequestStream())
                                    {
                                        var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", "file.pdf", Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", MediaTypeNames.Application.Pdf, Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        ms.CopyTo(requestStream);
                                        buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);

                                        buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "imageTTD", "ttd.png", Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", "image/png", Environment.NewLine));
                                        requestStream.Write(buffer, 0, buffer.Length);
                                        docfileTTD.CopyTo(requestStream);
                                        buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                                        requestStream.Write(buffer, 0, buffer.Length);

                                        var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                                        requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                                    }

                                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                    var responseStream = response.GetResponseStream();
                                    HttpStatusCode wRespStatusCode = response.StatusCode;
                                    WebHeaderCollection responseHeader = request.GetResponse().Headers;
                                    var iddokumen = responseHeader["id_dokumen"];
                                    string hasil = responseHeader.ToString();
                                    string status = wRespStatusCode == HttpStatusCode.OK ? "T" : "F";

                                    tr = mdl.UpdateLog(logid, hasil, status);
                                    if (!tr.Status)
                                    {
                                        tr.Pesan = string.Concat("[DB] Merubah Log : ", tr.Pesan);
                                        return Json(tr, JsonRequestBehavior.AllowGet);
                                    }

                                    // Display the content.                  
                                    using (var stream = new MemoryStream())
                                    {
                                        responseStream.CopyTo(stream);
                                        content.Add(new StreamContent(new MemoryStream(stream.ToArray())), "file", string.Concat(tipe, ".pdf"));
                                        reqmessage.Method = HttpMethod.Post;
                                        reqmessage.Content = content;
                                        reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                                        using (var client = new HttpClient())
                                        {
                                            var reqresult = client.SendAsync(reqmessage).Result;
                                            tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                                            if (!tr.Status)
                                            {
                                                tr.Pesan = string.Concat("[Konten] Gagal Menyimpan File : ", reqresult.ReasonPhrase);
                                            }
                                            else
                                            {
                                                tr = dataMasterModel.persetujuanJabatanPelaksana(usr.UserId, usr.PegawaiId, id, resp);
                                                if (!tr.Status)
                                                {
                                                    tr.Pesan = string.Concat("[DB] ", tr.Pesan);
                                                }
                                            }
                                            return Json(tr, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                catch (WebException wex)
                                {
                                    using (var stream = wex.Response.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        string responseFromServer = reader.ReadToEnd();
                                        if (!String.IsNullOrEmpty(responseFromServer))
                                        {
                                            tr.Status = false;
                                            StatusErrorResponse responseResult = JsonConvert.DeserializeObject<StatusErrorResponse>(responseFromServer);
                                            tr.Pesan = string.Concat("[BSSN] Proses TTE Gagal : ", responseResult.message, " ", responseResult.error.Replace("!!! 2031", "!"));
                                        }
                                        else
                                        {
                                            tr.Status = false;
                                            tr.Pesan = "[BSSN] Proses TTE Gagal : Tidak ada respon";
                                        }
                                        return Json(tr, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    tr = mdl.UpdateLog(logid, ex.Message, "F");
                                    tr.Status = false;
                                    tr.Pesan = string.Concat("[BSSN] Proses TTE Gagal : ", ex.Message);
                                    return Json(tr, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            tr.Status = false;
                            tr.Pesan = string.Concat("[Pdf] Gagal Membuat File : ", ex.Message);
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    tr = dataMasterModel.persetujuanJabatanPelaksana(usr.UserId, usr.PegawaiId, id, resp, alasan);
                }
            }
            catch (Exception ex)
            {
                tr.Status = false;
                tr.Pesan = string.Concat("[DB] : ", ex.Message);
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }
    }
}