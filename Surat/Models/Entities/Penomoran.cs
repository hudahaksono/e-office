using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Surat.Models.Entities
{
    public class BukuPenomran
    {
        public string BukuNomorId { get; set; }
        public string Nama { get; set; }
        public string StatusAktif { get; set; }
        public string TanggalBuat { get; set; }
        public List<PenandatanganBuku> ListPenandatanganBuku { get; set; }
        public List<AksesBuku> ListAksesBuku { get; set; }
        public bool Update { get; set; }
    }

    public class PenandatanganBuku
    {
        public string BukuNomorId { get; set; }
        public string JabatanNama { get; set; }
        public string ProfileId { get; set; }
        public string UnitKerjaId { get; set; }
        public string StatusAktif { get; set; }
        public string TanggalTerdaftar { get; set; }
    }

    public class AksesBuku
    {
        public string BukuNomorId { get; set; }
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string UnitKerjaId { get; set; }
        public string StatusAktif { get; set; }
        public string TanggalTerdaftar { get; set; }
    }

    public class DataPenomoran
    {
        public decimal? Total { get; set; }
        public string Penomoranid { get; set; }
        public string TanggalInput { get; set; }
        public decimal NomorUrut { get; set; }
        public string sNomorUrut { get; set; }
        public string TanggalSurat { get; set; }
        public string NomorSurat { get; set; }
        public string Perihal { get; set; }
        public string JenisNaskahDinas { get; set; }
        public string BukuNomorId { get; set; }
        public string Status { get; set; }
        public string Keterangan { get; set; }
        public string StatusBatal { get; set; }
        public string details { get; set; }
        public string ProfilePenandatangan { get; set; }
        public string KlasifikasiArsip { get; set; }
        public string Pemohon { get; set; }
        public string DokumenTTE { get; set; }
        public string PIC { get; set; }
        public string UnitKerja { get; set; }
        public decimal JumlahNomor { get; set; }
        public bool BackDate { get; set; }
        public bool Update { get; set; }
        public bool Book { get; set; }
        public bool ismenteri { get; set; }
        public bool isTTE { get; set; }
        public bool NoTerakhir { get; set; }
        public decimal? Sisip { get; set; }
    }

    public class DataBooking
    {
        public string Penomoranid { get; set; }
        public decimal NomorUrut { get; set; }
        public string NomorSurat { get; set; }
        public string TanggalSurat { get; set; }
        public string JenisNaskahDinas { get; set; }
    }

    public class ViewDataRequest
    {
        public List<Jabatan> ListJabatan { get; set; }
        public List<TipeSurat> ListTipeSurat { get; set; }
    }

    public class KeteranganPenomoran
    {
        public string Penomoranid { get; set; }
        public string TextKeterangan { get; set; }
        public string ValueKeterangan { get; set; }
        public string TanggalArsip { get; set; }
    }

    public class Jabatan
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string Profileid { get; set; }
        public string Nama { get; set; }
        public decimal? TipeEselonId { get; set; }
        public string KodeTTD { get; set; }
        public string UnitKerja { get; set; }
        public string Status { get; set; }
    }

    public class findDokumenTTE
    {
        public string DokumenelektronikId { get; set; }
        public string NomorSurat { get; set; }
        public string Status { get; set; }
    }
    public class RiwayatPengajuan
    {
        public string Penomoranid { get; set; }
        public string TanggalUpdate { get; set; }
        public string NomorSurat { get; set; }
        public string TanggalSurat { get; set; }
        public string Status { get; set; }
        public string Keterangan { get; set; }
        public string ProfilePenandatangan { get; set; }
        public string NamaJabatan { get; set; }
        public string JenisNaskahDinas { get; set; }
        public string Perihal { get; set; }
        public string StatusBatal { get; set; }
    }
}