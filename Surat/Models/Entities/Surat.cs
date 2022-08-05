﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Surat.Models.Entities
{
    #region Flow Surat

    public class InfoSurat
    {
        public Surat DataSurat { get; set; }
        public List<Models.Entities.SuratInbox> ListSuratInbox { get; set; }
    }

    public class ReferensiSurat
    {
        public string SuratId { get; set; }
        public string NomorSurat { get; set; }
        public string NomorAgenda { get; set; }
        public string Referensi { get; set; }
    }

    public class FindSurat
    {
        public string Metadata { get; set; }
        public string NomorSurat { get; set; }
        public string TanggalSurat { get; set; }
        public string TanggalInput { get; set; }
        public string TanggalDari { get; set; }
        public string TanggalSampai { get; set; }
        public string NomorAgenda { get; set; }
        public string Perihal { get; set; }
        public string TipeSurat { get; set; }
        public string SifatSurat { get; set; }
        public string PenerimaSurat { get; set; }
        public string KeteranganSurat { get; set; }
        public string Redaksi { get; set; }
        public List<TipeSurat> ListTipeSurat { get; set; }
        public List<SifatSurat> ListSifatSurat { get; set; }
        public List<KodeKlasifikasi> ListKodeKlasifikasi { get; set; }

        public string UnitKerjaIdTujuan { get; set; }
        public string ProfileIdTujuan { get; set; }
        public List<UnitKerja> ListUnitKerja { get; set; }
        public List<Profile> ListProfileTujuan { get; set; }
        public List<ProfilePegawai> ListMyProfiles { get; set; }
        public List<Pegawai> ListPegawai { get; set; }
        public string NipPenerima { get; set; }
        public int JumlahMyProfiles { get; set; }

        public string SortBy { get; set; }
        public string SortType { get; set; }
        public string SpesificProfileId { get; set; }
        public string StatusSurat { get; set; }
        public string KategoriSurat { get; set; }

        public string Sumber_Keterangan { get; set; }
        public string Arah { get; set; }
    }

    public class SuratIds
    {
        public string suratid { get; set; }
        public string profileidtujuan { get; set; }
        public string namapenerima { get; set; }
        public string tanggalinput { get; set; }
        public string tanggaldari { get; set; }
        public string tanggalsampai { get; set; }
        public string suratinboxid { get; set; }
    }

    public class Surat
    {
        [Key]
        public string SuratId { get; set; }
        public string SuratInboxId { get; set; }
        public string UserId { get; set; }
        public string PegawaiId { get; set; }
        public string TanggalSurat { get; set; }
        public string TanggalProses { get; set; }
        public string TanggalArsip { get; set; }
        public string TargetSelesai { get; set; }
        public string InfoTargetSelesai { get; set; }
        public string TargetSelesaiSuratMasuk { get; set; }
        public string InfoTargetSelesaiSuratMasuk { get; set; }
        public string InfoTanggalInput { get; set; }
        public string TanggalUndangan { get; set; }
        public string InfoTanggalUndangan { get; set; }
        //[Required(ErrorMessage = "Nomor Surat Wajib Diisi.")]
        [Display(Name = "Nomor Surat")]
        public string NomorSurat { get; set; }
        //[Required(ErrorMessage = "Nomor Agenda Wajib Diisi.")]
        [Display(Name = "Nomor Agenda")]
        public string NomorAgenda { get; set; }
        public string NomorAgendaSurat { get; set; }
        [Required(ErrorMessage = "Perihal Wajib Diisi.")]
        [Display(Name = "Perihal")]
        public string Perihal { get; set; }
        public string PengirimSurat { get; set; }
        public string PenerimaSurat { get; set; }
        public string Arah { get; set; }
        public string ArahSurat { get; set; }
        public string SifatSurat { get; set; }
        public string TipeSurat { get; set; }
        public string Kategori { get; set; }
        public string ArahSuratKeluar { get; set; }
        public string KeteranganSurat { get; set; }
        public string KeteranganSuratRedaksi { get; set; }
        public string PerintahDisposisi { get; set; }
        public string PerintahDisposisiSebelumnya { get; set; }
        public List<string> ArrPerintahDisposisi { get; set; }
        public string KodeKlasifikasi { get; set; }
        public string NomenklaturSeksi { get; set; }
        public string NamaSeksi { get; set; }
        public int? JumlahLampiran { get; set; }
        public string IsiSingkatSurat { get; set; }
        public int? StatusSurat { get; set; }
        public int? StatusArsip { get; set; }
        public string ReferensiSurat { get; set; }
        public string UnitKerjaIdTujuan { get; set; }
        public string UnitKerjaIdHistoriSurat { get; set; }
        public string ProfileIdTujuan { get; set; }
        public string PegawaiIdTujuan { get; set; }
        public string PegawaiIdTujuanEkspedisi { get; set; }
        public string NamaProfileTujuan { get; set; }
        public string NamaPegawaiTujuan { get; set; }
        public string InfoNamaProfileTujuan { get; set; }
        public string InfoNamaPegawaiTujuan { get; set; }
        public string Referensi { get; set; }
        public string ReferensiAsalSurat { get; set; }
        public string ReferensiTujuanSurat { get; set; }
        public string ReferensiNomorAgenda { get; set; }
        public string ReferensiNomorSurat { get; set; }
        public string ReferensiTanggalSurat { get; set; }
        public string ReferensiPerihal { get; set; }
        public string ReferensiKategori { get; set; }
        public string ReferensiUnitKerjaId { get; set; }
        public string IsBedaUnitKerja { get; set; }
        public string Output { get; set; }
        public string LabelTitleJenisSurat { get; set; }

        public string TanggalKirim { get; set; }
        public string TanggalBuka { get; set; }
        public string TanggalTerima { get; set; }
        public string TanggalInput { get; set; }
        public string TanggalInbox { get; set; }
        public string InfoTanggalSurat { get; set; }
        public string InfoTanggalTerima { get; set; }
        public string InfoTanggalTerimaCetak { get; set; }
        public string InfoTanggalProses { get; set; }
        public string NIP { get; set; }
        public string KategoriTujuanSurat { get; set; }
        public string ProfileIdPengirim { get; set; }
        public string ProfileIdPenerima { get; set; }
        public string ProfileIdPenerimaEkspedisi { get; set; }
        public string ProfilePengirim { get; set; }
        public string ProfilePenerima { get; set; }
        public string ProfilePegawaiIdPenerima { get; set; }
        public string TindakLanjut { get; set; }
        public string NamaPegawai { get; set; }
        public string Keterangan { get; set; }
        public string Redaksi { get; set; }
        public int? StatusKirim { get; set; }
        public int? StatusBuka { get; set; }
        public int? StatusTerkunci { get; set; }
        public int? StatusForwardTU { get; set; }
        public int? StatusUrgent { get; set; }
        bool? Status_Urgent;
        [Display(Name = "Urgent")]
        public bool IsStatusUrgent
        {
            get { return Status_Urgent ?? false; }
            set { Status_Urgent = value; }
        }

        public string TanggalTerimaFisik { get; set; }
        public string TerimaFisik { get; set; }
        public string NamaProfilePengirim { get; set; }
        public string NamaProfilePenerima { get; set; }
        public string NamaPengirim { get; set; }
        public string NamaPenerima { get; set; }
        public string CatatanSebelumnya { get; set; }
        //[Required(ErrorMessage = "Catatan Wajib Diisi.")]
        [Display(Name = "Catatan Anda")]
        public string CatatanAnda { get; set; }
        public int? Urutan { get; set; }
        public string Node { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public string LampiranSuratId { get; set; }
        public string FolderFile { get; set; } // path
        public string NamaFile { get; set; }
        public string TipeFile { get; set; }
        public byte[] ObjectFile { get; set; }

        public List<TipeSurat> ListTipeSurat { get; set; }
        public List<SifatSurat> ListSifatSurat { get; set; }
        public List<PerintahDisposisi> ListPerintahDisposisi { get; set; }
        public List<KodeKlasifikasi> ListKodeKlasifikasi { get; set; }
        public List<Seksi> ListSeksi { get; set; }
        public List<UnitKerja> ListUnitKerja { get; set; }
        public List<UnitKerja> ListUnitKerjaHistoriSurat { get; set; }
        public List<Profile> ListProfileTujuan { get; set; }
        public List<Pegawai> ListTujuanPegawai { get; set; }
        public List<ProfilePegawai> ListProfilePegawai { get; set; }
        public List<Profile> ListProfilePenerima { get; set; }
        public List<ProfilePegawai> ListProfilePegawaiPenerima { get; set; }

        public List<Profile> ListProfileDisposisi { get; set; }
        public string ProfileIdDisposisi { get; set; }

        public string CariNipPegawai { get; set; }
        public string CariNamaPegawai { get; set; }
        public List<Profile> ListProfiles { get; set; }
        public string ProfileIdSelected { get; set; }
        public string PegawaiIdSelected { get; set; }

        public string IsProfileTataUsaha { get; set; }

        public string IsProfileFixLembarDispo { get; set; }

        public string PenanggungJawab { get; set; }
        public List<SessionTujuanSurat> ListTujuanSurat { get; set; }
        public List<TujuanSurat> ListTujuan { get; set; }
        public List<Files> ListFiles { get; set; }
        public bool kirimPusat { get; set; }
        public bool kirimKanwil { get; set; }
        public bool kirimKantah { get; set; }
        public string NomorUmum { get; set; }
        public string DaftarTujuan { get; set; }
        public bool MergerSurat { get; set; }
        [Required]
        public string Sumber_Keterangan { get; set; }
        public DiffWaktu Waktu { get; set; }
        public DiffWaktu WaktuTunggak { get; set; }
        public List<Kantor> ListKantor { get; set; }

    }

    public class StatusSurat
    {
        public string SuratInboxId { get; set; }
        public string SuratId { get; set; }
        public string UnitKerjaId { get; set; }
        public string Status { get; set; }
        public string Nama { get; set; }
        public string Keterangan { get; set; }
        public string Redaksi { get; set; }
        public decimal? Urut { get; set; }
    }


    public class SuratInbox
    {
        [Key]
        public string SuratInboxId { get; set; }
        public string SuratId { get; set; }
        public string NIP { get; set; }
        public string ProfileIdPengirim { get; set; }
        public string ProfileIdPenerima { get; set; }
        public string ProfilePengirim { get; set; }
        public string ProfilePenerima { get; set; }
        public string ProfilePegawaiIdPenerima { get; set; }
        public string TanggalKirim { get; set; }
        public string TanggalBuka { get; set; }
        public string TanggalTerima { get; set; }
        public string TanggalInput { get; set; }
        public string InfoTanggalSurat { get; set; }
        public string InfoTanggalTerima { get; set; }
        public string InfoTanggalKirim { get; set; }
        public string InfoTanggalProses { get; set; }
        public string InfoTanggalBuka { get; set; }
        public string TindakLanjut { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaPengirim { get; set; }
        public string NamaPenerima { get; set; }
        public string Keterangan { get; set; }
        public string Kategori { get; set; }
        public int? StatusKirim { get; set; }
        public int? StatusBuka { get; set; }
        public int? StatusTerkunci { get; set; }
        public int? StatusForwardTU { get; set; }
        public int? StatusUrgent { get; set; }
        public string TanggalSurat { get; set; }
        public string TanggalProses { get; set; }
        public string TanggalArsip { get; set; }
        public string TargetSelesai { get; set; }
        public string InfoTargetSelesai { get; set; }
        public string TargetSelesaiSuratMasuk { get; set; }
        public string InfoTargetSelesaiSuratMasuk { get; set; }
        public string InfoTanggalInput { get; set; }
        public string TanggalUndangan { get; set; }
        public string InfoTanggalUndangan { get; set; }
        public string NomorSurat { get; set; }
        public string NomorAgenda { get; set; }
        public string NomorAgendaSurat { get; set; }
        public string Perihal { get; set; }
        public string PengirimSurat { get; set; }
        public string PenerimaSurat { get; set; }
        public string Arah { get; set; }
        public string ArahSurat { get; set; }
        public string SifatSurat { get; set; }
        public string TipeSurat { get; set; }
        public string KeteranganSurat { get; set; }
        public string Redaksi { get; set; }
        public string PerintahDisposisi { get; set; }
        public string PerintahDisposisiSebelumnya { get; set; }
        public List<string> ListPerintahDisposisi { get; set; }
        public string KodeKlasifikasi { get; set; }
        public string NomenklaturSeksi { get; set; }
        public int? JumlahLampiran { get; set; }
        public int? StatusSurat { get; set; }
        public int? StatusArsip { get; set; }
        public string IsiSingkatSurat { get; set; }
        public string ReferensiSurat { get; set; }
        public string NamaProfilePengirim { get; set; }
        public string NamaProfilePenerima { get; set; }
        public string KetStatusTerkirim { get; set; }
        public string Node { get; set; }
        public string StatusHapus { get; set; }
        public string TanggalKembali { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public string LampiranSuratId { get; set; }
        public string FolderFile { get; set; } // path
        public string NamaFile { get; set; }
        public string TipeFile { get; set; }
        public int Tier { get; set; }

        public List<TipeSurat> ListTipeSurat { get; set; }
        public List<SifatSurat> ListSifatSurat { get; set; }
        public List<KodeKlasifikasi> ListKodeKlasifikasi { get; set; }
        public List<Seksi> ListSeksi { get; set; }
        public List<ProfilePegawai> ListProfilePegawai { get; set; }
        public List<Profile> ListProfilePenerima { get; set; }
        public List<ProfilePegawai> ListProfilePegawaiPenerima { get; set; }

        public string UnitKerjaNama { get; set; }

        public string Sumber_Keterangan { get; set; }
    }

    public class SuratOutbox
    {
        [Key]
        public string SuratOutboxId { get; set; }
        public string SuratInboxId { get; set; }
        public string SuratId { get; set; }
        public string NIP { get; set; }
        public string ProfileIdPengirim { get; set; }
        public string ProfileIdPenerima { get; set; }
        public string ProfilePengirim { get; set; }
        public string ProfilePenerima { get; set; }
        public string ProfilePegawaiIdPenerima { get; set; }
        public string TanggalKirim { get; set; }
        public string TanggalBuka { get; set; }
        public string TanggalTerima { get; set; }
        public string TanggalInput { get; set; }
        public string InfoTanggalSurat { get; set; }
        public string InfoTanggalTerima { get; set; }
        public string InfoTanggalKirim { get; set; }
        public string InfoTanggalProses { get; set; }
        public string InfoTanggalBuka { get; set; }
        public string TindakLanjut { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaPengirim { get; set; }
        public string NamaPenerima { get; set; }
        public string Keterangan { get; set; }
        public int? StatusKirim { get; set; }
        public int? StatusBuka { get; set; }
        public int? StatusTerkunci { get; set; }
        public int? StatusForwardTU { get; set; }
        public string TanggalSurat { get; set; }
        public string TanggalProses { get; set; }
        public string TanggalArsip { get; set; }
        public string TargetSelesai { get; set; }
        public string InfoTargetSelesai { get; set; }
        public string InfoTanggalInput { get; set; }
        public string TanggalUndangan { get; set; }
        public string InfoTanggalUndangan { get; set; }
        public string NomorSurat { get; set; }
        public string NomorAgenda { get; set; }
        public string NomorAgendaSurat { get; set; }
        public string Perihal { get; set; }
        public string PengirimSurat { get; set; }
        public string PenerimaSurat { get; set; }
        public string Arah { get; set; }
        public string ArahSurat { get; set; }
        public string SifatSurat { get; set; }
        public string TipeSurat { get; set; }
        public string KeteranganSurat { get; set; }
        public string Redaksi { get; set; }
        public string PerintahDisposisi { get; set; }
        public List<string> ListPerintahDisposisi { get; set; }
        public string KodeKlasifikasi { get; set; }
        public string NomenklaturSeksi { get; set; }
        public int? JumlahLampiran { get; set; }
        public int? StatusSurat { get; set; }
        public int? StatusArsip { get; set; }
        public string IsiSingkatSurat { get; set; }
        public string ReferensiSurat { get; set; }
        public string NamaProfilePengirim { get; set; }
        public string NamaProfilePenerima { get; set; }
        public string KetStatusTerkirim { get; set; }
        public string Node { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public string LampiranSuratId { get; set; }
        public string FolderFile { get; set; } // path
        public string NamaFile { get; set; }
        public string TipeFile { get; set; }

        public List<TipeSurat> ListTipeSurat { get; set; }
        public List<SifatSurat> ListSifatSurat { get; set; }
        public List<KodeKlasifikasi> ListKodeKlasifikasi { get; set; }
        public List<Seksi> ListSeksi { get; set; }
        public List<ProfilePegawai> ListProfilePegawai { get; set; }
        public List<Profile> ListProfilePenerima { get; set; }
        public List<ProfilePegawai> ListProfilePegawaiPenerima { get; set; }

        public string Sumber_Keterangan { get; set; }
    }

    public class SessionLampiranSurat
    {
        public decimal? RNumber { get; set; }
        public string LampiranSuratId { get; set; }
        public string NamaFile { get; set; }
        public string UserId { get; set; }
        public byte[] ObjectFile { get; set; }
        public string Nip { get; set; }
        public string Ext { get; set; }
    }

    public class LampiranSurat
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string LampiranSuratId { get; set; }
        public string SuratId { get; set; }
        public string FolderFile { get; set; }
        public string NamaFile { get; set; }
        public string Keterangan { get; set; }
        public string ProfileId { get; set; }
        public byte[] ObjectFile { get; set; }
        public string Nip { get; set; }
        public string NamaPegawai { get; set; }
        public string Tanggal { get; set; }
        public string KantorId { get; set; }
    }

    public class SessionTujuanSurat
    {
        public decimal? RNumber { get; set; }
        public string TujuanSuratId { get; set; }
        public string UserId { get; set; }
        public string Redaksi { get; set; }
        public string ProfileId { get; set; }
        public string NIP { get; set; }
        public string NamaJabatan { get; set; }
        public string NamaPegawai { get; set; }
        public int? StatusUrgent { get; set; }
        bool? Status_Urgent;
        [Display(Name = "Urgent")]
        public bool IsStatusUrgent
        {
            get { return Status_Urgent ?? false; }
            set { Status_Urgent = value; }
        }
        public string UnitKerjaId { get; set; }
        public string KantorId { get; set; }
    }

    public class TujuanSuratKeluar
    {
        public decimal? RNumber { get; set; }
        public string TujuanSuratKeluarId { get; set; }
        public string SuratId { get; set; }
        public string Redaksi { get; set; }
        public string ProfileId { get; set; }
        public string NIP { get; set; }
        public string NamaJabatan { get; set; }
        public string NamaPegawai { get; set; }
    }

    public class DisposisiSurat
    {
        public decimal? RNumber { get; set; }
        public string DisposisiSuratId { get; set; }
        public string SuratId { get; set; }
        public string UnitKerjaId { get; set; }
        public string ProfileId { get; set; }
        public string NIP { get; set; }
        public string NamaJabatan { get; set; }
        public string NamaPegawai { get; set; }
        public int TipeEselonId { get; set; }
    }

    public class SuratKembali
    {
        public string SuratKembaliId { get; set; }
        public string SuratInboxId { get; set; }
        public string SuratOutboxId { get; set; }
        public string TanggalKembali { get; set; }
        public string Keterangan { get; set; }
        public string ProfilePengirim { get; set; }
        public string NipPengirim { get; set; }
        public string NamaPengirim { get; set; }
        public string ProfilePenerima { get; set; }
        public string NipPenerima { get; set; }
        public string NamaPenerima { get; set; }
        public string SuratId { get; set; }
        public string NomorSurat { get; set; }
        public string NomorAgenda { get; set; }
        public string Perihal { get; set; }
        public string AsalSurat { get; set; }

        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }

        public string UserId { get; set; }
    }

    public class FindPengantarSurat
    {
        public string Metadata { get; set; }

        public string PengantarSuratId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Nomor { get; set; }
        public string Tujuan { get; set; }
        public string TanggalDari { get; set; }
        public string TanggalSampai { get; set; }
        public string TanggalTerima { get; set; }
        public string TanggalInput { get; set; }
        public string Waktu { get; set; }
        public string NamaPenerima { get; set; }
        public string InfoTanggalDari { get; set; }
        public string InfoTanggalSampai { get; set; }
        public string InfoTanggalTerima { get; set; }

        public string DetilPengantarId { get; set; }
        public string SuratId { get; set; }
        public string TanggalSurat { get; set; }
        public string NomorSurat { get; set; }
        public string NomorAgenda { get; set; }
        public string Perihal { get; set; }
        public string Pengirim { get; set; }
        public string SifatSurat { get; set; }
        public string KeteranganSurat { get; set; }
        public string Redaksi { get; set; }

        public string NomorEdit { get; set; }
        public string TanggalDariEdit { get; set; }
        public string TanggalSampaiEdit { get; set; }
        public string TujuanEdit { get; set; }
        public string TanggalTerimaEdit { get; set; }
        public string TanggalInputEdit { get; set; }
        public string NamaPenerimaEdit { get; set; }
        public string InfoTanggalSurat { get; set; }
    }

    public class PengantarSurat
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PengantarSuratId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Nomor { get; set; }
        public string Tujuan { get; set; }
        public string ProfileIdTujuan { get; set; }
        public string TanggalDari { get; set; }
        public string TanggalSampai { get; set; }
        public string TanggalTerima { get; set; }
        public string TanggalInput { get; set; }
        public string Waktu { get; set; }
        public string NamaPenerima { get; set; }
        public string InfoTanggalDari { get; set; }
        public string InfoTanggalSampai { get; set; }
        public string InfoTanggalTerima { get; set; }
        public string Status { get; set; }
        public string stCheck { get; set; }
        public string stTTE { get; set; }
        public string TanggalSurat { get; set; }
        public string UserId { get; set; }
        public string UnitKerjaId { get; set; }
        public string NomorSurat { get; set; }
        public string TujuanSurat { get; set; }
        public string Penandatangan { get; set; }
        public string LstSurat { get; set; }
        public string StatusTTE { get; set; }
        public string NamaPembuat { get; set; }
        public bool Gnumber { get; set; }
    }

    public class DetilPengantar
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string DetilPengantarId { get; set; }
        public string PengantarSuratId { get; set; }
        public string SuratId { get; set; }
        public string TanggalSurat { get; set; }
        public string NomorSurat { get; set; }
        public string NomorAgenda { get; set; }
        public string Perihal { get; set; }
        public string Pengirim { get; set; }
        public string SifatSurat { get; set; }
        public string KeteranganSurat { get; set; }
        public string Redaksi { get; set; }
    }

    public class FindNotulen
    {
        public string Metadata { get; set; }
    }

    public class Notulen
    {
        public string SuratId { get; set; }
        public string TanggalSurat { get; set; }
        public string NomorSurat { get; set; }
        public string Perihal { get; set; }
        public string Kategori { get; set; }

        public string NotulenId { get; set; }
        public string UnitKerjaId { get; set; }
        public string NIP { get; set; }
        public string NamaPegawai { get; set; }
        public string Judul { get; set; }
        public string JudulShort { get; set; }
        public string Tanggal { get; set; }
        public string TanggalInfo { get; set; }
        public string TanggalInfo2 { get; set; }
        public string Waktu { get; set; }
        public string Tempat { get; set; }
        public string IsiNotulen { get; set; }
        public string Metadata { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class DelegasiSurat
    {
        public string SuraDelegasiSuratId { get; set; }
        public string ProfilePengirim { get; set; }
        public string ProfilePenerima { get; set; }
        public string NIPPenerima { get; set; }
        public string NamaPenerima { get; set; }
        public string Tanggal { get; set; }
        public int Status { get; set; }
    }

    public class ListDelegasi
    {
        [Key]
        public string DelegasiSuratId { get; set; }
        public string UnitKerjaPengirim { get; set; }
        public string ProfilePengirim { get; set; }
        public string JabatanPengirim { get; set; }
        public string UnitKerjaPenerima { get; set; }
        public string ProfilePenerima { get; set; }
        public string JabatanPenerima { get; set; }
        public DateTime Tanggal { get; set; }
        public int Status { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class FindDelegasi
    {
        public string CariDelegasiSuratId { get; set; }
        public string CariUnitKerjaPengirim { get; set; }
        public string CariProfilePengirim { get; set; }
        public string CariUnitKerjaPenerima { get; set; }
        public string CariProfilePenerima { get; set; }
        public string DelegasiSuratId { get; set; }
        public string ProfilePengirim { get; set; }
        public string ProfilePenerima { get; set; }
        public int Status { get; set; }
        public List<UnitKerja> ListUnitKerja { get; set; }
        public List<Profile> ListProfilePengirim { get; set; }
        public List<Profile> ListProfilePenerima { get; set; }
    }

    #endregion

    #region Surat Pengantar

    public class CariSuratPengantar
    {
        public string Nomor { get; set; }
        public string Tanggal { get; set; }
        public string Tujuan { get; set; }
        public string MetaData { get; set; }
        public string SortBy { get; set; }
        public string Tipe { get; set; }
        public string UnitKerjaId { get; set; }
        public bool Status { get; set; }
    }

    public class SuratPengantar
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PengantarSuratId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Nomor { get; set; }
        public string Tujuan { get; set; }
        public string ProfileIdTujuan { get; set; }
        public string TanggalDari { get; set; }
        public string TanggalSampai { get; set; }
        public string TanggalTerima { get; set; }
        public string TanggalInput { get; set; }
        public string Waktu { get; set; }
        public string NamaPenerima { get; set; }
        public string InfoTanggalDari { get; set; }
        public string InfoTanggalSampai { get; set; }
        public string InfoTanggalTerima { get; set; }
    }


    #endregion

    #region Rapat Online

    public class FindRapatOnline
    {
        public string Metadata { get; set; }
    }

    public class RapatOnline
    {
        public string RapatOnlineId { get; set; }
        public string UnitKerjaId { get; set; }
        public string Judul { get; set; }
        public string JudulShort { get; set; }
        public string Tanggal { get; set; }
        public string TanggalInfo { get; set; }
        public string TanggalInfo2 { get; set; }
        public string TanggalInfoLengkap { get; set; }
        public string UrlMeeting { get; set; }
        public string Waktu { get; set; }
        public string Keterangan { get; set; }
        public string TipeRapat { get; set; }
        public string Metadata { get; set; }
        public int LewatJatuhTempo { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KodeRapat { get; set; }
        public decimal? Jumlah_Peserta { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }


        // PesertaRapatOnline
        public string PesertaRapatOnlineId { get; set; }
        public string Nip { get; set; }
        public string ProfileId { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaJabatan { get; set; }
        public string KeteranganPeserta { get; set; }
        public string ProfileIdSelected { get; set; }
        public string PegawaiIdSelected { get; set; }
        public string UnitKerjaIdTujuan { get; set; }
        public string NamaProfileTujuan { get; set; }
        public string NamaPegawaiTujuan { get; set; }
        public List<UnitKerja> ListUnitKerja { get; set; }


        // LampiranRapatOnline
        public string LampiranRapatOnlineId { get; set; }
        public string NamaFile { get; set; }
        public string TanggalFile { get; set; }
        public string JudulLampiran { get; set; }
        public string TipeFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public HttpPostedFileBase FileDokumen { get; set; }
        public string NipPengupload { get; set; }
        public string NamaPengupload { get; set; }
        public string UrlFile { get; set; }

        public List<AbsensiRapatOnline> listAbsensi { get; set; }
        public string QRCode { get; set; }
        public string KodePeserta { get; set; }
        public string NamaPeserta { get; set; }
        public List<RekapPresensiRapatUnitKerja> listPresensi { get; set; }
    }

    public class PesertaRapatOnline
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PesertaRapatOnlineId { get; set; }
        public string RapatOnlineId { get; set; }
        public string Nip { get; set; }
        public string ProfileId { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaJabatan { get; set; }
        public string KeteranganPeserta { get; set; }
        public int Terkonfirmasi { get; set; }
    }

    public class LampiranRapatOnline
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string LampiranRapatOnlineId { get; set; }
        public string RapatOnlineId { get; set; }
        public string NamaFile { get; set; }
        public string TanggalFile { get; set; }
        public string JudulLampiran { get; set; }
        public string TipeFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public HttpPostedFileBase FileDokumen { get; set; }
        public string NipPengupload { get; set; }
        public string NamaPengupload { get; set; }
        public string UrlFile { get; set; }
    }

    public class AbsensiRapatOnline
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PegawaiID { get; set; }
        public string Tanggal { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Nama { get; set; }
        public string Jabatan { get; set; }
        public string Eselon { get; set; }
        public string KantorId { get; set; }
        public int Terdaftar { get; set; }
        public int Terkonfirmasi { get; set; }
    }

    public class LokasiKantor
    {
        public string kantorId { get; set; }
        public string Nama { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime ServerTime { get; set; }
        public int Ct { get; set; }
    }

    public class RekapPresensiRapat
    {
        public List<RekapPresensiRapatUnitKerja> list { get; set; }
        public string NamaRapat { get; set; }
    }

    public class RekapPresensiRapatUnitKerja
    {
        public string Induk { get; set; }
        public string Kantor { get; set; }
        public int Tipe { get; set; }
        public string UnitKerja { get; set; }
        public decimal Jumlah { get; set; }
    }

    #endregion


    #region Tanda Tangan Dokumen

    public class CariDokumenTTE
    {
        public string NomorSurat { get; set; }
        public string SifatSurat { get; set; }
        public string TanggalDibuatDari { get; set; }
        public string TanggalDibuatSampai { get; set; }
        public string MetaData { get; set; }
        public string SortBy { get; set; }
        public string Tipe { get; set; }
        public string KantorId { get; set; }
        public bool status { get; set; }
    }

    public class DokumenTTE
    {
        public string DokumenElektronikId { get; set; }

        [Required(ErrorMessage = "Nomor Surat Harus Diisi")]
        public string NomorSurat { get; set; }

        [Required(ErrorMessage = "Tanggal Harus Diisi")]
        public string TanggalSurat { get; set; }

        [Required(ErrorMessage = "Perihal Surat Harus Diisi")]
        public string Perihal { get; set; }

        [Required(ErrorMessage = "Sifat Surat Harus Diisi")]
        public string SifatSurat { get; set; }

        [Required(ErrorMessage = "Tipe Surat Harus Diisi")]
        public string TipeSurat { get; set; }

        [Required(ErrorMessage = "Isi Surat Harus Diisi")]
        public string IsiSurat { get; set; }
        public string Penerima { get; set; }
        public string Pengirim { get; set; }
        [Required(ErrorMessage = "Kode Arsip Harus Diisi")]
        public string KodeArsip { get; set; }
        public string Lampiran { get; set; }
        public string UserPembuat { get; set; }
        public string Status { get; set; }
        public string TanggalDibuat { get; set; }
        public string TanggalTTE { get; set; }
        public string PosisiTTE { get; set; }
        public List<UserTTE> TTE { get; set; }
        public string listTTE { get; set; }
        public string NamaFile { get; set; }
        public string Pass { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public HttpPostedFileBase FileDokumen { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string TanggalSetuju { get; set; }
        public string TanggalTolak { get; set; }
        public bool isTU { get; set; }
        public bool GenerateFooter { get; set; }
        public bool isDraft { get; set; }
        public bool isExpired { get; set; }
    }

    public class UserTTE
    {
        public string DokumenElektronikId { get; set; }
        public string DraftCode { get; set; }
        public string PenandatanganId { get; set; }
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string ProfileId { get; set; }
        public string Jabatan { get; set; }
        public int Urut { get; set; }
        public string Tipe { get; set; }
        public string EMeterai { get; set; }
        public string Status { get; set; }
    }

    public class SejarahDokumenTTE
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string tanggal { get; set; }
        public string nama { get; set; }
        public string nip { get; set; }
        public string jabatan { get; set; }
        public string tipe { get; set; }
        public string status { get; set; }
    }

    public class PenandatanganInfo
    {
        public string DokumenElektronikId { get; set; }
        public string profileid { get; set; }
        public string nik { get; set; }
        public string nama { get; set; }
        public string nip { get; set; }
        public string jabatan { get; set; }
        public string passphrase { get; set; }
        public string ttdid { get; set; }
        public string kantorid { get; set; }
        public string kodespopp { get; set; }
        public string dokid { get; set; }
    }

    public class StatusResponse
    {
        public string status_code { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    public class StatusErrorResponse
    {
        public string timestamp { get; set; }
        public int status { get; set; }
        public string error { get; set; }
        public string message { get; set; }
        public string path { get; set; }
    }

    public class TipeSuratTTE
    {
        public string Nama { get; set; }
        public string Induk { get; set; }
        public string FormatNomor { get; set; }
    }

    public class SifatSuratTTE
    {
        public string Nama { get; set; }
        public int Prioritas { get; set; }
    }

    public class CariExpoSertipikat
    {
        public string NamaAcara { get; set; }
        public DateTime TanggalAcara { get; set; }
        public string NamaPeserta { get; set; }
        public string Status { get; set; }
        public string Metadata { get; set; }
    }

    public class BuatExpoSertipikat
    {
        [Required(ErrorMessage = "Nama Acara Harus Diisi")]
        public string NamaAcara { get; set; }
        [Required(ErrorMessage = "Tanggal Acara Harus Diisi")]
        public DateTime TanggalAcara { get; set; }
        [Required(ErrorMessage = "Nama Penandatangan Harus Dipilih")]
        public string UserTTE { get; set; }
        public HttpPostedFileBase[] Files { get; set; }
    }

    public class ExpoSertipikat
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string ExpoSertipikatId { get; set; }

        [Required(ErrorMessage = "Nama Acara Harus Diisi")]
        public string NamaAcara { get; set; }

        [Required(ErrorMessage = "Tanggal Acara Harus Diisi")]
        public DateTime TanggalAcara { get; set; }
        public string NamaPeserta { get; set; }
        public string FileName { get; set; }
        public string UserUnggah { get; set; }
        public string TanggalUnggah { get; set; }
        public string UserTTE { get; set; }
        public string NamaTTE { get; set; }
        public string TanggalTTE { get; set; }
        public string UserHapus { get; set; }
        public string TanggalHapus { get; set; }
        public string Status { get; set; }
    }

    #endregion

    public class DaftarKantor
    {
        public string IdKantor { get; set; }
        public string NamaKantor { get; set; }
        public int RNumber { get; set; }
        public int Total { get; set; }
    }

    public class CariDraftSurat
    {
        public string DraftCode { get; set; }
        public string UnitKerjaId { get; set; }
        public string Tujuan { get; set; }
        public string MetaData { get; set; }
        public string SortBy { get; set; }
        public bool Status { get; set; }
    }

    public class ListDraft
    {
        public string DraftCode { get; set; }
        public string UnitKerjaId { get; set; }
        public string Perihal { get; set; }
        public string SifatSurat { get; set; }
        public string TipeSurat { get; set; }
        public string UserBuat { get; set; }
        public string NamaBuat { get; set; }
        public string UserUbah { get; set; }
        public string NamaUbah { get; set; }
        public string TanggalBuat { get; set; }
        public string TanggalUbah { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string Status { get; set; }
        public string stCheck { get; set; }
        public string stTT { get; set; }
        public string Keterangan { get; set; }
        public string Penandatangan { get; set; }
        public string Notifikasi { get; set; }
        public string LampiranId { get; set; }
        public string PerjalananKonsep { get; set; }
        public string StatusAcc { get; set; }
        public string DokumenElektronikId { get; set; }
        public decimal? Open { get; set; }
        public string KodeKlasifikasi { get; set; }
        public decimal? esfilter { get; set; }
    }

    public class DraftSuratDetail
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class DraftSurat
    {
        public string DraftCode { get; set; }
        public string UnitKerjaId { get; set; }
        [Required(ErrorMessage = "Perihal Surat Harus Diisi")]
        public string Perihal { get; set; }

        [Required(ErrorMessage = "Sifat Surat Harus Diisi")]
        public string SifatSurat { get; set; }

        [Required(ErrorMessage = "Tipe Surat Harus Diisi")]
        public string TipeSurat { get; set; }

        [Required(ErrorMessage = "Isi Surat Harus Diisi")]
        public string IsiSurat { get; set; }
        public string Penerima { get; set; }
        public string Pengirim { get; set; }
        [Required(ErrorMessage = "Kode Arsip Harus Diisi")]
        public string KodeArsip { get; set; }
        public string NamaFileLampiran { get; set; }
        public string EkstensiLampiran { get; set; }
        public HttpPostedFileBase FileLampiran { get; set; }
        public string LampiranId { get; set; }
        public string UserPembuat { get; set; }
        public string NamaPembuat { get; set; }
        public string Status { get; set; }
        public string TanggalDibuat { get; set; }
        public List<SelectListItem> ListKodeKopSurat { get; set; }
        [Required(ErrorMessage = "Kop Surat Harus Dipilih")]
        public string KopSurat { get; set; }
        public string PosisiTTE { get; set; }
        public List<UserTTE> TTE { get; set; }
        public List<DraftSuratDetail> Detail { get; set; }
        public List<string> Tujuan { get; set; }
        public List<string> Tembusan { get; set; }
        public string listTujuan { get; set; }
        public string listTembusan { get; set; }
        public string listTTE { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string TanggalSetuju { get; set; }
        public string TanggalTolak { get; set; }
        public bool isTU { get; set; }
        public string Pass { get; set; }
        public string MetaData { get; set; }
        public List<SelectListItem> ListProfile { get; set; }
        [Required(ErrorMessage = "Pengirim Harus Dipilih")]
        public string ProfilePengirim { get; set; }
        public string TanggalUndangan { get; set; }
        public string TempatUndangan { get; set; }
        public string YangTandaTangan { get; set; }
        public string MenerangkanBahwa { get; set; }
        public string AlamatKeterangan { get; set; }
        public string NamaKeterangan { get; set; }
        public string NoIndukKeterangan { get; set; }
        public string PangkatKeterangan { get; set; }
        public string JabatanKeterangan { get; set; }
        public bool TanpaGelar { get; set; }
        public string AtasNama { get; set; }
        public string JumlahLampiran { get; set; }
        public string NomorSurat { get; set; }
        public string TanggalSurat { get; set; }
        public string JabatanAdhoc { get; set; }
        public bool TujuanTerlampir { get; set; }
        public bool TandaTanganBasah { get; set; }
        public bool isHaveLampiran { get; set; }
        public bool islampiranChange { get; set; }
        public bool AutoNumAvail { get; set; }
        public List<string> lstLampiranId { get; set; }
        public List<string> lstLampiranIdSave { get; set; }
        public List<lampiranDraft> lstFileLampiran { get; set; }
        public List<HttpPostedFileBase> fileUploadStream { get; set; }
        public List<SelectListItem> ListForTTE { get; set; }
    }

    public class lampiranDraft
    {
        public string LampiranId { get; set; }
        public string namaFile { get; set; }
        public bool save { get; set; }
        public byte[] ObjectFile { get; set; }
    }

    public class KoordinasiDraft
    {
        public string Kor_Id { get; set; }
        public string DraftCode { get; set; }
        public string Verifikator { get; set; }
        public string UserId { get; set; }
        public string Nama { get; set; }
        public string Rank { get; set; }
        public string Max { get; set; }
        public string StatusKoordinasi { get; set; }
        public string Tanggal { get; set; }
    }

    public class UserKoordinasiDraft
    {
        public string DokumenElektronikId { get; set; }
        public string DraftCode { get; set; }
        public string PenandatanganId { get; set; }
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string ProfileId { get; set; }
        public string Jabatan { get; set; }
        public int Urut { get; set; }
        public string Tipe { get; set; }
        public string Status { get; set; }
    }

    public class KoordinasiHist
    {
        public string DraftCode { get; set; }
        public string Kor_Id { get; set; }
        public string KorHist_Id { get; set; }
        public string Pesan { get; set; }
        public string PsFrom { get; set; }
        public string PsFromNama { get; set; }
        public string Tanggal { get; set; }
        public int isUser { get; set; }
    }


    public class KopSurat
    {
        public string UnitKerjaId { get; set; }
        public string UnitKerjaName { get; set; }
        public string NamaKantor_L1 { get; set; }
        public string NamaKantor_L2 { get; set; }
        public string Alamat { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public int FontSize { get; set; }
        public int RNumber { get; set; }
        public int Total { get; set; }
    }

    public class Petugas
    {
        public string value { get; set; }
        public string data { get; set; }
    }

    public class TujuanSurat
    {
        public string ProfileId { get; set; }
        public string NIP { get; set; }
        public string Redaksi { get; set; }
    }

    public class Files
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string FilesId { get; set; }
        public string Tipe { get; set; }
        public string PengenalFile { get; set; }
        public string TanggalDibuat { get; set; }
        public string Keterangan { get; set; }
        public string SifatFile { get; set; }
        public string UnitKerjaId { get; set; }
        public string KantorId { get; set; }
        public string Path { get; set; }
        public byte[] ObjectFile { get; set; }
    }


    public class JumlahSurat
    {
        public string Tipe { get; set; }
        public int Jumlah { get; set; }
    }

    public class ComboList
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class ListSuratPending
    {
        public int RNumber { get; set; }
        public int Total { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public int Jumlah { get; set; }
    }

    public class DiffWaktu
    {
        public int Tahun { get; set; }
        public int Bulan { get; set; }
        public int Hari { get; set; }
        public int Jam { get; set; }
        public int Menit { get; set; }
    }

    #region naskahdinas

    public class PegawaiDetail
    {
        public string PegawaiId { get; set; }
        public string Nama { get; set; }
        public string ProfileId { get; set; }
        public string NamaJabatan { get; set; }
        public string Eselon { get; set; }
        public string Pangkat { get; set; }
        public string Golongan { get; set; }
        
    }

    #endregion

    public class DetailAkun
    {
        public string TipeAkun { get; set; }
        public string PegawaiId { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string Email { get; set; }
        public string NomorHP { get; set; }
        public string Eselon { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Satker { get; set; }
        public bool IsActive { get; set; }
        public List<Kantor> KantorIds { get; set; }
        public string PesanError { get; set; }
        public string Status { get; set; }
    }

    public class CatatanSurat
    {
        public DateTime TanggalKirim { get; set; }
        public string PegawaiId { get; set; }
        public string NamaLengkap { get; set; }
        public string Keterangan { get; set; }
    }

    public class BukaDokumen
    {
        public string NomorSurat { get; set; }
        public string DokumenId { get; set; }
        public string Kode { get; set; }
        public string MetaData { get; set; }
    }

    public class LinkSurat
    {
        public int RNumber { get; set; }
        public int Total { get; set; }
        public string SuratId { get; set; }
        public string DokumenId { get; set; }
        public string KontenAktifId { get; set; }
        public string DokumenTipe { get; set; }
        public string NomorDokumen { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Note { get; set; }
    }

    public class DokumenSurat
    {
        public int RNumber { get; set; }
        public int Total { get; set; }
        public string DokumenId { get; set; }
        public string Nomor { get; set; }
        public string Wilayah { get; set; }
        public string StatusHapus { get; set; }
        public string Tipe { get; set; }
        public DateTime? ValidSejak { get; set; }
        public string TglValidSejak
        {
            get { return ValidSejak == null ? "" : ((DateTime)ValidSejak).ToString("yyyy-MM-dd"); }
            set { ValidSejak = Convert.ToDateTime(value); }
        }
        public DateTime? ValidSampai { get; set; }
        public string TglValidSampai
        {
            get { return ValidSampai == null ? "" : ((DateTime)ValidSampai).ToString("yyyy-MM-dd"); }
            set { ValidSampai = Convert.ToDateTime(value); }
        }
        public string KontenAktifId { get; set; }
        public string KantorId { get; set; }
    }

    public class CariRekapPresensi
    {
        [Display(Name = "Pegawai")]
        public string PegawaiId { get; set; }
        [Display(Name = "Unit Kerja")]
        public string UnitKerjaId { get; set; }
        [Display(Name = "Periode")]
        public DateTime? TanggalMulai { get; set; }
        public DateTime? TanggalSampai { get; set; }
        [Display(Name = "Tipe Pegawai")]
        public string TipePegawai { get; set; }
        public List<SelectListItem> listUnitKerja { get; set; }
        public List<SelectListItem> listPegawai { get; set; }
        public List<SelectListItem> listTipePegawai { get; set; }
    }

    public class RekapPresensi
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string UnitKerjaId { get; set; }
        public string NamaUnitKerja { get; set; }
        public DateTime Period { get; set; }
        public string strPeriod
        {
            get { return Period.ToString("yyyy-MM-dd"); }
            set { Period = Convert.ToDateTime(value); }
        }
        public string Masuk { get; set; }
        public string Keluar { get; set; }
        public string Masuk_Status { get; set; }
        public string Keluar_Status { get; set; }
        public string Masuk_Lokasi { get; set; }
        public string Keluar_Lokasi { get; set; }
        public string Masuk_Catatan { get; set; }
        public string Keluar_Catatan { get; set; }
        public string Jam_Kerja { get; set; }
        public string Sumber { get; set; }
    }

    public class GeoLocation
    {
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Catatan { get; set; }
    }


    #region kuisioner
    public class Pertanyaan
    {
        public string NamaPegawai { get; set; }
        public string Pertanyaan_Id { get; set; }
        public string Nama_Pertanyaan { get; set; }
        public string Tipe { get; set; }
        public string Status { get; set; }
        public string StatusHapus { get; set; }
        public string Tanggal { get; set; }
        public string UserId { get; set; }
        public string Pilihan { get; set; }
        public List<KuisionerJawaban> ListJawaban { get; set; }
        public List<KuisionerJawabanall> ListJawabanall { get; set; }
    }

    public class Pertanyaanall
    {
        public string NamaPegawai { get; set; }
        public string Pertanyaan_Id { get; set; }
        public string Nama_Pertanyaan { get; set; }
        public string Tipe { get; set; }
        public string Status { get; set; }
        public string StatusHapus { get; set; }
        public string Tanggal { get; set; }
        public string UserId { get; set; }
        public string Pilihan { get; set; }
        public List<KuisionerJawaban> ListJawaban { get; set; }
        public List<KuisionerJawaban> ListJawabanall { get; set; }
    }

    public class Kuisioner
    {
        public string UserId { get; set; }
        public List<Pertanyaan> ListPertanyaan { get; set; }
        public int Ct { get; set; }
    }

    public class KuisionerReport
    {
        public string Nama_Lengkap { get; set; }
        public string UserId { get; set; }
        public string JawabanId { get; set; }
        public List<Pertanyaan> ListPertanyaan { get; set; }
        public List<Pertanyaan> ListPertanyaanTerakhir { get; set; }
        public List<Pertanyaanall> ListPertanyaanall { get; set; }
        public int Ct { get; set; }
        public int Ctall { get; set; }
        public List<KuisionerJawaban> ListJawaban { get; internal set; }
        public string Judul { get; set; }
    }

    public class KuisionerJawaban
    {
        public string Nama_Lengkap { get; set; }
        public string PegawaiID { get; set; }
        public string UserId { get; set; }
        public string Pertanyaan_Id { get; set; }
        public string Nama_Jawaban { get; set; }
        public string Nama_Pertanyaan { get; set; }
        public string Tipe { get; set; }
        public string Jawaban_ID { get; set; }
        public string UserIDHapus { get; set; }
        public string TanggalHapus { get; set; }
        public string StatusHapus { get; set; }
        public int jml_jawaban { get; set; }
        public int jml_jawaban_all { get; set; }
        public int jumlah_bar_all { get; set; }
        public int nilai { get; set; }
        public List<KuisionerJawaban> ListKuisionerJawaban1 { get; set; }
        public List<KuisionerJawaban> ListKuisionerJawaban2 { get; set; }
    }
    public class KuisionerJawabanall
    {
        public string Nama_Lengkap { get; set; }
        public string UserIdHapus { get; set; }
        public string PegawaiID { get; set; }
        public string UserId { get; set; }
        public string Pertanyaan_Id { get; set; }
        public string Nama_Jawaban { get; set; }
        public string Nama_Pertanyaan { get; set; }
        public string Tipe { get; set; }
        public int jml_jawaban { get; set; }
        public int jml_jawaban_all { get; set; }
        public int Nilai { get; set; }
    }

    public class ReportIndividu
    {
        public string JawabanId { get; set; }
        public string UserId { get; set; }
        public string UserIdHapus { get; set; }
        public string Pertanyaan_Id { get; set; }
        public string Nama_Jawaban { get; set; }
        public int jml_jawaban { get; set; }
        public string Nama_Pertanyaan { get; set; } 
        public string Tipe { get; set; } 
        public List<Pertanyaan> ListPertanyaan { get; set; }
        public int Ct { get; set; }
    }

    #endregion

    public class DataBukuTamu
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string TamuId { get; set; }
        [Display(Name = "Nomor Induk Kependudukan")]
        [StringLength(20, ErrorMessage = "Maks Panjang Karakter NIK adalah 20.")]
        [Required(ErrorMessage = "Nomor Induk Kependudukan Wajib Diisi.")]
        public string NIK { get; set; }
        [Display(Name = "Nama Lengkap (Sesuai KTP)")]
        [StringLength(50, ErrorMessage = "Maks Panjang Karakter Nama Lengkap adalah 50.")]
        [Required(ErrorMessage = "Nama Lengkap Wajib Diisi.")]
        public string NamaLengkap { get; set; }
        [Display(Name = "Tempat Lahir (Sesuai KTP)")]
        [StringLength(100, ErrorMessage = "Maks Panjang Karakter Tempat Lahir adalah 100.")]
        [Required(ErrorMessage = "Tempat Lahir Wajib Diisi.")]
        public string TempatLahir { get; set; }
        [Display(Name = "Tanggal Lahir (Sesuai KTP)")]
        [StringLength(10, ErrorMessage = "Maks Tanggal Lahir adalah 10.")]
        [Required(ErrorMessage = "Tanggal Lahir Wajib Diisi.")]
        public string TanggalLahir { get; set; }
        [Display(Name = "Nomor Telepon (yang dapat dihubungi)")]
        [StringLength(20, ErrorMessage = "Maks Panjang Karakter Nomor Telepon adalah 20.")]
        [Required(ErrorMessage = "Nomor Telepon Wajib Diisi.")]
        public string NoTelp { get; set; }
        [Display(Name = "Alamat (Sesuai KTP)")]
        [StringLength(380, ErrorMessage = "Maks Panjang Karakter Alamat adalah 380.")]
        [Required(ErrorMessage = "Alamat Wajib Diisi.")]
        public string Alamat { get; set; }
        [Display(Name = "Email")]
        [StringLength(100, ErrorMessage = "Maks Panjang Karakter Email adalah 100.")]
        [Required(ErrorMessage = "Email Wajib Diisi.")]
        [EmailAddress(ErrorMessage = "Format Email Salah")]
        public string Email { get; set; }
        [Display(Name = "Instansi")]
        [StringLength(200, ErrorMessage = "Maks Panjang Karakter Nama Instansi adalah 200.")]
        [Required(ErrorMessage = "Instansi Wajib Diisi.")]
        public string Instansi { get; set; }
        [Display(Name = "Keperluan")]
        [StringLength(1970, ErrorMessage = "Maks Panjang Karakter Keperluan adalah 1970.")]
        [Required(ErrorMessage = "Keperluan Wajib Diisi.")]
        public string Keperluan { get; set; }
        public string UserId { get; set; }
        public string KantorId { get; set; }
        public string UnitKerjaId { get; set; }
        public string NamaUnitKerja { get; set; }
        public string LinkPeduliLindungi { get; set; }
        public DateTime TanggalBerkunjung { get; set; }
        public string Waktu_Kedatangan
        {
            get { return TanggalBerkunjung.ToString("yyyy-MM-dd HH:mm").Replace(".",":"); }
            set { TanggalBerkunjung = Convert.ToDateTime(value); }
        }
        public string ResponStatus { get; set; }
        public string StatusTamu
        {
            get { return string.IsNullOrEmpty(ResponStatus)? "Menunggu": ResponStatus.Equals("1")? "Diterima": "Ditolak"; }
            set { ResponStatus = value; }
        }
        public string ResponCatatan { get; set; }
        public string ResponUserId { get; set; }
        public string StatusDukcapil { get; set; }
        //public string ResponPegawaiId
        //{
        //    get { return string.IsNullOrEmpty(ResponUserId) ? "" : ResponStatus.Equals("1") ? "Diterima" : "Ditolak"; }
        //    set { ResponStatus = value; }
        //}
    }

    public class CariBukuTamu
    {
        public string Metadata { get; set; }
        [Display(Name = "Unit Kerja")]
        public string UnitKerjaId { get; set; }
        [Display(Name = "Periode")]
        public DateTime? TanggalMulai { get; set; }
        public DateTime? TanggalSampai { get; set; }
        [Display(Name = "Tipe Pegawai")]
        public string TipePegawai { get; set; }
        public List<SelectListItem> listUnitKerja { get; set; }
    }

    public class ParameterDukcapil
    {
        public string NIK { get; set; }
        public string NAMA_LGKP { get; set; }
        public string TMPT_LHR { get; set; }
        public string TGL_LHR { get; set; }
        public string USER_ID { get; set; }
    }

    public class jumlahKotakSurat
    {
        public decimal? Email { get; set; }
        public decimal? Loket { get; set; }
        public decimal? Internal { get; set; }
    }

}