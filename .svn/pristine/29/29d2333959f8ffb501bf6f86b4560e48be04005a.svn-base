using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class MonitoringController : Controller
    {
        Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

        public ActionResult DalamProses()
        {
            return View();
        }

        public ActionResult DaftarRekapSurat()
        {
            var result = new List<Models.Entities.RekapSurat>();
            decimal? total = 0;

            string unitkerjaid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).UnitKerjaId;
            string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
            string profileidtu = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).ProfileIdTU;
            string tipe = Request.Form["tipe"].ToString();
            string load = Request.Form["load"].ToString();


            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            if (!String.IsNullOrEmpty(unitkerjaid))
            {
                if (!string.IsNullOrEmpty(load) && load.Equals("1"))
                {
                    result = persuratanmodel.GetRekapSurat(unitkerjaid, satkerid, tipe);
                }

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }
    }
}