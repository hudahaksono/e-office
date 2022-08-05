using Surat.Codes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class KonterModel
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
                        string result = ctx.Database.SqlQuery<string>("SELECT to_char(sysdate,'DD/MM/YYYY') FROM DUAL").FirstOrDefault<string>();
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

        public string GetNomorKonterSurat(string kantorid, string unitkerjaid, string profileidtu, string tipe)
        {
            string result = "";

            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);

            decimal nilaikonter = GetNilaiKonterSurat(kantorid, unitkerjaid, profileidtu, tipekantorid, tipe);

            // Get Bulan
            int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
            //string strBulan = Convert.ToString(bulan).PadLeft(2, '0');
            string strBulan = Functions.NomorRomawi(bulan);

            string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
            string kodesurat = "";

            if (tipe == "Agenda")
            {
                if (tipekantorid == 1)
                {
                    // Pusat
                    kodesurat = "AG-";
                }
                else if (tipekantorid == 2)
                {
                    // Kanwil
                    kodesurat = "AG-";
                }
                else if (tipekantorid == 3 || tipekantorid == 4)
                {
                    // Kantah/Perwakilan
                    kodesurat = "AG-";
                }
            }

            result = Convert.ToString(nilaikonter) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());

            return result;
        }

        public string GetNomorAgendaSuratAndUpdate(string kantorid, string unitkerjaid, string profileidtu, string suratid)
        {
            string result = "";

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
                        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();
                        int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                        string satkerid = kantorid;
                        if (tipekantorid == 1)
                        {
                            //satkerid = profileidtu;
                            satkerid = unitkerjaid;
                        }


                        //// Cek duplikasi
                        //sql =
                        //    "SELECT count(*) " +
                        //    "FROM agendasurat " +
                        //    "WHERE suratid = :SuratId AND kantorid = :SatkerId";
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        //parameters = arrayListParameters.OfType<object>().ToArray();
                        //int jumlahrecord = ctx.Database.SqlQuery<int>(sql, parameters).First();
                        //if (jumlahrecord > 0)
                        //{
                        //    tc.Dispose();
                        //    ctx.Dispose();
                        //    return result;
                        //}


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

                        result = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());


                        //// Insert AGENDASURAT
                        //sql =
                        //    @"INSERT INTO agendasurat (
                        //            agendasuratid, suratid, nomoragenda, kantorid) VALUES 
                        //            (
                        //                SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
                        //sql = sWhitespace.Replace(sql, " ");
                        //arrayListParameters.Clear();
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", suratid));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", result));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                        //parameters = arrayListParameters.OfType<object>().ToArray();
                        //ctx.Database.ExecuteSqlCommand(sql, parameters);



//                        decimal nilaikonter = GetNilaiKonterSurat(kantorid, profileidtu, tipekantorid, "Agenda");

//                        // Get Bulan
//                        int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
//                        //string strBulan = Convert.ToString(bulan).PadLeft(2, '0');
//                        string strBulan = Functions.NomorRomawi(bulan);

//                        string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
//                        string kodesurat = "";

//                        if (tipekantorid == 1)
//                        {
//                            // Pusat
//                            kodesurat = "AG-";
//                        }
//                        else if (tipekantorid == 2)
//                        {
//                            // Kanwil
//                            kodesurat = "AG-";
//                        }
//                        else if (tipekantorid == 3 || tipekantorid == 4)
//                        {
//                            // Kantah/Perwakilan
//                            kodesurat = "AG-";
//                        }

//                        //result += Convert.ToString(nilaikonter) + "/" + strBulan + "/" + Convert.ToString(GetServerYear());
//                        result = Convert.ToString(nilaikonter) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());


//                        // Update KONTERSURAT
//                        sql = @"UPDATE kontersurat SET nilaikonter = :Nilai WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = 'Agenda'";
//                        arrayListParameters.Clear();
//                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nilai", nilaikonter));
//                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
//                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
//                        parameters = arrayListParameters.OfType<object>().ToArray();
//                        ctx.Database.ExecuteSqlCommand(sql, parameters);


//                        // Insert AGENDASURAT
//                        sql =
//                            @"INSERT INTO agendasurat (
//                                agendasuratid, suratid, nomoragenda, kantorid) VALUES 
//                                (
//                                    SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
//                        sql = sWhitespace.Replace(sql, " ");
//                        arrayListParameters.Clear();
//                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", suratid));
//                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", result));
//                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
//                        parameters = arrayListParameters.OfType<object>().ToArray();
//                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update SURAT field NOMORAGENDA
                        sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".surat SET nomoragenda = :NomorAgenda WHERE nomoragenda IS NULL AND suratid = :SuratId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorAgenda", result));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
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

            return result;
        }

        public decimal GetNilaiKonterSurat(string kantorid, string unitkerjaid, string profileidtu, int tipekantorid, string tipe)
        {
            decimal result = 1;

            string satkerid = kantorid;
            if (tipekantorid == 1)
            {
                //satkerid = profileidtu;
                satkerid = unitkerjaid;
            }

            string namaskema = System.Web.Mvc.OtorisasiUser.NamaSkema + ".";

            using (var ctx = new BpnDbContext())
            {
                int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", tipe));

                string query = "select count(*) from " + namaskema + "kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();

                if (jumlahrecord > 0)
                {
                    query = "select NVL(nilaikonter+1, 1) from " + namaskema + "kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";

                    arrayListParameters.Clear();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", tipe));
                    parameters = arrayListParameters.OfType<object>().ToArray();

                    result = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();
                }
                else
                {
                    using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            // Bila tidak ada, Insert KONTERSURAT
                            query =
                                "INSERT INTO " + namaskema + "kontersurat ( " +
                                "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                " ( " +
                                "    SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", tipe));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(query, parameters);

                            tc.Commit();
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.ToString();
                            tc.Rollback();
                        }
                        finally
                        {
                            tc.Dispose();
                            ctx.Dispose();
                        }
                    }
                }
            }

            return result;
        }

        public string NomorAgendaSurat(string suratid, string satkerid)
        {
            string result = "";

            string query =
                "SELECT nomoragenda " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".agendasurat " +
                "WHERE suratid = :SuratId AND kantorid = :SatkerId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratId", suratid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SatkerId", satkerid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }
    }
}