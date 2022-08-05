using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Surat.Codes;
using Surat.Models;
using Surat.Models.Entities;
using System.Data;
using OfficeOpenXml;

namespace Surat.Controllers
{
    [AccessDeniedAuthorize]
    public class LaporanController : Controller
    {
        Functions functions = new Functions();
        LaporanModel lm = new LaporanModel();
        public ActionResult PenggunaEoffice()
        {
            if (OtorisasiUser.isTU() || OtorisasiUser.IsProfile("AdminSatker") || OtorisasiUser.IsProfile("Administrator"))
            {
                var usr = functions.claimUser();
                ViewBag.TipeKantor = new DataMasterModel().GetTipeKantor(usr.KantorId);
                var unitkerjalist = new PenomoranModel().GetListUnitKerjaStruktural(pusat: true);
                return View(unitkerjalist);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult GetPenggunaKantor(string kantorid, decimal tipe)
        {
            List<LaporanKantor> laporan = new List<LaporanKantor>();
            laporan = lm.GetPenggunaEofficeKantor(kantorid, tipe);
            return Json(new { Status = true, data = laporan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDetailPengguna(string kantorid, string menu)
        {
            List<DetailLaporan> details = new List<DetailLaporan>();
            details = lm.GetDetailLaporanDaerah(kantorid, menu);
            return Json(new { Status = true, data = details }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDetailPenggunaPusat(string unitkerja, string menu)
        {
            List<DetailLaporan> details = new List<DetailLaporan>();
            details = lm.GetDetailLaporanPusat(unitkerja, menu);
            return Json(new { Status = true, data = details }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTTEKantor(string kantorid, decimal tipe)
        {
            List<LaporanKantor> laporan = new List<LaporanKantor>();
            laporan = lm.GetPenggunaTTEKantor(kantorid, tipe);
            return Json(new { Status = true, data = laporan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendaftarTTE(string kantorid, decimal tipe)
        {
            List<LaporanKantor> laporan = new List<LaporanKantor>();
            laporan = lm.GetPendaftarTTE(kantorid, tipe);
            return Json(new { Status = true, data = laporan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPenggunaPusat(string type, string unitkerjaid = null)
        {
            List<LaporanKantor> laporan = new List<LaporanKantor>();
            laporan = lm.GetPenggunaEofficePusat(ukid: unitkerjaid, type: type);
            return Json(new { Status = true, data = laporan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProvinsiKantorid()
        {
            List<string> result = new List<string>();
            var usr = functions.claimUser();
            int tipekantor = new DataMasterModel().GetTipeKantor(usr.KantorId);

            if (tipekantor == 1)
            {
                result = lm.GetProvinsiKantorid();
            }
            else if (tipekantor == 2)
            {
                result.Add($"{usr.KantorId}|{usr.NamaKantor.Replace("Kantor Wilayah ", "")}");
            }
            return Json(new { Status = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListKantorid()
        {
            List<string> result = new List<string>();
            var usr = functions.claimUser();
            int tipekantor = new DataMasterModel().GetTipeKantor(usr.KantorId);
            if (tipekantor == 1)
            {
                result = lm.GetListKantorid();
            }
            else if (tipekantor == 2)
            {
                result = lm.GetListKantorid(usr.KantorId);
            }
            else
            {
                result.Add($"{usr.KantorId}|{usr.NamaKantor}");
            }
            return Json(new { Status = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RekapPresensi()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            var data = new CariRekapPresensi();
            data.listUnitKerja = new List<SelectListItem>();
            data.listPegawai = new List<SelectListItem>();
            data.listTipePegawai = new List<SelectListItem>();
            data.UnitKerjaId = usr.UnitKerjaId;
            data.listUnitKerja.Add(new SelectListItem() { Text = new DataMasterModel().GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId, Selected = true });
            if (OtorisasiUser.IsRoleAdministrator())
            {
                foreach (var _unitkerja in new DataMasterModel().GetListUnitKerja("", "", "", true))
                {
                    if (!_unitkerja.UnitKerjaId.Equals(usr.UnitKerjaId))
                    {
                        data.listUnitKerja.Add(new SelectListItem() { Text = _unitkerja.NamaUnitKerja, Value = _unitkerja.UnitKerjaId });
                    }
                }
                data.listPegawai.Add(new SelectListItem() { Text = "-- Semua Pegawai --", Value = "", Selected = true });
                foreach (var _pegawai in new DataMasterModel().GetPegawaiByUnitKerjaJabatanNama(usr.UnitKerjaId, "", ""))
                {
                    data.listPegawai.Add(new SelectListItem() { Text = _pegawai.NamaLengkap, Value = _pegawai.PegawaiId });
                }
                data.listTipePegawai.Add(new SelectListItem() { Text = "-- Semua Pegawai --", Value = "", Selected = true });
                data.listTipePegawai.Add(new SelectListItem() { Text = "PNS", Value = "ASN" });
                data.listTipePegawai.Add(new SelectListItem() { Text = "PPNPN", Value = "PPNPN" });
            }
            else
            {
                if (OtorisasiUser.isTU())
                {
                    data.listPegawai.Add(new SelectListItem() { Text = "-- Semua Pegawai --", Value = "", Selected = true });
                    foreach (var _pegawai in new DataMasterModel().GetPegawaiByUnitKerjaJabatanNama(usr.UnitKerjaId, "", ""))
                    {
                        data.listPegawai.Add(new SelectListItem() { Text = _pegawai.NamaLengkap, Value = _pegawai.PegawaiId });
                    }
                    data.listTipePegawai.Add(new SelectListItem() { Text = "-- Semua Pegawai --", Value = "", Selected = true });
                    data.listTipePegawai.Add(new SelectListItem() { Text = "PNS", Value = "ASN" });
                    data.listTipePegawai.Add(new SelectListItem() { Text = "PPNPN", Value = "PPNPN" });
                }
                else
                {
                    data.listPegawai.Add(new SelectListItem() { Text = usr.NamaPegawai, Value = usr.PegawaiId, Selected = true });
                    data.PegawaiId = usr.PegawaiId;
                    var tipe = new DataMasterModel().GetTipeUser(usr.PegawaiId, usr.KantorId);
                    if (tipe.Status)
                    {
                        if (tipe.ReturnValue.Equals("ASN"))
                        {
                            data.listTipePegawai.Add(new SelectListItem() { Text = "PNS", Value = "ASN" });
                        }
                        if (tipe.ReturnValue.Equals("PPNPN"))
                        {
                            data.listTipePegawai.Add(new SelectListItem() { Text = "PPNPN", Value = "PPNPN" });
                        }
                    }
                }
            }

            return View(data);
        }

        public ActionResult DaftarPresensi(int? draw, int? start, int? length, CariRekapPresensi f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<RekapPresensi>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;


            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 20;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.UnitKerjaId = OtorisasiUser.IsRoleAdministrator() ? f.UnitKerjaId : usr.UnitKerjaId;
                f.PegawaiId = OtorisasiUser.isTU() || OtorisasiUser.IsRoleAdministrator() ? f.PegawaiId : usr.PegawaiId;
                f.TanggalMulai = DateTime.Today;
                f.TanggalSampai = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (!string.IsNullOrEmpty(Request.Form["cariMulai"]))
                {
                    f.TanggalMulai = Convert.ToDateTime(Request.Form["cariMulai"]);
                }
                if (!string.IsNullOrEmpty(Request.Form["cariSampai"]))
                {
                    f.TanggalSampai = Convert.ToDateTime(Request.Form["cariSampai"]);
                }
                result = lm.GetRekapPresensi(f, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }

                foreach (var _dt in result)
                {
                    var _locKantor = lm.GetListLocationKantor(_dt.KantorId);
                    var _loc = new GeoLocation();
                    decimal _jamKerja = 0;
                    _dt.Masuk_Status = "-";
                    _dt.Masuk_Lokasi = "-";
                    _dt.Keluar_Status = "-";
                    _dt.Keluar_Lokasi = "-";
                    if (!string.IsNullOrEmpty(_dt.Masuk) && !_dt.Masuk.Equals("-"))
                    {
                        _dt.Masuk_Status = "WFA";
                        _loc = lm.getLocationPresensi(_dt.PegawaiId, _dt.KantorId, string.Concat(_dt.Period.ToString("dd/MM/yyyy"), " ", _dt.Masuk));
                        foreach (var _lk in _locKantor)
                        {
                            if (_dt.Masuk_Status.Equals("WFA"))
                            {
                                var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _lk.Latitude, _lk.Longitude);
                                _dt.Masuk_Status = jarak <= 1000 ? "WFO" : "WFA";
                                if (_dt.Masuk_Status.Equals("WFO"))
                                {
                                    _dt.Masuk_Lokasi = _lk.Nama;
                                }
                            }
                        }
                        if (_dt.Masuk_Status.Equals("WFA") && _locKantor.Count > 1)
                        {
                            var _locUtama = lm.GetListLocationKantor(_dt.KantorId, true);
                            var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _locUtama[0].Latitude, _locUtama[0].Longitude);
                            _dt.Masuk_Status = jarak <= 1000 ? "WFO" : "WFA";
                            if (_dt.Masuk_Status.Equals("WFO"))
                            {
                                _dt.Masuk_Lokasi = _locUtama[0].Nama;
                            }
                        }
                        if (_dt.Masuk_Status.Equals("WFA"))
                        {
                            _dt.Masuk_Lokasi = "Diluar Kantor";
                        }
                        _dt.Masuk_Catatan = string.IsNullOrEmpty(_loc.Catatan) ? Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2))) > 83000 ? "Terlambat" : _loc.Catatan : _loc.Catatan;
                        //_jamKerja = Convert.ToInt32(DateTime.Now.ToString("HHmmss")) - Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2)));
                    }
                    if (!string.IsNullOrEmpty(_dt.Keluar) && !_dt.Keluar.Equals("-"))
                    {
                        _dt.Keluar_Status = "WFA";
                        _loc = lm.getLocationPresensi(_dt.PegawaiId, _dt.KantorId, string.Concat(_dt.Period.ToString("dd/MM/yyyy"), " ", _dt.Keluar));
                        foreach (var _lk in _locKantor)
                        {
                            if (_dt.Keluar_Status.Equals("WFA"))
                            {
                                var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _lk.Latitude, _lk.Longitude);
                                _dt.Keluar_Status = jarak <= 1000 ? "WFO" : "WFA";
                                if (_dt.Keluar_Status.Equals("WFO"))
                                {
                                    _dt.Keluar_Lokasi = _lk.Nama;
                                }
                            }
                        }
                        if (_dt.Keluar_Status.Equals("WFA") && _locKantor.Count > 1)
                        {
                            var _locUtama = lm.GetListLocationKantor(_dt.KantorId, true);
                            var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _locUtama[0].Latitude, _locUtama[0].Longitude);
                            _dt.Keluar_Status = jarak <= 1000 ? "WFO" : "WFA";
                            if (_dt.Keluar_Status.Equals("WFO"))
                            {
                                _dt.Keluar_Status = _locUtama[0].Nama;
                            }
                        }
                        if (_dt.Keluar_Status.Equals("WFA"))
                        {
                            _dt.Keluar_Lokasi = "Diluar Kantor";
                        }
                        if (!string.IsNullOrEmpty(_dt.Masuk) && !_dt.Masuk.Equals("-"))
                        {
                            int waktuKerja = 83000;
                            _dt.Keluar_Catatan = string.IsNullOrEmpty(_loc.Catatan) ? Convert.ToInt32(string.Concat(_dt.Keluar.Substring(0, 2), _dt.Keluar.Substring(3, 2), _dt.Keluar.Substring(6, 2))) - Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2))) < waktuKerja ? "Pulang Cepat" : _loc.Catatan : _loc.Catatan;
                            _jamKerja = Convert.ToInt32(string.Concat(_dt.Keluar.Substring(0, 2), _dt.Keluar.Substring(3, 2), _dt.Keluar.Substring(6, 2))) - Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2)));
                        }
                        else
                        {
                            _dt.Keluar_Catatan = _loc.Catatan;
                        }
                    }

                    var sec_num = Math.Floor((_jamKerja / 10000) * 3600);
                    var hours = Math.Floor(sec_num / 3600);
                    var minutes = Math.Floor((sec_num - (hours * 3600)) / 60);
                    //var seconds = sec_num - (hours * 3600) - (minutes * 60);
                    _dt.Jam_Kerja = string.Concat(hours<10?"0":"", hours,":", minutes < 10 ? "0" : "", minutes);
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListPegawai(string unitkerjaid, string tipe)
        {
            var lstPegawai = new List<Pegawai>();
            var usr = functions.claimUser();
            foreach (var _pegawai in new DataMasterModel().GetPegawaiByUnitKerjaAndTipe(unitkerjaid, tipe))
            {
                if (OtorisasiUser.isTU() || OtorisasiUser.IsRoleAdministrator())
                {
                    lstPegawai.Add(_pegawai);
                }
                else
                {
                    if (_pegawai.PegawaiId.Equals(usr.PegawaiId))
                    {
                        lstPegawai.Add(_pegawai);
                    }
                }
            }
            return Json(lstPegawai, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetExportRekap(string ukid, string tp, string pg, string dtm, string dts)
        {
            int from = 0;
            int to = 0; ;
            var f = new CariRekapPresensi();
            f.UnitKerjaId = ukid;
            f.TipePegawai = tp;
            f.PegawaiId = pg;
            f.TanggalMulai = DateTime.Today;
            f.TanggalSampai = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (!string.IsNullOrEmpty(dtm))
            {
                f.TanggalMulai = Convert.ToDateTime(dtm);
            }
            if (!string.IsNullOrEmpty(dts))
            {
                f.TanggalSampai = Convert.ToDateTime(dts);
            }
            var rekap = lm.GetRekapPresensi(f, from, to);
            var dt = new DataTable("Rekap");
            dt.Columns.AddRange(new DataColumn[14]
            {
                new DataColumn("#",typeof(int)),
                new DataColumn("NIP/NIK"),
                new DataColumn("Nama Pegawai"),
                new DataColumn("Nama Unit Kerja"),
                new DataColumn("Periode"),
                new DataColumn("Waktu Masuk"),
                new DataColumn("Lokasi Masuk"),
                new DataColumn("Status Masuk"),
                new DataColumn("Catatan Masuk"),
                new DataColumn("Waktu Keluar"),
                new DataColumn("Lokasi Keluar"),
                new DataColumn("Status Keluar"),
                new DataColumn("Catatan Keluar"),
                new DataColumn("Jam Kerja")
            });
            foreach (var _dt in rekap)
            {
                var _locKantor = lm.GetListLocationKantor(_dt.KantorId);
                var _loc = new GeoLocation();
                decimal _jamKerja = 0;
                _dt.Masuk_Status = "-";
                _dt.Masuk_Lokasi = "-";
                _dt.Keluar_Status = "-";
                _dt.Keluar_Lokasi = "-";
                if (!string.IsNullOrEmpty(_dt.Masuk) && !_dt.Masuk.Equals("-"))
                {
                    _dt.Masuk_Status = "WFA";
                    _loc = lm.getLocationPresensi(_dt.PegawaiId, _dt.KantorId, string.Concat(_dt.Period.ToString("dd/MM/yyyy"), " ", _dt.Masuk));
                    foreach (var _lk in _locKantor)
                    {
                        if (_dt.Masuk_Status.Equals("WFA"))
                        {
                            var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _lk.Latitude, _lk.Longitude);
                            _dt.Masuk_Status = jarak <= 1000 ? "WFO" : "WFA";
                            if (_dt.Masuk_Status.Equals("WFO"))
                            {
                                _dt.Masuk_Lokasi = _lk.Nama;
                            }
                        }
                    }
                    if (_dt.Masuk_Status.Equals("WFA") && _locKantor.Count > 1)
                    {
                        var _locUtama = lm.GetListLocationKantor(_dt.KantorId, true);
                        var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _locUtama[0].Latitude, _locUtama[0].Longitude);
                        _dt.Masuk_Status = jarak <= 1000 ? "WFO" : "WFA";
                        if (_dt.Masuk_Status.Equals("WFO"))
                        {
                            _dt.Masuk_Lokasi = _locUtama[0].Nama;
                        }
                    }
                    if (_dt.Masuk_Status.Equals("WFA"))
                    {
                        _dt.Masuk_Lokasi = "Diluar Kantor";
                    }
                    _dt.Masuk_Catatan = string.IsNullOrEmpty(_loc.Catatan) ? Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2))) > 83000 ? "Terlambat" : _loc.Catatan : _loc.Catatan;
                }
                if (!string.IsNullOrEmpty(_dt.Keluar) && !_dt.Keluar.Equals("-"))
                {
                    _dt.Keluar_Status = "WFA";
                    _loc = lm.getLocationPresensi(_dt.PegawaiId, _dt.KantorId, string.Concat(_dt.Period.ToString("dd/MM/yyyy"), " ", _dt.Keluar));
                    foreach (var _lk in _locKantor)
                    {
                        if (_dt.Keluar_Status.Equals("WFA"))
                        {
                            var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _lk.Latitude, _lk.Longitude);
                            _dt.Keluar_Status = jarak <= 1000 ? "WFO" : "WFA";
                            if (_dt.Keluar_Status.Equals("WFO"))
                            {
                                _dt.Keluar_Lokasi = _lk.Nama;
                            }
                        }
                    }
                    if (_dt.Keluar_Status.Equals("WFA") && _locKantor.Count > 1)
                    {
                        var _locUtama = lm.GetListLocationKantor(_dt.KantorId, true);
                        var jarak = new Functions().getDistance(_loc.Latitude, _loc.Longitude, _locUtama[0].Latitude, _locUtama[0].Longitude);
                        _dt.Keluar_Status = jarak <= 1000 ? "WFO" : "WFA";
                        if (_dt.Keluar_Status.Equals("WFO"))
                        {
                            _dt.Keluar_Status = _locUtama[0].Nama;
                        }
                    }
                    if (_dt.Keluar_Status.Equals("WFA"))
                    {
                        _dt.Keluar_Lokasi = "Diluar Kantor";
                    }
                    if (!string.IsNullOrEmpty(_dt.Masuk) && !_dt.Masuk.Equals("-"))
                    {
                        int waktuKerja = 83000;
                        _dt.Keluar_Catatan = string.IsNullOrEmpty(_loc.Catatan) ? Convert.ToInt32(string.Concat(_dt.Keluar.Substring(0, 2), _dt.Keluar.Substring(3, 2), _dt.Keluar.Substring(6, 2))) - Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2))) < waktuKerja ? "Pulang Cepat" : _loc.Catatan : _loc.Catatan;
                        _jamKerja = Convert.ToInt32(string.Concat(_dt.Keluar.Substring(0, 2), _dt.Keluar.Substring(3, 2), _dt.Keluar.Substring(6, 2))) - Convert.ToInt32(string.Concat(_dt.Masuk.Substring(0, 2), _dt.Masuk.Substring(3, 2), _dt.Masuk.Substring(6, 2)));
                    }
                    else
                    {
                        _dt.Keluar_Catatan = _loc.Catatan;
                    }
                }
                var sec_num = Math.Floor((_jamKerja / 10000) * 3600);
                var hours = Math.Floor(sec_num / 3600);
                var minutes = Math.Floor((sec_num - (hours * 3600)) / 60);
                _dt.Jam_Kerja = string.Concat(hours < 10 ? "0" : "", hours, ":", minutes < 10 ? "0" : "", minutes);
                dt.Rows.Add(_dt.RNumber, _dt.PegawaiId, _dt.NamaPegawai, _dt.NamaUnitKerja, _dt.strPeriod, _dt.Masuk, _dt.Masuk_Lokasi, _dt.Masuk_Status, _dt.Masuk_Catatan, _dt.Keluar, _dt.Keluar_Lokasi, _dt.Keluar_Status, _dt.Keluar_Catatan, _dt.Jam_Kerja);
            }

            var memoryStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Rekap Presensi");
                worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                worksheet.Cells["A1:N1"].AutoFitColumns();
                worksheet.Cells["A1:N1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).AutoFit();
                worksheet.Column(9).AutoFit();
                worksheet.Column(10).AutoFit();
                worksheet.Column(11).AutoFit();
                worksheet.Column(12).AutoFit();
                worksheet.Column(13).AutoFit();
                worksheet.Column(14).AutoFit();
                worksheet.Column(14).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells["A1:N1"].Style.Font.Bold = true;

                //worksheet.Column(5).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                byte[] dataExcel = excelPackage.GetAsByteArray() as byte[];
                return File(dataExcel, "application/vnd.ms-excel", "RekapPresensi.xls");
            }
        }

        [HttpPost]
        public JsonResult doUbahPresensi(string pid, string kid, string ukid, string prd, string wkt, string ctt)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            var log = functions.claimUser();

            if (string.IsNullOrEmpty(pid))
            {
                tr.Pesan = "Pegawai Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(kid))
            {
                tr.Pesan = "Unit Kerja Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(prd))
            {
                tr.Pesan = "Periode Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(wkt))
            {
                tr.Pesan = "Waktu Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(ctt))
            {
                tr.Pesan = "Catatan Kosong";
            }
            else
            {
                tr = lm.doUbahPresensi(pid, kid, ukid, string.Concat(prd, " ", wkt), ctt);
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RekapBukuTamu()
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var usr = functions.claimUser();
            if (usr == null)
            {
                return RedirectToAction("Index", "Account");
            }
            var data = new CariBukuTamu();
            data.listUnitKerja = new List<SelectListItem>();
            data.UnitKerjaId = usr.UnitKerjaId;
            data.listUnitKerja.Add(new SelectListItem() { Text = new DataMasterModel().GetNamaUnitKerjaById(usr.UnitKerjaId), Value = usr.UnitKerjaId, Selected = true });
            if (OtorisasiUser.IsRoleAdministrator())
            {
                foreach (var _unitkerja in new DataMasterModel().GetListUnitKerja("", "", "", true))
                {
                    if (!_unitkerja.UnitKerjaId.Equals(usr.UnitKerjaId))
                    {
                        data.listUnitKerja.Add(new SelectListItem() { Text = _unitkerja.NamaUnitKerja, Value = _unitkerja.UnitKerjaId });
                    }
                }
            }

            return View(data);
        }

        public ActionResult DaftarBukuTamu(int? draw, int? start, int? length, CariBukuTamu f)
        {
            Response.AppendHeader("X-Frame-Options", "DENY");
            Response.AppendHeader("Content-Security-Policy", "frame-ancestors 'none'");
            var result = new List<DataBukuTamu>();
            decimal? total = 0;

            var usr = functions.claimUser();
            string userid = usr.UserId;


            if (!string.IsNullOrEmpty(userid))
            {
                int recNumber = start ?? 0;
                int RecordsPerPage = length ?? 20;
                int from = recNumber + 1;
                int to = from + RecordsPerPage - 1;
                f.UnitKerjaId = OtorisasiUser.IsRoleAdministrator() ? f.UnitKerjaId : usr.UnitKerjaId;
                f.TanggalMulai = DateTime.Today;
                f.TanggalSampai = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (!string.IsNullOrEmpty(Request.Form["cariMulai"]))
                {
                    f.TanggalMulai = Convert.ToDateTime(Request.Form["cariMulai"]);
                }
                if (!string.IsNullOrEmpty(Request.Form["cariSampai"]))
                {
                    f.TanggalSampai = Convert.ToDateTime(Request.Form["cariSampai"]);
                }
                result = lm.GetRekapBukuTamu(f, from, to);

                if (result.Count > 0)
                {
                    total = result[0].Total;
                }
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetExportBukuTamu(string ukid, string dtm, string dts)
        {
            int from = 0;
            int to = 0; ;
            var f = new CariBukuTamu();
            f.UnitKerjaId = ukid;
            f.TanggalMulai = DateTime.Today;
            f.TanggalSampai = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (!string.IsNullOrEmpty(dtm))
            {
                f.TanggalMulai = Convert.ToDateTime(dtm);
            }
            if (!string.IsNullOrEmpty(dts))
            {
                f.TanggalSampai = Convert.ToDateTime(dts);
            }
            var rekap = lm.GetRekapBukuTamu(f, from, to);
            var dt = new DataTable("Rekap");
            dt.Columns.AddRange(new DataColumn[10]
            {
                new DataColumn("#",typeof(int)),
                new DataColumn("NIK"),
                new DataColumn("Nama Lengkap"),
                new DataColumn("Nomor Telepon"),
                new DataColumn("Alamat"),
                new DataColumn("Instansi"),
                new DataColumn("Keperluan"),
                new DataColumn("Waktu Kedatangan"),
                new DataColumn("Status"),
                new DataColumn("Catatan")
            });
            foreach (var _dt in rekap)
            {
                dt.Rows.Add(_dt.RNumber, _dt.NIK, _dt.NamaLengkap, _dt.NoTelp, _dt.Alamat, _dt.Instansi, _dt.Keperluan, _dt.Waktu_Kedatangan, _dt.StatusTamu, _dt.ResponCatatan);
            }

            var memoryStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Rekap Buku Tamu");
                worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                worksheet.Cells["A1:J1"].AutoFitColumns();
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).AutoFit();
                worksheet.Column(9).AutoFit();
                worksheet.Column(10).AutoFit();

                worksheet.Cells["A1:J1"].Style.Font.Bold = true;

                byte[] dataExcel = excelPackage.GetAsByteArray() as byte[];
                return File(dataExcel, "application/vnd.ms-excel", "RekapBukuTamu.xls");
            }
        }

        [HttpPost]
        public JsonResult doUbahBukuTamu(string nik, string nm, string tpl, string tgl, string tlp, string eml, string alm, string ins, string kep, string tid, string ukid, string stt, string ctt, bool std)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            var usr = functions.claimUser();

            if (string.IsNullOrEmpty(tid))
            {
                tr.Pesan = "Buku Tamu Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(ukid))
            {
                tr.Pesan = "Unit Kerja Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(stt))
            {
                tr.Pesan = "Status Tamu Belum Dipilih";
            }
            else if (string.IsNullOrEmpty(ctt))
            {
                tr.Pesan = "Catatan Kosong";
            }
            else
            {
                if (new DataMasterModel().checkUseridOnUnitkerjaid(ukid, usr.UserId))
                {
                    var data = new DataBukuTamu();
                    data.TamuId = tid;
                    data.NIK = nik;
                    data.NamaLengkap = nm;
                    data.TempatLahir = tpl;
                    data.TanggalLahir = tgl;
                    data.NoTelp = tlp;
                    data.Email = eml;
                    data.Alamat = alm;
                    data.Instansi = ins;
                    data.Keperluan = kep;
                    data.ResponStatus = stt;
                    data.ResponCatatan = ctt;
                    data.ResponUserId = usr.UserId;

                    tr = lm.doUbahBukuTamu(data);
                }
                else
                {
                    tr.Pesan = "Anda tidak memiliki hak di Unit Kerja ini.";
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult cekDukcapil(string nik, string nm, string tpl, string tgl)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            var log = functions.claimUser();

            if (string.IsNullOrEmpty(nik))
            {
                tr.Pesan = "NIK Kosong";
            }
            else if (string.IsNullOrEmpty(nm))
            {
                tr.Pesan = "Nama Kosong";
            }
            else if (string.IsNullOrEmpty(tpl))
            {
                tr.Pesan = "Tempat Lahir Kosong";
            }
            else if (string.IsNullOrEmpty(tgl))
            {
                tr.Pesan = "Tanggal Lahir Kosong";
            }
            else
            {
                var apiServices = new ApiServices();
                var param = new ParameterDukcapil();
                param.NIK = nik;
                string nama = nm.ToUpper();
                if (nm.IndexOf(",") > 0)
                {
                    nama = nm.Substring(0, nm.IndexOf(",")).ToUpper();
                }
                param.NAMA_LGKP = nama;
                param.TMPT_LHR = tpl.ToUpper();
                param.TGL_LHR = tgl;
                tr = apiServices.Cek_NIK(param);
                if (tr.Status)
                {
                    tr.Pesan = "Data sesuai dengan Dukcapil";
                }
            }

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getQrCodeBukuTamu(string host)
        {
            var usr = functions.claimUser();
            if (usr == null)
            {
                return RedirectToAction("Index", "Account");
            }
            var Link = string.Concat(host, "/Public/Load?id=", usr.UnitKerjaId, "&kd=", usr.UserId, "&tgt=bt");
            var QrCode = new Functions().createQR(Link, false);

            return new JsonResult()
            {
                Data = new { Url = Link, QrCode = QrCode },
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}