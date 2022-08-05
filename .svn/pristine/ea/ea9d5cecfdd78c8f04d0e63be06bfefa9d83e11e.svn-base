using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

[assembly: OwinStartup(typeof(Surat.Startup))]

namespace Surat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            PDFEditor.Init.Setup();

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "ATRBPN2020",
                        ValidAudience = "absenorpeg",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("K@J8HE28hei2uhei28e98H@*^#@&*khajhg"))
                    }
                });
            var configuration = new HubConfiguration
            {
                EnableJSONP = true
            };
            app.MapSignalR(configuration);

        }
    }
}