//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Mime;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using Newtonsoft.Json;
//using QRCoder;
//using Surat.Models;
//using Surat.Models.Entities;
//using iTextSharp.text;
//using Surat.Codes;
//using System.Text.RegularExpressions;
//using iTextSharp.text.pdf.security;
//using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
//using PDFEditor;

//namespace Surat.Controllers
//{
//    [AccessDeniedAuthorize]
//    public class EvidenceSPBEController : Controller
//    {
//        DataMasterModel dataMasterModel = new DataMasterModel();
//        TandaTanganElektronikModel mdl = new TandaTanganElektronikModel();
//        SuratModel surat = new SuratModel();
//        Functions functions = new Functions();
//        bool isDev = OtorisasiUser.NamaSkema.Equals("surattrain");

//        string EndSPBE = string.Empty;// ConfigurationManager.AppSettings["UrlSPBE"].ToString();

//        public class ReturnSPBE
//        {
//            public bool status { get; set; }
//            public string message { get; set; }
//            public List<ReturnDataSPBE> data { get; set; }
//        }
//        public class ReturnDataSPBE
//        {
//            public string id { get; set; }
//            public string label { get; set; }
//        }

//        public ActionResult getListJenisDokumen()
//        {
//            var usr = User.Identity as InternalUserIdentity;
//            var data = new List<ComboList>();
//            var address = string.Concat(EndSPBE, "/ext/jenis");
//            var request = WebRequest.Create(address);
//            //request.Headers.Add("ssoUserId", usr.UserId);
//            request.Headers.Add("ssoUserId", isDev?"CD58967FF5B70CB7E0400B0A9A145D33": usr.UserId);
//            request.Method = "GET";
//            try
//            {
//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//                var responseStream = response.GetResponseStream();
//                HttpStatusCode wRespStatusCode = response.StatusCode;
//                WebHeaderCollection responseHeader = request.GetResponse().Headers;
//                var _data = new List<ReturnDataSPBE>();
//                using (var reader = new StreamReader(responseStream))
//                {
//                    string responseFromServer = reader.ReadToEnd();
//                    if (!String.IsNullOrEmpty(responseFromServer))
//                    {
//                        ReturnSPBE responseResult = JsonConvert.DeserializeObject<ReturnSPBE>(responseFromServer);
//                        if (responseResult.status)
//                        {
//                            foreach(var _dt in responseResult.data)
//                            {
//                                data.Add(new ComboList() { Text = _dt.label, Value = _dt.id });
//                            }
//                        }
//                    }
//                }
//            }
//            catch(Exception ex)
//            {
//                var msg = ex.Message;
//            }
//            return Json(data);
//        }

//        public ActionResult getListIndikator()
//        {
//            var data = new List<ComboList>();
//            var usr = User.Identity as InternalUserIdentity;
//            var address = string.Concat(EndSPBE, "/ext/indikator");
//            var request = WebRequest.Create(address);
//            request.Headers.Add("ssoUserId", isDev ? "CD58967FF5B70CB7E0400B0A9A145D33" : usr.UserId);
//            request.Method = "GET";
//            try
//            {
//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//                var responseStream = response.GetResponseStream();
//                HttpStatusCode wRespStatusCode = response.StatusCode;
//                WebHeaderCollection responseHeader = request.GetResponse().Headers;
//                var _data = new List<ReturnDataSPBE>();
//                using (var reader = new StreamReader(responseStream))
//                {
//                    string responseFromServer = reader.ReadToEnd();
//                    if (!String.IsNullOrEmpty(responseFromServer))
//                    {
//                        ReturnSPBE responseResult = JsonConvert.DeserializeObject<ReturnSPBE>(responseFromServer);
//                        if (responseResult.status)
//                        {
//                            foreach (var _dt in responseResult.data)
//                            {
//                                data.Add(new ComboList() { Text = _dt.label, Value = _dt.id });
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                var msg = ex.Message;
//            }
//            return Json(data);
//        }

//        public ActionResult getListTag()
//        {
//            var data = new List<ComboList>();
//            var usr = User.Identity as InternalUserIdentity;
//            var address = string.Concat(EndSPBE, "/ext/hashtag");
//            var request = WebRequest.Create(address);
//            request.Headers.Add("ssoUserId", isDev ? "CD58967FF5B70CB7E0400B0A9A145D33" : usr.UserId);
//            request.Method = "GET";
//            try
//            {
//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//                var responseStream = response.GetResponseStream();
//                HttpStatusCode wRespStatusCode = response.StatusCode;
//                WebHeaderCollection responseHeader = request.GetResponse().Headers;
//                var _data = new List<ReturnDataSPBE>();
//                using (var reader = new StreamReader(responseStream))
//                {
//                    string responseFromServer = reader.ReadToEnd();
//                    if (!String.IsNullOrEmpty(responseFromServer))
//                    {
//                        ReturnSPBE responseResult = JsonConvert.DeserializeObject<ReturnSPBE>(responseFromServer);
//                        if (responseResult.status)
//                        {
//                            foreach (var _dt in responseResult.data)
//                            {
//                                data.Add(new ComboList() { Text = _dt.label, Value = _dt.id });
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                var msg = ex.Message;
//            }
//            return Json(data);
//        }

