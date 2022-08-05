using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Npgsql;

namespace Surat.Models
{
    public class KearsipanModel
    {
        Regex sWhitespace = new Regex(@"\s+");


        /// MENU MASTER ARSIP /// - 12 April 2022 FIX
        public List<Models.Entities.MasterArsip> GetListSemuaArsip(string metadata, string golonganarsip, string tahun, int from, int to, string unitkerja)
        {
            List<Models.Entities.MasterArsip> records = new List<Models.Entities.MasterArsip>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            string query = $@"SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER,  DENSE_RANK() OVER(ORDER BY masterarsip.id) AS NoUrut,
                           masterarsipdetail.id,                           
                           masterarsip.nomorsk,
                           masterarsip.kodeklasifikasi,
                           masterarsip.golonganarsip,
                           masterarsipdetail.nomorurut,
                           masterarsipdetail.jenisarsip,
                           masterarsipdetail.tahun,
                           masterarsipdetail.jumlahberkas,
                           masterarsipdetail.perkembangan,
                           masterarsip.gedung,
                           masterarsip.lantai,
                           masterarsip.rak,
                           masterarsip.nomorboks,
                           masterarsipdetail.keterangan, 
                           masterarsipdetail.statusberkas
                           FROM ""{skema}"".masterarsip
                           LEFT JOIN ""{skema}"".masterarsipdetail ON masterarsipdetail.nomorsk = masterarsip.nomorsk 
                           WHERE masterarsip.statushapus = '0' AND masterarsipdetail.statushapus = '0'
                           AND masterarsip.unitkerjaid = :unitkerjaid";

            arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", unitkerja));

            if (!String.IsNullOrEmpty(tahun))
            {
                arrayListParameters.Add(new NpgsqlParameter("tahun", tahun));
                query += $" AND masterarsip.tahun = :tahun";
            }
            if (!String.IsNullOrEmpty(golonganarsip))
            {
                arrayListParameters.Add(new NpgsqlParameter("golonganarsip", golonganarsip));
                query += $" AND masterarsip.golonganarsip = :golonganarsip";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new NpgsqlParameter("metadata", metadata));
                query += $" AND UPPER(masterarsip.nomorsk) LIKE '%'||UPPER(:metadata)||'%'";
                //query += $" AND UPPER(masterarsip.nomorsk) LIKE '%'||UPPER(:metadata)||'%'" +
                //         $" OR UPPER(masterarsipdetail.jenisarsip) LIKE '%'||UPPER(:metadata)||'%'";
            }

            query += $" ORDER BY  masterarsip.id, masterarsipdetail.nomorurut ASC";

            query = string.Concat("SELECT * FROM (", query, ") AS T WHERE NoUrut BETWEEN :pStart AND :pEnd");
            arrayListParameters.Add(new NpgsqlParameter("pStart", from));
            arrayListParameters.Add(new NpgsqlParameter("pEnd", to));


            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.MasterArsip>(query, parameters).ToList<Models.Entities.MasterArsip>();
            }

