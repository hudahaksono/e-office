using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static Surat.Codes.Functions;

namespace Surat.Models
{
    public class KuisionerModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();


        public Entities.TransactionResult SimpanPertanyaan(Entities.Pertanyaan data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())

            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        // Edit mode
                        if (!string.IsNullOrEmpty(data.Pertanyaan_Id))
                        {

                            var sql = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.KUISIONER SET STATUS='{data.Status}', NAMA_PERTANYAAN = '{data.Nama_Pertanyaan}', METADATAPERTANYAAN = '{data.Pilihan}' 
                                      WHERE PERTANYAAN_ID = '{data.Pertanyaan_Id}' ";
                            ctx.Database.ExecuteSqlCommand(sql);
                            tr.Pesan = "diubah";
                        }

                        else
                        {
                            // Insert Mode
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                            data.Pertanyaan_Id = id;

                            var sql = $@"INSERT INTO {System.Web.Mvc.OtorisasiUser.NamaSkema}.KUISIONER (PERTANYAAN_ID, STATUS, STATUSHAPUS, NAMA_PERTANYAAN, TANGGAL, METADATAPERTANYAAN, USERID) 
                                    VALUES ('{id}', '{data.Status}', '{data.StatusHapus}', '{data.Nama_Pertanyaan}', SYSDATE, '{data.Pilihan}', '{data.UserId}')
                                    ";
                            ctx.Database.ExecuteSqlCommand(sql);
                            tr.Pesan = "berhasil";
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.Pertanyaan_Id;
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


        public List<Pertanyaan> GetPertanyaan(string id, string userId, Boolean tx)
        {
            List<Pertanyaan> records = new List<Pertanyaan>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT PERTANYAAN_ID, TIPE, STATUS, STATUSHAPUS, NAMA_PERTANYAAN, TO_CHAR(TANGGAL,'MM-DD-YYYY HH24:MI:SS') TANGGAL, METADATAPERTANYAAN PILIHAN, USERID   
                         FROM {skema}.KUISIONER 
                         WHERE STATUSHAPUS = '0' ";
            if (!string.IsNullOrEmpty(id))
            {
                sql += $"AND PERTANYAAN_ID = '{id}' ";
            }
            if (!string.IsNullOrEmpty(userId))
            {
                sql += $"AND USERID = '{userId}' ";
            }
            if (tx)
            {
                sql += $"AND TIPE IS NOT NULL ";
            }

            if (tx)
            {
                sql += "ORDER BY TO_NUMBER(TIPE, '99') ASC ";
            }
            else
            {
                sql += "ORDER BY TANGGAL DESC ";
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Pertanyaan>(sql).ToList();
            }
            return records;
        }
        public List<Pertanyaan> GetPertanyaanTerakhir(string id, string userId, Boolean tx)
        {
            List<Pertanyaan> records = new List<Pertanyaan>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT PERTANYAAN_ID, TIPE, STATUS, STATUSHAPUS, NAMA_PERTANYAAN, TO_CHAR(TANGGAL,'MM-DD-YYYY HH24:MI:SS') TANGGAL, METADATAPERTANYAAN PILIHAN, USERID   
                         FROM {skema}.KUISIONER 
                         WHERE STATUSHAPUS = '0' ";
            if (!string.IsNullOrEmpty(id))
            {
                sql += $"AND PERTANYAAN_ID = '{id}' ";
            }
            if (!string.IsNullOrEmpty(userId))
            {
                sql += $"AND USERID = '{userId}' ";
            }
            if (tx)
            {
                sql += $"AND TIPE IS NOT NULL ";
            }

            if (tx)
            {
                sql += "ORDER BY TO_NUMBER(TIPE, '99') DESC";
            }
            else
            {
                sql += "ORDER BY TANGGAL DESC";
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Pertanyaan>(sql).ToList();
            }
            return records;
        }

        public List<Pertanyaanall> GetPertanyaanall(string id, string userId, Boolean tx)
        {
            List<Pertanyaanall> records = new List<Pertanyaanall>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT PERTANYAAN_ID, TIPE, STATUS, STATUSHAPUS, NAMA_PERTANYAAN, TO_CHAR(TANGGAL,'MM-DD-YYYY HH24:MI:SS') TANGGAL, METADATAPERTANYAAN PILIHAN, USERID   
                         FROM {skema}.KUISIONER 
                         WHERE STATUSHAPUS = '0' ";
            if (!string.IsNullOrEmpty(id))
            {
                sql += $"AND PERTANYAAN_ID = '{id}' ";
            }
            if (!string.IsNullOrEmpty(userId))
            {
                sql += $"AND USERID = '{userId}' ";
            }
            if (tx)
            {
                sql += $"AND TIPE IS NOT NULL ";
            }

            if (tx)
            {
                sql += "ORDER BY TO_NUMBER(TIPE, '99') ASC ";
            }
            else
            {
                sql += "ORDER BY TANGGAL DESC ";
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Pertanyaanall>(sql).ToList();
            }
            return records;
        }


        internal List<KuisionerJawaban> GetReportJawaban(string id, string userId, Boolean tx)
        {
            List<KuisionerJawaban> records = new List<KuisionerJawaban>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT
                        jawaban_kuisioner.pertanyaan_id,
                        jawaban_kuisioner.nama_jawaban,
                        Count(jawaban_kuisioner.nama_jawaban) AS jml_jawaban
                        FROM
                        {System.Web.Mvc.OtorisasiUser.NamaSkema}.jawaban_kuisioner ";
            sql += $"WHERE pertanyaan_id = '{id}' AND statushapus = '0' ";
            sql += $"GROUP BY jawaban_kuisioner.pertanyaan_id, jawaban_kuisioner.nama_jawaban ";
            sql += $"ORDER BY jawaban_kuisioner.nama_jawaban ASC";
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<KuisionerJawaban>(sql).ToList();
            }
            return records;
        }

