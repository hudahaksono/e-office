﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;

namespace Surat.Models
{
    public class LaporanModel
    {
        public List<LaporanKantor> GetPegawaiKantor(string kantorid, decimal tipe = 0)
        {
            List<LaporanKantor> pegawaiKantor = new List<LaporanKantor>();
            ArrayList arrayListParameters = new ArrayList();
            string addwhere = string.Empty;

            if (tipe > 0)
            {
                arrayListParameters.Clear();
                if (tipe == 1)
                {
                    addwhere = " AND KT.INDUK = :kantorid ";
                }
                else if (tipe == 2)
                {
                    addwhere = " AND KT.KANTORID = :kantorid ";
                }
                arrayListParameters.Add(new OracleParameter(":kantorid", kantorid));
            }

            string query = $@"SELECT
                                RST.KANTORID, RST.KANTORNAMA, COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI, COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE null END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL,PE.ESELON) RST
                            GROUP BY RST.KANTORID, RST.KANTORNAMA
                            ORDER BY RST.KANTORNAMA";
            using (var ctx = new BpnDbContext())
            {
                if (!string.IsNullOrEmpty(kantorid))
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    pegawaiKantor = ctx.Database.SqlQuery<LaporanKantor>(query, parameters).ToList();
                }
                else
                {
                    pegawaiKantor = ctx.Database.SqlQuery<LaporanKantor>(query).ToList();
                }
            }

