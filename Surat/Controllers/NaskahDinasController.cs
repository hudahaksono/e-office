﻿using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using PDFEditor;
using Surat.Codes;
using Surat.Codes.TataNaskah;
using Surat.Models;
using Surat.Models.Entities;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class NaskahDinasController : Controller
    {

        DataMasterModel dataMasterModel = new DataMasterModel();
        NaskahDinasModel nd = new NaskahDinasModel();
        SuratModel mdl = new SuratModel();
        Functions functions = new Functions();
        TandaTanganElektronikModel TTEM = new TandaTanganElektronikModel();
        PenomoranModel pm = new PenomoranModel();

        // GET: NaskahDinas
        public ActionResult KonsepList()
        {
            var usr = functions.claimUser();

            var srch = new CariDraftSurat();

            var data = nd.GetMyKonsep(usr.UserId,usr.UnitKerjaId, "P");
            foreach(var dt in data)
            {
                if (!string.IsNullOrEmpty(dt.Perihal))
                {
                    dt.Perihal = (dt.Perihal == "undefined") ? "-" : Server.UrlDecode(dt.Perihal);
                }                       
            }
            ViewBag.list = data;
            ViewBag.listEditor = nd.GetFormatPenomoran();

            return View(srch);
        }

        public ContentResult JumlahKonsepList()
        {
            string result = "";
            var usr = functions.claimUser();

            int jumlah = 0;

            try
            {
                var data = nd.GetKonsepList(usr.UserId, "P", "");
                jumlah = data.Count();
                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ContentResult JumlahPersetujuanKonsep()
        {
            string result = "";
            var usr = functions.claimUser();

            int jumlah = 0;

            try
            {
                var data = nd.GetJumlahPersetujuanKonsep(usr.UserId);
                jumlah = Decimal.ToInt32(data);
                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ActionResult ProsesList()
        {
            var usr = functions.claimUser();

            var srch = new CariDraftSurat();

            var data = nd.GetKonsepList(usr.UserId, "W", "");

            List<ListDraft> Pengolah = new List<ListDraft>();
            List<ListDraft> Persetujuan = new List<ListDraft>();
            List<ListDraft> Disetujui = new List<ListDraft>();

            foreach (var dt in data)
            {
                dt.Perihal = Server.UrlDecode(dt.Perihal);
                if(dt.StatusAcc == "P")
                {
                    dt.PerjalananKonsep = nd.RevisiCheck(dt.DraftCode) ? "REVISI" : "";
                    Pengolah.Add(dt);
                } else if (dt.StatusAcc == "W")
                {
                    Persetujuan.Add(dt);
                } 
                else if (dt.StatusAcc == "A")
                {
                    Disetujui.Add(dt);
                }
            }

            ViewBag.Pengolah = Pengolah;
            ViewBag.Persetujuan = Persetujuan;
            ViewBag.Disetujui = Disetujui;

            return View(srch);
        }

        public ContentResult JumlahProsesList()
        {
            string result = "";
            var usr = functions.claimUser();

            int jumlah = 0;

            try
            {
                var data = nd.GetKonsepList(usr.UserId, "W", "");
                jumlah = data.Count();
                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ActionResult KonsepFinal()
        {
            if (OtorisasiUser.isTU())
            {
                //var usr = functions.claimUser();
                //var unitkerja = $"{usr.UnitKerjaId}";
                //if (OtorisasiUser.IsActiveRole("'H2081100'"))
                //{
                //    unitkerja = $"'020102'";
                //} else if (OtorisasiUser.isTU())
                //{
                //    unitkerja = "";
                //    var lstunitkerja = nd.GetUnitKerjaFromTU(usr.PegawaiId);
                //    foreach (var unit in lstunitkerja)
                //    {
                //        unitkerja += string.IsNullOrEmpty(unitkerja) ? $"'{unit}'" : $",'{unit}'";
                //    }
                //}
                //var data = nd.GetFinalDraft(unitkerja, "");
                //foreach (var dt in data)
                //{
                //    dt.Perihal = Server.UrlDecode(dt.Perihal);
                //}
                //return View();
            }
            return View();
        }

        public JsonResult ListKonsepFinal(int? draw, int? start, int? length, string stage, string srchkey)
        {
            var usr = functions.claimUser();
            var unitkerja = OtorisasiUser.isTU() ? "" : usr.UnitKerjaId;
            var pegawaiid = OtorisasiUser.isTU() ? usr.PegawaiId : usr.UserId;
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            var result = nd.GetFinalDraftV2(OtorisasiUser.isTU(), usr, stage, from,to, srchkey);
            var total = result.Count > 0 ? result[0].Total : 0;
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }
                
        public JsonResult GetDetailTTE(string draftcode)
        {
            var usr = functions.claimUser();
            var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
            data.NamaPembuat = new WebApiModel().GetPegawaiNameByUserId(data.UserPembuat);
            data.Perihal = Server.UrlDecode(data.Perihal);
            data.TTE = nd.GetUserTtd(draftcode);
            data.AutoNumAvail = false;

            var buku = new BukuPenomran();

            var usrTTE = nd.GetUserTtd(draftcode);
            buku = pm.getBukuFromPenandatangan(usrTTE.Find(x => x.Tipe == "1").ProfileId);


            if (string.IsNullOrEmpty(data.NomorSurat) && buku != null)
            {
                data.NomorSurat = nd.PenomoranBuilder(data);
                data.AutoNumAvail = true;
            }            
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SimpanDraftNaskahDinas(DraftSurat data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string pNama = usr.NamaPegawai;
            data.UserPembuat = usr.UserId;
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");

            if (string.IsNullOrEmpty(data.listTujuan))
            {
                return Json(new { Status = false, Pesan = "Tujuan Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.UserPembuat))
            {
                return Json(new { Status = false, Pesan = "Kode Pembuat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.KodeArsip))
            {
                return Json(new { Status = false, Pesan = "Kode Arsip Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.TipeSurat))
            {
                return Json(new { Status = false, Pesan = "Tipe Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(data.SifatSurat))
            {
                return Json(new { Status = false, Pesan = "Sifat Surat Kosong" }, JsonRequestBehavior.AllowGet);
            }


            if (!string.IsNullOrEmpty(data.listTTE) && data.listTTE != "")
            {
                string[] str = data.listTTE.Split('|');
                data.TTE = new List<UserTTE>();
                UserTTE usertte = new UserTTE();
                int urut = 1;
                if (!string.IsNullOrEmpty(data.Pass))
                {
                    usertte.PenandatanganId = usr.UserId;
                    usertte.Tipe = "0";
                    usertte.ProfileId = usr.ProfileIdTU;
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
                        if (tte.Count() == 3)
                        {
                            usertte = new UserTTE();
                            usertte.PenandatanganId = new TandaTanganElektronikModel().getUserId(tte[0].ToString());
                            usertte.Tipe = tte[1].ToString();
                            usertte.ProfileId = tte[2].ToString();
                            usertte.Urut = (tte[1].ToString() == "0") ? urut : str.Length ;
                            data.TTE.Add(usertte);
                            urut = (tte[1].ToString() == "0")? urut+1 : urut;
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

            data.Perihal = data.Perihal;
            data.UnitKerjaId = string.IsNullOrEmpty(data.UnitKerjaId) ? usr.UnitKerjaId : data.UnitKerjaId;
            data.JumlahLampiran = "-";
            data.lstFileLampiran = new List<lampiranDraft>();

            if (data.isHaveLampiran)
            {
                if(data.lstLampiranIdSave != null) 
                {
                    foreach (var lamp in data.lstLampiranIdSave)
                    {
                        lampiranDraft datafile = new lampiranDraft();
                        datafile.namaFile = lamp;
                        datafile.save = true;
                        data.lstFileLampiran.Add(datafile);
                    }
                }

                if (data.fileUploadStream.Count > 1 && data.lstLampiranId != null)
                {
                    foreach (var mfile in data.fileUploadStream)
                    {
                        if (mfile != null && data.lstLampiranId.Contains(mfile.FileName))
                        {
                            if (mfile.ContentType != "application/pdf")
                            {
                                return Json(new { Status = false, Pesan = "Lampiran harus pdf" }, JsonRequestBehavior.AllowGet);
                            }

                            lampiranDraft datafile = new lampiranDraft();
                            datafile.namaFile = mfile.FileName;
                            MemoryStream ms1 = new MemoryStream();
                            mfile.InputStream.CopyTo(ms1);
                            datafile.ObjectFile = ms1.ToArray();
                            datafile.save = false;
                            data.lstFileLampiran.Add(datafile);
                        }
                    }
                }
            }            

            tr = nd.SimpanDraftNaskahDinas(data, pNama, kantorid, usr);
            return Json(new { Status = tr.Status, Pesan = tr.Pesan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PengajuanDraft(string id)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    var usr = functions.claimUser();
                    result = nd.PengajuanDraft(id, usr.UnitKerjaId, usr.UserId, usr.ProfileIdTU);
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

        public FileStreamResult GenerateDokumenElektronik(string id)
        {
            FileStreamResult docfile = null;
            var usr = functions.claimUser();
            var streamNd = new MemoryStream();
            var data = new DraftSurat();
            if (!string.IsNullOrEmpty(id))
            {
                data = nd.GetDraftSurat(id, usr.UnitKerjaId);
                try
                {
                    streamNd = getStreamNaskahDinas(data);
                    docfile = new FileStreamResult(streamNd, MediaTypeNames.Application.Pdf);
                }
                catch (Exception e)
                {
                    int loopcount = 0;

                    while (loopcount < 10)
                    {
                        data.IsiSurat += "<br>";
                        try
                        {
                            streamNd = getStreamNaskahDinas(data);
                            docfile = new FileStreamResult(streamNd, MediaTypeNames.Application.Pdf);
                            break;
                        }
                        catch(Exception ex)
                        {
                            loopcount++;
                        }
                    }
                }
            }
            ViewBag.FileSP = docfile;
            var InputStr = new PDFBuilder().Build(
                 streamPdf: streamNd,
                 template: PDFBuilder.Template.FOOTER,
                 pageTte: Int32.Parse(data.PosisiTTE));
            if(InputStr.Output != null)
            {
                docfile = new FileStreamResult(InputStr.Output, MediaTypeNames.Application.Pdf);
            }
            return docfile;
        }


        public MemoryStream getStreamNaskahDinas(DraftSurat data)
        {
            MemoryStream result = null;
            var usr = functions.claimUser();
            string pathx = Server.MapPath("~/Reports");
            if (!string.IsNullOrEmpty(data.DraftCode))
            {
                if (data.TipeSurat.Equals("Nota Dinas"))
                {
                    var ND = new NotaDinas();
                    result = ND.prepare(data, functions.claimUser(), pathx);
                }

                else if (data.TipeSurat.Equals("Surat Dinas"))
                {
                    var SD = new SuratDinas();
                    result = SD.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Undangan"))
                {
                    var SU = new SuratUndangan();
                    result = SU.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Keputusan"))
                {
                    var SK = new SuratKeputusan();
                    result = SK.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Tugas"))
                {
                    var ST = new SuratTugas();
                    result = ST.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Pernyataan"))
                {
                    var SP = new SuratPernyataan();
                    result = SP.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Perintah"))
                {
                    var SH = new SuratPelaksanaHarian();
                    result = SH.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Edaran"))
                {
                    var SE = new SuratEdaran();
                    result = SE.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Pengantar"))
                {
                    var SPND = new SuratPengantarNaskahDinas();
                    result = SPND.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Pengumuman"))
                {
                    var PENG = new Pengumuman();
                    result = PENG.prepare(data, functions.claimUser(), pathx);
                }
                else if (data.TipeSurat.Equals("Surat Keterangan"))
                {
                    var SKET = new SuratKeterangan();
                    result = SKET.prepare(data, functions.claimUser(), pathx);
                }
            }

            using (var stream = new MemoryStream(result.ToArray()))
            {
                using (var reader = new PdfReader(stream))
                {
                    using (var document = new PdfDocument(reader))
                    {
                        if (!int.TryParse(data.PosisiTTE, out int value))
                        {
                            data.PosisiTTE = string.Empty;
                        }
                        int numberOfPages = document.GetNumberOfPages();
                        for (int page = 1; page <= numberOfPages; page++)
                        {
                            document.GetPage(page).SetIgnorePageRotationForContent(true);
                            string check = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(document.GetPage(page));                           
                            if (check.Contains(data.DraftCode) &&  data.PosisiTTE != page.ToString())
                            {
                                var ipt = nd.SetHalamanTTE(page.ToString(), data.DraftCode);
                                if (ipt.Status)
                                {
                                    data.PosisiTTE = page.ToString();
                                }
                            }
                        }
                    }
                }
            }


            return result;
        }

        public ActionResult OpenEditor(string id, string konsep, bool salin = false, bool t = false, bool f = true, bool v = false)
        {
            var usr = functions.claimUser();
            var data = new DraftSurat();
            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
            ViewBag.Salin = false;
            ViewBag.Kembali = f;
            ViewBag.Verif = v;
            if (string.IsNullOrEmpty(id))
            {
                if (string.IsNullOrEmpty(konsep))
                {
                    return RedirectToAction("KonsepList", "NaskahDinas");
                }
                data.Tujuan = new List<string>();
                data.Tembusan = new List<string>();
                ViewBag.Judul = "Baru";
                if (!string.IsNullOrEmpty(konsep))
                {
                    data.TipeSurat = konsep;
                }
                ViewBag.Tampil = false;
            }
            else
            {
                data = nd.GetDraftSurat(id, usr.UnitKerjaId);
                if (data.TipeSurat != konsep)
                {
                    return RedirectToAction("KonsepList", "NaskahDinas");
                }
                ViewBag.Tampil = t;
                if (!salin)
                {
                    if (data == null || data.Status == "D" || data.Status == "F")
                    {
                        return RedirectToAction("KonsepList", "NaskahDinas");
                    } else 
                    {
                        var participant = nd.GetKonsepParticipant(id);
                        if (!participant.Contains(usr.UserId))
                        {
                            return RedirectToAction("ProsesList", "NaskahDinas");
                        } 
                    }
                }

                konsep = data.TipeSurat;
                data.Perihal = Server.UrlDecode(data.Perihal);
                data.IsiSurat = Server.UrlDecode(data.IsiSurat);

                data.listTujuan = "";

                int idxtj = 0;
                foreach (var j in data.Tujuan)
                {
                    data.listTujuan += j;
                    if ((idxtj+1) != data.Tujuan.Count())
                    {
                        data.listTujuan += "|";
                    }
                    idxtj++;
                }
                idxtj = 0;
                foreach (var j in data.Tembusan)
                {
                    data.listTembusan += j;
                    if ((idxtj + 1) != data.Tembusan.Count())
                    {
                        data.listTembusan += "|";
                    }
                    idxtj++;
                }
                ViewBag.Judul = $"{data.TipeSurat} [{data.DraftCode}]";
                data.Status = string.IsNullOrEmpty(data.Status) ? "P" : data.Status;
                if (salin)
                {
                    ViewBag.Salin = true;
                    data.DraftCode = "";
                    data.LampiranId = "";
                    data.Status = "";
                    ViewBag.Judul = "Salin Konsep";
                }
            }
            data.isTU = OtorisasiUser.isTU();
            data.ListKodeKopSurat = new List<SelectListItem>();
            var listProfile = new List<Pegawai>();
            var listAtasNama = new List<Profile>();
            data.ListProfile = new List<SelectListItem>();

            //kopsurat
            List<KopSurat> kop = nd.GetListKopSurat(usr.UnitKerjaId);
            if (kop.Count < 1)
            {
                kop = nd.GetListKopSurat(nd.GetIndukUnitKerjaId(usr.UnitKerjaId));
            }

            try
            {
                data.ListKodeKopSurat.Add(new SelectListItem { Text = kop[0].UnitKerjaName, Value = kop[0].UnitKerjaId });
            }
            catch
            {
                data.ListKodeKopSurat.Add(new SelectListItem { Text = "", Value = "" });
            }

            var lstForTTE = new List<SelectListItem>();
            var listUnit = new SuratModel().GetProfilesByUnitKerja(usr.UnitKerjaId);
            if (tipekantorid == 1)
            {
                
                listAtasNama = nd.GetIndukByUnitKerja(usr.UnitKerjaId, true);
                listProfile = nd.GetPenandatanganNaskahDinas(usr.UnitKerjaId, usr.ProfileIdTU, "", "", "", "", 0, 0, usr);
                listAtasNama.AddRange(listUnit);
                data.ListKodeKopSurat.Add(new SelectListItem { Text = "Kementerian Agraria dan Tata Ruang/Badan Pertanahan Nasional", Value = "02" });
                var getKopAdhoc = nd.getListAdhoc(usr.UnitKerjaId);
                foreach (var kopadhoc in getKopAdhoc)
                {
                    data.ListKodeKopSurat.Add(new SelectListItem { Text = kopadhoc.UnitKerjaName, Value = kopadhoc.UnitKerjaId });
                }

                lstForTTE.Add(new SelectListItem { Text = "Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional", Value = "H0000001" });
                lstForTTE.Add(new SelectListItem { Text = "Wakil Menteri Agraria dan Tata Ruang/Wakil Kepala Badan Pertanahan Nasional", Value = "H0000002" });
                var indukthisunit = nd.GetIndukUnitKerjaId(usr.UnitKerjaId);
                if (!string.IsNullOrEmpty(indukthisunit) && usr.UnitKerjaId != indukthisunit)
                {                    
                    lstForTTE.Add(new SelectListItem { Text = dataMasterModel.GetNamaUnitKerjaById(indukthisunit), Value = indukthisunit });
                }
            } else
            {
                listProfile = nd.GetPenandatanganNaskahDinas(usr.UnitKerjaId, usr.ProfileIdTU, "", "", "", "", 0, 0, usr);
                listAtasNama = nd.GetIndukByUnitKerja(usr.UnitKerjaId, true);
                listAtasNama.AddRange(listUnit);
            }
            lstForTTE.Add(new SelectListItem { Text = dataMasterModel.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId, Selected = true });
            data.ListForTTE = lstForTTE;
            ViewBag.ListTujuanProfile = new List<Profile>();
            ViewBag.ListTujuanProfile = listProfile;

            ViewBag.ListAtasNama = listAtasNama;
            var unitkerjalist = nd.NDGetUnitKerja();
            ViewBag.ListUnitkerja = unitkerjalist;

            ViewBag.ListUnitKerjaForSelect = new List<UnitKerja>();
            string[] listEx = { "Surat Tugas", "Surat Perintah", "Surat Pernyataan" };
            if (listEx.Contains(konsep))
            {
                //ViewBag.ListUnitKerjaForSelect = nd.GetUnitKerjaForSelect(usr.UnitKerjaId);
                ViewBag.ListUnitKerjaForSelect = unitkerjalist;
            }

            string[] Korespondensi = { "Nota Dinas", "Surat Dinas", "Surat Undangan", "Surat Edaran", "Surat Pengantar", "Pengumuman", "Surat Keterangan" };
            string[] Peraturan = { "Surat Keputusan", "Surat Tugas", "Surat Perintah", "Surat Pernyataan" };
            string OpenView = "";

            if (Korespondensi.Contains(konsep))
            {
                OpenView = "EditorKorespondensi";
            }
            else if (Peraturan.Contains(konsep)) 
            {
                OpenView = "EditorPeraturan";
            } else
            {
                return RedirectToAction("KonsepList", "NaskahDinas");
            }
            return View(OpenView, data);
        }

        public JsonResult GetPetugasSatker()
        {
            var usr = functions.claimUser();
            string unitkerjaid = usr.UnitKerjaId;
            List<Models.Entities.Petugas> result = nd.GetPetugasSatker(unitkerjaid);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJabatanPetugas(string nama)
        {
            string result = dataMasterModel.GetPegawaiIdFromNamaAtauNip(nama, "");

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditorKorespondensi(string id, string konsep)
        {
            if (string.IsNullOrEmpty(konsep))
            {
                return RedirectToAction("KonsepList", "NaskahDinas");
            }
            var usr = functions.claimUser();
            var data = new DraftSurat();
            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
            if (string.IsNullOrEmpty(id))
            {
                    data.Tujuan = new List<string>();
                    data.Tembusan = new List<string>();
                    ViewBag.Judul = "Baru";
                    if (!string.IsNullOrEmpty(konsep))
                    {
                        data.TipeSurat = konsep;
                    }
            }
            else
            {
                data = nd.GetDraftSurat(id, usr.UnitKerjaId);
                data.Perihal = Server.UrlDecode(data.Perihal);
                data.IsiSurat = Server.UrlDecode(data.IsiSurat);
                ViewBag.Judul = data.DraftCode;
                data.Status = string.IsNullOrEmpty(data.Status) ? "P" : data.Status;
            }
            data.isTU = OtorisasiUser.isTU();
            data.ListKodeKopSurat = new List<SelectListItem>();
            var listUnit = new SuratModel().GetProfilesByUnitKerja(usr.UnitKerjaId);
            var listProfile = nd.GetIndukByUnitKerja(usr.UnitKerjaId, true);
            listProfile.AddRange(listUnit);
            data.ListProfile = new List<SelectListItem>();
            foreach (var p in listProfile)
            {
                data.ListProfile.Add(new SelectListItem { Text = p.NamaProfile, Value = p.ProfileId });
            }

            var listKop = nd.GetListKop("");
            foreach(var l in listKop)
            {
                data.ListKodeKopSurat.Add(new SelectListItem { Text = l.UnitKerjaName, Value = l.UnitKerjaId });
            }

            ViewBag.ListUnitkerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            return View(data);
        }

        public ActionResult EditorPeraturan(string id, string konsep)
        {
            if (string.IsNullOrEmpty(konsep))
            {
                return RedirectToAction("KonsepList", "NaskahDinas");
            }
            var usr = functions.claimUser();
            var data = new DraftSurat();
            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
            if (string.IsNullOrEmpty(id))
            {
                data.Tujuan = new List<string>();
                data.Tembusan = new List<string>();
                ViewBag.Judul = "Baru";
                if (!string.IsNullOrEmpty(konsep))
                {
                    data.TipeSurat = konsep;
                }
            }
            else
            {
                data = nd.GetDraftSurat(id, usr.UnitKerjaId);
                data.Perihal = Server.UrlDecode(data.Perihal);
                data.IsiSurat = Server.UrlDecode(data.IsiSurat);
                ViewBag.Judul = data.DraftCode;
                data.Status = string.IsNullOrEmpty(data.Status) ? "P" : data.Status;
            }
            data.isTU = OtorisasiUser.isTU();
            data.ListKodeKopSurat = new List<SelectListItem>();
            var listUnit = new SuratModel().GetProfilesByUnitKerja(usr.UnitKerjaId);
            var listProfile = nd.GetIndukByUnitKerja(usr.UnitKerjaId, true);
            listProfile.AddRange(listUnit);
            data.ListProfile = new List<SelectListItem>();
            foreach (var p in listProfile)
            {
                data.ListProfile.Add(new SelectListItem { Text = p.NamaProfile, Value = p.ProfileId });
            }

            var listKop = nd.GetListKop("");
            foreach (var l in listKop)
            {
                data.ListKodeKopSurat.Add(new SelectListItem { Text = l.UnitKerjaName, Value = l.UnitKerjaId });
            }

            ViewBag.ListUnitkerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            return View(data);
        }

        public ActionResult SettingNaskahDinas()
        {
            bool allow = OtorisasiUser.IsActiveRole("'H2081100'");
            if (!allow)
            {
                return RedirectToAction("Index", "Home");
            } 
            else
            {
                return View();
            }
        }

        public ActionResult SettingFormatPenomoran()
        {
            if (Request.IsAjaxRequest())
            {
                List<TipeSurat> data = nd.GetFormatPenomoran();
                return PartialView(data);
            }
            else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<h2>Opss..</h2>",
                    ContentEncoding = System.Text.Encoding.UTF8
                };
            }
        }

        [HttpPost]
        public JsonResult FormatNomor()
        {
            string action = Request.Form["action"].ToString();
            string namaTipeSurat = Request.Form["namaTipeSurat"].ToString();
            string formatNomor = Request.Form["formatNomor"].ToString();
            string kodeJenis = string.Empty;
            if (action.Equals("Insert"))
            {
                kodeJenis = Request.Form["kodeJenis"].ToString();
            }
            if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(namaTipeSurat) && !string.IsNullOrEmpty(formatNomor))
            {
                if(formatNomor.Contains("%3CNomor%3E") && formatNomor.Contains("%3CBulan%3E") && formatNomor.Contains("%3CTahun%3E"))
                {
                    bool simpan = nd.SimpanFormatNomor(action, namaTipeSurat, formatNomor, kodeJenis);
                    if (simpan)
                    {
                        return Json(new { status = simpan, pesan = "Data Berhasil Disimpan" }, JsonRequestBehavior.AllowGet);
                    } else
                    {
                        return Json(new { status = simpan, pesan = "Opss... Data Gagal Disimpan" }, JsonRequestBehavior.AllowGet);
                    }
                    
                } else
                {
                    return Json(new { status = false, pesan = "Data Tidak Lengkap" }, JsonRequestBehavior.AllowGet);
                }
            } else
            {
                return Json(new { status = false, pesan = "Perintah Tidak Dikenali" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SetAktifEditor(string param, string tipe)
        {
            bool Status = false;
            string Pesan = "Terjadi Kesalahan Pada Data";
            if (Request.IsAjaxRequest())
            {
                var tr = nd.setEditorAktif(param, tipe);
                Status = tr.Status;
                Pesan = tr.Pesan;
            }

            return Json(new { Status = Status, Pesan = Pesan }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SettingKopSurat()
        {
            if (Request.IsAjaxRequest())
            {
                var list = nd.GetListKop("");
                ViewBag.Pusat = nd.GetUnitkerjaKop(tipekantor: 1, tampil: true, eselon: 1, namelike: "Jenderal");
                ViewBag.Kanwil = nd.GetUnitkerjaKop(tipekantor: 2, tampil: true, namelike: "Wilayah");
                ViewBag.Kantah = nd.GetUnitkerjaKop(tipekantor: 3, tampil: true, namelike: "Pertanahan");
                return PartialView(list);
            } else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<h2>Opss..</h2>",
                    ContentEncoding = System.Text.Encoding.UTF8
                };
            }
            
        }

        public ActionResult SettingKopAdhoc()
        {
            if (Request.IsAjaxRequest())
            {
                ViewBag.ListUnitkerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                return PartialView();
            }
            else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<h2>Opss... Menu Ini Tidak Tersedia. <a href='/'>Klik Untuk Kembali</a></h2>",
                    ContentEncoding = System.Text.Encoding.UTF8
                };
            }
        }

        [HttpPost]
        public ActionResult getListAdhoc()
        {
            var result = nd.getListAdhoc();
            return Json(new { 
                Status = result.Count > 0, 
                Data = result, 
                Count = result.Count, 
                Pesan = result.Count > 0 ? "Data Berhasil didapatkan" : "Terdapat Masalah dalam Mendapatkan Data" 
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult hapusKop(string ukid)
        {
            var result = nd.deleteKopSurat(ukid);
            return Json(new
            {
                Status = result.Status,
                Pesan = result.Pesan
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult simpanKopAdhoc(KopSurat kopSurat)
        {
            bool valid = !string.IsNullOrEmpty(kopSurat.UnitKerjaId ?? kopSurat.NamaKantor_L1 ?? kopSurat.Alamat);
            TransactionResult result = new TransactionResult();
            if (OtorisasiUser.isTU() && valid)
            {
                result = nd.simpanKopAdhoc(kopSurat);
            }
            return Json(new { Status = result.Status, Pesan = result.Pesan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult KopEditor(string id,string kantorin)
        {
            id = (string.IsNullOrEmpty(id)) ? null : id;
            KopSurat data = new KopSurat();
            if (!string.IsNullOrEmpty(id))
            {

                if (!string.IsNullOrEmpty(kantorin))
                {
                    data = nd.GetKopDetailbyUnitKerja(kantorin);
                    data.UnitKerjaId = id;
                    int tipekantorid = dataMasterModel.GetTipeKantor(kantorin);
                    if (tipekantorid == 2)
                    {
                        data.NamaKantor_L2 = data.UnitKerjaName.Substring(14);
                    } 
                    else if (tipekantorid == 3)
                    {
                        string induk = dataMasterModel.GetKantorIdIndukFromKantorId(kantorin);
                        Kantor kantorInduk = dataMasterModel.GetKantor(induk);
                        data.NamaKantor_L2 = kantorInduk.NamaKantor.Substring(14);
                    }
                    
                    return PartialView("KopEditor", data);
                }


                data = nd.getKopDetail(id);
                return PartialView("KopEditor", data);
            }
            else 
            {
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                string kantorid = (functions.claimUser()).KantorId;
                var kantor = dataMasterModel.GetKantor(kantorid);
                string alamatkantor = myTI.ToTitleCase(kantor.Alamat.ToLower());
                string teleponkantor = kantor.Telepon;
                data.UnitKerjaName = "";
                data.NamaKantor_L1 = "";
                data.NamaKantor_L2 = "";
                data.Alamat = alamatkantor;
                data.Telepon = teleponkantor;
                data.Email = "surat@atrbpn.go.id";
                data.FontSize = 11;
                ViewBag.ListUnitkerja = nd.GetUnitKerjaE1();
                return PartialView("KopEditor", data);
            }
        }

        public ActionResult ViewPdf_KopSurat(string ukid)
        {
            string pathx = Server.MapPath("~/Reports");

            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument pdfDocument = new PdfDocument(pw);
            Rectangle pageSize = new Rectangle(width: 595, height: 300);
            Document doc = new Document(pdfDocument, new PageSize(pageSize));
            doc.SetMargins(40, 20, 80, 20);
            DataMasterModel dm = new DataMasterModel();
            string thisUK = ukid;
            if(ukid.Substring(0,1) == "A")
            {
                thisUK = ukid.Substring(3, ukid.Length - 3);
            }
            string kantorid = dm.GetKantorIdByUnitKerjaId(thisUK);
            #region call data
            var usr = functions.claimUser();
            string unitkerjaid = thisUK;
            var kopsurat = nd.getKopDetail(ukid);

            #endregion


            KopSuratAlamat kop = new KopSuratAlamat();
            kop.generate(doc, pathx, kopsurat, kantorid, unitkerjaid);

            doc.Close();
            pdfDocument.Close();
            pw.Close();

            byte[] byteArray = ms.ToArray();
            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, "application/pdf");

            return docfile;
        }

        public ActionResult GetPegawaiForTTE(int? draw, int? start, int? length)
        {
            var result = new List<Pegawai>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string Rawmetadata = Request.Form["metadata"].ToString();
            string slctdUnitkerjaid = Request.Form["unitkerjaid"].ToString();
            var profileidtu = usr.ProfileIdTU;
            var unitkerja = usr.UnitKerjaId;
            if (slctdUnitkerjaid != unitkerja)
            {
                bool ismenteri = false;
                if (slctdUnitkerjaid == "H0000001" || slctdUnitkerjaid == "H0000002")
                {
                    ismenteri = true;
                }
                profileidtu = mdl.GetProfileidTuFromUnitKerja(slctdUnitkerjaid, ismenteri);
                unitkerja = string.Empty;
            }
            string metadata = string.Empty;
            foreach (var m in Rawmetadata.Split('|'))
            {
                if (!string.IsNullOrEmpty(m))
                {
                    string[] x = m.Split(',');
                    metadata += string.IsNullOrEmpty(metadata) ? Server.UrlEncode(x[0]) : $"|{new Functions().TextEncode(x[0])}";
                }
            }

            string namajabatan = Request.Form["namajabatan"].ToString();
            namajabatan = string.IsNullOrEmpty(namajabatan) ? string.Empty : new Functions().TextEncode(namajabatan);
            string namapegawai = Request.Form["namapegawai"].ToString();
            namapegawai = string.IsNullOrEmpty(namapegawai) ? string.Empty : new Functions().TextEncode(namapegawai);
            string tipe = Request.Form["tipe"].ToString();

            if (!string.IsNullOrEmpty(slctdUnitkerjaid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = nd.GetPegawaiForTTE(unitkerja, profileidtu, namajabatan, namapegawai, metadata, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetKonsepDibuat(int? draw, int? start, int? length, string status)
        {
            var result = new List<ListDraft>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;
            string unitkerjaid = usr.UnitKerjaId;
            string searchKey = Request.Form["searchKey"].ToString();
            searchKey = Server.UrlEncode(searchKey);
            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = nd.GetKonsepDibuat(userid,unitkerjaid, status, searchKey, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;


                    foreach (var dt in result)
                    {
                        dt.Perihal = Server.UrlDecode(dt.Perihal);
                        if (dt.Status == "F")
                        {
                            dt.DokumenElektronikId = nd.GetDokumenElektronikIdFromDraftCode(dt.DraftCode);
                            if (!string.IsNullOrEmpty(dt.DokumenElektronikId))
                            {
                                dt.Status = "S";
                            }
                        }
                    }

                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListProsesKoordinasi(string dc)
        {
            List<UserTTE> result = nd.GetListProsesKoordinasi(dc);
            return Json(new {Status = result.Count > 0, data = result, recordsTotal = result.Count});
        }

        public ActionResult GetIdDokumenTTE(string dc)
        {
            string result = string.Empty;
            if (dc.Length > 6)
            {
                result = dc;
            }
            else
            {
                result = nd.GetIdDokumenTTE(dc);
            }
            return Json(new { Status = !string.IsNullOrEmpty(result), data = result });
        }


        public ActionResult KoordinasiDraft(string id)
        {
            var data = new ListDraft();
            var usr = functions.claimUser();
            var splitstring = Server.UrlDecode(id).Split('|');
            var status = "W";
            ViewBag.isReOpen = false;
            ViewBag.userId = usr.UserId;
            if(splitstring.Length > 1)
            {
                id = splitstring[1];
                status = (splitstring[0] == "S") ? "F" : splitstring[0];
                ViewBag.isReOpen = true;
            } 

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ProsesList", "NaskahDinas");
            }
            else
            {
                var participant = nd.GetKonsepParticipant(id);
                if (!participant.Contains(usr.UserId))
                {
                    return RedirectToAction("ProsesList", "NaskahDinas");
                }


                try
                {
                    data = nd.GetKonsepList(usr.UserId, status, id)[0];
                }
                catch
                {
                    return RedirectToAction("ProsesList", "NaskahDinas");
                }
                ViewBag.Judul = data.DraftCode;
                if (usr.UserId == data.UserBuat) {
                    ViewBag.OpenEditor = true;
                } else
                {
                    ViewBag.OpenEditor = false;
                }
            }

            var koordinasi = nd.GetKoordinasiDraft(id, "");
            try { 
                ViewBag.max = koordinasi[0].Max;
                ViewBag.Btn = false;
                foreach (var itm in koordinasi)
                {
                    if (itm.UserId == usr.UserId  )
                    {
                        ViewBag.Kor_Id = itm.Kor_Id;
                        if (itm.StatusKoordinasi == "W")
                        {
                            ViewBag.Btn = true;
                        }
                    }
                }
            } 
            catch { 
                ViewBag.max = "0";
                ViewBag.Btn = false;
            };
            
            ViewBag.TTD = nd.GetListProsesKoordinasi(id);

            return View(data);
        }

        [HttpPost]
        public JsonResult GetKoordinasiHistory(string draftcode)
        {
            var result = new List<KoordinasiHist>();
            var usr = functions.claimUser();
            result = nd.GetHistroyKoordinasi(draftcode, usr.UserId);

            return Json(new { data = result, length = result.Count }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult KirimTextHistory()
        {
            var text = Request.Form["InputText"].ToString();
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = nd.InputTextHistroy(text, usr.UserId, draftcode);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RevisiNotification()
        {
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = nd.RevisiNotification(draftcode, usr.UserId) ;
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult infokanPerubahan()
        {
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = nd.infokanPerubahan(draftcode, usr.UserId);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }


        public JsonResult infokanPerubahanVerifikator()
        {
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = nd.infokanPerubahanVerifikator(draftcode, usr.UserId);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult NotifHandler()
        {
            var tipeNotifikasi = Request.Form["tipeNotifikasi"].ToString();
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = new TransactionResult();
            if (tipeNotifikasi == "ReSubmit")
            {
                string text = "!RESUBMIT!";
                tr = nd.InputTextHistroy(text, usr.UserId, draftcode);
            }
            else if (tipeNotifikasi == "ReSubmitVerifikator")
            {
                string text = "!RESUBMITVERIFIKATOR!";
                tr = nd.InputTextHistroy(text, usr.UserId, draftcode);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }


        public JsonResult HapusNotif(string draftcode)
        {
            var tr = nd.HapusNotif(draftcode);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SetujuiKonsep()
        {
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = nd.SetujuiKonsepV2(draftcode, usr);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BatalkanPersetujuan()
        {
            var draftcode = Request.Form["DraftCode"].ToString();
            var usr = functions.claimUser();
            var tr = nd.BatalkanPersetujuan(draftcode, usr);
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> getLampiranFile(string id)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string tipe = "DokumenTTE";
                string versi = "0";
                string filename = id;
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
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

        public JsonResult getPosisiTTEInfo(string draftcode)
        {
            var usr = functions.claimUser();
            var save = JoinNdLampiran(draftcode);
            if (save != null)
            {
                var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
                if(string.IsNullOrEmpty(data.PosisiTTE)||!int.TryParse(data.PosisiTTE, out int value))
                {
                    var naskahByte = getStreamNaskahDinas(data);
                    data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
                }
                return Json(new { status = true, data = data.PosisiTTE }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = false, data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<FileStreamResult> JoinNdLampiran(string draftcode)
        {
            var usr = functions.claimUser();
            var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
            FileStreamResult docfile = null;
            MemoryStream naskahByte = new MemoryStream();
            byte[] naskahByteArray = null;
            MemoryStream mss;
            byte[] byteArray;

            if (!data.isHaveLampiran)
            {
                try
                {
                    naskahByte = getStreamNaskahDinas(data);
                    naskahByteArray = naskahByte.ToArray();
                }
                catch (Exception e)
                {
                    int loopcount = 0;
                    while (loopcount < 6)
                    {
                        data.IsiSurat += "<br>";
                        try
                        {
                            naskahByte = getStreamNaskahDinas(data);
                            naskahByteArray = naskahByte.ToArray();
                            break;
                        }
                        catch
                        {
                            loopcount++;
                        }
                    }
                }

                mss = new MemoryStream();
                mss.Write(naskahByteArray, 0, naskahByteArray.Length);
                mss.Position = 0;

                docfile = new FileStreamResult(mss, MediaTypeNames.Application.Pdf);
                docfile.FileDownloadName = $"{data.TipeSurat}[{data.DraftCode}].pdf";
                return docfile;
            }
            else if(data.isHaveLampiran)
            {
                int jumlahlampiran = data.lstLampiranId.Count();
                bool files = jumlahlampiran > 1 ? true : false;
                var prevMs = new MemoryStream();
                var pdfList = new List<byte[]> {};

                foreach (var lampiranid in data.lstLampiranId)
                {
                    var lampiranByte = new MemoryStream();
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    string kantorid = usr.KantorId;
                    string tipe = "DokumenTTE";
                    string versi = "0";
                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(lampiranid), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent(versi), "versionNumber");

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Retrieve"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            strm.CopyTo(lampiranByte);
                            pdfList.Add(lampiranByte.ToArray());
                        }
                        else
                        {
                            // 3 kali percobaan request
                            int loopcount = 0;
                            while (loopcount < 4)
                            {
                                reqresult = client.SendAsync(reqmessage).Result;
                                if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                                {
                                    var strm = await reqresult.Content.ReadAsStreamAsync();
                                    strm.CopyTo(lampiranByte);
                                    pdfList.Add(lampiranByte.ToArray());
                                    loopcount = 4;
                                }
                                else
                                {
                                    loopcount++;
                                }
                            }
                        }                        
                    }             
                }//endforeach

                string[] intText = { "", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "delapan", "sembilan", "sepuluh" };
                if (!files)
                {
                    int page = 0;
                    byte[] lampiranfile = pdfList[0];
                    var lampiranByte = new MemoryStream();
                    lampiranByte.Write(lampiranfile,0, lampiranfile.Length);
                    if (lampiranByte != null)
                    {

                        using (var stream = new MemoryStream(lampiranByte.ToArray()))
                        {
                            using (var reader = new PdfReader(stream))
                            {
                                using (var document = new PdfDocument(reader))
                                {
                                    page = document.GetNumberOfPages();
                                }
                            }
                        }
                    }

                    if (page > 0 && page <= 10)
                    {
                        data.JumlahLampiran = $"{page.ToString()} ({intText[page]}) Lembar";
                    }
                    else if (page > 10)
                    {
                        data.JumlahLampiran = "1 (satu) Berkas";
                    }
                }
                else
                {
                    if (jumlahlampiran <= 10)
                    {
                        data.JumlahLampiran = $"{jumlahlampiran.ToString()} ({intText[jumlahlampiran]}) Berkas";
                    }
                    else
                    {
                        data.JumlahLampiran = $"1 (satu) Bundel";
                    }
                }

                //panggil naskah dinas yang telah dihitung lampirannya
                try
                {
                    naskahByte = getStreamNaskahDinas(data);
                    naskahByteArray = naskahByte.ToArray();
                }
                catch (Exception e)
                {
                    int loopcount = 0;

                    while (loopcount < 6)
                    {
                        data.IsiSurat += "<br>";
                        try
                        {
                            naskahByte = getStreamNaskahDinas(data);
                            naskahByteArray = naskahByte.ToArray();
                            break;
                        }
                        catch
                        {
                            loopcount++;
                        }
                    }
                }

                var byteMerge = new List<byte[]> {naskahByteArray};
                byteMerge.AddRange(pdfList);
                using (var writerMemoryStream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(writerMemoryStream))
                    {
                        using (var mergedDocument = new PdfDocument(writer))
                        {
                            var merger = new PdfMerger(mergedDocument);

                            foreach (var pdfBytes in byteMerge)
                            {
                                using (var copyFromMemoryStream = new MemoryStream(pdfBytes))
                                {
                                    using (var reader = new PdfReader(copyFromMemoryStream))
                                    {
                                        using (var copyFromDocument = new PdfDocument(reader))
                                        {
                                            merger.Merge(copyFromDocument, 1, copyFromDocument.GetNumberOfPages());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    byteArray = writerMemoryStream.ToArray();

                    mss = new MemoryStream();

                    mss.Write(byteArray, 0, byteArray.Length);
                    mss.Position = 0;



                    docfile = new FileStreamResult(mss, MediaTypeNames.Application.Pdf);
                    docfile.FileDownloadName = $"{data.TipeSurat}[{data.DraftCode}].pdf";
                }
            }
            return docfile;
        }

        public async Task<FileStreamResult> JoinNdLampiranOld(string draftcode)
        {
            var usr = functions.claimUser();
            var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
            MemoryStream mss;
            byte[] byteArray;
            FileStreamResult docfile = null; 
            var lampiranid = nd.GetLampiranIdByDraftcode(draftcode);
            if (!string.IsNullOrEmpty(lampiranid))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string tipe = "DokumenTTE";
                string versi = "0";
                string filename = "test";
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(lampiranid), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Retrieve"));
                
                using (var client = new HttpClient())
                {
                    var reqresult = client.SendAsync(reqmessage).Result;
                    var lampiranByte = new MemoryStream();
                    if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                    {
                        var strm = await reqresult.Content.ReadAsStreamAsync();
                        strm.CopyTo(lampiranByte);
                    } else
                    {
                        // 3 kali percobaan request
                        int loopcount = 0;
                        while (loopcount < 4)
                        {
                            reqresult = client.SendAsync(reqmessage).Result;
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                            {
                                var strm = await reqresult.Content.ReadAsStreamAsync();
                                strm.CopyTo(lampiranByte);
                                loopcount = 4;
                            }
                            else
                            {
                                loopcount++;
                            }
                        }
                    }

                    //pagecount
                    int page = 0;

                    if(lampiranByte != null)
                    {

                        using (var stream = new MemoryStream(lampiranByte.ToArray()))
                        {
                            using (var reader = new PdfReader(stream))
                            {
                                using (var document = new PdfDocument(reader))
                                {
                                    page = document.GetNumberOfPages();
                                }
                            }
                        }
                    }

                    string[] intText = { "", "Satu", "Dua", "Tiga", "Empat", "Lima", "Enam", "Tujuh", "Delapan", "Sembilan", "Sepuluh" }; 

                    if (page > 0 && page <= 10)
                    {
                        data.JumlahLampiran = $"{page.ToString()} ({intText[page]}) Lembar";
                    }
                    else if (page > 10)
                    {
                        data.JumlahLampiran = "1 (Satu) Berkas";
                    }

                    MemoryStream naskahByte;
                    byte[] naskahByteArray = null;
                    try
                    {
                        naskahByte = getStreamNaskahDinas(data);
                        naskahByteArray = naskahByte.ToArray();
                    }
                    catch (Exception e)
                    {
                        int loopcount = 0;

                        while (loopcount < 6)
                        {
                            data.IsiSurat += "<br>";
                            try
                            {
                                naskahByte = getStreamNaskahDinas(data);
                                naskahByteArray = naskahByte.ToArray();
                                break;
                            }
                            catch
                            {
                                loopcount++;
                            }
                        }
                    }

                    var pdfList = new List<byte[]> { naskahByteArray, lampiranByte.ToArray() };
                    using (var writerMemoryStream = new MemoryStream())
                    {
                        using (var writer = new PdfWriter(writerMemoryStream))
                        {
                            using (var mergedDocument = new PdfDocument(writer))
                            {
                                var merger = new PdfMerger(mergedDocument);

                                foreach (var pdfBytes in pdfList)
                                {
                                    using (var copyFromMemoryStream = new MemoryStream(pdfBytes))
                                    {
                                        using (var reader = new PdfReader(copyFromMemoryStream))
                                        {
                                            using (var copyFromDocument = new PdfDocument(reader))
                                            {
                                                merger.Merge(copyFromDocument, 1, copyFromDocument.GetNumberOfPages());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        byteArray = writerMemoryStream.ToArray();

                        mss = new MemoryStream();

                        mss.Write(byteArray, 0, byteArray.Length);
                        mss.Position = 0;



                        docfile = new FileStreamResult(mss, MediaTypeNames.Application.Pdf);
                        docfile.FileDownloadName = string.IsNullOrEmpty(filename) ? string.Concat(tipe, ".pdf") : filename;
                        return docfile;
                    }
                }
                
            }
            else
            {
                try
                {
                    docfile = new FileStreamResult(getStreamNaskahDinas(data), "application/pdf");
                }
                catch (Exception c)
                {
                    int loopcount = 0;

                    while (loopcount < 6)
                    {
                        data.IsiSurat += "<br>";
                        try
                        {
                            docfile = new FileStreamResult(getStreamNaskahDinas(data), "application/pdf");
                            break;
                        }
                        catch (Exception x)
                        {
                            loopcount++;
                        }
                    }
                }
                return docfile;
            }
        }

        public async Task<MemoryStream> getLastNaskah(string draftcode, string NomorSurat = "", string TanggalSurat = "", bool ttb = false)
        {
            var usr = functions.claimUser();
            var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
            data.NomorSurat = NomorSurat;
            data.TanggalSurat = TanggalSurat;
            data.TandaTanganBasah = ttb;
            MemoryStream naskahByte;
            MemoryStream mss;
            byte[] byteArray;
            FileStreamResult docfile = null;
            var lampiranid = nd.GetLampiranIdByDraftcode(draftcode);
            if (!string.IsNullOrEmpty(lampiranid))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string tipe = "DokumenTTE";
                string versi = "0";
                string filename = "test";
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(lampiranid), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
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
                            var lampiranByte = new MemoryStream();
                            strm.CopyTo(lampiranByte);

                            //pagecount
                            int page = 0;
                            using (var stream = new MemoryStream(lampiranByte.ToArray()))
                            {
                                using (var reader = new PdfReader(stream))
                                {
                                    using (var document = new PdfDocument(reader))
                                    {
                                        page = document.GetNumberOfPages();
                                    }
                                }
                            }

                            if (page > 0 && page <= 3)
                            {
                                data.JumlahLampiran = $"{page.ToString()} Lembar";
                            }
                            else if (page > 3)
                            {
                                data.JumlahLampiran = "1 Berkas";
                            }

                            naskahByte = new MemoryStream();
                            try
                            {
                                naskahByte = getStreamNaskahDinas(data);
                            }
                            catch (Exception e)
                            {
                                int loopcount = 0;
                                while (loopcount < 8)
                                {
                                    data.IsiSurat += "<br>";
                                    try
                                    {
                                        naskahByte = getStreamNaskahDinas(data);
                                        break;
                                    }
                                    catch
                                    {
                                        loopcount++;
                                    }
                                }
                            }

                            var pdfList = new List<byte[]> { naskahByte.ToArray(), lampiranByte.ToArray() };
                            using (var writerMemoryStream = new MemoryStream())
                            {
                                using (var writer = new PdfWriter(writerMemoryStream))
                                {
                                    using (var mergedDocument = new PdfDocument(writer))
                                    {
                                        var merger = new PdfMerger(mergedDocument);

                                        foreach (var pdfBytes in pdfList)
                                        {
                                            using (var copyFromMemoryStream = new MemoryStream(pdfBytes))
                                            {
                                                using (var reader = new PdfReader(copyFromMemoryStream))
                                                {
                                                    using (var copyFromDocument = new PdfDocument(reader))
                                                    {
                                                        merger.Merge(copyFromDocument, 1, copyFromDocument.GetNumberOfPages());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                byteArray = writerMemoryStream.ToArray();

                                mss = new MemoryStream();

                                mss.Write(byteArray, 0, byteArray.Length);
                                mss.Position = 0;

                                return mss;
                            }

                        }
                    }
                }
                catch
                {
                    return getStreamNaskahDinas(data);
                }
            }
            else
            {
                try
                {
                    return getStreamNaskahDinas(data);
                } catch (Exception e)
                {
                    int loopcount = 0;
                    while (loopcount < 8)
                    {
                        data.IsiSurat += "<br>";
                        try
                        {
                            return getStreamNaskahDinas(data);
                        }
                        catch
                        {
                            loopcount++;
                        }
                    }
                }
            }
            return getStreamNaskahDinas(data);
        }

        [HttpPost]
        public JsonResult SaveLastDraft(string draftcode)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "Gagal" };
            var usr = functions.claimUser();
            var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);

            try
            {
                data.NomorSurat = Request.Form["NomorSurat"].ToString();
                data.TanggalSurat = Request.Form["TanggalSurat"].ToString();
                data.Perihal = Request.Form["Perihal"].ToString();
                data.AutoNumAvail = Request.Form["AutoNum"].ToString() == "Y";

                if (data.AutoNumAvail)
                {
                    UserTTE usertte = nd.GetUserTtd(draftcode).Find(x => x.Tipe == "1");
                    BukuPenomran buku = new PenomoranModel().getBukuFromPenandatangan(usertte.ProfileId);
                    if(buku == null) {
                        return Json(new { Status = false, Pesan = "Penandatangan tidak memiliki Buku Penomoran aktif", dokumenId = ""}, JsonRequestBehavior.AllowGet);
                    }
                    var dataNomor = new DataPenomoran()
                    {
                        isTTE = true,
                        TanggalSurat = data.TanggalSurat,
                        Status = "0",
                        JenisNaskahDinas = data.TipeSurat,
                        BukuNomorId = buku.BukuNomorId,
                        Perihal = data.Perihal,
                        Keterangan = "TTE",
                        KlasifikasiArsip = data.KodeArsip,
                        ProfilePenandatangan = usertte.ProfileId
                    };
                    var sent = new PenomoranModel().SimpanDataPenomoran(usr, dataNomor);
                    if (sent.Status)
                    {
                        data.NomorSurat = sent.ReturnValue;
                    }
                    else
                    {
                        return Json(new { Status = false, Pesan = sent.Pesan, dokumenId = ""}, JsonRequestBehavior.AllowGet);
                    }                    
                }
            } 
            catch
            {
                return Json(new { Status = false, Pesan = "Terdapat Masalah dalam Pada Input Data", dokumenId = "" }, JsonRequestBehavior.AllowGet);
            }
            Stream draft = getLastNaskah(draftcode,data.NomorSurat,data.TanggalSurat).Result;
            if(string.IsNullOrEmpty(data.PosisiTTE) || !int.TryParse(data.PosisiTTE, out int value))
            {
                var getnew = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
                data.PosisiTTE = getnew.PosisiTTE;
            }
            int posTTE = Convert.ToInt32(data.PosisiTTE);
            Stream strInput;
            var InputStr = new PDFBuilder().Build(
                       streamPdf: draft,
                       template: PDFBuilder.Template.FOOTER,
                       pageTte: posTTE);
            strInput = InputStr.Output;
            try
            {
                var dokumenTTEId = nd.NewGuID();
                int versi = 0;
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                var tipe = "DokumenTTE";
                content.Add(new StringContent(usr.KantorId), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(dokumenTTEId), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi.ToString()), "versionNumber");
                content.Add(new StreamContent(strInput), "file", $"Naskah_Dinas_[{draftcode}].pdf");
                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[TTEM.apiUrl(TTEM.GetServerDate())].ToString(), "Store"));

                using (var client = new HttpClient())
                {
                    var reqresult = client.SendAsync(reqmessage).Result;
                    if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                    {
                        tr.Pesan = "Berhasil Menyimpan Dokumen";
                        tr.Status = true;
                        return Json(new { Status = tr.Status, Pesan = tr.Pesan , dokumenId = dokumenTTEId, Autonum = data.AutoNumAvail, Nomor = data.NomorSurat }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Status = false, Pesan = string.Concat("Gagal Menyimpan Dokumen, \n", reqresult.ReasonPhrase) }, JsonRequestBehavior.AllowGet);
                    }
                }
            } catch
            {
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
        }

        public TransactionResult SavePdfStream(Stream pdf, int posisiTTE, string namafile)
        {
            var tr = new TransactionResult() { Status = false };
            var usr = functions.claimUser();
            try
            {
                int numberOfPages = 0;
                var m1 = new MemoryStream();
                pdf.CopyTo(m1);
                if (posisiTTE == 0)
                {
                    using (var stream = new MemoryStream(m1.ToArray()))
                    {
                        using (var reader = new PdfReader(stream))
                        {
                            using (var document = new PdfDocument(reader))
                            {
                                numberOfPages = document.GetNumberOfPages();
                            }
                        }
                    }
                } else
                {
                    numberOfPages = posisiTTE;
                }
                Stream strInput;
                var InputStr = new PDFBuilder().Build(
                           streamPdf: pdf,
                           template: PDFBuilder.Template.FOOTER,
                           pageTte: numberOfPages);
                strInput = InputStr.Output;

                var dokumenTTEId = nd.NewGuID();
                int versi = 0;
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                var tipe = "DokumenTTE";
                content.Add(new StringContent(usr.KantorId), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(dokumenTTEId), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi.ToString()), "versionNumber");
                content.Add(new StreamContent(strInput), "file", namafile);
                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings[TTEM.apiUrl(TTEM.GetServerDate())].ToString(), "Store"));

                using (var client = new HttpClient())
                {
                    var reqresult = client.SendAsync(reqmessage).Result;
                    if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                    {
                        tr.Status = true;
                        tr.ReturnValue = dokumenTTEId;
                        tr.ReturnValue2 = numberOfPages.ToString();
                    }
                    else
                    {
                        tr.Pesan = string.Concat("Gagal Menyimpan Dokumen, \n", reqresult.ReasonPhrase);
                    }
                }
            }
            catch (Exception e)
            {
                tr.Pesan = e.Message;
            }

            return tr;
        }

        [HttpPost]
        public JsonResult PengajuanTTEDraft(DokumenTTE data)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            var userIdentity = User.Identity as InternalUserIdentity;
            string kantorid = userIdentity.KantorId;
            string pNama = userIdentity.NamaPegawai;
            data.UserPembuat = string.IsNullOrEmpty(data.UserPembuat) ? userIdentity.UserId : data.UserPembuat;
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            bool status = false;
            string pesan = "";
            string tid = "DokumenTTE";

            string draftcode = data.IsiSurat;
            data.NamaFile = (data.TipeSurat == "Pengantar Surat Masuk") ? $"Pengantar_[{data.IsiSurat}].pdf" : $"Naskah_Dinas_[{draftcode}].pdf";
            data.Ekstensi = ".pdf";

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

            if (data.TipeSurat == "Pengantar Surat Masuk")
            {
                Stream pdf = SuratPengantarPersuratan(data.IsiSurat).FileStream;
                var r = SavePdfStream(pdf, 0, data.NamaFile);
                if (r.Status)
                {
                    data.DokumenElektronikId = r.ReturnValue;
                    draftcode = "";
                    if(userIdentity.UnitKerjaId == "02010208")
                    {
                        var pd = dataMasterModel.GetPegawaiByProfileId("H2081100");
                        if(pd.Count > 0)
                        {
                            data.UserPembuat = new TandaTanganElektronikModel().getUserId(pd[0].PegawaiId);
                            pNama = pd[0].NamaLengkap;
                            data.PosisiTTE = r.ReturnValue2;
                        } else
                        {
                            return Json(new { Status = false, Pesan = "Tidak Dapat Mengajukan Dokumen" }, JsonRequestBehavior.AllowGet);
                        }
                    }                    
                }
                else
                {
                    return Json(new { Status = false, Pesan = r.Pesan }, JsonRequestBehavior.AllowGet);
                }
            }

            if (OtorisasiUser.isTU() || data.TipeSurat == "Pengantar Surat Masuk" || (OtorisasiUser.PembuatDokumenElektronik() && OtorisasiUser.IsPembuatNomorSuratRole()))
            {
                data.Status = "A";
                string[] str = data.listTTE.Split('|');
                data.TTE = new List<UserTTE>();
                UserTTE usertte = new UserTTE();
                int urut = 1;
                if (!string.IsNullOrEmpty(data.Pass))
                {
                    usertte.PenandatanganId = userIdentity.UserId;
                    usertte.Tipe = "0";
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
                        if (tte.Count() == 2)
                        {
                            usertte = new UserTTE();
                            string ttepegawaiid = (data.TipeSurat == "Pengantar Surat Masuk") ? dataMasterModel.GetPegawaiIdFromProfileId(tte[0].ToString()) : tte[0].ToString();
                            usertte.PenandatanganId = new TandaTanganElektronikModel().getUserId(ttepegawaiid);
                            usertte.Tipe = tte[1].ToString();
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
                if (string.IsNullOrEmpty(data.DokumenElektronikId))
                {
                    return Json(new { Status = false, Pesan = "Id Pengajuan Tidak Ditemukan" }, JsonRequestBehavior.AllowGet);
                }


                data.NomorSurat = Server.UrlEncode(data.NomorSurat);
                data.Perihal = Server.UrlEncode(data.Perihal);


                tr = new TandaTanganElektronikModel().SimpanPengajuanDraft(data, pNama, kantorid, tid, draftcode);
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

        public FileStreamResult GetForTTB(string draftcode,string tanggalsurat, string nomorsurat)
        {
            var usr = functions.claimUser();
            bool savenomor = nd.setNomordanTanggal(draftcode, nomorsurat, tanggalsurat);
            FileStreamResult docfile;
            if (savenomor)
            {
                var data = nd.GetDraftSurat(draftcode, usr.UnitKerjaId);
                MemoryStream draft = getLastNaskah(draftcode, nomorsurat, tanggalsurat, true).Result;
                byte[] byteArray = draft.ToArray();

                MemoryStream mss = new MemoryStream();

                mss.Write(byteArray, 0, byteArray.Length);
                mss.Position = 0;


                docfile = new FileStreamResult(mss, MediaTypeNames.Application.Pdf);
                docfile.FileDownloadName = string.Concat(data.DraftCode, ".pdf");
                return docfile;
            }
            docfile = null;
            return docfile;
        }

        public JsonResult SetFinalDraft(string draftcode)
        {
            if (!string.IsNullOrEmpty(draftcode))
            {
                var simpan = nd.SetDraftStatus(draftcode, "F");
                if (simpan)
                {
                    return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult HapusKop()
        {
            string unitkerjaid = Request.Form["unitkerjaid"].ToString();
            var result = nd.deleteKopSurat(unitkerjaid);
            return Json(new
            {
                Status = result.Status,
                Pesan = result.Pesan
            }, JsonRequestBehavior.AllowGet);
        }

        public FileStreamResult SuratPengantarPersuratan(string psid)
        {
            var usr = functions.claimUser();
            string unitkerja = usr.UnitKerjaId;
            string pathx = Server.MapPath("~/Reports");
            var data = new PersuratanModel().GetNewPengantarSurat(psid);
            var ms = new MemoryStream();
            if (!string.IsNullOrEmpty(data.NomorSurat) && !string.IsNullOrEmpty(data.TanggalSurat) && !string.IsNullOrEmpty(data.LstSurat) && !string.IsNullOrEmpty(data.Penandatangan))
            {
                var suratSP = new SuratPengantarPersuratan();
                ms = suratSP.prepare(data.NomorSurat, data.TanggalSurat, data.LstSurat, data.Penandatangan, pathx, unitkerja, usr.KantorId, data.ProfileIdTujuan);
            }

            var docfile = new FileStreamResult(ms, "application/pdf");
            docfile.FileDownloadName = data.NomorSurat;
            return docfile;
        }

        public async Task<ActionResult> getFinalVersion(string draftcode)
        {
            string id = new TandaTanganElektronikModel().getDokidFromDraft(draftcode);
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string tipe = "DokumenTTE";
                string versi = "0";
                string filename = id;
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
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

        [HttpPost]
        public ActionResult DaftarKlasifikasiArsip()
        {
            var data = dataMasterModel.GetKodeKlasifikasiArsip();
            return PartialView("DaftarKodeKlasifikasiArsip", data);
        }


        [HttpPost]
        public JsonResult GetPegawaiDetailByUnitKerja(string UnitKerjaDetail)
        {
            return Json(nd.GetPegawaiDetailByUnitKerja(UnitKerjaDetail));
        }

        public ActionResult GetPegawaiByUnitKerjaJabatanNama()
        {
            List<Surat.Models.Entities.Pegawai> result = new List<Models.Entities.Pegawai>();
            decimal? total = 0;

            string unitkerjaid = Request.Form["unitkerjaid"].ToString();
            string namajabatan = Request.Form["namajabatan"].ToString();
            string namapegawai = Request.Form["namapegawai"].ToString();

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                result = dataMasterModel.GetPegawaiByUnitKerjaJabatanNama(unitkerjaid, namajabatan, namapegawai);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                    foreach (var r in result)
                    {
                        if (r.Jabatan == "Sekretaris Jenderal")
                        {
                            r.Jabatan = "Sekretaris Jenderal Kementerian Agraria dan Tata Ruang/Badan Pertanahan Nasional";
                        }
                        try
                        {
                            if (nd.ProfileIdIsTU(r.ProfileId) && r.ProfileId == nd.TuUnitKerja(unitkerjaid))
                            {
                                var unitNama = nd.NdUnitkerjaNama(r.ProfileId, r.PegawaiId);

                                if (r.Jabatan.Contains(unitNama.Split(' ')[0]))
                                {
                                    r.Jabatan = r.Jabatan + " " + unitNama.Replace(unitNama.Split(' ')[0], "");
                                }
                                else
                                {
                                    r.Jabatan = r.Jabatan + " " + unitNama;
                                }
                            }
                        } catch {

                        }
                    }
                } 

            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }


        #region EditorNaskah

        public ActionResult OpenEditorNaskah(string id, string konsep)
        {
            if (string.IsNullOrEmpty(konsep))
            {
                return RedirectToAction("KonsepList", "NaskahDinas");
            }

            var usr = functions.claimUser();
            var data = new DraftSurat();
            data.TipeSurat = konsep;

            //kopSurat
            List<KopSurat> kop = nd.GetListKopSurat(usr.UnitKerjaId);
            kop = (kop.Count < 1) ? nd.GetListKopSurat(nd.GetIndukUnitKerjaId(usr.UnitKerjaId)) : kop;
            ViewBag.KopSurat = (kop.Count > 0) ? kop[0] : new KopSurat { };


            return View(data);
        }

        #endregion




        public JsonResult TestPostgres()
        {
            using (var ctx = new PostgresDbContext())
            {
                return Json(ctx.Database.SqlQuery<string>("SELECT NAMAFILE FROM KEARSIPAN WHERE ARSIP_ID = 1").FirstOrDefault(), JsonRequestBehavior.AllowGet); 
            }
        }
    }
}
