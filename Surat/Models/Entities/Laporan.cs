using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Surat.Models.Entities
{
    public class FilterLaporan
    {
        public string MetaData { get; set; }
        public string UnitKerjaId { get; set; }
        public string SortBy { get; set; }
    }

    public class CariLaporan
    {
        public string Tipe { get; set; }
        public FilterLaporan Filter { get; set; }
        public bool IsAdmin { get; set; }
        public int TipeKantorId { get; set; }
        public string UnitKerjaId { get; set; }
    }

    public class LaporanPengguna
    {
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string IndukId { get; set; }
        public string IndukName { get; set; }
        public decimal TotalSemua { get; set; }
        public decimal TotalStruktural { get; set; }
        public int Tipe { get; set; }
    }

    public class LaporanKantor
    {
        public string KantorId { get; set; }
        public string TipeKantor { get; set; }
        public string Provinsi { get; set; }
        public string KantorNama { get; set; }
        public decimal? ASN { get; set; }
        public decimal? ST { get; set; }
        public decimal? TotalPegawai { get; set; }
        public decimal? Jumlah { get; set; }
    }

    public class DetailLaporan
    {
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string Status { get; set; }
    }

}