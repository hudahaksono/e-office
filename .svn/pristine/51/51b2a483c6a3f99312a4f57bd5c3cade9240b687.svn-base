using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Threading.Tasks;
using Surat.Models.Entities;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Net.Http.Headers;
using System.Web;

namespace Surat.Controllers
{
    public class WebApiController : ApiController
    {
        Models.WebApiModel webapimodel = new Models.WebApiModel();

        //[HttpPost]
        //public HttpResponseMessage Login(string nama_pengguna, string kata_sandi)
        //{
        //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

        //    WebApiUser res = new WebApiUser();


        //    OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

        //    if (provider.ValidateUser(nama_pengguna, kata_sandi))
        //    {
        //        res = webapimodel.GetWebApiUser(nama_pengguna);

        //        res.pesan = "sukses";
        //    }
        //    else
        //    {
        //        res.pesan = "gagal login";
        //    }

        //    var json = new JavaScriptSerializer().Serialize(res);

        //    result.Content = new StringContent(json);

        //    return result;
        //}

        [HttpPost]
        public async Task<HttpResponseMessage> Login() // API Lama
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            WebApiUser res = new WebApiUser();

            var nama_pengguna = string.Empty;
            var kata_sandi = string.Empty;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "nama_pengguna":
                            nama_pengguna = await stream.ReadAsStringAsync();
                            break;
                        case "kata_sandi":
                            kata_sandi = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

                string pass = provider.EncodePassword(kata_sandi);
                res = webapimodel.GetWebApiUser(nama_pengguna, pass);
                if (string.IsNullOrEmpty(res.kantorid))
                {
                    res.pesan = "gagal login";
                }
                else
                {
                    res.auth_token = generateToken(nama_pengguna, res.id_pegawai);
                    res.pesan = "sukses";
                }
                /*
                if (provider.ValidateUser(nama_pengguna, kata_sandi))
                {
                    res = webapimodel.GetWebApiUser(nama_pengguna);
                    res.auth_token = generateToken(nama_pengguna, res.id_pegawai);
                    res.pesan = "sukses";
                }
                else
                {
                    res.pesan = "gagal login";
                }
                */

                var json = new JavaScriptSerializer().Serialize(res);

                result.Content = new StringContent(json);
            } 
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> DataSurat() // API Lama
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            DataSurat res = new DataSurat();

