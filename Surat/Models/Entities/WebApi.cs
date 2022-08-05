using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Surat.Models.Entities
{
    public class WebApiUser
    {
        public string id_pengguna { get; set; }
        public string nama_pengguna { get; set; }
        public string email { get; set; }
        public string id_pegawai { get; set; }
        public string namapegawai { get; set; }
        public string jabatan { get; set; }
        public string id_jabatan { get; set; }
        public string kantorid { get; set; }
        public string namakantor { get; set; }
        public string auth_token { get; set; }
        public string pesan { get; set; }
        public string latitudekantor { get; set; }
        public string longitudekantor { get; set; }
        public string zonawaktu { get; set; }
        public List<AnakKantor> anakKantors { get; set; }
        public string tipe_user { get; set; }
        public string foto_profil { get; set; }
        public string unitkerjaid { get; set; }
        public string namaunitkerja { get; set; }
        public int tipekantorid { get; set; }
        public int istu { get; set; }
    }
    
    public class AnakKantor
    {
        public string id_anakkantor { get; set; }
        public string anakkantor { get; set; }
        public string anakkantorlat { get; set; }
        public string anakkantorlong { get; set; }
        public string anakkantorzonawaktu { get; set; }
    }
    public class DataSurat
    {
        public string id_pengguna { get; set; }
        public string nama_pengguna { get; set; }
        public string id_pegawai { get; set; }
        public string namapegawai { get; set; }
        public string jabatan { get; set; }
        public string kantorid { get; set; }
        public string foto_profil { get; set; }
        public string jumlah_surat { get; set; }
        public string jumlah_inbox { get; set; }
        public string jumlah_terkirim { get; set; }
        public string jumlah_selesai { get; set; }
        public string jumlah_belum_terbaca { get; set; }
        public string jumlah_rapat_online { get; set; }
        public string jumlah_permintaan_tte { get; set; }
        public string inisiatif {get; set; }
        public string kotak_masuk {get; set; }
        public string pesan { get; set; }
    }

    public class TokenValid
    {
        public string id_pegawai { get; set; }
        public string nama_pengguna { get; set; }
        public string status { get; set; }
    }
}