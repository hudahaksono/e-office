using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Surat
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public override void Init()
        {
            this.PostAuthenticateRequest += MvcApplication_PostAuthenticateRequest;
            base.Init();
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                Models.Entities.InternalUserData ud = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Entities.InternalUserData>(authTicket.UserData);
                var userlogin = new Models.Entities.InternalUserIdentity(authTicket);
                userlogin.PegawaiId = ud.pegawaiid;
                userlogin.KantorId = ud.kantorid;
                userlogin.NamaKantor = ud.namakantor;
                userlogin.ProfileIdTU = ud.profileidtu;
                userlogin.UnitKerjaId = ud.unitkerjaid;
                userlogin.UserId = ud.userid;
                userlogin.UserName = ud.username;
                userlogin.Email = ud.email;
                userlogin.Password = ud.password;
                userlogin.NamaPegawai = ud.namapegawai;

                var prin = new System.Security.Principal.GenericPrincipal(userlogin, ud.userroles.ToArray());
                HttpContext.Current.User = prin;
            }
        }
    }
}