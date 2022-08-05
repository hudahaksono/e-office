using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Surat.Codes;

namespace Surat.Models
{
    public class PengaduanModel
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

        public Entities.NIKResult SearchPemohonFromKemendagri(string nik)
        {
            Entities.NIKResult _infoInputPemohon = new Entities.NIKResult();

            ApiServices _apiservice = new ApiServices();

            _infoInputPemohon = _apiservice.Call_NIK(nik);

            return _infoInputPemohon;
        }

        public List<Entities.LokasiKejadian> GetLokasiKejadian()
        {
            var list = new List<Entities.LokasiKejadian>();

            string query =
                "select " +
                "    'Provinsi ' || nama AS value, 'Provinsi ' || nama AS data " +
                "from wilayah where validsampai is null and tipewilayahid = 1 " + 
                "union " +
                "select " +
                "    'Kabupaten ' || nama AS value, 'Kabupaten ' || nama AS data " +
                "from wilayah where validsampai is null and tipewilayahid = 2 " +
                "union " +
                "select " +
                "    'Kotamadya ' || nama AS value, 'Kotamadya ' || nama AS data " +
                "from wilayah where validsampai is null and tipewilayahid = 3 " +
                "union " +
                "select " +
                "    'Kota Administratif ' || nama AS value, 'Kota Administratif ' || nama AS data " +
                "from wilayah where validsampai is null and tipewilayahid = 4 " +
                "union " +
                "select " +
                "    'Kecamatan ' || nama AS value, 'Kecamatan ' || nama AS data " +
                "from wilayah where validsampai is null and tipewilayahid = 5 " +
                //"union " +
                //"select " +
                //"    'Desa ' || nama AS value, 'Desa ' || nama AS data " +
                //"from wilayah where validsampai is null and tipewilayahid = 6 " +
                //"union " +
                //"select " +
                //"    'Kelurahan ' || nama AS value, 'Kelurahan ' || nama AS data " +
                //"from wilayah where validsampai is null and tipewilayahid = 7 " +
                "";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Entities.LokasiKejadian>(query).ToList();
            }

