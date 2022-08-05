using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

namespace Surat.Models.Entities
{
    public class InternalUserData
    {
        public string userid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string konfirmasipassword { get; set; }
        public string email { get; set; }
        public string pegawaiid { get; set; }
        public string kantorid { get; set; }
        public string namakantor { get; set; }
        public string profileidtu { get; set; }
        public string unitkerjaid { get; set; }
        public string namapegawai { get; set; }
        public int tipekantorid { get; set; }
        public List<string> userroles { get; set; }
    }

    public class InternalUserIdentity : System.Security.Principal.IIdentity, System.Security.Principal.IPrincipal
    {
        private readonly FormsAuthenticationTicket _ticket;

        public InternalUserIdentity(FormsAuthenticationTicket ticket)
        {
            _ticket = ticket;
        }

        public string AuthenticationType
        {
            get { return "User"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return _ticket.Name; }
        }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string KonfirmasiPassword { get; set; }

        public string Email { get; set; }

        public string PegawaiId { get; set; }

        public string KantorId { get; set; }

        public string NamaKantor { get; set; }

        public string ProfileIdTU { get; set; }

        public string UnitKerjaId { get; set; }

        public int TipeKantorId { get; set; }

        public string NamaPegawai { get; set; }

        public bool IsInRole(string role)
        {
            return Roles.IsUserInRole(role);
        }

        public System.Security.Principal.IIdentity Identity
        {
            get { return this; }
        }

    }

    public class AkunSaya
    {
        public string myprofileid { get; set; }
        //public string namalengkap { get; set; }
        public string tipeakun { get; set; }
        public string nomortelepon { get; set; }
        //public string username { get; set; }
        //public string email { get; set; }
        public InternalUserData DataUserData { get; set; }
        public List<Models.Entities.Profile> ListMyProfile { get; set; }
    }

    public class ProfilPengguna
    {
        [Key]
        public string profileid { get; set; }
        public string nama { get; set; }
        public string aktif { get; set; }
        public int eselon { get; set; }
    }

    public class DataPengguna
    {
        [Key]
        public string idpengguna { get; set; }
        public string idpegawai { get; set; }
        public string namapengguna { get; set; }
        public string namalengkap { get; set; }
        public string displaynip { get; set; }
        public string namajabatan { get; set; }
        public string namasatker { get; set; }
        public string kantorid { get; set; }
        public List<Kantor> kantorids { get; set; }
    }

