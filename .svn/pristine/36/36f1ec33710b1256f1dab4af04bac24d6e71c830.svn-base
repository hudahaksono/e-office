using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Surat.Codes;
using Surat.Models;
using Surat.Models.Entities;
using System.Net;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class PenomoranController : Controller
    {
        PenomoranModel pm = new PenomoranModel();
        NaskahDinasModel nd = new NaskahDinasModel();
        Functions functions = new Functions();
        public ActionResult Index()
        {
            var usr = functions.claimUser();
            if (usr.UnitKerjaId != "02010208") { return RedirectToAction("Index", "Home"); }
            var unitkerjalist = nd.NDGetUnitKerja();
            ViewBag.myUnitKerja = usr.UnitKerjaId;
            ViewBag.ListUnitkerja = unitkerjalist;
            return View();
        }

        public JsonResult SetBukuPenomoran(string NamaBukuPenomoran, string Penandatangan, string Akses, string BukuNomorId = null, bool update = false)
        {
            BukuPenomran data = new BukuPenomran();
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "Data gagal disimpan" };
            var usr = functions.claimUser();
            if (!string.IsNullOrEmpty(usr.UserId))
            {
                if (!string.IsNullOrEmpty(NamaBukuPenomoran ?? Penandatangan ?? Akses))
                {
                    var thisGuid = update ? BukuNomorId : pm.NewUUID();
                    var thisDate = DateTime.Now.ToString("dd/MM/yyyy");
                    data.Update = update;
                    data.BukuNomorId = thisGuid;
                    data.Nama = Server.UrlDecode(NamaBukuPenomoran);
                    var listPenandatagan = new List<PenandatanganBuku>();
                    var listAkses = new List<AksesBuku>();
                    foreach (var p in Server.UrlDecode(Penandatangan).Split('&'))
                    {
                        var en = Server.UrlDecode(p);
                        var ip = en.Split('|');
                        PenandatanganBuku penandatangan = new PenandatanganBuku()
                        {
                            BukuNomorId = thisGuid,
                            ProfileId = ip[0],
                            JabatanNama = ip[2],
                            UnitKerjaId = ip[1],
                            StatusAktif = "1",
                            TanggalTerdaftar = thisDate
                        };
                        listPenandatagan.Add(penandatangan);
                    }
                    data.ListPenandatanganBuku = listPenandatagan;
                    listAkses.Add(new AksesBuku()
                    {
                        BukuNomorId = thisGuid,
                        PegawaiId = usr.PegawaiId,
                        Nama = usr.NamaPegawai,
                        UnitKerjaId = usr.UnitKerjaId,
                        StatusAktif = "2",
                        TanggalTerdaftar = thisDate
                    });
                    foreach (var a in Server.UrlDecode(Akses).Split('&'))
                    {
                        var ia = Server.UrlDecode(a).Split('|');
                        if (ia[0] != usr.PegawaiId)
                        {
                            AksesBuku AksesBuku = new AksesBuku()
                            {
                                BukuNomorId = thisGuid,
                                PegawaiId = ia[0],
                                Nama = ia[2],
                                UnitKerjaId = ia[1],
                                StatusAktif = "1",
                                TanggalTerdaftar = thisDate
                            };
                            listAkses.Add(AksesBuku);
                        }
                    }
                    data.ListAksesBuku = listAkses;
                    data.StatusAktif = "1";
                    data.TanggalBuat = thisDate;
                    tr = pm.SimpanBukuPenomoran(data, usr);
                    //catatan statusakses 0 - tidak aktif, 1 - aktif, 2 - pembuat 
                }
            }
            else
            {
                tr.Pesan = "Sesi Login anda habis, Silahkan Login Kembali";
            }

            return Json(new { Status = tr.Status, Pesan = tr.Pesan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getListBukuNomor(string bukunomorid = null)
        {
            var usr = functions.claimUser();
            var data = new List<BukuPenomran>();
            bool status = false;
            string pesan = "Terdapat kendala dalam mengakses data";
            if (!string.IsNullOrEmpty(usr.UserId))
            {
                data = pm.getListBukuPenomoran(usr.PegawaiId, usr.UnitKerjaId, bukunomorid);
                status = true;
                pesan = "data berahsi di akses";
            }
            else
            {
                pesan = "Sesi Login anda habis, Silahkan Login Kembali";
            }
            return Json(new { Status = status, Data = data, Pesan = pesan }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Open(string i)
        {
            if (string.IsNullOrEmpty(i))
            {
                return RedirectToAction("Index");
            }
            var data = new BukuPenomran();
            var usr = functions.claimUser();
            var getdata = pm.getListBukuPenomoran(usr.PegawaiId, usr.UnitKerjaId, i);
            if (getdata.Count() > 0 && pm.validasiUserBukuPenomoran(i, usr.PegawaiId))
            {
                data = getdata[0];
                foreach (var d in data.ListPenandatanganBuku)
                {
                    d.JabatanNama = new SuratModel().GetNamaJabatan(d.ProfileId);
                }
                ViewBag.CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.JenisNaskahDinas = pm.getTipeSurat();
                var tahun = pm.getListTahun(i);
                int currentYear = int.Parse(DateTime.Now.Year.ToString());
                if (tahun.Count > 0)
                {
                    int recordedYear = int.Parse(tahun[0]);
                    if (recordedYear < currentYear)
                    {
                        tahun.Add(currentYear.ToString());
                    }
                }
                else
                {
                    tahun.Add(currentYear.ToString());
                }
                ViewBag.ListTahun = tahun;
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }

        [HttpPost]
        public JsonResult getDataTablePenomoran(
            string Bukunomorid,
            string Jenis,
            string Tahun,
            int? start = 0,
            int? length = 10,
            string searchKey = null,
            string Bulan = null,
            string StatArsip = null

            )
        {
            var datas = new List<DataPenomoran>();
            string dtTerakhir;
            var usr = functions.claimUser();
            bool uservalid = pm.validasiUserBukuPenomoran(Bukunomorid, usr.PegawaiId);
            decimal? totaldata = 0;

            if (uservalid && !string.IsNullOrEmpty(Bukunomorid ?? Jenis ?? Tahun))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 10;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                datas = pm.getDataPenomoran(Bukunomorid,
                    Uri.EscapeDataString(Jenis),
                    Tahun, from: from, to: to,
                    searchKey: Uri.EscapeDataString(searchKey),
                    Bulan: Bulan, StatArsip: StatArsip
                    );

                dtTerakhir = pm.getDataPenomoranAkhir(Bukunomorid, Uri.EscapeDataString(Jenis));

                totaldata = datas.Count;
                totaldata = (totaldata > 0) ? datas[0].Total : totaldata;
                foreach (var data in datas)
                {
                    if (data.Keterangan == "TTE")
                    {
                        data.Status = "TTE";
                    }
                    data.NomorSurat = Server.UrlDecode(data.NomorSurat);
                    data.Perihal = Server.UrlDecode(data.Perihal);
                    data.JenisNaskahDinas = Server.UrlDecode(data.JenisNaskahDinas);
                    data.TanggalInput = data.TanggalInput.ToString(new System.Globalization.CultureInfo("id-ID"));

                    if (data.NomorSurat == Server.UrlDecode(dtTerakhir))
                    {
                        data.NoTerakhir = true;
                    }  
                }
             
            }
            return Json(new { data = datas, recordsTotal = totaldata, recordsFiltered = totaldata });
        }


        [HttpPost]
        public JsonResult GetDokumenElektronikByNomorSurat(string list)
        {
            var dokid = new findDokumenTTE();
            var doks = new List<findDokumenTTE>();
            foreach (var l in list.Split('|'))
            {
                if (!string.IsNullOrEmpty(l))
                {
                    dokid = pm.findDokumenTTEbyNomor(Server.UrlEncode(l));
                    if (dokid != null)
                    {
                        dokid.NomorSurat = l;
                        doks.Add(dokid);
                    }
                }
            }

            return Json(new { status = doks.Count > 0, data = doks });
        }

        [HttpPost]
        public JsonResult getDataTableKlasArsip(int? start, int? length, string searchKey = null)
        {
            var datas = new List<KlasifikasiArsip>();
            var usr = functions.claimUser();
            decimal? totaldata = 0;
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            datas = pm.getListKlasArsip(from: from, to: to, searchKey: new Functions().TextEncode(searchKey));
            totaldata = datas.Count;
            totaldata = (totaldata > 0) ? datas[0].Total : totaldata;

            return Json(new { data = datas, recordsTotal = totaldata, recordsFiltered = totaldata });
        }

        [HttpPost]
        public JsonResult InsertPenomoran(DataPenomoran data, HttpPostedFileBase fileUploadStream)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "Data Gagal Disimpan" };
            var usr = functions.claimUser();
            string method = data.isTTE ? "TTE" : "Manual";
            data.Keterangan = method;

            if (data.Update && data.Penomoranid != null)
            {
                tr = pm.UpdateDataPenomoran(usr, data);
            }
            else
            {
                if (!string.IsNullOrEmpty(data.Penomoranid))
                {
                    return Json(new { status = false, pesan = "Terdapat Kesalahan dalam menarik data" });
                }
                tr = pm.SimpanDataPenomoran(usr, data, fileUploadStream);
                pm.simpanLogPengajuan(tr.ReturnValue2, "Pengajuan Baru", $"{usr.PegawaiId}|{usr.UnitKerjaId}|{tr.ReturnValue}|{method}"); ;
            }
            return Json(new { status = tr.Status, data = data, pesan = tr.Pesan });
        }

        public JsonResult GetDetailsPenomoran(string penomoranid)
        {
            var datas = new List<KeteranganPenomoran>();
            var usr = functions.claimUser();
            bool ServerStatus = true;

            try
            {
                datas = pm.GetDetailsPenomoran(penomoranid);
                foreach (var data in datas)
                {
                    data.ValueKeterangan = (data.TextKeterangan == "UserInput") ? new SuratModel().GetNamaPegawai(HttpUtility.UrlDecode(data.ValueKeterangan)) : HttpUtility.UrlDecode(data.ValueKeterangan);
                    data.ValueKeterangan = (data.TextKeterangan == "penandatangan") ? new SuratModel().GetNamaJabatan(HttpUtility.UrlDecode(data.ValueKeterangan)) : HttpUtility.UrlDecode(data.ValueKeterangan);
                }
            }
            catch
            {
                ServerStatus = false;
            }
            return Json(new { status = datas.Count > 0, data = datas, serverStatus = ServerStatus });
        }


        public JsonResult HapusBuku(string penomoranid)
        {
            var usr = functions.claimUser();
            bool akses = pm.validasiUserBukuPenomoran(penomoranid, usr.PegawaiId);
            var tr = new TransactionResult() { Status = false, Pesan = "Anda tidak memiliki akses" };
            if (akses)
            {
                tr = pm.HapusBuku(penomoranid);
            }
            return Json(new { status = tr.Status, data = tr.Pesan });
        }

        public ActionResult SettingKodePenandatangan()
        {
            var unitkerjalist = pm.GetListUnitKerjaStruktural();
            return View(unitkerjalist);
        }


        public JsonResult getEs2(string induk)
        {
            var unitkerjalist = pm.GetListUnitKerjaStrukturalE2(induk);
            return Json(new { data = unitkerjalist }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult getListJabatan(string unitkerjaid, int? start = 0, int? length = 10)
        {
            var datas = new List<Jabatan>();
            var usr = functions.claimUser();
            decimal? totaldata = 0;

            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;


            datas = pm.getListJabatan(unitkerjaid, from: from, to: to);
            totaldata = datas.Count;
            totaldata = (totaldata > 0) ? datas[0].Total : totaldata;

            return Json(new { data = datas, recordsTotal = totaldata, recordsFiltered = totaldata });
        }


        [HttpPost]
        public JsonResult SaveKodeTTD(string profileid, string eselon, string kodettd, string KodeTTDSakter)
        {
            TransactionResult tr = pm.simpanKodeTtd(profileid, eselon, kodettd, KodeTTDSakter);
            return Json(new { status = tr.Status, pesan = tr.Pesan });
        }

        public ActionResult PermintaanNomor()
        {
            var usr = functions.claimUser();
            if (!(OtorisasiUser.IsProfile("PembuatSuratMasuk") || OtorisasiUser.IsProfile("PembuatSuratElektronik")) || !usr.KantorId.Equals("980FECFC746D8C80E0400B0A9214067D"))
            {
                return RedirectToAction("Index", "Home");
            }
            var viewData = new ViewDataRequest();
            viewData.ListTipeSurat = pm.getTipeSurat();
            ViewBag.CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
            return View(viewData);
        }

        public JsonResult AjukanPenomoran(DataPenomoran data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "Data Gagal Disimpan" };
            var usr = functions.claimUser();
            data.BukuNomorId = "QasKCHstnCfTwgK8jPNRyKEwUB5s90ATRBPN";
            data.Status = "0";
            string method = data.isTTE ? "TTE" : "Manual";
            decimal masal = data.JumlahNomor;
            data.Keterangan = method;

            for (int i = 0; i < masal; i++)
            {
                tr = pm.SimpanDataPenomoran(usr, data);

                if (tr.Status)
                {
                    pm.simpanLogPengajuan(tr.ReturnValue2, "Pengajuan Baru", $"{usr.PegawaiId}|{usr.UnitKerjaId}|{tr.ReturnValue}|{method}"); ;
                }
            }

            string nms = data.isTTE ? tr.ReturnValue : "";
            return Json(new { status = tr.Status, pesan = tr.Pesan, nomorsurat = nms });
        }


        public JsonResult SerahkanArsip(string penomoranid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "Data Gagal Disimpan" };
            var usr = functions.claimUser();
            tr = pm.penyerahanArsip(penomoranid, usr);
            return Json(new { status = tr.Status, pesan = tr.Pesan });
        }

        public JsonResult BatalkanNomor(string penomoranid, HttpPostedFileBase fileUploadStream)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "Nomor Gagal dibatalkan" };
            var usr = functions.claimUser();
            tr = pm.batalkanNomor(usr, penomoranid, fileUploadStream);
            return Json(new { status = tr.Status, pesan = tr.Pesan });
        }

        public JsonResult PenghapusanNomor(string penomoranid)
        {
            var usr = functions.claimUser();
            var tr = new TransactionResult() { Status = false, Pesan = "Nomor Gagal dihapus" };
            tr = pm.penghapusanNomor(usr, penomoranid);
            return Json(new { status = tr.Status, pesan = tr.Pesan });
        }

        public JsonResult RestoreNomor(string penomoranid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "Nomor Gagal dikembalikan" };
            var usr = functions.claimUser();
            tr = pm.restoreNomor(penomoranid);
            return Json(new { status = tr.Status, pesan = tr.Pesan });
        }

        public JsonResult RiwayatPengajuan( 
          string StatArsip = null)
        {

            List<RiwayatPengajuan> datas = new List<RiwayatPengajuan>();
            var usr = functions.claimUser();
            string bukuid = "QasKCHstnCfTwgK8jPNRyKEwUB5s90ATRBPN"; 
            datas = pm.getRiwayatPengajuan(usr.PegawaiId, usr.UnitKerjaId, bukuid, StatArsip);

          
            return Json(new { status = datas.Count > 0, data = datas });

        }


        [HttpPost]
        public JsonResult ExportPenomoran(
          string Bukunomorid,
          string Jenis,
          string Tahun,
          int? start = 0,
          int? length = 10,
          string searchKey = null,
          string Bulan = null,
          string StatArsip = null
          )
        {
            var datas = new List<DataPenomoran>();
            var usr = HttpContext.User.Identity as InternalUserIdentity;
            bool uservalid = pm.validasiUserBukuPenomoran(Bukunomorid, usr.PegawaiId);
            decimal? totaldata = 0;

            if (uservalid && !string.IsNullOrEmpty(Bukunomorid ?? Jenis ?? Tahun))
            {
                datas = pm.getDataPenomoranExport(Bukunomorid,
                    Uri.EscapeDataString(Jenis),
                    Tahun, searchKey: Uri.EscapeDataString(searchKey),
                    Bulan: Bulan, StatArsip: StatArsip
                    );
                totaldata = datas.Count;
                totaldata = (totaldata > 0) ? datas[0].Total : totaldata;
                foreach (var data in datas)
                {
                    if (data.Keterangan == "TTE")
                    {
                        data.Status = "TTE";
                    }
                    data.NomorSurat = Server.UrlDecode(data.NomorSurat);
                    data.Perihal = Server.UrlDecode(data.Perihal);
                    data.JenisNaskahDinas = Server.UrlDecode(data.JenisNaskahDinas);
                    data.TanggalInput = data.TanggalInput.ToString(new System.Globalization.CultureInfo("id-ID"));
                    data.PIC = new SuratModel().GetNamaPegawai(HttpUtility.UrlDecode(data.PIC));
                    data.UnitKerja = Server.UrlDecode(data.UnitKerja);
                }
            }
            return Json(new { data = datas, recordsTotal = totaldata, recordsFiltered = totaldata });
        }

        public JsonResult CekDokumenExt(string penomoranid, string type)
        {
            var data = pm.getInfoDokumenExtNomor(penomoranid, type);
            return Json(new { Status = data.Status, Pesan = data.Pesan, Value1 = data.ReturnValue, Value2 = data.ReturnValue2 });
        }


        public async Task<ActionResult> getDokumen(string id)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();
            var dokid = id.Substring(id.LastIndexOf('_') + 1);

            if (!string.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();
                string kantorid = usr.KantorId;
                string tipe = "BerkasPenomoran";
                string versi = "0";
                string filename = id;
                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(dokid), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

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
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = string.IsNullOrEmpty(id) ? "dokumen" : filename;

                            result.Status = true;
                            result.StreamResult = docfile;

                            return docfile;
                        }
                        else
                        {
                            result.Pesan = "Dokumen tidak ditemukan";
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Pesan = ex.Message;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}