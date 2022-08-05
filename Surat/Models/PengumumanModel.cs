using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;

namespace Surat.Models
{
    public class PengumumanModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();

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

        public List<DataPengumuman> GetListPengumuman(string unitkerjaid, CariPengumuman filter, int start, int end)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var list = new List<DataPengumuman>();
            var lstparams = new List<object>();
            var parameters = lstparams.ToArray();
            try
            {
                string query =
                    string.Format(@"
	                    SELECT
	                      ROW_NUMBER() OVER (ORDER BY TANGGALDIBUAT) AS RNUMBER, 
	                      COUNT(1) OVER() AS TOTAL,
	                      PENGUMUMANID, 
	                      JUDUL, 
	                      ISI, 
	                      URL, 
	                      VALIDSEJAK, 
	                      VALIDSAMPAI, 
	                      TARGET, 
	                      STATUS
	                    FROM {0}.TBLPENGUMUMAN 
                        WHERE NVL(STATUSHAPUS,'0') = '0' ", skema);

                if (!string.IsNullOrEmpty(filter.Tipe))
                {
                    if (filter.Tipe.Equals("Scheduled"))
                    {
                        query += " AND TRUNC(VALIDSAMPAI) <= TRUNC(SYSDATE) ";
                    }
                    else if (filter.Tipe.Equals("Past"))
                    {
                        query += " AND TRUNC(VALIDSAMPAI) > TRUNC(SYSDATE) ";
                    }
                }
                if (!string.IsNullOrEmpty(filter.MetaData))
                {
                    query += " AND LOWER(JUDUL || ISI || URL) LIKE :metadata ";
                    lstparams.Add(new OracleParameter("metadata", string.Concat("%",filter.MetaData.ToLower(),"%")));
                }
                if (!string.IsNullOrEmpty(unitkerjaid))
                {
                    query += " AND UNITKERJAID = :unitkerja ";
                    lstparams.Add(new OracleParameter("unitkerja", unitkerjaid));
                }

                if (start < end)
                {
                    query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :startCnt AND :limitCnt");
                    lstparams.Add(new OracleParameter("startCnt", start));
                    lstparams.Add(new OracleParameter("limitCnt", end));
                }

                using (var ctx = new BpnDbContext())
                {
                    parameters = lstparams.ToArray();
                    list = ctx.Database.SqlQuery<DataPengumuman>(query, parameters).ToList();                  
                }
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
            }

            return list;
        }

