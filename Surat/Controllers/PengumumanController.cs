using System;
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
using Surat.Models;
using Surat.Models.Entities;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class PengumumanController : Controller
    {
        Functions functions = new Functions();
        PengumumanModel mPengumuman = new PengumumanModel();
        DataMasterModel mDataMaster = new DataMasterModel();
        KontentModel mkonten = new KontentModel();

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetImagePengumuman(string id)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                var konten = mkonten.getKontenAktif(id);
                if(konten != null)
                {
                    string kantorid = konten.KANTORID;
                    string tipe = konten.TIPE;
                    string versi = konten.VERSI.ToString();
                    string ext = konten.EKSTENSI;
                    ext = string.IsNullOrEmpty(ext) ? ".pdf" : string.Concat(ext.Substring(0, 1).Equals(".") ? "" : ".", ext);
                    string filename = string.Concat(tipe, ext);

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
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var strm = await reqresult.Content.ReadAsStreamAsync();
                                var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                                if (ext.Equals(".jpg"))
                                {
                                    docfile = new FileStreamResult(strm, "image/jpeg");
                                }
                                else if (ext.Equals(".png"))
                                {
                                    docfile = new FileStreamResult(strm, "image/png");
                                }
                                docfile.FileDownloadName = filename;

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
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanPengumuman(DataPengumuman data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = (HttpContext.User.Identity as InternalUserIdentity);
            data.UserId = usr.UserId;
            data.Nip = usr.PegawaiId;
            data.NamaPengirim = usr.NamaPegawai;
            data.UnitKerjaId = usr.UnitKerjaId;
            data.TanggalBuat = mPengumuman.GetServerDate();
            data.PengumumanID = string.IsNullOrEmpty(data.PengumumanID) ? mPengumuman.GetUID() : data.PengumumanID;
            data.Judul = Server.UrlEncode(data.Judul);
            data.Isi = Server.UrlEncode(data.Isi);
            data.WebUrl = string.IsNullOrEmpty(data.WebUrl) ? data.WebUrl : Server.UrlEncode(data.WebUrl);
            data.ImageUrl = string.IsNullOrEmpty(data.ImageUrl) ? data.WebUrl : Server.UrlEncode(data.ImageUrl);
            data.Gambar = new List<FilePengumuman>();
            data.ValidSejak = data.ValidSejak == null ? mPengumuman.GetServerDate() : data.ValidSejak;
            data.ValidSampai = data.ValidSampai == null ? mPengumuman.GetServerDate() : data.ValidSampai;
            try
            {
                HttpPostedFileBase mfile = Request.Files[0];
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

                    data.Gambar.Add(new FilePengumuman() { NamaFile = mfile.FileName, FileId = mPengumuman.GetUID(), ExtFile = mfile.FileName.Split('.').Last() });

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(usr.UnitKerjaId), "kantorId");
                    content.Add(new StringContent("Pengumuman"), "tipeDokumen");
                    content.Add(new StringContent(data.PengumumanID), "dokumenId");
                    content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        content.Add(new StringContent(fileExtension), "fileExtension");
                    }

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                        tr.Pesan = reqresult.ReasonPhrase;
                    }

                    if (tr.Status)
                    {
                        if(mkonten.SimpanKontenFile(data.UnitKerjaId, data.PengumumanID, data.Judul, data.NamaPengirim, "", "Pengumuman", out versi, fileExtension.ToLower()).Status)
                        {

                        }                        
                    }
                }
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
            }

            tr = mPengumuman.SimpanPengumuman(data);
            if (tr.Status)
            {
                var tipekantorid = mDataMaster.GetTipeKantor(usr.KantorId);
                if (data.Target.Equals("All"))
                {
                    var listunit = mDataMaster.GetListUnitKerjaPengumuman();
                    foreach (var unit in listunit)
                    {
                        new Mobile().KirimBroadcast(unit.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                    }
                }
                else if (data.Target.Equals("Pusat"))
                {
                    if (tipekantorid == 1)
                    {
                        var listunit = mDataMaster.GetListUnitKerjaPengumuman(data.Target);
                        foreach(var unit in listunit)
                        {
                            new Mobile().KirimBroadcast(unit.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                        }
                    }
                }
                else if (data.Target.Equals("Kanwil"))
                {
                    if (tipekantorid == 1)
                    {
                        var listunit = mDataMaster.GetListUnitKerjaPengumuman(data.Target);
                        foreach (var unit in listunit)
                        {
                            new Mobile().KirimBroadcast(unit.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                        }

                    }
                    else
                    {
                        new Mobile().KirimBroadcast(usr.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                    }
                }
                else if (data.Target.Equals("Kantah"))
                {
                    if(tipekantorid == 1)
                    {
                        var listunit = mDataMaster.GetListUnitKerjaPengumuman(data.Target);
                        foreach (var unit in listunit)
                        {
                            new Mobile().KirimBroadcast(unit.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                        }
                    }
                    else if (tipekantorid == 2)
                    {
                        var listunit = mDataMaster.GetListUnitKerjaPengumuman(data.Target,usr.UnitKerjaId);
                        foreach (var unit in listunit)
                        {
                            new Mobile().KirimBroadcast(unit.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                        }
                    }
                    else
                    {
                        new Mobile().KirimBroadcast(usr.UnitKerjaId, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                    }
                }
                else if (data.Target.Equals("Custom"))
                {

                }
                else
                {
                    new Mobile().KirimBroadcast(data.Target, data.Judul, data.Isi, null, data.ImageUrl, data.WebUrl);
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuatPengumuman()
        {
            var usr = (HttpContext.User.Identity as InternalUserIdentity);
            ViewBag.IsAdministrator = OtorisasiUser.IsRoleAdministrator();
            var data = new DataPengumuman();
            data.UserId = usr.UserId;
            data.Nip = usr.PegawaiId;
            data.NamaPengirim = usr.NamaPegawai;
            data.UnitKerjaId = usr.UnitKerjaId;
            data.PilihanTarget = new List<SelectListItem>();
            data.ListUnitKerja = new List<UnitKerja>();
            var tipekantorid = mDataMaster.GetTipeKantor(usr.KantorId);
            data.PilihanTarget.Add(new SelectListItem() { Text = mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId });
            if (tipekantorid == 1)
            {
                data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Akun", Value = "All" });
                data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Unit Pusat", Value = "Pusat" });
                data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Unit Kanwil", Value = "Kanwil" });
            }
            if (tipekantorid <= 2)
            {
                string namaunit= tipekantorid.Equals(2) ? string.Concat("Semua Unit Kantah di ", mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId).Replace("Kantor", "")) : "Semua Unit Kantah";
                data.PilihanTarget.Add(new SelectListItem() { Text = namaunit, Value = "Kantah" });
            }
            //data.PilihanTarget.Add(new SelectListItem() { Text = "Unit Kerja Pilihan", Value = "Custom" });
            data.ListUnitKerja.Add(new UnitKerja() { NamaUnitKerja = mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId), UnitKerjaId = usr.UnitKerjaId });
            //if (ViewBag.IsAdministrator)
            //{
            //    data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Akun", Value = "All" });
            //    data.PilihanTarget.Add(new SelectListItem() { Text = "Unit Kerja Pilihan", Value = "Custom" });
            //}
            //else
            //{
            //    data.PilihanTarget.Add(new SelectListItem() { Text = mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId });
            //    data.ListUnitKerja.Add(new UnitKerja() { NamaUnitKerja = mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId), UnitKerjaId= usr.UnitKerjaId });
            //}

            return PartialView("Form", data);
        }

        public ActionResult BukaPengumuman(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var usr = (HttpContext.User.Identity as InternalUserIdentity);
                ViewBag.IsAdministrator = OtorisasiUser.IsRoleAdministrator();

                DataPengumuman data = mPengumuman.GetDataPengumuman(id);

                data.PilihanTarget = new List<SelectListItem>();
                data.ListUnitKerja = new List<UnitKerja>();
                var tipekantorid = mDataMaster.GetTipeKantor(usr.KantorId);
                data.PilihanTarget.Add(new SelectListItem() { Text = mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId });
                if (tipekantorid == 1)
                {
                    data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Akun", Value = "All" });
                    data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Unit Pusat", Value = "Pusat" });
                    data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Unit Kanwil", Value = "Kanwil" });
                }
                if (tipekantorid <= 2)
                {
                    string namakanwil = tipekantorid.Equals(2) ? string.Concat("Semua Unit Kantah di ", mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId).Replace("Kantor", "")) : "Semua Unit Kantah";
                    data.PilihanTarget.Add(new SelectListItem() { Text = "Semua Unit Kantah", Value = "Kantah" });
                }
                data.PilihanTarget.Add(new SelectListItem() { Text = "Unit Kerja Pilihan", Value = "Custom" });
                data.ListUnitKerja.Add(new UnitKerja() { NamaUnitKerja = mDataMaster.GetNamaUnitKerjaById(usr.UnitKerjaId), UnitKerjaId = usr.UnitKerjaId });
                data.Judul = Server.UrlDecode(data.Judul);
                data.Isi = Server.UrlDecode(data.Isi);
                data.WebUrl = string.IsNullOrEmpty(data.WebUrl) ? "" : Server.UrlDecode(data.WebUrl);
                data.ImageUrl = string.IsNullOrEmpty(data.ImageUrl) ? "" : Server.UrlDecode(data.ImageUrl);
                data.UserId = usr.UserId;
                data.Nip = usr.PegawaiId;
                data.NamaPengirim = usr.NamaPegawai;
                data.UnitKerjaId = usr.UnitKerjaId;
                data.UnitKerjaPenerima = data.Target;
                data.TanggalUbah = mPengumuman.GetServerDate();

                return PartialView("Form", data);
            }
            else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "notfound",
                    ContentEncoding = System.Text.Encoding.UTF8
                };
            }
        }

        public ActionResult DaftarListPengumuman(int? draw, int? start, int? length, CariPengumuman f)
        {
            var usr = (HttpContext.User.Identity as InternalUserIdentity);
            var result = new List<DataPengumuman>();
            decimal? total = 0;

            if(usr != null)
            {
                string sort = Request.Form["orderby"];
                string dir = Request.Form["orderdir"];
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.MetaData = string.IsNullOrEmpty(f.MetaData) ? string.Empty : Server.UrlEncode(f.MetaData);
                result = mPengumuman.GetListPengumuman(usr.UnitKerjaId, f, from, to);
                if (result.Count > 0)
                {
                    foreach (var r in result)
                    {
                        r.Judul = Server.UrlDecode(r.Judul);
                        r.Isi = Server.UrlDecode(r.Isi);

                        if (r.Target.Equals("Custom"))
                        {
                            var listUnitKerja = mPengumuman.GetListUnitKerja(r.PengumumanID);
                            foreach(var uk in listUnitKerja)
                            {
                                r.DetailTarget += string.Concat(string.IsNullOrEmpty(r.DetailTarget)?"":", ",uk.NamaUnitKerja);
                            }
                        }
                        else if (r.Target.Equals("Satker"))
                        {
                            var listUnitKerja = mPengumuman.GetListUnitKerja(r.PengumumanID);
                            foreach (var uk in listUnitKerja)
                            {
                                r.DetailTarget += string.Concat(string.IsNullOrEmpty(r.DetailTarget) ? "" : ", ", uk.NamaUnitKerja);
                            }
                        }
                        else if (r.Target.Equals("Personal"))
                        {

                        }
                        else if (r.Target.Equals("All"))
                        {

                        }
                        else
                        {
                            r.Target = mDataMaster.GetNamaUnitKerjaById(r.Target);
                            r.DetailTarget = r.Target;
                        }
                    }
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }
    }
}