            return records;
        }

        public List<Models.Entities.MasterArsip> GetListSemuaArsipExport(string metadata, string golonganarsip, string tahun, int from, int to, string unitkerja)
        {
            List<Models.Entities.MasterArsip> records = new List<Models.Entities.MasterArsip>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            string query = $@"SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER,  DENSE_RANK() OVER(ORDER BY masterarsip.id) AS NoUrut,
                           masterarsip.id,                           
                           masterarsip.nomorsk,
                           masterarsip.kodeklasifikasi,
                           masterarsip.golonganarsip,
                           masterarsipdetail.nomorurut,
                           masterarsipdetail.jenisarsip,
                           masterarsipdetail.tahun,
                           masterarsipdetail.jumlahberkas,
                           masterarsipdetail.perkembangan,
                           masterarsip.gedung,
                           masterarsip.lantai,
                           masterarsip.rak,
                           masterarsip.nomorboks,
                           masterarsipdetail.keterangan  
                           FROM ""{skema}"".masterarsip
                           LEFT JOIN ""{skema}"".masterarsipdetail ON masterarsipdetail.nomorsk = masterarsip.nomorsk 
                           WHERE masterarsip.statushapus = '0' AND masterarsipdetail.statushapus = '0' AND unitkerjaid = :unitkerja";

            arrayListParameters.Add(new NpgsqlParameter("unitkerja", unitkerja));


            if (!String.IsNullOrEmpty(tahun))
            {
                arrayListParameters.Add(new NpgsqlParameter("tahun", tahun));
                query += $" AND masterarsip.tahun = :tahun";
            }
            if (!String.IsNullOrEmpty(golonganarsip))
            {
                arrayListParameters.Add(new NpgsqlParameter("golonganarsip", golonganarsip));
                query += $" AND masterarsip.golonganarsip = :golonganarsip";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new NpgsqlParameter("metadata", metadata));
                query += $" AND UPPER(masterarsip.nomorsk) LIKE '%'||UPPER(:metadata)||'%'" +
                         $" OR UPPER(masterarsipdetail.jenisarsip) LIKE '%'||UPPER(:metadata)||'%'";
            }

            query += $" ORDER BY  masterarsip.id, masterarsipdetail.nomorurut ASC";

            //query = string.Concat("SELECT * FROM (", query, ") AS T WHERE NoUrut BETWEEN :pStart AND :pEnd");
            //arrayListParameters.Add(new NpgsqlParameter("pStart", from));
            //arrayListParameters.Add(new NpgsqlParameter("pEnd", to));


            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.MasterArsip>(query, parameters).ToList<Models.Entities.MasterArsip>();
            }

            return records;
        }
        public List<Models.Entities.MasterArsip> GetMasterArsip()
        {
            Models.Entities.MasterArsip data = new Models.Entities.MasterArsip();
            var list = new List<Models.Entities.MasterArsip>();
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;
            string query = $@"SELECT id, nomorsk, kodeklasifikasi, jenisarsip, tahun, jumlahberkas, perkembangan, gedung, lantai, rak, nomorboks, keterangan, golonganarsip FROM ""{skema}"".masterarsip WHERE statushapus = '0'";

            using (var ctx = new PostgresDbContext())
            {
                list = ctx.Database.SqlQuery<Models.Entities.MasterArsip>(query).ToList();
            }
            return list;

        }
        public List<Models.Entities.KlasifikasiMasterArsip> GetKlasifikasiMasterArsip()
        {
            var list = new List<KlasifikasiMasterArsip>();
            string skema = OtorisasiUser.NamaSkema;

            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string query = $@"SELECT id, kodeklasifikasi, jenisarsip, keterangan, kategori, listarsip, keteranganlokasi FROM ""{skema}"".klasifikasimasterarsip WHERE statushapus = '0'";

            using (var ctx = new PostgresDbContext())
            {
                list = ctx.Database.SqlQuery<KlasifikasiMasterArsip>(query).ToList<KlasifikasiMasterArsip>();
            }
            return list;
        }
        public List<Models.Entities.MasterArsip> GetListMasterArsip(string Tahun, string GolonganArsip, string MetaData, int from, int to, string unitkerja)
        {
            List<Models.Entities.MasterArsip> records = new List<Models.Entities.MasterArsip>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            string query = $@"SELECT ROW_NUMBER() OVER(ORDER BY { 1}) AS RNUMBER, id, nomorsk, kodeklasifikasi, jenisarsip, tahun, gedung, lantai, rak, nomorboks, golonganarsip from ""{skema}"".masterarsip WHERE statushapus = '0' AND unitkerjaid = :unitkerja";
            arrayListParameters.Add(new NpgsqlParameter("unitkerja", unitkerja));

            if (!String.IsNullOrEmpty(Tahun))
            {
                arrayListParameters.Add(new NpgsqlParameter("Tahun", Tahun));
                query += $" AND tahun= :Tahun";
            }
            if (!String.IsNullOrEmpty(GolonganArsip))
            {
                arrayListParameters.Add(new NpgsqlParameter("GolonganArsip", GolonganArsip));
                query += $" AND golonganarsip= :GolonganArsip";
            }
            if (!String.IsNullOrEmpty(MetaData))
            {
                arrayListParameters.Add(new NpgsqlParameter("MetaData", HttpUtility.UrlEncode(MetaData)));
                query += $" AND UPPER(nomorsk) LIKE '%'||UPPER(:metadata)||'%'";

                //query += $" AND UPPER(nomorsk) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(kodeklasifikasi) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(jenisarsip) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(jumlahberkas) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(perkembangan) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(gedung) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(lantai) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(rak) LIKE '%'||UPPER(:metadata)||'%'" +
                //       $"OR UPPER(nomorboks) LIKE '%'||UPPER(:metadata)||'%'";
            }
            query += $" ORDER BY golonganarsip, id DESC";


            query = string.Concat("SELECT * FROM (", query, ") AS T WHERE RNUMBER BETWEEN :pStart AND :pEnd");
            arrayListParameters.Add(new NpgsqlParameter("pStart", from));
            arrayListParameters.Add(new NpgsqlParameter("pEnd", to));


            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.MasterArsip>(query, parameters).ToList<Models.Entities.MasterArsip>();
                //records = ctx.Database.SqlQuery<MasterArsip>($"SELECT * FROM ({query}) AS M WHERE RNUMBER BETWEEN {from} AND {to} ").ToList();
            }

            return records;
        }
        public Entities.TransactionResult InsertMasterArsip(Models.Entities.MasterArsip masterArsip)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            int? idedit = masterArsip.Id;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (idedit == 0)
                        {
                            // Insert Master
                            string query = $@"INSERT INTO ""{skema}"".masterarsip (nomorsk, kodeklasifikasi, jenisarsip, tahun, jumlahberkas, perkembangan, gedung, lantai, rak, nomorboks, keterangan, statushapus, tanggalinput, userinput, unitkerjaid, golonganarsip, kodeunik) 
                            VALUES ('{masterArsip.NomorSK}', '{masterArsip.KodeKlasifikasi}', '{masterArsip.JenisArsip}', '{masterArsip.Tahun}', '{masterArsip.JumlahBerkas}', '{masterArsip.Perkembangan}', '{masterArsip.Gedung}', '{masterArsip.Lantai}', '{masterArsip.Rak}', '{masterArsip.NomorBoks}', '{masterArsip.Keterangan}', '{masterArsip.StatusHapus}', '{masterArsip.TanggalInput}', '{masterArsip.UserInput}', '{masterArsip.UnitKerjaId}','{masterArsip.GolonganArsip}','{masterArsip.KodeUnik}' )";

                            ctx.Database.ExecuteSqlCommand(query);
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil ditambahkan";
                        }
                        else
                        {
                            // Update Master
                            string query = $@"UPDATE ""{skema}"".masterarsip SET nomorsk = '{masterArsip.NomorSK}', kodeklasifikasi = '{masterArsip.KodeKlasifikasi}', tahun = '{masterArsip.Tahun}',   gedung = '{masterArsip.Gedung}', lantai = '{masterArsip.Lantai}', rak = '{masterArsip.Rak}', nomorboks = '{masterArsip.NomorBoks}', statushapus = '{masterArsip.StatusHapus}', tanggalinput = '{masterArsip.TanggalInput}', userinput = '{masterArsip.UserInput}', tanggalhapus = '{masterArsip.TanggalHapus}' , golonganarsip = '{masterArsip.GolonganArsip}' WHERE Id = '{masterArsip.Id}'";
                            ctx.Database.ExecuteSqlCommand(query);

                            // Update nomorsk detail
                            string query2 = $@"UPDATE ""{skema}"".masterarsipdetail SET nomorsk = '{masterArsip.NomorSK}' WHERE kodeunik = '{masterArsip.Id}'";
                            ctx.Database.ExecuteSqlCommand(query2);
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil diupdate";
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
        public Entities.TransactionResult DeleteMasterArsip(Models.Entities.MasterArsip data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string query = $@"UPDATE ""{skema}"".masterarsip SET statushapus = '1', tanggalhapus = '{data.TanggalHapus}', userhapus = '{data.UserHapus}' WHERE Id = '{id}'";
                        ctx.Database.ExecuteSqlCommand(query);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data Arsip Berhasil DiHapus";
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

        public int GetFirstNum(string nourut)
        {
            string final = "0"; //if there's nothing, it'll return 0
            foreach (char c in nourut) //loop the string
            {
                try
                {
                    Convert.ToInt32(c.ToString()); //if it can convert
                    final += c.ToString(); //add to final string
                }
                catch (FormatException) //if NaN
                {
                    break; //break out of loop
                }
            }

            return Convert.ToInt32(final); //return the int
        }

        public Entities.TransactionResult InsertMasterArsipDetail(Models.Entities.MasterArsipDetail arsipDetail, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            int? idedit = arsipDetail.Id;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (idedit == 0)
                        {
                            // Insert Master
                            string queri = $@"INSERT INTO ""{skema}"".masterarsipdetail (nomorsk, nomorurut, statusberkas, jenisarsip, tahun, jumlahberkas, perkembangan, keterangan, statushapus, tanggalinput, userinput, kodeunik) 
                            VALUES ('{arsipDetail.NomorSK}', '{arsipDetail.NomorUrut}', '0', '{arsipDetail.JenisArsip}', '{arsipDetail.Tahun}', '{arsipDetail.JumlahBerkas}', '{arsipDetail.Perkembangan}', '{arsipDetail.Keterangan}', '{arsipDetail.StatusHapus}', '{arsipDetail.TanggalInput}', '{arsipDetail.UserInput}', '{arsipDetail.KodeUnik}')";

                            ctx.Database.ExecuteSqlCommand(queri);
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil ditambahkan";
                        }
                        else
                        {
                            // Update Master
                            string queri = $@"UPDATE ""{skema}"".masterarsipdetail SET nomorsk = '{arsipDetail.NomorSK}', nomorurut = '{arsipDetail.NomorUrut}', jenisarsip = '{arsipDetail.JenisArsip}',  tahun = '{arsipDetail.Tahun}', jumlahberkas = '{arsipDetail.JumlahBerkas}', perkembangan = '{arsipDetail.Perkembangan}', keterangan = '{arsipDetail.Keterangan}', statushapus = '{arsipDetail.StatusHapus}', tanggalinput = '{arsipDetail.TanggalInput}', userinput = '{arsipDetail.UserInput}', tanggalhapus = '{arsipDetail.TanggalHapus}' WHERE Id = '{arsipDetail.Id}'";
                            ctx.Database.ExecuteSqlCommand(queri);
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil diupdate";
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
        public Entities.TransactionResult DeleteMasterArsipDetail(Models.Entities.MasterArsipDetail data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {

                        string queri = $@"UPDATE ""{skema}"".masterarsipdetail SET statushapus = '1', tanggalhapus = '{data.TanggalHapus}', userhapus = '{data.UserHapus}' WHERE Id = '{id}'";
                        ctx.Database.ExecuteSqlCommand(queri);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data telah dihapus";
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
        public List<Models.Entities.MasterArsipDetail> GetKeteranganDetailArsip()
        {
            List<Models.Entities.MasterArsipDetail> records = new List<Models.Entities.MasterArsipDetail>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            string query = $@"SELECT keterangan, COUNT(*) FROM ""{skema}"".masterarsipdetail WHERE statushapus = '0' GROUP BY keterangan HAVING COUNT(*) > 1";

            using (var ctx = new PostgresDbContext())
            {
                records = ctx.Database.SqlQuery<Models.Entities.MasterArsipDetail>(query).ToList<Models.Entities.MasterArsipDetail>();
            }

            return records;
        }
        public List<Models.Entities.MasterArsipDetail> GetListMasterArsipDetail(string nomorsk, int from, int to)
        {
            List<Models.Entities.MasterArsipDetail> records = new List<Models.Entities.MasterArsipDetail>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            string query = $@"SELECT id, nomorsk, jenisarsip, tahun, jumlahberkas, perkembangan, keterangan, COUNT(*) AS TOTAL FROM ""{skema}"".masterarsipdetail WHERE statushapus = '0' AND nomorsk = '{nomorsk}' GROUP BY id ORDER BY jenisarsip ASC";

            using (var ctx = new PostgresDbContext())
            {
                records = ctx.Database.SqlQuery<Models.Entities.MasterArsipDetail>(query).ToList<Models.Entities.MasterArsipDetail>();
            }

            return records;
        }
        public List<Models.Entities.GolonganMasterArsip> GetGolonganMasterArsip()
        {
            var list = new List<GolonganMasterArsip>();
            string skema = OtorisasiUser.NamaSkema;

            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string query = $@"select id, namagolongan, userinput, userhapus, tglhapus, statushapus from ""{skema}"".golonganmasterarsip  WHERE statushapus = '0'";

            using (var ctx = new PostgresDbContext())
            {
                list = ctx.Database.SqlQuery<GolonganMasterArsip>(query).ToList<GolonganMasterArsip>();
            }
            return list;
        }
        public List<Models.Entities.GolonganMasterArsip> GetGolonganMasterArsip2(string unitkerja)
        {
            var list = new List<GolonganMasterArsip>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();


            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            string query = $@" SELECT DISTINCT GM.NAMAGOLONGAN AS NAMAJENIS,
                                      (GM.NAMAGOLONGAN || ' (' || COUNT(GM.NAMAGOLONGAN) || ')') AS VALUEJENISARSIP FROM ""{skema}"".GOLONGANMASTERARSIP GM
                                      INNER JOIN ""{skema}"".MASTERARSIP M ON GM.NAMAGOLONGAN = M.GOLONGANARSIP
                                      WHERE M.STATUSHAPUS = '0' AND M.UNITKERJAID = :unitkerjaid
                                      GROUP BY GM.NAMAGOLONGAN";

            arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", unitkerja));

            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                list = ctx.Database.SqlQuery<GolonganMasterArsip>(query, parameters).ToList<GolonganMasterArsip>();
            }
            return list;
        }

        /// MENU JENIS NASKAH DINAS /// - 12 April 2022 FIX
        public List<Models.Entities.GolonganMasterArsip> GetListGolonganArsip(int from, int to)
        {
            List<Models.Entities.GolonganMasterArsip> records = new List<Models.Entities.GolonganMasterArsip>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();


            string query = $@"SELECT id, namagolongan, userinput, userhapus, tglhapus, statushapus FROM ""{skema}"".golonganmasterarsip  WHERE statushapus = '0' ORDER BY namagolongan ASC";


            using (var ctx = new PostgresDbContext())
            {
                records = ctx.Database.SqlQuery<Models.Entities.GolonganMasterArsip>(query).ToList<Models.Entities.GolonganMasterArsip>();
            }

            return records;
        }
        public Entities.TransactionResult InsertGolonganMasterArsip(Models.Entities.GolonganMasterArsip data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            // Insert Golongan Master
                            string query = $@"INSERT INTO ""{skema}"".golonganmasterarsip (namagolongan, userinput, statushapus) 
                            VALUES ('{data.NamaGolongan}', '{data.UserInput}', '{data.StatusHapus}')";
                            ctx.Database.ExecuteSqlCommand(query);
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil ditambahkan";
                        }
                        else
                        {
                            // Update Golongan Master
                            string query = $@"UPDATE ""{skema}"".golonganmasterarsip SET namagolongan = '{data.NamaGolongan}', userinput ='{data.UserInput}' WHERE Id = '{id}'";
                            ctx.Database.ExecuteSqlCommand(query);
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil diupdate";
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
        public Entities.TransactionResult DeleteGolonganMasterArsip(Models.Entities.GolonganMasterArsip data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {

                        string queri = $@"UPDATE ""{skema}"".golonganmasterarsip SET statushapus = '1', tglhapus = '{data.TglHapus}', userhapus = '{data.UserHapus}' WHERE Id = '{id}'";
                        ctx.Database.ExecuteSqlCommand(queri);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Golongan Master Arsip Berhasil DiHapus";
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

        /// MENU KLASIFIKASI ARSIP /// - 12 April 2022 FIX
        public List<Models.Entities.KlasifikasiMasterArsip> GetListKlasifikasiMasterArsip(string ListArsip, string KeteranganLokasi, string MetaData, int from, int to)
        {
            List<Models.Entities.KlasifikasiMasterArsip> records = new List<Models.Entities.KlasifikasiMasterArsip>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();


            string query = $@"SELECT id, kodeklasifikasi, jenisarsip, keterangan, kategori, keteranganlokasi from ""{skema}"".klasifikasimasterarsip  WHERE statushapus = '0'";


            if (!String.IsNullOrEmpty(ListArsip))
            {
                arrayListParameters.Add(new NpgsqlParameter("ListArsip", ListArsip));
                query += $" AND kategori= :ListArsip";
            }
            if (!String.IsNullOrEmpty(KeteranganLokasi))
            {
                arrayListParameters.Add(new NpgsqlParameter("KeteranganLokasi", KeteranganLokasi));
                query += $" AND keteranganlokasi= :KeteranganLokasi";
            }
            if (!String.IsNullOrEmpty(MetaData))
            {
                arrayListParameters.Add(new NpgsqlParameter("MetaData", MetaData));
                query += $" AND UPPER(kodeklasifikasi) LIKE '%'||UPPER(:metadata)||'%'";
            }

            query += $" ORDER BY kodeklasifikasi ASC";

            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.KlasifikasiMasterArsip>(query, parameters).ToList<Models.Entities.KlasifikasiMasterArsip>();
            }

            return records;
        }

        public Entities.TransactionResult InsertKlasifikasiMasterArsip(Models.Entities.KlasifikasiMasterArsip data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            // Insert Klasifikasi Master
                            string queri = $@"INSERT INTO ""{skema}"".klasifikasimasterarsip (kodeklasifikasi, jenisarsip, keterangan, kategori, keteranganlokasi,  statushapus, userinput) 
                            VALUES ('{data.KodeKlasifikasi}', '{data.JenisArsip}', '{data.Keterangan}', '{data.Kategori}', '{data.KeteranganLokasi}', '{data.StatusHapus}', '{data.UserInput} = Input {DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt")}')";

                            ctx.Database.ExecuteSqlCommand(queri);

                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil ditambahkan";
                        }
                        else
                        {
                            // Update Klasifikasi Master 
                            if (data.Kategori == null & data.KeteranganLokasi != null)
                            {

                                string queri = $@"UPDATE ""{skema}"".klasifikasimasterarsip SET kodeklasifikasi = '{data.KodeKlasifikasi}', jenisarsip = '{data.JenisArsip}', keterangan = '{data.Keterangan}', keteranganlokasi = '{data.KeteranganLokasi}', userinput = '{data.UserInput} = Update {DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt")}' WHERE Id = '{id}'";
                                ctx.Database.ExecuteSqlCommand(queri);

                                tc.Commit();
                                tr.Status = true;
                                tr.Pesan = "Data berhasil diupdate";
                            }
                            else if (data.Kategori != null & data.KeteranganLokasi == null)
                            {
                                string queri = $@"UPDATE ""{skema}"".klasifikasimasterarsip SET kodeklasifikasi = '{data.KodeKlasifikasi}', jenisarsip = '{data.JenisArsip}', keterangan = '{data.Keterangan}', kategori = '{data.Kategori}', userinput = '{data.UserInput} = Update {DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt")}' WHERE Id = '{id}'";
                                ctx.Database.ExecuteSqlCommand(queri);

                                tc.Commit();
                                tr.Status = true;
                                tr.Pesan = "Data berhasil diupdate";
                            }

                            else if (data.Kategori != null & data.KeteranganLokasi != null)
                            {
                                string queri = $@"UPDATE ""{skema}"".klasifikasimasterarsip SET kodeklasifikasi = '{data.KodeKlasifikasi}', jenisarsip = '{data.JenisArsip}', keterangan = '{data.Keterangan}', kategori = '{data.Kategori}',keteranganlokasi = '{data.KeteranganLokasi}', userinput = '{data.UserInput} = Update {DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt")}' WHERE Id = '{id}'";
                                ctx.Database.ExecuteSqlCommand(queri);

                                tc.Commit();
                                tr.Status = true;
                                tr.Pesan = "Data berhasil diupdate";

                            }
                            else if (data.Kategori == null & data.KeteranganLokasi == null)
                            {
                                string queri = $@"UPDATE ""{skema}"".klasifikasimasterarsip SET kodeklasifikasi = '{data.KodeKlasifikasi}', jenisarsip = '{data.JenisArsip}', keterangan = '{data.Keterangan}', userinput = '{data.UserInput} = Update {DateTime.Now.ToString("dd:MM:yyyy - HH:mm:ss tt")}' WHERE Id = '{id}'";
                                ctx.Database.ExecuteSqlCommand(queri);
                                tc.Commit();
                                tr.Status = true;
                                tr.Pesan = "Data berhasil diupdate";
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
        public Entities.TransactionResult DeleteKlasifikasiMasterArsip(Models.Entities.KlasifikasiMasterArsip data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {

                        string queri = $@"UPDATE ""{skema}"".klasifikasimasterarsip SET statushapus = '1', tanggalhapus = '{data.TanggalHapus}', userhapus = '{data.UserHapus}' WHERE Id = '{id}'";
                        ctx.Database.ExecuteSqlCommand(queri);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Klasifikasi Master Arsip Berhasil DiHapus";
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
        public Entities.TransactionResult InsertGolonganKlasifikasiMasterArsip(Models.Entities.GolonganKlasifikasi golonganKlasifikasi, string id, string kodeklasifikasi)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            var dtMasaAktif = golonganKlasifikasi.MasaAktif;
            var dtCaptionAktif = golonganKlasifikasi.CaptionAktif;
            int? idedit = golonganKlasifikasi.Id;

            if (dtCaptionAktif == "1 Tahun")
            {
                dtMasaAktif = "365";
            }
            else if (dtCaptionAktif == "2 Tahun")
            {
                dtMasaAktif = "730";
            }
            else if (dtCaptionAktif == "3 Tahun")
            {
                dtMasaAktif = "1095";
            }
            else if (dtCaptionAktif == "4 Tahun")
            {
                dtMasaAktif = "1465";
            }
            else if (dtCaptionAktif == "5 Tahun")
            {
                dtMasaAktif = "1825";
            }
            else
            {
                dtMasaAktif = "0";
            }

            var dtMasaInaktif = golonganKlasifikasi.MasaInaktif;
            var dtCaptionInaktif = golonganKlasifikasi.CaptionInaktif;

            if (dtCaptionInaktif == "1 Tahun")
            {
                dtMasaInaktif = "365";
            }
            else if (dtCaptionInaktif == "2 Tahun")
            {
                dtMasaInaktif = "730";
            }
            else if (dtCaptionInaktif == "3 Tahun")
            {
                dtMasaInaktif = "1095";
            }
            else if (dtCaptionInaktif == "4 Tahun")
            {
                dtMasaInaktif = "1465";
            }
            else if (dtCaptionInaktif == "5 Tahun")
            {
                dtMasaInaktif = "1825";
            }
            else
            {
                dtMasaInaktif = "0";
            }

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (idedit == 0)
                        {
                            // Insert Golongan Klasifikasi
                            string query = $@"INSERT INTO ""{skema}"".golonganklasifikasi (kodeklasifikasi, namajenisarsip, captionaktif, captioninaktif, masaaktif, masainaktif, hasilakhir, statusaktif, statusinaktif, keterangan, statushapus, userinput) 
                            VALUES ('{golonganKlasifikasi.KodeKlasifikasi}', '{golonganKlasifikasi.NamaJenisArsip}', '{golonganKlasifikasi.CaptionAktif}', '{golonganKlasifikasi.CaptionInaktif}', '{dtMasaAktif}', '{dtMasaInaktif}', '{golonganKlasifikasi.HasilAkhir}', '{golonganKlasifikasi.StatusAktif}', '{golonganKlasifikasi.StatusInaktif}', '{golonganKlasifikasi.Keterangan}', '{golonganKlasifikasi.StatusHapus}', '{golonganKlasifikasi.UserInput}')";
                            ctx.Database.ExecuteSqlCommand(query);

                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil ditambahkan";
                        }
                        else
                        {
                            // Update Golongan Klasifikasi
                            string query = $@"UPDATE ""{skema}"".golonganklasifikasi SET namajenisarsip = '{golonganKlasifikasi.NamaJenisArsip}', captionaktif = '{golonganKlasifikasi.CaptionAktif}', captioninaktif = '{golonganKlasifikasi.CaptionInaktif}',  masaaktif = '{dtMasaAktif}', masainaktif = '{dtMasaInaktif}', hasilakhir = '{golonganKlasifikasi.HasilAkhir}', statusaktif = '{golonganKlasifikasi.StatusAktif}', statusinaktif = '{golonganKlasifikasi.StatusInaktif}', keterangan = '{golonganKlasifikasi.Keterangan}', userinput = '{golonganKlasifikasi.UserInput}'  WHERE Id = '{golonganKlasifikasi.Id}'";
                            ctx.Database.ExecuteSqlCommand(query);

                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data Berhasil diupdate";
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
        public Entities.TransactionResult DeleteGolonganKlasifikasiMasterArsip(Models.Entities.GolonganKlasifikasi data, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {

                        string queri = $@"UPDATE ""{skema}"".golonganklasifikasi SET statushapus = '1', tanggalhapus = '{data.TanggalHapus}', userhapus = '{data.UserHapus}' WHERE Id = '{id}'";
                        ctx.Database.ExecuteSqlCommand(queri);
                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Golongan Klasifikasi Master Arsip Berhasil DiHapus";
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
        public List<Models.Entities.GolonganKlasifikasi> GetListMasa(string kodeklasifikasi, int from, int to)
        {
            List<Models.Entities.GolonganKlasifikasi> records = new List<Models.Entities.GolonganKlasifikasi>();
            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();


            string query = $@"select id, kodeklasifikasi, masaaktif, captionaktif, masainaktif, captioninaktif, hasilakhir, namajenisarsip, keterangan, statusretensi, statusaktif, statusinaktif from ""{skema}"".golonganklasifikasi  WHERE statushapus = '0' AND kodeklasifikasi = '{kodeklasifikasi}'";


            using (var ctx = new PostgresDbContext())
            {
                records = ctx.Database.SqlQuery<Models.Entities.GolonganKlasifikasi>(query).ToList<Models.Entities.GolonganKlasifikasi>();
            }

            return records;
        }

        public TransactionResult UploadLampiranArsip(Entities.LampiranArsip data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string namapegawaipengirim, List<SessionLampiranArsip> dataSessionLampiran)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };
            string skema = OtorisasiUser.NamaSkema;

            var arrayListParameters = new ArrayList();

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Insert LAMPIRAN ARSIP
                        foreach (SessionLampiranArsip lampiranArsip in dataSessionLampiran)
                        {
                            if (lampiranArsip.ObjectFile.Length > 0)
                            {
                                data.LampiranArsipId = lampiranArsip.LampiranArsipId;
                                data.NamaFile = lampiranArsip.NamaFile;
                                int idDetail = data.IdMasterArsipDetail;
                                string folderfile = "-";
                                string query = $@"INSERT INTO ""{skema}"".lampiranarsip (lampiranarsipid, idmasterarsipdetail, folderfile, namafile, profileid, kantorid, unitkerjaid, namapegawai, tanggalupload, userinput, statushapus)
                                VALUES('{data.LampiranArsipId}', '{data.IdMasterArsipDetail}','{folderfile}', '{data.NamaFile}', '{myprofileid}', '{kantorid}', '{unitkerjaid}', '{namapegawaipengirim}' ,'{data.TanggalUpload}', '{data.UserInput}', '0')";
                                //arrayListParameters.Add(new NpgsqlParameter("IdMasterArsipDetail", data.IdMasterArsipDetail));
                                //arrayListParameters.Add(new NpgsqlParameter("FolderFile", folderfile));
                                //arrayListParameters.Add(new NpgsqlParameter("NamaFIle", data.NamaFile));
                                //arrayListParameters.Add(new NpgsqlParameter("ProfileId", myprofileid));
                                //arrayListParameters.Add(new NpgsqlParameter("KantorId", kantorid));
                                //arrayListParameters.Add(new NpgsqlParameter("UnitKerjaId", unitkerjaid));
                                //arrayListParameters.Add(new NpgsqlParameter("TanggalUpload", data.TanggalUpload));
                                //arrayListParameters.Add(new NpgsqlParameter("UserInput", myprofileid));
                                //arrayListParameters.Add(new NpgsqlParameter("StatusHapus", "0")); 
                                ctx.Database.ExecuteSqlCommand(query);

                                string sql = $@"UPDATE ""{skema}"".masterarsipdetail SET statusberkas = '1' WHERE id = :idDetail";
                                arrayListParameters.Add(new NpgsqlParameter("idDetail", idDetail));

                                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.NamaFile;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public List<LampiranArsip> GetListLampiranArsip(int? IdMasterArsipDetail, string satkerid)
        {
            var records = new List<LampiranArsip>();

            string skema = OtorisasiUser.NamaSkema;
            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format($@"
                SELECT LA.lampiranarsipid, 
                SUBSTR(LA.namafile, POSITION('|' IN LA.namafile) +1) AS NAMAFILE,
                    LA.kantorid,
                    COUNT(idmasterarsipdetail) OVER() TOTAL
                FROM ""{skema}"".lampiranarsip LA ");

            if (IdMasterArsipDetail == null)
            {
                query += $@"WHERE LA.idmasterarsipdetail = 0";
            }
            else
            {
                query += $@"WHERE LA.idmasterarsipdetail = :param2";
                arrayListParameters.Add(new NpgsqlParameter("param2", IdMasterArsipDetail));
            }

            query += $@" AND statushapus = '0'";
            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<LampiranArsip>(query, parameters).ToList();
            }

            return records;
        }

        public FileArsip getFileLampiran(string lampiranid)
        {
            var records = new FileArsip();

            string query = string.Format(@"
               SELECT
                 LA.lampiranarsipid AS FILESID,
                 LA.folderfile AS PATH,
                 LA.namafile AS PENGENALFILE,
                 LA.kantorid
               FROM {0}.lampiranarsip LA
               WHERE
                 LA.lampiranarsipid = '{1}'  AND statushapus = '0'", OtorisasiUser.NamaSkema, lampiranid);

            using (var ctx = new PostgresDbContext())
            {
                records = ctx.Database.SqlQuery<FileArsip>(query).FirstOrDefault();
            }

            return records;
        }

        public Entities.TransactionResult DeleteLampiranArsip(string id, string namafile, Entities.LampiranArsip data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;
            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = $@"UPDATE ""{skema}"".lampiranarsip SET statushapus = '1',  tanggalhapus = '{data.TanggalHapus}', userhapus = '{data.UserHapus}'  WHERE statushapus = '0' AND lampiranarsipid = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new NpgsqlParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data file arsip berhasil dihapus";
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