            return pegawaiKantor;
        }

        public List<LaporanKantor> GetPenggunaEofficeKantor(string kantorid, decimal tipe = 0)
        {
            List<LaporanKantor> penggunaKantor = new List<LaporanKantor>();
            ArrayList arrayListParameters = new ArrayList();
            string addwhere = string.Empty;

            if (tipe > 0)
            {
                arrayListParameters.Clear();
                if (tipe == 1)
                {
                    addwhere = " AND (KT.INDUK = :kantorid OR KT.KANTORID = :kantorid) ";
                }
                else if (tipe == 2)
                {
                    addwhere = " AND KT.KANTORID = :kantorid ";
                }
                arrayListParameters.Add(new OracleParameter(":kantorid", kantorid));
            }

            string query = $@"
                            SELECT JML.KANTORID, JML.KANTORNAMA, JML.TOTALPEGAWAI, JML.ST, NVL(DATA.JUMLAH, 0) AS JUMLAH
                            FROM
                            (SELECT
                                RST.KANTORID, RST.KANTORNAMA, COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI, COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE null END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL,PE.ESELON) RST
                            GROUP BY RST.KANTORID, RST.KANTORNAMA
                            ORDER BY RST.KANTORNAMA) JML	
                            LEFT JOIN
                            (SELECT
                              RST.KANTORID, COUNT(RST.PEGAWAIID) AS JUMLAH
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL) RST
                               WHERE
                                 RST.PEGAWAIID IN (SELECT NIP FROM SURAT.SURATINBOX WHERE KANTORID = RST.KANTORID AND NIP = RST.PEGAWAIID AND TANGGALBUKA IS NOT NULL AND NVL(STATUSHAPUS,'0') = '0' GROUP BY NIP)
                               GROUP BY RST.KANTORID, RST.KANTORNAMA
                               ORDER BY RST.KANTORNAMA) DATA ON JML.KANTORID = DATA.KANTORID";

            using (var ctx = new BpnDbContext())
            {
                if (!string.IsNullOrEmpty(kantorid))
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query, parameters).ToList();
                }
                else
                {
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query).ToList();
                }
            }
            return penggunaKantor;
        }

        public List<DetailLaporan> GetDetailLaporanDaerah(string kantorid,string menu)
        {
            List<DetailLaporan> details = new List<DetailLaporan>();
            ArrayList arrayListParameters = new ArrayList();

            if (!string.IsNullOrEmpty(kantorid))
            {
                string query = string.Empty;
                string kriteria = string.Empty;
                if (menu == "PenggunaEoffice")
                {
                    kriteria = " SELECT NIP FROM SURAT.SURATINBOX WHERE KANTORID = RS.KANTORID AND NIP = RS.PEGAWAIID AND TANGGALBUKA IS NOT NULL AND NVL(STATUSHAPUS,'0') = '0' GROUP BY NIP ";
                } else if (menu == "PenggunaTTE")
                {
                    kriteria = @" SELECT PG.PEGAWAIID 
                                  FROM SURAT.TBLDOKUMENELEKTRONIK TDE 
                                  INNER JOIN SURAT.TBLDOKUMENTTE TTE 
                                    ON TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID 
                                    AND TTE.STATUS = 'A' AND NVL(TTE.STATUSHAPUS,'0') = '0' 
                                  INNER JOIN PEGAWAI PG 
                                    ON PG.PEGAWAIID = RS.PEGAWAIID 
                                    AND PG.USERID = TTE.USERPENANDATANGAN 
                                    AND (PG.VALIDSAMPAI IS NULL 
                                    OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) 
                                  WHERE NVL(TDE.STATUSHAPUS,'0') = '0' 
                                    AND TDE.KANTORID = RS.KANTORID ";
                } else if (menu == "PendaftarTTE")
                {
                    kriteria = @" SELECT NIP FROM REGISTERUSERTTDDIGITAL ";
                }
                query = $@"SELECT RS.KANTORID, RS.TIPE, RS.KANTORNAMA, RS.PEGAWAIID, RS.EMAIL, RS.ESELON, RS.NAMAPEGAWAI,
                               CASE WHEN RS.PEGAWAIID IN ({kriteria}) THEN 'AKTIF' ELSE 'TIDAK' END AS STATUS
                               FROM
                               (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON, PE.NAMA AS NAMAPEGAWAI                               
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                   AND KT.KANTORID = :kantorid 
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL,PE.ESELON, PE.NAMA) RS";
                arrayListParameters.Add(new OracleParameter(":kantorid", kantorid));
                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    details = ctx.Database.SqlQuery<DetailLaporan>(query, parameters).ToList();
                }
            }
            return details;
        }

        public List<DetailLaporan> GetDetailLaporanPusat(string unitkerjaid, string menu)
        {
            List<DetailLaporan> details = new List<DetailLaporan>();
            ArrayList arrayListParameters = new ArrayList();
            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                string query = string.Empty;
                string kriteria = string.Empty;
                if (menu == "PenggunaEoffice")
                {
                    kriteria = " SELECT NIP FROM SURAT.SURATINBOX WHERE KANTORID = RS.UNITKERJAID AND NIP = RS.PEGAWAIID AND TANGGALBUKA IS NOT NULL AND NVL(STATUSHAPUS,'0') = '0' GROUP BY NIP ";
                }
                else if (menu == "PenggunaTTE")
                {
                    kriteria = @" SELECT PG.PEGAWAIID 
                                  FROM SURAT.TBLDOKUMENELEKTRONIK TDE 
                                  INNER JOIN SURAT.TBLDOKUMENTTE TTE 
                                    ON TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID 
                                    AND TTE.STATUS = 'A' AND NVL(TTE.STATUSHAPUS,'0') = '0' 
                                  INNER JOIN PEGAWAI PG 
                                    ON PG.PEGAWAIID = RS.PEGAWAIID 
                                    AND PG.USERID = TTE.USERPENANDATANGAN 
                                    AND (PG.VALIDSAMPAI IS NULL 
                                    OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) 
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = RS.PEGAWAIID AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                  INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   UK.UNITKERJAID = RS.UNITKERJAID
                                  WHERE NVL(TDE.STATUSHAPUS,'0') = '0'";
                }
                else if (menu == "PendaftarTTE")
                {
                    kriteria = @" SELECT NIP FROM REGISTERUSERTTDDIGITAL ";
                }
                query = $@"SELECT RS.UNITKERJAID, RS.TIPE, RS.KANTORNAMA, RS.PEGAWAIID, RS.EMAIL, RS.ESELON, RS.NAMAPEGAWAI,
                               CASE WHEN RS.PEGAWAIID IN ({kriteria}) THEN 'AKTIF' ELSE 'TIDAK' END AS STATUS
                               FROM
                               (SELECT
                                 UK.UNITKERJAID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON, PE.NAMA AS NAMAPEGAWAI                               
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1 AND
                                   UK.UNITKERJAID = :unitkerja
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID = 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                               GROUP BY
                                 UK.UNITKERJAID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL,PE.ESELON, PE.NAMA) RS
                               ORDER BY RS.ESELON";
                arrayListParameters.Add(new OracleParameter(":unitkerja", unitkerjaid));
                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    details = ctx.Database.SqlQuery<DetailLaporan>(query, parameters).ToList();
                }
            }
            return details;
        }


        public List<LaporanKantor> GetPenggunaTTEKantor(string kantorid, decimal tipe = 0)
        {
            List<LaporanKantor> penggunaKantor = new List<LaporanKantor>();
            ArrayList arrayListParameters = new ArrayList();
            string addwhere = string.Empty;

            if (tipe > 0)
            {
                arrayListParameters.Clear();
                if (tipe == 1)
                {
                    addwhere = " AND (KT.INDUK = :kantorid OR KT.KANTORID = :kantorid) ";
                }
                else if (tipe == 2)
                {
                    addwhere = " AND KT.KANTORID = :kantorid ";
                }
                arrayListParameters.Add(new OracleParameter(":kantorid", kantorid));
            }

            string query = $@"
                            SELECT JML.KANTORID, JML.KANTORNAMA, JML.TOTALPEGAWAI, JML.ST, NVL(DATA.JUMLAH, 0) AS JUMLAH
                            FROM
                            (SELECT
                                RST.KANTORID, RST.KANTORNAMA, COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI, COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE null END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL,PE.ESELON) RST
                            GROUP BY RST.KANTORID, RST.KANTORNAMA
                            ORDER BY RST.KANTORNAMA) JML	
                            LEFT JOIN
                            (SELECT
                              RST.KANTORID, COUNT(RST.PEGAWAIID) AS JUMLAH
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL) RST
                               WHERE
                                 RST.PEGAWAIID IN (SELECT PG.PEGAWAIID FROM SURAT.TBLDOKUMENELEKTRONIK TDE INNER JOIN SURAT.TBLDOKUMENTTE TTE ON TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID AND TTE.STATUS = 'A' AND NVL(TTE.STATUSHAPUS,'0') = '0' INNER JOIN PEGAWAI PG ON PG.PEGAWAIID = RST.PEGAWAIID AND PG.USERID = TTE.USERPENANDATANGAN AND (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) WHERE NVL(TDE.STATUSHAPUS,'0') = '0' AND TDE.KANTORID = RST.KANTORID )
                               GROUP BY RST.KANTORID, RST.KANTORNAMA
                               ORDER BY RST.KANTORNAMA) DATA ON JML.KANTORID = DATA.KANTORID";

            using (var ctx = new BpnDbContext())
            {
                if (!string.IsNullOrEmpty(kantorid))
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query, parameters).ToList();
                }
                else
                {
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query).ToList();
                }
            }
            return penggunaKantor;
        }

        public List<LaporanKantor> GetPendaftarTTE(string kantorid, decimal tipe = 0)
        {
            List<LaporanKantor> penggunaKantor = new List<LaporanKantor>();
            ArrayList arrayListParameters = new ArrayList();
            string addwhere = string.Empty;

            if (tipe > 0)
            {
                arrayListParameters.Clear();
                if (tipe == 1)
                {
                    addwhere = " AND (KT.INDUK = :kantorid OR KT.KANTORID = :kantorid) ";
                }
                else if (tipe == 2)
                {
                    addwhere = " AND KT.KANTORID = :kantorid ";
                }
                arrayListParameters.Add(new OracleParameter(":kantorid", kantorid));
            }

            string query = $@"
                            SELECT JML.KANTORID, JML.KANTORNAMA, JML.TOTALPEGAWAI, JML.ST, NVL(DATA.JUMLAH, 0) AS JUMLAH
                            FROM
                            (SELECT
                                RST.KANTORID, RST.KANTORNAMA, COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI, COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE null END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL,PE.ESELON) RST
                            GROUP BY RST.KANTORID, RST.KANTORNAMA
                            ORDER BY RST.KANTORNAMA) JML	
                            LEFT JOIN
                            (SELECT
                              RST.KANTORID, COUNT(RST.PEGAWAIID) AS JUMLAH
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, JP.PEGAWAIID,
                                 PE.EMAIL
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID > 1 AND
                                   (KT.VALIDSAMPAI IS NULL OR TRUNC(CAST(KT.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                    {addwhere}
                                 INNER JOIN REGISTERUSERTTDDIGITAL RT
	                               ON JP.PEGAWAIID = RT.NIP
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, JP.PEGAWAIID, PE.EMAIL) RST
                               GROUP BY RST.KANTORID, RST.KANTORNAMA
                               ORDER BY RST.KANTORNAMA) DATA ON JML.KANTORID = DATA.KANTORID";

            using (var ctx = new BpnDbContext())
            {
                if (!string.IsNullOrEmpty(kantorid))
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query, parameters).ToList();
                }
                else
                {
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query).ToList();
                }
            }
            return penggunaKantor;
        }


        public List<LaporanKantor> GetPenggunaEofficePusat(string ukid = null, string type = "eoffice")
        {
            List<LaporanKantor> penggunaKantor = new List<LaporanKantor>();
            ArrayList arrayListParameters = new ArrayList();
            string addwhere = string.Empty;

            if (!string.IsNullOrEmpty(ukid))
            {
                arrayListParameters.Clear();
                addwhere = $" AND (UK.UNITKERJAID = :ukid OR UK.INDUK = :induk OR SUBSTR(UK.UNITKERJAID, 0, 6) = :induk2)";
                arrayListParameters.Add(new OracleParameter(":ukid", ukid));
                arrayListParameters.Add(new OracleParameter(":induk", ukid));
                arrayListParameters.Add(new OracleParameter(":induk2", ukid));
            }
            string query = string.Empty;
            if(type == "eoffice")
            {
                query = $@"
                            SELECT
	                            RST.UNITKERJAID AS KANTORID, 
	                            RST.NAMAUNITKERJA AS KANTORNAMA, 
	                            COUNT(CASE WHEN RST.PEGAWAIID IN (SELECT NIP FROM SURAT.SURATINBOX WHERE KANTORID = RST.UNITKERJAID AND NIP = RST.PEGAWAIID AND TANGGALBUKA IS NOT NULL AND NVL(STATUSHAPUS,'0') = '0' GROUP BY NIP) THEN 1 ELSE NULL END) AS JUMLAH, 
	                            COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI,
	                            COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE NULL END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, UK.UNITKERJAID, UK.NAMAUNITKERJA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0'
                                   {addwhere}
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, UK.UNITKERJAID, UK.NAMAUNITKERJA, JP.PEGAWAIID, PE.EMAIL, PE.ESELON) RST 
                            GROUP BY RST.KANTORID, RST.KANTORNAMA, RST.UNITKERJAID, RST.NAMAUNITKERJA
                            ORDER BY RST.KANTORNAMA, RST.UNITKERJAID";
            } else if (type == "TTE")
            {
                query = $@"
                            SELECT
	                            RST.UNITKERJAID AS KANTORID, 
	                            RST.NAMAUNITKERJA AS KANTORNAMA, 
	                            COUNT(CASE WHEN RST.PEGAWAIID IN (
                                            SELECT PG.PEGAWAIID 
                                            FROM SURAT.TBLDOKUMENELEKTRONIK TDE 
                                            INNER JOIN SURAT.TBLDOKUMENTTE TTE 
                                                ON TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID 
                                                AND TTE.STATUS = 'A' AND NVL(TTE.STATUSHAPUS,'0') = '0' 
                                            INNER JOIN PEGAWAI PG 
                                                ON PG.PEGAWAIID = RST.PEGAWAIID 
                                                AND PG.USERID = TTE.USERPENANDATANGAN 
                                                AND (PG.VALIDSAMPAI IS NULL 
                                                OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) 
                                            WHERE NVL(TDE.STATUSHAPUS,'0') = '0' AND TDE.KANTORID = RST.KANTORID 
                                ) THEN 1 ELSE NULL END) AS JUMLAH, 
	                            COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI,
	                            COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE NULL END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, UK.UNITKERJAID, UK.NAMAUNITKERJA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0'
                                   {addwhere}
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, UK.UNITKERJAID, UK.NAMAUNITKERJA, JP.PEGAWAIID, PE.EMAIL, PE.ESELON) RST 
                            GROUP BY RST.KANTORID, RST.KANTORNAMA, RST.UNITKERJAID, RST.NAMAUNITKERJA
                            ORDER BY RST.KANTORNAMA, RST.UNITKERJAID";
            } else if (type == "PendaftarTTE")
            {
                query = $@"
                            SELECT
	                            RST.UNITKERJAID AS KANTORID, 
	                            RST.NAMAUNITKERJA AS KANTORNAMA, 
	                            COUNT(CASE WHEN RST.PEGAWAIID IN (
                                         SELECT NIP FROM REGISTERUSERTTDDIGITAL   
                                ) THEN 1 ELSE NULL END) AS JUMLAH, 
	                            COUNT(RST.PEGAWAIID) AS TOTALPEGAWAI,
	                            COUNT(CASE WHEN RST.ESELON IS NOT NULL THEN 1 ELSE NULL END) AS ST
                            FROM
                              (SELECT
                                 KT.KANTORID, KT.TIPEKANTORID AS TIPE, KT.NAMA AS KANTORNAMA, UK.UNITKERJAID, UK.NAMAUNITKERJA, JP.PEGAWAIID,
                                 PE.EMAIL, PE.ESELON
                               FROM SIMPEG_2702.V_PEGAWAI_EOFFICE PE
                                 INNER JOIN JABATANPEGAWAI JP ON
                                   JP.PEGAWAIID = PE.NIPBARU AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                                 INNER JOIN JABATAN JB ON
                                   JB.PROFILEID = JP.PROFILEID AND
                                   (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                   NVL(JB.SEKSIID,'XX') <> 'A800'
                                 INNER JOIN UNITKERJA UK ON
                                   UK.UNITKERJAID = JB.UNITKERJAID AND
                                   UK.TAMPIL = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0'
                                   {addwhere}
                                 INNER JOIN KANTOR KT ON
                                   KT.KANTORID = UK.KANTORID AND
                                   KT.TIPEKANTORID = 1 AND
                                   NVL(JP.STATUSHAPUS,'0') = '0' AND
                                   (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                               GROUP BY
                                 KT.KANTORID, KT.TIPEKANTORID, KT.NAMA, UK.UNITKERJAID, UK.NAMAUNITKERJA, JP.PEGAWAIID, PE.EMAIL, PE.ESELON) RST 
                            GROUP BY RST.KANTORID, RST.KANTORNAMA, RST.UNITKERJAID, RST.NAMAUNITKERJA
                            ORDER BY RST.KANTORNAMA, RST.UNITKERJAID";
            }

            using (var ctx = new BpnDbContext())
            {
                if (!string.IsNullOrEmpty(ukid))
                {
                    object[] parameters = null;
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query, parameters).ToList();
                }
                else
                {
                    penggunaKantor = ctx.Database.SqlQuery<LaporanKantor>(query).ToList();
                }
            }
            return penggunaKantor;
        }


        public List<string> GetProvinsiKantorid()
        {
            List<string> result = new List<string>();
            string sql = @"SELECT REPLACE(NAMA, 'Kantor Wilayah ', KANTORID || '|') AS PROVINSI FROM KANTOR WHERE TIPEKANTORID = 2 GROUP BY REPLACE(NAMA, 'Kantor Wilayah ', KANTORID || '|') ORDER BY REPLACE(NAMA, 'Kantor Wilayah ', KANTORID || '|')";
            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<string>(sql).ToList();
            }
            return result;
        }

        public List<string> GetListKantorid(string kantorid = null)
        {
            List<string> result = new List<string>();
            string addwhere = "";
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters = null;
            if (!string.IsNullOrEmpty(kantorid))
            {
                addwhere = " AND (INDUK = :kantorid OR KANTORID = :kantorid) ";
                arrayListParameters.Clear();
                arrayListParameters.Add(new OracleParameter("kantorid", kantorid));
            }
            string sql = $"SELECT KANTORID ||'|'|| NAMA AS KANTOR FROM KANTOR WHERE TIPEKANTORID <> 1 {addwhere}";
            using (var ctx = new BpnDbContext())
            {
                if (!string.IsNullOrEmpty(kantorid))
                {
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(sql, parameters).ToList();
                }
                else
                {
                    result = ctx.Database.SqlQuery<string>(sql).ToList();
                }
            }
            return result;
        }

        public List<string> GetListKantoridFromProvinsi(string kantorid)
        {
            List<string> result = new List<string>();
            string sql = @"SELECT KANTORID FROM KANTOR WHERE TIPEKANTORID <> 1 AND INDUK = :Provinsi";
            if (!string.IsNullOrEmpty(kantorid))
            {
                using (var ctx = new BpnDbContext())
                {
                    ArrayList arrayListParameters = new ArrayList();
                    object[] parameters = null;
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new OracleParameter("Provinsi", kantorid));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    result = ctx.Database.SqlQuery<string>(sql, parameters).ToList();
                }
            }
            return result;
        }

        public List<RekapPresensi> GetRekapPresensi(CariRekapPresensi f, int from, int to)
        {
            var records = new List<RekapPresensi>();
            string skema = OtorisasiUser.NamaSkema;
            var lstparams = new List<object>();
            string query = string.Empty;
            query = string.Format(@"
	            SELECT
		            TLP.PEGAWAIID,
		            TLP.KANTORID,
		            TLP.UNITKERJAID,
		            TRUNC(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) AS PERIOD,
		            MIN(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) AS MASUK,
		            MAX(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) AS KELUAR,
		            'EOFFICE' AS SUMBER
	            FROM {0}.TBLLOGPRESENSI TLP
	            INNER JOIN KANTOR KT ON
		            KT.KANTORID = TLP.KANTORID", skema);
            string param = string.Empty;
            if (!string.IsNullOrEmpty(f.UnitKerjaId))
            {
                param = string.Concat(param, string.IsNullOrEmpty(param)?" WHERE ":" AND ", "TLP.UNITKERJAID = :param1");
                lstparams.Add(new OracleParameter("param1", f.UnitKerjaId));
            }
            if (!string.IsNullOrEmpty(f.PegawaiId))
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TLP.PEGAWAIID = :param2");
                lstparams.Add(new OracleParameter("param2", f.PegawaiId));
            }
            if(f.TanggalMulai != null)
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TRUNC(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) >= TRUNC(TO_DATE(:param3,'DD/MM/YYYY'))");
                lstparams.Add(new OracleParameter("param3", ((DateTime)f.TanggalMulai).ToString("dd/MM/yyyy")));
            }
            if (f.TanggalSampai != null)
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TRUNC(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) <= TRUNC(TO_DATE(:param4,'DD/MM/YYYY'))");
                lstparams.Add(new OracleParameter("param4", ((DateTime)f.TanggalSampai).ToString("dd/MM/yyyy")));
            }
            param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "NVL(TLP.STATUSHAPUS,'0') = '0'");

            query = string.Concat(@"
            SELECT
              ROW_NUMBER() OVER (ORDER BY RST.PERIOD) AS RNUMBER,
              COUNT(1) OVER() AS TOTAL,
	            RST.PEGAWAIID,
	            NVL(PG.NAMA,PP.NAMA) AS NAMAPEGAWAI,
	            RST.KANTORID,
	            CASE UK.TIPEKANTORID WHEN 1 THEN 'Kantor Pusat' ELSE UK.NAMAUNITKERJA END AS NAMAKANTOR,
	            RST.UNITKERJAID,
	            UK.NAMAUNITKERJA,
	            RST.PERIOD,
	            CASE WHEN CAST(TO_CHAR(RST.MASUK,'HH24') AS INTEGER) < 12 THEN TO_CHAR(RST.MASUK,'HH24:MI:SS') ELSE '-' END AS MASUK,
	            CASE WHEN CAST(TO_CHAR(RST.KELUAR,'HH24') AS INTEGER) >= 12 THEN TO_CHAR(RST.KELUAR,'HH24:MI:SS') ELSE '-' END AS KELUAR
            FROM (",query,param, @"
	            GROUP BY
		            TLP.PEGAWAIID,
		            TLP.KANTORID,
		            TLP.UNITKERJAID,
		            TRUNC(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END)) RST
            INNER JOIN UNITKERJA UK ON
	            UK.UNITKERJAID = RST.UNITKERJAID AND
                UK.TAMPIL = 1
            LEFT JOIN PEGAWAI PG ON
	            PG.PEGAWAIID = RST.PEGAWAIID AND
                (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
            LEFT JOIN PPNPN PP ON
	            PP.NIK = RST.PEGAWAIID");
            if (!string.IsNullOrEmpty(f.TipePegawai))
            {
                if (f.TipePegawai.Equals("ASN"))
                {
                    query = query.Replace("LEFT JOIN PEGAWAI", "INNER JOIN PEGAWAI");
                }
                if (f.TipePegawai.Equals("PPNPN"))
                {
                    query = query.Replace("LEFT JOIN PPNPN", "INNER JOIN PPNPN");
                }
            }

            if (from + to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                lstparams.Add(new OracleParameter("pStart", from));
                lstparams.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<RekapPresensi>(query, lstparams.ToArray()).ToList();
            }

            return records;
        }

        public List<RekapPresensi> GetRekapPresensiSimpeg(CariRekapPresensi f, int from, int to)
        {
            var records = new List<RekapPresensi>();
            string skema = OtorisasiUser.NamaSkema;
            var lstparams = new List<object>();
            string query = string.Empty;
            query = @"
                SELECT
	                 TLP.NIP AS PEGAWAIID,
	                 JP.KANTORID,
	                 JB.UNITKERJAID,
	                 TRUNC(TLP.CHECKTIME + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) AS PERIOD,
	                 MIN(TLP.CHECKTIME + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) AS MASUK,
	                 MAX(TLP.CHECKTIME + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) AS KELUAR,
	                 'SIMPEG' AS SUMBER
                FROM SIMPEG_2702.ABSN_ATT_LOG_MOBILE TLP
                INNER JOIN JABATANPEGAWAI JP ON
	                JP.PEGAWAIID = TLP.NIP
                INNER JOIN JABATAN JB ON
	                JB.PROFILEID = JP.PROFILEID AND
	                NVL(JB.SEKSIID,'XXX') <> 'A800'
                INNER JOIN KANTOR KT ON
 	                KT.KANTORID = JP.KANTORID";
            string param = string.Empty;
            if (!string.IsNullOrEmpty(f.UnitKerjaId))
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "JB.UNITKERJAID = :param1");
                lstparams.Add(new OracleParameter("param1", f.UnitKerjaId));
            }
            if (!string.IsNullOrEmpty(f.PegawaiId))
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TLP.NIP = :param2");
                lstparams.Add(new OracleParameter("param2", f.PegawaiId));
            }
            if (f.TanggalMulai != null)
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TRUNC(TLP.CHECKTIME + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) >= TRUNC(TO_DATE(:param3,'DD/MM/YYYY'))");
                lstparams.Add(new OracleParameter("param3", ((DateTime)f.TanggalMulai).ToString("dd/MM/yyyy")));
            }
            if (f.TanggalSampai != null)
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TRUNC(TLP.CHECKTIME + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) <= TRUNC(TO_DATE(:param4,'DD/MM/YYYY'))");
                lstparams.Add(new OracleParameter("param4", ((DateTime)f.TanggalSampai).ToString("dd/MM/yyyy")));
            }
            param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "NVL(TLP.STATUSHAPUS,'0') = '0'");

            query = string.Concat(@"
            SELECT
              ROW_NUMBER() OVER (ORDER BY RST.PERIOD) AS RNUMBER,
              COUNT(1) OVER() AS TOTAL,
	            RST.PEGAWAIID,
	            NVL(PG.NAMA,PP.NAMA) AS NAMAPEGAWAI,
	            RST.KANTORID,
	            CASE UK.TIPEKANTORID WHEN 1 THEN 'Kantor Pusat' ELSE UK.NAMAUNITKERJA END AS NAMAKANTOR,
	            RST.UNITKERJAID,
	            UK.NAMAUNITKERJA,
	            RST.PERIOD,
	            CASE WHEN CAST(TO_CHAR(RST.MASUK,'HH24') AS INTEGER) < 12 THEN TO_CHAR(RST.MASUK,'HH24:MI:SS') ELSE '-' END AS MASUK,
	            CASE WHEN CAST(TO_CHAR(RST.KELUAR,'HH24') AS INTEGER) >= 12 THEN TO_CHAR(RST.KELUAR,'HH24:MI:SS') ELSE '-' END AS KELUAR
            FROM (", query, param, @"
                GROUP BY
	                 TLP.NIP,
	                 JP.KANTORID,
	                 JB.UNITKERJAID,
	                 TRUNC(TLP.CHECKTIME + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END)) RST
            INNER JOIN UNITKERJA UK ON
	            UK.UNITKERJAID = RST.UNITKERJAID AND
                UK.TAMPIL = 1
            LEFT JOIN PEGAWAI PG ON
	            PG.PEGAWAIID = RST.PEGAWAIID AND
                (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
            LEFT JOIN PPNPN PP ON
	            PP.NIK = RST.PEGAWAIID");
            if (!string.IsNullOrEmpty(f.TipePegawai))
            {
                if (f.TipePegawai.Equals("ASN"))
                {
                    query = query.Replace("LEFT JOIN PEGAWAI", "INNER JOIN PEGAWAI");
                }
                if (f.TipePegawai.Equals("PPNPN"))
                {
                    query = query.Replace("LEFT JOIN PPNPN", "INNER JOIN PPNPN");
                }
            }

            if (from + to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                lstparams.Add(new OracleParameter("pStart", from));
                lstparams.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<RekapPresensi>(query, lstparams.ToArray()).ToList();
            }

            return records;
        }

        public List<LokasiKantor> GetListLocationKantor(string kantorid, bool utama = false)
        {
            var result = new List<LokasiKantor>();
            var lstparams = new ArrayList();
            if (!string.IsNullOrEmpty(kantorid))
            {
                using (var ctx = new BpnDbContext())
                {
                    string sql = string.Empty;
                    if (utama)
                    {
                        sql = @"
                            SELECT 
                              KT.NAMA, CAST(KT.LATITUDE AS VARCHAR2(30)) AS LATITUDE, 
                              CAST(KT.LONGITUDE AS VARCHAR2(30)) AS LONGITUDE, 
                              SYSDATE + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END AS SERVERTIME
                            FROM KANTOR KT
                            WHERE
                              KT.KANTORID = :param1";
                        lstparams.Clear();
                        lstparams.Add(new OracleParameter("param1", kantorid));
                        result = ctx.Database.SqlQuery<LokasiKantor>(sql, lstparams.ToArray()).ToList();
                    }
                    else
                    {
                        var _tipeKantor = new DataMasterModel().GetTipeKantor(kantorid);
                        if (_tipeKantor.Equals(1))
                        {
                            sql = @"
                                SELECT
                                  NAMA, LATITUDE, LONGITUDE, SYSDATE + CASE ZONAWAKTU WHEN 'WITA' THEN 1/24 WHEN 'WIT' THEN 2/24 ELSE 0 END AS SERVERTIME
                                FROM LPPB.KANTORABSENPUSAT
                                WHERE
                                  KANTOREOFFICEID = :param1 AND
                                  NVL(STATUSHAPUS,'0') = '0'";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", kantorid));
                            result = ctx.Database.SqlQuery<LokasiKantor>(sql, lstparams.ToArray()).ToList();
                        }
                        else if (_tipeKantor.Equals(2))
                        {
                            sql = @"
                                SELECT 
                                  KT.NAMA, CAST(KT.LATITUDE AS VARCHAR2(30)) AS LATITUDE, 
                                  CAST(KT.LONGITUDE AS VARCHAR2(30)) AS LONGITUDE, 
                                  SYSDATE + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END AS SERVERTIME
                                FROM KANTOR KT
                                WHERE
                                  KT.KANTORID = :param1";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", kantorid));
                            result = ctx.Database.SqlQuery<LokasiKantor>(sql, lstparams.ToArray()).ToList();
                            sql = @"
                                SELECT 
                                  KT.NAMA, CAST(KT.LATITUDE AS VARCHAR2(30)) AS LATITUDE, 
                                  CAST(KT.LONGITUDE AS VARCHAR2(30)) AS LONGITUDE, 
                                  SYSDATE + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END AS SERVERTIME
                                FROM KANTOR KT
                                WHERE
                                  KT.INDUK = :param1";
                            result.AddRange(ctx.Database.SqlQuery<LokasiKantor>(sql, lstparams.ToArray()).ToList());
                        }
                        else
                        {
                            sql = @"
                                SELECT
                                  KT.INDUK AS KANTORID,
                                  KI.NAMA,
                                  CAST(KI.LATITUDE AS VARCHAR2(30)) AS LATITUDE,
                                  CAST(KI.LONGITUDE AS VARCHAR2(30)) AS LONGITUDE,
                                  SYSDATE + CASE KI.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END AS SERVERTIME
                                FROM KANTOR KT
                                INNER JOIN KANTOR KI ON
	                                KI.KANTORID = KT.INDUK
                                WHERE
                                  KT.KANTORID = :param1";
                            lstparams.Clear();
                            lstparams.Add(new OracleParameter("param1", kantorid));
                            result = ctx.Database.SqlQuery<LokasiKantor>(sql, lstparams.ToArray()).ToList();
                            if (result.Count > 0)
                            {
                                sql = @"
                                    SELECT 
                                      KT.NAMA, CAST(KT.LATITUDE AS VARCHAR2(30)) AS LATITUDE, 
                                      CAST(KT.LONGITUDE AS VARCHAR2(30)) AS LONGITUDE, 
                                      SYSDATE + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END AS SERVERTIME
                                    FROM KANTOR KT
                                    WHERE
                                      KT.KANTORID = :param1";
                                result.AddRange(ctx.Database.SqlQuery<LokasiKantor>(sql, lstparams.ToArray()).ToList());
                            }
                        }
                    }
                }
            }
            return result;
        }

        public GeoLocation getLocationPresensi(string pegawaiid, string kantorid, string presensitime)
        {
            var result = new GeoLocation();
            var lstparams = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;
            using (var ctx = new BpnDbContext())
            {
                string sql = string.Format(@"
                    SELECT
                      TLP.LATITUDE, TLP.LONGITUDE, TLP.CATATAN
                    FROM {0}.TBLLOGPRESENSI TLP
                    INNER JOIN KANTOR KT ON
	                    KT.KANTORID = TLP.KANTORID
                    WHERE
                      TLP.PEGAWAIID = :param1 AND
                      TLP.KANTORID = :param2 AND
                      TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END = TO_DATE(:param3,'DD/MM/YYYY hh24:mi:ss') AND
                      NVL(TLP.STATUSHAPUS,'0') = '0'", skema);
                lstparams.Clear();
                lstparams.Add(new OracleParameter("param1", pegawaiid));
                lstparams.Add(new OracleParameter("param2", kantorid));
                lstparams.Add(new OracleParameter("param3", presensitime));
                result = ctx.Database.SqlQuery<GeoLocation>(sql, lstparams.ToArray()).FirstOrDefault();
            }
            return result;
        }

        public TransactionResult doUbahPresensi(string pegawaiid, string kantorid, string unitkerjaid, string presensitime, string catatan)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new ArrayList();
                        lstparams.Add(new OracleParameter("param0", catatan));
                        lstparams.Add(new OracleParameter("param1", pegawaiid));
                        lstparams.Add(new OracleParameter("param2", kantorid));
                        lstparams.Add(new OracleParameter("param3", unitkerjaid));
                        lstparams.Add(new OracleParameter("param4", presensitime));
                        string skema = OtorisasiUser.NamaSkema;

                        string sql = string.Format(@"
                            MERGE INTO {0}.TBLLOGPRESENSI TLP
                            USING KANTOR KT
                            ON (KT.KANTORID = TLP.KANTORID)
                            WHEN MATCHED THEN UPDATE SET TLP.CATATAN = :param0
                            WHERE
                               TLP.PEGAWAIID = :param1 AND
                               TLP.KANTORID = :param2 AND
                               TLP.UNITKERJAID = :param3 AND
                               TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END = TO_DATE(:param4,'DD/MM/YYYY hh24:mi:ss') AND
                               NVL(TLP.STATUSHAPUS,'0') = '0'", skema);
                        ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Perubahan Presensi Berhasil";
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                        tc.Rollback();
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

        public GeoLocation getLastPresensi(string pegawaiid, string kantorid)
        {
            var result = new GeoLocation();
            var lstparams = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;
            using (var ctx = new BpnDbContext())
            {
                string sql = "SELECT SYSDATE FROM DUAL";
                var _serverTime = Convert.ToInt32(ctx.Database.SqlQuery<DateTime>(sql).FirstOrDefault().ToString("HHmmss"));
                var _quot = _serverTime < 120000 ? "<" : ">=";
                var _quot2 = _serverTime < 120000 ? "ASC" : "DESC";
                sql = string.Format(@"
                    SELECT TO_CHAR(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END,'hh24:mi:ss') AS CATATAN, TLP.LATITUDE, TLP.LONGITUDE
                    FROM {0}.TBLLOGPRESENSI TLP
                    INNER JOIN KANTOR KT ON
	                    KT.KANTORID = TLP.KANTORID
                    WHERE
                      TLP.PEGAWAIID = :param1 AND
                      TLP.KANTORID = :param2 AND
                      NVL(TLP.STATUSHAPUS,'0') = '0' AND
                      TRUNC(TLP.PRESENSITIMESTAMP) = TRUNC(SYSDATE) AND
                      CAST(TO_CHAR(TLP.PRESENSITIMESTAMP + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END,'HH24') AS INTEGER) {1} 12
                    ORDER BY TLP.PRESENSITIMESTAMP {2}", skema, _quot, _quot2);
                lstparams.Clear();
                lstparams.Add(new OracleParameter("param1", pegawaiid));
                lstparams.Add(new OracleParameter("param2", kantorid));
                result = ctx.Database.SqlQuery<GeoLocation>(sql, lstparams.ToArray()).FirstOrDefault();
                if(result != null)
                {
                    result.Catatan = string.Concat(_serverTime < 120000 ? "Presensi Masuk pukul " : "Presensi Keluar pukul ", result.Catatan);
                }
            }
            return result;
        }

        public List<DataBukuTamu> GetRekapBukuTamu(CariBukuTamu f, int from, int to)
        {
            var records = new List<DataBukuTamu>();
            string skema = OtorisasiUser.NamaSkema;
            var lstparams = new List<object>();
            string query = string.Empty;
            query = string.Format(@"
	            SELECT
                  TLB.BUKUTAMUID AS TAMUID,
                  TLB.KANTORID,
                  TLB.UNITKERJAID,
                  TLB.NIK,
                  TLB.NAMALENGKAP,
                  TLB.TEMPATLAHIR,
                  TLB.TANGGALLAHIR,
                  TLB.NOTELP,
                  TLB.EMAIL,
                  TLB.ALAMAT,
                  TLB.KEPERLUAN,
                  TLB.INSTANSI,
                  TLB.RESPONSTATUS,
                  TLB.RESPONCATATAN,
                  TLB.RESPONUSERID,
                  TLB.STATUSDUKCAPIL,
                  TLB.TANGGALBERKUNJUNG + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END AS TANGGALBERKUNJUNG
	            FROM {0}.TBLLOGBUKUTAMU TLB
	            INNER JOIN KANTOR KT ON
		            KT.KANTORID = TLB.KANTORID", skema);
            string param = string.Empty;
            if (!string.IsNullOrEmpty(f.UnitKerjaId))
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TLB.UNITKERJAID = :param1");
                lstparams.Add(new OracleParameter("param1", f.UnitKerjaId));
            }
            if (f.TanggalMulai != null)
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TRUNC(TLB.TANGGALBERKUNJUNG + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) >= TRUNC(TO_DATE(:param3,'DD/MM/YYYY'))");
                lstparams.Add(new OracleParameter("param3", ((DateTime)f.TanggalMulai).ToString("dd/MM/yyyy")));
            }
            if (f.TanggalSampai != null)
            {
                param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "TRUNC(TLB.TANGGALBERKUNJUNG + CASE KT.BAGIANWILAYAH WHEN 'Tengah' THEN 1/24 WHEN 'Timur' THEN 2/24 ELSE 0 END) <= TRUNC(TO_DATE(:param4,'DD/MM/YYYY'))");
                lstparams.Add(new OracleParameter("param4", ((DateTime)f.TanggalSampai).ToString("dd/MM/yyyy")));
            }
            param = string.Concat(param, string.IsNullOrEmpty(param) ? " WHERE " : " AND ", "NVL(TLB.STATUSHAPUS,'0') = '0'");

            query = string.Concat(@"
            SELECT
                ROW_NUMBER() OVER (ORDER BY RST.TANGGALBERKUNJUNG) AS RNUMBER,
                COUNT(1) OVER() AS TOTAL,
	            RST.TAMUID,
	            RST.KANTORID,
	            CASE UK.TIPEKANTORID WHEN 1 THEN 'Kantor Pusat' ELSE UK.NAMAUNITKERJA END AS NAMAKANTOR,
	            RST.UNITKERJAID,
	            UK.NAMAUNITKERJA,
	            RST.TANGGALBERKUNJUNG,
	            RST.NIK,
	            RST.NAMALENGKAP,
	            RST.TEMPATLAHIR,
	            RST.TANGGALLAHIR,
	            RST.NOTELP,
	            RST.EMAIL,
	            RST.ALAMAT,
	            RST.INSTANSI,
	            RST.KEPERLUAN,
	            RST.RESPONSTATUS,
	            RST.RESPONCATATAN,
	            RST.RESPONUSERID,
	            RST.STATUSDUKCAPIL
            FROM (", query, param, @") RST
            INNER JOIN UNITKERJA UK ON
	            UK.UNITKERJAID = RST.UNITKERJAID AND
                UK.TAMPIL = 1");

            if (from + to > 0)
            {
                query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pStart AND :pEnd");
                lstparams.Add(new OracleParameter("pStart", from));
                lstparams.Add(new OracleParameter("pEnd", to));
            }

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<DataBukuTamu>(query, lstparams.ToArray()).ToList();
            }

            return records;
        }

        public TransactionResult doUbahBukuTamu(DataBukuTamu data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var lstparams = new ArrayList();
                        lstparams.Add(new OracleParameter("param1", data.NIK));
                        lstparams.Add(new OracleParameter("param2", data.NamaLengkap));
                        lstparams.Add(new OracleParameter("param3", data.TempatLahir));
                        lstparams.Add(new OracleParameter("param4", data.TanggalLahir));
                        lstparams.Add(new OracleParameter("param5", data.NoTelp));
                        lstparams.Add(new OracleParameter("param6", data.Email));
                        lstparams.Add(new OracleParameter("param7", data.Alamat));
                        lstparams.Add(new OracleParameter("param8", data.Instansi));
                        lstparams.Add(new OracleParameter("param9", data.Keperluan));
                        lstparams.Add(new OracleParameter("param10", data.StatusDukcapil));
                        lstparams.Add(new OracleParameter("param11", data.ResponCatatan));
                        lstparams.Add(new OracleParameter("param12", data.ResponStatus));
                        lstparams.Add(new OracleParameter("param13", data.TamuId));
                        string skema = OtorisasiUser.NamaSkema;

                        string sql = string.Format(@"
                            UPDATE {0}.TBLLOGBUKUTAMU SET 
                                NIK = :param1,  
                                NAMALENGKAP = :param2,  
                                TEMPATLAHIR = :param3,  
                                TANGGALLAHIR = :param4,  
                                NOTELP = :param5,  
                                EMAIL = :param6,  
                                ALAMAT = :param7,  
                                INSTANSI = :param8,  
                                KEPERLUAN = :param9, 
                                STATUSDUKCAPIL = :param10,  
                                RESPONCATATAN = :param11,  
                                RESPONSTATUS = :param12  
                            WHERE BUKUTAMUID = :param13", skema);
                        ctx.Database.ExecuteSqlCommand(sql, lstparams.ToArray());

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Perubahan Buku Tamu Berhasil";
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                        tc.Rollback();
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