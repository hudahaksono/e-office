using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Surat.Codes;
using System.Net.Http;
using System.Net;
using Surat.Models.Entities;
using System.Configuration;
using System.Threading.Tasks;
using System.Net.Mime;
using Newtonsoft.Json;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class AdminController : Controller
    {
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.PersuratanModel mPersuratan = new Models.PersuratanModel();
        Functions functions = new Functions();

        private string ConstructViewString(string viewName, object objModel, Dictionary<string, object> addViewData)
        {
            string strView = "";

            using (var sw = new StringWriter())
            {
                ViewData.Model = objModel;
                if (addViewData != null)
                {
                    foreach (string ky in addViewData.Keys)
                    {
                        ViewData[ky] = addViewData[ky];
                    }
                }

                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewCtx = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewCtx, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
                strView = sw.ToString();
            }

            return strView;
        }

        //[AccessDeniedAuthorize(Roles = "Administrator")]
        public ActionResult SettingEkspedisiSurat()
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            var listProfile = new List<Profile>();
            if (tipekantorid == 1)
            {
                // Pusat
                listProfile = dataMasterModel.GetProfilePusat();
            }
            else if (tipekantorid == 2)
            {
                // Kanwil
                listProfile = dataMasterModel.GetProfileKanwil();
            }
            else if (tipekantorid == 3 || tipekantorid == 4)
            {
                // Kantah/Perwakilan
                listProfile = dataMasterModel.GetProfileKantah();
            }

            var find = new FindProfileFlow();
            find.ListProfile = listProfile;

            return View(find);
        }

        public ActionResult KonfigurasiJabatan()
        {
            var find = new FindJabatan();
            find.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            find.ListProfileTU = new List<Profile>();
            find.ListProfileBA = new List<Profile>();

            return View(find);
        }

        public ActionResult KonfigurasiAsalSurat()
        {
            return View(new FindAsalSurat());
        }

        public ActionResult KonfigurasiSifatSurat()
        {
            return View(new SifatSurat());
        }

        public ActionResult KonfigurasiKlasifikasiArsip()
        {
            return View(new FindKlasifikasiArsip());
        }

        public ActionResult KonfigurasiKonterSurat()
        {
            return View(new FindKonterSurat());
        }

        //[AccessDeniedAuthorize(Roles = "Administrator")]
        public ActionResult KonfigurasiProfileTU()
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            var listProfile = new List<Profile>();
            if (tipekantorid == 1)
            {
                // Pusat
                listProfile = dataMasterModel.GetProfilePusat();
            }
            else if (tipekantorid == 2)
            {
                // Kanwil
                listProfile = dataMasterModel.GetProfileKanwil();
            }
            else if (tipekantorid == 3 || tipekantorid == 4)
            {
                // Kantah/Perwakilan
                listProfile = dataMasterModel.GetProfileKantah();
            }

            var find = new FindProfileTataUsaha();
            find.ListProfile = listProfile;

            return View(find);
        }

        //[AccessDeniedAuthorize(Roles = "Administrator")]
        public ActionResult KonfigurasiUnitKerja()
        {
            ViewBag.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true, false, null, true);
            return View();
        }

        //[AccessDeniedAuthorize(Roles = "Administrator")]
        public ActionResult UserLogin()
        {
            var usr = functions.claimUser();
            string unitkerjaid = usr.UnitKerjaId;

            ViewBag.UnitKerjaId = unitkerjaid;

            var listProfile = new List<Profile>();

            var find = new FindUserLogin();
            find.ListUnitKerja = dataMasterModel.GetListUnitKerja(unitkerjaid, "", "", true, true);
            find.ListProfile = listProfile;

            return View(find);
        }

        public ActionResult UserPPNPN()
        {
            var usr = functions.claimUser();
            string unitkerjaid = usr.UnitKerjaId;

            ViewBag.UnitKerjaId = unitkerjaid;

            var listProfile = new List<Profile>();

            var find = new FindUserLogin();
            find.ListUnitKerja = dataMasterModel.GetListUnitKerja(unitkerjaid, "", "", true,true);
            find.ListProfile = listProfile;

            return View(find);
        }

        //public ActionResult HakAkses()
        //{
        //    return View();
        //}

        //public ActionResult SetAdminSatker()
        //{
        //    SetAdminSatker data = new SetAdminSatker();
        //    return View(data);
        //}

        //[HttpPost]
        //public JsonResult SimpanAdminSatker(SetAdminSatker data)
        //{
        //    var tr = new TransactionResult() { Status = false, Pesan = "" };

        //    tr = dataMasterModel.SimpanAdminSatker(data);

        //    return Json(tr, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult SinkronisasiPegawai()
        //{
        //    SetAdminSatker data = new SetAdminSatker();

        //    return View(data);
        //}

        //public ActionResult GantiPassword()
        //{
        //    GantiPassword data = new GantiPassword();

        //    return View(data);
        //}

        //public ActionResult ExecuteSql()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public JsonResult DoExecuteSql(string textcommand)
        //{
        //    var tr = new TransactionResult() { Status = false, Pesan = "" };

        //    tr = dataMasterModel.DoExecuteSql(textcommand);

        //    return Json(tr, JsonRequestBehavior.AllowGet);
        //}

        //public ContentResult GetSqlQuery(string textcommand)
        //{
        //    string result = dataMasterModel.GetSqlQuery(textcommand);

        //    return Content(result);
        //}

        public JsonResult GetPegawaiByNamaAtauNip(string nip, string nama)
        {
            string pegawaiid = dataMasterModel.GetPegawaiIdFromNamaAtauNip(nama, nip);

            var record = dataMasterModel.GetPegawaiByPegawaiId(pegawaiid);

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPegawaiSimpegByNip(string pegawaiid)
        {
            pegawaiid = pegawaiid.Replace(" ", "").Trim();
            var record = dataMasterModel.GetPegawaiSimpegByNip(pegawaiid);

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProfilesByPegawaiId(string pegawaiid)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;

            var listProfile = dataMasterModel.GetProfilesByPegawaiId(pegawaiid, kantorid);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllProfilesByPegawaiId(string pegawaiid)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;

            var listProfile = dataMasterModel.GetAllProfilesByPegawaiId(pegawaiid, kantorid);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProfilesByUnitKerja(string unitkerjaid)
        {
            var listProfile = dataMasterModel.GetProfilesByUnitKerja(unitkerjaid);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProfilesPlusIDByUnitKerja(string unitkerjaid)
        {
            var listProfile = dataMasterModel.GetProfilesPlusIDByUnitKerja(unitkerjaid);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProfilePPNPNByUnitKerja(string unitkerjaid)
        {
            var listProfile = dataMasterModel.GetProfilePPNPNByUnitKerja(unitkerjaid);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProfilesByNama(string namaprofile)
        {
            var listProfile = dataMasterModel.GetProfilesByNama(namaprofile);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPegawaiByNama(string namapegawai)
        {
            var listPegawai = new List<Pegawai>();
            if (!string.IsNullOrEmpty(namapegawai))
            {
                listPegawai = dataMasterModel.GetPegawaiByNama(namapegawai);
            }
            return Json(listPegawai, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListPegawaiByNama()
        {
            var result = new List<Pegawai>();
            decimal? total = 0;

            string namapegawai = Request.Form["namapegawai"].ToString();

            if (!string.IsNullOrEmpty(namapegawai))
            {
                result = dataMasterModel.GetPegawaiByNama(namapegawai);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPegawaiByUnitKerjaJabatanNama()
        {
            var result = new List<Pegawai>();
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
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }




        #region Profile Flow

        //public ActionResult DaftarProfileFlow(int? pageNum, FindProfileFlow f)
        //{
        //    int pageNumber = pageNum ?? 0;
        //    int RecordsPerPage = 20;
        //    int from = (pageNumber * RecordsPerPage) + 1;
        //    int to = from + RecordsPerPage - 1;

        //    string kantorid = (HttpContext.User.Identity as InternalUserIdentity).KantorId;

        //    string namaProfileDari = f.NamaProfileDari;
        //    string namaProfileTujuan = f.NamaProfileTujuan;

        //    List<ProfileFlow> result = dataMasterModel.GetProfileFlow(kantorid, namaProfileDari, namaProfileTujuan, from, to);

        //    int custIndex = from;
        //    Dictionary<int, ProfileFlow> dict = result.ToDictionary(x => custIndex++, x => x);

        //    if (result.Count > 0)
        //    {
        //        if (Request.IsAjaxRequest())
        //        {
        //            return PartialView("DaftarProfileFlow", dict);
        //        }
        //        else
        //        {
        //            return RedirectToAction("SettingEkspedisiSurat", "Flow");
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

        //[HttpPost]
        //public JsonResult SimpanProfileFlow(ProfileFlow profileflow)
        //{
        //    var tr = new TransactionResult() { Status = false, Pesan = "" };

        //    string kantorid = (HttpContext.User.Identity as InternalUserIdentity).KantorId;

        //    profileflow.KantorId = kantorid;

        //    // Cek duplikat
        //    string msg = string.Empty;
        //    int cekrow = dataMasterModel.JumlahProfileFlow(profileflow.ProfileDari, profileflow.ProfileTujuan, kantorid);
        //    if (cekrow > 0)
        //    {
        //        msg = String.Concat("Setting disposisi dari profile ", profileflow.NamaProfileDari, " ke ", profileflow.NamaProfileTujuan, " sudah ada.");
        //        return Json(new { Status = false, Pesan = msg }, JsonRequestBehavior.AllowGet);
        //    }

        //    tr = dataMasterModel.SimpanProfileFlow(profileflow);

        //    return Json(tr, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult HapusProfileFlowById()
        //{
        //    var result = new TransactionResult() { Status = false, Pesan = "" };
        //    try
        //    {
        //        string id = Request.Form["id"].ToString();
        //        if (!String.IsNullOrEmpty(id))
        //        {
        //            result = dataMasterModel.HapusProfileFlowById(id);
        //            if (!result.Status)
        //            {
        //                return Json(result, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Status = false;
        //        result.Pesan = ex.Message;
        //    }

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        #endregion


        #region List Jabatan

        public ActionResult DaftarJabatan(int? pageNum, FindJabatan f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            string profileId = f.CariProfileId;
            string namaProfile = f.CariNamaProfile;
            string unitkerjaId = f.CariUnitKerjaId;

            var result = dataMasterModel.GetJabatan(profileId, namaProfile, unitkerjaId, from, to);

            int custIndex = from;
            Dictionary<int, ListJabatan> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarJabatan", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiJabatan", "Flow");
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

        public JsonResult GetJabatanByProfileId(string profileid)
        {
            var result = dataMasterModel.GetJabatan(profileid, "", "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateJabatan(ListJabatan data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                var usr = functions.claimUser();
                string userid = usr.UserId;
                data.UserId = userid;

                tr = dataMasterModel.UpdateJabatan(data);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Asal Surat

        public ActionResult DaftarAsalSurat(int? pageNum, FindAsalSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string namaAsalSurat = string.IsNullOrEmpty(f.CariNamaAsalSurat) ? string.Empty : Server.UrlEncode(f.CariNamaAsalSurat);

            var result = dataMasterModel.GetListAsalSurat(namaAsalSurat, from, to);

            int custIndex = from;
            Dictionary<int, ListAsalSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarAsalSurat", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiAsalSurat", "Flow");
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

        public JsonResult GetAsalSurat()
        {
            var result = dataMasterModel.GetAsalSurat();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertAsalSurat(ListAsalSurat data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.InsertAsalSurat(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusAsalSurat()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string namaasalsurat = Request.Form["namaasalsurat"].ToString();
                if (!string.IsNullOrEmpty(namaasalsurat))
                {
                    result = dataMasterModel.HapusAsalSurat(namaasalsurat);
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


        #region sifatsurat

        public ActionResult DaftarSifatSurat(int? pageNum, SifatSurat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string namaSifatSurat = string.IsNullOrEmpty(f.CariSifatSurat) ? string.Empty : Server.UrlEncode(f.CariSifatSurat);

            var result = dataMasterModel.GetListSifatSurat(namaSifatSurat, from, to);

            int custIndex = from;
            Dictionary<int, SifatSurat> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSifatSurat", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiSifatSurat", "Flow");
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
        public JsonResult InsertSifatSurat(SifatSurat data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.InsertSifatSurat(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusSifatSurat()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string namasifatsurat = Request.Form["namasifatsurat"].ToString();
                if (!string.IsNullOrEmpty(namasifatsurat))
                {
                    result = dataMasterModel.HapusSifatSurat(namasifatsurat);
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


        #region Klasifikasi Arsip

        public JsonResult GetListKlasifikasiArsip()
        {
            var result = dataMasterModel.GetListKlasifikasiArsip();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarKlasifikasiArsip(int? pageNum, FindKlasifikasiArsip f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string kodeKlasifikasi = string.IsNullOrEmpty(f.CariKodeKlasifikasi) ? string.Empty : Server.UrlEncode(f.CariKodeKlasifikasi);
            string jenisArsip = string.IsNullOrEmpty(f.CariJenisArsip) ? string.Empty : Server.UrlEncode(f.CariJenisArsip);
            string keterangan = string.IsNullOrEmpty(f.CariKeterangan) ? string.Empty : Server.UrlEncode(f.CariKeterangan);

            var result = dataMasterModel.GetKlasifikasiArsip(kodeKlasifikasi, jenisArsip, keterangan, from, to);

            int custIndex = from;
            Dictionary<int, KlasifikasiArsip> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarKlasifikasiArsip", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiKlasifikasiArsip", "Flow");
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
        public JsonResult InsertKlasifikasiArsip(KlasifikasiArsip data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.InsertKlasifikasiArsip(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusKlasifikasiArsip()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string kodeklasifikasi = Request.Form["kodeklasifikasi"].ToString();
                if (!string.IsNullOrEmpty(kodeklasifikasi))
                {
                    result = dataMasterModel.HapusKlasifikasiArsip(kodeklasifikasi);
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


        #region Profile Tata Usaha

        public ActionResult DaftarProfileTU(int? pageNum, FindProfileTataUsaha f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            string profileId = f.CariProfileId;
            string namaProfile = f.CariNamaProfile;
            string namaProfileTU = f.CariNamaProfileTU;

            var result = dataMasterModel.GetProfileTataUsaha(tipekantorid, profileId, namaProfile, namaProfileTU, from, to);

            int custIndex = from;
            Dictionary<int, ProfileTataUsaha> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarProfileTU", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiProfileTU", "Flow");
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

        public JsonResult GetProfileTUByProfileId(string profileid)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            var result = dataMasterModel.GetProfileTataUsaha(tipekantorid, profileid, "", "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateProfileTU(ProfileTataUsaha data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.UpdateProfileTU(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Unit Kerja

        public ActionResult DaftarUnitKerja(int? pageNum, FindUnitKerja f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            string unitkerjaid = f.CariUnitKerjaId;
            string namaunitkerja = f.CariNamaUnitKerja;
            string kode = f.CariKode;
            bool issatkerpusat = f.IsSatkerPusat;
            bool issatkerkanwil = f.IsSatkerKanwil;
            bool issatkerkantah = f.IsSatkerKantah;
            string tampil = f.CariTampil;


            var result = dataMasterModel.GetUnitKerja(unitkerjaid, namaunitkerja, kode, issatkerpusat, issatkerkanwil, issatkerkantah, tampil, from, to);

            int custIndex = from;
            Dictionary<int, UnitKerja> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarUnitKerja", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiProfileTU", "Flow");
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

        public JsonResult UpdateStatusUnitKerja(string unitkerjaid, string tampil)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.UpdateStatusUnitKerja(unitkerjaid, tampil);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateUnitKerja(string unitkerjaid, string namaunitkerja, string kode, string tampil, string induk)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.UpdateUnitKerja(unitkerjaid, namaunitkerja, kode, tampil, induk:induk);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnitKerjaById(string unitkerjaid)
        {
            var result = dataMasterModel.GetUnitKerja(unitkerjaid, "", "", true, true, true, "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarProfile()
        {
            var result = new List<Profile>();
            decimal? total = 0;

            string jeniskantor = Request.Form["jeniskantor"].ToString();

            if (!string.IsNullOrEmpty(jeniskantor))
            {
                if (jeniskantor == "Pusat")
                {
                    result = dataMasterModel.GetProfilePusat();
                }
                else if (jeniskantor == "Kanwil")
                {
                    result = dataMasterModel.GetProfileKanwil();
                }
                else if (jeniskantor == "Kantah")
                {
                    result = dataMasterModel.GetProfileKantah();
                }

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region User Login

        public ActionResult DaftarUserLogin(int? pageNum, FindUserLogin f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string nip = f.CariNip;
            string nama = f.CariNama;
            string jabatan = f.CariJabatan;
            string satker = f.CariSatker;

            string unitkerjaid = usr.UnitKerjaId;

            var result = dataMasterModel.GetListUserLogin(nip, nama, jabatan, satker, unitkerjaid, from, to);

            int custIndex = from;
            Dictionary<int, UserLogin> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarUserLogin", dict);
                }
                else
                {
                    return RedirectToAction("UserLogin", "Admin");
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

        public JsonResult GetUserLoginByNip(string nip)
        {
            var result = dataMasterModel.GetUserLogin(nip, "", "", "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateUserLogin(UserLogin userlogin)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.UpdateUserLogin(userlogin);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SinkronisasiUser(UserLogin userlogin)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.SinkronisasiUser(userlogin);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region User PPNPN

        public ActionResult DaftarUserPPNPN(int? pageNum, FindUserLogin f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string nik = f.CariNik;
            string nama = f.CariNama;
            string satker = f.CariSatker;

            string unitkerjaid = usr.UnitKerjaId;

            var result = dataMasterModel.GetListUserPPNPN(nik, nama, satker, unitkerjaid, from, to);

            int custIndex = from;
            Dictionary<int, UserPPNPN> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarUserPPNPN", dict);
                }
                else
                {
                    return RedirectToAction("UserPPNPN", "Admin");
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

        public JsonResult GetUserPPNPNByNik(string nik)
        {
            var result = dataMasterModel.GetUserPPNPN(nik, "", "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateUserPPNPN(UserPPNPN userppnpn)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.UpdateUserPPNPN(userppnpn);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region User Profiles

        public ActionResult DaftarProfilePegawai()
        {
            var result = new List<ProfilePegawai>();
            decimal? total = 0;

            string nip = Request.Form["nip"].ToString();

            if (!string.IsNullOrEmpty(nip))
            {
                result = dataMasterModel.GetAllProfilePegawai(nip);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarJabatanPegawai()
        {
            var result = new List<ProfilePegawai>();
            decimal? total = 0;
            var usr = functions.claimUser();

            string nip = Request.Form["nip"].ToString();

            string kantorid = string.Empty;
            if (!OtorisasiUser.IsRoleAdministrator())
            {
                kantorid = usr.KantorId;
            }

            if (!String.IsNullOrEmpty(nip))
            {
                result = dataMasterModel.GetJabatanPegawai(nip,kantorid);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult InsertProfilePegawai(UserLogin userlogin)
        //{
        //    var tr = new TransactionResult() { Status = false, Pesan = "" };

        //    string kantorid = (HttpContext.User.Identity as InternalUserIdentity).KantorId;


        //    if (OtorisasiUser.IsRoleAdministrator() == true)
        //    {
        //        string _kantorid = dataMasterModel.GetKantorIdFromUnitKerjaId(userlogin.Satker);
        //        kantorid = (string.IsNullOrEmpty(_kantorid) ? kantorid : _kantorid);
        //    }

        //    // Cek duplikat
        //    string msg = string.Empty;
        //    int cekrow = dataMasterModel.JumlahProfilePegawai(userlogin.PegawaiId, userlogin.ProfileId, kantorid);
        //    if (cekrow > 0)
        //    {
        //        msg = String.Concat("Posisi ", userlogin.Jabatan, " untuk pegawai ", userlogin.NamaLengkap, " sudah ada.");
        //        return Json(new { Status = false, Pesan = msg }, JsonRequestBehavior.AllowGet);
        //    }

        //    tr = dataMasterModel.InsertProfilePegawai(userlogin, kantorid);

        //    return Json(tr, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult HapusProfilePegawai()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            try
            {
                string userid = usr.UserId;
                string id = Request.Form["id"].ToString();
                if (!string.IsNullOrEmpty(id))
                {
                    result = dataMasterModel.HapusProfilePegawai(id, userid);
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


        #region Hak Akses

        [HttpPost]
        public JsonResult DaftarHakAkses(string uname, string dnip, string nl, string nip, string jabatan, string satker, string kantorid, List<Kantor> kantorids)
        {
            string strlsthakakses = string.Empty;
            try
            {
                var result = dataMasterModel.ListProfilPengguna(kantorid, nip);
                int custIndex = 1;
                Dictionary<int, ProfilPengguna> hadic = result.ToDictionary(x => custIndex++, x => x);
                Dictionary<string, object> addViewData = new Dictionary<string, object>();
                addViewData.Add("dnip", dnip);
                addViewData.Add("nip", nip);
                addViewData.Add("namalengkap", nl);
                addViewData.Add("uname", uname);
                addViewData.Add("jabatan", jabatan);
                addViewData.Add("satker", satker);
                addViewData.Add("kantorids", kantorids);
                addViewData.Add("kantorid", kantorid);
                strlsthakakses = ConstructViewString("ListHakAkses", hadic, addViewData);
            }
            catch (Exception ex)
            {
                strlsthakakses = "Error " + ex.Message;
            }

            return Json(new { ListHA = strlsthakakses }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CariPengguna(string q)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            q = HttpUtility.UrlDecode(q);
            List<DataPengguna> ldp = dataMasterModel.ListPengguna(q, kantorid, unitkerjaid);

            return Json(new { results = ldp }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CariPegawai()
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            string unitkerjaid = usr.UnitKerjaId;
            List<DataPengguna> ldp = dataMasterModel.ListPengguna(kantorid, unitkerjaid);

            return Json(ldp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CariDetailPegawai(string nip)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;

            var _rst = dataMasterModel.DetailPengguna(nip, kantorid);

            return Json(_rst, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
      //public JsonResult SimpanHakAkses(string uname, string dnip, string nl, string nip, string jabatan, string satker, string kantorid, List<Kantor> kantorids)
        public JsonResult SimpanHakAkses(string jdata, string u, string nip, string nl, string dnip, string kid)
        {
            var usr = functions.claimUser();
            var tr = dataMasterModel.MergeHakAkses(jdata, nip, usr.UserId, kid);
            bool status = tr.Status;
            string msg = tr.Pesan;
            string strlsthakakses = string.Empty;

            try
            {
                // List Hakakses
                //var lstha = dataMasterModel.ListProfilPengguna(useridentity.KantorId, nip);
                var _rst = dataMasterModel.DetailPengguna(nip, kid);
                var lstha = dataMasterModel.ListProfilPengguna(kid, nip);
                int custIndex = 1;
                Dictionary<int, ProfilPengguna> hadic = lstha.ToDictionary(x => custIndex++, x => x);
                Dictionary<string, object> addViewData = new Dictionary<string, object>();
                addViewData.Add("nip", nip);
                addViewData.Add("dnip", dnip);
                addViewData.Add("namalengkap", nl);
                addViewData.Add("uname", u);
                addViewData.Add("jabatan", _rst.namajabatan);
                addViewData.Add("satker", _rst.namasatker);
                addViewData.Add("kantorids", _rst.kantorids);
                addViewData.Add("kantorid", _rst.kantorid);
                strlsthakakses = ConstructViewString("ListHakAkses", hadic, addViewData);
            }
            catch (Exception ex)
            {
                status = false;
                msg = "Error " + ex.Message;
            }

            return Json(new { Status = status, Message = msg, LHA = strlsthakakses }, JsonRequestBehavior.AllowGet);
        }

        public ContentResult IsPembuatNomorAgendaRole()
        {
            string result = "0";
            if (OtorisasiUser.IsPembuatNomorAgendaRole())
            {
                result = "1";
            }

            return Content(result);
        }

        public ContentResult IsPembuatNomorSuratRole()
        {
            string result = "0";
            if (OtorisasiUser.IsPembuatNomorSuratRole())
            {
                result = "1";
            }

            return Content(result);
        }

        #endregion

        public ActionResult KonfigurasiDelegasi()
        {
            FindDelegasi find = new FindDelegasi();
            find.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
            find.ListProfilePengirim = new List<Profile>();
            find.ListProfilePenerima = new List<Profile>();
            return View(find);
        }


        #region List Delegasi

        public ActionResult DaftarDelegasi(int? pageNum, FindDelegasi f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;
            var usr = functions.claimUser();

            string kantorid = usr.KantorId;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            string DelegasiSuratId = f.CariDelegasiSuratId;
            string ProfilePengirim = f.CariProfilePengirim;
            string ProfilePenerima = f.CariProfilePenerima;

            var result = dataMasterModel.GetDelegasi(DelegasiSuratId, ProfilePengirim, ProfilePenerima, from, to);

            int custIndex = from;
            Dictionary<int, ListDelegasi> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarDelegasi", dict);
                }
                else
                {
                    return RedirectToAction("KonfigurasiDelegasi", "Admin");
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

        public JsonResult GetDelegasiById(string id)
        {
            var result = dataMasterModel.GetDelegasi(id, "", "", 1, 1);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateDelegasi(ListDelegasi data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            tr = dataMasterModel.UpdateDelegasi(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult IndexUserAkses()
        {
            if (OtorisasiUser.IsProfile("AdminSatker") || OtorisasiUser.IsProfile("Administrator") || OtorisasiUser.isTU())
            {
                var usr = functions.claimUser();

                ViewBag.UnitKerjaId = usr.UnitKerjaId;
                ViewBag.IsAdmin = OtorisasiUser.IsRoleAdministrator();
                ViewBag.Tipe = dataMasterModel.GetTipeUser(usr.PegawaiId, usr.KantorId).ReturnValue;

                var listProfile = new List<Profile>();

                var find = new FindUserLogin();
                find.ListUnitKerja = dataMasterModel.GetListUnitKerja(usr.UnitKerjaId, "", "", true, true);
                find.ListProfile = listProfile;

                return View(find);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public JsonResult ListPegawai()
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            List<DataPengguna> ldp = dataMasterModel.ListPegawai(kantorid);

            return new JsonResult()
            {
                Data = ldp,
                MaxJsonLength = Int32.MaxValue // Use this value to set your maximum size for all of your Requests
            };
        }

        public ActionResult DaftarProfilesPegawai() // Arya :: 2020-07-28
        {
            var result = new List<ProfilePegawai>();
            decimal? total = 0;

            string nip = Request.Form["nip"].ToString();
            string kantorid = Request.Form["kantorid"].ToString();

            if (!String.IsNullOrEmpty(nip))
            {
                result = dataMasterModel.GetProfilesPegawai(nip, kantorid);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPegawaiSimpegDetail(string pegawaiid) // Arya :: 2020-07-28
        {
            var usr = functions.claimUser();
            string kantorid = string.Empty;
            if (!OtorisasiUser.IsRoleAdministrator())
            {
                kantorid = usr.KantorId;
            }
            PegawaiSimpeg record = dataMasterModel.GetPegawaiSimpegDetail(pegawaiid, kantorid);

            if (record == null)
            {
                return Json("noresult", JsonRequestBehavior.AllowGet);
            }

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AktifasiUser(string nip, string kid, string kgt)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };
            var usr = functions.claimUser();
            if (kid != dataMasterModel.getSimpegKantorId(nip, "").KantorId && !OtorisasiUser.IsRoleAdministrator() && (kgt == null || !kgt.Equals("HapusJabatan")))
            {
                // User sudah tidak bertugas di satker
                tr.Status = false;
                tr.ReturnValue = "HapusJabatan";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            string userid = usr.UserId;

            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                tr = dataMasterModel.SynchUser(nip, kid, userid, kgt);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AktifasiPPNPN(string nik)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                tr = dataMasterModel.AktifasiPPNPN(nik, usr.UserId, usr.KantorId, usr.UnitKerjaId, OtorisasiUser.IsRoleAdministrator());
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult NonAktifasiPPNPN(string nik)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string userid = usr.UserId;
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                tr = dataMasterModel.NonAktifasiPPNPN(nik, userid);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cekSimpegkantor(string nip)
        {
            var usr = functions.claimUser();
            string kid = usr.KantorId;
            Kantor kis = dataMasterModel.getSimpegKantorId(nip, kid);
            return Json(kis, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListUnitKerjaByKantorId(string kid)
        {
            var listProfile = dataMasterModel.GetListUnitKerjaByKantorId(kid,true,true);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddProfilePegawai(string nip, string unit, string pid, string tjt, string jbt, string nap, string vsk, string vsp)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                var usr = functions.claimUser();
                // Cek duplikat
                string msg = string.Empty;
                string kid = dataMasterModel.GetKantorIdByUnitKerjaId(unit);
                int cekrow = dataMasterModel.JumlahProfilePegawai(nip, pid, kid);
                if (cekrow > 0)
                {
                    msg = string.Concat("Posisi ", jbt, " untuk pegawai ", nap, " sudah ada.");
                    return Json(new { Status = false, Pesan = msg }, JsonRequestBehavior.AllowGet);
                }
                var userlogin = new UserLogin();
                userlogin.ProfileId = pid;
                userlogin.PegawaiId = nip;
                userlogin.IsStatusPlt = !tjt.Equals("0");
                userlogin.TipeJabatan = tjt;
                userlogin.Jabatan = jbt;
                userlogin.UserId = usr.UserId;


                HttpPostedFileBase mfile = Request.Files[0];
                if (mfile == null || mfile.ContentType != "application/pdf")
                {
                    return Json(new { Status = false, Pesan = "File harus pdf" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "AksesKKP";
                    string jpid = mPersuratan.GetUID();
                    content.Add(new StringContent(kid), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(jpid), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent("0"), "versionNumber");
                    content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);
                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            int tipekantorid = dataMasterModel.GetTipeKantor(usr.KantorId);
                            if (tipekantorid > 2)
                            {
                                if (kid.Equals(usr.KantorId))
                                {
                                    tr = dataMasterModel.InsertProfilePegawai(userlogin, kid, jpid, vsk, vsp);
                                }
                                else
                                {
                                    tr = dataMasterModel.InsertPengajuanJabatanPegawai(userlogin, kid, jpid, vsk, vsp);
                                }
                            }
                            else
                            {
                                tr = dataMasterModel.InsertProfilePegawai(userlogin, kid, jpid, vsk, vsp);
                            }
                        }
                        else
                        {
                            return Json(new { Status = false, Pesan = string.Concat("Gagal Upload ", reqresult.ReasonPhrase.ToString()) }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTipeUser(string nr) // Arya :: 2020-09-10
        {
            string kantorid = string.Empty;
            var usr = functions.claimUser();
            nr = nr.Replace(" ", "").Trim();
            if (!OtorisasiUser.IsRoleAdministrator())
            {
                kantorid = usr.KantorId;
            }
            var tr = dataMasterModel.GetTipeUser(nr, kantorid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPPNPNDetail(string nik) // Arya :: 2020-07-28
        {
            string kantorid = string.Empty;
            var usr = functions.claimUser();
            nik = nik.Replace(" ", "").Trim();
            if (!OtorisasiUser.IsRoleAdministrator())
            {
                kantorid = usr.KantorId;
            }
            UserPPNPN record = dataMasterModel.GetPPNPNDetail(nik, kantorid);

            if (record == null)
            {
                return Json("noresult", JsonRequestBehavior.AllowGet);
            }

            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAksesKhusus(string pid, string kid)
        {
            var result = dataMasterModel.GetAksesKhusus(pid, kid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetHakAkses(string pid, string kid, bool vA81001, bool vA81002, bool vA81003, bool vA81004, bool vA80100, bool vA80300)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string userid = usr.UserId;
            string unitkerjaid = usr.UnitKerjaId;
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                tr = dataMasterModel.SetHakAkses(pid, kid, userid, unitkerjaid, OtorisasiUser.IsRoleAdministrator(), vA81001, vA81002, vA81003, vA81004, vA80100, vA80300);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateAkunSaya(string tip, string tlp, string pss)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string userid = usr.UserId;
            string pegawaiid = usr.PegawaiId;
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                tr = dataMasterModel.UpdateAkunSaya(userid, pegawaiid, tip, tlp, pss);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult UserPresensi()
        //{
        //    var usr = functions.claimUser();
        //    string unitkerjaid = usr.UnitKerjaId;

        //    ViewBag.UnitKerjaId = unitkerjaid;
        //    ViewBag.IsAdmin = OtorisasiUser.IsRoleAdministrator();

        //    var listProfile = new List<Profile>();

        //    var find = new FindUserLogin();
        //    find.ListUnitKerja = dataMasterModel.GetListUnitKerja(unitkerjaid, "", "", true, true);
        //    find.ListProfile = listProfile;

        //    return View(find);
        //}

        //[HttpPost]
        //public JsonResult HapusFotoPresensi(string pid)
        //{
        //    var tr = new TransactionResult() { Status = false, Pesan = "" };
        //    if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
        //    {
        //        tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
        //    }
        //    else
        //    {
        //        var reqmessage = new HttpRequestMessage();
        //        var content = new MultipartFormDataContent();
        //        if (string.IsNullOrEmpty(pid))
        //        {
        //            tr.Status = false;
        //            tr.Pesan = "NIP/NIK tidak ditemukan";
        //            return Json(tr, JsonRequestBehavior.AllowGet);
        //        }
        //        reqmessage.Method = HttpMethod.Delete;
        //        reqmessage.Content = content;
        //        reqmessage.RequestUri = new Uri(string.Concat("http://10.20.21.21/facefr/", pid));

        //        using (var client = new HttpClient())
        //        {
        //            var reqresult = client.SendAsync(reqmessage).Result;
        //            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
        //            {
        //                tr.Status = true;
        //                tr.Pesan = "Foto Presensi Berhasil Dihapus";
        //            }
        //            else
        //            {
        //                tr.Status = false;
        //                throw new Exception(reqresult.ReasonPhrase);
        //            }
        //        }
        //    }
        //    return Json(tr, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult GetImagePersesi(string nip)
        //{
        //    string res = Url.Content("~/resources/images/avatar.png");
        //    if (link.CheckLink(string.Concat("http://10.20.21.21/facefr/listface/album/", nip, "/img.jpg")))
        //    {
        //        res = string.Concat("https://mobileeoffice.atrbpn.go.id/facefr/listface/album/", nip, "/img.jpg");
        //    }
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult GetImagePersesiStream(string nip)
        //{
        //    string res = Url.Content("~/resources/images/avatar.png");
        //    if (link.CheckLink(string.Concat("http://10.20.21.21/facefr/listface/album/", nip, "/img.jpg")))
        //    {
        //        res = string.Concat("https://mobileeoffice.atrbpn.go.id/facefr/listface/album/", nip, "/img.jpg");
        //    }
        //    var img = new KontenController().WebsiteImage(res);
        //    return Json(img, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetTipeUserPresensi(string nr) // Arya :: 2020-09-10
        //{
        //    string kantorid = (HttpContext.User.Identity as InternalUserIdentity).KantorId;
        //    string unitkerjaid = (HttpContext.User.Identity as InternalUserIdentity).UnitKerjaId;
        //    var tr = dataMasterModel.GetTipeUser(nr, kantorid, unitkerjaid);

        //    return Json(tr, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult PengalihanSurat()
        {
            var usr = functions.claimUser();
            if (OtorisasiUser.isTU() || OtorisasiUser.IsAdminRole())
            {
                var data = new FindPengalihanSurat();
                string unitkerjaid = usr.UnitKerjaId;
                data.UnitKerjaId = unitkerjaid;

                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult getListJabatan(string ukid)
        {
            var usr = functions.claimUser();
            var profileTU = dataMasterModel.GetProfileIfTU(usr.PegawaiId,usr.KantorId);
            var result = new List<Profile>();
            if (profileTU.Count > 0)
            {
                foreach(var pr in profileTU)
                {
                    result.AddRange(mPersuratan.GetJabatanPunyaSuratPending(pr.UnitKerjaId));
                }
            }
            else
            {
                result = mPersuratan.GetJabatanPunyaSuratPending(ukid);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarSuratPending(int? draw, int? start, int? length, FindPengalihanSurat f)
        {
            var result = new List<ListSuratPending>();
            decimal? total = 0;
            string msg = string.Empty;

            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(f.ProfileId))
            {
                int indexStart = start ?? 0;
                int limit = length ?? 10;
                int from = (indexStart * limit) + 1;
                int to = from + limit - 1;
                var profileTU = dataMasterModel.GetProfileIfTU(usr.PegawaiId, usr.KantorId);
                var listUnitKerja = new List<string>();
                if (profileTU.Count > 0)
                {
                    foreach (var pr in profileTU)
                    {
                        listUnitKerja.Add(pr.UnitKerjaId);
                    }
                    result = mPersuratan.GetPejabatPunyaSuratPending(listUnitKerja, f.ProfileId, from, to);
                }
                else
                {
                    listUnitKerja.Add(f.UnitKerjaId);
                    result = mPersuratan.GetPejabatPunyaSuratPending(listUnitKerja, f.ProfileId, from, to);
                }
                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total, pesanError = msg }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getListPegawaiBaru(string ukid, string prid)
        {
            var usr = functions.claimUser();
            var profileTU = dataMasterModel.GetProfileIfTU(usr.PegawaiId, usr.KantorId);
            var result = new List<Profile>();
            if (profileTU.Count > 0)
            {
                foreach (var pr in profileTU)
                {
                    result.AddRange(dataMasterModel.GetPegawaiAktif(pr.UnitKerjaId,prid));
                }
            }
            else
            {
                result = dataMasterModel.GetPegawaiAktif(ukid, prid);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DoPeralihanSurat(string ukid, string prid, string opid, string npid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            if (string.IsNullOrEmpty(ukid))
            {
                tr.Pesan = "Unit Kerja Belum Dipilih";
            }else if (string.IsNullOrEmpty(prid))
            {
                tr.Pesan = "Jabatan Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(opid))
            {
                tr.Pesan = "Pegawai Lama Tidak Ditemukan";
            }
            else if (string.IsNullOrEmpty(npid))
            {
                tr.Pesan = "Pegawai Aktif Belum Dipilih";
            }
            else
            {
                var profileTU = dataMasterModel.GetProfileIfTU(usr.PegawaiId, usr.KantorId);
                var result = new List<Profile>();
                if (profileTU.Count > 0)
                {
                    foreach (var pr in profileTU)
                    {
                        tr = mPersuratan.DoPeralihanSurat(pr.UnitKerjaId, prid, opid, npid, usr.NamaPegawai);
                    }
                }
                else
                {
                    tr = mPersuratan.DoPeralihanSurat(ukid, prid, opid, npid, usr.NamaPegawai);
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LaporMasalah(string id, string txt)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            string userid = usr.UserId;
            if(txt.Length > 200)
            {
                tr.Pesan = "Laporan Terlalu Panjang [Max. 200 char]";
            }else if (string.IsNullOrEmpty(id))
            {
                tr.Pesan = "Module tidak ditemukan";
            }
            else if (string.IsNullOrEmpty(txt))
            {
                tr.Pesan = "Laporan Harus Diisi";
            }
            else
            {
                tr = dataMasterModel.LaporMasalah(id, HttpUtility.UrlEncode(txt), userid);
            }
            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> cekDokumen(string id, string tip, string x)
        {
            var result = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "0", ReturnValue2 = "terakhir" };
            var usr = functions.claimUser();
            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string version = "0";
                if (string.IsNullOrEmpty(x))
                {
                    var JP = dataMasterModel.getProfilePegawai(id);
                    kantorid = JP.KantorId;
                }
                else if (x.Equals("PengajuanAkses"))
                {
                    kantorid = dataMasterModel.getKantorIdFromPengajuanAkses(id);
                }
                else if (x.Equals("PengajuanJabatan"))
                {
                    kantorid = dataMasterModel.getKantorIdFromPengajuanJabatan(id);
                }
                else if (x.Equals("PersetujuanAkses"))
                {
                    version = "1";
                }
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tip), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(version), "versionNumber");

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
                            if(strm != null)
                            {
                                result.Status = true;
                            }
                            else
                            {
                                result.Pesan = "Dokumen tidak ditemukan";
                            }
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

        public async Task<ActionResult> getDokumen(string id, string tip, string x)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string filename = string.Empty;
                string version = "0";
                if (string.IsNullOrEmpty(x))
                {
                    var JP = dataMasterModel.getProfilePegawai(id);
                    kantorid = JP.KantorId;
                    filename = JP.NomorSK;
                }
                else if (x.Equals("PengajuanAkses"))
                {
                    kantorid = dataMasterModel.getKantorIdFromPengajuanAkses(id);
                }
                else if (x.Equals("PengajuanJabatan"))
                {
                    kantorid = dataMasterModel.getKantorIdFromPengajuanJabatan(id);
                }
                else if (x.Equals("PersetujuanAkses"))
                {
                    version = "1";
                }
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tip), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(version), "versionNumber");

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
                            docfile.FileDownloadName = string.Concat(string.IsNullOrEmpty(filename) ? "dokumen" : filename.Replace("/", " ").Replace(":", " "),".pdf");

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

        public ActionResult openSettingAksesKKP(string uid, string kid, string pid)
        {
            var data = new HakAksesKKP();
            data.ListTipe = new List<TipeKantor>();
            data.UserIdAkses = uid;
            data.KantorIdAkses = kid;
            data.PegawaiIdAkses = pid;

            var ListTipe = dataMasterModel.GetListTipeKantor();

            if (ListTipe.Count > 0)
            {
                data.ListTipe.Add(new TipeKantor() { TipeKantorId = 0, Tipe = "Semua Kantor" });
                foreach (var tipe in ListTipe)
                {
                    data.ListTipe.Add(tipe);
                }
                if (Request.IsAjaxRequest())
                {
                    return PartialView("SettingHakAksesKKP", data);
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
        public JsonResult loadListKantor(string uid, int tip, string aks)
        {
            if (!string.IsNullOrEmpty(aks))
            {
                aks = string.Concat("99000", aks);
            }
            return Json(dataMasterModel.GetListAksesKKP(uid, tip, aks), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult simpanPengajuanAkses(SimpanAkses data, string ListAkses)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            var oldList = dataMasterModel.GetListAksesKKP(data.UserId);
            if (!string.IsNullOrEmpty(data.TipeAkses))
            {
                data.TipeAkses = string.Concat("99000", data.TipeAkses);
            }
            try
            {
                dynamic json = JsonConvert.DeserializeObject(ListAkses);

                foreach (var js in json)
                {
                    data.ListAkses.Add(JsonConvert.DeserializeObject<KantorKKP>(JsonConvert.SerializeObject(js.Value)));
                }
            }
            catch
            {
                tr.Pesan = "Data yang dikirimkan tidak sesuai";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }
            var ListTrans = new List<KantorKKP>();
            int ct = 0;
            foreach (var lt in oldList)
            {
                if (lt.Status.Equals("A"))
                {
                    string st = "D";
                    foreach (var lt2 in data.ListAkses)
                    {
                        if (lt2.KantorId.Equals(lt.KantorId))
                        {
                            st = "U";
                        }
                    }
                    if (st.Equals("D"))
                    {
                        ct += 1;
                    }
                    ListTrans.Add(new KantorKKP()
                    {
                        KantorId = lt.KantorId,
                        NamaKantor = lt.NamaKantor,
                        Status = st
                    });
                }
            }
            foreach (var lt in data.ListAkses)
            {
                bool add = true;
                foreach (var lt2 in ListTrans)
                {
                    if (lt2.KantorId.Equals(lt.KantorId))
                    {
                        add = false;
                    }
                }
                if (add)
                {
                    ListTrans.Add(new KantorKKP()
                    {
                        KantorId = lt.KantorId,
                        NamaKantor = lt.NamaKantor,
                        Status = "I"
                    });
                    ct += 1;
                }
            }
            if(ListTrans.Count() > 0)
            {
                if (ct == 0)
                {
                    tr.Pesan = "Tidak ditemukan Perubahan Akses";
                }
                else
                {
                    data.ListAkses = ListTrans;

                    HttpPostedFileBase mfile = Request.Files[0];
                    if (mfile == null || mfile.ContentType != "application/pdf")
                    {
                        return Json(new { Status = false, Pesan = "File harus pdf" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var reqmessage = new HttpRequestMessage();
                        var content = new MultipartFormDataContent();
                        var tipe = "AksesKKP";
                        string id = mPersuratan.GetUID();
                        content.Add(new StringContent(usr.KantorId), "kantorId");
                        content.Add(new StringContent(tipe), "tipeDokumen");
                        content.Add(new StringContent(id), "dokumenId");
                        content.Add(new StringContent(".pdf"), "fileExtension");
                        content.Add(new StringContent("0"), "versionNumber");
                        content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);
                        reqmessage.Method = HttpMethod.Post;
                        reqmessage.Content = content;
                        reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                        using (var client = new HttpClient())
                        {
                            var reqresult = client.SendAsync(reqmessage).Result;
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                            {
                                int eselon = dataMasterModel.geteselon(data.PegawaiId);
                                tr = dataMasterModel.KirimPengajuan(usr.UserId, "H1601000", "AksesKKP", usr.KantorId, data, id);
                            }
                            else
                            {
                                return Json(new { Status = false, Pesan = string.Concat("Gagal Upload ", reqresult.ReasonPhrase.ToString()) }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            else
            {
                tr.Pesan = "Tidak ditemukan Perubahan Akses";
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDetailPengajuanAkses(int? draw, int? start, int? length, string dokid)
        {
            var result = new List<DetailAksesKKP>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;

            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                if (!string.IsNullOrEmpty(dokid))
                {
                    result = dataMasterModel.getListHakAkses(dokid, from, to);
                }

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPegawaiDetail(string pegawaiid)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            pegawaiid = pegawaiid.Replace(" ", "").Trim();
            var tr = dataMasterModel.GetTipeUser(pegawaiid, kantorid);
            var data = new DetailAkun();
            if (tr.Status)
            {
                data = dataMasterModel.GetDetailAkun(pegawaiid, kantorid, tr.ReturnValue);
            }
            else
            {
                data.PesanError = tr.Pesan;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IndexAksesKKP()
        {
            if (OtorisasiUser.isTU())
            {
                var find = new FindUserLogin();

                return View(find);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult DaftarProfileKKP(int? draw, int? start, int? length, string nip)
        {
            var result = new List<ProfilePegawai>();
            var usr = functions.claimUser();
            decimal? total = 0;

            string kantorid = usr.KantorId;

            if (!String.IsNullOrEmpty(nip))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = dataMasterModel.GetProfileKKP(nip, kantorid,from,to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListProfileKKP(string pid, string tip)
        {
            var usr = functions.claimUser();
            string kantorid = usr.KantorId;
            var listProfile = dataMasterModel.GetProfilesKKPByKantorId(pid, kantorid, tip);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusProfileKKP()
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            try
            {
                string userid = usr.UserId;
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    result = dataMasterModel.HapusProfileKKP(id, userid);
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
        public JsonResult AddProfileKKP(string nip, string unit, string pid, bool plt, bool bbk, string jbt, string nap, string vsp, string tip, string uid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            if (OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
            {
                tr.Pesan = "Fitur ini tidak dapat digunakan pada mode belajar";
            }
            else
            {
                // Cek duplikat
                string kid = usr.KantorId;
                int cekrow = dataMasterModel.JumlahProfileKKP(nip, pid, kid);
                if (cekrow > 0)
                {
                    tr.Pesan = string.Concat("Profile ", jbt, " untuk pegawai ", nap, " sudah ada.");
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                cekrow = dataMasterModel.JumlahPemilikProfileStruktural(pid, kid);
                if (cekrow > 0)
                {
                    tr.Pesan = String.Concat("Profile ", jbt, " masih ada yang menjabat");
                    return Json(tr, JsonRequestBehavior.AllowGet);
                }
                var _profile = new ProfilePegawai();
                _profile.ProfileId = pid;
                _profile.PegawaiId = nip;
                _profile.StatusPlt = (plt) ? 1 : 0;
                _profile.BisaBooking = (bbk) ? 1 : 0;
                _profile.NamaProfile = jbt;


                HttpPostedFileBase mfile = Request.Files[0];
                if (mfile == null || mfile.ContentType != "application/pdf")
                {
                    return Json(new { Status = false, Pesan = "File harus pdf" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _profile.NomorSK = mfile.FileName;
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "AksesKKP";
                    string jpid = mPersuratan.GetUID();
                    content.Add(new StringContent(kid), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(jpid), "dokumenId");
                    content.Add(new StringContent(".pdf"), "fileExtension");
                    content.Add(new StringContent("0"), "versionNumber");
                    content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);
                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                        {
                            var _pimpinan = dataMasterModel.GetProfilePimpinan(usr.UnitKerjaId);
                            if(_pimpinan == null)
                            {
                                tr.Pesan = "Profile Pimpinan Tidak Ditemukan";
                            }
                            else
                            {
                                tr = dataMasterModel.KirimPengajuanProfileKKP(usr.UserId, _pimpinan.ProfileId, "ProfileKKP", usr.KantorId, vsp, _profile, jpid, uid);
                            }
                        }
                        else
                        {
                            tr.Pesan = string.Concat("Gagal Upload ", reqresult.ReasonPhrase.ToString());
                            return Json(tr, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListUnitKerjaByKantorInduk(string kid)
        {
            var listProfile = dataMasterModel.GetListUnitKerjaByKantorInduk(kid);
            return Json(listProfile, JsonRequestBehavior.AllowGet);
        }
    }
}