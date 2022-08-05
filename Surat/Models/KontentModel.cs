using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class Konten
    {
        public string DOKID { get; set; }
        public string KANTORID { get; set; }
        public string TIPE { get; set; }
        public string EKSTENSI { get; set; }
        public int VERSI { get; set; }
    }

    public class KontentModel
    {
        public static Konten getKontenEoffice(string pTipe, string pKantorId, string pKeterangan)
        {
            var result = new Konten();
            using (var ctx = new BpnDbContext())
            {
                var lstparams = new List<object>();
                string sql = @"
                        SELECT
	                        KA.KONTENEOFFICEID AS DOKID,
	                        KA.VERSI,
	                        KA.TIPE,
	                        KA.KANTORID,
	                        KA.EXTFILE AS EKSTENSI
                        FROM KONTENEOFFICE KA
                        WHERE
                          KA.TIPE = :param1 AND
                          KA.KANTORID = :param2 AND
                          KA.KETERANGAN = :param3 AND
                          KA.TIPE = 'A'";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTipe));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pKantorId));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKeterangan));
                var parameters = lstparams.ToArray();
                result = ctx.Database.SqlQuery<Konten>(sql, parameters).FirstOrDefault();
            }
            return result;
        }
        public static Konten getDataFotoPPNPN(string nik)
        {
            var result = new Konten();
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    List<object> lstparams = new List<object>();
                    string sql = @"
                        SELECT
	                        KA.KONTENAKTIFID AS DOKID,
	                        KA.VERSI,
	                        KA.TIPE,
	                        KA.KANTORID,
	                        KA.EKSTENSI
                        FROM PPNPN P
                          INNER JOIN PPNPN.FILEPPNPN FL ON
                            FL.PPNPNID = P.PPNPNID AND
                            FL.TIPE = 'FOTOPROFILE' AND
                            NVL(FL.STATUSHAPUS,'0') = '0'
                          INNER JOIN KONTENAKTIF KA ON
  	                        KA.KONTENAKTIFID = FL.FILEPPNPNID AND
  	                        KA.TIPE = FL.FOLDER
                        WHERE
                          P.NIK = :nik AND
                          NVL(P.STATUSHAPUS,'0') = '0'";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nik", nik));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<Konten>(sql, parameters).FirstOrDefault();
                }
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
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT versi FROM kontenaktif WHERE kontenaktifid = :id";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
                }
            }
            return result;
        }

        public int JumlahKonten(string id)
        {
            int result = 0;

            string query = "SELECT count(*) FROM kontenaktif WHERE kontenaktifid = :id";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public string GetKontentAktif(string id)
        {
            string result = "";
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT kontenaktifid FROM kontenaktif WHERE kontenaktifid = :id";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                }
            }
            return result;
        }

        public Entities.TransactionResult SimpanKontenFile(string kantorid, string dokumenid, string judul, string petugas, string tanggaldokumen, string tipedokumen, out int versi, string fileext = "pdf")
        {
            versi = 0;
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        string kontenid = this.GetKontentAktif(dokumenid);
                        if (String.IsNullOrEmpty(kontenid))
                        {
                            // Insert mode
                            sql = "INSERT INTO KONTENAKTIF (KONTENAKTIFID,VERSI,TANGGALSISIP,PETUGASSISIP,TANGGALSUNTING,PETUGASSUNTING,TIPE,KANTORID,JUDUL,EKSTENSI) VALUES (:pId,0,NVL(NULLIF(:pTanggal,''),SYSDATE),:pNamaPetugas,SYSDATE,:pNamaPetugasSunting,:pTipeDokumen,:pKantorId,:pJudul,:pExt)";
                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("pId", dokumenid);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("pTanggal", tanggaldokumen);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugas", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugasSunting", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("pTipeDokumen", tipedokumen);
                            Oracle.ManagedDataAccess.Client.OracleParameter p6 = new Oracle.ManagedDataAccess.Client.OracleParameter("pKantorId", kantorid);
                            Oracle.ManagedDataAccess.Client.OracleParameter p7 = new Oracle.ManagedDataAccess.Client.OracleParameter("pJudul", judul);
                            Oracle.ManagedDataAccess.Client.OracleParameter p8 = new Oracle.ManagedDataAccess.Client.OracleParameter("pExt", fileext);
                            object[] parameters = new object[8] { p1, p2, p3, p4, p5, p6, p7, p8 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Edit mode
                            //sql = "INSERT INTO KONTENPASIF SELECT SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, :DokumenId, 1 FROM KONTENAKTIF WHERE KONTENAKTIFID = :DokumenId";
                            sql = @"
                                INSERT INTO KONTEN.KONTENPASIF (KONTENPASIFID, KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI, IPADDRESS)
                                SELECT
                                  SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP,
                                  TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL,
                                  EKSTENSI, TANGGALSINKRONISASI, DOKUMENID, EDISI,IPADDRESS
                                FROM KONTEN.KONTENAKTIF
                                WHERE
                                  KONTENAKTIFID = :DokumenId";
                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("DokumenId", dokumenid);
                            object[] parameters = new object[1] { p1 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("DokumenId", dokumenid);
                            parameters = new object[1] { p1 };
                            versi = ctx.Database.SqlQuery<int>("SELECT nvl(max(versi),0)+1 FROM kontenpasif WHERE kontenaktifid = :DokumenId", parameters).FirstOrDefault();

                            sql = "UPDATE KONTENAKTIF SET VERSI = :pVersi, TANGGALSISIP = SYSDATE, PETUGASSISIP = :pNamaPetugas, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :pNamaPetugasSunting, JUDUL = :pJudul, EKSTENSI = :pExt WHERE KONTENAKTIFID = :DokumenId";
                            p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("pVersi", versi);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugas", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugasSunting", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("pJudul", judul);
                            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("pExt", fileext);
                            Oracle.ManagedDataAccess.Client.OracleParameter p6 = new Oracle.ManagedDataAccess.Client.OracleParameter("DokumenId", dokumenid);
                            parameters = new object[6] { p1, p2, p3, p4, p5, p6 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tr.Status = true;
                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public Konten getKontenAktif(string id)
        {
            var result = new Konten();
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    List<object> lstparams = new List<object>();
                    string sql = @"
                        SELECT 
                          KONTENAKTIFID, VERSI, TIPE, KANTORID, EKSTENSI
                        FROM KONTENAKTIF
                        WHERE
                          KONTENAKTIFID = :param1 AND
                          (TANGGALAKHIRAKSES IS NULL OR TRUNC(CAST(TANGGALAKHIRAKSES AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", id));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<Konten>(sql, parameters).FirstOrDefault();
                }
            }
            return result;
        }
    }
}