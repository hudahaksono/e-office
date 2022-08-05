﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Surat.Codes;
using Newtonsoft.Json;
//using Microsoft.Exchange.WebServices.Data;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class FlowController : Controller
    {
        Functions functions = new Functions();
        Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.KontentModel kontentm = new Models.KontentModel();
        Models.KonterModel kontermodel = new Models.KonterModel();

        public ActionResult SuratMasuk()
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            //find.ListMyProfiles = dataMasterModel.GetJabatanPegawaiKantor(kantorid, pegawaiid);
            find.ListMyProfiles = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            find.JumlahMyProfiles = find.ListMyProfiles.Count;
            if (find.JumlahMyProfiles == 1)
            {
                find.SpesificProfileId = find.ListMyProfiles[0].ProfileId;
            }
            //find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        //public ActionResult SuratKeluar()
        //{
        //    Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
        //    find.ListSifatSurat = persuratanmodel.GetSifatSurat();
        //    find.ListTipeSurat = persuratanmodel.GetTipeSurat();
        //    return View(find);
        //}

        public ActionResult SuratInisiatif()
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            //find.ListMyProfiles = dataMasterModel.GetJabatanPegawaiKantor(kantorid, pegawaiid);
            find.ListMyProfiles = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            find.JumlahMyProfiles = find.ListMyProfiles.Count;
            if (find.JumlahMyProfiles == 1)
            {
                find.SpesificProfileId = find.ListMyProfiles[0].ProfileId;
            }
            return View(find);
        }

        public ActionResult SuratOutbox()
        {
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        public ActionResult SuratKembali()
        {
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        //[AccessDeniedAuthorize(Roles = "PembuatSurat")]
        public ActionResult SuratForEdit()
        {
            if (!OtorisasiUser.IsProfile("PembuatSuratMasuk"))
            {
                return View("Forbidden", "Error");
            }
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        public ActionResult ProsesSurat()
        {
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        public ActionResult LembarDisposisi()
        {
            // Terbaru
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            return View(find);
        }

        public ActionResult EntriDisposisiSurat(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                ViewBag.UnitKerjaId = unitkerjaid;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(id, satkerid);
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListProfileTujuan = new List<Models.Entities.Profile>();
                //surat.ListProfileTujuan = dataMasterModel.GetProfileDisposisi(profileidtu, false);
                surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
                //surat.ListProfiles = new List<Models.Entities.Profile>();
                surat.IsProfileFixLembarDispo = "0";
                surat.NomorSurat = string.IsNullOrEmpty(surat.NomorSurat) ? surat.NomorSurat : HttpUtility.UrlDecode(surat.NomorSurat);
                surat.NamaPengirim = string.IsNullOrEmpty(surat.NamaPengirim) ? surat.NamaPengirim : HttpUtility.UrlDecode(surat.NamaPengirim);
                surat.NamaPegawai = string.IsNullOrEmpty(surat.NamaPegawai) ? surat.NamaPegawai : HttpUtility.UrlDecode(surat.NamaPegawai);

                string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
                if (myProfiles.Contains("H0000001") || myProfiles.Contains("H0000002") || myProfiles.Contains("H0000003"))
                {
                    surat.IsProfileFixLembarDispo = "1";
                }

                return View("EntriDisposisiSurat", surat);
            }
            else
            {
                return RedirectToAction("LembarDisposisi", "Flow");
            }
        }

        //public ActionResult BuatSurat()
        //{
        //    //string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

        //    //Persuratan.Models.Entities.Surat surat = new Persuratan.Models.Entities.Surat();
        //    //surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
        //    //surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
        //    //surat.ListProfilePegawaiPenerima = dataMasterModel.GetProfilePegawaiTujuan(nip);
        //    ////surat.ListKodeKlasifikasi = persuratanmodel.GetKodeKlasifikasi();
        //    ////surat.ListSeksi = persuratanmodel.GetSeksi();

        //    //return View(surat);
        //    return View();
        //}

        //[AccessDeniedAuthorize(Roles = "PembuatSurat")]
        public ActionResult BuatSuratMasuk()
        {
            if (!OtorisasiUser.IsProfile("PembuatSuratMasuk"))
            {
                return View("Forbidden", "Error");
            }
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            if (kantorid == "980FECFC746D8C80E0400B0A9214067D")
            {
                ViewBag.UnitKerjaId = "";
            }
            else
            {
                ViewBag.UnitKerjaId = unitkerjaid;
            }

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            Surat.Models.Entities.Surat surat = new Surat.Models.Entities.Surat();
            surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
            surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
            //surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);

            string profileba = dataMasterModel.GetProfileIdBAFromProfileId(profileidtu);
            //surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true, false, kantorid, true);

            if (kantorid == "980FECFC746D8C80E0400B0A9214067D")
            {
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true, false, null, true);
            }
            else
            {
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true, false, kantorid, true);
            }
            surat.ListProfileTujuan = new List<Models.Entities.Profile>(); // dataMasterModel.GetProfileTujuan(myProfiles, kantorid);
            surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
            surat.ListProfiles = new List<Models.Entities.Profile>();

            if (OtorisasiUser.IsPembuatNomorAgendaRole())
            {
                surat.NomorAgenda = kontermodel.GetNomorKonterSurat(kantorid, unitkerjaid, profileidtu, "Agenda");
            }

            //surat.ListProfilePegawaiPenerima = dataMasterModel.GetProfilePegawaiTujuan(myProfiles);
            ////surat.ListKodeKlasifikasi = persuratanmodel.GetKodeKlasifikasi();
            ////surat.ListSeksi = persuratanmodel.GetSeksi();

            return View(surat);
        }

        public ActionResult BuatSuratKeluar(string referensi, string nomorsuratref, string refpengirimsurat)
        {
            string referensiAsalSurat = "";
            string referensiTujuanSurat = "";
            string referensiNomorAgenda = "";
            string referensiNomorSurat = "";
            string referensiTanggalSurat = "";
            string referensiPerihal = "";
            string referensiKategori = "";

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            if (!string.IsNullOrEmpty(referensi))
            {
                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    satkerid = unitkerjaid;
                }
                Surat.Models.Entities.ReferensiSurat refsurat = persuratanmodel.GetReferensiSurat(referensi, satkerid);
                if (refsurat != null)
                {
                    if (!string.IsNullOrEmpty(refsurat.SuratId))
                    {
                        string nosuratref = "";
                        if (!string.IsNullOrEmpty(nomorsuratref))
                        {
                            nosuratref = "No. " + nomorsuratref;
                        }
                        else
                        {
                            nosuratref = "tersebut";
                        }
                        string errmess = "Surat " + nosuratref + " sudah pernah dibalas ";
                        if (!string.IsNullOrEmpty(refsurat.NomorSurat))
                        {
                            errmess += "dengan Surat Keluar no. " + refsurat.NomorSurat;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(refsurat.NomorAgenda))
                            {
                                errmess += "oleh surat dengan agenda no. " + refsurat.NomorAgenda;
                            }
                        }
                        errmess += ", atau Inisiatif tersebut sudah ada output Surat Keluar.";
                        ViewBag.ErrMessage = errmess;

                        string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

                        Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
                        //find.ListMyProfiles = dataMasterModel.GetJabatanPegawaiKantor(kantorid, pegawaiid);
                        find.ListMyProfiles = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
                        find.JumlahMyProfiles = find.ListMyProfiles.Count;
                        if (find.JumlahMyProfiles == 1)
                        {
                            find.SpesificProfileId = find.ListMyProfiles[0].ProfileId;
                        }

                        return View("SuratInisiatif", find);
                    }
                }
                else
                {
                    string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

                    // ambil data surat sebagai referensi
                    Models.Entities.Surat referensisurat = persuratanmodel.GetSuratBySuratId(referensi, satkerid);
                    referensiAsalSurat = referensisurat.PengirimSurat;
                    referensiTujuanSurat = referensisurat.PenerimaSurat;
                    referensiNomorAgenda = referensisurat.NomorAgenda;
                    referensiNomorSurat = referensisurat.NomorSurat;
                    referensiTanggalSurat = referensisurat.TanggalSurat;
                    referensiPerihal = referensisurat.Perihal;
                    referensiKategori = referensisurat.Kategori;
                }
            }

            ViewBag.UnitKerjaId = unitkerjaid;

            Surat.Models.Entities.Surat surat = new Surat.Models.Entities.Surat();
            surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
            surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
            surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            surat.ListProfileTujuan = new List<Models.Entities.Profile>();
            surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
            surat.ListProfiles = new List<Models.Entities.Profile>();
            surat.Referensi = referensi;
            surat.PenerimaSurat = refpengirimsurat;

            // Referensi Surat (Surat Masuk / Inisiatif)
            surat.ReferensiAsalSurat = referensiAsalSurat;
            surat.ReferensiTujuanSurat = referensiTujuanSurat;
            surat.ReferensiNomorAgenda = referensiNomorAgenda;
            surat.ReferensiNomorSurat = referensiNomorSurat;
            surat.ReferensiTanggalSurat = referensiTanggalSurat;
            surat.ReferensiPerihal = referensiPerihal;
            surat.ReferensiKategori = referensiKategori;

            return View(surat);
        }

        //[AccessDeniedAuthorize(Roles = "PembuatSurat")]
        //public ActionResult BuatSuratKeluarInternal()
        //{
        //    string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

        //    ViewBag.UnitKerjaId = unitkerjaid;

        //    Surat.Models.Entities.Surat surat = new Surat.Models.Entities.Surat();
        //    surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
        //    surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
        //    surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
        //    surat.ListProfileTujuan = new List<Models.Entities.Profile>();
        //    surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
        //    surat.ListProfiles = new List<Models.Entities.Profile>();

        //    ////surat.ListKodeKlasifikasi = persuratanmodel.GetKodeKlasifikasi();
        //    ////surat.ListSeksi = persuratanmodel.GetSeksi();

        //    return View(surat);
        //}

        //[AccessDeniedAuthorize(Roles = "PembuatSurat")]
        //public ActionResult BuatSuratKeluarExternal()
        //{
        //    Surat.Models.Entities.Surat surat = new Surat.Models.Entities.Surat();
        //    surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
        //    surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
        //    surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
        //    surat.ListProfileTujuan = new List<Models.Entities.Profile>();
        //    surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
        //    surat.ListProfiles = new List<Models.Entities.Profile>();

        //    return View(surat);
        //}

        public ActionResult BuatSuratInisiatif()
        {
            var usr = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity);

            ViewBag.UnitKerjaId = usr.UnitKerjaId;

            Surat.Models.Entities.Surat surat = new Surat.Models.Entities.Surat();
            //surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
            //surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
            surat.ListUnitKerja = dataMasterModel.GetListUnitKerjaInisiatif(usr.PegawaiId);
            surat.ListProfileTujuan = new List<Models.Entities.Profile>();
            surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
            surat.ListProfiles = new List<Models.Entities.Profile>();

            return View(surat);
        }

        //public JsonResult GetProfileTujuanByProfileId(string profileid)
        //{
        //    List<Surat.Models.Entities.Profile> listProfile = dataMasterModel.GetProfileTujuanByProfileId(profileid);
        //    return Json(listProfile, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetPegawaiByProfileId(string profileid)
        {
            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(profileid);
            return Json(listPegawai, JsonRequestBehavior.AllowGet);
        }

        public ContentResult JumlahSurat()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            int jumlah = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "");

            result = String.Format("{0:#,##0}", jumlah);

            return Content(result);
        }

        public ContentResult JumlahSuratBelumDibuka()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            int jumlah = persuratanmodel.JumlahSuratBelumDibuka(satkerid, pegawaiid, myProfiles);
            result = String.Format("{0:#,##0}", jumlah);

            return Content(result);
        }

        public ContentResult JumlahSuratMasuk()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            try
            {
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                int jumlah = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Masuk");

                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ContentResult JumlahSuratInisiatif()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            try
            {
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                int jumlah = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Inisiatif");

                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ContentResult JumlahSuratKeluar()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            try
            {
            int jumlah = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Keluar");
            result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ContentResult JumlahProsesSurat()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            //int jumlah = persuratanmodel.JumlahProsesSurat(pegawaiid, satkerid);
            int jumlah = 0;

            try
            {
                if (dataMasterModel.GetIsMyProfileTU(pegawaiid).Equals("1"))
                {
                    jumlah = persuratanmodel.JumlahProsesSuratV2(dataMasterModel.getUserProfileTU(pegawaiid, unitkerjaid), satkerid);
                }
                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ContentResult GetMyEselonId()
        {
            string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            string result = dataMasterModel.GetMyEselonId(nip);

            return Content(result);
        }

        public ContentResult GetIsMyProfileTU()
        {
            string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            string result = dataMasterModel.GetIsMyProfileTU(nip);

            return Content(result);
        }

        public ContentResult GetNomorAgendaSurat()
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            string result = kontermodel.GetNomorKonterSurat(kantorid, unitkerjaid, profileidtu, "Agenda");

            return Content(result);
        }

        public ContentResult GetNomorAgendaSuratAndUpdate(string suratid)
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            string result = kontermodel.GetNomorAgendaSuratAndUpdate(kantorid, unitkerjaid, profileidtu, suratid);

            return Content(result);
        }

        public ContentResult GetStatusForwardTU(string suratinboxid)
        {
            string result = persuratanmodel.GetStatusForwardTU(suratinboxid);

            return Content(result);
        }

        public ContentResult GetCatatanSebelumnya(string suratinboxid)
        {
            string result = persuratanmodel.GetCatatanSebelumnya(suratinboxid);

            return Content(result);
        }

        public JsonResult GetSuratByNomorSurat(string nomorsurat)
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string suratid = persuratanmodel.GetSuratIdFromNomorSurat(nomorsurat);

            Models.Entities.Surat record = persuratanmodel.GetSuratBySuratId(suratid, satkerid);

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSuratByNomorAgenda(string nomoragenda)
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string suratid = persuratanmodel.GetSuratIdFromNomorAgenda(nomoragenda, satkerid);

            Models.Entities.Surat record = persuratanmodel.GetSuratBySuratId(suratid, satkerid);

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSuratBySuratId(string suratid)
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            Models.Entities.Surat record = persuratanmodel.GetSuratBySuratId(suratid, satkerid);

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CetakLembarDisposisi()
        {
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            Surat.Models.Entities.Surat surat = new Surat.Models.Entities.Surat();
            surat.ListProfileDisposisi = dataMasterModel.GetProfileDisposisiByProfileId(pegawaiid, kantorid, true);
            surat.ListProfileTujuan = dataMasterModel.GetProfileDisposisi(profileidtu, false); //new List<Models.Entities.Profile>();
            surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();

            return View(surat);
        }

        public ActionResult InfoSurat()
        {
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByUnitKerjaJabatanNama(unitkerjaid, "", "");
            find.ListPegawai = listPegawai;

            return View(find);
        }

        public ActionResult InfoSuratV2(string metadata = null, string statussurat = null, string kategorisurat = null, string nippenerima = null, string sumber = null, int pageNum = 0)
        {
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByUnitKerjaJabatanNama(unitkerjaid, "", "");
            find.ListPegawai = listPegawai;

            return View(find);
        }


        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        //public ActionResult DaftarSuratMasuk(int? pageNum, Models.Entities.FindSurat f)
        //{
        //    int pageNumber = pageNum ?? 0;
        //    int RecordsPerPage = 20;
        //    int from = (pageNumber * RecordsPerPage) + 1;
        //    int to = from + RecordsPerPage - 1;

        //    string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
        //    string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
        //    string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
        //    string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

        //    string satkerid = kantorid;
        //    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
        //    if (tipekantorid == 1)
        //    {
        //        //satkerid = profileidtu;
        //        satkerid = unitkerjaid;
        //    }

        //    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

        //    string metadata = f.Metadata;
        //    string nomorsurat = f.NomorSurat;
        //    string nomoragenda = f.NomorAgenda;
        //    string perihal = f.Perihal;
        //    string tanggalsurat = f.TanggalSurat;
        //    string tipesurat = f.TipeSurat;
        //    string sifatsurat = f.SifatSurat;
        //    string sortby = f.SortBy;
        //    string sorttype = f.SortType;
        //    string spesificprofileid = f.SpesificProfileId;

        //    // Status Surat "1" = Inbox
        //    List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratInbox(satkerid, pegawaiid, "1", "Masuk", myProfiles, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, spesificprofileid, from, to);

        //    int custIndex = from;
        //    Dictionary<int, Models.Entities.SuratInbox> dict = result.ToDictionary(x => custIndex++, x => x);

        //    if (result.Count > 0)
        //    {
        //        if (Request.IsAjaxRequest())
        //        {
        //            return PartialView("DaftarSuratMasuk", dict);
        //        }
        //        else
        //        {
        //            return RedirectToAction("SuratMasuk", "Flow");
        //        }
        //    }
        //    else
        //    {
        //        return new ContentResult
        //        {
        //            ContentType = "text/html",
        //            Content = "noresults",
        //            ContentEncoding = System.Text.Encoding.UTF8
        //        };
        //    }
        //}

        public ActionResult ListSuratMasuk(int? start, int? length, Models.Entities.FindSurat f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

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

            // Status Surat "1" = Inbox
            List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratInbox(satkerid, pegawaiid, "1", "Masuk", myProfiles, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, spesificprofileid, from, to, sumber);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                    dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                    dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSuratKeluar(int? start, int? length, Models.Entities.FindSurat f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            decimal? total = 0;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                satkerid = unitkerjaid;
            }

            string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
            string nomorsurat = f.NomorSurat;
            string nomoragenda = f.NomorAgenda;
            string perihal = f.Perihal;
            string tanggalsurat = f.TanggalSurat;
            string tipesurat = f.TipeSurat;
            string sifatsurat = f.SifatSurat;
            string sortby = f.SortBy;
            string sorttype = f.SortType;
            string sumber = f.Sumber_Keterangan;

            List<Models.Entities.SuratOutbox> result = persuratanmodel.GetSuratOutbox(satkerid, pegawaiid, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, from, to, sumber);

            if (result.Count > 0)
            {
                foreach (var dt in result)
                {
                    dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                    dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                    dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
                }
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult DaftarSuratKeluar(int? pageNum, Models.Entities.FindSurat f)
        //{
        //    int pageNumber = pageNum ?? 0;
        //    int RecordsPerPage = 20;
        //    int from = (pageNumber * RecordsPerPage) + 1;
        //    int to = from + RecordsPerPage - 1;

        //    string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
        //    string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
        //    string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
        //    string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

        //    string satkerid = kantorid;
        //    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
        //    if (tipekantorid == 1)
        //    {
        //        //satkerid = profileidtu;
        //        satkerid = unitkerjaid;
        //    }

        //    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

        //    string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
        //    string nomorsurat = f.NomorSurat;
        //    string nomoragenda = f.NomorAgenda;
        //    string perihal = f.Perihal;
        //    string tanggalsurat = f.TanggalSurat;
        //    string tipesurat = f.TipeSurat;
        //    string sifatsurat = f.SifatSurat;
        //    string sortby = f.SortBy;
        //    string sorttype = f.SortType;
        //    string spesificprofileid = f.SpesificProfileId;

        //    // Status Surat "1" = Inbox
        //    List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratInbox(satkerid, pegawaiid, "1", "Keluar", myProfiles, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, spesificprofileid, from, to);

        //    foreach (var dt in result)
        //    {
        //        dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
        //        dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
        //        dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
        //    }
        //    int custIndex = from;
        //    Dictionary<int, Models.Entities.SuratInbox> dict = result.ToDictionary(x => custIndex++, x => x);

        //    if (result.Count > 0)
        //    {
        //        if (Request.IsAjaxRequest())
        //        {
        //            return PartialView("DaftarSuratKeluar", dict);
        //        }
        //        else
        //        {
        //            return RedirectToAction("SuratKeluar", "Flow");
        //        }
        //    }
        //    else
        //    {
        //        return new ContentResult
        //        {
        //            ContentType = "text/html",
        //            Content = "noresults",
        //            ContentEncoding = System.Text.Encoding.UTF8
        //        };
        //    }
        //}

        //public ActionResult DaftarSuratInisiatif(int? pageNum, Models.Entities.FindSurat f)
        //{
        //    int pageNumber = pageNum ?? 0;
        //    int RecordsPerPage = 20;
        //    int from = (pageNumber * RecordsPerPage) + 1;
        //    int to = from + RecordsPerPage - 1;

        //    string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
        //    string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
        //    string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
        //    string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

        //    string satkerid = kantorid;
        //    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
        //    if (tipekantorid == 1)
        //    {
        //        //satkerid = profileidtu;
        //        satkerid = unitkerjaid;
        //    }

        //    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

        //    string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
        //    string nomorsurat = f.NomorSurat;
        //    string nomoragenda = f.NomorAgenda;
        //    string perihal = f.Perihal;
        //    string tanggalsurat = f.TanggalSurat;
        //    string tipesurat = f.TipeSurat;
        //    string sifatsurat = f.SifatSurat;
        //    string sortby = f.SortBy;
        //    string sorttype = f.SortType;
        //    string spesificprofileid = f.SpesificProfileId;

        //    // Status Surat "1" = Inbox
        //    List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratInbox(satkerid, pegawaiid, "1", "Inisiatif", myProfiles, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, spesificprofileid, from, to);

        //    int custIndex = from;
        //    Dictionary<int, Models.Entities.SuratInbox> dict = result.ToDictionary(x => custIndex++, x => x);

        //    if (result.Count > 0)
        //    {
        //        if (Request.IsAjaxRequest())
        //        {
        //            return PartialView("DaftarSuratInisiatif", dict);
        //        }
        //        else
        //        {
        //            return RedirectToAction("SuratInisiatif", "Flow");
        //        }
        //    }
        //    else
        //    {
        //        return new ContentResult
        //        {
        //            ContentType = "text/html",
        //            Content = "noresults",
        //            ContentEncoding = System.Text.Encoding.UTF8
        //        };
        //    }
        //}

        public ActionResult ListSuratInisiatif(int? start, int? length, Models.Entities.FindSurat f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

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

            // Status Surat "1" = Inbox
            List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratInbox(satkerid, pegawaiid, "1", "Inisiatif", myProfiles, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, spesificprofileid, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult DaftarSuratOutbox(int? pageNum, Models.Entities.FindSurat f)
        //{
        //    int pageNumber = pageNum ?? 0;
        //    int RecordsPerPage = 20;
        //    int from = (pageNumber * RecordsPerPage) + 1;
        //    int to = from + RecordsPerPage - 1;

        //    string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
        //    string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
        //    string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
        //    string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

        //    string satkerid = kantorid;
        //    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
        //    if (tipekantorid == 1)
        //    {
        //        //satkerid = profileidtu;
        //        satkerid = unitkerjaid;
        //    }

        //    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

        //    string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
        //    string nomorsurat = f.NomorSurat;
        //    string nomoragenda = f.NomorAgenda;
        //    string perihal = f.Perihal;
        //    string tanggalsurat = f.TanggalSurat;
        //    string tipesurat = f.TipeSurat;
        //    string sifatsurat = f.SifatSurat;
        //    string sortby = f.SortBy;
        //    string sorttype = f.SortType;
        //    string sumber = f.Sumber_Keterangan;

        //    List<Models.Entities.SuratOutbox> result = persuratanmodel.GetSuratOutbox(satkerid, pegawaiid, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, from, to, sumber);

        //    foreach (var dt in result)
        //    {
        //        dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
        //        dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
        //        dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
        //    }
        //    int custIndex = from;
        //    Dictionary<int, Models.Entities.SuratOutbox> dict = result.ToDictionary(x => custIndex++, x => x);

        //    if (result.Count > 0)
        //    {
        //        if (Request.IsAjaxRequest())
        //        {
        //            return PartialView("DaftarSuratOutbox", dict);
        //        }
        //        else
        //        {
        //            return RedirectToAction("SuratOutbox", "Flow");
        //        }
        //    }
        //    else
        //    {
        //        return new ContentResult
        //        {
        //            ContentType = "text/html",
        //            Content = "noresults",
        //            ContentEncoding = System.Text.Encoding.UTF8
        //        };
        //    }
        //}

        //UNTUK NON TU JUMLAH SURAT OUTBOX (TERKIRIM)
        public ContentResult JumlahSuratOutbox()
        {

            string result = "";
            string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            try
            {
                int jumlah = persuratanmodel.JumlahSuratOutbox(nip);

                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ActionResult DaftarSuratKembali(int? pageNum, Models.Entities.FindSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            string metadata = f.Metadata;

            List<Models.Entities.SuratKembali> result = persuratanmodel.GetSuratKembali(myProfiles, pegawaiid, metadata, from, to);

            int custIndex = from;
            Dictionary<int, Models.Entities.SuratKembali> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSuratKembali", dict);
                }
                else
                {
                    return RedirectToAction("SuratKembali", "Flow");
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

        public ActionResult DaftarSuratForEdit(int? pageNum, Models.Entities.FindSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

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
            string sumber = f.Sumber_Keterangan;

            List<Models.Entities.SuratOutbox> result = persuratanmodel.GetSuratOutbox(satkerid, pegawaiid, metadata, nomorsurat, nomoragenda, perihal, tanggalsurat, tipesurat, sifatsurat, sortby, sorttype, from, to, sumber);

            foreach (var dt in result)
            {
                dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
            }
            int custIndex = from;
            Dictionary<int, Models.Entities.SuratOutbox> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSuratForEdit", dict);
                }
                else
                {
                    return RedirectToAction("SuratForEdit", "Flow");
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

        public ActionResult DaftarProsesSurat(int? pageNum, Models.Entities.FindSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }
            string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
            string sortby = f.SortBy;
            string sorttype = f.SortType;
            string sumber = f.Sumber_Keterangan;
            /*
            if (!string.IsNullOrEmpty(pegawaiid))
            {
                if (dataMasterModel.CheckIsPLT(pegawaiid, profileidtu, kantorid))
                {
                    pegawaiid = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                }
            }*/
            string profileids = profileidtu;// functions.MyProfiles(pegawaiid,kantorid);

            // Status Surat "1" = Inbox
            var result = new List<Models.Entities.SuratInbox>();

            if (dataMasterModel.GetIsMyProfileTU(pegawaiid).Equals("1"))
            {
                profileids = dataMasterModel.GetProfileTUByNipUnitKerja(pegawaiid, unitkerjaid);
                result = persuratanmodel.GetProsesSuratV2(satkerid, profileids, metadata, sortby, sorttype, from, to, sumber);
                foreach (var dt in result)
                {
                    dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                    dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                    dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
                }
            }
            int custIndex = from;
            Dictionary<int, Models.Entities.SuratInbox> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarProsesSurat", dict);
                }
                else
                {
                    return RedirectToAction("ProsesSurat", "Flow");
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

        public ActionResult DaftarSemuaSurat(int? pageNum, Models.Entities.FindSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            string pegawaiidRestriction = pegawaiid;
            if (@OtorisasiUser.IsRoleAdministrator() == true)
            {
                myProfiles = "";
                pegawaiidRestriction = "";
            }

            string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
            string statussurat = f.StatusSurat;
            string kategorisurat = f.KategoriSurat;
            string nippenerima = f.NipPenerima;
            string sumber = f.Sumber_Keterangan;
            string type = f.TanggalInput;

            List<Models.Entities.Surat> result = persuratanmodel.GetListSurat(satkerid, myProfiles, metadata, statussurat, kategorisurat, nippenerima, sumber, from, to, pegawaiid: pegawaiidRestriction);

            foreach (var dt in result)
            {
                dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
            }
            int custIndex = from;
            Dictionary<int, Models.Entities.Surat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSemuaSurat", dict);
                }
                else
                {
                    return RedirectToAction("InfoSurat", "Flow");
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

        public ActionResult DaftarSuratDisposisi(int? pageNum, Models.Entities.FindSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            //string myProfiles = ""; // functions.MyProfiles(pegawaiid, kantorid);

            //string metadata = f.Metadata;

            //List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratDisposisi(satkerid, myProfiles, metadata, from, to);

            //int custIndex = from;
            //Dictionary<int, Models.Entities.SuratInbox> dict = result.ToDictionary(x => custIndex++, x => x);

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            //string metadata = f.Metadata;
            string metadata = string.IsNullOrEmpty(f.Metadata) ? string.Empty : new Functions().TextEncode(f.Metadata);
            string statussurat = f.StatusSurat;
            string kategorisurat = f.KategoriSurat;
            string nippenerima = f.NipPenerima;
            string sumber = f.Sumber_Keterangan;

            List<Models.Entities.Surat> result = persuratanmodel.GetListSurat(satkerid, myProfiles, metadata, statussurat, kategorisurat, nippenerima, sumber, from, to);

            foreach (var dt in result)
            {
                dt.NomorSurat = string.IsNullOrEmpty(dt.NomorSurat) ? dt.NomorSurat : HttpUtility.UrlDecode(dt.NomorSurat);
                dt.NamaPengirim = string.IsNullOrEmpty(dt.NamaPengirim) ? dt.NamaPengirim : HttpUtility.UrlDecode(dt.NamaPengirim);
                dt.NamaPegawai = string.IsNullOrEmpty(dt.NamaPegawai) ? dt.NamaPegawai : HttpUtility.UrlDecode(dt.NamaPegawai);
            }
            int custIndex = from;
            Dictionary<int, Models.Entities.Surat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSuratDisposisi", dict);
                }
                else
                {
                    return RedirectToAction("LembarDisposisi", "Flow");
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

        public ActionResult DaftarSuratSP(int? pageNum, Models.Entities.FindSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            //string tanggaldari = f.TanggalDari;
            //string tanggalsampai = f.TanggalSampai;
            string tanggalinput = f.TanggalInput;
            string penerimasurat = "";
            string tipesurat = f.TipeSurat;
            string sifatsurat = f.SifatSurat;
            string keterangansurat = f.KeteranganSurat;
            string redaksi = f.Redaksi;
            string metadata = f.Metadata;

            if (!string.IsNullOrEmpty(f.PenerimaSurat))
            {
                penerimasurat = dataMasterModel.GetProfileNameFromId(f.PenerimaSurat);
            }

            List<Models.Entities.Surat> result = persuratanmodel.GetSuratSP(myProfiles, tanggalinput, penerimasurat, tipesurat, sifatsurat, keterangansurat, redaksi, metadata, from, to);

            int custIndex = from;
            Dictionary<int, Models.Entities.Surat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSuratSP", dict);
                }
                else
                {
                    return RedirectToAction("SuratPengantarBaru", "Flow");
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

        public ActionResult DaftarLampiranSurat(string suratid)
        {
            
            List<Models.Entities.LampiranSurat> result = persuratanmodel.GetListLampiranSurat(suratid, "");

            int custIndex = 1;
            Dictionary<int, Models.Entities.LampiranSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

                    ViewBag.IsProfileTataUsaha = "0";

                    string ismyprofiletu = dataMasterModel.GetIsMyProfileTU(pegawaiid);
                    if (ismyprofiletu == "1")
                    {
                        ViewBag.IsProfileTataUsaha = "1";
                    }

                    return PartialView("DaftarLampiranSurat", dict);
                }
                else
                {
                    return RedirectToAction("SuratMasuk", "Flow");
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

        public ActionResult DaftarLampiranSuratKeluar(string suratid)
        {

            List<Models.Entities.LampiranSurat> result = persuratanmodel.GetListLampiranSurat(suratid, "");

            int custIndex = 1;
            Dictionary<int, Models.Entities.LampiranSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarLampiranSuratKeluar", dict);
                }
                else
                {
                    return RedirectToAction("SuratKeluar", "Flow");
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

        public ActionResult DaftarLampiranSuratView(string suratid)
        {

            List<Models.Entities.LampiranSurat> result = persuratanmodel.GetListLampiranSurat(suratid, "");

            int custIndex = 1;
            Dictionary<int, Models.Entities.LampiranSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

                    return PartialView("DaftarLampiranSuratView", dict);
                }
                else
                {
                    return RedirectToAction("SuratMasuk", "Flow");
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

        [HttpPost]
        public ActionResult HapusLampiranSuratById()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string suratid = Request.Form["suratid"].ToString();
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(suratid) && !String.IsNullOrEmpty(id))
                {
                    result = persuratanmodel.HapusLampiranSuratById(suratid, id);
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
        public JsonResult InsertLampiranSurat(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            data.UserId = pegawaiid;
            data.NIP = pegawaiid;
            data.NamaPengirim = namapengirim;

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            string judul = "Lampiran surat nomor: " + data.NomorSurat;

            var mfile = Request.Files["file"];
            if (mfile != null && 
                (mfile.ContentType == "application/pdf" ||
                 mfile.ContentType == "image/jpeg" ||
                 mfile.ContentType == "image/png" ||
                 mfile.ContentType == "application/vnd.ms-excel" ||
                 mfile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ||
                 mfile.ContentType == "application/msword" ||
                 mfile.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                )
            {
                string id = persuratanmodel.GetUID();

                int versi = 0;


                #region set fileExtension

                string fileExtension = "";
                if (mfile.ContentType == "application/pdf")
                {
                    fileExtension = ".pdf";
                }
                else if (mfile.ContentType == "image/jpeg")
                {
                    fileExtension = ".jpg";
                }
                else if (mfile.ContentType == "image/png")
                {
                    fileExtension = ".png";
                }
                else if (mfile.ContentType == "application/vnd.ms-excel")
                {
                    fileExtension = ".xls";
                }
                else if (mfile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    fileExtension = ".xlsx";
                }
                else if (mfile.ContentType == "application/msword")
                {
                    fileExtension = ".doc";
                }
                else if (mfile.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    fileExtension = ".docx";
                }

                #endregion


                data.NamaFile = mfile.FileName;
                data.LampiranSuratId = id;

                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                DateTime tglSunting = DateTime.Now;
                string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent("Surat"), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);
                content.Add(new StringContent(versi.ToString()), "versionNumber");
                if (!string.IsNullOrEmpty(fileExtension))
                {
                    content.Add(new StringContent(fileExtension), "fileExtension");
                }

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Store"));

                using (var client = new HttpClient())
                {
                    var reqresult = client.SendAsync(reqmessage).Result;
                    tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                    tr.Pesan = reqresult.ReasonPhrase;
                }

                tr = kontentm.SimpanKontenFile(kantorid, id, judul, namapengirim, data.TanggalSurat, "Surat", out versi);
            }

            tr = persuratanmodel.InsertLampiranSurat(data, kantorid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ContentResult GetJumlahLampiranSurat(string suratid)
        {
            string result = persuratanmodel.GetJumlahLampiranSurat(suratid);

            return Content(result);
        }

        //public ActionResult EntriSurat(string suratinboxid, string nomorsurat)
        //{
        //    if (!String.IsNullOrEmpty(suratinboxid))
        //    {
        //        string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

        //        //string myProfiles = functions.MyProfiles(nip);

        //        Surat.Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid);
        //        surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
        //        surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
        //        //surat.ListProfilePegawaiPenerima = dataMasterModel.GetProfilePegawaiTujuan(nip);
        //        ////surat.ListProfileTujuan = dataMasterModel.GetProfileTujuan(myProfiles);
        //        ////surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();

        //        if (Request.IsAjaxRequest())
        //        {
        //            // Update Flag Buka Surat
        //            string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
        //            Models.Entities.TransactionResult tr = persuratanmodel.BukaSuratInbox(surat.SuratId, surat.SuratInboxId, nip, namapegawai);

        //            return PartialView("EntriSurat", surat);
        //        }
        //        else
        //        {
        //            return RedirectToAction("SuratMasuk", "Flow");
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("SuratMasuk", "Flow");
        //    }
        //}

        public ActionResult ViewSurat(string suratid, string nomorsurat)
        {
            if (!String.IsNullOrEmpty(suratid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
                surat.ListUnitKerjaHistoriSurat = persuratanmodel.GetUnitKerjaSuratHistory(suratid);

                //List<Models.Entities.SuratInbox> surathistory = persuratanmodel.GetSuratHistory(suratid);

                //Models.Entities.InfoSurat info = new Models.Entities.InfoSurat();
                //info.DataSurat = surat;
                //info.ListSuratInbox = surathistory;

                // 23 Juni 2021 menambahkan batasan pada surat yang bersifat rahasia
                if (!OtorisasiUser.isTU() && surat.SifatSurat == "Rahasia")
                {
                    List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratHistory(suratid, "", unitkerjaid);
                    foreach (var lst in result)
                    {
                        if (lst.NIP == pegawaiid && lst.KetStatusTerkirim != "Pembuat Surat")
                        {
                            return View("ViewSurat", surat);
                        }
                    }
                    return RedirectToAction("InfoSurat", "Flow");
                } else
                {
                    return View("ViewSurat", surat);
                }
            }
            else
            {
                return RedirectToAction("InfoSurat", "Flow");
            }
        }


        public ActionResult ViewSuratByTU(string suratinboxid, string nomorsurat, string suratid)
        {
            if (!String.IsNullOrEmpty(suratinboxid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }


                Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid);
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();

                return View("ViewSuratByTU", surat);
            }
            else
            {
                return RedirectToAction("InfoSurat", "Flow");
            }
        }

        public ActionResult ViewSuratFromList(string suratid, string nomorsurat)
        {
            if (!String.IsNullOrEmpty(suratid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
                surat.ListUnitKerjaHistoriSurat = persuratanmodel.GetUnitKerjaSuratHistory(suratid);

                //List<Models.Entities.SuratInbox> surathistory = persuratanmodel.GetSuratHistory(suratid);

                //Models.Entities.InfoSurat info = new Models.Entities.InfoSurat();
                //info.DataSurat = surat;
                //info.ListSuratInbox = surathistory;

                return PartialView("ViewSurat", surat);
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

        public ActionResult EditSurat(string suratid, string nomorsurat)
        {
            if (!String.IsNullOrEmpty(suratid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();

                //List<Models.Entities.SuratInbox> surathistory = persuratanmodel.GetSuratHistory(suratid);

                //Models.Entities.InfoSurat info = new Models.Entities.InfoSurat();
                //info.DataSurat = surat;
                //info.ListSuratInbox = surathistory;

                return View("EditSurat", surat);
            }
            else
            {
                return RedirectToAction("SuratForEdit", "Flow");
            }
        }

        public ActionResult DaftarSuratHistory(string suratid, string unitkerjaid)
        {
            string satkerid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            List<Models.Entities.SuratInbox> result = persuratanmodel.GetSuratHistory(suratid, unitkerjaid, satkerid);

            foreach (var r in result)
            {
                r.UnitKerjaNama = dataMasterModel.GetUnitKerjaFromProfileId(r.ProfilePenerima);
            }

            int custIndex = 1;
            Dictionary<int, Models.Entities.SuratInbox> dict = result.ToDictionary(x => custIndex++, x => x);

            int jml = dict.Count();
            List<string> seen = new List<string>();
            int tier = 1;

            //cari pembuat
            foreach (var lst in dict)
            {
                if (lst.Value.KetStatusTerkirim == "Pembuat Surat")
                {
                    seen.Add(lst.Value.NamaPenerima);
                }
            }

            //tiering
            bool loop = true;
            int sorted = 0;
            while (loop && jml > 0)
            {
                foreach (var lst in dict)
                {
                    if (seen.Contains(lst.Value.NamaPengirim) && lst.Value.Tier == 0)
                    {
                        lst.Value.Tier = tier;
                        sorted += 1;
                    }
                }
                foreach (var lst in dict)
                {
                    if (lst.Value.Tier == tier && !seen.Contains(lst.Value.NamaPenerima))
                    {
                        seen.Add(lst.Value.NamaPenerima);
                    }
                }
                if (sorted >= jml || sorted == 0)
                {
                    loop = false;
                }
                tier += 1;
            }

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    //ViewBag.MaxTier = tier - 1;
                    return PartialView("DaftarSuratHistory", dict);
                }
                else
                {
                    return RedirectToAction("ViewSurat", "Flow");
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

        public JsonResult TerkirimHistory(string suratoutboxid)
        {
            var usr = HttpContext.User.Identity as Models.Entities.InternalUserIdentity;
            List<Models.Entities.SuratInbox> data = persuratanmodel.GetHistoryTerkirim(suratoutboxid, usr.NamaPegawai);
            var find = data.Find(x => x.Redaksi == "Penanggung Jawab");
            var st = new Models.Entities.StatusSurat();
            if(find == null || string.IsNullOrEmpty(find.Redaksi)) { find = data.Find(x => x.Redaksi == "Asli"); }
            if (find != null && !string.IsNullOrEmpty(find.TanggalBuka) && find.Keterangan != "Surat Telah diselesaikan")
            {
                st = persuratanmodel.GetStatusSurat(find, usr);
            }
            else
            {
                try
                {
                    st = persuratanmodel.GetStatusSurat(data[0], usr, pj: false);
                } catch
                {
                    st = new Models.Entities.StatusSurat()
                    {
                        Redaksi = "Penanggung Jawab",
                        Nama = "-",
                        Status = "Selesai",
                        Keterangan = "data terkirim tidak ditemukan",
                    };
                }              
            }

            if (st.Nama == null)
            {
                find = data[0];
                if (!string.IsNullOrEmpty(find.Keterangan) && find.Keterangan.Contains("Surat Telah diselesaikan"))
                {
                    st = new Models.Entities.StatusSurat()
                    {
                        Redaksi = "Penanggung Jawab",
                        Nama = find.NamaPenerima,
                        Status = "Selesai",
                        Keterangan = find.Keterangan,
                    };
                }
                else if (string.IsNullOrEmpty(find.TanggalBuka))
                {
                    st = new Models.Entities.StatusSurat()
                    {
                        Redaksi = "Penanggung Jawab",
                        Nama = find.NamaPenerima,
                        Status = "Belum diproses",
                        Keterangan = "",
                    };
                }
                else
                {
                    st = new Models.Entities.StatusSurat()
                    {
                        Redaksi = "Penanggung Jawab",
                        Nama = find.NamaPenerima,
                        Status = "-",
                        Keterangan = "",
                    };
                }
            }

            return Json(new { status = data.Count > 0, data = data, st = st }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BukaSuratMasuk(string suratid, string suratinboxid, string nomorsurat)
        {
            if (!String.IsNullOrEmpty(suratinboxid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;


                ViewBag.UnitKerjaId = unitkerjaid;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

                Surat.Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid);
                if (surat.NIP != pegawaiid)
                {
                    return RedirectToAction("SuratMasuk", "Flow");
                }
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
                surat.ListPerintahDisposisi = persuratanmodel.GetPerintahDisposisi();
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListUnitKerjaHistoriSurat = persuratanmodel.GetUnitKerjaSuratHistory(suratid);
                surat.ListProfileTujuan = new List<Models.Entities.Profile>(); // dataMasterModel.GetProfileTujuan(myProfiles, kantorid);
                surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
                surat.ListProfiles = new List<Models.Entities.Profile>();
                surat.CatatanSebelumnya = persuratanmodel.GetCatatanSebelumnya(suratinboxid);
                surat.PerintahDisposisiSebelumnya = persuratanmodel.GetDisposisiSebelumnya(suratinboxid);

                if (string.IsNullOrEmpty(surat.TanggalTerima))
                {
                    surat.TanggalTerima = persuratanmodel.GetServerDate().ToString("dd/MM/yyyy HH:mm");
                }

                //ViewBag.IsProfileTataUsaha = "0";

                string ismyprofiletu = dataMasterModel.GetIsMyProfileTU(pegawaiid);
                if (ismyprofiletu == "1")
                {
                    //ViewBag.IsProfileTataUsaha = "1";

                    if (string.IsNullOrEmpty(surat.NomorAgendaSurat))
                    {
                        // Bila profile user login adalah profile TU dan belum ada nomor agenda, langsung buat nomor agenda pada satker user login
                        surat.NomorAgendaSurat = kontermodel.GetNomorAgendaSuratAndUpdate(kantorid, unitkerjaid, profileidtu, surat.SuratId);
                    }
                }
                
                //surat.ListProfilePegawaiPenerima = dataMasterModel.GetProfilePegawaiTujuan(myProfiles);
                ////surat.ListProfileTujuan = dataMasterModel.GetProfileTujuan(myProfiles);
                ////surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();

                // Update Flag Buka Surat
                string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                Models.Entities.TransactionResult tr = persuratanmodel.BukaSuratInbox(surat.SuratId, surat.SuratInboxId, pegawaiid, namapegawai);

                if (surat.KeteranganSuratRedaksi.ToLower().Contains("tembusan"))
                {
                    surat.PenanggungJawab = persuratanmodel.GetPenanggungJawab(suratid, satkerid,pegawaiid);

                    if (string.IsNullOrEmpty(surat.PenanggungJawab))
                    {
                        surat.PenanggungJawab = "-";
                    }
                }
                surat.Waktu = persuratanmodel.GetWaktuProses(suratinboxid);
                surat.WaktuTunggak = persuratanmodel.GetWaktuTunggak(suratinboxid);

                return View("EntriSuratMasuk", surat);
            }
            else
            {
                return RedirectToAction("SuratMasuk", "Flow");
            }
        }



        public ActionResult KirimUlangSurat(string suratid, string nomorsurat)
        {
            string suratinboxid = persuratanmodel.GetSuratInboxIdFromSuratId(suratid);
            if (!String.IsNullOrEmpty(suratinboxid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                ViewBag.UnitKerjaId = unitkerjaid;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

                Surat.Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid);
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
                surat.ListPerintahDisposisi = persuratanmodel.GetPerintahDisposisi();
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListProfileTujuan = new List<Models.Entities.Profile>();
                surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
                surat.ListProfiles = new List<Models.Entities.Profile>();
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

                    //if (string.IsNullOrEmpty(surat.NomorAgendaSurat))
                    //{
                    //    // Bila profile user login adalah profile TU dan belum ada nomor agenda, langsung buat nomor agenda pada satker user login
                    //    surat.NomorAgendaSurat = kontermodel.GetNomorAgendaSuratAndUpdate(kantorid, unitkerjaid, profileidtu, surat.SuratId);
                    //}
                }

                return View("KirimUlangSurat", surat);
            }
            else
            {
                return RedirectToAction("SuratOutbox", "Flow");
            }
        }

        public ActionResult BukaSuratKeluar(string suratid, string suratinboxid, string nomorsurat)
        {
            if (!String.IsNullOrEmpty(suratinboxid))
            {
                string referensiAsalSurat = "";
                string referensiTujuanSurat = "";
                string referensiNomorAgenda = "";
                string referensiNomorSurat = "";
                string referensiTanggalSurat = "";
                string referensiPerihal = "";
                string referensiKategori = "";
                string referensiUnitKerjaId = "";

                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                ViewBag.UnitKerjaId = unitkerjaid;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

                Surat.Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid);
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListUnitKerjaHistoriSurat = persuratanmodel.GetUnitKerjaSuratHistory(suratid);
                surat.ListProfileTujuan = new List<Models.Entities.Profile>(); // dataMasterModel.GetProfileTujuan(myProfiles, kantorid);
                surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
                //surat.ListProfilePegawaiPenerima = dataMasterModel.GetProfilePegawaiTujuan(myProfiles);
                surat.CatatanSebelumnya = persuratanmodel.GetCatatanSebelumnya(suratinboxid);
                surat.IsBedaUnitKerja = "0";
                surat.LabelTitleJenisSurat = "SURAT KELUAR";

                if (!string.IsNullOrEmpty(surat.ReferensiSurat))
                {
                    // ambil data surat sebagai referensi
                    Models.Entities.Surat referensisurat = persuratanmodel.GetSuratBySuratId(surat.ReferensiSurat, satkerid);
                    referensiAsalSurat = referensisurat.PengirimSurat;
                    referensiTujuanSurat = referensisurat.PenerimaSurat;
                    referensiNomorAgenda = referensisurat.NomorAgenda;
                    referensiNomorSurat = referensisurat.NomorSurat;
                    referensiTanggalSurat = referensisurat.TanggalSurat;
                    referensiPerihal = referensisurat.Perihal;
                    referensiKategori = referensisurat.Kategori;
                    referensiUnitKerjaId = referensisurat.ReferensiUnitKerjaId;

                    // Referensi Surat (Surat Masuk / Inisiatif)
                    surat.ReferensiAsalSurat = referensiAsalSurat;
                    surat.ReferensiTujuanSurat = referensiTujuanSurat;
                    surat.ReferensiNomorAgenda = referensiNomorAgenda;
                    surat.ReferensiNomorSurat = referensiNomorSurat;
                    surat.ReferensiTanggalSurat = referensiTanggalSurat;
                    surat.ReferensiPerihal = referensiPerihal;
                    surat.ReferensiKategori = referensiKategori;
                    surat.ReferensiUnitKerjaId = referensiUnitKerjaId;

                    if (referensiUnitKerjaId != unitkerjaid)
                    {
                        surat.IsBedaUnitKerja = "1";
                        surat.LabelTitleJenisSurat = "SURAT MASUK";
                    }
                }

                if(surat.KeteranganSuratRedaksi.ToLower().Contains("tembusan"))
                {
                    surat.PenanggungJawab = persuratanmodel.GetPenanggungJawab(suratid, satkerid, pegawaiid);
                }

                // Update Flag Buka Surat
                string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                Models.Entities.TransactionResult tr = persuratanmodel.BukaSuratInbox(surat.SuratId, surat.SuratInboxId, pegawaiid, namapegawai);
                surat.Waktu = persuratanmodel.GetWaktuProses(suratinboxid);
                surat.WaktuTunggak = persuratanmodel.GetWaktuTunggak(suratinboxid);

                return View("EntriSuratKeluar", surat);
            }
            else
            {
                return RedirectToAction("SuratInisiatif", "Flow");
            }
        }

        public ActionResult BukaSuratInisiatif(string suratid, string suratinboxid, string nomorsurat)
        {
            if (!String.IsNullOrEmpty(suratinboxid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                ViewBag.UnitKerjaId = unitkerjaid;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

                Surat.Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratInboxId(suratinboxid, satkerid, suratid);
                surat.ListSifatSurat = persuratanmodel.GetSifatSurat();
                surat.ListTipeSurat = persuratanmodel.GetTipeSurat();
                //surat.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                surat.ListUnitKerja = dataMasterModel.GetListUnitKerjaInisiatif(pegawaiid);
                surat.ListUnitKerjaHistoriSurat = persuratanmodel.GetUnitKerjaSuratHistory(suratid);
                surat.ListProfileTujuan = new List<Models.Entities.Profile>(); // dataMasterModel.GetProfileTujuan(myProfiles, kantorid);
                surat.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
                //surat.ListProfilePegawaiPenerima = dataMasterModel.GetProfilePegawaiTujuan(myProfiles);
                surat.CatatanSebelumnya = persuratanmodel.GetCatatanSebelumnya(suratinboxid);

                // Update Flag Buka Surat
                string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                Models.Entities.TransactionResult tr = persuratanmodel.BukaSuratInbox(surat.SuratId, surat.SuratInboxId, pegawaiid, namapegawai);

                return View("EntriSuratInisiatif", surat);
            }
            else
            {
                return RedirectToAction("SuratInisiatif", "Flow");
            }
        }

        public async Task<ActionResult> GetFileSurat(string id, string kantorid)
        {
            Models.Entities.TransactionResult result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                //kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                if (string.IsNullOrEmpty(kantorid))
                {
                    kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                }
                string tipe = "Surat";
                string versi = kontentm.CekVersi(id).ToString();
                string ext = new Models.SuratModel().GetExt(id);
                ext = string.IsNullOrEmpty(ext) ? ".pdf" : ext;

                if (kantorid.Length < 32) // Tambahan Sementara :: Arya :: 2020-09-17
                {
                    kantorid = dataMasterModel.GetKantorIdFromUnitKerjaId(kantorid);
                }

                var file = new Models.SuratModel().getFileLampiran(id);
                var filePath = file.Path.Split('|');
                DateTime tglSunting = persuratanmodel.getTglSunting(id,tipe);
                string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);
                //serviceurl = "ServiceBaseUrl";
                if (filePath.Length == 2)
                {
                    tipe = filePath[0];
                    id = filePath[1];
                    versi = new Models.TandaTanganElektronikModel().CekVersi(id).ToString();
                    file.PengenalFile = string.Concat(file.PengenalFile, ".pdf");
                    serviceurl = "ServiceEofficeUrl";
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

        public async Task<ActionResult> GetFileSuratWithExt(string id, string kantorid, string namafile, string extension)
        {
            var result = new { Status = false, Message = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                //string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                if (string.IsNullOrEmpty(kantorid))
                {
                    kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                }
                string tipe = "Surat";
                string versi = kontentm.CekVersi(id).ToString();

                DateTime tglSunting = persuratanmodel.getTglSunting(id, tipe);
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
                            docfile.FileDownloadName = namafile;
                            if (extension.Equals(".jpg"))
                            {
                                docfile = new FileStreamResult(strm, "image/jpeg");
                            }
                            else if (extension.Equals(".png"))
                            {
                                docfile = new FileStreamResult(strm, "image/png");
                            }
                            else if (extension.Equals(".xls"))
                            {
                                docfile = new FileStreamResult(strm, "application/vnd.ms-excel");
                            }
                            else if (extension.Equals(".xlsx"))
                            {
                                docfile = new FileStreamResult(strm, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                            }
                            else if (extension.Equals(".doc"))
                            {
                                docfile = new FileStreamResult(strm, "application/msword");
                            }
                            else if (extension.Equals(".docx"))
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


        #region Simpan Surat

        [HttpPost]
        public JsonResult InsertSuratMasuk(Models.Entities.Surat data, string daftarTujuan, List<HttpPostedFileBase> fileUploadStream)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawaipengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            string myClientId = Functions.MyClientId;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            data.UserId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            string judul = "Surat masuk dari " + data.PengirimSurat + " nomor: " + data.NomorSurat;

            // Cek Tujuan Surat
            List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = new List<Models.Entities.SessionTujuanSurat>();
            try
            {
                dynamic json = JsonConvert.DeserializeObject(daftarTujuan);

                foreach (var js in json)
                {
                    dataSessionTujuanSurat.Add(JsonConvert.DeserializeObject<Models.Entities.SessionTujuanSurat>(JsonConvert.SerializeObject(js.Value)));
                }
            }
            catch
            {
                tr.Pesan = "Data yang dikirimkan tidak sesuai";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            //dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId); // data.UserId 
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            //List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = persuratanmodel.GetListSessionLampiran(myClientId); // pegawaiid
            List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = new List<Models.Entities.SessionLampiranSurat>();
            foreach (HttpPostedFileBase file in fileUploadStream)
            {
                if (file != null)
                {
                    Models.Entities.SessionLampiranSurat datafile = new Models.Entities.SessionLampiranSurat();
                    datafile.NamaFile = file.FileName;
                    MemoryStream ms1 = new MemoryStream();
                    file.InputStream.CopyTo(ms1);
                    datafile.ObjectFile = ms1.ToArray();
                    datafile.LampiranSuratId = persuratanmodel.GetUID();
                    datafile.Nip = pegawaiid;
                    dataSessionLampiran.Add(datafile);
                }
            }


            string isFileAttMandatory = ConfigurationManager.AppSettings["IsFileAttMandatory"].ToString();
            if (isFileAttMandatory == "true")
            {
                if (dataSessionLampiran.Count == 0)
                {
                    tr.Pesan = "File Surat wajib diupload";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            string suratid_duplikat = "";

            if (!string.IsNullOrEmpty(data.NomorSurat))
            {
                Regex sWhitespace = new Regex(@"[^0-9a-zA-Z-./]+");

                string nomorsurat = sWhitespace.Replace(data.NomorSurat, "");
                data.NomorSurat = nomorsurat;

                //suratid_duplikat = persuratanmodel.GetSuratIdFromNomorSurat(nomorsurat);
                suratid_duplikat = persuratanmodel.GetSuratIdFromNomorSuratDanPengirim(nomorsurat, data.PengirimSurat);
            }


            #region Simpan File Fisik

            foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId; // persuratanmodel.GetUID();

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    DateTime tglSunting = DateTime.Now;
                    string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("Surat"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranSurat.NamaFile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            tr = kontentm.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal Membuat Surat, ada file lampiran yang bermasalah\nHarap cek ulang lampiran anda.";

                            string msg = reqresult.ReasonPhrase;
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                        //tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                        //tr.Pesan = reqresult.ReasonPhrase;
                    }

                    //tr = kontentm.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                }
            }

            #endregion


            if (!string.IsNullOrEmpty(suratid_duplikat))
            {
                //tr.Pesan = "Nomor Surat " + data.NomorSurat + " sudah ada.";

                // Merger Surat Masuk

                data.SuratId = suratid_duplikat;

                tr = persuratanmodel.MergeSuratMasuk(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim, dataSessionTujuanSurat, dataSessionLampiran);
            }
            else
            {
                // Insert Surat Masuk

                tr = persuratanmodel.InsertSuratMasuk(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim, dataSessionTujuanSurat, dataSessionLampiran);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SimpanSumberSurat(string Sumber, string id)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };



            tr = persuratanmodel.SimpanSumberSurat(Sumber, id);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSuratKeluar(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawaipengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            string myClientId = Functions.MyClientId;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            data.UserId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            string judul = "Surat keluar untuk " + data.PenerimaSurat + " nomor: " + data.NomorSurat;

            if (data.ArahSuratKeluar == "Internal")
            {
                // Cek Tujuan Surat
                List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId); // data.UserId
                if (dataSessionTujuanSurat.Count == 0)
                {
                    tr.Pesan = "Tujuan Surat wajib diisi";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }

            List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = persuratanmodel.GetListSessionLampiran(myClientId); // pegawaiid
            //if (dataSessionLampiran.Count == 0)
            //{
            //    tr.Pesan = "File Surat wajib diupload";
            //    return Json(tr, JsonRequestBehavior.AllowGet);
            //}

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId; // persuratanmodel.GetUID();

                    string namafile = lampiranSurat.NamaFile;

                    #region set fileExtension

                    string fileExtension = "";
                    if (namafile.Contains(".pdf"))
                    {
                        fileExtension = ".pdf";
                    }
                    else if (namafile.Contains(".jpg"))
                    {
                        fileExtension = ".jpg";
                    }
                    else if (namafile.Contains(".png"))
                    {
                        fileExtension = ".png";
                    }
                    else if (namafile.Contains(".xls"))
                    {
                        fileExtension = ".xls";
                    }
                    else if (namafile.Contains(".xlsx"))
                    {
                        fileExtension = ".xlsx";
                    }
                    else if (namafile.Contains(".doc"))
                    {
                        fileExtension = ".doc";
                    }
                    else if (namafile.Contains(".docx"))
                    {
                        fileExtension = ".docx";
                    }

                    #endregion

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

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
                    reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                        tr.Pesan = reqresult.ReasonPhrase;
                    }

                    tr = kontentm.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                }
            }

            if (!string.IsNullOrEmpty(data.NomorSurat))
            {
                Regex sWhitespace = new Regex(@"[^0-9a-zA-Z-./]+");

                string nomorsurat = sWhitespace.Replace(data.NomorSurat, "");
                data.NomorSurat = nomorsurat;
            }

            tr = persuratanmodel.InsertSuratKeluar(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSuratInisiatif(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawaipengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            string myClientId = Functions.MyClientId;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            data.UserId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            string judul = "Surat keluar untuk " + data.PenerimaSurat + " nomor: " + data.NomorSurat;

            if (data.ArahSuratKeluar == "Internal")
            {
                // Cek Tujuan Surat
                List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId); // data.UserId
                if (dataSessionTujuanSurat.Count == 0)
                {
                    tr.Pesan = "Tujuan Surat wajib diisi";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }

            List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = persuratanmodel.GetListSessionLampiran(myClientId); // pegawaiid
            //if (dataSessionLampiran.Count == 0)
            //{
            //    tr.Pesan = "File Surat wajib diupload";
            //    return Json(tr, JsonRequestBehavior.AllowGet);
            //}

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId; // persuratanmodel.GetUID();

                    string namafile = lampiranSurat.NamaFile;

                    #region set fileExtension

                    string fileExtension = "";
                    if (namafile.Contains(".pdf"))
                    {
                        fileExtension = ".pdf";
                    }
                    else if (namafile.Contains(".jpg"))
                    {
                        fileExtension = ".jpg";
                    }
                    else if (namafile.Contains(".png"))
                    {
                        fileExtension = ".png";
                    }
                    else if (namafile.Contains(".xls"))
                    {
                        fileExtension = ".xls";
                    }
                    else if (namafile.Contains(".xlsx"))
                    {
                        fileExtension = ".xlsx";
                    }
                    else if (namafile.Contains(".doc"))
                    {
                        fileExtension = ".doc";
                    }
                    else if (namafile.Contains(".docx"))
                    {
                        fileExtension = ".docx";
                    }

                    #endregion


                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

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
                    reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                        tr.Pesan = reqresult.ReasonPhrase;
                    }

                    tr = kontentm.SimpanKontenFile(kantorid, id, judul, namapegawaipengirim, data.TanggalSurat, "Surat", out versi);
                }
            }

            tr = persuratanmodel.InsertSuratInisiatif(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditSurat(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (!string.IsNullOrEmpty(data.SuratId))
            {
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string namapegawaipengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                data.UserId = pegawaiid;
                data.NamaPengirim = namapegawaipengirim;

                // Profile Id Pengirim
                //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
                List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
                if (listProfilePegawai.Count > 0)
                {
                    data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                    data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
                }

                if (!string.IsNullOrEmpty(data.NomorSurat))
                {
                    Regex sWhitespace = new Regex(@"[^0-9a-zA-Z-./]+");

                    string nomorsurat = sWhitespace.Replace(data.NomorSurat, "");
                    data.NomorSurat = nomorsurat;
                }

                tr = persuratanmodel.EditSurat(data, kantorid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapegawaipengirim);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditSuratByTU(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (!string.IsNullOrEmpty(data.SuratId))
            {

                tr = persuratanmodel.EditSuratByTU(data);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KirimSuratMasuk(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            string myClientId = Functions.MyClientId;

            data.UserId = pegawaiid;
            data.NamaPengirim = namapengirim;

            // Cek Tujuan Surat
            List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = new List<Models.Entities.SessionTujuanSurat>();
            if (!string.IsNullOrEmpty(data.DaftarTujuan))
            {
                var listtujuan = data.DaftarTujuan.Split('^');
                foreach(var l in listtujuan)
                {
                    var split = l.Split('|');
                    dataSessionTujuanSurat.Add(new Models.Entities.SessionTujuanSurat
                    {
                        ProfileId = split[0],
                        NIP = split[1],
                        NamaJabatan = split[2],
                        NamaPegawai = split[3],
                        Redaksi = split[4]
                    });
                }
            }
            else
            {
                dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId); // data.UserId
            }

            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            } else
            {
                data.ListTujuanSurat = dataSessionTujuanSurat;
            }

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            Models.Entities.Profile _profile = dataMasterModel.getDataPengirimBySuratInboxId(data.SuratInboxId);
            if (_profile != null)
            {
                data.ProfileIdPengirim = _profile.ProfileId;
                data.NamaProfilePengirim = _profile.NamaProfile;
                unitkerjaid = _profile.UnitKerjaId;
                profileidtu = _profile.ProfileIdTu;
            }

            tr = persuratanmodel.KirimSuratMasuk(data, kantorid, unitkerjaid, profileidtu, pegawaiid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KirimSuratKeluar(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

            string myClientId = Functions.MyClientId;

            data.UserId = pegawaiid;
            data.NamaPengirim = namapengirim;

            // Cek Tujuan Surat
            List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId); // data.UserId
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            Models.Entities.Profile _profile = dataMasterModel.getDataPengirimBySuratInboxId(data.SuratInboxId);
            if (_profile != null)
            {
                data.ProfileIdPengirim = _profile.ProfileId;
                data.NamaProfilePengirim = _profile.NamaProfile;
                unitkerjaid = _profile.UnitKerjaId;
                profileidtu = _profile.ProfileIdTu;
            }

            tr = persuratanmodel.KirimSuratKeluar(data, kantorid, unitkerjaid, profileidtu, pegawaiid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanCatatanAnda(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            tr = persuratanmodel.SimpanCatatanAnda(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProsesSuratMasuk(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            data.UserId = pegawaiid;
            data.NamaPengirim = namapengirim;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            //// Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid);
            //if (listProfilePegawai.Count > 0)
            //{
            //    data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
            //    data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            //}

            tr = persuratanmodel.ProsesSuratMasuk(data, kantorid, satkerid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapengirim);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProsesSuratMasukMassive(List<Models.Entities.SuratIds> suratIds)
        {
            bool sukses = true;
            string pesan = string.Empty;
            string id = string.Empty;

            if (suratIds != null)
            {
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

                string catatananda = "Proses surat masuk secara masif oleh " + namapengirim;

                try
                {
                    if (!string.IsNullOrEmpty(suratIds[0].suratid) && !string.IsNullOrEmpty(suratIds[0].suratinboxid))
                    {
                        Models.Entities.TransactionResult tr = persuratanmodel.ProsesSuratMasukMassive(suratIds, kantorid, satkerid, unitkerjaid, myProfileId, profileidtu, pegawaiid, namapengirim, catatananda);

                        if (!tr.Status)
                        {
                            throw new Exception(tr.Pesan);
                        }

                        pesan = tr.Pesan;
                    }
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

            return Json(new { Status = sukses, Pesan = pesan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProsesSuratKeluar(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            data.UserId = pegawaiid;
            data.NamaPengirim = namapengirim;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");


            tr = persuratanmodel.ProsesSuratKeluar(data, kantorid, satkerid, myProfileId, profileidtu, pegawaiid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanSuratKeluar(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            tr = persuratanmodel.SimpanSuratKeluar(data, kantorid, unitkerjaid, profileidtu);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanNomorSurat(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            tr = persuratanmodel.SimpanNomorSurat(data, kantorid, unitkerjaid, profileidtu);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanTargetSelesaiSuratMasuk(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            tr = persuratanmodel.SimpanTargetSelesaiSuratMasuk(data, kantorid, unitkerjaid, profileidtu);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ArsipSurat(Models.Entities.Surat data)
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                data.UserId = pegawaiid;
                data.NamaPengirim = namapengirim;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                // Profile Id Pengirim
                //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
                List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
                if (listProfilePegawai.Count > 0)
                {
                    data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                    data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
                }

                result = persuratanmodel.ArsipSurat(data, kantorid, satkerid, pegawaiid);
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

        [HttpPost]
        public ActionResult TolakSurat(Models.Entities.Surat data)
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            try
            {
                result = persuratanmodel.TolakSurat(data);
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

        [HttpPost]
        public ActionResult KembalikanSurat(Models.Entities.SuratKembali data)
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            try
            {
                string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
                data.UserId = userid;

                result = persuratanmodel.KembalikanSurat(data);
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

        [HttpPost]
        public ActionResult SelesaiSuratInbox(Models.Entities.Surat data)
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                data.UserId = pegawaiid;
                data.NamaPengirim = namapengirim;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                // Profile Id Pengirim
                //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
                List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
                if (listProfilePegawai.Count > 0)
                {
                    data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                    data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
                }

                result = persuratanmodel.SelesaiSuratInbox(data, kantorid, satkerid, pegawaiid);
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

        #endregion


        #region Session Lampiran Surat

        public ActionResult HapusSessionLampiran()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                //string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string nip = Functions.MyClientId;
                if (!String.IsNullOrEmpty(nip))
                {
                    result = persuratanmodel.HapusSessionLampiran(nip);
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
        public JsonResult InsertSessionLampiran(Models.Entities.SessionLampiranSurat data)
        {
            //data.UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            data.UserId = Functions.MyClientId;
            data.Nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            var mfile = Request.Files["file"];
            if (mfile != null &&
                (mfile.ContentType == "application/pdf" ||
                 mfile.ContentType == "image/jpeg" ||
                 mfile.ContentType == "image/png" ||
                 mfile.ContentType == "application/vnd.ms-excel" ||
                 mfile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ||
                 mfile.ContentType == "application/msword" ||
                 mfile.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                )
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

            Models.Entities.TransactionResult tr = persuratanmodel.InsertSessionLampiran(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusSessionLampiranById()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    result = persuratanmodel.HapusSessionLampiranById(id);
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

        public ActionResult ListSessionNewFileSurat(int? draw, int? start, int? length)
        {
            List<Models.Entities.SessionLampiranSurat> result = new List<Models.Entities.SessionLampiranSurat>();
            decimal? total = 0;

            //string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string nip = Functions.MyClientId;

            if (!String.IsNullOrEmpty(nip))
            {
                result = persuratanmodel.GetSessionLampiranForTable(nip);
                if (result.Count > 0)
                {
                    total = result.Count;
                }
            }

            return Json(new { data = result, draw = draw, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSessionLampiran(int? draw, int? start, int? length)
        {
            List<Models.Entities.SessionLampiranSurat> result = new List<Models.Entities.SessionLampiranSurat>();
            decimal? total = 0;

            //string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string nip = Functions.MyClientId;

            if (!String.IsNullOrEmpty(nip))
            {
                result = persuratanmodel.GetSessionLampiranForTable(nip);
                if (result.Count > 0)
                {
                    total = result.Count;
                }
            }

            return Json(new { data = result, draw = draw, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFileLampiranById(string lampiransuratid)
        {
            byte[] byteArray = persuratanmodel.GetSessionLampiranById(lampiransuratid);

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("FileLampiran", ".pdf");

            return docfile;
        }

        public ActionResult GetFileLampiranByIdWithExt(string lampiransuratid, string namafile, string extension)
        {
            byte[] byteArray = persuratanmodel.GetSessionLampiranById(lampiransuratid);

            MemoryStream mss = new MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat(namafile, ".pdf");
            if (extension == ".jpg")
            {
                docfile = new FileStreamResult(mss, "image/jpeg");
                docfile.FileDownloadName = String.Concat(namafile, ".jpg");
            }
            else if (extension == ".png")
            {
                docfile = new FileStreamResult(mss, "image/png");
                docfile.FileDownloadName = String.Concat(namafile, ".png");
            }
            else if (extension == ".xls")
            {
                docfile = new FileStreamResult(mss, "application/vnd.ms-excel");
                docfile.FileDownloadName = String.Concat(namafile, ".xls");
            }
            else if (extension == ".xlsx")
            {
                docfile = new FileStreamResult(mss, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                docfile.FileDownloadName = String.Concat(namafile, ".xlsx");
            }
            else if (extension == ".doc")
            {
                docfile = new FileStreamResult(mss, "application/msword");
                docfile.FileDownloadName = String.Concat(namafile, ".doc");
            }
            else if (extension == ".docx")
            {
                docfile = new FileStreamResult(mss, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                docfile.FileDownloadName = String.Concat(namafile, ".docx");
            }

            return docfile;
        }

        #endregion


        #region Session Tujuan Surat

        public ActionResult HapusSessionTujuanSurat()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                //string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string nip = Functions.MyClientId;
                if (!String.IsNullOrEmpty(nip))
                {
                    result = persuratanmodel.HapusSessionTujuanSurat(nip);
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

        public JsonResult InsertSessionTujuanSurat(Models.Entities.SessionTujuanSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            //data.UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            data.UserId = Functions.MyClientId;

            if (data.Redaksi == "Penanggung Jawab")
            {
                int icek = persuratanmodel.JumlahTujuanSuratPJ(data.UserId);
                if (icek > 0)
                {
                    tr.Pesan = "Penanggung Jawab sudah ada";
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
            }

            tr = persuratanmodel.InsertSessionTujuanSurat(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HapusSessionTujuanSuratById()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    result = persuratanmodel.HapusSessionTujuanSuratById(id);
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

        public ActionResult ListSessionNewTujuanSurat(int? draw, int? start, int? length)
        {
            List<Models.Entities.SessionTujuanSurat> result = new List<Models.Entities.SessionTujuanSurat>();
            decimal? total = 0;

            //string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string nip = Functions.MyClientId;

            if (!String.IsNullOrEmpty(nip))
            {
                result = persuratanmodel.GetListSessionTujuanSurat(nip);
                if (result.Count > 0)
                {
                    total = result.Count;
                }
            }

            return Json(new { data = result, draw = draw, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Tujuan Disposisi Surat

        public ActionResult ListDisposisiSurat(int? draw, int? start, int? length)
        {
            List<Models.Entities.DisposisiSurat> result = new List<Models.Entities.DisposisiSurat>();
            decimal? total = 0;

            string suratid = Request.Form["suratid"].ToString();

            if (!String.IsNullOrEmpty(suratid))
            {
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                // ambil data disposisi, per surat per unitkerja
                result = persuratanmodel.GetListDisposisiSurat(suratid, unitkerjaid, "");
                if (result.Count > 0)
                {
                    total = result.Count;
                }
                else
                {
                    // bila data di atas tidak ada, ambil data disposisi, per surat tanpa unitkerja
                    result = persuratanmodel.GetListDisposisiSurat(suratid, "", "");
                    if (result.Count > 0)
                    {
                        total = result.Count;
                    }
                }
            }

            return Json(new { data = result, draw = draw, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult InsertDisposisiSurat(Models.Entities.DisposisiSurat data)
        {
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            data.UnitKerjaId = unitkerjaid;

            Models.Entities.TransactionResult tr = persuratanmodel.InsertDisposisiSurat(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult InsertDispoUnitKerja(string suratid)
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            Models.Entities.TransactionResult tr = persuratanmodel.InsertDispoUnitKerja(kantorid, pegawaiid, suratid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HapusDisposisiSuratById()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    result = persuratanmodel.HapusDisposisiSuratById(id);
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


        #region Surat Pengantar
        public ActionResult SuratPengantar() {
            ViewBag.userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            return View();
        }

        public ActionResult ListSuratPengantar(int? draw, int? start, int? length, string srchkey)
        {
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string useriunitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            List<Models.Entities.PengantarSurat> result = persuratanmodel.GetListPengantar(from,to,unitkerjaid: useriunitkerjaid, nomorpengantar:srchkey);
            decimal? total = 0;
            if(result.Count > 0)
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

                    if (r.UserId == (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId)
                    {
                        r.Status = "Pembuat";
                    }
                    else
                    {
                        r.Status = "None";
                    }

                    r.NamaPembuat = new Models.InternalUser().GetNamaPengguna(r.UserId);
                    var doktte = new Models.NaskahDinasModel().fDokTTEPengantarSuratMasuk(r.PengantarSuratId);
                    if(doktte != null)
                    {
                        r.StatusTTE = !string.IsNullOrEmpty(doktte.DokumenElektronikId) ? $"{doktte.Status}|{doktte.DokumenElektronikId}" : "";
                    }
                }

                total = result[0].Total > 50 ? 50 : result[0].Total;
            }


            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSuratByPengirim(string namapegawai, string ukidPenerima, string tanggal)
        {
            bool ismenteri = false;
            if (ukidPenerima == "H0000001" || ukidPenerima == "H0000002")
            {
                ismenteri = true;
            }
            var profileidTu = persuratanmodel.GetProfileidTuFromUnitKerja(ukidPenerima, ismenteri);

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            string satkerid;
            if (tipekantorid == 1)
            {
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                satkerid = unitkerjaid;
            }
            else
            {
                satkerid = kantorid;
            }

            string[] parapengirim = namapegawai.Split('|');
            List<Models.Entities.Surat> result = new List<Models.Entities.Surat>();
            foreach (var pengirim in parapengirim)
            {
                result.AddRange(persuratanmodel.GetListSuratForPengantar(pengirim, profileidTu, tanggal));
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

        public JsonResult SimpanNewSuratPengantar(Models.Entities.PengantarSurat ps)
        {
            ps.UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            ps.UnitKerjaId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "data gagal disimpan"};
            string mssg = string.Empty;
            if (string.IsNullOrEmpty(ps.NomorSurat))
            {
                if (ps.Gnumber)
                {
                    var nomorakhir = new Models.NaskahDinasModel().nomorAkhirPengantar(ps.UnitKerjaId);
                    if(nomorakhir == 0)
                    {
                        mssg = "Terdapat Perbedaan Format pada nomor sebelumnya";
                        return Json(new { Status = result.Status, pesan = mssg }, JsonRequestBehavior.AllowGet);
                    }
                    ps.NomorSurat = $"{nomorakhir+1}/P-100.5.1/{new Models.NaskahDinasModel().ToRoman(DateTime.Now.Month)}/{DateTime.Now.Year.ToString()}";
                    result = persuratanmodel.SimpanNewSuratPengantar(ps);
                    mssg = result.Pesan;
                } 
                else
                {
                    mssg = "Nomor Surat Kosong";
                    return Json(new { Status = result.Status, pesan = mssg }, JsonRequestBehavior.AllowGet);
                }
            } else
            {
                result = persuratanmodel.SimpanNewSuratPengantar(ps);
                mssg = result.Pesan;
            }
            return Json(new { Status = result.Status, pesan = mssg, psid = result.ReturnValue}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult HapusSuratPengantar(string psid)
        {
            var Data = persuratanmodel.GetNewPengantarSurat(psid);
            var usrid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            bool status = false;
            string mssg = "";
            if (Data.UserId == usrid || OtorisasiUser.isTU())
            {
                mssg = persuratanmodel.HapusSuratPengantar(psid, (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId);
                if(mssg == "berhasil")
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

        public ActionResult NewSuratPengantar(string pengantarsuratid)
        {
            var usr = HttpContext.User.Identity as Models.Entities.InternalUserIdentity;
            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
            List<Models.Entities.UnitKerja> lstUnitkerja = new List<Models.Entities.UnitKerja>();
            var lst = new Models.Entities.UnitKerja();
            if (tipekantorid == 1)
            {
                lst.UnitKerjaId = "H0000001";
                lst.NamaUnitKerja = "Menteri Agraria dan Tata Ruang/Kepala Badan Pertanahan Nasional";
                lstUnitkerja.Add(lst);
                lst = new Models.Entities.UnitKerja();
                lst.UnitKerjaId = "H0000002";
                lst.NamaUnitKerja = "Wakil Menteri Agraria dan Tata Ruang/Wakil Kepala Badan Pertanahan Nasional";
                lstUnitkerja.Add(lst);
            }
            var units = dataMasterModel.GetListUnitKerjaByKantorId(usr.KantorId, true, false);
            lstUnitkerja.AddRange(units);

            ViewBag.ListUnitkerja = lstUnitkerja;
            ViewBag.ListPenandatangan = new Models.SuratModel().GetProfilesByUnitKerja(usr.UnitKerjaId);
            ViewBag.ListPetugasEntri = persuratanmodel.GetPetugasSuratMasukByUnitKerja(usr.UnitKerjaId);
            ViewBag.ListSuratTersimpan = new List<Models.Entities.Surat>();
            var Data = new Models.Entities.PengantarSurat();
            if (!string.IsNullOrEmpty(pengantarsuratid))
            {                
                Data = persuratanmodel.GetNewPengantarSurat(pengantarsuratid);
                if (Data.UnitKerjaId == usr.UnitKerjaId || OtorisasiUser.isTU())
                {
                    List<Models.Entities.Surat> lstSurat = new List<Models.Entities.Surat>();
                    string satkerid;
                    if (tipekantorid == 1)
                    {
                        string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                        satkerid = unitkerjaid;
                    }
                    else
                    {
                        satkerid = usr.KantorId;
                    }
                    foreach (var d in Data.LstSurat.Split('|'))
                    {
                        var findsurat = persuratanmodel.GetSuratBySuratId(d, satkerid);
                        lstSurat.Add(findsurat);
                    }
                    ViewBag.ListSuratTersimpan = lstSurat;
                } else
                {
                    RedirectToAction("SuratPengantar", "Flow");
                }                
            }
            
            return View(Data);
        }


        public ActionResult SuratPengantarBaru()
        {
            Surat.Models.Entities.FindSurat find = new Surat.Models.Entities.FindSurat();
            find.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            find.ListProfileTujuan = new List<Models.Entities.Profile>();
            find.ListTipeSurat = persuratanmodel.GetTipeSurat();
            find.ListSifatSurat = persuratanmodel.GetSifatSurat();
            return View(find);
        }

        [HttpPost]
        public JsonResult InsertSuratPengantar(Models.Entities.FindPengantarSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }
            data.KantorId = satkerid;

            tr = persuratanmodel.InsertSuratPengantar(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuatSuratPengantar(List<Models.Entities.SuratIds> suratIds)
        {
            bool sukses = true;
            string pesan = string.Empty;
            string id = string.Empty;
            string nomorsp = "";

            if (suratIds != null)
            {
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                string satkerid = kantorid;
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                if (tipekantorid == 1)
                {
                    //satkerid = profileidtu;
                    satkerid = unitkerjaid;
                }

                try
                {
                    if (!string.IsNullOrEmpty(suratIds[0].profileidtujuan) && !string.IsNullOrEmpty(suratIds[0].namapenerima) && !string.IsNullOrEmpty(suratIds[0].tanggalinput))
                    {
                        Models.Entities.TransactionResult tr = persuratanmodel.BuatSuratPengantar(suratIds, tipekantorid, kantorid, satkerid, unitkerjaid, profileidtu, out nomorsp);

                        if (!tr.Status)
                        {
                            throw new Exception(tr.Pesan);
                        }

                        pesan = tr.Pesan;
                        id = tr.ReturnValue;
                    }
                    else
                    {
                        sukses = false;
                        pesan = "Isian wajib belum anda isi.";
                    }
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

        [HttpPost]
        public JsonResult UpdateSuratPengantar(Models.Entities.FindPengantarSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            tr = persuratanmodel.UpdateSuratPengantar(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarSuratPengantar(int? pageNum, Models.Entities.FindPengantarSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string metadata = f.Metadata;

            List<Models.Entities.PengantarSurat> result = persuratanmodel.GetSuratPengantar("", satkerid, metadata, from, to);

            int custIndex = from;
            Dictionary<int, Models.Entities.PengantarSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSuratPengantar", dict);
                }
                else
                {
                    return RedirectToAction("SuratPengantar", "Flow");
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

        public JsonResult GetSuratPengantarById(string id)
        {
            List<Models.Entities.PengantarSurat> result = persuratanmodel.GetSuratPengantar(id, "", "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarDetilPengantar()
        {
            List<Models.Entities.DetilPengantar> result = new List<Models.Entities.DetilPengantar>();
            decimal? total = 0;

            string pengantarsuratid = Request.Form["pengantarsuratid"].ToString();

            if (!String.IsNullOrEmpty(pengantarsuratid))
            {
                result = persuratanmodel.GetDetilPengantar(pengantarsuratid);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertDetilPengantar(Models.Entities.FindPengantarSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            //// Cek duplikat
            //string msg = string.Empty;
            //int cekrow = persuratanmodel.JumlahDetilPengantar(data.PengantarSuratId, data.SuratId);
            //if (cekrow > 0)
            //{
            //    msg = String.Concat("Surat tersebut sudah sudah ada.");
            //    return Json(new { Status = false, Pesan = msg }, JsonRequestBehavior.AllowGet);
            //}

            Models.Entities.Surat dataSurat = persuratanmodel.GetSuratBySuratId(data.SuratId, satkerid);

            data.TanggalSurat = dataSurat.TanggalSurat;
            data.Perihal = dataSurat.Perihal;
            data.Pengirim = dataSurat.PengirimSurat;
            data.SifatSurat = dataSurat.SifatSurat;
            data.KeteranganSurat = dataSurat.KeteranganSurat;
            data.Redaksi = dataSurat.Redaksi;

            tr = persuratanmodel.InsertDetilPengantar(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusDetilPengantar()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    result = persuratanmodel.HapusDetilPengantar(id);
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
        
        public JsonResult GetTipeSurat(string profileid, string arah)
        {
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                satkerid = unitkerjaid;
            }
            List<Models.Entities.TipeSurat> _list = persuratanmodel.GetTipeSurat(satkerid, pegawaiid, arah, profileid);
            return Json(_list, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SuratLamaHilang(string mssg)
        {

            if (!string.IsNullOrEmpty(mssg))
            {
                ViewBag.mssg = mssg;
            }
            else
            {
                ViewBag.mssg = "gagal";
            }
            return View();
        }


        public JsonResult NomorSuratCheckDuplicate(string nomor)
        {
            var nomorsurat = new Functions().TextEncode(nomor);
            List<Models.Entities.Surat> Surats = persuratanmodel.GetSuratidFromNomorSuratLike(nomorsurat);
            foreach(var surat in Surats)
            {
                surat.NomorSurat = HttpUtility.UrlDecode(surat.NomorSurat);
            }
            if(Surats.Count() > 0)
            {
                return Json(new { status = true, data = Surats }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}