            return list;
        }

        public List<Entities.JenisPekerjaan> GetJenisPekerjaan()
        {
            var list = new List<Entities.JenisPekerjaan>();

            string query =
                "select " +
                "    pekerjaan AS value, pekerjaan AS data " +
                "from " + System.Web.Mvc.OtorisasiUser.NamaSkemaMaster + ".jenispekerjaan where aktif = 1 order by pekerjaan";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Entities.JenisPekerjaan>(query).ToList();
            }

            return list;
        }

        public List<Entities.KategoriPengaduan> GetKategoriPengaduan()
        {
            var list = new List<Models.Entities.KategoriPengaduan>();

            string query =
                "SELECT " +
                "   kategoripengaduan.kategori NamaKategoriPengaduan " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkemaMaster + ".kategoripengaduan " +
                "WHERE kategoripengaduan.aktif = 1 ORDER BY kategoripengaduan.kategori";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Entities.KategoriPengaduan>(query).ToList();
            }

            return list;
        }


        #region Lampiran Pengaduan

        public List<Entities.LampiranPengaduan> GetListLampiranPengaduan(string aduanid, string lampiranpengaduanid)
        {
            List<Entities.LampiranPengaduan> records = new List<Entities.LampiranPengaduan>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT " +
                 "     ROW_NUMBER() over (ORDER BY lampiranpengaduan.tanggal, lampiranpengaduan.namafile) RNUMBER, " +
                 "     lampiranpengaduan.lampiranpengaduanid, lampiranpengaduan.aduanid, lampiranpengaduan.path FolderFile, lampiranpengaduan.namafile, " +
                 "     lampiranpengaduan.keterangan, lampiranpengaduan.profileid, lampiranpengaduan.nip, " +
                 "     NVL(pegawai.nama, ppnpn.nama) AS NamaPegawai, " +
                 "     to_char(lampiranpengaduan.tanggal, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') Tanggal, " +
                 "     lampiranpengaduan.unitkerjaid, " +
                 "     COUNT(1) OVER() TOTAL " +
                 " FROM " +
                 "     " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranpengaduan " +
                 "     LEFT JOIN pegawai ON pegawai.pegawaiid = lampiranpengaduan.nip " +
                 "     LEFT JOIN ppnpn ON ppnpn.nik = lampiranpengaduan.nip " +
                 " WHERE " +
                 "     lampiranpengaduan.aduanid = :AduanId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));

            if (!String.IsNullOrEmpty(lampiranpengaduanid))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranPengaduanId", lampiranpengaduanid));
                query += " AND lampiranpengaduan.lampiranpengaduanid = :LampiranPengaduanId ";
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.LampiranPengaduan>(query, parameters).ToList();
            }

            return records;
        }

        public Entities.TransactionResult HapusLampiranPengaduanById(string aduanid, string id)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranpengaduan WHERE lampiranpengaduanid = :Id";
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

        public Models.Entities.TransactionResult InsertLampiranPengaduan(Models.Entities.Pengaduan data, string unitkerjaid)
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
                             "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranpengaduan ( " +
                             "       lampiranpengaduanid, aduanid, path, namafile, profileid, unitkerjaid, keterangan, nip) VALUES " +
                             "( " +
                             "       :LampiranPengaduanId,:AduanId,:FolderFile,:NamaFile,:ProfileIdPengirim,:UnitKerjaId,:Keterangan,:Nip)";

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranPengaduanId", data.LampiranPengaduanId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", "-"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.NamaFile));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", data.Nip));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Jumlah Lampiran di tabel ADUAN
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan SET " +
                             "   jumlahlampiran = (SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranpengaduan WHERE aduanid = :AduanId1) " +
                             " WHERE aduanid = :AduanId2";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId1", data.AduanId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId2", data.AduanId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = data.LampiranPengaduanId;
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

        public string GetJumlahLampiranPengaduan(string aduanid)
        {
            string result = "0";

            string query =
                "SELECT count(*) " +
                "FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranpengaduan " +
                "WHERE aduanid = :AduanId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));

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

        #endregion



        public Models.Entities.AduanInbox GetAduanInboxId(string aduaninboxid)
        {
            Models.Entities.AduanInbox data = new Models.Entities.AduanInbox();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    aduaninbox.aduaninboxid, aduaninbox.aduanid, aduaninbox.nip, aduaninbox.profilepengirim, aduaninbox.profilepenerima, " +
                "    to_char(aduaninbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "    to_char(aduaninbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "    to_char(aduaninbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "    NVL(to_char(aduaninbox.tanggalbuka, 'dd/mm/yyyy HH24:MI', 'nls_date_language=INDONESIAN'), '...') InfoTanggalBuka, " +
                "    aduaninbox.tindaklanjut, aduaninbox.namapegawai, aduaninbox.namapengirim, aduaninbox.keterangan, " +
                "    aduaninbox.statusterkirim, decode(aduaninbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "    aduaninbox.redaksi, aduaninbox.statusurgent, aduaninbox.perintahdisposisi, " +
                "    profiledari.nama AS NamaProfilePengirim, aduaninbox.namapegawai AS NamaPenerima, " +
                "    profiledari.nama AS NamaProfilePengirim, profiletujuan.nama AS NamaProfilePenerima, " +
                //"    NVL(profilepengirim.nama,'-') AS NamaProfilePengirim, NVL(profilepenerima.nama, '-') AS NamaProfilePenerima, " +
                "    to_char(aduan.tanggaladuan, 'dd/mm/yyyy') TanggalAduan, " +
                "    trim(to_char(aduan.tanggaladuan, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN')) InfoTanggalAduan, " +
                "    to_char(aduan.tanggalkejadian, 'dd/mm/yyyy') TanggalKejadian, " +
                "    to_char(aduan.tanggalkejadian, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN') InfoTanggalKejadian, " +
                "    to_char(aduan.tanggaltarget, 'dd/mm/yyyy') TanggalTarget, " +
                "    to_char(aduan.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "    to_char(aduan.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "    to_char(aduan.tanggalselesai, 'dd-mm-yyyy') TanggalSelesai, " +
                "    to_char(aduan.tanggalselesai, 'fmDD Month YYYY') InfoTanggalSelesai, " +
                "    aduan.juduladuan, aduan.lokasikejadian, aduan.tujuanpengaduan, aduan.nomorlaporan, aduan.statuslaporan, " +
                "    aduan.kategori, aduan.lapormelalui, aduan.sumber, aduan.statusmoderasi, aduan.uraian, aduan.hasil, " +
                "    aduan.penyebab, aduan.tanggapan, aduan.namapengadu, aduan.alamatpengadu, aduan.nikpengadu, " +
                "    aduan.teleponpengadu, aduan.emailpengadu, aduan.kategoripengadu, aduan.pekerjaanpengadu, " +
                "    CASE " +
                "        WHEN aduan.tanggalselesai IS NOT NULL THEN 'Sudah Diarsipkan/Selesai' " +
                "        WHEN aduaninbox.urutan = 1 THEN 'Pembuat Pengaduan' " +
                "        WHEN aduaninbox.statusterkirim = 1 THEN 'Telah Terkirim' " +
                "        WHEN aduaninbox.tanggalbuka IS NULL THEN 'Belum Dibuka' " +
                "        ELSE 'Dalam Proses' " +
                "    END AS KetStatusTerkirim " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox " +
                "    JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan ON aduan.aduanid = aduaninbox.aduanid " +
                "    JOIN jabatan profiletujuan ON profiletujuan.profileid = aduaninbox.profilepenerima " +
                "    LEFT JOIN jabatan profiledari ON profiledari.profileid = aduaninbox.profilepengirim " +
                "WHERE " +
                "    aduaninbox.aduaninboxid = :AduanInboxId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Models.Entities.AduanInbox>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public List<Entities.AduanInbox> GetPengaduan(string unitkerjaid, string nip, string metadata, int from, int to)
        {
            List<Entities.AduanInbox> records = new List<Entities.AduanInbox>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY aduaninbox.statusurgent DESC, aduan.tanggaladuan, aduaninbox.tanggalkirim DESC) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        aduaninbox.aduaninboxid, aduaninbox.aduanid, aduaninbox.nip, aduaninbox.profilepengirim, aduaninbox.profilepenerima, " +
                "        to_char(aduaninbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(aduaninbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(aduaninbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        NVL(to_char(aduaninbox.tanggalbuka, 'dd/mm/yyyy HH24:MI', 'nls_date_language=INDONESIAN'), '...') InfoTanggalBuka, " +
                "        aduaninbox.tindaklanjut, aduaninbox.namapegawai, aduaninbox.namapengirim, aduaninbox.keterangan, " +
                "        aduaninbox.statusterkirim, decode(aduaninbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        aduaninbox.redaksi, aduaninbox.statusurgent, aduaninbox.perintahdisposisi, " +
                "        aduaninbox.namapegawai AS NamaPenerima, " +
                "        profiledari.nama AS NamaProfilePengirim, profiletujuan.nama AS NamaProfilePenerima, " +
                //"        NVL(profilepengirim.nama,'-') AS NamaProfilePengirim, NVL(profilepenerima.nama, '-') AS NamaProfilePenerima, " +
                "        to_char(aduan.tanggaladuan, 'dd/mm/yyyy') TanggalAduan, " +
                "        trim(to_char(aduan.tanggaladuan, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN')) InfoTanggalAduan, " +
                "        to_char(aduan.tanggalkejadian, 'dd/mm/yyyy') TanggalKejadian, " +
                "        to_char(aduan.tanggalkejadian, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN') InfoTanggalKejadian, " +
                "        to_char(aduan.tanggaltarget, 'dd/mm/yyyy') TanggalTarget, " +
                "        to_char(aduan.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(aduan.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "        to_char(aduan.tanggalselesai, 'dd-mm-yyyy') TanggalSelesai, " +
                "        to_char(aduan.tanggalselesai, 'fmDD Month YYYY') InfoTanggalSelesai, " +
                "        aduan.juduladuan, aduan.lokasikejadian, aduan.tujuanpengaduan, aduan.nomorlaporan, aduan.statuslaporan, " +
                "        aduan.kategori, aduan.lapormelalui, aduan.sumber, aduan.statusmoderasi, aduan.uraian, aduan.hasil, " +
                "        aduan.penyebab, aduan.tanggapan, aduan.namapengadu, aduan.alamatpengadu, aduan.nikpengadu, " +
                "        aduan.teleponpengadu, aduan.emailpengadu, aduan.kategoripengadu, aduan.pekerjaanpengadu, " +
                "        CASE " +
                "            WHEN aduan.tanggalselesai IS NOT NULL THEN 'Sudah Diarsipkan/Selesai' " +
                "            WHEN aduaninbox.urutan = 1 THEN 'Pembuat Pengaduan' " +
                "            WHEN aduaninbox.statusterkirim = 1 THEN 'Telah Terkirim' " +
                "            WHEN aduaninbox.tanggalbuka IS NULL THEN 'Belum Dibuka' " +
                "            ELSE 'Dalam Proses' " +
                "        END AS KetStatusTerkirim " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan ON aduan.aduanid = aduaninbox.aduanid " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = aduaninbox.profilepenerima " +
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = aduaninbox.profilepengirim " +
                "    WHERE " +
                "        aduaninbox.statusterkirim = 0 AND aduan.statuspengaduan = 1 " +
                "        AND (aduaninbox.statushapus IS NULL OR aduaninbox.statushapus = '0') ";

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                query += " AND (aduaninbox.nip IS NULL OR aduaninbox.nip = :Nip) ";
            }
            if (!string.IsNullOrEmpty(unitkerjaid) && string.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                query += " AND aduan.unitkerjaid = :UnitKerjaId ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(aduan.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.AduanInbox>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.AduanInbox> GetPengaduanHistori(string aduanid, string unitkerjaid)
        {
            List<Entities.AduanInbox> records = new List<Entities.AduanInbox>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY aduaninbox.urutan) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        aduaninbox.aduaninboxid, aduaninbox.aduanid, aduaninbox.nip, aduaninbox.profilepengirim, aduaninbox.profilepenerima, " +
                "        to_char(aduaninbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(aduaninbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(aduaninbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        NVL(to_char(aduaninbox.tanggalbuka, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN'), '...') InfoTanggalBuka, " +
                "        aduaninbox.tindaklanjut, aduaninbox.namapegawai, aduaninbox.namapengirim, aduaninbox.keterangan, " +
                "        aduaninbox.statusterkirim, decode(aduaninbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        aduaninbox.redaksi, aduaninbox.statusurgent, aduaninbox.perintahdisposisi, " +
                "        aduaninbox.namapegawai AS NamaPenerima, " +
                "        NVL(profilepengirim.nama,'-') AS NamaProfilePengirim, NVL(profilepenerima.nama, '-') AS NamaProfilePenerima, " +
                "        to_char(aduan.tanggaladuan, 'dd/mm/yyyy') TanggalAduan, " +
                "        trim(to_char(aduan.tanggaladuan, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN')) InfoTanggalAduan, " +
                "        to_char(aduan.tanggalkejadian, 'dd/mm/yyyy') TanggalKejadian, " +
                "        to_char(aduan.tanggalkejadian, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN') InfoTanggalKejadian, " +
                "        to_char(aduan.tanggaltarget, 'dd/mm/yyyy') TanggalTarget, " +
                "        to_char(aduan.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(aduan.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "        to_char(aduan.tanggalselesai, 'dd-mm-yyyy') TanggalSelesai, " +
                "        to_char(aduan.tanggalselesai, 'fmDD Month YYYY') InfoTanggalSelesai, " +
                "        aduan.juduladuan, aduan.lokasikejadian, aduan.tujuanpengaduan, aduan.nomorlaporan, aduan.statuslaporan, " +
                "        aduan.kategori, aduan.lapormelalui, aduan.sumber, aduan.statusmoderasi, aduan.uraian, aduan.hasil, " +
                "        aduan.penyebab, aduan.tanggapan, aduan.namapengadu, aduan.alamatpengadu, aduan.nikpengadu, " +
                "        aduan.teleponpengadu, aduan.emailpengadu, aduan.kategoripengadu, aduan.pekerjaanpengadu, " +
                "        CASE " +
                "            WHEN aduan.tanggalselesai IS NOT NULL THEN 'Sudah Diarsipkan/Selesai' " +
                "            WHEN aduaninbox.urutan = 1 THEN 'Pembuat Pengaduan' " +
                "            WHEN aduaninbox.statusterkirim = 1 THEN 'Telah Terkirim' " +
                "            WHEN aduaninbox.tanggalbuka IS NULL THEN 'Belum Dibuka' " +
                "            ELSE 'Dalam Proses' " +
                "        END AS KetStatusTerkirim " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan ON aduan.aduanid = aduaninbox.aduanid " +
                "        LEFT JOIN jabatan profilepengirim ON profilepengirim.profileid = aduaninbox.profilepengirim " +
                "        LEFT JOIN jabatan profilepenerima ON profilepenerima.profileid = aduaninbox.profilepenerima " +
                //"        JOIN jabatan profiletujuan ON profiletujuan.profileid = aduaninbox.profilepenerima " +
                //"        LEFT JOIN jabatan profiledari ON profiledari.profileid = aduaninbox.profilepengirim " +
                "    WHERE " +
                "        aduan.aduanid = :AduanId " +
                "        AND (aduaninbox.statushapus IS NULL OR aduaninbox.statushapus = '0') ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                query += " AND aduan.unitkerjaid = :UnitKerjaId ";
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.AduanInbox>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.Pengaduan> GetInfoPengaduan(string unitkerjaid, string myProfiles, string metadata, int from, int to)
        {
            List<Entities.Pengaduan> records = new List<Entities.Pengaduan>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY aduan.tanggaladuan, aduan.tanggalproses) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        aduan.aduanid, " +
                "        to_char(aduan.tanggaladuan, 'dd/mm/yyyy') TanggalAduan, " +
                "        trim(to_char(aduan.tanggaladuan, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN')) InfoTanggalAduan, " +
                "        to_char(aduan.tanggalkejadian, 'dd/mm/yyyy') TanggalKejadian, " +
                "        to_char(aduan.tanggalkejadian, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN') InfoTanggalKejadian, " +
                "        to_char(aduan.tanggaltarget, 'dd/mm/yyyy') TanggalTarget, " +
                "        to_char(aduan.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(aduan.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "        to_char(aduan.tanggalselesai, 'dd-mm-yyyy') TanggalSelesai, " +
                "        to_char(aduan.tanggalselesai, 'fmDD Month YYYY') InfoTanggalSelesai, " +
                "        aduan.juduladuan, aduan.lokasikejadian, aduan.tujuanpengaduan, aduan.nomorlaporan, aduan.statuslaporan, " +
                "        aduan.kategori, aduan.lapormelalui, aduan.sumber, aduan.statusmoderasi, aduan.uraian, aduan.hasil, " +
                "        aduan.penyebab, aduan.tanggapan, aduan.namapengadu, aduan.alamatpengadu, aduan.nikpengadu, " +
                "        aduan.teleponpengadu, aduan.emailpengadu, aduan.kategoripengadu, aduan.pekerjaanpengadu, " +
                "        decode(arsipaduan.arsipaduanid, null, 0, 1) AS StatusArsip " +
                "    FROM " +
                "        " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan " +
                "        LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipaduan ON arsipaduan.aduanid = aduan.aduanid AND arsipaduan.unitkerjaid = :UnitKerjaId " +
                "    WHERE " +
                "        aduan.aduanid IS NOT NULL ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

            //if (!string.IsNullOrEmpty(unitkerjaid))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
            //    query += " AND aduan.unitkerjaid = :UnitKerjaId ";
            //}
            if (!String.IsNullOrEmpty(myProfiles))
            {
                query += " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox WHERE aduaninbox.aduanid = aduan.aduanid AND aduaninbox.profilepenerima IN (" + myProfiles + ")) ";
            }
            if (!String.IsNullOrEmpty(metadata))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                query += " AND LOWER(utl_raw.cast_to_varchar2(aduan.metadata)) LIKE :Metadata ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.Pengaduan>(query, parameters).ToList();
            }

            return records;
        }

        public Entities.Pengaduan GetPengaduanById(string aduanid, string unitkerjaid)
        {
            Entities.Pengaduan data = new Entities.Pengaduan();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    aduan.aduanid, " +
                "    to_char(aduan.tanggaladuan, 'dd/mm/yyyy') TanggalAduan, " +
                "    trim(to_char(aduan.tanggaladuan, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN')) InfoTanggalAduan, " +
                "    to_char(aduan.tanggalkejadian, 'dd/mm/yyyy') TanggalKejadian, " +
                "    to_char(aduan.tanggalkejadian, 'fmDay, dd Month yyyy', 'nls_date_language=INDONESIAN') InfoTanggalKejadian, " +
                "    to_char(aduan.tanggaltarget, 'dd/mm/yyyy') TanggalTarget, " +
                "    to_char(aduan.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "    to_char(aduan.tanggalproses, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalProses, " +
                "    to_char(aduan.tanggalselesai, 'dd-mm-yyyy') TanggalSelesai, " +
                "    to_char(aduan.tanggalselesai, 'fmDD Month YYYY') InfoTanggalSelesai, " +
                "    aduan.juduladuan, aduan.lokasikejadian, aduan.tujuanpengaduan, aduan.nomorlaporan, aduan.statuslaporan, " +
                "    aduan.kategori, aduan.lapormelalui, aduan.sumber, aduan.statusmoderasi, aduan.uraian, aduan.hasil, " +
                "    aduan.penyebab, aduan.tanggapan, aduan.namapengadu, aduan.alamatpengadu, aduan.nikpengadu, " +
                "    aduan.teleponpengadu, aduan.emailpengadu, aduan.kategoripengadu, aduan.pekerjaanpengadu, " +
                "    decode(arsipaduan.arsipaduanid, null, 0, 1) AS StatusArsip " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan " +
                "    LEFT JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipaduan ON arsipaduan.aduanid = aduan.aduanid AND arsipaduan.unitkerjaid = :UnitKerjaId " +
                "WHERE " +
                "    aduan.aduanid = :AduanId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.Pengaduan>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public int JumlahPengaduan(string unitkerjaid, string nip, string myProfiles)
        {
            int result = 0;

            if (string.IsNullOrEmpty(myProfiles))
            {
                myProfiles = "'xx'";
            }

            ArrayList arrayListParameters = new ArrayList();

            string query = 
                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox " +
                "       JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan ON aduan.aduanid = aduaninbox.aduanid AND aduan.statuspengaduan = 1 " +
                "WHERE  statusterkirim = 0 " +
                "       AND (aduaninbox.statushapus IS NULL OR aduaninbox.statushapus = '0') " +
                "       AND (aduaninbox.nip = :Nip OR aduaninbox.nip is null) " +
                "       AND aduaninbox.profilepenerima IN (" + myProfiles + ") " +
                "       AND NOT EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipaduan WHERE arsipaduan.aduanid = aduan.aduanid AND arsipaduan.unitkerjaid = :UnitKerjaId) ";
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public string GetStatusForwardTU(string aduaninboxid)
        {
            string result = "0";

            string query = "SELECT to_char(statusforwardtu) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox WHERE aduaninboxid = :AduanInboxId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetCatatanSebelumnya(string aduaninboxid)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("id-ID");
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("id-ID");

            string result = "";

            // Get Tanggal Kirim
            string query =
                "SELECT inbox1.tanggalkirim FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox1 " +
                "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox2 ON inbox2.aduanid = inbox1.aduanid " +
                "             AND inbox2.aduaninboxid = :AduanInboxId " +
                "             AND inbox1.urutan = (inbox2.urutan-1)";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                DateTime tanggalkirim = ctx.Database.SqlQuery<DateTime>(query, parameters).First();

                result += "<span style='font-size:9pt; color:Navy'><u>" + tanggalkirim.ToString("dddd, d MMMM yyyy") + "</u></span>";

                query =
                    "SELECT " +
                    "     decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                    "     decode(pegawai.nama, '', '', pegawai.nama) || " +
                    "     decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap " +
                    " FROM pegawai WHERE pegawaiid IN " +
                    "         (SELECT inbox1.nip FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox1 " +
                    "                 JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox2 ON inbox2.aduanid = inbox1.aduanid " +
                    "                      AND inbox2.aduaninboxid = :AduanInboxId1 " +
                    "                      AND inbox1.urutan = (inbox2.urutan-1)) " +
                    "UNION " +
                    "SELECT " +
                    "     ppnpn.nama AS NamaLengkap " +
                    " FROM ppnpn WHERE nik IN " +
                    "         (SELECT inbox1.nip FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox1 " +
                    "                 JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox2 ON inbox2.aduanid = inbox1.aduanid " +
                    "                      AND inbox2.aduaninboxid = :AduanInboxId2 " +
                    "                      AND inbox1.urutan = (inbox2.urutan-1)) ";
                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId1", aduaninboxid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId2", aduaninboxid));
                parameters = arrayListParameters.OfType<object>().ToArray();
                string namapengirim = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();


                // Get Keterangan
                query =
                     "SELECT inbox1.keterangan FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox1 " +
                     "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox2 ON inbox2.aduanid = inbox1.aduanid " +
                     "             AND inbox2.aduaninboxid = :AduanInboxId " +
                     "             AND inbox1.urutan = (inbox2.urutan-1)";
                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));
                parameters = arrayListParameters.OfType<object>().ToArray();
                string keterangan = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();


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

        public string GetDisposisiSebelumnya(string aduaninboxid)
        {
            string result = "";

            if (!string.IsNullOrEmpty(aduaninboxid))
            {
                string query =
                     "SELECT inbox1.perintahdisposisi FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox1 " +
                     "        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox inbox2 ON inbox2.aduanid = inbox1.aduanid " +
                     "             AND inbox2.aduaninboxid = :AduanInboxId " +
                     "             AND inbox1.urutan = (inbox2.urutan-1)";

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
            }

            return result;
        }

        public List<Models.Entities.UnitKerja> GetUnitKerjaAduanHistory(string aduanid)
        {
            List<Models.Entities.UnitKerja> records = new List<Models.Entities.UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "select unitkerjaid, namaunitkerja from unitkerja where unitkerjaid in ( " +
                "select distinct unitkerjaid from jabatan where profileid in ( " +
                "select distinct profilepenerima from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox where aduanid = :AduanId " +
                ")) order by unitkerjaid";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.UnitKerja>(query, parameters).ToList<Models.Entities.UnitKerja>();
            }

            return records;
        }

        public Models.Entities.TransactionResult BukaAduanInbox(string aduanid, string aduaninboxid, string nip, string namapegawai)
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
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox SET " +
                             "   nip = :Nip, namapegawai = :NamaPegawai, tanggalbuka = SYSDATE " +
                             " WHERE aduaninboxid = :AduanInboxId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // update tanggalterima, bila kosong
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox SET " +
                             "   tanggalterima = SYSDATE " +
                             " WHERE tanggalterima IS NULL AND aduaninboxid = :AduanInboxId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // Update tanggalproses di table ADUAN, bila kosong
                        sql =
                             "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan SET " +
                             "       tanggalproses = SYSDATE " +
                             " WHERE aduanid = :AduanId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));
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

        public int GetMaxUrutanAduanInbox(BpnDbContext ctx, string aduanid)
        {
            int result = 0;

            string query = "select max(urutan)+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox where aduanid = :AduanId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", aduanid));

            object[] parameters = arrayListParameters.OfType<object>().ToArray();
            result = ctx.Database.SqlQuery<int>(query, parameters).First();

            return result;
        }

        public Models.Entities.TransactionResult InsertPengaduan(Models.Entities.Pengaduan data, string unitkerjaid, string myprofileid, string nip, string namapegawaipengirim)
        {
            DataMasterModel dataMasterModel = new DataMasterModel();
            PersuratanModel persuratanModel = new PersuratanModel();

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
                        string myClientId = Functions.MyClientId;


                        #region Session Lampiran

                        List<Models.Entities.SessionLampiranSurat> dataSessionLampiran = persuratanModel.GetListSessionLampiran(myClientId);

                        data.JumlahLampiran = dataSessionLampiran.Count;

                        #endregion


                        string id = persuratanModel.GetUID();
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        data.AduanId = id;
                        data.Sumber = "Data Entri";
                        data.StatusLaporan = "Tulis Laporan";


                        #region NOMOR LAPORAN

                        // Cek Konter Nomor Laporan
                        string query = "select count(*) from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :UnitKerjaId and tahun = :Tahun AND tipesurat = :Tipe";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Pengaduan"));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                        if (jumlahrecord == 0)
                        {
                            // Bila tidak ada, Insert KONTERSURAT
                            query =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                "( " +
                                "       SYS_GUID(), :UnitKerjaId, :TipeSurat, :Tahun, 0)";
                            //query = sWhitespace.Replace(query, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeSurat", "Pengaduan"));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(query, parameters);
                        }

                        decimal nilainomorpengaduan = 1;

                        query =
                            "select nilaikonter+1 from " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :UnitKerjaId and tahun = :Tahun AND tipesurat = :Tipe " +
                            "FOR UPDATE NOWAIT";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Pengaduan"));
                        parameters = arrayListParameters.OfType<object>().ToArray();

                        nilainomorpengaduan = ctx.Database.SqlQuery<decimal>(query, parameters).FirstOrDefault();


                        sql = "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :UnitKerjaId AND tahun = :Tahun AND tipesurat = :Tipe";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiKonter", nilainomorpengaduan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", "Pengaduan"));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Binding Nomor Laporan
                        int bulan = Convert.ToDateTime(persuratanModel.GetServerDate(), theCultureInfo).Month;
                        string strBulan = Functions.NomorRomawi(bulan);
                        string kodeindentifikasi = persuratanModel.GetKodeIdentifikasi(unitkerjaid);
                        string kodesurat = "PG-";

                        string strNomorPengaduan = Convert.ToString(nilainomorpengaduan) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(persuratanModel.GetServerYear());
                        data.NomorLaporan = strNomorPengaduan;

                        #endregion



                        #region Set Metadata
                        string metadata = "";
                        metadata += data.JudulAduan + " ";
                        metadata += data.TanggalAduan + " ";
                        metadata += data.TanggalKejadian + " ";
                        metadata += data.LokasiKejadian + " ";
                        metadata += data.TujuanPengaduan + " ";
                        metadata += data.NomorLaporan + " ";
                        metadata += data.Kategori + " ";
                        metadata += data.LaporMelalui + " ";
                        metadata += data.Sumber + " ";
                        metadata += data.Uraian + " ";
                        metadata += data.Penyebab + " ";
                        metadata += data.NamaPengadu + " ";
                        metadata += data.AlamatPengadu + " ";
                        metadata += data.NIKPengadu + " ";
                        metadata += data.TeleponPengadu + " ";
                        metadata += data.EmailPengadu + " ";
                        metadata += data.KategoriPengadu + " ";
                        metadata = metadata.Trim();
                        #endregion


                        // Insert ADUAN
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan ( " +
                            "       aduanid, unitkerjaid, juduladuan, tanggaladuan, tanggalkejadian, lokasikejadian, nomorlaporan, kategori, lapormelalui, sumber, statuslaporan, uraian, " +
                            "       namapengadu, alamatpengadu, nikpengadu, teleponpengadu, emailpengadu, pekerjaanpengadu, kategoripengadu, jumlahlampiran, metadata) VALUES " +
                            "( " +
                            "       :Id, :UnitKerjaId, :JudulAduan, TO_DATE(:TanggalAduan,'DD/MM/YYYY'), TO_DATE(:TanggalKejadian,'DD/MM/YYYY'), " +
                            "       :LokasiKejadian, :NomorLaporan, :Kategori, :LaporMelalui, :Sumber, :StatusLaporan, :Uraian, " +
                            "       :NamaPengadu, :AlamatPengadu, :NIKPengadu, :TeleponPengadu, :EmailPengadu, :PekerjaanPengadu, :KategoriPengadu, :JumlahLampiran, utl_raw.cast_to_raw(:Metadata))";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulAduan", data.JudulAduan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalAduan", data.TanggalAduan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalKejadian", data.TanggalKejadian));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LokasiKejadian", data.LokasiKejadian));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorLaporan", data.NomorLaporan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Kategori", data.Kategori));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LaporMelalui", data.LaporMelalui));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Sumber", data.Sumber));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusLaporan", data.StatusLaporan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Uraian", data.Uraian));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengadu", data.NamaPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPengadu", data.AlamatPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NIKPengadu", data.NIKPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TeleponPengadu", data.TeleponPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("EmailPengadu", data.EmailPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PekerjaanPengadu", data.PekerjaanPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KategoriPengadu", data.KategoriPengadu));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahLampiran", data.JumlahLampiran));
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
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampiranpengaduan ( " +
                                    "       lampiranpengaduanid, aduanid, path, namafile, profileid, unitkerjaid, keterangan, nip) VALUES " +
                                    "( " +
                                    "       :LampiranPengaduanId,:AduanId,:FolderFile,:NamaFile,:ProfileIdPengirim,:UnitKerjaId,:Keterangan,:Nip)";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranPengaduanId", lampiranSurat.LampiranSuratId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("FolderFile", folderfile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", lampiranSurat.Nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }



                        string aduaninboxid = "";
                        int urutan = 0;


                        #region Insert ADUANINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        aduaninboxid = persuratanModel.GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox ( " +
                            "       ADUANINBOXID, ADUANID, UNITKERJAID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "       urutan) VALUES " +
                            "( " +
                            "       :AduanInboxId,:AduanId,:UnitKerjaId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "       :Nip,:NamaPegawai,SYSDATE,SYSDATE,:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "       1)";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SuratInboxId", aduaninboxid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", myprofileid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip)); // nip Pengirim
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Pengaduan
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", "")); // data.Uraian
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", ""));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        #endregion



                        #region Session Tujuan Surat

                        List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanModel.GetListSessionTujuanSurat(myClientId);

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            //if (tujuanSurat.Redaksi == "Asli")
                            //{
                            //    data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            //}


                            // GET JABATAN DAN PEGAWAI TUJUAN

                            // Cek Delegasi Surat ------------------------
                            string profileidtujuan = tujuanSurat.ProfileId;
                            string niptujuan = tujuanSurat.NIP;
                            string namapegawaitujuan = tujuanSurat.NamaPegawai;

                            Entities.DelegasiSurat delegasiSurat = persuratanModel.GetDelegasiSurat(tujuanSurat.ProfileId);
                            if (delegasiSurat != null)
                            {
                                profileidtujuan = delegasiSurat.ProfilePenerima;
                                niptujuan = delegasiSurat.NIPPenerima;
                                namapegawaitujuan = delegasiSurat.NamaPenerima;
                            }
                            // Eof Cek Delegasi Surat --------------------

                            // Eof // GET JABATAN DAN PEGAWAI TUJUAN------



                            #region Kirim ke Tujuan Surat

                            // detail Jabatan dan Pegawai Tujuan sudah disetting di atas, di awal Loop

                            #region Cek Duplikasi

                            bool BisaKirim = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox " +
                                "WHERE aduanid = :AduanId AND unitkerjaid = :UnitKerjaId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtujuan));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahdata > 0)
                            {
                                BisaKirim = false;
                            }

                            #endregion

                            if (BisaKirim)
                            {
                                aduaninboxid = persuratanModel.GetUID();
                                urutan = GetMaxUrutanAduanInbox(ctx, id);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox ( " +
                                    "       ADUANINBOXID, ADUANID, UNITKERJAID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       urutan) VALUES " +
                                    "( " +
                                    "       :AduanInboxId,:AduanId,:UnitKerjaId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE,SYSDATE,:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :Urutan)";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", id));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", profileidtujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", niptujuan)); // nip penerima surat
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", namapegawaitujuan)); // nama penerima surat
                                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalTerima", data.TanggalTerima));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", "")); // data.Uraian
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }

                            #endregion

                        }

                        #endregion


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.AduanId;
                        tr.Pesan = "Pengaduan berhasil dikirim";
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

        public Models.Entities.TransactionResult KirimPengaduan(Models.Entities.AduanInbox data, string unitkerjaid, string nip)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();
            PersuratanModel persuratanModel = new PersuratanModel();

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
                        string myClientId = Functions.MyClientId;

                        // Update Table ADUANINBOX (Update status AduanInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox SET " +
                            "       statusterkirim = 1, keterangan = :Keterangan, perintahdisposisi = :PerintahDisposisi " +
                            "WHERE aduaninboxid = :AduanInboxId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Keterangan", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PerintahDisposisi", data.PerintahDisposisi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", data.AduanInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        #region Session Tujuan Surat

                        string aduaninboxid = "";
                        int urutan = 0;

                        List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = persuratanModel.GetListSessionTujuanSurat(myClientId);

                        foreach (Models.Entities.SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            Models.Entities.Pegawai pegawaiTujuan = dataMasterModel.GetPegawaiByPegawaiId(tujuanSurat.NIP);


                            #region Cek Duplikasi

                            bool BisaKirim = true;

                            sql =
                                "SELECT count(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox " +
                                "WHERE aduanid = :AduanId " +
                                "AND profilepenerima = :ProfileIdPenerima AND nip = :Nip AND statusterkirim = 0";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            int jumlahinbox = ctx.Database.SqlQuery<int>(sql, parameters).First();
                            if (jumlahinbox > 0)
                            {
                                BisaKirim = false;
                            }

                            #endregion


                            if (BisaKirim)
                            {
                                if (string.IsNullOrEmpty(data.TanggalTerima))
                                {
                                    data.TanggalTerima = GetServerDate().ToString("dd/MM/yyyy HH:mm");
                                }

                                // Insert ADUANINBOX
                                aduaninboxid = persuratanModel.GetUID();
                                urutan = GetMaxUrutanAduanInbox(ctx, data.AduanId);
                                sql =
                                    "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox ( " +
                                    "       ADUANINBOXID, ADUANID, UNITKERJAID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, statusurgent, redaksi, urutan) VALUES " +
                                    "( " +
                                    "       :AduanInboxId,:AduanId,:UnitKerjaId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE,SYSDATE,:TindakLanjut,0,:StatusUrgent,:Redaksi,:Urutan)";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", aduaninboxid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("UnitKerjaId", unitkerjaid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId)); // dataPegawaiPenerima.ProfileId
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPengirim", data.NamaPengirim));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat | dataPegawaiPenerima.PegawaiId
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TindakLanjut", "Ekspedisi"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusUrgent", tujuanSurat.StatusUrgent));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Urutan", urutan));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Update Status Pengaduan (Beri Tanggapan)
                                sql =
                                    "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan SET " +
                                    "         statuslaporan = :StatusLaporan " +
                                    "  WHERE  aduanid = :AduanId";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusLaporan", "Beri Tanggapan"));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }

                        #endregion


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.AduanId;
                        tr.Pesan = "Pengaduan berhasil dikirim";
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

        public Models.Entities.TransactionResult SimpanCatatanAnda(Models.Entities.Pengaduan data)
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
                        // Update Table ADUANINBOX (Update Catatan Anda)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox SET " +
                            "         keterangan = :CatatanAnda " +
                            "  WHERE aduaninboxid = :AduanInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", data.AduanInboxId));
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

        public Models.Entities.TransactionResult ArsipPengaduan(Models.Entities.Pengaduan data, string unitkerjaid, string nip)
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
                        // Update Table ADUAN
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan SET " +
                            "    tanggalselesai = SYSDATE, tanggalarsip = SYSDATE, statuspengaduan = 0, statuslaporan = :StatusLaporan " +
                            "  WHERE aduanid = :AduanId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusLaporan", "Selesai"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table ADUANINBOX (Update status AduanInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox SET " +
                            "         nip = :Nip, namapegawai = :NamaPegawai, keterangan = :CatatanAnda, tindaklanjut = 'Selesai' " +
                            "  WHERE  aduaninboxid = :AduanInboxId";
                        //sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", data.AduanInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Insert ARSIPADUAN
                        string newid = GetUID();
                        sql =
                            "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".arsipaduan ( " +
                            "       arsipaduanid, aduanid, unitkerjaid, tanggal, profilearsip) VALUES " +
                            "( " +
                            "      :ArsipAduanId,:AduanId,:KantorId,SYSDATE,:ProfileIdPengirim)";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ArsipAduanId", newid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", unitkerjaid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);



                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.AduanId;
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

        public Models.Entities.TransactionResult SelesaiAduanInbox(Models.Entities.Pengaduan data, string unitkerjaid, string nip)
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
                        // Update Table ADUAN
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduan SET " +
                            "    tanggalselesai = SYSDATE, statuspengaduan = 0, statuslaporan = :StatusLaporan " +
                            "  WHERE aduanid = :AduanId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusLaporan", "Selesai"));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanId", data.AduanId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        // Update Table ADUANINBOX (Update status AduanInbox menjadi terkirim)
                        sql =
                            "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".aduaninbox SET " +
                            "         nip = :Nip, namapegawai = :NamaPegawai, keterangan = :CatatanAnda, tindaklanjut = 'Selesai', statusterkirim = 1 " +
                            "  WHERE  aduaninboxid = :AduanInboxId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawai", data.NamaPengirim));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("CatatanAnda", data.CatatanAnda));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AduanInboxId", data.AduanInboxId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.AduanId;
                        tr.Pesan = "Pengaduan berhasil diarsip";
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