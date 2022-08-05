using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Surat.Models.Entities
{

    public class PengajuanAkses
    {
        public string PersetujuanId { get; set; }
        public string ProfileId { get; set; }
        public string Tipe { get; set; }
        public string TanggalDibuat { get; set; }
        public string NomorPersetujuan { get; set; }
        public string TanggalPersetujuan { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string Pengaju { get; set; }
        public string Status { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class CariPengajuanAkses
    {
        public string MetaData { get; set; }
        public string PegawaiId { get; set; }
        public string UnitKerjaId { get; set; }
    }

    public class KantorKKP
    {
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Status { get; set; }
    }

    public class TipeKantor
    {
        public int TipeKantorId { get; set; }
        public string Tipe { get; set; }
    }

    public class HakAksesKKP
    {
        public string TipeAkses { get; set; }
        public string UserIdAkses { get; set; }
        public string PegawaiIdAkses { get; set; }
        public string KantorIdAkses { get; set; }
        public List<TipeKantor> ListTipe { get; set; }
    }

    public class SimpanAkses
    {
        public string UserId { get; set; }
        public string TipeAkses { get; set; }
        public string ProfileId { get; set; }
        public string PegawaiId { get; set; }
        public string ValidSampai { get; set; }
        public List<KantorKKP> ListAkses { get; set; }
    }

    public class PersetujuanAkses
    {
        public string AksesId { get; set; }
        public string UserId { get; set; }
        public string PegawaiId { get; set; }
        public string KantorId { get; set; }
        public string ProfileId { get; set; }
        public string TipeStatus { get; set; }
        public DateTime ValidSampai { get; set; }
        public int StatusPLT { get; set; }
        public int BisaBooking { get; set; }
    }

    public class CetakPersetujuanAksesKKP
    {
        public string PersetujuanId { get; set; }
        public string NamaPembuat { get; set; }
        public string TanggalDibuat { get; set; }
        public string NomorPersetujuan { get; set; }
        public string TanggalPersetujuan { get; set; }
        public string TargetId { get; set; }
        public string NamaTarget { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string Jabatan { get; set; }
        public List<DetailAksesKKP> ListAkses { get; set; }
    }

    public class DetailAksesKKP
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string Kantor { get; set; }
        public string Profile { get; set; }
        public string Tipe { get; set; }
    }

    public class PersetujuanJabatan
    {
        public string PengajuanId { get; set; }
        public string ProfileId { get; set; }
        public string PegawaiId { get; set; }
        public string KantorId { get; set; }
        public int StatusPLT { get; set; }
        public DateTime ValidSejak { get; set; }
        public DateTime ValidSampai { get; set; }
    }

    public class CetakPersetujuanJabatan
    {
        public string PengajuanId { get; set; }
        public string NamaPembuat { get; set; }
        public string TanggalDibuat { get; set; }
        public string TanggalPersetujuan { get; set; }
        public string TargetId { get; set; }
        public string NamaTarget { get; set; }
        public string KantorTarget { get; set; }
        public int StatusPlt { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string Jabatan { get; set; }
    }
}