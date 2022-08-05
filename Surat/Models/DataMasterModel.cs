﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Surat.Models.Entities;
using Npgsql;

namespace Surat.Models
{
    public class DataMasterModel
    {
        Regex sWhitespace = new Regex(@"\s+");

        private const string encryptionKey = "AE09F72B007CAAB5";

        public string GetProfileIdFromName(string profilename)
        {
            string result = "";

            string query = "SELECT profileid FROM jabatan WHERE nama = :ProfileName";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("ProfileName", profilename));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetProfileNameFromId(string profileid)
        {
            string result = "";

            string query = "SELECT nama FROM jabatan WHERE profileid = :ProfileId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetNamaUnitKerjaById(string unitkerjaid)
        {
            string result = "";

            string query = "SELECT namaunitkerja FROM unitkerja WHERE unitkerjaid = :Id";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Id", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetUnitKerjaFromProfileId(string profileid)
        {
            string result = "";

            string query = "SELECT namaunitkerja FROM unitkerja WHERE unitkerjaid = (SELECT unitkerjaid FROM jabatan WHERE profileid = :ProfileId)";

            ArrayList arrayListParameters = new ArrayList();

            if (profileid.Contains(","))
            {
                string additionalClause = String.Empty;

                int count = 0;
                string clause = " AND (";
                string[] arrProfileId = profileid.Split(",".ToCharArray());
                foreach (string iprofileid in arrProfileId)
                {
                    string param = ":Profile" + count;

                    additionalClause = additionalClause + clause + @"profileid = " + param;
                    clause = " OR ";

                    arrayListParameters.Add(new OracleParameter(param, iprofileid));

                    count = count + 1;
                }

                additionalClause = additionalClause + ") ";

                query = "SELECT namaunitkerja FROM unitkerja WHERE unitkerjaid IN (SELECT unitkerjaid FROM jabatan WHERE profileid IS NOT NULL " + additionalClause + ")";
            }
            else
            {
                arrayListParameters.Add(new OracleParameter("ProfileId", profileid));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetProfileIdTuFromProfileId(string profileid)
        {
            string result = "";

            string query = "SELECT profileidtu FROM jabatan WHERE profileid = :ProfileId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetProfileIdBAFromProfileId(string profileid)
        {
            string result = "";

            string query = "SELECT profileidba FROM jabatan WHERE profileid = :ProfileId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetPegawaiIdFromProfileId(string profileid, bool isPLT = false)
        {
            string result = "";

            string query = "SELECT pegawaiid FROM jabatanpegawai WHERE (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND profileid = :ProfileId AND NVL(STATUSPLT,'0') = '0'";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));
            if (isPLT)
            {
                query = query.Replace("NVL(STATUSPLT,'0') = '0'", "NVL(STATUSPLT,'0') IN ('1','2')");
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetProfileIdTuByNip(string nip, string kantorid)
        {
            string result = "";

            string query = @"
                SELECT PROFILEID
                FROM JABATAN
                WHERE
                  PROFILEIDTU = PROFILEID AND
                  JABATAN.PROFILEID IN 
                    (SELECT JP.PROFILEID
                     FROM JABATANPEGAWAI JP
                       INNER JOIN JABATAN JB ON
                         JB.PROFILEID = JP.PROFILEID AND
                         NVL(JB.SEKSIID,'X') <> 'A800' AND
                         JB.PROFILEIDTU IS NOT NULL 
                       INNER JOIN UNITKERJA UK ON
                         UK.UNITKERJAID = JB.UNITKERJAID AND
                         UK.KANTORID = :KantorId1 
                     WHERE
                       JP.PEGAWAIID = :Nip1 AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0' 
                     GROUP BY JP.PROFILEID) OR
                  PROFILEIDBA = PROFILEID AND
                  JABATAN.PROFILEID IN 
                    (SELECT JP.PROFILEID
                     FROM JABATANPEGAWAI JP
                       INNER JOIN JABATAN JB ON
                         JB.PROFILEID = JP.PROFILEID AND
                         NVL(JB.SEKSIID,'X') <> 'A800' AND
                         JB.PROFILEIDTU IS NOT NULL 
                       INNER JOIN UNITKERJA UK ON
                         UK.UNITKERJAID = JB.UNITKERJAID AND
                         UK.KANTORID = :KantorId2 
                     WHERE
                       JP.PEGAWAIID = :Nip2 AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0' 
                     GROUP BY JP.PROFILEID)";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("KantorId1", kantorid));
            arrayListParameters.Add(new OracleParameter("Nip1", nip));
            arrayListParameters.Add(new OracleParameter("KantorId2", kantorid));
            arrayListParameters.Add(new OracleParameter("Nip2", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetProfileIdTuByDelegasi(string myprofileid, string nip)
        {
            string result = "";

            string query =
                "SELECT count(*) " +
                "FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                "WHERE statusterkunci = 0 " +
                "      AND statusterkirim = 0 " +
                "      AND statusforwardtu = 1 " +
                "      AND nip = :Nip";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Nip", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = myprofileid;
                }
            }

            return result;
        }

        public string GetMyEselonId(string nip, string kantorid = "")
        {
            string result = "6";

            string query = @"
                SELECT NVL(TO_CHAR(JB.TIPEESELONID), '6') AS TIPEESELONID
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    JB.PROFILEIDTU IS NOT NULL
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID AND
                    UK.KANTORID = NVL(NULLIF(:KantorId,''),UK.KANTORID)
                WHERE
                  JP.PEGAWAIID = :Nip AND
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  NVL(JP.STATUSHAPUS,'0') = '0'
                GROUP BY NVL(TO_CHAR(JB.TIPEESELONID), '6')";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
            arrayListParameters.Add(new OracleParameter("Nip", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string getUserProfileTU(string nip, string unitkerjaid)
        {
            string result = string.Empty;

            string query = @"
                SELECT PROFILEID
                FROM JABATAN
                WHERE
                  PROFILEIDTU = PROFILEID AND
                  JABATAN.PROFILEID IN
                    (SELECT JP.PROFILEID
                     FROM JABATANPEGAWAI JP
                     WHERE
                       JP.PEGAWAIID = :Nip1 AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     GROUP BY JP.PROFILEID) OR
                  PROFILEIDBA = PROFILEID AND
                  JABATAN.PROFILEID IN
                    (SELECT JP.PROFILEID
                     FROM JABATANPEGAWAI JP
                     WHERE
                       JP.PEGAWAIID = :Nip2 AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     GROUP BY JP.PROFILEID) AND
                  NVL(SEKSIID,'X') <> 'A800' AND
                  PROFILEIDTU IS NOT NULL AND
                  UNITKERJAID = :Unitkerja";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Nip1", nip));
            arrayListParameters.Add(new OracleParameter("Nip2", nip));
            arrayListParameters.Add(new OracleParameter("Unitkerja", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                var records = ctx.Database.SqlQuery<string>(query, parameters).ToList().ToArray();

                foreach (string strProfileId in records)
                {
                    result += "'" + strProfileId + "',";
                }

                if (result.Length > 0)
                {
                    result = result.Remove(result.Length - 1, 1);
                }
            }

            return result;
        }

        public string GetPegawaiIdFromNamaAtauNip(string nama, string nip)
        {
            string result = "";

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT pegawaiid
                  FROM pegawai
                  WHERE LOWER(nama) LIKE :Nama";
            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Clear();
            arrayListParameters.Add(new OracleParameter("Nama", String.Concat("%", nama.ToLower(), "%")));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();

                if (string.IsNullOrEmpty(result))
                {
                    query =
                        @"SELECT pegawaiid
                          FROM pegawai
                          WHERE pegawaiid LIKE :Nip";
                    query = sWhitespace.Replace(query, " ");

                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("Nip", String.Concat(nip, "%")));

                    parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
            }

            return result;
        }

        public int GetTipeKantor(string kantorid)
        {
            int result = 0;

            string query = "SELECT tipekantorid FROM kantor WHERE kantorid = :KantorId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public bool CheckUserProfile(string pegawaiid, string kantorid, string profileid)
        {
            bool result = false;

            string query = "SELECT COUNT(1) FROM JABATANPEGAWAI WHERE (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND PEGAWAIID = :PegawaiId AND KANTORID = :KantorId AND PROFILEID IN (" + profileid + ")";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public Kantor GetKantor(string kantorid)
        {
            Kantor record = new Kantor();

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                      kantorid, kode, nama NamaKantor, kota, alamat, telepon, fax, email, kodesk, tipekantorid
                  FROM kantor
                  WHERE kantorid = :KantorId";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                record = ctx.Database.SqlQuery<Kantor>(query, parameters).FirstOrDefault();
            }

            return record;
        }

        public List<Kantor> GetKantorWilayah()
        {
            var list = new List<Kantor>();

            string query =
                "select " +
                "    kantorid, UPPER(replace(nama, 'Kantor Wilayah ', '')) NamaKantor " +
                "from kantor where tipekantorid = 2 order by kode";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Kantor>(query).ToList();
            }

            return list;
        }

        public List<Kantor> GetKantorPertanahanByKanwilId(string kanwilid)
        {
            var list = new List<Kantor>();

            OracleParameter p1 = new OracleParameter("KanwilId", kanwilid);
            object[] parameters = new object[1] { p1 };

            string query =
                "select " +
                "    kantorid, UPPER(replace(nama, 'Kantor Pertanahan ', '')) NamaKantor " +
                "from kantor where tipekantorid in (3, 4) and induk = :KanwilId order by kode";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Kantor>(query, parameters).ToList();
            }

            return list;
        }

        public List<Provinsi> GetProvinsi()
        {
            var list = new List<Provinsi>();

            string query =
                "SELECT " +
                "    wilayah.wilayahid ProvinsiId, UPPER(tipewilayah.tipe || ' ' || wilayah.nama) NamaProvinsi " +
                "FROM " +
                "    wilayah, tipewilayah " +
                "WHERE " +
                "    wilayah.validsampai IS NULL " +
                "    AND tipewilayah.tipewilayahid = wilayah.tipewilayahid " +
                "    AND wilayah.tipewilayahid = 1 " +
                "ORDER BY wilayah.kode";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Provinsi>(query).ToList();
            }

            return list;
        }

        public List<Kabupaten> GetKabupatenByKantorIdProvinsi(string kantoridprovinsi)
        {
            var list = new List<Kabupaten>();

            OracleParameter p1 = new OracleParameter("KantorIdProvinsi", kantoridprovinsi);
            object[] parameters = new object[1] { p1 };

            string query =
                "SELECT " +
                "    wilayah.wilayahid KABUPATENID, UPPER(tipewilayah.tipe || ' ' || wilayah.nama) NAMAKABUPATEN " +
                "FROM " +
                "    wilayah, tipewilayah " +
                "WHERE " +
                "    wilayah.validsampai IS NULL " +
                "    AND tipewilayah.tipewilayahid = wilayah.tipewilayahid " +
                "    AND wilayah.tipewilayahid IN (2,3,4) AND wilayah.induk IN ( " +
                "        SELECT wilayahid FROM wilayah WHERE wilayahid IN ( " +
                "            SELECT DISTINCT wilayahid FROM wilayahkantor WHERE kantorid = :KantorIdProvinsi) AND tipewilayahid = 1) " +
                "ORDER BY wilayah.nama";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Kabupaten>(query, parameters).ToList();
            }

            return list;
        }

        public List<Kabupaten> GetKabupatenByProvinsiId(string provinsiid)
        {
            var list = new List<Kabupaten>();

            OracleParameter p1 = new OracleParameter("ProvinsiId", provinsiid);
            object[] parameters = new object[1] { p1 };

            string query =
                "SELECT " +
                "    wilayah.wilayahid KABUPATENID, UPPER(tipewilayah.tipe || ' ' || wilayah.nama) NAMAKABUPATEN " +
                "FROM " +
                "    wilayah, tipewilayah " +
                "WHERE " +
                "    wilayah.validsampai IS NULL " +
                "    AND tipewilayah.tipewilayahid = wilayah.tipewilayahid " +
                "    AND wilayah.tipewilayahid IN (2,3,4) AND wilayah.induk = :ProvinsiId " +
                "ORDER BY wilayah.nama";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Kabupaten>(query, parameters).ToList();
            }

            return list;
        }

        public List<Kecamatan> GetKecamatanByKabupatenId(string kabupatenid)
        {
            var list = new List<Kecamatan>();

            OracleParameter p1 = new OracleParameter("KabupatenId", kabupatenid);
            object[] parameters = new object[1] { p1 };

            string query =
                "SELECT wilayah.wilayahid KECAMATANID, 'KECAMATAN ' || UPPER(wilayah.nama) NAMAKECAMATAN " +
                "FROM wilayah " +
                "WHERE " +
                "    wilayah.validsampai IS NULL " +
                "    AND wilayah.induk = :KabupatenId " +
                "ORDER BY wilayah.nama";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Kecamatan>(query, parameters).ToList();
            }

            return list;
        }

        public List<Desa> GetDesaByKecamatanId(string kecamatanid)
        {
            var list = new List<Desa>();

            OracleParameter p1 = new OracleParameter("KecamatanId", kecamatanid);
            object[] parameters = new object[1] { p1 };

            string query =
                "SELECT wilayah.wilayahid DESAID, UPPER(tipewilayah.tipe || ' ' || wilayah.nama) NAMADESA " +
                "FROM wilayah, tipewilayah " +
                "WHERE " +
                "    wilayah.validsampai IS NULL " +
                "    AND tipewilayah.tipewilayahid = wilayah.tipewilayahid " +
                "    AND wilayah.induk = :KecamatanId " +
                "ORDER BY wilayah.nama";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Desa>(query, parameters).ToList();
            }

            return list;
        }

        public List<UnitKerja> GetListUnitKerja(string id, string nama, string induk, bool aktif, bool cek = false, string kid = null, bool cek2 = false)
        {
            List<UnitKerja> records = new List<UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT unitkerjaid, induk, namaunitkerja, eselon, tipekantorid FROM unitkerja WHERE unitkerjaid IS NOT NULL ";
            if (!string.IsNullOrEmpty(kid))
            {
                if (cek2)
                {
                    query += " AND KANTORID = :KantorId";
                    arrayListParameters.Add(new OracleParameter("KantorId", kid));
                }
                else
                {
                    query += " AND KANTORID <> :KantorId";
                    arrayListParameters.Add(new OracleParameter("KantorId", kid));
                }
            }
            if (!string.IsNullOrEmpty(id))
            {
                if (cek)
                {
                    if (!OtorisasiUser.IsRoleAdministrator())
                    {
                        query += " AND unitkerjaid = :UnitKerjaId";
                        arrayListParameters.Add(new OracleParameter("UnitKerjaId", id));
                    }
                }
                else
                {
                    query += " AND unitkerjaid = :UnitKerjaId";
                    arrayListParameters.Add(new OracleParameter("UnitKerjaId", id));
                }
            }
            if (!String.IsNullOrEmpty(nama))
            {
                arrayListParameters.Add(new OracleParameter("NamaUnitKerja", String.Concat("%", nama.ToLower(), "%")));
                query += " AND LOWER(namaunitkerja) LIKE :NamaUnitKerja ";
            }
            if (!string.IsNullOrEmpty(induk))
            {
                query += " AND induk = :Induk";
                arrayListParameters.Add(new OracleParameter("Induk", induk));
            }

            if (aktif)
            {
                query += " AND tampil = 1 ";
            }

            query += "ORDER BY unitkerjaid";

            

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList<UnitKerja>();
            }

            return records;
        }

        public List<Kantor> GetKantor()
        {
            List<Kantor> records = new List<Kantor>();

            string query = "SELECT kantorid, nama NamaKantor FROM kantor WHERE tipekantorid in (2,3,4) AND validsampai IS NULL ORDER BY kode";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Kantor>(query).ToList<Kantor>();
            }

            return records;
        }

        public List<Profile> GetProfileDisposisi(string profileidtu, bool isprofilettd)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT 
                        profileid, nama NamaProfile, tipeeselonid
                  FROM  jabatan
                  WHERE profileidtu = :ProfileIdTU
                        AND profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100')
                        AND nama NOT IN ('Penyusun Tata Usaha','Pengadministrasi Umum','Pengelola Ketatausahaan') ";

            if (isprofilettd)
            {
                query += " and tipeeselonid <= 2 ";
            }
            else
            {
                query += " and tipeeselonid > 2 ";
            }
            query += "ORDER BY tipeeselonid, profileid";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new OracleParameter("ProfileIdTU", profileidtu));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfileDisposisiByProfileId(string pegawaiidtu, string kantorid, bool isprofilettd)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                        profileid, nama NamaProfile, tipeeselonid
                  FROM  jabatan
                  WHERE profileid in (
                    SELECT profileid FROM jabatanpegawai
                    WHERE profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100')
                    AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND pegawaiid = :PegawaiIdTu AND kantorid = :KantorId
                  ) ";

