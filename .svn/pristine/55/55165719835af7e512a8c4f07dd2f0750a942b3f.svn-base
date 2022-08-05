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

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class PengaduanController : Controller
    {
        Functions functions = new Functions();
        Models.PengaduanModel pengaduanModel = new Models.PengaduanModel();
        Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.KontentModel kontentm = new Models.KontentModel();

        public JsonResult SearchPemohonFromKemendagri(string nik)
        {
            Models.Entities.NIKResult data = pengaduanModel.SearchPemohonFromKemendagri(nik);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLokasiKejadian()
        {
            List<Models.Entities.LokasiKejadian> result = pengaduanModel.GetLokasiKejadian();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJenisPekerjaan()
        {
            List<Models.Entities.JenisPekerjaan> result = pengaduanModel.GetJenisPekerjaan();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarLampiranAduan(string aduanid)
        {

            List<Models.Entities.LampiranPengaduan> result = pengaduanModel.GetListLampiranPengaduan(aduanid, "");

            int custIndex = 1;
            Dictionary<int, Models.Entities.LampiranPengaduan> dict = result.ToDictionary(x => custIndex++, x => x);

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

                    return PartialView("DaftarLampiranAduan", dict);
                }
                else
                {
                    return RedirectToAction("ListPengaduan", "Pengaduan");
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

        public async Task<ActionResult> GetFilePengaduan(string id, string unitkerjaid)
        {
            Models.Entities.TransactionResult result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                if (string.IsNullOrEmpty(unitkerjaid))
                {
                    unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                }
                string tipe = "Pengaduan";
                string versi = kontentm.CekVersi(id).ToString();

                DateTime tglSunting = persuratanmodel.getTglSunting(id, tipe);
                string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                content.Add(new StringContent(unitkerjaid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
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
                            docfile.FileDownloadName = String.Concat(tipe, ".pdf");

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

        public async Task<ActionResult> GetFilePengaduanWithExt(string id, string unitkerjaid, string namafile, string extension)
        {
            var result = new { Status = false, Message = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                if (string.IsNullOrEmpty(unitkerjaid))
                {
                    unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
                }
                string tipe = "Pengaduan";
                string versi = kontentm.CekVersi(id).ToString();

                DateTime tglSunting = persuratanmodel.getTglSunting(id, tipe);
                string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                content.Add(new StringContent(unitkerjaid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(extension), "fileExtension");
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

        [HttpPost]
        public JsonResult InsertLampiranPengaduan(Models.Entities.Pengaduan data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            data.UserId = pegawaiid;
            data.Nip = pegawaiid;
            data.NamaPengirim = namapengirim;

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            string judul = "Lampiran pengaduan nomor: " + data.NomorLaporan;

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
                data.LampiranPengaduanId = id;

                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                DateTime tglSunting = DateTime.Now;
                string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                content.Add(new StringContent(unitkerjaid), "kantorId");
                content.Add(new StringContent("Pengaduan"), "tipeDokumen");
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

                tr = kontentm.SimpanKontenFile(unitkerjaid, id, judul, namapengirim, data.TanggalAduan, "Pengaduan", out versi);
            }

            tr = pengaduanModel.InsertLampiranPengaduan(data, unitkerjaid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusLampiranPengaduanById()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string aduanid = Request.Form["aduanid"].ToString();
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(aduanid) && !String.IsNullOrEmpty(id))
                {
                    result = pengaduanModel.HapusLampiranPengaduanById(aduanid, id);
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

        public ContentResult GetJumlahLampiranPengaduan(string aduanid)
        {
            string result = pengaduanModel.GetJumlahLampiranPengaduan(aduanid);

            return Content(result);
        }

        public ContentResult GetStatusForwardTU(string aduaninboxid)
        {
            string result = pengaduanModel.GetStatusForwardTU(aduaninboxid);

            return Content(result);
        }

        public ContentResult GetCatatanSebelumnya(string aduaninboxid)
        {
            string result = pengaduanModel.GetCatatanSebelumnya(aduaninboxid);

            return Content(result);
        }

        public ContentResult JumlahPengaduan()
        {
            string result = "";
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

            int jumlah = pengaduanModel.JumlahPengaduan(unitkerjaid, pegawaiid, myProfiles);

            result = String.Format("{0:#,##0}", jumlah);

            return Content(result);
        }

        public ActionResult BuatPengaduan()
        {
            Models.Entities.Pengaduan data = new Models.Entities.Pengaduan();
            data.ListKategoriPengaduan = pengaduanModel.GetKategoriPengaduan();
            data.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);

            return View(data);
        }

        public ActionResult BukaPengaduan(string aduaninboxid)
        {
            if (!String.IsNullOrEmpty(aduaninboxid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                ViewBag.UnitKerjaId = unitkerjaid;

                Models.Entities.AduanInbox data = pengaduanModel.GetAduanInboxId(aduaninboxid);
                data.ListKategoriPengaduan = pengaduanModel.GetKategoriPengaduan();
                data.ListUnitKerja = dataMasterModel.GetListUnitKerja("", "", "", true);
                data.ListUnitKerjaHistoriSurat = pengaduanModel.GetUnitKerjaAduanHistory(data.AduanId);
                data.ListPerintahDisposisi = persuratanmodel.GetPerintahDisposisi();
                data.ListProfileTujuan = new List<Models.Entities.Profile>();
                data.ListTujuanPegawai = new List<Models.Entities.Pegawai>();
                data.ListProfiles = new List<Models.Entities.Profile>();
                data.CatatanSebelumnya = pengaduanModel.GetCatatanSebelumnya(aduaninboxid);
                data.PerintahDisposisiSebelumnya = pengaduanModel.GetDisposisiSebelumnya(aduaninboxid);

                if (string.IsNullOrEmpty(data.TanggalTerima))
                {
                    data.TanggalTerima = pengaduanModel.GetServerDate().ToString("dd/MM/yyyy HH:mm");
                }

                // Update Flag Buka Surat
                string namapegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                Models.Entities.TransactionResult tr = pengaduanModel.BukaAduanInbox(data.AduanId, data.AduanInboxId, pegawaiid, namapegawai);

                return View("BukaPengaduan", data);
            }
            else
            {
                return RedirectToAction("ListPengaduan", "Pengaduan");
            }
        }


        public ActionResult ListPengaduan()
        {
            Models.Entities.FindPengaduan find = new Models.Entities.FindPengaduan();
            return View(find);
        }

        public ActionResult DaftarListPengaduan(int? pageNum, Models.Entities.FindPengaduan f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            string metadata = f.Metadata;

            List<Models.Entities.AduanInbox> result = pengaduanModel.GetPengaduan(unitkerjaid, nip, metadata, from, to);

            int custIndex = from;
            Dictionary<int, Models.Entities.AduanInbox> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarListPengaduan", dict);
                }
                else
                {
                    return RedirectToAction("ListPengaduan", "Pengaduan");
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

        public ActionResult DaftarAduanHistory(string aduanid, string unitkerjaid)
        {
            List<Models.Entities.AduanInbox> result = pengaduanModel.GetPengaduanHistori(aduanid, unitkerjaid);

            int custIndex = 1;
            Dictionary<int, Models.Entities.AduanInbox> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarAduanHistory", dict);
                }
                else
                {
                    return RedirectToAction("ListPengaduan", "Pengaduan");
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

        public ActionResult InfoPengaduan()
        {
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            Models.Entities.FindPengaduan find = new Models.Entities.FindPengaduan();

            return View(find);
        }

        public ActionResult DaftarSemuaPengaduan(int? pageNum, Models.Entities.FindPengaduan f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
            if (@OtorisasiUser.IsRoleAdministrator() == true)
            {
                myProfiles = "";
            }

            string metadata = f.Metadata;

            List<Models.Entities.Pengaduan> result = pengaduanModel.GetInfoPengaduan(unitkerjaid, myProfiles, metadata, from, to);

            int custIndex = from;
            Dictionary<int, Models.Entities.Pengaduan> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarSemuaPengaduan", dict);
                }
                else
                {
                    return RedirectToAction("InfoPengaduan", "Pengaduan");
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

        public ActionResult ViewPengaduan(string aduanid)
        {
            if (!String.IsNullOrEmpty(aduanid))
            {
                string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

                Models.Entities.Pengaduan data = pengaduanModel.GetPengaduanById(aduanid, unitkerjaid);
                data.ListUnitKerjaHistoriSurat = pengaduanModel.GetUnitKerjaAduanHistory(data.AduanId);

                return View("ViewPengaduan", data);
            }
            else
            {
                return RedirectToAction("InfoPengaduan", "Pengaduan");
            }
        }

        [HttpPost]
        public JsonResult InsertPengaduan(Models.Entities.Pengaduan data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            string namapegawaipengirim = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            string myProfileId = functions.MyProfiles(pegawaiid, kantorid).Replace("'", "");

            string myClientId = Functions.MyClientId;

            //data.UserId = pegawaiid;
            data.NamaPengirim = namapegawaipengirim;

            string judul = "Pengaduan dari " + data.NamaPengadu;

            // Cek Tujuan Surat
            List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId);
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Surat wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = persuratanmodel.GetListSessionLampiran(myClientId);
            //string isFileAttMandatory = ConfigurationManager.AppSettings["IsFileAttMandatory"].ToString();
            //if (isFileAttMandatory == "true")
            //{
            //    if (dataSessionLampiran.Count == 0)
            //    {
            //        tr.Pesan = "File Surat wajib diupload";
            //        return Json(tr, JsonRequestBehavior.AllowGet);
            //    }
            //}

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }


            #region Simpan File Fisik

            foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
            {
                if (lampiranSurat.ObjectFile.Length > 0)
                {
                    int versi = 0;
                    string id = lampiranSurat.LampiranSuratId;

                    Stream stream = new MemoryStream(lampiranSurat.ObjectFile);

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    DateTime tglSunting = DateTime.Now;
                    string serviceurl = persuratanmodel.GetServiceKonten(tglSunting);

                    content.Add(new StringContent(unitkerjaid), "kantorId"); // kirim unitkerjaid ke parameter kantorid
                    content.Add(new StringContent("Pengaduan"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(stream), "file", lampiranSurat.NamaFile);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings[serviceurl].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        tr.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                        tr.Pesan = reqresult.ReasonPhrase;
                    }

                    tr = kontentm.SimpanKontenFile(unitkerjaid, id, judul, namapegawaipengirim, data.TanggalAduan, "Pengaduan", out versi);
                }
            }

            #endregion


            // Insert Pengaduan

            tr = pengaduanModel.InsertPengaduan(data, unitkerjaid, myProfileId, pegawaiid, namapegawaipengirim);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult KirimPengaduan(Models.Entities.AduanInbox data)
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

            // Cek Tujuan Pengaduan
            List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanmodel.GetListSessionTujuanSurat(myClientId);
            if (dataSessionTujuanSurat.Count == 0)
            {
                tr.Pesan = "Tujuan Pengaduan wajib diisi";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            // Profile Id Pengirim
            //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
            List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
            if (listProfilePegawai.Count > 0)
            {
                data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
            }

            tr = pengaduanModel.KirimPengaduan(data, unitkerjaid, pegawaiid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanCatatanAnda(Models.Entities.Pengaduan data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            tr = pengaduanModel.SimpanCatatanAnda(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ArsipPengaduan(Models.Entities.Pengaduan data)
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

                // Profile Id Pengirim
                //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
                List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
                if (listProfilePegawai.Count > 0)
                {
                    data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                    data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
                }

                result = pengaduanModel.ArsipPengaduan(data, unitkerjaid, pegawaiid);
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
        public ActionResult SelesaiAduanInbox(Models.Entities.Pengaduan data)
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

                // Profile Id Pengirim
                //List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai(pegawaiid, kantorid);
                List<Models.Entities.ProfilePegawai> listProfilePegawai = dataMasterModel.GetProfilePegawai_Simpeg(pegawaiid, kantorid);
                if (listProfilePegawai.Count > 0)
                {
                    data.ProfileIdPengirim = listProfilePegawai[0].ProfileId;
                    data.NamaProfilePengirim = listProfilePegawai[0].NamaProfile;
                }

                result = pengaduanModel.SelesaiAduanInbox(data, unitkerjaid, pegawaiid);
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
    }
}