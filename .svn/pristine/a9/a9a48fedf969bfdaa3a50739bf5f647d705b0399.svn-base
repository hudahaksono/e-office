using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class PublicModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();


        #region Rapat Online

        public Entities.RapatOnline GetRapatOnlineDetail(string id, string kode)
        {
            var records = new Entities.RapatOnline();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT
                  rapatonline.rapatonlineid,
                  rapatonline.koderapat,
                  rapatonline.judul,
                  rapatonline.urlmeeting,
                  CASE WHEN SYSDATE > rapatonline.tanggal THEN 1 ELSE 0 END AS Status,
                  rapatonline.waktu,
                  rapatonline.keterangan,
                  CASE WHEN kantor.bagianwilayah = 'Barat' THEN to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' WIB' WHEN kantor.bagianwilayah = 'Tengah' THEN to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' WITA' WHEN kantor.bagianwilayah = 'Timur' THEN to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' WIT' ELSE '-' END AS Tanggal
                FROM {0}.rapatonline
                  JOIN unitkerja ON
                    unitkerja.unitkerjaid = rapatonline.unitkerjaid
                  JOIN kantor ON
                    kantor.kantorid = unitkerja.kantorid
                WHERE
                  rapatonline.rapatonlineid IS NOT NULL", System.Web.Mvc.OtorisasiUser.NamaSkema);

            if (!string.IsNullOrEmpty(id))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                query += " AND rapatonline.rapatonlineid = :Id ";
            }
            if (!string.IsNullOrEmpty(kode))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kode", kode));
                query += " AND rapatonline.koderapat = :Kode ";
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RapatOnline>(query, parameters).FirstOrDefault();
            }

            return records;
        }
        public List<Entities.AbsensiRapatOnline> GetListAbsensi(string id)
        {
            var records = new List<Entities.AbsensiRapatOnline>();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                    SELECT
	                    ARO.PEGAWAIID,
	                    NVL(PG.NAMA,PP.NAMA) AS NAMA,
	                    DECODE(PG.PEGAWAIID,NULL,'PPNPN','ASN') AS JABATAN,
	                    TO_CHAR(ARO.TANGGAL, 'MM/DD/YYYY HH24:MI', 'nls_date_language=INDONESIAN') AS TANGGAL
                    FROM {0}.ABSENSIRAPATONLINE ARO
                    LEFT JOIN PEGAWAI PG ON
	                    PG.PEGAWAIID = ARO.PEGAWAIID
                    LEFT JOIN PPNPN PP ON
	                    PP.NIK = ARO.PEGAWAIID
                    WHERE
                      ARO.RAPATONLINEID = :param1
                    GROUP BY
	                    ARO.PEGAWAIID,
	                    NVL(PG.NAMA,PP.NAMA),
	                    DECODE(PG.PEGAWAIID,NULL,'PPNPN','ASN'),
	                    ARO.TANGGAL
                    ORDER BY
	                    ARO.TANGGAL", System.Web.Mvc.OtorisasiUser.NamaSkema);
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", id));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.AbsensiRapatOnline>(query, parameters).ToList();
            }

            return records;
        }

        #endregion
        public string getKodeFile(string id)
        {
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var lstparams = new List<object>();
            string filename = "DokumenTTE.pdf";
            string query = string.Format(@"
                SELECT TDE.KODEFILE
                FROM {0}.TBLDOKUMENELEKTRONIK TDE
                INNER JOIN {0}.TBLDOKUMENTTE TTE ON
	                TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID AND
	                TTE.TIPE = '1' AND
	                TTE.STATUS = 'A'
                WHERE
                  NVL(TDE.STATUSHAPUS,'0') = '0' AND
                  TDE.DOKUMENELEKTRONIKID = :param1 AND
                  TDE.KODEFILE IS NOT NULl
                GROUP BY TDE.KODEFILE", skema);
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    lstparams.Clear();
                    lstparams.Add(new OracleParameter("param1", id));
                    filename = ctx.Database.SqlQuery<string>(query, lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return filename;
        }

        public TransactionResult SimpanBukuTamu(DataBukuTamu data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string sql = "";
            var lstparams = new ArrayList();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        sql = string.Format(@"
                            INSERT INTO {0}.TBLLOGBUKUTAMU 
                              (KANTORID, UNITKERJAID, USERID, NIK, NAMALENGKAP, TEMPATLAHIR, TANGGALLAHIR, NOTELP, EMAIL, ALAMAT, INSTANSI, KEPERLUAN)
                            VALUES 
                              (:param1, :param2, :param3, :param4, :param5, :param6, :param7, :param8, :param9, :param10, :param11, :param12)", skema);
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("param1", data.KantorId));
                        lstparams.Add(new OracleParameter("param2", data.UnitKerjaId));
                        lstparams.Add(new OracleParameter("param3", data.UserId));
                        lstparams.Add(new OracleParameter("param4", data.NIK));
                        lstparams.Add(new OracleParameter("param5", data.NamaLengkap));
                        lstparams.Add(new OracleParameter("param6", data.TempatLahir));
                        lstparams.Add(new OracleParameter("param7", data.TanggalLahir));
                        lstparams.Add(new OracleParameter("param8", data.NoTelp));
                        lstparams.Add(new OracleParameter("param9", data.Email));
                        lstparams.Add(new OracleParameter("param10", data.Alamat));
                        lstparams.Add(new OracleParameter("param11", data.Instansi));
                        lstparams.Add(new OracleParameter("param12", data.Keperluan));
                        ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Buku Tamu Berhasil Ditambahkan";
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