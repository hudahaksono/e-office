using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Surat.Codes;
using Surat.Models;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class ExpiredTTEController : Controller
    {
        ExpiredTTEModel model = new ExpiredTTEModel();
        Functions functions = new Functions();
        [HttpPost]
        public JsonResult GetExpiredNotif()
        {
            var userData = functions.claimUser();
            var expired = model.GetExpired(userData.Email);

            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");

            return Json(expired);
        }

        [HttpPost]
        public JsonResult ReadExpiredNotif()
        {
            var userData = functions.claimUser();
            var isSuccessUpdate = model.UpdateExpired(userData.Email);

            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");

            return Json(new { Success = isSuccessUpdate });
        }
    }
}