        public List<UnitKerja> GetListUnitKerja(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var list = new List<UnitKerja>();
            var lstparams = new List<object>();
            var parameters = lstparams.ToArray();
            try
            {
                string query =
                    string.Format(@"
	                    SELECT
	                      TPT.TARGETID AS UNITKERJAID, 
	                      UK.NAMAUNITKERJA AS NAMAUNITKERJA
	                    FROM {0}.TBLPENGUMUMAN_TARGET TPT 
                        INNER JOIN UNITKERJA UK ON
                          UK.UNITKERJAID = TPT.TARGETID
                        WHERE 
                          TPT.PENGUMUMANID = :id AND
                          TPT.TIPE = 'UNITKERJA' AND
                          NVL(TPT.STATUSHAPUS,'0') = '0' ", skema);


                using (var ctx = new BpnDbContext())
                {
                    lstparams.Add(new OracleParameter("id", id));
                    parameters = lstparams.ToArray();
                    list = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return list;
        }

        public DataPengumuman GetDataPengumuman(string id)
        {
            var result = new DataPengumuman();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            string query = string.Format(@"
                SELECT
                    PENGUMUMANID,
	                JUDUL,
	                ISI,
	                URL,
                    IMAGEURL,
	                VALIDSEJAK,
	                VALIDSAMPAI,
	                TARGET,
	                STATUS
                FROM {0}.TBLPENGUMUMAN
                WHERE
	                PENGUMUMANID = '{1}' AND NVL(STATUSHAPUS,'0') = '0'", skema, id);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<DataPengumuman>(query).First();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public TransactionResult SimpanPengumuman(DataPengumuman data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new List<object>();
                        var parameters = lstparams.ToArray();
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

                        string sql = string.Format("SELECT COUNT(1) FROM {0}.TBLPENGUMUMAN WHERE PENGUMUMANID = :PengumumanId AND NVL(STATUSHAPUS,'0') = '0'", skema);
                        lstparams.Add(new OracleParameter("PengumumanId", data.PengumumanID));
                        parameters = lstparams.ToArray();
                        if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0) //Update
                        {
                            sql = string.Format(@"
                            UPDATE {0}.TBLPENGUMUMAN SET JUDUL = :Judul, ISI = :Isi, URL = :Url, IMAGEURL = :Image, VALIDSEJAK = :ValidSejak, VALIDSAMPAI = :ValidSampai, TARGET = :Target, UNITKERJAID = :UnitKerjaId, STATUS = 'A', TANGGALPERUBAHAN = SYSDATE, USERPERUBAHAN = :UserId WHERE PENGUMUMANID = :PengumumanId AND NVL(STATUSHAPUS,'0') = '0'", skema);
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("Judul", data.Judul));
                            lstparams.Add(new OracleParameter("Isi", data.Isi));
                            lstparams.Add(new OracleParameter("Url", data.WebUrl));
                            lstparams.Add(new OracleParameter("Image", data.ImageUrl));
                            lstparams.Add(new OracleParameter("ValidSejak", data.ValidSejak));
                            lstparams.Add(new OracleParameter("ValidSampai", data.ValidSampai));
                            lstparams.Add(new OracleParameter("Target", data.Target));
                            lstparams.Add(new OracleParameter("UnitKerjaId", data.UnitKerjaId));
                            lstparams.Add(new OracleParameter("UserId", data.UserId));
                            lstparams.Add(new OracleParameter("PengumumanId", data.PengumumanID));
                            parameters = lstparams.ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else //Insert
                        {
                            sql = string.Format(@"
                                INSERT INTO {0}.TBLPENGUMUMAN (PENGUMUMANID, JUDUL, ISI, URL, IMAGEURL, VALIDSEJAK, VALIDSAMPAI, TARGET, UNITKERJAID, TANGGALDIBUAT, USERPEMBUAT, STATUS)
                                VALUES (:PengumumanId, :Judul, :Isi, :Url, :Image, :ValidSejak, :ValidSampai, :Target, :UnitKerjaId, SYSDATE, :UserId, 'A')", skema);
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("PengumumanId", data.PengumumanID));
                            lstparams.Add(new OracleParameter("Judul", data.Judul));
                            lstparams.Add(new OracleParameter("Isi", data.Isi));
                            lstparams.Add(new OracleParameter("Url", data.WebUrl));
                            lstparams.Add(new OracleParameter("Image", data.ImageUrl));
                            lstparams.Add(new OracleParameter("ValidSejak", data.ValidSejak));
                            lstparams.Add(new OracleParameter("ValidSampai", data.ValidSampai));
                            lstparams.Add(new OracleParameter("Target", data.Target));
                            lstparams.Add(new OracleParameter("UnitKerjaId", data.UnitKerjaId));
                            lstparams.Add(new OracleParameter("UserId", data.UserId));
                            parameters = lstparams.ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Pengumuman berhasil diubah";
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

        public TransactionResult HapusPengumuman(string id, string userid, string username, string alasan)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                        string sql = string.Format(@"
                            UPDATE {0}.TBLPENGUMUMAN SET STATUSHAPUS = '1', TANGGALHAPUS = SYSDATE, USERHAPUS = '{2}', ALASANHAPUS = '{3}' WHERE PENGUMUMANID = '{1}' AND NVL(STATUSHAPUS,'0') = '0'", skema, id, userid, alasan);
                        ctx.Database.ExecuteSqlCommand(sql);

                        sql = string.Format("UPDATE KONTENAKTIF SET TANGGALAKHIRAKSES = SYSDATE, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = '{1}'  WHERE KONTENAKTIFID = '{0}'", id, username);
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Pengumuman berhasil dihapus";
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