using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
//using DeviceId;
using Surat.Codes;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class HomeController : Controller
    {
        //Functions functions = new Functions();
        Functions functions = new Functions();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
        Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();

        public ActionResult Index()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            return View();
        }

        public async Task<ActionResult> Dashboard()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            string pegawaiid = usr.PegawaiId;
            string unitkerjaid = usr.UnitKerjaId;
            string namaunitkerja = (string.IsNullOrEmpty(unitkerjaid)) ? "" : dataMasterModel.GetNamaUnitKerjaById(unitkerjaid);

            string satkerid = dataMasterModel.GetSatkerId(usr.UnitKerjaId);

            Models.Entities.ChartSurat chartData = new Models.Entities.ChartSurat();

            chartData.UnitKerjaId = unitkerjaid;
            chartData.NamaUnitKerja = namaunitkerja;


            #region Data Progress, tipe Chart Gauge

            decimal totalSuratSatker = 0;
            decimal jumlahSuratSatkerSelesai = 0;

            Models.Entities.ChartSuratGauge chartDataGaugeSatker = new Models.Entities.ChartSuratGauge();
            Models.Entities.ChartSuratGauge chartDataGaugePersonal = new Models.Entities.ChartSuratGauge();

            chartDataGaugePersonal.TotalSuratPersonal = 0;
            chartDataGaugePersonal.JumlahSelesaiPersonal = 0;

            chartData.JumlahSurat = "0";
            chartData.JumlahInbox = "0";
            chartData.JumlahTerkirim = "0";
            chartData.JumlahSelesai = "0";

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                //var _tr = dataMasterModel.GetTipeUser(pegawaiid,kantorid);
                //if (_tr.Status)
                //{
                //    string tipeUser = _tr.ReturnValue;
                //    var listRekapSurat = persuratanmodel.GetRekapSurat(unitkerjaid, satkerid, "ALL");
                //    foreach (Models.Entities.RekapSurat item in listRekapSurat)
                //    {
                //        if (item.NIP == pegawaiid)
                //        {
                //            chartDataGaugePersonal.TotalSuratPersonal = item.JumlahSurat;
                //            chartDataGaugePersonal.JumlahSelesaiPersonal = item.JumlahSelesai;

                //            chartData.JumlahSurat = string.Format("{0:#,##0}", item.JumlahSurat);
                //            chartData.JumlahInbox = string.Format("{0:#,##0}", item.JumlahInbox);
                //            chartData.JumlahTerkirim = string.Format("{0:#,##0}", item.JumlahTerkirim);
                //            chartData.JumlahSelesai = string.Format("{0:#,##0}", item.JumlahSelesai);
                //        }

                //        totalSuratSatker += item.JumlahSurat;
                //        jumlahSuratSatkerSelesai += item.JumlahSelesai;
                //    }
                //}

                var listRekapSurat = persuratanmodel.GetRekapSurat(unitkerjaid, satkerid, "ALL");
                foreach (Models.Entities.RekapSurat item in listRekapSurat)
                {
                    if (item.NIP == pegawaiid)
                    {
                        chartDataGaugePersonal.TotalSuratPersonal = item.JumlahSurat;
                        chartDataGaugePersonal.JumlahSelesaiPersonal = item.JumlahSelesai;

                        chartData.JumlahSurat = string.Format("{0:#,##0}", item.JumlahSurat);
                        chartData.JumlahInbox = string.Format("{0:#,##0}", item.JumlahInbox);
                        chartData.JumlahTerkirim = string.Format("{0:#,##0}", item.JumlahTerkirim);
                        chartData.JumlahSelesai = string.Format("{0:#,##0}", item.JumlahSelesai);
                    }

                    totalSuratSatker += item.JumlahSurat;
                    jumlahSuratSatkerSelesai += item.JumlahSelesai;
                }
            }
            else
            {
                chartDataGaugePersonal.TotalSuratPersonal = 0;
                chartDataGaugePersonal.JumlahSelesaiPersonal = 0;
            }

            chartDataGaugeSatker.TotalSuratSatker = totalSuratSatker;
            chartDataGaugeSatker.JumlahSelesaiSatker = jumlahSuratSatkerSelesai;

            chartData.chartGaugeSatker = chartDataGaugeSatker;
            chartData.chartGaugePersonal = chartDataGaugePersonal;

            //chartData.SuratBelumDibuka = persuratanmodel.JumlahSuratBelumDibukaDenganTipe(satkerid,pegawaiid, functions.MyProfiles(pegawaiid, kantorid));

            chartData.SuratBelumDibuka = persuratanmodel.JumlahSuratBelumDibukaDenganTipeV2(unitkerjaid, pegawaiid);

            #endregion

            return View(chartData);
        }

        [HttpPost]
        public JsonResult GetNotifBadge()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            int cSuratMasuk = OtorisasiUser.getCountSurat("Masuk");
            int cSuratInisiatif = OtorisasiUser.getCountSurat("Inisiatif");
            int cProsesSurat = OtorisasiUser.getCountProsesSurat();
            int cProsesTTE = OtorisasiUser.getCountProsesTTE();
            int cProsesDraft = decimal.ToInt32(new Models.NaskahDinasModel().GetJumlahPersetujuanKonsep(usr.UserId));
            int cPengaduan = OtorisasiUser.getCountPengaduan();
            int cRapatOnline = OtorisasiUser.getCountRapatOnline();
            int cTotal = cProsesSurat + cProsesDraft + cProsesTTE + cSuratMasuk + cSuratInisiatif + cPengaduan + cRapatOnline;

            return Json(new
            {
                status = cTotal > 0,
                countData = new
                {
                    cProsesSurat = new
                    {
                        total = string.Format("{0:#,##0}", cProsesSurat),
                        title = "Jumlah Menunggu Persetujuan",
                        href = Url.Action("ProsesSurat", "Flow")
                    },
                    cProsesDraft = new
                    {
                        total = string.Format("{0:#,##0}", cProsesDraft),
                        title = "Jumlah Koordinasi Konsep Naskah Dinas",
                        href = Url.Action("ProsesList", "NaskahDinas")
                    },
                    cProsesTTE = new
                    {
                        total = string.Format("{0:#,##0}", cProsesTTE),
                        title = "Jumlah Permohonan TTE",
                        href = Url.Action("ProsesTTE", "TandaTanganElektronik")
                    },
                    cSuratMasuk = new
                    {
                        total = string.Format("{0:#,##0}", cSuratMasuk),
                        title = "Jumlah Surat Masuk",
                        href = Url.Action("KotakMasuk", "Surat")
                    },
                    cSuratInisiatif = new
                    {
                        total = string.Format("{0:#,##0}", cSuratInisiatif),
                        title = "Jumlah Surat Inisiatif",
                        href = Url.Action("SuratInisiatif", "Flow")
                    },
                    cPengaduan = new
                    {
                        total = string.Format("{0:#,##0}", cPengaduan),
                        title = "Jumlah Surat Pengaduan",
                        href = Url.Action("ListPengaduan", "Pengaduan")
                    },
                    cRapatOnline = new
                    {
                        total = string.Format("{0:#,##0}", cRapatOnline),
                        title = "Jumlah Rapat Online",
                        href = Url.Action("ListRapatOnline", "Meeting")
                    }
                },
                total = cTotal,
                totalstring = string.Format("{0:#,##0}", cTotal)
            });
        }

        public ActionResult GetIPAddress()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            Response.AppendHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var ipAdd = string.Empty;
            string last = string.Empty;
            try
            {
                ipAdd = Request.ServerVariables["LOCAL_ADDR"];
                if (string.IsNullOrEmpty(ipAdd))
                {
                    ipAdd = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                if (string.IsNullOrEmpty(ipAdd))
                {
                    ipAdd = Request.ServerVariables["HTTP_CLIENT_IP"];
                }
                if (string.IsNullOrEmpty(ipAdd))
                {
                    ipAdd = Request.ServerVariables["REMOTE_ADDR"];
                }
                if (string.IsNullOrEmpty(ipAdd))
                {
                    ipAdd = Request.ServerVariables.ToString();
                }
                else
                {
                    string[] data = ipAdd.Split('.');
                    last = data.LastOrDefault();
                }
            }
            catch
            {
                ipAdd = string.Empty;
            }
            return Json(ipAdd, JsonRequestBehavior.AllowGet);
        }
    }
}