//        [HttpPost]
//        //public JsonResult KirimDokumenSPBE(string did, string ind, string tag)
//        public JsonResult KirimDokumenSPBE(FormCollection frmobj)
//        {
//            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
//            string did = frmobj["dokumenid"] ?? string.Empty;
//            string jns = frmobj["ddlJenisDokumen"] ?? string.Empty;
//            string ind = frmobj["ddlIndikator"] ?? string.Empty; 
//            string tag = frmobj["ddlHashtag"] ?? string.Empty;
//            if (string.IsNullOrEmpty(jns))
//            {
//                tr.Pesan = "Jenis Dokumen Wajib Dipilih";
//            }
//            else
//            {
//                if (OtorisasiUser.isTU())
//                {
//                    var usr = User.Identity as InternalUserIdentity;

//                    try
//                    {
//                        tr = KirimEvidenceSPBE(usr, did, jns, ind, tag).Result;
//                    }
//                    catch (Exception ex)
//                    {
//                        tr.Pesan = ex.Message;
//                    }
//                }
//                else tr.Pesan = "Akun anda tidak memiliki hak akses";
//            }

//            return Json(tr, JsonRequestBehavior.AllowGet);
//        }

//        private async Task<TransactionResult> KirimEvidenceSPBE(InternalUserIdentity usr, string dokid, string jenis, string indikator, string tag)
//        {
//            TransactionResult result = new TransactionResult() { Status = false, Pesan = "" };
//            string userid = usr.UserId;
//            string pegawaid = usr.PegawaiId;
//            string nama = usr.NamaPegawai;

//            var _dok = mdl.GetKodeFile(dokid);
//            var data = mdl.GetDokumenElektronik(dokid);

//            if (_dok.Status)
//            {
//                string kode = _dok.Pesan;
//                var kvp = new[] {
//                    new KeyValuePair<string, string>("judul",data.NomorSurat),
//                    new KeyValuePair<string, string>("documentId",dokid),
//                    new KeyValuePair<string, string>("versi",dokid),
//                    new KeyValuePair<string, string>("jenisID", jenis),
//                    new KeyValuePair<string, string>("indikatorId", indikator),
//                    new KeyValuePair<string, string>("hashtagId", tag)
//                };

//                var address = string.Format(EndSPBE + "/ext/dokumen?{0}", string.Join("&", kvp.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))));
//                try
//                {
//                    var request = WebRequest.Create(address);
//                    request.Headers.Add("ssoUserId", isDev ? "CD58967FF5B70CB7E0400B0A9A145D33" : usr.UserId);
//                    request.Method = "POST";
//                    var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
//                    request.ContentType = "multipart/form-data; boundary=" + boundary;
//                    boundary = "--" + boundary;

//                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//                    var responseStream = response.GetResponseStream();
//                    HttpStatusCode wRespStatusCode = response.StatusCode;
//                    WebHeaderCollection responseHeader = request.GetResponse().Headers;
//                    using (var reader = new StreamReader(responseStream))
//                    {
//                        string responseFromServer = reader.ReadToEnd();
//                        if (!String.IsNullOrEmpty(responseFromServer))
//                        {
//                            var responseResult = JsonConvert.DeserializeObject<ReturnSPBE>(responseFromServer);
//                            result.Status = responseResult.status;
//                            result.Pesan = responseResult.message;
//                        }
//                    }
//                }
//                catch (WebException wex)
//                {
//                    using (var stream = wex.Response.GetResponseStream())
//                    using (var reader = new StreamReader(stream))
//                    {
//                        string responseFromServer = reader.ReadToEnd();
//                        if (!String.IsNullOrEmpty(responseFromServer))
//                        {
//                            result.Status = false;
//                            var responseResult = JsonConvert.DeserializeObject<ReturnSPBE>(responseFromServer);
//                            result.Pesan = responseResult.message;
//                        }
//                        else
//                        {
//                            result.Status = false;
//                            result.Pesan = "Server SPBE tidak mengirimkan pesan apapun";
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    result.Pesan = ex.Message;
//                    result.Status = false;
//                }
//            }
//            else
//            {
//                result.Pesan = "Kode Dokumen Gagal Dibuat";
//            }

//            return result;
//        }
//    }
//}