            var nama_pengguna = string.Empty;
            var kata_sandi = string.Empty;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "nama_pengguna":
                            nama_pengguna = await stream.ReadAsStringAsync();
                            break;
                        case "kata_sandi":
                            kata_sandi = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];
                var dt = provider.ValidateLogin(nama_pengguna, kata_sandi);
                if (dt.Status)
                {
                    res = webapimodel.GetDataSurat(nama_pengguna);

                    res.pesan = "sukses";
                }else
                {
                    res.pesan = "gagal";
                }
                //if (provider.ValidateUser(nama_pengguna, kata_sandi))
                //{
                //    res = webapimodel.GetDataSurat(nama_pengguna);

                //    res.pesan = "sukses";
                //}
                //else
                //{
                //    res.pesan = "gagal";
                //}

                var json = new JavaScriptSerializer().Serialize(res);

                result.Content = new StringContent(json);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        //token
        public static string ShiftString(string t)
        {
            return t.Substring(1, t.Length - 1) + t.Substring(0, 1);
        }


        [HttpPost]
        public async Task<HttpResponseMessage> getToken()
        {
            
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var res = "";

            var nip = string.Empty;
            
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "nip":
                            nip = await stream.ReadAsStringAsync();
                            break;
                       
                        default:
                            break;
                    }
                }

                String s = "b4ikEN2m5U";
                for(int i=0; i<nip.Length; i++)
                {
                    res += s[nip[i] - '0'];
                }

               // res = nip[nip.Length-1].ToString();
                

                var json = new JavaScriptSerializer().Serialize(res);

                result.Content = new StringContent(json);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        public string generateToken(String nama_pengguna, string id_pegawai)
        {
            string SecKey = "K@J8HE28hei2uhei28e98H@*^#@&*khajhg";
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var claims = new List<Claim>();
            claims.Add(new Claim("nama_pengguna", nama_pengguna));
            claims.Add(new Claim("id_pegawai", id_pegawai));

            //create token
            var token = new JwtSecurityToken(
                    issuer: "ATRBPN2020",
                    audience: "absenorpeg",
                    expires: DateTime.Now.AddYears(1),
                    signingCredentials: signingCredentials
                    , claims: claims
                );
            
            var tokens = new JwtSecurityTokenHandler().WriteToken(token);
            return tokens;
        }

        public string getnip()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var name = claims.Where(p => p.Type == "nama_pengguna").FirstOrDefault()?.Value;
                return name;

            }
            return null;
        }

        public string get_id_peg()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var name = claims.Where(p => p.Type == "id_pegawai").FirstOrDefault()?.Value;
                return name;

            }
            return null;
        }

        [Authorize]
        [HttpPost]
        public HttpResponseMessage Validate()
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var nama_pengguna = getnip();
            var id_pegawai = get_id_peg();
            TokenValid res = new TokenValid();
            res.nama_pengguna = getnip();
            res.id_pegawai = get_id_peg();
            res.status = "sukses";
            var json = new JavaScriptSerializer().Serialize(res);
            result.Content = new StringContent(json);

            return result;

        }

        [Authorize]
        [HttpPost]
        public HttpResponseMessage DataSuratV2()
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var identity = getnip();

            DataSurat res = new DataSurat();
            res = webapimodel.GetDataSurat(identity);
            res.pesan = "sukses";
            var json = new JavaScriptSerializer().Serialize(res);
            result.Content = new StringContent(json);

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> SettingSignature()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            TransactionResult res = new TransactionResult();

            var userid = string.Empty;
            var pegawaiid = string.Empty;
            var kantorid = string.Empty;
            var ext = string.Empty;
            byte[] file = null;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
          
            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "userid":
                            userid = await stream.ReadAsStringAsync();
                            break;
                        case "pegawaiid":
                            pegawaiid = await stream.ReadAsStringAsync();
                            break;
                        case "kantorid":
                            kantorid = await stream.ReadAsStringAsync();
                            break;
                        case "file":
                            file = await stream.ReadAsByteArrayAsync();
                            break;
                        case "ext":
                            ext = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

                if(file == null)
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim File Digital Tandatangan [file]";
                }
                else if (string.IsNullOrEmpty(userid))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Id User [userid]";
                }
                else if (string.IsNullOrEmpty(pegawaiid))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim NIP [pegawaiid]";
                }
                else if (string.IsNullOrEmpty(kantorid))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Id Kantor [kantorid]";
                }
                else if (string.IsNullOrEmpty(ext))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Tipe File [ext]";
                }
                else
                {
                    var nama = webapimodel.GetPegawaiNameByUserId(userid);
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    var tipe = "SpesimenTTD";
                    var id = webapimodel.NewGuID();
                    var versi = "0";
                    if (ext.Substring(0, 1) != ".") ext = string.Concat(".", ext);
                    var filename = string.Concat(pegawaiid,ext);
                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(ext), "fileExtension");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(new MemoryStream(file.ToArray())), "file", filename);
                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceBaseUrl"].ToString(), "Store"));

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        res.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
                        res.Pesan = "Storage Server - " + reqresult.ReasonPhrase;
                        if (res.Status)
                        {
                            res = webapimodel.SettingSignature(id, userid, nama, kantorid, tipe, filename, ext, pegawaiid);
                        }
                    }
                }

                var json = new JavaScriptSerializer().Serialize(res);

                result.Content = new StringContent(json);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> OnlineAttendanceMeeting()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            TransactionResult res = new TransactionResult();

            var userid = string.Empty;
            var meetingcode = string.Empty;
            var longitude = string.Empty;
            var latitude = string.Empty;


            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "userid":
                            userid = await stream.ReadAsStringAsync();
                            break;
                        case "meetingcode":
                            meetingcode = await stream.ReadAsStringAsync();
                            break;
                        case "longitude":
                            longitude = await stream.ReadAsStringAsync();
                            break;
                        case "latitude":
                            latitude = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

                if (string.IsNullOrEmpty(userid))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Id User [userid]";
                }
                else if (string.IsNullOrEmpty(meetingcode))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Kode Rapat [meetingcode]";
                }
                else if (string.IsNullOrEmpty(longitude))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Longitude Pengguna [longitude]";
                }
                else if (string.IsNullOrEmpty(latitude))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Latitude Pengguna [latitude]";
                }
                else
                {
                    res = webapimodel.MeetingAttend(meetingcode, userid, longitude, latitude);
                }

                var json = new JavaScriptSerializer().Serialize(res);

                result.Content = new StringContent(json);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> GetFotoKantor()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            TransactionResult res = new TransactionResult();

            var kantorid = string.Empty;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "kantorid":
                            kantorid = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];
                Stream strm = null;
                if (string.IsNullOrEmpty(kantorid))
                {
                    res.Status = false;
                    res.Pesan = "Harap Kirim Id Kantor [kantorid]";
                }
                else
                {
                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();
                    string tipe = "Foto";
                    string versi = "0";
                    string ext = webapimodel.GetTipeFile(kantorid, tipe, kantorid);

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent(tipe), "tipeDokumen");
                    content.Add(new StringContent(kantorid), "dokumenId");
                    content.Add(new StringContent(ext), "fileExtension");
                    content.Add(new StringContent(versi), "versionNumber");

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceBaseUrl"].ToString(), "Retrieve"));

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var reqresult = client.SendAsync(reqmessage).Result;
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                            {
                                strm = reqresult.Content.ReadAsStreamAsync().Result;

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    strm.CopyTo(ms);
                                    res.ByteResult = ms.ToArray();
                                }
                                res.Status = true;
                                res.ReturnValue = ext;
                                /*
                                result.Content = new StreamContent(strm);
                                if (ext == ".jpg" || ext == ".jpeg")
                                {
                                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                                }
                                else if (ext == ".png")
                                {
                                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                                }
                                */
                            }
                            else
                            {
                                res.Status = false;
                                res.Pesan = "Foto Tidak Ditemukan";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Status = false;
                        res.Pesan = "Terjadi Masalah Saat Mengambil Gambar";
                    }
                }
                var json = new JavaScriptSerializer().Serialize(res);

                result.Content = new StringContent(json);
                //if (strm == null)
                //{
                //    var json = new JavaScriptSerializer().Serialize(res);

                //    result.Content = new StringContent(json);
                //}
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> LoginV2()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var res = new List<WebApiUser>();

            var nama_pengguna = string.Empty;
            var kata_sandi = string.Empty;

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "nama_pengguna":
                            nama_pengguna = await stream.ReadAsStringAsync();
                            break;
                        case "kata_sandi":
                            kata_sandi = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)Membership.Providers["OracleMembershipProvider"];
                var rst = provider.ValidateLogin(nama_pengguna, kata_sandi);
                if (rst.Status)
                {
                    string userid = rst.Pesan;
                    string tipe = rst.ReturnValue;
                    res = webapimodel.GetDataLogin(userid, tipe);
                    if (res.Count > 0)
                    {
                        var ct = res.Count() > 1;
                        foreach(var dt in res)
                        {
                            dt.tipe_user = tipe.Equals("PEGAWAI") ? "ASN" :"PPNPN";
                            dt.auth_token = generateToken(nama_pengguna, dt.id_pegawai);
                            dt.pesan = "sukses";
                            dt.id_pengguna = userid;
                            dt.nama_pengguna = nama_pengguna;
                            if (ct)
                            {
                                if (dt.kantorid.Equals("980FECFC746D8C80E0400B0A9214067D"))
                                {
                                    var unitkerja = new Models.DataMasterModel().GetUnitKerja(dt.unitkerjaid, "", "", true, true, true, "", 1, 1);
                                    dt.namakantor = string.Concat(unitkerja[0].NamaUnitKerja, " [", dt.jabatan, "]");
                                }
                                else
                                {
                                    dt.namakantor = string.Concat(dt.namakantor," [", dt.jabatan, "]");
                                }
                            }
                        }
                        var json = new JavaScriptSerializer().Serialize(res);
                        result.Content = new StringContent(json);
                    }
                    else
                    {
                        var json = new JavaScriptSerializer().Serialize(new { pesan = "Akun belum memiliki jabatan" });
                        result.Content = new StringContent(json);
                    }
                }
                else
                {
                    var json = new JavaScriptSerializer().Serialize(new { pesan = "Nama Pengguna dengan Kata Sandi tidak ditemukan" });
                    result.Content = new StringContent(json);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> MainBadge()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            DataSurat res = new DataSurat();

            var userid = string.Empty;
            var unitkerjaid = string.Empty;
            var tipe = string.Empty;
            var jabatanid = string.Empty;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "userid":
                            userid = await stream.ReadAsStringAsync();
                            break;
                        case "unitkerjaid":
                            unitkerjaid = await stream.ReadAsStringAsync();
                            break;
                        case "tipe":
                            tipe = await stream.ReadAsStringAsync();
                            break;
                        case "jabatanid":
                            jabatanid = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)Membership.Providers["OracleMembershipProvider"];

                if (string.IsNullOrEmpty(userid))
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { Status = false, Pesan = "Harap Kirim User Id [userid]" }));
                }
                else if (string.IsNullOrEmpty(unitkerjaid))
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { Status = false, Pesan = "Harap Kirim Unit Kerja Id [unitkerjaid]" }));
                }
                else if (string.IsNullOrEmpty(tipe))
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { Status = false, Pesan = "Harap Kirim Unit Tipe Akun [tipe]" }));
                }
                else if (string.IsNullOrEmpty(jabatanid))
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { Status = false, Pesan = "Harap Kirim Jabatan Id Akun [jabatanid]" }));
                }
                else
                {
                    res = webapimodel.GetMainbadge(userid, unitkerjaid, tipe, jabatanid);
                    if (string.IsNullOrEmpty(res.jabatan))
                    {
                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { Status = false, Pesan = "Data Pegawai Tidak Sesuai" }));
                    }
                    else
                    {
                        var json = new JavaScriptSerializer().Serialize(res);

                        result.Content = new StringContent(json);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> LoginV3()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var res = new List<WebApiUser>();

            var nama_pengguna = string.Empty;
            var kata_sandi = string.Empty;
            var ip_public = string.Empty;
            var target = string.Empty;

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "nama_pengguna":
                            nama_pengguna = await stream.ReadAsStringAsync();
                            break;
                        case "kata_sandi":
                            kata_sandi = await stream.ReadAsStringAsync();
                            break;
                        case "ip_public":
                            ip_public = await stream.ReadAsStringAsync();
                            break;
                        case "target":
                            target = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }
                var authValue = Request.Headers.Authorization;
                string _username = string.IsNullOrEmpty(ConfigurationManager.AppSettings["API_Username"]) ? "kkp2web" : ConfigurationManager.AppSettings["API_Username"].ToString();
                string _password = string.IsNullOrEmpty(ConfigurationManager.AppSettings["API_Password"]) ? "securetransactions!!!" : ConfigurationManager.AppSettings["API_Password"].ToString();
                string _link = string.IsNullOrEmpty(ConfigurationManager.AppSettings["PageToken"]) ? string.Concat(Request.RequestUri.Scheme, "://", Request.RequestUri.Authority, "/account/token") : ConfigurationManager.AppSettings["PageToken"].ToString();
                if (authValue != null && !string.IsNullOrWhiteSpace(authValue.Parameter))
                {
                    string _auth = Convert.ToBase64String(Encoding.Default.GetBytes(_username + ":" + _password));
                    if (!_auth.Equals(authValue.Parameter))
                    {
                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Authkey Tidak Sesuai" }));
                    }
                    else
                    {
                        OracleMembershipProvider provider = (OracleMembershipProvider)Membership.Providers["OracleMembershipProvider"];
                        ip_public = string.IsNullOrEmpty(ip_public)?HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : ip_public: ip_public;
                        bool isDev = false;
                        if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin.Equals("surattrain") && !string.IsNullOrEmpty(target) && target.Equals("dev"))
                        {
                            isDev = true;
                            var dateNow = DateTime.Today;
                            kata_sandi = string.Concat("Eoffice", (dateNow.DayOfWeek + 100).ToString().Substring(1), (dateNow.Month + 100).ToString().Substring(1));
                        }

                        if (string.IsNullOrEmpty(nama_pengguna) || string.IsNullOrEmpty(kata_sandi) || string.IsNullOrEmpty(ip_public))
                        {
                            result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "parameter wajib tidak lengkap" }));
                        }
                        else
                        {
                            if (new Codes.Functions().isIp(ip_public))
                            {
                                var rst = provider.ValidateLogin(nama_pengguna, kata_sandi);
                                if (rst.Status)
                                {
                                    var cu = new Models.InternalUser();
                                    string userid = rst.Pesan;
                                    string tipe = rst.ReturnValue;
                                    var _email = cu.GetEmail(userid);
                                    var _namapengguna = cu.GetNamaPengguna(userid);
                                    if (!string.IsNullOrEmpty(_email))
                                    {
                                        if (!_email.Equals("no-reply@atrbpn.go.id"))
                                        {
                                            bool doLogin = false;
                                            if (isDev)
                                            {
                                                doLogin = true;
                                            }
                                            else
                                            {
                                                var _t = cu.GetToken(userid, ip_public, _email);
                                                if (!string.IsNullOrEmpty(_t.Token))
                                                {
                                                    string _path = System.Web.Hosting.HostingEnvironment.MapPath("\\");
                                                    var _result = new AccountController().SendEmailToken(_link, _path, _t, _email, _namapengguna, ip_public);
                                                    var tr = cu.InsertLogEmail("Eoffice", "Kirim Token Login", "Login OTP", "service.eoffice@atrbpn.go.id", _email);
                                                    if (tr.Status)
                                                    {
                                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = string.Concat("Konfirmasi Login Telah dikirim ke ", new Codes.Functions().HideEmail(_email)) }));
                                                    }
                                                    else
                                                    {
                                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = tr.Pesan }));
                                                    }
                                                }
                                                else
                                                {
                                                    doLogin = true;
                                                }
                                            }

                                            if (doLogin)
                                            {
                                                res = webapimodel.GetDataLogin(userid, tipe);
                                                if (res.Count > 0)
                                                {
                                                    var ct = res.Count() > 1;
                                                    foreach (var dt in res)
                                                    {
                                                        dt.tipe_user = tipe.Equals("PEGAWAI") ? "ASN" : "PPNPN";
                                                        dt.auth_token = generateToken(nama_pengguna, dt.id_pegawai);
                                                        dt.pesan = "sukses";
                                                        dt.id_pengguna = userid;
                                                        dt.nama_pengguna = nama_pengguna;
                                                        if (ct)
                                                        {
                                                            if (dt.kantorid.Equals("980FECFC746D8C80E0400B0A9214067D"))
                                                            {
                                                                var unitkerja = new Models.DataMasterModel().GetUnitKerja(dt.unitkerjaid, "", "", true, true, true, "", 1, 1);
                                                                dt.namakantor = string.Concat(unitkerja[0].NamaUnitKerja, " [", dt.jabatan, "]");
                                                            }
                                                            else
                                                            {
                                                                dt.namakantor = string.Concat(dt.namakantor, " [", dt.jabatan, "]");
                                                            }
                                                        }
                                                    }
                                                    var json = new JavaScriptSerializer().Serialize(res);
                                                    result.Content = new StringContent(json);
                                                }
                                                else
                                                {
                                                    var json = new JavaScriptSerializer().Serialize(new { pesan = "Akun belum memiliki jabatan" });
                                                    result.Content = new StringContent(json);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Email tidak ditemukan, Pastikan Anda terdaftar di Akun Pertanahan" }));
                                    }
                                }
                            }
                            else
                            {
                                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "IP Public Tidak Valid" }));
                            }
                            /*
                            var rst = provider.ValidateLogin(nama_pengguna, kata_sandi);
                            if (rst.Status)
                            {
                                string userid = rst.Pesan;
                                string tipe = rst.ReturnValue;
                                var _email = cu.GetEmail(userId);
                                if (rst.Token == null)
                                {
                                    List<OfficeMember> user;
                                    var cu = new InternalUser();
                                    user = cu.GetOfficeActive(userid, tipe);
                                    if (user.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(target))
                                        {
                                            var res = new WebAkun();
                                            var ham = new HakAksesModel();
                                            res.status = true;
                                            res.userid = userid;
                                            res.pegawaiid = user[0].PegawaiId;
                                            res.namapegawai = user[0].NamaPegawai;
                                            res.tipeuser = tipe;
                                            res.akses = new List<AkunUnit>();
                                            foreach (var dt in user)
                                            {
                                                //var userRoles = ham.GetRoleJabatanForUser(res.pegawaiid, dt.KantorId);
                                                var _akses = cu.GetAkunUnit(res.pegawaiid, dt.KantorId);
                                                foreach (var _a in _akses)
                                                {
                                                    res.akses.Add(new AkunUnit()
                                                    {
                                                        kantorid = dt.KantorId,
                                                        namakantor = dt.NamaKantor,
                                                        tipekantorid = dt.TipeKantorId,
                                                        unitkerjaid = _a.unitkerjaid,
                                                        namaunitkerja = _a.namaunitkerja,
                                                        jabatanid = _a.jabatanid,
                                                        jabatan = _a.jabatan,
                                                        isPlt = _a.isPlt,
                                                        latitude = _a.latitude,
                                                        longitude = _a.longitude,
                                                        zona = _a.zona
                                                    });
                                                }
                                            }
                                            var json = new JavaScriptSerializer().Serialize(res);
                                            result.Content = new StringContent(json);
                                        }
                                        else if (target.Equals("SPBE"))
                                        {
                                            var res = new AkunSPBE();
                                            var ham = new HakAksesModel();
                                            res.status = true;
                                            res.userid = userid;
                                            res.pegawaiid = user[0].PegawaiId;
                                            res.namapegawai = user[0].NamaPegawai;
                                            res.email = user[0].Email;
                                            res.phone = user[0].Phone;
                                            res.tipeuser = tipe;
                                            res.akses = new List<AkunUnitSPBE>();
                                            foreach (var dt in user)
                                            {
                                                var _akses = cu.GetAkunUnitSPBE(res.pegawaiid, dt.KantorId);
                                                foreach (var _a in _akses)
                                                {
                                                    _a.admin = cu.checkJabatan(user[0].PegawaiId, dt.KantorId, "A80200");
                                                }
                                                res.akses.AddRange(_akses);
                                            }
                                            var json = new JavaScriptSerializer().Serialize(res);
                                            result.Content = new StringContent(json);
                                        }
                                    }
                                    else
                                    {
                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Akun tidak memiliki kantor aktif" }));
                                    }
                                }
                                else
                                {
                                    var _internal = new InternalUser();
                                    //var _email = "arya.itdev@gmail.com";;
                                    var _email = _internal.GetEmail(userid);
                                    var _ip = model._IpPublic;
                                    var _durasi = _internal.GetDurasi(userid, _ip);
                                    if (string.IsNullOrEmpty(_email))
                                    {
                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Email tidak ditemukan" }));
                                    }
                                    string _path = System.Web.Hosting.HostingEnvironment.MapPath("\\");

                                    var _result = new AccountController().SendEmailToken(_link, _path, rst.Token, _email, model.UserName, _ip, _durasi);
                                    var tr = _internal.InsertLogEmail("HomeKKP", "Kirim Token Login", model.UserName, "noreply.kkp@atrbpn.go.id", _email);
                                    if (tr.Status)
                                    {
                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = string.Concat("Konfirmasi Login Telah dikirim ke ", _email) }));
                                    }
                                    else
                                    {
                                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = tr.Pesan }));
                                    }
                                }
                            }
                            else
                            {
                                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Nama Pengguna dengan Kata Sandi tidak ditemukan" }));
                            }
                            */
                        }
                    }
                }
                else
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "AuthKey Not Found" }));
                }
            }
            catch (Exception ex)
            {
                Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = ex.Message }));
            }

            return result;
        }

        //[HttpPost]
        //public async Task<HttpResponseMessage> UploadExpoSertifikat2020()
        //{
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    TransactionResult res = new TransactionResult();

        //    var namaacara = string.Empty;
        //    var tanggalacara = string.Empty;
        //    var namapeserta = string.Empty;
        //    var sertifikatid = string.Empty;
        //    var ext = string.Empty;
        //    byte[] file = null;

        //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

        //    try
        //    {
        //        var reqContent = await Request.Content.ReadAsMultipartAsync();

        //        foreach (var stream in reqContent.Contents)
        //        {
        //            switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
        //            {
        //                case "namaacara":
        //                    namaacara = await stream.ReadAsStringAsync();
        //                    break;
        //                case "tanggalacara":
        //                    tanggalacara = await stream.ReadAsStringAsync();
        //                    break;
        //                case "namapeserta":
        //                    namapeserta = await stream.ReadAsStringAsync();
        //                    break;
        //                case "sertifikatid":
        //                    sertifikatid = await stream.ReadAsStringAsync();
        //                    break;
        //                case "file":
        //                    file = await stream.ReadAsByteArrayAsync();
        //                    break;
        //                case "ext":
        //                    ext = await stream.ReadAsStringAsync();
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //        OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

        //        DateTime test;
        //        if (file == null)
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim File Digital Tandatangan [file]";
        //        }
        //        else if (string.IsNullOrEmpty(namaacara))
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim Nama Acara [namaacara]";
        //        }
        //        else if (string.IsNullOrEmpty(tanggalacara))
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim Tanggal Acara [tanggalacara], format : DD/MM/YYYY";
        //        }
        //        else if (!DateTime.TryParseExact(tanggalacara, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out test))
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim Tanggal Acara [tanggalacara], format : DD/MM/YYYY";
        //        }
        //        else if (string.IsNullOrEmpty(namapeserta))
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim Nama Peserta [namapeserta]";
        //        }
        //        else if (string.IsNullOrEmpty(sertifikatid))
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim Id Sertifikat [sertifikatid]";
        //        }
        //        else if (string.IsNullOrEmpty(ext))
        //        {
        //            res.Status = false;
        //            res.Pesan = "Harap Kirim Tipe File [ext]";
        //        }
        //        else
        //        {
        //            var reqmessage = new HttpRequestMessage();
        //            var content = new MultipartFormDataContent();
        //            var tipe = "ExpoSertipikat";
        //            var id = webapimodel.NewGuID();
        //            var versi = "0";
        //            if (ext.Substring(0, 1) != ".") ext = string.Concat(".", ext);
        //            var filename = string.Concat(sertifikatid, ext);
        //            content.Add(new StringContent("980FECFC746D8C80E0400B0A9214067D"), "kantorId");
        //            content.Add(new StringContent(tipe), "tipeDokumen");
        //            content.Add(new StringContent(id), "dokumenId");
        //            content.Add(new StringContent(ext), "fileExtension");
        //            content.Add(new StringContent(versi.ToString()), "versionNumber");
        //            content.Add(new StreamContent(new MemoryStream(file.ToArray())), "file", filename);
        //            reqmessage.Method = HttpMethod.Post;
        //            reqmessage.Content = content;
        //            reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

        //            using (var client = new HttpClient())
        //            {
        //                var reqresult = client.SendAsync(reqmessage).Result;
        //                res.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK;
        //                res.Pesan = "Storage Server - " + reqresult.ReasonPhrase;
        //                if (res.Status)
        //                {
        //                    var sertipikat = new ExpoSertipikat();
        //                    sertipikat.FileName = filename;
        //                    sertipikat.NamaPeserta = namapeserta;
        //                    sertipikat.ExpoSertipikatId = id;
        //                    sertipikat.UserUnggah = "Eventy";
        //                    sertipikat.UserTTE = "0f5f5e5a-d1c9-41c8-a6f7-b83eba019be4";
        //                    sertipikat.NamaAcara = namaacara;
        //                    sertipikat.TanggalAcara = DateTime.ParseExact(tanggalacara, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

        //                    res = new Models.TandaTanganElektronikModel().SimpanPengajuanExpoSertipikat(sertipikat);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        res.Status = false;
        //        res.Pesan = ex.Message;
        //    }

        //    var json = new JavaScriptSerializer().Serialize(res);

        //    result.Content = new StringContent(json);

        //    return result;
        //}

        [HttpPost]
        public async Task<HttpResponseMessage> LoginAdmin()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var res = new List<WebApiUser>();

            var nama_pengguna = string.Empty;
            var kata_sandi = string.Empty;

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "nama_pengguna":
                            nama_pengguna = await stream.ReadAsStringAsync();
                            break;
                        case "kata_sandi":
                            kata_sandi = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }

                OracleMembershipProvider provider = (OracleMembershipProvider)Membership.Providers["OracleMembershipProvider"];
                var rst = provider.ValidateLogin(nama_pengguna, kata_sandi);
                if (rst.Status)
                {
                    string userid = rst.Pesan;
                    string tipe = rst.ReturnValue;
                    res = webapimodel.GetDataAdmin(userid, tipe);
                    if (res.Count > 0)
                    {
                        var feed = "[";
                        var ct = 0;
                        foreach (var dt in res)
                        {
                            var userData =
                                "{\"pegawaiid\":\"" + dt.id_pegawai +
                                "\",\"namapegawai\":\"" + dt.namapegawai +
                                "\",\"namakantor\":\"" + dt.namakantor + "\"}";
                            feed += string.Concat(ct > 0?",":"", userData);
                            dt.tipe_user = tipe.Equals("PEGAWAI") ? "ASN" : "PPNPN";
                            dt.auth_token = generateToken(nama_pengguna, dt.id_pegawai);
                            dt.pesan = "sukses";
                            dt.id_pengguna = userid;
                            dt.nama_pengguna = nama_pengguna;
                            ct += 1;
                        }
                        feed += "]";
                        var json = feed;// new JavaScriptSerializer().Serialize(feed);
                        result.Content = new StringContent(json);
                    }
                    else
                    {
                        var json = new JavaScriptSerializer().Serialize(new { pesan = "Akun bukan Administrator" });
                        result.Content = new StringContent(json);
                    }
                }
                else
                {
                    var json = new JavaScriptSerializer().Serialize(new { pesan = "Nama Pengguna dengan Kata Sandi tidak ditemukan" });
                    result.Content = new StringContent(json);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ListUnitKerja()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var param1 = string.Empty;
            var param2 = string.Empty;

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "param1":
                            param1 = await stream.ReadAsStringAsync();
                            break;
                        case "param2":
                            param2 = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }
                if (string.IsNullOrEmpty(param1))
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "parameter wajib tidak lengkap" }));
                }
                else
                {
                    var authValue = Request.Headers.Authorization;
                    string _username = string.IsNullOrEmpty(ConfigurationManager.AppSettings["API_Username"]) ? "kkp2web" : ConfigurationManager.AppSettings["API_Username"].ToString();
                    string _password = string.IsNullOrEmpty(ConfigurationManager.AppSettings["API_Password"]) ? "securetransactions!!!" : ConfigurationManager.AppSettings["API_Password"].ToString();
                    if (authValue != null && !string.IsNullOrWhiteSpace(authValue.Parameter))
                    {
                        string _auth = Convert.ToBase64String(Encoding.Default.GetBytes(_username + ":" + _password));
                        if (!_auth.Equals(authValue.Parameter))
                        {
                            result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Authkey Tidak Sesuai" }));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(param1))
                            {
                                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "parameter wajib tidak lengkap" }));
                            }
                            else
                            {
                                var dt = new List<ComboList>();
                                if (!string.IsNullOrEmpty(param2))
                                {
                                    switch (param2)
                                    {
                                        case "SPBE":
                                            if (param1.Equals("1"))
                                                dt = new List<ComboList>();// webapimodel.getListUnitKerjaSPBE();
                                            break;
                                        case "EP":
                                            dt = webapimodel.getListUnitKerja(param1);
                                            break;
                                    }
                                }

                                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = true, data = dt }));
                            }
                        }
                    }
                    else
                    {
                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "AuthKey Not Found" }));
                    }
                }
            }
            catch (Exception ex)
            {
                Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = ex.Message }));
            }

            return result;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ListAkun()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var param1 = string.Empty;
            var param2 = string.Empty;

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var reqContent = await Request.Content.ReadAsMultipartAsync();

                foreach (var stream in reqContent.Contents)
                {
                    switch (stream.Headers.ContentDisposition.Name.Replace("\"", string.Empty).ToLower())
                    {
                        case "param1":
                            param1 = await stream.ReadAsStringAsync();
                            break;
                        case "param2":
                            param2 = await stream.ReadAsStringAsync();
                            break;
                        default:
                            break;
                    }
                }
                if (string.IsNullOrEmpty(param1))
                {
                    result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "parameter wajib tidak lengkap" }));
                }
                else
                {
                    var authValue = Request.Headers.Authorization;
                    string _username = string.IsNullOrEmpty(ConfigurationManager.AppSettings["API_Username"]) ? "kkp2web" : ConfigurationManager.AppSettings["API_Username"].ToString();
                    string _password = string.IsNullOrEmpty(ConfigurationManager.AppSettings["API_Password"]) ? "securetransactions!!!" : ConfigurationManager.AppSettings["API_Password"].ToString();
                    if (authValue != null && !string.IsNullOrWhiteSpace(authValue.Parameter))
                    {
                        string _auth = Convert.ToBase64String(Encoding.Default.GetBytes(_username + ":" + _password));
                        if (!_auth.Equals(authValue.Parameter))
                        {
                            result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "Authkey Tidak Sesuai" }));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(param1))
                            {
                                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "parameter wajib tidak lengkap" }));
                            }
                            else
                            {
                                var dt = new List<ComboList>();
                                if (!string.IsNullOrEmpty(param2))
                                {
                                    switch (param2)
                                    {
                                        case "EP":
                                            dt = webapimodel.getListAkun(param1);
                                            break;
                                    }
                                }

                                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = true, data = dt }));
                            }
                        }
                    }
                    else
                    {
                        result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = "AuthKey Not Found" }));
                    }
                }
            }
            catch (Exception ex)
            {
                Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                result.Content = new StringContent(new JavaScriptSerializer().Serialize(new { status = false, pesan = ex.Message }));
            }

            return result;
        }
    }
}