            if (isprofilettd)
            {
                query += " and tipeeselonid <= 2 ";
            }
            query += "ORDER BY tipeeselonid, profileid";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new OracleParameter("PegawaiIdTu", pegawaiidtu));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetListProfileDisposisi(string pegawaiidtu, string kantorid, bool OnlyEselonTop3)
        {
            List<Profile> records = new List<Profile>();

            List<Profile> listProfile = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                        profileid, nama NamaProfile, tipeeselonid, unitkerjaid
                  FROM  jabatan
                  WHERE profileidtu in (
                    SELECT profileid FROM jabatanpegawai
                    WHERE profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100')
                    AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND pegawaiid = :PegawaiIdTu AND kantorid = :KantorId
                  ) AND tipeeselonid IS NOT NULL 
                  ORDER BY tipeeselonid, profileid";

            query = sWhitespace.Replace(query, " ");
            arrayListParameters.Clear();
            arrayListParameters.Add(new OracleParameter("PegawaiIdTu", pegawaiidtu));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                listProfile = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();

                if (listProfile.Count > 0)
                {
                    string induk = listProfile[0].ProfileId;

                    query =
                        "SELECT " +
                        "      profileid, nama NamaProfile, tipeeselonid, unitkerjaid " +
                        "FROM  jabatan " +
                        "WHERE profileid <> induk AND tipeeselonid IS NOT NULL " +
                        "      AND profileid IN (SELECT profileid FROM jabatan START WITH profileid = :Induk CONNECT BY NOCYCLE PRIOR profileid = induk) ";

                    if (OnlyEselonTop3)
                    {
                        query += " AND tipeeselonid < 4 ";
                    }

                    query += "ORDER BY tipeeselonid, profileid";

                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("Induk", induk));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
                }
            }

            return records;
        }

        public List<Profile> GetProfileDisposisiByMyProfiles(string myProfiles, bool OnlyEselonTop3)
        {
            List<Profile> records = new List<Profile>();

            string query =
                        "SELECT " +
                        "      profileid, nama NamaProfile, tipeeselonid, unitkerjaid " +
                        "FROM  jabatan " +
                        "WHERE profileid <> induk AND profileid NOT IN (" + myProfiles + ") AND tipeeselonid IS NOT NULL " +
                        "      AND profileid IN (SELECT profileid FROM jabatan START WITH profileid IN (" + myProfiles + ") CONNECT BY NOCYCLE PRIOR profileid = induk) ";

            if (OnlyEselonTop3)
            {
                query += " AND tipeeselonid <= 4 ";
            }

            query += "ORDER BY tipeeselonid, profileid";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Profile>(query).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfilePPNPNByUnitKerja(string unitkerjaid)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT jabatan.profileid, jabatan.nama AS NamaProfile, '' as rolename, " +
                 "       ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL " +
                 "FROM jabatan " +
                 "WHERE jabatan.unitkerjaid = :UnitKerjaId AND UPPER(jabatan.nama) = 'PPNPN'";

            arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfilesByUnitKerja(string unitkerjaid)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT jabatan.profileid, jabatan.nama AS NamaProfile, " +
                 "       jabatan.nama || ' - ' || jabatan.profileid || decode(pegawai.nama, null, '', ' - ' || pegawai.nama) AS NamaProfilePlusID, " +
                 "       '' as rolename, " +
                 "       ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL " +
                 "FROM jabatan " +
                 "     LEFT JOIN jabatanpegawai ON jabatanpegawai.profileid = jabatan.profileid " +
                 "     LEFT JOIN pegawai ON pegawai.pegawaiid = jabatanpegawai.pegawaiid " +
                 "WHERE jabatan.unitkerjaid = :UnitKerjaId AND jabatan.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') " +
                 "ORDER BY jabatan.profileid";

            query = "SELECT profileid, nama AS NamaProfile FROM JABATAN WHERE UNITKERJAID = :UnitKerjaId AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') ORDER BY profileid"; // Arya :: 2020-07-22

            arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfilesPlusIDByUnitKerja(string unitkerjaid)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT jabatan.profileid, jabatan.nama AS NamaProfile, " +
                 "       jabatan.nama || ' - ' || jabatan.profileid || decode(pegawai.nama, null, '', ' - ' || pegawai.nama) AS NamaProfilePlusID, " +
                 "       '' as rolename, " +
                 "       ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL " +
                 "FROM jabatan " +
                 "     LEFT JOIN jabatanpegawai ON jabatanpegawai.profileid = jabatan.profileid AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0' " +
                 "     LEFT JOIN pegawai ON pegawai.pegawaiid = jabatanpegawai.pegawaiid " +
                 "WHERE jabatan.unitkerjaid = :UnitKerjaId AND jabatan.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') " +
                 "ORDER BY jabatan.profileid";
            arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfilesByNama(string namaprofile)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                 "SELECT jabatan.profileid, jabatan.nama AS NamaProfile, '' as rolename, " +
                 "       ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL " +
                 "FROM jabatan " +
                 "WHERE jabatan.profileid not like 'A%' " +
                 "      AND jabatan.profileid not like 'B%' " +
                 "      AND jabatan.profileid not like 'M%' " +
                 "      AND NVL(jabatan.jabatanlama,0) = 0 " +
                 "      AND LOWER(jabatan.nama) LIKE :NamaProfile " +
                 "ORDER BY jabatan.nama";

            arrayListParameters.Add(new OracleParameter("NamaProfile", String.Concat("%", namaprofile.ToLower(), "%")));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public string GetUnitKerjaIdByNip(string pegawaiid, string kantorid)
        {
            string result = "";

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT JB.UNITKERJAID
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
  	                JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID AND
                    UK.KANTORID = :KantorId
                WHERE
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  NVL(JP.STATUSHAPUS,'0') = '0' AND
                  JP.PEGAWAIID = :Nip
                ORDER BY JB.TIPEESELONID";

            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
            arrayListParameters.Add(new OracleParameter("Nip", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public ProfileTU GetProfileTUByNip(string pegawaiid, string kantorid)
        {
            var data = new ProfileTU();

            var arrayListParameters = new ArrayList();

            string query = @"
                SELECT JB.PROFILEIDTU
                FROM JABATAN JB
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PROFILEID = JB.PROFILEID  AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0' AND
                       JP.PEGAWAIID = :Nip AND
                       JP.KANTORID = :KantorId

                WHERE
                  JB.PROFILEIDTU IS NOT NULL AND
                  NVL(JB.SEKSIID,'XXX') <> 'A800'
                GROUP BY JB.PROFILEIDTU";

            arrayListParameters.Add(new OracleParameter("Nip", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<ProfileTU>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public Pegawai GetPegawaiByPegawaiId(string pegawaiid)
        {
            Pegawai data = new Pegawai();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    pegawai.pegawaiid, pegawai.nama, pegawai.nomorhp, users.email, " +
                "    NVL(v_pegawai_eoffice.namajabatan, pegawai.jabatan) AS jabatan, " +
                "    decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                "           decode(pegawai.nama, '', '', pegawai.nama) || " +
                "           decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap " +
                "FROM " +
                "    pegawai " +
                "    LEFT JOIN simpeg_2702.v_pegawai_eoffice ON v_pegawai_eoffice.nipbaru = pegawai.pegawaiid " + // jabatan db simpeg
                "    LEFT JOIN users ON users.userid = pegawai.userid " +
                "WHERE " +
                "    pegawai.pegawaiid = :PegawaiId ";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Pegawai>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public PegawaiSimpeg GetPegawaiSimpegByNip(string nip)
        {
            PegawaiSimpeg data = new PegawaiSimpeg();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    nipbaru pegawaiid, nama, nama_lengkap NamaLengkap, " +
                "    gelardepan, gelarbelakang, alamat, email, " +
                "    hp nomorhp, satkerid, satker namasatker, " +
                "    namajabatan " +
                "FROM " +
                "    siap_vw_pegawai " +
                "WHERE " +
                "    nipbaru = :Nip";


            arrayListParameters.Add(new OracleParameter("Nip", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<PegawaiSimpeg>(query, parameters).FirstOrDefault();


                #region Get KantorId Satker

                int lensatkerid = data.SatkerId.Length;
                for (int i = lensatkerid; i >= 4; i -= 2)
                {
                    string substrsatkerid = data.SatkerId.Substring(0, i);

                    query = "SELECT satker FROM simpeg_2702.satker WHERE satkerid = :SatkerId";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("SatkerId", substrsatkerid));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    string namasatker = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                    if (!string.IsNullOrEmpty(namasatker))
                    {
                        if (substrsatkerid.Length == 6)
                        {
                            query = "SELECT kantorid FROM unitkerja WHERE unitkerjaid = :SatkerId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("SatkerId", substrsatkerid));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            string kantoridsatker = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                            data.KantorId = kantoridsatker;

                            if (data.KantorId != "980FECFC746D8C80E0400B0A9214067D")
                            {
                                data.TipeKantorId = 3;

                                if (namasatker.ToLower().Contains("wilayah"))
                                {
                                    data.TipeKantorId = 2;
                                }
                            }
                        }
                    }

                    //if (substrsatkerid.Length == 6)
                    //{
                    //    query = "SELECT kantorid FROM unitkerja WHERE unitkerjaid = :SatkerId";
                    //    arrayListParameters.Clear();
                    //    arrayListParameters.Add(new OracleParameter("SatkerId", substrsatkerid));
                    //    parameters = arrayListParameters.OfType<object>().ToArray();
                    //    string kantoridsatker = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                    //    data.KantorId = kantoridsatker;
                    //}
                }

                #endregion

                // Get data KKP
                List<UserLogin> datauserlogin = GetUserLogin(nip, "", "", "", 1, 1);
                if (datauserlogin.Count > 0)
                {
                    data.NomorHPKKP = datauserlogin[0].NomorTelepon;
                    data.EmailKKP = datauserlogin[0].Email;
                    data.JabatanKKP = datauserlogin[0].Jabatan;
                    data.UserId = datauserlogin[0].UserId;
                    data.Username = datauserlogin[0].Username;
                    data.Password = datauserlogin[0].Password;
                }
            }

            return data;
        }

        public List<Pegawai> GetPegawaiByNama(string namapegawai)
        {
            List<Pegawai> records = new List<Pegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    ROW_NUMBER() over (ORDER BY pegawai.nama) RNumber, COUNT(1) OVER() TOTAL, " +
                "    pegawai.pegawaiid, pegawai.nama, NVL(v_pegawai_eoffice.namajabatan, pegawai.jabatan) AS jabatan, " +
                "    decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                "           decode(pegawai.nama, '', '', pegawai.nama) || " +
                "           decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap " +
                "FROM " +
                "    pegawai " +
                "    LEFT JOIN simpeg_2702.v_pegawai_eoffice ON v_pegawai_eoffice.nipbaru = pegawai.pegawaiid " + // jabatan db simpeg
                "WHERE " +
                "    LOWER(pegawai.nama) LIKE :NamaPegawai ";

            arrayListParameters.Add(new OracleParameter("NamaPegawai", String.Concat("%", namapegawai.ToLower(), "%")));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pegawai>(query, parameters).ToList<Pegawai>();
            }

            return records;
        }

        public List<Pegawai> GetPegawaiByUnitKerjaJabatanNama(string unitkerjaid, string namajabatan, string namapegawai)
        {
            List<Pegawai> records = new List<Pegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  ROW_NUMBER() OVER(ORDER BY TIPEUSER, TIPEESELONID, NAMA) AS RNUMBER,
                  COUNT(1) OVER() TOTAL,
                  PEGAWAIID,
                  NAMA,
                  PROFILEID,
                  JABATAN,
                  NAMALENGKAP,
                  TIPEESELONID,
                  NAMA || ', ' || JABATAN AS NAMADANJABATAN
                FROM
                  (SELECT
                     PG.PEGAWAIID,
                     PG.NAMA,
                     JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '') AS JABATAN,
                     DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, NULL, '', ', ' || PG.GELARBELAKANG) AS NAMALENGKAP,
                     JB.PROFILEID,
                     JB.TIPEESELONID, 0 AS TIPEUSER
                   FROM PEGAWAI PG
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PG.PEGAWAIID AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                     JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = JB.UNITKERJAID AND
                       UK.UNITKERJAID = :UNITKERJAID1
                   WHERE
                       (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                   UNION
                   SELECT
                     PP.NIK AS PEGAWAIID,
                     PP.NAMA,
                     JB.NAMA AS JABATAN,
                     PP.NAMA AS NAMALENGKAP,
                     JB.PROFILEID,
                     JB.TIPEESELONID,
                     1 AS TIPEUSER
                   FROM PPNPN PP
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PP.NIK AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       JB.PROFILEID = JP.PROFILEID  AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                     JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = JB.UNITKERJAID AND
                       UK.UNITKERJAID = :UNITKERJAID2)
                WHERE
                  PEGAWAIID IS NOT NULL";
                //"SELECT " +
                //"    ROW_NUMBER() over(ORDER BY tipeuser, tipeeselonid, nama) RNumber, COUNT(1) OVER() TOTAL, " +
                //"    pegawaiid, nama, profileid, jabatan, namalengkap, tipeeselonid, nama || ', ' || jabatan AS NamaDanJabatan " +
                //"FROM ( " +
                //"SELECT " +
                //"    pegawai.pegawaiid, pegawai.nama, " +
                ////"    NVL(siap_vw_pegawai.namajabatan, pegawai.jabatan) AS jabatan, " +
                //"    jabatan.nama || decode(jabatanpegawai.statusplt, 1, ' (PLT)', '') AS jabatan, " +
                //"    decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                //"           decode(pegawai.nama, '', '', pegawai.nama) || " +
                //"           decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap, " +
                //"    jabatan.profileid, jabatan.tipeeselonid, 0 AS tipeuser " +
                //"FROM " +
                //"    pegawai " +
                ////"    LEFT JOIN siap_vw_pegawai ON siap_vw_pegawai.nipbaru = pegawai.pegawaiid " + // jabatan db simpeg
                //"    JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = pegawai.pegawaiid " +
                //"        AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0' " +
                //"    JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
                //"    JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid AND unitkerja.unitkerjaid = :UnitKerjaId1 " +
                //"UNION " +
                //"SELECT " +
                //"    ppnpn.nik AS pegawaiid, ppnpn.nama, 'PPNPN' AS jabatan, " +
                //"    ppnpn.nama AS NamaLengkap, jabatan.profileid, jabatan.tipeeselonid, 1 AS tipeuser " +
                //"FROM " +
                //"    ppnpn " +
                //"    JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = ppnpn.nik " +
                //"        AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0' " +
                //"    JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
                //"    JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid AND unitkerja.unitkerjaid = :UnitKerjaId2 " +
                //"    ) " +
                //"WHERE pegawaiid IS NOT NULL ";

            arrayListParameters.Add(new OracleParameter("UnitKerjaId1", unitkerjaid));
            arrayListParameters.Add(new OracleParameter("UnitKerjaId2", unitkerjaid));

            if (!String.IsNullOrEmpty(namajabatan))
            {
                arrayListParameters.Add(new OracleParameter("NamaJabatan", String.Concat("%", namajabatan.ToLower(), "%")));
                query += " AND LOWER(jabatan) LIKE :NamaJabatan ";
            }
            if (!String.IsNullOrEmpty(namapegawai))
            {
                arrayListParameters.Add(new OracleParameter("NamaPegawai", String.Concat("%", namapegawai.ToLower(), "%")));
                query += " AND LOWER(nama) LIKE :NamaPegawai ";
            }

            query += " ORDER BY tipeuser, tipeeselonid, nama ";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pegawai>(query, parameters).ToList<Pegawai>();
            }

            return records;
        }

        public List<Pegawai> GetPegawaiByProfileId(string profileid, string pegawaiid = null)
        {
            List<Pegawai> records = new List<Pegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT pegawai.pegawaiid, pegawai.nama, jabatan.nama as jabatan,
                         decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') ||
                         decode(pegawai.nama, '', '', pegawai.nama) ||
                         decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap
                  FROM jabatanpegawai
                       JOIN pegawai ON pegawai.pegawaiid = jabatanpegawai.pegawaiid
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                            AND jabatan.profileid not like 'A%' 
                            AND jabatan.profileid not like 'B%' 
                            AND jabatan.profileid not like 'M%' 
                            AND NVL(jabatan.jabatanlama,0) = 0
                  WHERE jabatanpegawai.profileid = :ProfileId
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  UNION
                  SELECT ppnpn.nik AS pegawaiid, ppnpn.nama, 'PPNPN' AS jabatan,
                         ppnpn.nama AS NamaLengkap
                  FROM jabatanpegawai
                       JOIN ppnpn ON ppnpn.nik = jabatanpegawai.pegawaiid
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                            AND jabatan.profileid not like 'A%'
                            AND jabatan.profileid not like 'B%'
                            AND jabatan.profileid not like 'M%'
                            AND NVL(jabatan.jabatanlama,0) = 0
                  WHERE jabatanpegawai.profileid = :ProfileId2
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'";

            query = sWhitespace.Replace(query, " ");
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));
            arrayListParameters.Add(new OracleParameter("ProfileId2", profileid));
            if (!string.IsNullOrEmpty(pegawaiid))
            {
                query += " AND ppnpn.nik = :PegawaiId";
                arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            }
            query += " ORDER BY nama";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pegawai>(query, parameters).ToList<Pegawai>();
            }

            return records;
        }

        public List<Profile> GetProfilesByPegawaiId(string pegawaiid, string kantorid)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatan.profileid, jabatan.nama NamaProfile
                  FROM jabatanpegawai
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                  WHERE jabatanpegawai.pegawaiid = :PegawaiId AND jabatanpegawai.kantorid = :KantorId
                        AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100')
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  ORDER BY jabatan.nama";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetAllProfilesByPegawaiId(string pegawaiid, string kantorid)
        {
            List<Profile> records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatan.profileid, jabatan.nama NamaProfile
                  FROM jabatanpegawai
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                  WHERE jabatanpegawai.pegawaiid = :PegawaiId AND jabatanpegawai.kantorid = :KantorId
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  ORDER BY jabatan.nama";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList<Profile>();
            }

            return records;
        }

        public ProfilePegawai GetProfilePegawaiByPrimaryKey(string id)
        {
            ProfilePegawai data = new ProfilePegawai();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatanpegawai.profilepegawaiid, jabatanpegawai.profileid, jabatanpegawai.pegawaiid,
                         jabatan.nama AS NAMAPROFILE, pegawai.nama as namapegawai, pegawai.nomorhp as nomortelepon, users.email
                  FROM jabatanpegawai, jabatan, pegawai, users
                  WHERE 
                       jabatan.profileid = jabatanpegawai.profileid
                       AND pegawai.pegawaiid = jabatanpegawai.pegawaiid
                       AND users.userid = pegawai.userid
                       AND jabatanpegawai.profilepegawaiid = :Id
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  ORDER BY jabatan.nama";

            arrayListParameters.Add(new OracleParameter("Id", id));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public List<ProfilePegawai> GetAllProfilePegawai(string pegawaiid)
        {
            List<ProfilePegawai> records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatanpegawai.profilepegawaiid, jabatanpegawai.profileid, jabatanpegawai.pegawaiid, 
                         jabatanpegawai.statusplt, decode(jabatanpegawai.statusplt, 1, 'PLT', '-') AS IsStatusPlt,
                         jabatan.nama AS NamaProfile, KANTOR.NAMA AS NAMAKANTOR,
                         ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL
                  FROM jabatanpegawai
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                       JOIN KANTOR ON KANTOR.KANTORID = JABATANPEGAWAI.KANTORID
                  WHERE jabatanpegawai.pegawaiid = :PegawaiId
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  ORDER BY jabatan.nama";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).ToList<ProfilePegawai>();
            }

            return records;
        }

        public List<ProfilePegawai> GetJabatanPegawai(string pegawaiid, string kantorid)
        {
            List<ProfilePegawai> records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatanpegawai.profilepegawaiid, jabatanpegawai.profileid, jabatanpegawai.pegawaiid, 
                         jabatanpegawai.statusplt, decode(jabatanpegawai.statusplt, 1, 'PLT', '-') AS IsStatusPlt,
                         jabatan.nama AS NamaProfile, jabatan.nama || decode(jabatanpegawai.statusplt, 1, ' (PLT)', '') AS NamaProfileLengkap, 
                         ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL
                  FROM jabatanpegawai
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                  WHERE jabatanpegawai.pegawaiid = :PegawaiId
                        AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND (JABATANPEGAWAI.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JABATANPEGAWAI.STATUSHAPUS,'0') = '0'";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            if (!string.IsNullOrEmpty(kantorid))
            {
                query += " AND JABATANPEGAWAI.KANTORID = :KantorId";
                arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
            }
            query += " ORDER BY jabatan.nama";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).ToList<ProfilePegawai>();
            }

            return records;
        }

        public List<ProfilePegawai> GetJabatanPegawaiKantor(string kantorid, string pegawaiid)
        {
            List<ProfilePegawai> records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatanpegawai.profilepegawaiid, jabatanpegawai.profileid, jabatanpegawai.pegawaiid, 
                         jabatanpegawai.statusplt, decode(jabatanpegawai.statusplt, 1, 'PLT', '-') AS IsStatusPlt,
                         jabatan.nama AS NamaProfile, jabatan.nama || decode(jabatanpegawai.statusplt, 1, ' (PLT)', '') AS NamaProfileLengkap, 
                         ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL
                  FROM jabatanpegawai
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                  WHERE jabatanpegawai.pegawaiid = :PegawaiId
                        AND jabatanpegawai.kantorid = :KantorId
                        AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') 
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  ORDER BY jabatan.nama";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).ToList<ProfilePegawai>();
            }

            return records;
        }

        public List<ProfilePegawai> GetProfilePegawai(string pegawaiid, string kantorid)
        {
            List<ProfilePegawai> records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT jabatanpegawai.profilepegawaiid, jabatanpegawai.profileid, jabatanpegawai.pegawaiid, jabatan.nama AS NamaProfile,
                         ROW_NUMBER() over (ORDER BY jabatan.nama) RNumber, COUNT(1) OVER() TOTAL
                  FROM jabatanpegawai
                       JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
                  WHERE jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') 
                        AND jabatanpegawai.profileid not like 'M%' 
                        AND jabatanpegawai.pegawaiid = :PegawaiId AND jabatanpegawai.kantorid = :KantorId
                        AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
                  ORDER BY jabatan.nama";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).ToList<ProfilePegawai>();
            }

            return records;
        }


        #region Profile Flow

        public List<Profile> GetProfileKantah()
        {
            List<Profile> records = new List<Profile>();

            string query =
                "SELECT " +
                "    jabatan.profileid, jabatan.nama AS NamaProfile, jabatan.nama || ' - ' || jabatan.profileid AS NamaProfilePlusID, " +
                "    0 AS Pilih, " +
                "    ROW_NUMBER() over (ORDER BY jabatan.profileid) RNumber, COUNT(1) OVER() TOTAL " +
                "FROM " +
                "    jabatan " +
                "WHERE jabatan.profileid like 'N%' ORDER BY jabatan.profileid";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Profile>(query).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfileKanwil()
        {
            List<Profile> records = new List<Profile>();

            string query =
                "SELECT " +
                "    jabatan.profileid, jabatan.nama AS NamaProfile, jabatan.nama || ' - ' || jabatan.profileid AS NamaProfilePlusID, " +
                "    0 AS Pilih, " +
                "    ROW_NUMBER() over (ORDER BY jabatan.profileid) RNumber, COUNT(1) OVER() TOTAL " +
                "FROM " +
                "    jabatan " +
                "WHERE jabatan.profileid like 'R%' ORDER BY jabatan.profileid";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Profile>(query).ToList<Profile>();
            }

            return records;
        }

        public List<Profile> GetProfilePusat()
        {
            List<Profile> records = new List<Profile>();

            string query =
                "SELECT " +
                "    jabatan.profileid, jabatan.nama AS NamaProfile, jabatan.nama || ' - ' || jabatan.profileid AS NamaProfilePlusID, " +
                "    0 AS Pilih, " +
                "    ROW_NUMBER() over (ORDER BY jabatan.profileid) RNumber, COUNT(1) OVER() TOTAL " +
                "FROM " +
                "    jabatan " +
                "WHERE " +
                "    jabatan.profileid like 'C%' " +
                "    OR jabatan.profileid like 'D%' " +
                "    OR jabatan.profileid like 'E%' " +
                "    OR jabatan.profileid like 'F%' " +
                "    OR jabatan.profileid like 'G%' " +
                "    OR jabatan.profileid like 'H%' " +
                "ORDER BY jabatan.profileid";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Profile>(query).ToList<Profile>();
            }

            return records;
        }

        //public List<ProfileFlow> GetProfileFlow(string kantorid, string namaprofiledari, string namaprofiletujuan, int from, int to)
        //{
        //    List<ProfileFlow> records = new List<ProfileFlow>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        "SELECT * FROM ( " +
        //        "    SELECT " +
        //        "        ROW_NUMBER() over (ORDER BY profileflow.profiledari, profileflow.profiletujuan) RNUMBER, COUNT(1) OVER() TOTAL, " +
        //        "        profileflow.profileflowid, profileflow.kantorid, profileflow.profiledari, profileflow.profiletujuan, " +
        //        "        profile1.nama AS NamaProfileDari, profile2.nama AS NamaProfileTujuan " +
        //        "    FROM " +
        //        "        profileflow, profile profile1, profile profile2 " +
        //        "    WHERE " +
        //        "        profileflow.kantorid = :KantorId " +
        //        "        AND profile1.profileid = profileflow.profiledari " + 
        //        "        AND profile2.profileid = profileflow.profiletujuan ";

        //    arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

        //    if (!String.IsNullOrEmpty(namaprofiledari))
        //    {
        //        arrayListParameters.Add(new OracleParameter("NamaProfileDari", String.Concat("%", namaprofiledari.ToLower(), "%")));
        //        query += " AND LOWER(profile1.nama) LIKE :NamaProfileDari ";
        //    }
        //    if (!String.IsNullOrEmpty(namaprofiletujuan))
        //    {
        //        arrayListParameters.Add(new OracleParameter("NamaProfileTujuan", String.Concat("%", namaprofiletujuan.ToLower(), "%")));
        //        query += " AND LOWER(profile2.nama) LIKE :NamaProfileTujuan ";
        //    }

        //    query +=
        //        " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new OracleParameter("limitCnt", to));

        //    using (var ctx = new BpnDbContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<ProfileFlow>(query, parameters).ToList<ProfileFlow>();
        //    }

        //    return records;
        //}

        //public int JumlahProfileFlow(string profiledari, string profiletujuan, string kantorid)
        //{
        //    int result = 0;

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query = "SELECT count(*) FROM profileflow WHERE profiledari = :ProfileDari AND profiletujuan = :ProfileTujuan AND kantorid = :KantorId ";

        //    arrayListParameters.Add(new OracleParameter("ProfileDari", profiledari));
        //    arrayListParameters.Add(new OracleParameter("ProfileTujuan", profiletujuan));
        //    arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

        //    using (var ctx = new BpnDbContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        result = ctx.Database.SqlQuery<int>(query, parameters).First();
        //    }

        //    return result;
        //}

        //        public TransactionResult SimpanProfileFlow(ProfileFlow profileflow)
        //        {
        //            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

        //            Regex sWhitespace = new Regex(@"\s+");

        //            string sql = "";
        //            ArrayList arrayListParameters = new ArrayList();
        //            object[] parameters = null;

        //            using (var ctx = new BpnDbContext())
        //            {
        //                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //                {
        //                    try
        //                    {
        //                        string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();

        //                        sql =
        //                            @"INSERT INTO profileflow (
        //                                profileflowid, profiledari, profiletujuan, kantorid, aktif) VALUES 
        //                                (
        //                                    :Id, :ProfileDari, :ProfileTujuan, :KantorId, 1)";

        //                        sql = sWhitespace.Replace(sql, " ");

        //                        profileflow.ProfileFlowId = id;

        //                        arrayListParameters.Clear();
        //                        arrayListParameters.Add(new OracleParameter("Id", id));
        //                        arrayListParameters.Add(new OracleParameter("ProfileDari", profileflow.ProfileDari));
        //                        arrayListParameters.Add(new OracleParameter("ProfileTujuan", profileflow.ProfileTujuan));
        //                        arrayListParameters.Add(new OracleParameter("KantorId", profileflow.KantorId));
        //                        parameters = arrayListParameters.OfType<object>().ToArray();
        //                        ctx.Database.ExecuteSqlCommand(sql, parameters);

        //                        tc.Commit();
        //                        tr.Status = true;
        //                        tr.ReturnValue = profileflow.ProfileFlowId;
        //                        tr.Pesan = "Data berhasil disimpan";
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        tc.Rollback();
        //                        tr.Pesan = ex.Message.ToString();
        //                    }
        //                    finally
        //                    {
        //                        tc.Dispose();
        //                        ctx.Dispose();
        //                    }
        //                }
        //            }


        //            return tr;
        //        }

        //public TransactionResult HapusProfileFlowById(string id)
        //{
        //    TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

        //    using (var ctx = new BpnDbContext())
        //    {
        //        using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                ArrayList arrayListParameters = new ArrayList();

        //                string sql = @"DELETE FROM profileflow WHERE profileflowid = :Id";
        //                arrayListParameters.Clear();
        //                arrayListParameters.Add(new OracleParameter("Id", id));
        //                object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //                ctx.Database.ExecuteSqlCommand(sql, parameters);

        //                tc.Commit();
        //                tr.Status = true;
        //                tr.Pesan = "Data berhasil dihapus";
        //            }
        //            catch (Exception ex)
        //            {
        //                tc.Rollback();
        //                tr.Pesan = ex.Message.ToString();
        //            }
        //            finally
        //            {
        //                tc.Dispose();
        //                ctx.Dispose();
        //            }
        //        }
        //    }

        //    return tr;
        //}

        #endregion


        #region Jabatan

        public List<ListJabatan> GetJabatan(string profileid, string namaprofile, string unitkerjaid, int from, int to)
        {
            List<ListJabatan> records = new List<ListJabatan>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY profileutama.profileid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        profileutama.profileid, profileutama.nama NamaProfile, " + 
                "        profiletu.profileid ProfileIdTU, profiletu.nama NamaProfileTU, " +
                "        profileba.profileid ProfileIdBA, profileba.nama NamaProfileBA, " +
                "        unitkerjautama.unitkerjaid, unitkerjautama.namaunitkerja, " +
                "        unitkerjatu.unitkerjaid UnitKerjaIdTU, unitkerjatu.namaunitkerja NamaUnitKerjaTU, " +
                "        unitkerjaba.unitkerjaid UnitKerjaIdBA, unitkerjaba.namaunitkerja NamaUnitKerjaBA " +
                "    FROM " +
                "        jabatan profileutama " +
                "        LEFT JOIN jabatan profiletu ON profiletu.profileid = profileutama.profileidtu " +
                "        LEFT JOIN jabatan profileba ON profileba.profileid = profileutama.profileidba " +
                "        LEFT JOIN unitkerja unitkerjautama ON unitkerjautama.unitkerjaid = profileutama.unitkerjaid " +
                "        LEFT JOIN unitkerja unitkerjatu ON unitkerjatu.unitkerjaid = profiletu.unitkerjaid " +
                "        LEFT JOIN unitkerja unitkerjaba ON unitkerjaba.unitkerjaid = profileba.unitkerjaid " +
                "    WHERE " +
                "        profileutama.profileid IS NOT NULL " +
                "        AND (profileutama.VALIDSAMPAI IS NULL OR TRUNC(CAST(profileutama.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) " +
                "        AND profileutama.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') ";

            
            if (!String.IsNullOrEmpty(profileid))
            {
                arrayListParameters.Add(new OracleParameter("ProfileId", String.Concat(profileid.ToLower(), "%")));
                query += " AND LOWER(profileutama.profileid) LIKE :ProfileId ";
            }
            if (!String.IsNullOrEmpty(namaprofile))
            {
                arrayListParameters.Add(new OracleParameter("NamaProfile", String.Concat("%", namaprofile.ToLower(), "%")));
                query += " AND LOWER(profileutama.nama) LIKE :NamaProfile ";
            }
            if (!String.IsNullOrEmpty(unitkerjaid))
            {
                arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));
                query += " AND unitkerjautama.unitkerjaid = :UnitKerjaId ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ListJabatan>(query, parameters).ToList<ListJabatan>();
            }

            return records;
        }

        public TransactionResult UpdateJabatan(ListJabatan data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (string.IsNullOrEmpty(data.ProfileId))
                        {
                            // Insert Jabatan
                            if (!string.IsNullOrEmpty(data.NewProfileId))
                            {
                                sql =
                                    "INSERT INTO jabatan ( " +
                                    "            profileid, nama, unitkerjaid, profileidtu, profileidba, validsejak, " +
                                    "            userupdate, lastupdate) VALUES " +
                                    "( " +
                                    "            :ProfileId,:NamaProfile,:UnitKerjaId,:ProfileIdTU,:ProfileIdBA,SYSDATE, " +
                                    "            :UserUpdate,SYSDATE)";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("ProfileId", data.NewProfileId));
                                arrayListParameters.Add(new OracleParameter("NamaProfile", data.NamaProfile));
                                arrayListParameters.Add(new OracleParameter("UnitKerjaId", data.UnitKerjaId));
                                arrayListParameters.Add(new OracleParameter("ProfileIdTU", data.ProfileIdTU));
                                arrayListParameters.Add(new OracleParameter("ProfileIdBA", data.ProfileIdBA));
                                arrayListParameters.Add(new OracleParameter("UserUpdate", data.UserId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }
                        else
                        {
                            // Update Jabatan
                            sql =
                                "UPDATE jabatan SET " +
                                "       nama = :NamaProfile, " +
                                "       unitkerjaid = :UnitKerjaId, " +
                                "       profileidtu = :ProfileIdTU, " +
                                "       profileidba = :ProfileIdBA, " +
                                "       lastupdate = SYSDATE, " +
                                "       userupdate = :UserUpdate " +
                                "WHERE profileid = :ProfileId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("NamaProfile", data.NamaProfile));
                            arrayListParameters.Add(new OracleParameter("UnitKerjaId", data.UnitKerjaId));
                            arrayListParameters.Add(new OracleParameter("ProfileIdTU", data.ProfileIdTU));
                            arrayListParameters.Add(new OracleParameter("ProfileIdBA", data.ProfileIdBA));
                            arrayListParameters.Add(new OracleParameter("UserUpdate", data.UserId));
                            arrayListParameters.Add(new OracleParameter("ProfileId", data.ProfileId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
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

        #endregion


        #region Asal Surat

        public List<AsalSurat> GetAsalSurat()
        {
            var list = new List<AsalSurat>();

            string query =
                "select " +
                "    namaasalsurat AS value, namaasalsurat AS data " +
                "from " + OtorisasiUser.NamaSkemaMaster + ".asalsurat where status = 1 order by namaasalsurat";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<AsalSurat>(query).ToList();
            }

            return list;
        }

        public List<ListAsalSurat> GetListAsalSurat(string namaasalsurat, int from, int to)
        {
            List<ListAsalSurat> records = new List<ListAsalSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY asalsurat.namaasalsurat) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        asalsurat.namaasalsurat " +
                "    FROM " +
                "        " + OtorisasiUser.NamaSkemaMaster + ".asalsurat " +
                "    WHERE " +
                "        asalsurat.status = 1 ";


            if (!String.IsNullOrEmpty(namaasalsurat))
            {
                arrayListParameters.Add(new OracleParameter("NamaAsalSurat", String.Concat("%", namaasalsurat.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(asalsurat.namaasalsurat)) LIKE :NamaAsalSurat ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ListAsalSurat>(query, parameters).ToList<ListAsalSurat>();
            }

            return records;
        }

        public TransactionResult InsertAsalSurat(ListAsalSurat data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        object[] parameters = null;

                        string sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkemaMaster + ".asalsurat ( " +
                            "            namaasalsurat, status) VALUES " +
                            "( " +
                            "            :NamaAsalSurat,1)";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("NamaAsalSurat", data.NamaAsalSurat));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Data Asal Surat " + data.NamaAsalSurat + " sudah ada.";
                        }
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

        public TransactionResult HapusAsalSurat(string namaasalsurat)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + OtorisasiUser.NamaSkemaMaster + ".asalsurat WHERE namaasalsurat = :NamaAsalSurat";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("NamaAsalSurat", namaasalsurat));
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

        #endregion

        #region SifatSurat
        public List<SifatSurat> GetListSifatSurat(string namasifatsurat, int from, int to)
        {
            List<SifatSurat> records = new List<SifatSurat>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY sifatsurat.urutan) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        sifatsurat.nama as namasifatsurat, sifatsurat.urutan, sifatsurat.prioritas " +
                "    FROM " +
                "        " + OtorisasiUser.NamaSkema + ".sifatsurat " +
                "    WHERE " +
                "        sifatsurat.aktif = 1 ";


            if (!String.IsNullOrEmpty(namasifatsurat))
            {
                arrayListParameters.Add(new OracleParameter("NamaSifatSurat", String.Concat("%", namasifatsurat.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(asalsurat.nama)) LIKE :NamaSifatSurat ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<SifatSurat>(query, parameters).ToList();
            }

            return records;
        }

        public TransactionResult InsertSifatSurat(SifatSurat data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        object[] parameters = null;

                        string sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".sifatsurat ( " +
                            "            nama, aktif, urutan, prioritas) VALUES " +
                            "( " +
                            "            :NamaSifatSurat,1,:urutan,:prioritas)";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("NamaSifatSurat", data.NamaSifatSurat));
                        arrayListParameters.Add(new OracleParameter("urutan", data.Urutan));
                        arrayListParameters.Add(new OracleParameter("prioritas", data.Prioritas));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Data Sifat Surat sudah ada.";
                        }
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

        public TransactionResult HapusSifatSurat(string namasifatsurat)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + OtorisasiUser.NamaSkema + ".sifatsurat WHERE nama = :namasifatsurat";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("namasifatsurat", namasifatsurat));
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
        #endregion

        #region Klasifikasi Arsip

        public List<KlasifikasiArsip> GetKodeKlasifikasiArsip()
        {
            var list = new List<KlasifikasiArsip>();

            string query =
                "select " +
                "    kodeklasifikasi, jenisarsip, keterangan " +
                "from " + OtorisasiUser.NamaSkemaMaster + ".klasifikasiarsip order by kodeklasifikasi";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<KlasifikasiArsip>(query).ToList();
            }

            return list;
        }

        public List<KlasifikasiArsip> GetListKlasifikasiArsip()
        {
            var list = new List<KlasifikasiArsip>();

            string query =
                "select " +
                "    kodeklasifikasi, jenisarsip " +
                "from " + OtorisasiUser.NamaSkemaMaster + ".klasifikasiarsip order by kodeklasifikasi";

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<KlasifikasiArsip>(query).ToList();
            }

            return list;
        }

        public List<KlasifikasiArsip> GetKlasifikasiArsip(string kodeklasifikasi, string jenisarsip, string keterangan, int from, int to)
        {
            List<KlasifikasiArsip> records = new List<KlasifikasiArsip>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY klasifikasiarsip.kodeklasifikasi) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        klasifikasiarsip.kodeklasifikasi, klasifikasiarsip.jenisarsip, klasifikasiarsip.keterangan " +
                "    FROM " +
                "        " + OtorisasiUser.NamaSkemaMaster + ".klasifikasiarsip " +
                "    WHERE " +
                "        klasifikasiarsip.kodeklasifikasi IS NOT NULL ";

            if (!String.IsNullOrEmpty(kodeklasifikasi))
            {
                arrayListParameters.Add(new OracleParameter("KodeKlasifikasi", string.Concat(kodeklasifikasi.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(klasifikasiarsip.kodeklasifikasi)) LIKE :KodeKlasifikasi ";
            }

            if (!String.IsNullOrEmpty(jenisarsip))
            {
                arrayListParameters.Add(new OracleParameter("JenisArsip", string.Concat("%", jenisarsip.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(klasifikasiarsip.jenisarsip)) LIKE :JenisArsip ";
            }

            if (!String.IsNullOrEmpty(keterangan))
            {
                arrayListParameters.Add(new OracleParameter("Keterangan", string.Concat("%", keterangan.ToLower(), "%")));
                query += " AND LOWER(APEX_UTIL.URL_ENCODE(klasifikasiarsip.keterangan)) LIKE :Keterangan ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<KlasifikasiArsip>(query, parameters).ToList<KlasifikasiArsip>();
            }

            return records;
        }

        public TransactionResult InsertKlasifikasiArsip(KlasifikasiArsip data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        object[] parameters = null;

                        string sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkemaMaster + ".klasifikasiarsip ( " +
                            "            kodeklasifikasi, jenisarsip, keterangan) VALUES " +
                            "( " +
                            "            :KodeKlasifikasi, :JenisArsip, :Keterangan)";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("KodeKlasifikasi", data.KodeKlasifikasi.ToUpper()));
                        arrayListParameters.Add(new OracleParameter("JenisArsip", data.JenisArsip));
                        arrayListParameters.Add(new OracleParameter("Keterangan", data.Keterangan));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToLower().Contains("unique constraint"))
                        {
                            tr.Pesan = "Data Klasifikasi Arsip " + data.KodeKlasifikasi + " sudah ada.";
                        }
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

        public TransactionResult HapusKlasifikasiArsip(string kodeklasifikasi)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + OtorisasiUser.NamaSkemaMaster + ".klasifikasiarsip WHERE kodeklasifikasi = :KodeKlasifikasi";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("KodeKlasifikasi", kodeklasifikasi));
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

        #endregion


        #region Profile Tata Usaha

        public List<ProfileTataUsaha> GetProfileTataUsaha(int tipekantorid, string profileid, string namaprofile, string namaprofiletu, int from, int to)
        {
            List<ProfileTataUsaha> records = new List<ProfileTataUsaha>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY profileutama.profileid, profiletu.profileid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        profileutama.profileid, profileutama.nama NamaProfile, profiletu.profileid ProfileIdTU, profiletu.nama NamaProfileTU " +
                "    FROM " +
                "        jabatan profileutama " +
                "        LEFT JOIN jabatan profiletu ON profiletu.profileid = profileutama.profileidtu " +
                "    WHERE " +
                "        profileutama.profileid IS NOT NULL ";

            //string query =
            //    "SELECT * FROM ( " +
            //    "    SELECT " +
            //    "        ROW_NUMBER() over (ORDER BY profileutama.profileid, profiletu.profileid) RNUMBER, COUNT(1) OVER() TOTAL, " +
            //    "        profileutama.profileid, profileutama.nama NamaProfile, profiletu.profileid ProfileIdTU, profiletu.nama NamaProfileTU " +
            //    "    FROM " +
            //    "        jabatan profileutama " +
            //    "        LEFT JOIN jabatan profiletu ON profiletu.profileid = profileutama.profileidtu " +
            //    "    WHERE " +
            //    "        profileutama.profileid IS NOT NULL " +
            //    "        AND (profileutama.profileid like 'C%' " +
            //    "             OR profileutama.profileid like 'D%' " +
            //    "             OR profileutama.profileid like 'E%' " +
            //    "             OR profileutama.profileid like 'F%' " +
            //    "             OR profileutama.profileid like 'G%' " +
            //    "             OR profileutama.profileid like 'H%' " +
            //    "             OR profileutama.profileid like 'R%' " +
            //    "             OR profileutama.profileid like 'N%') ";

            //if (tipekantorid == 1)
            //{
            //    query +=
            //        "AND (profileutama.profileid like 'C%' " +
            //        "  OR profileutama.profileid like 'D%' " +
            //        "  OR profileutama.profileid like 'E%' " +
            //        "  OR profileutama.profileid like 'F%' " +
            //        "  OR profileutama.profileid like 'G%') ";
            //}
            //else if (tipekantorid == 2)
            //{
            //    query += "AND profileutama.profileid like 'R%'";
            //}
            //else if (tipekantorid == 3 || tipekantorid == 4)
            //{
            //    query += "AND profileutama.profileid like 'N%'";
            //}

            if (!String.IsNullOrEmpty(profileid))
            {
                arrayListParameters.Add(new OracleParameter("ProfileId", String.Concat(profileid.ToLower(), "%")));
                query += " AND LOWER(profileutama.profileid) LIKE :ProfileId ";
            }
            if (!String.IsNullOrEmpty(namaprofile))
            {
                arrayListParameters.Add(new OracleParameter("NamaProfile", String.Concat("%", namaprofile.ToLower(), "%")));
                query += " AND LOWER(profileutama.nama) LIKE :NamaProfile ";
            }
            if (!String.IsNullOrEmpty(namaprofiletu))
            {
                arrayListParameters.Add(new OracleParameter("NamaProfileTu", String.Concat("%", namaprofiletu.ToLower(), "%")));
                query += " AND LOWER(profiletu.nama) LIKE :NamaProfileTu ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfileTataUsaha>(query, parameters).ToList<ProfileTataUsaha>();
            }

            return records;
        }

        public TransactionResult UpdateProfileTU(ProfileTataUsaha data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        sql = @"UPDATE jabatan set profileidtu = :ProfileIdTU where profileid = :ProfileId";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("ProfileIdTU", data.ProfileIdTU));
                        arrayListParameters.Add(new OracleParameter("ProfileId", data.ProfileId));
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

        #endregion


        #region Unit Kerja

        public List<UnitKerja> GetUnitKerja(string unitkerjaid, string namaunitkerja, string kode, bool issatkerpusat, bool issatkerkanwil, bool issatkerkantah, string tampil, int from, int to)
        {
            List<UnitKerja> records = new List<UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY unitkerja.unitkerjaid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        unitkerja.unitkerjaid, unitkerja.namaunitkerja, unitkerja.induk, unitkerja.kode, unitkerja.kantorid, unitkerja.tampil " +
                "    FROM " +
                "        unitkerja " +
                "    WHERE " +
                "        unitkerja.unitkerjaid IS NOT NULL ";


            #region Filter Jenis Kantor

            string filtertingkatsatker = "(";

            if (issatkerpusat)
            {
                filtertingkatsatker += "1,";
            }
            if (issatkerkanwil)
            {
                filtertingkatsatker += "2,";
            }
            if (issatkerkantah)
            {
                filtertingkatsatker += "3,";
            }
            filtertingkatsatker = filtertingkatsatker.Substring(0, filtertingkatsatker.Length - 1);

            if (!string.IsNullOrEmpty(filtertingkatsatker))
            {
                filtertingkatsatker += ")";

                query += " AND unitkerja.tipekantorid IN " + filtertingkatsatker + " ";
            }

            #endregion


            if (!String.IsNullOrEmpty(unitkerjaid))
            {
                arrayListParameters.Add(new OracleParameter("UnitKerjaId", String.Concat(unitkerjaid.ToLower(), "%")));
                query += " AND unitkerja.unitkerjaid LIKE :UnitKerjaId ";
            }
            if (!String.IsNullOrEmpty(namaunitkerja))
            {
                arrayListParameters.Add(new OracleParameter("NamaUnitKerja", String.Concat("%", namaunitkerja.ToLower(), "%")));
                query += " AND LOWER(unitkerja.namaunitkerja) LIKE :NamaUnitKerja ";
            }
            if (!String.IsNullOrEmpty(kode))
            {
                arrayListParameters.Add(new OracleParameter("Kode", String.Concat(kode, "%")));
                query += " AND unitkerja.kode LIKE :Kode ";
            }
            if (!String.IsNullOrEmpty(tampil))
            {
                arrayListParameters.Add(new OracleParameter("Tampil", Convert.ToInt32(tampil)));
                query += " AND unitkerja.tampil = :Tampil ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList<UnitKerja>();
            }

            return records;
        }

        public TransactionResult UpdateStatusUnitKerja(string unitkerjaid, string tampil)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        sql = @"UPDATE unitkerja set tampil = :Tampil where unitkerjaid = :Id";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("Tampil", Convert.ToInt32(tampil)));
                        arrayListParameters.Add(new OracleParameter("Id", unitkerjaid));
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

        public TransactionResult UpdateUnitKerja(string unitkerjaid, string namaunitkerja, string kode, string tampil, string induk = null)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        sql = 
                            "UPDATE unitkerja SET " +
                            "       namaunitkerja = :NamaUnitKerja, " +
                            "       induk = :Induk, "+
                            "       kode = :Kode, " +
                            "       tampil = :Tampil " + 
                            "WHERE unitkerjaid = :Id";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("NamaUnitKerja", namaunitkerja));
                        arrayListParameters.Add(new OracleParameter("Induk", induk));
                        arrayListParameters.Add(new OracleParameter("Kode", kode));
                        arrayListParameters.Add(new OracleParameter("Tampil", Convert.ToInt32(tampil)));
                        arrayListParameters.Add(new OracleParameter("Id", unitkerjaid));
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

        #endregion


        #region User Login

        public string EncodePassword(string password)
        {
            string encodedPassword = password;

            HMACSHA1 hash = new HMACSHA1();
            hash.Key = HexToByte(encryptionKey);
            encodedPassword =
              Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));

            return encodedPassword;
        }

        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public List<UserLogin> GetUserLogin(string nip, string nama, string jabatan, string satker, int from, int to)
        {
            List<UserLogin> records = new List<UserLogin>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY pegawai.nama, pegawai.pegawaiid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        pegawai.pegawaiid, pegawai.nama, rjsimpeg.namajabatan AS jabatan, pegawai.nomorhp AS NomorTelepon, " +
                "        decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') ||" +
                "            decode(pegawai.nama, '', '', pegawai.nama) ||" +
                "            decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap, " +
                "        users.userid, users.password, users.username, users.email, " +
                "        psimpeg.foto, simpeg_2702.FNSATKER(psimpeg.satkerid) Satker " +
                "    FROM " +
                "        pegawai " +
                "        LEFT JOIN simpeg_2702.pegawai psimpeg ON psimpeg.nipbaru = pegawai.pegawaiid " +
                "        LEFT JOIN simpeg_2702.riwayatjabatan rjsimpeg ON rjsimpeg.pegawaiid = psimpeg.pegawaiid AND NVL(rjsimpeg.isnew, 0) = 1 " +
                "        LEFT JOIN simpeg_2702.VWPANGKATTERAKHIR pangkatsimpeg ON pangkatsimpeg.pegawaiid = psimpeg.pegawaiid AND pangkatsimpeg.ranking = 1 " +
                "        LEFT JOIN simpeg_2702.pangkat refpangkatsimpeg ON refpangkatsimpeg.pangkatid = pangkatsimpeg.pangkatid " +
                "        LEFT JOIN users ON users.userid = pegawai.userid " +
                "    WHERE " +
                "        (pegawai.VALIDSAMPAI IS NULL OR TRUNC(CAST(pegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) ";

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new OracleParameter("Nip", String.Concat("%", nip, "%")));
                query += " AND pegawai.pegawaiid LIKE :Nip ";
            }
            if (!String.IsNullOrEmpty(nama))
            {
                arrayListParameters.Add(new OracleParameter("Nama", String.Concat("%", nama.ToLower(), "%")));
                query += " AND LOWER(pegawai.nama) LIKE :Nama ";
            }
            if (!String.IsNullOrEmpty(jabatan))
            {
                arrayListParameters.Add(new OracleParameter("Jabatan", String.Concat("%", jabatan.ToLower(), "%")));
                query += " AND LOWER(rjsimpeg.namajabatan) LIKE :Jabatan ";
            }
            if (!String.IsNullOrEmpty(satker))
            {
                arrayListParameters.Add(new OracleParameter("Satker", String.Concat("%", satker.ToLower(), "%")));
                query += " AND LOWER(simpeg_2702.FNSATKER(psimpeg.satkerid)) LIKE :Satker ";

                query = query.Replace("(ORDER BY pegawai.nama, pegawai.pegawaiid)", "(ORDER BY refpangkatsimpeg.kodepangkat DESC)");
            }
            //if (!String.IsNullOrEmpty(jabatan))
            //{
            //    arrayListParameters.Add(new OracleParameter("Jabatan", String.Concat("%", jabatan, "%")));
            //    query += " AND EXISTS (SELECT 1 FROM jabatanpegawai JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
            //        "WHERE jabatanpegawai.pegawaiid = pegawai.pegawaiid AND jabatanpegawai.validsampai IS NULL AND LOWER(jabatan.nama) LIKE :Jabatan) ";
            //}

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UserLogin>(query, parameters).ToList<UserLogin>();
            }

            return records;
        }

        public List<UserLogin> GetListUserLogin(string nip, string nama, string jabatan, string satker, string myunitkerjaid, int from, int to)
        {
            List<UserLogin> records = new List<UserLogin>();

            ArrayList arrayListParameters = new ArrayList();

            bool IsLoginAdminApp = OtorisasiUser.IsRoleAdministrator();

            //string query = 
            //    "SELECT * FROM ( " +
            //    "    SELECT " +
            //    "        ROW_NUMBER() over (ORDER BY pegawai.nama) RNUMBER, COUNT(1) OVER() TOTAL, " +
            //    "        pegawai.pegawaiid, pegawai.nama, rjsimpeg.namajabatan AS jabatan, pegawai.nomorhp AS NomorTelepon, " +
            //    "        decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') ||" +
            //    "            decode(pegawai.nama, '', '', pegawai.nama) ||" +
            //    "            decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap, " +
            //    "        users.userid, users.password, users.username, users.email, " +
            //    "        psimpeg.foto, simpeg_2702.FNSATKER(psimpeg.satkerid) Satker " +
            //    "    FROM " +
            //    "        pegawai " +
            //    "        LEFT JOIN simpeg_2702.pegawai psimpeg ON psimpeg.nipbaru = pegawai.pegawaiid " +
            //    "        LEFT JOIN simpeg_2702.riwayatjabatan rjsimpeg ON rjsimpeg.pegawaiid = psimpeg.pegawaiid AND NVL(rjsimpeg.isnew, 0) = 1 " +
            //    "        LEFT JOIN simpeg_2702.VWPANGKATTERAKHIR pangkatsimpeg ON pangkatsimpeg.pegawaiid = psimpeg.pegawaiid AND pangkatsimpeg.ranking = 1 " +
            //    "        LEFT JOIN simpeg_2702.pangkat refpangkatsimpeg ON refpangkatsimpeg.pangkatid = pangkatsimpeg.pangkatid " +
            //    "        LEFT JOIN users ON users.userid = pegawai.userid " +
            //    "    WHERE " +
            //    "        pegawai.validsampai IS NULL ";

            string query = @"
                SELECT * FROM(
	                 SELECT
                     ROW_NUMBER() OVER (ORDER BY PG.NAMA) RNUMBER,
                     PG.PEGAWAIID,
                     PS.NAMA_LENGKAP AS NAMALENGKAP,
                     PS.FOTO,
                     PS.NAMAJABATAN AS JABATAN,
                     PS.SATKER
                   FROM PEGAWAI PG
                     INNER JOIN SIMPEG_2702.V_PEGAWAI_EOFFICE PS ON
                       PS.NIPBARU = PG.PEGAWAIID
                   WHERE
                     (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))  "; // Arya :: 2020-07-23

            if (!IsLoginAdminApp)
            {
                // Bila hanya Admin Satker, query pegawai hanya di satker tsb
                /*
                query += " AND pegawai.pegawaiid in ( " +
                         " SELECT DISTINCT jabatanpegawai.pegawaiid " +
                         " FROM jabatanpegawai " +
                         "      JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
                         "           and jabatan.profileid NOT IN ('A80100', 'A80300', 'A81001', 'A81002', 'A81003') " +
                         " WHERE jabatan.unitkerjaid = :UnitKerjaId) ";
                arrayListParameters.Add(new OracleParameter("UnitKerjaId", myunitkerjaid));
                */
                query += string.Format(@"
                         AND PG.PEGAWAIID IN 
                           (SELECT DISTINCT JABATANPEGAWAI.PEGAWAIID
                            FROM JABATANPEGAWAI
                              JOIN JABATAN ON
                                JABATAN.PROFILEID = JABATANPEGAWAI.PROFILEID AND
                                JABATAN.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300')
                            WHERE
                              JABATAN.UNITKERJAID = '{0}' AND 
                              (JABATANPEGAWAI.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                              NVL(JABATANPEGAWAI.STATUSHAPUS,'0') = '0'
                            UNION ALL
                            SELECT VP.NIPBARU AS PEGAWAIID
                            FROM SIMPEG_2702.V_PEGAWAI_EOFFICE VP
                              INNER JOIN SIMPEG_2702.SATKER SI ON
                                SI.KDSATKER = VP.KODESATKER AND
                                SI.SATKERID = '{0}'
                            WHERE
                              SI.EOFFICE_PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') )", myunitkerjaid); // Arya :: 2020-07-22

            }

            if (!String.IsNullOrEmpty(nip))
            {
                arrayListParameters.Add(new OracleParameter("Nip", String.Concat("%", nip, "%")));
                //query += " AND pegawai.pegawaiid LIKE :Nip ";
                query += " AND PG.PEGAWAIID LIKE :Nip ";
            }
            if (!String.IsNullOrEmpty(nama))
            {
                arrayListParameters.Add(new OracleParameter("Nama", String.Concat("%", nama.ToLower(), "%")));
                //query += " AND LOWER(pegawai.nama) LIKE :Nama ";
                query += " AND LOWER(PG.NAMA) LIKE :Nama ";
            }
            if (!String.IsNullOrEmpty(jabatan))
            {
                arrayListParameters.Add(new OracleParameter("Jabatan", String.Concat("%", jabatan.ToLower(), "%")));
                //query += " AND LOWER(rjsimpeg.namajabatan) LIKE :Jabatan ";
                query += " AND LOWER(PS.NAMAJABATAN) LIKE :Jabatan ";
            }
            if (!String.IsNullOrEmpty(satker))
            {
                arrayListParameters.Add(new OracleParameter("Satker", String.Concat("%", satker.ToLower(), "%")));
                //query += " AND LOWER(simpeg_2702.FNSATKER(psimpeg.satkerid)) LIKE :Satker ";
                query += " AND LOWER(PS.SATKER) LIKE :Satker ";

                //query = query.Replace("(ORDER BY pegawai.nama, pegawai.pegawaiid)", "(ORDER BY refpangkatsimpeg.kodepangkat DESC)");
            }
            //if (!String.IsNullOrEmpty(jabatan))
            //{
            //    arrayListParameters.Add(new OracleParameter("Jabatan", String.Concat("%", jabatan, "%")));
            //    query += " AND EXISTS (SELECT 1 FROM jabatanpegawai JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
            //        "WHERE jabatanpegawai.pegawaiid = pegawai.pegawaiid AND jabatanpegawai.validsampai IS NULL AND LOWER(jabatan.nama) LIKE :Jabatan) ";
            //}

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt ";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UserLogin>(query, parameters).ToList<UserLogin>();
                foreach(var rec in records)
                {
                    rec.IsActive = (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM JABATANPEGAWAI WHERE PEGAWAIID = '{0}' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'", rec.PegawaiId)).FirstOrDefault()) > 0;
                }
            }

            return records;
        }

        public TransactionResult UpdateUserLogin(UserLogin userlogin)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(userlogin.UserId))
                        {
                            sql = @"UPDATE users set username = :Username, email = :Email where userid = :Id";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Username", userlogin.Username));
                            arrayListParameters.Add(new OracleParameter("Email", userlogin.Email));
                            arrayListParameters.Add(new OracleParameter("Id", userlogin.UserId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            if (userlogin.Password != "********")
                            {
                                sql = @"UPDATE users set password = :Password where userid = :Id";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Password", EncodePassword(userlogin.Password)));
                                arrayListParameters.Add(new OracleParameter("Id", userlogin.UserId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }

                            // Update field nomorhp di tabel PEGAWAI
                            sql =
                                @"UPDATE pegawai SET
                                         nomorhp = :NomorTelepon
                                  WHERE pegawaiid = :PegawaiId";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("NomorTelepon", userlogin.NomorTelepon));
                            arrayListParameters.Add(new OracleParameter("PegawaiId", userlogin.PegawaiId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Create Users
                            string id = ctx.Database.SqlQuery<string>("SELECT substr(lower(sys_guid()),1,8) || '-' || substr(lower(sys_guid()),9,4) || '-' || substr(lower(sys_guid()),13,4) || '-' || substr(lower(sys_guid()),17,4) || '-' || substr(lower(sys_guid()),21,12) FROM DUAL").FirstOrDefault<string>();

                            userlogin.UserId = id;

                            sql =
                                @"INSERT INTO users (
                                     userid, username, applicationname, email, password, passwordquestion, passwordanswer, isapproved, IsLockedOut, creationdate) VALUES 
                                (
                                    :Id, :Username, 'Surat', :Email, :Password, 'surat', 'UuR/8s0q6T1AHSnM3Rk6KoFtkCI=', 1, 0, SYSDATE)";
                            sql = sWhitespace.Replace(sql, " ");

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Id", id));
                            arrayListParameters.Add(new OracleParameter("Username", userlogin.Username));
                            arrayListParameters.Add(new OracleParameter("Email", userlogin.Email));
                            arrayListParameters.Add(new OracleParameter("Password", EncodePassword(userlogin.Password)));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            // Update field userid di tabel PEGAWAI
                            sql =
                                @"UPDATE pegawai SET
                                         userid = :UserId, nomorhp = :NomorTelepon
                                  WHERE pegawaiid = :PegawaiId";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("UserId", userlogin.UserId));
                            arrayListParameters.Add(new OracleParameter("NomorTelepon", userlogin.NomorTelepon));
                            arrayListParameters.Add(new OracleParameter("PegawaiId", userlogin.PegawaiId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = userlogin.UserId;
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

        public TransactionResult SinkronisasiUser(UserLogin userlogin)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(userlogin.PegawaiId))
                        {
                            // Ambil data pegawai dari database SIMPEG
                            PegawaiSimpeg datapegawai = new PegawaiSimpeg();

                            sql =
                                //"SELECT p.nipbaru pegawaiid, p.nama, " +
                                //"       p.gelardepan, p.gelarbelakang, " +
                                //"       decode(p.gelardepan, '', '', p.gelardepan || ' ') || " +
                                //"           decode(p.nama, '', '', p.nama) || " +
                                //"           decode(p.gelarbelakang, null, '', ', ' || p.gelarbelakang) AS NamaLengkap, " +
                                //"       p.alamat, p.email, p.hp nomorhp, " +
                                //"       p.satkerid, s.satker namasatker, s.namajabatan " +
                                //"FROM   simpeg_2702.pegawai p " +
                                //"       JOIN simpeg_2702.satker s ON s.satkerid = p.satkerid " +
                                //"WHERE p.nipbaru = :Nip";
                                "SELECT " +
                                "    nipbaru pegawaiid, nama, nama_lengkap NamaLengkap, " +
                                "    gelardepan, gelarbelakang, alamat, email, " +
                                "    hp nomorhp, satkerid, satker namasatker, " +
                                "    namajabatan " +
                                "FROM " +
                                "    siap_vw_pegawai " +
                                "WHERE " +
                                "    nipbaru = :Nip";

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Nip", userlogin.PegawaiId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            datapegawai = ctx.Database.SqlQuery<PegawaiSimpeg>(sql, parameters).FirstOrDefault();
                            if (datapegawai != null)
                            {
                                // Update PEGAWAI
                                sql = @"UPDATE pegawai SET
                                               nama = :Nama,
                                               jabatan = :Jabatan,
                                               gelardepan = :GelarDepan,
                                               gelarbelakang = :GelarBelakang,
                                               alamat = :Alamat,
                                               nomorhp = :NomorHP
                                        WHERE pegawaiid = :PegawaiId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Nama", datapegawai.Nama));
                                arrayListParameters.Add(new OracleParameter("Jabatan", datapegawai.NamaJabatan));
                                arrayListParameters.Add(new OracleParameter("GelarDepan", datapegawai.GelarDepan));
                                arrayListParameters.Add(new OracleParameter("GelarBelakang", datapegawai.GelarBelakang));
                                arrayListParameters.Add(new OracleParameter("Alamat", datapegawai.Alamat));
                                arrayListParameters.Add(new OracleParameter("NomorHP", datapegawai.NomorHP));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", userlogin.PegawaiId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Update USERS
                                sql = @"UPDATE users SET
                                               email = :Email
                                        WHERE userid = :UserId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Email", datapegawai.Email));
                                arrayListParameters.Add(new OracleParameter("UserId", userlogin.UserId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                tr.Status = true;
                                tr.Pesan = "Proses sinkronisasi data Simpeg sudah selesai";
                            }
                            else
                            {
                                tr.Status = false;
                                tr.Pesan = "Gagal mendapatkan data pegawai dari Simpeg";
                            }
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal mendapatkan ID pegawai";
                        }

                        tc.Commit();
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

        public TransactionResult SimpanAdminSatker(SetAdminSatker data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string profileid_adminsatker = "A80300";

                        if (string.IsNullOrEmpty(data.KantorId))
                        {
                            data.KantorId = "980FECFC746D8C80E0400B0A9214067D";
                        }

                        #region Cek Duplikasi

                        bool BisaSimpanProfileAdminSatker = true;

                        sql =
                            "SELECT count(*) FROM jabatanpegawai " +
                            "WHERE profileid = :ProfileId AND pegawaiid = :PegawaiId AND kantorid = :KantorId " +
                            "AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("ProfileId", profileid_adminsatker));
                        arrayListParameters.Add(new OracleParameter("PegawaiId", data.PegawaiId));
                        arrayListParameters.Add(new OracleParameter("KantorId", data.KantorId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                        if (jumlahdata > 0)
                        {
                            BisaSimpanProfileAdminSatker = false;
                        }

                        #endregion

                        if (BisaSimpanProfileAdminSatker)
                        {
                            sql =
                                @"INSERT INTO jabatanpegawai (
                                              profilepegawaiid, profileid, pegawaiid, kantorid, validsejak, keterangan,
                                              userupdate, lastupdate) VALUES 
                                  (
                                              SYS_GUID(),:ProfileId,:PegawaiId,:KantorId,SYSDATE,:Keterangan,
                                              :UserUpdate,SYSDATE)";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("ProfileId", profileid_adminsatker));
                            arrayListParameters.Add(new OracleParameter("PegawaiId", data.PegawaiId));
                            arrayListParameters.Add(new OracleParameter("KantorId", data.KantorId));
                            arrayListParameters.Add(new OracleParameter("Keterangan", "dari eoffice"));
                            arrayListParameters.Add(new OracleParameter("UserUpdate", "pusdatin"));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        if (data.KantorId != "980FECFC746D8C80E0400B0A9214067D")
                        {
                            if (data.TipeKantorId == 3)
                            {
                                // KANTAH
                                // Simpan Profile Pengadministrasi Umum
                                string profileid_administrasiumum = "N" + data.SatkerId.Substring(0, 6) + "010301";

                                #region Cek Duplikasi

                                bool BisaSimpanProfileAdminUmum = true;

                                sql =
                                    "SELECT count(*) FROM jabatanpegawai " +
                                    "WHERE profileid LIKE 'N%' AND pegawaiid = :PegawaiId AND kantorid = :KantorId " +
                                    "AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                arrayListParameters.Clear();
                                //arrayListParameters.Add(new OracleParameter("ProfileId", profileid_administrasiumum));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", data.PegawaiId));
                                arrayListParameters.Add(new OracleParameter("KantorId", data.KantorId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                if (jumlahdata > 0)
                                {
                                    BisaSimpanProfileAdminUmum = false;
                                }

                                #endregion

                                if (BisaSimpanProfileAdminUmum)
                                {
                                    sql =
                                        @"INSERT INTO jabatanpegawai (
                                                      profilepegawaiid, profileid, pegawaiid, kantorid, validsejak, keterangan,
                                                      userupdate, lastupdate) VALUES 
                                          (
                                                      SYS_GUID(),:ProfileId,:PegawaiId,:KantorId,SYSDATE,:Keterangan,
                                                      :UserUpdate,SYSDATE)";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("ProfileId", profileid_administrasiumum));
                                    arrayListParameters.Add(new OracleParameter("PegawaiId", data.PegawaiId));
                                    arrayListParameters.Add(new OracleParameter("KantorId", data.KantorId));
                                    arrayListParameters.Add(new OracleParameter("Keterangan", "dari eoffice"));
                                    arrayListParameters.Add(new OracleParameter("UserUpdate", "pusdatin"));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                            else if (data.TipeKantorId == 2)
                            {
                                // KANWIL
                                // Simpan Profile Pengadministrasi Umum
                                string profileid_administrasiumum = "R" + data.SatkerId.Substring(0, 6) + "010301";

                                #region Cek Duplikasi

                                bool BisaSimpanProfileAdminUmum = true;

                                sql =
                                    "SELECT count(*) FROM jabatanpegawai " +
                                    "WHERE profileid LIKE 'R%' AND pegawaiid = :PegawaiId AND kantorid = :KantorId " +
                                    "AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                arrayListParameters.Clear();
                                //arrayListParameters.Add(new OracleParameter("ProfileId", profileid_administrasiumum));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", data.PegawaiId));
                                arrayListParameters.Add(new OracleParameter("KantorId", data.KantorId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                if (jumlahdata > 0)
                                {
                                    BisaSimpanProfileAdminUmum = false;
                                }

                                #endregion

                                if (BisaSimpanProfileAdminUmum)
                                {
                                    sql =
                                        @"INSERT INTO jabatanpegawai (
                                                      profilepegawaiid, profileid, pegawaiid, kantorid, validsejak, keterangan,
                                                      userupdate, lastupdate) VALUES 
                                          (
                                                      SYS_GUID(),:ProfileId,:PegawaiId,:KantorId,SYSDATE,:Keterangan,
                                                      :UserUpdate,SYSDATE)";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("ProfileId", profileid_administrasiumum));
                                    arrayListParameters.Add(new OracleParameter("PegawaiId", data.PegawaiId));
                                    arrayListParameters.Add(new OracleParameter("KantorId", data.KantorId));
                                    arrayListParameters.Add(new OracleParameter("Keterangan", "dari eoffice"));
                                    arrayListParameters.Add(new OracleParameter("UserUpdate", "pusdatin"));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                        }


                        tc.Commit();
                        //tc.Rollback(); // for test
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

        //public TransactionResult SetPasswordStandar(GantiPassword data)
        //{
        //    TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

        //    string sql = "";
        //    ArrayList arrayListParameters = new ArrayList();
        //    object[] parameters = null;

        //    using (var ctx = new BpnDbContext())
        //    {
        //        using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                if (!string.IsNullOrEmpty(data.UserId))
        //                {
        //                    string passwordstandar = "IbpWMR6PoM7+d+bBm3bYVQYt8OI="; //atrbpn2016

        //                    sql = @"UPDATE users set password = :Password where userid = :Id";
        //                    sql = sWhitespace.Replace(sql, " ");
        //                    arrayListParameters.Clear();
        //                    arrayListParameters.Add(new OracleParameter("Password", passwordstandar));
        //                    arrayListParameters.Add(new OracleParameter("Id", data.UserId));
        //                    parameters = arrayListParameters.OfType<object>().ToArray();
        //                    ctx.Database.ExecuteSqlCommand(sql, parameters);
        //                }


        //                tc.Commit();
        //                //tc.Rollback(); // for test
        //                tr.Status = true;
        //                tr.Pesan = "Password berhasil disimpan ke password standar yaitu atrbpn2016";
        //            }
        //            catch (Exception ex)
        //            {
        //                tc.Rollback();
        //                tr.Pesan = ex.Message.ToString();
        //            }
        //            finally
        //            {
        //                tc.Dispose();
        //                ctx.Dispose();
        //            }
        //        }
        //    }


        //    return tr;
        //}

        //public TransactionResult KembalikanPassword(GantiPassword data)
        //{
        //    TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

        //    string sql = "";
        //    ArrayList arrayListParameters = new ArrayList();
        //    object[] parameters = null;

        //    using (var ctx = new BpnDbContext())
        //    {
        //        using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                if (!string.IsNullOrEmpty(data.UserId))
        //                {
        //                    sql = @"UPDATE users set password = :Password where userid = :Id";
        //                    sql = sWhitespace.Replace(sql, " ");
        //                    arrayListParameters.Clear();
        //                    arrayListParameters.Add(new OracleParameter("Password", data.PasswordLama));
        //                    arrayListParameters.Add(new OracleParameter("Id", data.UserId));
        //                    parameters = arrayListParameters.OfType<object>().ToArray();
        //                    ctx.Database.ExecuteSqlCommand(sql, parameters);
        //                }


        //                tc.Commit();
        //                //tc.Rollback(); // for test
        //                tr.Status = true;
        //                tr.Pesan = "Password berhasil disimpan ke password lama yaitu " + data.PasswordLama;
        //            }
        //            catch (Exception ex)
        //            {
        //                tc.Rollback();
        //                tr.Pesan = ex.Message.ToString();
        //            }
        //            finally
        //            {
        //                tc.Dispose();
        //                ctx.Dispose();
        //            }
        //        }
        //    }


        //    return tr;
        //}

        //public TransactionResult DoExecuteSql(string textcommand)
        //{
        //    TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

        //    string sql = "";

        //    using (var ctx = new BpnDbContext())
        //    {
        //        using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                sql = @textcommand;
        //                sql = sWhitespace.Replace(sql, " ");
        //                ctx.Database.ExecuteSqlCommand(sql);

        //                tc.Commit();
        //                //tc.Rollback(); // for test
        //                tr.Status = true;
        //                tr.Pesan = "Execute Sql Command berhasil dijalankan";
        //            }
        //            catch (Exception ex)
        //            {
        //                tc.Rollback();
        //                tr.Pesan = ex.Message.ToString();
        //            }
        //            finally
        //            {
        //                tc.Dispose();
        //                ctx.Dispose();
        //            }
        //        }
        //    }

        //    return tr;
        //}

        //public string GetSqlQuery(string textcommand)
        //{
        //    string result = "";

        //    string query = @textcommand;

        //    try
        //    {
        //        using (var ctx = new BpnDbContext())
        //        {
        //            result = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }

        //    return result;
        //}

        #endregion


        #region User PPNPN

        public List<UserPPNPN> GetUserPPNPN(string nik, string nama, string satker, int from, int to)
        {
            List<UserPPNPN> records = new List<UserPPNPN>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY ppnpn.nama, ppnpn.ppnpnid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        ppnpn.ppnpnid, ppnpn.nik, ppnpn.nama, ppnpn.nohp AS NomorTelepon, " +
                "        ppnpn.nama AS NamaLengkap, " +
                "        userppnpn.userid, userppnpn.username, userppnpn.email, " +
                "        ppnpn.urlprofile AS Foto, unitkerja.namaunitkerja AS Satker " +
                "    FROM " +
                "        ppnpn " +
                "        LEFT JOIN userppnpn ON userppnpn.userid = ppnpn.userid " +
                "        LEFT JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = ppnpn.nik  " +
                "        LEFT JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid  " +
                "        LEFT JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid " +
                "    WHERE " +
                "        ppnpn.tanggalvalidasi IS NOT NULL ";

            if (!String.IsNullOrEmpty(nik))
            {
                arrayListParameters.Add(new OracleParameter("Nik", String.Concat("%", nik, "%")));
                query += " AND ppnpn.nik LIKE :Nik ";
            }
            if (!String.IsNullOrEmpty(nama))
            {
                arrayListParameters.Add(new OracleParameter("Nama", String.Concat("%", nama.ToLower(), "%")));
                query += " AND LOWER(ppnpn.nama) LIKE :Nama ";
            }
            if (!String.IsNullOrEmpty(satker))
            {
                arrayListParameters.Add(new OracleParameter("Satker", String.Concat("%", satker.ToLower(), "%")));
                query += " AND LOWER(unitkerja.namaunitkerja) LIKE :Satker ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UserPPNPN>(query, parameters).ToList<UserPPNPN>();
            }

            return records;
        }

        public List<UserPPNPN> GetListUserPPNPN(string nik, string nama, string satker, string myunitkerjaid, int from, int to)
        {
            List<UserPPNPN> records = new List<UserPPNPN>();

            ArrayList arrayListParameters = new ArrayList();

            bool IsLoginAdminApp = false;
            if (OtorisasiUser.IsRoleAdministrator() == true)
            {
                IsLoginAdminApp = true;
            }

            string query = @"
                SELECT *
                FROM
                  (SELECT
                     ROW_NUMBER() over (ORDER BY UR.NAMA, UR.PPNPNID) RNUMBER,
                     COUNT(1) OVER() TOTAL, UR.PPNPNID, UR.NIK, UR.NAMA,
                     UR.NOHP AS NOMORTELEPON, UR.NAMA AS NAMALENGKAP, UP.USERID,
                     UP.USERNAME, UP.EMAIL, UR.URLPROFILE AS FOTO,
                     UK.NAMAUNITKERJA AS SATKER
                   FROM PPNPN UR
                     LEFT JOIN USERPPNPN UP ON
                       UP.USERID = UR.USERID
                     LEFT JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = UR.NIK  AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JP.STATUSHAPUS,'0') = '0'
                     LEFT JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID
                     LEFT JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = JB.UNITKERJAID
                   WHERE
                     UR.TANGGALVALIDASI IS NOT NULL";

            if (!String.IsNullOrEmpty(nik))
            {
                arrayListParameters.Add(new OracleParameter("Nik", String.Concat("%", nik, "%")));
                query += " AND UR.NIK LIKE :Nik ";
            }
            if (!String.IsNullOrEmpty(nama))
            {
                arrayListParameters.Add(new OracleParameter("Nama", String.Concat("%", nama.ToLower(), "%")));
                query += " AND LOWER(UR.NAMA) LIKE :Nama ";
            }
            if (!String.IsNullOrEmpty(satker))
            {
                arrayListParameters.Add(new OracleParameter("Satker", String.Concat("%", satker.ToLower(), "%")));
                query += " AND LOWER(UK.NAMAUNITKERJA) LIKE :Satker ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new OracleParameter("startCnt", from));
            arrayListParameters.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UserPPNPN>(query, parameters).ToList<UserPPNPN>();
                foreach(var itm in records)
                {
                    if (IsLoginAdminApp)
                    {
                        itm.inSatker = "Y";
                    }
                    else
                    {
                        itm.inSatker = (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM JABATANPEGAWAI JOIN JABATAN ON JABATAN.PROFILEID = JABATANPEGAWAI.PROFILEID AND JABATAN.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') WHERE JABATAN.UNITKERJAID = '{0}' AND JABATANPEGAWAI.PEGAWAIID = '{1}'", myunitkerjaid, itm.NIK)).FirstOrDefault() > 0)?"Y":"N";
                    }
                }
            }

            return records;
        }

        public TransactionResult UpdateUserPPNPN(UserPPNPN userppnpn)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(userppnpn.UserId))
                        {
                            sql = @"UPDATE userppnpn set username = :Username, email = :Email where userid = :Id";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Username", userppnpn.Username));
                            arrayListParameters.Add(new OracleParameter("Email", userppnpn.Email));
                            arrayListParameters.Add(new OracleParameter("Id", userppnpn.UserId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            if (userppnpn.Password != "********")
                            {
                                sql = @"UPDATE users set password = :Password where userid = :Id";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Password", EncodePassword(userppnpn.Password)));
                                arrayListParameters.Add(new OracleParameter("Id", userppnpn.UserId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }

                            // Update field nomorhp di tabel PPNPN
                            sql =
                                @"UPDATE ppnpn SET
                                         nohp = :NomorTelepon
                                  WHERE  ppnpnid = :PPNPNId";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("NomorTelepon", userppnpn.NomorTelepon));
                            arrayListParameters.Add(new OracleParameter("PPNPNId", userppnpn.PPNPNId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Create Users
                            string id = ctx.Database.SqlQuery<string>("SELECT substr(lower(sys_guid()),1,8) || '-' || substr(lower(sys_guid()),9,4) || '-' || substr(lower(sys_guid()),13,4) || '-' || substr(lower(sys_guid()),17,4) || '-' || substr(lower(sys_guid()),21,12) FROM DUAL").FirstOrDefault<string>();

                            userppnpn.UserId = id;

                            sql =
                                @"INSERT INTO userppnpn (
                                     userid, username, applicationname, email, password, passwordquestion, passwordanswer, isapproved, IsLockedOut, creationdate) VALUES 
                                (
                                    :Id, :Username, 'Surat', :Email, :Password, 'surat', 'UuR/8s0q6T1AHSnM3Rk6KoFtkCI=', 1, 0, SYSDATE)";
                            sql = sWhitespace.Replace(sql, " ");

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Id", id));
                            arrayListParameters.Add(new OracleParameter("Username", userppnpn.Username));
                            arrayListParameters.Add(new OracleParameter("Email", userppnpn.Email));
                            arrayListParameters.Add(new OracleParameter("Password", EncodePassword(userppnpn.Password)));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            // Update field userid di tabel PPNPN
                            sql =
                                @"UPDATE ppnpn SET
                                         userid = :UserId, nohp = :NomorTelepon
                                  WHERE  ppnpnid = :PPNPNId";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("UserId", userppnpn.UserId));
                            arrayListParameters.Add(new OracleParameter("NomorTelepon", userppnpn.NomorTelepon));
                            arrayListParameters.Add(new OracleParameter("PPNPNId", userppnpn.PPNPNId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = userppnpn.UserId;
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

        #endregion


        #region User Profiles


        public int JumlahProfilePegawai(string pegawaiid, string profileid, string kantorid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT count(*) FROM jabatanpegawai WHERE pegawaiid = :PegawaiId AND profileid = :ProfileId AND kantorid = :KantorId AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'"; // Arya :: 2020-09-08

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public TransactionResult InsertProfilePegawai(UserLogin userlogin, string kantorid, string jabatanPegawaiId, string validsejak, string validsampai)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            Regex sWhitespace = new Regex(@"\s+");

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        int statusplt = (userlogin.IsStatusPlt == true) ? 1 : 0;
                        // Insert PROFILEPEGAWAI / JABATANPEGAWAI

                        sql =
                            @"INSERT INTO jabatanpegawai (
                                profilepegawaiid, profileid, pegawaiid, kantorid, validsejak, statusplt, keterangan, userupdate, lastupdate) VALUES 
                                (
                                    :Id, :ProfileId, :PegawaiId, :KantorId, TO_DATE(:ValidSejak,'DD/MM/YYYY') , :StatusPlt, 'Pengaturan User', :UserId, SYSDATE)";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("Id", jabatanPegawaiId));
                        arrayListParameters.Add(new OracleParameter("ProfileId", userlogin.ProfileId));
                        arrayListParameters.Add(new OracleParameter("PegawaiId", userlogin.PegawaiId));
                        arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
                        arrayListParameters.Add(new OracleParameter("ValidSejak", validsejak));
                        arrayListParameters.Add(new OracleParameter("StatusPlt", userlogin.TipeJabatan));
                        arrayListParameters.Add(new OracleParameter("UserId", userlogin.UserId));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);


                        if (statusplt == 0)
                        {
                            sql = @"UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :idUser, LASTUPDATE = SYSDATE, KETERANGAN = 'Pengaturan User' 
                                     WHERE PROFILEID <> :idProfile
                                       AND PEGAWAIID = :idPegawai 
                                       AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                            OracleParameter p1 = new OracleParameter("idUser", userlogin.UserId );
                            OracleParameter p2 = new OracleParameter("idProfile", userlogin.ProfileId);
                            OracleParameter p3 = new OracleParameter("idPegawai", userlogin.PegawaiId);
                            OracleParameter p4 = new OracleParameter("idKantor", kantorid);
                            parameters = new object[3] { p1, p2, p3 };
                            sql = sWhitespace.Replace(sql, " ");
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            sql = string.Format("SELECT NVL(TIPEESELONID,9) FROM JABATAN WHERE PROFILEID = '{0}'",userlogin.ProfileId);
                            var _eselon = ctx.Database.SqlQuery<int>(sql).FirstOrDefault();
                            if (_eselon < 4 && !OtorisasiUser.isFungsional(userlogin.Jabatan))
                            {
                                sql = @"UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :idUser, LASTUPDATE = SYSDATE, KETERANGAN = 'Pengaturan User'  
                                     WHERE PROFILEID = :idProfile 
                                       AND PEGAWAIID <> :idPegawai 
                                       AND KANTORID = :idKantor
                                       AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                parameters = new object[4] { p1, p2, p3, p4 };
                                sql = sWhitespace.Replace(sql, " ");
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(validsampai))
                            {
                                sql = @" UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TO_DATE(:Tanggal,'DD/MM/YYYY') 
                                         WHERE PROFILEPEGAWAIID = :Id
                                           AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Tanggal", validsampai));
                                arrayListParameters.Add(new OracleParameter("Id", jabatanPegawaiId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }


                        sql = string.Format("SELECT COUNT(1) FROM JABATAN WHERE PROFILEID = '{0}' AND PROFILEIDTU = '{0}'", userlogin.ProfileId);
                        if (ctx.Database.SqlQuery<int>(sql).FirstOrDefault() > 0)
                        {
                            int _tipeKantorId = GetTipeKantor(kantorid);
                            if(_tipeKantorId > 1)
                            {
                                string _adminId = _tipeKantorId == 2 ? "A80500" : "A80400";

                                sql = @"UPDATE PROFILEPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :idUser, LASTUPDATE = SYSDATE, KETERANGAN = 'Pengaturan User Eoffice'  
                                        WHERE PROFILEID = :idProfile 
                                        AND PEGAWAIID <> :idPegawai 
                                        AND KANTORID = :idKantor
                                        AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                        AND NVL(STATUSHAPUS,'0') = '0'";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("idUser", userlogin.UserId));
                                arrayListParameters.Add(new OracleParameter("idProfile", _adminId));
                                arrayListParameters.Add(new OracleParameter("idPegawai", userlogin.PegawaiId));
                                arrayListParameters.Add(new OracleParameter("idKantor", kantorid));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                sql = "SELECT COUNT(1) FROM PROFILEPEGAWAI WHERE PROFILEID = :idProfile AND PEGAWAIID = :idPegawai AND KANTORID = :idKantor AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("idProfile", _adminId));
                                arrayListParameters.Add(new OracleParameter("idPegawai", userlogin.PegawaiId));
                                arrayListParameters.Add(new OracleParameter("idKantor", kantorid));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                if (ctx.Database.SqlQuery<int>(sql,parameters).FirstOrDefault() == 0)
                                {
                                    sql =
                                        @"INSERT INTO PROFILEPEGAWAI (
                                            PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, KETERANGAN,
                                            BISABOOKING, USERUPDATE, LASTUPDATE) VALUES 
                                            (
                                            SYS_GUID(), :ProfileId, :PegawaiId, :KantorId, SYSDATE, 'Pengaturan User Eoffice',
                                            0, :UserUpdate, SYSDATE)";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("ProfileId", _adminId));
                                    arrayListParameters.Add(new OracleParameter("PegawaiId", userlogin.PegawaiId));
                                    arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
                                    arrayListParameters.Add(new OracleParameter("UserUpdate", userlogin.UserId));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                        }

                        tc.Commit();
                        //tc.Rollback();
                        tr.Status = true;
                        tr.ReturnValue = jabatanPegawaiId;
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

        public TransactionResult HapusProfilePegawai(string id, string userid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = @"UPDATE jabatanpegawai SET STATUSHAPUS = '1', USERHAPUS = :UserId, TANGGALPERUBAHAN = SYSDATE WHERE PROFILEPEGAWAIID = :Id"; // Arya :: 2020-07-22
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("UserId", userid));
                        arrayListParameters.Add(new OracleParameter("Id", id));
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

        #endregion


        #region Hak Akses

        public List<ProfilPengguna> ListProfilPengguna(string pKantorId, string pIdPegawai)
        {
            List<ProfilPengguna> _lstProfil = new List<ProfilPengguna>();

            using (var ctx = new BpnDbContext())
            {
                /*
                if (OtorisasiUser.IsRoleAdministrator())
                {
                    string _kantorid = ctx.Database.SqlQuery<string>(string.Format("SELECT uk.kantorid FROM simpeg_2702.v_pegawai_eoffice vp, unitkerja uk WHERE vp.satkerid = uk.unitkerjaid AND vp.nipbaru = '{0}'", pIdPegawai)).FirstOrDefault();
                    if (string.IsNullOrEmpty(_kantorid))
                    {
                        _kantorid = ctx.Database.SqlQuery<string>(string.Format("SELECT KANTORID FROM PEGAWAI WHERE PEGAWAIID = '{0}' AND (pegawai.validsampai IS NULL OR TO_DATE(TRIM(pegawai.validsampai)) > TO_DATE(TRIM(SYSDATE)))", pIdPegawai)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(_kantorid))
                        {
                            pKantorId = _kantorid;
                        }
                    }
                    else
                    {
                        pKantorId = _kantorid;
                    }
                }
                */
                Regex sWhitespace = new Regex(@"\s+");
                string sql =
                    @"SELECT p.profileid,INITCAP(p.nama) nama,DECODE(profilepegawaiid, null, 'False', 'True') aktif, NVL(p.tipeeselonid,0) eselon 
                        FROM jabatan p
  		                     LEFT JOIN jabatanpegawai pp
  		 	                     ON pp.profileid = p.profileid AND pp.kantorid = :idKantor AND pp.pegawaiid = :idPegawai AND (pp.validsampai IS NULL OR TRUNC(CAST(pp.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(pp.statushapus,'0') = '0'
                       WHERE p.profileid IN ({0}) 
                         AND (p.validsampai IS NULL OR TRUNC(CAST(p.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) 
                       ORDER BY p.nama";

                OracleParameter p1 = new OracleParameter("idKantor", pKantorId);
                OracleParameter p2 = new OracleParameter("idPegawai", pIdPegawai);
                var parameters = new object[2] { p1, p2 };
                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["ListProfile"].ToString()), " ");
                _lstProfil = ctx.Database.SqlQuery<ProfilPengguna>(sql, parameters).ToList<ProfilPengguna>();
            }

            return _lstProfil;
        }

        public List<DataPengguna> ListPengguna(string nama, string idkantor, string unitkerjaid)
        {
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            bool IsLoginAdminApp = false;
            if (OtorisasiUser.IsRoleAdministrator() == true)
            { 
                IsLoginAdminApp = true;
            }

            List<DataPengguna> _lstPengguna = new List<DataPengguna>();

            using (var ctx = new BpnDbContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                string sql =
                    "SELECT " +
                    "    DISTINCT(users.userid) idpengguna, pegawai.pegawaiid as idpegawai, users.username namapengguna, " +
                    "    TRIM(pegawai.gelardepan || ' ' || pegawai.nama || ' ' || pegawai.gelarbelakang) namalengkap, " +
                    "    pegawai.displaynip, v_pegawai_eoffice.namajabatan, v_pegawai_eoffice.satker namasatker " +
                    "FROM " +
                    "    users " +
                    "    JOIN pegawai JOIN jabatanpegawai " +
                    "         ON jabatanpegawai.pegawaiid = pegawai.pegawaiid ";

                if (!IsLoginAdminApp)
                {
                    sql +=
                    "            AND jabatanpegawai.kantorid = :idKantor ";

                    arrayListParameters.Add(new OracleParameter("idKantor", idkantor));
                }

                sql +=
                    "            AND (jabatanpegawai.validsampai IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) " +
                    "         ON pegawai.userid = users.userid " +
                    "            AND (pegawai.validsampai IS NULL OR TRUNC(CAST(pegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) " +
                    "    LEFT JOIN simpeg_2702.v_pegawai_eoffice ON v_pegawai_eoffice.nipbaru = pegawai.pegawaiid ";

                if (!IsLoginAdminApp)
                {
                    sql +=
                    "    JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid " +
                    "         AND jabatan.unitkerjaid = :UnitKerjaId ";

                    arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));
                }

                sql +=
                    "WHERE " +
                    "    UPPER(pegawai.nama) like :upNama " +
                    "    AND (users.validsampai IS NULL OR TRUNC(CAST(users.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";

                sql = sWhitespace.Replace(sql, " ");

                //arrayListParameters.Add(new OracleParameter("idKantor", idkantor));
                arrayListParameters.Add(new OracleParameter("upNama", string.Concat("%", nama.ToUpper(), "%")));
                parameters = arrayListParameters.OfType<object>().ToArray();

                _lstPengguna = ctx.Database.SqlQuery<DataPengguna>(sql, parameters).ToList<DataPengguna>();
            }

            return _lstPengguna;
        }

        public List<DataPengguna> ListPengguna(string idkantor, string unitkerjaid)
        {
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            bool IsLoginAdminApp = false;
            if (OtorisasiUser.IsRoleAdministrator() == true)
            {
                IsLoginAdminApp = true;
            }

            List<DataPengguna> _lstPengguna = new List<DataPengguna>();

            using (var ctx = new BpnDbContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                string sql = @"
                    SELECT DISTINCT 
                      IDPEGAWAI, NAMALENGKAP
                    FROM 
                      (SELECT DISTINCT 
                         PG.PEGAWAIID AS IDPEGAWAI, 
                         TRIM(NVL(NULLIF(PG.GELARDEPAN,'-'),'') || ' ' || PG.NAMA || ' ' || NVL(NULLIF(PG.GELARBELAKANG,'-'),'')) AS NAMALENGKAP, 
                         JP.KANTORID, JT.UNITKERJAID
                       FROM JABATANPEGAWAI JP
                         INNER JOIN PEGAWAI PG ON
                           PG.PEGAWAIID = JP.PEGAWAIID 
                         INNER JOIN USERS UR ON
                           UR.USERID = PG.USERID 
	                     INNER JOIN JABATAN JT ON
	  	                   JT.PROFILEID = JP.PROFILEID
                       WHERE
                         (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         (UR.VALIDSAMPAI IS NULL OR TRUNC(CAST(UR.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) 
                       UNION ALL 
                       SELECT DISTINCT 
                         PP.NIK AS IDPEGAWAI, TRIM(PP.NAMA || ' (PPNPN)') AS NAMALENGKAP, 
                         JP.KANTORID, JT.UNITKERJAID
                       FROM JABATANPEGAWAI JP
                         INNER JOIN PPNPN PP ON
                           PP.NIK = JP.PEGAWAIID AND PP.TANGGALVALIDASI IS NOT NULL 
                         INNER JOIN USERPPNPN UP ON
                           UP.USERID = PP.USERID 
	                     INNER JOIN JABATAN JT ON
	  	                   JT.PROFILEID = JP.PROFILEID
                       WHERE
                         (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         (UP.VALIDSAMPAI IS NULL OR TRUNC(CAST(UP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) )";

                if (!IsLoginAdminApp)
                {
                    sql +=
                    " WHERE KANTORID = :idKantor OR UNITKERJAID = :unitKerja";

                    arrayListParameters.Add(new OracleParameter("idKantor", idkantor));
                    arrayListParameters.Add(new OracleParameter("unitKerja", unitkerjaid));
                }

                sql += " ORDER BY NAMALENGKAP ";

                sql = sWhitespace.Replace(sql, " ");

                parameters = arrayListParameters.OfType<object>().ToArray();

                _lstPengguna = ctx.Database.SqlQuery<DataPengguna>(sql, parameters).ToList<DataPengguna>();
            }

            return _lstPengguna;
        }

        public DataPengguna DetailPengguna(string nip, string kantorid)
        {
            var _rst = new DataPengguna();

            using (var ctx = new BpnDbContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                string sql = string.Format(@"
                    SELECT DISTINCT
                      RST.*
                    FROM
                      (SELECT DISTINCT
                         PG.PEGAWAIID AS IDPEGAWAI,
                         UR.USERID AS IDPENGGUNA,
                         UR.USERNAME AS NAMAPENGGUNA,
                         PG.DISPLAYNIP,
                         TRIM(NVL(NULLIF(PG.GELARDEPAN,'-'),'') || ' ' || PG.NAMA || ' ' || NVL(NULLIF(PG.GELARBELAKANG,'-'),'')) AS NAMALENGKAP,
                         VE.NAMAJABATAN,
                         VE.SATKER AS NAMASATKER
                       FROM JABATANPEGAWAI JP
                         INNER JOIN KANTOR KT ON
                           KT.KANTORID = JP.KANTORID
                         INNER JOIN PEGAWAI PG ON
                           PG.PEGAWAIID = JP.PEGAWAIID
                         INNER JOIN USERS UR ON
                           UR.USERID = PG.USERID
	                       INNER JOIN simpeg_2702.v_pegawai_eoffice VE ON
	                     		 VE.NIPBARU = PG.PEGAWAIID
                       WHERE
                         (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         (UR.VALIDSAMPAI IS NULL OR TRUNC(CAST(UR.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                       UNION ALL
                       SELECT DISTINCT
                         PP.NIK AS IDPEGAWAI,
                         UP.USERID AS IDPENGGUNA,
                         UP.USERNAME AS NAMAPENGGUNA,
                         PP.NIK AS DISPLAYNIP,
                         TRIM(PP.NAMA || ' (PPNPN)') AS NAMALENGKAP,
                         JT.NAMA AS NAMAJABATAN,
                         UK.NAMAUNITKERJA AS NAMASATKER
                       FROM JABATANPEGAWAI JP
                         INNER JOIN KANTOR KT ON
                           KT.KANTORID = JP.KANTORID
                         INNER JOIN PPNPN PP ON
                           PP.NIK = JP.PEGAWAIID AND PP.TANGGALVALIDASI IS NOT NULL
                         INNER JOIN USERPPNPN UP ON
                           UP.USERID = PP.USERID
	                       INNER JOIN JABATAN JT ON
	  	                     JT.PROFILEID = JP.PROFILEID
	                       INNER JOIN UNITKERJA UK ON
	  	                     UK.UNITKERJAID = JT.UNITKERJAID
                       WHERE
                         (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         (UP.VALIDSAMPAI IS NULL OR TRUNC(CAST(UP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) ) RST
                    WHERE RST.IDPEGAWAI = '{0}'", nip);

                sql = sWhitespace.Replace(sql, " ");

                _rst = ctx.Database.SqlQuery<DataPengguna>(sql).FirstOrDefault();

                _rst.kantorids = new List<Kantor>();
                if (OtorisasiUser.IsRoleAdministrator())
                {
                    _rst.kantorids = ctx.Database.SqlQuery<Kantor>(string.Format("SELECT DISTINCT JABATANPEGAWAI.KANTORID, KANTOR.NAMA AS NAMAKANTOR FROM JABATANPEGAWAI, KANTOR WHERE JABATANPEGAWAI.KANTORID = KANTOR.KANTORID AND JABATANPEGAWAI.PEGAWAIID = '{0}' AND (JABATANPEGAWAI.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", nip)).ToList();
                    _rst.kantorid = kantorid;
                    string _kantorid = ctx.Database.SqlQuery<string>(string.Format("SELECT uk.kantorid FROM simpeg_2702.v_pegawai_eoffice vp, unitkerja uk WHERE vp.satkerid = uk.unitkerjaid AND vp.nipbaru = '{0}'", nip)).FirstOrDefault();
                    if (string.IsNullOrEmpty(_kantorid))
                    {
                        _kantorid = ctx.Database.SqlQuery<string>(string.Format("SELECT KANTORID FROM PEGAWAI WHERE PEGAWAIID = '{0}' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", nip)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(_kantorid))
                        {
                            _rst.kantorid = _kantorid;
                        }
                    }
                    else
                    {
                        _rst.kantorid = _kantorid;
                    }
                    bool cek = false;
                    foreach(var x in _rst.kantorids)
                    {
                        if(x.KantorId == _rst.kantorid)
                        {
                            cek = true;
                        }
                    }
                    if (!cek)
                    {
                        _rst.kantorid = _rst.kantorids[0].KantorId;
                    }
                }
                else
                {
                    _rst.kantorid = kantorid;
                    _rst.kantorids = ctx.Database.SqlQuery<Kantor>(string.Format("SELECT KANTORID, NAMA AS NAMAKANTOR FROM KANTOR WHERE KANTORID = '{0}'", kantorid)).ToList();
                }
            }

            return _rst;
        }

        public TransactionResult MergeHakAkses(string jsonData, string pIdPegawai, string pIdUser, string pKantorId)
        {
            TransactionResult tr = new TransactionResult() { Status = true, Pesan = "Hak Akses berhasil diproses" };

            JObject json = JObject.Parse(jsonData);
            List<JObject> on = new List<JObject>();
            List<JObject> off = new List<JObject>();
            foreach (var item in json)
            {
                string val = json[item.Key].ToString();
                JObject obj = JObject.Parse(val);
                dynamic jobj = new JObject();
                jobj.tipe = item.Key;
                jobj.pejabat = obj["pejabat"];
                jobj.ket = obj["ket"];

                if (obj["aktif"].ToString() == "on")
                {
                    on.Add(jobj);
                }
                else
                {
                    off.Add(jobj);
                }
            }

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Regex sWhitespace = new Regex(@"\s+");
                        string sql = string.Empty;
                        var parameters = new object[1] { sWhitespace };

                        foreach (dynamic profile in on)
                        {
                            string tipe = profile.tipe;
                            bool pejabat = profile.pejabat;
                            if (pejabat)
                            {
                                // Cek jika ada pejabat aktif
                                sql = @"SELECT profilepegawaiid FROM jabatanpegawai WHERE profileid = :idProfile AND kantorid = :idKantor AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                OracleParameter p1 = new OracleParameter("idProfile", tipe);
                                OracleParameter p2 = new OracleParameter("idKantor", pKantorId);
                                parameters = new object[2] { p1, p2 };
                                string pejabataktif = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault<string>();
                                if (!String.IsNullOrEmpty(pejabataktif))
                                {
                                    throw new Exception(String.Concat("Jabatan ", profile.ket, " masih dijabat."));
                                }
                            }

                            sql = @"INSERT INTO jabatanpegawai (PROFILEPEGAWAIID,PROFILEID,PEGAWAIID,KANTORID,VALIDSEJAK, USERUPDATE) VALUES (SYS_GUID(),:idProfile,:idPegawai,:idKantor,SYSDATE, :idUser)";
                            OracleParameter p3 = new OracleParameter("idProfile", tipe);
                            OracleParameter p4 = new OracleParameter("idPegawai", pIdPegawai);
                            OracleParameter p5 = new OracleParameter("idKantor", pKantorId);
                            OracleParameter p6 = new OracleParameter("idUser", pIdUser);
                            parameters = new object[4] { p3, p4, p5, p6 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        foreach (dynamic profile in off)
                        {
                            string tipe = profile.tipe;
                            sql = @"UPDATE jabatanpegawai SET validsampai = SYSDATE, USERUPDATE = :idUser, LASTUPDATE = SYSDATE 
                                     WHERE profileid = :idProfile 
                                       AND pegawaiid = :idPegawai 
                                       AND kantorid = :idKantor
                                       AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                            OracleParameter p1 = new OracleParameter("idUser", pIdUser);
                            OracleParameter p2 = new OracleParameter("idProfile", tipe);
                            OracleParameter p3 = new OracleParameter("idPegawai", pIdPegawai);
                            OracleParameter p4 = new OracleParameter("idKantor", pKantorId);
                            parameters = new object[4] { p1, p2, p3, p4 };
                            sql = sWhitespace.Replace(sql, " ");
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Status = false;
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


        #region Delegasi

        public List<ListDelegasi> GetDelegasi(string delegasisuratid, string profilepengirim, string profilepenerima, int from, int to)
        {
            List<ListDelegasi> records = new List<ListDelegasi>();

            ArrayList arrayListParameters = new ArrayList();
            try
            {
                string query =
                    @"SELECT * FROM (
			        SELECT 
				        ROW_NUMBER() over (ORDER BY DS.PROFILEPENGIRIM, DS.PROFILEPENERIMA) RNUMBER, COUNT(1) OVER() TOTAL, 
				        DS.DELEGASISURATID,
				        DS.PROFILEPENGIRIM,
				        U1.UNITKERJAID AS UNITKERJAPENGIRIM,
				        J1.NAMA AS JABATANPENGIRIM,
				        DS.PROFILEPENERIMA,
				        U2.UNITKERJAID AS UNITKERJAPENERIMA,
				        J2.NAMA AS JABATANPENERIMA,
				        DS.TANGGAL,
                        DS.STATUS
			        FROM " + OtorisasiUser.NamaSkema + @".DELEGASISURAT DS
                    INNER JOIN JABATAN J1 ON
				        J1.PROFILEID = DS.PROFILEPENGIRIM
			        INNER JOIN UNITKERJA U1 ON
				        U1.UNITKERJAID = J1.UNITKERJAID
			        INNER JOIN JABATAN J2 ON
				        J2.PROFILEID = DS.PROFILEPENERIMA
			        INNER JOIN UNITKERJA U2 ON
				        U2.UNITKERJAID = J2.UNITKERJAID
                    WHERE DS.PROFILEPENGIRIM IS NOT NULL AND DS.PROFILEPENERIMA IS NOT NULL";


                if (!String.IsNullOrEmpty(delegasisuratid))
                {
                    arrayListParameters.Add(new OracleParameter("DelegasiSuratId", delegasisuratid));
                    query += " AND DS.DELEGASISURATID = :DelegasiSuratId ";
                }
                if (!String.IsNullOrEmpty(profilepengirim))
                {
                    arrayListParameters.Add(new OracleParameter("ProfilePengirim", String.Concat(profilepengirim.ToLower(), "%")));
                    query += " AND LOWER(DS.PROFILEPENGIRIM) LIKE :ProfilePengirim ";
                }
                if (!String.IsNullOrEmpty(profilepenerima))
                {
                    arrayListParameters.Add(new OracleParameter("ProfilePenerima", String.Concat("%", profilepenerima.ToLower(), "%")));
                    query += " AND LOWER(DS.PROFILEPENERIMA) LIKE :ProfilePenerima ";
                }

                query +=
                    " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";
                query = sWhitespace.Replace(query, " ");

                arrayListParameters.Add(new OracleParameter("startCnt", from));
                arrayListParameters.Add(new OracleParameter("limitCnt", to));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    records = ctx.Database.SqlQuery<ListDelegasi>(query, parameters).ToList<ListDelegasi>();
                }
            }
            catch(Exception ex)
            {
                string msg = ex.ToString();
                records = null;
            }

            return records;
        }

        public TransactionResult UpdateDelegasi(ListDelegasi data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (string.IsNullOrEmpty(data.DelegasiSuratId))
                        {
                            sql =
                                "INSERT INTO " + OtorisasiUser.NamaSkema + @".DELEGASISURAT ( " +
                                "            DELEGASISURATID, PROFILEPENGIRIM, PROFILEPENERIMA, TANGGAL, STATUS) VALUES " +
                                "( " +
                                "            SYS_GUID(),:ProfilePengirim,:ProfilePenerima,SYSDATE,:Status)";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("ProfilePengirim", data.ProfilePengirim));
                            arrayListParameters.Add(new OracleParameter("ProfilePenerima", data.ProfilePenerima));
                            arrayListParameters.Add(new OracleParameter("Status", data.Status));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            sql =
                                "UPDATE " + OtorisasiUser.NamaSkema + @".DELEGASISURAT SET " +
                                "       PROFILEPENGIRIM = :ProfilePengirim, " +
                                "       PROFILEPENERIMA = :ProfilePenerima, " +
                                "       TANGGAL = SYSDATE, " +
                                "       STATUS = "+ data.Status + " " +
                                "WHERE DELEGASISURATID = :DelegasiSuratId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("ProfilePengirim", data.ProfilePengirim));
                            arrayListParameters.Add(new OracleParameter("ProfilePenerima", data.ProfilePenerima));
                            arrayListParameters.Add(new OracleParameter("DelegasiSuratId", data.DelegasiSuratId));
                            //arrayListParameters.Add(new OracleParameter("Status", data.Status));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

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

        #endregion


        public ProfileTU GetProfileTUByNipAndKantorID(string pegawaiid, string kantorid) // Arya :: 2020-07-22
        {
            var data = new ProfileTU();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT DISTINCT *
                FROM
                  (SELECT DISTINCT
                     JB.profileidtu AS profileidtu, UK.UNITKERJAID AS unitkerjaid, UK.KANTORID,
                     CAST(SUBSTR(NVL(VP.ESELON,99),0,1) AS NUMBER(1)) AS ESELON, 0 AS PLT
                   FROM simpeg_2702.satker SK
                     INNER JOIN simpeg_2702.v_pegawai_eoffice VP ON
                       VP.SATKERID = SK.SATKERID AND
                       VP.NIPBARU = :param1
                     INNER JOIN SURATTRAIN.MAPPING_UNITKERJA MU ON
                       MU.SATKERID = VP.SATKERID
                     INNER JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = MU.UNITKERJAID AND
                       UK.KANTORID = :param2 AND
                       UK.TAMPIL = 1
                     INNER JOIN JABATAN JB ON
                       JB.UNITKERJAID = UK.UNITKERJAID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       JB.PROFILEIDTU IS NOT NULL AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                   UNION ALL
                   SELECT
                     JB.PROFILEIDTU, JB.UNITKERJAID, JP.KANTORID,
                     NVL(JB.TIPEESELONID,99) AS ESELON, NVL(JP.STATUSPLT,0) AS PLT
                   FROM JABATANPEGAWAI JP
                     INNER JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       JB.PROFILEIDTU IS NOT NULL AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                     INNER JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = JB.UNITKERJAID AND
                       UK.KANTORID = :param3 AND
                       UK.TAMPIL = 1
                   WHERE
                     JP.PEGAWAIID = :param4 AND
                     (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                     NVL(JP.STATUSHAPUS,'0') = '0' )
                ORDER BY PLT, ESELON";

            using (var ctx = new BpnDbContext())
            {
                arrayListParameters.Add(new OracleParameter("param1", pegawaiid));
                arrayListParameters.Add(new OracleParameter("param2", kantorid));
                arrayListParameters.Add(new OracleParameter("param3", kantorid));
                arrayListParameters.Add(new OracleParameter("param4", pegawaiid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<ProfileTU>(query,parameters).FirstOrDefault();
                if(data != null)
                {
                    if (!string.IsNullOrEmpty(data.UnitKerjaId) && !kantorid.Equals(data.KantorId))
                    {
                        var tr = CheckAndReplaceKantorIdJabatanPegawai(pegawaiid, data.UnitKerjaId, data.KantorId);
                    }
                }
            }

            return data;
        }

        public TransactionResult CheckAndReplaceKantorIdJabatanPegawai(string pegawaiid, string unitkerjaid, string kantorid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = @"SELECT COUNT(1) FROM UNITKERJA WHERE UNITKERJAID = :param1 AND KANTORID = :param2";
                        var arrayListParameters = new ArrayList();
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("param1", unitkerjaid));
                        arrayListParameters.Add(new OracleParameter("param2", kantorid));
                        var parameters = arrayListParameters.OfType<object>().ToArray();
                        if(ctx.Database.SqlQuery<int>(sql,parameters).FirstOrDefault() == 0)
                        {
                            sql = @"
                                MERGE
                                INTO    JABATANPEGAWAI JP
                                USING   (
			                                   SELECT
			                                     JP.PROFILEPEGAWAIID AS ppid, UK.KANTORID AS nKid
			                                   FROM JABATANPEGAWAI JP
			                                     INNER JOIN JABATAN JB ON
			                                       JB.PROFILEID = JP.PROFILEID AND
			                                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                                                   JB.PROFILEIDTU IS NOT NULL AND
                                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
			                                     INNER JOIN UNITKERJA UK ON
			                                       UK.UNITKERJAID = JB.UNITKERJAID AND
			                                       UK.KANTORID <> JP.KANTORID AND
                                                   UK.TAMPIL = 1
			                                   WHERE
			                                     JP.PEGAWAIID = :param1 AND
			                                     JP.KANTORID = :param2 AND
                                                 (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
			                                     NVL(JP.STATUSHAPUS,'0') = '0'
                                        ) SRC
                                ON      (JP.PROFILEPEGAWAIID = SRC.ppid)
                                WHEN MATCHED THEN UPDATE
                                    SET JP.KANTORID = SRC.nKid";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", pegawaiid));
                            arrayListParameters.Add(new OracleParameter("param2", kantorid));
                            parameters = arrayListParameters.OfType<object>().ToArray();
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

        public List<ProfilePegawai> GetProfilePegawai_Simpeg(string pegawaiid, string kantorid) // Arya :: 2020-07-24
        {
            var records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT DISTINCT 
                  PROFILEID, NAMAPROFILE, ESELON, PLT --, SRC
                FROM 
                  (SELECT 
                     JB.PROFILEID, JB.NAMA AS NAMAPROFILE, 'SIMPEG' AS SRC, CAST(SUBSTR(NVL(VP.ESELON,99),0,1) AS NUMBER(1)) AS ESELON, 0 AS PLT
                   FROM simpeg_2702.satker SK
                     INNER JOIN simpeg_2702.v_pegawai_eoffice VP ON
                       VP.SATKERID = SK.SATKERID AND
                       VP.NIPBARU = :param1 
                     INNER JOIN KANTOR KT ON
                       (KT.KANTORID = SK.KKP_KANTORID OR KT.KODESATKER = SK.KDSATKER) AND
                       KT.KANTORID = :param2 
                     INNER JOIN JABATAN JB ON
                       JB.PROFILEID LIKE NVL(SK.EOFFICE_PROFILEID,(CASE KT.TIPEKANTORID WHEN 1 THEN SK.EOFFICE_PROFILEID WHEN 2 THEN 'R' || SUBSTR(SK.SATKERID,1,6) WHEN 3 THEN 'N' || SUBSTR(SK.SATKERID,1,6) END) || '%') AND
                       JB.NAMA = VP.NAMAJABATAN AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       JB.PROFILEIDTU IS NOT NULL AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                     INNER JOIN JABATANPEGAWAI JP ON
                       JP.PROFILEID = JB.PROFILEID AND
                       JP.PEGAWAIID = VP.PEGAWAIID
                   UNION ALL 
                   SELECT
                     JP.PROFILEID, JB.NAMA AS NamaProfile, 'EOFFICE' AS SRC, NVL(JB.TIPEESELONID,99) AS ESELON, NVL(JP.STATUSPLT,0) AS PLT
                   FROM JABATANPEGAWAI JP
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       JB.PROFILEIDTU IS NOT NULL AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                   WHERE
                     JP.PEGAWAIID = :param3 AND
                     JP.KANTORID = :param4 AND
                     (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                     NVL(JP.STATUSHAPUS,'0') = '0')
                ORDER BY PLT, ESELON,PROFILEID";

            using (var ctx = new BpnDbContext())
            {
                arrayListParameters.Add(new OracleParameter("param1", pegawaiid));
                arrayListParameters.Add(new OracleParameter("param2", kantorid));
                arrayListParameters.Add(new OracleParameter("param3", pegawaiid));
                arrayListParameters.Add(new OracleParameter("param4", kantorid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query,parameters).ToList();
            }

            return records;
        }

        public TransactionResult SimpegSynch(UserLogin userlogin) // Arya :: 2020-07-22
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(userlogin.PegawaiId))
                        {
                            PegawaiSimpeg datapegawai = new PegawaiSimpeg();

                            sql =
                                "SELECT " +
                                "    nipbaru pegawaiid, nama, nama_lengkap NamaLengkap, " +
                                "    gelardepan, gelarbelakang, alamat, email, " +
                                "    hp nomorhp, satkerid, satker namasatker, " +
                                "    namajabatan " +
                                "FROM " +
                                "    siap_vw_pegawai " +
                                "WHERE " +
                                "    nipbaru = :Nip";

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Nip", userlogin.PegawaiId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            datapegawai = ctx.Database.SqlQuery<PegawaiSimpeg>(sql, parameters).FirstOrDefault();
                            if (datapegawai != null)
                            {
                                // Update PEGAWAI
                                sql = @"UPDATE pegawai SET
                                               nama = :Nama,
                                               jabatan = :Jabatan,
                                               gelardepan = :GelarDepan,
                                               gelarbelakang = :GelarBelakang,
                                               alamat = :Alamat,
                                               nomorhp = :NomorHP
                                        WHERE pegawaiid = :PegawaiId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Nama", datapegawai.Nama));
                                arrayListParameters.Add(new OracleParameter("Jabatan", datapegawai.NamaJabatan));
                                arrayListParameters.Add(new OracleParameter("GelarDepan", datapegawai.GelarDepan));
                                arrayListParameters.Add(new OracleParameter("GelarBelakang", datapegawai.GelarBelakang));
                                arrayListParameters.Add(new OracleParameter("Alamat", datapegawai.Alamat));
                                arrayListParameters.Add(new OracleParameter("NomorHP", datapegawai.NomorHP));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", userlogin.PegawaiId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                // Update USERS
                                sql = @"UPDATE users SET
                                               email = :Email
                                        WHERE userid = :UserId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Email", datapegawai.Email));
                                arrayListParameters.Add(new OracleParameter("UserId", userlogin.UserId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);


                                tr.Status = true;
                                tr.Pesan = "Proses sinkronisasi data Simpeg sudah selesai";
                            }
                            else
                            {
                                tr.Status = false;
                                tr.Pesan = "Gagal mendapatkan data pegawai dari Simpeg";
                            }
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal mendapatkan ID pegawai";
                        }

                        tc.Commit();
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

        public string GetKantorIdFromUnitKerjaId(string unitkerjaid)
        {
            string result = "";

            string query = "SELECT KKP_KANTORID FROM simpeg_2702.satker WHERE SATKERID = :UnitKerjaId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                if (string.IsNullOrEmpty(result))
                {
                    query = "SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = :UnitKerjaId";
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
            }

            return result;
        }

        public List<DataPengguna> ListPegawai(string idkantor)
        {
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            bool IsLoginAdminApp = false;
            if (OtorisasiUser.IsRoleAdministrator() == true)
            {
                IsLoginAdminApp = true;
            }

            List<DataPengguna> _lstPengguna = new List<DataPengguna>();

            using (var ctx = new BpnDbContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                string sql = @"
                    SELECT DISTINCT 
                      PS.NIPBARU AS IDPEGAWAI, 
                      TRIM(PS.NIPBARU || ' - '|| PS.NAMA_LENGKAP) AS NAMALENGKAP
                    FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PS
                      INNER JOIN PEGAWAI PG ON
                        PG.PEGAWAIID = PS.NIPBARU 
                    WHERE
                      (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) ";

                if (!IsLoginAdminApp)
                {
                    sql +=
                    " AND PS.KODESATKER IN (SELECT KODESATKER FROM KANTOR WHERE KANTORID = :idKantor)";

                    arrayListParameters.Add(new OracleParameter("idKantor", idkantor));
                }

                sql += @"
                    UNION ALL
                       SELECT DISTINCT
                         JP.PEGAWAIID, TRIM(PG.PEGAWAIID || ' - '|| CASE WHEN NULLIF(PG.GELARDEPAN,'') IS NULL THEN '' ELSE PG.GELARDEPAN || '. ' END || PG.NAMA || CASE WHEN NULLIF(PG.GELARBELAKANG,'') IS NULL THEN '' ELSE ', ' || PG.GELARBELAKANG END) AS NAMALENGKAP
                       FROM JABATANPEGAWAI JP
                         INNER JOIN PEGAWAI PG ON
                           PG.PEGAWAIID = JP.PEGAWAIID
                       WHERE
                         (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                         NVL(JP.STATUSHAPUS,0) = 0 AND
                         JP.STATUSPLT = 1 ";
                if (!IsLoginAdminApp)
                {
                    sql += string.Format(" AND JP.KANTORID = '{0}'", idkantor);
                }

                sql = string.Concat("SELECT DISTINCT * FROM (",sql,")");
                sql = sWhitespace.Replace(sql, " ");

                parameters = arrayListParameters.OfType<object>().ToArray();

                _lstPengguna = ctx.Database.SqlQuery<DataPengguna>(sql, parameters).ToList();
            }

            return _lstPengguna;
        }

        public List<ProfilePegawai> GetProfilesPegawai(string pegawaiid, string kantorid) // Arya :: 2020-07-28
        {
            List<ProfilePegawai> records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT DISTINCT
                      JP.PROFILEPEGAWAIID,
                      JP.PROFILEID,
                      JP.STATUSPLT,
                      JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)' || DECODE(JP.VALIDSAMPAI, null, '', '<br>[Sampai dengan ' || TO_CHAR(JP.VALIDSAMPAI,'fmDD Month YYYY') || ']'), 2, ' (PLH)' || DECODE(JP.VALIDSAMPAI, null, '', '<br>[Sampai dengan ' || TO_CHAR(JP.VALIDSAMPAI,'fmDD Month YYYY') || ']'), '' || '<br>[' || UK.NAMAUNITKERJA || ']' ) AS NamaProfileLengkap
                    FROM JABATANPEGAWAI JP
                      INNER JOIN JABATAN JB ON
                        JB.PROFILEID = JP.PROFILEID AND 
                        NVL(JB.SEKSIID,'X') <> 'A800'
                      INNER JOIN UNITKERJA UK ON
                        UK.UNITKERJAID = JB.UNITKERJAID AND
                        UK.TAMPIL = 1 AND
                        UK.KANTORID IS NOT NULL
                    WHERE
                      JP.PEGAWAIID = :PegawaiId AND
                      (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                      NVL(JP.STATUSHAPUS,'0') = '0'";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            if (!string.IsNullOrEmpty(kantorid))
            {
                query += " AND JP.KANTORID = :KantorId";
                arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
            }
            query += " ORDER BY NamaProfileLengkap";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).ToList<ProfilePegawai>();
            }

            return records;
        }

        public PegawaiSimpeg GetPegawaiSimpegDetail(string nip, string kantorid) // Arya :: 2020-07-28
        {
            PegawaiSimpeg data = new PegawaiSimpeg();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    nipbaru pegawaiid, nama, nama_lengkap NamaLengkap, " +
                "    gelardepan, gelarbelakang, alamat, email, " +
                "    hp nomorhp, satkerid, satker namasatker, " +
                "    namajabatan " +
                "FROM " +
                "    siap_vw_pegawai " +
                "WHERE " +
                "    nipbaru = :Nip AND STATUSPEGAWAIID IN (1,2,23,24)";


            arrayListParameters.Add(new OracleParameter("Nip", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<PegawaiSimpeg>(query, parameters).FirstOrDefault();
                if (data == null)
                {
                    return null;
                }

                #region Get KantorId Satker
                query = @"
                    SELECT NVL(MUK.UNITKERJAID,SPG.SATKERID) AS UNITKERJAID FROM
                    (SELECT
	                    CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
		                        WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
			                        WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
			                        ELSE SUBSTR(VPE.SATKERID,1,8) END  AS SATKERID, ST.SATKER
                    FROM SIMPEG_2702.V_PEGAWAI_EOFFICE VPE
                    INNER JOIN SIMPEG_2702.SATKER ST ON ST.SATKERID =
	                    CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
		                        WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
			                        WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
			                        ELSE SUBSTR(VPE.SATKERID,1,8) END
                    WHERE VPE.NIPBARU = :param1) SPG
                    LEFT JOIN SURATTRAIN.MAPPING_UNITKERJA MUK ON
	                    MUK.SATKERID = SPG.SATKERID";
                arrayListParameters.Clear();
                arrayListParameters.Add(new OracleParameter("param1", nip));
                parameters = arrayListParameters.OfType<object>().ToArray();
                data.SatkerId = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();

                //data.SatkerId = data.SatkerId.Substring(0, 2).Equals("02") ? data.SatkerId : data.SatkerId.Remove(0, 2);
                int lensatkerid = data.SatkerId.Length;
                for (int i = lensatkerid; i >= 4; i -= 2)
                {
                    string substrsatkerid = data.SatkerId.Substring(0, i);
                    
                    query = "SELECT satker FROM simpeg_2702.satker WHERE satkerid = :SatkerId";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("SatkerId", substrsatkerid));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    string namasatker = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                    if (!string.IsNullOrEmpty(namasatker))
                    {
                        if (substrsatkerid.Length == 6)
                        {
                            query = "SELECT kantorid FROM unitkerja WHERE unitkerjaid = :SatkerId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("SatkerId", substrsatkerid));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            string kantoridsatker = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                            data.KantorId = kantoridsatker;

                            if (data.KantorId != "980FECFC746D8C80E0400B0A9214067D")
                            {
                                data.TipeKantorId = 3;

                                if (namasatker.ToLower().Contains("wilayah"))
                                {
                                    data.TipeKantorId = 2;
                                }
                            }
                        }
                    }
                }

                #endregion

                // Get data KKP
                data.UserId = ctx.Database.SqlQuery<string>(string.Format("SELECT USERID FROM PEGAWAI WHERE PEGAWAIID = '{0}' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", nip)).FirstOrDefault();
                data.Username = ctx.Database.SqlQuery<string>(string.Format("SELECT USERNAME FROM USERS WHERE USERID = '{0}'", data.UserId)).FirstOrDefault();
                if (string.IsNullOrEmpty(kantorid))
                {
                    data.KantorIds = ctx.Database.SqlQuery<Kantor>(string.Format("SELECT DISTINCT JABATANPEGAWAI.KANTORID, KANTOR.NAMA AS NAMAKANTOR FROM JABATANPEGAWAI, KANTOR WHERE JABATANPEGAWAI.KANTORID = KANTOR.KANTORID AND JABATANPEGAWAI.PEGAWAIID = '{0}' AND (JABATANPEGAWAI.VALIDSAMPAI IS NULL OR TRUNC(CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JABATANPEGAWAI.STATUSHAPUS,'0') = '0'", nip)).ToList();
                    data.JabatanKKP = ctx.Database.SqlQuery<string>(string.Format("SELECT NAMA FROM JABATAN WHERE PROFILEID IN (SELECT PROFILEID FROM JABATANPEGAWAI WHERE PEGAWAIID = '{0}' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND STATUSPLT = 0 AND PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300'))", nip)).FirstOrDefault();
                    data.IsActive = (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM JABATANPEGAWAI JP INNER JOIN JABATAN JB ON JB.PROFILEID = JP.PROFILEID AND NVL(JB.SEKSIID,'XXX') <> 'A800' WHERE JP.PEGAWAIID = '{0}' AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JP.STATUSHAPUS,'0') = '0'", nip)).FirstOrDefault()) > 0;
                    if (data.KantorIds.Count == 0)
                    {
                        data.KantorIds = ctx.Database.SqlQuery<Kantor>(string.Format("SELECT KANTORID, NAMA AS NAMAKANTOR FROM KANTOR WHERE KANTORID = '{0}'", data.KantorId)).ToList();
                    }
                }
                else
                {
                    data.KantorIds = ctx.Database.SqlQuery<Kantor>(string.Format("SELECT KANTORID, NAMA AS NAMAKANTOR FROM KANTOR WHERE KANTORID = '{0}'", kantorid)).ToList();
                    data.JabatanKKP = ctx.Database.SqlQuery<string>(string.Format("SELECT NAMA FROM JABATAN WHERE PROFILEID IN (SELECT PROFILEID FROM JABATANPEGAWAI WHERE PEGAWAIID = '{0}' AND KANTORID = '{1}' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND STATUSPLT = 0) AND NVL(SEKSIID,'X') <> 'A800' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", nip, kantorid)).FirstOrDefault();
                    data.IsActive = (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM JABATANPEGAWAI JP INNER JOIN JABATAN JB ON JB.PROFILEID = JP.PROFILEID AND NVL(JB.SEKSIID,'XXX') <> 'A800' WHERE JP.PEGAWAIID = '{0}' AND JP.KANTORID = '{1}' AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JP.STATUSHAPUS,'0') = '0'", nip, kantorid)).FirstOrDefault()) > 0;
                }
                if (data.IsActive)
                {
                    if (OtorisasiUser.IsRoleAdministrator())
                    {
                        data.Total = ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM JABATANPEGAWAI JP INNER JOIN JABATAN JB ON JB.PROFILEID = JP.PROFILEID AND NVL(JB.SEKSIID,'XXX') <> 'A800' WHERE JP.PEGAWAIID = '{0}' AND NVL(JP.STATUSPLT,'0') = '0' AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JP.STATUSHAPUS,'0') = '0'", nip)).FirstOrDefault();
                    }
                    else
                    {
                        data.Total = ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM JABATANPEGAWAI JP INNER JOIN JABATAN JB ON JB.PROFILEID = JP.PROFILEID AND NVL(JB.SEKSIID,'XXX') <> 'A800' WHERE JP.PEGAWAIID = '{0}' AND JP.KANTORID = '{1}' AND NVL(JP.STATUSPLT,'0') = '0' AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(JP.STATUSHAPUS,'0') = '0'", nip, kantorid)).FirstOrDefault();
                    }
                }
                data.JabatanKKP = (string.IsNullOrEmpty(data.JabatanKKP) ? "" : data.JabatanKKP);
            }

            return data;
        }

        public TransactionResult SynchUser(string nip, string kid, string uid, string kegiatan) // Arya :: 2020-07-28
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(nip))
                        {
                            PegawaiSimpeg datapegawai = new PegawaiSimpeg();

                            sql = @"
                                SELECT
                                  nipbaru pegawaiid, nama, nama_lengkap NamaLengkap, gelardepan, gelarbelakang,
                                  alamat, email, hp nomorhp, satkerid, satker namasatker, namajabatan, SUBSTR(ESELONID,0,1) AS eselon, tipepegawaiid, simpeg_2702.EOFFICE_GETSATKERINDUK(satkerid) satkerinduk
                                FROM siap_vw_pegawai
                                WHERE
                                  nipbaru = :Nip AND STATUSPEGAWAIID IN (1,2,23,24)";

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Nip", nip));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            datapegawai = ctx.Database.SqlQuery<PegawaiSimpeg>(sql, parameters).FirstOrDefault();
                            if (datapegawai != null)
                            {
                                sql = @"UPDATE PEGAWAI SET
                                               NAMA = :Nama,
                                               JABATAN = :Jabatan,
                                               GELARDEPAN = :GelarDepan,
                                               GELARBELAKANG = :GelarBelakang,
                                               ALAMAT = :Alamat,
                                               NOMORHP = :NomorHP,
                                               EMAIL = :Email,
                                               USERUPDATE = :UserId,
                                               LASTUPDATE = SYSDATE
                                        WHERE PEGAWAIID = :PegawaiId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Nama", datapegawai.Nama));
                                arrayListParameters.Add(new OracleParameter("Jabatan", datapegawai.NamaJabatan));
                                arrayListParameters.Add(new OracleParameter("GelarDepan", datapegawai.GelarDepan));
                                arrayListParameters.Add(new OracleParameter("GelarBelakang", datapegawai.GelarBelakang));
                                arrayListParameters.Add(new OracleParameter("Alamat", datapegawai.Alamat));
                                arrayListParameters.Add(new OracleParameter("NomorHP", datapegawai.NomorHP));
                                arrayListParameters.Add(new OracleParameter("Email", datapegawai.Email));
                                arrayListParameters.Add(new OracleParameter("UserId", uid));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);

                                sql = @"
                                    SELECT
                                      USERID
                                    FROM PEGAWAI
                                    WHERE
                                      PEGAWAIID = :PegawaiId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                datapegawai.UserId = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();

                                sql = @"UPDATE users SET
                                               email = NVL(:Email,email)
                                        WHERE userid = :UserId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Email", datapegawai.Email));
                                arrayListParameters.Add(new OracleParameter("UserId", datapegawai.UserId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                
                                #region Hapus Jabatan
                                if (!string.IsNullOrEmpty(kegiatan) && kegiatan.Equals("HapusJabatan"))
                                {
                                    sql = @"
                                        UPDATE JABATANPEGAWAI SET
                                            VALIDSAMPAI = SYSDATE,
                                            USERUPDATE = :UserId,
                                            LASTUPDATE = SYSDATE
                                        WHERE PEGAWAIID = :Nip AND 
                                            STATUSPLT = 0 AND 
                                            (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND 
                                            NVL(STATUSHAPUS,'0') = '0' AND
                                            KANTORID = :KantorId";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("UserId", uid));
                                    arrayListParameters.Add(new OracleParameter("Nip", nip));
                                    arrayListParameters.Add(new OracleParameter("KantorId", kid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);

                                    tr.Status = true;
                                    tc.Commit();
                                    return tr;
                                }
                                #endregion

                                sql = @"
                                    SELECT NVL(MUK.UNITKERJAID,SPG.SATKERID) AS UNITKERJAID FROM
                                    (SELECT
	                                    CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
		                                       WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
			                                     WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
			                                     ELSE SUBSTR(VPE.SATKERID,1,8) END  AS SATKERID, ST.SATKER
                                    FROM SIMPEG_2702.V_PEGAWAI_EOFFICE VPE
                                    INNER JOIN SIMPEG_2702.SATKER ST ON ST.SATKERID =
	                                    CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
		                                       WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
			                                     WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
			                                     ELSE SUBSTR(VPE.SATKERID,1,8) END
                                    WHERE VPE.NIPBARU = :Nip) SPG
                                    LEFT JOIN SURATTRAIN.MAPPING_UNITKERJA MUK ON
	                                    MUK.SATKERID = SPG.SATKERID";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("Nip", nip));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                string _sid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                datapegawai.SatkerInduk = string.IsNullOrEmpty(_sid) ? datapegawai.SatkerInduk : _sid;
                                if (!(datapegawai.SatkerId.Substring(0, datapegawai.SatkerInduk.Length).Equals(datapegawai.SatkerInduk)))
                                {
                                    datapegawai.SatkerId = string.Concat(datapegawai.SatkerInduk, datapegawai.SatkerId.Substring(datapegawai.SatkerInduk.Length));
                                }

                                sql = @"SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = :UnitKerjaId";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("UnitKerjaId", datapegawai.SatkerInduk));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                string _KantorId = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                kid = string.IsNullOrEmpty(_KantorId)?kid: _KantorId;
                                int _tipeKantorId = ctx.Database.SqlQuery<int>(string.Format(@"SELECT TIPEKANTORID
                                                                                                    FROM KANTOR
                                                                                                   WHERE KANTORID = '{0}'", kid)).FirstOrDefault();
                                _tipeKantorId = (_tipeKantorId == 0) ? 1 : _tipeKantorId;

                                if (string.IsNullOrEmpty(kid))
                                {
                                    tr.Status = false;
                                    tr.Pesan = "Gagal mendapatkan unit kerja aktif";
                                    tc.Rollback();
                                }else
                                {
                                    string _profileID = string.Empty;// ctx.Database.SqlQuery<string>(string.Format("SELECT EOFFICE_PROFILEID FROM simpeg_2702.satker WHERE SATKERID = '{0}'", datapegawai.SatkerId)).FirstOrDefault();

                                    if (string.IsNullOrEmpty(_profileID)) // Arya :: 2021-08-13 :: Synch Pusat
                                    {
                                        if (_tipeKantorId == 1)
                                        {
                                            if (datapegawai.SatkerId.Length > 4)
                                            {
                                                _profileID = string.Concat("H", decimal.Parse(datapegawai.SatkerId.Substring(4)));
                                                for (int i = _profileID.Length; i < 8; i++)
                                                {
                                                    _profileID += "0";
                                                }
                                            }
                                        }
                                        string _qr = "";
                                        if (string.IsNullOrEmpty(datapegawai.Eselon))
                                        {
                                            if (datapegawai.TipePegawaiId.Equals("2"))
                                            {
                                                _qr = "AND LENGTH(PROFILEID) > 8";
                                            }
                                        }

                                        int point = 90;
                                        do
                                        {
                                            _profileID = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID LIKE '{0}%' {3} AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND UNITKERJAID = '{2}' AND UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) >= {4} ORDER BY UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) DESC", _profileID, datapegawai.NamaJabatan, datapegawai.SatkerInduk, _qr, point)).FirstOrDefault();
                                            point -= 5;
                                        } while (string.IsNullOrEmpty(_profileID) && point > 50);
                                    }
                                    if (string.IsNullOrEmpty(_profileID))
                                    {
                                        if (_tipeKantorId == 1)
                                        {
                                            _profileID = "H" + datapegawai.SatkerId.Substring(0, 6);
                                        }
                                        else if (_tipeKantorId == 2)
                                        {
                                            _profileID = "R" + datapegawai.SatkerId.Substring(0, 6);
                                        }
                                        else if (_tipeKantorId == 3 || _tipeKantorId == 4)
                                        {
                                            _profileID = "N" + datapegawai.SatkerId.Substring(0, 6);
                                        }
                                        if (string.IsNullOrEmpty(datapegawai.Eselon))
                                        {
                                            if (datapegawai.TipePegawaiId.Equals("2"))
                                            {
                                                _profileID += "02";
                                            }
                                        }
                                        _profileID = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID LIKE '{0}%' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND UNITKERJAID = '{2}' AND UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) >= 75 ORDER BY UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) DESC", _profileID, datapegawai.NamaJabatan, datapegawai.SatkerInduk)).FirstOrDefault();
                                    }
                                    if (string.IsNullOrEmpty(_profileID))
                                    {
                                        if (_tipeKantorId == 1)
                                        {
                                            _profileID = "H" + datapegawai.SatkerId;
                                        }
                                        else if (_tipeKantorId == 2)
                                        {
                                            _profileID = "R" + datapegawai.SatkerId;
                                        }
                                        else if (_tipeKantorId == 3 || _tipeKantorId == 4)
                                        {
                                            _profileID = "N" + datapegawai.SatkerId;
                                        }
                                        if (string.IsNullOrEmpty(datapegawai.Eselon))
                                        {
                                            if (datapegawai.TipePegawaiId.Equals("2"))
                                            {
                                                _profileID += "02";
                                            }
                                        }
                                        _profileID = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID = '{0}' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) >= 75 ORDER BY UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) DESC", _profileID, datapegawai.NamaJabatan)).FirstOrDefault();
                                    }
                                    if (string.IsNullOrEmpty(_profileID)) // Arya :: 2020-10-05 :: perubahan prefix simpeg
                                    {
                                        _tipeKantorId = (_tipeKantorId == 0) ? 1 : _tipeKantorId;
                                        if (datapegawai.SatkerId.Substring(0, 2).Equals("98")) datapegawai.SatkerId = datapegawai.SatkerId.Remove(0, 2);
                                        if (datapegawai.SatkerInduk.Substring(0, 2).Equals("98")) datapegawai.SatkerInduk = datapegawai.SatkerInduk.Remove(0, 2);
                                        if (_tipeKantorId == 1)
                                        {
                                            _profileID = "H" + datapegawai.SatkerId;
                                        }
                                        else if (_tipeKantorId == 2)
                                        {
                                            _profileID = "R" + datapegawai.SatkerId.Substring(0, 6);
                                        }
                                        else if (_tipeKantorId == 3 || _tipeKantorId == 4)
                                        {
                                            _profileID = "N" + datapegawai.SatkerId.Substring(0, 6);
                                        }
                                        if (string.IsNullOrEmpty(datapegawai.Eselon))
                                        {
                                            if (datapegawai.TipePegawaiId.Equals("2"))
                                            {
                                                _profileID += "02";
                                            }
                                        }
                                        _profileID = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID LIKE '{0}%' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) >= 75 ORDER BY UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) DESC", _profileID, datapegawai.NamaJabatan)).FirstOrDefault();
                                    }

                                    #region Jabatan Baru
                                    if (string.IsNullOrEmpty(_profileID) && !string.IsNullOrEmpty(kegiatan) && kegiatan.Equals("TambahJabatan"))
                                    {
                                        string _prefix = string.Empty;
                                        string ProfileInduk = string.Empty;
                                        if (_tipeKantorId == 1)
                                        {
                                            int ct = datapegawai.SatkerId.Length;
                                            if (ct > 4)
                                            {
                                                _prefix = "H";
                                                _profileID = string.Concat("H", decimal.Parse(datapegawai.SatkerId.Substring(4)));
                                                ProfileInduk = _profileID.Substring(0, _profileID.Length-2);
                                                for (int i = _profileID.Length; i < 8; i++)
                                                {
                                                    _profileID += "0";
                                                }
                                                for (int i = ProfileInduk.Length; i < 8; i++)
                                                {
                                                    ProfileInduk += "0";
                                                }
                                                //_profileID = "P" + datapegawai.SatkerId.Substring(4, datapegawai.SatkerId.Length - 4);
                                            }
                                            else
                                            {
                                                _prefix = "H";
                                                _profileID = "P";
                                            }
                                            int len = _profileID.Length;
                                            if (len < 8)
                                            {
                                                for (int i = 0; i < 7 - len; i++)
                                                {
                                                    _profileID += "0";
                                                }
                                                int kt = ctx.Database.SqlQuery<int>(string.Format("SELECT CAST(NVL(MAX(REPLACE(PROFILEID,'{0}',''))+1,'0') AS NUMBER) FROM JABATAN WHERE PROFILEID LIKE '{0}%' AND UNITKERJAID = '{1}'", _profileID, datapegawai.SatkerId)).FirstOrDefault();
                                                if (datapegawai.Eselon == "1")
                                                {
                                                    ProfileInduk = "H0000001";
                                                }
                                                else
                                                {
                                                    ProfileInduk = _profileID + "0";
                                                }
                                                _profileID += kt.ToString();
                                            }

                                            //if (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(0) FROM UNITKERJA WHERE UNITKERJAID = '{0}'", datapegawai.SatkerId)).FirstOrDefault() == 0)
                                            //{
                                            //    string satker = ctx.Database.SqlQuery<string>(string.Format("SELECT SATKER FROM SIMPEG_2702.SATKER WHERE SATKERID = '{0}'", datapegawai.SatkerId)).FirstOrDefault();
                                            //    sql = @"
                                            //        INSERT INTO UNITKERJA (UNITKERJAID, NAMAUNITKERJA, TAMPIL, TIPEKANTORID, KANTORID)
                                            //        VALUES (:SatkerId, :NamaSatker, 1, 1, :KantorId)";
                                            //    arrayListParameters.Clear();
                                            //    arrayListParameters.Add(new OracleParameter("SatkerId", datapegawai.SatkerId));
                                            //    arrayListParameters.Add(new OracleParameter("NamaSatker", satker));
                                            //    arrayListParameters.Add(new OracleParameter("KantorId", kid));
                                            //    parameters = arrayListParameters.OfType<object>().ToArray();
                                            //    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            //}
                                        }
                                        else if (_tipeKantorId == 2)
                                        {
                                            _prefix = "R";
                                            _profileID = "R" + datapegawai.SatkerId;
                                        }
                                        else if (_tipeKantorId == 3 || _tipeKantorId == 4)
                                        {
                                            _prefix = "N";
                                            _profileID = "N" + datapegawai.SatkerId;
                                        }
                                        if (string.IsNullOrEmpty(datapegawai.Eselon))
                                        {
                                            if (!datapegawai.TipePegawaiId.Equals("1"))
                                            {
                                                ProfileInduk = _profileID;
                                                int i = 2;
                                                for (i = 2; i < 100; i++)
                                                {
                                                    string test_profileID = string.Concat(_profileID, ((100 + i).ToString().Substring(1, 2)));
                                                    if (!string.IsNullOrEmpty(ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID = '{0}'", test_profileID)).FirstOrDefault()))
                                                    {
                                                        if (!string.IsNullOrEmpty(ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID = '{0}%' AND UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) >= 75 ORDER BY UTL_MATCH.edit_distance_similarity(REPLACE(LOWER(NAMA),' ',''), REPLACE(LOWER('{1}'),' ','')) DESC", test_profileID, datapegawai.NamaJabatan)).FirstOrDefault()))
                                                        {
                                                            _profileID = test_profileID;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _profileID = test_profileID;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        string cek = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEID FROM JABATAN WHERE PROFILEID = '{0}'", _profileID)).FirstOrDefault();
                                        if (string.IsNullOrEmpty(cek))
                                        {
                                            sql = @"SELECT PROFILEIDTU FROM JABATAN WHERE PROFILEID = :Profileid";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("Profileid", ProfileInduk));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            string _profileTU = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                            string _profileBA = ctx.Database.SqlQuery<string>(@"SELECT PROFILEIDBA FROM JABATAN WHERE PROFILEID = :Profileid", parameters).FirstOrDefault();

                                            string _satkerid = datapegawai.SatkerId;
                                            if (_satkerid.Length > 6)
                                            {
                                                _satkerid = _satkerid.Substring(0, _satkerid.Length - 2);
                                            }
                                            if (string.IsNullOrEmpty(_profileTU))
                                            {

                                                _profileTU = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEIDTU FROM JABATAN WHERE UNITKERJAID = '{0}' AND PROFILEIDTU IS NOT NULL", _satkerid)).FirstOrDefault();
                                                _profileBA = ctx.Database.SqlQuery<string>(string.Format("SELECT PROFILEIDBA FROM JABATAN WHERE UNITKERJAID = '{0}' AND PROFILEIDTU IS NOT NULL", _satkerid)).FirstOrDefault();
                                            }
                                            if (string.IsNullOrEmpty(_profileTU))
                                            {
                                                if (string.IsNullOrEmpty(ctx.Database.SqlQuery<string>(string.Format("SELECT UNITKERJAID FROM UNITKERJA WHERE KANTORID = '{0}'", kid)).FirstOrDefault()))
                                                {
                                                    tr.Status = false;
                                                    tr.Pesan = "Gagal mendapatkan data unit kerja eoffice";
                                                    tc.Rollback();
                                                    return tr;
                                                }
                                                //tr.Status = false;
                                                //tr.Pesan = "Gagal mendapatkan data unit kerja eoffice";
                                                //tc.Rollback();
                                            }
                                            sql = @"
                                            INSERT INTO JABATAN (PROFILEID, NAMA, USERUPDATE, LASTUPDATE, TIPEESELONID, JABATANLAMA, VALIDSEJAK, INDUK, KATEGORI, UNITKERJAID, PROFILEIDTU, PROFILEIDBA)
                                            VALUES (:ProfileId, :ProfileName, :UserId, SYSDATE, :Eselon, 0, SYSDATE, :Induk, 1, :SatkerId, :ProfileIdTu, :ProfileIdBA)";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                            arrayListParameters.Add(new OracleParameter("ProfileName", datapegawai.NamaJabatan));
                                            arrayListParameters.Add(new OracleParameter("UserId", uid));
                                            arrayListParameters.Add(new OracleParameter("Eselon", datapegawai.Eselon));
                                            arrayListParameters.Add(new OracleParameter("Induk", ProfileInduk));
                                            arrayListParameters.Add(new OracleParameter("SatkerId", _satkerid));
                                            arrayListParameters.Add(new OracleParameter("ProfileIdTu", _profileTU));
                                            arrayListParameters.Add(new OracleParameter("ProfileIdBA", _profileBA));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }
                                    }
                                    #endregion

                                    if (!string.IsNullOrEmpty(_profileID))
                                    {
                                        #region Cek Duplikasi

                                        bool BisaSimpanProfile = true;
                                        bool MatikanProfileLain = true;
                                        sql = "SELECT UK.KANTORID FROM JABATAN JB INNER JOIN UNITKERJA UK ON UK.UNITKERJAID = JB.UNITKERJAID WHERE JB.PROFILEID = :ProfileId AND (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        string _kantorprofile = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                        _kantorprofile = (string.IsNullOrEmpty(_kantorprofile)) ? kid : _kantorprofile;

                                        sql =
                                            "SELECT count(*) FROM jabatanpegawai " +
                                            "WHERE profileid = :ProfileId AND pegawaiid = :PegawaiId " +
                                            "AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                        arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
                                        //arrayListParameters.Add(new OracleParameter("KantorId", kid));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                        if (jumlahdata > 0)
                                        {
                                            BisaSimpanProfile = false;
                                            sql = "SELECT KANTORID FROM JABATANPEGAWAI WHERE PEGAWAIID = :PegawaiId AND PROFILEID = :ProfileId AND KANTORID <> :KantorId AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
                                            arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                            arrayListParameters.Add(new OracleParameter("KantorId", _kantorprofile));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            string _kid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                            if (_kantorprofile == kid && _kantorprofile == (string.IsNullOrEmpty(_kid) ? _kantorprofile : _kid))
                                            {
                                                sql = "UPDATE JABATAN SET NAMA = :Jabatan, LASTUPDATE = SYSDATE, USERUPDATE = :userId, TIPEESELONID = :Eselon WHERE PROFILEID = :ProfileId AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new OracleParameter("Jabatan", datapegawai.NamaJabatan));
                                                arrayListParameters.Add(new OracleParameter("UserId", datapegawai.UserId));
                                                arrayListParameters.Add(new OracleParameter("Eselon", datapegawai.Eselon));
                                                arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                            else
                                            {
                                                sql = @"
                                                    UPDATE JABATANPEGAWAI SET
                                                        VALIDSAMPAI = TRUNC(SYSDATE-1),
                                                        USERUPDATE = :UserId,
                                                        LASTUPDATE = SYSDATE
                                                    WHERE PEGAWAIID = :Nip AND 
                                                        PROFILEID IN (SELECT PROFILEID FROM JABATAN WHERE NVL(SEKSIID,'X') <> 'A800' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))) AND
                                                        STATUSPLT = 0 AND 
                                                        (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                                                        NVL(STATUSHAPUS,'0') = '0' AND
                                                        PROFILEID = :ProfileId AND
                                                        KANTORID <> :KantorId";
                                                sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new OracleParameter("UserId", uid));
                                                arrayListParameters.Add(new OracleParameter("Nip", nip));
                                                arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                                arrayListParameters.Add(new OracleParameter("KantorId", _kantorprofile));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                        }
                                        #endregion

                                        sql = @"
                                            SELECT COUNT(1) FROM JABATANPEGAWAI
                                            WHERE PEGAWAIID = :Nip AND 
                                                PROFILEID IN (SELECT PROFILEID FROM JABATAN WHERE NVL(SEKSIID,'X') <> 'A800' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))) AND
                                                STATUSPLT = 0 AND 
                                                (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND 
                                                NVL(STATUSHAPUS,'0') = '0' AND
                                                PROFILEID <> :ProfileId";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("Nip", nip));
                                        arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                        //arrayListParameters.Add(new OracleParameter("KantorId", _kantorprofile));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                        MatikanProfileLain = (jumlahdata > 0);

                                        if (MatikanProfileLain)
                                        {
                                            sql = @"
                                            UPDATE JABATANPEGAWAI SET
                                                VALIDSAMPAI = TRUNC(SYSDATE-1),
                                                USERUPDATE = :UserId,
                                                LASTUPDATE = SYSDATE
                                            WHERE PEGAWAIID = :Nip AND 
                                                STATUSPLT = 0 AND 
                                                (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND 
                                                NVL(STATUSHAPUS,'0') = '0' AND
                                                PROFILEID IN (SELECT PROFILEID FROM JABATAN WHERE NVL(SEKSIID,'X') <> 'A800' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))) AND
                                                PROFILEID <> :ProfileId";
                                            sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("UserId", uid));
                                            arrayListParameters.Add(new OracleParameter("Nip", nip));
                                            arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                                        }

                                        if (BisaSimpanProfile)
                                        {
                                            sql =
                                                @"INSERT INTO jabatanpegawai (
                                                  profilepegawaiid, profileid, pegawaiid, kantorid, validsejak, keterangan,
                                                  userupdate, lastupdate) VALUES 
                                                  (
                                                  SYS_GUID(),:ProfileId,:PegawaiId,:KantorId,SYSDATE,:Keterangan,
                                                  :UserUpdate,SYSDATE)";
                                            sql = sWhitespace.Replace(sql, " ");
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                            arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
                                            arrayListParameters.Add(new OracleParameter("KantorId", _kantorprofile));
                                            arrayListParameters.Add(new OracleParameter("Keterangan", "synch simpeg"));
                                            arrayListParameters.Add(new OracleParameter("UserUpdate", uid));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                                            sql = string.Format("SELECT COUNT(1) FROM JABATAN WHERE PROFILEID = '{0}' AND PROFILEIDTU = '{0} AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))'", _profileID);
                                            if(ctx.Database.SqlQuery<int>(sql).FirstOrDefault() > 0)
                                            {
                                                if(_tipeKantorId > 1)
                                                {
                                                    string _adminId = _tipeKantorId == 2 ? "A80500" : "A80400";

                                                    sql = @"UPDATE PROFILEPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :idUser, LASTUPDATE = SYSDATE, KETERANGAN = 'Pengaturan User Eoffice'  
                                                             WHERE PROFILEID = :idProfile 
                                                               AND PEGAWAIID <> :idPegawai 
                                                               AND KANTORID = :idKantor
                                                               AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                                               AND NVL(STATUSHAPUS,'0') = '0'";
                                                    sql = sWhitespace.Replace(sql, " ");
                                                    arrayListParameters.Clear();
                                                    arrayListParameters.Add(new OracleParameter("idUser", uid));
                                                    arrayListParameters.Add(new OracleParameter("idProfile", _adminId));
                                                    arrayListParameters.Add(new OracleParameter("idPegawai", nip));
                                                    arrayListParameters.Add(new OracleParameter("idKantor", _kantorprofile));
                                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                                    ctx.Database.ExecuteSqlCommand(sql, parameters);

                                                    sql = "SELECT COUNT(1) FROM PROFILEPEGAWAI WHERE PROFILEID = :idProfile AND PEGAWAIID = :idPegawai AND KANTORID = :idKantor AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                                    sql = sWhitespace.Replace(sql, " ");
                                                    arrayListParameters.Clear();
                                                    arrayListParameters.Add(new OracleParameter("idProfile", _adminId));
                                                    arrayListParameters.Add(new OracleParameter("idPegawai", nip));
                                                    arrayListParameters.Add(new OracleParameter("idKantor", _kantorprofile));
                                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() == 0)
                                                    {
                                                        sql =
                                                            @"INSERT INTO PROFILEPEGAWAI (
                                                              PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, KETERANGAN,
                                                              BISABOOKING, USERUPDATE, LASTUPDATE) VALUES 
                                                              (
                                                              SYS_GUID(), :ProfileId, :PegawaiId, :KantorId, SYSDATE, 'Pengaturan User Eoffice',
                                                              0, :UserUpdate, SYSDATE)";
                                                        sql = sWhitespace.Replace(sql, " ");
                                                        arrayListParameters.Clear();
                                                        arrayListParameters.Add(new OracleParameter("ProfileId", _adminId));
                                                        arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
                                                        arrayListParameters.Add(new OracleParameter("KantorId", _kantorprofile));
                                                        arrayListParameters.Add(new OracleParameter("UserUpdate", uid));
                                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                                    }
                                                }
                                            }

                                            //sql = "SELECT NVL(TIPEESELONID,9) FROM JABATAN WHERE PROFILEID = ProfileId AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))";
                                            //sql = sWhitespace.Replace(sql, " ");
                                            //arrayListParameters.Clear();
                                            //arrayListParameters.Add(new OracleParameter("ProfileId", _profileID));
                                            //parameters = arrayListParameters.OfType<object>().ToArray();
                                            //var _eselon = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
                                            if (!(datapegawai.TipePegawaiId.Equals("2") || datapegawai.TipePegawaiId.Equals("3")))
                                            {
                                                sql = @"UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :idUser, LASTUPDATE = SYSDATE, KETERANGAN = 'Pengaturan User'  
                                                         WHERE PROFILEID = :idProfile 
                                                           AND PEGAWAIID <> :idPegawai 
                                                           AND KANTORID = :idKantor
                                                           AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                                           AND NVL(STATUSHAPUS,'0') = '0'";
                                                sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new OracleParameter("idUser", uid));
                                                arrayListParameters.Add(new OracleParameter("idProfile", _profileID));
                                                arrayListParameters.Add(new OracleParameter("idPegawai", nip));
                                                arrayListParameters.Add(new OracleParameter("idKantor", _kantorprofile));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                        }
                                        tr.Status = true;
                                        tc.Commit();
                                        //tc.Rollback();
                                    }
                                    else
                                    {
                                        tr.Status = false;
                                        tr.Pesan = "Gagal mendapatkan profile jabatan eoffice";
                                        tr.ReturnValue = "TambahJabatan";
                                        tc.Rollback();
                                    }
                                }
                            }
                            else
                            {
                                tr.Status = false;
                                tr.Pesan = "Gagal mendapatkan data pegawai dari Simpeg";
                                tc.Rollback();
                            }
                        }
                        else
                        {
                            tr.Status = false;
                            tr.Pesan = "Gagal mendapatkan ID pegawai";
                            tc.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Status = false;
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

        public List<UnitKerja> GetListUnitKerjaByKantorId(string kid, bool aktif,bool cek)
        {
            var records = new List<UnitKerja>();

            string query = @"
                SELECT 
                  UNITKERJAID, NAMAUNITKERJA
                FROM UNITKERJA
                WHERE
                  KANTORID = '{0}' AND
                  UNITKERJAID IS NOT NULL";
            if (aktif)
            {
                query += " AND TAMPIL = 1 ";
            }
            if (cek)
            {
                query = query.Replace("KANTORID = '{0}' AND","");
            }
            else
            {
                query = string.Format(query, kid);
            }

            query += "ORDER BY UNITKERJAID";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<UnitKerja>(query).ToList();
            }

            return records;
        }

        public Kantor getSimpegKantorId(string nip, string kid)
        {
            string kantorid = string.Empty;
            var kantor = new Kantor();

            using (var ctx = new BpnDbContext())
            {
                var kantorids = new List<string>();
                string kodesatker = ctx.Database.SqlQuery<string>(string.Format(@"SELECT KODESATKER FROM SIMPEG_2702.V_PEGAWAI_EOFFICE WHERE NIPBARU = '{0}'", nip)).FirstOrDefault();
                if (!string.IsNullOrEmpty(kodesatker))
                {
                    //kantorid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT KANTORID FROM KANTOR WHERE KODESATKER = '{0}'", kodesatker)).FirstOrDefault();
                    kantorids = ctx.Database.SqlQuery<string>(string.Format(@"SELECT KANTORID FROM KANTOR WHERE KODESATKER = '{0}'", kodesatker)).ToList();
                    if(kantorids.Count == 1)
                    {
                        kantorid = kantorids[0];
                    }
                    else
                    {
                        string unitkerjaid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT SATKERINDUK FROM SIMPEG_2702.V_PEGAWAI_EOFFICE WHERE NIPBARU = '{0}'", nip)).FirstOrDefault();
                        if (!unitkerjaid.Substring(0, 2).Equals("02")) unitkerjaid = unitkerjaid.Remove(0, 2);
                        kantorid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = '{0}'", unitkerjaid)).FirstOrDefault();
                    }
                    //if (string.IsNullOrEmpty(kantorid))
                    //{
                    //    string unitkerjaid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT SATKERINDUK FROM SIMPEG_2702.V_PEGAWAI_EOFFICE WHERE NIPBARU = '{0}'", nip)).FirstOrDefault();
                    //    if (!unitkerjaid.Substring(0, 1).Equals("02")) unitkerjaid = unitkerjaid.Remove(0, 2);
                    //    kantorid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = '{0}'", unitkerjaid)).FirstOrDefault();
                    //}
                }
                else
                {
                    kantorid = ctx.Database.SqlQuery<string>(string.Format(@"
                        SELECT K.KANTORID FROM KANTOR K
                        INNER JOIN SIMPEG_2702.SATKER S ON
	                        S.KKP_KANTORID = K.KANTORID
                        INNER JOIN SIMPEG_2702.V_PEGAWAI_EOFFICE P ON
	                        P.NIPBARU = '{0}' AND
	                        P.SATKERINDUK = S.SATKERID", nip)).FirstOrDefault();
                    if (string.IsNullOrEmpty(kantorid))
                    {
                        if (kid == "980FECFC746D8C80E0400B0A9214067D")
                        {
                            kantorid = kid;
                        }
                        else
                        {
                            string unitkerjaid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT SATKERINDUK FROM SIMPEG_2702.V_PEGAWAI_EOFFICE WHERE NIPBARU = '{0}'", nip)).FirstOrDefault();
                            if (!unitkerjaid.Substring(0, 2).Equals("02")) unitkerjaid = unitkerjaid.Remove(0, 2);
                            kantorid = ctx.Database.SqlQuery<string>(string.Format(@"SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = '{0}'", unitkerjaid)).FirstOrDefault();
                        }
                    }
                }
            }
            kantor.KantorId = kantorid;
            kantor.NamaKantor = GetNamaKantorById(kantorid);
            return kantor;
        }

        public List<ProfileTU> GetProfileIfTU(string pegawaiid, string kantorid) // Arya :: 2020-07-22
        {
            var data = new List<ProfileTU>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  JB.PROFILEIDTU, JB.UNITKERJAID, JP.KANTORID
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    JB.PROFILEIDTU IS NOT NULL AND
                    JP.PROFILEID = JB.PROFILEIDTU
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID AND
                    UK.KANTORID = :Param1
                WHERE
                  JP.PEGAWAIID = :Param2 AND
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  NVL(JP.STATUSHAPUS,'0') = '0'
                GROUP BY JB.PROFILEIDTU, JB.UNITKERJAID, JP.KANTORID";
            arrayListParameters.Clear();
            arrayListParameters.Add(new OracleParameter("Param1", kantorid));
            arrayListParameters.Add(new OracleParameter("Param2", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<ProfileTU>(query,parameters).ToList();
            }

            return data;
        }

        public bool CheckIsTU(string pegawaiid, string kantorid)
        {
            bool result = false;

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  COUNT(JB.UNITKERJAID)
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                    JB.PROFILEIDTU IS NOT NULL AND
                    (JP.PROFILEID = JB.PROFILEIDTU OR JP.PROFILEID = JB.PROFILEIDBA)
                  INNER JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID AND
                    UK.KANTORID = :Param1
                WHERE
                  JP.PEGAWAIID = :Param2 AND
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  NVL(JP.STATUSHAPUS,'0') = '0'";
            arrayListParameters.Clear();
            arrayListParameters.Add(new OracleParameter("Param1", kantorid));
            arrayListParameters.Add(new OracleParameter("Param2", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query,parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public string GetKantorIdByUnitKerjaId(string unitkerjaid)
        {
            string result = "";

            string query = "SELECT KANTORID FROM UNITKERJA WHERE UNITKERJAID = :UnitKerjaId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        class SP
        {
            public string StatusPegawaiId { get; set; }
            public string StatusPegawai { get; set; }
        }

        public TransactionResult GetTipeUser(string nr, string kid, string uid = null) // Arya :: 2020-09-10
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    ArrayList arrayListParameters = new ArrayList();
                    string sql = @"SELECT COUNT(1) FROM SIMPEG_2702.V_PEGAWAI_EOFFICE WHERE NIPBARU = :nip";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("nip", nr));
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                    {
                        tr.Status = true;
                        tr.ReturnValue = "ASN";
                        if (!string.IsNullOrEmpty(uid))
                        {
                            sql = @"
                                SELECT COUNT(1)
                                FROM JABATANPEGAWAI JP
                                  INNER JOIN JABATAN JB ON
                                    JB.PROFILEID = JP.PROFILEID AND
                                    JB.UNITKERJAID = :unit 
                                WHERE
                                  JP.PEGAWAIID = :nip AND
                                  JP.KANTORID = :kid AND
                                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                  NVL(JP.STATUSHAPUS,'0') = '0'";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("unit", uid));
                            arrayListParameters.Add(new OracleParameter("nip", nr));
                            arrayListParameters.Add(new OracleParameter("kid", kid));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() == 0)
                            {
                                tr.Status = false;
                                tr.Pesan = "Pegawai tidak terdaftar di unit anda";
                                return tr;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(kid))
                            {
                                int tipekantorid = GetTipeKantor(kid);
                                sql = @"
                                SELECT COUNT(1) AS CT
                                FROM JABATANPEGAWAI JP
                                  INNER JOIN JABATAN JB ON
                                    JB.PROFILEID = JP.PROFILEID AND
                                    NVL(JB.SEKSIID,'X') <> 'A800' AND
                                    (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                  INNER JOIN UNITKERJA UK ON
                                    UK.UNITKERJAID = JB.UNITKERJAID AND
                                    UK.TAMPIL = 1 AND
                                    UK.KANTORID = :param1
                                WHERE
                                  JP.PEGAWAIID = :param2 AND
                                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                  NVL(JP.STATUSHAPUS,'0') = '0'";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("param1", kid));
                                arrayListParameters.Add(new OracleParameter("param2", nr));
                                if (tipekantorid == 2)
                                {
                                    sql = string.Concat("SELECT SUM(CT) FROM (", sql, @" UNION ALL
                                            SELECT COUNT(1) AS CT
                                            FROM JABATANPEGAWAI JP
                                              INNER JOIN JABATAN JB ON
                                                JB.PROFILEID = JP.PROFILEID AND
                                                NVL(JB.SEKSIID,'X') <> 'A800' AND
                                                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                              INNER JOIN UNITKERJA UK ON
                                                UK.UNITKERJAID = JB.UNITKERJAID AND
                                                UK.TAMPIL = 1 AND
                                                UK.KANTORID = :param3
                                            WHERE
                                              JP.PEGAWAIID = :param4 AND
                                              (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                              NVL(JP.STATUSHAPUS,'0') = '0'", ") RST");
                                    arrayListParameters.Add(new OracleParameter("param3", kid));
                                    arrayListParameters.Add(new OracleParameter("param4", nr));
                                }
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() == 0)
                                {
                                    sql = @"
                                        SELECT
	                                        COUNT(1) AS CT
                                        FROM
                                        (SELECT
                                         CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
                                             WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
                                            WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
                                            ELSE SUBSTR(VPE.SATKERID,1,8) END  AS SATKERID, ST.SATKER
                                        FROM SIMPEG_2702.V_PEGAWAI_EOFFICE VPE
                                        INNER JOIN SIMPEG_2702.SATKER ST ON ST.SATKERID =
                                         CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
                                             WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
                                            WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
                                            ELSE SUBSTR(VPE.SATKERID,1,8) END
                                        WHERE VPE.NIPBARU = :param1) SPG
                                        LEFT JOIN SURATTRAIN.MAPPING_UNITKERJA MUK ON
                                         MUK.SATKERID = SPG.SATKERID
                                        INNER JOIN UNITKERJA UK ON
	                                        UK.UNITKERJAID = NVL(MUK.UNITKERJAID,SPG.SATKERID) AND
	                                        UK.TAMPIL = 1 AND
	                                        UK.KANTORID = :param2";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("param1", nr));
                                    arrayListParameters.Add(new OracleParameter("param2", kid));
                                    if (tipekantorid == 2)
                                    {
                                        sql = string.Concat("SELECT SUM(CT) FROM (", sql, @" UNION ALL
                                                SELECT
	                                                COUNT(1) AS CT
                                                FROM
                                                (SELECT
                                                 CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
                                                     WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
                                                    WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
                                                    ELSE SUBSTR(VPE.SATKERID,1,8) END  AS SATKERID, ST.SATKER
                                                FROM SIMPEG_2702.V_PEGAWAI_EOFFICE VPE
                                                INNER JOIN SIMPEG_2702.SATKER ST ON ST.SATKERID =
                                                 CASE WHEN LENGTH(VPE.SATKERID) < 6 THEN VPE.SATKERID
                                                     WHEN SUBSTR(VPE.SATKERID,1,4) <> '0201' THEN SUBSTR(VPE.SATKERID,1,6)
                                                    WHEN CAST(SUBSTR(VPE.SATKERID,5,2) AS INTEGER) > 9 THEN SUBSTR(VPE.SATKERID,1,6)
                                                    ELSE SUBSTR(VPE.SATKERID,1,8) END
                                                WHERE VPE.NIPBARU = :param3) SPG
                                                LEFT JOIN SURATTRAIN.MAPPING_UNITKERJA MUK ON
                                                 MUK.SATKERID = SPG.SATKERID
                                                INNER JOIN UNITKERJA UK ON
	                                                UK.UNITKERJAID = NVL(MUK.UNITKERJAID,SPG.SATKERID) AND
	                                                UK.TAMPIL = 1
                                                INNER JOIN KANTOR KT ON
  	                                                KT.KANTORID = UK.KANTORID AND
  	                                                KT.INDUK = :param4", ") RST");
                                        arrayListParameters.Add(new OracleParameter("param3", nr));
                                        arrayListParameters.Add(new OracleParameter("param4", kid));
                                    }
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() == 0)
                                    {
                                        tr.Status = false;
                                        tr.Pesan = "Pegawai tidak terdaftar di unit anda";
                                        return tr;
                                    }
                                }
                            }
                        }
                        sql = @"SELECT VALIDSAMPAI FROM PEGAWAI WHERE PEGAWAIID = :nip AND (VALIDSAMPAI IS NOT NULL AND TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) < TRUNC(SYSDATE))";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("nip", nr));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        DateTime? validsampai = ctx.Database.SqlQuery<DateTime?>(sql, parameters).FirstOrDefault();
                        if (validsampai != null)
                        {
                            sql = @"
                                    MERGE
                                    INTO    PEGAWAI PG
                                    USING   (SELECT P2.NIPBARU,
                                                    TRUNC(CAST(NVL(P1.TMTPENSIUN,P1.TGLPENSIUN) AS TIMESTAMP)) AS TMTPENSIUN
                                            FROM
                                                    SIMPEG_2702.PENSIUN2 P1
                                            INNER JOIN SIMPEG_2702.PEGAWAI P2 ON P2.PEGAWAIID = P1.PEGAWAIID
                                            ) SP
                                    ON
				                                    (SP.NIPBARU = PG.PEGAWAIID AND PG.PEGAWAIID = :nip)
                                    WHEN MATCHED THEN
                                    UPDATE SET PG.VALIDSAMPAI = SP.TMTPENSIUN";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("nip", nr));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                    }
                    else
                    {
                        var sp = ctx.Database.SqlQuery<SP>("SELECT STATUSPEGAWAIID,STATUSPEGAWAI FROM SIAP_VW_PEGAWAI WHERE NIPBARU = :nip AND STATUSPEGAWAIID IN ('14','3','5','6','8','22')", parameters).FirstOrDefault();
                        if (sp != null)
                        {
                            ctx.Database.ExecuteSqlCommand(@"UPDATE PEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1) WHERE PEGAWAIID = :nip AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", parameters);
                            ctx.Database.ExecuteSqlCommand(@"UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1) WHERE PEGAWAIID = :nip AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", parameters);
                            tr.Status = false;
                            tr.Pesan = string.Concat("Status : ", sp.StatusPegawai);
                        }
                        else
                        {
                            sql = @"SELECT COUNT(1) FROM PPNPN WHERE NIK = :nik AND NVL(STATUSHAPUS,'0') = '0'";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("nik", nr));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                            {
                                tr.Status = true;
                                tr.ReturnValue = "PPNPN";
                                sql = @"SELECT PPNPNID FROM PPNPN WHERE NIK = :nik AND STATUSVALIDASI = '1' AND TANGGALVALIDASI IS NOT NULL";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("nik", nr));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                string _ppnpnid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                if (string.IsNullOrEmpty(_ppnpnid))
                                {
                                    tr.Status = false;
                                    tr.Pesan = "Data PPNPN belum di validasi";
                                }
                                else
                                {
                                    sql = @"SELECT COUNT(1) FROM PPNPN.RIWAYATKONTRAK WHERE PPNPNID = :ppnpnid AND NVL(STATUSHAPUS,'0')='0' AND TRUNC(CAST(NVL(TANGGALSELESAIKONTRAK,(ADD_MONTHS(TRUNC(TANGGALKONTRAK, 'y'), 12) - 1)) AS TIMESTAMP)) >= TRUNC(SYSDATE)";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("ppnpnid", _ppnpnid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                                    {
                                        if (!string.IsNullOrEmpty(kid))
                                        {
                                            sql = @"SELECT KANTORID FROM PPNPN WHERE NIK = :nik AND STATUSVALIDASI = '1' AND TANGGALVALIDASI IS NOT NULL";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("nik", nr));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            string _kid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                            if (string.IsNullOrEmpty(_kid))
                                            {
                                                tr.Status = false;
                                                tr.Pesan = "Data Kantor PPNPN belum didaftarkan";
                                            }
                                            else
                                            {
                                                if (kid != _kid)
                                                {
                                                    sql = @"SELECT NAMA FROM KANTOR WHERE KANTORID = :kid";
                                                    arrayListParameters.Clear();
                                                    arrayListParameters.Add(new OracleParameter("kid", _kid));
                                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                                    string kn = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                                    tr.Status = false;
                                                    tr.Pesan = string.Concat("Data PPNPN terdaftar di\n", kn);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("nik", nr));
                                        ctx.Database.ExecuteSqlCommand(@"UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1) WHERE PEGAWAIID = :nik AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", arrayListParameters.ToArray());
                                        tr.Status = false;
                                        tr.Pesan = "Tidak Ditemukan Kontrak PPNPN Aktif";
                                    }
                                }
                            }
                            else
                            {
                                sql = @"SELECT COUNT(1) FROM PIHAK3 WHERE NIK = :nik AND NVL(STATUSHAPUS,'0') = '0'";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("nik", nr));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                                {
                                    tr.Status = true;
                                    tr.ReturnValue = "PIHAK3";
                                    sql = @"
                                        SELECT COUNT(1)
                                        FROM MITRAKERJA.REGISTERUSERPERTANAHAN RP
                                          INNER JOIN PIHAK3 P ON
                                            P.NIK = RP.NIK AND
  	                                        NVL(P.STATUSHAPUS,'0') = '0'
                                          INNER JOIN MITRAKERJA.DETAILKONTRAKPIHAK3 DKP ON
  	                                        DKP.PIHAK3ID = P.PIHAK3ID AND
  	                                        NVL(DKP.STATUSHAPUS,'0') = '0'
                                          INNER JOIN MITRAKERJA.KONTRAKPIHAK3 KP ON
  	                                        KP.KONTRAKPIHAK3ID = DKP.KONTRAKPIHAK3ID AND
  	                                        NVL(KP.STATUSHAPUS,'0') = '0' AND
  	                                        (KP.TANGGALMULAI IS NULL OR TRUNC(CAST(KP.TANGGALMULAI AS TIMESTAMP)) <= TRUNC(SYSDATE)) AND
  	                                        (KP.TANGGALSELESAI IS NULL OR TRUNC(CAST(KP.TANGGALSELESAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
  	                                        KP.SATUANKERJA = :param1
                                        WHERE
                                          RP.NIK = :param2 AND
                                          RP.STATUS = 'USER SUDAH AKTIF'";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("param1", kid));
                                    arrayListParameters.Add(new OracleParameter("param2", nr));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                                    {

                                    }
                                    else
                                    {
                                        tr.Status = false;
                                        tr.Pesan = "Tidak Ditemukan Kontrak Aktif di kantor ini";
                                    }
                                }
                                else
                                {
                                    tr.Status = false;
                                    tr.Pesan = "NIP/NIK yang anda masukkan tidak terdaftar";
                                }    
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    tr.Status = false;
                    tr.Pesan = ex.Message.ToString();
                }
            }
            return tr;
        }

        public UserPPNPN GetPPNPNDetail(string nik, string kantorid) // Arya :: 2020-09-10
        {
            var data = new UserPPNPN();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  UR.NIK, UR.NAMA, UR.NOHP AS NOMORTELEPON, UR.USERID,
                  UP.USERNAME, UP.EMAIL, UK.NAMAUNITKERJA AS SATKER, UR.KANTORID, KT.NAMA AS KANTORNAMA, DECODE(UR.KANTORID,UK.KANTORID,'1','0') AS INSATKER
                FROM PPNPN UR
                  INNER JOIN KANTOR KT ON
                    KT.KANTORID = UR.KANTORID
                  LEFT JOIN USERPPNPN UP ON
                    UP.USERID = UR.USERID
                  LEFT JOIN JABATANPEGAWAI JP ON
                    JP.PEGAWAIID = UR.NIK AND
                    JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300') AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                  LEFT JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND JB.NAMA = 'PPNPN'
                  LEFT JOIN UNITKERJA UK ON
                    UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
	                UR.NIK = :nik AND
	                UR.STATUSVALIDASI = '1' AND
                  UR.TANGGALVALIDASI IS NOT NULL";


            arrayListParameters.Add(new OracleParameter("nik", nik));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<UserPPNPN>(query, parameters).FirstOrDefault();
                if (data == null)
                {
                    return null;
                }
            }

            return data;
        }

        public TransactionResult AktifasiPPNPN(string nik, string uid, string kid, string ukd, bool isAdmin) // Arya :: 2020-09-11
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        string sql = @"SELECT COUNT(1) FROM PPNPN WHERE NIK = :nik AND STATUSVALIDASI = '1' AND TANGGALVALIDASI IS NOT NULL";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("nik", nik));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() == 0)
                        {
                            tr.Status = false;
                            tr.Pesan = "Data PPNPN belum di validasi";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(kid))
                            {
                                sql = @"SELECT KANTORID FROM PPNPN WHERE NIK = :nik AND STATUSVALIDASI = '1' AND TANGGALVALIDASI IS NOT NULL";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("nik", nik));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                string _kid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                if (string.IsNullOrEmpty(_kid))
                                {
                                    tr.Status = false;
                                    tr.Pesan = "Data Kantor PPNPN belum didaftarkan";
                                }
                                else
                                {
                                    if (isAdmin && _kid != "980FECFC746D8C80E0400B0A9214067D")
                                    {
                                        sql = @"SELECT UNITKERJAID FROM UNITKERJA WHERE KANTORID = :kid";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("kid", _kid));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ukd = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                    }
                                    if (kid != _kid && !isAdmin)
                                    {
                                        sql = @"SELECT NAMA FROM KANTOR WHERE KANTORID = :kid";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("kid", _kid));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        string kn = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                        tr.Status = false;
                                        tr.Pesan = string.Concat("Data PPNPN terdaftar di\n", kn);
                                    }
                                    else
                                    {
                                        sql = "SELECT PROFILEID FROM JABATAN WHERE NAMA = 'PPNPN' AND UNITKERJAID = :ukd AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("ukd", ukd));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        string _profileid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                                        if (string.IsNullOrEmpty(_profileid))
                                        {
                                            tr.Status = false;
                                            tr.Pesan = "Jabatan PPNPN tidak ditemukan";
                                        }
                                        else
                                        {
                                            #region Cek Duplikasi

                                            bool BisaSimpanProfile = true;
                                            bool MatikanProfileLain = true;
                                            sql =
                                                "SELECT COUNT(*) FROM JABATANPEGAWAI " +
                                                "WHERE PROFILEID = :ProfileId AND PEGAWAIID = :PegawaiId AND KANTORID = :KantorId " +
                                                "AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("ProfileId", _profileid));
                                            arrayListParameters.Add(new OracleParameter("PegawaiId", nik));
                                            arrayListParameters.Add(new OracleParameter("KantorId", _kid));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                            if (jumlahdata > 0)
                                            {
                                                BisaSimpanProfile = false;
                                            }
                                            sql = @"
                                            SELECT COUNT(1)
                                            FROM JABATANPEGAWAI PG
                                            INNER JOIN JABATAN JB ON
	                                            NVL(JB.SEKSIID,'X') <> 'A800' AND (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                                            WHERE
                                              PG.PEGAWAIID = :Nik AND
                                              PG.STATUSPLT = 0 AND
                                              (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                                              NVL(PG.STATUSHAPUS,'0') = '0' AND
                                              PG.PROFILEID <> :ProfileId";
                                            arrayListParameters.Clear();
                                            arrayListParameters.Add(new OracleParameter("Nik", nik));
                                            arrayListParameters.Add(new OracleParameter("ProfileId", _profileid));
                                            parameters = arrayListParameters.OfType<object>().ToArray();
                                            jumlahdata = ctx.Database.SqlQuery<int>(sql, parameters).First();
                                            MatikanProfileLain = (jumlahdata > 0);
                                            #endregion

                                            if (MatikanProfileLain)
                                            {
                                                sql = @"
                                                UPDATE JABATANPEGAWAI SET
                                                    VALIDSAMPAI = TRUNC(SYSDATE-1),
                                                    USERUPDATE = :UserId,
                                                    LASTUPDATE = SYSDATE
                                                WHERE PEGAWAIID = :Nip AND 
                                                    STATUSPLT = 0 AND 
                                                    (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND 
                                                    NVL(STATUSHAPUS,'0') = '0' AND
                                                    PROFILEID IN (SELECT PROFILEID FROM JABATAN WHERE NVL(SEKSIID,'X') <> 'A800') AND
                                                    PROFILEID <> :ProfileId";
                                                sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new OracleParameter("UserId", uid));
                                                arrayListParameters.Add(new OracleParameter("Nik", nik));
                                                arrayListParameters.Add(new OracleParameter("ProfileId", _profileid));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }

                                            if (BisaSimpanProfile)
                                            {
                                                sql = @"
                                                INSERT INTO JABATANPEGAWAI (
                                                      PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, KETERANGAN,
                                                      USERUPDATE, LASTUPDATE) 
                                                VALUES (
                                                      SYS_GUID(),:ProfileId,:PegawaiId,:KantorId,SYSDATE,:Keterangan,
                                                      :UserUpdate,SYSDATE)";
                                                sql = sWhitespace.Replace(sql, " ");
                                                arrayListParameters.Clear();
                                                arrayListParameters.Add(new OracleParameter("ProfileId", _profileid));
                                                arrayListParameters.Add(new OracleParameter("PegawaiId", nik));
                                                arrayListParameters.Add(new OracleParameter("KantorId", _kid));
                                                arrayListParameters.Add(new OracleParameter("Keterangan", "synch ppnpn"));
                                                arrayListParameters.Add(new OracleParameter("UserUpdate", uid));
                                                parameters = arrayListParameters.OfType<object>().ToArray();
                                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                            }
                                            tr.Status = true;
                                            tc.Commit();
                                        }
                                    }
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

        public TransactionResult NonAktifasiPPNPN(string nik, string uid) // Arya :: 2020-09-15
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        string sql = @"
                            UPDATE JABATANPEGAWAI SET
                                VALIDSAMPAI = TRUNC(SYSDATE-1),
                                USERUPDATE = :UserId,
                                LASTUPDATE = SYSDATE
                            WHERE PEGAWAIID = :Nip AND 
                                (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND 
                                NVL(STATUSHAPUS,'0') = '0'";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("UserId", uid));
                        arrayListParameters.Add(new OracleParameter("Nik", nik));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                        tr.Status = true;
                        tc.Commit();
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

        //public TransactionResult ResetSandi(string pid, string pass, string tip) // Arya :: 2020-09-28
        //{
        //    TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
        //    using (var ctx = new BpnDbContext())
        //    {
        //        using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                ArrayList arrayListParameters = new ArrayList();
        //                object[] parameters = null;
        //                string sql = string.Empty;
        //                string tbl1 = string.Empty;
        //                string tbl2 = string.Empty;
        //                string field = string.Empty;
        //                if (tip == "ASN")
        //                {
        //                    tbl1 = "PEGAWAI";
        //                    tbl2 = "USERS";
        //                    field = "PEGAWAIID";
        //                } else if (tip == "PPNPN")
        //                {
        //                    tbl1 = "PPNPN";
        //                    tbl2 = "USERPPNPN";
        //                    field = "NIK";
        //                }
        //                else
        //                {
        //                    tc.Rollback();
        //                    tr.Pesan = "Tipe Akun Tidak Ditemukan";
        //                    return tr;
        //                }
        //                sql = string.Format("SELECT USERID FROM {0} WHERE {1} = :Id",tbl1, field);
        //                sql = sWhitespace.Replace(sql, " ");
        //                arrayListParameters.Clear();
        //                arrayListParameters.Add(new OracleParameter("Id", pid));
        //                parameters = arrayListParameters.OfType<object>().ToArray();
        //                pid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();

        //                sql = string.Format("UPDATE {0} SET PASSWORD = :Password, LASTPASSWORDCHANGEDDATE = SYSDATE WHERE USERID = :Id", tbl2);
        //                sql = sWhitespace.Replace(sql, " ");
        //                arrayListParameters.Clear();
        //                arrayListParameters.Add(new OracleParameter("Password", EncodePassword(pass)));
        //                arrayListParameters.Add(new OracleParameter("Id", pid));
        //                parameters = arrayListParameters.OfType<object>().ToArray();
        //                ctx.Database.ExecuteSqlCommand(sql, parameters);
        //                tr.Status = true;
        //                tc.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                tc.Rollback();
        //                tr.Pesan = ex.Message.ToString();
        //            }
        //            finally
        //            {
        //                tc.Dispose();
        //                ctx.Dispose();
        //            }
        //        }
        //    }
        //    return tr;
        //}

        public List<AksesKhusus> GetAksesKhusus(string pid, string kid)
        {
            string skema = OtorisasiUser.NamaSkema;
            var result = new List<AksesKhusus>();
            string query = string.Format(@"
                SELECT
	                JB.PROFILEID,
	                JB.NAMA AS PROFILENAMA,
	                DECODE(JP.PROFILEID,NULL,0,1) AS STATUS
                FROM JABATAN JB
                LEFT JOIN JABATANPEGAWAI JP ON
	                JP.PROFILEID = JB.PROFILEID AND
	                JP.PEGAWAIID = '{0}' AND
	                JP.KANTORID = '{1}' AND
	                NVL(JP.STATUSHAPUS,'0') = '0' AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                WHERE
                  JB.PROFILEID IN ('A81001','A81002','A81003','A81004','A80100','A80300') AND
                  (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", pid, kid);
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<AksesKhusus>(query).ToList();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public TransactionResult SetHakAkses(string pid, string kid, string uid, string unit, bool isAdmin, bool vA81001, bool vA81002, bool vA81003, bool vA81004, bool vA80100, bool vA80300)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            Regex sWhitespace = new Regex(@"\s+");

            string sql = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        List<AksesKhusus> listAkses = GetAksesKhusus(pid, kid);
                        foreach(var akses in listAkses)
                        {
                            bool oldS = (akses.Status == 1) ? true : false;
                            bool newS = true;
                            if(akses.ProfileId.Equals("A81001"))
                            {
                                newS = (isAdmin || unit.Equals("02010208") || !kid.Equals("980FECFC746D8C80E0400B0A9214067D")) ? vA81001: oldS;
                            }
                            else if (akses.ProfileId.Equals("A81002"))
                            {
                                newS = vA81002;
                            }
                            else if (akses.ProfileId.Equals("A81003"))
                            {
                                newS = vA81003;
                            }
                            else if (akses.ProfileId.Equals("A81004"))
                            {
                                newS = vA81004;
                            }
                            else if (akses.ProfileId.Equals("A80100"))
                            {
                                newS = (isAdmin && unit.Equals("020116")) ? vA80100:oldS;
                                if (!kid.Equals("980FECFC746D8C80E0400B0A9214067D"))
                                {
                                    newS = false;
                                }
                            }
                            else if (akses.ProfileId.Equals("A80300"))
                            {
                                newS = isAdmin?vA80300:oldS;
                            }
                            if (oldS != newS)
                            {
                                if (newS)
                                {
                                    int cek = JumlahProfilePegawai(akses.ProfileId, pid, kid);
                                    if (cek == 0)
                                    {
                                        sql =
                                            @"INSERT INTO JABATANPEGAWAI (
                                                      PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, KETERANGAN,
                                                      USERUPDATE, LASTUPDATE) VALUES (
                                                      SYS_GUID(),:ProfileId,:PegawaiId,:KantorId,SYSDATE,:Keterangan,
                                                      :UserUpdate,SYSDATE)";
                                        sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("ProfileId", akses.ProfileId));
                                        arrayListParameters.Add(new OracleParameter("PegawaiId", pid));
                                        arrayListParameters.Add(new OracleParameter("KantorId", kid));
                                        arrayListParameters.Add(new OracleParameter("Keterangan", "menu Pengaturan User"));
                                        arrayListParameters.Add(new OracleParameter("UserUpdate", uid));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                }
                                else
                                {
                                    sql = @"UPDATE JABATANPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :idUser, LASTUPDATE = SYSDATE 
                                                 WHERE PROFILEID = :idProfile 
                                                   AND PEGAWAIID = :idPegawai 
                                                   AND KANTORID = :idKantor
                                                   AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                                                   AND NVL(STATUSHAPUS,'0') = '0'";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("idUser", uid));
                                    arrayListParameters.Add(new OracleParameter("idProfile", akses.ProfileId));
                                    arrayListParameters.Add(new OracleParameter("idPegawai", pid));
                                    arrayListParameters.Add(new OracleParameter("idKantor", kid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                        }
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

        public bool CheckIsPLT(string pegawaiid, string profileid, string kantorid)
        {
            bool result = false;

            string query = string.Format(@"
                SELECT COUNT(1)
                FROM JABATANPEGAWAI
                WHERE
	                PROFILEID = '{0}' AND
	                PEGAWAIID = '{1}' AND
	                KANTORID = '{2}' AND
	                (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(STATUSHAPUS,'0') = '0' AND
	                STATUSPLT = '1'", profileid, pegawaiid, kantorid);

            using (var ctx = new BpnDbContext())
            {
                result = (ctx.Database.SqlQuery<int>(query).First() > 0);
            }

            return result;
        }

        public string GetPhone(string pid, string tipe)
        {
            var result = string.Empty;
            string query = string.Empty;
            if (tipe == "ASN")
            {
                query = string.Format(@"
                    SELECT NOMORHP
                    FROM PEGAWAI
                    WHERE
	                    PEGAWAIID = '{0}'", pid);
            }
            else if(tipe == "PPNPN")
            {
                query = string.Format(@"
                    SELECT NOHP
                    FROM PPNPN
                    WHERE
	                    NIK = '{0}'", pid);
            }

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<string>(query).FirstOrDefault() ;
            }

            return result;
        }

        public TransactionResult UpdateAkunSaya(string uid, string pid, string tipe, string tlp, string pss)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(uid))
                        {
                            if (tipe == "ASN")
                            {
                                if (pss != "********")
                                {
                                    sql = "SELECT COUNT(1) FROM USERS WHERE PASSWORD != :Password AND USERID = :Id ";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("Password", EncodePassword(pss)));
                                    arrayListParameters.Add(new OracleParameter("Id", uid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    if (ctx.Database.SqlQuery<int>(sql,parameters).FirstOrDefault() > 0)
                                    {
                                        sql = "UPDATE USERS SET PASSWORD = :Password, LASTPASSWORDCHANGEDDATE = SYSDATE WHERE USERID = :Id";
                                        sql = sWhitespace.Replace(sql, " ");
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                    else
                                    {
                                        tc.Rollback();
                                        tc.Dispose();
                                        ctx.Dispose();
                                        tr.Pesan = "Password harus berbeda dengan yang sebelumnya";
                                        return tr;
                                    }
                                }
                                sql =
                                    @"UPDATE pegawai SET
                                         nomorhp = :NomorTelepon
                                  WHERE pegawaiid = :PegawaiId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("NomorTelepon", tlp));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", pid));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                            else if(tipe == "PPNPN")
                            {
                                if (pss != "********")
                                {
                                    sql = "SELECT COUNT(1) FROM USERPPNPN WHERE PASSWORD != :Password AND USERID = :Id ";
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("Password", EncodePassword(pss)));
                                    arrayListParameters.Add(new OracleParameter("Id", uid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    if (ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0)
                                    {
                                        sql = "UPDATE USERPPNPN SET PASSWORD = :Password, LASTPASSWORDCHANGEDDATE = SYSDATE WHERE USERID = :Id";
                                        sql = sWhitespace.Replace(sql, " ");
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                    else
                                    {
                                        tc.Rollback();
                                        tc.Dispose();
                                        ctx.Dispose();
                                        tr.Pesan = "Password harus berbeda dengan yang sebelumnya";
                                        return tr;
                                    }
                                }
                                sql =
                                    @"UPDATE PPNPN SET
                                         NOHP = :NomorTelepon
                                  WHERE NIK = :PegawaiId";
                                sql = sWhitespace.Replace(sql, " ");
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new OracleParameter("NomorTelepon", tlp));
                                arrayListParameters.Add(new OracleParameter("PegawaiId", pid));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                            }
                        }
                        else
                        {
                            tc.Rollback();
                            tr.Pesan = "Akun Tidak Ditemukan";
                        }

                        tc.Commit();
                        //tc.Rollback();
                        tr.Status = true;
                        tr.ReturnValue = uid;
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

        public int JumlahDataAktif(string tipe, string unit)
        {
            int result = 0;
            string query = @"
               SELECT
                 JB.UNITKERJAID, UK.NAMAUNITKERJA, UK.TIPEKANTORID,
                 COUNT(SI.NIP) CT
               FROM JABATANPEGAWAI JP
                 LEFT JOIN ( SELECT DISTINCT SI.NIP FROM SURAT.SURATINBOX SI ) SI ON
                   JP.PEGAWAIID = SI.NIP
                 INNER JOIN JABATAN JB ON
                   JB.PROFILEID = JP.PROFILEID AND
                   NVL(JB.SEKSIID,'X') <> 'A800' AND
                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                 INNER JOIN UNITKERJA UK ON
                   UK.UNITKERJAID = JB.UNITKERJAID
                 INNER JOIN KANTOR KT ON
                   UK.KANTORID = KT.KANTORID
               WHERE
                 (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                 NVL(JP.STATUSHAPUS,'0') = '0' AND
                 NVL(JP.STATUSPLT,'0') = '0'
               GROUP BY
                 JB.UNITKERJAID, UK.NAMAUNITKERJA, UK.TIPEKANTORID";

            string field = string.Empty;
            switch (tipe)
            {
                case "CT":
                    field = "COUNT(1)";
                    break;
                case "SM":
                    field = "SUM(CT)";
                    break;
            }
            switch (unit)
            {
                case "pusat":
                    query =string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID = 1");
                    break;
                case "kanwil":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID = 2");
                    break;
                case "kantah":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID > 2");
                    break;
                case "kanwil_a":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID = 2 AND CT > 0");
                    break;
                case "kantah_a":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID > 2 AND CT > 0");
                    break;
                case "total":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST");
                    break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                using (var ctx = new BpnDbContext())
                {
                    result = ctx.Database.SqlQuery<int>(query).First();
                }
            }

            return result;
        }

        public int JumlahTTEAktif(string tipe, string unit)
        {
            int result = 0;
            string query = @"
               SELECT
                 JB.UNITKERJAID, UK.NAMAUNITKERJA, UK.TIPEKANTORID,
                 COUNT(SI.NIP) CT
               FROM JABATANPEGAWAI JP
                 LEFT JOIN ( SELECT DISTINCT SI.NIP FROM SURAT.SURATINBOX SI ) SI ON
                   JP.PEGAWAIID = SI.NIP
                 INNER JOIN JABATAN JB ON
                   JB.PROFILEID = JP.PROFILEID AND
                   NVL(JB.SEKSIID,'X') <> 'A800' AND
                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                 INNER JOIN UNITKERJA UK ON
                   UK.UNITKERJAID = JB.UNITKERJAID
                 INNER JOIN KANTOR KT ON
                   UK.KANTORID = KT.KANTORID
               WHERE
                 (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                 NVL(JP.STATUSHAPUS,'0') = '0' AND
                 NVL(JP.STATUSPLT,'0') = '0'
               GROUP BY
                 JB.UNITKERJAID, UK.NAMAUNITKERJA, UK.TIPEKANTORID";

            string field = string.Empty;
            switch (tipe)
            {
                case "CT":
                    field = "COUNT(1)";
                    break;
                case "SM":
                    field = "SUM(CT)";
                    break;
            }
            switch (unit)
            {
                case "pusat":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID = 1");
                    break;
                case "kanwil":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID = 2");
                    break;
                case "kantah":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID > 2");
                    break;
                case "kanwil_a":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID = 2 AND CT > 0");
                    break;
                case "kantah_a":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST WHERE TIPEKANTORID > 2 AND CT > 0");
                    break;
                case "total":
                    query = string.Concat("SELECT ", field, " FROM (", query, ") RST");
                    break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                using (var ctx = new BpnDbContext())
                {
                    result = ctx.Database.SqlQuery<int>(query).First();
                }
            }

            return result;
        }

        public int JumlahDataTotal(string unit)
        {
            int result = 0;
            string query = @"
                SELECT
                  COUNT(1)
                FROM UNITKERJA UK
                  INNER JOIN KANTOR KT ON
                    KT.KANTORID = UK.KANTORID
                WHERE
	                UK.TAMPIL = 1";

            string field = string.Empty;
            switch (unit)
            {
                case "kanwil":
                    query = string.Concat(query, " AND UK.TIPEKANTORID = 2");
                    break;
                case "kantah":
                    query = string.Concat(query, " AND UK.TIPEKANTORID = 3");
                    break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                using (var ctx = new BpnDbContext())
                {
                    result = ctx.Database.SqlQuery<int>(query).First();
                }
            }

            return result;
        }

        //public List<DaftarKantor> DataKantorBelumDaftar(string unit, int from, int to)
        //{
        //    var result = new List<DaftarKantor>();
        //    string query = @"
        //        SELECT
        //          ROW_NUMBER() OVER (ORDER BY KT.NAMA) AS RNUMBER, COUNT(1) OVER() TOTAL,
        //          KT.KANTORID AS IDKANTOR, KT.NAMA AS NAMAKANTOR
        //        FROM UNITKERJA UK
	       //         INNER JOIN KANTOR KT ON
		      //          KT.KANTORID = UK.KANTORID
        //          LEFT JOIN (
        //             SELECT DISTINCT
        //               UK.KANTORID, UK.NAMAUNITKERJA
        //             FROM JABATANPEGAWAI JP
        //               INNER JOIN JABATAN JB ON
        //                 JB.PROFILEID = JP.PROFILEID AND
        //                 NVL(JB.SEKSIID,'X') <> 'A800' AND
        //                 (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
        //               INNER JOIN UNITKERJA UK ON
        //                 UK.UNITKERJAID = JB.UNITKERJAID
        //               INNER JOIN KANTOR KT ON
        //                 UK.KANTORID = KT.KANTORID
        //             WHERE
        //               (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
        //               NVL(JP.STATUSHAPUS,'0') = '0' AND
        //               NVL(JP.STATUSPLT,'0') = '0')RST ON
        //            RST.KANTORID = UK.KANTORID
        //        WHERE
        //          RST.KANTORID IS NULL AND UK.TAMPIL = 1 AND
        //          UK.TIPEKANTORID = {0}";
        //    switch (unit)
        //    {
        //        case "kanwil":
        //            query = string.Format(query,2);
        //            break;
        //        case "kantah":
        //            query = string.Format(query, 3);
        //            break;
        //    }
        //    if (!string.IsNullOrEmpty(query))
        //    {
        //        using (var ctx = new BpnDbContext())
        //        {
        //            if (from + to > 0)
        //            {
        //                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
        //                arrayListParameters.Add(new OracleParameter("pStart", from));
        //                arrayListParameters.Add(new OracleParameter("pEnd", to));
        //            }
        //            result = ctx.Database.SqlQuery<DaftarKantor>(query).ToList();
        //        }
        //    }

        //    return result;
        //}

        //public List<DaftarKantor> DataKantorBelumGunakanTTE(string unit, int from, int to)
        //{
        //    var result = new List<DaftarKantor>();
        //    string query = @"
        //        SELECT
        //          ROW_NUMBER() OVER (ORDER BY KT.NAMA) AS RNUMBER, COUNT(1) OVER() TOTAL,
        //          KT.KANTORID AS IDKANTOR, KT.NAMA AS NAMAKANTOR
        //        FROM UNITKERJA UK
	       //         INNER JOIN KANTOR KT ON
		      //          KT.KANTORID = UK.KANTORID
        //          LEFT JOIN (
        //             SELECT DISTINCT
        //               UK.KANTORID, UK.NAMAUNITKERJA
        //             FROM JABATANPEGAWAI JP
        //               INNER JOIN JABATAN JB ON
        //                 JB.PROFILEID = JP.PROFILEID AND
        //                 NVL(JB.SEKSIID,'X') <> 'A800' AND
        //                 (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
        //               INNER JOIN UNITKERJA UK ON
        //                 UK.UNITKERJAID = JB.UNITKERJAID
        //               INNER JOIN KANTOR KT ON
        //                 UK.KANTORID = KT.KANTORID
        //             WHERE
        //               (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
        //               NVL(JP.STATUSHAPUS,'0') = '0' AND
        //               NVL(JP.STATUSPLT,'0') = '0')RST ON
        //            RST.KANTORID = UK.KANTORID
        //        WHERE
        //          RST.KANTORID IS NULL AND UK.TAMPIL = 1 AND
        //          UK.TIPEKANTORID = {0}";
        //    switch (unit)
        //    {
        //        case "kanwil":
        //            query = string.Format(query, 2);
        //            break;
        //        case "kantah":
        //            query = string.Format(query, 3);
        //            break;
        //    }
        //    if (!string.IsNullOrEmpty(query))
        //    {
        //        using (var ctx = new BpnDbContext())
        //        {
        //            if (from + to > 0)
        //            {
        //                query = string.Format(string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN {0} AND {1}"), from, to);
        //            }
        //            result = ctx.Database.SqlQuery<DaftarKantor>(query).ToList();
        //        }
        //    }

        //    return result;
        //}

        public string GetIsMyProfileTU(string nip)
        {
            string result = "0";

            string query =
                "SELECT count(*) " +
                "FROM jabatan " +
                "WHERE profileidtu = profileid " +
                "      AND jabatan.profileid IN " +
                "         (SELECT distinct profileid FROM jabatanpegawai WHERE (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND pegawaiid = :Nip1) " +
                "OR profileidba = profileid " +
                "      AND jabatan.profileid IN " +
                "         (SELECT distinct profileid FROM jabatanpegawai WHERE(VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0' AND pegawaiid = :Nip2)";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Nip1", nip));
            arrayListParameters.Add(new OracleParameter("Nip2", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = "1";
                }
                else
                {
                    // Bila ada pegawai mendapat delegasi untuk tugas TU, ambil status dari table SURATINBOX
                    query = string.Format(@"
                        SELECT COUNT(1)
                        FROM {0}.SURATINBOX SI
                        INNER JOIN JABATANPEGAWAI JP ON
	                        JP.PROFILEID = SI.PROFILEPENERIMA AND NVL(JP.STATUSHAPUS,'0') = '0' AND (JP.VALIDSAMPAI is null OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                        WHERE
                          SI.STATUSTERKUNCI = 0 AND
                          SI.STATUSTERKIRIM = 0 AND
                          SI.STATUSFORWARDTU = 1 AND
                          NVL(SI.STATUSHAPUS,'0') = '0' AND
                          SI.NIP = :Nip", OtorisasiUser.NamaSkema);
                        //"SELECT count(*) " +
                        //"FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                        //"WHERE statusterkunci = 0 " +
                        //"      AND statusterkirim = 0 " +
                        //"      AND statusforwardtu = 1 " +
                        //"      AND NVL(STATUSHAPUS,'0') = '0' " +
                        //"      AND nip = :Nip";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("Nip", nip));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                    if (jumlahrecord > 0)
                    {
                        result = "1";
                    }
                }
            }

            return result;
        }

        public string GetKantorIdIndukFromKantorId(string kantorid)
        {
            string result = "";

            string query = "SELECT INDUK FROM KANTOR WHERE KANTORID = :Id";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Id", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public string GetUnitKerjaIdFromKantorId(string kantorid)
        {
            string result = "";

            string query = "SELECT UNITKERJAID FROM UNITKERJA WHERE KANTORID = :Id";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Id", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public bool CheckGantiPassword(string userid, string pegawaiid, int max = 60)
        {
            bool result = false;

            using (var ctx = new BpnDbContext())
            {
                string query = string.Format(@"
                    SELECT 'PEGAWAI' AS TIPE
                    FROM PEGAWAI
                    WHERE USERID = '{0}' AND PEGAWAIID = '{1}'
                    UNION ALL
                    SELECT 'PPNPN' AS TIPE
                    FROM PPNPN
                    WHERE USERID = '{0}' AND NIk = '{1}'", userid, pegawaiid);
                string tipe = ctx.Database.SqlQuery<string>(query).First();

                if (string.IsNullOrEmpty(tipe))
                {
                    return result;
                }
                else
                {
                    if (tipe.Equals("PEGAWAI"))
                    {
                        query = @"
                            SELECT 
                              DECODE(LASTPASSWORDCHANGEDDATE,NULL,100,TRUNC(SYSDATE) - TRUNC(CAST(LASTPASSWORDCHANGEDDATE AS TIMESTAMP)))
                            FROM USERS
                            WHERE
                              USERID = :userId AND
                              (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))";
                    }else if (tipe.Equals("PPNPN"))
                    {
                        query = @"
                            SELECT 
                              DECODE(LASTPASSWORDCHANGEDDATE,NULL,100,TRUNC(SYSDATE) - TRUNC(CAST(LASTPASSWORDCHANGEDDATE AS TIMESTAMP)))
                            FROM USERPPNPN
                            WHERE
                              USERID = :userId AND
                              (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))";
                    }
                    ArrayList arrayListParameters = new ArrayList();
                    arrayListParameters.Add(new OracleParameter("userId", userid));
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    int record = ctx.Database.SqlQuery<int>(query, parameters).First();
                    if (record > max)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        public string GetTipeAkun(string pid, string uid)
        {
            string tipe = string.Empty;
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    string query = string.Format(@"
                        SELECT 'ASN' AS TIPE
                        FROM PEGAWAI
                        WHERE
                          USERID = '{0}' AND
                          PEGAWAIID = '{1}' 
                        UNION ALL 
                        SELECT 'PPNPN' AS TIPE
                        FROM PPNPN
                        WHERE
                          USERID = '{0}' AND
                          NIK = '{1}' 
                        UNION ALL 
                        SELECT 'MITRA' AS TIPE
                        FROM MITRAKERJA MK
                          INNER JOIN PIHAK3 P ON
                            P.PIHAK3ID = MK.MITRAKERJAID 
                        WHERE
                          MK.USERID = '{0}' AND
                          P.NIK = '{1}'", uid, pid);
                    tipe = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message.ToString();
                }
            }
            return tipe;
        }

        public List<Profile> GetPegawaiAktif(string unitkerjaid, string profileid)
        {
            var records = new List<Profile>();

            string query = @"
                SELECT 
	                JP.PEGAWAIID, 
	                DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS NAMAPEGAWAI
                FROM JABATANPEGAWAI JP
                INNER JOIN JABATAN JB ON
	                JB.PROFILEID = JP.PROFILEID AND
	                JB.UNITKERJAID = :param1
                INNER JOIN PEGAWAI PG ON
	                PG.PEGAWAIID = JP.PEGAWAIID
                WHERE
                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                   JP.PROFILEID = :param2";

            using (var ctx = new BpnDbContext())
            {
                var arrayListParameters = new ArrayList();
                arrayListParameters.Add(new OracleParameter("param1", unitkerjaid));
                arrayListParameters.Add(new OracleParameter("param2", profileid));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList();
            }

            return records;
        }

        public TransactionResult LaporMasalah(string id, string txt, string uid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        string sql = @"
                            INSERT INTO TBLAPPREVIEW (APPREVIEWID, APPID, USERID, REVIEW, UPDTIME, UPDUSER, STATUS)
                            VALUES (RAWTOHEX(SYS_GUID()), :AppId, :UserId, :Review, SYSDATE, :UserId, 'P')";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("AppId", id));
                        arrayListParameters.Add(new OracleParameter("UserId", uid));
                        arrayListParameters.Add(new OracleParameter("Review", txt));
                        arrayListParameters.Add(new OracleParameter("UserId", uid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                        tr.Status = true;
                        tc.Commit();
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

        public List<UnitKerja> GetListUnitKerjaPegawai(string pegawaiid)
        {
            List<UnitKerja> records = new List<UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT JB.UNITKERJAID, UK.NAMAUNITKERJA
                FROM JABATANPEGAWAI JP
                  INNER JOIN JABATAN JB ON
                    JB.PROFILEID = JP.PROFILEID AND
                    NVL(JB.SEKSIID,'X') <> 'A800'
                  INNER JOIN UNITKERJA UK ON
  	                UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
                  JP.PEGAWAIID = :PegawaiId AND
                  NVL(JP.STATUSHAPUS,'0') = '0' AND
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";

            using (var ctx = new BpnDbContext())
            {
                arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList<UnitKerja>();
            }

            return records;
        }

        public List<UnitKerja> GetListUnitKerjaInisiatif(string pegawaiid)
        {
            List<UnitKerja> records = new List<UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();
            string query = @"
                    SELECT JB.UNITKERJAID, UK.NAMAUNITKERJA, UK.INDUK, UK.TIPEKANTORID, UK.KANTORID 
                    FROM JABATANPEGAWAI JP
                        INNER JOIN JABATAN JB ON
                        JB.PROFILEID = JP.PROFILEID AND
                        NVL(JB.SEKSIID,'X') <> 'A800'
                        INNER JOIN UNITKERJA UK ON
  	                    UK.UNITKERJAID = JB.UNITKERJAID
                    WHERE
                        JP.PEGAWAIID = :PegawaiId AND
                        NVL(JP.STATUSHAPUS,'0') = '0' AND
                        (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                    ORDER BY LENGTH(JB.UNITKERJAID)";

            using (var ctx = new BpnDbContext())
            {                
                arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList<UnitKerja>();
            }

            try
            {
                if (records != null || records.Count() > 0)
                {
                    List<UnitKerja> addExtra = new List<UnitKerja>();
                    foreach (var unit in records)
                    {
                        if (unit.UnitKerjaId == "02")
                        {
                            //menteri dan wamen bisa inisiatif ke semua
                            query = $@"SELECT
	                                UNITKERJAID,
	                                NAMAUNITKERJA,
	                                INDUK,
	                                TIPEKANTORID,
                                    KANTORID
                                FROM
	                                UNITKERJA 
                                WHERE
	                                TAMPIL = 1 AND UNITKERJAID != '{unit.UnitKerjaId}'
                                ORDER BY ESELON, NAMAUNITKERJA";
                            using (var ctx = new BpnDbContext())
                            {
                                addExtra.AddRange(ctx.Database.SqlQuery<UnitKerja>(query).ToList<UnitKerja>());
                                break;
                            }
                        }
                        else if (unit.TipeKantorId == 1)
                        {
                            //sekjen dan staff dan tenaga ahli disatukan
                            query = $@"SELECT
	                                UNITKERJAID,
	                                NAMAUNITKERJA,
	                                INDUK,
	                                TIPEKANTORID,
                                    KANTORID 
                                FROM
	                                UNITKERJA 
                                WHERE
	                                TAMPIL = 1 
	                                AND TIPEKANTORID = {unit.TipeKantorId} 
	                                AND ( {(string.IsNullOrEmpty(unit.Induk) ? $"INDUK = '{unit.UnitKerjaId.Substring(0, 6)}' OR " : $"INDUK = '{unit.Induk}' OR ")} SUBSTR(UNITKERJAID,0,6) = {(string.IsNullOrEmpty(unit.Induk) ? $"SUBSTR('{unit.UnitKerjaId}',0,6)" : unit.Induk)} {(unit.Induk == "020102" || unit.UnitKerjaId.Substring(0, 6) == "020102" ? " OR NAMAUNITKERJA LIKE 'Staf%Menteri%'" : "")}) 
	                                AND UNITKERJAID != '{unit.UnitKerjaId}'
                                ORDER BY ESELON, NAMAUNITKERJA";
                            using (var ctx = new BpnDbContext())
                            {
                                var addUnit = ctx.Database.SqlQuery<UnitKerja>(query).ToList<UnitKerja>();
                                if (records.Count() > 1)
                                {
                                    addExtra.AddRange(addUnit.Where(x => !addExtra.Any(y => y.UnitKerjaId == x.UnitKerjaId)));
                                }
                                else
                                {
                                    addExtra.AddRange(addUnit);
                                }
                            }
                        }
                        else if (unit.TipeKantorId == 2)
                        {
                            query = $@"SELECT
	                                UK.UNITKERJAID,
	                                UK.NAMAUNITKERJA,
	                                UK.INDUK,
	                                UK.TIPEKANTORID,
	                                UK.KANTORID 
                                FROM
	                                UNITKERJA UK
                                WHERE
	                                UK.TAMPIL = 1 AND 
	                                UK.KANTORID IN (
		                                SELECT KT.KANTORID FROM KANTOR KT WHERE KT.INDUK = '{unit.KantorId}'
	                                )";
                            using (var ctx = new BpnDbContext())
                            {
                                var addUnit = ctx.Database.SqlQuery<UnitKerja>(query).ToList<UnitKerja>();
                                if (records.Count() > 1)
                                {
                                    addExtra.AddRange(addUnit.Where(x => !addExtra.Any(y => y.UnitKerjaId == x.UnitKerjaId)));
                                }
                                else
                                {
                                    addExtra.AddRange(addUnit);
                                }
                            }
                        }
                        else if (unit.TipeKantorId == 3)
                        {
                            query = $@"SELECT
	                                UK.UNITKERJAID,
	                                UK.NAMAUNITKERJA,
	                                UK.INDUK,
	                                UK.TIPEKANTORID,
	                                UK.KANTORID 
                                FROM
	                                UNITKERJA UK
                                WHERE
	                                UK.TAMPIL = 1 AND 
	                                UK.KANTORID IN (
		                                SELECT KT.INDUK FROM KANTOR KT WHERE KT.KANTORID = '{unit.KantorId}'
	                                )";
                            using (var ctx = new BpnDbContext())
                            {
                                var addUnit = ctx.Database.SqlQuery<UnitKerja>(query).ToList<UnitKerja>();
                                if (records.Count() > 1)
                                {
                                    addExtra.AddRange(addUnit.Where(x => !addExtra.Any(y => y.UnitKerjaId == x.UnitKerjaId)));
                                }
                                else
                                {
                                    addExtra.AddRange(addUnit);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (records.Count() > 1)
                    {
                        records.AddRange(addExtra.Where(x => !records.Any(y => y.UnitKerjaId == x.UnitKerjaId)));
                    }
                    else
                    {
                        records.AddRange(addExtra);
                    }
                }
            } catch
            {
                return records;
            }

            return records;
        }

        public List<UnitKerja> GetListUnitKerjaPengumuman(string tipekantor = null, string induk = null)
        {
            var records = new List<UnitKerja>();

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT UNITKERJAID, INDUK, NAMAUNITKERJA, ESELON, TIPEKANTORID FROM UNITKERJA WHERE UNITKERJAID IS NOT NULL AND KANTORID IS NOT NULL AND TAMPIL = 1 ";
            if (!string.IsNullOrEmpty(tipekantor))
            {
                if (tipekantor.Equals("Pusat"))
                {
                    query += " AND TIPEKANTORID = 1";
                }
                else if (tipekantor.Equals("Kanwil"))
                {
                    query += " AND TIPEKANTORID = 2";
                }
                else if (tipekantor.Equals("Kantah"))
                {
                    query += " AND TIPEKANTORID IN (3,4)";
                }
            }

            if (!string.IsNullOrEmpty(induk))
            {
                query += " AND induk = :Induk";
                arrayListParameters.Add(new OracleParameter("Induk", induk));
            }

            query += "ORDER BY unitkerjaid";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList();
            }

            return records;
        }

        public ProfilePegawai getProfilePegawai(string id)
        {
            var data = new ProfilePegawai();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  JP.PROFILEPEGAWAIID,
                  JP.PROFILEID,
                  JP.PEGAWAIID,
                  JP.STATUSPLT,
                  JP.NOMORSK,
                  JP.VALIDSAMPAI,
                  JB.NAMA AS NAMAPROFILE,
                  UK.KANTORID
                FROM JABATANPEGAWAI JP
                INNER JOIN JABATAN JB ON
                  JB.PROFILEID = JP.PROFILEID AND
                  NVL(JB.SEKSIID,'X') <> 'A800' AND
                  JB.PROFILEIDTU IS NOT NULL
                INNER JOIN UNITKERJA UK ON
                  UK.UNITKERJAID = JB.UNITKERJAID
                INNER JOIN KANTOR KT ON
	                KT.KANTORID = UK.KANTORID
                WHERE
                  JP.PROFILEPEGAWAIID = :param1 AND
                  (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  NVL(JP.STATUSHAPUS,'0') = '0'";

            using (var ctx = new BpnDbContext())
            {
                arrayListParameters.Add(new OracleParameter("param1", id));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<ProfilePegawai>(query,parameters).FirstOrDefault();
            }

            return data;
        }

        public int GetCountPengajuanAkses(string pegawaiid, string unitkerjaid)
        {
            int result = 0;
            string skema = OtorisasiUser.NamaSkema;
            try
            {
                string query = string.Format(@"
                SELECT COUNT(DISTINCT PA.PERSETUJUANID)
                FROM {0}.PERSETUJUANAKSES PA
                INNER JOIN {0}.AKSESKKP KKP ON
                    KKP.PERSETUJUANID = PA.PERSETUJUANID AND
                    (KKP.VALIDSAMPAI IS NULL OR TRUNC(CAST(KKP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(KKP.STATUSHAPUS,'0') = '0'
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PROFILEID = PA.PROFILEID AND
	                JP.PEGAWAIID = :param1 AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(JP.STATUSHAPUS,'0') = '0'
                INNER JOIN JABATAN P ON
	                P.PROFILEID = PA.PROFILEID AND
	                NVL(P.UNITKERJAID,'020116') = DECODE(PA.PROFILEID,'A80100',NVL(P.UNITKERJAID,'020116'),:param2)
                WHERE
	                PA.STATUS = 'W' AND
	                NVL(PA.STATUSHAPUS,'0') = '0'", skema);

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Add(new OracleParameter("param1", pegawaiid));
                arrayListParameters.Add(new OracleParameter("param2", unitkerjaid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<int>(query, parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }

            return result;
        }

        public List<KantorKKP> GetListAksesKKP(string userid, int tipekantorid = 0, string _akses = null)
        {
            var records = new List<KantorKKP>();
            try
            {
                string skema = OtorisasiUser.NamaSkema;
                ArrayList arrayListParameters = new ArrayList();

                string query = string.Format(@"
                SELECT
                  KT.TIPEKANTORID,
                  KT.KANTORID,
                  KT.NAMA AS NAMAKANTOR,
                  DECODE(PP.PROFILEPEGAWAIID,NULL,'U',NVL(AKS.STATUS,'U')) AS STATUS
                FROM KANTOR KT
                  INNER JOIN PEGAWAI PG ON
                    PG.USERID = :param1 AND
                    (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                --  INNER JOIN JABATANPEGAWAI JP ON
                --    JP.PEGAWAIID = PG.PEGAWAIID AND
                --    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                --    NVL(JP.STATUSHAPUS,'0') = '0'
                --  INNER JOIN JABATAN JB ON
                --    JB.PROFILEID = JP.PROFILEID AND
                --    NVL(JB.SEKSIID,'X') <> 'A800' AND
                --    JB.PROFILEIDTU IS NOT NULL
                  INNER JOIN UNITKERJA UK ON
                --    UK.UNITKERJAID = JB.UNITKERJAID AND
                    UK.TAMPIL = '1' AND
                    UK.KANTORID = KT.KANTORID
                  LEFT JOIN PROFILEPEGAWAI PP ON
                    PP.PEGAWAIID = PG.PEGAWAIID AND
                    (PP.VALIDSAMPAI IS NULL OR TRUNC(CAST(PP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(PP.STATUSHAPUS,'0') = '0'
                  LEFT JOIN surat.AKSESKKP AKS ON
                    AKS.KANTORID = KT.KANTORID AND
                    AKS.USERID = PG.USERID AND
                    (AKS.VALIDSAMPAI IS NULL OR TRUNC(CAST(AKS.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(AKS.STATUSHAPUS,'0') = '0' AND
                    AKS.STATUS IN ('W','A') AND
                    AKS.TIPESTATUS = 'I'", skema);
                arrayListParameters.Add(new OracleParameter("param1", userid));
                if (!string.IsNullOrEmpty(_akses))
                {
                    query += " AND AKS.PROFILEID = CASE KT.TIPEKANTORID WHEN 1 THEN 'C' WHEN 2 THEN 'B' ELSE 'A' END || :param3";
                    arrayListParameters.Add(new OracleParameter("param3", _akses));
                }
                //query = string.Concat(query, " WHERE KT.TIPEKANTORID > 1 ");
                if (!tipekantorid.Equals(0))
                {
                    query += " AND KT.TIPEKANTORID = :param2";
                    arrayListParameters.Add(new OracleParameter("param2", tipekantorid));
                }
                query += " GROUP BY NVL(NULLIF(KT.INDUK,'980FECFC746D8C80E0400B0A9214067D'),KT.KANTORID), KT.TIPEKANTORID, KT.KANTORID, KT.NAMA, DECODE(PP.PROFILEPEGAWAIID, NULL, 'U', NVL(AKS.STATUS, 'U')) ORDER BY NVL(NULLIF(KT.INDUK,'980FECFC746D8C80E0400B0A9214067D'),KT.KANTORID), KT.TIPEKANTORID, KT.NAMA";

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    records = ctx.Database.SqlQuery<KantorKKP>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new List<KantorKKP>();
            }

            return records;
        }

        public List<TipeKantor> GetListTipeKantor()
        {
            var result = new List<TipeKantor>();

            string query = @"
                SELECT
	                TK.TIPEKANTORID,
	                TK.TIPE
                FROM TIPEKANTOR TK
                INNER JOIN KANTOR KT ON
	                KT.TIPEKANTORID = TK.TIPEKANTORID AND
 	                (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                GROUP BY
	                TK.TIPEKANTORID,
	                TK.TIPE
                ORDER BY TK.TIPEKANTORID";

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<TipeKantor>(query).ToList();
            }

            return result;
        }

        public int geteselon(string pegawaiid)
        {
            int result = 9;

            ArrayList arrayListParameters = new ArrayList();
            try
            {
                string query = @"SELECT CAST(SUBSTR(NVL(ESELON,99),0,1) AS NUMBER(1)) AS ESELON FROM SIMPEG_2702.V_PEGAWAI_EOFFICE WHERE NIPBARU = :Param1";
                arrayListParameters.Clear();
                arrayListParameters.Add(new OracleParameter("Param1", pegawaiid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<int>(query, parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 9;
            }

            return result;
        }

        public TransactionResult KirimPengajuan(string uid, string pid, string tip, string kid, SimpanAkses dt, string id)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        string sql = string.Format(@"
                            INSERT INTO {0}.PERSETUJUANAKSES (PERSETUJUANID, PROFILEID, TIPE, USERPEMBUAT, TANGGALDIBUAT, KANTORID, STATUS)
                            VALUES (:PersetujuanId, :ProfileId, :Tipe, :UserId, SYSDATE, :KantorId, 'W')",skema);
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("PersetujuanId", id));
                        arrayListParameters.Add(new OracleParameter("ProfileId", pid));
                        arrayListParameters.Add(new OracleParameter("Tipe", tip));
                        arrayListParameters.Add(new OracleParameter("UserId", uid));
                        arrayListParameters.Add(new OracleParameter("KantorId", kid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        foreach(var lt in dt.ListAkses)
                        {
                            if (tip.Equals("AksesKKP"))
                            {
                                if (!lt.Status.Equals("U"))
                                {
                                    sql = string.Format(@"
                                    INSERT INTO {0}.AKSESKKP (AKSESID, PERSETUJUANID, USERID, KANTORID, PROFILEID, TIPESTATUS, USERPEMBUAT, TANGGALDIBUAT, STATUS, STATUSPLT, BISABOOKING)
                                    VALUES (RAWTOHEX(SYS_GUID()), :PersetujuanId, :AkunId, :KantorId, :ProfileId, :TipeStatus, :UserId, SYSDATE, 'W', 0, 0)", skema);
                                    sql = sWhitespace.Replace(sql, " ");
                                    arrayListParameters.Clear();
                                    switch (GetTipeKantor(lt.KantorId))
                                    {
                                        case 1:
                                            dt.ProfileId = string.Concat("C",dt.TipeAkses);
                                            break;
                                        case 2:
                                            dt.ProfileId = string.Concat("B", dt.TipeAkses);
                                            break;
                                        case 3:
                                            dt.ProfileId = string.Concat("A", dt.TipeAkses);
                                            break;
                                        case 4:
                                            dt.ProfileId = string.Concat("A", dt.TipeAkses);
                                            break;
                                    }
                                    arrayListParameters.Add(new OracleParameter("PersetujuanId", id));
                                    arrayListParameters.Add(new OracleParameter("AkunId", dt.UserId));
                                    arrayListParameters.Add(new OracleParameter("KantorId", lt.KantorId));
                                    arrayListParameters.Add(new OracleParameter("ProfileId", dt.ProfileId));
                                    arrayListParameters.Add(new OracleParameter("TipeStatus", lt.Status));
                                    arrayListParameters.Add(new OracleParameter("UserId", uid));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);


                                    if (!string.IsNullOrEmpty(dt.ValidSampai))
                                    {
                                        sql = string.Format(@" UPDATE {0}.AKSESKKP SET VALIDSAMPAI = TO_DATE(:Tanggal,'DD/MM/YYYY') 
                                         WHERE PERSETUJUANID = :Id ", skema);
                                        sql = sWhitespace.Replace(sql, " ");
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("Tanggal", dt.ValidSampai));
                                        arrayListParameters.Add(new OracleParameter("Id", id));
                                        parameters = arrayListParameters.OfType<object>().ToArray();
                                        ctx.Database.ExecuteSqlCommand(sql, parameters);
                                    }
                                }
                            }
                        }

                        tr.Status = true;
                        tc.Commit();
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

        public TransactionResult persetujuanAksesKKP(string uid, string pid, string persetujuanid, bool resp, string alasan = null)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (resp)
                        {
                            ArrayList arrayListParameters = new ArrayList();
                            string sql = string.Format(@"
                                SELECT
                                  AK.AKSESID, AK.USERID, COALESCE(PG.PEGAWAIID, PPNPN.NIK, P3.NIK) AS PEGAWAIID, AK.KANTORID, AK.PROFILEID, AK.TIPESTATUS, NVL(AK.VALIDSAMPAI,ADD_MONTHS(TRUNC(SYSDATE, 'y'), 12) - 1) AS VALIDSAMPAI
                                FROM {0}.AKSESKKP AK
                                  LEFT JOIN PEGAWAI PG ON
                                    PG.USERID = AK.USERID
                                  LEFT JOIN PPNPN ON
                                    PPNPN.USERID = AK.USERID
                                  LEFT JOIN PIHAK3 P3 ON
                                  	P3.USERID = AK.USERID
                                WHERE
                                  AK.PERSETUJUANID = :param1 AND
                                  AK.STATUS = 'W'", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", persetujuanid));
                            var ListAkses = ctx.Database.SqlQuery<PersetujuanAkses>(sql, arrayListParameters.ToArray()).ToList();

                            foreach (var lt in ListAkses)
                            {
                                if (lt.TipeStatus.Equals("I"))
                                {
                                    sql = string.Format(@"
                                        UPDATE {0}.AKSESKKP SET STATUS = 'A', VALIDSEJAK = SYSDATE, VALIDSAMPAI = NVL(VALIDSAMPAI,ADD_MONTHS(TRUNC(SYSDATE, 'y'), 12) - 1), TANGGALPERUBAHAN = SYSDATE, USERPERUBAHAN = :param1 WHERE AKSESID = :param2 AND STATUS = 'W'", skema);
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("param1", uid));
                                    arrayListParameters.Add(new OracleParameter("param2", lt.AksesId));
                                    ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());

                                    if (skema.ToLower().Equals("surat"))
                                    {
                                        sql = string.Format(@"
                                        INSERT INTO PROFILEPEGAWAI (PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, VALIDSAMPAI, BISABOOKING, KETERANGAN, USERUPDATE, LASTUPDATE, JENISTRUKTUR, STATUSPLT, TIPEPROFILE, TANGGALPERUBAHAN, NOMORSK)
                                        VALUES (:param1, :param2, :param3, :param4, SYSDATE, :param5, :param6, 'AksesKKP', :param7, SYSDATE, 1, :param8, 1, SYSDATE,:param9)", skema);
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("param1", lt.AksesId));
                                        arrayListParameters.Add(new OracleParameter("param2", lt.ProfileId));
                                        arrayListParameters.Add(new OracleParameter("param3", lt.PegawaiId));
                                        arrayListParameters.Add(new OracleParameter("param4", lt.KantorId));
                                        arrayListParameters.Add(new OracleParameter("param5", lt.ValidSampai));
                                        arrayListParameters.Add(new OracleParameter("param6", lt.BisaBooking));
                                        arrayListParameters.Add(new OracleParameter("param7", uid));
                                        arrayListParameters.Add(new OracleParameter("param8", lt.StatusPLT));
                                        arrayListParameters.Add(new OracleParameter("param9", persetujuanid));
                                        ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());
                                    }
                                }
                                else if (lt.TipeStatus.Equals("D"))
                                {
                                    sql = string.Format(@"
                                        UPDATE {0}.AKSESKKP SET STATUS = 'A', VALIDSEJAK = SYSDATE, TANGGALPERUBAHAN = SYSDATE, USERPERUBAHAN = :param1 WHERE AKSESID = :param2 AND STATUS = 'W'", skema);
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new OracleParameter("param1", uid));
                                    arrayListParameters.Add(new OracleParameter("param2", lt.AksesId));
                                    ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());

                                    if (skema.ToLower().Equals("surat"))
                                    {
                                        sql = string.Format(@"
                                            UPDATE {0}.AKSESKKP SET VALIDSAMPAI = TRUNC(SYSDATE-1), TANGGALPERUBAHAN = SYSDATE, USERPERUBAHAN = :param1 WHERE USERID = :param2 AND PROFILEID = :param3 AND KANTORID = :param4 AND TIPESTATUS = 'I' AND STATUS = 'A' AND NVL(STATUSHAPUS,'0') = '0' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", skema);
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("param1", uid));
                                        arrayListParameters.Add(new OracleParameter("param2", lt.UserId));
                                        arrayListParameters.Add(new OracleParameter("param3", lt.ProfileId));
                                        arrayListParameters.Add(new OracleParameter("param4", lt.KantorId));
                                        ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());

                                        sql = string.Format(@"
                                            UPDATE PROFILEPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :param1, LASTUPDATE = SYSDATE
                                            WHERE PROFILEID = :param2 AND PEGAWAIID = :param3 AND KANTORID = :param4 AND NVL(STATUSHAPUS,'0') = '0' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))", skema);
                                        arrayListParameters.Clear();
                                        arrayListParameters.Add(new OracleParameter("param1", uid));
                                        arrayListParameters.Add(new OracleParameter("param2", lt.ProfileId));
                                        arrayListParameters.Add(new OracleParameter("param3", lt.PegawaiId));
                                        arrayListParameters.Add(new OracleParameter("param4", lt.KantorId));
                                        ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());
                                    }
                                }
                            }
                            sql = string.Format(@"
                                UPDATE {0}.PERSETUJUANAKSES SET STATUS = 'A', PEGAWAIID = :param1, TANGGALPERSETUJUAN = SYSDATE, TANGGALPERUBAHAN = SYSDATE, USERPERUBAHAN = :param2 WHERE PERSETUJUANID = :param3 AND STATUS = 'W'", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", pid));
                            arrayListParameters.Add(new OracleParameter("param2", uid));
                            arrayListParameters.Add(new OracleParameter("param3", persetujuanid));
                            ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());
                        }
                        else
                        {
                            ArrayList arrayListParameters = new ArrayList();
                            string sql = string.Format(@"
                                UPDATE {0}.PERSETUJUANAKSES SET STATUS = 'R', TANGGALTOLAK = SYSDATE, USERTOLAK = :param1, ALASANTOLAK = :param2 WHERE PERSETUJUANID = :param3 AND STATUS = 'W'", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", uid));
                            arrayListParameters.Add(new OracleParameter("param2", alasan));
                            arrayListParameters.Add(new OracleParameter("param3", persetujuanid));
                            ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());

                            sql = string.Format(@"
                                UPDATE {0}.AKSESKKP SET STATUS = 'R' WHERE PERSETUJUANID = :param1 AND STATUS = 'W'", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", persetujuanid));
                            ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());
                        }

                        tr.Status = true;
                        tc.Commit();
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

        public string getKantorIdFromPengajuanAkses(string id)
        {
            string result = "";
            string skema = OtorisasiUser.NamaSkema;

            string query = string.Format("SELECT KANTORID FROM {0}.PERSETUJUANAKSES WHERE PERSETUJUANID = :Param1",skema);

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Param1", id));

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }catch(Exception ex)
                {
                    var msg = ex.Message;
                }
            }

            return result;
        }

        public List<DetailAksesKKP> getListHakAkses(string id, int from, int to)
        {
            var list = new List<DetailAksesKKP>();
            string skema = OtorisasiUser.NamaSkema;
            try
            {
                ArrayList arrayListParameters = new ArrayList();
                string query = string.Format(@"
                SELECT
                  ROW_NUMBER() OVER (ORDER BY KT.TIPEKANTORID, KT.NAMA) AS RNUMBER,
                  COUNT(1) OVER() TOTAL,
                  AK.KANTORID, 
                  KT.NAMA AS KANTOR, AK.PROFILEID || ' - ' || PR.NAMA AS PROFILE, 
                  DECODE(AK.TIPESTATUS,'I','Tambah (' || TO_CHAR(NVL(AK.VALIDSEJAK,SYSDATE),'fmDD Month YYYY') || ' - ' || TO_CHAR(NVL(AK.VALIDSAMPAI,ADD_MONTHS(TRUNC(SYSDATE, 'y'), 12) - 1),'fmDD Month YYYY') || ')','D','Hapus','Ubah') AS TIPE
                FROM {0}.AKSESKKP AK
                INNER JOIN KANTOR KT ON
	                KT.KANTORID = AK.KANTORID
                INNER JOIN PROFILE PR ON
	                PR.PROFILEID = AK.PROFILEID
                WHERE
                  AK.PERSETUJUANID = :Param1 AND
                  NVL(AK.STATUSHAPUS,'0') = '0'", skema);
                arrayListParameters.Add(new OracleParameter("Param1", id));
                if (from + to > 0)
                {
                    query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :Param2 AND :Param3");
                    arrayListParameters.Add(new OracleParameter("Param2", from));
                    arrayListParameters.Add(new OracleParameter("Param3", to));
                }

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    list = ctx.Database.SqlQuery<DetailAksesKKP>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return list;
        }

        public CetakPersetujuanAksesKKP getPengajuanAksesKKP(string id)
        {
            var data = new CetakPersetujuanAksesKKP();

            ArrayList arrayListParameters = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;

            try
            {
                string query = string.Format(@"
                SELECT
                  PA.PERSETUJUANID,
                  TO_CHAR(PA.TANGGALDIBUAT, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                  TO_CHAR(SYSDATE, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') AS TANGGALPERSETUJUAN,
                  NVL(PG.PEGAWAIID,PN.NIK) AS TARGETID,
                  NVL(PG.NAMA,PN.NAMA) AS NAMATARGET,
                  JB.NAMA AS JABATAN
                FROM {0}.PERSETUJUANAKSES PA
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = PA.PROFILEID AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                  INNER JOIN {0}.AKSESKKP KKP ON
                    KKP.PERSETUJUANID = PA.PERSETUJUANID AND
                    (KKP.VALIDSAMPAI IS NULL OR TRUNC(CAST(KKP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(KKP.STATUSHAPUS,'0') = '0'
                  LEFT JOIN PEGAWAI PG ON
                    PG.USERID = KKP.USERID
                  LEFT JOIN PPNPN PN ON
                    PN.USERID = KKP.USERID
                  INNER JOIN JABATAN JB ON
  	                JB.PROFILEID = PA.PROFILEID
                WHERE
                  PA.PERSETUJUANID = :param1 AND
                  PA.STATUS = 'W' AND
                  NVL(PA.STATUSHAPUS,'0') = '0'
                GROUP BY
                  PA.PERSETUJUANID,
                  PA.TANGGALDIBUAT,
                  PA.TIPE,
                  NVL(PG.PEGAWAIID,PN.NIK),
                  NVL(PG.NAMA,PN.NAMA),
                  JB.NAMA", skema);

                using (var ctx = new BpnDbContext())
                {
                    arrayListParameters.Add(new OracleParameter("param1", id));
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    data = ctx.Database.SqlQuery<CetakPersetujuanAksesKKP>(query, parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return data;
        }

        public Profile getDataPengirimBySuratInboxId(string suratinboxid)
        {
            var result = new Profile();
            string skema = OtorisasiUser.NamaSkema;

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Format(@"
                SELECT PROFILEPENERIMA AS ProfileId, JB.NAMA AS NamaProfile, DECODE(UK.TIPEKANTORID,1,JB.UNITKERJAID,UK.KANTORID) AS UNITKERJAID, JB.PROFILEIDTU
                FROM {0}.SURATINBOX SI
                  INNER JOIN JABATAN JB ON
		                JB.PROFILEID = SI.PROFILEPENERIMA
	                INNER JOIN UNITKERJA UK ON
		                UK.UNITKERJAID = JB.UNITKERJAID
                WHERE
                  SI.SURATINBOXID = :param1",skema);
            arrayListParameters.Add(new OracleParameter("param1", suratinboxid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<Profile>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public DetailAkun GetDetailAkun(string nip, string kantorid, string tipe)
        {
            var data = new DetailAkun();

            ArrayList arrayListParameters = new ArrayList();

            string query = string.Empty;
            arrayListParameters.Add(new OracleParameter("PegawaiId", nip));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
            if (tipe.Equals("ASN"))
            {
                query = @"
                    SELECT
                      NVL(VP.NIPBARU,VP.NIP) AS PEGAWAIID,
                      VP.NIK,
                      VP.NAMA_LENGKAP AS NAMA,
                      VP.EMAIL,
                      VP.HP AS NOMORHP,
                      'ASN' AS TIPEAKUN,
                      UP.USERID,
                      UP.USERNAME,
                      VP.STATUSPEGAWAIID AS STATUS,
                      KT.NAMA AS SATKER
                    FROM SIAP_VW_PEGAWAI VP
                    INNER JOIN PEGAWAI PG ON
	                    PG.PEGAWAIID = NVL(VP.NIPBARU,VP.NIP) AND
                      (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                    INNER JOIN USERS UP ON
	                    UP.USERID = PG.USERID
                    INNER JOIN JABATANPEGAWAI JP ON
	                    JP.PEGAWAIID = PG.PEGAWAIID AND
	                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                    NVL(JP.STATUSHAPUS,'0') = '0'
                    INNER JOIN JABATAN JB ON
	                    JB.PROFILEID = JP.PROFILEID AND
	                    NVL(JB.SEKSIID,'XXX') <> 'A800' AND
	                    (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                    INNER JOIN UNITKERJA UK ON
	                    UK.UNITKERJAID = JB.UNITKERJAID AND
	                    UK.TAMPIL = 1
                    INNER JOIN KANTOR KT ON
	                    KT.KANTORID = UK.KANTORID
                    WHERE
	                    NVL(VP.NIPBARU,VP.NIP) = :PegawaiId AND UK.KANTORID = :KantorId";
            }else if (tipe.Equals("PPNPN"))
            {
                query = @"
                    SELECT
                      VP.NIKBARU AS PEGAWAIID,
                      VP.NIKBARU AS NIK,
                      VP.NAMA,
                      VP.EMAIL,
                      VP.TELEPON AS NOMORHP,
                      'PPNPN' AS TIPEAKUN,
                      UP.USERID,
                      UP.USERNAME,
                      VP.STATUSPEGAWAIID AS STATUS,
                      KT.NAMA AS SATKER
                    FROM SIMPEG_PPNPN.V_PEGAWAI VP
                    INNER JOIN PPNPN PN ON
	                    PN.PPNPNID = VP.PPNPNID
                    INNER JOIN USERPPNPN UP ON
	                    UP.USERID = PN.USERID AND
                        (UP.VALIDSAMPAI IS NULL OR TRUNC(CAST(UP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                    INNER JOIN PPNPN.RIWAYATKONTRAK RK ON
                        NVL(RK.STATUSHAPUS,'0')='0' AND
	                    RK.PPNPNID = PN.PPNPNID AND
                        TRUNC(CAST(NVL(RK.TANGGALSELESAIKONTRAK,(ADD_MONTHS(TRUNC(RK.TANGGALKONTRAK, 'y'), 12) - 1)) AS TIMESTAMP)) >= TRUNC(SYSDATE)
                    INNER JOIN KANTOR KT ON
	                    KT.KANTORID = RK.KANTORID
                    WHERE
                      VP.NIKBARU = :PegawaiId AND RK.KANTORID = :KantorId";
            }
            else if (tipe.Equals("PIHAK3"))
            {
                query = @"
                    SELECT
	                    RP.NIK AS PEGAWAIID,
	                    RP.NIK AS NIK,
	                    RP.NAMA,
	                    RP.EMAIL,
	                    RP.NOHP AS NOMORHP,
	                    'PIHAK3' AS TIPEAKUN,
	                    RP.USERID,
	                    RP.USERNAME,
	                    '1' AS STATUS,
	                    KT.NAMA AS SATKER
                    FROM MITRAKERJA.REGISTERUSERPERTANAHAN RP
                      INNER JOIN PIHAK3 P ON
                        P.NIK = RP.NIK AND
  	                    NVL(P.STATUSHAPUS,'0') = '0'
                      INNER JOIN MITRAKERJA.DETAILKONTRAKPIHAK3 DKP ON
  	                    DKP.PIHAK3ID = P.PIHAK3ID AND
  	                    NVL(DKP.STATUSHAPUS,'0') = '0'
                      INNER JOIN MITRAKERJA.KONTRAKPIHAK3 KP ON
  	                    KP.KONTRAKPIHAK3ID = DKP.KONTRAKPIHAK3ID AND
  	                    NVL(KP.STATUSHAPUS,'0') = '0' AND
  	                    (KP.TANGGALMULAI IS NULL OR TRUNC(CAST(KP.TANGGALMULAI AS TIMESTAMP)) <= TRUNC(SYSDATE)) AND
  	                    (KP.TANGGALSELESAI IS NULL OR TRUNC(CAST(KP.TANGGALSELESAI AS TIMESTAMP)) >= TRUNC(SYSDATE))

                      INNER JOIN KANTOR KT ON
  	                    KT.KANTORID = KP.SATUANKERJA
                    WHERE
                      RP.NIK = :PegawaiId AND
                      KP.SATUANKERJA = :KantorId AND
                      RP.STATUS = 'USER SUDAH AKTIF'";
            }
            else
            {
                data.PesanError = "Tipe Akun Tidak Diketahui";
                return data;
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<DetailAkun>(query, parameters).FirstOrDefault();
                if(data == null)
                {
                    data = new DetailAkun();
                    data.PesanError = "Pegawai Tidak Terdaftar";
                }
                else
                {
                    data.PesanError = "";
                }
            }

            return data;
        }

        public List<ProfilePegawai> GetProfileKKP(string pegawaiid, string kantorid, int from, int to)
        {
            List<ProfilePegawai> records = new List<ProfilePegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  ROW_NUMBER() OVER (ORDER BY PR.NAMA) AS RNUMBER,
                  COUNT(1) OVER() TOTAL,
                  PP.PROFILEPEGAWAIID,
                  PP.PROFILEID,
                  PP.NOMORSK,
                  PR.NAMA || DECODE(PP.VALIDSAMPAI, null, '', '<br>[Sampai dengan ' || TO_CHAR(PP.VALIDSAMPAI,'fmDD Month YYYY') || ']') AS NAMAPROFILE,
                  PP.PEGAWAIID,
                  PP.KANTORID,
                  PP.VALIDSAMPAI
                FROM	
                	PROFILE PR
				INNER JOIN SEKSI SK ON
                    SK.SEKSIID(+) = PR.SEKSIID
			    INNER JOIN PROFILEPEGAWAI PP ON
                  PR.PROFILEID = PP.PROFILEID AND
                  NVL(PP.STATUSHAPUS,'0') = '0' AND
  				  (PP.VALIDSAMPAI IS NULL OR TRUNC(CAST(PP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  PP.PEGAWAIID = :PegawaiId AND
                  PP.KANTORID = :KantorId
                WHERE
  				  (PR.VALIDSAMPAI IS NULL OR TRUNC(CAST(PR.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
            if (from + to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                arrayListParameters.Add(new OracleParameter("pStart", from));
                arrayListParameters.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ProfilePegawai>(query, parameters).ToList();
            }

            return records;
        }

        public List<Profile> GetProfilesKKPByKantorId(string pegawaiid, string kantorid, string tipeakun)
        {
            var records = new List<Profile>();

            ArrayList arrayListParameters = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;
            int tipekantor = GetTipeKantor(kantorid);
            string dontlist, matchlist;
            dontlist = "'A80000', 'A80100', 'A80200', 'A80400', 'A80500','B80000', 'B80100', 'B80200', 'B80400', 'B80500','C80000', 'C80100', 'C80200', 'C80400', 'C80500'";
            if (tipekantor > 2)
            {
                if (tipeakun.Equals("ASN"))
                {
                    matchlist = "(PR.PROFILEID LIKE 'N%' OR PR.PROFILEID LIKE 'A92%' OR PR.PROFILEID LIKE 'L%' OR PR.PROFILEID IN ('AVDHAK','AVDSUK','A990001','A80301','A10106','BKVLD','REVLD','A990007') OR PR.PROFILEID LIKE 'A91%' OR PR.PROFILEID like 'A95%') AND";
                }
                else if (tipeakun.Equals("PPNPN"))
                {
                    matchlist = "LOWER(PR.NAMA) NOT LIKE '%ketua%' AND ((PR.PROFILEID LIKE 'N%' AND NVL(PR.TIPEESELONID,99) > 5) OR PR.PROFILEID LIKE 'A92%' OR PR.PROFILEID LIKE 'L%' OR PR.PROFILEID IN ('AVDHAK','AVDSUK','A990001','A80301','A10106','BKVLD','REVLD','A990007') OR PR.PROFILEID LIKE 'A91%' OR PR.PROFILEID like 'A95%') AND";
                }
                else
                {
                    matchlist = "PR.PROFILEID IN ('A990001','N20102','A91006','AVDSUK','AVDHAK','A80301','A990007') AND";
                }
            }
            else if (tipekantor == 2)
            {
                if (tipeakun.Equals("ASN"))
                {
                    matchlist = "(PR.PROFILEID LIKE 'R%' OR PR.PROFILEID LIKE 'A93%' OR PR.PROFILEID IN ('B990001','B990007')) AND";
                }
                else if (tipeakun.Equals("PPNPN"))
                {
                    matchlist = "((PR.PROFILEID LIKE 'R%' AND NVL(PR.TIPEESELONID,99) > 5) OR PR.PROFILEID LIKE 'A93%' OR PR.PROFILEID IN ('B990001','B990007')) AND";
                }
                else
                {
                    matchlist = "PR.PROFILEID IN ('B990001','N20102','A91006','AVDSUK','AVDHAK','A80301','B990007') AND";
                }
            }
            else
            {
                if (tipeakun.Equals("ASN"))
                {
                    matchlist = "(PR.PROFILEID LIKE 'C%' OR PR.PROFILEID LIKE 'G%' OR PR.PROFILEID IN ('C990001','C990007')) AND";
                }
                else if (tipeakun.Equals("PPNPN"))
                {
                    matchlist = "(((PR.PROFILEID LIKE 'C%' OR PR.PROFILEID LIKE 'G%') AND NVL(PR.TIPEESELONID,99) > 5) OR PR.PROFILEID IN ('C990001','C990007')) AND";
                }
                else
                {
                    matchlist = "PR.PROFILEID IN ('C990001','N20102','A91006','AVDSUK','AVDHAK','A80301','C990007') AND";
                }
            }
            string cekpengajuan = string.Format(@"
              LEFT JOIN {0}.AKSESKKP AK ON
  	            AK.USERID IN (SELECT USERID FROM PPNPN WHERE NIK = :Param1 UNION ALL SELECT USERID FROM PEGAWAI WHERE PEGAWAIID = :Param2) AND
  	            AK.PROFILEID = PR.PROFILEID AND
  	            AK.KANTORID = :Param3 AND
  	            AK.STATUS = 'W'", skema, pegawaiid,kantorid);
            arrayListParameters.Add(new OracleParameter("Param1", pegawaiid));
            arrayListParameters.Add(new OracleParameter("Param2", pegawaiid));
            arrayListParameters.Add(new OracleParameter("Param3", kantorid));

            string query = @"
                SELECT
                  PR.PROFILEID, PR.NAMA AS NAMAPROFILE
                FROM PROFILE PR
				  INNER JOIN SEKSI SK ON
                    SK.SEKSIID(+) = PR.SEKSIID
                  {2}
                  LEFT JOIN PROFILEPEGAWAI PP ON
                    PP.PROFILEID = PR.PROFILEID AND
                    NVL(PP.STATUSHAPUS,'0') = '0' AND
                    (PP.VALIDSAMPAI IS NULL OR TRUNC(CAST(PP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    PP.PEGAWAIID = :Param4 AND
                    PP.KANTORID = :Param5
                WHERE
	                (PR.VALIDSAMPAI IS NULL OR TRUNC(CAST(PR.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                PP.PROFILEPEGAWAIID IS NULL AND
                    AK.AKSESID IS NULL AND
	                PR.JABATANLAMA = 0 AND
	                {1}
	                PR.PROFILEID NOT IN ({0})
                GROUP BY PR.PROFILEID, PR.NAMA
                ORDER BY PR.NAMA";

            query = sWhitespace.Replace(query, " ");
            query = String.Format(query, dontlist, matchlist, cekpengajuan);

            arrayListParameters.Add(new OracleParameter("Param4", pegawaiid));
            arrayListParameters.Add(new OracleParameter("Param5", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).ToList();
            }

            return records;
        }

        public TransactionResult HapusProfileKKP(string id, string userid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = @"UPDATE PROFILEPEGAWAI SET VALIDSAMPAI = TRUNC(SYSDATE-1), USERUPDATE = :UserId, LASTUPDATE = SYSDATE WHERE PROFILEPEGAWAIID = :Id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("UserId", userid));
                        arrayListParameters.Add(new OracleParameter("Id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Profile berhasil dihapus";
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
        public int JumlahProfileKKP(string pegawaiid, string profileid, string kantorid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT COUNT(1) FROM PROFILEPEGAWAI WHERE PEGAWAIID = :PegawaiId AND PROFILEID = :ProfileId AND KANTORID = :KantorId AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND NVL(STATUSHAPUS,'0') = '0'";

            arrayListParameters.Add(new OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public int JumlahPemilikProfileStruktural(string profileid, string kantorid)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT COUNT(1)
                FROM PROFILEPEGAWAI PP
                INNER JOIN PROFILE PR ON
	                PR.PROFILEID = PP.PROFILEID AND
	                PR.TIPEESELONID < 6 AND
                  (PR.VALIDSAMPAI IS NULL OR TRUNC(CAST(PR.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                WHERE
                  PP.PROFILEID = :ProfileId AND
                  PP.KANTORID = :KantorId AND
                  (PP.VALIDSAMPAI IS NULL OR TRUNC(CAST(PP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                  NVL(PP.STATUSHAPUS,'0') = '0'";

            arrayListParameters.Add(new OracleParameter("ProfileId", profileid));
            arrayListParameters.Add(new OracleParameter("KantorId", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public TransactionResult InsertProfileKKP(ProfilePegawai _profile, string userid, string kantorid, string validsampai, string tipeakun)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            Regex sWhitespace = new Regex(@"\s+");

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
                            @"INSERT INTO PROFILEPEGAWAI 
                              (PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, VALIDSAMPAI, STATUSPLT, BISABOOKING, NOMORSK, KETERANGAN, USERUPDATE, LASTUPDATE) VALUES 
                              (:Id, :ProfileId, :PegawaiId, :KantorId, TRUNC(SYSDATE), NVL(TO_DATE(:ValidSampai,'DD/MM/YYYY'),DECODE(:Tipe,'PPNPN',ADD_MONTHS(TRUNC(SYSDATE, 'y'), 12) - 1,NULL)), :StatusPlt, :StatusBooking, :NomorSK, 'Pengaturan Akses', :UserId, SYSDATE)";
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("Id", _profile.ProfilePegawaiId));
                        arrayListParameters.Add(new OracleParameter("ProfileId", _profile.ProfileId));
                        arrayListParameters.Add(new OracleParameter("PegawaiId", _profile.PegawaiId));
                        arrayListParameters.Add(new OracleParameter("KantorId", kantorid));
                        arrayListParameters.Add(new OracleParameter("ValidSampai", _profile.ValidSampai));
                        arrayListParameters.Add(new OracleParameter("Tipe", tipeakun));
                        arrayListParameters.Add(new OracleParameter("StatusPlt", _profile.StatusPlt));
                        arrayListParameters.Add(new OracleParameter("StatusBooking", _profile.BisaBooking));
                        arrayListParameters.Add(new OracleParameter("NomorSK", _profile.NomorSK));
                        arrayListParameters.Add(new OracleParameter("UserId", userid));
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

        public Profile GetProfilePimpinan(string unitkerjaid)
        {
            var records = new Profile();

            string query = @"
                SELECT JB.PROFILEID, JB.NAMA AS NAMAPROFILE
                FROM UNITKERJA UK
                INNER JOIN JABATAN JB ON
	                JB.UNITKERJAID = UK.UNITKERJAID AND
	                JB.TIPEESELONID = CASE WHEN UK.TIPEKANTORID > 2 THEN 3 ELSE UK.TIPEKANTORID END
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PROFILEID = JB.PROFILEID AND
	                NVL(JP.STATUSHAPUS,'0') = '0' AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                WHERE UK.UNITKERJAID = :param1
                GROUP BY JB.PROFILEID, JB.NAMA
                ORDER BY LENGTH(JB.PROFILEID)";

            using (var ctx = new BpnDbContext())
            {
                var arrayListParameters = new ArrayList();
                arrayListParameters.Add(new OracleParameter("param1", unitkerjaid));
                var parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, parameters).FirstOrDefault();
            }

            return records;
        }

        public TransactionResult KirimPengajuanProfileKKP(string uid, string pid, string tip, string kid, string vsp, ProfilePegawai dt, string id, string akunid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        string sql = string.Format(@"
                            INSERT INTO {0}.PERSETUJUANAKSES (PERSETUJUANID, PROFILEID, TIPE, USERPEMBUAT, TANGGALDIBUAT, KANTORID, STATUS)
                            VALUES (:PersetujuanId, :ProfileId, :Tipe, :UserId, SYSDATE, :KantorId, 'W')", skema);
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("PersetujuanId", id));
                        arrayListParameters.Add(new OracleParameter("ProfileId", pid));
                        arrayListParameters.Add(new OracleParameter("Tipe", tip));
                        arrayListParameters.Add(new OracleParameter("UserId", uid));
                        arrayListParameters.Add(new OracleParameter("KantorId", kid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql = string.Format(@"
                                    INSERT INTO {0}.AKSESKKP (AKSESID, PERSETUJUANID, USERID, KANTORID, PROFILEID, TIPESTATUS, USERPEMBUAT, TANGGALDIBUAT, STATUS, STATUSPLT, BISABOOKING)
                                    VALUES (RAWTOHEX(SYS_GUID()), :PersetujuanId, :AkunId, :KantorId, :ProfileId, 'I', :UserId, SYSDATE, 'W', :StatusPLT, :BisaBooking)", skema);
                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new OracleParameter("PersetujuanId", id));
                        arrayListParameters.Add(new OracleParameter("AkunId", akunid));
                        arrayListParameters.Add(new OracleParameter("KantorId", kid));
                        arrayListParameters.Add(new OracleParameter("ProfileId", dt.ProfileId));
                        arrayListParameters.Add(new OracleParameter("UserId", uid));
                        arrayListParameters.Add(new OracleParameter("StatusPLT", dt.StatusPlt));
                        arrayListParameters.Add(new OracleParameter("BisaBooking", dt.BisaBooking));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        if (!string.IsNullOrEmpty(vsp))
                        {
                            sql = string.Format(@" UPDATE {0}.AKSESKKP SET VALIDSAMPAI = TO_DATE(:Tanggal,'DD/MM/YYYY') 
                                         WHERE PERSETUJUANID = :Id ", skema);
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("Tanggal", vsp));
                            arrayListParameters.Add(new OracleParameter("Id", id));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tr.Status = true;
                        tc.Commit();
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

        public string GetSatkerId(string unitkerjaid)
        {
            string result = unitkerjaid;

            string query = "SELECT DECODE(TIPEKANTORID,1,UNITKERJAID,KANTORID) AS SATKERID FROM UNITKERJA WHERE UNITKERJAID = :UnitKerjaId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public List<Kantor> GetAllKantor()
        {
            var records = new List<Kantor>();

            string query = "SELECT KANTORID, NAMA AS NAMAKANTOR FROM KANTOR WHERE NAMAALIAS IS NOT NULL AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) ORDER BY KODE";

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Kantor>(query).ToList();
            }

            return records;
        }

        public string GetNamaKantorById(string kantorid)
        {
            string result = "";

            string query = "SELECT NAMA FROM KANTOR WHERE KANTORID = :param1  AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("param1", kantorid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).First();
            }

            return result;
        }

        public List<UnitKerja> GetListUnitKerjaByKantorInduk(string kid)
        {
            var records = new List<UnitKerja>();
            var aParams = new ArrayList();

            string query = @"
                SELECT
                  UK.UNITKERJAID, UK.NAMAUNITKERJA
                FROM UNITKERJA UK
                WHERE
	                UK.KANTORID = :param1 AND
                  UK.UNITKERJAID IS NOT NULL AND
                  UK.TAMPIL = 1 AND
                  UNITKERJAID IS NOT NULL
                UNION ALL
                SELECT
                  UK.UNITKERJAID, UK.NAMAUNITKERJA
                FROM UNITKERJA UK
                  INNER JOIN KANTOR KT ON
                    KT.KANTORID = UK.KANTORID AND
                    KT.INDUK = :param2
                WHERE
                  UK.UNITKERJAID IS NOT NULL AND
                  UK.TAMPIL = 1 AND
                  UNITKERJAID IS NOT NULL";

            using (var ctx = new BpnDbContext())
            {
                aParams.Add(new OracleParameter("param1", kid));
                aParams.Add(new OracleParameter("param2", kid));
                records = ctx.Database.SqlQuery<UnitKerja>(query,aParams.ToArray()).ToList();
            }

            return records;
        }

        public TransactionResult InsertPengajuanJabatanPegawai(UserLogin userlogin, string kantorid, string pengajuanid, string validsejak, string validsampai)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            string sql = string.Empty;
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                sql = string.Format("SELECT COUNT(1) FROM {0}.PENGAJUANJABATANPETUGAS WHERE PROFILEID = :param1 AND PEGAWAIID = :param2 AND KANTORID = :param3 AND STATUS = 'W'", skema);
                sql = sWhitespace.Replace(sql, " ");
                aParams.Clear();
                aParams.Add(new OracleParameter("param1", userlogin.ProfileId));
                aParams.Add(new OracleParameter("param2", userlogin.PegawaiId));
                aParams.Add(new OracleParameter("param3", kantorid));
                if (ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).FirstOrDefault() > 0)
                {
                    tr.Pesan = "Pegawai dalam proses pengajuan jabatan ini";
                    return tr;
                }
                sql = "SELECT COUNT(1) FROM JABATANPEGAWAI WHERE PROFILEID = :param1 AND PEGAWAIID = :param2 AND KANTORID = :param3 AND NVL(STATUSHAPUS,'0') = '0' AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                sql = sWhitespace.Replace(sql, " ");
                aParams.Clear();
                aParams.Add(new OracleParameter("param1", userlogin.ProfileId));
                aParams.Add(new OracleParameter("param2", userlogin.PegawaiId));
                aParams.Add(new OracleParameter("param3", kantorid));
                if (ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).FirstOrDefault() > 0)
                {
                    tr.Pesan = "Pegawai telah memiliki jabatan ini";
                    return tr;
                }
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string _profileidPersetujuan = "A80100";
                        int tipekantorid = GetTipeKantor(kantorid);
                        if(tipekantorid > 2)
                        {
                            string _kantorInduk = GetKantorIdIndukFromKantorId(kantorid);
                            string _unitInduk = GetUnitKerjaIdFromKantorId(_kantorInduk);
                            sql = "SELECT PROFILEIDTU FROM JABATAN WHERE UNITKERJAID = :param1 AND PROFILEID = PROFILEIDTU GROUP BY PROFILEIDTU";
                            sql = sWhitespace.Replace(sql, " ");
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", _unitInduk));
                            _profileidPersetujuan = ctx.Database.SqlQuery<string>(sql, aParams.ToArray()).FirstOrDefault();
                        }

                        sql = string.Format(
                            @"INSERT INTO {0}.PENGAJUANJABATANPETUGAS (
                                PENGAJUANID, PROFILEID, PEGAWAIID, KANTORID, STATUSPLT, VALIDSEJAK, USERPEMBUAT, PROFILEIDPERSETUJUAN) VALUES 
                                (
                                    :param1, :param2, :param3, :param4, :param5, TO_DATE(:param6,'DD/MM/YYYY'), :param7, :param8)", skema);
                        sql = sWhitespace.Replace(sql, " ");
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", pengajuanid));
                        aParams.Add(new OracleParameter("param2", userlogin.ProfileId));
                        aParams.Add(new OracleParameter("param3", userlogin.PegawaiId));
                        aParams.Add(new OracleParameter("param4", kantorid));
                        aParams.Add(new OracleParameter("param5", userlogin.TipeJabatan));
                        aParams.Add(new OracleParameter("param6", validsejak));
                        aParams.Add(new OracleParameter("param7", userlogin.UserId));
                        aParams.Add(new OracleParameter("param8", _profileidPersetujuan));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        if (!string.IsNullOrEmpty(validsampai))
                        {
                            sql = string.Format(@" UPDATE {0}.PENGAJUANJABATANPETUGAS SET VALIDSAMPAI = TO_DATE(:param1,'DD/MM/YYYY') 
                                         WHERE PENGAJUANID = :Id ", skema);
                            sql = sWhitespace.Replace(sql, " ");
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", validsampai));
                            aParams.Add(new OracleParameter("Id", pengajuanid));
                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                        }

                        tc.Commit();
                        //tc.Rollback();
                        tr.Status = true;
                        tr.ReturnValue = pengajuanid;
                        tr.Pesan = "Pengajuan Berhasil Dikirim";
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

        public string getKantorIdFromPengajuanJabatan(string id)
        {
            string result = "";
            string skema = OtorisasiUser.NamaSkema;

            string query = string.Format("SELECT KANTORID FROM {0}.PENGAJUANJABATANPETUGAS WHERE PENGAJUANID = :Param1", skema);

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("Param1", id));

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
            }

            return result;
        }

        public TransactionResult persetujuanJabatanPelaksana(string uid, string pid, string persetujuanid, bool resp, string alasan = null)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (resp)
                        {
                            ArrayList aParams = new ArrayList();
                            string sql = string.Format(@"
                                SELECT
                                  PJP.PENGAJUANID, 
                                  PJP.PROFILEID, 
                                  PG.PEGAWAIID,
                                  PJP.KANTORID, 
                                  PJP.STATUSPLT, 
                                  PJP.VALIDSEJAK,
                                  NVL(PJP.VALIDSAMPAI,ADD_MONTHS(TRUNC(SYSDATE, 'y'), 12) - 1) AS VALIDSAMPAI
                                FROM {0}.PENGAJUANJABATANPETUGAS PJP
                                  INNER JOIN PEGAWAI PG ON
                                    PG.PEGAWAIID = PJP.PEGAWAIID
                                WHERE
                                  PJP.PENGAJUANID = :param1 AND
                                  PJP.STATUS = 'W'", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", persetujuanid));
                            var _jabatan = ctx.Database.SqlQuery<PersetujuanJabatan>(sql, aParams.ToArray()).FirstOrDefault();

                            if (skema.ToLower().Equals("surat"))
                            {
                                sql = string.Format(@"
                                        UPDATE {0}.PENGAJUANJABATANPETUGAS SET STATUS = 'A', VALIDSAMPAI = NVL(VALIDSAMPAI,ADD_MONTHS(TRUNC(SYSDATE, 'y'), 12) - 1), TANGGALPERUBAHAN = SYSDATE, USERPERUBAHAN = :param1 WHERE PENGAJUANID = :param2 AND STATUS = 'W'", skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("param1", uid));
                                aParams.Add(new OracleParameter("param2", _jabatan.PengajuanId));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                sql = string.Format(@"
                                        INSERT INTO JABATANPEGAWAI (PROFILEPEGAWAIID, PROFILEID, PEGAWAIID, KANTORID, VALIDSEJAK, VALIDSAMPAI, KETERANGAN, USERUPDATE, LASTUPDATE, JENISTRUKTUR, STATUSPLT, TIPEPROFILE, TANGGALPERUBAHAN, NOMORSK)
                                        VALUES (:param1, :param2, :param3, :param4, :param5, :param6, 'Persetujuan Pelaksana', :param7, SYSDATE, 1, :param8, 1, SYSDATE,:param9)", skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("param1", _jabatan.PengajuanId));
                                aParams.Add(new OracleParameter("param2", _jabatan.ProfileId));
                                aParams.Add(new OracleParameter("param3", _jabatan.PegawaiId));
                                aParams.Add(new OracleParameter("param4", _jabatan.KantorId));
                                aParams.Add(new OracleParameter("param5", _jabatan.ValidSejak));
                                aParams.Add(new OracleParameter("param6", _jabatan.ValidSampai));
                                aParams.Add(new OracleParameter("param7", uid));
                                aParams.Add(new OracleParameter("param8", _jabatan.StatusPLT));
                                aParams.Add(new OracleParameter("param9", persetujuanid));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            }

                            tr.Pesan = "Pengajuan Berhasil diproses";
                        }
                        else
                        {
                            ArrayList arrayListParameters = new ArrayList();
                            string sql = string.Format(@"
                                UPDATE {0}.PENGAJUANJABATANPETUGAS SET STATUS = 'R', TANGGALTOLAK = SYSDATE, USERTOLAK = :param1, ALASANTOLAK = :param2 WHERE PENGAJUANID = :param3 AND STATUS = 'W'", skema);
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new OracleParameter("param1", uid));
                            arrayListParameters.Add(new OracleParameter("param2", alasan));
                            arrayListParameters.Add(new OracleParameter("param3", persetujuanid));
                            ctx.Database.ExecuteSqlCommand(sql, arrayListParameters.ToArray());

                            tr.Pesan = "Pengajuan Berhasil ditolak";
                        }

                        tr.Status = true;
                        tc.Commit();
                        //tc.Rollback();
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

        public int GetCountPengajuanJabatan(string pegawaiid, string unitkerjaid)
        {
            int result = 0;
            string skema = OtorisasiUser.NamaSkema;
            try
            {
                string query = string.Format(@"
                SELECT COUNT(DISTINCT PJP.PENGAJUANID)
                FROM {0}.PENGAJUANJABATANPETUGAS PJP
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PROFILEID = PJP.PROFILEIDPERSETUJUAN AND
	                JP.PEGAWAIID = :param1 AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(JP.STATUSHAPUS,'0') = '0'
                INNER JOIN JABATAN P ON
	                P.PROFILEID = JP.PROFILEID AND
	                NVL(P.UNITKERJAID,'020116') = DECODE(PJP.PROFILEIDPERSETUJUAN,'A80100',NVL(P.UNITKERJAID,'020116'),:param2)
                WHERE
	                PJP.STATUS = 'W' AND
                    (PJP.VALIDSAMPAI IS NULL OR TRUNC(CAST(PJP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(PJP.STATUSHAPUS,'0') = '0'", skema);

                ArrayList arrayListParameters = new ArrayList();
                arrayListParameters.Add(new OracleParameter("param1", pegawaiid));
                arrayListParameters.Add(new OracleParameter("param2", unitkerjaid));

                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<int>(query, parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return 0;
            }

            return result;
        }

        public CetakPersetujuanJabatan getPengajuanJabatan(string id)
        {
            var data = new CetakPersetujuanJabatan();

            ArrayList arrayListParameters = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;

            try
            {
                string query = string.Format(@"
                    SELECT
                      PA.PENGAJUANID,
                      TO_CHAR(PA.TANGGALPENGAJUAN, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                      TO_CHAR(SYSDATE, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') AS TANGGALPERSETUJUAN,
                      PG.PEGAWAIID AS TARGETID,
                      PG.NAMA AS NAMATARGET,
                      JB.NAMA AS JABATAN,
                      KT.NAMA AS KANTORTARGET,
                      PA.STATUSPLT
                    FROM {0}.PENGAJUANJABATANPETUGAS PA
                      INNER JOIN JABATAN JB ON
                        JB.PROFILEID = PA.PROFILEID
                      INNER JOIN PEGAWAI PG ON
                        PG.PEGAWAIID = PA.PEGAWAIID
                      INNER JOIN KANTOR KT ON
  	                    KT.KANTORID = PA.KANTORID
                    WHERE
                      PA.PENGAJUANID = :param1 AND
                      PA.STATUS = 'W' AND
                      (PA.VALIDSAMPAI IS NULL OR TRUNC(CAST(PA.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                      NVL(PA.STATUSHAPUS,'0') = '0'
                    GROUP BY
                      PA.PENGAJUANID, PA.TANGGALPENGAJUAN,
                      JB.NAMA, PG.PEGAWAIID, PG.NAMA, KT.NAMA, PA.STATUSPLT", skema);

                using (var ctx = new BpnDbContext())
                {
                    arrayListParameters.Add(new OracleParameter("param1", id));
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    data = ctx.Database.SqlQuery<CetakPersetujuanJabatan>(query, parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return data;
        }

        public UnitKerja GetUnitKerjaDetail(string unitkerjaid)
        {
            var data = new UnitKerja();
            var aParams = new ArrayList();

            string query = @"
                SELECT
	                UK.UNITKERJAID,
	                UK.NAMAUNITKERJA,
	                NVL(UK.LATITUDE,CAST(K.LATITUDE AS VARCHAR2(20))) AS LATITUDE,
	                NVL(UK.LONGITUDE,CAST(K.LONGITUDE AS VARCHAR2(20))) AS LONGITUDE
                FROM UNITKERJA UK
                INNER JOIN KANTOR K ON
	                K.KANTORID = UK.KANTORID
                WHERE
                  UK.TAMPIL = 1 AND
                  UK.UNITKERJAID = :param1";

            aParams.Add(new OracleParameter("param1", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                data = ctx.Database.SqlQuery<UnitKerja>(query, aParams.ToArray()).FirstOrDefault();
            }

            return data;
        }

        public List<Pegawai> GetPegawaiByUnitKerjaAndTipe(string unitkerjaid, string tipepegawai)
        {
            List<Pegawai> records = new List<Pegawai>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"
                SELECT
                  ROW_NUMBER() OVER(ORDER BY TIPEUSER, TIPEESELONID, NAMA) AS RNUMBER,
                  COUNT(1) OVER() TOTAL,
                  PEGAWAIID,
                  NAMA,
                  PROFILEID,
                  JABATAN,
                  NAMALENGKAP,
                  TIPEESELONID,
                  NAMA || ', ' || JABATAN AS NAMADANJABATAN
                FROM
                  (SELECT
                     PG.PEGAWAIID,
                     PG.NAMA,
                     JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '') AS JABATAN,
                     DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, NULL, '', ', ' || PG.GELARBELAKANG) AS NAMALENGKAP,
                     JB.PROFILEID,
                     JB.TIPEESELONID, 0 AS TIPEUSER
                   FROM PEGAWAI PG
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PG.PEGAWAIID AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                     JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = JB.UNITKERJAID AND
                       UK.UNITKERJAID = :UNITKERJAID1
                   WHERE
                       (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                   UNION
                   SELECT
                     PP.NIK AS PEGAWAIID,
                     PP.NAMA,
                     JB.NAMA AS JABATAN,
                     PP.NAMA AS NAMALENGKAP,
                     JB.PROFILEID,
                     JB.TIPEESELONID,
                     1 AS TIPEUSER
                   FROM PPNPN PP
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PP.NIK AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       JB.PROFILEID = JP.PROFILEID  AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                     JOIN UNITKERJA UK ON
                       UK.UNITKERJAID = JB.UNITKERJAID AND
                       UK.UNITKERJAID = :UNITKERJAID2)
                WHERE
                  PEGAWAIID IS NOT NULL";

            arrayListParameters.Add(new OracleParameter("UnitKerjaId1", unitkerjaid));
            arrayListParameters.Add(new OracleParameter("UnitKerjaId2", unitkerjaid));

            if (!string.IsNullOrEmpty(tipepegawai))
            {
                if (tipepegawai.Equals("ASN"))
                {
                    query = string.Concat(query, " AND TIPEUSER = 0");
                }
                if (tipepegawai.Equals("PPNPN"))
                {
                    query = string.Concat(query, " AND TIPEUSER = 1");
                }
            }

            query += " ORDER BY tipeuser, tipeeselonid, nama ";

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pegawai>(query, parameters).ToList<Pegawai>();
            }

            return records;
        }


        // 10 Mei 2022
        public bool CheckUserAdminKearsipan(string pegawaiid, string kantorid, string unitkerjaid)
        {
            bool result = false;

            string query = "SELECT COUNT(1)" + " FROM " + OtorisasiUser.NamaSkema + ".hakakses WHERE statushapus = '0' AND aksesid = 'AdminKearsipan' AND  pegawaiid = :PegawaiId  AND kantorid = :KantorId AND unitkerjaid = :UnitKerjaId ";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new NpgsqlParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new NpgsqlParameter("KantorId", kantorid));
            arrayListParameters.Add(new NpgsqlParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }
        public bool CheckUserAdminPersuratan(string pegawaiid, string kantorid, string unitkerjaid)
        {
            bool result = false;

            string query = "SELECT COUNT(1)" + " FROM " + OtorisasiUser.NamaSkema + ".hakakses WHERE statushapus = '0' AND aksesid = 'AdminPersuratan'  AND pegawaiid = :PegawaiId  AND kantorid = :KantorId AND unitkerjaid = :UnitKerjaId ";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new NpgsqlParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new NpgsqlParameter("KantorId", kantorid));
            arrayListParameters.Add(new NpgsqlParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }
        public bool CheckUserEntriPersuratan(string pegawaiid, string kantorid, string unitkerjaid)
        {
            bool result = false;

            string query = "SELECT COUNT(1)" + " FROM " + OtorisasiUser.NamaSkema + ".hakakses WHERE statushapus = '0' AND aksesid = 'Persuratan'  AND pegawaiid = :PegawaiId  AND kantorid = :KantorId AND unitkerjaid = :UnitKerjaId ";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new NpgsqlParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new NpgsqlParameter("KantorId", kantorid));
            arrayListParameters.Add(new NpgsqlParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }
        public bool CheckUserEntriKearsipan(string pegawaiid, string kantorid, string unitkerjaid)
        {
            bool result = false;

            string query = "SELECT COUNT(1)" + " FROM " + OtorisasiUser.NamaSkema + ".hakakses WHERE statushapus = '0' AND aksesid = 'Kearsipan' AND pegawaiid = :PegawaiId  AND kantorid = :KantorId AND unitkerjaid = :UnitKerjaId ";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new NpgsqlParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new NpgsqlParameter("KantorId", kantorid));
            arrayListParameters.Add(new NpgsqlParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new PostgresDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public string GetProfileTUByNipUnitKerja(string pegawaiid, string unitkerjaid)
        {
            var data = string.Empty;

            string query = @"
                SELECT JB.PROFILEID
                FROM JABATAN JB
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PEGAWAIID = :param1 AND
                    JP.PROFILEID = JB.PROFILEID AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                WHERE
                  NVL(JB.SEKSIID,'X') <> 'A800' AND
                  JB.PROFILEIDTU IS NOT NULL AND
                  JB.UNITKERJAID = :param2 AND
                  (JB.PROFILEIDTU = JB.PROFILEID OR
                  JB.PROFILEIDBA = JB.PROFILEID)";

            var aParams = new ArrayList();
            aParams.Add(new OracleParameter("param1", pegawaiid));
            aParams.Add(new OracleParameter("param2", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                var records = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).ToList();
                if(records.Count() > 1)
                {
                    foreach (string strProfileId in records)
                    {
                        data += "'" + strProfileId + "',";
                    }

                    if (data.Length > 0)
                    {
                        data = data.Remove(data.Length - 1, 1);
                    }
                }
                else if(records.Count() == 1)
                {
                    data = records[0].ToString();
                }
            }

            return data;
        }

        public bool checkUseridOnUnitkerjaid(string unitkerjaid, string userid)
        {
            var result = false;

            string query = @"
                SELECT COUNT(1)
                FROM JABATAN JB
                  LEFT JOIN PEGAWAI PG ON
                    PG.USERID = :param1 AND
                    (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                  LEFT JOIN PPNPN PP ON
                    PP.USERID = :param2
                  INNER JOIN JABATANPEGAWAI JP ON
  	                JP.PEGAWAIID = NVL(PG.PEGAWAIID,PP.NIK) AND
                    JP.PROFILEID = JB.PROFILEID AND
                    NVL(JP.STATUSHAPUS,'0') = '0' AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                WHERE
                  JB.UNITKERJAID = :param3 AND
                  NVL(JB.SEKSIID, 'XXX') <> 'A800' AND
                  (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new OracleParameter("param1", userid));
            arrayListParameters.Add(new OracleParameter("param2", userid));
            arrayListParameters.Add(new OracleParameter("param3", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).FirstOrDefault() > 0;
            }

            return result;
        }
    }
}