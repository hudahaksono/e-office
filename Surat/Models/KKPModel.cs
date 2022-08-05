using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Surat.Models
{
    public class KKPModel
    {
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();
        DataMasterModel mDataMaster = new DataMasterModel();

        public string GetUID()
        {
            string id = "";

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
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
                try
                {
                    string result = ctx.Database.SqlQuery<string>("SELECT to_char(sysdate,'DD/MM/YYYY HH24:MI') FROM DUAL").FirstOrDefault();
                    retval = Convert.ToDateTime(result);
                }
                finally
                {
                    ctx.Dispose();
                }
            }

            return retval;
        }

        public int GetServerYear()
        {
            int retval = DateTime.Now.Year;

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    string result = ctx.Database.SqlQuery<string>("SELECT to_char(sysdate,'YYYY') FROM DUAL").FirstOrDefault();
                    retval = Convert.ToInt32(result);
                }
                finally
                {
                    ctx.Dispose();
                }
            }

            return retval;
        }

        public List<ComboList> GetProvinsiList(string kantorid)
        {
            var records = new List<ComboList>();
            var arrayListParameters = new ArrayList();

            try
            {
                string query = string.Empty;
                using (var ctx = new BpnDbContext())
                {
                    var tipekantorid = mDataMaster.GetTipeKantor(kantorid);
                    if (tipekantorid == 1)
                    {
                        query = "SELECT WILAYAHID AS VALUE, INITCAP(NAMA) AS TEXT FROM WILAYAH WHERE TIPEWILAYAHID = 1 AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) GROUP BY WILAYAHID, NAMA, KODE ORDER BY NAMA";
                    }
                    else
                    {
                        query = "SELECT WL.WILAYAHID AS VALUE, INITCAP(WL.NAMA) AS TEXT FROM WILAYAH WL INNER JOIN WILAYAHKANTOR WK ON WK.KANTORID = :param1 AND WL.WILAYAHID = WK.WILAYAHID WHERE WL.TIPEWILAYAHID = 1 AND (WL.VALIDSAMPAI IS NULL OR TRUNC(CAST(WL.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) GROUP BY WL.WILAYAHID, WL.NAMA, WL.KODE ORDER BY WL.NAMA";
                        arrayListParameters.Add(new OracleParameter("param1", kantorid));
                    }
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    records = ctx.Database.SqlQuery<ComboList>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return records;
        }

        public List<ComboList> GetWilayahList(string kantorid, string induk)
        {
            var records = new List<ComboList>();
            var arrayListParameters = new ArrayList();

            try
            {
                using (var ctx = new BpnDbContext())
                {
                    var tipekantorid = mDataMaster.GetTipeKantor(kantorid);
                    string query = string.Concat("SELECT WL.WILAYAHID AS VALUE, INITCAP(WL.NAMA) AS TEXT FROM WILAYAH WL INNER JOIN WILAYAHKANTOR WK ON WL.WILAYAHID = WK.WILAYAHID WHERE WL.INDUK = :param1 ", tipekantorid > 1 ? " AND WK.KANTORID = :param2 " : "", " AND (WL.VALIDSAMPAI IS NULL OR TRUNC(CAST(WL.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) GROUP BY WL.WILAYAHID, WL.NAMA, WL.KODE ORDER BY WL.NAMA");
                    arrayListParameters.Add(new OracleParameter("param1", induk));
                    if (tipekantorid > 1)
                    {
                        arrayListParameters.Add(new OracleParameter("param2", kantorid));
                    }
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    records = ctx.Database.SqlQuery<ComboList>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return records;
        }


        public List<ComboList> GetTahunList(string jenis, string wilayahid, string tipe)
        {
            var records = new List<ComboList>();
            var arrayListParameters = new ArrayList();

            try
            {
                using (var ctx = new BpnDbContext())
                {
                    if (jenis.Equals("DOKUMENPENGUKURAN"))
                    {
                        string query = @"
                    SELECT CAST(TAHUN AS VARCHAR2(4)) AS VALUE, CAST(TAHUN AS VARCHAR2(4)) AS TEXT
                    FROM DOKUMENPENGUKURAN
                    WHERE
                      WILAYAHID = :param1 AND
                      TIPEDOKUMENID = :param2 AND
                      (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) 
                    GROUP BY TAHUN
                    ORDER BY TAHUN";
                        arrayListParameters.Add(new OracleParameter("param1", wilayahid));
                        arrayListParameters.Add(new OracleParameter("param2", tipe));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        records = ctx.Database.SqlQuery<ComboList>(query, parameters).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return records;
        }

        public List<LinkSurat> GetSuratLink(string suratid, int from, int to)
        {
            var records = new List<LinkSurat>();

            var arrayListParameters = new ArrayList();

            try
            {
                string query = string.Format(@"
                SELECT * FROM (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY TDL.DOKUMENTIPE, TDL.NOMORDOKUMEN) AS RNUMBER, COUNT(1) OVER() AS TOTAL,
                        TDL.LINKID, TDL.SURATID, TDL.KANTORID, KT.NAMA AS NAMAKANTOR, TDL.DOKUMENID, TDL.DOKUMENTIPE, TDL.NOMORDOKUMEN, TDL.KETERANGAN AS NOTE,
                        (SELECT DISTINCT KONTENAKTIFID
                            FROM KONTENAKTIF
                            WHERE
                                KONTENAKTIFID = TDL.DOKUMENID AND
                                LOWER(TIPE) LIKE DECODE(TDL.DOKUMENTIPE,'BT1','bukutanah%','BT2','bukutanah%','BT3','bukutanah%','BT4','bukutanah%','BT5','bukutanah%','BT7','bukutanah%','BT8','bukutanah%','%%')) AS KONTENAKTIFID
                    FROM
                        {0}.TBLDOKUMENLINK TDL
                    INNER JOIN KANTOR KT ON
                        KT.KANTORID = TDL.KANTORID
                    WHERE
                        TDL.SURATID = :param1 AND NVL(TDL.STATUS,'A') <> 'D'", OtorisasiUser.NamaSkema);

                arrayListParameters.Add(new OracleParameter("param1", suratid));

                query +=
                    " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

                arrayListParameters.Add(new OracleParameter("startCnt", from));
                arrayListParameters.Add(new OracleParameter("limitCnt", to));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    records = ctx.Database.SqlQuery<LinkSurat>(query, parameters).ToList();
                }
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
            }

            return records;
        }

        public List<DokumenSurat> GetDokumen(string suratid, string kantorid, string jenis, string tipe, string wilayahid, string nomor, string tahun, int from, int to)
        {
            var records = new List<DokumenSurat>();

            string query = string.Empty;
            try
            {

                if (jenis.Equals("DOKUMENHAK"))
                {
                    query = @"
                    SELECT RST.*
                    FROM
                      (SELECT *
                       FROM
                         (SELECT
                            DOKUMENID, NOMOR, WILAYAH, VALIDSEJAK, VALIDSAMPAI,
                            STATUSHAPUS, TIPE, R, KONTENAKTIFID, KANTORID,
                            ROW_NUMBER() OVER(ORDER BY NOMOR ASC) AS RNUMBER,
                            COUNT(1) OVER() AS TOTAL
                          FROM
                            (SELECT
                               dh.DOKUMENHAKID AS DOKUMENID, dh.NOMOR, INITCAP(q1.NAMA) AS WILAYAH, dh.VALIDSEJAK, dh.VALIDSAMPAI,
                               NVL(dh.STATUSHAPUS,'0') AS STATUSHAPUS, dh.TIPEDOKUMENID AS TIPE,
                               (SELECT DISTINCT KONTENAKTIFID
                                FROM KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = dh.DOKUMENHAKID AND
                                  LOWER(TIPE) LIKE 'bukutanah%') KONTENAKTIFID, NULL BUKUTANAHID,
                               (SELECT KANTORID
                                FROM WILAYAHKANTOR
                                WHERE
                                  WILAYAHID = dh.WILAYAHID) KANTORID,
                               ROW_NUMBER() OVER (ORDER BY dh.NOMOR ASC) ROWNUMS,
                               COUNT(1) OVER() TOTAL,
                               RANK() OVER (PARTITION BY dh.DOKUMENHAKID ORDER BY dh.VALIDSAMPAI DESC) R
                             FROM DOKUMENHAK dh
                               INNER JOIN (SELECT WILAYAHID, NAMA FROM WILAYAH w WHERE w.TIPEWILAYAHID IN (2,3,4,6,7) START WITH w.WILAYAHID = :idWilayah CONNECT BY PRIOR w.WILAYAHID = w.INDUK) q1 ON
                                 dh.WILAYAHID = q1.WILAYAHID
                               INNER JOIN WILAYAHKANTOR wk ON
                                 wk.WILAYAHID= dh.WILAYAHID AND
                                 wk.KANTORID = :idKantor
                             WHERE
                               dh.TIPEDOKUMENID = :tipeHak AND dh.NIBEL IS NULL ";
                    string additionalClause = string.Empty;
                    if (!string.IsNullOrEmpty(nomor))
                    {
                        string[] separatedByComma = nomor.Split(",".ToCharArray());
                        string clause = " AND (";
                        foreach (String s in separatedByComma)
                        {
                            string[] separatedByDash = s.Split("-".ToCharArray());
                            if (separatedByDash.Length == 2)
                            {
                                if (Decimal.Parse(separatedByDash[0]) > Decimal.Parse(separatedByDash[1]))
                                {
                                    additionalClause = additionalClause + clause + @"(TO_NUMBER(REGEXP_Replace(SUBSTR(dh.NOMOR,10,5), '\D', '')) >= " + Decimal.Parse(separatedByDash[1]) + @" AND TO_NUMBER(REGEXP_Replace(SUBSTR(dh.NOMOR,10,5), '\D', '')) <= " + Decimal.Parse(separatedByDash[0]) + ")";
                                }
                                else
                                {
                                    additionalClause = additionalClause + clause + @"(TO_NUMBER(REGEXP_Replace(SUBSTR(dh.NOMOR,10,5), '\D', '')) >= " + Decimal.Parse(separatedByDash[0]) + @" AND TO_NUMBER(REGEXP_Replace(SUBSTR(dh.NOMOR,10,5), '\D', '')) <= " + Decimal.Parse(separatedByDash[1]) + ")";
                                }
                                clause = " OR ";
                            }
                            else
                            {
                                additionalClause = additionalClause + clause + @"TO_NUMBER(REGEXP_Replace(SUBSTR(dh.NOMOR,10,5), '\D', '')) = " + Decimal.Parse(s);
                                clause = " OR ";
                            }
                        }

                        additionalClause = additionalClause + ")";
                        query += additionalClause;
                    }

                    Regex sWhitespace = new Regex(@"\s+");
                    var tipekantor = new DataMasterModel().GetTipeKantor(kantorid);
                    if (tipekantor == 1 || tipekantor == 2)
                    {
                        query = sWhitespace.Replace(query, " ");
                        query = query.Replace("AND wk.KANTORID = :idKantor", "AND wk.KANTORID IN (SELECT kantorid FROM kantor START WITH kantorid = :idKantor CONNECT BY PRIOR kantorid=induk)");
                    }
                    query += @"
                        ORDER BY dh.NOMOR)
                          WHERE
                            R = 1)
                       WHERE
                         RNUMBER BETWEEN :startCnt AND
                         :limitCnt) RST";
                    using (var ctx = new BpnDbContext())
                    {
                        List<object> lstparams = new List<object>();
                        lstparams.Add(new OracleParameter("idWilayah", wilayahid));
                        lstparams.Add(new OracleParameter("idKantor", kantorid));
                        lstparams.Add(new OracleParameter("tipeHak", tipe));
                        lstparams.Add(new OracleParameter("startCnt", from));
                        lstparams.Add(new OracleParameter("limitCnt", to));
                        query = sWhitespace.Replace(query, " ");

                        records = ctx.Database.SqlQuery<DokumenSurat>(query, lstparams.ToArray()).ToList();
                    }
                }
                else if (jenis.Equals("DOKUMENPENGUKURAN"))
                {
                    query = query =
                    @"SELECT * FROM (
                  SELECT d.DOKUMENPENGUKURANID AS DOKUMENID, (d.NOMOR || '/' || d.TAHUN) AS NOMOR, 
			             d.TIPEDOKUMENID AS TIPE, 
			             d.VALIDSEJAK, d.VALIDSAMPAI, 
			             COALESCE(NVL(s.STATUSHAPUS,'0'), NVL(d.STATUSHAPUS,'0')) AS STATUSHAPUS,
			             INITCAP(w.NAMA) AS WILAYAH,
			             wk.KANTORID,
                         (SELECT DISTINCT KONTENAKTIFID FROM KONTENAKTIF WHERE KONTENAKTIFID = d.DOKUMENPENGUKURANID) KONTENAKTIFID,
  		                 ROW_NUMBER() OVER (ORDER BY NOMOR ASC) AS RNUMBER, COUNT(1) OVER() AS TOTAL
                    FROM DOKUMENPENGUKURAN d, SURATUKUR s, WILAYAHKANTOR wk,
			             (SELECT WILAYAHID, NAMA, t.TIPE
   				            FROM WILAYAH w, TIPEWILAYAH t
   			               WHERE w.TIPEWILAYAHID IN (2,3,4,6,7)
     			             AND t.TIPEWILAYAHID = w.TIPEWILAYAHID
   			               START WITH w.WILAYAHID = :idWilayah
   		                 CONNECT BY PRIOR w.WILAYAHID = w.INDUK) w,
  		                 (SELECT KANTORID FROM KANTOR START WITH KANTORID = :idKantor CONNECT BY PRIOR KANTORID = INDUK) k
                   WHERE s.DOKUMENPENGUKURANID = d.DOKUMENPENGUKURANID
                     AND s.VALIDSAMPAI IS NULL
                     AND w.WILAYAHID = d.WILAYAHID(+)
 	                 AND wk.WILAYAHID = w.WILAYAHID
 	                 AND k.KANTORID = wk.KANTORID";
                    string additionalClause = String.Empty;
                    if (!String.IsNullOrEmpty(nomor))
                    {
                        string[] separatedByComma = nomor.Split(",".ToCharArray());
                        string clause = " AND (";
                        foreach (String s in separatedByComma)
                        {
                            string[] separatedByDash = s.Split("-".ToCharArray());
                            if (separatedByDash.Length == 2)
                            {
                                if (Decimal.Parse(separatedByDash[0]) > Decimal.Parse(separatedByDash[1]))
                                {
                                    additionalClause = additionalClause + clause + @"(MOD(NOMORVIRTUAL,100000) >= " + Decimal.Parse(separatedByDash[1]) + @" AND MOD(NOMORVIRTUAL,100000) <= " + Decimal.Parse(separatedByDash[0]) + ")";
                                }
                                else
                                {
                                    additionalClause = additionalClause + clause + @"(MOD(NOMORVIRTUAL,100000) >= " + Decimal.Parse(separatedByDash[0]) + @" AND MOD(NOMORVIRTUAL,100000) <= " + Decimal.Parse(separatedByDash[1]) + ")";
                                }
                                clause = " OR ";
                            }
                            else
                            {
                                additionalClause = additionalClause + clause + @"MOD(NOMORVIRTUAL,100000) = " + Decimal.Parse(s);
                                clause = " OR ";
                            }
                        }

                        additionalClause = additionalClause + ")";
                        query += additionalClause;
                    }

                    if (!String.IsNullOrEmpty(tahun))
                    {
                        additionalClause = String.Empty;
                        string[] separatedByComma = tahun.Split(",".ToCharArray());
                        string clause = " AND (";
                        foreach (String s in separatedByComma)
                        {
                            string[] separatedByDash = s.Split("-".ToCharArray());
                            if (separatedByDash.Length == 2)
                            {
                                if (Decimal.Parse(separatedByDash[0]) > Decimal.Parse(separatedByDash[1]))
                                {
                                    additionalClause = additionalClause + clause + @"(TAHUN >= " + Decimal.Parse(separatedByDash[1]) + @" AND TAHUN <= " + Decimal.Parse(separatedByDash[0]) + ")";
                                }
                                else
                                {
                                    additionalClause = additionalClause + clause + @"(TAHUN >= " + Decimal.Parse(separatedByDash[0]) + @" AND TAHUN <= " + Decimal.Parse(separatedByDash[1]) + ")";
                                }
                                clause = " OR ";
                            }
                            else
                            {
                                additionalClause = additionalClause + clause + @"TAHUN = " + Decimal.Parse(s);
                                clause = " OR ";
                            }
                        }

                        additionalClause = additionalClause + ") ";
                        query += additionalClause;
                    }

                    string tipeClause = tipe == "*" ? " AND TIPEDOKUMENID IN ('SU','GS','SUS','PLL','GT') " : " AND TIPEDOKUMENID = :tipeSU ";
                    query += tipeClause + ") WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

                    using (var ctx = new BpnDbContext())
                    {
                        Regex sWhitespace = new Regex(@"\s+");
                        List<object> lstparams = new List<object>();
                        lstparams.Add(new OracleParameter("idWilayah", wilayahid));
                        lstparams.Add(new OracleParameter("idKantor", kantorid));
                        if (tipe != "*")
                        {
                            lstparams.Add(new OracleParameter("tipeSU", tipe));
                        }
                        lstparams.Add(new OracleParameter("startCnt", from));
                        lstparams.Add(new OracleParameter("limitCnt", to));
                        query = sWhitespace.Replace(query, " ");
                        records = ctx.Database.SqlQuery<DokumenSurat>(query, lstparams.ToArray()).ToList();
                    }
                }
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
            }

            return records;
        }

        public TransactionResult TambahLinkDokumen(LinkSurat data, string userid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Empty;
                        string skema = OtorisasiUser.NamaSkema;
                        var lstparams = new List<object>();
                        sql = string.Format("SELECT COUNT(1) FROM {0}.TBLDOKUMENLINK WHERE SURATID = :param1 AND DOKUMENID = :param2 AND NVL(STATUS,'A') <> 'D'", skema);
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("param1", data.SuratId));
                        lstparams.Add(new OracleParameter("param2", data.DokumenId));
                        if (ctx.Database.SqlQuery<int>(sql,lstparams.ToArray()).FirstOrDefault() == 0)
                        {
                            sql = string.Format(@"INSERT INTO {0}.TBLDOKUMENLINK (LINKID, SURATID, DOKUMENID, KANTORID, DOKUMENTIPE, NOMORDOKUMEN, USERBUAT, TANGGALBUAT, KETERANGAN, STATUS) 
                                                    VALUES (:param1, :param2, :param3, :param4, :param5, :param6, :param7, SYSDATE, :param8, 'A')", skema);
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", GetUID()));
                            lstparams.Add(new OracleParameter("param2", data.SuratId));
                            lstparams.Add(new OracleParameter("param3", data.DokumenId));
                            lstparams.Add(new OracleParameter("param4", data.KantorId));
                            lstparams.Add(new OracleParameter("param5", data.DokumenTipe));
                            lstparams.Add(new OracleParameter("param6", data.NomorDokumen));
                            lstparams.Add(new OracleParameter("param7", userid));
                            lstparams.Add(new OracleParameter("param8", data.Note));
                            ctx.Database.ExecuteSqlCommand(sql,lstparams.ToArray());

                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Tambah Link Dokumen Berhasil";
                        }
                        else
                        {
                            tc.Rollback();
                            tr.Pesan = "Dokumen sudah pernah di link kan";
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
    }
}