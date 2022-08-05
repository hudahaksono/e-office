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
    public class MeetingModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();


        #region Rapat Online

        public List<Models.Entities.RapatOnline> GetRapatOnline(string id, string unitkerjaid, string metadata, string nip, int from, int to)
        {
            List<Models.Entities.RapatOnline> records = new List<Models.Entities.RapatOnline>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY rapatonline.tanggal DESC, rapatonline.judul) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        rapatonline.rapatonlineid, rapatonline.unitkerjaid, rapatonline.judul, rapatonline.tiperapat, rapatonline.longitude, rapatonline.latitude, rapatonline.jumlah_peserta , rapatonline.koderapat, rapatonline.urlmeeting, " +
                "        to_char(rapatonline.tanggal, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') Tanggal, " +
                "        to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalInfo, " +
                "        to_char(rapatonline.tanggal, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') TanggalInfo2, " +
                "        CASE WHEN SYSDATE > rapatonline.tanggal THEN 1 ELSE 0 END AS LewatJatuhTempo, " +
                "        rapatonline.waktu, rapatonline.keterangan, " +
                "        CASE " +
                "            WHEN kantor.bagianwilayah = 'Barat' THEN to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' WIB' " +
                "            WHEN kantor.bagianwilayah = 'Tengah' THEN to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' WITA' " +
                "            WHEN kantor.bagianwilayah = 'Timur' THEN to_char(rapatonline.tanggal, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' WIT' " +
                "            ELSE '-' " +
                "        END AS TanggalInfoLengkap " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".rapatonline " +
                "        JOIN unitkerja ON unitkerja.unitkerjaid = rapatonline.unitkerjaid " +
                "        JOIN kantor ON kantor.kantorid = unitkerja.kantorid " +
                "    WHERE " +
                "        rapatonline.rapatonlineid IS NOT NULL ";

            if (!string.IsNullOrEmpty(id))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                query += " AND rapatonline.rapatonlineid = :Id ";
            }
            if (!string.IsNullOrEmpty(unitkerjaid) && string.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                query += " AND rapatonline.unitkerjaid = :UnitKerjaId ";
            }
            if (!string.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(rapatonline.metadata)) LIKE :Metadata ";
            }
            if (!string.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pesertarapatonline WHERE pesertarapatonline.rapatonlineid = rapatonline.rapatonlineid AND pesertarapatonline.nip = :Nip) ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.RapatOnline>(query, parameters).ToList();
            }

            return records;
        }

        public int JumlahRapatOnlineSaya(string unitkerjaid, string nip)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".rapatonline ";

            query +=
                "WHERE EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pesertarapatonline WHERE pesertarapatonline.rapatonlineid = rapatonline.rapatonlineid AND pesertarapatonline.nip = :Nip) " +
                "      AND SYSDATE < rapatonline.tanggal ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));

            if (!string.IsNullOrEmpty(unitkerjaid) && string.IsNullOrEmpty(nip))
            {
                query += " AND rapatonline.unitkerjaid = :UnitKerjaId ";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public Entities.TransactionResult UbahKodeRapat(Models.Entities.RapatOnline data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    int result = 0;
                    if (!string.IsNullOrEmpty(data.RapatOnlineId))
                    {
                        string query = $@"
                        SELECT count(*) 
                        FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.RAPATONLINE 
                        WHERE KODERAPAT = '{data.KodeRapat}' 
                        ";
                        result = ctx.Database.SqlQuery<int>(query).First();

                        if (result == 0)
                        {
                            query = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.RAPATONLINE SET KODERAPAT = '{data.KodeRapat}' WHERE RAPATONLINEID = '{data.RapatOnlineId}'";
                            ctx.Database.ExecuteSqlCommand(query);
                            tc.Commit();
                            tr.Pesan = "Kode Absen Berhasil diubah";
                        }
                        else
                        {
                            tr.Pesan = "Kode Absen Sudah Terpakai";
                        }
                    }
                }
            }
            return tr;
        }

        public Entities.TransactionResult SimpanRapatOnline(Entities.RapatOnline data)
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
                        metadata += data.Judul + " ";
                        metadata += data.Tanggal + " ";
                        metadata += data.UrlMeeting + " ";
                        metadata += data.Keterangan + " ";
                        if (!string.IsNullOrEmpty(data.Keterangan))
                        {
                            if (data.Keterangan.Length > 1500)
                            {
                                metadata += data.Keterangan.Substring(0, 1500) + " ";
                            }
                            else
                            {
                                metadata += data.Keterangan + " ";
                            } 
                        }
                        metadata = metadata.Trim();
                        #endregion

                        if (!string.IsNullOrEmpty(data.RapatOnlineId))
                        {
                            // Edit mode
                            sql =
                                 "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".rapatonline SET " +
                                 "   judul = :Judul, urlmeeting = :UrlMeeting,  keterangan = :Keterangan,  jumlah_peserta = :Jumlah_Peserta , " +
                                 "   tanggal = TO_DATE(:Tanggal,'DD/MM/YYYY HH24:MI'),  " +
                                 "   tiperapat = :TipeRapat, " +
                                 "   latitude = :Latitude, " +
                                 "   longitude = :Longitude, " +
                                 "   metadata = utl_raw.cast_to_raw(:Metadata) " +
                                 "WHERE rapatonlineid = :Id";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", data.Judul));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UrlMeeting", data.UrlMeeting));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.Keterangan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Jumlah_Peserta", data.Jumlah_Peserta));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal", data.Tanggal));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeRapat", data.TipeRapat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Latitude", data.Latitude));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Longitude", data.Longitude));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", data.RapatOnlineId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            messfooter = " berhasil disimpan.";
                        }
                        else
                        {
                            // Insert Mode
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".rapatonline ( " +
                                "    rapatonlineid, unitkerjaid, judul, tiperapat, jumlah_peserta, urlmeeting, keterangan, tanggal, latitude, longitude, metadata) VALUES " +
                                "( " +
                                "    :Id,:UnitKerjaId,:Judul,:TipeRapat,:Jumlah_Peserta,:UrlMeeting,:Keterangan,TO_DATE(:Tanggal,'DD/MM/YYYY HH24:MI'), :Latitude, :Longitude, " +
                                "    utl_raw.cast_to_raw(:Metadata))";

                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                            data.RapatOnlineId = id;

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", data.UnitKerjaId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", data.Judul));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeRapat", data.TipeRapat));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Jumlah_Peserta", data.Jumlah_Peserta));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UrlMeeting", data.UrlMeeting));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.Keterangan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tanggal", data.Tanggal));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Latitude", data.Latitude));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Longitude", data.Longitude));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", metadata));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.RapatOnlineId;
                        tr.Pesan = "Data Rapat Online " + data.Judul + messfooter;
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

        public Entities.TransactionResult HapusRapatOnline(string id, string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranrapatonline WHERE rapatonlineid = :id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pesertarapatonline WHERE rapatonlineid = :id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".rapatonline WHERE rapatonlineid = :id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data Rapat Online berhasil dihapus";
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

        public Entities.TransactionResult GetKodeRapat(string id) // Arya :: 2020-08-01
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
                        string sql = string.Format("SELECT KODERAPAT FROM {0}.rapatonline WHERE RAPATONLINEID = '{1}'", skema, id);
                        string _kode = ctx.Database.SqlQuery<string>(sql).FirstOrDefault();
                        if (string.IsNullOrEmpty(_kode))
                        {
                            bool check = true;
                            do
                            {
                                _kode = functions.RndCode(6);
                                sql = string.Format("SELECT COUNT(1) FROM {0}.rapatonline WHERE KODERAPAT = '{1}'", skema, _kode);
                                check = (ctx.Database.SqlQuery<int>(sql).FirstOrDefault() > 0);
                            } while (check);

                            sql = string.Format("UPDATE {0}.rapatonline SET KODERAPAT = '{1}' WHERE RAPATONLINEID = '{2}'", skema, _kode, id);
                            ctx.Database.ExecuteSqlCommand(sql);
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

        #endregion


        #region Peserta Rapat

        public List<Models.Entities.PesertaRapatOnline> GetListPesertaRapat(string rapatonlineid)
        {
            List<Models.Entities.PesertaRapatOnline> records = new List<Models.Entities.PesertaRapatOnline>();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                    SELECT
                      ROW_NUMBER() OVER (ORDER BY PRO.NAMAPEGAWAI) AS RNUMBER,
                      COUNT(1) OVER() AS TOTAL,
                      PRO.PESERTARAPATONLINEID,
                      PRO.RAPATONLINEID,
                      PRO.PROFILEID,
                      PRO.NIP,
                      PRO.NAMAJABATAN,
                      PRO.NAMAPEGAWAI,
                      PRO.KETERANGAN AS KETERANGANPESERTA,
	                  DECODE(NVL(PRO.STATUS,'A'),'V',1,0) AS TERKONFIRMASI
                    FROM {0}.PESERTARAPATONLINE PRO
                    WHERE
                      PRO.RAPATONLINEID = :param1 AND
                      NVL(PRO.STATUS,'A') <> 'D' ", System.Web.Mvc.OtorisasiUser.NamaSkema);

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", rapatonlineid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.PesertaRapatOnline>(query, parameters).ToList();
            }

            return records;
        }

        public Models.Entities.TransactionResult SimpanPesertaRapat(Models.Entities.RapatOnline data)
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
                        if (string.IsNullOrEmpty(data.PesertaRapatOnlineId))
                        {
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                            // Insert PESERTARAPATONLINE
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pesertarapatonline ( " +
                                "       pesertarapatonlineid, rapatonlineid, profileid, nip, namajabatan, namapegawai, keterangan) VALUES " +
                                "( " +
                                "       :PesertaRapatOnlineId, :RapatOnlineId, :ProfileId, :Nip, :NamaJabatan, :NamaPegawai, :KeteranganPeserta)";

                            data.PesertaRapatOnlineId = id;

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PesertaRapatOnlineId", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RapatOnlineId", data.RapatOnlineId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileId", data.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.Nip));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaJabatan", data.NamaJabatan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPegawai));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganPeserta", data.KeteranganPeserta));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Update PESERTARAPATONLINE
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pesertarapatonline SET " +
                                "       keterangan = :KeteranganPeserta " +
                                "WHERE pesertarapatonlineid = :PesertaRapatOnlineId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KeteranganPeserta", data.KeteranganPeserta));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PesertaRapatOnlineId", data.PesertaRapatOnlineId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.PesertaRapatOnlineId;
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

        public Entities.TransactionResult HapusPesertaRapat(string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pesertarapatonline SET STATUS = 'D' WHERE pesertarapatonlineid = :PesertaRapatOnlineId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PesertaRapatOnlineId", id));
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

        public List<Entities.AbsensiRapatOnline> GetAbsenData(string rapatid)
        {
            List<Entities.AbsensiRapatOnline> records = new List<Entities.AbsensiRapatOnline>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var aParam = new ArrayList();

            string query = $@"
            SELECT 
                ARO.PEGAWAIID,  
                to_char(ARO.TANGGAL, 'MM/DD/YYYY HH24:MI', 'nls_date_language=INDONESIAN') AS TANGGAL,  
                ARO.LONGITUDE,  
                ARO.LATITUDE,  
                NVL(PPNPN.NAMA, CONCAT( CONCAT(NVL(CONCAT(PEGAWAI.GELARDEPAN, ' '),''), PEGAWAI.NAMA),CONCAT(' ',NVL(PEGAWAI.GELARBELAKANG,'')))) AS NAMA,  
                COALESCE(simpeg.NAMAJABATAN,PEGAWAI.JABATAN, CASE WHEN PPNPN.NAMA IS NOT NULL THEN 'PPNPN' END ,'-' ) AS JABATAN,  
                PEGAWAI.KANTORID,  
                DECODE(PRO.PESERTARAPATONLINEID,NULL,0,1) AS TERDAFTAR,
                DECODE(NVL(PRO.STATUS,'A'),'V',1,0) AS TERKONFIRMASI, 
                SIMPEG.ESELON 
            FROM {skema}.ABSENSIRAPATONLINE  ARO
              LEFT JOIN PEGAWAI ON
                PEGAWAI.PEGAWAIID = ARO.PEGAWAIID
              LEFT JOIN PPNPN ON
                PPNPN.NIK = ARO.PEGAWAIID
              LEFT JOIN SIAP_VW_PEGAWAI simpeg ON
                simpeg.NIPBARU = ARO.PEGAWAIID
              LEFT JOIN {skema}.PESERTARAPATONLINE PRO ON
  	            PRO.RAPATONLINEID =  ARO.RAPATONLINEID AND
  	            PRO.NIP = ARO.PEGAWAIID AND
                NVL(PRO.STATUS,'A') <> 'D'
            WHERE ARO.RAPATONLINEID = :param1 ORDER BY ARO.TANGGAL DESC 
            ";                
            using (var ctx = new BpnDbContext())
            {
                aParam.Clear();
                aParam.Add(new OracleParameter("param1", rapatid));
                records = ctx.Database.SqlQuery<AbsensiRapatOnline> (query, aParam.ToArray()).ToList();
            }

            return records;
        }

        public List<Entities.LokasiKantor> GetLokasiKantorAbsen(string rapatid)
        {
            List<Entities.LokasiKantor> records = new List<Entities.LokasiKantor>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            string query = $@"
            SELECT K.KANTORID, K.NAMA,TO_CHAR(K.LONGITUDE) LONGITUDE, TO_CHAR(K.LATITUDE) LATITUDE , COUNT(AB.PEGAWAIID) AS CT
            FROM KANTOR K LEFT JOIN (
                SELECT UK.KANTORID, AR.PEGAWAIID FROM UNITKERJA UK INNER JOIN JABATAN JB ON JB.UNITKERJAID = UK.UNITKERJAID AND NVL(JB.SEKSIID,'XX') <> 'A800' INNER JOIN JABATANPEGAWAI JP ON JP.PROFILEID = JB.PROFILEID AND NVL(JP.STATUSHAPUS,'0') = '0' AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) INNER JOIN  {skema}.ABSENSIRAPATONLINE AR ON AR.PEGAWAIID = JP.PEGAWAIID AND AR.RAPATONLINEID = '{rapatid}') AB ON AB.KANTORID = K.KANTORID
            WHERE K.LONGITUDE IS NOT NULL AND K.LATITUDE IS NOT NULL  GROUP BY K.KANTORID, K.NAMA,TO_CHAR(K.LONGITUDE), TO_CHAR(K.LATITUDE)
            ";
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Entities.LokasiKantor>(query).ToList();
            }

            return records;
        }

        public List<Entities.LokasiKantor> GetLokasiKantor()
        {
            List<Entities.LokasiKantor> records = new List<Entities.LokasiKantor>();
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;

            string query = $@"
            SELECT KANTORID, NAMA,TO_CHAR(LONGITUDE) LONGITUDE, TO_CHAR(LATITUDE) LATITUDE 
            FROM KANTOR 
            WHERE LONGITUDE IS NOT NULL AND LATITUDE IS NOT NULL 
            ";
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Entities.LokasiKantor>(query).ToList();
            }

            return records;
        }
        #endregion

        #region File Lampiran

        public List<Entities.LampiranRapatOnline> GetLampiranRapatOnline(string rapatonlineid)
        {
            List<Entities.LampiranRapatOnline> records = new List<Entities.LampiranRapatOnline>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT * FROM (
                    SELECT
                        ROW_NUMBER() over (ORDER BY lampiranrapatonline.tipefile, lampiranrapatonline.namafile) RNumber,
                        lampiranrapatonline.lampiranrapatonlineid, lampiranrapatonline.rapatonlineid, lampiranrapatonline.judul JudulLampiran,
                        to_char(lampiranrapatonline.tanggal, 'dd-mm-yyyy') TanggalFile, lampiranrapatonline.tipefile, lampiranrapatonline.urlfile, 
                        lampiranrapatonline.namafile, lampiranrapatonline.ekstensi, lampiranrapatonline.nip NipPengupload, pegawai.nama NamaPengupload, 
                        lampiranrapatonline.objectfile, 
                        COUNT(1) OVER() Total
                    FROM
                        " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".lampiranrapatonline
                        LEFT JOIN pegawai ON pegawai.pegawaiid = lampiranrapatonline.nip
                    WHERE
                        lampiranrapatonline.rapatonlineid = :RapatOnlineId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RapatOnlineId", rapatonlineid));

            query +=
                " )";

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.LampiranRapatOnline>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.LampiranRapatOnline> GetLampiranRapatOnlineForTable(string rapatonlineid)
        {
            List<Entities.LampiranRapatOnline> records = new List<Entities.LampiranRapatOnline>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT * FROM (
                    SELECT
                        ROW_NUMBER() over (ORDER BY lampiranrapatonline.tipefile, lampiranrapatonline.namafile) RNumber,
                        lampiranrapatonline.lampiranrapatonlineid, lampiranrapatonline.rapatonlineid, lampiranrapatonline.judul JudulLampiran,
                        to_char(lampiranrapatonline.tanggal, 'dd-mm-yyyy') TanggalFile, lampiranrapatonline.tipefile, NVL(lampiranrapatonline.urlfile, '-') UrlFile, 
                        lampiranrapatonline.namafile, lampiranrapatonline.ekstensi, lampiranrapatonline.nip NipPengupload, pegawai.nama NamaPengupload, 
                        COUNT(1) OVER() Total
                    FROM
                        " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".lampiranrapatonline
                        LEFT JOIN pegawai ON pegawai.pegawaiid = lampiranrapatonline.nip
                    WHERE
                        lampiranrapatonline.rapatonlineid = :RapatOnlineId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RapatOnlineId", rapatonlineid));

            query +=
                " )";

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.LampiranRapatOnline>(query, parameters).ToList();
            }

            return records;
        }

        public Entities.TransactionResult SimpanLampiranRapatOnline(Entities.RapatOnline data)
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

                        if (string.IsNullOrEmpty(data.LampiranRapatOnlineId))
                        {
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                            data.LampiranRapatOnlineId = id;

                            // Insert LAMPIRANRAPATONLINE
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranrapatonline ( " +
                                "            lampiranrapatonlineid, rapatonlineid, judul, namafile, " +
                                "            tanggal, tipefile, ekstensi, objectfile, nip, urlfile) VALUES " +
                                "( " +
                                "            :LampiranRapatOnlineId, :RapatOnlineId, :JudulLampiran, :NamaFile, " +
                                "            SYSDATE, :TipeFile, :Ekstensi, :ObjectFile, :NipPengupload, :UrlFile)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranRapatOnlineId", data.LampiranRapatOnlineId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RapatOnlineId", data.RapatOnlineId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulLampiran", data.JudulLampiran.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalFile", data.TanggalFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeFile", data.TipeFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ekstensi", data.Ekstensi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengupload", data.NipPengupload));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UrlFile", data.UrlFile));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Update LAMPIRANRAPATONLINE
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranrapatonline SET " +
                                "       tanggal = SYSDATE, " +
                                "       judul = :JudulLampiran, " +
                                "       namafile = :NamaFile, " +
                                "       tipefile = :TipeFile, " +
                                "       ekstensi = :Ekstensi, " +
                                "       objectfile = :ObjectFile, " +
                                "       nip = :NipPengupload, " +
                                "       urlfile = :UrlFile " +
                                "WHERE lampirankembalianid = :LampiranKembalianId";
                            arrayListParameters.Clear();
                            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalFile", data.TanggalFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulLampiran", data.JudulLampiran.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeFile", data.TipeFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ekstensi", data.Ekstensi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengupload", data.NipPengupload));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UrlFile", data.UrlFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranRapatOnlineId", data.LampiranRapatOnlineId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.ReturnValue = data.LampiranRapatOnlineId;
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

        public Entities.TransactionResult HapusLampiranRapatOnline(string lampiranrapatonlineid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranrapatonline WHERE lampiranrapatonlineid = :LampiranRapatOnlineId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranRapatOnlineId", lampiranrapatonlineid));
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

        public byte[] GetFileLampiranById(string lampiranrapatonlineid, out string namafile, out string ekstensi)
        {
            byte[] theFile = null;
            List<Entities.LampiranRapatOnline> data = new List<Entities.LampiranRapatOnline>();

            namafile = "";
            ekstensi = "";

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    lampiranrapatonlineid, rapatonlineid, judul JudulLampiran, to_char(tanggal, 'dd-mm-yyyy') TanggalFile, " +
                "    namafile, tipefile, ekstensi, nip NipPengupload, objectfile " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranrapatonline " +
                "WHERE " +
                "    lampiranrapatonlineid = :LampiranRapatOnlineId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranRapatOnlineId", lampiranrapatonlineid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.LampiranRapatOnline>(query, parameters).ToList();

                if (data.Count > 0)
                {
                    theFile = data[0].ObjectFile;
                    namafile = data[0].NamaFile;
                    ekstensi = data[0].Ekstensi;
                }
            }

            return theFile;
        }

        #endregion

        #region "Absensi Rapat"

        public TransactionResult MeetingAttend(string kod, string uid, string lon, string lat)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var aParam = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Format(@"SELECT RAPATONLINEID FROM {0}.RAPATONLINE WHERE KODERAPAT = :param1", skema);
                        aParam.Clear();
                        aParam.Add(new OracleParameter("param1", kod));
                        string rapatid = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                        if (string.IsNullOrEmpty(rapatid))
                        {
                            tc.Rollback();
                            tr.Status = false;
                            tr.Pesan = "Kode Rapat Daring Tidak Terdaftar";
                        }
                        else
                        {
                            sql = @"
                                SELECT PEGAWAIID
                                FROM PEGAWAI
                                WHERE
	                                USERID = :param1 AND
	                                (VALIDSAMPAI IS NULL OR TO_DATE(TRIM(VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY'))
                                UNION ALL
                                SELECT NIK
                                FROM PPNPN
                                WHERE
	                                USERID = :param2 AND
	                                TANGGALVALIDASI IS NOT NULL AND
	                                NVL(STATUSHAPUS,'0') = '0'";
                            aParam.Clear();
                            aParam.Add(new OracleParameter("param1", uid));
                            aParam.Add(new OracleParameter("param2", uid));
                            string pid = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                            bool bisaabsen = false;
                            try
                            {
                                sql = string.Format(@"SELECT TIPERAPAT FROM {0}.RAPATONLINE WHERE RAPATONLINEID = :param1", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                string tipe = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                                bisaabsen = tipe.Equals("Terbuka");
                            }
                            catch (Exception ex)
                            {
                                bisaabsen = false;
                            }
                            if (!bisaabsen)
                            {
                                sql = string.Format(@"SELECT COUNT(1) FROM {0}.PESERTARAPATONLINE WHERE RAPATONLINEID = :param1 AND NIP = :param2", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                aParam.Add(new OracleParameter("param2", pid));
                                bisaabsen = (ctx.Database.SqlQuery<int>(sql, aParam.ToArray()).FirstOrDefault() > 0);
                            }

                            if (bisaabsen)
                            {
                                sql = string.Format(@"SELECT COUNT(1) FROM {0}.ABSENSIRAPATONLINE WHERE RAPATONLINEID = :param1 AND PEGAWAIID = :param2 AND NVL(STATUSHAPUS,'0') = '0'", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                aParam.Add(new OracleParameter("param2", pid));
                                if (ctx.Database.SqlQuery<int>(sql, aParam.ToArray()).FirstOrDefault() > 0)
                                {
                                    tc.Rollback();
                                    tr.Status = true;
                                    tr.Pesan = "Anda Sudah Pernah Mengisi Presensi Rapat ini sebelumnya";
                                }
                                else
                                {
                                    sql = string.Format(@"SELECT CASE WHEN (TANGGAL-INTERVAL'10'MINUTE) > SYSDATE THEN 'Rapat Online Belum Mulai' ELSE '' END FROM {0}.RAPATONLINE WHERE KODERAPAT = :param1", skema);
                                    aParam.Clear();
                                    aParam.Add(new OracleParameter("param1", kod));
                                    string pesanerror = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(pesanerror))
                                    {
                                        tc.Rollback();
                                        tr.Status = false;
                                        tr.Pesan = pesanerror;
                                    }
                                    else
                                    {
                                        sql = string.Format(@"
                                        INSERT INTO {0}.ABSENSIRAPATONLINE (ABSENSIID, RAPATONLINEID, PEGAWAIID, TANGGAL, LONGITUDE, LATITUDE)
                                        VALUES (RAWTOHEX(SYS_GUID()), :param1, :param2, SYSDATE, :param3, :param4)
                                        ", skema);
                                        aParam.Clear();
                                        aParam.Add(new OracleParameter("param1", rapatid));
                                        aParam.Add(new OracleParameter("param2", pid));
                                        aParam.Add(new OracleParameter("param3", lon));
                                        aParam.Add(new OracleParameter("param4", lat));
                                        ctx.Database.ExecuteSqlCommand(sql,aParam.ToArray());

                                        tc.Commit();
                                        tr.Status = true;

                                        sql = string.Format(@"
                                        SELECT JUDUL FROM {0}.RAPATONLINE WHERE RAPATONLINEID = :param1", skema);
                                        aParam.Clear();
                                        aParam.Add(new OracleParameter("param1", rapatid));
                                        string judul = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                                        tr.Pesan = string.Concat("Selamat Datang di ", judul, "");
                                    }
                                }
                            }
                            else
                            {
                                tc.Rollback();
                                tr.Status = false;
                                tr.Pesan = "Anda tidak terdaftar didalam list peserta";
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

        public RapatOnline GetRapatOnlineDetail(string id, string kode)
        {
            var records = new RapatOnline();

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

        public List<Entities.AbsensiRapatOnline> GetListAbsensi(string id, int from, int to)
        {
            var records = new List<Entities.AbsensiRapatOnline>();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY ARO.TANGGAL DESC) AS RNUMBER, 
                        COUNT(1) OVER() TOTAL,
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
	                    ARO.TANGGAL", System.Web.Mvc.OtorisasiUser.NamaSkema);
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", id));

            using (var ctx = new BpnDbContext())
            {
                if (from + to > 0)
                {
                    query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                    arrayListParameters.Add(new OracleParameter("pStart", from));
                    arrayListParameters.Add(new OracleParameter("pEnd", to));
                }
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.AbsensiRapatOnline>(query, parameters).ToList();
            }

            return records;
        }

        public TransactionResult PendaftaranPeserta(string kode, string pegawaiid, string namapegawai, string jabatanid, string namajabatan)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var aParam = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Format(@"SELECT RAPATONLINEID FROM {0}.RAPATONLINE WHERE KODERAPAT = :param1", skema);
                        aParam.Clear();
                        aParam.Add(new OracleParameter("param1", kode));
                        string rapatid = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                        if (string.IsNullOrEmpty(rapatid))
                        {
                            tc.Rollback();
                            tr.Status = false;
                            tr.Pesan = "Kode Rapat Daring Tidak Terdaftar";
                        }
                        else
                        {
                            bool bisaabsen = false;
                            try
                            {
                                sql = string.Format(@"SELECT TIPERAPAT FROM {0}.RAPATONLINE WHERE RAPATONLINEID = :param1", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                string tipe = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                                bisaabsen = tipe.Equals("Terbuka");
                            }
                            catch (Exception ex)
                            {
                                bisaabsen = false;
                            }

                            if (!bisaabsen)
                            {
                                sql = string.Format(@"SELECT COUNT(1) FROM {0}.PESERTARAPATONLINE WHERE RAPATONLINEID = :param1 AND NIP = :param2", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                aParam.Add(new OracleParameter("param2", pegawaiid));
                                if(ctx.Database.SqlQuery<int>(sql, aParam.ToArray()).FirstOrDefault() > 0)
                                {
                                    tc.Rollback();
                                    tr.Status = true;
                                    tr.Pesan = "Anda Sudah Terdaftar Pada Kegiatan ini";
                                }
                                else
                                {
                                    tc.Rollback();
                                    tr.Status = false;
                                    tr.Pesan = "Kegiatan ini tidak bisa melakukan pendaftaran mandiri";
                                }
                            }
                            else
                            {
                                sql = string.Format(@"SELECT COUNT(1) FROM {0}.PESERTARAPATONLINE WHERE RAPATONLINEID = :param1 AND NIP = :param2", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                aParam.Add(new OracleParameter("param2", pegawaiid));
                                if (ctx.Database.SqlQuery<int>(sql, aParam.ToArray()).FirstOrDefault() > 0)
                                {
                                    tc.Rollback();
                                    tr.Status = true;
                                    tr.Pesan = "Anda Sudah Terdaftar Pada Kegiatan ini";
                                }
                                else
                                {
                                    sql = string.Format(@"
                                        INSERT INTO {0}.PESERTARAPATONLINE (PESERTARAPATONLINEID, RAPATONLINEID, PROFILEID, NIP, NAMAJABATAN, NAMAPEGAWAI, KETERANGAN)
                                        VALUES (RAWTOHEX(SYS_GUID()), :param1, :param2, :param3, :param4, :param5, :param6)
                                        ", skema);
                                    aParam.Clear();
                                    aParam.Add(new OracleParameter("param1", rapatid));
                                    aParam.Add(new OracleParameter("param2", jabatanid));
                                    aParam.Add(new OracleParameter("param3", pegawaiid));
                                    aParam.Add(new OracleParameter("param4", namajabatan));
                                    aParam.Add(new OracleParameter("param5", namapegawai));
                                    aParam.Add(new OracleParameter("param6", ""));
                                    ctx.Database.ExecuteSqlCommand(sql, aParam.ToArray());

                                    tc.Commit();
                                    tr.Status = true;

                                    sql = string.Format(@"
                                        SELECT JUDUL FROM {0}.RAPATONLINE WHERE RAPATONLINEID = :param1", skema);
                                    aParam.Clear();
                                    aParam.Add(new OracleParameter("param1", rapatid));
                                    string judul = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();
                                    tr.Pesan = string.Concat("Selamat Datang di ", judul, "");
                                }
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
        public TransactionResult ValidasiPeserta(string rapatid, string pid, string uid, string lon, string lat)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
            var aParam = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Empty;
                        if (string.IsNullOrEmpty(rapatid))
                        {
                            tc.Rollback();
                            tr.Status = false;
                            tr.Pesan = "Data Rapat Daring Tidak Terdaftar";
                        }
                        else
                        {
                            string status = "A";
                            sql = string.Format(@"SELECT NVL(STATUS,'A') FROM {0}.PESERTARAPATONLINE WHERE RAPATONLINEID = :param1 AND NIP = :param2 AND NVL(STATUS,'A') <> 'D'", skema);
                            aParam.Clear();
                            aParam.Add(new OracleParameter("param1", rapatid));
                            aParam.Add(new OracleParameter("param2", pid));
                            status = ctx.Database.SqlQuery<string>(sql, aParam.ToArray()).FirstOrDefault();

                            if (status.Equals("A"))
                            {
                                sql = string.Format(@"
                                        UPDATE {0}.PESERTARAPATONLINE SET
                                            STATUS = 'V'
                                        WHERE RAPATONLINEID = :param1 AND NIP = :param2 AND NVL(STATUS,'A') = 'A'
                                        ", skema);
                                aParam.Clear();
                                aParam.Add(new OracleParameter("param1", rapatid));
                                aParam.Add(new OracleParameter("param2", pid));
                                ctx.Database.ExecuteSqlCommand(sql, aParam.ToArray());
                                tr.Pesan = "Validasi Peserta Berhasil";
                                tc.Commit();
                            }
                            else if(status.Equals("V"))
                            {
                                tc.Rollback();
                                tr.Status = false;
                                tr.Pesan = "Peserta telah di validasi sebelumnya";
                            }
                            else
                            {
                                tc.Rollback();
                                tr.Status = false;
                                tr.Pesan = "Peserta tidak terdaftar";
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
        #endregion

        public List<RekapPresensiRapatUnitKerja> GetRekapPresensiRapat(string id)
        {
            var records = new List<RekapPresensiRapatUnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT
	                NVL(KT1.TIPEKANTORID,KT.TIPEKANTORID) AS TIPE,
	                UK.UNITKERJAID,
	                NVL(KT1.NAMA,KT.NAMA) AS INDUK,
	                KT.NAMA AS KANTOR,
	                UK.NAMAUNITKERJA AS SATKER,
	                COUNT(UK.UNITKERJAID) AS JUMLAH
                FROM {0}.RAPATONLINE RO
                INNER JOIN {0}.ABSENSIRAPATONLINE ARO ON
	                ARO.RAPATONLINEID = RO.RAPATONLINEID AND
	                NVL(ARO.STATUSHAPUS,'0') = '0' 
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PEGAWAIID = ARO.PEGAWAIID AND
	                NVL(JP.STATUSHAPUS,'0') = '0' AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(STATUSPLT,0) = 0
                INNER JOIN JABATAN JB ON
	                JB.PROFILEID = JP.PROFILEID AND
	                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(JB.SEKSIID,'XXX') <> 'A800' AND
	                JB.PROFILEIDTU IS NOT NULL
                INNER JOIN UNITKERJA UK ON
	                UK.UNITKERJAID = JB.UNITKERJAID
                INNER JOIN KANTOR KT ON
	                KT.KANTORID = UK.KANTORID
                LEFT JOIN KANTOR KT1 ON
	                KT1.KANTORID = KT.INDUK AND
	                KT1.TIPEKANTORID = 2
                WHERE
                  RO.RAPATONLINEID = :param1
                GROUP BY
	                NVL(KT1.TIPEKANTORID,KT.TIPEKANTORID),
	                UK.UNITKERJAID,
	                NVL(KT1.NAMA,KT.NAMA),
	                KT.NAMA,
	                UK.NAMAUNITKERJA
                ORDER BY 
	                NVL(KT1.TIPEKANTORID,KT.TIPEKANTORID),
	                UK.UNITKERJAID", System.Web.Mvc.OtorisasiUser.NamaSkema);
            arrayListParameters.Add(new OracleParameter("param1", id));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<RekapPresensiRapatUnitKerja>(query, parameters).ToList();
            }

            return records;
        }

        public List<RekapPresensiRapatUnitKerja> GetRekapPresensiRapatInduk(string id, int tipe = 0)
        {
            var records = new List<RekapPresensiRapatUnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT
	                RST.INDUK,
                    SUM(RST.JUMLAH) AS JUMLAH
                FROM (
                SELECT
	                KT.TIPEKANTORID AS TIPE,
	                NVL(KT1.NAMA,KT.NAMA) AS INDUK,
	                KT.NAMA AS KANTOR,
	                UK.NAMAUNITKERJA AS SATKER,
	                COUNT(UK.UNITKERJAID) AS JUMLAH
                FROM {0}.RAPATONLINE RO
                INNER JOIN {0}.ABSENSIRAPATONLINE ARO ON
	                ARO.RAPATONLINEID = RO.RAPATONLINEID
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PEGAWAIID = ARO.PEGAWAIID AND
	                NVL(JP.STATUSHAPUS,'0') = '0' AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(STATUSPLT,0) = 0
                INNER JOIN JABATAN JB ON
	                JB.PROFILEID = JP.PROFILEID AND
	                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(JB.SEKSIID,'XXX') <> 'A800' AND
	                JB.PROFILEIDTU IS NOT NULL
                INNER JOIN UNITKERJA UK ON
	                UK.UNITKERJAID = JB.UNITKERJAID
                INNER JOIN KANTOR KT ON
	                KT.KANTORID = UK.KANTORID
                LEFT JOIN KANTOR KT1 ON
	                KT1.KANTORID = KT.INDUK AND
	                KT1.TIPEKANTORID = 2
                WHERE
                  RO.RAPATONLINEID = :param1 {1}
                GROUP BY
	                KT.TIPEKANTORID,
	                NVL(KT1.NAMA,KT.NAMA),
	                KT.NAMA,
	                UK.NAMAUNITKERJA
                ORDER BY KT.TIPEKANTORID
                )RST GROUP BY RST.INDUK ORDER BY RST.INDUK", System.Web.Mvc.OtorisasiUser.NamaSkema, tipe.Equals(1)? " AND KT.TIPEKANTORID = 1 ": tipe.Equals(2)? " AND KT.TIPEKANTORID > 1 ": "");
            arrayListParameters.Add(new OracleParameter("param1", id));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<RekapPresensiRapatUnitKerja>(query, parameters).ToList();
            }

            return records;
        }
    }
}