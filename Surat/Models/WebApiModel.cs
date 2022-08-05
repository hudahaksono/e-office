using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;

namespace Surat.Models
{
    public class WebApiModel
    {
        public Models.Entities.WebApiUser GetWebApiUser(string username, string password)
        {
            var data = new WebApiUser();

            var list = new List<Models.Entities.WebApiUser>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT DISTINCT " +
                "    u.userid AS id_pengguna, u.username AS nama_pengguna, u.email, " +
                "    p.pegawaiid AS id_pegawai, p.nama namapegawai, j.nama jabatan, " + 
                "    k.kantorid, k.nama namakantor, " +
                "    ko.latitude AS latitudekantor, ko.longitude AS longitudekantor, ko.zonawaktu " +
                "FROM " +
                "    users u, pegawai p, jabatanpegawai pp, jabatan j, kantor k, UNITKERJA uk, lppb.kantoreoffice ko " +
                "WHERE " +
                "    (pp.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(pp.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND NVL(pp.STATUSHAPUS,'0') = '0' " +
                "    AND (p.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(p.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) " +
                "    AND u.userid = p.userid " +
                "    AND p.pegawaiid = pp.pegawaiid " +
                "    AND pp.profileid = j.profileid " +
                "    AND j.UNITKERJAID = uk.UNITKERJAID " +
                "    AND uk.KANTORID = k.kantorid " +
                "    AND k.kantorid = ko.kantorid " +
                "    AND u.username = :Username " +
                "ORDER BY k.nama";
            query = @"
                SELECT DISTINCT
                  U.USERID AS ID_PENGGUNA,
                  'ASN' AS TIPE_USER,
                  U.USERNAME AS NAMA_PENGGUNA,
                  U.EMAIL,
                  P.PEGAWAIID AS ID_PEGAWAI,
                  P.NAMA AS NAMAPEGAWAI,
                  'https://simpeg.atrbpn.go.id/app/client/bpn/uploads/siap/foto/' || VP.FOTO AS FOTO_PROFIL,
                  NVL(JB.NAMA,VP.NAMAJABATAN) AS JABATAN,
                  JB.UNITKERJAID,
                  NVL(KV.KODE,KJ.KODE) AS KODE,
                  NVL(KV.KANTORID,KJ.KANTORID) AS KANTORID,
                  NVL(KV.NAMA,KJ.NAMA) AS NAMAKANTOR,
                  REPLACE(CAST(NVL(KV.LATITUDE,KJ.LATITUDE) AS VARCHAR2(20)),',','.') AS LATITUDEKANTOR,
                  REPLACE(CAST(NVL(KV.LONGITUDE,KJ.LONGITUDE) AS VARCHAR2(20)),',','.') AS LONGITUDEKANTOR,
                  DECODE(LOWER(NVL(KV.BAGIANWILAYAH,KJ.BAGIANWILAYAH)),'timur','WIT','tengah','WITA','WIB') AS ZONAWAKTU
                FROM USERS U
                  INNER JOIN PEGAWAI P ON
                    (p.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(p.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
                    p.USERID = u.USERID
                  INNER JOIN JABATANPEGAWAI PP ON
                    (PP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(PP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
                    PP.PEGAWAIID = P.PEGAWAIID AND
                    NVL(PP.STATUSHAPUS,'0') = '0'
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = PP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JB.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
                  INNER JOIN KANTOR KJ ON
                    KJ.KANTORID = PP.KANTORID
                  LEFT JOIN simpeg_2702.v_pegawai_eoffice VP ON
                    VP.NIPBARU = P.PEGAWAIID
                  LEFT JOIN KANTOR KV ON
                    KV.KODESATKER = VP.KODESATKER
                WHERE
                  U.USERNAME = :Username AND U.PASSWORD = :Password AND U.ISLOCKEDOUT = 0 AND
                  (U.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(U.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
                ORDER BY NVL(KV.KODE,KJ.KODE)"; // Arya : 2020-10-13

            arrayListParameters.Add(new OracleParameter("Username", username));
            arrayListParameters.Add(new OracleParameter("Password", password));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                list = ctx.Database.SqlQuery<Models.Entities.WebApiUser>(query, parameters).ToList<Models.Entities.WebApiUser>();

                if (list.Count > 0)
                {
                    data = list[0];
                    data.anakKantors = this.GetAnakKantors(data.kantorid);
                    //if (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM LPPB.KANTORABSENPUSAT WHERE KANTOREOFFICEID = '{0}' AND NVL(STATUSHAPUS,'0') = '0'", data.kantorid)).FirstOrDefault() > 0)
                    //{
                    //    List<Models.Entities.AnakKantor> anakKantors = this.GetAnakKantors();
                    //    data.anakKantors = anakKantors;
                    //}
                }
                else
                {
                    ArrayList ArrayList = new ArrayList();

                    // cek ke data PPNPN
                    string sql =
                        "SELECT DISTINCT u.userid AS id_pengguna, u.username AS nama_pengguna, u.email ,p.NIK AS id_pegawai " +
                        ", p.NAMA AS namapegawai , j.nama AS jabatan ,k.kantorid, k.nama namakantor " +
                        ", 'https://mitra.atrbpn.go.id/ppnpn/' || p.URLPROFILE AS foto, " +
                        "    ko.latitude AS latitudekantor, ko.longitude AS longitudekantor, ko.zonawaktu " +
                        "FROM PPNPN, PPNPN.USERS u , PPNPN.PPNPN p, jabatanpegawai pp, jabatan j , kantor k, lppb.kantoreoffice ko " +
                        "WHERE u.userid = p.userid " +
                        "    AND p.nik = pp.pegawaiid " +
                        "    AND j.PROFILEID= pp.PROFILEID " +
                        "    AND k.KANTORID = pp.KANTORID " +
                        "    AND k.kantorid = ko.kantorid " +
                        "    AND u.username = :Username ";
                    sql = @"
                        SELECT DISTINCT
                          U.USERID AS ID_PENGGUNA,
                          'PPNPN' AS TIPE_USER,
                          U.USERNAME AS NAMA_PENGGUNA,
                          U.EMAIL,
                          P.NIK AS ID_PEGAWAI,
                          P.NAMA AS NAMAPEGAWAI,
                          'https://mitra.atrbpn.go.id/ppnpn/' || P.URLPROFILE AS FOTO_PROFIL,
                          JB.NAMA AS JABATAN,
                          JB.UNITKERJAID,
                          KT.KANTORID,
                          KT.NAMA AS NAMAKANTOR,
                          REPLACE(CAST(KT.LATITUDE AS VARCHAR2(20)),',','.') AS LATITUDEKANTOR,
                          REPLACE(CAST(KT.LONGITUDE AS VARCHAR2(20)),',','.') AS LONGITUDEKANTOR,
                          DECODE(LOWER(KT.BAGIANWILAYAH),'timur','WIT','tengah','WITA','WIB') AS ZONAWAKTU
                        FROM PPNPN P
                          INNER JOIN PPNPN.USERS U ON
                            U.USERID = p.userid AND
                            (U.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(U.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
                            U.USERNAME = :Username AND U.PASSWORD = :Password AND U.ISLOCKEDOUT = 0
                          INNER JOIN JABATANPEGAWAI PP ON
                            PP.PEGAWAIID = P.NIK AND
                            (PP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(PP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
                            NVL(PP.STATUSHAPUS,'0') = '0'
                          INNER JOIN JABATAN JB ON
                            JB.PROFILEID = PP.PROFILEID AND
                            NVL(JB.SEKSIID,'X') <> 'A800' AND
                            (JB.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JB.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
                          INNER JOIN KANTOR KT ON
                            KT.KANTORID = PP.KANTORID";
                    
                    ArrayList.Add(new OracleParameter("Username", username));
                    ArrayList.Add(new OracleParameter("Password", password));

                    object[] parameterss = ArrayList.OfType<object>().ToArray();
                    list.Clear();
                    list = ctx.Database.SqlQuery<Models.Entities.WebApiUser>(sql, parameterss).ToList<Models.Entities.WebApiUser>();
                    if (list.Count > 0)
                    {
                        data = list[0];
                        data.anakKantors = this.GetAnakKantors(data.kantorid);
                        //if (data.kantorid == "980FECFC746D8C80E0400B0A9214067D")
                        //{
                        //    List<Models.Entities.AnakKantor> anakKantors = this.GetAnakKantors();
                        //    data.anakKantors = anakKantors;
                        //}
                        return data;
                    }
                    else
                    {
                        data = new WebApiUser();
                    }
                }
            }

            return data;
        }

        public List<Models.Entities.AnakKantor> GetAnakKantors(string kantorid)
        {
            var list = new List<Models.Entities.AnakKantor>();

            string query =
                "SELECT a.KANTORPUSATID AS id_anakkantor, " +
                "a.NAMA AS anakkantor,a.LATITUDE AS anakkantorlat, " +
                "a.LONGITUDE AS anakkantorlong,a.ZONAWAKTU AS anakkantorzonawaktu " +
                "FROM LPPB.KANTORABSENPUSAT a " +
                "WHERE a.KANTOREOFFICEID ='" + kantorid+ "' ORDER BY a.KODE";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.AnakKantor>(query).ToList<Models.Entities.AnakKantor>();
            }

            return list;
        }
        public Models.Entities.RekapSurat rekapsurat;

        public Models.Entities.DataSurat GetDataSurat(string username)
        {
            Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
            Functions functions = new Functions();

            string unitkerjaid = "";
            //string profileidtu = "";
            string kantorid = "";

            Models.Entities.DataSurat data = new Models.Entities.DataSurat();

            List<Models.Entities.DataSurat> list = new List<Models.Entities.DataSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT DISTINCT " +
                "    u.userid AS id_pengguna, u.username AS nama_pengguna, u.email, " +
                "    p.pegawaiid AS id_pegawai, p.nama namapegawai, j.nama jabatan, " +
                "    k.kantorid, k.nama namakantor, 'https://simpeg.atrbpn.go.id/app/client/bpn/uploads/siap/foto/' || psimpeg.foto foto_profil " +
                "FROM " +
                "    users u, pegawai p, jabatanpegawai pp, jabatan j, kantor k, " + 
                "    simpeg_2702.pegawai psimpeg " +
                "WHERE " +
                "    (pp.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(pp.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND NVL(pp.STATUSHAPUS,'0') = '0' " +
                "    AND (p.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(p.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) " +
                "    AND u.userid = p.userid " +
                "    AND p.pegawaiid = pp.pegawaiid " +
                "    AND pp.profileid = j.profileid " +
                "    AND pp.kantorid = k.kantorid " +
                "    AND NVL(psimpeg.nipbaru,psimpeg.nip) = p.pegawaiid " +
                "    AND u.username = :Username " +
                "ORDER BY k.nama";

            arrayListParameters.Add(new OracleParameter("Username", username));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                list = ctx.Database.SqlQuery<Models.Entities.DataSurat>(query, parameters).ToList<Models.Entities.DataSurat>();

                if (list.Count > 0)
                {
                    data = list[0];

                    kantorid = data.kantorid;

                    //Models.Entities.ProfileTU profiletu = dataMasterModel.GetProfileTUByNip(data.id_pegawai, kantorid);
                    //if (profiletu != null)
                    //{
                    //    profileidtu = profiletu.ProfileIdTU;
                    //}
                    unitkerjaid = dataMasterModel.GetUnitKerjaIdByNip(data.id_pegawai, kantorid);
                    string satkerid = kantorid;
                    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                    if (tipekantorid == 1)
                    {
                        //satkerid = profileidtu;
                        satkerid = unitkerjaid;
                    }

                    this.rekapsurat = persuratanmodel.GetRekapSuratPegawai(satkerid, data.id_pegawai);

                }
                else
                {
                    ArrayList arrayListParameterss = new ArrayList();
                    string sql =
                            "SELECT DISTINCT u.userid AS id_pengguna, u.username AS nama_pengguna, u.email ,p.NIK AS id_pegawai " +
                            ", p.NAMA AS namapegawai , j.nama AS jabatan ,k.kantorid, k.nama namakantor " +
                            ",'https://mitra.atrbpn.go.id/ppnpn/' || p.URLPROFILE AS foto_profil " +
                            "FROM PPNPN, PPNPN.USERS u , PPNPN.PPNPN p, jabatanpegawai pp, jabatan j , kantor k " +
                            "WHERE u.userid = p.userid " +
                            "AND p.nik = pp.pegawaiid " +
                            "AND j.PROFILEID= pp.PROFILEID " +
                            "AND k.KANTORID = pp.KANTORID " +
                            "AND u.username = :Username ";

                    arrayListParameterss.Add(new OracleParameter("Username", username));

                    object[] parameterss = arrayListParameterss.OfType<object>().ToArray();
                    list.Clear();
                    list = ctx.Database.SqlQuery<Models.Entities.DataSurat>(sql, parameterss).ToList<Models.Entities.DataSurat>();

                    if (list.Count > 0)
                    {
                        data = list[0];
                        kantorid = data.kantorid;

                        //Models.Entities.ProfileTU profiletu = dataMasterModel.GetProfileTUByNip(data.id_pegawai, kantorid);
                        //if (profiletu != null)
                        //{
                        //    profileidtu = profiletu.ProfileIdTU;
                        //}
                        unitkerjaid = dataMasterModel.GetUnitKerjaIdByNip(data.id_pegawai, kantorid);
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = profileidtu;
                            satkerid = unitkerjaid;
                        }

                        this.rekapsurat = persuratanmodel.GetRekapSuratPPNPN(satkerid, data.id_pegawai);
                    }
                    else
                    {

                    }
                }

                // Ambil data surat
                if (list.Count > 0)
                {
                    data.jumlah_surat = "";
                    data.jumlah_inbox = "";
                    data.jumlah_terkirim = "";
                    data.jumlah_selesai = "";
                    data.jumlah_belum_terbaca = "";
                    data.jumlah_rapat_online = "";
                    data.kotak_masuk = "";
                    data.inisiatif = "";

                    //Models.Entities.RekapSurat inisiatifmasuk =persuratanmodel.JumlahSurat(data,id_pegawai, myProfiles, "Inisiatif");

                    //Baru

                    //Functions functions = new Functions();
                    string pegawaiid = data.id_pegawai; 
                    kantorid = data.kantorid;
                    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

                    //Models.Entities.ProfileTU profiletu = dataMasterModel.GetProfileTUByNip(pegawaiid, kantorid);
                    //if (profiletu != null)
                    //{
                    //    profileidtu = profiletu.ProfileIdTU;
                    //}
                    unitkerjaid = dataMasterModel.GetUnitKerjaIdByNip(pegawaiid, kantorid);

                    string satkerid = kantorid;
                    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                    if (tipekantorid == 1)
                    {
                        //satkerid = profileidtu;
                        satkerid = unitkerjaid;
                    }

                    int jumlahInisiatif = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Inisiatif");
                    int jumlahSuratMasuk = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Masuk");
                    int jumlahsuratBelumDibuka = persuratanmodel.JumlahSuratBelumDibuka(satkerid, pegawaiid, myProfiles);
                    int jumlahrapatonline = new MeetingModel().JumlahRapatOnlineSaya(satkerid, pegawaiid);
                    int jumlahpermintaantte = new TandaTanganElektronikModel().JumlahProsesDokumen(data.id_pengguna);
                    //Baru
                    data.jumlah_surat = string.Format("{0:#,##0}", rekapsurat.JumlahSurat);
                    data.jumlah_inbox = string.Format("{0:#,##0}", rekapsurat.JumlahInbox);
                    data.jumlah_terkirim = string.Format("{0:#,##0}", rekapsurat.JumlahTerkirim);
                    data.jumlah_selesai = string.Format("{0:#,##0}", rekapsurat.JumlahSelesai);
                     data.kotak_masuk = String.Format("{0:#,##0}", jumlahSuratMasuk);
                     data.inisiatif = String.Format("{0:#,##0}", jumlahInisiatif);
                    data.jumlah_belum_terbaca = String.Format("{0:#,##0}", jumlahsuratBelumDibuka);
                    data.jumlah_rapat_online = String.Format("{0:#,##0}", jumlahrapatonline);
                    data.jumlah_permintaan_tte = String.Format("{0:#,##0}", jumlahpermintaantte);
                    //data.inisatif = string.Format("{0:#,##0}", rekapsurat.Inisatif);
                    //data.kotak_masuk = string.Format("{0:#,##0}", rekapsurat.KotakMasuk);
                }
            }

            return data;
        }

        public string NewGuID()
        {
            string _result = "";
            using (var ctx = new BpnDbContext())
            {
                _result = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
            }

            return _result;
        }

        public TransactionResult SettingSignature(string did, string uid, string unm, string kid, string dok, string fnm, string fex, string pid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var arrayListParameters = new ArrayList();
                        var parameters = arrayListParameters.OfType<object>().ToArray();
                        fex = (fex.Substring(0, 1) == ".") ? fex.Remove(0, 1) : fex;
                        string sql = string.Format(@"SELECT COUNT(1) FROM {0}.TBLUSERTTE WHERE PEGAWAIID = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("param1", pid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                        {
                            sql = string.Format(@"
                                UPDATE {0}.TBLUSERTTE SET TTDID = :param1, USERPERUBAHAN = :param2, TANGGALPERUBAHAN = SYSDATE
                                WHERE PEGAWAIID = :param3 AND NVL(STATUSHAPUS,'0') = '0'
                                ", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", did));
                            arrayListParameters.Add(new OracleParameter("param2", uid));
                            arrayListParameters.Add(new OracleParameter("param3", pid));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            sql = string.Format(@"
                                INSERT INTO {0}.TBLUSERTTE (PEGAWAIID, TTDID, TANGGALPERUBAHAN, USERPERUBAHAN, STATUSHAPUS)
                                VALUES (:param1,:param2,SYSDATE,:param3,'0')
                                ", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", pid));
                            arrayListParameters.Add(new OracleParameter("param2", did));
                            arrayListParameters.Add(new OracleParameter("param3", uid));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                  
                        int versi = CekVersi(did);
                        if (versi < 0)
                        {
                            sql = @"
                                INSERT INTO KONTENAKTIF (KONTENAKTIFID,KONTEN,VERSI,TANGGALSISIP,PETUGASSISIP,TANGGALSUNTING,PETUGASSUNTING,TIPE,KANTORID,JUDUL,EKSTENSI)
                                VALUES (:param1,:param2,0,SYSDATE,:param3,SYSDATE,:param4,:param5,:param6,:param7,:param8)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", did));
                            arrayListParameters.Add(new OracleParameter("param2", pid));
                            arrayListParameters.Add(new OracleParameter("param3", unm));
                            arrayListParameters.Add(new OracleParameter("param4", unm));
                            arrayListParameters.Add(new OracleParameter("param5", dok));
                            arrayListParameters.Add(new OracleParameter("param6", kid));
                            arrayListParameters.Add(new OracleParameter("param7", fnm));
                            arrayListParameters.Add(new OracleParameter("param8", fex));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            versi += 1;
                            //sql = string.Format("INSERT INTO KONTENPASIF SELECT SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI FROM KONTENAKTIF WHERE KONTENAKTIFID = '{0}'", did);
                            sql = @"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = :param1'";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", did));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            //sql = string.Format("UPDATE KONTENAKTIF SET KONTEN = '{0}', VERSI = {1}, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = '{2}', JUDUL = '{4}', EKSTENSI = '{5}  WHERE KONTENAKTIFID = '{3}'", pid, versi, unm, did, fnm, fex);
                            //ctx.Database.ExecuteSqlCommand(sql);

                            sql = "UPDATE KONTENAKTIF SET VERSI = :param1, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :param2, JUDUL = :param3, EKSTENSI = :param4  WHERE KONTENAKTIFID = :param5";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", versi));
                            arrayListParameters.Add(new OracleParameter("param2", unm));
                            arrayListParameters.Add(new OracleParameter("param3", fnm));
                            arrayListParameters.Add(new OracleParameter("param4", fex));
                            arrayListParameters.Add(new OracleParameter("param5", did));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = did;
                        tr.Pesan = "Spesimen Tanda Tangan NIP. " + pid + " berhasil disimpan.";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public int CekVersi(string id)
        {
            int result = 0;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    string sql = string.Format("SELECT COUNT(*) FROM KONTENAKTIF WHERE KONTENAKTIFID = '{0}'",id);
                    result = ctx.Database.SqlQuery<int>(sql).FirstOrDefault();
                    if (result == 0)
                    {
                        result = -1;
                    }
                    else
                    {
                        sql = string.Format("SELECT VERSI FROM KONTENAKTIF WHERE KONTENAKTIFID = '{0}'", id);
                        result = ctx.Database.SqlQuery<int>(sql).FirstOrDefault();
                    }
                }
            }
            return result;
        }

        public string GetPegawaiNameByUserId(string id)
        {
            string result = string.Empty;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    string sql = string.Format(@"
                        SELECT NAMA
                        FROM PEGAWAI
                        WHERE
	                        USERID = '{0}' AND
	                        TANGGALVALID IS NOT NULL AND
	                        (VALIDSAMPAI IS NULL OR TO_DATE(TRIM(VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY'))
                        UNION ALL
                        SELECT NAMA
                        FROM PPNPN
                        WHERE
	                        USERID = '{0}' AND
	                        TANGGALVALIDASI IS NOT NULL AND
	                        NVL(STATUSHAPUS,'0') = '0'", id);
                    result = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                }
            }
            return result;
        }

        public TransactionResult MeetingAttend(string kod, string uid, string lon, string lat)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Format(@"SELECT RAPATONLINEID FROM {0}.RAPATONLINE WHERE KODERAPAT = '{1}'", skema, kod);
                        string rapatid = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                        if (string.IsNullOrEmpty(rapatid)){
                            tc.Rollback();
                            tr.Status = false;
                            tr.Pesan = "Rapat Online Tidak Terdaftar";
                        }
                        else
                        {
                            sql = string.Format(@"
                                SELECT PEGAWAIID
                                FROM PEGAWAI
                                WHERE
	                                USERID = '{0}' AND
	                                (VALIDSAMPAI IS NULL OR TO_DATE(TRIM(VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY'))
                                UNION ALL
                                SELECT NIK
                                FROM PPNPN
                                WHERE
	                                USERID = '{0}' AND
	                                TANGGALVALIDASI IS NOT NULL AND
	                                NVL(STATUSHAPUS,'0') = '0'", uid);
                            string pid = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                            bool bisaabsen = false;
                            try
                            {
                                sql = string.Format(@"SELECT TIPERAPAT FROM {0}.RAPATONLINE WHERE RAPATONLINEID = '{1}'", skema, rapatid);
                                string tipe = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                                bisaabsen = tipe.Equals("Terbuka");
                            }
                            catch(Exception ex)
                            {
                                bisaabsen = false;
                            }
                            if (!bisaabsen)
                            {
                                sql = string.Format(@"SELECT COUNT(1) FROM {0}.PESERTARAPATONLINE WHERE RAPATONLINEID = '{1}' AND NIP = '{2}'", skema, rapatid, pid);
                                bisaabsen = (ctx.Database.SqlQuery<int>(sql).FirstOrDefault() > 0);
                            }

                            if (bisaabsen)
                            {
                                sql = string.Format(@"SELECT COUNT(1) FROM {0}.ABSENSIRAPATONLINE WHERE RAPATONLINEID = '{1}' AND PEGAWAIID = '{2}' AND NVL(STATUSHAPUS,'0') = '0'", skema, rapatid, pid);
                                if (ctx.Database.SqlQuery<int>(sql).FirstOrDefault() > 0)
                                {
                                    tc.Rollback();
                                    tr.Status = false;
                                    tr.Pesan = "Anda Sudah Pernah Mengisi Persensi Rapat ini sebelumnya";
                                }
                                else
                                {
                                    sql = string.Format(@"SELECT CASE WHEN (TANGGAL-INTERVAL'10'MINUTE) > SYSDATE THEN 'Rapat Online Belum Mulai' ELSE '' END FROM {0}.RAPATONLINE WHERE KODERAPAT = '{1}'", skema, kod);
                                    string pesanerror = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(pesanerror))
                                    {
                                        tc.Rollback();
                                        tr.Status = false;
                                        tr.Pesan = pesanerror;
                                    }else
                                    {
                                        sql = string.Format(@"
                                        INSERT INTO {0}.ABSENSIRAPATONLINE (ABSENSIID, RAPATONLINEID, PEGAWAIID, TANGGAL, LONGITUDE, LATITUDE)
                                        VALUES (RAWTOHEX(SYS_GUID()), '{1}', '{2}', SYSDATE, '{3}', '{4}')
                                        ", skema, rapatid, pid, lon, lat);
                                        ctx.Database.ExecuteSqlCommand(sql);

                                        tc.Commit();
                                        tr.Status = true;

                                        sql = string.Format(@"
                                        SELECT JUDUL FROM {0}.RAPATONLINE WHERE RAPATONLINEID = '{1}'", skema, rapatid);
                                        string judul = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                                        tr.Pesan = string.Concat("Selamat Datang di ",judul,"");
                                    }
                                }
                            }
                            else
                            {
                                tc.Rollback();
                                tr.Status = false;
                                tr.Pesan = "Anda tidak terdaftar didalam list peserta";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public string GetTipeFile(string id, string tip, string kid)
        {
            string result = "";

            string query = "SELECT EKSTENSI FROM KONTENAKTIF WHERE KONTENAKTIFID = :Id AND TIPE = :Tipe AND KANTORID = :KantorId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Id", id));
            arrayListParameters.Add(new OracleParameter("Tipe", tip));
            arrayListParameters.Add(new OracleParameter("KantorId", kid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                if (string.IsNullOrEmpty(result)) result = ".jpg";
                else
                {
                    if (result.Substring(0, 1) != ".") result = "." + result;
                }
            }

            return result;
        }
        /*
        public string getPhotoProfile(string idpegawai, string tipe)
        {
            Models.PersuratanModel persuratanmodel = new Models.PersuratanModel();
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
            Functions functions = new Functions();

            string unitkerjaid = "";
            //string profileidtu = "";
            string kantorid = "";

            Models.Entities.DataSurat data = new Models.Entities.DataSurat();

            List<Models.Entities.DataSurat> list = new List<Models.Entities.DataSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT DISTINCT " +
                "    u.userid AS id_pengguna, u.username AS nama_pengguna, u.email, " +
                "    p.pegawaiid AS id_pegawai, p.nama namapegawai, j.nama jabatan, " +
                "    k.kantorid, k.nama namakantor, 'https://simpeg.atrbpn.go.id/app/client/bpn/uploads/siap/foto/' || psimpeg.foto foto_profil " +
                "FROM " +
                "    users u, pegawai p, jabatanpegawai pp, jabatan j, kantor k, " +
                "    simpeg_2702.pegawai psimpeg " +
                "WHERE " +
                "    (pp.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(pp.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND NVL(pp.STATUSHAPUS,'0') = '0' " +
                "    AND (p.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(p.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) " +
                "    AND u.userid = p.userid " +
                "    AND p.pegawaiid = pp.pegawaiid " +
                "    AND pp.profileid = j.profileid " +
                "    AND pp.kantorid = k.kantorid " +
                "    AND psimpeg.nipbaru = p.pegawaiid " +
                "    AND u.username = :Username " +
                "ORDER BY k.nama";

            arrayListParameters.Add(new OracleParameter("Username", username));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                list = ctx.Database.SqlQuery<Models.Entities.DataSurat>(query, parameters).ToList<Models.Entities.DataSurat>();

                if (list.Count > 0)
                {
                    data = list[0];

                    kantorid = data.kantorid;

                    //Models.Entities.ProfileTU profiletu = dataMasterModel.GetProfileTUByNip(data.id_pegawai, kantorid);
                    //if (profiletu != null)
                    //{
                    //    profileidtu = profiletu.ProfileIdTU;
                    //}
                    unitkerjaid = dataMasterModel.GetUnitKerjaIdByNip(data.id_pegawai, kantorid);
                    string satkerid = kantorid;
                    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                    if (tipekantorid == 1)
                    {
                        //satkerid = profileidtu;
                        satkerid = unitkerjaid;
                    }

                    this.rekapsurat = persuratanmodel.GetRekapSuratPegawai(satkerid, data.id_pegawai);

                }
                else
                {
                    ArrayList arrayListParameterss = new ArrayList();
                    string sql =
                            "SELECT DISTINCT u.userid AS id_pengguna, u.username AS nama_pengguna, u.email ,p.NIK AS id_pegawai " +
                            ", p.NAMA AS namapegawai , j.nama AS jabatan ,k.kantorid, k.nama namakantor " +
                            ",'https://mitra.atrbpn.go.id/ppnpn/' || p.URLPROFILE AS foto_profil " +
                            "FROM PPNPN, PPNPN.USERS u , PPNPN.PPNPN p, jabatanpegawai pp, jabatan j , kantor k " +
                            "WHERE u.userid = p.userid " +
                            "AND p.nik = pp.pegawaiid " +
                            "AND j.PROFILEID= pp.PROFILEID " +
                            "AND k.KANTORID = pp.KANTORID " +
                            "AND u.username = :Username ";

                    arrayListParameterss.Add(new OracleParameter("Username", username));

                    object[] parameterss = arrayListParameterss.OfType<object>().ToArray();
                    list.Clear();
                    list = ctx.Database.SqlQuery<Models.Entities.DataSurat>(sql, parameterss).ToList<Models.Entities.DataSurat>();

                    if (list.Count > 0)
                    {
                        data = list[0];
                        kantorid = data.kantorid;

                        //Models.Entities.ProfileTU profiletu = dataMasterModel.GetProfileTUByNip(data.id_pegawai, kantorid);
                        //if (profiletu != null)
                        //{
                        //    profileidtu = profiletu.ProfileIdTU;
                        //}
                        unitkerjaid = dataMasterModel.GetUnitKerjaIdByNip(data.id_pegawai, kantorid);
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = profileidtu;
                            satkerid = unitkerjaid;
                        }

                        this.rekapsurat = persuratanmodel.GetRekapSuratPPNPN(satkerid, data.id_pegawai);
                    }
                    else
                    {

                    }
                }

                // Ambil data surat
                if (list.Count > 0)
                {
                    data.jumlah_surat = "";
                    data.jumlah_inbox = "";
                    data.jumlah_terkirim = "";
                    data.jumlah_selesai = "";
                    data.jumlah_belum_terbaca = "";
                    data.jumlah_rapat_online = "";
                    data.kotak_masuk = "";
                    data.inisiatif = "";

                    //Models.Entities.RekapSurat inisiatifmasuk =persuratanmodel.JumlahSurat(data,id_pegawai, myProfiles, "Inisiatif");

                    //Baru

                    //Functions functions = new Functions();
                    string pegawaiid = data.id_pegawai;
                    kantorid = data.kantorid;
                    string myProfiles = functions.MyProfiles(pegawaiid, kantorid);

                    //Models.Entities.ProfileTU profiletu = dataMasterModel.GetProfileTUByNip(pegawaiid, kantorid);
                    //if (profiletu != null)
                    //{
                    //    profileidtu = profiletu.ProfileIdTU;
                    //}
                    unitkerjaid = dataMasterModel.GetUnitKerjaIdByNip(pegawaiid, kantorid);

                    string satkerid = kantorid;
                    int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                    if (tipekantorid == 1)
                    {
                        //satkerid = profileidtu;
                        satkerid = unitkerjaid;
                    }

                    int jumlahInisiatif = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Inisiatif");
                    int jumlahSuratMasuk = persuratanmodel.JumlahSurat(satkerid, pegawaiid, myProfiles, "Masuk");
                    int jumlahsuratBelumDibuka = persuratanmodel.JumlahSuratBelumDibuka(satkerid, pegawaiid, myProfiles);
                    int jumlahrapatonline = new MeetingModel().JumlahRapatOnlineSaya(satkerid, pegawaiid);
                    //Baru
                    data.jumlah_surat = string.Format("{0:#,##0}", rekapsurat.JumlahSurat);
                    data.jumlah_inbox = string.Format("{0:#,##0}", rekapsurat.JumlahInbox);
                    data.jumlah_terkirim = string.Format("{0:#,##0}", rekapsurat.JumlahTerkirim);
                    data.jumlah_selesai = string.Format("{0:#,##0}", rekapsurat.JumlahSelesai);
                    data.kotak_masuk = String.Format("{0:#,##0}", jumlahSuratMasuk);
                    data.inisiatif = String.Format("{0:#,##0}", jumlahInisiatif);
                    data.jumlah_belum_terbaca = String.Format("{0:#,##0}", jumlahsuratBelumDibuka);
                    data.jumlah_rapat_online = String.Format("{0:#,##0}", jumlahrapatonline);
                    //data.inisatif = string.Format("{0:#,##0}", rekapsurat.Inisatif);
                    //data.kotak_masuk = string.Format("{0:#,##0}", rekapsurat.KotakMasuk);
                }
            }

            return data;
        }
        */

        public List<WebApiUser> GetDataLogin(string userid, string tipe)
        {
            var data = new List<WebApiUser>();

            var arrayListParameters = new ArrayList();
            string query = @"
                SELECT
                  JP.PEGAWAIID AS ID_PEGAWAI, COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA) AS NAMAPEGAWAI,
                  COALESCE(PS.EMAIL,PG.EMAIL,PP.EMAIL) AS EMAIL,
                  (JB.NAMA || DECODE(JP.STATUSPLT,1,' [PLT]','')) AS JABATAN, JP.PROFILEID AS ID_JABATAN, JB.UNITKERJAID, UK.NAMAUNITKERJA, UK.KANTORID, KT.NAMA AS NAMAKANTOR, UK.TIPEKANTORID, DECODE(JB.PROFILEID,JB.PROFILEIDTU,1,0) AS ISTU,
                  REPLACE(CAST(KT.LATITUDE AS VARCHAR2(20)),',','.') AS LATITUDEKANTOR,
                  REPLACE(CAST(KT.LONGITUDE AS VARCHAR2(20)),',','.') AS LONGITUDEKANTOR,
                  DECODE(LOWER(KT.BAGIANWILAYAH),'timur','WIT','tengah','WITA','WIB') AS ZONAWAKTU,
                  DECODE(PS.FOTO,NULL,'https://mitra.atrbpn.go.id/ppnpn/'||PP.URLPROFILE, 'https://simpeg.atrbpn.go.id/app/client/bpn/uploads/siap/foto/'||PS.FOTO) AS FOTO_PROFIL
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                  INNER JOIN UNITKERJA UK ON
  	                UK.UNITKERJAID = JB.UNITKERJAID
                  INNER JOIN KANTOR KT ON
                    KT.KANTORID = UK.KANTORID
                  LEFT JOIN simpeg_2702.v_pegawai_eoffice PS ON
                    PS.nipbaru = JP.PEGAWAIID
                  LEFT JOIN PEGAWAI PG ON
                    PG.PEGAWAIID = JP.PEGAWAIID AND
                    (PG.VALIDSAMPAI IS NULL OR CAST(PG.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                  LEFT JOIN PPNPN PP ON
                    PP.NIK = JP.PEGAWAIID AND
                    NVL(PP.STATUSHAPUS,'0')= '0'
                WHERE
                  JP.PEGAWAIID = :PegawaiId AND
                  (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                  NVL(JP.STATUSHAPUS,'0') = '0'"; 

            using (var ctx = new BpnDbContext())
            {
                string pegawaiid = string.Empty;
                if (tipe.ToLower().Equals("pegawai"))
                {
                    pegawaiid = ctx.Database.SqlQuery<string>(string.Format("SELECT PEGAWAIID FROM PEGAWAI WHERE USERID = '{0}'", userid)).FirstOrDefault();
                }
                else if (tipe.ToLower().Equals("ppnpn"))
                {
                    pegawaiid = ctx.Database.SqlQuery<string>(string.Format("SELECT NIK FROM PPNPN WHERE USERID = '{0}'", userid)).FirstOrDefault();
                }
                arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<WebApiUser>(query, parameters).ToList();
                foreach(var dt in data)
                {
                    dt.anakKantors = this.GetAnakKantors(dt.kantorid);
                    //if (dt.kantorid == "980FECFC746D8C80E0400B0A9214067D")
                    //{
                    //    List<AnakKantor> anakKantors = this.GetAnakKantors();
                    //    dt.anakKantors = anakKantors;
                    //}
                }
            }

            return data;
        }

        public DataSurat GetMainbadge(string userid, string unitkerjaid, string tipe, string profileid)
        {
            var persuratanmodel = new PersuratanModel();
            var dataMasterModel = new DataMasterModel();
            Functions functions = new Functions();

            string kantorid = "";
            string pegawaiid = "";

            var data = new DataSurat();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Empty;


            using (var ctx = new BpnDbContext())
            {
                if (tipe.ToLower().Equals("asn"))
                {
                    pegawaiid = ctx.Database.SqlQuery<string>(string.Format("SELECT PEGAWAIID FROM PEGAWAI WHERE USERID = '{0}'", userid)).FirstOrDefault();
                    data.foto_profil = "https://simpeg.atrbpn.go.id/app/client/bpn/uploads/siap/foto/";
                    data.nama_pengguna = ctx.Database.SqlQuery<string>(string.Format("SELECT USERNAME FROM USERS WHERE USERID = '{0}'", userid)).FirstOrDefault();
                }
                else if (tipe.ToLower().Equals("ppnpn"))
                {
                    pegawaiid = ctx.Database.SqlQuery<string>(string.Format("SELECT NIK FROM PPNPN WHERE USERID = '{0}'", userid)).FirstOrDefault();
                    data.foto_profil = "https://mitra.atrbpn.go.id/ppnpn/";
                    data.nama_pengguna = ctx.Database.SqlQuery<string>(string.Format("SELECT USERNAME FROM USERPPNPN WHERE USERID = '{0}'", userid)).FirstOrDefault();
                }
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                kantorid = ctx.Database.SqlQuery<string>(string.Format("SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = '{0}'", unitkerjaid)).FirstOrDefault();
                string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
                int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

                data.id_pengguna = userid;
                data.id_pegawai = pegawaiid;
                data.kantorid = kantorid;

                if (tipekantorid == 1)
                {
                    kantorid = unitkerjaid;
                }
                query = string.Format(@"
                    SELECT
                      JP.PEGAWAIID AS NIP, COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA) AS NAMAPEGAWAI,
                      COALESCE(PS.NAMAJABATAN, RST.JABATAN, JB.NAMA) AS JABATAN,
                      COALESCE(PS.foto, PP.URLPROFILE, 'foto.jpg') AS FOTO,
                      NVL(MAX(DECODE(TIPE, 'JUMLAHSURAT', JUMLAH)),0) AS JUMLAHSURAT,
                      NVL(MAX(DECODE(TIPE, 'JUMLAHINBOX', JUMLAH)),0) AS JUMLAHINBOX,
                      NVL(MAX(DECODE(TIPE, 'JUMLAHTERKIRIM', JUMLAH)),0) AS JUMLAHTERKIRIM,
                      NVL(MAX(DECODE(TIPE, 'JUMLAHSELESAI', JUMLAH)),0) AS JUMLAHSELESAI
                    FROM JABATANPEGAWAI JP
                      INNER JOIN JABATAN JB ON
                        JB.PROFILEID = JP.PROFILEID AND
                        NVL(JB.SEKSIID,'X') <> 'A800' AND
                        (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                      LEFT JOIN (
		                    SELECT
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA AS JABATAN, 'JUMLAHSURAT' AS TIPE,
		                      COUNT(*) AS JUMLAH
		                    FROM {0}.SURATINBOX SI
		                      INNER JOIN {0}.SURAT S ON
		                        S.SURATID = SI.SURATID
		                      INNER JOIN JABATANPEGAWAI JP ON
		                        JP.PROFILEID = SI.PROFILEPENERIMA AND
		                        JP.PEGAWAIID = SI.NIP AND
		                        (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
		                        NVL(JP.STATUSHAPUS, '0') = '0'
		                      INNER JOIN JABATAN JB ON
		                        JB.PROFILEID = JP.PROFILEID AND
		                        NVL(JB.SEKSIID,'X') <> 'A800' AND
		                        (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
		                    WHERE
		                      SI.STATUSTERKUNCI = 0 AND
		                      SI.STATUSFORWARDTU = 0 AND
		                      NVL(SI.STATUSHAPUS,'0')= '0'
		                    GROUP BY
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA
		                    UNION ALL
		                    SELECT
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA AS JABATAN, 'JUMLAHINBOX' AS TIPE,
		                      COUNT(*) AS JUMLAH
		                    FROM {0}.SURATINBOX SI
		                      INNER JOIN {0}.SURAT S ON
		                        S.SURATID = SI.SURATID
		                      INNER JOIN JABATANPEGAWAI JP ON
		                        JP.PROFILEID = SI.PROFILEPENERIMA AND
		                        JP.PEGAWAIID = SI.NIP AND
		                        (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
		                        NVL(JP.STATUSHAPUS, '0') = '0'
		                      INNER JOIN JABATAN JB ON
		                        JB.PROFILEID = JP.PROFILEID AND
		                        NVL(JB.SEKSIID,'X') <> 'A800' AND
		                        (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
		                    WHERE
		                      SI.STATUSTERKUNCI = 0 AND
		                      SI.STATUSFORWARDTU = 0 AND
		                      SI.STATUSTERKIRIM = 0 AND
		                      NOT EXISTS
		                        (SELECT 1
		                         FROM SURAT.ARSIPSURAT
		                         WHERE
		                           SURATID = SI.SURATID AND
		                           KANTORID = '{2}') AND
		                      NVL(SI.STATUSHAPUS,'0')= '0'
		                    GROUP BY
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA
		                    UNION ALL
		                    SELECT
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA AS JABATAN, 'JUMLAHTERKIRIM' AS TIPE,
		                      COUNT(*) AS JUMLAH
		                    FROM {0}.SURATINBOX SI
		                      INNER JOIN {0}.SURAT S ON
		                        S.SURATID = SI.SURATID
		                      INNER JOIN JABATANPEGAWAI JP ON
		                        JP.PROFILEID = SI.PROFILEPENERIMA AND
		                        JP.PEGAWAIID = SI.NIP AND
		                        (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
		                        NVL(JP.STATUSHAPUS, '0') = '0'
		                      INNER JOIN JABATAN JB ON
		                        JB.PROFILEID = JP.PROFILEID AND
		                        NVL(JB.SEKSIID,'X') <> 'A800' AND
		                        (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
		                    WHERE
		                      SI.STATUSTERKUNCI = 0 AND
		                      SI.STATUSFORWARDTU = 0 AND
		                      SI.STATUSTERKIRIM = 1 AND
		                      NOT EXISTS
		                        (SELECT 1
		                         FROM SURAT.ARSIPSURAT
		                         WHERE
		                           SURATID = SI.SURATID AND
		                           KANTORID = '{2}') AND
		                      NVL(SI.STATUSHAPUS,'0')= '0'
		                    GROUP BY
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA
		                    UNION ALL
		                    SELECT
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA AS JABATAN, 'JUMLAHSELESAI' AS TIPE,
		                      COUNT(*) AS JUMLAH
		                    FROM {0}.SURATINBOX SI
		                      INNER JOIN {0}.SURAT S ON
		                        S.SURATID = SI.SURATID
		                      INNER JOIN JABATANPEGAWAI JP ON
		                        JP.PROFILEID = SI.PROFILEPENERIMA AND
		                        JP.PEGAWAIID = SI.NIP AND
		                        (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
		                        NVL(JP.STATUSHAPUS, '0') = '0'
		                      INNER JOIN JABATAN JB ON
		                        JB.PROFILEID = JP.PROFILEID AND
		                        NVL(JB.SEKSIID,'X') <> 'A800' AND
		                        (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
		                    WHERE
		                      SI.STATUSTERKUNCI = 0 AND
		                      SI.STATUSFORWARDTU = 0 AND
		                      EXISTS
		                        (SELECT 1
		                         FROM SURAT.ARSIPSURAT
		                         WHERE
		                           SURATID = SI.SURATID AND
		                           KANTORID = '{2}') AND
		                      NVL(SI.STATUSHAPUS,'0')= '0'
		                    GROUP BY
		                      JP.PEGAWAIID, JB.UNITKERJAID, JB.NAMA) RST ON
                        RST.PEGAWAIID = JP.PEGAWAIID
                      LEFT JOIN simpeg_2702.v_pegawai_eoffice PS ON
                        PS.nipbaru = JP.PEGAWAIID
                      LEFT JOIN PEGAWAI PG ON
                        PG.PEGAWAIID = JP.PEGAWAIID AND
                        (PG.VALIDSAMPAI IS NULL OR CAST(PG.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                      LEFT JOIN PPNPN PP ON
                        PP.NIK = JP.PEGAWAIID AND
                        NVL(PP.STATUSHAPUS,'0')= '0'
                    WHERE
                      (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                      NVL(JP.STATUSHAPUS, '0') = '0' AND
                      JB.UNITKERJAID = '{1}' AND
                      JP.PEGAWAIID = '{3}' AND
                      JP.PROFILEID = '{4}'
                    GROUP BY
                      JP.PEGAWAIID, COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA), NVL(PS.GOL,99),
                      COALESCE(PS.NAMAJABATAN, RST.JABATAN, JB.NAMA),
                      COALESCE(PS.foto, PP.URLPROFILE, 'foto.jpg')", System.Web.Mvc.OtorisasiUser.NamaSkema, unitkerjaid, kantorid, pegawaiid, profileid);
                rekapsurat = ctx.Database.SqlQuery<RekapSurat>(query).FirstOrDefault();
                if(rekapsurat != null)
                {
                    int jumlahInisiatif = persuratanmodel.JumlahSurat(kantorid, pegawaiid, myProfiles, "Inisiatif");
                    int jumlahSuratMasuk = persuratanmodel.JumlahSurat(kantorid, pegawaiid, myProfiles, "Masuk");
                    int jumlahsuratBelumDibuka = persuratanmodel.JumlahSuratBelumDibuka(kantorid, pegawaiid, myProfiles);
                    int jumlahrapatonline = new MeetingModel().JumlahRapatOnlineSaya(kantorid, pegawaiid);
                    int jumlahpermintaantte = new TandaTanganElektronikModel().JumlahProsesDokumen(userid);

                    data.jabatan = rekapsurat.Jabatan;
                    data.namapegawai = rekapsurat.NamaPegawai;
                    data.foto_profil += rekapsurat.Foto;
                    data.jumlah_surat = string.Format("{0:#,##0}", rekapsurat.JumlahSurat);
                    data.jumlah_inbox = string.Format("{0:#,##0}", rekapsurat.JumlahInbox);
                    data.jumlah_terkirim = string.Format("{0:#,##0}", rekapsurat.JumlahTerkirim);
                    data.jumlah_selesai = string.Format("{0:#,##0}", rekapsurat.JumlahSelesai);
                    data.kotak_masuk = String.Format("{0:#,##0}", jumlahSuratMasuk);
                    data.inisiatif = String.Format("{0:#,##0}", jumlahInisiatif);
                    data.jumlah_belum_terbaca = String.Format("{0:#,##0}", jumlahsuratBelumDibuka);
                    data.jumlah_rapat_online = String.Format("{0:#,##0}", jumlahrapatonline);
                    data.jumlah_permintaan_tte = String.Format("{0:#,##0}", jumlahpermintaantte);
                }
            }

            return data;
        }

        public List<WebApiUser> GetDataAdmin(string userid, string tipe)
        {
            var data = new List<WebApiUser>();

            var arrayListParameters = new ArrayList();
            string query = @"
                SELECT 
                  JP.PEGAWAIID AS ID_PEGAWAI, 
                  COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA) AS NAMAPEGAWAI, JB.UNITKERJAID, 
                  UK.KANTORID, KT.NAMA AS NAMAKANTOR
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) 
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID 
                  INNER JOIN KANTOR KT ON
                    KT.KANTORID = UK.KANTORID 
                  LEFT JOIN simpeg_2702.v_pegawai_eoffice PS ON
                    PS.nipbaru = JP.PEGAWAIID 
                  LEFT JOIN PEGAWAI PG ON
                    PG.PEGAWAIID = JP.PEGAWAIID AND
                    (PG.VALIDSAMPAI IS NULL OR CAST(PG.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) 
                  LEFT JOIN PPNPN PP ON
                    PP.NIK = JP.PEGAWAIID AND
                    NVL(PP.STATUSHAPUS,'0')= '0' 
                  INNER JOIN JABATANPEGAWAI AD ON
                    AD.KANTORID = UK.KANTORID AND
                    AD.PROFILEID = 'A80300' AND
                    AD.PEGAWAIID = JP.PEGAWAIID AND
                    (AD.VALIDSAMPAI IS NULL OR TRUNC(CAST(AD.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                    NVL(AD.STATUSHAPUS,'0') = '0' 
                WHERE
                  JP.PEGAWAIID = :PegawaiId AND
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                  NVL(JP.STATUSHAPUS,'0') = '0'";

            using (var ctx = new BpnDbContext())
            {
                string pegawaiid = string.Empty;
                if (tipe.ToLower().Equals("pegawai"))
                {
                    pegawaiid = ctx.Database.SqlQuery<string>(string.Format("SELECT PEGAWAIID FROM PEGAWAI WHERE USERID = '{0}'", userid)).FirstOrDefault();
                }
                else if (tipe.ToLower().Equals("ppnpn"))
                {
                    pegawaiid = ctx.Database.SqlQuery<string>(string.Format("SELECT NIK FROM PPNPN WHERE USERID = '{0}'", userid)).FirstOrDefault();
                }
                arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<WebApiUser>(query, parameters).ToList();
            }

            return data;
        }

        public List<ComboList> getListUnitKerjaSPBE()
        {
            var records = new List<ComboList>();

            string query = @"
                SELECT
                  UK.UNITKERJAID AS VALUE, UK.NAMAUNITKERJA AS TEXT
                FROM UNITKERJA UK
                  INNER JOIN JABATAN JB ON
                    JB.UNITKERJAID = UK.UNITKERJAID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = JB.PROFILEID AND
                    (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                WHERE
                  UK.TAMPIL = 1 AND
                  UK.TIPEKANTORID = 1 AND
                  UK.ESELON IS NOT NULL
                GROUP BY
                  UK.UNITKERJAID, UK.NAMAUNITKERJA, UK.ESELON
                ORDER BY UK.ESELON";
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<ComboList>(query).ToList();
            }

            return records;
        }

        public List<ComboList> getListUnitKerja(string tipekantorid)
        {
            var records = new List<ComboList>();

            string query = @"
                SELECT
                  UK.UNITKERJAID AS VALUE, UK.NAMAUNITKERJA AS TEXT
                FROM UNITKERJA UK
                  INNER JOIN JABATAN JB ON
                    JB.UNITKERJAID = UK.UNITKERJAID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = JB.PROFILEID AND
                    (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                WHERE
                  UK.TAMPIL = 1 AND
                  UK.TIPEKANTORID = :param1
                GROUP BY
                  UK.UNITKERJAID, UK.NAMAUNITKERJA, UK.ESELON
                ORDER BY UK.ESELON";
            var parameters = new ArrayList();
            parameters.Add(new OracleParameter("param1", tipekantorid));
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<ComboList>(query,parameters.ToArray()).ToList();
            }

            return records;
        }

        public List<ComboList> getListAkun(string unitkerjaid)
        {
            var records = new List<ComboList>();
            var arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                    US.USERID AS VALUE, PG.PEGAWAIID || ' - ' || PG.NAMA AS TEXT
                FROM UNITKERJA UK
                    INNER JOIN JABATAN JB ON
                    JB.UNITKERJAID = UK.UNITKERJAID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                    INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = JB.PROFILEID AND
                    (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                    INNER JOIN PEGAWAI PG ON
  	                PG.PEGAWAIID = JP.PEGAWAIID AND
                    (PG.VALIDSAMPAI IS NULL OR CAST(PG.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                    INNER JOIN USERS US ON
  	                US.USERID = PG.USERID
                WHERE
                    UK.TAMPIL = 1 AND
                    UK.UNITKERJAID = :param1
                GROUP BY
                    US.USERID, PG.PEGAWAIID || ' - ' || PG.NAMA
                ORDER BY PG.PEGAWAIID || ' - ' || PG.NAMA";
            using (var ctx = new BpnDbContext())
            {
                arrayListParameters.Add(new OracleParameter("param1", unitkerjaid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ComboList>(query,parameters).ToList();
            }

            return records;
        }
    }
}