        internal List<KuisionerJawabanall> GetReportJawabanall(string id, string userId, Boolean tx)
        {
            List<KuisionerJawabanall> records = new List<KuisionerJawabanall>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT
                        Count(jawaban_kuisioner.nama_jawaban) AS jml_jawaban_all
                        FROM
                        {System.Web.Mvc.OtorisasiUser.NamaSkema}.jawaban_kuisioner ";
            sql += $"WHERE pertanyaan_id = '{id}' AND statushapus = '0' ";
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<KuisionerJawabanall>(sql).ToList();
            }
            return records;
        }

        internal List<KuisionerJawaban> GetJawabanIndividu(string userId)
        {
            List<KuisionerJawaban> records = new List<KuisionerJawaban>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT KS.NAMA_PERTANYAAN, JK.NAMA_JAWABAN, KS.TIPE FROM {skema}.JAWABAN_KUISIONER JK 
            LEFT JOIN {skema}.KUISIONER KS ON KS.PERTANYAAN_ID = JK.PERTANYAAN_ID 
            WHERE JK.USERID = '{userId}' AND JK.STATUSHAPUS = '0' ORDER BY TO_NUMBER(KS.TIPE, '99') ASC";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<KuisionerJawaban>(sql).ToList();
            }
            return records;
        }




        public Entities.TransactionResult SimpanJawaban(List<KuisionerJawaban> datas)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var data in datas)
                        {
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                            var sql = $@"INSERT INTO {skema}.JAWABAN_KUISIONER (JAWABAN_ID, NAMA_JAWABAN, PERTANYAAN_ID, TANGGAL, USERID, STATUSHAPUS) 
                                      VALUES ('{id}', '{data.Nama_Jawaban}', '{data.Pertanyaan_Id}', SYSDATE, '{data.UserId}','0')  ";
                            ctx.Database.ExecuteSqlCommand(sql);
                        }
                        tc.Commit();
                        tr.Pesan = $@"Jawaban Berhasil disimpan";
                        tr.Status = true;
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

        public bool GetNotifKuisioner(userIdentity log)
        {
            ArrayList arrayListParameters = new ArrayList();
            var tipeeselonid = "";
            string query = $@"SELECT TO_CHAR(JB.TIPEESELONID)
                            FROM JABATANPEGAWAI JP
                            LEFT JOIN JABATAN JB ON JP.PROFILEID = JB.PROFILEID 
                            WHERE PEGAWAIID = :pegawaiid 
                            AND  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) 
                            AND (JB.TIPEESELONID > 0 AND JB.TIPEESELONID <= 4)
                            ORDER BY JB.TIPEESELONID DESC";
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pegawaiid", log.PegawaiId));
            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                tipeeselonid = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }
            return !string.IsNullOrWhiteSpace(tipeeselonid) && int.Parse(tipeeselonid) <= 3;
        }

        public Entities.TransactionResult HapusPertanyaan(string id, string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            var sql = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.KUISIONER SET STATUSHAPUS='1', TIPE = NULL, USERIDHAPUS = '{userid}', TANGGALHAPUS = SYSDATE WHERE PERTANYAAN_ID = '{id}' AND USERID = '{userid}' ";
                            ctx.Database.ExecuteSqlCommand(sql);
                            tc.Commit();
                            tr.Pesan = $@"Pertanyaan Dengan Id [{id}] Berhasil Dihapus";
                            tr.Status = true;
                            tr.ReturnValue = id;
                        }
                        else
                        {
                            tr.Pesan = "Id Pertanyaan Tidak ada";
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
        public Entities.TransactionResult SetTipe(string id, string userid, string nom)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            int result = 0;
                            string query = $@"
                            SELECT count(*) 
                            FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.KUISIONER 
                            WHERE USERID = '{userid}' AND TIPE = '{nom}' 
                            ";
                            result = ctx.Database.SqlQuery<int>(query).First();

                            if (result == 0)
                            {
                                var sql = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.KUISIONER SET TIPE = '{nom}' WHERE PERTANYAAN_ID = '{id}' AND USERID = '{userid}' ";
                                ctx.Database.ExecuteSqlCommand(sql);
                                tc.Commit();
                                tr.Pesan = $@"Berhasil [{nom}]";
                                tr.Status = true;
                                tr.ReturnValue = id;
                            }
                            else
                            {
                                tr.Pesan = "Nomor Sudah digunakan";
                            }

                        }
                        else
                        {
                            tr.Pesan = "Id Pertanyaan Tidak ada";
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

        public int GetDataPengisian(string userid)
        {
            int hasil = 0;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var sql = $@"SELECT COUNT(USERID) FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.JAWABAN_KUISIONER WHERE USERID = '{userid}' AND STATUSHAPUS = '0' ";
                        int result = ctx.Database.SqlQuery<int>(sql).FirstOrDefault();
                        hasil = Convert.ToInt32(result);
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }
            return hasil;
        }




        public List<KuisionerJawaban> GetidHapus(string jawaban_id)
        {
            List<KuisionerJawaban> records = new List<KuisionerJawaban>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT JAWABAN_ID    
                         FROM {skema}.JAWABAN_KUISIONER 
                         WHERE STATUSHAPUS = '0' JAWABAN_ID = '{jawaban_id}'";

            //var sql = $@"SELECT KS.NAMA_PERTANYAAN, JK.NAMA_JAWABAN, KS.TIPE, JK.STATUSHAPUS  FROM {skema}.JAWABAN_KUISIONER JK 
            //LEFT JOIN {skema}.KUISIONER KS ON KS.PERTANYAAN_ID = JK.PERTANYAAN_ID 
            //WHERE JK.STATUSHAPUS = '0' ORDER BY TO_NUMBER(KS.TIPE, '99') ASC";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<KuisionerJawaban>(sql).ToList();
            }
            return records;
        }




        public Entities.TransactionResult HapusTabelHasilJawaban1(KuisionerJawaban data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {

                    try
                    {
                        //string jawaban_id = ctx.Database.SqlQuery<string>("SELECT JAWABAN_ID FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.JAWABAN_KUISIONER").FirstOrDefault();
                        var sql = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.JAWABAN_KUISIONER 
                        SET STATUSHAPUS='1', TANGGALHAPUS = SYSDATE, USERIDHAPUS = '{data.UserIDHapus}'
                        WHERE STATUSHAPUS = '0'";
                        ctx.Database.ExecuteSqlCommand(sql);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Jawaban Kuisioner Berhasil DiHapus";
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

        public Entities.TransactionResult HapusTabelHasilJawaban2(KuisionerJawaban data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {

                    try
                    {
                        //string jawaban_id = ctx.Database.SqlQuery<string>("SELECT JAWABAN_ID {System.Web.Mvc.OtorisasiUser.NamaSkema}.JAWABAN_KUISIONER  ").FirstOrDefault();
                        var sql = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.JAWABAN_KUISIONER 
                        SET STATUSHAPUS='2', TANGGALHAPUS = '{data.TanggalHapus}', USERIDHAPUS = '{data.UserIDHapus}'
                        WHERE STATUSHAPUS = '1'";
                        ctx.Database.ExecuteSqlCommand(sql);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Jawaban Kuisioner Berhasil DiHapus";
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

        public List<KuisionerJawaban> GetTabelHasilJawaban1()
        {
            List<KuisionerJawaban> records = new List<KuisionerJawaban>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT KS.NAMA_PERTANYAAN, KS.PERTANYAAN_ID, JK.JAWABAN_ID, JK.NAMA_JAWABAN, KS.TIPE, JK.STATUSHAPUS
                        FROM {skema}.JAWABAN_KUISIONER JK 
                        LEFT JOIN {skema}.KUISIONER KS ON KS.PERTANYAAN_ID = JK.PERTANYAAN_ID 
                        WHERE JK.STATUSHAPUS = '0' ORDER BY TO_NUMBER(KS.TIPE, '99') ASC";
            //var sql = $@"UPDATE {skema}.JAWABAN_KUISIONER SET STATUSHAPUS = '1'  WHERE STATUSHAPUS = '0'";



            //var sql = $@"SELECT KS.NAMA_PERTANYAAN, JK.NAMA_JAWABAN, KS.TIPE, JK.STATUSHAPUS  FROM {skema}.JAWABAN_KUISIONER JK 
            //LEFT JOIN {skema}.KUISIONER KS ON KS.PERTANYAAN_ID = JK.PERTANYAAN_ID 
            //WHERE JK.STATUSHAPUS = '0' ORDER BY TO_NUMBER(KS.TIPE, '99') ASC";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<KuisionerJawaban>(sql).ToList();
            }
            return records;

        }

        public List<KuisionerJawaban> GetTabelHasilJawaban2()
        {
            List<KuisionerJawaban> records = new List<KuisionerJawaban>();
            var skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            var sql = $@"SELECT KS.NAMA_PERTANYAAN, KS.PERTANYAAN_ID, JK.JAWABAN_ID, JK.NAMA_JAWABAN, KS.TIPE, JK.STATUSHAPUS
                        FROM {skema}.JAWABAN_KUISIONER JK 
                        LEFT JOIN {skema}.KUISIONER KS ON KS.PERTANYAAN_ID = JK.PERTANYAAN_ID 
                        WHERE JK.STATUSHAPUS = '1' ORDER BY TO_NUMBER(KS.TIPE, '99') ASC";
            //var sql = $@"UPDATE {skema}.JAWABAN_KUISIONER SET STATUSHAPUS = '1'  WHERE STATUSHAPUS = '0'";



            //var sql = $@"SELECT KS.NAMA_PERTANYAAN, JK.NAMA_JAWABAN, KS.TIPE, JK.STATUSHAPUS  FROM {skema}.JAWABAN_KUISIONER JK 
            //LEFT JOIN {skema}.KUISIONER KS ON KS.PERTANYAAN_ID = JK.PERTANYAAN_ID 
            //WHERE JK.STATUSHAPUS = '0' ORDER BY TO_NUMBER(KS.TIPE, '99') ASC";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<KuisionerJawaban>(sql).ToList();
            }
            return records;

        }


        /// ///////////////////////////////////////////////






    }
}
