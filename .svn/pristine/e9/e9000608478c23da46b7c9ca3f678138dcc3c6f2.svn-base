using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using Newtonsoft.Json;
using Surat.Codes;
using Surat.Models;
using Surat.Models.Entities;

namespace Surat.Controllers
{
    public class AccountController : Controller
    {
        Functions functions = new Functions();
        public ActionResult Index()
        {
            ViewBag.AlertMess = "";
            string _token = new Codes.Functions().RndCode(8);
            ViewBag.Token = _token;
            ViewBag.TokenEx = new DataMasterModel().EncodePassword(_token);
            return View();
        }

        public ActionResult CountDownHTOnline()
        {
            return View();
        }

        [AccessDeniedAuthorize]
        public ActionResult AkunSaya()
        {
            DataMasterModel dataMasterModel = new DataMasterModel();
            var usr = functions.claimUser();

            string pegawaiid = usr.PegawaiId;
            string kantorid = usr.KantorId;

            var akunsaya = new AkunSaya();

            var inuser = new InternalUserData();
            inuser.userid = usr.UserId;
            inuser.namapegawai = usr.NamaPegawai;
            inuser.pegawaiid = usr.PegawaiId;
            inuser.email = usr.Email;
            //inuser.password = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).Password;
            //inuser.konfirmasipassword = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).Password;

            //Models.Entities.TransactionResult tr = dataMasterModel.GetTipeUser(pegawaiid, kantorid);
            //if (tr.Status)
            //{
            //    string tipe = tr.ReturnValue;
            //    akunsaya.tipeakun = tipe;
            //    akunsaya.nomortelepon = dataMasterModel.GetPhone(pegawaiid, tipe);
            //}
            akunsaya.tipeakun = dataMasterModel.GetTipeAkun(inuser.pegawaiid, inuser.userid);
            akunsaya.nomortelepon = dataMasterModel.GetPhone(inuser.pegawaiid, akunsaya.tipeakun);
            akunsaya.ListMyProfile = dataMasterModel.GetProfilesByPegawaiId(pegawaiid, kantorid);
            //if (datauserlogin.Count > 0)
            //{
            //    inuser.namapegawai = string.IsNullOrEmpty(datauserlogin[0].NamaLengkap)? datauserlogin[0].NamaLengkap:inuser.namapegawai;
            //    akunsaya.nomortelepon = datauserlogin[0].NomorTelepon;
            //    akunsaya.username = string.IsNullOrEmpty(datauserlogin[0].Username)? datauserlogin[0].Username:inuser.username;
            //    akunsaya.email = string.IsNullOrEmpty(datauserlogin[0].Email)? datauserlogin[0].Email:inuser.email;
            //}
            //else
            //{
            //    akunsaya.username = inuser.username;
            //    akunsaya.email = inuser.email;
            //}
            akunsaya.DataUserData = inuser;

            return View(akunsaya);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        public string GetClientIpValue()
        {
            var ipAdd = string.Empty;
            try
            {
                ipAdd = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
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
            }
            catch
            {
                ipAdd = string.Empty;
            }

            return ipAdd;
        }

        [HttpPost]
        public ActionResult MasukAkun(AccountModel model, string returnUrl)
        {
            var pesan = "Gagal login. Nama pengguna atau Kata sandi yang Anda masukan salah.";
            ViewBag.Token = model.Token;
            ViewBag.TokenEx = model.TokenEx;
            if (ModelState.IsValid)
            {
                if(!string.IsNullOrEmpty(model.Token) && !string.IsNullOrEmpty(model.TokenEx) && model.TokenEx.Equals(new DataMasterModel().EncodePassword(model.Token)))
                {
                    var provider = (OracleMembershipProvider)Membership.Providers["OracleMembershipProvider"];
                    var result = provider.ValidateLogin(model.UserName, model.Password);
                    if (result.Status)
                    {
                        return SetupFormsAuthTicket(result.Pesan, result.ReturnValue, model.RememberMe, returnUrl);
                    }
                    else
                    {
                        pesan = result.Pesan;
                    }
                }
                else
                {
                    pesan = "Anda Mencoba Login Secara Tidak Resmi";
                }
                /*
                if (provider.ValidateUser(model.UserName, model.Password))
                {
                    return SetupFormsAuthTicket(model.UserName, model.RememberMe, returnUrl);
                }
                */
            }

            // If we got this far, something failed, redisplay form
            string _token = new Codes.Functions().RndCode(8);
            ViewBag.Token = _token;
            ViewBag.TokenEx = new DataMasterModel().EncodePassword(_token);
            ViewBag.AlertMess = pesan;
            return View("Index");
        }

        private ActionResult SetupFormsAuthTicket(string userId, string tipe, bool persistanceFlag, string returnUrl)
        {
            var cu = new InternalUser();
            var user = cu.GetOffices(userId,tipe);
            string _token = new Codes.Functions().RndCode(8);
            ViewBag.Token = _token;
            ViewBag.TokenEx = new DataMasterModel().EncodePassword(_token);

            if (user.Count == 1)
            {
                var dataMasterModel = new DataMasterModel();

                var profiletu = dataMasterModel.GetProfileTUByNipAndKantorID(user[0].PegawaiId, user[0].KantorId); //Arya :: 2020-07-22
                if (profiletu == null)
                {
                    // Cek apakah punya profileid TU ?
                    profiletu = dataMasterModel.GetProfileTUByNip(user[0].PegawaiId, user[0].KantorId);
                    if (profiletu == null)
                    {
                        ViewBag.AlertMess = "Gagal login. Login Anda belum memiliki ID Profile Tata Usaha.";
                        return View("Index");
                    }

                    // Cek apakah punya Unit Kerja ?
                    profiletu.UnitKerjaId = dataMasterModel.GetUnitKerjaIdByNip(user[0].PegawaiId, user[0].KantorId);
                    if (string.IsNullOrEmpty(profiletu.UnitKerjaId))
                    {
                        ViewBag.AlertMess = "Gagal login. Login Anda belum memiliki ID Unit Kerja.";
                        return View("Index");
                    }
                }

                string _unitkerjaid = profiletu.UnitKerjaId.Substring(0, 2).Equals("02") ? profiletu.UnitKerjaId : profiletu.UnitKerjaId.Substring(2, profiletu.UnitKerjaId.Length - 2);
                DateTime StartRaker = new DateTime(2022, 07, 26);
                DateTime EndRaker = new DateTime(2022, 07, 29);
                if(DateTime.Today > EndRaker || DateTime.Today < StartRaker)
                {
                    //if (_unitkerjaid.Equals("020116") && OtorisasiUser.NamaSkema.Equals("surat"))
                    if (_unitkerjaid.Length >= 4 && (_unitkerjaid.Substring(0, 4).Equals("0201") || _unitkerjaid.Substring(0, 4).Equals("0210") || _unitkerjaid.Substring(0, 4).Equals("0211") || _unitkerjaid.Substring(0, 4).Equals("0212") || _unitkerjaid.Substring(0, 4).Equals("0213") || _unitkerjaid.Substring(0, 4).Equals("0214") || _unitkerjaid.Substring(0, 4).Equals("0229")) && OtorisasiUser.NamaSkema.Equals("surat"))
                    {
                        string ipPublic = GetClientIpValue();

                        if (new Codes.Functions().isIp(ipPublic))
                        {
                            var _email = cu.GetEmail(userId);// user[0].Email;
                            if (!string.IsNullOrEmpty(_email))
                            {
                                if (!_email.Equals("no-reply@atrbpn.go.id"))
                                {
                                    var _t = cu.GetToken(userId, ipPublic, _email);
                                    if (!string.IsNullOrEmpty(_t.Token))
                                    {
                                        var _result = SendEmail(_t.Token, _email, user[0].NamaPegawai, ipPublic, _t.Durasi);
                                        var tr = cu.InsertLogEmail("Eoffice", "Kirim Token Login", "Login OTP", "login.eoffice@atrbpn.go.id", _email);
                                        if (tr.Status)
                                        {
                                            var token = new VerifikasiToken();
                                            token.UserId = userId;
                                            token.Tipe = tipe;
                                            token.UserName = user[0].Username;
                                            token.KantorId = user[0].KantorId;
                                            token.ReturnUrl = returnUrl;
                                            token.Persistent = persistanceFlag;
                                            token.KirimKe = new Codes.Functions().HideEmail(_email);
                                            token.Ip = ipPublic;
                                            token.Durasi = _t.Durasi;
                                            return View("Verifikasi", token);
                                        }
                                        else
                                        {
                                            ViewBag.AlertMess = tr.Pesan;
                                            return View("Index");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.AlertMess = "Email tidak ditemukan, Pastikan Anda terdaftar di Akun Pertanahan";
                                return View("Index");
                            }
                        }
                    }
                }

                var userData =
                    "{\"userid\":\"" + user[0].UserId +
                    "\",\"username\":\"" + user[0].Username +
                    "\",\"email\":\"" + user[0].Email +
                    "\",\"password\":\"" + user[0].Password +
                    "\",\"pegawaiid\":\"" + user[0].PegawaiId +
                    "\",\"kantorid\":\"" + user[0].KantorId +
                    "\",\"namakantor\":\"" + user[0].NamaKantor +
                    "\",\"profileidtu\":\"" + profiletu.ProfileIdTU +
                    "\",\"unitkerjaid\":\"" + _unitkerjaid +
                    "\",\"namapegawai\":\"" + user[0].NamaPegawai +
                    "\", \"userroles\":" + JsonConvert.SerializeObject(new List<string>().ToArray()) + "}";

                HttpCookie cookie = FormsAuthentication.GetAuthCookie(user[0].Username, true);
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                var newTicket = new FormsAuthenticationTicket(ticket.Version,
                    ticket.Name,
                    ticket.IssueDate,
                    ticket.Expiration,
                    persistanceFlag,
                    userData,
                    ticket.CookiePath);
                cookie.Value = FormsAuthentication.Encrypt(newTicket);
                cookie.Expires = DateTime.Now.AddHours(5);//newTicket.Expiration.AddHours(24);
                Response.Cookies.Set(cookie);

                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else if (!string.IsNullOrEmpty(returnUrl) && Codes.Security.cekWhitelist(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            else if (user.Count > 1)
            {
                SelectOffice a = new SelectOffice();
                a.OfficeList = user;
                a.SelectedOffice = user[0].KantorId;
                a.ReturnUrl = returnUrl;
                a.Persistent = persistanceFlag;
                a.UserId = userId;
                return View("ListKantor", a);
            }
            else
            {
                ViewBag.AlertMess = "Gagal login. Login Anda belum memiliki Satker.";
                return View("Index");
            }
        }

        public ActionResult SetKantor(SelectOffice m)
        {
            InternalUser cu = new InternalUser();
            OfficeMember user = cu.GetOffice(m.UserId, m.SelectedOffice);
            string _token = new Codes.Functions().RndCode(8);
            ViewBag.Token = _token;
            ViewBag.TokenEx = new DataMasterModel().EncodePassword(_token);

            HakAksesModel ham = new HakAksesModel();
            DataMasterModel dataMasterModel = new DataMasterModel();
            if (user == null)
            {
                ViewBag.AlertMess = "Gagal login. Akun Anda belum di aktifasi di kantor ini.";
                return View("Index");
            }

            var profiletu = dataMasterModel.GetProfileTUByNipAndKantorID(user.PegawaiId, user.KantorId); //Arya :: 2020-07-22
            if (profiletu == null)
            {
                // Cek apakah punya profileid TU ?
                profiletu = dataMasterModel.GetProfileTUByNip(user.PegawaiId, user.KantorId);
                if (profiletu == null)
                {
                    ViewBag.AlertMess = "Gagal login. Login Anda belum memiliki ID Profile Tata Usaha.";
                    return View("Index");
                }

                // Cek apakah punya Unit Kerja ?
                profiletu.UnitKerjaId = dataMasterModel.GetUnitKerjaIdByNip(user.PegawaiId, user.KantorId);
                if (string.IsNullOrEmpty(profiletu.UnitKerjaId))
                {
                    ViewBag.AlertMess = "Gagal login. Login Anda belum memiliki ID Unit Kerja.";
                    return View("Index");
                }
            }

            var userData =
                "{\"userid\":\"" + user.UserId +
                "\",\"username\":\"" + user.Username +
                "\",\"email\":\"" + user.Email +
                "\",\"password\":\"" + user.Password +
                "\",\"pegawaiid\":\"" + user.PegawaiId +
                "\",\"kantorid\":\"" + user.KantorId +
                "\",\"namakantor\":\"" + user.NamaKantor +
                "\",\"profileidtu\":\"" + profiletu.ProfileIdTU +
                "\",\"unitkerjaid\":\"" + (profiletu.UnitKerjaId.Substring(0, 2).Equals("02") ? profiletu.UnitKerjaId : profiletu.UnitKerjaId.Substring(2, profiletu.UnitKerjaId.Length - 2)) +
                "\",\"namapegawai\":\"" + user.NamaPegawai +
                "\", \"userroles\":" + JsonConvert.SerializeObject(new List<string>().ToArray()) + "}";

            HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(user.Username, true);
            var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version,
                ticket.Name,
                ticket.IssueDate,
                ticket.Expiration,
                m.Persistent,
                userData,
                ticket.CookiePath);
            cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
            cookie.Expires = DateTime.Now.AddHours(5);//newTicket.Expiration.AddHours(24);
            Response.Cookies.Set(cookie);

            if (Url.IsLocalUrl(m.ReturnUrl) && m.ReturnUrl.Length > 1 && m.ReturnUrl.StartsWith("/")
                    && !m.ReturnUrl.StartsWith("//") && !m.ReturnUrl.StartsWith("/\\"))
            {
                return Redirect(m.ReturnUrl);
            }
            else if (!string.IsNullOrEmpty(m.ReturnUrl) && Codes.Security.cekWhitelist(m.ReturnUrl))
            {
                return Redirect(m.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public JsonResult ListPegawaiByKantorProfile(string profileid)
        {
            List<Models.Entities.Pegawai> listpegawai = new List<Models.Entities.Pegawai>();

            if (!String.IsNullOrEmpty(profileid))
            {
                string kantorid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).KantorId;
                AccountModel model = new AccountModel();

                listpegawai = model.ListPegawaiByKantorProfile(kantorid, profileid);
            }

            return Json(listpegawai, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFotoPegawai()
        {
            string pegawaiid = (HttpContext.User.Identity as Models.Entities.InternalUserIdentity).PegawaiId;

            InternalUser model = new InternalUser();
            GetFotoPegawai getfotopegawai = model.GetFotoPegawai(pegawaiid);

            return Json(getfotopegawai, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Denied()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginAs(string uid, string tipe, string returnUrl)
        {
            if (OtorisasiUser.IsRoleAdministrator())
            {
                return SetupFormsAuthTicket(uid, tipe, false, returnUrl);
            }
            return View("Index");
        }

        public async Task<TransactionResult> SendEmail(string token, string emailtujuan, string namatujuan, string ip, decimal durasi)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            await Task.Run(() =>
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Kementerian Agraria dan Tata Ruang/Badan Pertanahan Nasional", "login.eoffice@atrbpn.go.id"));
                    message.To.Add(new MailboxAddress(namatujuan, emailtujuan));

                    message.Subject = "Kode Unik Login e-Office";
                    TimeSpan t = TimeSpan.FromSeconds((double)durasi);
                    string _durasi = durasi == 86400 ? "24 Jam" : string.Format("{0:D2}:{1:D2}:{2:D2}",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds);
                    string bodyMessageText = "Anda memperoleh email ini karena anda telah melakukan login pada ip " + ip + "<br /><br />Silahkan masukkan kode unik <b>" + token + "</b> pada konfirmasi kode unik di Halaman e-Office anda<br /><br />Kode Unik ini kadaluwarsa dalam " + _durasi + ".";

                    string mailText;
                    using (var sr = new StreamReader(Server.MapPath("~\\resources\\html\\template.html")))
                    {
                        mailText = sr.ReadToEnd();
                    }

                    mailText = mailText.Replace("{pemohon}", namatujuan);
                    mailText = mailText.Replace("{keterangan}", bodyMessageText);

                    var builder = new BodyBuilder();

                    var image = builder.LinkedResources.Add(Server.MapPath("~\\resources\\images\\logo.png"));
                    image.ContentId = MimeUtils.GenerateMessageId();
                    mailText = mailText.Replace("{logobpn}", string.Format(@"<img src=""cid:{0}"" style=""width:64px;height:64px;border:0;"" />", image.ContentId));

                    var imageBanner = builder.LinkedResources.Add(Server.MapPath("~\\resources\\images\\banner.jpg"));
                    imageBanner.ContentId = MimeUtils.GenerateMessageId();
                    mailText = mailText.Replace("{banner}", string.Format(@"<img src=""cid:{0}"" style=""display:block;"" />", imageBanner.ContentId));

                    string bodyMessage = mailText;
                    builder.HtmlBody = bodyMessage;

                    message.Body = builder.ToMessageBody();

                    using (var client = new SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        var smtpserver = ConfigurationManager.AppSettings["SmtpClient"].ToString();
                        client.Connect(smtpserver, 25, false);

                        client.Send(message);
                        client.Disconnect(true);
                    }

                    tr.Status = true;
                    tr.Pesan = "Pesan berhasil terkirim";
                }
                catch (Exception ex)
                {
                    tr.Pesan = ex.Message.ToString();
                }
            });

            return tr;
        }

        public async Task<TransactionResult> SendEmailToken(string link, string path, LogToken token, string emailtujuan, string namatujuan, string ip)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            await Task.Run(() =>
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Kementerian Agraria dan Tata Ruang/Badan Pertanahan Nasional", "service.eoffice@atrbpn.go.id"));
                    message.To.Add(new MailboxAddress(namatujuan, emailtujuan));

                    message.Subject = "Kode Unik Login e-Office";
                    TimeSpan t = TimeSpan.FromSeconds((int)token.Durasi);
                    string _durasi = token.Durasi == 86400 ? "24 Jam" : string.Format("{0:D2}:{1:D2}:{2:D2}",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds);
                    string _link = string.Concat(link, "?id=", token.Id);
                    string bodyMessageText = string.Concat("Anda memperoleh email ini karena anda telah melakukan login pada ip ", ip, "<br /><br />Silahkan masukkan kode unik <b>", token.Token, "</b><br />Pada halaman <a href='", _link, "'>Konfirmasi Login e-Office</a><br /><br />Kode Unik ini kadaluwarsa dalam ", _durasi, ".");

                    string mailText;
                    using (var sr = new StreamReader(string.Concat(path, "resources\\html\\template.html")))
                    {
                        mailText = sr.ReadToEnd();
                    }

                    mailText = mailText.Replace("{pemohon}", namatujuan);
                    mailText = mailText.Replace("{keterangan}", bodyMessageText);

                    var builder = new BodyBuilder();

                    var image = builder.LinkedResources.Add(string.Concat(path, "resources\\images\\logo.png"));
                    image.ContentId = MimeUtils.GenerateMessageId();
                    mailText = mailText.Replace("{logobpn}", string.Format(@"<img src=""cid:{0}"" style=""width:64px;height:64px;border:0;"" />", image.ContentId));

                    var imageBanner = builder.LinkedResources.Add(string.Concat(path, "resources\\images\\banner.jpg"));
                    imageBanner.ContentId = MimeUtils.GenerateMessageId();
                    mailText = mailText.Replace("{banner}", string.Format(@"<img src=""cid:{0}"" style=""display:block;"" />", imageBanner.ContentId));

                    string bodyMessage = mailText;
                    builder.HtmlBody = bodyMessage;

                    message.Body = builder.ToMessageBody();

                    using (var client = new SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        var smtpserver = ConfigurationManager.AppSettings["SmtpClient"].ToString();
                        client.Connect(smtpserver, 25, false);

                        client.Send(message);
                        client.Disconnect(true);
                    }

                    tr.Status = true;
                    tr.Pesan = "Pesan berhasil terkirim";
                }
                catch (Exception ex)
                {
                    tr.Pesan = ex.Message.ToString();
                }
            });

            return tr;
        }

        public ActionResult SetToken(VerifikasiToken model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                ViewBag.AlertMess = "Akun Tidak Ditemukan";
                return View("Index");
            }
            else
            {
                var _internal = new InternalUser();
                string _token = _internal.GetToken(model.UserId, model.Ip);
                if (_token.Equals(model.iToken))
                {
                    var tr = _internal.DaftarkanIP(model.UserId, model.Ip, _token);
                    if (tr.Status)
                    {
                        var _set = new SelectOffice();
                        _set.UserId = model.UserId;
                        _set.SelectedOffice = model.KantorId;
                        _set.Persistent = model.Persistent;
                        _set.ReturnUrl = model.ReturnUrl;
                        return SetKantor(_set);
                    }
                    else
                    {
                        var token = new VerifikasiToken();
                        token.UserId = model.UserId;
                        token.Tipe = model.Tipe;
                        token.UserName = model.UserName;
                        token.KantorId = model.KantorId;
                        token.ReturnUrl = model.ReturnUrl;
                        token.Persistent = model.Persistent;
                        token.KirimKe = model.KirimKe;
                        token.Ip = model.Ip;
                        token.Durasi = _internal.GetDurasi(model.UserId, model.Ip);
                        ViewBag.AlertMess = tr.Pesan;
                        return View("Verifikasi", token);
                    }
                }
                else
                {
                    var token = new VerifikasiToken();
                    token.UserId = model.UserId;
                    token.Tipe = model.Tipe;
                    token.UserName = model.UserName;
                    token.KantorId = model.KantorId;
                    token.ReturnUrl = model.ReturnUrl;
                    token.Persistent = model.Persistent;
                    token.KirimKe = model.KirimKe;
                    token.Ip = model.Ip;
                    token.Durasi = _internal.GetDurasi(model.UserId, model.Ip);
                    ViewBag.AlertMess = "Kode Unik Tidak Sesuai";
                    return View("Verifikasi", token);
                }
            }
        }

        public ActionResult Token(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.AlertMess = "Kode Unik Tidak Ditemukan";
                return View("Index");
            }
            else
            {
                var _internal = new InternalUser();
                var _token = _internal.GetTokenData(id);
                if (_token != null && !string.IsNullOrEmpty(_token.UserId))
                {
                    _token.Status = true;
                    _token.Durasi = _internal.GetDurasi(_token.UserId, _token.PublicIp);
                    _token.Email = new Codes.Functions().HideEmail(_token.Email);
                    return View(_token);
                }
                else
                {
                    _token = new DataToken();
                    _token.Status = false;
                    _token.Pesan = "Kode Unik Tidak Dikenali";
                    return View(_token);
                }
            }
        }

        public ActionResult KirimToken(string id, string ip, string it)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            if (string.IsNullOrEmpty(id))
            {
                result.Pesan = "Kode Pengenal Tidak Ditemukan";
            }
            else
            {
                var _internal = new InternalUser();
                string _token = _internal.GetToken(id, ip);
                if (_token.Equals(it))
                {
                    result = _internal.DaftarkanIP(id, ip, it);
                }
                else
                {
                    result.Pesan = "Kode Unik Tidak Sesuai";
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AccessDeniedAuthorize]
        public ActionResult FaceRecogV3()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            var usr = functions.claimUser();
            if (usr == null)
            {
                return RedirectToAction("Index", "Account");
            }
            ViewBag.Pegawaiid = usr.PegawaiId;
            return View();
        }

        [HttpPost]
        public JsonResult simpanPresensi(string pLong, string pLat, string pWaktu)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            if (usr == null)
            {
                tr.Status = false;
                tr.Pesan = "Harap Login Terlebih Dahulu";
            }
            else if (string.IsNullOrEmpty(pLong) || string.IsNullOrEmpty(pLat))
            {
                tr.Status = false;
                tr.Pesan = "Lokasi Perangkat Tidak Ditemukan";
            }
            else
            {
                var mDataMaster = new DataMasterModel();
                var mLaporan = new LaporanModel();
                var _internal = new InternalUser();

                var _unitkerja = mDataMaster.GetUnitKerjaDetail(usr.UnitKerjaId);
                var _locKantor = mLaporan.GetListLocationKantor(usr.KantorId);
                string st = "WFA";
                double jarak = 0;
                var _kantor = usr.NamaKantor;
                foreach (var _lk in _locKantor)
                {
                    if (st.Equals("WFA"))
                    {
                        jarak = functions.getDistance(pLat, pLong, _lk.Latitude, _lk.Longitude);
                        st = jarak <= 1000 ? "WFO" : "WFA";
                        if (st.Equals("WFO"))
                        {
                            _kantor = _lk.Nama;
                        }
                    }
                }
                if (st.Equals("WFA") && _locKantor.Count > 1)
                {
                    var _locUtama = mLaporan.GetListLocationKantor(usr.KantorId, true);
                    jarak = functions.getDistance(pLat, pLong, _locUtama[0].Latitude, _locUtama[0].Longitude);
                    st = jarak <= 1000 ? "WFO" : "WFA";
                }
                string _jarak = jarak <= 1000 ? string.Concat(jarak.ToString(), " m") : string.Concat(Math.Round((decimal)jarak / 1000, 2).ToString(), " km");
                tr = _internal.InsertLogPresensi(usr.PegawaiId, usr.KantorId, usr.UnitKerjaId, pLat, pLong, usr.UserId, pWaktu);
                tr.ReturnValue = jarak <= 1000 ? "WFO" : "WFA";
                tr.ReturnValue2 = string.Concat("Presensi Berhasil, <br/>Nama : ",usr.NamaPegawai, "<br/>Unit : ", _unitkerja.NamaUnitKerja,
                                                                   "<br/>Waktu : ", pWaktu, "<br/>Posisi : ", jarak <= 1000 ? "WFO" : "WFA", " (Radius ", _jarak, ")",
                                                                   "<br/>Lokasi Presensi : ", _kantor);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDistanceString(string pLong, string pLat)
        {
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            if (usr == null)
            {
                tr.Status = false;
                tr.Pesan = "Harap Login Terlebih Dahulu";
            }
            else if (string.IsNullOrEmpty(pLong) || string.IsNullOrEmpty(pLat))
            {
                tr.Status = false;
                tr.Pesan = "Lokasi Perangkat Tidak Ditemukan";
            }
            else
            {
                var mLaporan = new LaporanModel();
                var _locKantor = mLaporan.GetListLocationKantor(usr.KantorId);
                string st = "WFA";
                double jarak = 0;
                string tanggal = string.Empty;
                string waktu = string.Empty;
                var _kantor = usr.NamaKantor;
                foreach (var _lk in _locKantor)
                {
                    if (st.Equals("WFA"))
                    {
                        jarak = functions.getDistance(pLat, pLong, _lk.Latitude, _lk.Longitude);
                        st = jarak <= 1000 ? "WFO" : "WFA";
                        if (st.Equals("WFO"))
                        {
                            _kantor = _lk.Nama;
                        }
                        tanggal = _lk.ServerTime.ToString("dd/MM/yyyy");
                        waktu = _lk.ServerTime.ToLongTimeString().Replace(".", ":");
                    }
                }
                if (st.Equals("WFA") && _locKantor.Count > 1)
                {
                    var _locUtama = mLaporan.GetListLocationKantor(usr.KantorId,true);
                    jarak = functions.getDistance(pLat, pLong, _locUtama[0].Latitude, _locUtama[0].Longitude);
                    st = jarak <= 1000 ? "WFO" : "WFA";
                    tanggal = _locUtama[0].ServerTime.ToString("dd/MM/yyyy");
                    waktu = _locUtama[0].ServerTime.ToLongTimeString().Replace(".", ":");
                }
                string _jarak = jarak <= 1000 ? string.Concat(jarak.ToString(), " m") : string.Concat(Math.Round((decimal)jarak / 1000, 2).ToString(), " km");
                tr.Status = true;
                tr.ReturnValue = string.Concat(pLat, ", ", pLong, " (", _jarak, ")<br/>[", _kantor, "]");
                tr.ReturnValue2 = string.Concat(st,"|",_jarak,"|",_kantor, "|", waktu, "|", tanggal);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPresensi()
        {
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            if (usr == null)
            {
                tr.Status = false;
                tr.Pesan = "Harap Login Terlebih Dahulu";
            }
            else
            {
                var mLaporan = new LaporanModel();
                var _lastPresensi = mLaporan.getLastPresensi(usr.PegawaiId, usr.KantorId);
                if(_lastPresensi != null)
                {
                    var _locKantor = mLaporan.GetListLocationKantor(usr.KantorId);
                    string st = "WFA";
                    double jarak = 0;
                    var _kantor = usr.NamaKantor;
                    foreach (var _lk in _locKantor)
                    {
                        if (st.Equals("WFA"))
                        {
                            jarak = functions.getDistance(_lastPresensi.Latitude, _lastPresensi.Longitude, _lk.Latitude, _lk.Longitude);
                            st = jarak <= 1000 ? "WFO" : "WFA";
                            if (st.Equals("WFO"))
                            {
                                _kantor = _lk.Nama;
                            }
                        }
                    }
                    if (st.Equals("WFA") && _locKantor.Count > 1)
                    {
                        var _locUtama = mLaporan.GetListLocationKantor(usr.KantorId, true);
                        jarak = functions.getDistance(_lastPresensi.Latitude, _lastPresensi.Longitude, _locUtama[0].Latitude, _locUtama[0].Longitude);
                        st = jarak <= 1000 ? "WFO" : "WFA";
                    }
                    string _jarak = jarak <= 1000 ? string.Concat(jarak.ToString(), " m") : string.Concat(Math.Round((decimal)jarak / 1000, 2).ToString(), " km");
                    tr.Status = true;
                    tr.Pesan = string.Concat("Anda telah melakukan ", _lastPresensi.Catatan, ".\n", st, " dengan jarak ", _jarak, " dari ", _kantor, ".\nApakah anda mau melakukan presensi kembali?");
                    tr.ReturnValue = string.Concat(_lastPresensi.Latitude, ", ", _lastPresensi.Longitude, " (", _jarak, ")<br/>[", _kantor, "]");
                    tr.ReturnValue2 = string.Concat(st, "|", _jarak, "|", _kantor);
                }
                else
                {
                    tr.Status = true;
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        private double toRad(double val)
        {
            return val * Math.PI / 180;
        }
    }
}