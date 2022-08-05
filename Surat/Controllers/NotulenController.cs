using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class NotulenController : Controller
    {
        Models.NotulenModel notulenmodel = new Models.NotulenModel();
        Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

        public ActionResult Notulen()
        {
            Surat.Models.Entities.FindNotulen find = new Surat.Models.Entities.FindNotulen();
            return View(find);
        }

        public ActionResult DaftarNotulen(int? pageNum, Models.Entities.FindNotulen f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 20;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            string metadata = f.Metadata;

            List<Models.Entities.Notulen> result = notulenmodel.GetNotulen("", nip, metadata, from, to);

            int custIndex = from;
            Dictionary<int, Models.Entities.Notulen> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarNotulen", dict);
                }
                else
                {
                    return RedirectToAction("Notulen", "Flow");
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

        // UNTUK NON TU JUMLAH NOTULEN
        public ContentResult JumlahNotulensi()
        {
            string result = "";
            string nip = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            try
            {
                int jumlah = notulenmodel.JumlahNotulensi(nip);

                result = String.Format("{0:#,##0}", jumlah);
            }
            catch
            {
                result = "--";
            }

            return Content(result);
        }

        public ActionResult GetNotulenTop4()
        {
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            List<Models.Entities.Notulen> result = notulenmodel.GetNotulenTop4(unitkerjaid);

            int custIndex = 1;
            Dictionary<int, Models.Entities.Notulen> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("GetNotulenTop4", dict);
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

        public ActionResult InformasiNotulenFromList(string notulenid)
        {
            if (!String.IsNullOrEmpty(notulenid))
            {
                Models.Entities.Notulen infonotulen = notulenmodel.GetNotulenById(notulenid);

                if (infonotulen != null && !String.IsNullOrEmpty(infonotulen.NotulenId))
                {
                    return PartialView("ViewNotulen", infonotulen);
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

        public ActionResult BuatNotulenBaru()
        {
            Models.Entities.Notulen notulen = new Models.Entities.Notulen();
            return View("EntriNotulen", notulen);

            //if (Request.IsAjaxRequest())
            //{
            //    return PartialView("EntriNotulen", notulen);
            //}
            //else
            //{
            //    return RedirectToAction("Notulen", "Notulen");
            //}
        }

        public ActionResult EntriDataNotulen(string suratid, string tanggalsurat, string nomorsurat, string kategori)
        {
            Models.Entities.Notulen notulen = new Models.Entities.Notulen();

            string perihal = "";

            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }
            Models.Entities.Surat surat = persuratanmodel.GetSuratBySuratId(suratid, satkerid);
            if (surat != null)
            {
                perihal = surat.Perihal;
            }

            string notulenid = notulenmodel.GetNotulenIdBySuratId(suratid);

            if (!string.IsNullOrEmpty(notulenid))
            {
                notulen = notulenmodel.GetNotulenById(notulenid);
            }

            notulen.SuratId = suratid;
            notulen.Perihal = perihal;
            notulen.TanggalSurat = tanggalsurat;
            notulen.NomorSurat = nomorsurat;
            notulen.NotulenId = notulenid;
            notulen.Kategori = kategori;

            return View("EntriDataNotulen", notulen);
        }

        public ActionResult EditNotulen(string id, string judul)
        {
            if (!String.IsNullOrEmpty(id))
            {
                Models.Entities.Notulen notulen = notulenmodel.GetNotulenById(id);

                return View("EntriNotulen", notulen);
            }
            else
            {
                return RedirectToAction("Notulen", "Notulen");
            }
        }

        [HttpPost]
        public JsonResult SimpanNotulen(Surat.Models.Entities.Notulen notulen)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            // Cek duplikat dokumen
            if (String.IsNullOrEmpty(notulen.NotulenId))
            {
                string msg = string.Empty;
                int cekrow = notulenmodel.JumlahNotulen(notulen.Judul);
                if (cekrow > 0)
                {
                    msg = String.Concat("Pengumuman ", notulen.Judul, " sudah ada.");
                    return Json(new { Status = false, Pesan = msg }, JsonRequestBehavior.AllowGet);
                }
            }

            notulen.NIP = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            notulen.NamaPegawai = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            notulen.UnitKerjaId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;

            tr = notulenmodel.SimpanNotulen(notulen);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusNotulen()
        {
            var result = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    string userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
                    result = notulenmodel.HapusNotulen(id, userid);
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
    }
}