using iTextSharp.text;
using iTextSharp.text.pdf;
using Surat.Models;
using Surat.Models.Entities;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Surat.Controllers
{
    public class KontenController : Controller
    {
        private string KontenPPNPN = "http://10.20.20.75/api/Document/";
        public ActionResult GetPdf(string filename)
        {
            var path = Server.MapPath(@"~/Contents/" + filename);

            var fileStream = new FileStream(path,
                                             FileMode.Open,
                                             FileAccess.Read
                                           );
            var fsResult = new FileStreamResult(fileStream, "application/pdf");
            return fsResult;
        }

        public ActionResult DocViewer()
        {
            return PartialView("DocViewer");
        }

        public ActionResult DocViewerSP()
        {
            return PartialView("DocViewerSP");
        }

        public ActionResult ImageViewer()
        {
            return PartialView("ImageViewer");
        }

        public ActionResult ImageViewerWithoutHeader()
        {
            return PartialView("ImageViewerWithoutHeader");
        }

        public ActionResult WordExcelViewer()
        {
            return PartialView("WordExcelViewer");
        }

        public ActionResult DocViewerWithoutHeader()
        {
            return PartialView("DocViewerWithoutHeader");
        }


        public async Task<ActionResult> getPhotoPPNPN(string id)
        {
            var _k = new Konten();
            var defaultAvatarAsBase64 = "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";
            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = "980FECFC746D8C80E0400B0A9214067D";
                _k = KontentModel.getDataFotoPPNPN(id);
                if(_k != null)
                {
                    _k.EKSTENSI = _k.EKSTENSI.Substring(0, 1).Equals(".") ? _k.EKSTENSI.Trim() : string.Concat(".", _k.EKSTENSI.Trim());
                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent(_k.TIPE), "tipeDokumen");
                    content.Add(new StringContent(_k.DOKID), "dokumenId");
                    content.Add(new StringContent(_k.EKSTENSI), "fileExtension");
                    content.Add(new StringContent(_k.VERSI.ToString()), "versionNumber");

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new Uri(string.Concat(KontenPPNPN, "Retrieve"));

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var reqresult = client.SendAsync(reqmessage).Result;
                            if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                            {
                                var strm = await reqresult.Content.ReadAsStreamAsync();
                                var docfile = new FileStreamResult(strm, "image/jpeg");
                                if (_k.EKSTENSI == ".jpg" || _k.EKSTENSI == ".jpeg")
                                {
                                    docfile = new FileStreamResult(strm, "image/jpeg");
                                }
                                else if (_k.EKSTENSI == ".png")
                                {
                                    docfile = new FileStreamResult(strm, "image/png");
                                }
                                docfile.FileDownloadName = string.Concat("fotoppnpn", _k.EKSTENSI);

                                return docfile;
                            }
                            else
                            {
                                var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
                                var imageStream = new MemoryStream(bytes);
                                return new FileStreamResult(imageStream, "image/jpeg");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
                        var imageStream = new MemoryStream(bytes);
                        return new FileStreamResult(imageStream, "image/jpeg");
                    }
                }
                else
                {
                    var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
                    var imageStream = new MemoryStream(bytes);
                    return new FileStreamResult(imageStream, "image/jpeg");
                }
            }
            else
            {
                var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
                var imageStream = new MemoryStream(bytes);
                return new FileStreamResult(imageStream, "image/jpeg");
            }
        }
        public FileStreamResult WebsiteImage(string url)
        {
            var defaultAvatarAsBase64 = "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";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var imageStream = new MemoryStream();
            try
            {
                byte[] imageData = null;

                using (var wc = new WebClient())
                    imageData = wc.DownloadData(url);

                imageStream =  new MemoryStream(imageData);
                //var imageUrl = url;
                //var request = (HttpWebRequest)WebRequest.Create(imageUrl);
                //var response = (HttpWebResponse)request.GetResponse();

                //if (response.StatusCode != HttpStatusCode.OK) throw new Exception("GOTO_CATCH");
                //using (var stream = response.GetResponseStream())
                //{
                //    using (var tempStream = new MemoryStream())
                //    {
                //        stream.CopyTo(tempStream);
                //        imageStream = new MemoryStream(tempStream.ToArray());
                //    }
                //}
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
                var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
                imageStream = new MemoryStream(bytes);
            }

            var result = new FileStreamResult(imageStream, "image/jpeg");

            result.FileDownloadName = "profile.jpg";

            return result;
        }

        public async Task<ActionResult> getPhotoProfile(string id)
        {
            var _k = new Konten();
            var usr = HttpContext.User.Identity as InternalUserIdentity;
            var defaultAvatarAsBase64 = "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";
            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = "980FECFC746D8C80E0400B0A9214067D";
                var _tr = new DataMasterModel().GetTipeUser(usr.PegawaiId, usr.KantorId);
                if (_tr.Status)
                {
                    var _ket = string.Concat(_tr.ReturnValue,"||",usr.PegawaiId);
                    _k = KontentModel.getKontenEoffice("PHOTOPROFILE", kantorid, _ket);
                    if (_k != null)
                    {
                        content.Add(new StringContent(kantorid), "kantorId");
                        content.Add(new StringContent(_k.TIPE), "tipeDokumen");
                        content.Add(new StringContent(_k.DOKID), "dokumenId");
                        content.Add(new StringContent(_k.EKSTENSI), "fileExtension");
                        content.Add(new StringContent(_k.VERSI.ToString()), "versionNumber");

                        reqmessage.Method = HttpMethod.Post;
                        reqmessage.Content = content;
                        reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Retrieve"));

                        try
                        {
                            using (var client = new HttpClient())
                            {
                                var reqresult = client.SendAsync(reqmessage).Result;
                                if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
                                {
                                    var strm = await reqresult.Content.ReadAsStreamAsync();
                                    var docfile = new FileStreamResult(strm, "image/jpeg");
                                    if (_k.EKSTENSI.ToLower().Replace(".","").Equals("jpg") || _k.EKSTENSI.ToLower().Replace(".", "").Equals("jpeg"))
                                    {
                                        docfile = new FileStreamResult(strm, "image/jpeg");
                                    }
                                    else if (_k.EKSTENSI.ToLower().Replace(".", "").Equals("png"))
                                    {
                                        docfile = new FileStreamResult(strm, "image/png");
                                    }
                                    docfile.FileDownloadName = string.Concat("photoprofile", _k.EKSTENSI);

                                    return docfile;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = ex.Message;
                        }
                    }
                }
            }
            var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
            var imageStream = new MemoryStream(bytes);
            return new FileStreamResult(imageStream, "image/jpeg");
        }

        public string getBase64(string url)
        {
            string base64 = string.Empty;
            var defaultAvatarAsBase64 = "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";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            return base64;
        }
    }
}