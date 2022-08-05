using Surat.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace System.Web.Mvc
{
    public class OtorisasiUser
    {
        static Functions functions = new Functions();
        public static string NamaSkema
        {
            get
            {
                string namaSkema = System.Configuration.ConfigurationManager.AppSettings["NamaSkema"].ToString();

                return namaSkema;
            }
        }

        public static string NamaSkemaMaster
        {
            get
            {
                string namaSkema = System.Configuration.ConfigurationManager.AppSettings["NamaSkemaMaster"].ToString();

                return namaSkema;
            }
        }

        public static string NamaSkemaLogin
        {
            get
            {
                string namaSkema = System.Configuration.ConfigurationManager.AppSettings["NamaSkemaLogin"].ToString();

                return namaSkema;
            }
        }

        public static string ApiKeyGoogle
        {
            get
            {
                string key = System.Configuration.ConfigurationManager.AppSettings["ApiKeyGoogle"].ToString();

                return key;
            }
        }

        public static string GetJenisKantorUser()
        {
            Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

            string CurrentUserRole = "Kanwil";

            try
            {
                var usr = functions.claimUser();
                string kantorid = usr.KantorId;

                int tipekantor = model.GetTipeKantor(kantorid);

                if (tipekantor == 1)
                {
                    CurrentUserRole = "Pusat";
                }
                else if (tipekantor == 2)
                {
                    CurrentUserRole = "Kanwil";
                }
                else if (tipekantor == 3 || tipekantor == 4)
                {
                    CurrentUserRole = "Kantah";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrentUserRole;
        }

        public static bool IsAdminRole()
        {
            bool CurrentAdminRole = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string profileid = "'A80100','A80300','A80400','A80500','B80100'";

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                CurrentAdminRole = model.CheckUserProfile(pegawaiid, kantorid, profileid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool isTU()
        {
            bool CurrentAdminRole = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                CurrentAdminRole = model.CheckIsTU(pegawaiid, kantorid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool PembuatDokumenElektronik()
        {
            bool rst = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                rst = model.CheckIsTU(pegawaiid, kantorid);
                if (!rst)
                {
                    rst = model.CheckUserProfile(pegawaiid, kantorid, "'A81004'");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return rst;
        }

        public static bool IsRoleAdministrator()
        {
            bool CurrentAdminRole = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string profileid = "'A80100'";

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                CurrentAdminRole = model.CheckUserProfile(pegawaiid, kantorid, profileid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool IsProfile(string profile)
        {
            bool result = false;

            try
            {
                var usr = functions.claimUser();
                string profileid = string.Empty;
                switch (profile)
                {
                    case "Administrator":
                        profileid = "'A80100'";
                        break;
                    case "AdminSatker":
                        profileid = "'A80300'";
                        break;
                    case "AdminSPBE":
                        profileid = "'A80200'";
                        break;
                    case "PembuatSuratMasuk":
                        profileid = "'A81001'";
                        break;
                    case "PembuatNomorAgenda":
                        profileid = "'A81002'";
                        break;
                    case "PembuatNomorSurat":
                        profileid = "'A81003'";
                        break;
                    case "PembuatSuratElektronik":
                        profileid = "'A81004'";
                        break;
                }
                if (string.IsNullOrEmpty(profileid))
                {
                    return false;
                }
                else
                {
                    Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                    result = model.CheckUserProfile(usr.PegawaiId, usr.KantorId, profileid);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }

        public static bool IsPembuatNomorAgendaRole()
        {
            bool CurrentAdminRole = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string profileid = "'A81002'"; // profile Pembuat Nomor Agenda

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                CurrentAdminRole = model.CheckUserProfile(pegawaiid, kantorid, profileid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool IsPembuatNomorSuratRole()
        {
            bool CurrentAdminRole = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string profileid = "'A81003'"; // profile Pembuat Nomor Surat

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                CurrentAdminRole = model.CheckUserProfile(pegawaiid, kantorid, profileid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool IsActiveRole(string profileid)
        {
            bool CurrentAdminRole = false;

            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                CurrentAdminRole = model.CheckUserProfile(pegawaiid, kantorid, profileid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool GantiPassword()
        {
            bool ganti = false;

            try
            {
                var usr = functions.claimUser();
                string userid = usr.UserId;
                string pegawaiid = usr.PegawaiId;

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                ganti = model.CheckGantiPassword(userid, pegawaiid, 60);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
            return ganti;
        }
        public static bool NotifGantiPassword()
        {
            bool ganti = false;

            try
            {
                var usr = functions.claimUser();
                string userid = usr.UserId;
                string pegawaiid = usr.PegawaiId;

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                ganti = model.CheckGantiPassword(userid, pegawaiid, 53);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
            return ganti;
        }
        public static bool isTrainBSSN
        {
            get
            {
                bool ret = System.Configuration.ConfigurationManager.AppSettings["DigitalSignatureUrl"].ToString().Equals("http://10.20.22.101/");

                return ret;
            }
        }

        public static bool isFungsional(string pJabatan)
        {
            bool result = false;

            try
            {
                result = (pJabatan.ToLower().Contains("analis") || pJabatan.ToLower().Contains("petugas") || pJabatan.ToLower().Contains("pengelola") || pJabatan.ToLower().Contains("pengadministrasi") || ((pJabatan.ToLower().Contains("penata ") || pJabatan.ToLower().Contains("widyaiswara ") || pJabatan.ToLower().Contains("assessor ")) && (pJabatan.ToLower().Contains(" muda") || pJabatan.ToLower().Contains(" pertama") || pJabatan.ToLower().Contains(" utama") || pJabatan.ToLower().Contains(" madya"))) || pJabatan.ToLower().Equals("tenaga ahli"));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public static int getPengajuanAkses()
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                ct = model.GetCountPengajuanAkses(usr.PegawaiId, usr.UnitKerjaId);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        public static int getCountSurat(string tipe)
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();
                string satkerid = usr.KantorId;
                int tipekantorid = new Surat.Models.DataMasterModel().GetTipeKantor(usr.KantorId);
                if (tipekantorid == 1)
                {
                    satkerid = usr.UnitKerjaId;
                }
                string myProfiles = new Surat.Codes.Functions().MyProfiles(usr.PegawaiId, usr.KantorId);

                Surat.Models.PersuratanModel model = new Surat.Models.PersuratanModel();

                ct = model.JumlahSurat(satkerid, usr.PegawaiId, myProfiles, tipe);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        public static int getCountProsesSurat()
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();
                string satkerid = usr.KantorId;
                int tipekantorid = new Surat.Models.DataMasterModel().GetTipeKantor(usr.KantorId);
                if (tipekantorid == 1)
                {
                    satkerid = usr.UnitKerjaId;
                }
                string myProfiles = new Surat.Codes.Functions().MyProfiles(usr.PegawaiId, usr.KantorId);

                Surat.Models.PersuratanModel model = new Surat.Models.PersuratanModel();

                if (new Surat.Models.DataMasterModel().GetIsMyProfileTU(usr.PegawaiId).Equals("1"))
                {
                    ct = model.JumlahProsesSuratV2(new Surat.Models.DataMasterModel().getUserProfileTU(usr.PegawaiId, usr.UnitKerjaId), satkerid);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        public static int getCountProsesTTE()
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();
                var model = new Surat.Models.TandaTanganElektronikModel();

                ct = model.JumlahProsesDokumen(usr.UserId);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        public static int getCountPengaduan()
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();
                string myProfiles = new Surat.Codes.Functions().MyProfiles(usr.PegawaiId, usr.KantorId);
                var model = new Surat.Models.PengaduanModel();

                ct = model.JumlahPengaduan(usr.UnitKerjaId, usr.PegawaiId, myProfiles);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        public static int getCountRapatOnline()
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();
                var model = new Surat.Models.MeetingModel();

                ct = model.JumlahRapatOnlineSaya(usr.UnitKerjaId, usr.PegawaiId);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        public static int getPengajuanJabatan()
        {
            int ct = 0;

            try
            {
                var usr = functions.claimUser();

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                ct = model.GetCountPengajuanJabatan(usr.PegawaiId, usr.UnitKerjaId);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }
            return ct;
        }

        /////////////////////Arsip dan Persuratan////////////////

        public static bool IsProfileAdminKearsipan()
        {
            bool result = false;
            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string unitkerjaid = usr.UnitKerjaId;

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                result = model.CheckUserAdminKearsipan(pegawaiid, kantorid, unitkerjaid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }
        public static bool IsProfileAdminPersuratan()
        {
            bool result = false;
            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string unitkerjaid = usr.UnitKerjaId;


                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                result = model.CheckUserAdminPersuratan(pegawaiid, kantorid, unitkerjaid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }
        public static bool IsProfileEntriPersuratan()
        {
            bool result = false;
            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string unitkerjaid = usr.UnitKerjaId;


                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                result = model.CheckUserEntriPersuratan(pegawaiid, kantorid, unitkerjaid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }
        public static bool IsProfileEntriKearsipan()
        {
            bool result = false;
            try
            {
                var usr = functions.claimUser();
                string pegawaiid = usr.PegawaiId;
                string kantorid = usr.KantorId;
                string unitkerjaid = usr.UnitKerjaId;


                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();

                result = model.CheckUserEntriKearsipan(pegawaiid, kantorid, unitkerjaid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }
        public static bool IsPengisianKuisioner()
        {
            bool result = false;
            try
            {
                var usr = functions.claimUser();

                Surat.Models.DataMasterModel model = new Surat.Models.DataMasterModel();
                bool isEsellon3 = new Surat.Models.KuisionerModel().GetNotifKuisioner(usr);
                if (isEsellon3)
                {
                    result = false;
                }
                else
                {
                    string UserId = usr.UserId;

                    var check = new Surat.Models.KuisionerModel().GetJawabanIndividu(UserId);
                    if (check.Count > 0)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static string UrlVideoPanduanRakernas
        {
            get
            {
                string key = System.Configuration.ConfigurationManager.AppSettings["UrlVideoRakernas"] != null? System.Configuration.ConfigurationManager.AppSettings["UrlVideoRakernas"].ToString(): "";

                return key;
            }
        }

        public static string UrlPanduanRakernas
        {
            get
            {
                string key = System.Configuration.ConfigurationManager.AppSettings["UrlPanduanRakernas"] != null ? System.Configuration.ConfigurationManager.AppSettings["UrlPanduanRakernas"].ToString() : "";

                return key;
            }
        }
    }
}