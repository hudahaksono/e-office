using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using QRCoder;
using Surat.Models;
using Surat.Models.Entities;
using iTextSharp.text;
using Surat.Codes;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using PDFEditor;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class KKPController : Controller
    {
        DataMasterModel dataMasterModel = new DataMasterModel();
        PersuratanModel persuratanmodel = new PersuratanModel();
        KontentModel kontentmodel = new KontentModel();
        KKPModel mdl = new KKPModel();
        Functions functions = new Functions();
        
        public JsonResult GetProvinsiList(string kid)
        {
            var list = mdl.GetProvinsiList(kid);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWilayahList(string kid, string idk)
        {
            var list = mdl.GetWilayahList(kid,idk);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTahunList(string jns,string wid, string tip)
        {
            var list = mdl.GetTahunList(jns, wid, tip);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLinkSurat(int? draw, int? start, int? length, string suratid)
        {
            var result = new List<LinkSurat>();
            decimal? total = 0;

            if (!String.IsNullOrEmpty(suratid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = mdl.GetSuratLink(suratid, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDokumen(int? draw, int? start, int? length, string suratid, FormCollection frmobj)
        {
            var result = new List<DokumenSurat>();
            decimal? total = 0;
            string _jns = frmobj["ddlJenisDokumen"] ?? string.Empty;
            string _kid = frmobj["ddlKantorId"] ?? string.Empty;
            string _tbt = frmobj["ddlTipeHak"] ?? string.Empty;
            string _tsu = frmobj["ddlTipeSU"] ?? string.Empty;
            string _pro = frmobj["ddlProvinsi"] ?? string.Empty;
            string _kab = frmobj["ddlKabupaten"] ?? string.Empty;
            string _kec = frmobj["ddlKecamatan"] ?? string.Empty;
            string _kel = frmobj["ddlKelurahan"] ?? string.Empty;
            string _thn = frmobj["ddlTahun"] ?? string.Empty;
            string _nmr = frmobj["txtNomorDokumen"] ?? string.Empty;
            if (!string.IsNullOrEmpty(suratid) && !string.IsNullOrEmpty(_jns) &&
                !string.IsNullOrEmpty(_kid) && (!string.IsNullOrEmpty(_tbt) ||
                !string.IsNullOrEmpty(_tsu)) && !string.IsNullOrEmpty(_pro) &&
                !string.IsNullOrEmpty(_kab) && !string.IsNullOrEmpty(_kec) &&
                !string.IsNullOrEmpty(_kel))
            {
                string wilayahid = !string.IsNullOrEmpty(_kel) ? _kel.Substring(0, Math.Min(_kel.Length, 32)) : (!string.IsNullOrEmpty(_kec) ? _kec.Substring(0, Math.Min(_kec.Length, 32)) : (string.IsNullOrEmpty(_kab) ? "" : _kab.Substring(0, Math.Min(_kab.Length, 32))));

                if (_tbt == "BT2" || _tbt == "BT5")
                {
                    wilayahid = _kab.Substring(0, 32);
                }
                string tipe = string.Empty;
                switch (_jns)
                {
                    case "DOKUMENHAK":
                        tipe = _tbt;
                        break;
                    case "DOKUMENPENGUKURAN":
                        tipe = _tsu;
                        break;
                }

                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                result = mdl.GetDokumen(suratid, _kid, _jns, tipe, wilayahid, _nmr, _thn, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public class transStream
        {
            public bool Status { get; set; }
            public string Pesan { get; set; }
            public string stDS { get; set; }
        }

        [HttpPost]
        public JsonResult CekArsipV2(string did)
        {
            var result = new transStream();

            var konten = kontentmodel.getKontenAktif(did);
            if(konten != null)
            {
                string kantorid = konten.KANTORID;
                string tipe = konten.TIPE;
                string versi = konten.VERSI.ToString();
                string ext = konten.EKSTENSI;
                ext = string.IsNullOrEmpty(ext) ? ".pdf" : string.Concat(ext.Substring(0, 1).Equals(".") ? "" : ".", ext);

                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(did), "dokumenId");
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
                            result.Status = true;
                            var cek = new DigitalSignature().validasiStream(reqresult.Content.ReadAsStreamAsync().Result);
                            result.stDS = "Tidak";
                            if (cek.Status)
                            {
                                result.stDS = "Ya";
                            }
                            result.Pesan = "Dokumen Digital Ditemukan";
                        }
                        else
                        {
                            result.Status = false;
                            result.Pesan = "Dokumen Digital Tidak Ditemukan";
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Pesan = "Dokumen Digital Tidak Ditemukan";
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> LihatArsip(string did)
        {
            var konten = kontentmodel.getKontenAktif(did);
            if (konten != null)
            {
                string kantorid = konten.KANTORID;
                string tipe = konten.TIPE;
                string versi = konten.VERSI.ToString();
                string ext = konten.EKSTENSI;
                ext = string.IsNullOrEmpty(ext) ? ".pdf" : string.Concat(ext.Substring(0, 1).Equals(".") ? "" : ".", ext);

                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(did), "dokumenId");
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
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = String.Concat(tipe, ".pdf");
                            return docfile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Status = false, Message = "Dokumen Digital Tidak Ditemukan" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = false, Message = "Dokumen tidak valid" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TambahLink(string did, string kid, string tip, string nmr, string ktn, string note, string sid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = HttpContext.User.Identity as InternalUserIdentity;
            string userid = usr.UserId;
            var data = new LinkSurat();
            data.DokumenId = did;
            data.KantorId = kid;
            data.DokumenTipe = tip;
            data.NomorDokumen = nmr;
            data.KontenAktifId = ktn;
            data.Note = note;
            data.SuratId = sid;

            tr = mdl.TambahLinkDokumen(data, userid);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }
    }
}