    public class Pegawai
    {
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string NamaLengkap { get; set; }
        public string NamaDanJabatan { get; set; }
        public string ProfileId { get; set; }
        public string Jabatan { get; set; }
        public string NomorHP { get; set; }
        public string Email { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class PegawaiSimpeg
    {
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string GelarDepan { get; set; }
        public string GelarBelakang { get; set; }
        public string NamaLengkap { get; set; }
        public string Alamat { get; set; }
        public string Email { get; set; }
        public string NomorHP { get; set; }
        public string SatkerInduk { get; set; }
        public string SatkerId { get; set; }
        public string NamaSatker { get; set; }
        public string NamaJabatan { get; set; }
        public string KantorId { get; set; }
        public string Eselon { get; set; }
        public string TipePegawaiId { get; set; }
        public int? TipeKantorId { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string JabatanKKP { get; set; }
        public string EmailKKP { get; set; }
        public string NomorHPKKP { get; set; }
        public bool IsActive { get; set; }
        public List<Kantor> KantorIds { get; set; }
    }

    public class FindUserLogin
    {
        public string CariNip { get; set; }
        public string CariNik { get; set; }
        public string CariNama { get; set; }
        public string CariJabatan { get; set; }
        public string CariSatker { get; set; }

        public string PegawaiId { get; set; }
        public string PPNPNId { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string NamaLengkap { get; set; }
        public string Jabatan { get; set; }
        public string Satker { get; set; }
        public string Kategori { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string NomorTelepon { get; set; }
        public string Password { get; set; }
        public string KonfirmasiPassword { get; set; }

        public string UnitKerjaIdTujuan { get; set; }

        public List<UnitKerja> ListUnitKerja { get; set; }
        public List<Profile> ListProfile { get; set; }
        public string ProfileId { get; set; }
        public int StatusPlt { get; set; }
        bool? Is_StatusPlt;
        [Display(Name = "PLT")]
        public bool IsStatusPlt
        {
            get { return Is_StatusPlt ?? false; }
            set { Is_StatusPlt = value; }
        }
    }

    public class UserLogin
    {
        [Key]
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string NamaLengkap { get; set; }
        public string Jabatan { get; set; }
        public string Satker { get; set; }
        public string Golongan { get; set; }
        public string Foto { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string NomorTelepon { get; set; }
        public string Password { get; set; }
        public string KonfirmasiPassword { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public List<Profile> ListProfile { get; set; }
        public string ProfileId { get; set; }

        bool? Is_StatusPlt;
        [Display(Name = "PLT ?")]
        public bool IsStatusPlt
        {
            get { return Is_StatusPlt ?? false; }
            set { Is_StatusPlt = value; }
        }
        public string TipeJabatan { get; set; }
        public bool IsActive;
    }

    public class UserPPNPN
    {
        [Key]
        public string PPNPNId { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string NamaLengkap { get; set; }
        public string Satker { get; set; }
        public string Foto { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string NomorTelepon { get; set; }
        public string Password { get; set; }
        public string KonfirmasiPassword { get; set; }
        public string inSatker { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public List<Profile> ListProfile { get; set; }
        public string ProfileId { get; set; }
        public string KantorId { get; set; }
        public string KantorNama { get; set; }
    }

    public class Kantor
    {
        [Key]
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Kode { get; set; }
        public string Kota { get; set; }
        public string Alamat { get; set; }
        public string Telepon { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string KodeSK { get; set; }
        public int? TipeKantorId { get; set; }
    }

    public class TipeEselon
    {
        public int TipeEselonId { get; set; }
    }

    public class FindUnitKerja
    {
        public string CariUnitKerjaId { get; set; }
        public string CariNamaUnitKerja { get; set; }
        public string CariKode { get; set; }
        public string CariTampil { get; set; }

        bool? Satker_Pusat;
        [Display(Name = "Pusat")]
        public bool IsSatkerPusat
        {
            get { return Satker_Pusat ?? true; }
            set { Satker_Pusat = value; }
        }
        bool? Satker_Kanwil;
        [Display(Name = "Kanwil")]
        public bool IsSatkerKanwil
        {
            get { return Satker_Kanwil ?? true; }
            set { Satker_Kanwil = value; }
        }
        bool? Satker_Kantah;
        [Display(Name = "Kantah")]
        public bool IsSatkerKantah
        {
            get { return Satker_Kantah ?? true; }
            set { Satker_Kantah = value; }
        }

        public string JenisKantor { get; set; }

        public string SelectedUnitKerjaId { get; set; }
        public string UnitKerjaId { get; set; }
        public string Induk { get; set; }
        public string NamaUnitKerja { get; set; }
        public string Kode { get; set; }
        public string KantorId { get; set; }
        public decimal? Tampil { get; set; }
        public decimal? Eselon { get; set; }
        public decimal? TipeKantorId { get; set; }

        bool? Satker_Tampil;
        [Display(Name = "Tampil")]
        public bool IsSatkerTampil
        {
            get { return Satker_Tampil ?? false; }
            set { Satker_Tampil = value; }
        }
        bool? Satker_Tidak_Tampil;
        [Display(Name = "Tidak Tampil")]
        public bool IsSatkerTidakTampil
        {
            get { return Satker_Tidak_Tampil ?? false; }
            set { Satker_Tidak_Tampil = value; }
        }

        public string NewUnitKerjaId { get; set; }
        public string NewNamaUnitKerja { get; set; }
        public decimal? NewTipeKantorId { get; set; }
        public string NewKode { get; set; }
    }

    public class UnitKerja
    {
        public string UnitKerjaId { get; set; }
        public string Induk { get; set; }
        public string NamaUnitKerja { get; set; }
        public string Kode { get; set; }
        public string KantorId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string ProfileIdTU { get; set; }
        public decimal? Tampil { get; set; }
        public decimal? Eselon { get; set; }
        public decimal? TipeKantorId { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class ProfileTU
    {
        public string ProfileIdTU { get; set; }
        public string UnitKerjaId { get; set; } // Arya :: 2020-07-22
        public string KantorId { get; set; } // Arya :: 2021-08-12
    }

    public class Profile
    {
        public string ProfileId { get; set; }
        public string NamaProfile { get; set; }
        public string NamaProfilePlusID { get; set; }
        public string RoleName { get; set; }
        public string UnitKerjaId { get; set; }
        public string ProfileIdTu { get; set; }
        public string NamaProfileBaTu { get; set; }
        public decimal? TipeEselonId { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public decimal? Pilih { get; set; }
    }

    public class ProfilePegawai
    {
        public string ProfilePegawaiId { get; set; }
        public string ProfileId { get; set; }
        public string PegawaiId { get; set; }
        public string NamaProfile { get; set; }
        public string NamaProfileLengkap { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaPegawaiLengkap { get; set; }
        public string NomorTelepon { get; set; }
        public int StatusPlt { get; set; }
        public string IsStatusPlt { get; set; }
        public string Email { get; set; }
        public string NamaKantor { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string NomorSK { get; set; }
        public int BisaBooking { get; set; }
        public string KantorId { get; set; }
        public DateTime? ValidSampai { get; set; }
    }

    public class FindProfileFlow
    {
        public string ProfileDari { get; set; }
        public string ProfileTujuan { get; set; }
        public string NamaProfileDari { get; set; }
        public string NamaProfileTujuan { get; set; }
        public List<Profile> ListProfile { get; set; }
        public bool ArahBolakBalik { get; set; }
    }

    public class ProfileFlow
    {
        [Key]
        public string ProfileFlowId { get; set; }
        public string KantorId { get; set; }
        public string ProfileDari { get; set; }
        public string ProfileTujuan { get; set; }
        public string NamaProfileDari { get; set; }
        public string NamaProfileTujuan { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class FindJabatan
    {
        public string CariProfileId { get; set; }
        public string CariNamaProfile { get; set; }
        public string CariUnitKerjaId { get; set; }

        public string NewProfileId { get; set; }
        public string UserId { get; set; }

        public string SelectedProfileId { get; set; }
        public string SelectedProfileIdTU { get; set; }
        public string SelectedProfileIdBA { get; set; }
        public string ProfileId { get; set; }
        public string NamaProfile { get; set; }

        public string UnitKerjaId { get; set; }
        public string NamaUnitKerja { get; set; }

        public string UnitKerjaIdTU { get; set; }
        public string NamaUnitKerjaTU { get; set; }

        public string UnitKerjaIdBA { get; set; }
        public string NamaUnitKerjaBA { get; set; }

        public string ProfileIdTU { get; set; }
        public string NamaProfileTU { get; set; }

        public string ProfileIdBA { get; set; }
        public string NamaProfileBA { get; set; }

        public List<UnitKerja> ListUnitKerja { get; set; }
        public List<Profile> ListProfileTU { get; set; }
        public List<Profile> ListProfileBA { get; set; }
    }

    public class ListJabatan
    {
        [Key]
        public string ProfileId { get; set; }
        public string NamaProfile { get; set; }
        public string UnitKerjaId { get; set; }
        public string NamaUnitKerja { get; set; }
        public string ProfileIdTU { get; set; }
        public string NamaProfileTU { get; set; }
        public string ProfileIdBA { get; set; }
        public string NamaProfileBA { get; set; }
        public string UnitKerjaIdTU { get; set; }
        public string NamaUnitKerjaTU { get; set; }
        public string UnitKerjaIdBA { get; set; }
        public string NamaUnitKerjaBA { get; set; }
        public string NewProfileId { get; set; }
        public string UserId { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class FindProfileTataUsaha
    {
        public string CariProfileId { get; set; }
        public string CariNamaProfile { get; set; }
        public string CariNamaProfileTU { get; set; }
        public List<Profile> ListProfile { get; set; }

        public string SelectedProfileId { get; set; }
        public string ProfileId { get; set; }
        public string NamaProfile { get; set; }
        public string ProfileIdTU { get; set; }
        public string NamaProfileTU { get; set; }
    }

    public class ProfileTataUsaha
    {
        [Key]
        public string ProfileId { get; set; }
        public string NamaProfile { get; set; }
        public string ProfileIdTU { get; set; }
        public string NamaProfileTU { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class SetAdminSatker
    {
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string ProfileId { get; set; }
        public string PegawaiId { get; set; }
        public string KategoriSatker { get; set; }
        public string SatkerId { get; set; }
        public int? TipeKantorId { get; set; }
        public string NomorHP { get; set; }
        public string Email { get; set; }

        public string NamaLengkap { get; set; }
        public string NamaSatker { get; set; }
        public string NamaJabatan { get; set; }

        public string UserId { get; set; }
        public string NamaJabatanKKP { get; set; }
        public string NomorHPKKP { get; set; }
        public string EmailKKP { get; set; }

        //public List<Kantor> ListKantor { get; set; }
    }

    public class GantiPassword
    {
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string ProfileId { get; set; }
        public string PegawaiId { get; set; }
        public string KategoriSatker { get; set; }
        public string SatkerId { get; set; }
        public int? TipeKantorId { get; set; }
        public string NomorHP { get; set; }
        public string Email { get; set; }

        public string NamaLengkap { get; set; }
        public string NamaSatker { get; set; }
        public string NamaJabatan { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordLama { get; set; }
        public string NamaJabatanKKP { get; set; }
        public string NomorHPKKP { get; set; }
        public string EmailKKP { get; set; }
    }

    public class AksesKhusus
    {
        public string ProfileId { get; set; }
        public string ProfileNama { get; set; }
        public int Status { get; set; }
    }

    public class FindPengalihanSurat
    {
        public string UnitKerjaId { get; set; }
        public string ProfileId { get; set; }
    }
}