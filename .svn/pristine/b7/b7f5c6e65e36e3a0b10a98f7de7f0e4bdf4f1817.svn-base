using Surat.Codes;
using System;
using System.Collections;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text.RegularExpressions;
using Surat.Models.Entities;
using System.Web;

namespace Surat.Models
{
    public class TandaTanganElektronikModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();
        
        public List<DokumenTTE> GetListDokumenTTE(string userid, string profiletu, bool isTU, CariDokumenTTE f, int from, int to)
        {
            var records = new List<DokumenTTE>();
            var arrayListParameters = new ArrayList();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            f.SortBy = string.IsNullOrEmpty(f.SortBy) ? "TDE.TANGGALDIBUAT DESC" : f.SortBy;
            string query = string.Format(@"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER, 
                    COUNT(1) OVER() TOTAL,
                    TDE.DOKUMENELEKTRONIKID,
                    TDE.NOMORSURAT,
                    TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT,
                    TO_CHAR(TDE.TANGGALDIBUAT, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                    TO_CHAR(TDE.TANGGALSETUJU, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSETUJU, 
                    TO_CHAR(TDE.TANGGALTOLAK, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALTOLAK, 
                    TDE.PERIHAL,
                    TDE.SIFATSURAT, TDE.HALAMANTTE AS POSISITTE
                FROM {0}.TBLDOKUMENELEKTRONIK TDE WHERE TDE.USERPEMBUAT = :param1 AND NVL(TDE.STATUSHAPUS,'0') = '0'", skema, f.SortBy);
            arrayListParameters.Clear();
            arrayListParameters.Add(new OracleParameter("param1", userid));

            if (f.Tipe == "pembuat" && isTU)
            {
                query = string.Format(@"
                        SELECT 
                          ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER,  
                          COUNT(1) OVER() TOTAL, RST.DOKUMENELEKTRONIKID, RST.NOMORSURAT, 
                          RST.TANGGALSURAT, 
                          TO_CHAR(RST.TANGGALDIBUAT, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                          RST.TANGGALTOLAK,
                          RST.PERIHAL, RST.SIFATSURAT
                        FROM 
                          (SELECT DISTINCT 
                             TDE.DOKUMENELEKTRONIKID, TDE.NOMORSURAT, 
                             TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT, 
                             TDE.TANGGALDIBUAT, 
                             TO_CHAR(TDE.TANGGALTOLAK, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALTOLAK, 
                             TDE.PERIHAL, TDE.SIFATSURAT, TDE.HALAMANTTE AS POSISITTE
                           FROM {0}.TBLDOKUMENELEKTRONIK TDE
                             LEFT JOIN PEGAWAI PG ON
                               PG.USERID = TDE.USERPEMBUAT AND
                               (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                             LEFT JOIN PPNPN PP ON
                               PP.USERID = TDE.USERPEMBUAT 
                             INNER JOIN JABATANPEGAWAI JP ON
                               (JP.PEGAWAIID = PG.PEGAWAIID OR JP.PEGAWAIID = PP.NIK) AND
                               (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                               NVL(JP.STATUSHAPUS,'0') = '0' AND
                               JP.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') AND
                               JP.KANTORID = :param1 
                             INNER JOIN JABATAN JB ON
                               JB.PROFILEID = JP.PROFILEID AND
                               (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                               (JB.PROFILEIDTU = :param2 OR JB.PROFILEIDBA = :param3) 
                             INNER JOIN {0}.TBLDOKUMENTTE TTE ON
                               TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID 
                           WHERE
                             NVL(TDE.STATUSHAPUS,'0') = '0' AND
                             TDE.KANTORID = :param4", skema, f.SortBy.Replace("TDE.","RST."));
                arrayListParameters.Clear();
                arrayListParameters.Add(new OracleParameter("param1", f.KantorId));
                arrayListParameters.Add(new OracleParameter("param2", profiletu));
                arrayListParameters.Add(new OracleParameter("param3", profiletu));
                arrayListParameters.Add(new OracleParameter("param4", f.KantorId));
            }
            if (f.Tipe == "persetujuan")
            {
                query = string.Format(@"
                SELECT
                    ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER,
                    COUNT(1) OVER() TOTAL,
                    TDE.DOKUMENELEKTRONIKID,
                    TDE.NOMORSURAT,
                    TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT,
                    TO_CHAR(TDE.TANGGALDIBUAT, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                    TDE.PERIHAL,
                    TDE.SIFATSURAT,
                    NVL(PG.NAMA,PP.NAMA) AS USERPEMBUAT
                FROM {0}.TBLDOKUMENELEKTRONIK TDE
                LEFT JOIN PEGAWAI PG ON
                    PG.USERID = TDE.USERPEMBUAT AND
                    (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                LEFT JOIN PPNPN PP ON
                    PP.USERID = TDE.USERPEMBUAT
                INNER JOIN JABATANPEGAWAI JP ON
                    (JP.PEGAWAIID = PG.PEGAWAIID OR JP.PEGAWAIID = PP.NIK) AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                	NVL(JP.STATUSHAPUS,'0') = '0' AND
                	JP.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') AND
                	JP.KANTORID = :param1
                INNER JOIN JABATAN JB ON
                	JB.PROFILEID = JP.PROFILEID AND
                    (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                	(JB.PROFILEIDTU = :param2 OR JB.PROFILEIDBA = :param3)
                LEFT JOIN {0}.TBLDOKUMENTTE TTE ON
                	TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID AND TTE.URUT = 1
                WHERE NVL(TDE.STATUSHAPUS,'0') = '0' AND (TTE.STATUS = 'P' OR TTE.DOKUMENELEKTRONIKID IS NULL) AND TDE.KANTORID = :param4 AND TDE.TANGGALTOLAK IS NULL AND TDE.TANGGALSETUJU IS NULL", skema, f.SortBy);
                arrayListParameters.Clear();
                arrayListParameters.Add(new OracleParameter("param1", f.KantorId));
                arrayListParameters.Add(new OracleParameter("param2", profiletu));
                arrayListParameters.Add(new OracleParameter("param3", profiletu));
                arrayListParameters.Add(new OracleParameter("param4", f.KantorId));
            }
            else if (f.Tipe != "pembuat")
            {
                string st = "W";
                string fd = string.Empty;
                if (f.Tipe == "proses") { st = "W"; fd = "AND TANGGALTOLAK IS NULL"; }
                else if (f.Tipe == "sudah") { st = "A"; }
                query = string.Format(@"
                   SELECT 
                     ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER, 
                     COUNT(1) OVER() TOTAL, TDE.DOKUMENELEKTRONIKID, NOMORSURAT, 
                     TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT, 
                     TO_CHAR(TDE.TANGGALDIBUAT, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                     TO_CHAR(TTE.TANGGAL, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALTTE,
                     TO_CHAR(TDE.TANGGALTOLAK, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALTOLAK, 
                     TDE.PERIHAL, TDE.SIFATSURAT, TDE.HALAMANTTE AS POSISITTE
                   FROM {0}.TBLDOKUMENELEKTRONIK TDE
                     INNER JOIN {0}.TBLDOKUMENTTE TTE ON
                       TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID AND
                       TTE.USERPENANDATANGAN = :param1 AND
                       TTE.STATUS = :param2 AND
                       NVL(TTE.STATUSHAPUS,'0') = '0' 
                   WHERE
                     NVL(TDE.STATUSHAPUS,'0') = '0' {2}", skema, f.SortBy, fd);
                arrayListParameters.Clear();
                arrayListParameters.Add(new OracleParameter("param1", userid));
                arrayListParameters.Add(new OracleParameter("param2", st));
            }

            if (!string.IsNullOrEmpty(f.NomorSurat))
            {
                query += " AND TDE.NOMORSURAT LIKE :filter1 ";
                arrayListParameters.Add(new OracleParameter("filter1", string.Concat("%", f.NomorSurat, "%")));
            }
            if (!string.IsNullOrEmpty(f.SifatSurat))
            {
                query += " AND TDE.SIFATSURAT = :filter2 ";
                arrayListParameters.Add(new OracleParameter("filter2", f.SifatSurat));
            }
            if (!string.IsNullOrEmpty(f.MetaData))
            {
                query += " AND LOWER(TDE.PERIHAL || TDE.NOMORSURAT) LIKE :filter3 ";
                arrayListParameters.Add(new OracleParameter("filter3", string.Concat("%", f.MetaData.ToLower(), "%")));
            }
            if(f.Tipe == "pembuat" && isTU)
            {
                query += @" 
                           GROUP BY
                             TDE.DOKUMENELEKTRONIKID,
                             TDE.NOMORSURAT,
                             TDE.TANGGALSURAT,
                             TDE.TANGGALDIBUAT,
                             TDE.TANGGALTOLAK,
                             TDE.PERIHAL,
                             TDE.SIFATSURAT,
                             TDE.HALAMANTTE)RST";
            }
            if (from+to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                arrayListParameters.Add(new OracleParameter("pStart", from));
                arrayListParameters.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<DokumenTTE>(query, parameters).ToList();

                if (f.Tipe == "pembuat")
                {
                    foreach (var i in records)
                    {
                        string id = i.DokumenElektronikId;
                        i.Status = "A";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("param1", id));
                        i.PosisiTTE = ctx.Database.SqlQuery<string>(string.Format("SELECT PG.NAMA FROM {0}.TBLDOKUMENTTE TTE INNER JOIN PEGAWAI PG ON PG.USERID = TTE.USERPENANDATANGAN WHERE TTE.DOKUMENELEKTRONIKID = :param1 AND TTE.STATUS IN ('P','W') AND NVL(TTE.STATUSHAPUS,'0') = '0' ORDER BY URUT", skema), arrayListParameters.ToArray()).FirstOrDefault();
                        
                        if (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM {0}.TBLDOKUMENTTE TTE INNER JOIN PEGAWAI PG ON PG.USERID = TTE.USERPENANDATANGAN WHERE TTE.DOKUMENELEKTRONIKID = :param1 AND TTE.STATUS IN ('P','W') AND NVL(TTE.STATUSHAPUS,'0') = '0'", skema), arrayListParameters.ToArray()).FirstOrDefault() > 0)
                        {
                            i.Status = "P";
                        }
                        int ct = ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM {0}.TBLDOKUMENTTE WHERE DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0' AND STATUS != 'P'", skema, i.DokumenElektronikId)).FirstOrDefault();
                        if(ct == 0)
                        {
                            if (string.IsNullOrEmpty(i.TanggalSetuju))
                            {
                                i.Status = "U";
                            }
                        }
                        if (!string.IsNullOrEmpty(i.TanggalTolak))
                        {
                            i.Status = "R";
                        }
                    }
                }else if(f.Tipe == "sudah")
                {
                    foreach (var i in records)
                    {
                        string tampilan = getTipeTTE(i.DokumenElektronikId, userid);
                        i.Status = "N";
                        if (tampilan == "visible") i.Status = "A";
                        if (!string.IsNullOrEmpty(i.TanggalTolak))
                        {
                            i.Status = "R";
                        }
                    }
                }
            }

            return records;
        }

        public DokumenTTE GetDokumenElektronik(string id)
        {
            var result = new DokumenTTE();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            //(TO_CHAR(TDE.TANGGALSURAT, 'dd', 'nls_date_language=INDONESIAN') || ' ' || TRIM(TO_CHAR(TDE.TANGGALSURAT, 'Month', 'nls_date_language=INDONESIAN')) || ' ' || TO_CHAR(TDE.TANGGALSURAT, 'yyyy', 'nls_date_language=INDONESIAN')) AS TANGGALSURAT,
            string query = string.Format(@"
                SELECT
                    TDE.DOKUMENELEKTRONIKID,
	                TDE.NOMORSURAT,
	                TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT,
	                TDE.PERIHAL,
	                TDE.SIFATSURAT,
	                TDE.NAMAFILE,
	                TDE.HALAMANTTE
                FROM {0}.TBLDOKUMENELEKTRONIK TDE
                WHERE
	                TDE.DOKUMENELEKTRONIKID = :param1 AND NVL(TDE.STATUSHAPUS,'0') = '0'", skema);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var arrayListParameters = new ArrayList();
                    arrayListParameters.Add(new OracleParameter("param1", id));
                    var parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<DokumenTTE>(query,parameters).First();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public int JumlahTTE(string id)
        {
            int result = 0;
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            string query = string.Format("SELECT COUNT(1) FROM {0}.TBLDOKUMENTTE TTE WHERE TTE.DOKUMENELEKTRONIKID = :param1 AND NVL(TTE.STATUSHAPUS,'0') = '0'", skema);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var arrayListParameters = new ArrayList();
                    arrayListParameters.Add(new OracleParameter("param1", id));
                    var parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<int>(query,parameters).First();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public string CekStatusDokumen(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var status = "";
            var data = new DokumenTTE();
            var arrayListParameters = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                string query = $@"SELECT TDE.DOKUMENELEKTRONIKID,
                                         TDE.TANGGALSETUJU,
                                         TO_CHAR(TDE.TANGGALSETUJU, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSETUJU,
                                         TO_CHAR(TDE.TANGGALTOLAK, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALTOLAK,
                                         TTE.STATUS
                                  FROM {skema}.TBLDOKUMENELEKTRONIK TDE
                                  LEFT JOIN {skema}.TBLDOKUMENTTE TTE ON TDE.DOKUMENELEKTRONIKID = TTE.DOKUMENELEKTRONIKID
                                  WHERE TDE.DOKUMENELEKTRONIKID = :dokid AND NVL(TDE.STATUSHAPUS,'0') = '0' AND TTE.TIPE = 1 AND NVL(TTE.STATUSHAPUS,'0') = '0' ";
                arrayListParameters.Add(new OracleParameter("dokid", id));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                try
                {
                    data = ctx.Database.SqlQuery<DokumenTTE>(query, parameters).First();
                } catch (Exception e)
                {
                    data = new DokumenTTE();
                }                
            }

            if (!string.IsNullOrEmpty(data.TanggalTolak))
            {
                status = "R";
            }
            else {
                if (string.IsNullOrEmpty(data.TanggalSetuju))
                {
                    status = "P";
                } else
                {
                    status = data.Status == "A" ? "A" : "U";
                }
            }

            return status;
        }

        public List<UserTTE> GetListUserTTE(string dokid, bool all = false)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var data = new List<UserTTE>();
            var arrayListParameters = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                string query = $@"SELECT TTE.DOKUMENELEKTRONIKID,
                                         TTE.USERPENANDATANGAN AS PENANDATANGANID,
                                         NVL(PG.PEGAWAIID,PPNPN.NIK) AS PEGAWAIID,
                                         NVL(DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, NULL, '', ', ' || PG.GELARBELAKANG),PPNPN.NAMA) AS NAMA,
                                         TTE.URUT, TTE.TIPE, TTE.STATUS
                                  FROM {skema}.TBLDOKUMENTTE TTE
                                  LEFT JOIN PEGAWAI PG ON TTE.USERPENANDATANGAN = PG.USERID AND (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) 
                                  LEFT JOIN PPNPN ON TTE.USERPENANDATANGAN = PPNPN.USERID
                                  WHERE TTE.DOKUMENELEKTRONIKID = :dokid {(all ? "" : "AND NVL(TTE.STATUSHAPUS,'0') = '0' ")}
                                  ORDER BY TTE.URUT";
                arrayListParameters.Add(new OracleParameter("dokid", dokid));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<UserTTE>(query, parameters).ToList();
            }
            return data;
        }

        public int JumlahProsesDokumen(string pegawaiid)
        {
            int result = 0;
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            string query = string.Format("SELECT COUNT(1) FROM {0}.TBLDOKUMENTTE TTE INNER JOIN {0}.TBLDOKUMENELEKTRONIK DOK ON DOK.DOKUMENELEKTRONIKID = TTE.DOKUMENELEKTRONIKID AND NVL(DOK.STATUSHAPUS,'0') = '0' AND DOK.TANGGALTOLAK IS NULL WHERE TTE.USERPENANDATANGAN = :param1 AND NVL(TTE.STATUSHAPUS,'0') = '0' AND TTE.STATUS = 'W'", skema);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var arrayListParameters = new ArrayList();
                    arrayListParameters.Add(new OracleParameter("param1", pegawaiid));
                    var parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<int>(query,parameters).First();
                }catch(Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public TransactionResult SimpanPengajuan(DokumenTTE data, string nama, string kantorid, string tipe)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var arrayListParameters = new ArrayList();
            var parameters = arrayListParameters.OfType<object>().ToArray();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        data.SifatSurat = string.IsNullOrEmpty(data.SifatSurat) ? "Biasa" : data.SifatSurat;
                        string sql = string.Format(@"
                            INSERT INTO {0}.TBLDOKUMENELEKTRONIK (DOKUMENELEKTRONIKID, NOMORSURAT, TANGGALSURAT, PERIHAL, SIFATSURAT, USERPEMBUAT, NAMAFILE, KANTORID, HALAMANTTE)
                            VALUES (:param1,:param2,TO_DATE (:param3, 'DD/MM/YYYY HH24:MI:SS'),:param4,:param5,:param6,:param7,:param8,:param9)
                            ", skema);
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                        arrayListParameters.Add(new OracleParameter("param2", data.NomorSurat));
                        arrayListParameters.Add(new OracleParameter("param3", Convert.ToDateTime(data.TanggalSurat).ToString(@"dd'/'MM'/'yyyy")));
                        arrayListParameters.Add(new OracleParameter("param4", data.Perihal));
                        arrayListParameters.Add(new OracleParameter("param5", data.SifatSurat));
                        arrayListParameters.Add(new OracleParameter("param6", data.UserPembuat));
                        arrayListParameters.Add(new OracleParameter("param7", data.NamaFile));
                        arrayListParameters.Add(new OracleParameter("param8", kantorid));
                        arrayListParameters.Add(new OracleParameter("param9", data.PosisiTTE));
                        if (data.Status == "A")
                        {
                            sql = string.Format(@"
                            INSERT INTO {0}.TBLDOKUMENELEKTRONIK (DOKUMENELEKTRONIKID, NOMORSURAT, TANGGALSURAT, PERIHAL, SIFATSURAT, USERPEMBUAT, NAMAFILE, KANTORID, HALAMANTTE, TANGGALSETUJU, USERSETUJU)
                            VALUES (:param1,:param2,TO_DATE (:param3, 'DD/MM/YYYY HH24:MI:SS'),:param4,:param5,:param6,:param7,:param8,:param9, SYSDATE, :param10)
                            ", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                            arrayListParameters.Add(new OracleParameter("param2", data.NomorSurat));
                            arrayListParameters.Add(new OracleParameter("param3", Convert.ToDateTime(data.TanggalSurat).ToString(@"dd'/'MM'/'yyyy")));
                            arrayListParameters.Add(new OracleParameter("param4", data.Perihal));
                            arrayListParameters.Add(new OracleParameter("param5", data.SifatSurat));
                            arrayListParameters.Add(new OracleParameter("param6", data.UserPembuat));
                            arrayListParameters.Add(new OracleParameter("param7", data.NamaFile));
                            arrayListParameters.Add(new OracleParameter("param8", kantorid));
                            arrayListParameters.Add(new OracleParameter("param9", data.PosisiTTE));
                            arrayListParameters.Add(new OracleParameter("param10", data.UserPembuat));
                        }
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql,parameters);
                        if (data.TTE != null)
                        {
                            foreach (var i in data.TTE)
                            {
                                sql = string.Format(@"
                                INSERT INTO {0}.TBLDOKUMENTTE (DOKUMENELEKTRONIKID, USERPENANDATANGAN, URUT, TIPE)
                                VALUES (:param1,:param2,:param3,:param4)
                                ", skema);
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                                arrayListParameters.Add(new OracleParameter("param2", i.PenandatanganId));
                                arrayListParameters.Add(new OracleParameter("param3", i.Urut));
                                arrayListParameters.Add(new OracleParameter("param4", i.Tipe));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql,parameters);

                                if (i.EMeterai.Equals("1"))
                                {
                                    sql = string.Format(@"
                                INSERT INTO {0}.TBLDOKUMENMETERAI (DOKUMENELEKTRONIKID, USERPENANDATANGAN, URUT)
                                VALUES (:param1,:param2,:param3)
                                ", skema);
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                                    arrayListParameters.Add(new OracleParameter("param2", i.PenandatanganId));
                                    arrayListParameters.Add(new OracleParameter("param3", i.Urut));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }

                            if(data.Status == "A")
                            {
                                sql = string.Format(@"
                                UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'W'
                                WHERE DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0' AND STATUS = 'P' AND URUT = 1
                                ", skema);
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }                            
                        }

                        //cek konten                        
                        int versi = CekVersi(data.DokumenElektronikId);
                        if (versi < 0)
                        {
                            sql = @"
                                INSERT INTO KONTENAKTIF (KONTENAKTIFID,KONTEN,VERSI,TANGGALSISIP,PETUGASSISIP,TANGGALSUNTING,PETUGASSUNTING,TIPE,KANTORID,JUDUL,EKSTENSI)
                                VALUES (:param1,:param2,0,SYSDATE,:param3,SYSDATE,:param4,:param5,:param6,:param7,:param8)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                            arrayListParameters.Add(new OracleParameter("param2", data.NomorSurat));
                            arrayListParameters.Add(new OracleParameter("param3", nama));
                            arrayListParameters.Add(new OracleParameter("param4", nama));
                            arrayListParameters.Add(new OracleParameter("param5", tipe));
                            arrayListParameters.Add(new OracleParameter("param6", kantorid));
                            arrayListParameters.Add(new OracleParameter("param7", data.NamaFile));
                            arrayListParameters.Add(new OracleParameter("param8", data.Ekstensi.Replace(".", "")));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql,parameters);
                        }
                        else
                        {
                            versi += 1;
                            //sql = string.Format("INSERT INTO KONTENPASIF SELECT SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI FROM KONTENAKTIF WHERE KONTENAKTIFID = '{0}'",data.DokumenElektronikId);
                            sql = @"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = :param1";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", data.DokumenElektronikId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql,parameters);

                            //sql = "UPDATE KONTENAKTIF SET KONTEN = :param1, VERSI = :param2, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :param3, JUDUL = :param4, EKSTENSI = :param5  WHERE KONTENAKTIFID = :param6";
                            //arrayListParameters.Clear();
                            //arrayListParameters.Add(new OracleParameter("param1", data.NomorSurat));
                            //arrayListParameters.Add(new OracleParameter("param2", versi));
                            //arrayListParameters.Add(new OracleParameter("param3", nama));
                            //arrayListParameters.Add(new OracleParameter("param4", data.NamaFile));
                            //arrayListParameters.Add(new OracleParameter("param5", data.Ekstensi));
                            //arrayListParameters.Add(new OracleParameter("param6", data.DokumenElektronikId));
                            //parameters = arrayListParameters.OfType<object>().ToArray();
                            //ctx.Database.ExecuteSqlCommand(sql,parameters);

                            sql = "UPDATE KONTENAKTIF SET VERSI = :param1, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :param2, JUDUL = :param3, EKSTENSI = :param4  WHERE KONTENAKTIFID = :param5";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", versi));
                            arrayListParameters.Add(new OracleParameter("param2", nama));
                            arrayListParameters.Add(new OracleParameter("param3", data.NamaFile));
                            arrayListParameters.Add(new OracleParameter("param4", data.Ekstensi));
                            arrayListParameters.Add(new OracleParameter("param5", data.DokumenElektronikId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.DokumenElektronikId;
                        tr.Pesan = "Dokumen Elektronik No. " + data.NomorSurat + " berhasil dibuat.";
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


        public TransactionResult SimpanPengajuanDraft(DokumenTTE data, string nama, string kantorid, string tipe, string KodeFile)
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
                        data.SifatSurat = string.IsNullOrEmpty(data.SifatSurat) ? "Biasa" : data.SifatSurat;
                        string sql = string.Format(@"
                            INSERT INTO {0}.TBLDOKUMENELEKTRONIK (DOKUMENELEKTRONIKID, KODEFILE, NOMORSURAT, TANGGALSURAT, PERIHAL, SIFATSURAT, USERPEMBUAT, NAMAFILE, KANTORID, HALAMANTTE)
                            VALUES ('{1}','{10}','{2}',TO_DATE ('{3}', 'DD/MM/YYYY HH24:MI:SS'),'{4}','{5}','{6}','{7}','{8}','{9}')
                            ", skema, data.DokumenElektronikId, data.NomorSurat, Convert.ToDateTime(data.TanggalSurat).ToString(@"dd'/'MM'/'yyyy"), data.Perihal, data.SifatSurat, data.UserPembuat, data.NamaFile, kantorid, data.PosisiTTE, KodeFile);
                        if (data.Status == "A")
                        {
                            sql = string.Format(@"
                            INSERT INTO {0}.TBLDOKUMENELEKTRONIK (DOKUMENELEKTRONIKID, KODEFILE, NOMORSURAT, TANGGALSURAT, PERIHAL, SIFATSURAT, USERPEMBUAT, NAMAFILE, KANTORID, HALAMANTTE, TANGGALSETUJU, USERSETUJU)
                            VALUES ('{1}','{10}','{2}',TO_DATE ('{3}', 'DD/MM/YYYY HH24:MI:SS'),'{4}','{5}','{6}','{7}','{8}','{9}', SYSDATE, '{6}')
                            ", skema, data.DokumenElektronikId, data.NomorSurat, Convert.ToDateTime(data.TanggalSurat).ToString(@"dd'/'MM'/'yyyy"), data.Perihal, data.SifatSurat, data.UserPembuat, data.NamaFile, kantorid, data.PosisiTTE, KodeFile);
                        }
                        ctx.Database.ExecuteSqlCommand(sql);
                        if (data.TTE != null)
                        {
                            foreach (var i in data.TTE)
                            {
                                sql = string.Format(@"
                                INSERT INTO {0}.TBLDOKUMENTTE (DOKUMENELEKTRONIKID, USERPENANDATANGAN, URUT, TIPE)
                                VALUES ('{1}','{2}',{3}, '{4}')
                                ", skema, data.DokumenElektronikId, i.PenandatanganId, i.Urut, i.Tipe);
                                ctx.Database.ExecuteSqlCommand(sql);
                            }

                            sql = string.Format(@"
                                UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'W'
                                WHERE DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0' AND STATUS = 'P' AND URUT = 1
                                ", skema, data.DokumenElektronikId);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }

                        //cek konten                        
                        int versi = CekVersi(data.DokumenElektronikId);
                        if (versi < 0)
                        {
                            sql = string.Format(@"
                                INSERT INTO KONTENAKTIF (KONTENAKTIFID,KONTEN,VERSI,TANGGALSISIP,PETUGASSISIP,TANGGALSUNTING,PETUGASSUNTING,TIPE,KANTORID,JUDUL,EKSTENSI)
                                VALUES ('{0}','{1}',0,SYSDATE,'{2}',SYSDATE,'{2}','{3}','{4}','{5}','{6}')
                                ", data.DokumenElektronikId, data.NomorSurat, nama, tipe, kantorid, data.NamaFile, data.Ekstensi.Replace(".", ""));
                            ctx.Database.ExecuteSqlCommand(sql);
                        }
                        else
                        {
                            versi += 1;
                            //sql = string.Format("INSERT INTO KONTENPASIF SELECT SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI FROM KONTENAKTIF WHERE KONTENAKTIFID = '{0}'", data.DokumenElektronikId);
                            sql = string.Format(@"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = '{0}'", data.DokumenElektronikId);
                            ctx.Database.ExecuteSqlCommand(sql);

                            sql = string.Format("UPDATE KONTENAKTIF SET KONTEN = '{0}', VERSI = {1}, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = '{2}', JUDUL = '{4}', EKSTENSI = '{5}  WHERE KONTENAKTIFID = '{3}'", data.NomorSurat, versi, nama, data.DokumenElektronikId, data.NamaFile, data.Ekstensi);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.DokumenElektronikId;
                        tr.Pesan = "Dokumen Elektronik No. " + data.NomorSurat + " berhasil dibuat.";
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



        public TransactionResult HapusDokumen(string id, string userid, string username, string alasan)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var arrayListParameters = new ArrayList();
                        var parameters = arrayListParameters.OfType<object>().ToArray();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDOKUMENELEKTRONIK SET STATUSHAPUS = '1', TANGGALHAPUS = SYSDATE, USERHAPUS = :param1, ALASANHAPUS = :param2 WHERE DOKUMENELEKTRONIKID = :param3 AND NVL(STATUSHAPUS,'0') = '0'", skema);
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("param1", userid));
                        arrayListParameters.Add(new OracleParameter("param2", alasan));
                        arrayListParameters.Add(new OracleParameter("param3", id));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql = "UPDATE KONTENAKTIF SET TANGGALAKHIRAKSES = SYSDATE, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :param1  WHERE KONTENAKTIFID = :param2";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("param1", username));
                        arrayListParameters.Add(new OracleParameter("param2", id));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Dokumen TTE berhasil dihapus";
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

        public string NewGuID()
        {
            string _result = "";
            using (var ctx = new BpnDbContext())
            {
                _result = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
            }

            return _result;
        }

        public string GetTipeDokumen(string kontenaktifid)
        {
            string result = "";
            List<object> lstparams = new List<object>();
            string sql = @"SELECT TIPE FROM KONTENAKTIF WHERE KONTENAKTIFID = :id";

            lstparams = new List<object>();
            lstparams.Add(new OracleParameter("id", kontenaktifid));
            var parameters = lstparams.ToArray();
            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
            }

            return result;
        }

        public int CekVersi(string id)
        {
            int result = 0;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    //cek exists
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT COUNT(*) FROM KONTENAKTIF WHERE KONTENAKTIFID = :id";
                    lstparams.Add(new OracleParameter("id", id));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
                    if (result == 0)
                    {
                        result = -1;
                    }
                    else
                    {
                        lstparams = new List<object>();
                        sql = "SELECT VERSI FROM KONTENAKTIF WHERE KONTENAKTIFID = :id";
                        lstparams.Add(new OracleParameter("id", id));
                        parameters = lstparams.ToArray();
                        result = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
                    }
                }
            }
            return result;
        }

        public TransactionResult InsertLog(string logid, string tipepengirim, string idpengirim, string aplikasi, string tipedokumen, string berkasid, string servis, string parameter)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        string sql =
                            @"INSERT INTO BSSNSERVICELOG (BSSNSERVICELOGID, SENDERTYPE, SENDERID, APPLICATIONNAME, DOCUMENTTYPE, BERKASID, SERVICENAME, SERVICEPARAMETER, REQUESTDATE) 
                                VALUES (:bssnservicelogid, :sendertype, :senderid, :applicationname, :documenttype, :berkasid, :servicename, :serviceparameter, SYSDATE)";
                        lstparams = new List<object>();
                        lstparams.Add(new OracleParameter("bssnservicelogid", logid));
                        lstparams.Add(new OracleParameter("sendertype", tipepengirim));
                        lstparams.Add(new OracleParameter("senderid", idpengirim));
                        lstparams.Add(new OracleParameter("applicationname", aplikasi));
                        lstparams.Add(new OracleParameter("documenttype", tipedokumen));
                        lstparams.Add(new OracleParameter("berkasid", berkasid));
                        lstparams.Add(new OracleParameter("servicename", servis));
                        lstparams.Add(new OracleParameter("serviceparameter", parameter));

                        var parameters = lstparams.ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        result.Status = true;
                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        result.Pesan = ex.Message;
                        tc.Rollback();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }
            return result;
        }

        public TransactionResult UpdateLog(string logid, string hasil, string status)
        {
            var result = new TransactionResult() { Status = false, Pesan = "" };
            List<object> lstparams = new List<object>();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql =
                            @"UPDATE BSSNSERVICELOG SET SERVICERESULT = :serviceresult, RESPONSEDATE = SYSDATE, STATUS = :status WHERE BSSNSERVICELOGID = :bssnservicelogid";
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("serviceresult", hasil));
                        lstparams.Add(new OracleParameter("status", status));
                        lstparams.Add(new OracleParameter("bssnservicelogid", logid));

                        var parameters = lstparams.ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        result.Status = true;
                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        result.Pesan = ex.Message;
                        tc.Rollback();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }
            return result;
        }

        public PenandatanganInfo getPenandatanganInfo(string pegawaiid)
        {
            var data = new PenandatanganInfo();
            string sql = "SELECT NIK, NAMA, NIP, JABATAN, TTDID, KANTORID FROM REGISTERUSERTTDDIGITAL WHERE PEGAWAIID = :nip ";
            List<object> lstparams = new List<object>();
            lstparams.Add(new OracleParameter("nip", pegawaiid));
            using (var ctx = new BpnDbContext())
            {
                data = ctx.Database.SqlQuery<PenandatanganInfo>(sql, lstparams.ToArray()).FirstOrDefault();
                if (data == null)
                {
                    sql = "SELECT NOMORIDENTITAS FROM PEGAWAI WHERE PEGAWAIID = :nip";
                    string nik = ctx.Database.SqlQuery<string>(sql, lstparams.ToArray()).FirstOrDefault();
                    if(!string.IsNullOrEmpty(nik))
                    {
                        sql = string.Format("SELECT NIK, NAMA, NIP, JABATAN, TTDID, KANTORID FROM REGISTERUSERTTDDIGITAL WHERE NIK = :nik ", nik);
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("nik", nik));
                        data = ctx.Database.SqlQuery<PenandatanganInfo>(sql, lstparams.ToArray()).FirstOrDefault();
                    }
                }
            }

            return data;
        }

        public TransactionResult ProsesTandaTangan(string id, string userid, string nama)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        lstparams.Add(new OracleParameter("param1", id));
                        lstparams.Add(new OracleParameter("param2", userid));
                        UserTTE tte = ctx.Database.SqlQuery<UserTTE>(string.Format("SELECT TIPE, URUT FROM {0}.TBLDOKUMENTTE WHERE DOKUMENELEKTRONIKID = :param1 AND USERPENANDATANGAN = :param2 AND STATUS = 'W' AND NVL(STATUSHAPUS,'0') = '0' ORDER BY URUT", skema),lstparams.ToArray()).FirstOrDefault();
                        
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'A', TANGGAL = SYSDATE
                            WHERE DOKUMENELEKTRONIKID = :param1 AND USERPENANDATANGAN = :param2 AND STATUS = 'W' AND URUT = :param3 AND NVL(STATUSHAPUS,'0') = '0'
                            ", skema); 
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("param1", id));
                        lstparams.Add(new OracleParameter("param2", userid));
                        lstparams.Add(new OracleParameter("param3", tte.Urut));
                        ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());

                        try
                        {
                            sql = string.Format(@"
                                SELECT PG.PEGAWAIID
                                FROM {0}.TBLDOKUMENTTE TTE
                                INNER JOIN PEGAWAI PG ON
	                                PG.USERID = TTE.USERPENANDATANGAN
                                WHERE
                                  TTE.DOKUMENELEKTRONIKID = :param1 AND
                                  NVL(TTE.STATUSHAPUS,'0') = '0' AND
                                  TTE.STATUS = 'P' AND
                                  TTE.URUT = :param2
                                ", skema);
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", id));
                            lstparams.Add(new OracleParameter("param2", tte.Urut + 1));
                            string nip = ctx.Database.SqlQuery<string>(sql, lstparams.ToArray()).FirstOrDefault();
                            if (string.IsNullOrEmpty(nip))
                            {
                                tr.ReturnValue = "Finish";
                            }
                            else
                            {
                                try
                                {
                                    new Mobile().KirimNotifikasi(nip, "asn", "Eoffice", "Permintaan Tandatangan Dokumen Elektronik", "Permintaan TTE");
                                }
                                catch (Exception ex)
                                {
                                    var str = ex.Message;
                                }

                                sql = string.Format(@"
                                    SELECT MIN(URUT)
                                    FROM {0}.TBLDOKUMENTTE TTE
                                    WHERE
                                      TTE.DOKUMENELEKTRONIKID = :param1 AND
                                      NVL(TTE.STATUSHAPUS,'0') = '0' AND
                                      TTE.STATUS = 'P' AND
                                      TTE.URUT > :param2
                                    ", skema);
                                lstparams.Clear();
                                lstparams.Add(new OracleParameter("param1", id));
                                lstparams.Add(new OracleParameter("param2", tte.Urut));
                                int _urutan = ctx.Database.SqlQuery<int>(sql, lstparams.ToArray()).FirstOrDefault();
                                if (_urutan > 0)
                                {
                                    sql = string.Format(@"
                                UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'W'
                                WHERE DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0' AND STATUS = 'P' AND URUT = :param2
                                ", skema);
                                lstparams.Clear();
                                lstparams.Add(new OracleParameter("param1", id));
                                    lstparams.Add(new OracleParameter("param2", _urutan));
                                ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());
                            }
                        }
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                        }

                        //if (tte.Tipe == "0")
                        //{
                        //    try
                        //    {
                        //        sql = string.Format(@"
                        //        SELECT PG.PEGAWAIID
                        //        FROM {0}.TBLDOKUMENTTE TTE
                        //        INNER JOIN PEGAWAI PG ON
	                       //         PG.USERID = TTE.USERPENANDATANGAN
                        //        WHERE
                        //          TTE.DOKUMENELEKTRONIKID = :param1 AND
                        //          NVL(TTE.STATUSHAPUS,'0') = '0' AND
                        //          TTE.STATUS = 'P' AND
                        //          TTE.URUT = :param2
                        //        ", skema);
                        //        lstparams.Clear();
                        //        lstparams.Add(new OracleParameter("param1", id));
                        //        lstparams.Add(new OracleParameter("param2", tte.Urut + 1));
                        //        string nip = ctx.Database.SqlQuery<string>(sql, lstparams.ToArray()).FirstOrDefault();
                        //        try
                        //        {
                        //            new Mobile().KirimNotifikasi(nip, "asn", "Eoffice", "Permintaan Tandatangan Dokumen Elektronik", "Permintaan TTE");
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            var str = ex.Message;
                        //        }
                        //    }
                        //    catch(Exception ex)
                        //    {
                        //        string msg = ex.Message;
                        //    }

                        //    sql = string.Format(@"
                        //        UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'W'
                        //        WHERE DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0' AND STATUS = 'P' AND URUT = :param2
                        //        ", skema);
                        //    lstparams.Clear();
                        //    lstparams.Add(new OracleParameter("param1", id));
                        //    lstparams.Add(new OracleParameter("param2", tte.Urut + 1));
                        //    ctx.Database.ExecuteSqlCommand(sql,lstparams.ToArray());
                        //}
                        //else
                        //{
                        //    tr.ReturnValue = "Finish";
                        //}

                        int versi = CekVersi(id);
                        if (versi < 0)
                        {
                            tr.Pesan = "Gagal Mendapatkan Konten Aktif";
                            tc.Rollback();
                            return tr;
                        }
                        else
                        {
                            versi += 1;
                            sql = @"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = :param1";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", id));
                            ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());

                            sql = "UPDATE KONTENAKTIF SET VERSI = :param1, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :param2 WHERE KONTENAKTIFID = :param3";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", versi));
                            lstparams.Add(new OracleParameter("param2", nama));
                            lstparams.Add(new OracleParameter("param3", id));
                            ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Proses Berhasil";
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

        public TransactionResult updateKontenAktifDokumenId(string kontenaktifid, string dokumenid, string berkasid, string versi)
        {
            TransactionResult result = new TransactionResult() { Status = false, Pesan = "" };
            var lstparams = new List<object>();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "UPDATE kontenaktif SET dokumenid = :dokid, berkasid = :berkasid ";
                        lstparams.Add(new OracleParameter("dokid", dokumenid));
                        lstparams.Add(new OracleParameter("berkasid", berkasid));

                        if (!String.IsNullOrEmpty(versi))
                        {
                            sql = String.Concat(sql, "  , versi = :versi ");
                            lstparams.Add(new OracleParameter("versi", versi));
                        }

                        sql = String.Concat(sql, "  WHERE kontenaktifid = :kontenid ");
                        lstparams.Add(new OracleParameter("kontenid", kontenaktifid));

                        var parameters = lstparams.ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        sql = "UPDATE BERKASDARING SET TANGGALCETAK = SYSDATE WHERE BERKASID = :berkasid ";
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("berkasid", berkasid));
                        parameters = lstparams.ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        result.Status = true;
                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        result.Pesan = String.Concat("updateKontenAktifDokumenId", berkasid, ex.Message);
                        tc.Rollback();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }
            return result;
        }

        public TransactionResult GetKodeFile(string id)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new ArrayList();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                        string sql = string.Format("SELECT KODEFILE FROM {0}.TBLDOKUMENELEKTRONIK WHERE DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("param1", id));
                        string _kode = ctx.Database.SqlQuery<string>(sql, lstparams.ToArray()).FirstOrDefault();
                        if (string.IsNullOrEmpty(_kode) || _kode.Length == 6)
                        {
                            bool check = true;
                            do
                            {
                                _kode = functions.RndCode(8);
                                sql = string.Format("SELECT COUNT(1) FROM {0}.TBLDOKUMENELEKTRONIK WHERE KODEFILE = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
                                lstparams.Clear();
                                lstparams.Add(new OracleParameter("param1", _kode));
                                check = (ctx.Database.SqlQuery<int>(sql,lstparams.ToArray()).FirstOrDefault() > 0);
                            } while (check);

                            sql = string.Format("UPDATE {0}.TBLDOKUMENELEKTRONIK SET KODEFILE = :param1 WHERE DOKUMENELEKTRONIKID = :param2 AND NVL(STATUSHAPUS,'0') = '0'", skema);
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", _kode));
                            lstparams.Add(new OracleParameter("param2", id));
                            ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());
                        } 

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = _kode;
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

        public string getFileName(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            string filename = "DokumenTTE.pdf";
            string query = string.Format(@"
                SELECT NAMAFILE
                FROM {0}.TBLDOKUMENELEKTRONIK
                WHERE
                  DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    lstparams.Clear();
                    lstparams.Add(new OracleParameter("param1", id));
                    filename = ctx.Database.SqlQuery<string>(query,lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return filename;
        }

        public string getUserId(string nip)
        {
            string userid = string.Empty;
            var lstparams = new ArrayList();
            string query = @"SELECT USERID FROM PEGAWAI WHERE PEGAWAIID = :param1";
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", nip));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    userid = ctx.Database.SqlQuery<string>(query,lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return userid;
        }

        public string getTipeTTE(string dokid, string userid)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string result = string.Empty;
            var lstparams = new ArrayList();
            string query = string.Format(@"SELECT DECODE(TIPE,'1','visible','invisible') FROM {0}.TBLDOKUMENTTE WHERE DOKUMENELEKTRONIKID = :param1 AND USERPENANDATANGAN = :param2 AND NVL(USERHAPUS,'0') = '0'", skema);
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", dokid));
            lstparams.Add(new OracleParameter("param2", userid));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<string>(query, lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    result = "invisible";
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public string getDokid(string kode)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            string id = string.Empty;
            string query = string.Format(@"
                SELECT DOKUMENELEKTRONIKID
                FROM {0}.TBLDOKUMENELEKTRONIK
                WHERE
                  KODEFILE = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", kode));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    id = ctx.Database.SqlQuery<string>(query,lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return id;
        }

        public string getDokidFromDraft(string kode)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            string id = string.Empty;
            string query = string.Format(@"
                SELECT DOKUMENELEKTRONIKID
                FROM {0}.TBLDOKUMENELEKTRONIK
                WHERE
                  KODEFILE = :param1 AND NVL(STATUSHAPUS,'0') = '0' ORDER BY TANGGALDIBUAT DESC", skema);
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", kode));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    id = ctx.Database.SqlQuery<string>(query,lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return id;
        }


        public string getKantorid(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            string kantorid = string.Empty;
            string query = string.Format(@"
                SELECT KANTORID
                FROM {0}.TBLDOKUMENELEKTRONIK
                WHERE
                  DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", id));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    kantorid = ctx.Database.SqlQuery<string>(query,lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return kantorid;
        }

        public string getPosisiTTE(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            string halaman = "terakhir";
            string query = string.Format(@"
                SELECT HALAMANTTE
                FROM {0}.TBLDOKUMENELEKTRONIK
                WHERE
                  DOKUMENELEKTRONIKID = :param1 AND NVL(STATUSHAPUS,'0') = '0'", skema);
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", id));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    halaman = ctx.Database.SqlQuery<string>(query, lstparams.ToArray()).FirstOrDefault();
                    if (string.IsNullOrEmpty(halaman)) { halaman = "terakhir"; }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return halaman;
        }

        public List<SejarahDokumenTTE> GetDetailDokumen(string id, int from, int to, bool showsurat = false)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            var result = new List<SejarahDokumenTTE>();
            string query = string.Format(@"
                SELECT
	                0 AS URUT,
	                TO_CHAR(TDE.TANGGALDIBUAT, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGAL,
	                NVL(PG.PEGAWAIID,PP.NIK) AS NIP,
	                TRIM(NVL(PG.GELARDEPAN,'') || ' ' || NVL(PG.NAMA,PP.NAMA) || ' ' || NVL(PG.GELARBELAKANG,'')) AS NAMA,
	                COALESCE(PE.NAMAJABATAN,PG.JABATAN,'PPNPN') AS JABATAN,
	                'Pembuat Surat' AS TIPE,
                    'A' AS STATUS
                FROM {0}.TBLDOKUMENELEKTRONIK TDE
                LEFT JOIN PPNPN PP ON
	                PP.USERID = TDE.USERPEMBUAT
                LEFT JOIN PEGAWAI PG ON
	                PG.USERID = TDE.USERPEMBUAT
                LEFT JOIN SIMPEG_2702.V_PEGAWAI_EOFFICE PE ON
	                PE.NIPBARU = PG.PEGAWAIID
                WHERE
                  TDE.DOKUMENELEKTRONIKID = :param1
                UNION ALL
                SELECT
	                TTE.URUT,
	                TO_CHAR(TTE.TANGGAL, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGAL,
	                PG.PEGAWAIID AS NIP,
	                DIG.NAMA AS NAMA,
	                NVL(PE.NAMAJABATAN,PG.JABATAN) AS JABATAN,
	                DECODE(TTE.TIPE, '0', 'Paraf', 'Tanda Tangan') AS TIPE,
                    TTE.STATUS
                FROM {0}.TBLDOKUMENTTE TTE
                INNER JOIN PEGAWAI PG ON
	                PG.USERID = TTE.USERPENANDATANGAN
                INNER JOIN REGISTERUSERTTDDIGITAL DIG ON
	                DIG.NIP = PG.PEGAWAIID AND DIG.TANGGALKADALUARSA IS NULL
                LEFT JOIN SIMPEG_2702.V_PEGAWAI_EOFFICE PE ON
	                PE.NIPBARU = PG.PEGAWAIID
                WHERE
                  TTE.DOKUMENELEKTRONIKID = :param2 AND NVL(TTE.STATUSHAPUS,'0') = '0'
                ORDER BY URUT", skema);
            lstparams.Clear();
            lstparams.Add(new OracleParameter("param1", id));
            lstparams.Add(new OracleParameter("param2", id));
            query = string.Format(string.Concat("SELECT * FROM (SELECT ROW_NUMBER() over (ORDER BY RST.URUT) RNUMBER, COUNT(1) OVER() TOTAL, RST.* FROM ({0})RST ) WHERE RNUMBER BETWEEN :param3 AND :param4"),query);
            lstparams.Add(new OracleParameter("param3", from));
            lstparams.Add(new OracleParameter("param4", to));
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<SejarahDokumenTTE>(query, lstparams.ToArray()).ToList();
                    int urutan = result.Count;
                    if (showsurat)
                    {
                        query = string.Format("SELECT TO_CHAR(S.TANGGALINPUT, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGAL, S.NOMORSURAT AS NIP, S.PENGIRIM AS NAMA, S.TIPESURAT AS TIPE, 'A' AS STATUS FROM {0}.LAMPIRANSURAT LS INNER JOIN {0}.SURAT S ON S.SURATID = LS.SURATID WHERE LS.PATH = 'DokumenTTE|{1}'", skema,id);
                        var surat = ctx.Database.SqlQuery<SejarahDokumenTTE>(query).ToList();
                        foreach(var itm in surat)
                        {
                            result.Add(new SejarahDokumenTTE() { 
                            RNumber = urutan,
                            tanggal = itm.tanggal,
                            nip = HttpUtility.UrlDecode(itm.nip),
                            nama = itm.nama,
                            jabatan = "",
                            tipe = itm.tipe,
                            status = itm.status
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public string getNamaPenandatangan(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new ArrayList();
            string nama = string.Empty;
            string query = string.Format(@"
                SELECT
	                DIG.NAMA AS NAMA
                FROM {0}.TBLDOKUMENTTE TTE
                INNER JOIN PEGAWAI PG ON
	                PG.USERID = TTE.USERPENANDATANGAN
                INNER JOIN REGISTERUSERTTDDIGITAL DIG ON
	                DIG.NIP = PG.PEGAWAIID AND DIG.TANGGALKADALUARSA IS NULL
                WHERE
                  TTE.DOKUMENELEKTRONIKID = '{1}' AND TTE.TIPE = '1'", skema, id);
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    nama = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return nama;
        }

        public TransactionResult SetujuPengajuan(DokumenTTE data, string userid, string kantorid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var arrayListParameters = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDOKUMENELEKTRONIK SET 
                                TANGGALSETUJU = SYSDATE,
                                USERSETUJU = :usersetuju,
                                HALAMANTTE = :posisitte,
                                TANGGALSURAT = TO_DATE(:tanggalsurat, 'DD/MM/YYYY HH24:MI:SS')
                            WHERE DOKUMENELEKTRONIKID = :dokid AND NVL(STATUSHAPUS,'0') = '0'
                            ", skema);
                        arrayListParameters.Add(new OracleParameter("usersetuju", data.UserPembuat));
                        arrayListParameters.Add(new OracleParameter("posisitte", data.PosisiTTE));
                        arrayListParameters.Add(new OracleParameter("tanggalsurat", data.TanggalSurat));
                        arrayListParameters.Add(new OracleParameter("dokid", data.DokumenElektronikId));
                        var parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql,parameters);

                        if (data.TTE != null)
                        {
                            var recordedUserTTE = GetListUserTTE(data.DokumenElektronikId,true);

                            foreach(var r in recordedUserTTE)
                            {
                                if(!data.TTE.Exists(x=> x.PenandatanganId == r.PenandatanganId))
                                {
                                    arrayListParameters = new ArrayList();
                                    sql = string.Format(@"
                                    UPDATE {0}.TBLDOKUMENTTE SET STATUSHAPUS = '1', TIPE = '0' 
                                    WHERE DOKUMENELEKTRONIKID = :dokid AND USERPENANDATANGAN = :penandatanganid
                                    ", skema);
                                    arrayListParameters.Add(new OracleParameter("dokid", data.DokumenElektronikId));
                                    arrayListParameters.Add(new OracleParameter("penandatanganid", r.PenandatanganId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }

                            foreach (var i in data.TTE)
                            {
                                if(recordedUserTTE.Exists(x => x.PenandatanganId == i.PenandatanganId))
                                {
                                    arrayListParameters = new ArrayList();
                                    sql = string.Format(@"
                                    UPDATE {0}.TBLDOKUMENTTE SET URUT = :urut, TIPE = :tipe, STATUSHAPUS = '0' 
                                    WHERE DOKUMENELEKTRONIKID = :dokid AND USERPENANDATANGAN = :penandatanganid
                                    ", skema);
                                    arrayListParameters.Add(new OracleParameter("urut", i.Urut));
                                    arrayListParameters.Add(new OracleParameter("tipe", i.Tipe));
                                    arrayListParameters.Add(new OracleParameter("dokid", data.DokumenElektronikId));
                                    arrayListParameters.Add(new OracleParameter("penandatanganid", i.PenandatanganId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql,parameters);
                                }
                                else
                                {
                                    sql = string.Format(@"
                                    INSERT INTO {0}.TBLDOKUMENTTE (DOKUMENELEKTRONIKID, USERPENANDATANGAN, URUT, TIPE)
                                    VALUES ('{1}','{2}',{3}, '{4}')
                                    ", skema, data.DokumenElektronikId, i.PenandatanganId, i.Urut, i.Tipe);
                                        ctx.Database.ExecuteSqlCommand(sql);
                                }
                                
                            }

                            sql = string.Format(@"
                                UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'W'
                                WHERE DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0' AND STATUS = 'P' AND URUT = 1
                                ", skema, data.DokumenElektronikId);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.DokumenElektronikId;
                        tr.Pesan = "Dokumen Elektronik No. " + data.NomorSurat + " Telah Disetujui.";
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

        public TransactionResult TolakPengajuan(string id, string userid, string alasan)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDOKUMENELEKTRONIK SET TANGGALTOLAK = SYSDATE, USERTOLAK = '{2}', ALASANTOLAK = '{3}' WHERE DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0'", skema, id, userid, alasan);
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Pengajuan TTE berhasil ditolak";
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

        public string getInfoTTE(string pegawaiid, string field)
        {
            string result = "";

            string query = string.Format("SELECT {0} FROM REGISTERUSERTTDDIGITAL WHERE NIP = '{1}' AND TANGGALVALIDASI IS NOT NULL", field,pegawaiid);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var lstparams = new List<object>();
                    result = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }


        public List<ExpoSertipikat> GetListExpoSertipikat(string userid, string tipe, CariExpoSertipikat f, int from, int to)
        {
            var records = new List<ExpoSertipikat>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new List<object>();
            string query = string.Empty;
            query = string.Format(@"
                        SELECT 
                          ROW_NUMBER() OVER (ORDER BY XS.TANGGALUNGGAH) AS RNUMBER,  
                          COUNT(1) OVER() TOTAL, 
                          XS.EXPOSERTIPIKATID, 
                          XS.NAMAACARA, 
                          XS.TANGGALACARA, 
                          XS.NAMAPESERTA, 
                          XS.USERTTE, 
                          XS.TANGGALTTE
                        FROM {0}.TBLEXPOSERTIPIKAT XS
                             INNER JOIN PEGAWAI PG ON
                               PG.USERID = XS.USERTTE 
                           WHERE
                             XS.TANGGALHAPUS IS NULL", skema);
            if (tipe.Equals("pembuat"))
            {
                query += string.Format(" AND XS.USERUNGGAH = '{0}'", userid);
            }
            else
            {
                query += string.Format(" AND XS.USERTTE = '{0}'", userid);
                if (tipe.Equals("proses"))
                {
                    query += " AND XS.TANGGALTTE IS NULL";
                }
                else if (tipe.Equals("sudah"))
                {
                    query += " AND XS.TANGGALTTE IS NOT NULL";
                }
            }

            if (!string.IsNullOrEmpty(f.NamaAcara))
            {
                query += string.Format(" AND XS.NAMAACARA LIKE '%{0}%' ", f.NamaAcara);
            }
            if (!string.IsNullOrEmpty(f.Status))
            {
                if (f.Status.Equals("P"))
                {
                    query += " AND XS.TANGGALTTE IS NULL";
                }
                else if (f.Status.Equals("A"))
                {
                    query += " AND XS.TANGGALTTE IS NOT NULL";
                }
            }
            if (from + to > 0)
            {
                query = string.Format(string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN {0} AND {1}"), from, to);
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<ExpoSertipikat>(query).ToList(); 
            }

            return records;
        }

        public int getJumlahExpoTungguTTE(string userid)
        {
            int jumlah = 0;
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string query = string.Format("SELECT COUNT(1) FROM {0}.TBLEXPOSERTIPIKAT WHERE USERTTE = '{1}' AND TANGGALTTE IS NULL AND TANGGALHAPUS IS NULL", skema, userid);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var lstparams = new List<object>();
                    jumlah = ctx.Database.SqlQuery<int>(query).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return jumlah;
        }

        public List<string> getAllPendingSertifikat(string userid)
        {
            var data = new List<string>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string query = string.Format("SELECT EXPOSERTIPIKATID FROM {0}.TBLEXPOSERTIPIKAT WHERE USERTTE = '{1}' AND TANGGALTTE IS NULL AND TANGGALHAPUS IS NULL", skema, userid);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var lstparams = new List<object>();
                    data = ctx.Database.SqlQuery<string>(query).ToList(); ;
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return data;
        }

        public TransactionResult SimpanPengajuanExpoSertipikat(ExpoSertipikat data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        string sql = string.Format(@"
                            INSERT INTO {0}.TBLEXPOSERTIPIKAT (EXPOSERTIPIKATID, NAMAACARA, TANGGALACARA, NAMAPESERTA, FILENAME, USERUNGGAH, TANGGALUNGGAH, USERTTE)
                            VALUES ('{1}','{2}',TO_DATE ('{3}', 'DD/MM/YYYY HH24:MI:SS'),'{4}','{5}','{6}',SYSDATE,'{7}')
                            ", skema, data.ExpoSertipikatId, data.NamaAcara, data.TanggalAcara.ToString(@"dd'/'MM'/'yyyy"), data.NamaPeserta, data.FileName, data.UserUnggah, data.UserTTE);
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.ExpoSertipikatId;
                        tr.Pesan = "Sertipikat No. " + data.FileName + " berhasil disimpan.";
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

        public string getFileName_expo(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string filename = "DokumenTTE.pdf";
            string query = string.Format(@"
                SELECT FILENAME
                FROM {0}.TBLEXPOSERTIPIKAT
                WHERE
                  EXPOSERTIPIKATID = '{1}' AND TANGGALHAPUS IS NULL", skema, id);
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var lstparams = new List<object>();
                    filename = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return filename;
        }

        public TransactionResult ProsesTandaTangan_expo(string id, string userid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        string sql = string.Format(@"
                            UPDATE {0}.TBLEXPOSERTIPIKAT SET TANGGALTTE = SYSDATE
                            WHERE EXPOSERTIPIKATID = '{1}' AND USERTTE = '{2}' AND TANGGALHAPUS IS NULL AND TANGGALTTE IS NULL
                            ", skema, id, userid);
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Proses Berhasil";
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

        public TransactionResult cekPenolakanDokumen(string dokid)
        {
            var rst = new TransactionResult() { Status = false, Pesan = "" };

            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            string query = string.Format(@"
                SELECT
	                ('Ditolak oleh <b>' || PG.NAMA || '</b><br>Tanggal <b>' || TO_CHAR(TDE.TANGGALTOLAK, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || '</b><br><br>' || TDE.ALASANTOLAK) AS PESAN
                FROM {0}.TBLDOKUMENELEKTRONIK TDE
                INNER JOIN PEGAWAI PG ON
	                PG.USERID = TDE.USERTOLAK
                WHERE
                  DOKUMENELEKTRONIKID = '{1}'", skema,dokid);

            using (var ctx = new BpnDbContext())
            {
                string pesan = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                if (!string.IsNullOrEmpty(pesan))
                {
                    rst.Status = true;
                    rst.Pesan = pesan;
                }
            }

            return rst;
        }

        //public TransactionResult SimpanDraft(DraftSurat data, string nama, string kantorid)
        //{
        //    var tr = new TransactionResult() { Status = false, Pesan = "" };
        //    string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

        //    using (var ctx = new BpnDbContext())
        //    {
        //        using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                string sql = string.Empty;
        //                if (string.IsNullOrEmpty(data.DraftCode))
        //                {
        //                    data.DraftCode = NewDraftCode(data.UnitKerjaId);
        //                    sql = string.Format(@"
        //                        INSERT INTO {0}.TBLDRAFTSURAT (DRAFTCODE, UNITKERJAID, PERIHAL, KOPSURAT, KODEARSIP, SIFATSURAT, TIPESURAT, HALAMANTTE, ISISURAT, STATUS, UPDTIME, UPDUSER, PROFILEPENGIRIM)
        //                        VALUES ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','P',SYSDATE,'{10}','{11}')
        //                        ", skema, data.DraftCode, data.UnitKerjaId, data.Perihal, data.KopSurat, data.KodeArsip, data.SifatSurat, data.TipeSurat, data.PosisiTTE, data.IsiSurat, data.UserPembuat, data.ProfilePengirim);
        //                    ctx.Database.ExecuteSqlCommand(sql);
        //                    sql = string.Format(@"
        //                        INSERT INTO {0}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT)
        //                        VALUES (RAWTOHEX(SYS_GUID()),'{1}','{2}',SYSDATE,'New')
        //                        ", skema, data.DraftCode, data.UserPembuat);
        //                    ctx.Database.ExecuteSqlCommand(sql);
        //                }
        //                else
        //                {
        //                    string log = string.Empty;
        //                    string update = string.Empty;
        //                    var old = GetDraftSurat(data.DraftCode, data.UnitKerjaId);
        //                    if (string.IsNullOrEmpty(old.Perihal) || !old.Perihal.Equals(data.Perihal))
        //                    {
        //                        log = string.Concat(log,"PERIHAL|!",old.Perihal,"||");
        //                        update = string.Concat(update,"PERIHAL = '",data.Perihal,"',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.KopSurat) || !old.KopSurat.Equals(data.KopSurat))
        //                    {
        //                        log = string.Concat(log, "KOPSURAT|!", old.KopSurat, "||");
        //                        update = string.Concat(update, "KOPSURAT = '", data.KopSurat, "',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.KodeArsip) || !old.KodeArsip.Equals(data.KodeArsip))
        //                    {
        //                        log = string.Concat(log, "KODEARSIP|!", old.KodeArsip, "||");
        //                        update = string.Concat(update, "KODEARSIP = '", data.KodeArsip, "',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.SifatSurat) || !old.SifatSurat.Equals(data.SifatSurat))
        //                    {
        //                        log = string.Concat(log, "SIFATSURAT|!", old.SifatSurat, "||");
        //                        update = string.Concat(update, "SIFATSURAT = '", data.SifatSurat, "',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.TipeSurat) || !old.TipeSurat.Equals(data.TipeSurat))
        //                    {
        //                        log = string.Concat(log, "TIPESURAT|!", old.TipeSurat, "||");
        //                        update = string.Concat(update, "TIPESURAT = '", data.TipeSurat, "',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.PosisiTTE) || !old.PosisiTTE.Equals(data.PosisiTTE))
        //                    {
        //                        log = string.Concat(log, "HALAMANTTE|!", old.PosisiTTE, "||");
        //                        update = string.Concat(update, "HALAMANTTE = '", data.PosisiTTE, "',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.IsiSurat) || !old.IsiSurat.Equals(data.IsiSurat))
        //                    {
        //                        log = string.Concat(log, "ISISURAT|!", old.IsiSurat, "||");
        //                        update = string.Concat(update, "ISISURAT = '", data.IsiSurat, "',");
        //                    }
        //                    if (string.IsNullOrEmpty(old.ProfilePengirim) || !old.ProfilePengirim.Equals(data.ProfilePengirim))
        //                    {
        //                        log = string.Concat(log, "PROFILEPENGIRIM|!", old.ProfilePengirim, "||");
        //                        update = string.Concat(update, "PROFILEPENGIRIM = '", data.ProfilePengirim, "',");
        //                    }
        //                    if (!string.IsNullOrEmpty(update))
        //                    {
        //                        sql = string.Format(@"
        //                        UPDATE {0}.TBLDRAFTSURAT SET
        //                            {3} UPDTIME = SYSDATE, UPDUSER = '{4}'
        //                        WHERE DRAFTCODE = '{1}' AND UNITKERJAID = '{2}'
        //                        ", skema, data.DraftCode, data.UnitKerjaId, update, data.UserPembuat);
        //                        ctx.Database.ExecuteSqlCommand(sql);
        //                        sql = string.Format(@"
        //                        INSERT INTO {0}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT, LOGDETAIL)
        //                        VALUES (RAWTOHEX(SYS_GUID()),'{1}','{2}',SYSDATE,'Update','{3}')
        //                        ", skema, data.DraftCode, data.UserPembuat, log);
        //                        ctx.Database.ExecuteSqlCommand(sql);
        //                    }
        //                }
        //                if (!string.IsNullOrEmpty(data.listTujuan))
        //                {
        //                    string[] tujuan = data.listTujuan.Split('|');
        //                    int x = 1;
        //                    sql = string.Format(@"
        //                        DELETE {0}.TBLDRAFTSURATTUJUAN
        //                        WHERE DRAFTCODE = '{1}'
        //                        ", skema, data.DraftCode);
        //                    ctx.Database.ExecuteSqlCommand(sql);
        //                    foreach (var t in tujuan)
        //                    {
        //                        sql = string.Format(@"
        //                        INSERT INTO {0}.TBLDRAFTSURATTUJUAN (DRAFTCODE, URUTAN, NAMA, TEMBUSAN)
        //                        VALUES ('{1}',{2},'{3}','0')
        //                        ", skema, data.DraftCode, x, t, data.UserPembuat);
        //                        ctx.Database.ExecuteSqlCommand(sql);
        //                        x += 1;
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(data.LampiranId))
        //                {
        //                    sql = string.Format("SELECT COUNT(1) FROM {0}.TBLLAMPIRANDRAFTSURAT WHERE LAMPIRANID = '{1}'", skema, data.LampiranId);
        //                    if (ctx.Database.SqlQuery<int>(sql).FirstOrDefault() == 0)
        //                    {
        //                        sql = string.Format(@"
        //                            INSERT INTO {0}.TBLLAMPIRANDRAFTSURAT (LAMPIRANID, DRAFTCODE, STATUS, UPDTIME, UPDUSER)
        //                            VALUES ('{1}','{2}','A',SYSDATE,'{3}')
        //                            ", skema, data.LampiranId, data.DraftCode, data.UserPembuat);
        //                        ctx.Database.ExecuteSqlCommand(sql);
        //                    }
        //                    else
        //                    {
        //                        sql = string.Format(@"
        //                            UPDATE {0}.TBLLAMPIRANDRAFTSURAT SET 
        //                                STATUS = 'A', 
        //                                UPDTIME = SYSDATE, 
        //                                UPDUSER = '{3}'
        //                            WHERE LAMPIRANID = '{1}' AND DRAFTCODE = '{2}'
        //                            ", skema, data.LampiranId, data.DraftCode, data.UserPembuat);
        //                        ctx.Database.ExecuteSqlCommand(sql);
        //                    }
        //                }

        //                if (System.Web.Mvc.OtorisasiUser.isTU() && data.TTE != null && data.TTE.Count > 0)
        //                {
        //                    sql = string.Format(@"
        //                        DELETE {0}.TBLPENANDATANGANDRAFTSURAT
        //                        WHERE DRAFTCODE = '{1}'
        //                        ", skema, data.DraftCode);
        //                    ctx.Database.ExecuteSqlCommand(sql);
        //                    foreach (var tte in data.TTE)
        //                    {
        //                        sql = string.Format(@"
        //                        INSERT INTO {0}.TBLPENANDATANGANDRAFTSURAT (DRAFTCODE, USERID, TIPE, URUT, STATUS, UPDTIME, UPDUSER)
        //                        VALUES ('{1}','{2}','{3}',{4},'W',SYSDATE,'{5}')
        //                        ", skema, data.DraftCode, tte.PenandatanganId, tte.Tipe, tte.Urut, data.UserPembuat);
        //                        ctx.Database.ExecuteSqlCommand(sql);
        //                    }
        //                }

        //                tc.Commit();
        //                tr.Status = true;
        //                tr.Pesan = data.DraftCode;
        //            }
        //            catch (Exception ex)
        //            {
        //                tc.Rollback();
        //                tr.Pesan = ex.Message.ToString();
        //            }
        //            finally
        //            {
        //                tc.Dispose();
        //                ctx.Dispose();
        //            }
        //        }
        //    }

        //    return tr;
        //}

        public string apiUrl(DateTime dokdate)
        {
            string url = (DateTime.Compare(dokdate, Convert.ToDateTime("19/11/2020")) > 0) ? "ServiceEofficeUrl" : "ServiceBaseUrl";
            return url;
        }

        public DateTime GetServerDate()
        {
            DateTime retval = DateTime.Now;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string result = ctx.Database.SqlQuery<string>("SELECT to_char(sysdate,'DD/MM/YYYY HH24:MI') FROM DUAL").FirstOrDefault<string>();
                        retval = Convert.ToDateTime(result);
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return retval;
        }

        public DateTime getTglSunting(string id)
        {
            DateTime result = DateTime.Now;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    //cek exists
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT COUNT(*) FROM KONTENAKTIF WHERE KONTENAKTIFID = :id";
                    lstparams.Add(new OracleParameter("id", id));
                    var parameters = lstparams.ToArray();
                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                    {
                        lstparams = new List<object>();
                        sql = "SELECT TANGGALSUNTING FROM KONTENAKTIF WHERE KONTENAKTIFID = :id";
                        lstparams.Add(new OracleParameter("id", id));
                        parameters = lstparams.ToArray();
                        result = ctx.Database.SqlQuery<DateTime>(sql, parameters).FirstOrDefault();
                    }
                }
            }
            return result;
        }

        public List<UserTTE> getPenandatanganDraft(string code)
        {
            var list = new List<UserTTE>();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        List<object> lstparams = new List<object>();
                        string sql = string.Format(@"
                        SELECT 
	                        USERID AS PenandatanganId,
	                        URUT,
	                        TIPE
                        FROM {0}.TBLPENANDATANGANDRAFTSURAT
                        WHERE
                          DRAFTCODE = :DraftCode
                        ORDER BY URUT", System.Web.Mvc.OtorisasiUser.NamaSkema);
                        lstparams.Add(new OracleParameter("DraftCode", code));
                        var parameters = lstparams.ToArray();
                        list = ctx.Database.SqlQuery<UserTTE>(sql, parameters).ToList();
                    }
                    catch
                    {
                        list = new List<UserTTE>();
                    }
                }
            }

            return list;
        }

        public TransactionResult SimpanPengajuanDariDraft(DraftSurat data, string dokid, string nama, string kantorid, string tipe)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        data.SifatSurat = string.IsNullOrEmpty(data.SifatSurat) ? "Biasa" : data.SifatSurat;
                        string tipesurat = new SuratModel().GetKodeTipeSurat(data.TipeSurat);
                        string nomorsurat = tipesurat.Replace("<kode>", "."+data.KopSurat);
                        string sql = string.Format(@"
                            INSERT INTO {0}.TBLDOKUMENELEKTRONIK (DOKUMENELEKTRONIKID, NOMORSURAT, TANGGALSURAT, PERIHAL, SIFATSURAT, USERPEMBUAT, NAMAFILE, KANTORID, HALAMANTTE)
                            VALUES ('{1}','{2}',TO_DATE ('{3}', 'DD/MM/YYYY HH24:MI:SS'),'{4}','{5}','{6}','{7}','{8}','{9}')
                            ", skema, dokid, nomorsurat, Convert.ToDateTime(data.TanggalDibuat).ToString(@"dd'/'MM'/'yyyy"), data.Perihal, data.SifatSurat, data.UserPembuat, string.Concat(data.DraftCode, "|", data.TipeSurat), kantorid, data.PosisiTTE);
                        if (data.Status == "A")
                        {
                            sql = string.Format(@"
                            INSERT INTO {0}.TBLDOKUMENELEKTRONIK (DOKUMENELEKTRONIKID, NOMORSURAT, TANGGALSURAT, PERIHAL, SIFATSURAT, USERPEMBUAT, NAMAFILE, KANTORID, HALAMANTTE, TANGGALSETUJU, USERSETUJU)
                            VALUES ('{1}','{2}',TO_DATE ('{3}', 'DD/MM/YYYY HH24:MI:SS'),'{4}','{5}','{6}','{7}','{8}','{9}', SYSDATE, '{6}')
                            ", skema, dokid, nomorsurat, Convert.ToDateTime(data.TanggalDibuat).ToString(@"dd'/'MM'/'yyyy"), data.Perihal, data.SifatSurat, data.UserPembuat, string.Concat(data.DraftCode,"|",data.TipeSurat), kantorid, data.PosisiTTE);
                        }
                        ctx.Database.ExecuteSqlCommand(sql);
                        if (data.TTE != null)
                        {
                            foreach (var i in data.TTE)
                            {
                                sql = string.Format(@"
                                INSERT INTO {0}.TBLDOKUMENTTE (DOKUMENELEKTRONIKID, USERPENANDATANGAN, URUT, TIPE)
                                VALUES ('{1}','{2}',{3}, '{4}')
                                ", skema, dokid, i.PenandatanganId, i.Urut, i.Tipe);
                                ctx.Database.ExecuteSqlCommand(sql);
                            }

                            sql = string.Format(@"
                                UPDATE {0}.TBLDOKUMENTTE SET STATUS = 'W'
                                WHERE DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0' AND STATUS = 'P' AND URUT = 1
                                ", skema, dokid);
                            ctx.Database.ExecuteSqlCommand(sql);

                            sql = string.Format(@"
                                UPDATE {0}.TBLDRAFTSURAT SET STATUS = 'A'
                                WHERE DRAFTCODE = '{1}'
                                ", skema, data.DraftCode);
                            ctx.Database.ExecuteSqlCommand(sql);

                            //cek konten                        
                            int versi = CekVersi(dokid);
                            if (versi < 0)
                            {
                                sql = string.Format(@"
                                INSERT INTO KONTENAKTIF (KONTENAKTIFID,KONTEN,VERSI,TANGGALSISIP,PETUGASSISIP,TANGGALSUNTING,PETUGASSUNTING,TIPE,KANTORID,JUDUL,EKSTENSI)
                                VALUES ('{0}','{1}',0,SYSDATE,'{2}',SYSDATE,'{2}','{3}','{4}','{5}','{6}')
                                ", dokid, data.DraftCode, nama, tipe, kantorid, data.DraftCode, "pdf");
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            else
                            {
                                versi += 1;
                                //sql = string.Format("INSERT INTO KONTENPASIF SELECT SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI FROM KONTENAKTIF WHERE KONTENAKTIFID = '{0}'", dokid);
                                sql = string.Format(@"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = '{0}'", dokid);
                                ctx.Database.ExecuteSqlCommand(sql);

                                sql = string.Format("UPDATE KONTENAKTIF SET KONTEN = '{0}', VERSI = {1}, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = '{2}', JUDUL = '{4}', EKSTENSI = '{5}  WHERE KONTENAKTIFID = '{3}'", data.DraftCode, versi, nama, dokid, data.DraftCode, "pdf");
                                ctx.Database.ExecuteSqlCommand(sql);
                            }

                            tc.Commit();
                            //tc.Rollback();
                            tr.Status = true;
                            tr.ReturnValue = dokid;
                            tr.Pesan = "Dokumen Elektronik No. Konsep " + data.DraftCode + " berhasil dibuat.";
                        }
                        else
                        {
                            tc.Rollback();
                            tr.Pesan = "Penandatangan tidak ditemukan";
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

        //public string getPenomoranSurat(string id, string unit, DateTime dt)
        //{
        //    string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
        //    string nomor = string.Empty;
        //    string query = string.Empty;
        //    using (var ctx = new BpnDbContext())
        //    {
        //        try
        //        {
        //            var data = GetDokumenElektronik(id);
        //            var str = data.NamaFile.Split('|');
        //            if(str.Length == 2)
        //            {
        //                string code = str[0];
        //                string tipe = str[1];
        //                string strBulan = Functions.NomorRomawi(dt.Month);
        //                string strTahun = dt.Year.ToString();
        //                var draft = GetDraftSurat(code, unit);
        //                query = string.Format(@"SELECT NVL(MAX(TIPESURAT),0)+1 AS NOMOR FROM {0}.KONTERSURAT WHERE KANTORID = '{1}' AND TAHUN = '{2}' AND TIPESURAT = '{3}'", skema, unit, strTahun, tipe);
        //                nomor = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
        //                string template = GetKodeTipeSurat(tipe);
        //                template.Replace("<nomor>", nomor).Replace("<kode>", "-"+draft.KodeArsip).Replace("<bulan>", strBulan).Replace("<tahun>", strTahun);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string msg = ex.Message;
        //        }
        //    }

        //    return nomor;
        //}

        public TransactionResult SimpanFieldSurat(string id, string nomorsurat, DateTime tanggal)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDOKUMENELEKTRONIK SET NOMORSURAT = '{2}', TANGGALSURAT = TO_DATE ('{3}', 'DD/MM/YYYY HH24:MI:SS') WHERE DOKUMENELEKTRONIKID = '{1}'", skema, id, nomorsurat, tanggal.ToString(@"dd'/'MM'/'yyyy"));
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Nomor dan Tanggal Surat Berhasil disimpan";
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

        public List<PengajuanAkses> GetListPengajuanAkses(CariPengajuanAkses f, int from, int to)
        {
            var records = new List<PengajuanAkses>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string query = string.Format(@"
                SELECT 
                  ROW_NUMBER() OVER (ORDER BY RST.TANGGALDIBUAT) AS RNUMBER, 
                  COUNT(1) OVER() TOTAL, RST.*
                FROM 
                  (SELECT 
                     PA.PERSETUJUANID, PA.PROFILEID, TO_CHAR(PA.TANGGALDIBUAT, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT, PA.TIPE, NVL(PG.PEGAWAIID,PN.NIK) AS PEGAWAIID, 
                     NVL(PG.NAMA,PN.NAMA) AS NAMAPEGAWAI, DECODE(PGW.PEGAWAIID,NULL,PNW.NIK||' - '||PNW.NAMA,PGW.PEGAWAIID||' - '||PGW.NAMA) AS PENGAJU, PA.STATUS
                   FROM {0}.PERSETUJUANAKSES PA
                     INNER JOIN JABATANPEGAWAI JP ON
                       JP.PROFILEID = PA.PROFILEID AND
                       JP.PEGAWAIID = :param1 AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0' 
                     INNER JOIN JABATAN P ON
                       P.PROFILEID = PA.PROFILEID AND
	                   NVL(P.UNITKERJAID,'020116') = DECODE(PA.PROFILEID,'A80100',NVL(P.UNITKERJAID,'020116'),:param2)
                     INNER JOIN {0}.AKSESKKP KKP ON
                       KKP.PERSETUJUANID = PA.PERSETUJUANID AND
                       (KKP.VALIDSAMPAI IS NULL OR TRUNC(CAST(KKP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(KKP.STATUSHAPUS,'0') = '0' 
                    LEFT JOIN PEGAWAI PGW ON
                      PGW.USERID = PA.USERPEMBUAT
                    LEFT JOIN PPNPN PNW ON
                      PNW.USERID = PA.USERPEMBUAT
                     LEFT JOIN PEGAWAI PG ON
                       PG.USERID = KKP.USERID 
                     LEFT JOIN PPNPN PN ON
                       PN.USERID = KKP.USERID 
                   WHERE
                     PA.STATUS = 'W' AND
                     NVL(PA.STATUSHAPUS,'0') = '0' 
                   GROUP BY 
                     PA.PERSETUJUANID, PA.PROFILEID, PA.TANGGALDIBUAT, PA.TIPE, NVL(PG.PEGAWAIID,PN.NIK), 
                     NVL(PG.NAMA,PN.NAMA), DECODE(PGW.PEGAWAIID,NULL,PNW.NIK||' - '||PNW.NAMA,PGW.PEGAWAIID||' - '||PGW.NAMA), PA.STATUS)RST", skema);
            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("param1", f.PegawaiId));
            arrayListParameters.Add(new OracleParameter("param2", f.UnitKerjaId));

            if (!string.IsNullOrEmpty(f.MetaData))
            {
                //query += string.Format(" WHERE LOWER(RST.PEGAWAIID || RST.NAMAPEGAWAI || RST.TIPE) LIKE '%{0}%' ", f.MetaData.ToLower());
                query += " WHERE LOWER(RST.PEGAWAIID || RST.NAMAPEGAWAI || RST.TIPE) LIKE :param3 ";
                arrayListParameters.Add(new OracleParameter("param3", string.Concat("%",f.MetaData.ToLower(), "%")));
            }
            if (from + to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                arrayListParameters.Add(new OracleParameter("pStart", from));
                arrayListParameters.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<PengajuanAkses>(query, parameters).ToList();
            }

            return records;
        }

        public List<PengajuanAkses> GetListPengajuanPelaksana(CariPengajuanAkses f, int from, int to)
        {
            var records = new List<PengajuanAkses>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string query = string.Format(@"
                SELECT
                  ROW_NUMBER() OVER (ORDER BY RST.TANGGALDIBUAT) AS RNUMBER,
                  COUNT(1) OVER() TOTAL, RST.*
                FROM
                  (SELECT
                     PA.PENGAJUANID AS PERSETUJUANID, PA.PROFILEID,
                     TO_CHAR(PA.TANGGALPENGAJUAN, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                     JB.NAMA||' ('|| DECODE(PA.STATUSPLT,1,'PLT',2,'PLH','Definitif')||')' AS TIPE,
                     PG.PEGAWAIID, PG.NAMA AS NAMAPEGAWAI,
                     DECODE(PGW.PEGAWAIID,NULL,PNW.NIK||' - '||PNW.NAMA,PGW.PEGAWAIID||' - '||PGW.NAMA) AS PENGAJU,
                     PA.STATUS
                   FROM {0}.PENGAJUANJABATANPETUGAS PA
                     INNER JOIN JABATANPEGAWAI JP ON
                       (JP.PROFILEID = PA.PROFILEIDPERSETUJUAN OR JP.PEGAWAIID = PA.PEGAWAIIDPERSETUJUAN) AND
                       JP.PEGAWAIID = :param1 AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     INNER JOIN JABATAN JB ON
                       JB.PROFILEID = PA.PROFILEID
                     LEFT JOIN PEGAWAI PGW ON
                       PGW.USERID = PA.USERPEMBUAT
                     LEFT JOIN PPNPN PNW ON
                       PNW.USERID = PA.USERPEMBUAT
                     INNER JOIN PEGAWAI PG ON
                       PG.PEGAWAIID = PA.PEGAWAIID
                   WHERE
                     PA.STATUS = 'W' AND
                     (PA.VALIDSAMPAI IS NULL OR TRUNC(CAST(PA.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                     NVL(PA.STATUSHAPUS,'0') = '0'
                   GROUP BY
                     PA.PENGAJUANID, PA.PROFILEID, PA.TANGGALPENGAJUAN,
                     JB.NAMA||' ('|| DECODE(PA.STATUSPLT,1,'PLT',2,'PLH','Definitif')||')',
                     PG.PEGAWAIID, PG.NAMA,
                     DECODE(PGW.PEGAWAIID,NULL,PNW.NIK||' - '||PNW.NAMA,PGW.PEGAWAIID||' - '||PGW.NAMA),
                     PA.STATUS)RST", skema);
            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("param1", f.PegawaiId));

            if (!string.IsNullOrEmpty(f.MetaData))
            {
                query += " WHERE LOWER(RST.PEGAWAIID || RST.NAMAPEGAWAI || RST.TIPE) LIKE :param2 ";
                arrayListParameters.Add(new OracleParameter("param2", string.Concat("%", f.MetaData.ToLower(), "%")));
            }
            if (from + to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                arrayListParameters.Add(new OracleParameter("pStart", from));
                arrayListParameters.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<PengajuanAkses>(query, parameters).ToList();
            }

            return records;
        }

        public TransactionResult UpdateVersiDokumen(string id, int versi, string nama)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();

                        int _versi = CekVersi(id);
                        if (versi < 0)
                        {
                            tr.Pesan = "Gagal Mendapatkan Konten Aktif";
                            tc.Rollback();
                            return tr;
                        }else if(!_versi.Equals(versi))
                        {
                            string sql = @"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = :param1";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", id));
                            ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());

                            sql = "UPDATE KONTENAKTIF SET VERSI = :param1, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :param2 WHERE KONTENAKTIFID = :param3";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", versi));
                            lstparams.Add(new OracleParameter("param2", nama));
                            lstparams.Add(new OracleParameter("param3", id));
                            ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Proses Berhasil";
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
    }
}