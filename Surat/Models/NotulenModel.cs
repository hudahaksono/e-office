using Surat.Codes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class NotulenModel
    {
        public List<Models.Entities.Notulen> GetNotulen(string id, string nip, string metadata, int from, int to)
        {
            List<Models.Entities.Notulen> records = new List<Models.Entities.Notulen>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY notulen.tanggal DESC, notulen.judul, notulen.notulenid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        notulen.notulenid, notulen.unitkerjaid, notulen.nip, notulen.namapegawai, notulen.judul, " +
                "        to_char(notulen.tanggal, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') Tanggal, " +
                "        to_char(notulen.tanggal, 'Day, fmDD Month YYYY', 'nls_date_language=INDONESIAN') TanggalInfo, " +
                "        notulen.waktu, notulen.tempat " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen " +
                "    WHERE " +
                "        notulen.notulenid IS NOT NULL ";

            if (!String.IsNullOrEmpty(id))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                query += " AND (notulen.notulenid = :Id) ";
            }
            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (notulen.nip = :Nip) ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(notulen.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Notulen>(query, parameters).ToList<Models.Entities.Notulen>();
            }

            return records;
        }

        // UNTUK NON TU JUMLAH NOTULEN
        public int JumlahNotulensi(string nip)
        {
            int results = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen " +
                "    WHERE " +
                "        notulen.notulenid IS NOT NULL ";

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (notulen.nip = :Nip) ";
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                results = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return results;
        }

        public List<Models.Entities.Notulen> GetNotulenTop4(string unitkerjaid)
        {
            List<Models.Entities.Notulen> records = new List<Models.Entities.Notulen>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY notulen.tanggal DESC, notulen.judul, notulen.notulenid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        notulen.notulenid, notulen.unitkerjaid, notulen.nip, notulen.namapegawai, notulen.judul, " +
                "        to_char(notulen.tanggal, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') Tanggal, " +
                "        to_char(notulen.tanggal, 'Day, fmDD Month YYYY', 'nls_date_language=INDONESIAN') TanggalInfo, " +
                "        to_char(notulen.tanggal, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') TanggalInfo2, " +
                "        notulen.waktu, notulen.tempat, " +
                "        CASE WHEN LENGTH(notulen.judul) > 20 THEN " +
                "             substr(notulen.judul,1,20) || '...' " +
                "        ELSE notulen.judul END AS JudulShort " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen " +
                "    WHERE " +
                "        notulen.unitkerjaid = :UnitKerjaId " +
                ") WHERE RNUMBER BETWEEN 1 AND 4";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.Notulen>(query, parameters).ToList<Models.Entities.Notulen>();
            }

            return records;
        }

        public Entities.Notulen GetNotulenById(string id)
        {
            Entities.Notulen data = new Entities.Notulen();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    notulen.notulenid, notulen.unitkerjaid, notulen.nip, notulen.namapegawai, notulen.judul, " +
                "    to_char(notulen.tanggal, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') Tanggal, " +
                "    to_char(notulen.tanggal, 'Day, fmDD Month YYYY', 'nls_date_language=INDONESIAN') TanggalInfo, " +
                "    notulen.waktu, notulen.tempat, notulen.isinotulen, " +
                "    surat.suratid, surat.nomorsurat, surat.perihal, to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat ON surat.suratid = notulen.suratid " +
                "WHERE " +
                "    notulen.notulenid = :Id ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.Notulen>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public string GetNotulenIdBySuratId(string suratid)
        {
            string result = "";

            if (!string.IsNullOrEmpty(suratid))
            {
                string query = "SELECT notulenid FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen WHERE suratid = :SuratId";

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

        public int JumlahNotulen(string judul)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen WHERE notulen.notulenid IS NOT NULL ";

            if (!String.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", judul.ToLower()));
                query += " AND LOWER(judul) = :Judul ";
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public Entities.TransactionResult SimpanNotulen(Entities.Notulen notulen)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())

            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string messfooter = " berhasil dibuat.";
                        Regex sWhitespace = new Regex(@"\s+");

                        string sql = "";
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters = null;

                        #region Set Metadata
                        string metadata = "";
                        metadata += notulen.NIP + " ";
                        metadata += notulen.NamaPegawai + " ";
                        metadata += notulen.Perihal + " ";
                        metadata += notulen.NomorSurat + " ";
                        metadata += notulen.TanggalSurat + " ";
                        metadata += notulen.Judul + " ";
                        metadata += notulen.Tanggal + " ";
                        metadata += notulen.Tempat + " ";
                        if (notulen.IsiNotulen.Length > 1500)
                        {
                            metadata += notulen.IsiNotulen.Substring(0, 1500) + " ";
                        }
                        else
                        {
                            metadata += notulen.IsiNotulen + " ";
                        }
                        metadata += notulen.Waktu + " ";
                        metadata = metadata.Trim();
                        #endregion

                        if (!String.IsNullOrEmpty(notulen.NotulenId))
                        {
                            // Edit mode
                            sql =
                                 "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen SET " +
                                 "   judul = :Judul, isinotulen = :IsiNotulen,  " +
                                 "   waktu = :Waktu, tempat = :Tempat,  " +
                                 "   tanggal = TO_DATE(:Tanggal,'DD/MM/YYYY'),  " +
                                 "   metadata = utl_raw.cast_to_raw(:Metadata) " +
                                 "WHERE notulenid = :Id";
                            //sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", notulen.Judul));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiNotulen", notulen.IsiNotulen));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Waktu", notulen.Waktu));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tempat", notulen.Tempat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal", notulen.Tanggal));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", notulen.NotulenId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            messfooter = " berhasil disimpan.";
                        }
                        else
                        {
                            // Insert Mode
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen ( " +
                                "    notulenid, unitkerjaid, nip, namapegawai, judul, tanggal, waktu, tempat, isinotulen, metadata, suratid) VALUES " +
                                "( " +
                                "    :Id,:UnitKerjaId,:NIP,:NamaPegawai,:Judul,TO_DATE(:Tanggal,'DD/MM/YYYY'), " +
                                "    :Waktu,:Tempat,:IsiNotulen,utl_raw.cast_to_raw(:Metadata),:SuratId)";

                            //sql = sWhitespace.Replace(sql, " ");

                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
                            notulen.NotulenId = id;

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", notulen.UnitKerjaId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NIP", notulen.NIP));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", notulen.NamaPegawai));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", notulen.Judul));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal", notulen.Tanggal));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Waktu", notulen.Waktu));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tempat", notulen.Tempat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiNotulen", notulen.IsiNotulen));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", notulen.SuratId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = notulen.NotulenId;
                        tr.Pesan = "Data Notulen " + notulen.Judul + messfooter;
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

        public Entities.TransactionResult HapusNotulen(string id, string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".notulen WHERE notulenid = :id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Notulen berhasil dihapus";
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