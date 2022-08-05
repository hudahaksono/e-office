﻿using Surat.Codes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class PersuratanModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();

        public string GetUID()
        {
            string id = "";

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return id;
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

        public int GetServerYear()
        {
            int retval = DateTime.Now.Year;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string result = ctx.Database.SqlQuery<string>("SELECT to_char(sysdate,'YYYY') FROM DUAL").FirstOrDefault<string>();
                        retval = Convert.ToInt32(result);
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return retval;
        }

        public List<Models.Entities.TipeSurat> GetTipeSurat()
        {
            var list = new List<Models.Entities.TipeSurat>();

            string query =
                "SELECT " +
                "   tipesurat.nama NamaTipeSurat, tipesurat.formatnomor " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkemaMaster + ".tipesurat " +
                "WHERE tipesurat.aktif = 1 ORDER BY tipesurat.urutan";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.TipeSurat>(query).ToList<Models.Entities.TipeSurat>();
            }

            return list;
        }

        public List<Models.Entities.SifatSurat> GetSifatSurat()
        {
            var list = new List<Models.Entities.SifatSurat>();

            string query =
                "SELECT " +
                "   sifatsurat.nama NamaSifatSurat " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkemaMaster + ".sifatsurat " +
                "WHERE sifatsurat.aktif = 1 ORDER BY sifatsurat.urutan";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.SifatSurat>(query).ToList<Models.Entities.SifatSurat>();
            }

            return list;
        }



        public string GetKodeIdentifikasi(string unitkerjaid)
        {
            string result = "";

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                string query = "SELECT kode FROM unitkerja WHERE unitkerjaid = :UnitKerjaId";

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
            }

            return result;
        }

        public string GetJumlahLampiranSurat(string suratid)
        {
            string result = "0";

            string query =
                "SELECT count(*) " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat " +
                "WHERE suratid = :SuratId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = Convert.ToString(jumlahrecord);
                }
            }

            return result;
        }

        public string GetStatusForwardTU(string suratinboxid)
        {
            string result = "0";

            string query = "SELECT to_char(statusforwardtu) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinboxid = :SuratInboxId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetCatatanSebelumnya(string suratinboxid)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("id-ID");
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("id-ID");

            string result = "";

            // Get Tanggal Kirim
            string query = "SELECT tanggalkirim FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutboxid IN (SELECT suratoutboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi WHERE suratinboxid = :SuratInboxId)";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                DateTime tanggalkirim = ctx.Database.SqlQuery<DateTime>(query, parameters).First();

                //DateTime tanggal = Convert.ToDateTime(tanggalkirim);

                result += "<span style='font-size:9pt; color:Navy'><u>" + tanggalkirim.ToString("dddd, d MMMM yyyy") + "</u></span>";

                // Get Pengirim
                query =
                     "SELECT " +
                     "      decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                     "      decode(pegawai.nama, '', '', pegawai.nama) || " +
                     "      decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap " +
                     " FROM pegawai WHERE pegawaiid = " +
                     "             (SELECT nip FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutboxid IN " +
                     "                        (SELECT suratoutboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi WHERE suratinboxid = :SuratInboxId1)) " +
                     "UNION " +
                     "SELECT " +
                     "      ppnpn.nama AS NamaLengkap " +
                     " FROM ppnpn WHERE nik = " +
                     "             (SELECT nip FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutboxid IN " +
                     "                        (SELECT suratoutboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi WHERE suratinboxid = :SuratInboxId2)) ";
                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId1", suratinboxid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId2", suratinboxid));
                parameters = arrayListParameters.OfType<object>().ToArray();
                string namapengirim = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();

                // Get Keterangan
                query = "SELECT keterangan FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutboxid IN (SELECT suratoutboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi WHERE suratinboxid = :SuratInboxId)";
                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                parameters = arrayListParameters.OfType<object>().ToArray();
                string keterangan = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();


                #region Bila tidak ada, ambil dari suratinbox urutan sebelumnya

                if (string.IsNullOrEmpty(keterangan))
                {
                    // Get Pengirim
                    query =
                         "SELECT " +
                         "     decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                         "     decode(pegawai.nama, '', '', pegawai.nama) || " +
                         "     decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap " +
                         " FROM pegawai WHERE pegawaiid IN " +
                         "         (SELECT sinbox1.nip FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox sinbox1 " +
                         "                 JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox sinbox2 ON sinbox2.suratid = sinbox1.suratid " +
                         "                      AND sinbox2.suratinboxid = :SuratInboxId1 " +
                         "                      AND sinbox1.urutan = (sinbox2.urutan-1)) " +
                         "UNION " +
                         "SELECT " +
                         "     ppnpn.nama AS NamaLengkap " +
                         " FROM ppnpn WHERE nik IN " +
                         "         (SELECT sinbox1.nip FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox sinbox1 " +
                         "                 JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox sinbox2 ON sinbox2.suratid = sinbox1.suratid " +
                         "                      AND sinbox2.suratinboxid = :SuratInboxId2 " +
                         "                      AND sinbox1.urutan = (sinbox2.urutan-1)) ";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId1", suratinboxid));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId2", suratinboxid));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    namapengirim = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();

                    // Get Keterangan
                    query =
                         "SELECT sinbox1.keterangan FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox sinbox1 " +
                         "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox sinbox2 ON sinbox2.suratid = sinbox1.suratid " +
                         "             AND sinbox2.suratinboxid = :SuratInboxId " +
                         "             AND sinbox1.urutan = (sinbox2.urutan-1)";
                    //query = sWhitespace.Replace(query, " ");
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    keterangan = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }

                #endregion


                if (!string.IsNullOrEmpty(keterangan))
                {
                    result += "<br />" + "<div style='color:#dd6136'>" + namapengirim + ":</div>" + keterangan;
                }
                else
                {
                    result += "<br />-";
                }
            }

            return result;
        }

        public string GetDisposisiSebelumnya(string suratinboxid)
        {
            string result = "";

            if (!string.IsNullOrEmpty(suratinboxid))
            {
                string query =
                    "SELECT perintahdisposisi FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox " +
                    "JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ON suratoutboxrelasi.suratoutboxid = suratoutbox.suratoutboxid " +
                    "AND suratoutboxrelasi.suratinboxid = :SuratInboxId";

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
            }

            return result;
        }

        public string GetSuratInboxIdFirstRow(string suratid)
        {
            string result = "";

            if (!string.IsNullOrEmpty(suratid))
            {
                //string query =
                //    "select suratinboxid from suratinbox " + 
                //    "where suratid = :SuratId and statusforwardtu = 1 and statusterkirim = 1 and rownum = 1 " + 
                //    "order by tanggalkirim";
                string query =
                    "select suratinboxid from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                    "where suratid = :SuratId and urutan = 1 " +
                    "order by tanggalkirim";

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
            }

            return result;
        }

        public int GetMaxUrutanSuratInbox(BpnDbContext ctx, string suratid)
        {
            int result = 0;

            string query = "select max(urutan)+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox where suratid = :SuratId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratid));

            object[] parameters = arrayListParameters.OfType<object>().ToArray();
            result = ctx.Database.SqlQuery<int>(query, parameters).First();

            return result;
        }

        public bool IsSuratDiprosesTU(string unitkerjaid)
        {
            bool result = true; // default true

            string query = "";
            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                // Cek dulu apakah ada datanya atau tidak
                query =
                    "SELECT count(*) FROM settingsatker " +
                    "WHERE  satkerid = :UnitKerjaId";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrow = ctx.Database.SqlQuery<int>(query, parameters).First();

                if (jumlahrow > 0)
                {
                    query = "SELECT nvl(prosestu, 0) FROM settingsatker WHERE satkerid = :UnitKerjaId";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

                    parameters = arrayListParameters.OfType<object>().ToArray();
                    int icek = ctx.Database.SqlQuery<int>(query, parameters).FirstOrDefault();

                    if (icek == 0)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool IsNomorSuratDuplikat(string nomorsurat)
        {
            bool result = false; // default false

            string query = "";
            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                query =
                    "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                    "WHERE  UPPER(nomorsurat) = :NomorSurat";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", nomorsurat.ToUpper()));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrow = ctx.Database.SqlQuery<int>(query, parameters).First();

                if (jumlahrow > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public int JumlahSurat(string satkerid, string nip, string myProfiles, string arah)
        {
            int result = 0;

            if (string.IsNullOrEmpty(myProfiles))
            {
                myProfiles = "'xx'";
            }

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT COUNT(1)
                FROM {0}.SURATINBOX SI
                  JOIN {0}.SURAT S ON
                    S.SURATID = SI.SURATID
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = SI.PROFILEPENERIMA AND
	                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
                  SI.STATUSTERKIRIM = 0 AND
                  SI.STATUSTERKUNCI = 0 and
                  SI.STATUSFORWARDTU = 0 AND
                  SI.URUTAN > 1 AND
                  NVL(SI.statushapus,'0') = '0' AND
                  SI.NIP = :Nip AND
                  SI.PROFILEPENERIMA IN ({1}) AND
                  SI.TINDAKLANJUT <> 'Selesai' AND
                  NOT EXISTS
                    (SELECT 1
                     FROM {0}.ARSIPSURAT
                     WHERE
                       SURATID = S.SURATID AND
                       KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID))", System.Web.Mvc.OtorisasiUser.NamaSkema, myProfiles);
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));

            if (!string.IsNullOrEmpty(arah))
            {
                if (arah.Equals("Inisiatif"))
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kategori", arah));
                    query += " AND (S.ARAH = :Arah OR S.KATEGORI = :Kategori)";
                }
                else
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                    query += " AND S.ARAH = :Arah AND S.KATEGORI <> 'Inisiatif' ";
                }
            }


            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public Entities.jumlahKotakSurat JumlahSuratBySumber(string satkerid, string nip, string myProfiles, string arah)
        {
            var result = new Entities.jumlahKotakSurat();

            if (string.IsNullOrEmpty(myProfiles))
            {
                myProfiles = "'xx'";
            }

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT
                    COUNT(
                        CASE WHEN SS.SUMBER_KETERANGAN = 'Email' THEN 1 END
                    ) AS EMAIL,
                    COUNT(
                        CASE WHEN SS.SUMBER_KETERANGAN = 'Loket' THEN 1 END
                    ) AS LOKET,
                    COUNT(
                        CASE WHEN SS.SUMBER_KETERANGAN IS NULL THEN 1 END
                    ) AS INTERNAL
                FROM {0}.SURATINBOX SI
                  JOIN {0}.SURAT S ON
                    S.SURATID = SI.SURATID
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = SI.PROFILEPENERIMA AND
	                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID
                  LEFT JOIN {0}.SUMBER_SURAT SS ON
                    SS.SURAT_ID = SI.SURATID
                WHERE
                  SI.STATUSTERKIRIM = 0 AND
                  SI.STATUSTERKUNCI = 0 and
                  SI.STATUSFORWARDTU = 0 AND
                  SI.URUTAN > 1 AND
                  NVL(SI.statushapus,'0') = '0' AND
                  SI.NIP = :Nip AND
                  SI.PROFILEPENERIMA = :profileid AND
                  SI.TINDAKLANJUT <> 'Selesai' AND
                  NOT EXISTS
                    (SELECT 1
                     FROM {0}.ARSIPSURAT
                     WHERE
                       SURATID = S.SURATID AND
                       KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID))", System.Web.Mvc.OtorisasiUser.NamaSkema);
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("profileid", myProfiles));

            if (!string.IsNullOrEmpty(arah))
            {
                if (arah.Equals("Inisiatif"))
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kategori", arah));
                    query += " AND (S.ARAH = :Arah OR S.KATEGORI = :Kategori)";
                }
                else
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                    query += " AND S.ARAH = :Arah AND S.KATEGORI <> 'Inisiatif' ";
                }
            }


            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<Entities.jumlahKotakSurat>(query, parameters).FirstOrDefault();
            }

            return result;
        }


        public int JumlahSuratBelumDibuka(string satkerid, string nip, string myProfiles)
        {
            int result = 0;

            if (string.IsNullOrEmpty(myProfiles))
            {
                myProfiles = "'xx'";
            }

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT count(*) " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                //"    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                "WHERE " +
                "    statusterkirim = 0 AND statusterkunci = 0 and statusforwardtu = 0 " +
                "    AND suratinbox.tanggalbuka IS null " +
                "    AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "    AND (nip = :Nip OR nip is null) " +
                "    AND suratinbox.profilepenerima IN (" + myProfiles + ") " +
                "    AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId)";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public int JumlahProsesSurat(string nip, string satkerid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "       JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                //"       JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                //"       JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "WHERE  suratinbox.statusterkirim = 0 " +
                "       AND suratinbox.statusterkunci = 0 " +
                "       AND suratinbox.statusforwardtu = 1 " +
                "       AND NVL(suratinbox.statushapus,'0') = '0' " +
                "       AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId) " +
                "       AND suratinbox.nip = :Nip";
            //"       AND (suratinbox.nip IS NULL OR suratinbox.nip = :Nip)";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public List<Models.Entities.PerintahDisposisi> GetPerintahDisposisi()
        {
            var list = new List<Models.Entities.PerintahDisposisi>();

            string query =
                "SELECT " +
                "   perintahdisposisi.nama NamaPerintahDisposisi " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".perintahdisposisi " +
                "WHERE perintahdisposisi.aktif = 1 ORDER BY perintahdisposisi.urutan";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.PerintahDisposisi>(query).ToList<Models.Entities.PerintahDisposisi>();
            }

            return list;
        }

        public List<Models.Entities.KodeKlasifikasi> GetKodeKlasifikasi()
        {
            var list = new List<Models.Entities.KodeKlasifikasi>();

            string query =
                "SELECT " +
                "   kode, nama " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".tipekegiatan " +
                "ORDER BY to_number(kode)";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.KodeKlasifikasi>(query).ToList<Models.Entities.KodeKlasifikasi>();
            }

            return list;
        }

        public Models.Entities.DelegasiSurat GetDelegasiSurat(string profilepengirim)
        {
            Models.Entities.DelegasiSurat data = new Models.Entities.DelegasiSurat();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    delegasisurat.delegasisuratid, delegasisurat.profilepengirim, delegasisurat.profilepenerima, " +
                "    to_char(delegasisurat.tanggal, 'dd/mm/yyyy') tanggal, delegasisurat.status, " +
                "    pegawai.pegawaiid AS NIPPenerima, NVL(pegawai.namaalias, pegawai.nama) NamaPenerima " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".delegasisurat " +
                "    JOIN jabatanpegawai ON jabatanpegawai.profileid = delegasisurat.profilepenerima " +
                "    JOIN pegawai ON pegawai.pegawaiid = jabatanpegawai.pegawaiid " +
                "WHERE " +
                "    delegasisurat.profilepengirim = :ProfilePengirim " +
                "    AND delegasisurat.status = 1 ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfilePengirim", profilepengirim));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Models.Entities.DelegasiSurat>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public Models.Entities.ReferensiSurat GetReferensiSurat(string referensi, string satkerid = null)
        {
            Models.Entities.ReferensiSurat data = new Models.Entities.ReferensiSurat();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.referensi " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "WHERE " +
                "    surat.referensi = :Referensi";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Referensi", referensi));

            if (!string.IsNullOrEmpty(satkerid))
            {
                query += " AND surat.kantorid = :SatkerId";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Models.Entities.ReferensiSurat>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public Models.Entities.Surat GetSuratBySuratInboxId(string suratinboxid, string satkerid, string suratid, string sumber = null)
        {
            Models.Entities.Surat records = new Models.Entities.Surat();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, surat.pengirim PengirimSurat, surat.penerima PenerimaSurat, " +
                //"    NVL(agendasurat.nomoragenda, surat.nomoragenda) NomorAgendaSurat, " +
                //"    agendasurat.nomoragenda NomorAgendaSurat, " +
                "    surat.nomoragenda NomorAgendaSurat, " +
                "    to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "    to_char(surat.tanggalsurat, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalSurat, " +
                "    to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "    to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "    to_char(surat.tanggalproses, 'dd Month yyyy', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "    to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "    to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "    to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "    to_char(agendasurat.targetselesai, 'dd-mm-yyyy') TargetSelesaiSuratMasuk, " +
                "    to_char(agendasurat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesaiSuratMasuk, " +
                "    to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "    to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "    to_char(suratinbox.tanggalkirim, 'dd Mon yyyy HH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "    to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy HH24:MI') tanggalterima, " +
                "    to_char(suratinbox.tanggalkirim, 'Day, dd Mon yyyy HH24:MI', 'nls_date_language=INDONESIAN') TanggalInbox, " +
                "    surat.kategori, surat.arah, 'Surat ' || surat.arah ArahSurat, surat.tipesurat, surat.sifatsurat, surat.jumlahlampiran, " +
                "    surat.isisingkat IsiSingkatSurat, surat.statussurat, surat.referensi ReferensiSurat, surat.keterangansurat, " +
                "    decode(arsipsurat.arsipsuratid, null, 0, 1) AS StatusArsip, " +
                "    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy') TanggalTerimaFisik, " +
                "    decode(suratinbox.tanggalterima, null, 'Tidak', 'Ya') AS TerimaFisik, " +
                "    profiledari.nama AS NamaProfilePengirim, suratinbox.namapegawai AS NAMAPENERIMA, suratinbox.NIP as NIP, " +
                "    surat.keterangansurat || ', ' || suratinbox.redaksi AS KeteranganSuratRedaksi, " +
                "    suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.statusurgent, suratinbox.perintahdisposisi, " +
                "    suratinbox.profilepenerima, suratinbox.suratinboxid, suratinbox.keterangan CatatanAnda, suratinbox.urutan, " +
                "     sumber_surat.sumber_keterangan as sumber_keterangan " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ON arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerIdAgenda " +
                "    LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +

                (!string.IsNullOrEmpty(sumber) ? " INNER JOIN " : " LEFT JOIN ") + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratinbox.suratid " +
                (!string.IsNullOrEmpty(sumber) ? string.Format(" AND sumber_surat.sumber_keterangan = '{0}' ", sumber) : "") +


                //"    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratinbox.suratid " +
                "WHERE " +
                "    suratinbox.suratinboxid = :SuratInboxId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerIdAgenda", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));


            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).FirstOrDefault();
                //records.Sumber_Keterangan = ctx.Database.SqlQuery<string>($"SELECT SUMBER_KETERANGAN FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT WHERE SURAT_ID = '{suratid}'").FirstOrDefault();
            }

            return records;
        }

        public Models.Entities.Surat GetSuratBySuratId(string suratid, string satkerid)
        {
            Models.Entities.Surat records = new Models.Entities.Surat();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, surat.pengirim PengirimSurat, surat.penerima PenerimaSurat, surat.unitorganisasi Sumber_Keterangan, " +
                //"    NVL(agendasurat.nomoragenda, surat.nomoragenda) NomorAgendaSurat, " +
                //"    agendasurat.nomoragenda NomorAgendaSurat, " +
                //"    surat.unitorganisasi Sumber_Keterangan, " +
                "    surat.nomoragenda NomorAgendaSurat, " +
                "    to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "    to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "    to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "    to_char(surat.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "    to_char(surat.tanggalproses, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalTerimaCetak, " +
                "    to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "    to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "    to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "    to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "    to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                //"    to_char(suratinbox.tanggalkirim, 'dd/mm/yyyy HH24:MI:SS') tanggalkirim, " +
                "    to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "    to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                //"    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy HH24:MI:SS') tanggalterima, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "    to_char(suratinbox.tanggalkirim, 'Day, fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalInbox, " +
                "    to_char(surat.tanggalsurat, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalSurat, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalTerima, " +
                "    surat.kategori, surat.arah, 'Surat ' || surat.arah ArahSurat, surat.tipesurat, surat.sifatsurat, surat.jumlahlampiran, " +
                "    surat.isisingkat IsiSingkatSurat, surat.statussurat, surat.referensi ReferensiSurat, surat.keterangansurat, " +
                "    decode(arsipsurat.arsipsuratid, null, 0, 1) AS StatusArsip, " +
                "    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy') TanggalTerimaFisik, " +
                "    decode(suratinbox.tanggalterima, null, 'Tidak', 'Ya') AS TerimaFisik, " +
                "    profiledari.nama AS NamaProfilePengirim, profiledari.unitkerjaid AS ReferensiUnitKerjaId, " +
                "    surat.keterangansurat || ', ' || suratinbox.redaksi AS KeteranganSuratRedaksi, " +
                "    suratinbox.profilepenerima, suratinbox.suratinboxid, suratinbox.redaksi " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ON suratinbox.suratid = surat.suratid " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ON arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId " +
                //"    LEFT JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "    LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "WHERE " +
                "    surat.suratid = :SuratId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));


            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).FirstOrDefault();



                records.Sumber_Keterangan = ctx.Database.SqlQuery<string>($"SELECT SUMBER_KETERANGAN FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT WHERE SURAT_ID = '{suratid}'").FirstOrDefault();
                ////if (string.IsNullOrEmpty(check) && !string.IsNullOrEmpty(data.Sumber_Keterangan))
                //{
                //    ctx.Database.ExecuteSqlCommand($"INSERT INTO {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT (SUMBER_ID, SURAT_ID, SUMBER_KETERANGAN) VALUES (SYS_GUID(), '{data.SuratId}','{data.Sumber_Keterangan}' )");

                //else if (!string.IsNullOrEmpty(check))
                //{
                //    ctx.Database.ExecuteSqlCommand($"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT SET SUMBER_KETERANGAN = '{data.Sumber_Keterangan}'");
                //}



            }



            return records;
        }

        public Models.Entities.Surat GetSuratBySuratIdAndProfileTujuan(string suratid, string profileidtujuan)
        {
            Models.Entities.Surat records = new Models.Entities.Surat();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, surat.pengirim PengirimSurat, surat.penerima PenerimaSurat, " +
                "    surat.nomoragenda NomorAgendaSurat, " +
                "    to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "    to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "    to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "    to_char(surat.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "    to_char(surat.tanggalproses, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalTerimaCetak, " +
                "    to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "    to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "    to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "    to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "    to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "    to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "    to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "    to_char(suratinbox.tanggalkirim, 'Day, fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalInbox, " +
                "    to_char(surat.tanggalsurat, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalSurat, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalTerima, " +
                "    surat.kategori, surat.arah, 'Surat ' || surat.arah ArahSurat, surat.tipesurat, surat.sifatsurat, surat.jumlahlampiran, " +
                "    surat.isisingkat IsiSingkatSurat, surat.statussurat, surat.referensi ReferensiSurat, surat.keterangansurat, " +
                "    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy') TanggalTerimaFisik, " +
                "    decode(suratinbox.tanggalterima, null, 'Tidak', 'Ya') AS TerimaFisik, " +
                "    profiledari.nama AS NamaProfilePengirim, " +
                "    surat.keterangansurat || ', ' || suratinbox.redaksi AS KeteranganSuratRedaksi, " +
                "    suratinbox.profilepenerima, suratinbox.suratinboxid, suratinbox.redaksi " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ON suratinbox.suratid = surat.suratid AND suratinbox.profilepenerima = :ProfileIdTujuan " +
                "    LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "WHERE " +
                "    surat.suratid = :SuratId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTujuan", profileidtujuan));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).FirstOrDefault();
            }

            return records;
        }

        public string GetSuratIdFromNomorSurat(string nomorsurat)
        {
            string result = "";

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            if (string.IsNullOrEmpty(nomorsurat))
            {
                nomorsurat = "2db069933ac3";
            }

            string query =
                 "SELECT suratid " +
                 " FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                 " WHERE UPPER(nomorsurat) = :NomorSurat";

            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", nomorsurat.ToUpper()));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetSuratIdFromNomorSuratDanPengirim(string nomorsurat, string pengirim)
        {
            string result = "";

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            if (string.IsNullOrEmpty(nomorsurat))
            {
                nomorsurat = "2db069933ac3";
            }

            string query =
                 "SELECT suratid " +
                 " FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                 " WHERE UPPER(nomorsurat) = :NomorSurat AND UPPER(pengirim) = :PengirimSurat";

            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", nomorsurat.ToUpper()));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengirimSurat", pengirim.ToUpper()));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetSuratIdFromNomorAgenda(string nomoragenda, string satkerid)
        {
            string result = "";

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            if (string.IsNullOrEmpty(nomoragenda))
            {
                nomoragenda = "2db069933ac3";
            }

            //string query =
            //    @"SELECT suratid
            //        FROM agendasurat
            //        WHERE nomoragenda = :NomorAgenda AND kantorid = :SatkerId";
            string query =
                 "SELECT suratid " +
                 "   FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                 "   WHERE nomoragenda = :NomorAgenda";
            //query = sWhitespace.Replace(query, " ");

            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", nomoragenda));
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetSuratInboxIdFromSuratId(string suratid)
        {
            string result = "";

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT suratinboxid " +
                 "   FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                 "   WHERE suratid = :SuratId AND statusterkunci = 0 and statusterkirim = 1 and rownum = 1"; // statusforwardtu = 0 and 
            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public Models.Entities.SuratKembali GetSuratInboxForSuratKembali(string suratinboxid)
        {
            Models.Entities.SuratKembali records = new Models.Entities.SuratKembali();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    suratinboxsender.suratinboxid, suratinbox.profilepenerima AS ProfilePengirim, " +
                "    suratinbox.nip AS NipPengirim, suratinbox.namapegawai AS NamaPengirim, " +
                "    suratoutbox.suratoutboxid, suratoutbox.profilepengirim AS ProfilePenerima, " +
                "    suratoutbox.nip AS NipPenerima, suratinboxsender.namapegawai AS NamaPenerima " + //pegawai.nama AS NamaPenerima " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ON suratoutboxrelasi.suratinboxid = suratinbox.suratinboxid " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ON suratoutbox.suratoutboxid = suratoutboxrelasi.suratoutboxid " +
//                "    JOIN pegawai ON pegawai.pegawaiid = suratoutbox.nip " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox suratinboxsender ON suratinboxsender.suratid = suratinbox.suratid " +
                "         AND suratinboxsender.statusterkirim = 1 AND suratinboxsender.profilepenerima = suratoutbox.profilepengirim " +
                "WHERE " +
                "    suratinbox.suratinboxid = :SuratInboxId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratKembali>(query, parameters).FirstOrDefault();
            }

            return records;
        }

        public List<Models.Entities.Surat> GetListSurat(string satkerid, string myProfiles, string metadata, string statussurat, string kategorisurat, string nippenerima, string sumber, int from, int to, string pegawaiid = null)
        {
            List<Models.Entities.Surat> records = new List<Models.Entities.Surat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY surat.tanggalsurat DESC, surat.TanggalInput DESC, surat.tanggalproses DESC, surat.suratid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        NVL(agendasurat.nomoragenda,surat.nomoragenda) AS NomorAgendaSurat, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalproses, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, 'Surat ' || surat.arah ArahSurat, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, DECODE(surat.REFERENSI,NULL,NVL(surat.kategori,surat.arah),'Jawaban') AS kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, " +
                "        sumber_surat.sumber_keterangan as sumber_keterangan, " +


                //"        decode(arsipsurat.arsipsuratid, null, 'TRUE', 'FALSE') AS StatusArsip, " +
                "        decode(arsipsurat.arsipsuratid, null, 0, 1) AS StatusArsip, " +
                "        surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        DECODE(suratoutput.nomorsurat, NULL, '', suratoutput.tipesurat || ': ' || suratoutput.nomorsurat) " +
                "            || DECODE(suratoutput.nomorsurat, NULL, '', ', Perihal: ' || suratoutput.perihal) AS Output " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat suratoutput ON suratoutput.referensi = surat.suratid " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ON arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId2 " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = SURAT.SURATID " +
                "    WHERE " +
                "        surat.suratid IS NOT NULL ";
            //"        surat.kantorid = :SatkerId ";
            //"        EXISTS (SELECT 1 FROM suratoutbox WHERE suratoutbox.suratid = surat.suratid AND suratoutbox.kantorid = :SatkerId) ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId2", satkerid));

            if (!String.IsNullOrEmpty(statussurat))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusSurat", Convert.ToInt32(statussurat)));
                //query += " AND surat.statussurat = :StatusSurat ";
                if (statussurat == "1")
                {
                    query += " AND arsipsurat.arsipsuratid IS NULL ";
                }
                else if (statussurat == "0")
                {
                    query += " AND arsipsurat.arsipsuratid IS NOT NULL ";
                }
            }

            if (!String.IsNullOrEmpty(myProfiles))
            {
                query +=
                    " AND (EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.profilepenerima IN (" + myProfiles + ") " + (string.IsNullOrEmpty(pegawaiid)? "" : $"AND suratinbox.nip = '{pegawaiid}'") + " ) OR " +
                    "      EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutbox.suratid = surat.suratid AND suratoutbox.profilepengirim IN (" + myProfiles + ") " + (string.IsNullOrEmpty(pegawaiid) ? "" : $"AND suratoutbox.nip = '{pegawaiid}'") + " )) ";
            }

            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(utl_raw.cast_to_varchar2(surat.metadata))) LIKE :Metadata ";
                //query +=
                //    " AND (LOWER(APEX_UTIL.URL_ENCODE(utl_raw.cast_to_varchar2(surat.metadata))) LIKE :Metadata " +
                //    "      OR EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND LOWER(APEX_UTIL.URL_ENCODE(suratinbox.keterangan)) LIKE :CatatanAnda)) ";
            }

            //if (!String.IsNullOrEmpty(kategorisurat))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KategoriSurat", kategorisurat));
            //    query += " AND surat.kategori = :KategoriSurat ";
            //}

            if (!String.IsNullOrEmpty(nippenerima))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPenerima", nippenerima));
                query +=
                    " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.nip = :NipPenerima) ";
            }


            if (!String.IsNullOrEmpty(sumber))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Sumber_Keterangan", sumber));
                query += " AND sumber_surat.sumber_keterangan = :Sumber_Keterangan ";
            }

            if (!String.IsNullOrEmpty(kategorisurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KategoriSurat", kategorisurat));
                query += " AND DECODE(surat.REFERENSI,NULL,NVL(surat.kategori,surat.arah),'Jawaban') = :KategoriSurat ";
            }

            else
            {
                query += " AND DECODE(surat.REFERENSI,NULL,NVL(surat.kategori,surat.arah),'Jawaban') <> 'Inisiatif' ";
            }

            //if (!String.IsNullOrEmpty(type))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalInput", type));
            //    query += " AND surat.suratinput = :TanggalInput DESC ";
            //}

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).ToList<Models.Entities.Surat>();
            }

            return records;
        }



        public List<Models.Entities.SuratInbox> GetSuratInbox(string satkerid, string nip, string statussurat, string arah, string profileid, string metadata, string nomorsurat, string nomoragenda, string perihal, string tanggalsurat, string tipesurat, string sifatsurat, string sortby, string sorttype, string spesificprofileid, int from, int to, string sumber = null)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();

            ArrayList arrayListParameters = new ArrayList();

            // Default atau by SifatSurat
            string orderby = "suratinbox.statusurgent DESC, sifatsurat, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "SifatSurat")
                {
                    orderby = "suratinbox.statusurgent DESC, sifatsurat, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.statusurgent, sifatsurat DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                //else if (sortby == "JenisDisposisi")
                //{
                //    orderby = "sifatsurat.perintahdisposisi, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                //    if (!string.IsNullOrEmpty(sorttype))
                //    {
                //        if (sorttype == "DESC")
                //        {
                //            orderby = "sifatsurat.perintahdisposisi DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                //        }
                //    }
                //}
                else if (sortby == "TanggalTerima")
                {
                    orderby = "suratinbox.tanggalterima, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalterima DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalKirim")
                {
                    orderby = "suratinbox.tanggalkirim, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TargetSelesai")
                {
                    orderby = "surat.targetselesai, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "surat.targetselesai DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
            }

            string query =
                "SELECT * FROM ( " +
                "SELECT ROW_NUMBER() over (ORDER BY " + orderby.Replace("suratinbox.tanggalkirim", "tglkirim").Replace("surat.targetselesai", "tglselesai").Replace("suratinbox.tanggalterima", "tglterima").Replace("suratinbox.", "").Replace("surat.", "") + ") RNUMBER, COUNT(1) OVER() TOTAL, RST.* FROM (" + // Arya :: 2020-08-23
                "    SELECT DISTINCT" + // Arya :: 2020-08-23
                                        ////"        ROW_NUMBER() over (ORDER BY sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                                        //"        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " + // Arya :: 2020-08-23
                                        ////"        agendasurat.nomoragenda NomorAgendaSurat, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.profilepengirim, suratinbox.profilepenerima, " +
                "        suratinbox.tanggalkirim AS tglkirim, surat.targetselesai AS tglselesai, suratinbox.tanggalterima AS tglterima, " + // Arya :: 2020-08-23
                "        to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, " +
                "        suratinbox.statusterkirim, decode(suratinbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.statusurgent, suratinbox.perintahdisposisi, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(agendasurat.targetselesai, 'dd-mm-yyyy') TargetSelesaiSuratMasuk, " +
                "        to_char(agendasurat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesaiSuratMasuk, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        to_char(surat.tanggalinput, 'fmDD Month YYYY') InfoTanggalInput, " +
                "        to_char(suratkembali.tanggalkembali, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkembali, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM, profiletujuan.nama AS NAMAPROFILEPENERIMA, " +
                "        sumber_surat.sumber_keterangan as sumber_keterangan " +


                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +
                "        JOIN UNITKERJA UK ON UK.UNITKERJAID = profiletujuan.UNITKERJAID " + // Arya :: 2020-10-02
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                //"        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerIdAgenda " + // Arya :: 2020-10-02
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID) " + // Arya :: 2020-10-02
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratkembali ON suratkembali.suratinboxid = suratinbox.suratinboxid " +
                (!string.IsNullOrEmpty(sumber) ? " INNER JOIN " : " LEFT JOIN ") + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratinbox.suratid " +
                (!string.IsNullOrEmpty(sumber) ? string.Format(" AND sumber_surat.sumber_keterangan = '{0}' ", sumber) : "") +
                "    WHERE " +
                "        suratinbox.statusterkirim = 0 AND suratinbox.statusterkunci = 0 AND suratinbox.statusforwardtu = 0 AND suratinbox.tindaklanjut <> 'Selesai' " +
                //"        AND suratinbox.kantorid = :KantorId " +
                "        AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                //"        AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId) "; // STATUS SURAT DIUBAH DARI FIELD SURAT.STATUSSURAT KE TABLE ARSIPSURAT // Arya :: 2020-10-02
                "        AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)) "; // Arya :: 2020-10-02


            //if (!String.IsNullOrEmpty(sumber))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Sumber_Keterangan", sumber));
            //    query += " AND sumber_surat.sumber_keterangan = :Sumber_Keterangan ";
            //}

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerIdAgenda", satkerid));
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            //if (!String.IsNullOrEmpty(statussurat))
            //{
            //    query += " AND surat.statussurat IN ( " + statussurat + ") ";
            //}
            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (suratinbox.nip IS NULL OR suratinbox.nip = :Nip) ";
            }
            if (!String.IsNullOrEmpty(arah))
            {
                if (arah.Equals("Inisiatif"))
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kategori", arah));
                    query += " AND (surat.arah = :Arah OR surat.kategori = :Kategori)";
                }
                else
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                    query += " AND surat.arah = :Arah AND surat.kategori <> 'Inisiatif' ";
                }
            }
            if (string.IsNullOrEmpty(spesificprofileid))
            {
                if (!string.IsNullOrEmpty(profileid))
                {
                    query += " AND suratinbox.profilepenerima IN (" + profileid + ") ";
                }
            }
            if (!string.IsNullOrEmpty(spesificprofileid))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SpesificProfileId", spesificprofileid));
                query += " AND suratinbox.profilepenerima = :SpesificProfileId ";
            }



            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", String.Concat("%", metadata.ToLower(), "%")));
                //query += " AND LOWER(utl_raw.cast_to_varchar2(surat.metadata)) LIKE :Metadata ";
                query +=
                    " AND (LOWER(APEX_UTIL.URL_ENCODE(utl_raw.cast_to_varchar2(surat.metadata))) LIKE :Metadata " +
                    "      OR EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND LOWER(APEX_UTIL.URL_ENCODE(suratinbox.keterangan)) LIKE :CatatanAnda)) ";
            }

            if (!String.IsNullOrEmpty(nomorsurat))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", nomorsurat));
                //query += " AND surat.nomorsurat = :NomorSurat ";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", String.Concat("%", nomorsurat.ToLower(), "%")));
                query += " AND LOWER(surat.nomorsurat) LIKE :NomorSurat ";
            }
            if (!String.IsNullOrEmpty(nomoragenda))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", nomoragenda));
                query += " AND surat.nomoragenda = :NomorAgenda ";
            }
            if (!String.IsNullOrEmpty(perihal))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", String.Concat("%", perihal.ToLower(), "%")));
                query += " AND LOWER(surat.perihal) LIKE :Perihal ";
            }
            if (!String.IsNullOrEmpty(tanggalsurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggalsurat + " 00:00:00"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalsurat + " 23:59:59"));
                query += " AND (surat.tanggalsurat BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }
            if (!String.IsNullOrEmpty(tipesurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", tipesurat));
                query += " AND surat.tipesurat = :TipeSurat ";
            }
            if (!String.IsNullOrEmpty(sifatsurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", sifatsurat));
                query += " AND surat.sifatsurat = :SifatSurat ";
            }






            query +=
                " )RST ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratInbox>(query, parameters).ToList<Models.Entities.SuratInbox>();
                //records = ctx.Database.SqlQuery<Models.Entities.SuratInbox><Models.Entities.Kantor>($"SELECT SUMBER_KETERANGAN FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT WHERE SURAT_ID = suratinbox.suratid").FirstOrDefault();
            }

            return records;
        }

        public List<Models.Entities.SuratOutbox> GetSuratOutbox(string satkerid, string nip, string metadata, string nomorsurat, string nomoragenda, string perihal, string tanggalsurat, string tipesurat, string sifatsurat, string sortby, string sorttype, int from, int to, string sumber = null)
        {
            List<Models.Entities.SuratOutbox> records = new List<Models.Entities.SuratOutbox>();

            ArrayList arrayListParameters = new ArrayList();

            // Default atau by TanggalKirim Desc
            string orderby = "suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "TanggalKirim")
                {
                    orderby = "suratoutbox.tanggalkirim, suratoutbox.suratoutboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                        }
                    }
                }
                else if (sortby == "SifatSurat")
                {
                    orderby = "sifatsurat.prioritas, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.prioritas DESC, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                        }
                    }
                }
                else if (sortby == "JenisDisposisi")
                {
                    orderby = "sifatsurat.perintahdisposisi, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.perintahdisposisi DESC, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                        }
                    }
                }
            }

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " +
                //"        agendasurat.nomoragenda NomorAgendaSurat, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratoutbox.suratoutboxid, suratoutbox.suratid, suratoutbox.nip, suratoutbox.profilepengirim, " +
                "        to_char(suratoutbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratoutbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                //"        to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                //"        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, suratinbox.statusterkirim, " +
                //"        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.perintahdisposisi, suratinbox.profilepenerima, " +
                "        decode(suratoutbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                //"        surat.unitorganisasi AS sumber_keterangan, " +

                "        sumber_surat.sumber_keterangan as sumber_keterangan, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                //"        profiletujuan.nama AS NAMAPROFILEPENERIMA, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratoutbox.suratid " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                //"        LEFT JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratoutbox.profilepengirim " +
                //"        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +

                //"        LEFT JOIN surat ON surat.suratid = suratoutbox.suratid " +
                //"        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SURAT ON SURAT.SURATID = suratoutbox.SURATID " +

                (!string.IsNullOrEmpty(sumber) ? " INNER JOIN " : " LEFT JOIN ") + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratoutbox.suratid " +
                (!string.IsNullOrEmpty(sumber) ? string.Format(" AND sumber_surat.sumber_keterangan = '{0}' ", sumber) : "") +

                //"        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratoutbox.SURATID " +
                "    WHERE " +
                "        suratoutbox.suratoutboxid IS NOT NULL ";

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND suratoutbox.nip = :Nip ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(utl_raw.cast_to_varchar2(surat.metadata))) LIKE :Metadata ";
            }
            if (!String.IsNullOrEmpty(nomorsurat))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", nomorsurat));
                //query += " AND surat.nomorsurat = :NomorSurat ";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", String.Concat("%", nomorsurat.ToLower(), "%")));
                query += " AND LOWER(surat.nomorsurat) LIKE :NomorSurat ";
            }
            if (!String.IsNullOrEmpty(nomoragenda))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", nomoragenda));
                query += " AND surat.nomoragenda = :NomorAgenda ";
            }
            if (!String.IsNullOrEmpty(perihal))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", String.Concat("%", perihal.ToLower(), "%")));
                query += " AND LOWER(surat.perihal) LIKE :Perihal ";
            }
            if (!String.IsNullOrEmpty(tanggalsurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggalsurat + " 00:00:00"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalsurat + " 23:59:59"));
                query += " AND (surat.tanggalsurat BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }
            if (!String.IsNullOrEmpty(tipesurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", tipesurat));
                query += " AND surat.tipesurat = :TipeSurat ";
            }
            if (!String.IsNullOrEmpty(sifatsurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", sifatsurat));
                query += " AND surat.sifatsurat = :SifatSurat ";
            }
            //if (!String.IsNullOrEmpty(sumber))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Sumber_Keterangan", sumber));
            //    query += " AND sumber_surat.sumber_keterangan = :Sumber_Keterangan ";
            //}

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratOutbox>(query, parameters).ToList<Models.Entities.SuratOutbox>();
            }

            return records;
        }


        // UNTUK NON TU DAFTAR SURAT OUTBOX (TERKIRIM)
        public int JumlahSuratOutbox(string nip)
        {
            int results = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratoutbox.suratid " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                //"        LEFT JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratoutbox.profilepengirim " +
                //"        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +

                //"        LEFT JOIN surat ON surat.suratid = suratoutbox.suratid " +
                //"        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SURAT ON SURAT.SURATID = suratoutbox.SURATID " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratoutbox.SURATID " +
                "    WHERE " +
                "        suratoutbox.suratoutboxid IS NOT NULL ";

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND suratoutbox.nip = :Nip ";
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                results = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return results;
        }
        public List<Models.Entities.SuratKembali> GetSuratKembali(string profileid, string nip, string metadata, int from, int to)
        {
            List<Models.Entities.SuratKembali> records = new List<Models.Entities.SuratKembali>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over(ORDER BY suratkembali.tanggalkembali, suratkembali.suratkembaliid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        suratkembali.suratkembaliid, suratkembali.suratinboxid, " +
                "        to_char(suratkembali.tanggalkembali, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkembali, " +
                "        suratkembali.keterangan, suratkembali.profilepengirim, suratkembali.nippengirim, suratkembali.namapengirim, " +
                "        suratoutbox.suratoutboxid, suratoutbox.profilepengirim AS ProfilePenerima, " +
                "        suratoutbox.nip AS NipPenerima, pegawai.nama AS NamaPenerima, " +
                "        surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, surat.pengirim AS AsalSurat " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratkembali " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ON suratinbox.suratinboxid = suratkembali.suratinboxid " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ON suratoutbox.suratoutboxid = suratkembali.suratoutboxid " +
                "        JOIN pegawai ON pegawai.pegawaiid = suratoutbox.nip " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratoutbox.suratid " +
                "    WHERE " +
                "        suratinbox.statusterkunci = 0 ";

            if (!String.IsNullOrEmpty(profileid))
            {
                query += " AND suratkembali.profilepenerima IN (" + profileid + ") ";
            }

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (suratkembali.nippenerima IS NULL OR suratkembali.nippenerima = :Nip) ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(surat.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratKembali>(query, parameters).ToList<Models.Entities.SuratKembali>();
            }

            return records;
        }

        public List<Models.Entities.SuratInbox> GetProsesSurat(string satkerid, string nip, string metadata, string sortby, string sorttype, int from, int to)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();

            ArrayList arrayListParameters = new ArrayList();

            // Default atau by SifatSurat
            string orderby = "sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "SifatSurat")
                {
                    orderby = "sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.prioritas DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "JenisDisposisi")
                {
                    orderby = "sifatsurat.perintahdisposisi, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.perintahdisposisi DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalTerima")
                {
                    orderby = "suratinbox.tanggalterima, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalterima DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalKirim")
                {
                    orderby = "suratinbox.tanggalkirim, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
            }

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " +
                //"        agendasurat.nomoragenda NomorAgendaSurat, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.profilepengirim, suratinbox.profilepenerima, " +
                "        to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, " +
                "        suratinbox.statusterkirim, decode(suratinbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.perintahdisposisi, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM, profiletujuan.nama AS NAMAPROFILEPENERIMA " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                //"        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                //"        LEFT JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "    WHERE " +
                "        suratinbox.statusterkirim = 0 " +
                "        AND suratinbox.statusterkunci = 0 " +
                "        AND suratinbox.statusforwardtu = 1 " +
                "        AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId) ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (suratinbox.nip IS NULL OR suratinbox.nip = :Nip) ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(surat.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratInbox>(query, parameters).ToList<Models.Entities.SuratInbox>();
            }

            return records;
        }

        public List<Models.Entities.Surat> GetSuratSP(string myProfiles, string tanggalinput, string penerimasurat, string tipesurat, string sifatsurat, string keterangansurat, string redaksi, string metadata, int from, int to)
        {
            List<Models.Entities.Surat> records = new List<Models.Entities.Surat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY surat.tanggalinput DESC, surat.tanggalsurat DESC, surat.tanggalproses DESC, surat.suratid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalproses, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, 'Surat ' || surat.arah ArahSurat, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "    WHERE " +
                "        surat.suratid IS NOT NULL ";
            //"        surat.kantorid = :SatkerId ";
            //"        EXISTS (SELECT 1 FROM suratoutbox WHERE suratoutbox.suratid = surat.suratid AND suratoutbox.kantorid = :SatkerId) ";

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            if (!String.IsNullOrEmpty(myProfiles))
            {
                query += " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.profilepenerima IN (" + myProfiles + ")) ";
            }

            //if (!string.IsNullOrEmpty(tanggaldari))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", tanggaldari));
            //    query += " AND surat.tanggalinput >= TO_DATE(:TanggalDari, 'DD/MM/YYYY HH24:MI') ";
            //}
            //if (!string.IsNullOrEmpty(tanggalsampai))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSampai", tanggalsampai));
            //    query += " AND surat.tanggalinput < (TO_DATE(:TanggalSampai, 'DD/MM/YYYY HH24:MI')+(1/1440*5)) ";
            //}
            if (!String.IsNullOrEmpty(tanggalinput))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggalinput + " 00:00:00"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalinput + " 23:59:59"));
                query += " AND (surat.tanggalinput BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }
            if (!string.IsNullOrEmpty(penerimasurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", String.Concat("%", penerimasurat.ToLower(), "%")));
                query += " AND LOWER(surat.penerima) LIKE :PenerimaSurat ";
            }
            if (!string.IsNullOrEmpty(tipesurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", tipesurat));
                query += " AND surat.tipesurat = :TipeSurat ";
            }
            if (!String.IsNullOrEmpty(sifatsurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", sifatsurat));
                query += " AND surat.sifatsurat = :SifatSurat ";
            }
            if (!String.IsNullOrEmpty(keterangansurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", keterangansurat));
                query += " AND surat.keterangansurat = :KeteranganSurat ";
            }
            if (!String.IsNullOrEmpty(redaksi))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", redaksi));
                query += " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.redaksi = :Redaksi) ";
            }
            if (!string.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(surat.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).ToList<Models.Entities.Surat>();
            }

            return records;
        }

        public List<Models.Entities.SuratInbox> GetSuratSP2(string satkerid, string tanggaldari, string tanggalsampai, string penerimasurat, string tipesurat, string metadata, int from, int to)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY surat.tanggalinput, surat.tanggalsurat) RNUMBER, COUNT(1) OVER() TOTAL, " +
                //"        agendasurat.nomoragenda NomorAgendaSurat, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.profilepengirim, suratinbox.profilepenerima, " +
                "        to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, " +
                "        suratinbox.statusterkirim, decode(suratinbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.perintahdisposisi, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM, profiletujuan.nama AS NAMAPROFILEPENERIMA " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid AND surat.arah = 'Masuk' " +
                //"        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid AND surat.arah = 'Masuk' AND surat.statussurat = 1 " +
                //"        JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "    WHERE " +
                "        suratinbox.statusterkirim = 0 " +
                "        AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId) ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            //if (!string.IsNullOrEmpty(tanggaldari) && !string.IsNullOrEmpty(tanggalsampai))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggaldari));
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalsampai));
            //    query += " AND (surat.tanggalinput BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI')) ";
            //}
            if (!string.IsNullOrEmpty(tanggaldari))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", tanggaldari));
                query += " AND surat.tanggalinput >= TO_DATE(:TanggalDari, 'DD/MM/YYYY HH24:MI') ";
            }
            if (!string.IsNullOrEmpty(tanggalsampai))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSampai", tanggalsampai));
                query += " AND surat.tanggalinput < (TO_DATE(:TanggalSampai, 'DD/MM/YYYY HH24:MI')+(1/1440*5)) ";
            }
            if (!string.IsNullOrEmpty(penerimasurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", String.Concat("%", penerimasurat.ToLower(), "%")));
                query += " AND LOWER(surat.penerima) LIKE :PenerimaSurat ";
            }
            if (!string.IsNullOrEmpty(tipesurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", tipesurat));
                query += " AND surat.tipesurat = :TipeSurat ";
            }
            if (!string.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(surat.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratInbox>(query, parameters).ToList<Models.Entities.SuratInbox>();
            }

            return records;
        }

        public Models.Entities.TransactionResult BukaSuratInbox(string suratid, string suratinboxid, string nip, string namapegawai)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                             "   nip = :Nip, namapegawai = :NamaPegawai, tanggalbuka = SYSDATE " +
                             " WHERE suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // update tanggalterima, bila kosong
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                             "   tanggalterima = SYSDATE " +
                             " WHERE tanggalterima IS NULL AND suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // Update tanggalproses di table SURAT, bila kosong
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                             "       tanggalproses = SYSDATE " +
                             " WHERE suratid = :SuratId AND tanggalproses IS NULL";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi SET " +
                            "    statusbaca = 'R' " +
                            "  WHERE suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
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


        #region Histori Surat

        public List<Models.Entities.SuratInbox> GetSuratHistory(string suratid, string unitkerjaid, string satkerid)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.namapengirim, suratinbox.namapegawai AS NAMAPENERIMA, " +
                "    NVL(suratinbox.profilepengirim, '-') AS PROFILEPENGIRIM, NVL(suratinbox.profilepenerima, '-') AS PROFILEPENERIMA, " +
                "    NVL(profilepengirim.nama,'-') AS NAMAPROFILEPENGIRIM, NVL(profilepenerima.nama, '-') AS NAMAPROFILEPENERIMA, " +
                "    to_char(surat.tanggalsurat, 'dd/mm/yyyy') TanggalSurat, " +
                "    to_char(suratinbox.tanggalkirim, 'dd/mm/yyyy HH24:MI', 'nls_date_language=INDONESIAN') TanggalKirim, " +
                "    NVL(to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI', 'nls_date_language=INDONESIAN'), '...') InfoTanggalBuka, " +
                "    to_char(suratinbox.tanggalkirim, 'Day, fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalKirim, " +
                "    to_char(suratinbox.tanggalbuka, 'Day, fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalBuka, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalTerima, " +
                "    to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "    to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "    to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "    to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "    to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "    to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "    to_char(suratkembali.tanggalkembali, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkembali, " +
                "    suratinbox.keterangan, suratinbox.perintahdisposisi, suratinbox.tindaklanjut, suratinbox.redaksi, " +
                "    CASE " +
                "        WHEN arsipsurat.tanggal IS NOT NULL THEN 'Sudah Diarsipkan/Selesai' " +
                "        WHEN suratinbox.urutan = 1 AND suratinbox.statushapus = '0' THEN 'Pembuat Surat' " +
                "        WHEN suratinbox.statushapus = '1' THEN 'Dikembalikan' " +
                "        WHEN suratinbox.statusterkirim = 1 THEN 'Telah Terkirim' " +
                "        WHEN suratinbox.tanggalbuka IS NULL THEN 'Belum Dibuka' " +
                //"        WHEN suratinbox.tanggalbuka IS NOT NULL AND suratinbox.tanggalterima IS NULL THEN 'Belum Terima Fisik' " +
                "        ELSE 'Dalam Proses' " +
                "    END AS KetStatusTerkirim, " +
                "    surat.nomorsurat, surat.perihal, surat.nomoragenda, surat.pengirim, " +
                "    surat.sifatsurat, surat.isisingkat, suratinbox.statushapus " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratkembali ON suratkembali.suratinboxid = suratinbox.suratinboxid " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ON arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = suratinbox.kantorid AND arsipsurat.profilearsip = suratinbox.profilepenerima " +
                "    LEFT JOIN jabatan profilepengirim ON profilepengirim.profileid = suratinbox.profilepengirim " +
                "    LEFT JOIN jabatan profilepenerima ON profilepenerima.profileid = suratinbox.profilepenerima " +
                "WHERE " +
                "    suratinbox.suratid = :SuratId ";

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                query +=
                    " AND EXISTS (" +
                    "     SELECT 1 FROM unitkerja " +
                    "     WHERE " +
                    "         unitkerja.unitkerjaid = profilepenerima.unitkerjaid " +
                    "         AND unitkerja.unitkerjaid = :UnitKerjaId) ";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
            }

            query += "ORDER BY suratinbox.urutan";

            //"ORDER BY suratinbox.urutan, suratinbox.tanggalbuka, suratinbox.tanggalkirim, suratinbox.profilepenerima DESC";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratInbox>(query, parameters).ToList<Models.Entities.SuratInbox>();
            }

            return records;
        }

        public List<Models.Entities.UnitKerja> GetUnitKerjaSuratHistory(string suratid)
        {
            List<Models.Entities.UnitKerja> records = new List<Models.Entities.UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "select unitkerjaid, namaunitkerja from unitkerja where unitkerjaid in ( " +
                "select distinct unitkerjaid from jabatan where profileid in ( " +
                "select distinct profilepenerima from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox where suratid = :SuratId " +
                ")) order by unitkerjaid";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.UnitKerja>(query, parameters).ToList<Models.Entities.UnitKerja>();
            }

            return records;
        }

        public List<Models.Entities.SuratInbox> GetHistoryTerkirim(string suratoutboxid, string namapengirim)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            ArrayList arrayListParameters = new ArrayList();

            string query = $@"  SELECT DS.SURATID, DS.SURATINBOXID, DS.NAMAPENGIRIM, DS.NAMAPENERIMA, DS.STATUSBUKA, DS.TANGGALBUKA,
	                                CASE 
		                                WHEN SI2.SURATINBOXID IS NULL THEN NULL
		                                WHEN DS.TINDAKLANJUT = 'Selesai' THEN 'Selesai, Catatan : '||NVL(DS.KETERANGAN,'Tidak Ada Catatan')
		                                ELSE DS.PERINTAHDISPOSISI ||'|'||DS.KETERANGAN
	                                END AS KETERANGAN,
                                DS.REDAKSI
                                FROM
                                (SELECT
	                                SO.SURATID,
	                                SI.SURATINBOXID,
	                                SI.NAMAPENGIRIM,
	                                SI.NAMAPEGAWAI AS NAMAPENERIMA,
	                                SI.TINDAKLANJUT,
	                                SI.PERINTAHDISPOSISI,
	                                SI.KETERANGAN,
                                    CASE
		                                WHEN SI.TANGGALBUKA IS NULL THEN
		                                0 ELSE 1 
	                                END AS STATUSBUKA,
	                                TO_CHAR( SI.TANGGALBUKA, 'dd/mm/yyyy HH24:MI:SS' ) TANGGALBUKA,
                                    SI.REDAKSI
                                FROM
	                                {skema}.SURATOUTBOX SO
	                                LEFT JOIN {skema}.SURATINBOX SI ON SO.SURATID = SI.SURATID
                                WHERE
	                                SO.SURATOUTBOXID = :suratoutboxid
	                                AND SO.PROFILEPENGIRIM = SI.PROFILEPENGIRIM 
	                                AND SI.NAMAPENGIRIM LIKE '%'||:namapengirim||'%' 
	                                AND SI.NIP != SO.NIP AND SI.NAMAPEGAWAI IS NOT NULL) DS
                                LEFT JOIN {skema}.SURATINBOX SI2 ON SI2.SURATID = DS.SURATID
	                                AND SI2.NAMAPENGIRIM =  DS.NAMAPENGIRIM AND SI2.NAMAPEGAWAI = DS.NAMAPENERIMA";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("suratoutboxid", suratoutboxid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("namapengirim", namapengirim));


            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratInbox>(query, parameters).ToList<Models.Entities.SuratInbox>();
            }

            return records;
        }

        public Models.Entities.StatusSurat GetStatusSurat(Models.Entities.SuratInbox si, Models.Entities.InternalUserIdentity usr, bool pj = true)
        {
            var lastrecord = new Models.Entities.StatusSurat();
            var record = new Models.Entities.StatusSurat();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var nama = si.NamaPenerima;
            var sin = si.SuratInboxId;
            List <Models.Entities.StatusSurat > seen = new List<Models.Entities.StatusSurat>();
            int ctsurat = 0;

            while (lastrecord != null)
            {
                if(seen.Exists(x => x.Nama == nama))
                {
                    sin = "";
                    var xx = seen.FindAll(x => x.Nama == nama);
                    foreach(var x in xx)
                    {
                        sin += (string.IsNullOrEmpty(sin)) ? $"'{x.SuratInboxId}'" : $",'{x.SuratInboxId}'";
                    }
                } else
                {
                    sin = "''";
                }

                ArrayList arrayListParameters = new ArrayList();
                string query = $@"SELECT SURATID, SURATINBOXID,
                                     KANTORID AS UNITKERJAID, 
                                     TINDAKLANJUT AS STATUS,
                                     NAMAPEGAWAI AS NAMA, KETERANGAN, REDAKSI
                              FROM {skema}.SURATINBOX
                              WHERE SURATID = :suratid AND NAMAPENGIRIM = :nama AND SURATINBOXID NOT IN ({sin})  AND (REDAKSI = :redaksi OR REDAKSI = :redaksi2)";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("suratid", si.SuratId));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nama", nama));
                if (pj)
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("redaksi", "Penanggung Jawab"));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("redaksi2", "Asli"));
                }
                else
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("redaksi", "Tembusan"));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("redaksi2", "Asli"));
                }
                query += "ORDER BY URUTAN";
                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    lastrecord = ctx.Database.SqlQuery<Models.Entities.StatusSurat>(query, parameters).FirstOrDefault();
                    record = (lastrecord != null) ? lastrecord : record;
                    nama = (lastrecord == null) ? "" : lastrecord.Nama;

                    ctsurat = (lastrecord != null) ? ctsurat + 1 : ctsurat;
                    record.Urut = ctsurat;
                    seen.Add(record);
                }
            }        
            return record;
        }


        #endregion


        #region Session Lampiran Surat

        public Entities.TransactionResult HapusSessionLampiran(string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran WHERE userid = :UserId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public decimal GetMaxUrutanSessionLampiran(string userid)
        {
            decimal result = 0;

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));

            string query = "select count(userid)+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran where userid = :UserId";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public Models.Entities.TransactionResult InsertSessionLampiran(Models.Entities.SessionLampiranSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string id = GetUID();

                        decimal urutan = GetMaxUrutanSessionLampiran(data.UserId);

                        sql =
                             "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran ( " +
                             "       lampiransuratid, rnumber, namafile, userid, ObjectFile, nip) VALUES " +
                             "( " +
                             "       :Id, :RNumber, :NamaFile, :UserId, :ObjectFile,:Nip)";
                        //sql = sWhitespace.Replace(sql, " ");

                        data.LampiranSuratId = id;

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RNumber", urutan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", data.UserId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.Nip));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.LampiranSuratId;
                        tr.Pesan = "Data berhasil disimpan";
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

        public Entities.TransactionResult HapusSessionLampiranById(string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran WHERE lampiransuratid = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public List<Models.Entities.SessionLampiranSurat> GetListSessionLampiran(string userid)
        {
            List<Models.Entities.SessionLampiranSurat> records = new List<Models.Entities.SessionLampiranSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    lampiransuratid, rnumber, namafile, userid, ObjectFile, nip " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran " +
                "WHERE " +
                "    userid = :UserId " +
                "ORDER BY rnumber";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SessionLampiranSurat>(query, parameters).ToList<Models.Entities.SessionLampiranSurat>();
            }

            return records;
        }

        public List<Models.Entities.SessionLampiranSurat> GetSessionLampiranForTable(string userid)
        {
            List<Models.Entities.SessionLampiranSurat> records = new List<Models.Entities.SessionLampiranSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    lampiransuratid, rnumber, namafile, userid, nip " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran " +
                "WHERE " +
                "    userid = :UserId " +
                "ORDER BY rnumber";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SessionLampiranSurat>(query, parameters).ToList<Models.Entities.SessionLampiranSurat>();
            }

            return records;
        }

        public byte[] GetSessionLampiranById(string lampiransuratid)
        {
            byte[] theFile = null;
            List<Models.Entities.SessionLampiranSurat> data = new List<Models.Entities.SessionLampiranSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    lampiransuratid, rnumber, namafile, userid, ObjectFile, nip " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessionlampiran " +
                "WHERE " +
                "    lampiransuratid = :LampiranSuratId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", lampiransuratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Models.Entities.SessionLampiranSurat>(query, parameters).ToList<Models.Entities.SessionLampiranSurat>();

                if (data.Count > 0)
                {
                    theFile = data[0].ObjectFile;
                }
            }

            return theFile;
        }

        #endregion


        #region Lampiran Surat

        public List<Models.Entities.LampiranSurat> GetListLampiranSurat(string suratid, string lampiransuratid)
        {
            List<Models.Entities.LampiranSurat> records = new List<Models.Entities.LampiranSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT " +
                 "     ROW_NUMBER() over (ORDER BY lampiransurat.tanggal, lampiransurat.namafile, lampiransurat.lampiransuratid) RNUMBER, " +
                 "     lampiransurat.lampiransuratid, lampiransurat.suratid, lampiransurat.path FolderFile, SUBSTR(lampiransurat.namafile, INSTR(lampiransurat.namafile, '|', 1, 1) +1) as namafile , " +
                 "     lampiransurat.keterangan, lampiransurat.profileid, lampiransurat.nip, " +
                 "     NVL(pegawai.nama, ppnpn.nama) AS NamaPegawai, " +
                 "     to_char(lampiransurat.tanggal, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') Tanggal, " +
                 "     lampiransurat.kantorid, " +
                 "     COUNT(1) OVER() TOTAL " +
                 " FROM " +
                 "     " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat " +
                 "     LEFT JOIN pegawai ON pegawai.pegawaiid = lampiransurat.nip " +
                 "     LEFT JOIN ppnpn ON ppnpn.nik = lampiransurat.nip " +
                 " WHERE " +
                 "     lampiransurat.suratid = :SuratId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            if (!String.IsNullOrEmpty(lampiransuratid))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", lampiransuratid));
                query += " AND lampiransurat.lampiransuratid = :LampiranSuratId ";
            }

            //query = sWhitespace.Replace(query, " ");

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.LampiranSurat>(query, parameters).ToList<Models.Entities.LampiranSurat>();
            }

            return records;
        }

        public Entities.TransactionResult HapusLampiranSuratById(string suratid, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat WHERE lampiransuratid = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public Models.Entities.TransactionResult InsertLampiranSurat(Models.Entities.Surat data, string kantorid)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        sql =
                             "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat ( " +
                             "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                             "( " +
                             "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)";
                        //sql = sWhitespace.Replace(sql, " ");

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", data.LampiranSuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", "-"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.NamaFile));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.NIP));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Jumlah Lampiran di tabel SURAT
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                             "   jumlahlampiran = (SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat WHERE suratid = :SuratId1) " +
                             " WHERE suratid = :SuratId2";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId1", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId2", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.LampiranSuratId;
                        tr.Pesan = "Data berhasil disimpan";
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

        #endregion


        #region Session Tujuan Surat

        public Entities.TransactionResult HapusSessionTujuanSurat(string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessiontujuansurat WHERE userid = :UserId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public decimal GetMaxUrutanSessionTujuanSurat(string userid)
        {
            decimal result = 0;

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));

            string query = "select count(userid)+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessiontujuansurat where userid = :UserId";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public Models.Entities.TransactionResult InsertSessionTujuanSurat(Models.Entities.SessionTujuanSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string id = GetUID();

                        decimal urutan = GetMaxUrutanSessionTujuanSurat(data.UserId);

                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessiontujuansurat ( " +
                            "       tujuansuratid, rnumber, userid, redaksi, profileid, nip, namajabatan, namapegawai, statusurgent) VALUES " +
                            "( " +
                            "       :Id, :RNumber, :UserId, :Redaksi, :ProfileId, :Nip, :NamaJabatan, :NamaPegawai, :StatusUrgent)";
                        //sql = sWhitespace.Replace(sql, " ");

                        data.TujuanSuratId = id;

                        int statusurgent = (data.IsStatusUrgent == true) ? 1 : 0;

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RNumber", urutan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", data.UserId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", data.Redaksi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileId", data.ProfileId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.NIP));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaJabatan", data.NamaJabatan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPegawai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusUrgent", statusurgent));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.TujuanSuratId;
                        tr.Pesan = "Data berhasil disimpan";
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

        public Entities.TransactionResult HapusSessionTujuanSuratById(string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessiontujuansurat WHERE tujuansuratid = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public List<Models.Entities.SessionTujuanSurat> GetListSessionTujuanSurat(string userid)
        {
            List<Models.Entities.SessionTujuanSurat> records = new List<Models.Entities.SessionTujuanSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    sessiontujuansurat.tujuansuratid, sessiontujuansurat.rnumber, sessiontujuansurat.userid, sessiontujuansurat.profileid, " +
                "    sessiontujuansurat.nip, sessiontujuansurat.namajabatan, sessiontujuansurat.namapegawai, " +
                "    sessiontujuansurat.redaksi || decode(statusurgent, 1, ', Urgent', '') AS Redaksi, sessiontujuansurat.statusurgent, " +
                "    jabatan.unitkerjaid, unitkerja.kantorid " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessiontujuansurat " +
                "    JOIN jabatan ON jabatan.profileid = sessiontujuansurat.profileid " +
                "    JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid " +
                "WHERE " +
                "    sessiontujuansurat.userid = :UserId " +
                "ORDER BY rnumber";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SessionTujuanSurat>(query, parameters).ToList<Models.Entities.SessionTujuanSurat>();
            }

            return records;
        }

        public int JumlahTujuanSuratPJ(string userid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sessiontujuansurat " +
                "WHERE  userid = :UserId AND redaksi = :Redaksi";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", userid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", "Penanggung Jawab"));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        #endregion


        #region Tujuan Disposisi Surat

        public List<Models.Entities.TipeEselon> GetTipeEselonOnDisposisiSurat(string suratid)
        {
            List<Models.Entities.TipeEselon> records = new List<Models.Entities.TipeEselon>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "select distinct jabatan.tipeeselonid " +
                "from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".disposisisurat  " +
                "     join " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".jabatan on jabatan.profileid = disposisisurat.profileid " +
                "where disposisisurat.suratid = :SuratId " +
                "order by jabatan.tipeeselonid";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.TipeEselon>(query, parameters).ToList<Models.Entities.TipeEselon>();
            }

            return records;
        }

        public List<Models.Entities.DisposisiSurat> GetListDisposisiSurat(string suratid, string unitkerjaid, string tipeeselonid)
        {
            List<Models.Entities.DisposisiSurat> records = new List<Models.Entities.DisposisiSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    ROW_NUMBER() over (ORDER BY jabatan.tipeeselonid, disposisisurat.profileid) RNumber, " +
                "    disposisisurat.disposisisuratid, disposisisurat.suratid, disposisisurat.unitkerjaid, disposisisurat.profileid, " +
                "    disposisisurat.nip, disposisisurat.namajabatan, disposisisurat.namapegawai, NVL(jabatan.tipeeselonid, 6) tipeeselonid " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".disposisisurat " +
                "    JOIN jabatan ON jabatan.profileid = disposisisurat.profileid " +
                //"    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".jabatan ON jabatan.profileid = disposisisurat.profileid " +
                "WHERE " +
                "    disposisisurat.suratid = :SuratId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                query += "AND disposisisurat.unitkerjaid = :UnitKerjaId ";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
            }

            if (!string.IsNullOrEmpty(tipeeselonid))
            {
                query += "AND jabatan.tipeeselonid = :TipeEselonId ";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeEselonId", Convert.ToInt32(tipeeselonid)));
            }

            query += "ORDER BY jabatan.tipeeselonid, disposisisurat.profileid";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.DisposisiSurat>(query, parameters).ToList<Models.Entities.DisposisiSurat>();
            }

            return records;
        }

        public Models.Entities.TransactionResult InsertDisposisiSurat(Models.Entities.DisposisiSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string id = GetUID();

                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".disposisisurat ( " +
                            "       disposisisuratid, suratid, unitkerjaid, profileid, nip, namajabatan, namapegawai) VALUES " +
                            "( " +
                            "       :Id, :SuratId, :UnitKerjaId, :ProfileId, :Nip, :NamaJabatan, :NamaPegawai)";
                        //sql = sWhitespace.Replace(sql, " ");

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", data.UnitKerjaId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileId", data.ProfileId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.NIP));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaJabatan", data.NamaJabatan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPegawai));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Data tersebut sudah ada.";
                        }
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

        public Models.Entities.TransactionResult InsertDispoUnitKerja(string kantorid, string pegawaiid, string suratid)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            bool AdaDataTersimpan = false;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        List<Models.Entities.Profile> listProfileDisposisi = dataMasterModel.GetListProfileDisposisi(pegawaiid, kantorid, true);
                        if (listProfileDisposisi.Count == 0)
                        {
                            // Bila bukan login TU, ambil jabatan2 di bawahnya user login ybs
                            string myProfiles = functions.MyProfiles(pegawaiid, kantorid);
                            listProfileDisposisi = dataMasterModel.GetProfileDisposisiByMyProfiles(myProfiles, true);
                        }

                        foreach (Models.Entities.Profile profileSurat in listProfileDisposisi)
                        {
                            string nippejabatdisposisi = "";
                            string namapejabatdisposisi = "";
                            List<Surat.Models.Entities.Pegawai> listPegawai = dataMasterModel.GetPegawaiByProfileId(profileSurat.ProfileId);
                            if (listPegawai.Count > 0)
                            {
                                nippejabatdisposisi = listPegawai[0].PegawaiId;
                                namapejabatdisposisi = listPegawai[0].Nama;
                            }


                            #region Cek Duplikasi

                            bool CanSave = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".disposisisurat " +
                                "WHERE suratid = :SuratId AND unitkerjaid = :UnitKerjaId " +
                                "AND profileid = :ProfileId AND nip = :Nip";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", profileSurat.UnitKerjaId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileId", profileSurat.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nippejabatdisposisi));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahdata > 0)
                            {
                                CanSave = false;
                            }

                            #endregion

                            if (CanSave)
                            {
                                string id = GetUID();

                                sql =
                                     "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".disposisisurat ( " +
                                     "       disposisisuratid, suratid, unitkerjaid, profileid, nip, namajabatan, namapegawai) VALUES " +
                                     "( " +
                                     "       :Id, :SuratId, :UnitKerjaId, :ProfileId, :Nip, :NamaJabatan, :NamaPegawai)";
                                //sql = sWhitespace.Replace(sql, " ");

                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", profileSurat.UnitKerjaId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileId", profileSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nippejabatdisposisi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaJabatan", profileSurat.NamaProfile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapejabatdisposisi));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                AdaDataTersimpan = true;
                            }
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = (AdaDataTersimpan) ? "Data berhasil disimpan" : "Tidak ada data baru yang tersimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Data tersebut sudah ada.";
                        }
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

        public Entities.TransactionResult HapusDisposisiSuratById(string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".disposisisurat WHERE disposisisuratid = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        #endregion


        #region Simpan Surat

        public Models.Entities.TransactionResult InsertSuratMasuk(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim, List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat, List<Models.Entities.SessionLampiranSurat> dataSessionLampiran)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = myprofileidtu;
                            satkerid = unitkerjaid;
                        }

                        string myClientId = Functions.MyClientId;


                        #region Session Lampiran

                        //List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = this.GetListSessionLampiran(myClientId); // data.UserId

                        data.JumlahLampiran = dataSessionLampiran.Count;

                        #endregion


                        string id = GetUID();
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        data.SuratId = id;
                        data.Arah = "Masuk";
                        data.PenerimaSurat = ""; // ambil dari daftar session tujuan surat (asli)


                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.PenerimaSurat + " ";
                        metadata += data.IsiSingkatSurat + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata += data.UserId + " ";
                        metadata += data.TipeSurat + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.Kategori + " ";
                        metadata += data.KeteranganSurat + " ";
                        metadata += data.Sumber_Keterangan + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Insert SURAT
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ( " +
                            "       suratid, kantorid, tanggalsurat, tanggalproses, tanggalundangan, nomorsurat, nomoragenda, perihal, pengirim, penerima, arah, kategori, " +
                            "       tipesurat, sifatsurat, keterangansurat, jumlahlampiran, isisingkat, tipekegiatan, metadata) VALUES " +
                            "( " +
                            "       :Id, :SatkerId, TO_DATE(:TanggalSurat,'DD/MM/YYYY'), TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'), TO_DATE(:TanggalUndangan,'DD/MM/YYYY HH24:MI'), " +
                            "       :NomorSurat, :NomorAgenda, :Perihal, :PengirimSurat, :PenerimaSurat, :Arah, 'Masuk', " +
                            "       :TipeSurat, :SifatSurat, :KeteranganSurat, :JumlahLampiran, :IsiSingkatSurat, :KodeKlasifikasi, utl_raw.cast_to_raw(:Metadata))";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSurat", data.TanggalSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalUndangan", data.TanggalUndangan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", data.NomorAgenda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", data.Perihal));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengirimSurat", data.PengirimSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", data.Arah));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", data.TipeSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", data.SifatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiSingkatSurat", data.IsiSingkatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeKlasifikasi", data.KodeKlasifikasi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);



                        #region AGENDA SURAT

                        // Cek Konter Agenda
                        string query = "select count(*) from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                        if (jumlahrecord == 0)
                        {
                            // Bila tidak ada, Insert KONTERSURAT
                            query =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                "( " +
                                "       SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                            //query = sWhitespace.Replace(query, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", "Agenda"));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(query, parameters);
                        }

                        decimal nilainomoragenda = 1;

                        query =
                            "select nilaikonter+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                            "FOR UPDATE NOWAIT";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                        parameters = arrayListParameters.OfType<object>().ToArray();

                        nilainomoragenda = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();


                        sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiKonter", nilainomoragenda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Binding Nomor Agenda
                        int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                        string strBulan = Functions.NomorRomawi(bulan);
                        string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
                        string kodesurat = "AG-";

                        string strNomorAgenda = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());
                        data.NomorAgenda = strNomorAgenda;


                        // Insert AGENDASURAT
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat ( " +
                            "       agendasuratid, suratid, nomoragenda, kantorid) VALUES " +
                            "( " +
                            "       SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", data.NomorAgenda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        #endregion



                        // Insert LAMPIRAN SURAT
                        foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat ( " +
                                    "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                                    "( " +
                                    "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", folderfile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", lampiranSurat.Nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }



                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.UserId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        string suratinboxid = "";
                        int urutan = 0;


                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "       0,0,1)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip)); // nip Pengirim
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", ""));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        #endregion



                        #region Session Tujuan Surat

                        bool belumkirimtuba = true;

                        bool iskirimtuba = false;

                        int statusterkunci = 0;

                        //List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            if (tujuanSurat.Redaksi == "Asli")
                            {
                                data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            }

                            // Get SatkerId Tujuan. Bila Pusat dari UnitkerjaId, bila kanwil/kantah dari KantorId


                            string satkeridtujuan = string.Empty;
                            string unitkerjaTujuan = ctx.Database.SqlQuery<string>($"SELECT UNITKERJAID FROM JABATAN WHERE PROFILEID = '{tujuanSurat.ProfileId}'").First();

                            if (tipekantorid == 1)
                            {
                                satkeridtujuan = unitkerjaTujuan;
                            } else
                            {
                                satkeridtujuan = dataMasterModel.GetKantorIdByUnitKerjaId(unitkerjaTujuan);
                            }


                            // GET JABATAN DAN PEGAWAI TUJUAN

                            // Cek Delegasi Surat ------------------------
                            string profileidtujuan = tujuanSurat.ProfileId;
                            string niptujuan = tujuanSurat.NIP;
                            string namapegawaitujuan = tujuanSurat.NamaPegawai;

                            Entities.DelegasiSurat delegasiSurat = GetDelegasiSurat(tujuanSurat.ProfileId);
                            if (delegasiSurat != null)
                            {
                                profileidtujuan = delegasiSurat.ProfilePenerima;
                                niptujuan = delegasiSurat.NIPPenerima;
                                namapegawaitujuan = delegasiSurat.NamaPenerima;
                            }
                            // Eof Cek Delegasi Surat --------------------

                            // Eof // GET JABATAN DAN PEGAWAI TUJUAN------



                            int statusterkunciTujuan = 1;


                            #region Kirim ke TU Persuratan (Pusat)

                            if (belumkirimtuba)
                            {
                                string profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);

                                if (!string.IsNullOrEmpty(profileidba))
                                {
                                    statusterkunci = 1;

                                    string pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba, true);
                                    if (string.IsNullOrEmpty(pegawaiidba))
                                    {
                                        pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba);
                                    }

                                    Models.Entities.Pegawai pegawaiBA = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidba);
                                    if (pegawaiBA == null)
                                    {
                                        string namaprofile = dataMasterModel.GetProfileNameFromId(profileidba);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidba;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }

                                    #region Cek Duplikasi

                                    bool BisaKirimSurat = true;

                                    sql =
                                        "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimSurat = false;
                                    }

                                    #endregion

                                    if (pegawaiidba == nip) // Arya :: 2020-08-07 :: Cek Pembuat Surat Masuk, bila MailRoom maka langsung ke TU/Tujuan.  
                                    {
                                        BisaKirimSurat = false;
                                        iskirimtuba = true;
                                        statusterkunci = 0;
                                    }

                                    if (BisaKirimSurat)
                                    {
                                        // Insert SURATINBOX
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, id);
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "       0,1,:Urutan)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                                        try
                                        {
                                            new Mobile().KirimNotifikasi(pegawaiidba, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.IsiSingkatSurat) ? "Persetujuan Baru" : data.IsiSingkatSurat.ToString()), "Persetujuan");
                                        }
                                        catch (Exception ex)
                                        {
                                            var str = ex.Message;
                                        }

                                        // Insert SURATOUTBOXRELASI
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                                        iskirimtuba = true;
                                    }
                                }

                                belumkirimtuba = false;
                            }

                            #endregion


                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            string profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);
                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                // Cek Apakah dari MailRoom (Pembuat Surat), langsung dikirim ke Pejabat Tujuan,
                                // atau dikirim ke TU, dimana nanti TU yang akan mendistribusikan (alias Kirim seperti Biasa)
                                // Kalau dikirim ke TU, tidak usah buat Persetujuan TU, tapi kirim Inbox TU biasa

                                bool JadiKirimkeTU = true;

                                if (profileidtu == profileidtujuan)
                                {
                                    // Bila tujuan surat ke TU, tidak usah buat Inbox Persetujuan, buat inbox biasa ke TU
                                    JadiKirimkeTU = false;
                                    statusterkunciTujuan = 0;
                                }

                                if (profileidtu == myprofileid) // Arya :: 2020-08-07 :: Cek Pengirim Surat, Bila TU dari tujuan surat maka tidak usah buat Inbox Persetujuan, buat inbox biasa ke Tujuan
                                {
                                    JadiKirimkeTU = false;
                                    statusterkunciTujuan = 0;
                                }

                                if (JadiKirimkeTU)
                                {
                                    if (profileidtu != myprofileidtu)
                                    {
                                        // bila dikirim ke TU Pengolah Kantor Pusat


                                        // Cek Delegasi Surat ------------------------
                                        Entities.DelegasiSurat delegasiSuratTU = GetDelegasiSurat(profileidtu);
                                        if (delegasiSuratTU != null)
                                        {
                                            profileidtu = delegasiSuratTU.ProfilePenerima;
                                            //pegawaiidtu = delegasiSuratTU.NIPPenerima;
                                            //namapegawaitujuan = delegasiSuratTU.NamaPenerima;
                                        }
                                        // Eof Cek Delegasi Surat --------------------


                                        string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                        if (string.IsNullOrEmpty(pegawaiidtu))
                                        {
                                            pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                        }
                                        Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                        if (pegawaiTU == null)
                                        {
                                            string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidtu;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }


                                        #region Cek Duplikasi

                                        bool BisaKirimKeTUPengolah = true;

                                        query =
                                            "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimKeTUPengolah = false;
                                        }

                                        #endregion


                                        if (BisaKirimKeTUPengolah)
                                        {
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, id);
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "       :StatusTerkunci,1,:Urutan)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                                            // Insert SURATOUTBOXRELASI (ke Profile TU)
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                                            try
                                            {
                                                new Mobile().KirimNotifikasi(pegawaiidtu, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.IsiSingkatSurat) ? "Persetujuan Baru" : data.IsiSingkatSurat.ToString()), "Persetujuan");
                                            }
                                            catch (Exception ex)
                                            {
                                                var str = ex.Message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!iskirimtuba)
                                        {
                                            // bila dikirim ke TU Kanwil/Kantah

                                            string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                            if (string.IsNullOrEmpty(pegawaiidtu))
                                            {
                                                pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                            }
                                            Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                            if (pegawaiTU == null)
                                            {
                                                string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                                if (string.IsNullOrEmpty(namaprofile))
                                                {
                                                    namaprofile = profileidtu;
                                                }
                                                tc.Rollback();
                                                tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                                tc.Dispose();
                                                ctx.Dispose();

                                                return tr;
                                            }


                                            #region Cek Duplikasi

                                            bool BisaKirimKeTUPengolah = true;

                                            query =
                                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                            if (jumlahinbox > 0)
                                            {
                                                BisaKirimKeTUPengolah = false;
                                            }

                                            #endregion


                                            if (BisaKirimKeTUPengolah)
                                            {
                                                suratinboxid = GetUID();
                                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                                sql =
                                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                    "( " +
                                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                    "       0,1,:Urutan)";
                                                //sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                                // Insert SURATOUTBOXRELASI (ke Profile TU)
                                                sql =
                                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                    "    suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                    "( " +
                                                    "    :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                                //sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                        }
                                        else
                                        {
                                            // Bila Surat dikirim ke Biro Umum, dimana TU nya ada TU Mail-Room (Kantor Pusat)
                                            string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                            if (string.IsNullOrEmpty(pegawaiidtu))
                                            {
                                                pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                            }
                                            Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                            if (pegawaiTU == null)
                                            {
                                                string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                                if (string.IsNullOrEmpty(namaprofile))
                                                {
                                                    namaprofile = profileidtu;
                                                }
                                                tc.Rollback();
                                                tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                                tc.Dispose();
                                                ctx.Dispose();

                                                return tr;
                                            }

                                            #region Cek Duplikasi

                                            bool BisaKirimKeTUPengolah = true;

                                            query =
                                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                            if (jumlahinbox > 0)
                                            {
                                                BisaKirimKeTUPengolah = false;
                                            }

                                            #endregion


                                            if (BisaKirimKeTUPengolah)
                                            {
                                                suratinboxid = GetUID();
                                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                                sql =
                                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                    "( " +
                                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                    "       :StatusTerkunci,1,:Urutan)";
                                                //sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                                // Insert SURATOUTBOXRELASI (ke Profile TU)
                                                sql =
                                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                    "( " +
                                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                                //sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                        }
                                    }
                                } // End of if (JadiKirimkeTU) ------
                            }

                            #endregion


                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            // detail Jabatan dan Pegawai Tujuan sudah disetting di atas, di awal Loop

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 1 AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtujuan));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :StatusTerkunciTujuan,0,:Urutan)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", niptujuan)); // nip penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaitujuan)); // nama penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunciTujuan", statusterkunciTujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                #region AGENDA SURAT DI TUJUAN SURAT

                                // Bila Satker Pengirim tidak sama dengan Satker Tujuan Surat
                                if (satkerid != satkeridtujuan)
                                {
                                    // Cek Konter Agenda
                                    query = "select count(*) from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                                    if (jumlahrecord == 0)
                                    {
                                        // Bila tidak ada, Insert KONTERSURAT
                                        query =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                            "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                            "( " +
                                            "       SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                                        //query = sWhitespace.Replace(query, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", "Agenda"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(query, parameters);
                                    }

                                    nilainomoragenda = 1;

                                    query =
                                        "select nilaikonter+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                                        "FOR UPDATE NOWAIT";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                                    parameters = arrayListParameters.OfType<object>().ToArray();

                                    nilainomoragenda = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();

                                    sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiKonter", nilainomoragenda));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);

                                    // Binding Nomor Agenda
                                    //int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                                    //string strBulan = Functions.NomorRomawi(bulan);
                                    kodeindentifikasi = GetKodeIdentifikasi(tujuanSurat.UnitKerjaId);
                                    //string kodesurat = "AG-";

                                    strNomorAgenda = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());

                                    // Insert AGENDASURAT
                                    sql =
                                        "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat ( " +
                                        "       agendasuratid, suratid, nomoragenda, kantorid) VALUES " +
                                        "( " +
                                        "       SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", strNomorAgenda));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkeridtujuan));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }

                                #endregion

                            }

                            #endregion

                        }

                        #endregion


                        // Update Table SURAT
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                             "       penerima = :PenerimaSurat " +
                             "       WHERE suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT"))
                        {
                            tr.Pesan = "Nomor Surat " + data.NomorSurat + " sudah ada.";
                        }
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

        public Entities.TransactionResult SimpanSumberSurat(string Sumber, string surat_id)
        {

            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())

            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

                        var sql = $@"Insert into {skema}.Sumber_surat (sumber_id, surat_id, sumber_keterangan) values ('{id}', '{surat_id}', '{Sumber}')";
                        ctx.Database.ExecuteSqlCommand(sql);
                        tr.Pesan = "berhasil";

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = id;
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







        public Models.Entities.TransactionResult MergeSuratMasuk(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim, List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat, List<Models.Entities.SessionLampiranSurat> dataSessionLampiran)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            string query = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = myprofileidtu;
                            satkerid = unitkerjaid;
                        }

                        string myClientId = Functions.MyClientId;


                        #region Session Lampiran

                        //List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = this.GetListSessionLampiran(myClientId); // data.UserId

                        data.JumlahLampiran = dataSessionLampiran.Count;

                        #endregion


                        string id = data.SuratId;

                        data.PenerimaSurat = ""; // ambil dari daftar session tujuan surat (asli)



                        // Insert LAMPIRAN SURAT
                        foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat ( " +
                                    "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                                    "( " +
                                    "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:SatkerId,:Keterangan,:Nip)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", folderfile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", lampiranSurat.Nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }



                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.UserId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        string suratinboxid = "";
                        int urutan = 0;


                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "       0,0,1)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip)); // nip Pengirim
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", ""));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        #endregion



                        #region Session Tujuan Surat

                        //bool belumkirimtuba = true;

                        bool iskirimtuba = false; // bila merge surat, tidak perlu ke mailroom pusat

                        int statusterkunci = 0;

                        //List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            if (tujuanSurat.Redaksi == "Asli")
                            {
                                data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            }

                            #region Kirim ke TU Persuratan (Pusat) (Remarked, bila merge surat, tidak perlu ke mailroom pusat

                            //if (belumkirimtuba)
                            //{
                            //    string profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);

                            //    if (!string.IsNullOrEmpty(profileidba))
                            //    {
                            //        statusterkunci = 1;

                            //        string pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba);
                            //        Models.Entities.Pegawai pegawaiBA = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidba);
                            //        if (pegawaiBA == null)
                            //        {
                            //            string namaprofile = dataMasterModel.GetProfileNameFromId(profileidba);
                            //            if (string.IsNullOrEmpty(namaprofile))
                            //            {
                            //                namaprofile = profileidba;
                            //            }
                            //            tc.Rollback();
                            //            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                            //            tc.Dispose();
                            //            ctx.Dispose();

                            //            return tr;
                            //        }


                            //        #region Cek Duplikasi

                            //        bool BisaKirimSurat = true;

                            //        sql =
                            //            "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                            //            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                            //            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                            //        arrayListParameters.Clear();
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                            //        parameters = arrayListParameters.OfType<object>().ToArray();
                            //        int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            //        if (jumlahinbox > 0)
                            //        {
                            //            BisaKirimSurat = false;
                            //        }

                            //        #endregion

                            //        if (BisaKirimSurat)
                            //        {
                            //            // Insert SURATINBOX
                            //            suratinboxid = GetUID();
                            //            urutan = GetMaxUrutanSuratInbox(ctx, id);
                            //            sql =
                            //                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            //                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            //                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            //                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                            //                "( " +
                            //                "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            //                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                            //                "       0,1,:Urutan)";
                            //            //sql = sWhitespace.Replace(sql, " ");
                            //            arrayListParameters.Clear();
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                            //            parameters = arrayListParameters.OfType<object>().ToArray();
                            //            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            //            // Insert SURATOUTBOXRELASI
                            //            sql =
                            //                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            //                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            //                "( " +
                            //                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                            //            //sql = sWhitespace.Replace(sql, " ");
                            //            arrayListParameters.Clear();
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                            //            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                            //            parameters = arrayListParameters.OfType<object>().ToArray();
                            //            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            //            iskirimtuba = true;
                            //        }
                            //    }

                            //    belumkirimtuba = false;
                            //}

                            #endregion


                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            string profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);
                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                if (profileidtu != myprofileidtu)
                                {
                                    string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                    if (string.IsNullOrEmpty(pegawaiidtu))
                                    {
                                        pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                    }
                                    Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                    if (pegawaiTU == null)
                                    {
                                        string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidtu;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }


                                    #region Cek Duplikasi

                                    bool BisaKirimKeTUPengolah = true;

                                    query =
                                        "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimKeTUPengolah = false;
                                    }

                                    #endregion


                                    if (BisaKirimKeTUPengolah)
                                    {
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, id);
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "       :StatusTerkunci,1,:Urutan)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                                        // Insert SURATOUTBOXRELASI (ke Profile TU)
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                }
                                else
                                {
                                    if (!iskirimtuba)
                                    {
                                        // bila dikirim ke TU Kanwil/Kantah

                                        string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                        if (string.IsNullOrEmpty(pegawaiidtu))
                                        {
                                            pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                        }
                                        Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                        if (pegawaiTU == null)
                                        {
                                            string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidtu;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }


                                        #region Cek Duplikasi

                                        bool BisaKirimKeTUPengolah = true;

                                        query =
                                            "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimKeTUPengolah = false;
                                        }

                                        #endregion


                                        if (BisaKirimKeTUPengolah)
                                        {
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, id);
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "       0,1,:Urutan)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                                            // Insert SURATOUTBOXRELASI (ke Profile TU)
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "    suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "    :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }
                                    }
                                    else
                                    {
                                        // Bila Surat dikirim ke Biro Umum, dimana TU nya ada TU Mail-Room (Kantor Pusat)
                                        string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                        if (string.IsNullOrEmpty(pegawaiidtu))
                                        {
                                            pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                        }
                                        Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                        if (pegawaiTU == null)
                                        {
                                            string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidtu;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }

                                        #region Cek Duplikasi

                                        bool BisaKirimKeTUPengolah = true;

                                        query =
                                            "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimKeTUPengolah = false;
                                        }

                                        #endregion


                                        if (BisaKirimKeTUPengolah)
                                        {
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, id);
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "       :StatusTerkunci,1,:Urutan)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                                            // Insert SURATOUTBOXRELASI (ke Profile TU)
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }
                                    }
                                }
                            }

                            #endregion


                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            // Cek Delegasi Surat ------------------------
                            string profileidtujuan = tujuanSurat.ProfileId;
                            string niptujuan = tujuanSurat.NIP;
                            string namapegawaitujuan = tujuanSurat.NamaPegawai;

                            Entities.DelegasiSurat delegasiSurat = GetDelegasiSurat(tujuanSurat.ProfileId);
                            if (delegasiSurat != null)
                            {
                                profileidtujuan = delegasiSurat.ProfilePenerima;
                                niptujuan = delegasiSurat.NIPPenerima;
                                namapegawaitujuan = delegasiSurat.NamaPenerima;
                            }

                            // Eof Cek Delegasi Surat --------------------

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 1 AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtujuan));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       1,0,:Urutan)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", niptujuan)); // nip penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaitujuan)); // nama penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }

                            #endregion

                        }

                        #endregion


                        // Update Table SURAT
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                             "       jumlahlampiran = jumlahlampiran + :JumlahLampiran " +
                             "       WHERE suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public Models.Entities.TransactionResult InsertSuratKeluar(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = myprofileidtu;
                            satkerid = unitkerjaid;
                        }

                        string myClientId = Functions.MyClientId;


                        #region Session Lampiran

                        List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = this.GetListSessionLampiran(myClientId); // data.UserId

                        data.JumlahLampiran = dataSessionLampiran.Count;

                        #endregion


                        string id = GetUID();
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        data.SuratId = id;
                        data.Arah = "Masuk"; // Keluar
                        data.PenerimaSurat = data.PenerimaSurat;
                        data.PengirimSurat = dataMasterModel.GetUnitKerjaFromProfileId(data.ProfileIdPengirim);

                        if (data.ArahSuratKeluar == "Internal")
                        {
                            data.PenerimaSurat = ""; // ambil dari daftar session tujuan surat (asli)
                        }

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.PenerimaSurat + " ";
                        metadata += data.IsiSingkatSurat + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata += data.UserId + " ";
                        metadata += data.TipeSurat + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.Kategori + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Insert SURAT
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ( " +
                            "       suratid, kantorid, tanggalsurat, tanggalproses, tanggalundangan, nomorsurat, nomoragenda, perihal, pengirim, penerima, arah, kategori, " +
                            "       tipesurat, sifatsurat, keterangansurat, jumlahlampiran, isisingkat, unitorganisasi, tipekegiatan, referensi, metadata) VALUES " +
                            "( " +
                            "       :Id, :SatkerId, TO_DATE(:TanggalSurat,'DD/MM/YYYY'), TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'), TO_DATE(:TanggalUndangan,'DD/MM/YYYY HH24:MI'), " +
                            "       :NomorSurat, :NomorAgenda, :Perihal, :PengirimSurat, :PenerimaSurat, :Arah, :Kategori, " +
                            "       :TipeSurat, :SifatSurat, :KeteranganSurat, :JumlahLampiran, :IsiSingkatSurat, :NamaSeksi, :KodeKlasifikasi, :Referensi, utl_raw.cast_to_raw(:Metadata))";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSurat", data.TanggalSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalUndangan", data.TanggalUndangan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", data.NomorAgenda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", data.Perihal));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengirimSurat", data.PengirimSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", data.Arah));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kategori", data.ArahSuratKeluar));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", data.TipeSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", data.SifatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiSingkatSurat", data.IsiSingkatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaSeksi", data.NamaSeksi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeKlasifikasi", data.KodeKlasifikasi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Referensi", data.Referensi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert LAMPIRAN SURAT
                        foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat ( " +
                                    "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                                    "( " +
                                    "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", folderfile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", lampiranSurat.Nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }



                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.UserId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        string suratinboxid = "";
                        int urutan = 0;


                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "            statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "            0,0,1)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip)); // nip Pengirim
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", ""));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            "            suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "            :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        #endregion


                        #region Session Tujuan Surat

                        string profileidba = "";
                        bool belumkirimtuba = true;

                        List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            string profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            if (tujuanSurat.Redaksi == "Asli")
                            {
                                data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            }

                            int statusterkunci = 0;

                            bool IsKirimKeBA = true;

                            if (data.TipeSurat == "Nota Dinas" ||
                                data.TipeSurat == "Surat Pengantar")
                            {
                                IsKirimKeBA = false;
                            }


                            #region Kirim ke TU Persuratan (Pusat)

                            // Cek TU pengirim dan tujuan. Bila sama, tidak insert ke BA
                            if (profileidtu == myprofileidtu)
                            {
                                IsKirimKeBA = false;
                            }

                            if (IsKirimKeBA)
                            {
                                if (belumkirimtuba)
                                {
                                    profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);

                                    if (!string.IsNullOrEmpty(profileidba))
                                    {
                                        statusterkunci = 1;

                                        string pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba, true);
                                        if (string.IsNullOrEmpty(pegawaiidba))
                                        {
                                            pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba);
                                        }
                                        Models.Entities.Pegawai pegawaiBA = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidba);
                                        if (pegawaiBA == null)
                                        {
                                            string namaprofile = dataMasterModel.GetProfileNameFromId(profileidba);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidba;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }


                                        #region Cek Duplikasi

                                        bool BisaKirimSurat = true;

                                        sql =
                                            "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimSurat = false;
                                        }

                                        #endregion

                                        if (BisaKirimSurat)
                                        {
                                            // Insert SURATINBOX
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, id);
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "            statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "            0,1,:Urutan)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                                            // Insert SURATOUTBOXRELASI
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }
                                    }

                                    belumkirimtuba = false;
                                }
                            }

                            #endregion


                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                if (profileidtu != myprofileidtu)
                                {
                                    string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                    if (string.IsNullOrEmpty(pegawaiidtu))
                                    {
                                        pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                    }
                                    Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                    if (pegawaiTU == null)
                                    {
                                        string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidtu;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }


                                    #region Cek Duplikasi

                                    bool BisaKirimKeTUPengolah = true;

                                    string query =
                                        "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimKeTUPengolah = false;
                                    }

                                    #endregion


                                    if (BisaKirimKeTUPengolah)
                                    {
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, id);
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "       :StatusTerkunci,1,:Urutan)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                                        // Insert SURATOUTBOXRELASI (ke Profile TU)
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }

                                    statusterkunci = 1;
                                }
                            }

                            #endregion


                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND NIP = :PegawaiIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPenerima", tujuanSurat.NIP));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :StatusTerkunci,0,:Urutan)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai)); // nama penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }

                            #endregion

                        }

                        #endregion


                        #region Bila Eksternal dan tidak ada Tembusan Surat, kirim ke TU Persuratan (Pusat)

                        if (data.ArahSuratKeluar == "Eksternal")
                        {
                            if (dataSessionTujuanSurat.Count == 0)
                            {
                                profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);
                                if (!string.IsNullOrEmpty(profileidba))
                                {
                                    string pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba, true);
                                    if (string.IsNullOrEmpty(pegawaiidba))
                                    {
                                        pegawaiidba = dataMasterModel.GetPegawaiIdFromProfileId(profileidba);
                                    }
                                    Models.Entities.Pegawai pegawaiBA = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidba);
                                    if (pegawaiBA == null)
                                    {
                                        string namaprofile = dataMasterModel.GetProfileNameFromId(profileidba);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidba;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat Eksternal tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }


                                    #region Cek Duplikasi

                                    bool IsBisaKirim = true;

                                    sql =
                                        "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                    if (jumlahdata > 0)
                                    {
                                        IsBisaKirim = false;
                                    }

                                    #endregion


                                    if (IsBisaKirim)
                                    {
                                        // Insert SURATINBOX
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, id);
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "            statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "            0,1,:Urutan)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", "Asli"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                                        // Insert SURATOUTBOXRELASI
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidba));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                }
                            }
                        }

                        #endregion


                        if (data.ArahSuratKeluar == "Internal")
                        {
                            // Update Table SURAT
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                "       penerima = :PenerimaSurat " +
                                "WHERE suratid = :SuratId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", data.PenerimaSurat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }



                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";

                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT") || tr.Pesan.ToUpper().Contains("I2_SURAT"))
                        {
                            tr.Pesan = "Nomor Surat " + data.NomorSurat + " sudah ada.";
                        }
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

        public Models.Entities.TransactionResult InsertSuratInisiatif(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = myprofileidtu;
                            satkerid = unitkerjaid;
                        }

                        string myClientId = Functions.MyClientId;


                        #region Session Lampiran

                        List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = this.GetListSessionLampiran(myClientId); // data.UserId

                        data.JumlahLampiran = dataSessionLampiran.Count;

                        #endregion


                        string id = GetUID();
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        data.SuratId = id;
                        data.Arah = "Inisiatif";
                        data.PengirimSurat = data.NamaPengirim;


                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.TargetSelesai + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.UserId + " ";
                        metadata += data.CatatanAnda + " ";
                        metadata += data.Kategori + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Insert SURAT
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ( " +
                            "       suratid, kantorid, targetselesai, perihal, pengirim, arah, kategori, " +
                            "       jumlahlampiran, isisingkat, metadata) VALUES " +
                            "( " +
                            "       :Id, :SatkerId, TO_DATE(:TargetSelesai,'DD/MM/YYYY'), :Perihal, :PengirimSurat, :Arah, :Kategori, " +
                            "       :JumlahLampiran, :IsiSingkatSurat, utl_raw.cast_to_raw(:Metadata))";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TargetSelesai", data.TargetSelesai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", data.Perihal));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengirimSurat", data.PengirimSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", data.Arah));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kategori", data.ArahSuratKeluar));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiSingkatSurat", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert LAMPIRAN SURAT
                        foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiransurat ( " +
                                    "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                                    "( " +
                                    "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", folderfile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", lampiranSurat.Nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }



                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "    SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "    :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.UserId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        string suratinboxid = "";
                        int urutan = 0;


                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "            statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "            0,0,1)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip)); // nip Pengirim
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", ""));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            "            suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "            :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        #endregion



                        #region Session Tujuan Surat

                        List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            string profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            //if (tujuanSurat.Redaksi == "Asli")
                            //{
                            //    data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            //}

                            int statusterkunci = 0;


                            #region Kirim ke Tujuan Surat

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, statusurgent, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :StatusTerkunci,0,:StatusUrgent,:Urutan)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai)); // nama penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.IsiSingkatSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusUrgent", tujuanSurat.StatusUrgent));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                try
                                {
                                    new Mobile().KirimNotifikasi(tujuanSurat.NIP, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.IsiSingkatSurat) ? "Surat Inisiatif Baru" : data.IsiSingkatSurat.ToString()), "Surat Inisiatif");
                                }
                                catch (Exception ex)
                                {
                                    var str = ex.Message;
                                }
                            }

                            #endregion

                        }

                        #endregion



                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";

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

        public Models.Entities.TransactionResult EditSurat(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = myprofileidtu;
                            satkerid = unitkerjaid;
                        }


                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.PenerimaSurat + " ";
                        metadata += data.IsiSingkatSurat + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata += data.UserId + " ";
                        metadata += data.TipeSurat + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.Kategori + " ";
                        metadata += data.KeteranganSurat + " ";
                        metadata += data.Sumber_Keterangan + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "       pengirim = :PengirimSurat, " +
                            "       penerima = :PenerimaSurat, " +
                            "       sifatsurat = :SifatSurat, " +
                            "       tipesurat = :TipeSurat, " +
                            "       tanggalsurat = TO_DATE(:TanggalSurat,'DD/MM/YYYY'), " +
                            "       nomorsurat = :NomorSurat, " +
                            "       perihal = :Perihal, " +
                            "       keterangansurat = :KeteranganSurat, " +
                            "       metadata = utl_raw.cast_to_raw(:Metadata) " +
                            "WHERE  suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengirimSurat", data.PengirimSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", data.SifatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", data.TipeSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSurat", data.TanggalSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", data.Perihal));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();



                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        var check = ctx.Database.SqlQuery<string>($"SELECT SUMBER_KETERANGAN FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT WHERE SURAT_ID = '{data.SuratId}'").FirstOrDefault();


                        if (string.IsNullOrEmpty(check) && !string.IsNullOrEmpty(data.Sumber_Keterangan))
                        {
                            ctx.Database.ExecuteSqlCommand($"INSERT INTO {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT (SUMBER_ID, SURAT_ID, SUMBER_KETERANGAN) VALUES (SYS_GUID(), '{data.SuratId}','{data.Sumber_Keterangan}' WHERE SURAT_ID = '{data.SuratId}' )");
                        }
                        else if (!string.IsNullOrEmpty(check))
                        {
                            ctx.Database.ExecuteSqlCommand($"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT SET SUMBER_KETERANGAN = '{data.Sumber_Keterangan}' WHERE SURAT_ID = '{data.SuratId}'");
                        }


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Data surat berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT"))
                        {
                            tr.Pesan = "Nomor Surat " + data.NomorSurat + " sudah ada.";
                        }
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

        public Models.Entities.TransactionResult EditSuratByTU(Models.Entities.Surat data)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.PenerimaSurat + " ";
                        metadata += data.IsiSingkatSurat + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata += data.UserId + " ";
                        metadata += data.TipeSurat + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.Kategori + " ";
                        metadata += data.KeteranganSurat + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "       sifatsurat = :SifatSurat, " +
                            "       tipesurat = :TipeSurat, " +
                            "       tanggalsurat =  TO_DATE(:TanggalSurat,'DD/MM/YYYY'), " +
                            "       tanggalproses =  TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'), " +
                            "       tanggalundangan =  TO_DATE(:TanggalUndangan,'DD/MM/YYYY HH24:MI'), " +
                            "       nomorsurat = :NomorSurat, " +
                            "       perihal = :Perihal, " +
                            "       pengirim = :PengirimSurat, " +
                            "       penerima = :PenerimaSurat, " +
                            "       keterangansurat = :KeteranganSurat, " +
                            "       isisingkat = :IsiSingkatSurat, " +
                            "       metadata = utl_raw.cast_to_raw(:Metadata) " +
                            "WHERE  suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", data.SifatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", data.TipeSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSurat", data.TanggalSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalUndangan", data.TanggalUndangan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", data.Perihal));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengirimSurat", data.PengirimSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiSingkatSurat", data.IsiSingkatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();

                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        var check = ctx.Database.SqlQuery<string>($"SELECT SUMBER_KETERANGAN FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT WHERE SURAT_ID = '{data.SuratId}'").FirstOrDefault();


                        if (string.IsNullOrEmpty(check) && !string.IsNullOrEmpty(data.Sumber_Keterangan))
                        {
                            ctx.Database.ExecuteSqlCommand($"INSERT INTO {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT (SUMBER_ID, SURAT_ID, SUMBER_KETERANGAN) VALUES (SYS_GUID(), '{data.SuratId}','{data.Sumber_Keterangan}' WHERE SURAT_ID = '{data.SuratId}' )");
                        }
                        else if (!string.IsNullOrEmpty(check))
                        {
                            ctx.Database.ExecuteSqlCommand($"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.SUMBER_SURAT SET SUMBER_KETERANGAN = '{data.Sumber_Keterangan}' WHERE SURAT_ID = '{data.SuratId}'");
                        }




                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Data surat berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT"))
                        {
                            tr.Pesan = "Nomor Surat " + data.NomorSurat + " sudah ada.";
                        }
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

        public Models.Entities.TransactionResult KirimSuratMasuk(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileidtu, string nip)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string arahsurat = data.Arah;

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        //bool SalahSatuTidakBisaKirim = false;
                        //string namaTujuanTidakBisaTerkirim = "";

                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            satkerid = unitkerjaid;
                        }

                        string myClientId = Functions.MyClientId;

                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "       statussurat = 1 " +
                            "WHERE  suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "       statusterkirim = 1, keterangan = :Keterangan, perintahdisposisi = :PerintahDisposisi " +
                            "WHERE suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PerintahDisposisi", data.PerintahDisposisi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Get Penerima Surat
                        //Models.Entities.ProfilePegawai dataPegawaiPenerima = dataMasterModel.GetProfilePegawaiByPrimaryKey(data.ProfilePegawaiIdPenerima);


                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "    SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan, perintahdisposisi) VALUES " +
                            "( " +
                            "    :SuratOutboxId,:SuratId,:KantorId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda,:PerintahDisposisi)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PerintahDisposisi", data.PerintahDisposisi));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        #region Session Tujuan Surat

                        string suratinboxid = "";
                        int urutan = 0;

                        List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = new List<Models.Entities.SessionTujuanSurat>();
                        if (data.ListTujuanSurat.Count > 0)
                        {
                            dataSessionTujuanSurat = data.ListTujuanSurat;
                        } else
                        {
                            dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId
                        }
                        

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);
                            string profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            //int statusterkunci = (tujuanSurat.ProfileId == profileidtu) ? 0 : 1;
                            int statusterkunci = (myprofileidtu == profileidtu) ? 0 : 1;

                            // kalau profile tujuan surat buat punya profileid TU, tidak usah kunci suratinbox
                            if (string.IsNullOrEmpty(profileidtu))
                            {
                                statusterkunci = 0;
                            }


                            #region Cek Duplikasi

                            bool BisaKirimSurat = true;

                            //sql =
                            //    "SELECT count(*) FROM suratinbox " +
                            //    "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                            //    "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId " +
                                "AND profilepenerima = :ProfileIdPenerima AND nip = :Nip AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", kantorid));
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahinbox > 0)
                            {
                                BisaKirimSurat = false;
                                //SalahSatuTidakBisaKirim = true;
                                //namaTujuanTidakBisaTerkirim = tujuanSurat.NamaPegawai;
                            }

                            #endregion

                            if (BisaKirimSurat)
                            {
                                // Insert SURATINBOX
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, statusterkunci, statusurgent, redaksi, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:KantorId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:StatusTerkunci,:StatusUrgent,:Redaksi,:Urutan)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId)); // dataPegawaiPenerima.ProfileId
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat | dataPegawaiPenerima.PegawaiId
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusUrgent", tujuanSurat.StatusUrgent));
                                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PerintahDisposisi", data.PerintahDisposisi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId)); // dataPegawaiPenerima.ProfileId
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                #region Kirim Notifikasi Mobile
                                if(statusterkunci == 0)
                                {
                                    try
                                    {
                                        new Mobile().KirimNotifikasi(tujuanSurat.NIP, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.CatatanAnda) ? (string.IsNullOrEmpty(data.PerintahDisposisi) ? "Disposisi Baru" : data.PerintahDisposisi.ToString()) : data.CatatanAnda.ToString()), "Kotak Masuk");
                                    }
                                    catch (Exception ex)
                                    {
                                        var msg = ex.Message;
                                    }
                                }
                                #endregion

                                #region Kirim Email

                                //if (pegawaiTujuan != null)
                                //{
                                //    if (!string.IsNullOrEmpty(pegawaiTujuan.Email))
                                //    {
                                //        List<Models.Entities.LampiranSurat> dataLampiranSurat = GetListLampiranSurat(data.SuratId, "");

                                //        string isSendMail = ConfigurationManager.AppSettings["IsSendMail"].ToString();
                                //        if (isSendMail == "true")
                                //        {
                                //            string alamatEmail = pegawaiTujuan.Email;
                                //            string nomorSuratEmail = (string.IsNullOrEmpty(data.NomorSurat)) ? "" : ("No. " + data.NomorSurat);
                                //            string sifatSurat = (string.IsNullOrEmpty(data.SifatSurat)) ? "" : (" (" + data.SifatSurat + ")");
                                //            string subyekEmail = "Anda telah menerima surat " + nomorSuratEmail + " dari " + data.NamaPengirim + " - " + data.NamaProfilePengirim + sifatSurat;
                                //            string bodyEmail = data.PengirimSurat + ", " + data.TipeSurat + ", " + nomorSuratEmail + " Tgl. " + data.TanggalSurat + ": " + data.IsiSingkatSurat + ".";

                                //            string sendStatus = functions.SendEmail2(alamatEmail, subyekEmail, bodyEmail, dataLampiranSurat);
                                //        }
                                //    }
                                //}

                                #endregion


                                // Insert SURATINBOX (ke Profile TU)
                                if (!string.IsNullOrEmpty(profileidtu))
                                {
                                    if (profileidtu != myprofileidtu) // if (profileidtu != tujuanSurat.ProfileId)
                                    {
                                        string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu,true);
                                        if (string.IsNullOrEmpty(pegawaiidtu))
                                        {
                                            pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                        }
                                        Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                        if (pegawaiTU == null)
                                        {
                                            string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidtu;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }


                                        #region Cek Duplikasi

                                        bool BisaKirimSuratTU = true;

                                        //sql =
                                        //    "SELECT count(*) FROM suratinbox " +
                                        //    "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        //    "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                        sql =
                                            "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", kantorid));
                                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimSuratTU = false;
                                        }

                                        #endregion

                                        if (BisaKirimSuratTU)
                                        {
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, redaksi, " +
                                                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "       :SuratInboxId,:SuratId,:KantorId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Redaksi, " +
                                                "       0,1,:Urutan)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap));// Nama TU nya penerima surat
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                                            // Insert SURATOUTBOXRELASI (ke Profile TU)
                                            sql =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }
                                    }
                                }
                            }
                        }

                        #endregion


                        //if (SalahSatuTidakBisaKirim)
                        //{
                        //    tc.Rollback();
                        //    tr.Status = false;
                        //    tr.ReturnValue = "";
                        //    tr.Pesan = "Surat gagal dikirim karena " + namaTujuanTidakBisaTerkirim + " sudah pernah menerima surat tersebut";
                        //}
                        //else
                        //{
                        //    tc.Commit();
                        //    //tc.Rollback(); // for test
                        //    tr.Status = true;
                        //    tr.ReturnValue = data.SuratId;
                        //    tr.Pesan = "Surat berhasil dikirim";
                        //}

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public Models.Entities.TransactionResult KirimSuratKeluar(Models.Entities.Surat data, string kantorid, string unitkerjaid, string myprofileidtu, string nip)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string arahsurat = data.Arah;

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            //satkerid = myprofileidtu;
                            satkerid = unitkerjaid;
                        }

                        string myClientId = Functions.MyClientId;

                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "       statussurat = 1 " +
                            "WHERE  suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "    statusterkirim = 1, keterangan = :Keterangan, perintahdisposisi = :PerintahDisposisi " +
                            "  WHERE suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PerintahDisposisi", data.PerintahDisposisi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan, perintahdisposisi) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:KantorId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda,:PerintahDisposisi)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PerintahDisposisi", data.PerintahDisposisi));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);



                        #region Session Tujuan Surat

                        string suratinboxid = "";
                        int urutan = 0;

                        List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            string profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            //if (tujuanSurat.Redaksi == "Asli")
                            //{
                            //    data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            //}

                            //int statusterkunci = 0;
                            int statusterkunci = (myprofileidtu == profileidtu) ? 0 : 1;

                            // kalau profile tujuan surat buat punya profileid TU, tidak usah kunci suratinbox
                            if (string.IsNullOrEmpty(profileidtu))
                            {
                                statusterkunci = 0;
                            }

                            int jumlahinbox = 0;


                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                if (profileidtu != myprofileidtu)
                                {
                                    statusterkunci = 1;

                                    string pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu, true);
                                    if (string.IsNullOrEmpty(pegawaiidtu))
                                    {
                                        pegawaiidtu = dataMasterModel.GetPegawaiIdFromProfileId(profileidtu);
                                    }

                                    Models.Entities.Pegawai pegawaiTU = dataMasterModel.GetPegawaiByPegawaiId(pegawaiidtu);

                                    if (pegawaiTU == null)
                                    {
                                        string namaprofile = dataMasterModel.GetProfileNameFromId(profileidtu);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidtu;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }


                                    #region Cek Duplikasi

                                    bool BisaKirimKeTUPengolah = true;

                                    sql =
                                        "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimKeTUPengolah = false;
                                    }

                                    #endregion

                                    if (BisaKirimKeTUPengolah)
                                    {
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, redaksi, " +
                                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "       :SuratInboxId,:SuratId,:KantorId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Redaksi, " +
                                            "       0,1,:Urutan)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap));// Nama TU nya penerima surat
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                                        // Insert SURATOUTBOXRELASI (ke Profile TU)
                                        sql =
                                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtu));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                }
                            }

                            #endregion


                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            #region Cek Duplikasi

                            bool BisaKirimSurat = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND nip = :Nip AND statusterkunci = 0 AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahinbox > 0)
                            {
                                BisaKirimSurat = false;
                            }

                            #endregion

                            if (BisaKirimSurat)
                            {
                                // Insert SURATINBOX
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, statusterkunci, statusurgent, redaksi, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:KantorId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:StatusTerkunci,:StatusUrgent,:Redaksi,:Urutan)";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", "")); // data.CatatanAnda
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusTerkunci", statusterkunci));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusUrgent", tujuanSurat.StatusUrgent));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratoutboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusBaca", "D"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }

                            #endregion

                        }

                        #endregion



                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public Models.Entities.TransactionResult SimpanCatatanAnda(Models.Entities.Surat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Update Table SURATINBOX (Update Catatan Anda)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "         keterangan = :CatatanAnda " +
                            "  WHERE suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Catatan Anda berhasil disimpan";
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

        public Models.Entities.TransactionResult ProsesSuratMasuk(Models.Entities.Surat data, string kantorid, string satkerid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawai)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string arahsurat = data.Arah;

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string profileidtu = dataMasterModel.GetProfileIdTuByNip(nip, kantorid);

                        // Cek bila profileid nya Delegasi untuk Tugas TU
                        if (string.IsNullOrEmpty(profileidtu))
                        {
                            string profileidtudelegasi = dataMasterModel.GetProfileIdTuByDelegasi(myprofileid, nip);
                            if (!string.IsNullOrEmpty(profileidtudelegasi))
                            {
                                profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(myprofileid);
                            }
                        }

                        if (!string.IsNullOrEmpty(profileidtu))
                        {
                            // Update Table SURAT
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                "         statussurat = 1, tanggalproses = SYSDATE " +
                                "  WHERE suratid = :SuratId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                "         statusterkirim = 1, tanggalbuka = SYSDATE, keterangan = :CatatanAnda " +
                                "  WHERE suratinboxid = :SuratInboxId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // update tanggalterima, bila kosong
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                "    tanggalterima = SYSDATE " +
                                "  WHERE tanggalterima IS NULL AND suratinboxid = :SuratInboxId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Mark status read
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi SET " +
                                "    statusbaca = 'R' " +
                                "  WHERE suratinboxid = :SuratInboxId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Update Table SURATINBOX (Update status SuratInbox di Tujuan Surat oleh TU, menjadi tidak terkunci)
                            string profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);
                            if (!string.IsNullOrEmpty(profileidba))
                            {
                                //if (myprofileid == profileidba)
                                if (myprofileid.Contains(profileidba))
                                {
                                    // Bila Proses oleh TU Persuratan Pusat
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                        "    statusterkunci = 0, TANGGALKIRIM = SYSDATE " +
                                        "WHERE statusterkunci = 1 AND statusforwardtu = 1 AND suratid = :SuratId";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    //// Update Surat Keluar menjadi Surat Masuk di TU Pengolah
                                    //sql =
                                    //    @"UPDATE surat SET
                                    //             arah = 'Masuk'
                                    //      WHERE suratid = :SuratId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    //arrayListParameters.Clear();
                                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    //parameters = arrayListParameters.OfType<object>().ToArray();
                                    //ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    // Bila TU tujuan Surat adalah TU Persuratan Pusat, unlock Surat Inbox di jabatan tujuan surat
                                    sql = "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratid = :SuratId";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                    if (jumlahinbox <= 3)
                                    {
                                        sql =
                                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                            "    statusterkunci = 0, TANGGALKIRIM = SYSDATE " +
                                            "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId ";
                                        //"      AND profilepenerima IN " +
                                        //"          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", profileidtu));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }


                                    // Bila MailRoom kirim Surat ke TU Tujuan saja, bukan pejabat Tujuan Surat
                                    sql =
                                        "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratinbox.suratid = :SuratId " +
                                        "    AND profilepenerima IN (SELECT profileid FROM jabatan WHERE profileid = profileidtu) " +
                                        "    AND statusterkunci = 1 AND statusforwardtu = 0";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                    if (jumlahinbox > 0)
                                    {
                                        sql =
                                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                            "    statusterkunci = 0, TANGGALKIRIM = SYSDATE " +
                                            "WHERE suratinbox.suratinboxid IN (" +
                                            "   SELECT suratinbox.suratinboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "   WHERE suratinbox.suratid = :SuratId " +
                                            "      AND profilepenerima IN (SELECT profileid FROM jabatan WHERE profileid = profileidtu) " +
                                            "      AND statusterkunci = 1 AND statusforwardtu = 0 " +
                                            ")";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                }
                                else
                                {
                                    // Bila Proses oleh TU Pengolah
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                        "    statusterkunci = 0, TANGGALKIRIM = SYSDATE " +
                                        "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId " +
                                        "      AND profilepenerima IN " +
                                        "          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", profileidtu));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                            else
                            {
                                // Bila Proses oleh Satker Daerah (Perlu diuji)
                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                    "    statusterkunci = 0, TANGGALKIRIM = SYSDATE " +
                                    "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId " +
                                    "      AND profilepenerima IN " +
                                    "          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", profileidtu));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                // Update Surat Keluar menjadi Surat Masuk di TU Pengolah
                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                    "             arah = 'Masuk' " +
                                    "      WHERE suratid = :SuratId";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }


                            //// Buat Nomor Agenda di TU Pengolah
                            //string query =
                            //    @"SELECT nomoragenda
                            //        FROM agendasurat
                            //        WHERE suratid = :SuratId AND kantorid = :SatkerId";
                            //query = sWhitespace.Replace(query, " ");
                            //arrayListParameters.Clear();
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //parameters = arrayListParameters.OfType<object>().ToArray();
                            //string nomoragenda = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                            //if (string.IsNullOrEmpty(nomoragenda))
                            //{
                            //    Models.KonterModel kontermodel = new Models.KonterModel();
                            //    data.NomorAgendaSurat = kontermodel.GetNomorAgendaSuratAndUpdate(kantorid, unitkerjaid, profileidtu, data.SuratId);
                            //}

                            // Buat Nomor Agenda di TU Pengolah
                            var _na = new SuratModel().GetNomorAgenda(data.SuratId, satkerid, unitkerjaid, data.SuratInboxId);
                            if (_na.Status)
                            {
                                data.NomorAgendaSurat = _na.ReturnValue;
                            }
                            //string query =
                            //    "SELECT nomoragenda " +
                            //    "    FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                            //    "    WHERE suratid = :SuratId";
                            ////query = sWhitespace.Replace(query, " ");
                            //arrayListParameters.Clear();
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            //parameters = arrayListParameters.OfType<object>().ToArray();
                            //string nomoragenda = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                            //if (string.IsNullOrEmpty(nomoragenda))
                            //{
                            //    int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                            //    // Cek Konter Agenda
                            //    query = "select count(*) from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";
                            //    arrayListParameters.Clear();
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                            //    parameters = arrayListParameters.OfType<object>().ToArray();
                            //    int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                            //    if (jumlahrecord == 0)
                            //    {
                            //        // Bila tidak ada, Insert KONTERSURAT
                            //        query =
                            //            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat ( " +
                            //            "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                            //            "( " +
                            //            "       SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                            //        //query = sWhitespace.Replace(query, " ");
                            //        arrayListParameters.Clear();
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", "Agenda"));
                            //        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            //        parameters = arrayListParameters.OfType<object>().ToArray();
                            //        ctx.Database.ExecuteSqlCommand(query, parameters);
                            //    }

                            //    // Get Nilai
                            //    decimal nilainomoragenda = 1;

                            //    sql =
                            //        "select nilaikonter+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                            //        "FOR UPDATE NOWAIT";
                            //    arrayListParameters.Clear();
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                            //    parameters = arrayListParameters.OfType<object>().ToArray();

                            //    nilainomoragenda = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault();


                            //    sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                            //    arrayListParameters.Clear();
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiKonter", nilainomoragenda));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                            //    parameters = arrayListParameters.OfType<object>().ToArray();
                            //    ctx.Database.ExecuteSqlCommand(sql, parameters);


                            //    // Binding Nomor Agenda
                            //    int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                            //    string strBulan = Functions.NomorRomawi(bulan);
                            //    string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
                            //    string kodesurat = "AG-";

                            //    string nomoragendasurat = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());

                            //    // Update SURAT field NOMORAGENDA
                            //    sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET nomoragenda = :NomorAgenda WHERE nomoragenda IS NULL AND suratid = :SuratId";
                            //    arrayListParameters.Clear();
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", nomoragendasurat));
                            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            //    parameters = arrayListParameters.OfType<object>().ToArray();
                            //    ctx.Database.ExecuteSqlCommand(sql, parameters);

                                //data.NomorAgendaSurat = nomoragendasurat;
                            //}
                        }
                        else
                        {
                            tc.Dispose();
                            ctx.Dispose();
                            tr.Pesan = "Gagal mendapatkan Profile Tata Usaha";

                            return tr;
                        }


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public Entities.TransactionResult ProsesSuratMasukMassive(List<Models.Entities.SuratIds> suratIds, string kantorid, string satkerid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawai, string catatananda)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string profileidtu = dataMasterModel.GetProfileIdTuByNip(nip, kantorid);

                        // Cek bila profileid nya Delegasi untuk Tugas TU
                        if (string.IsNullOrEmpty(profileidtu))
                        {
                            string profileidtudelegasi = dataMasterModel.GetProfileIdTuByDelegasi(myprofileid, nip);
                            if (!string.IsNullOrEmpty(profileidtudelegasi))
                            {
                                profileidtu = dataMasterModel.GetProfileIdTuFromProfileId(myprofileid);
                            }
                        }

                        if (!string.IsNullOrEmpty(profileidtu))
                        {
                            int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                            foreach (var surat in suratIds)
                            {
                                if (!string.IsNullOrEmpty(surat.suratid) && !string.IsNullOrEmpty(surat.suratinboxid))
                                {
                                    // Update Table SURAT
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                        "         statussurat = 1, tanggalproses = SYSDATE " +
                                        "  WHERE suratid = :SuratId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                        "         statusterkirim = 1, tanggalkirim = SYSDATE, tanggalbuka = SYSDATE, keterangan = :CatatanAnda " +
                                        "  WHERE suratinboxid = :SuratInboxId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", catatananda));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", surat.suratinboxid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    // update tanggalterima, bila kosong
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                        "         tanggalterima = SYSDATE " +
                                        "  WHERE tanggalterima IS NULL AND suratinboxid = :SuratInboxId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", surat.suratinboxid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    // Mark status read
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi SET " +
                                        "         statusbaca = 'R' " +
                                        "  WHERE suratinboxid = :SuratInboxId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", surat.suratinboxid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    // Update Table SURATINBOX (Update status SuratInbox di Tujuan Surat oleh TU, menjadi tidak terkunci)
                                    string profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);
                                    if (!string.IsNullOrEmpty(profileidba))
                                    {
                                        //if (myprofileid == profileidba)
                                        if (myprofileid.Contains(profileidba))
                                        {
                                            // Bila Proses oleh TU Persuratan Pusat
                                            sql =
                                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                                "    statusterkunci = 0 " +
                                                "WHERE statusterkunci = 1 AND statusforwardtu = 1 AND suratid = :SuratId";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                                            // Bila TU tujuan Surat adalah TU Persuratan Pusat, unlock Surat Inbox di jabatan tujuan surat
                                            sql = "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratid = :SuratId";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                            if (jumlahinbox <= 3)
                                            {
                                                sql =
                                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                                    "    statusterkunci = 0 " +
                                                    "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId ";
                                                //"      AND profilepenerima IN " +
                                                //"          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", profileidtu));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }


                                            // Bila MailRoom kirim Surat ke TU Tujuan saja, bukan pejabat Tujuan Surat
                                            sql =
                                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratinbox.suratid = :SuratId " +
                                                "    AND profilepenerima IN (SELECT profileid FROM jabatan WHERE profileid = profileidtu) " +
                                                "    AND statusterkunci = 1 AND statusforwardtu = 0";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                            if (jumlahinbox > 0)
                                            {
                                                sql =
                                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                                    "    statusterkunci = 0 " +
                                                    "WHERE suratinbox.suratinboxid IN (" +
                                                    "   SELECT suratinbox.suratinboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                                                    "   WHERE suratinbox.suratid = :SuratId " +
                                                    "      AND profilepenerima IN (SELECT profileid FROM jabatan WHERE profileid = profileidtu) " +
                                                    "      AND statusterkunci = 1 AND statusforwardtu = 0 " +
                                                    ")";
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                        }
                                        else
                                        {
                                            // Bila Proses oleh TU Pengolah
                                            sql =
                                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                                "    statusterkunci = 0 " +
                                                "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId " +
                                                "      AND profilepenerima IN " +
                                                "          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", profileidtu));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }
                                    }
                                    else
                                    {
                                        // Bila Proses oleh Satker Daerah (Perlu diuji)
                                        sql =
                                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                            "    statusterkunci = 0 " +
                                            "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId " +
                                            "      AND profilepenerima IN " +
                                            "          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", profileidtu));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                                        // Update Surat Keluar menjadi Surat Masuk di TU Pengolah
                                        sql =
                                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                            "         arah = 'Masuk' " +
                                            "  WHERE suratid = :SuratId";
                                        //sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }


                                    //// Buat Nomor Agenda di TU Pengolah
                                    //string query =
                                    //    @"SELECT nomoragenda
                                    //      FROM agendasurat
                                    //      WHERE suratid = :SuratId AND kantorid = :SatkerId";
                                    //query = sWhitespace.Replace(query, " ");
                                    //arrayListParameters.Clear();
                                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                    //parameters = arrayListParameters.OfType<object>().ToArray();
                                    //string nomoragenda = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                                    //if (string.IsNullOrEmpty(nomoragenda))
                                    //{
                                    //    Models.KonterModel kontermodel = new Models.KonterModel();
                                    //    string nomorAgendaSurat = kontermodel.GetNomorAgendaSuratAndUpdate(kantorid, unitkerjaid, profileidtu, surat.suratid);
                                    //}

                                    // Buat Nomor Agenda di TU Pengolah
                                    string query =
                                        "SELECT nomoragenda " +
                                        "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                                        "WHERE suratid = :SuratId";
                                    //query = sWhitespace.Replace(query, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    string nomoragenda = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                                    if (string.IsNullOrEmpty(nomoragenda))
                                    {
                                        // Cek Konter Agenda
                                        query = "select count(*) from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                                        if (jumlahrecord == 0)
                                        {
                                            // Bila tidak ada, Insert KONTERSURAT
                                            query =
                                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                                "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                                "( " +
                                                "       SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                                            //query = sWhitespace.Replace(query, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", "Agenda"));
                                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(query, parameters);
                                        }

                                        // Get Nilai
                                        decimal nilainomoragenda = 1;

                                        sql =
                                            "select nilaikonter+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                                            "FOR UPDATE NOWAIT";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                                        parameters = arrayListParameters.OfType<object>().ToArray();

                                        nilainomoragenda = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault();


                                        sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiKonter", nilainomoragenda));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Agenda"));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                                        // Binding Nomor Agenda
                                        int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                                        string strBulan = Functions.NomorRomawi(bulan);
                                        string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
                                        string kodesurat = "AG-";

                                        string nomoragendasurat = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());

                                        // Update SURAT field NOMORAGENDA
                                        sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET nomoragenda = :NomorAgenda WHERE nomoragenda IS NULL AND suratid = :SuratId";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", nomoragendasurat));
                                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                                        string nomorAgendaSurat = nomoragendasurat;
                                    }


                                }
                            }
                        }
                        else
                        {
                            tc.Dispose();
                            ctx.Dispose();
                            tr.Pesan = "Gagal mendapatkan Profile Tata Usaha";

                            return tr;
                        }

                        tc.Commit();
                        //tc.Rollback(); // test
                        tr.Status = true;
                        tr.Pesan = "Surat berhasil diproses";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        //if (tr.Pesan.ToLower().Contains("unique constraint"))
                        //{
                        //    tr.Pesan = "Nomor Surat Pengantar tersebut sudah ada.";
                        //}
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

        public Models.Entities.TransactionResult ProsesSuratKeluar(Models.Entities.Surat data, string kantorid, string satkerid, string myprofileid, string myprofileidtu, string nip)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string arahsurat = data.Arah;

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string profileidtu = dataMasterModel.GetProfileIdTuByNip(nip, kantorid);
                        if (!string.IsNullOrEmpty(profileidtu))
                        {
                            // Update Table SURAT
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                "         statussurat = 1 " +
                                "  WHERE suratid = :SuratId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                "         statusterkirim = 1, tanggalkirim = SYSDATE, keterangan = :CatatanAnda " +
                                "  WHERE suratinboxid = :SuratInboxId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Update Table SURATINBOX (Update status SuratInbox di Tujuan Surat oleh TU, menjadi tidak terkunci)
                            string profileidba = dataMasterModel.GetProfileIdBAFromProfileId(myprofileidtu);
                            if (!string.IsNullOrEmpty(profileidba))
                            {
                                //if (myprofileid == profileidba)
                                if (myprofileid.Contains(profileidba))
                                {
                                    // Bila Proses oleh TU Persuratan
                                    //sql =
                                    //    "UPDATE suratinbox SET " +
                                    //    "    statusterkunci = 0 " +
                                    //    "WHERE statusterkunci = 1 AND statusforwardtu = 1 AND suratid = :SuratId";
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                        "    statusterkunci = 0 " +
                                        "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId " +
                                        "      AND profilepenerima IN " +
                                        "          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", myprofileid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);

                                    // Update Surat Keluar menjadi Surat Masuk di TU Pengolah
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                        "         arah = 'Masuk' " +
                                        "  WHERE suratid = :SuratId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                                else
                                {
                                    // Bila Proses oleh TU Pengolah
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                        "    statusterkunci = 0 " +
                                        "WHERE suratinbox.statusterkunci = 1 AND suratinbox.suratid = :SuratId " +
                                        "      AND profilepenerima IN " +
                                        "          (SELECT profileid FROM jabatan WHERE profileidtu = :ProfileIdTU)";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTU", myprofileid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                            else
                            {
                                // Bila Proses oleh Satker Daerah (Perlu diuji)
                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                    "    statusterkunci = 0 " +
                                    "WHERE statusterkunci = 1 AND suratid = :SuratId";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                // Update Surat Keluar menjadi Surat Masuk di TU Pengolah
                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                    "             arah = 'Masuk' " +
                                    "      WHERE suratid = :SuratId";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }


                            // Bila Surat Keluar External, dan tidak ada tembusan, langsung close Surat
                            if (data.Kategori == "Eksternal")
                            {

                                #region ARSIPKAN SURAT

                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                    "         tanggalarsip = SYSDATE " +
                                    "  WHERE  suratid = :SuratId";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                                    "         nip = :Nip, namapegawai = :NamaPegawai, keterangan = :CatatanAnda, tindaklanjut = 'Selesai' " +
                                    "  WHERE  suratinboxid = :SuratInboxId";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                // Insert ARSIPSURAT
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ( " +
                                    "       arsipsuratid, suratid, kantorid, tanggal, profilearsip) VALUES " +
                                    "( " +
                                    "       :ArsipSuratId,:SuratId,:KantorId,SYSDATE,:ProfileIdPengirim)";
                                //sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ArsipSuratId", this.GetUID()));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                #endregion


                                // Update Status Surat
                                string query = "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratid = :SuratId";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                int jumlahinbox = ctx.Database.SqlQuery<int>(query, parameters).First();
                                if (jumlahinbox == 1)
                                {
                                    // Bila tidak ada tembusan, close Surat (0)
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                        "         statussurat = 0 " +
                                        "  WHERE  suratid = :SuratId";
                                    //sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                                //                                else if (jumlahinbox > 1)
                                //                                {
                                //                                    // Bila ada tembusan, flag Surat = 8
                                //                                    sql =
                                //                                        @"UPDATE surat SET
                                //                                                 statussurat = 8
                                //                                          WHERE  suratid = :SuratId";
                                //                                    sql = sWhitespace.Replace(sql, " ");
                                //                                    arrayListParameters.Clear();
                                //                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                                //                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                //                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                //                                }
                            }
                        }
                        else
                        {
                            tc.Dispose();
                            ctx.Dispose();
                            tr.Pesan = "Gagal mendapatkan Profile Tata Usaha";

                            return tr;
                        }


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public Models.Entities.TransactionResult SimpanSuratKeluar(Models.Entities.Surat data, string kantorid, string unitkerjaid, string profileidtu)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string arahsurat = data.Arah;

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            // Update KONTERSURAT
            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Surat.Models.Entities.Surat surat = GetSuratBySuratInboxId(data.SuratInboxId, satkerid, data.SuratId);

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += surat.Perihal + " ";
                        metadata += surat.PengirimSurat + " ";
                        metadata += surat.PenerimaSurat + " ";
                        metadata += surat.IsiSingkatSurat + " ";
                        metadata += surat.NamaPenerima + " ";
                        metadata += surat.UserId + " ";
                        metadata += surat.TipeSurat + " ";
                        metadata += surat.SifatSurat + " ";
                        metadata += data.CatatanAnda + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "         nomorsurat = :NomorSurat, nomoragenda = :NomorAgenda, metadata = utl_raw.cast_to_raw(:Metadata) " +
                            "  WHERE suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", data.NomorAgenda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table SURATINBOX (Update Catatan Anda)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "         keterangan = :Keterangan " +
                            "  WHERE suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Data berhasil disimpan";
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

        public Models.Entities.TransactionResult SimpanNomorSurat(Models.Entities.Surat data, string kantorid, string unitkerjaid, string profileidtu)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Surat.Models.Entities.Surat surat = GetSuratBySuratInboxId(data.SuratInboxId, satkerid, data.SuratId);

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += surat.Perihal + " ";
                        metadata += surat.PengirimSurat + " ";
                        metadata += surat.PenerimaSurat + " ";
                        metadata += surat.IsiSingkatSurat + " ";
                        metadata += surat.NamaPenerima + " ";
                        metadata += surat.UserId + " ";
                        metadata += surat.TipeSurat + " ";
                        metadata += surat.SifatSurat + " ";
                        metadata += data.CatatanAnda + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "         nomorsurat = :NomorSurat, tanggalsurat = SYSDATE, metadata = utl_raw.cast_to_raw(:Metadata) " +
                            "  WHERE suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Data berhasil disimpan";
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

        public Models.Entities.TransactionResult SimpanTargetSelesaiSuratMasuk(Models.Entities.Surat data, string kantorid, string unitkerjaid, string profileidtu)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            string satkerid = kantorid;
            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Update Table AGENDASURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat SET " +
                            "         targetselesai = TO_DATE(:TargetSelesaiSuratMasuk,'DD/MM/YYYY') " +
                            "  WHERE suratid = :SuratId AND kantorid = :SatkerId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TargetSelesaiSuratMasuk", data.TargetSelesaiSuratMasuk));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
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

        public Models.Entities.TransactionResult ArsipSurat(Models.Entities.Surat data, string kantorid, string satkerid, string nip)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (!data.Redaksi.ToLower().Equals("tembusan"))
                        {
                            // Update Table SURAT
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                                "    tanggalarsip = SYSDATE " +
                                "  WHERE suratid = :SuratId";
                            //sql =
                            //    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            //    "    statussurat = 0, tanggalarsip = SYSDATE " +
                            //    "  WHERE suratid = :SuratId";
                            //sql =
                            //    @"UPDATE surat SET
                            //        tanggalarsip = SYSDATE
                            //      WHERE suratid = :SuratId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            sql = string.Format(@"
                                SELECT DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID) AS UNITKERJAID, JB.PROFILEID
                                FROM {0}.SURATINBOX SI
                                INNER JOIN JABATAN JB ON
	                                JB.PROFILEID = SI.PROFILEPENERIMA AND
	                                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                                INNER JOIN UNITKERJA UK ON
	                                UK.UNITKERJAID = JB.UNITKERJAID
                                WHERE
	                                SI.SURATINBOXID = :SuratInboxId", System.Web.Mvc.OtorisasiUser.NamaSkema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            var _profile = ctx.Database.SqlQuery<Entities.Profile>(sql, parameters).FirstOrDefault();

                            // Insert ARSIPSURAT
                            string newid = GetUID();
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ( " +
                                "       arsipsuratid, suratid, kantorid, tanggal, profilearsip) VALUES " +
                                "( " +
                                "      :ArsipSuratId,:SuratId,:KantorId,SYSDATE,:ProfileIdPengirim)";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ArsipSuratId", newid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", _profile.UnitKerjaId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", _profile.ProfileId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "         nip = :Nip, namapegawai = :NamaPegawai, keterangan = :CatatanAnda, tindaklanjut = 'Selesai' " +
                            "  WHERE  suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil diarsip";
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

        public Models.Entities.TransactionResult TolakSurat(Models.Entities.Surat data)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string suratinboxidfirstrow = GetSuratInboxIdFirstRow(data.SuratId);

                        // Update Table SURAT
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET " +
                            "    statussurat = 1, tanggaltolak = SYSDATE " +
                            "  WHERE suratid = :SuratId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table SURATINBOX (Update SELAIN suratinbox first row)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "         statusterkirim = 0, statusterkunci = 1 " +
                            "  WHERE  suratid = :SuratId AND suratinboxid <> :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxidfirstrow));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table SURATINBOX (Update suratinbox first row)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "         statusterkirim = 0, statusterkunci = 0 " +
                            "  WHERE  suratid = :SuratId AND suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxidfirstrow));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table SURATOUTBOX (Update keterangan)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox SET " +
                            "       keterangan = :CatatanAnda " +
                            "WHERE  suratoutboxid IN ( " +
                            "       SELECT suratoutboxid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutboxrelasi  " +
                            "       WHERE suratinboxid = :SuratInboxId)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", suratinboxidfirstrow));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);




                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikembalikan ke Mail Room";
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

        public Models.Entities.TransactionResult KembalikanSurat(Models.Entities.SuratKembali data)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string namapengirim = ".";

                        Entities.SuratKembali suratinbox4suratkembali = GetSuratInboxForSuratKembali(data.SuratInboxId);

                        if (suratinbox4suratkembali != null)
                        {
                            namapengirim = " ke " + suratinbox4suratkembali.NamaPenerima + ".";

                            // Insert SURATKEMBALI
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratkembali ( " +
                                "       suratkembaliid, suratinboxid, suratoutboxid, tanggalkembali, keterangan, " +
                                "       profilepengirim, nippengirim, namapengirim, " +
                                "       profilepenerima, nippenerima, namapenerima) VALUES " +
                                "( " +
                                "       SYS_GUID(), :SuratInboxId, :SuratOutboxId, SYSDATE, :Keterangan, " +
                                "       :ProfilePengirim, :NipPengirim, :NamaPengirim, " +
                                "       :ProfilePenerima, :NipPenerima, :NamaPenerima)";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId)); // inbox pengirim
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratOutboxId", suratinbox4suratkembali.SuratOutboxId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.Keterangan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfilePengirim", suratinbox4suratkembali.ProfilePengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengirim", suratinbox4suratkembali.NipPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", suratinbox4suratkembali.NamaPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfilePenerima", suratinbox4suratkembali.ProfilePenerima));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPenerima", suratinbox4suratkembali.NipPenerima));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPenerima", suratinbox4suratkembali.NamaPenerima));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            //// Delete Table SURATOUTBOXRELASI user yang kembalikan
                            //sql = @"DELETE FROM suratoutboxrelasi WHERE suratinboxid = :SuratInboxId";
                            //sql = sWhitespace.Replace(sql, " ");
                            //arrayListParameters.Clear();
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            //parameters = arrayListParameters.OfType<object>().ToArray();
                            //ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Delete Table SURATINBOX user yang kembalikan
                            //sql = @"DELETE FROM suratinbox WHERE suratinboxid = :SuratInboxId";
                            sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET STATUSHAPUS = '1', USERHAPUS = :UserId WHERE suratinboxid = :SuratInboxId";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UserId", data.UserId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikembalikan" + namapengirim;
                    }
                    catch (Exception ex)
                    {
                        string errmess = ex.Message.ToString();
                        if (errmess.Contains("ORA-01400"))
                        {
                            errmess = "Catatan wajib diisi";
                        }
                        tc.Rollback();
                        tr.Pesan = errmess;
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

        public Models.Entities.TransactionResult SelesaiSuratInbox(Models.Entities.Surat data, string kantorid, string satkerid, string nip)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Update Table SURATINBOX (Update status SuratInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox SET " +
                            "         nip = :Nip, namapegawai = :NamaPegawai, keterangan = :CatatanAnda, tindaklanjut = 'Selesai', statusterkirim = 1 " +
                            "  WHERE  suratinboxid = :SuratInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", data.SuratInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        //// Insert ARSIPSURAT
                        //string newid = GetUID();
                        //sql =
                        //    @"INSERT INTO arsipsurat (
                        //        arsipsuratid, suratid, kantorid, tanggal, profilearsip) VALUES 
                        //    (
                        //        :ArsipSuratId,:SuratId,:KantorId,SYSDATE,:ProfileIdPengirim)";
                        //sql = sWhitespace.Replace(sql, " ");
                        //arrayListParameters.Clear();
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ArsipSuratId", newid));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", satkerid));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        //parameters = arrayListParameters.OfType<object>().ToArray();
                        //ctx.Database.ExecuteSqlCommand(sql, parameters);



                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil diarsip";
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

        #endregion


        #region Surat Pengantar

        public List<Models.Entities.PengantarSurat> GetSuratPengantar(string id, string satkerid, string metadata, int from, int to)
        {
            List<Models.Entities.PengantarSurat> records = new List<Models.Entities.PengantarSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY pengantarsurat.tanggaldari, pengantarsurat.tanggalsampai, pengantarsurat.nomor, pengantarsurat.pengantarsuratid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        pengantarsurat.pengantarsuratid, pengantarsurat.kantorid, pengantarsurat.nomor, pengantarsurat.tujuan, pengantarsurat.namapenerima, " +
                "        to_char(pengantarsurat.tanggaldari, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') TanggalDari, " +
                "        to_char(pengantarsurat.tanggalsampai, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') TanggalSampai, " +
                "        to_char(pengantarsurat.tanggalterima, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') TanggalTerima, " +
                "        to_char(pengantarsurat.tanggaldari, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' s.d ' || " +
                "                to_char(pengantarsurat.tanggalsampai, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') Waktu, " +
                "        to_char(pengantarsurat.tanggalterima, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalTerima " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengantarsurat " +
                "    WHERE " +
                "        pengantarsurat.pengantarsuratid IS NOT NULL ";

            if (!String.IsNullOrEmpty(id))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                query += " AND (pengantarsurat.pengantarsuratid = :Id) ";
            }
            if (!String.IsNullOrEmpty(satkerid))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                query += " AND (pengantarsurat.kantorid = :SatkerId) ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata2", String.Concat("%", metadata.ToLower(), "%")));
                query +=
                    " AND (LOWER(utl_raw.cast_to_varchar2(pengantarsurat.metadata)) LIKE :Metadata " +
                    "      OR exists (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".detilpengantar WHERE detilpengantar.pengantarsuratid = pengantarsurat.pengantarsuratid " +
                    "                 AND LOWER(utl_raw.cast_to_varchar2(detilpengantar.metadata)) LIKE :Metadata2))";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.PengantarSurat>(query, parameters).ToList<Models.Entities.PengantarSurat>();
            }

            return records;
        }

        public Entities.TransactionResult InsertSuratPengantar(Models.Entities.FindPengantarSurat data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.Nomor + " ";
                        metadata += data.Tujuan + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata = metadata.Trim();
                        #endregion

                        string id = GetUID();

                        data.PengantarSuratId = id;

                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengantarsurat ( " +
                            "       pengantarsuratid, kantorid, nomor, tanggaldari, tujuan, tanggalterima, namapenerima, metadata) VALUES " +
                            "( " +
                            "      :PengantarSuratId, :SatkerId, :Nomor, TO_DATE(:TanggalDari,'DD/MM/YYYY'), :Tujuan, TO_DATE(:TanggalTerima,'DD/MM/YYYY'), :NamaPenerima, utl_raw.cast_to_raw(:Metadata))";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengantarSuratId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", data.KantorId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", data.Nomor));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", data.TanggalDari));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSampai", data.TanggalSampai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tujuan", data.Tujuan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPenerima", data.NamaPenerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.PengantarSuratId;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Nomor Surat Pengantar tersebut sudah ada.";
                        }
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

        public Entities.TransactionResult BuatSuratPengantar(List<Models.Entities.SuratIds> suratIds, int tipekantorid, string kantorid, string satkerid, string unitkerjaid, string profileidtu, out string nomorsp)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            nomorsp = "";

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string profileidtujuan = suratIds[0].profileidtujuan;
                        string namapenerima = suratIds[0].namapenerima;
                        string tanggalinput = suratIds[0].tanggalinput;
                        //string tanggaldari = suratIds[0].tanggaldari;
                        //string tanggalsampai = suratIds[0].tanggalsampai;

                        string sql = "";
                        object[] parameters = null;


                        #region Nomor Surat Pengantar

                        string tipe = "Surat Pengantar";

                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        decimal nilainomorsp = 1;

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", tipe));

                        string query = "select count(*) from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";

                        parameters = arrayListParameters.OfType<object>().ToArray();
                        int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();

                        if (jumlahrecord > 0)
                        {
                            query =
                                "select nilaikonter+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                                "FOR UPDATE NOWAIT";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", tipe));
                            parameters = arrayListParameters.OfType<object>().ToArray();

                            nilainomorsp = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();
                        }
                        else
                        {
                            // Bila tidak ada, Insert KONTERSURAT
                            query =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                "    kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                "    ( " +
                                "        SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                            //query = sWhitespace.Replace(query, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", tipe));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(query, parameters);

                        }

                        sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiKonter", nilainomorsp));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", tipe));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Get Bulan
                        int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                        string strBulan = Functions.NomorRomawi(bulan);
                        string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
                        string kodesurat = "P-";

                        nomorsp = Convert.ToString(nilainomorsp) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());

                        #endregion


                        #region Set Metadata
                        string metadata = "";
                        metadata += nomorsp + " ";
                        metadata += namapenerima + " ";
                        metadata = metadata.Trim();
                        #endregion


                        string pengantarsuratid = GetUID();

                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengantarsurat ( " +
                            "       pengantarsuratid, kantorid, nomor, tanggaldari, tujuan, profileidtujuan, metadata) VALUES " +
                            "( " +
                            "       :PengantarSuratId, :SatkerId, :Nomor, TO_DATE(:TanggalInput,'DD/MM/YYYY HH24:MI'), :Tujuan, :ProfileIdTujuan, utl_raw.cast_to_raw(:Metadata))";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengantarSuratId", pengantarsuratid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", nomorsp));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalInput", tanggalinput));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", tanggaldari));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalInput", tanggalsampai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tujuan", namapenerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdTujuan", profileidtujuan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        foreach (var surat in suratIds)
                        {
                            Models.Entities.Surat dataSurat = GetSuratBySuratIdAndProfileTujuan(surat.suratid, profileidtujuan);


                            #region Set Metadata
                            metadata = "";
                            metadata += dataSurat.NomorSurat + " ";
                            metadata += dataSurat.NomorAgenda + " ";
                            metadata += dataSurat.Perihal + " ";
                            metadata += dataSurat.PengirimSurat + " ";
                            metadata += dataSurat.SifatSurat + " ";
                            metadata += dataSurat.KeteranganSurat + " ";
                            metadata += dataSurat.Redaksi + " ";
                            metadata = metadata.Trim();
                            #endregion


                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".detilpengantar ( " +
                                "       detilpengantarid, pengantarsuratid, suratid, nomorsurat, nomoragenda, " +
                                "       tanggalsurat, pengirim, perihal, sifatsurat, keterangansurat, redaksi, metadata) VALUES " +
                                "( " +
                                "       SYS_GUID(), :PengantarSuratId, :SuratId, :NomorSurat, :NomorAgenda, " +
                                "       TO_DATE(:TanggalSurat,'DD/MM/YYYY'), :Pengirim, :Perihal, :SifatSurat, :KeteranganSurat, :Redaksi, utl_raw.cast_to_raw(:Metadata))";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengantarSuratId", pengantarsuratid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", surat.suratid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", dataSurat.NomorSurat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", dataSurat.NomorAgenda));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSurat", dataSurat.TanggalSurat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Pengirim", dataSurat.PengirimSurat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", dataSurat.Perihal));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", dataSurat.SifatSurat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", dataSurat.KeteranganSurat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", dataSurat.Redaksi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        //tc.Rollback(); // test
                        tr.Status = true;
                        tr.ReturnValue = pengantarsuratid;
                        tr.Pesan = "Surat Pengantar berhasil disimpan dengan nomor " + nomorsp;
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        //if (tr.Pesan.ToLower().Contains("unique constraint"))
                        //{
                        //    tr.Pesan = "Nomor Surat Pengantar tersebut sudah ada.";
                        //}
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

        public Entities.TransactionResult UpdateSuratPengantar(Models.Entities.FindPengantarSurat data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorEdit + " ";
                        metadata += data.TujuanEdit + " ";
                        metadata += data.NamaPenerimaEdit + " ";
                        metadata = metadata.Trim();
                        #endregion

                        if (!string.IsNullOrEmpty(data.PengantarSuratId))
                        {
                            sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengantarsurat SET " +
                                  "       nomor = :Nomor, " +
                                  "       tujuan = :Tujuan, " +
                                  "       namapenerima = :NamaPenerima, " +
                                  "       tanggaldari = TO_DATE(:TanggalDari,'DD/MM/YYYY'), " +
                                  "       tanggalterima = TO_DATE(:TanggalTerima,'DD/MM/YYYY'), " +
                                  "       metadata = utl_raw.cast_to_raw(:Metadata) " +
                                  "WHERE pengantarsuratid = :Id";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", data.NomorEdit));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tujuan", data.TujuanEdit));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPenerima", data.NamaPenerimaEdit));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", data.TanggalDariEdit));
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSampai", data.TanggalSampaiEdit));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerimaEdit));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", data.PengantarSuratId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.PengantarSuratId;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Nomor Surat Pengantar tersebut sudah ada.";
                        }
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

        public int JumlahDetilPengantar(string pengantarsuratid, string suratid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".detilpengantar WHERE pengantarsuratid = :PengantarSuratId AND suratid = :SuratId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengantarSuratId", pengantarsuratid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public List<Models.Entities.DetilPengantar> GetDetilPengantar(string pengantarsuratid)
        {
            List<Models.Entities.DetilPengantar> records = new List<Models.Entities.DetilPengantar>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT detilpengantar.detilpengantarid, detilpengantar.pengantarsuratid, detilpengantar.suratid, detilpengantar.nomorsurat, " +
                "       detilpengantar.nomoragenda, detilpengantar.pengirim, detilpengantar.perihal, detilpengantar.sifatsurat, " +
                "       detilpengantar.keterangansurat, detilpengantar.redaksi, " +
                "       to_char(detilpengantar.tanggalsurat, 'dd/mm/yyyy') TanggalSurat, " +
                "       ROW_NUMBER() over (ORDER BY detilpengantar.nomoragenda) RNumber, COUNT(1) OVER() TOTAL " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".detilpengantar " +
                "WHERE detilpengantar.pengantarsuratid = :PengantarSuratId " +
                "ORDER BY detilpengantar.nomoragenda";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengantarSuratId", pengantarsuratid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.DetilPengantar>(query, parameters).ToList<Models.Entities.DetilPengantar>();
            }

            return records;
        }

        public Models.Entities.TransactionResult InsertDetilPengantar(Models.Entities.FindPengantarSurat data)
        {
            Models.Entities.TransactionResult tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "" };

            Regex sWhitespace = new Regex(@"\s+");

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.Pengirim + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.KeteranganSurat + " ";
                        metadata += data.Redaksi + " ";
                        metadata = metadata.Trim();
                        #endregion

                        string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();

                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".detilpengantar ( " +
                            "       detilpengantarid, pengantarsuratid, suratid, nomorsurat, nomoragenda, " +
                            "       tanggalsurat, pengirim, perihal, sifatsurat, keterangansurat, redaksi, metadata) VALUES " +
                            "( " +
                            "       :Id, :PengantarSuratId, :SuratId, :NomorSurat, :NomorAgenda, " +
                            "       TO_DATE(:TanggalSurat,'DD/MM/YYYY'), :Pengirim, :Perihal, :SifatSurat, :KeteranganSurat, :Redaksi, utl_raw.cast_to_raw(:Metadata))";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengantarSuratId", data.PengantarSuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", data.SuratId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorSurat", data.NomorSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", data.NomorAgenda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSurat", data.TanggalSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Pengirim", data.Pengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Perihal", data.Perihal));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SifatSurat", data.SifatSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", data.Redaksi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = id;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Surat tersebut sudah sudah ada.";
                        }
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

        public Entities.TransactionResult HapusDetilPengantar(string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".detilpengantar WHERE detilpengantarid = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        #endregion


        #region Rekap

        public List<Models.Entities.RekapSurat> GetRekapSurat(string unitkerjaid, string satkerid, string tipe = null)
        {
            List<Models.Entities.RekapSurat> records = new List<Models.Entities.RekapSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string namaskema = System.Web.Mvc.OtorisasiUser.NamaSkema + ".";

            string query =
                 "SELECT " +
                 "     ROW_NUMBER() over (ORDER BY kodepangkat DESC) RNumber, COUNT(1) OVER() TOTAL, " +
                 "     refpegawai.nip, refpegawai.namapegawai, refpegawai.jabatan, refpegawai.foto, " +
                 "     NVL(total.JUMLAHSURAT, 0) JUMLAHSURAT, " +
                 "     NVL(inbox.JUMLAHINBOX, 0) JUMLAHINBOX, " +
                 "     NVL(terkirim.JUMLAHTERKIRIM, 0) JUMLAHTERKIRIM, " +
                 "     NVL(selesai.JUMLAHSELESAI, 0) JUMLAHSELESAI " +
                 " FROM " +
                 "    (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHSURAT " +
                 "         FROM " + namaskema + "suratinbox " +
                 "              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                 "              JOIN JABATANPEGAWAI JP ON " +
                 "                JP.PROFILEID = suratinbox.profilepenerima AND " +
                 "                JP.PEGAWAIID = suratinbox.nip AND " +
                 "                (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND " +
                 "                NVL(JP.STATUSHAPUS, '0') = '0' " +
                 "              JOIN JABATAN JB ON " +
                 "              JB.PROFILEID = JP.PROFILEID AND JB.UNITKERJAID = '" + unitkerjaid + "' " +
                 "         WHERE suratinbox.statusterkunci = 0 and statusforwardtu = 0 " +
                 "               AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                 //"               AND suratinbox.kantorid = :KantorId1 " +
                 "         GROUP BY suratinbox.nip) total, " +
                 "    (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHINBOX " +
                 "         FROM " + namaskema + "suratinbox " +
                 //"              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                 "              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                 "              JOIN JABATANPEGAWAI JP ON " +
                 "                JP.PROFILEID = suratinbox.profilepenerima AND " +
                 "                JP.PEGAWAIID = suratinbox.nip AND " +
                 "                (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND " +
                 "                NVL(JP.STATUSHAPUS, '0') = '0' " +
                 "              JOIN JABATAN JB ON " +
                 "              JB.PROFILEID = JP.PROFILEID AND JB.UNITKERJAID = '" + unitkerjaid + "' " +
                 "         WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusterkirim = 0 AND suratinbox.statusforwardtu = 0 " +
                 //"               AND suratinbox.kantorid = :KantorId2 " +
                 "               AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                 //       "               AND LOWER(SURATINBOX.TINDAKLANJUT) <> 'selesai' " + // Arya :: 2020-08-20
                 "               AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId1" +
                 //       "               AND ((lower(NVL(suratinbox.redaksi,' ')) IN ('penanggung jawab','asli',' ') AND arsipsurat.profilearsip = suratinbox.profilepenerima)) OR lower(NVL(suratinbox.redaksi,' ')) NOT IN ('penanggung jawab','asli',' ') " + // Arya :: 2020-08-20
                 ") " +
                 "         GROUP BY suratinbox.nip) inbox, " +
                 "    (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHTERKIRIM " +
                 "         FROM " + namaskema + "suratinbox " +
                 //"              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                 "              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                 "              JOIN JABATANPEGAWAI JP ON " +
                 "                JP.PROFILEID = suratinbox.profilepenerima AND " +
                 "                JP.PEGAWAIID = suratinbox.nip AND " +
                 "                (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND " +
                 "                NVL(JP.STATUSHAPUS, '0') = '0' " +
                 "              JOIN JABATAN JB ON " +
                 "              JB.PROFILEID = JP.PROFILEID AND JB.UNITKERJAID = '" + unitkerjaid + "' " +
                 "         WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusterkirim = 1 AND suratinbox.statusforwardtu = 0 " +
                 //"               AND suratinbox.kantorid = :KantorId3 " +
                 "               AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                 "               AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId2) " +
                 "         GROUP BY suratinbox.nip) terkirim, " +
                 "    (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHSELESAI " +
                 "         FROM " + namaskema + "suratinbox " +
                 //"              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 0 " +
                 "              JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                 "              JOIN JABATANPEGAWAI JP ON " +
                 "                JP.PROFILEID = suratinbox.profilepenerima AND " +
                 "                JP.PEGAWAIID = suratinbox.nip AND " +
                 "                (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND " +
                 "                NVL(JP.STATUSHAPUS, '0') = '0' " +
                 "              JOIN JABATAN JB ON " +
                 "              JB.PROFILEID = JP.PROFILEID AND JB.UNITKERJAID = '" + unitkerjaid + "' " +
                 "         WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusforwardtu = 0 " +
                 //"               AND suratinbox.kantorid = :KantorId4 " +
                 "               AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                 "               AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId3) " +
                 "         GROUP BY suratinbox.nip) selesai, " +
                 "    (SELECT DISTINCT p.pegawaiid as nip, p.nama as namapegawai, NVL(psimpeg.namajabatan,p.JABATAN) AS jabatan, psimpeg.gol kodepangkat, NVL(psimpeg.foto, 'foto.jpg') foto " +
                 "         FROM pegawai p " +
                 "             LEFT JOIN simpeg_2702.v_pegawai_eoffice psimpeg ON psimpeg.nipbaru = p.pegawaiid " +
                 //"              LEFT JOIN simpeg_2702.riwayatjabatan rjsimpeg ON rjsimpeg.pegawaiid = psimpeg.pegawaiid AND NVL(rjsimpeg.isnew, 0) = 1 " +
                 //"              LEFT JOIN simpeg_2702.VWPANGKATTERAKHIR pangkatsimpeg ON pangkatsimpeg.pegawaiid = psimpeg.pegawaiid AND pangkatsimpeg.ranking = 1 " +
                 //"              LEFT JOIN simpeg_2702.pangkat refpangkatsimpeg ON refpangkatsimpeg.pangkatid = pangkatsimpeg.pangkatid " +
                 "         WHERE p.VALIDSAMPAI IS NULL AND p.pegawaiid in ( " +
                 "             SELECT DISTINCT jabatanpegawai.pegawaiid " +
                 "             FROM jabatanpegawai " +
                 "                  JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
                 "                       and jabatan.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') " +
                 "             WHERE jabatan.unitkerjaid = :UnitKerjaId " +
                 "                AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(jabatanpegawai.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND " +
                 "                NVL(jabatanpegawai.STATUSHAPUS, '0') = '0') " +
                 "         GROUP BY p.pegawaiid, p.nama, NVL(psimpeg.namajabatan,p.JABATAN), psimpeg.gol, psimpeg.foto " +
                 "         ORDER BY psimpeg.gol DESC) refpegawai " +
                 " WHERE " +
                 "     total.nip(+) = refpegawai.nip " +
                 "     AND inbox.nip(+) = refpegawai.nip " +
                 "     AND terkirim.nip(+) = refpegawai.nip " +
                 "     AND selesai.nip(+) = refpegawai.nip " +
                 " ORDER BY " +
                 "     kodepangkat DESC";
            string qView = "AND JB.NAMA <> 'PPNPN'";
            if (!string.IsNullOrEmpty(tipe))
            {
                if (tipe.Equals("PPNPN"))
                {
                    qView = "AND JB.NAMA IN ('PPNPN','Tenaga Ahli')";
                }
                else if (tipe.Equals("ASN"))
                {
                    qView = "AND JB.NAMA NOT IN ('PPNPN','Tenaga Ahli')";
                }
                else if (tipe.Equals("ALL"))
                {
                    qView = "";
                }
            }
            query = string.Format(@"
                SELECT
                  ROW_NUMBER() over (ORDER BY NVL(PS.GOL,99) DESC, NVL(MAX(DECODE(TIPE, 'JUMLAHSELESAI', JUMLAH)),0) DESC) AS RNumber,
                  JP.PEGAWAIID AS NIP, COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA) AS NAMAPEGAWAI,
                  NVL(PS.NAMAJABATAN, JB.NAMA) AS JABATAN,
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
                          SI.TINDAKLANJUT <> 'Selesai' AND
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
                          SI.TINDAKLANJUT <> 'Selesai' AND
		                  NOT EXISTS
		                    (SELECT 1
		                     FROM SURAT.ARSIPSURAT
		                     WHERE
		                       SURATID = SI.SURATID AND
		                       KANTORID = :SatkerId1) AND
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
                          SI.TINDAKLANJUT <> 'Selesai' AND
		                  NOT EXISTS
		                    (SELECT 1
		                     FROM SURAT.ARSIPSURAT
		                     WHERE
		                       SURATID = SI.SURATID AND
		                       KANTORID = :SatkerId2) AND
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
		                       KANTORID = :SatkerId3) AND
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
                  COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA) IS NOT NULL AND
                  JB.UNITKERJAID = :UnitKerjaId {1}
                GROUP BY
                  JP.PEGAWAIID, COALESCE(PS.NAMA_LENGKAP,PG.NAMA,PP.NAMA), NVL(PS.GOL,99),
                  NVL(PS.NAMAJABATAN, JB.NAMA),
                  COALESCE(PS.foto, PP.URLPROFILE, 'foto.jpg')", System.Web.Mvc.OtorisasiUser.NamaSkema, qView);

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId1", satkerid));
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId2", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId1", satkerid));
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId3", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId2", satkerid));
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId4", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId3", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.RekapSurat>(query, parameters).ToList<Models.Entities.RekapSurat>();
                //records = ctx.Database.SqlQuery<Models.Entities.RekapSurat>(query).ToList<Models.Entities.RekapSurat>();
            }

            return records;
        }

        public Models.Entities.RekapSurat GetRekapSuratPegawai(string satkerid, string pegawaiid)
        {
            Models.Entities.RekapSurat data = new Models.Entities.RekapSurat();

            ArrayList arrayListParameters = new ArrayList();

            string namaskema = System.Web.Mvc.OtorisasiUser.NamaSkema + ".";

            string query =
                "SELECT " +
                "      ROW_NUMBER() over (ORDER BY kodepangkat DESC) RNumber, COUNT(1) OVER() TOTAL, " +
                "      refpegawai.nip, refpegawai.namapegawai, refpegawai.jabatan, refpegawai.foto, " +
                "      NVL(total.JUMLAHSURAT, 0) JUMLAHSURAT, " +
                "      NVL(inbox.JUMLAHINBOX, 0) JUMLAHINBOX, " +
                "      NVL(terkirim.JUMLAHTERKIRIM, 0) JUMLAHTERKIRIM, " +
                "      NVL(selesai.JUMLAHSELESAI, 0) JUMLAHSELESAI " +
                "  FROM " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHSURAT " +
                "          FROM " + namaskema + "suratinbox " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 " +
                "          GROUP BY suratinbox.nip) total, " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHINBOX " +
                "          FROM " + namaskema + "suratinbox " +
                //"               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusterkirim = 0 AND suratinbox.statusforwardtu = 0 " +
                "                AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "                AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId1) " +
                "          GROUP BY suratinbox.nip) inbox, " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHTERKIRIM " +
                "          FROM " + namaskema + "suratinbox " +
                //"               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusterkirim = 1 AND suratinbox.statusforwardtu = 0 " +
                "                AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "                AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId2) " +
                "          GROUP BY suratinbox.nip) terkirim, " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHSELESAI " +
                "          FROM " + namaskema + "suratinbox " +
                //"               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 0 " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusforwardtu = 0 " +
                "                AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "                AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId3) " +
                "          GROUP BY suratinbox.nip) selesai, " +
                "     (SELECT DISTINCT p.pegawaiid as nip, p.nama as namapegawai, psimpeg.namajabatan AS jabatan, psimpeg.gol kodepangkat, NVL(psimpeg.foto, 'foto.jpg') foto " +
                "          FROM pegawai p " +
                "               JOIN simpeg_2702.v_pegawai_eoffice psimpeg ON psimpeg.nipbaru = p.pegawaiid " +
                //"               LEFT JOIN simpeg_2702.riwayatjabatan rjsimpeg ON rjsimpeg.pegawaiid = psimpeg.pegawaiid AND NVL(rjsimpeg.isnew, 0) = 1 " +
                //"               LEFT JOIN simpeg_2702.VWPANGKATTERAKHIR pangkatsimpeg ON pangkatsimpeg.pegawaiid = psimpeg.pegawaiid AND pangkatsimpeg.ranking = 1 " +
                //"               LEFT JOIN simpeg_2702.pangkat refpangkatsimpeg ON refpangkatsimpeg.pangkatid = pangkatsimpeg.pangkatid " +
                "          WHERE (p.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(p.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) AND p.pegawaiid = :PegawaiId " +
                "          GROUP BY p.pegawaiid, p.nama, psimpeg.namajabatan, psimpeg.gol, psimpeg.foto " +
                "          ORDER BY psimpeg.gol DESC) refpegawai " +
                "  WHERE " +
                "      total.nip(+) = refpegawai.nip " +
                "      AND inbox.nip(+) = refpegawai.nip " +
                "      AND terkirim.nip(+) = refpegawai.nip " +
                "      AND selesai.nip(+) = refpegawai.nip " +
                "  ORDER BY " +
                "      kodepangkat DESC";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId1", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId2", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId3", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiId", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Models.Entities.RekapSurat>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public Models.Entities.RekapSurat GetRekapSuratPPNPN(string satkerid, string pegawaiid)
        {
            Models.Entities.RekapSurat data = new Models.Entities.RekapSurat();

            ArrayList arrayListParameters = new ArrayList();

            string namaskema = System.Web.Mvc.OtorisasiUser.NamaSkema + ".";

            string query =
                "SELECT " +
                "      COUNT(1) OVER() TOTAL, " +
                "      refpegawai.nip, refpegawai.namapegawai, refpegawai.jabatan, refpegawai.foto, " +
                "      NVL(total.JUMLAHSURAT, 0) JUMLAHSURAT, " +
                "      NVL(inbox.JUMLAHINBOX, 0) JUMLAHINBOX, " +
                "      NVL(terkirim.JUMLAHTERKIRIM, 0) JUMLAHTERKIRIM, " +
                "      NVL(selesai.JUMLAHSELESAI, 0) JUMLAHSELESAI " +
                "  FROM " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHSURAT " +
                "          FROM " + namaskema + "suratinbox " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 " +
                "          GROUP BY suratinbox.nip) total, " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHINBOX " +
                "          FROM " + namaskema + "suratinbox " +
                //"               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusterkirim = 0 AND suratinbox.statusforwardtu = 0 " +
                "                AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "                AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId1) " +
                "          GROUP BY suratinbox.nip) inbox, " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHTERKIRIM " +
                "          FROM " + namaskema + "suratinbox " +
                //"               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusterkirim = 1 AND suratinbox.statusforwardtu = 0 " +
                "                AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "                AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId2) " +
                "          GROUP BY suratinbox.nip) terkirim, " +
                "     (SELECT DISTINCT suratinbox.nip, count(*) AS JUMLAHSELESAI " +
                "          FROM " + namaskema + "suratinbox " +
                //"               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 0 " +
                "               JOIN " + namaskema + "surat ON surat.suratid = suratinbox.suratid " +
                "          WHERE suratinbox.statusterkunci = 0 AND suratinbox.statusforwardtu = 0 " +
                "                AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "                AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId3) " +
                "          GROUP BY suratinbox.nip) selesai, " +
                "     (SELECT DISTINCT u.userid AS id_pengguna, u.username AS nama_pengguna, u.email ,p.NIK AS nip " +
                "            , p.NAMA AS namapegawai , j.nama AS jabatan ,k.kantorid, k.nama namakantor " +
                "            ,'https://mitra.atrbpn.go.id/ppnpn/' || p.URLPROFILE AS foto " +
                "            FROM PPNPN, PPNPN.USERS u , PPNPN.PPNPN p, jabatanpegawai pp, jabatan j , kantor k " +
                "            WHERE u.userid = p.userid " +
                "            AND p.NIK = pp.pegawaiid " +
                "            AND j.PROFILEID= pp.PROFILEID " +
                "            AND k.KANTORID = pp.KANTORID " +
                "            AND p.NIK  = :PegawaiId ) refpegawai " +
                "  WHERE " +
                "      total.nip(+) = refpegawai.nip " +
                "      AND inbox.nip(+) = refpegawai.nip " +
                "      AND terkirim.nip(+) = refpegawai.nip " +
                "      AND selesai.nip(+) = refpegawai.nip";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId1", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId2", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId3", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiId", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Models.Entities.RekapSurat>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        #endregion


        public Models.Entities.Surat GetSuratBySuratIdWithProfileId(string suratid, string profileid)
        {
            Models.Entities.Surat records = new Models.Entities.Surat();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, surat.pengirim PengirimSurat, surat.penerima PenerimaSurat, " +
                //"    NVL(agendasurat.nomoragenda, surat.nomoragenda) NomorAgendaSurat, " +
                //"    agendasurat.nomoragenda NomorAgendaSurat, " +
                "    surat.nomoragenda NomorAgendaSurat, " +
                "    to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "    to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "    to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "    to_char(surat.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "    to_char(surat.tanggalproses, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalTerimaCetak, " +
                "    to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "    to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "    to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "    to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "    to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                //"    to_char(suratinbox.tanggalkirim, 'dd/mm/yyyy HH24:MI:SS') tanggalkirim, " +
                "    to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "    to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                //"    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy HH24:MI:SS') tanggalterima, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "    to_char(suratinbox.tanggalkirim, 'Day, fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalInbox, " +
                "    to_char(surat.tanggalsurat, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalSurat, " +
                "    to_char(suratinbox.tanggalterima, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalTerima, " +
                "    surat.kategori, surat.arah, 'Surat ' || surat.arah ArahSurat, surat.tipesurat, surat.sifatsurat, surat.jumlahlampiran, " +
                "    surat.isisingkat IsiSingkatSurat, surat.statussurat, surat.referensi ReferensiSurat, surat.keterangansurat, " +
                "    to_char(suratinbox.tanggalterima, 'dd/mm/yyyy') TanggalTerimaFisik, " +
                "    decode(suratinbox.tanggalterima, null, 'Tidak', 'Ya') AS TerimaFisik, " +
                "    profiledari.nama AS NamaProfilePengirim, " +
                "    surat.keterangansurat || ', ' || suratinbox.redaksi AS KeteranganSuratRedaksi, " +
                "    suratinbox.profilepenerima, suratinbox.suratinboxid, suratinbox.redaksi " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox ON suratinbox.suratid = surat.suratid " +
                //"    LEFT JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "    LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "WHERE " +
                "    surat.suratid = :SuratId AND suratinbox.profilepenerima = :ProfileId";

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileId", profileid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).FirstOrDefault();
            }

            return records;
        }

        public List<Models.Entities.SuratInbox> GetLaporanSuratInbox(string satkerid, string nip, string statussurat, string arah, string sortby, string sorttype, string tanggaldari, string tanggalsampai, string bulansurat, int from, int to)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();

            ArrayList arrayListParameters = new ArrayList();

            // Default atau by SifatSurat
            string orderby = "suratinbox.statusurgent DESC, sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "SifatSurat")
                {
                    orderby = "suratinbox.statusurgent DESC, sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.statusurgent, sifatsurat.prioritas DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "JenisDisposisi")
                {
                    orderby = "sifatsurat.perintahdisposisi, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.perintahdisposisi DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalTerima")
                {
                    orderby = "suratinbox.tanggalterima, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalterima DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalKirim")
                {
                    orderby = "suratinbox.tanggalkirim, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TargetSelesai")
                {
                    orderby = "surat.targetselesai, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "surat.targetselesai DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
            }

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.profilepengirim, suratinbox.profilepenerima, " +
                "        to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, " +
                "        suratinbox.statusterkirim, decode(suratinbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.statusurgent, suratinbox.perintahdisposisi, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        to_char(suratkembali.tanggalkembali, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkembali, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM, profiletujuan.nama AS NAMAPROFILEPENERIMA " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratkembali ON suratkembali.suratinboxid = suratinbox.suratinboxid " +
                "    WHERE " +
                "        suratinbox.statusterkirim = 0 AND suratinbox.statusterkunci = 0 AND suratinbox.statusforwardtu = 0 " +
                "        AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') ";

            // STATUS SURAT DIUBAH DARI FIELD SURAT.STATUSSURAT KE TABLE ARSIPSURAT
            query += " AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId) ";
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            //if (!String.IsNullOrEmpty(statussurat))
            //{
            //    query += " AND surat.statussurat IN ( " + statussurat + ") ";
            //}
            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (suratinbox.nip IS NULL OR suratinbox.nip = :Nip) ";
            }
            if (!String.IsNullOrEmpty(arah))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Arah", arah));
                query += " AND surat.arah = :Arah ";
            }
            if (!String.IsNullOrEmpty(tanggaldari) && !String.IsNullOrEmpty(tanggalsampai))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggaldari + " 00:00:00"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalsampai + " 23:59:59"));
                query += " AND (suratinbox.tanggalkirim BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }
            if (!String.IsNullOrEmpty(bulansurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Bulan", bulansurat));
                query += " AND TO_CHAR(suratinbox.tanggalkirim, 'YYYY-MM') = :Bulan ";
            }
            query += ")";
            if (from + to > 0)
            {
                query +=
                    " WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratInbox>(query, parameters).ToList<Models.Entities.SuratInbox>();
            }

            return records;
        }

        public List<Models.Entities.SuratOutbox> GetLaporanSuratOutbox(string nip, string sortby, string sorttype, string tanggaldari, string tanggalsampai, string bulansurat, int from, int to)
        {
            List<Models.Entities.SuratOutbox> records = new List<Models.Entities.SuratOutbox>();

            ArrayList arrayListParameters = new ArrayList();

            // Default atau by TanggalKirim Desc
            string orderby = "suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "TanggalKirim")
                {
                    orderby = "suratoutbox.tanggalkirim, suratoutbox.suratoutboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                        }
                    }
                }
                else if (sortby == "SifatSurat")
                {
                    orderby = "sifatsurat.prioritas, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.prioritas DESC, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                        }
                    }
                }
                else if (sortby == "JenisDisposisi")
                {
                    orderby = "sifatsurat.perintahdisposisi, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.perintahdisposisi DESC, suratoutbox.tanggalkirim DESC, suratoutbox.suratoutboxid";
                        }
                    }
                }
            }

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratoutbox.suratoutboxid, suratoutbox.suratid, suratoutbox.nip, suratoutbox.profilepengirim, " +
                "        to_char(suratoutbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratoutbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        decode(suratoutbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratoutbox.suratid " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratoutbox.profilepengirim " +
                "    WHERE " +
                "        suratoutbox.suratoutboxid IS NOT NULL ";


            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND suratoutbox.nip = :Nip ";
            }
            if (!String.IsNullOrEmpty(tanggaldari) && !String.IsNullOrEmpty(tanggalsampai))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggaldari + " 00:00:00"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalsampai + " 23:59:59"));
                query += " AND (suratoutbox.tanggalkirim BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }
            if (!String.IsNullOrEmpty(bulansurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Bulan", bulansurat));
                query += " AND TO_CHAR(suratoutbox.tanggalkirim, 'YYYY-MM') = :Bulan ";
            }
            query += ")";
            if (from + to > 0)
            {
                query +=
                    " WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.SuratOutbox>(query, parameters).ToList<Models.Entities.SuratOutbox>();
            }

            return records;
        }


        public List<Models.Entities.Surat> GetLaporanSurat(string satkerid, string myProfiles, string statussurat, string kategorisurat, string nippenerima, string sortby, string sorttype, string tanggaldari, string tanggalsampai, string bulansurat, int from, int to, string bypegawaiid = null, bool switchby = false)
        {
            List<Models.Entities.Surat> records = new List<Models.Entities.Surat>();

            ArrayList arrayListParameters = new ArrayList();

            string orderby = "surat.tanggalsurat DESC, surat.tanggalproses DESC, surat.suratid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "SifatSurat")
                {
                    orderby = "surat.sifatsurat, surat.tanggalproses DESC, surat.suratid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "surat.sifatsurat DESC, surat.tanggalproses DESC, surat.suratid";
                        }
                    }
                }
                else if (sortby == "TanggalTerima")
                {
                    orderby = "surat.tanggalproses, surat.tanggalproses DESC, surat.suratid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "surat.tanggalproses DESC, surat.tanggalproses DESC, surat.suratid";
                        }
                    }
                }
                else if (sortby == "TanggalSurat")
                {
                    orderby = "surat.tanggalsurat, surat.tanggalproses DESC, surat.suratid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "surat.tanggalsurat DESC, surat.tanggalproses DESC, surat.suratid";
                        }
                    }
                }
            }
            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalproses, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.suratid, surat.nomorsurat, surat.nomoragenda, surat.perihal, 'Surat ' || surat.arah ArahSurat, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, " +
                "        decode(arsipsurat.arsipsuratid, null, 0, 1) AS StatusArsip, " +
                "        surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        DECODE(suratoutput.nomorsurat, NULL, '', suratoutput.tipesurat || ': ' || suratoutput.nomorsurat) " +
                "            || DECODE(suratoutput.nomorsurat, NULL, '', ', Perihal: ' || suratoutput.perihal) AS Output " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat suratoutput ON suratoutput.referensi = surat.suratid " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat ON arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId " +
                "    WHERE " +
                "        surat.suratid IS NOT NULL ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            if (!String.IsNullOrEmpty(statussurat))
            {
                if (statussurat == "1")
                {
                    query += " AND arsipsurat.arsipsuratid IS NULL ";
                }
                else if (statussurat == "0")
                {
                    query += " AND arsipsurat.arsipsuratid IS NOT NULL ";
                }
            }

            if (!String.IsNullOrEmpty(myProfiles))
            {
                if (switchby)
                {
                    query +=
                    " AND (EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.nip = '"+ bypegawaiid +"') OR " +
                    "      EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutbox.suratid = surat.suratid AND suratoutbox.nip  = '" + bypegawaiid + "')) ";
                }
                else
                {
                    query +=
                    " AND (EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.profilepenerima IN (" + myProfiles + ") " + (string.IsNullOrEmpty(bypegawaiid) ? "" : $"AND suratinbox.nip = '{bypegawaiid}'") + " ) OR " +
                    "      EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratoutbox WHERE suratoutbox.suratid = surat.suratid AND suratoutbox.profilepengirim IN (" + myProfiles + ") " + (string.IsNullOrEmpty(bypegawaiid) ? "" : $"AND suratoutbox.nip = '{bypegawaiid}'") + " )) ";
                }
            }

            if (!String.IsNullOrEmpty(kategorisurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KategoriSurat", kategorisurat));
                query += " AND surat.kategori = :KategoriSurat ";
            }

            if (!String.IsNullOrEmpty(nippenerima))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPenerima", nippenerima));
                query +=
                    " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND suratinbox.nip = :NipPenerima) ";
            }

            if (!String.IsNullOrEmpty(tanggaldari) && !String.IsNullOrEmpty(tanggalsampai))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal1", tanggaldari + " 00:00:00"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal2", tanggalsampai + " 23:59:59"));
                query += " AND (surat.tanggalsurat BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }

            if (!String.IsNullOrEmpty(bulansurat))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Bulan", bulansurat));
                query += " AND TO_CHAR(surat.tanggalsurat, 'YYYY-MM') = :Bulan ";
            }

            query += ")";
            if (from + to > 0)
            {
                query +=
                    " WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));
            }
            query += " ORDER BY RNUMBER";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, parameters).ToList<Models.Entities.Surat>();
            }

            return records;
        }

        public string GetPenanggungJawab(string suratid, string unitkerjaid, string pegawaiid)
        {
            string result = string.Empty;

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT SI.NAMAPEGAWAI
                FROM {0}.SURAT SR
                  JOIN {0}.SURATINBOX SI ON
                    SI.SURATID = SR.suratid AND
                    LOWER(SI.redaksi) LIKE '%penanggung jawab%'
                  JOIN jabatan JB ON
                    JB.profileid = SI.profilepenerima
                WHERE
                  SI.SURATID = :SuratId ", System.Web.Mvc.OtorisasiUser.NamaSkema);

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));


            using (var ctx = new BpnDbContext())
            {
                string eselon = ctx.Database.SqlQuery<string>(string.Format("SELECT SUBSTR(SK.ESELON,0,1) AS ESELON FROM simpeg_2702.satker SK INNER JOIN simpeg_2702.v_pegawai_eoffice PE ON PE.SATKERID = SK.SATKERID WHERE PE.NIPBARU = '{0}'", pegawaiid)).FirstOrDefault();
                bool cekunitkerja = true;
                if (!string.IsNullOrEmpty(eselon))
                {
                    if (Convert.ToInt32(eselon) <= 2)
                    {
                        cekunitkerja = false;
                    }
                }
                if (cekunitkerja)
                {
                    query += @" AND
                  EXISTS
                    (SELECT 1
                     FROM unitkerja UK
                     WHERE
                       UK.unitkerjaid = JB.unitkerjaid AND
                       UK.unitkerjaid = :UnitKerjaId)";
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                }
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                List<string> _rst = ctx.Database.SqlQuery<string>(query, parameters).ToList();
                foreach (string rst in _rst)
                {
                    result += string.Concat(rst, " || ");
                }
                if (!string.IsNullOrEmpty(result))
                {
                    result = result.Remove(result.Length - 4, 4);
                }
            }

            return result;
        }

        public List<Entities.TipeSurat> GetTipeSurat(string satkerid, string nip, string arah, string profileid)
        {
            var list = new List<Entities.TipeSurat>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string query = string.Format(@"
                SELECT DISTINCT
	                TS.URUTAN,
                    TS.NAMA AS NAMATIPESURAT,
	                (TS.NAMA || ' (' || COUNT(TS.NAMA) || ')') AS VALUETIPESURAT
                FROM {0}.TIPESURAT TS
                INNER JOIN {0}.SURAT S ON
	                S.TIPESURAT = TS.NAMA
                INNER JOIN {0}.SURATINBOX SI ON
                    S.SURATID = SI.SURATID
                INNER JOIN JABATAN JB ON
                    JB.PROFILEID = SI.PROFILEPENERIMA AND
	                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
                  SI.STATUSTERKIRIM = 0 AND
                  SI.STATUSTERKUNCI = 0 AND
                  SI.STATUSFORWARDTU = 0 AND
                  NVL(SI.STATUSHAPUS,'0') = '0' AND
                  NOT EXISTS
                    (SELECT 1
                     FROM {0}.ARSIPSURAT
                     WHERE
                       ARSIPSURAT.SURATID = S.SURATID AND
                       ARSIPSURAT.KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)) AND
                  SI.NIP = '{2}' AND
                  S.ARAH = '{3}' AND
                  SI.PROFILEPENERIMA = '{4}' AND
	              TS.AKTIF = 1
                GROUP BY
	                TS.URUTAN,
	                TS.NAMA
                ORDER BY TS.URUTAN", skema,satkerid,nip,arah,profileid);

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Entities.TipeSurat>(query).ToList();
            }

            return list;
        }

        public List<Models.Entities.SuratInbox> GetProsesSuratV2(string satkerid, string profileidtu, string metadata, string sortby, string sorttype, int from, int to, string sumber = null)
        {
            List<Models.Entities.SuratInbox> records = new List<Models.Entities.SuratInbox>();

            ArrayList arrayListParameters = new ArrayList();

            // Default atau by SifatSurat
            string orderby = "sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "SifatSurat")
                {
                    orderby = "sifatsurat.prioritas, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.prioritas DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "JenisDisposisi")
                {
                    orderby = "sifatsurat.perintahdisposisi, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "sifatsurat.perintahdisposisi DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalTerima")
                {
                    orderby = "NVL(suratinbox.tanggalterima,suratinbox.tanggalkirim), suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "NVL(suratinbox.tanggalterima,suratinbox.tanggalkirim) DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalKirim")
                {
                    orderby = "suratinbox.tanggalkirim, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
            }

            string query = string.Format(
                "SELECT * FROM ( " +
                "    SELECT /*+ FULL(profiletujuan) FULL(profiledari)*/" +
                "        ROW_NUMBER() over (ORDER BY " + orderby + ") RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        surat.nomoragenda NomorAgendaSurat, " +
                "        suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.profilepengirim, suratinbox.profilepenerima, " +
                "        to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(nvl(suratinbox.tanggalterima,suratinbox.tanggalkirim), 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, " +
                "        suratinbox.statusterkirim, decode(suratinbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.perintahdisposisi, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM, profiletujuan.nama AS NAMAPROFILEPENERIMA, " +
                "        sumber_surat.sumber_keterangan as sumber_keterangan " +


                "    FROM " +
                "        {0}.suratinbox " +
                "        JOIN {0}.surat ON surat.suratid = suratinbox.suratid " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +

                (!string.IsNullOrEmpty(sumber) ? " INNER JOIN " : " LEFT JOIN ") + "{0}.SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratinbox.suratid " +
                (!string.IsNullOrEmpty(sumber) ? string.Format(" AND sumber_surat.sumber_keterangan = '{0}' ", sumber) : "") +

                "        LEFT JOIN {0}.ARSIPSURAT ARS ON ARS.SURATID = SURAT.SURATID AND ARS.KANTORID = :SatkerId " +
                "    WHERE " +
                "        suratinbox.statusterkirim = 0 " +
                "        AND suratinbox.statusterkunci = 0 " +
                "        AND suratinbox.statusforwardtu = 1 " +
                "        AND suratinbox.tindaklanjut <> 'Selesai' " +
                "        AND NVL(suratinbox.statushapus,'0') = '0' " +
                "        AND ARS.ARSIPSURATID IS NULL ", System.Web.Mvc.OtorisasiUser.NamaSkema);

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            if (!string.IsNullOrEmpty(profileidtu))
            {
                if (profileidtu.Substring(0, 1).Equals("'"))
                {
                    query += string.Format(" AND suratinbox.profilepenerima in ({0}) ", profileidtu);
                }
                else
                {
                    query += " AND suratinbox.profilepenerima = :profileidtu ";
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("profileidtu", profileidtu));
                }
            }
            if (!string.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(utl_raw.cast_to_varchar2(surat.metadata))) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.SuratInbox>(query, parameters).ToList();
            }

            return records;
        }

        public int JumlahProsesSuratV2(string profiletu, string satkerid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox " +
                "       JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                //"       JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid AND surat.statussurat = 1 " +
                //"       JOIN agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = :SatkerId " +
                "WHERE  suratinbox.statusterkirim = 0 " +
                "       AND suratinbox.statusterkunci = 0 " +
                "       AND suratinbox.statusforwardtu = 1 " +
                "       AND suratinbox.tindaklanjut <> 'Selesai' " +
                "       AND NVL(suratinbox.statushapus,'0') = '0' " +
                "       AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = :SatkerId) " +
                "       AND suratinbox.profilepenerima IN ("+profiletu+")";
            //"       AND (suratinbox.nip IS NULL OR suratinbox.nip = :Nip)";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }
            return result;
        }

        public string GetServiceKonten(DateTime dokdate)
        {
            string url = (DateTime.Compare(dokdate, Convert.ToDateTime("24/04/2021")) > 0) ? "ServiceEofficeUrl" : "ServiceBaseUrl";
            return url;
        }

        public DateTime getTglSunting(string id, string tipe)
        {
            DateTime result = DateTime.Now;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    //cek exists
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT COUNT(*) FROM KONTENAKTIF WHERE KONTENAKTIFID = :id AND TIPE = :tipe";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", tipe));
                    var parameters = lstparams.ToArray();
                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                    {
                        lstparams = new List<object>();
                        sql = "SELECT TANGGALSUNTING FROM KONTENAKTIF WHERE KONTENAKTIFID = :id AND TIPE = :tipe";
                        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", tipe));
                        parameters = lstparams.ToArray();
                        result = ctx.Database.SqlQuery<DateTime>(sql, parameters).FirstOrDefault();
                    }
                    else
                    {
                        if (tipe.Equals("Surat"))
                        {
                            lstparams = new List<object>();
                            sql = string.Concat("SELECT COUNT(*) FROM ", System.Web.Mvc.OtorisasiUser.NamaSkema, ".LAMPIRANSURAT WHERE LAMPIRANSURATID = :id");
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                            parameters = lstparams.ToArray();
                            if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                            {
                                lstparams = new List<object>();
                                sql = string.Concat("SELECT TANGGAL FROM ", System.Web.Mvc.OtorisasiUser.NamaSkema, ".LAMPIRANSURAT WHERE LAMPIRANSURATID = :id");
                                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                                parameters = lstparams.ToArray();
                                result = ctx.Database.SqlQuery<DateTime>(sql, parameters).FirstOrDefault();
                            }
                        }
                        else if (tipe.Equals("Pengaduan"))
                        {
                            lstparams = new List<object>();
                            sql = string.Concat("SELECT COUNT(*) FROM ", System.Web.Mvc.OtorisasiUser.NamaSkema, ".LAMPIRANPENGADUAN WHERE LAMPIRANPENGADUANID = :id");
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                            parameters = lstparams.ToArray();
                            if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                            {
                                lstparams = new List<object>();
                                sql = string.Concat("SELECT TANGGAL FROM ", System.Web.Mvc.OtorisasiUser.NamaSkema, ".LAMPIRANPENGADUAN WHERE LAMPIRANPENGADUANID = :id");
                                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                                parameters = lstparams.ToArray();
                                result = ctx.Database.SqlQuery<DateTime>(sql, parameters).FirstOrDefault();
                            }
                        }
                    }
                }
            }
            return result;
        }

        //Model surat pengantar baru
        #region Pengantar Baru
        //Model surat pengantar baru
        public List<Models.Entities.PengantarSurat> GetListPengantar(
            int from, int to,
            string pengantarid = null,
            string unitkerjaid = null,
            string nomorpengantar = null,
            string tanggalpengantar = null,
            string tujuanpengantar = null,
            string srchkey = null
            )
        {
            List<Models.Entities.PengantarSurat> records = new List<Models.Entities.PengantarSurat>();
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters;
            string sql = $@"SELECT  
                                COUNT(1) OVER() TOTAL, 
                                PT.PENGANTARSURATID, 
                                PT.NOMOR AS NOMORSURAT,
                                PT.TUJUAN AS USERID,
                                PT.NAMAPENERIMA AS PENANDATANGAN,
                                TO_CHAR(PT.TANGGALSURAT,'dd/mm/yyyy') AS TANGGALSURAT,
                                PT.PROFILEIDTUJUAN, 
                                utl_raw.cast_to_varchar2(PT.METADATA) AS LSTSURAT
                            FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT PT
                            WHERE PT.NOMOR IS NOT NULL AND PT.TUJUAN IS NOT NULL AND PT.TANGGALSURAT IS NOT NULL AND PT.KANTORID = :unitkerjaid ";
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("unitkerjaid", unitkerjaid));

            if (!string.IsNullOrEmpty(pengantarid))
            {
                sql += $" AND PT.PENGANTARSURATID = :pengantarsuratid";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pengantarsuratid", pengantarid));
            }
            if (!string.IsNullOrEmpty(nomorpengantar))
            {
                sql += $" AND PT.NOMOR LIKE '%'||:nomorsurat||'%' ";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorsurat", nomorpengantar));
            }
            //sementara jadi tanggal surat
            if (!string.IsNullOrEmpty(tanggalpengantar))
            {
                sql += $" AND TO_CHAR(TANGGALSURAT,'dd/mm/yyyy') = :tanggal";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggal", tanggalpengantar));
            }
            //bukan jabatan tapi satker tujuan
            if (!string.IsNullOrEmpty(tujuanpengantar))
            {
                sql += $" AND PT.PROFILEIDTUJUAN = :profiletujuan";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("profiletujuan", tujuanpengantar));
            }

            using (var ctx = new BpnDbContext())
            {
                sql += " ORDER BY PT.TANGGALSURAT DESC, PT.NOMOR DESC";
                sql = $"SELECT ROWNUM AS RNUMBER, TB.* FROM ({sql}) TB";
                sql = $"SELECT * FROM ({sql}) WHERE RNUMBER BETWEEN {from} AND {to} ";
                parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.PengantarSurat>(sql, parameters).ToList<Models.Entities.PengantarSurat>();
            }
            return records;
        }

        public List<Surat.Models.Entities.Pegawai> GetPetugasSuratMasukByUnitKerja(string ukid)
        {
            List<Surat.Models.Entities.Pegawai> list = new List<Surat.Models.Entities.Pegawai>();
            string sql = $@" SELECT ROW_NUMBER() over(ORDER BY TBL.tipeuser, TBL.tipeeselonid, TBL.nama) RNumber, COUNT(1) OVER() TOTAL, TBL.pegawaiid, TBL.nama, TBL.profileid, TBL.jabatan, TBL.namalengkap, TBL.tipeeselonid, TBL.nama || ', ' || TBL.jabatan AS NamaDanJabatan
                             FROM (
                                   SELECT 
			                             pegawai.pegawaiid, pegawai.nama, jabatan.nama || decode(jabatanpegawai.statusplt, 1, ' (PLT)', 2, ' (PLH)', '') AS jabatan, decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || decode(pegawai.nama, '', '', pegawai.nama) || decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap, jabatan.profileid, jabatan.tipeeselonid, 0 AS tipeuser 
			                             FROM
			                             pegawai
			                             JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = pegawai.pegawaiid AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND (jabatanpegawai.VALIDSAMPAI IS NULL OR CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
			                             JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid 
                                   JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid AND unitkerja.unitkerjaid = '{ukid}'
			                             UNION
			                             SELECT
			                             ppnpn.nik AS pegawaiid, ppnpn.nama, 'PPNPN' AS jabatan, ppnpn.nama AS NamaLengkap, jabatan.profileid, jabatan.tipeeselonid, 1 AS tipeuser
			                             FROM
			                             ppnpn
			                             JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = ppnpn.nik AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') 
			                             AND (jabatanpegawai.VALIDSAMPAI IS NULL OR CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0' 
			                             JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
			                             JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid AND unitkerja.unitkerjaid = '{ukid}'
			                    ) TBL
			                    INNER JOIN JABATANPEGAWAI JP ON JP.PEGAWAIID = TBL.PEGAWAIID AND JP.PROFILEID = 'A81001' AND (JP.VALIDSEJAK IS NOT NULL AND (JP.VALIDSAMPAI IS NULL OR CAST(JP.VALIDSAMPAI AS TIMESTAMP) > SYSDATE))
			                    WHERE TBL.pegawaiid IS NOT NULL
                                ORDER BY TIPEESELONID DESC, NAMA ASC";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.Pegawai>(sql).ToList<Models.Entities.Pegawai>();
            }

            return list;
        }

        public List<Models.Entities.Surat> GetListSuratForPengantar(string namapegawai, string profileidtu, string tanggal)
        {
            List<Models.Entities.Surat> result = new List<Models.Entities.Surat>();

            string sql = $@"
                            SELECT SI.SURATID FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SURATINBOX SI WHERE SI.NAMAPENGIRIM = '{namapegawai}' AND SI.PROFILEPENERIMA = '{profileidtu}' AND TRUNC(SI.TANGGALTERIMA) = TO_DATE('{tanggal}', 'DD/MM/YYYY') GROUP BY SI.SURATID
                            ";

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<Models.Entities.Surat>(sql).ToList<Models.Entities.Surat>();
            }

            return result;
        }

        public string GetProfileidTuFromUnitKerja(string ukid, bool isMenteri)
        {
            string result = "";

            string sql = $@"
                            SELECT PROFILEIDTU FROM JABATAN WHERE UNITKERJAID = '{ukid}' AND TIPEESELONID <> 0 AND ROWNUM = 1 ORDER BY TIPEESELONID
                            ";

            if (isMenteri)
            {
                sql = $@"SELECT PROFILEIDTU FROM JABATAN WHERE PROFILEID = '{ukid}' AND ROWNUM = 1 ORDER BY TIPEESELONID";
            }

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
            }

            return result;
        }

        public Entities.PengantarSurat GetNewPengantarSurat(string psid)
        {
            Entities.PengantarSurat result = new Entities.PengantarSurat();
            using (var ctx = new BpnDbContext())
            {
                ArrayList arrayListParameters = new ArrayList();
                object[] parameters;
                string query = $@"SELECT 
                                    PENGANTARSURATID, 
                                    KANTORID AS UNITKERJAID, 
                                    NOMOR AS NOMORSURAT, 
                                    TUJUAN AS USERID, 
                                    PROFILEIDTUJUAN, 
                                    NAMAPENERIMA AS PENANDATANGAN, 
                                    TO_CHAR(TANGGALSURAT,'dd/mm/yyyy') AS TANGGALSURAT, 
                                    utl_raw.cast_to_varchar2(METADATA) AS LSTSURAT 
                                FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT WHERE PENGANTARSURATID = :psid";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("psid", psid));
                parameters = arrayListParameters.OfType<object>().ToArray();
                if (!string.IsNullOrEmpty(psid))
                {
                    result = ctx.Database.SqlQuery<Entities.PengantarSurat>(query, parameters).FirstOrDefault();
                }
            }
            return result;
        }

        public string HapusSuratPengantar(string psid, string ukid)
        {
            string result = string.Empty;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters;
                        string query = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT SET KANTORID = :ukid WHERE PENGANTARSURATID = :psid";
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ukid", "D" + ukid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("psid", psid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(query, parameters);
                        tc.Commit();
                        result = "berhasil";
                    }
                    catch (Exception e)
                    {
                        tc.Rollback();
                        result = e.Message;
                    }
                }                
            }
            return result;
        }

        public Entities.TransactionResult SimpanNewSuratPengantar(Models.Entities.PengantarSurat ps)
        {
            var result = new Entities.TransactionResult() {Status = false, Pesan ="Terjadi Kesalahan dalam menyimpan data" } ;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters;
                        if (string.IsNullOrEmpty(ps.PengantarSuratId))
                        {
                            if (!string.IsNullOrEmpty(ps.NomorSurat) && !string.IsNullOrEmpty(ps.TanggalSurat) && !string.IsNullOrEmpty(ps.LstSurat) && !string.IsNullOrEmpty(ps.Penandatangan))
                            {

                                if (ps.ProfileIdTujuan == "H0000001" || ps.ProfileIdTujuan == "H0000002")
                                {
                                    var sm = new SuratModel();
                                    ps.TujuanSurat = sm.GetNamaJabatan(ps.ProfileIdTujuan);
                                }
                                else
                                {
                                    var dm = new DataMasterModel();
                                    ps.TujuanSurat = dm.GetNamaUnitKerjaById(ps.ProfileIdTujuan);
                                }

                                var psid = new Models.SuratModel().GetUID();

                                string sql = $@"INSERT INTO {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT (PENGANTARSURATID, KANTORID, TANGGALDARI, NOMOR, TUJUAN, NAMAPENERIMA, METADATA, PROFILEIDTUJUAN, TANGGALSURAT)
                                        VALUES (:psid, :unitkerja, SYSDATE, :nomorsurat, :pembuat, :penandatangan, utl_raw.cast_to_raw(:listsurat), :tujuansurat, TO_DATE(:tanggalsurat,'DD/MM/YYYY'))";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("psid", psid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("unitkerja", ps.UnitKerjaId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorsurat", ps.NomorSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pembuat", ps.UserId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("penandatangan", ps.Penandatangan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("listsurat", ps.LstSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tujuansurat", ps.ProfileIdTujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggalsurat", ps.TanggalSurat));

                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql,parameters);
                                tc.Commit();
                                result.Status = true;
                                result.Pesan = "Data Berhasil di tambahkan";
                                result.ReturnValue = psid;
                            }
                            else
                            {
                                result.Status = false;
                                result.Pesan = "Terdatap Kekurangan pada data isians";
                            }
                        }
                        else
                        {
                            var pengantar = GetNewPengantarSurat(ps.PengantarSuratId);
                            bool up = false;
                            string update = "";
                            if (ps.NomorSurat != pengantar.NomorSurat)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  NOMOR = :nomorsurat ";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorsurat", ps.NomorSurat));
                            }
                            if (ps.Penandatangan != pengantar.Penandatangan)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  NAMAPENERIMA = :penandatangan";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("penandatangan", ps.Penandatangan));
                            }
                            if (ps.ProfileIdTujuan != pengantar.ProfileIdTujuan)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  PROFILEIDTUJUAN = :tujuansurat";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tujuansurat", ps.ProfileIdTujuan));
                            }
                            if (ps.TanggalSurat != pengantar.TanggalSurat)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  TANGGALSURAT = TO_DATE(:tanggalsurat,'DD/MM/YYYY')";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggalsurat", ps.TanggalSurat));
                            }
                            if (ps.LstSurat != pengantar.LstSurat)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  METADATA = utl_raw.cast_to_raw(:listsurat)";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("listsurat", ps.LstSurat));
                            }

                            if (up)
                            {
                                update = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT SET {update} WHERE PENGANTARSURATID = :psid";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("psid", ps.PengantarSuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(update,parameters);
                                tc.Commit();
                                result.Status = true;
                                result.Pesan = "data Berhasil di update";
                                result.ReturnValue = ps.PengantarSuratId;
                            } else
                            {
                                result.Status = true;
                                result.Pesan = "Tidak ada perubahan pada data";
                                result.ReturnValue = ps.PengantarSuratId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        result.Status = false;
                        result.Pesan = ex.Message;
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

        #endregion Pengantar Baru

        public List<Entities.JumlahSurat> JumlahSuratBelumDibukaDenganTipe(string satkerid, string nip, string myProfiles)
        {
            var result = new List<Entities.JumlahSurat>();

            if (string.IsNullOrEmpty(myProfiles))
            {
                myProfiles = "'xx'";
            }

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT 
                  surat.arah AS Tipe, count(1) AS Jumlah
                FROM {0}.suratinbox
                  JOIN {0}.surat ON
                    surat.suratid = suratinbox.suratid 
                WHERE
                  statusterkirim = 0 AND
                  statusterkunci = 0 and
                  statusforwardtu = 0 AND
                  suratinbox.tindaklanjut <> 'Selesai' AND
                  suratinbox.tanggalbuka IS null AND
                  (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') AND
                  (nip = :Nip OR nip is null) AND
                  suratinbox.profilepenerima IN ({1}) AND
                  NOT EXISTS 
                    (SELECT 1
                     FROM surat.arsipsurat
                     WHERE
                       arsipsurat.suratid = surat.suratid AND
                       arsipsurat.kantorid = :SatkerId) 
                GROUP BY surat.arah", System.Web.Mvc.OtorisasiUser.NamaSkema, myProfiles);
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<Entities.JumlahSurat>(query, parameters).ToList();
            }

            return result;
        }

        public List<Entities.Profile> GetJabatanPunyaSuratPending(string unitkerjaid)
        {
            var records = new List<Entities.Profile>();

            string query = string.Format(@"
                SELECT
                  SI.PROFILEPENERIMA AS PROFILEID, JB.NAMA AS NAMAPROFILE
                FROM {0}.SURATINBOX SI
                  INNER JOIN {0}.SURAT S ON
                    S.SURATID = SI.SURATID AND
                    S.ARAH = 'Masuk'
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = SI.PROFILEPENERIMA AND
                    JP.PEGAWAIID = SI.NIP AND
                    (TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) < TRUNC(SYSDATE) OR NVL(JP.STATUSHAPUS, '0') = '1')
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                    JB.UNITKERJAID = :param1 AND
                    JB.NAMA NOT IN ('PPNPN','Tenaga Ahli')
                  INNER JOIN JABATANPEGAWAI JA ON
  	                JA.PROFILEID = JP.PROFILEID AND
                    NVL(JA.STATUSHAPUS,'0') = '0' AND
                    JA.PEGAWAIID <> JP.PEGAWAIID AND
                    (JA.VALIDSAMPAI IS NULL OR TRUNC(CAST(JA.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
	              INNER JOIN UNITKERJA UK ON
		            UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
                  SI.STATUSTERKUNCI = 0 AND
                  SI.STATUSFORWARDTU = 0 AND
                  SI.STATUSTERKIRIM = 0 AND
                  SI.TINDAKLANJUT <> 'Selesai' AND
                  NOT EXISTS
                    (SELECT 1
                     FROM {0}.ARSIPSURAT
                     WHERE
                       SURATID = SI.SURATID AND
                       KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)) AND
                  NVL(SI.STATUSHAPUS,'0')= '0'
                GROUP BY
                  SI.PROFILEPENERIMA, JB.NAMA", System.Web.Mvc.OtorisasiUser.NamaSkema);

            using (var ctx = new BpnDbContext())
            {
                var arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", unitkerjaid));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.Profile>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.ListSuratPending> GetPejabatPunyaSuratPending(List<string> listunitkerjaid, string profileid, int start, int limit)
        {
            var records = new List<Entities.ListSuratPending>();
            var arrayListParameters = new ArrayList();
            string listParam = string.Empty;
            int i = 1;
            foreach(string unitkerjaid in listunitkerjaid)
            {
                listParam = string.Concat(listParam,string.IsNullOrEmpty(listParam)?":unit":",:unit",i);
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter(string.Concat("unit",i), unitkerjaid));
                i++;
            }

            string query = string.Format(@"
                SELECT * FROM (
	                SELECT
	                  ROW_NUMBER() OVER (ORDER BY(SI.NIP)) AS RNUMBER,
	                  COUNT(1) OVER() AS TOTAL,
	                  SI.NIP AS PEGAWAIID,
	                  NVL(DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG),SI.NAMAPEGAWAI) AS NAMAPEGAWAI,
	                  COUNT(1) AS JUMLAH
                    FROM {0}.SURATINBOX SI
                      INNER JOIN {0}.SURAT S ON
                        S.SURATID = SI.SURATID AND
                        S.ARAH = 'Masuk'
                     LEFT JOIN PEGAWAI PG ON
     	                PG.PEGAWAIID = SI.NIP
                      INNER JOIN JABATANPEGAWAI JP ON
                        JP.PROFILEID = SI.PROFILEPENERIMA AND
                        JP.PEGAWAIID = SI.NIP AND
                        (TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) < TRUNC(SYSDATE) OR NVL(JP.STATUSHAPUS, '0') = '1')
                      INNER JOIN JABATAN JB ON
                        JB.PROFILEID = JP.PROFILEID AND
                        NVL(JB.SEKSIID,'X') <> 'A800' AND
                        (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                        JB.UNITKERJAID IN ({1}) AND
                        JB.NAMA NOT IN ('PPNPN','Tenaga Ahli')
                      INNER JOIN JABATANPEGAWAI JA ON
  	                    JA.PROFILEID = JP.PROFILEID AND
                        NVL(JA.STATUSHAPUS,'0') = '0' AND
                        JA.PEGAWAIID <> JP.PEGAWAIID AND
                        (JA.VALIDSAMPAI IS NULL OR TRUNC(CAST(JA.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
	                  INNER JOIN UNITKERJA UK ON
		                UK.UNITKERJAID = JB.UNITKERJAID
                    WHERE
                      SI.STATUSTERKUNCI = 0 AND
                      SI.STATUSFORWARDTU = 0 AND
                      SI.STATUSTERKIRIM = 0 AND
                      SI.TINDAKLANJUT <> 'Selesai' AND
                      NOT EXISTS
                        (SELECT 1
                         FROM {0}.ARSIPSURAT
                         WHERE
                           SURATID = SI.SURATID AND
                           KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)) AND
                      NVL(SI.STATUSHAPUS,'0')= '0' AND
                      SI.PROFILEPENERIMA = :param2
                    GROUP BY
                	  SI.NIP, NVL(DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG),SI.NAMAPEGAWAI)
                ) RST WHERE RNUMBER BETWEEN :param3 AND :param4", System.Web.Mvc.OtorisasiUser.NamaSkema, listParam);

            using (var ctx = new BpnDbContext())
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", profileid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", start));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", limit));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.ListSuratPending>(query, parameters).ToList();
            }

            return records;
        }

        public List<string> GetListSuratInboxId(string unitkerjaid, string profileid, string pegawaiid)
        {
            var records = new List<string>();

            string query = string.Format(@"
	            SELECT
                    SI.SURATINBOXID
                FROM {0}.SURATINBOX SI
                  INNER JOIN {0}.SURAT S ON
                    S.SURATID = SI.SURATID AND
                    S.ARAH = 'Masuk'
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = SI.PROFILEPENERIMA AND
                    JP.PEGAWAIID = SI.NIP AND
                    (TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) < TRUNC(SYSDATE) OR NVL(JP.STATUSHAPUS, '0') = '1')
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
                    JB.UNITKERJAID = :param1 AND
                    JB.NAMA NOT IN ('PPNPN','Tenaga Ahli')
                  INNER JOIN JABATANPEGAWAI JA ON
  	                JA.PROFILEID = JP.PROFILEID AND
                    NVL(JA.STATUSHAPUS,'0') = '0' AND
                    (JA.VALIDSAMPAI IS NULL OR TRUNC(CAST(JA.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
	              INNER JOIN UNITKERJA UK ON
		            UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
                    SI.STATUSTERKUNCI = 0 AND
                    SI.STATUSFORWARDTU = 0 AND
                    SI.STATUSTERKIRIM = 0 AND
                    SI.TINDAKLANJUT <> 'Selesai' AND
                    NOT EXISTS
                    (SELECT 1
                        FROM {0}.ARSIPSURAT
                        WHERE
                        SURATID = SI.SURATID AND
                        KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)) AND
                    NVL(SI.STATUSHAPUS,'0')= '0' AND
                    SI.PROFILEPENERIMA = :param2 AND
                    SI.NIP = :param3", System.Web.Mvc.OtorisasiUser.NamaSkema);

            using (var ctx = new BpnDbContext())
            {
                var arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", unitkerjaid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", profileid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pegawaiid));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<string>(query, parameters).ToList();
            }

            return records;
        }

        public Entities.TransactionResult DoPeralihanSurat(string unitkerjaid, string profileid, string pegawaiidlama, string pegawaiidbaru, string pelaksana)
        {
            var tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var listSuratInboxId = GetListSuratInboxId(unitkerjaid, profileid, pegawaiidlama);
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

                        string namapegawailama = ctx.Database.SqlQuery<string>(string.Format("SELECT DECODE(GELARDEPAN, '', '', GELARDEPAN || ' ') || DECODE(NAMA, '', '', NAMA) || DECODE(GELARBELAKANG, null, '', ', ' || GELARBELAKANG) FROM PEGAWAI WHERE PEGAWAIID = '{0}'", pegawaiidlama)).FirstOrDefault();
                        string namapegawaibaru = ctx.Database.SqlQuery<string>(string.Format("SELECT DECODE(GELARDEPAN, '', '', GELARDEPAN || ' ') || DECODE(NAMA, '', '', NAMA) || DECODE(GELARBELAKANG, null, '', ', ' || GELARBELAKANG) FROM PEGAWAI WHERE PEGAWAIID = '{0}'", pegawaiidbaru)).FirstOrDefault();
                        foreach (var id in listSuratInboxId)
                        {
                            string suratinboxid = GetUID();
                            string sql = string.Format(@"
                                INSERT INTO {0}.SURATINBOX (SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, REDAKSI, STATUSTERKUNCI, STATUSFORWARDTU, URUTAN, STATUSURGENT)
                                SELECT
	                                :param0 AS SURATINBOXID,
	                                SI.SURATID,
	                                SI.KANTORID,
	                                SI.PROFILEPENERIMA AS PROFILEPENGIRIM,
	                                SI.PROFILEPENERIMA,
	                                SI.NAMAPEGAWAI AS NAMAPENGIRIM,
	                                :param1 AS NIP,
	                                :param2 AS NAMAPEGAWAI,
	                                SYSDATE AS TANGGALKIRIM,
	                                TRUNC(SYSDATE) AS TANGGALTERIMA,
	                                SI.TINDAKLANJUT,
	                                SI.STATUSTERKIRIM,
	                                SI.REDAKSI,
	                                SI.STATUSTERKUNCI,
	                                SI.STATUSFORWARDTU,
	                                (SELECT MAX(URUTAN)+1 FROM {0}.SURATINBOX WHERE SURATID = SI.SURATID) AS URUTAN,
	                                SI.STATUSURGENT
                                FROM {0}.SURATINBOX SI
                                WHERE
                                  SI.SURATINBOXID = :param3", skema);
                            var arrayListParameters = new ArrayList();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param0", suratinboxid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pegawaiidbaru));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", namapegawaibaru));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", id));
                            var parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            string _id = ctx.Database.SqlQuery<string>(string.Format("SELECT SURATOUTBOXID FROM {0}.SURATOUTBOXRELASI WHERE SURATINBOXID = '{1}'", skema, id)).FirstOrDefault();
                            string suratoutboxid = GetUID();
                            sql = string.Format(@"
                                INSERT INTO {0}.SURATOUTBOX (SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, KETERANGAN, PERINTAHDISPOSISI)
                                SELECT
                                   :param0 AS SURATOUTBOXID,
                                   SURATID,
                                   KANTORID,
                                   :param1 AS PROFILEPENGIRIM,
                                   :param2 AS NIP,
                                   SYSDATE AS TANGGALKIRIM,
	                               KETERANGAN,
	                               :param3 AS PERINTAHDISPOSISI
                                FROM {0}.SURATOUTBOX
                                WHERE
                                  SURATOUTBOXID = :param4", skema);
                            arrayListParameters = new ArrayList();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param0", suratoutboxid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", profileid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pegawaiidlama));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", string.Concat("Peralihan Jabatan Kepada ", namapegawaibaru)));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", _id));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            sql = string.Format(@"
                                INSERT INTO {0}.SURATOUTBOXRELASI (SURATOUTBOXID, SURATINBOXID, PROFILEPENERIMA, KETERANGAN, STATUSBACA) 
                                VALUES (:param0, :param1, :param2, :param3, 'D')", skema);
                            arrayListParameters = new ArrayList();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param0", suratoutboxid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", suratinboxid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", profileid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", string.Concat("Peralihan Oleh ", pelaksana)));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            sql = string.Format(@"
                                UPDATE {0}.SURATINBOX SET  
	                                TINDAKLANJUT = 'Selesai',
	                                PERINTAHDISPOSISI = :param1
                                WHERE
	                                SURATINBOXID = :param2",skema);
                            arrayListParameters = new ArrayList();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", string.Concat("Peralihan Jabatan Kepada ",namapegawaibaru)));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", id));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Peralihan Surat Berhasil";
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

        public List<Models.Entities.Surat> GetSuratidFromNomorSuratLike (string nomorsurat)
        {
            List<Models.Entities.Surat> result;
            string query = $@"SELECT SURATID, NOMORSURAT, PERIHAL, PENGIRIM AS PENGIRIMSURAT, TO_CHAR(TANGGALSURAT,'dd/mm/yyyy') AS TANGGALSURAT, TO_CHAR(TANGGALINPUT, 'dd/mm/yyyy HH24:MI') AS TANGGALINPUT
                              FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.SURAT
                              WHERE UTL_MATCH.edit_distance_similarity(APEX_UTIL.URL_ENCODE(NOMORSURAT),'{nomorsurat}') >= 80 AND ARAH = 'Masuk'";

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<Models.Entities.Surat>(query).ToList<Models.Entities.Surat>();
            }

            return result;
        }

        public Entities.DiffWaktu GetWaktuProses(string suratinboxid)
        {
            var result = new Entities.DiffWaktu();
            result.Tahun = 0;
            result.Bulan = 0;
            result.Hari = 0;
            result.Jam = 0;
            result.Menit = 0;

            if (!string.IsNullOrEmpty(suratinboxid))
            {
                try
                {
                    string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                    string query = string.Format(@"
                        SELECT
                          TRUNC(MONTHS_BETWEEN(END_DATE,START_DATE)/12) AS Tahun,
                          TRUNC(MOD( MONTHS_BETWEEN(END_DATE,START_DATE) ,12)) AS Bulan,
                          TRUNC(END_DATE - add_months( START_DATE, MONTHS_BETWEEN(END_DATE,START_DATE))) AS Hari,
                          TRUNC(24*MOD(END_DATE - START_DATE,1)) AS Jam,
                          TRUNC( MOD(MOD(END_DATE - START_DATE,1)*24,1)*60 ) AS Menit
                        FROM
                          (SELECT
                             SYSDATE AS END_DATE, TANGGALKIRIM AS START_DATE
                           FROM {0}.SURATINBOX
                           WHERE
                             SURATINBOXID = :param1)", skema);

                    ArrayList arrayListParameters = new ArrayList();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", suratinboxid));
                    using (var ctx = new BpnDbContext())
                    {
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        result = ctx.Database.SqlQuery<Entities.DiffWaktu>(query, parameters).FirstOrDefault();
                    }
                }
                catch(Exception ex)
                {
                    var msg = ex.Message;
                    return result;
                }
            }

            return result;
        }
        public Entities.DiffWaktu GetWaktuTunggak(string suratinboxid)
        {
            var result = new Entities.DiffWaktu();
            result.Tahun = 0;
            result.Bulan = 0;
            result.Hari = 0;
            result.Jam = 0;
            result.Menit = 0;

            if (!string.IsNullOrEmpty(suratinboxid))
            {
                try
                {
                    string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                    string query = string.Format(@"
                        SELECT
                          TRUNC(MONTHS_BETWEEN(END_DATE,START_DATE)/12) AS Tahun,
                          TRUNC(MOD( MONTHS_BETWEEN(END_DATE,START_DATE) ,12)) AS Bulan,
                          TRUNC(END_DATE - add_months( START_DATE, MONTHS_BETWEEN(END_DATE,START_DATE))) AS Hari,
                          TRUNC(24*MOD(END_DATE - START_DATE,1)) AS Jam,
                          TRUNC( MOD(MOD(END_DATE - START_DATE,1)*24,1)*60 ) AS Menit
                        FROM
                          ( SELECT
                              SYSDATE AS END_DATE, MIN(SP.TANGGALKIRIM) AS START_DATE
                            FROM {0}.SURATINBOX SI
                              INNER JOIN {0}.SURATINBOX SP ON
                                SP.SURATID = SI.SURATID AND
                                SP.KANTORID = SI.KANTORID
                            WHERE
                              SI.SURATINBOXID = :param1)", skema);
                    query = string.Format(@"
                        SELECT
                          TRUNC(MONTHS_BETWEEN(END_DATE,START_DATE)/12) AS Tahun,
                          TRUNC(MOD( MONTHS_BETWEEN(END_DATE,START_DATE) ,12)) AS Bulan,
                          TRUNC(END_DATE - add_months( START_DATE, MONTHS_BETWEEN(END_DATE,START_DATE))) AS Hari,
                          TRUNC(24*MOD(END_DATE - START_DATE,1)) AS Jam,
                          TRUNC( MOD(MOD(END_DATE - START_DATE,1)*24,1)*60 ) AS Menit
                        FROM
                          ( SELECT
                              SYSDATE AS END_DATE, S.TANGGALINPUT AS START_DATE
                            FROM {0}.SURATINBOX SI
                              INNER JOIN {0}.SURAT S ON
                                S.SURATID = SI.SURATID
                            WHERE
                              SI.SURATINBOXID = :param1)", skema);

                    ArrayList arrayListParameters = new ArrayList();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", suratinboxid));
                    using (var ctx = new BpnDbContext())
                    {
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        result = ctx.Database.SqlQuery<Entities.DiffWaktu>(query, parameters).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    return result;
                }
            }

            return result;
        }

        public List<Entities.JumlahSurat> JumlahSuratBelumDibukaDenganTipeV2(string unitkerjaid, string nip)
        {
            var result = new List<Entities.JumlahSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT
                  ST.ARAH AS Tipe, COUNT(1) AS Jumlah
                FROM {0}.SURATINBOX SI
                  INNER JOIN {0}.SURAT ST ON
                    ST.SURATID = SI.SURATID
                  INNER JOIN JABATANPEGAWAI JP ON
  	                JP.PEGAWAIID = :param1 AND
  	                JP.PROFILEID = SI.PROFILEPENERIMA AND
  	                NVL(JP.STATUSHAPUS,'0') = '0' AND
  	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                  INNER JOIN JABATAN JB ON
  	                JB.PROFILEID = JP.PROFILEID AND
  	                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
  	                NVL(JB.SEKSIID,'XXX') <> 'A800' AND
  	                JB.UNITKERJAID = :param2
                  INNER JOIN UNITKERJA UK ON
  	                UK.UNITKERJAID = JB.UNITKERJAID
                  LEFT JOIN {0}.ARSIPSURAT SA ON
  	                SA.SURATID = ST.SURATID AND
  	                SA.KANTORID = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)
                WHERE
                  SI.STATUSTERKIRIM = 0 AND
                  SI.STATUSTERKUNCI = 0 and
                  SI.STATUSFORWARDTU = 0 AND
                  SI.TINDAKLANJUT <> 'Selesai' AND
                  SI.TANGGALBUKA IS NULL AND
                  NVL(SI.STATUSHAPUS,'0') = '0' AND
                  SI.NIP = JP.PEGAWAIID AND
	              SA.ARSIPSURATID IS NULL
                GROUP BY ST.ARAH", System.Web.Mvc.OtorisasiUser.NamaSkema);
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", nip));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<Entities.JumlahSurat>(query, parameters).ToList();
            }

            return result;
        }

        public Entities.TransactionResult DoPeralihanSuratPegawai(string unitkerjaid, string profileidlama, string profileidbaru, string pegawaiid)
        {
            var tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var arrayListParameters = new ArrayList();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", profileidbaru));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", profileidlama));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pegawaiid));
                        var parameters = arrayListParameters.OfType<object>().ToArray();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

                        // SURATINBOX PENGIRIM
                        string sql = string.Format(@"
                            UPDATE {0}.SURATINBOX 
                            SET PROFILEPENGIRIM = :param1
                            WHERE
                              SURATINBOXID IN 
                                (SELECT SURATINBOXID
                                 FROM {0}.SURATOUTBOXRELASI
                                 WHERE
                                   SURATOUTBOXID IN 
                                     (SELECT SURATOUTBOXID
                                      FROM {0}.SURATOUTBOX
                                      WHERE
                                        PROFILEPENGIRIM = :param2 AND
                                        NIP = :param3))", skema);
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // SURATOUTBOX PENGIRIM
                        sql = string.Format(@"
                            UPDATE {0}.SURATOUTBOX 
                            SET PROFILEPENGIRIM = :param1
                            WHERE
                              PROFILEPENGIRIM = :param2 AND
                              NIP = :param3", skema);
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // SURATOUTBOXRELASI PENERIMA
                        sql = string.Format(@"
                            UPDATE {0}.SURATOUTBOXRELASI 
                            SET PROFILEPENERIMA = :param1
                            WHERE
                              SURATINBOXID IN 
                                (SELECT DISTINCT SURATINBOXID
                                 FROM {0}.SURATINBOX
                                 WHERE
                                   PROFILEPENERIMA = :param2 AND
                                   NIP = :param3)", skema);
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // SURATINBOX PENERIMA
                        sql = string.Format(@"
                            UPDATE SURAT.SURATINBOX 
                            SET PROFILEPENERIMA = :param1
                            WHERE
                              PROFILEPENERIMA = :param2 AND
                              NIP = :param3", skema);
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Peralihan Surat Pegawai Berhasil";
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