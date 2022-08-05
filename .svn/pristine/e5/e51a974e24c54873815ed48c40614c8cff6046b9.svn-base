using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Surat.Models.Entities
{
    public class CariPengumuman
    {
        public string MetaData { get; set; }
        public string Tipe { get; set; }
        public string SortBy { get; set; }
        public bool Status { get; set; }
    }

    public class DataPengumuman
    {
        public decimal RNumber { get; set; }
        public decimal Total { get; set; }
        public string PengumumanID { get; set; }
        public DateTime TanggalBuat { get; set; }
        public DateTime TanggalUbah { get; set; }
        public string Judul { get; set; }
        [AllowHtml]
        public string Isi { get; set; }
        public DateTime? ValidSejak { get; set; }
        public DateTime? ValidSampai { get; set; }
        public string Jadwal
        {
            get { return ValidSejak.Equals(ValidSampai) ? ((DateTime)ValidSejak).ToString(@"dd/MM/yyyy") : string.Concat(((DateTime)ValidSejak).ToString(@"dd/MM/yyyy"), " - ", ((DateTime)ValidSampai).ToString(@"dd/MM/yyyy")); }
            set { ValidSejak = DateTime.Parse(value); }
        }
        public string Target { get; set; }
        public string DetailTarget { get; set; }
        public string WebUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public string UnitKerjaId { get; set; }
        public string UserId { get; set; }
        public string Nip { get; set; }
        public string NamaPengirim { get; set; }
        public string UnitKerjaPenerima { get; set; }
        public List<UnitKerja> ListUnitKerja { get; set; }
        public List<FilePengumuman> Gambar { get; set; }
        public List<SelectListItem> PilihanTarget { get; set; }
        public string NamaFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
    }
    public class FilePengumuman
    {
        public string FileId { get; set; }
        public string NamaFile { get; set; }
        public string ExtFile { get; set; }
    }
}