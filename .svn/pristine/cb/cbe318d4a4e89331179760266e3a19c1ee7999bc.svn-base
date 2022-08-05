using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

namespace Surat.Models.Entities
{

    #region Monitoring dan Laporan

    public class RekapSurat
    {
        public string NIP { get; set; }
        public string NamaPegawai { get; set; }
        public string Jabatan { get; set; }
        public string Foto { get; set; }
        public decimal TotalSurat { get; set; }
        public decimal JumlahSurat { get; set; }
        public decimal JumlahInbox { get; set; }
        public decimal JumlahTerkirim { get; set; }
        public decimal JumlahSelesai { get; set; }
        public decimal KotakMasuk { get; set; }
        public decimal Inisiatif { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class ChartSurat
    {
        public string UnitKerjaId { get; set; }
        public string NamaUnitKerja { get; set; }
        public ChartSuratGauge chartGaugeSatker { get; set; }
        public ChartSuratGauge chartGaugePersonal { get; set; }

        public string JumlahSurat { get; set; }
        public string JumlahInbox { get; set; }
        public string JumlahTerkirim { get; set; }
        public string JumlahSelesai { get; set; }

        public List<JumlahSurat> SuratBelumDibuka { get; set; }
        //public string satker_pusat { get; set; }
        //public string akun_pusat { get; set; }
        //public string satker_kanwil { get; set; }
        //public string satker_kanwil_aktif { get; set; }
        //public string satker_kanwil_total { get; set; }
        //public string akun_kanwil { get; set; }
        //public string satker_kantah { get; set; }
        //public string satker_kantah_aktif { get; set; }
        //public string satker_kantah_total { get; set; }
        //public string akun_kantah { get; set; }
        //public string satker_total { get; set; }
        //public string akun_total { get; set; }

    }

    public class ChartSuratGauge
    {
        public decimal TotalSuratSatker { get; set; }
        public decimal TotalSuratPersonal { get; set; }
        public decimal JumlahSelesaiSatker { get; set; }
        public decimal JumlahSelesaiPersonal { get; set; }
    }

    #endregion

}