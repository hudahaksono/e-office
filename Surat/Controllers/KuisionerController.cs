using Newtonsoft.Json;
using Surat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class KuisionerController : Controller
    {
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.KuisionerModel Kuisionermodel = new Models.KuisionerModel();

        [HttpPost]
        public ActionResult simpanPertanyaan(Models.Entities.Pertanyaan data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            data.UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;

            tr = Kuisionermodel.SimpanPertanyaan(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EntriPertanyaan(string id)
        {
            Models.Entities.Pertanyaan data = new Models.Entities.Pertanyaan();
            var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;

            if (!string.IsNullOrEmpty(id))
            {
                List<Models.Entities.Pertanyaan> list = Kuisionermodel.GetPertanyaan(id, UserId, false);
                data = list[0];
            }


            ViewBag.UserId = UserId;

            return View(data);
        }

        public JsonResult GetPertanyaan(string Id)
        {
            var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            List<Models.Entities.Pertanyaan> record = Kuisionermodel.GetPertanyaan(Id, UserId, false);

            return Json(new { data = record, recordsTotal = record.Count }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AdministrasiKuisioner(string mssg)
        {
            //var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            //if (UserId == 'cbad36a7-6e53-4f14-9dcf-6e7aa5027064')
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

        public JsonResult HapusPertanyaan(string id)
        {
            var userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            var record = Kuisionermodel.HapusPertanyaan(id, userid);
            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetTipe(string id, string nom)
        {
            var userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            var record = Kuisionermodel.SetTipe(id, userid, nom);
            return Json(record, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PengisianKuisioner()
        {
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            var check = Kuisionermodel.GetJawabanIndividu(UserId);

            if (check.Count > 2)
            {
                return RedirectToAction("ReportIndividu", "Kuisioner");
            }
            Models.Entities.Kuisioner data = new Models.Entities.Kuisioner();
            data.ListPertanyaan = Kuisionermodel.GetPertanyaan("", "", true);
            data.Ct = data.ListPertanyaan.Count;

            return View(data);
        }

        public ActionResult KirimJawaban()
        {
            List<Models.Entities.KuisionerJawaban> data = new List<Models.Entities.KuisionerJawaban>();
            var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;
            var parameters = Request.QueryString;
            foreach (string param in parameters)
            {
                Models.Entities.KuisionerJawaban item = new Models.Entities.KuisionerJawaban();
                item.Nama_Lengkap = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
                item.UserId = UserId;
                item.Pertanyaan_Id = param;
                item.Nama_Jawaban = Request[param].ToString();
                data.Add(item);
            }
            var result = Kuisionermodel.SimpanJawaban(data);
            //return Json(result, JsonRequestBehavior.AllowGet);


            return RedirectToAction("ReportIndividu", "Kuisioner");
        }

        public ActionResult ReportIndividu()
        {
            var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            var data = Kuisionermodel.GetJawabanIndividu(UserId);
            ViewBag.Datalist = data;
            ViewBag.namauser = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).NamaPegawai;
            return View();

        }


        public ActionResult ReportKuisioner()
        {
            var UserId = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            Models.Entities.KuisionerReport data = new Models.Entities.KuisionerReport();
            data.ListPertanyaan = Kuisionermodel.GetPertanyaan("", "", true);
            data.ListPertanyaanTerakhir = Kuisionermodel.GetPertanyaanTerakhir("", "", true);
            data.ListPertanyaanall = Kuisionermodel.GetPertanyaanall("", "", true);
            if (data.ListPertanyaan != null)
            {

                foreach (var jwb in data.ListPertanyaan)
                {
                    jwb.ListJawaban = Kuisionermodel.GetReportJawaban(jwb.Pertanyaan_Id, jwb.UserId, true);
                }
            }
            data.Ct = data.ListPertanyaan.Count;
            data.Ctall = data.ListPertanyaan.Count();

            foreach (var jwball in data.ListPertanyaan)
            {
                jwball.ListJawabanall = Kuisionermodel.GetReportJawabanall(jwball.Pertanyaan_Id, jwball.UserId, true);
            }
            data.Ctall += data.ListPertanyaan.Count;
            return View(data);
        }

        public JsonResult CekPengisian()
        {
            var userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            int hasillog = Kuisionermodel.GetDataPengisian(userid);
            return Json(hasillog, JsonRequestBehavior.AllowGet);
        }









        public JsonResult HapusTabelHasilJawaban1(Models.Entities.KuisionerJawaban data, string jawaban_id)
        {
            data.TanggalHapus = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
            data.UserIDHapus = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            //var id = Kuisionermodel.GetidHapus(jawaban_id);
            var userid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            var record = Kuisionermodel.HapusTabelHasilJawaban1(data, jawaban_id);
            return Json(record, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult HapusTabelHasilJawaban2(Models.Entities.KuisionerJawaban data, string id)
        {
            data.TanggalHapus = DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt");
            data.UserIDHapus = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UserId;
            var result = new TransactionResult() { Status = false, Pesan = "" };
            try
            {
                if (!String.IsNullOrEmpty(id))
                {

                    var usr = HttpContext.User.Identity as InternalUserIdentity;
                    result = Kuisionermodel.HapusTabelHasilJawaban2(data, id);
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

        public ActionResult TabelHasilJawaban()
        {

            if (Request.IsAjaxRequest())
            {
                Models.Entities.KuisionerJawaban data1 = new Models.Entities.KuisionerJawaban();

                data1.ListKuisionerJawaban1 = Kuisionermodel.GetTabelHasilJawaban1();
                data1.ListKuisionerJawaban2 = Kuisionermodel.GetTabelHasilJawaban2();

                return View(data1);
            }
            else
            {
                Models.Entities.KuisionerJawaban data1 = new Models.Entities.KuisionerJawaban();

                data1.ListKuisionerJawaban1 = Kuisionermodel.GetTabelHasilJawaban1();
                data1.ListKuisionerJawaban2 = Kuisionermodel.GetTabelHasilJawaban2();
                return View(data1);
            }
        }



        /// //////////////////////////////////////////////////////////


    }
}