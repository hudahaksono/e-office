using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Surat
{
    public class AccessDeniedAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                UriBuilder urlbld = new UriBuilder(filterContext.HttpContext.Request.Url);
                //urlbld.Port = -1;
                string returnUrl = urlbld.Uri.ToString();
                string loginUrl = FormsAuthentication.LoginUrl;
                filterContext.Result = new RedirectResult(loginUrl);
                //filterContext.Result = new RedirectResult(loginUrl + "?returnUrl=" + HttpContext.Current.Server.UrlEncode(returnUrl));
                return;
            }

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                //filterContext.Result = new RedirectResult("~/Account/Denied");
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}