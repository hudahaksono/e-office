using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Surat.Models
{
    public class SuratModel
    {
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
        Functions functions = new Functions();
        DataMasterModel mDataMaster = new DataMasterModel();

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

            return retval;
        }

        public int GetServerYear()
        {
            int retval = DateTime.Now.Year;

            using (var ctx = new BpnDbContext())
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

            return retval;
        }


        #region Surat Pengantar

        public List<PengantarSurat> GetSuratPengantar(string tipe, string sortby, CariSuratPengantar f, int from, int to)
        {
            var records = new List<PengantarSurat>();

            var aParams = new ArrayList();

            string query =
                "SELECT * FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY pengantarsurat.tanggaldari, pengantarsurat.tanggalsampai, pengantarsurat.nomor, pengantarsurat.pengantarsuratid) RNUMBER, COUNT(1) OVER() TOTAL, " +
                "        pengantarsurat.pengantarsuratid, pengantarsurat.kantorid, pengantarsurat.nomor, pengantarsurat.tujuan, pengantarsurat.namapenerima, " +
                "        to_char(pengantarsurat.tanggaldari, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') TanggalDari, " +
                "        to_char(pengantarsurat.tanggalsampai, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') TanggalSampai, " +
                "        to_char(pengantarsurat.tanggalterima, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') TanggalTerima, " +
                "        to_char(pengantarsurat.tanggaldari, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') || ' s.d ' || " +
                "                to_char(pengantarsurat.tanggalsampai, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') Waktu, " +
                "        to_char(pengantarsurat.tanggalterima, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') InfoTanggalTerima, " +
                "        '0' AS STCHECK, '0' AS STTTE, 'H' AS STATUS " +
                "    FROM " +
                "        " + OtorisasiUser.NamaSkema + ".pengantarsurat " +
                "    WHERE " +
                "        pengantarsurat.pengantarsuratid IS NOT NULL ";

            if (!String.IsNullOrEmpty(f.UnitKerjaId))
            {
                aParams.Add(new OracleParameter("SatkerId", f.UnitKerjaId));
                query += " AND (pengantarsurat.kantorid = :SatkerId) ";
            }
            if (!String.IsNullOrEmpty(f.MetaData))
            {
                aParams.Add(new OracleParameter("Metadata", String.Concat("%", f.MetaData.ToLower(), "%")));
                aParams.Add(new OracleParameter("Metadata2", String.Concat("%", f.MetaData.ToLower(), "%")));
                query +=
                    " AND (LOWER(utl_raw.cast_to_varchar2(pengantarsurat.metadata)) LIKE :Metadata " +
                    "      OR exists (SELECT 1 FROM " + OtorisasiUser.NamaSkema + ".detilpengantar WHERE detilpengantar.pengantarsuratid = pengantarsurat.pengantarsuratid " +
                    "                 AND LOWER(utl_raw.cast_to_varchar2(detilpengantar.metadata)) LIKE :Metadata2))";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            aParams.Add(new OracleParameter("startCnt", from));
            aParams.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = aParams.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<PengantarSurat>(query, aParams.ToArray()).ToList<PengantarSurat>();
            }

            return records;
        }


        public List<Entities.Surat> GetSuratForSP(string unitpenerima, string profilepenerima, string kantorid, DateTime cariMulai, DateTime cariSampai, int from, int to)
        {
            var records = new List<Entities.Surat>();

            var aParams = new ArrayList();
            string nb = string.Empty;
            if (!string.IsNullOrEmpty(unitpenerima))
            {
                aParams.Add(new OracleParameter("UnitPenerima", unitpenerima));
                nb += " AND JB.UNITKERJAID = :UnitPenerima";
            }
            if (!string.IsNullOrEmpty(profilepenerima))
            {
                aParams.Add(new OracleParameter("ProfilePenerima", profilepenerima));
                nb += " AND JB.PROFILEID = :ProfilePenerima";
            }

            string query = string.Format(@"
            SELECT *
            FROM 
              (SELECT 
                 COUNT(1) OVER() AS TOTAL, 
                 ROW_NUMBER() OVER (ORDER BY TANGGALINPUT, TANGGALSURAT DESC, NOMORSURAT) AS RNUMBER, 
                 SURATINBOXID, TO_CHAR(TANGGALINPUT, 'dd/mm/yyyy HH24:MI') TANGGALINPUT, 
                 PENGIRIMSURAT, PENERIMASURAT, PERIHAL, NOMORSURAT, 
                 TO_CHAR(TANGGALSURAT, 'dd/mm/yyyy') AS TANGGALSURAT, SIFATSURAT, REDAKSI, 
                 KETERANGANSURAT
               FROM 
                 (SELECT DISTINCT 
                    SI.SURATINBOXID, SU.TANGGALINPUT, SU.PENGIRIM AS PENGIRIMSURAT, 
                    SU.PENERIMA AS PENERIMASURAT, SU.PERIHAL, SU.NOMORSURAT, 
                    SU.TANGGALSURAT, SU.SIFATSURAT, 
                    CASE SI.REDAKSI WHEN 'Penanggung Jawab' THEN 'Asli' ELSE SI.REDAKSI END AS REDAKSI, 
                    SU.KETERANGANSURAT
                  FROM {0}.SURAT SU
                    INNER JOIN {0}.SURATINBOX SI ON
                      SI.SURATID = SU.SURATID AND
                      SI.KANTORID = '{1}' AND
                      LOWER(SI.REDAKSI) IN ('asli','tembusan')  
                    INNER JOIN JABATAN JB ON
                      JB.PROFILEID = SI.PROFILEPENERIMA {4}
                  WHERE
                    SU.suratid IS NOT NULL AND
                    SU.kategori = 'Masuk' AND
                    (TANGGALINPUT BETWEEN TO_DATE('{2}', 'DD/MM/YYYY hh24:mi:ss') AND TO_DATE('{3}', 'DD/MM/YYYY hh24:mi:ss')) ) RST)
            ", OtorisasiUser.NamaSkema, kantorid, cariMulai.ToString("dd/MM/yyyy HH.mm.ss"), cariSampai.ToString("dd/MM/yyyy HH.mm.ss"), nb);

            if (from + to > 0)
            {
                query +=
                    " WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

                aParams.Add(new OracleParameter("startCnt", from));
                aParams.Add(new OracleParameter("limitCnt", to));
            }

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = aParams.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.Surat>(query, aParams.ToArray()).ToList<Entities.Surat>();
            }

            return records;
        }


        #endregion

        public KopSurat getKopDetail(string id)
        {
            var result = new KopSurat();
            string skema = OtorisasiUser.NamaSkema;
            string query = string.Format(@"
                SELECT
                    UNITKERJAID,
                    NAMAUNITKERJA AS UNITKERJANAME,
                    NAMAKANTOR_L1,
                    NAMAKANTOR_L2,
                    ALAMAT,
                    TELEPON,
                    EMAIL,
                    FONTSIZE
                FROM {0}.TBLKANTOR 
                WHERE
	                TKS.UNITKERJAID = '{1}'", skema, id);

            using (var ctx = new PostgresDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<KopSurat>(query).First();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public TransactionResult SimpanKopSurat(KopSurat data, string userid)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PostgresDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Empty;
                        string skema = OtorisasiUser.NamaSkema;
                        if (ctx.Database.SqlQuery<int>(string.Format("SELECT COUNT(1) FROM {0}.TBLKANTOR WHERE UNITKERJAID = '{1}'", skema, data.UnitKerjaId)).FirstOrDefault() > 0)
                        {
                            sql = string.Format(@"UPDATE {0}.TBLKANTOR SET 
                                                        NAMAKANTOR_L1 = '{1}',
                                                        NAMAKANTOR_L2 = '{2}',
                                                        ALAMAT = '{3}',
                                                        TELEPON = '{4}',
                                                        EMAIL = '{5}'
                                                    WHERE UNITKERJAID = '{6}'", skema, data.NamaKantor_L1, data.NamaKantor_L2, data.Alamat, data.Telepon, data.Email, data.UnitKerjaId);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }
                        else
                        {
                            sql = string.Format(@"INSERT INTO {0}.TBLKANTOR (UNITKERJAID, NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL) 
                                                    VALUES ('{1}', '{2}', '{3}', '{4}', '{5}')", skema, data.UnitKerjaId, data.NamaKantor_L1, data.NamaKantor_L2, data.Alamat, data.Telepon, data.Email);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = data.UnitKerjaId;
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

        public Entities.TransactionResult BuatSuratPengantar(string unittujuan, string profiletujuan, DateTime tglMulai, DateTime tglSelesai, List<string> suratIds, string jabatantte, string pejabatcek, string pejabattte, string satkerid, string unitkerjaid, string userid, out string nomorsp)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            nomorsp = "";

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList aParams = new ArrayList();

                        string sql = "";
                        object[] parameters = null;
                        var dataMaster = new DataMasterModel();
                        string unitpenerima = dataMaster.GetNamaUnitKerjaById(unittujuan);
                        string namapenerima = dataMaster.GetProfileNameFromId(profiletujuan);

                        string periode = string.Concat(tglMulai.ToString("dd/MM/yyyy HH.mm"), " hingga ", tglSelesai.ToString("dd/MM/yyyy HH.mm"));
                        if (tglMulai == tglSelesai)
                        {
                            periode = tglMulai.ToString("dd/MM/yyyy HH.mm");
                        }

                        #region Nomor Surat Pengantar

                        string tipe = "Surat Pengantar";

                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        decimal nilainomorsp = 1;

                        aParams.Clear();
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("Tahun", tahun));
                        aParams.Add(new OracleParameter("Tipe", tipe));

                        string query = "select count(*) from " + OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";

                        parameters = aParams.OfType<object>().ToArray();
                        int jumlahrecord = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();

                        if (jumlahrecord > 0)
                        {
                            query =
                                "select nilaikonter+1 from " + OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                                "FOR UPDATE NOWAIT";
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                            aParams.Add(new OracleParameter("Tahun", tahun));
                            aParams.Add(new OracleParameter("Tipe", tipe));
                            parameters = aParams.OfType<object>().ToArray();

                            nilainomorsp = ctx.Database.SqlQuery<decimal>(query, aParams.ToArray()).FirstOrDefault();
                        }
                        else
                        {
                            // Bila tidak ada, Insert KONTERSURAT
                            query =
                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                "    kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                "    ( " +
                                "        SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                            //query = sWhitespace.Replace(query, " ");
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                            aParams.Add(new OracleParameter("TipeSurat", tipe));
                            aParams.Add(new OracleParameter("Tahun", tahun));
                            parameters = aParams.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());

                        }

                        sql = "UPDATE " + OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("NilaiKonter", nilainomorsp));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("Tahun", tahun));
                        aParams.Add(new OracleParameter("Tipe", tipe));
                        parameters = aParams.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                        // Get Bulan
                        int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                        string strBulan = Functions.NomorRomawi(bulan);
                        string kodeindentifikasi = new KonterModel().GetKodeIdentifikasi(unitkerjaid);
                        string kodesurat = "P-";

                        nomorsp = Convert.ToString(nilainomorsp) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());

                        #endregion


                        #region Set Metadata
                        string metadata = "";
                        metadata += nomorsp + " ";
                        metadata += namapenerima + " ";
                        metadata = metadata.Trim();
                        #endregion


                        string pengantarsuratid = GetUID();
                        /*
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".pengantarsurat ( " +
                            "       pengantarsuratid, kantorid, nomor, tanggaldari, tanggalsampai, tujuan, profileidtujuan, metadata) VALUES " +
                            "( " +
                            "       :PengantarSuratId, :SatkerId, :Nomor, TO_DATE(:TanggalMulai,'DD/MM/YYYY hh24:mi:ss'), TO_DATE(:TanggalSampai,'DD/MM/YYYY hh24:mi:ss'), :Tujuan, :ProfileIdTujuan, utl_raw.cast_to_raw(:Metadata))";
                        //sql = sWhitespace.Replace(sql, " ");
                        aParams.Clear();
                        aParams.Add(new OracleParameter("PengantarSuratId", pengantarsuratid));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("Nomor", nomorsp));
                        aParams.Add(new OracleParameter("TanggalMulai", tglMulai.ToString("dd/MM/yyyy HH.mm.ss")));
                        aParams.Add(new OracleParameter("TanggalSampai", tglSelesai.ToString("dd/MM/yyyy HH.mm.ss")));
                        aParams.Add(new OracleParameter("Tujuan", string.IsNullOrEmpty(namapenerima) ? unitpenerima : namapenerima));
                        aParams.Add(new OracleParameter("ProfileIdTujuan", profiletujuan));
                        aParams.Add(new OracleParameter("Metadata", metadata));
                        parameters = aParams.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                        */

                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".TBLTTEPENGANTARSURAT ( " +
                            "       PENGANTARSURATID, UNITKERJAID, UNITTUJUANNAMA, UNITTUJUANID, PROFILETUJUANNAMA, PROFILETUJUANID, NOMOR, PERIODEMULAI, PERIODESAMPAI, METADATA, JABATANTTE, PEJABATTTE, PEJABATCEK, STATUS, UPDTIME, UPDUSER) VALUES " +
                            "( " +
                            "       :PengantarSuratId, :UnitKerjaId, :UnitTujuanNama, :UnitTujuanId, :ProfileTujuanNama, :ProfileTujuanId, :Nomor, TO_DATE(:PeriodeMulai, 'DD/MM/YYYY hh24:mi:ss'), TO_DATE(:PeriodeSelesai, 'DD/MM/YYYY hh24:mi:ss'), utl_raw.cast_to_raw(:Metadata), :JabatanTTE, :PejabatTTE, :PejabatCek, 'P', SYSDATE, :UserId )";
                        //sql = sWhitespace.Replace(sql, " ");
                        aParams.Clear();
                        aParams.Add(new OracleParameter("PengantarSuratId", pengantarsuratid));
                        aParams.Add(new OracleParameter("UnitKerjaId", unitkerjaid));
                        aParams.Add(new OracleParameter("UnitTujuanNama", unitpenerima));
                        aParams.Add(new OracleParameter("UnitTujuanId", unittujuan));
                        aParams.Add(new OracleParameter("ProfileTujuanNama", namapenerima));
                        aParams.Add(new OracleParameter("ProfileTujuanId", profiletujuan));
                        aParams.Add(new OracleParameter("Nomor", nomorsp));
                        aParams.Add(new OracleParameter("PeriodeMulai", tglMulai.ToString("dd/MM/yyyy HH.mm.ss")));
                        aParams.Add(new OracleParameter("PeriodeSelesai", tglSelesai.ToString("dd/MM/yyyy HH.mm.ss")));
                        aParams.Add(new OracleParameter("Metadata", metadata));
                        aParams.Add(new OracleParameter("JabatanTTE", jabatantte));
                        aParams.Add(new OracleParameter("PejabatTTE", pejabattte));
                        aParams.Add(new OracleParameter("PejabatCek", pejabatcek));
                        aParams.Add(new OracleParameter("UserId", userid));
                        parameters = aParams.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        var listSurat = new List<Entities.Surat>();
                        if (suratIds != null && suratIds.Count > 0)
                        {

                        }
                        else
                        {
                            listSurat = GetSuratForSP(unittujuan, profiletujuan, unitkerjaid, tglMulai, tglSelesai, 0, 0);
                        }

                        foreach (var dataSurat in listSurat)
                        {
                            #region Set Metadata
                            metadata = "";
                            metadata += dataSurat.NomorSurat + " ";
                            metadata += dataSurat.NomorAgenda + " ";
                            metadata += dataSurat.Perihal + " ";
                            metadata += dataSurat.PengirimSurat + " ";
                            metadata += dataSurat.SifatSurat + " ";
                            metadata += dataSurat.KeteranganSurat + " ";
                            metadata += dataSurat.Redaksi + " ";
                            metadata = metadata.Trim();
                            #endregion

                            //sql =
                            //    "INSERT INTO " + OtorisasiUser.NamaSkema + ".detilpengantar ( " +
                            //    "       detilpengantarid, pengantarsuratid, suratid, nomorsurat, nomoragenda, " +
                            //    "       tanggalsurat, pengirim, perihal, sifatsurat, keterangansurat, redaksi, metadata) VALUES " +
                            //    "( " +
                            //    "       SYS_GUID(), :PengantarSuratId, :SuratId, :NomorSurat, :NomorAgenda, " +
                            //    "       TO_DATE(:TanggalSurat,'DD/MM/YYYY'), :Pengirim, :Perihal, :SifatSurat, :KeteranganSurat, :Redaksi, utl_raw.cast_to_raw(:Metadata))";
                            sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".TBLTTEPENGANTARSURAT_ISI ( " +
                            "       ISIPENGANTARSURATID, PENGANTARSURATID, SURATINBOXID, NOMORSURAT, TANGGALSURAT, PENGIRIMSURAT, PENERIMASURAT, PERIHAL, SIFATSURAT, REDAKSI, KETERANGANSURAT, METADATA, STATUS, UPDTIME, UPDUSER) VALUES " +
                            "( " +
                            "       SYS_GUID(), :PengantarSuratId, :SuratInboxId, :NomorSurat,  TO_DATE(:TanggalSurat,'DD/MM/YYYY'), :Pengirim, :Penerima, :Perihal, :SifatSurat, :Redaksi, :KeteranganSurat, utl_raw.cast_to_raw(:Metadata), 'A', SYSDATE, :UserId )";

                            //sql = sWhitespace.Replace(sql, " ");
                            aParams.Clear();
                            aParams.Add(new OracleParameter("PengantarSuratId", pengantarsuratid));
                            aParams.Add(new OracleParameter("SuratInboxId", dataSurat.SuratInboxId));
                            aParams.Add(new OracleParameter("NomorSurat", dataSurat.NomorSurat));
                            aParams.Add(new OracleParameter("TanggalSurat", dataSurat.TanggalSurat));
                            aParams.Add(new OracleParameter("Pengirim", dataSurat.PengirimSurat));
                            aParams.Add(new OracleParameter("Penerima", dataSurat.PenerimaSurat));
                            aParams.Add(new OracleParameter("Perihal", dataSurat.Perihal));
                            aParams.Add(new OracleParameter("SifatSurat", dataSurat.SifatSurat));
                            aParams.Add(new OracleParameter("KeteranganSurat", dataSurat.KeteranganSurat));
                            aParams.Add(new OracleParameter("Redaksi", dataSurat.Redaksi));
                            aParams.Add(new OracleParameter("Metadata", metadata));
                            aParams.Add(new OracleParameter("UserId", userid));
                            parameters = aParams.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                        }


                        tc.Commit();
                        //tc.Rollback(); // test
                        tr.Status = true;
                        tr.ReturnValue = pengantarsuratid;
                        tr.Pesan = "Surat Pengantar berhasil disimpan dengan nomor " + nomorsp;
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        //if (tr.Pesan.ToLower().Contains("unique constraint"))
                        //{
                        //    tr.Pesan = "Nomor Surat Pengantar tersebut sudah ada.";
                        //}
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

        internal string apiUrl(DateTime tglSunting)
        {
            throw new NotImplementedException();
        }

        #region Surat Pengantar

        public List<ListDraft> GetListDraft(string tipe, string userid, string sortby, CariDraftSurat f, int from, int to)
        {
            var records = new List<ListDraft>();

            var aParams = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;

            string query = string.Format(@"
            SELECT
	            RST.RNUMBER,
	            RST.TOTAL,
	            RST.DRAFTCODE,
	            RST.UNITKERJAID,
	            RST.PERIHAL,
	            RST.SIFATSURAT,
	            RST.TIPESURAT,
	            TO_CHAR(RST.TANGGALUBAH, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALUBAH,
	            NVL(PNS2.NAMA,PPNPN2.NAMA) AS NAMAUBAH,
	            TO_CHAR(RST.TANGGALBUAT, 'Day, fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALBUAT,
	            NVL(PNS1.NAMA,PPNPN1.NAMA) AS NAMABUAT,
	            RST.STATUS
            FROM (
                SELECT *
                FROM 
                  (SELECT 
                     ROW_NUMBER() OVER (ORDER BY TDS.UPDTIME) RNUMBER, COUNT(1) OVER() TOTAL, 
                     TDS.DRAFTCODE, TDS.UNITKERJAID, TDS.PERIHAL, TDS.SIFATSURAT, TDS.TIPESURAT, 
                     TDS.UPDTIME AS TANGGALUBAH, TDS.UPDUSER AS USERUBAH, 
                     TLD.LOGTIME AS TANGGALBUAT, TLD.USERID AS USERBUAT, TDS.STATUS
                   FROM {0}.TBLDRAFTSURAT TDS
                     INNER JOIN {0}.TBLLOGDRAFT TLD ON
                       TLD.DRAFTCODE = TDS.DRAFTCODE AND
                       TLD.LOGTEXT = 'New' 
                   WHERE
                     TDS.UNITKERJAID = :UnitKerjaId ", skema);
            aParams.Add(new OracleParameter("UnitKerjaId", f.UnitKerjaId));
            if (tipe.ToLower().Equals("pembuat"))
            {
                aParams.Add(new OracleParameter("UserId", userid));
                if (OtorisasiUser.isTU())
                {
                    query += " AND ((TLD.USERID = :UserId AND TDS.STATUS = 'P' ) OR TDS.STATUS = 'W') ";
                }
                else
                {
                    query += " AND (TLD.USERID = :UserId) AND TDS.STATUS IN ('P','W') ";
                }
            }

            if (!string.IsNullOrEmpty(f.MetaData))
            {
                aParams.Add(new OracleParameter("Metadata", String.Concat("%", f.MetaData.ToLower(), "%")));
                query +=
                    " AND (LOWER(TDS.PERIHAL || TDS.DRAFTCODE || TDS.SIFATSURAT || TDS.TIPESURAT) LIKE :Metadata ";
            }

            query +=
                @" ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt) RST
                LEFT JOIN PEGAWAI PNS1 ON

                    PNS1.USERID = RST.USERBUAT
                LEFT JOIN PPNPN PPNPN1 ON

                    PPNPN1.USERID = RST.USERBUAT
                LEFT JOIN PEGAWAI PNS2 ON

                    PNS2.USERID = RST.USERUBAH
                LEFT JOIN PPNPN PPNPN2 ON

                    PPNPN2.USERID = RST.USERUBAH ";

            aParams.Add(new OracleParameter("startCnt", from));
            aParams.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = aParams.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<ListDraft>(query, aParams.ToArray()).ToList();
            }

            return records;
        }

        #endregion

        public List<Profile> GetProfilesByUnitKerja(string unitkerjaid)
        {
            var records = new List<Profile>();

            var aParams = new ArrayList();

            string query = @"
                SELECT
	                JB.PROFILEID,
	                JB.NAMA AS NAMAPROFILE, JB.*
                FROM JABATAN JB
                WHERE
                  JB.UNITKERJAID = :UnitKerjaId AND
	                NVL(JB.SEKSIID,'XX') != 'A800' AND
	                (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE)
                ORDER BY NVL(TIPEESELONID,99)";

            aParams.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = aParams.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Profile>(query, aParams.ToArray()).ToList();
            }

            return records;
        }



        public DraftSurat GetDraftSurat(string draftcode, string unitkerjaid)
        {
            var result = new DraftSurat();
            string skema = OtorisasiUser.NamaSkema;
            //(TO_CHAR(TDE.TANGGALSURAT, 'dd', 'nls_date_language=INDONESIAN') || ' ' || TRIM(TO_CHAR(TDE.TANGGALSURAT, 'Month', 'nls_date_language=INDONESIAN')) || ' ' || TO_CHAR(TDE.TANGGALSURAT, 'yyyy', 'nls_date_language=INDONESIAN')) AS TANGGALSURAT,
            string query = string.Format(@"
                SELECT
                    TDS.DRAFTCODE,
                    TDS.UNITKERJAID,
	                TDS.PERIHAL,
	                TDS.KOPSURAT,
	                TDS.KODEARSIP,
	                TDS.SIFATSURAT,
	                TDS.TIPESURAT,
	                TDS.HALAMANTTE AS POSISITTE,
	                TDS.ISISURAT,
	                TDS.STATUS,
	                TDS.PROFILEPENGIRIM
                FROM {0}.TBLDRAFTSURAT TDS
                WHERE
	                TDS.DRAFTCODE = '{1}' AND TDS.STATUS <> 'D'", skema, draftcode);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<DraftSurat>(query).First();
                    if (!string.IsNullOrEmpty(result.DraftCode))
                    {

                        result.UserPembuat = ctx.Database.SqlQuery<string>(string.Format("SELECT USERID FROM {0}.TBLLOGDRAFT WHERE DRAFTCODE = '{1}' AND LOGTEXT = 'New' ORDER BY LOGTIME", skema, result.DraftCode)).FirstOrDefault();
                        result.TanggalDibuat = ctx.Database.SqlQuery<string>(string.Format("SELECT to_char(LOGTIME,'DD/MM/YYYY HH24:MI') FROM {0}.TBLLOGDRAFT WHERE DRAFTCODE = '{1}' AND LOGTEXT = 'New' ORDER BY LOGTIME", skema, result.DraftCode)).FirstOrDefault();


                        //get from table detail
                        var details = ctx.Database.SqlQuery<DraftSuratDetail>($"SELECT DETAILTEXT AS TEXT , DETAILVALUE AS VALUE FROM {skema}.TBLDRAFTSURATDETAIL WHERE DRAFTCODE='{result.DraftCode}'").ToList();
                        foreach (var detail in details)
                        {
                            if (result.TipeSurat.Equals("Surat Undangan"))
                            {
                                if (detail.Text.Equals("TanggalUndangan"))
                                {
                                    result.TanggalUndangan = detail.Value;
                                }
                                if (detail.Text.Equals("TempatUndangan"))
                                {
                                    result.TempatUndangan = detail.Value;
                                }
                            }
                            if (result.TipeSurat.Equals("Surat Keterangan"))
                            {
                                if (detail.Text.Equals("YangTandaTangan"))
                                {
                                    result.YangTandaTangan = detail.Value;
                                }
                                if (detail.Text.Equals("MenerangkanBahwa"))
                                {
                                    result.MenerangkanBahwa = detail.Value;
                                }
                                if (detail.Text.Equals("AlamatKeterangan"))
                                {
                                    result.AlamatKeterangan = detail.Value;
                                }
                                if (detail.Text.Equals("NamaKeterangan"))
                                {
                                    result.NamaKeterangan = detail.Value;
                                }
                                if (detail.Text.Equals("NoIndukKeterangan"))
                                {
                                    result.NoIndukKeterangan = detail.Value;
                                }
                                if (detail.Text.Equals("PangkatKeterangan"))
                                {
                                    result.PangkatKeterangan = detail.Value;
                                }
                                if (detail.Text.Equals("JabatanKeterangan"))
                                {
                                    result.JabatanKeterangan = detail.Value;
                                }
                            }

                            if (detail.Text.Equals("OptionTTD"))
                            {
                                result.TanpaGelar = !string.IsNullOrEmpty(detail.Value) ? true : false;
                            }
                            if (detail.Text.Equals("OptionAn"))
                            {
                                result.AtasNama = string.IsNullOrEmpty(detail.Value) ? null : detail.Value;
                            }
                            if (detail.Text.Equals("OptionTujuan"))
                            {
                                result.TujuanTerlampir = !string.IsNullOrEmpty(detail.Value) ? true : false;
                            }


                            if (detail.Text.Equals("NomorSurat"))
                            {
                                result.NomorSurat = string.IsNullOrEmpty(detail.Value) ? "" : detail.Value;
                            }
                            if (detail.Text.Equals("TanggalSurat"))
                            {
                                result.TanggalSurat = string.IsNullOrEmpty(detail.Value) ? "" : detail.Value;
                            }
                        }


                        result.TTE = ctx.Database.SqlQuery<UserTTE>($@"
                            SELECT TPS.DRAFTCODE, TPS.USERID AS PENANDATANGANID, TPS.PEGAWAIID, TPS.NAMA, TPS.PROFILEID, TPS.JABATAN, TPS.URUT, TPS.TIPE, TPS.STATUS 
                            FROM {skema}.TBLPENANDATANGANDRAFTSURAT TPS 
                            WHERE TPS.DRAFTCODE = '{result.DraftCode}' AND TPS.STATUS != 'D' ORDER BY URUT
                            ").ToList();
                        //result.TTE = new TandaTanganElektronikModel().getPenandatanganDraft(result.DraftCode);

                        query = string.Format(@"
                            SELECT
                                TDS.NAMA||(CASE WHEN TDS.PROFILID IS NULL THEN '' ELSE CONCAT('%', TDS.PROFILID) END) AS NAMA
                            FROM {0}.TBLDRAFTSURATTUJUAN TDS
                            WHERE
	                            TDS.DRAFTCODE = '{1}' AND TEMBUSAN = '0' ORDER BY URUTAN", skema, draftcode);
                        result.Tujuan = ctx.Database.SqlQuery<string>(query).ToList();

                        //ganti replace 0 -- 1 bisa eror apabila draftcode ada 0 nya
                        //result.Tembusan = ctx.Database.SqlQuery<string>(query.Replace("0", "1")).ToList();

                        query = string.Format(@"
                            SELECT
                                TDS.NAMA||(CASE WHEN TDS.PROFILID IS NULL THEN '' ELSE CONCAT('%', TDS.PROFILID) END) AS NAMA
                            FROM {0}.TBLDRAFTSURATTUJUAN TDS
                            WHERE
	                            TDS.DRAFTCODE = '{1}' AND TEMBUSAN = '1' ORDER BY URUTAN", skema, draftcode);
                        result.Tembusan = ctx.Database.SqlQuery<string>(query).ToList();

                        result.LampiranId = ctx.Database.SqlQuery<string>($"SELECT LAMPIRANID FROM {skema}.TBLLAMPIRANDRAFTSURAT WHERE DRAFTCODE = '{result.DraftCode}' AND STATUS = 'A'").FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public TransactionResult HapusDraft(string id, string unitkerjaid, string userid, string alasan)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string skema = OtorisasiUser.NamaSkema;
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDRAFTSURAT SET UPDTIME = SYSDATE, UPDUSER = '{3}', STATUS = 'D' WHERE DRAFTCODE = '{1}' AND UNITKERJAID = '{2}' AND STATUS <> 'D'", skema, id, unitkerjaid, userid);
                        ctx.Database.ExecuteSqlCommand(sql);
                        sql = string.Format(@"
                                INSERT INTO {0}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT, LOGDETAIL)
                                VALUES (RAWTOHEX(SYS_GUID()),'{1}','{2}',SYSDATE,'Delete','{3}')
                                ", skema, id, userid, alasan);
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Pengajuan Konsep Berhasil DiHapus";
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

        public TransactionResult PengajuanDraft(string id, string unitkerjaid, string userid, string profileidtu)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string skema = OtorisasiUser.NamaSkema;


                        //sql = string.Format(@"
                        //    SELECT PEGAWAIID FROM JABATANPEGAWAI WHERE PROFILEID = '{0}' AND NVL(STATUSHAPUS,'0') = '0' AND (VALIDSAMPAI IS NULL OR CAST(VALIDSAMPAI AS TIMESTAMP) > SYSDATE)", profileidtu);
                        //var lstTU = ctx.Database.SqlQuery<string>(sql).ToList();
                        //foreach (var tu in lstTU)
                        //{
                        //    try
                        //    {
                        //        new Mobile().KirimNotifikasi(tu, "asn", "Eoffice", string.Concat("Konsep Surat - ", id), "Pengajuan Konsep");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        var msg = ex.Message;
                        //        break;
                        //    }
                        //}

                        string checknull = ctx.Database.SqlQuery<string>($@" SELECT USERID FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE = '{id}' AND ROWNUM = 1 ").FirstOrDefault();

                        if (string.IsNullOrEmpty(checknull))
                        {
                            var penandatnaganId = ctx.Database.SqlQuery<string>($@"SELECT USERID FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{id}' AND TIPE = 1 ").FirstOrDefault();
                            string sql;
                            if (penandatnaganId == userid)
                            {
                                sql = $@"UPDATE {skema}.TBLDRAFTSURAT SET STATUS = 'A' WHERE DRAFTCODE = '{id}'";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            else
                            {
                                sql = string.Format(@"UPDATE {0}.TBLDRAFTSURAT SET UPDTIME = SYSDATE, UPDUSER = '{3}', STATUS = 'W' WHERE DRAFTCODE = '{1}' AND UNITKERJAID = '{2}'", skema, id, unitkerjaid, userid);
                                ctx.Database.ExecuteSqlCommand(sql);
                                var korid = GetUID();
                                var max = ctx.Database.SqlQuery<decimal>($@"SELECT MAX(URUT) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{id}' GROUP BY DRAFTCODE ").FirstOrDefault();
                                var min = ctx.Database.SqlQuery<decimal>($@"SELECT MIN(URUT) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{id}' GROUP BY DRAFTCODE ").FirstOrDefault();

                                sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
                                 SELECT 
                                    '{korid}' AS KOR_ID, 
                                     TP.DRAFTCODE,
                                     TP.PROFILEID AS VERIFIKATOR,
                                     TP.USERID,
                                     {min} AS V_RANK,
                                     '{max}' AS V_MAX, 
                                     'W' AS STATUS,
                                      SYSDATE AS TANGGAL 
                                FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
                                WHERE TP.DRAFTCODE = '{id}' AND TP.URUT = '{min}'";
                                ctx.Database.ExecuteSqlCommand(sql);

                                sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL)
                                 VALUES (RAWTOHEX(SYS_GUID()), '{id}', '!Pengajuan Konsep Baru!', '{userid}', SYSDATE) ";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Pengajuan Konsep berhasil dikirim";
                        }
                        else
                        {
                            tr.Pesan = "Pengajuan Gagal";
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

        public TransactionResult HapusDokumen(string id, string userid, string username, string alasan)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string skema = OtorisasiUser.NamaSkema;
                        string sql = string.Format(@"
                            UPDATE {0}.TBLDOKUMENELEKTRONIK SET STATUSHAPUS = '1', TANGGALHAPUS = SYSDATE, USERHAPUS = '{2}', ALASANHAPUS = '{3}' WHERE DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0'", skema, id, userid, alasan);
                        ctx.Database.ExecuteSqlCommand(sql);

                        sql = string.Format("UPDATE KONTENAKTIF SET TANGGALAKHIRAKSES = SYSDATE, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = '{1}'  WHERE KONTENAKTIFID = '{0}'", id, username);
                        ctx.Database.ExecuteSqlCommand(sql);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Dokumen TTE berhasil dihapus";
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

        public DokumenTTE GetDokumenElektronik(string id)
        {
            var result = new DokumenTTE();
            string skema = OtorisasiUser.NamaSkema;
            //(TO_CHAR(TDE.TANGGALSURAT, 'dd', 'nls_date_language=INDONESIAN') || ' ' || TRIM(TO_CHAR(TDE.TANGGALSURAT, 'Month', 'nls_date_language=INDONESIAN')) || ' ' || TO_CHAR(TDE.TANGGALSURAT, 'yyyy', 'nls_date_language=INDONESIAN')) AS TANGGALSURAT,
            string query = string.Format(@"
                SELECT
                    TDE.DOKUMENELEKTRONIKID,
	                TDE.NOMORSURAT,
	                TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT,
	                TDE.PERIHAL,
	                TDE.SIFATSURAT,
	                TDE.NAMAFILE,
	                TDE.HALAMANTTE
                FROM {0}.TBLDOKUMENELEKTRONIK TDE
                WHERE
	                TDE.DOKUMENELEKTRONIKID = '{1}' AND NVL(TDE.STATUSHAPUS,'0') = '0'", skema, id);

            using (var ctx = new BpnDbContext())
            {
                try
                {
                    result = ctx.Database.SqlQuery<DokumenTTE>(query).First();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return result;
        }

        public string getPenomoranSurat(string id, string unit, DateTime dt)
        {
            string skema = OtorisasiUser.NamaSkema;
            string nomor = string.Empty;
            string query = string.Empty;
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    var data = GetDokumenElektronik(id);
                    var str = data.NamaFile.Split('|');
                    if (str.Length == 2)
                    {
                        string code = str[0];
                        string tipe = str[1];
                        string strBulan = Functions.NomorRomawi(dt.Month);
                        string strTahun = dt.Year.ToString();
                        var draft = GetDraftSurat(code, unit);
                        query = string.Format(@"SELECT NVL(MAX(TIPESURAT),0)+1 AS NOMOR FROM {0}.KONTERSURAT WHERE KANTORID = '{1}' AND TAHUN = '{2}' AND TIPESURAT = '{3}'", skema, unit, strTahun, tipe);
                        nomor = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                        string template = GetKodeTipeSurat(tipe);
                        template.Replace("<nomor>", nomor).Replace("<kode>", "-" + draft.KodeArsip).Replace("<bulan>", strBulan).Replace("<tahun>", strTahun);
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return nomor;
        }

        public string GetKodeTipeSurat(string tipe)
        {
            string skema = OtorisasiUser.NamaSkema;
            string txt = string.Empty;
            string query = string.Format(@"
                SELECT
                  FORMATNOMOR
                FROM {0}.TIPESURAT
                WHERE
                  NAMA = '{1}' AND
                  AKTIF = 1 AND
                  INDUK IS NOT NULL AND
                  FORMATNOMOR IS NOT NULL
                ORDER BY
                  INDUK, URUTAN", skema, tipe);

            using (var ctx = new BpnDbContext())
            {
                txt = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
            }

            return txt;
        }

        public string getNomorSurat(string id)
        {
            string skema = OtorisasiUser.NamaSkema;
            string nomor = string.Empty;
            string query = string.Format(@"
                SELECT NOMORSURAT
                FROM {0}.TBLDOKUMENELEKTRONIK
                WHERE
                  DOKUMENELEKTRONIKID = '{1}' AND NVL(STATUSHAPUS,'0') = '0'", skema, id);
            using (var ctx = new BpnDbContext())
            {
                try
                {
                    nomor = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return nomor;
        }

        public List<AsalSurat> GetTujuanSurat(string jenis)
        {
            var list = new List<Entities.AsalSurat>();
            string query = string.Empty;
            switch (jenis)
            {
                case "pusat":
                    query = @"
                        SELECT 
                          NAMAUNITKERJA AS VALUE, NAMAUNITKERJA AS DATA
                        FROM UNITKERJA
                        WHERE
                          TIPEKANTORID = 1 AND
                          TAMPIL = 1 AND
                          LENGTH(UNITKERJAID) <= 8 AND
                          KANTORID = '980FECFC746D8C80E0400B0A9214067D' 
                        ORDER BY CAST(UNITKERJAID AS NUMBER)";
                    break;
                case "kanwil":
                    query = @"
                        SELECT 
                          NAMA AS VALUE, NAMA AS DATA
                        FROM KANTOR
                        WHERE
                          TIPEKANTORID = 2 
                        ORDER BY KODE";
                    break;
                case "kantah":
                    query = @"
                        SELECT 
                          NAMA AS VALUE, NAMA AS DATA
                        FROM KANTOR
                        WHERE
                          TIPEKANTORID = 3 
                        ORDER BY KODE";
                    break;
                case "bank":
                    query = string.Format(@"
                        SELECT NAMAASALSURAT AS VALUE, NAMAASALSURAT AS DATA
                        FROM surat.ASALSURAT
                        WHERE
                          STATUS = 1 AND
                          LOWER(NAMAASALSURAT) LIKE '%bank%'
                        ORDER BY NAMAASALSURAT", OtorisasiUser.NamaSkemaMaster);
                    break;
                case "kl":
                    query = string.Format(@"
                        SELECT NAMAASALSURAT AS VALUE, NAMAASALSURAT AS DATA
                        FROM surat.ASALSURAT
                        WHERE
                          STATUS = 1 AND
                          (LOWER(NAMAASALSURAT) LIKE '%badan%' OR LOWER(NAMAASALSURAT) LIKE '%kementerian%') 
                        ORDER BY NAMAASALSURAT", OtorisasiUser.NamaSkemaMaster);
                    break;
                case "lain":
                    query = string.Format(@"
                        SELECT NAMAASALSURAT AS VALUE, NAMAASALSURAT AS DATA
                        FROM surat.ASALSURAT
                        WHERE
                          STATUS = 1 AND
                          (LOWER(NAMAASALSURAT) LIKE '%badan%' OR LOWER(NAMAASALSURAT) LIKE '%kementerian%' OR LOWER(NAMAASALSURAT) LIKE '%bank%') 
                        ORDER BY NAMAASALSURAT", OtorisasiUser.NamaSkemaMaster);
                    break;
                case "bpn":
                    query = @"
                        SELECT DISTINCT
                          DECODE(SUBSTR(ST.SATKERID,0,2),'98',SUBSTR(ST.SATKERID,3,LENGTH(ST.SATKERID)-2),ST.SATKERID) AS SATKERID,
                          ST.NAMAJABATAN AS DATA, ST.KEPALA AS VALUE
                        FROM SIMPEG_2702.SATKER ST
                        WHERE
                          LENGTH(DECODE(SUBSTR(ST.SATKERID,0,2),'98',SUBSTR(ST.SATKERID,3,LENGTH(ST.SATKERID)-2),ST.SATKERID)) <= 6 AND
                          ST.KEPALANIP IS NOT NULL-- AND ST.NAMAJABATAN LIKE '%Kantor Wilayah%'
                        ORDER BY
                          LENGTH(DECODE(SUBSTR(ST.SATKERID,0,2),'98',SUBSTR(ST.SATKERID,3,LENGTH(ST.SATKERID)-2),ST.SATKERID))";
                    break;
            }
            if (!string.IsNullOrEmpty(query))
            {
                using (var ctx = new BpnDbContext())
                {
                    list = ctx.Database.SqlQuery<AsalSurat>(query).ToList();
                }
            }

            return list;
        }

        public List<Pegawai> GetPegawaiByJabatanNama(string profileidtu, string namajabatan, string namapegawai, string metadata, string userid, int from, int to, string unitkerja = null)
        {
            var records = new List<Pegawai>();

            ArrayList aParams = new ArrayList();

            string query = string.Format(@"
                SELECT
                  ROW_NUMBER() OVER(ORDER BY TIPEUSER, TIPEESELONID, RS.NAMA) RNUMBER,
                  COUNT(1) OVER() TOTAL, RS.PEGAWAIID, RS.NAMA, RS.PROFILEID, RS.JABATAN,
                  RS.USERID
                FROM
                  (SELECT DISTINCT
                     PG.PEGAWAIID, JP.PROFILEID,
                     JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '') AS JABATAN, PG.USERID,
                     DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS NAMA,
                     JB.TIPEESELONID, 0 AS TIPEUSER
                   FROM PEGAWAI PG
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PG.PEGAWAIID AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       JB.PROFILEIDTU = '{0}'
                   WHERE
                     (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                   UNION ALL
                   SELECT DISTINCT
                     PPNPN.NIK AS PEGAWAIID, JP.PROFILEID, 'PPNPN' AS JABATAN, PPNPN.USERID,
                     PPNPN.NAMA, JB.TIPEESELONID, 1 AS TIPEUSER
                   FROM PPNPN
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PPNPN.NIK AND
                       JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       JB.PROFILEIDTU = '{0}') RS
                  INNER JOIN REGISTERUSERTTDDIGITAL TTE ON
                    TTE.NIP = RS.PEGAWAIID", profileidtu);

            if (!string.IsNullOrEmpty(unitkerja))
            {
                query = string.Format(@"
                SELECT
                  ROW_NUMBER() OVER(ORDER BY TIPEUSER, TIPEESELONID, RS.NAMA) RNUMBER,
                  COUNT(1) OVER() TOTAL, RS.PEGAWAIID, RS.NAMA, RS.PROFILEID, RS.JABATAN,
                  RS.USERID
                FROM
                  (SELECT DISTINCT
                     PG.PEGAWAIID, JP.PROFILEID,
                     JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '') AS JABATAN, PG.USERID,
                     DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS NAMA,
                     JB.TIPEESELONID, 0 AS TIPEUSER
                   FROM PEGAWAI PG
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PG.PEGAWAIID AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       JB.UNITKERJAID = '{0}'
                   WHERE
                     (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                   UNION ALL
                   SELECT DISTINCT
                     PPNPN.NIK AS PEGAWAIID, JP.PROFILEID, 'PPNPN' AS JABATAN, PPNPN.USERID,
                     PPNPN.NAMA, JB.TIPEESELONID, 1 AS TIPEUSER
                   FROM PPNPN
                     JOIN JABATANPEGAWAI JP ON
                       JP.PEGAWAIID = PPNPN.NIK AND
                       JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND
                       (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       NVL(JP.STATUSHAPUS,'0') = '0'
                     JOIN JABATAN JB ON
                       JB.PROFILEID = JP.PROFILEID AND
                       NVL(JB.SEKSIID,'X') <> 'A800' AND
                       (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                       JB.UNITKERJAID = '{0}') RS
                  INNER JOIN REGISTERUSERTTDDIGITAL TTE ON
                    TTE.NIP = RS.PEGAWAIID", unitkerja);
            }


            if (!string.IsNullOrEmpty(metadata))
            {
                string[] str = metadata.Split('|');
                metadata = "";
                foreach (var s in str)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] ss = s.Split(',');
                        metadata += string.Concat("'", ss[0], "',");
                    }
                }
                metadata = metadata.Remove(metadata.Length - 1, 1);
                query += string.Format(" AND RS.PEGAWAIID NOT IN ({0}) ", metadata);
            }

            if (!String.IsNullOrEmpty(userid))
            {
                query += string.Format(" AND RS.USERID <> '{0}' ", userid);
            }

            if (!String.IsNullOrEmpty(namajabatan))
            {
                aParams.Add(new OracleParameter("NamaJabatan", String.Concat("%", namajabatan.ToLower(), "%")));
                query += " AND LOWER(RS.JABATAN) LIKE :NamaJabatan ";
            }
            if (!String.IsNullOrEmpty(namapegawai))
            {
                aParams.Add(new OracleParameter("NamaPegawai", String.Concat("%", namapegawai.ToLower(), "%")));
                query += " AND LOWER(RS.NAMA) LIKE :NamaPegawai ";
            }

            query = string.Format(string.Concat("SELECT * FROM (" + query + ") WHERE RNumber BETWEEN {0} AND {1}"), from, to);

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = aParams.OfType<object>().ToArray();
                try
                {
                    records = ctx.Database.SqlQuery<Pegawai>(query, aParams.ToArray()).ToList<Pegawai>();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            return records;
        }

        public List<TipeSuratTTE> GetTipeSurat()
        {
            var list = new List<TipeSuratTTE>();

            string skema = OtorisasiUser.NamaSkema;

            string query = string.Format(@"
                SELECT
                  NAMA, INDUK, FORMATNOMOR
                FROM {0}.TIPESURAT
                WHERE
                  AKTIF = 1 AND
                  INDUK IS NOT NULL AND
                  FORMATNOMOR IS NOT NULL
                ORDER BY
                  INDUK, URUTAN", skema);

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<TipeSuratTTE>(query).ToList();
            }

            return list;
        }

        public List<SifatSuratTTE> GetSifatSurat()
        {
            var list = new List<SifatSuratTTE>();

            string skema = OtorisasiUser.NamaSkema;

            string query = string.Format(@"
                SELECT 
                  NAMA, PRIORITAS
                FROM {0}.SIFATSURAT
                ORDER BY URUTAN", skema);

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<SifatSuratTTE>(query).ToList();
            }

            return list;
        }

        public string NewDraftCode()
        {
            string _result = "";
            string skema = OtorisasiUser.NamaSkema;
            using (var ctx = new BpnDbContext())
            {
                string sql = string.Empty;
                bool check = true;
                do
                {
                    _result = functions.RndCode(6);
                    sql = string.Format("SELECT COUNT(1) FROM {0}.TBLDRAFTSURAT WHERE DRAFTCODE = '{1}' AND STATUS <> 'D'", skema, _result);
                    check = (ctx.Database.SqlQuery<int>(sql).FirstOrDefault() > 0);
                } while (check);
            }

            return _result;
        }

        public TransactionResult SimpanDraftNaskahDinas(DraftSurat data, string nama, string kantorid)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Empty;
                        var aParams = new ArrayList();
                        var parameters = aParams.OfType<object>().ToArray();
                        DraftSurat old = new DraftSurat();
                        if (string.IsNullOrEmpty(data.DraftCode))
                        {
                            data.DraftCode = NewDraftCode();
                            sql = string.Format(@"
                                INSERT INTO {0}.TBLDRAFTSURAT (DRAFTCODE, UNITKERJAID, PERIHAL, KOPSURAT, KODEARSIP, SIFATSURAT, TIPESURAT, HALAMANTTE, ISISURAT, STATUS, UPDTIME, UPDUSER, PROFILEPENGIRIM)
                                VALUES ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',:IsiSurat,'P',SYSDATE,'{9}','{10}')
                                ", skema, data.DraftCode, data.UnitKerjaId, data.Perihal, data.KopSurat, data.KodeArsip, data.SifatSurat, data.TipeSurat, data.PosisiTTE, data.UserPembuat, data.ProfilePengirim);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("IsiSurat", data.IsiSurat));
                            parameters = aParams.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            sql = string.Format(@"
                                INSERT INTO {0}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT)
                                VALUES (RAWTOHEX(SYS_GUID()),'{1}','{2}',SYSDATE,'New')
                                ", skema, data.DraftCode, data.UserPembuat);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }
                        else
                        {
                            string log = string.Empty;
                            string update = string.Empty;
                            old = GetDraftSurat(data.DraftCode, data.UnitKerjaId);
                            aParams.Clear();
                            if (string.IsNullOrEmpty(old.Perihal) || !old.Perihal.Equals(data.Perihal))
                            {
                                log = string.Concat(log, "PERIHAL|!", old.Perihal, "||");
                                update = string.Concat(update, "PERIHAL = :data,");
                                aParams.Add(new OracleParameter("data", data.Perihal));
                            }
                            if (string.IsNullOrEmpty(old.KopSurat) || !old.KopSurat.Equals(data.KopSurat))
                            {
                                log = string.Concat(log, "KOPSURAT|!", old.KopSurat, "||");
                                update = string.Concat(update, "KOPSURAT = :data,");
                                aParams.Add(new OracleParameter("data", data.KopSurat));
                            }
                            if (string.IsNullOrEmpty(old.KodeArsip) || !old.KodeArsip.Equals(data.KodeArsip))
                            {
                                log = string.Concat(log, "KODEARSIP|!", old.KodeArsip, "||");
                                update = string.Concat(update, "KODEARSIP = :data,");
                                aParams.Add(new OracleParameter("data", data.KodeArsip));
                            }
                            if (string.IsNullOrEmpty(old.SifatSurat) || !old.SifatSurat.Equals(data.SifatSurat))
                            {
                                log = string.Concat(log, "SIFATSURAT|!", old.SifatSurat, "||");
                                update = string.Concat(update, "SIFATSURAT = :data,");
                                aParams.Add(new OracleParameter("data", data.SifatSurat));
                            }
                            if (string.IsNullOrEmpty(old.TipeSurat) || !old.TipeSurat.Equals(data.TipeSurat))
                            {
                                log = string.Concat(log, "TIPESURAT|!", old.TipeSurat, "||");
                                update = string.Concat(update, "TIPESURAT = :data,");
                                aParams.Add(new OracleParameter("data", data.TipeSurat));
                            }
                            if (string.IsNullOrEmpty(old.PosisiTTE) || !old.PosisiTTE.Equals(data.PosisiTTE))
                            {
                                log = string.Concat(log, "HALAMANTTE|!", old.PosisiTTE, "||");
                                update = string.Concat(update, "HALAMANTTE = :data,");
                                aParams.Add(new OracleParameter("data", data.PosisiTTE));
                            }
                            if (string.IsNullOrEmpty(old.IsiSurat) || !old.IsiSurat.Equals(data.IsiSurat))
                            {
                                log = string.Concat(log, "ISISURAT|!", old.IsiSurat, "||");
                                update = string.Concat(update, "ISISURAT = :data,");
                                aParams.Add(new OracleParameter("data", data.IsiSurat));
                            }
                            if (string.IsNullOrEmpty(old.ProfilePengirim) || !old.ProfilePengirim.Equals(data.ProfilePengirim))
                            {
                                log = string.Concat(log, "PROFILEPENGIRIM|!", old.ProfilePengirim, "||");
                                update = string.Concat(update, "PROFILEPENGIRIM = :data,");
                                aParams.Add(new OracleParameter("data", data.ProfilePengirim));
                            }
                            if (!string.IsNullOrEmpty(update))
                            {
                                sql = string.Format(@"
                                UPDATE {0}.TBLDRAFTSURAT SET
                                    {3} UPDTIME = SYSDATE, UPDUSER = '{4}'
                                WHERE DRAFTCODE = '{1}' AND UNITKERJAID = '{2}'
                                ", skema, data.DraftCode, data.UnitKerjaId, update, data.UserPembuat);
                                parameters = aParams.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                if (old.Status != "P")
                                {
                                    sql = string.Format(@"
                                    INSERT INTO {0}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT, LOGDETAIL)
                                    VALUES (RAWTOHEX(SYS_GUID()),'{1}','{2}',SYSDATE,'Update',:log)
                                    ", skema, data.DraftCode, data.UserPembuat);
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("log", log));
                                    parameters = aParams.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                }
                            }
                        }


                        //check DraftSuratDetail
                        var details = ctx.Database.SqlQuery<DraftSuratDetail>($"SELECT DETAILTEXT AS TEXT , DETAILVALUE AS VALUE FROM {skema}.TBLDRAFTSURATDETAIL WHERE DRAFTCODE='{data.DraftCode}'").ToList();
                        var cTanggalUnd = true;
                        var cTempatlUnd = true;
                        var cYangTtd = true;
                        var cMenerangkan = true;
                        var cAlamatKeterangan = true;
                        var cNamaKeterangan = true;
                        var cNoIndukKeterangan = true;
                        var cPangkatKeterangan = true;
                        var cJabatanKeterangan = true;
                        var cOptTTD = true;
                        var cOptAn = true;
                        var cOptAb = true;
                        var cOpttj = true;


                        //check and update
                        if (details.Count > 0)
                        {
                            foreach (var detail in details)
                            {
                                if (data.TipeSurat.Equals("Surat Undangan"))
                                {
                                    if (detail.Text.Equals("TanggalUndangan") && !string.IsNullOrEmpty(data.TanggalUndangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.TanggalUndangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'TanggalUndangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cTanggalUnd = false;
                                    }
                                    if (detail.Text.Equals("TempatUndangan") && !string.IsNullOrEmpty(data.TempatUndangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.TempatUndangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'TempatUndangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cTempatlUnd = false;
                                    }
                                }
                                if (data.TipeSurat.Equals("Surat Keterangan"))
                                {
                                    if (detail.Text.Equals("YangTandaTangan") && !string.IsNullOrEmpty(data.YangTandaTangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.YangTandaTangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'YangTandaTangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cYangTtd = false;
                                    }
                                    if (detail.Text.Equals("MenerangkanBahwa") && !string.IsNullOrEmpty(data.MenerangkanBahwa))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.MenerangkanBahwa}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'MenerangkanBahwa'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cMenerangkan = false;
                                    }
                                    if (detail.Text.Equals("AlamatKeterangan") && !string.IsNullOrEmpty(data.AlamatKeterangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.AlamatKeterangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'AlamatKeterangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cAlamatKeterangan = false;
                                    }
                                    if (detail.Text.Equals("NamaKeterangan") && !string.IsNullOrEmpty(data.NamaKeterangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.NamaKeterangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'NamaKeterangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cNamaKeterangan = false;
                                    }
                                    if (detail.Text.Equals("NoIndukKeterangan") && !string.IsNullOrEmpty(data.NoIndukKeterangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.NoIndukKeterangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'NoIndukKeterangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cNoIndukKeterangan = false;
                                    }
                                    if (detail.Text.Equals("PangkatKeterangan") && !string.IsNullOrEmpty(data.PangkatKeterangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.PangkatKeterangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'PangkatKeterangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cPangkatKeterangan = false;
                                    }
                                    if (detail.Text.Equals("JabatanKeterangan") && !string.IsNullOrEmpty(data.JabatanKeterangan))
                                    {
                                        sql = $@"
                                    UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                    SET DETAILVALUE = '{data.JabatanKeterangan}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'JabatanKeterangan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cJabatanKeterangan = false;
                                    }
                                }

                                if (detail.Text.Equals("OptionTTD"))
                                {
                                    if (data.TanpaGelar)
                                    {
                                        sql = $@"
                                        UPDATE {skema}.TBLDRAFTSURATDETAIL 
                                        SET DETAILVALUE ='Tanpa Gelar' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'OptionTTD'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cOptTTD = false;
                                    }
                                    else
                                    {
                                        sql = $@"
                                        UPDATE {skema}.TBLDRAFTSURATDETAIL 
                                        SET DETAILVALUE = NULL WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'OptionTTD'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cOptTTD = false;
                                    }
                                }
                                if (detail.Text.Equals("OptionAn"))
                                {
                                    if (!string.IsNullOrEmpty(data.AtasNama))
                                    {
                                        sql = $@"
                                        UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                        SET DETAILVALUE = '{data.AtasNama}' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'OptionAn'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cOptAn = false;
                                    }
                                    else
                                    {
                                        sql = $@"
                                        UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                        SET DETAILVALUE = NULL WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'OptionAn'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cOptAn = false;
                                    }
                                }
                                if (detail.Text.Equals("OptionTujuan"))
                                {
                                    if (data.TujuanTerlampir)
                                    {
                                        sql = $@"
                                        UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                        SET DETAILVALUE = 'Terlampir' WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'OptionTujuan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cOpttj = false;
                                    }
                                    else
                                    {
                                        sql = $@"
                                        UPDATE {skema}.TBLDRAFTSURATDETAIL  
                                        SET DETAILVALUE = NULL WHERE DRAFTCODE ='{data.DraftCode}' AND DETAILTEXT = 'OptionTujuan'";
                                        ctx.Database.ExecuteSqlCommand(sql);
                                        cOpttj = false;
                                    }
                                }
                            }
                        }

                        //if new then create
                        if (data.TipeSurat.Equals("Surat Undangan"))
                        {
                            if (cTanggalUnd && !string.IsNullOrEmpty(data.TanggalUndangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','TanggalUndangan','{data.TanggalUndangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cTempatlUnd && !string.IsNullOrEmpty(data.TempatUndangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','TempatUndangan','{data.TempatUndangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                        }
                        if (data.TipeSurat.Equals("Surat Keterangan"))
                        {
                            if (cYangTtd && !string.IsNullOrEmpty(data.YangTandaTangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','YangTandaTangan','{data.YangTandaTangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cMenerangkan && !string.IsNullOrEmpty(data.MenerangkanBahwa))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','MenerangkanBahwa','{data.MenerangkanBahwa}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cAlamatKeterangan && !string.IsNullOrEmpty(data.AlamatKeterangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','AlamatKeterangan','{data.AlamatKeterangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cNamaKeterangan && !string.IsNullOrEmpty(data.NamaKeterangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','NamaKeterangan','{data.NamaKeterangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cNoIndukKeterangan && !string.IsNullOrEmpty(data.NoIndukKeterangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','NoIndukKeterangan','{data.NoIndukKeterangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cPangkatKeterangan && !string.IsNullOrEmpty(data.PangkatKeterangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','PangkatKeterangan','{data.PangkatKeterangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            if (cJabatanKeterangan && !string.IsNullOrEmpty(data.JabatanKeterangan))
                            {
                                sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','JabatanKeterangan','{data.JabatanKeterangan}')";
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                        }

                        if (cOptTTD && data.TanpaGelar)
                        {
                            sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','OptionTTD','Tanpa Gelar')";
                            ctx.Database.ExecuteSqlCommand(sql);
                        }
                        if (cOptAn && !string.IsNullOrEmpty(data.AtasNama))
                        {
                            sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','OptionAn','{data.AtasNama}')";
                            ctx.Database.ExecuteSqlCommand(sql);
                        }

                        if (cOpttj && data.TujuanTerlampir)
                        {
                            sql = $@"
                                INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
                                VALUES ('{data.DraftCode}','OptionTujuan','Terlampir')";
                            ctx.Database.ExecuteSqlCommand(sql);
                        }


                        if (!string.IsNullOrEmpty(data.listTujuan))
                        {
                            string[] tujuan = data.listTujuan.Split('|');
                            int x = 1;
                            sql = string.Format(@"
                                DELETE {0}.TBLDRAFTSURATTUJUAN
                                WHERE DRAFTCODE = '{1}' AND TEMBUSAN = '0'
                                ", skema, data.DraftCode);
                            ctx.Database.ExecuteSqlCommand(sql);
                            foreach (var t in tujuan)
                            {
                                string fnama;
                                string fprofileid;
                                if (t == "SK")
                                {
                                    fnama = "SK";
                                    fprofileid = "SK";
                                }
                                else
                                {
                                    fnama = t.Split('%')[0];
                                    fprofileid = t.Split('%')[1];
                                }
                                sql = string.Format(@"
                                INSERT INTO {0}.TBLDRAFTSURATTUJUAN (DRAFTCODE, URUTAN, NAMA, TEMBUSAN, PROFILID)
                                VALUES ('{1}',{2},'{3}','0','{4}')
                                ", skema, data.DraftCode, x, fnama, fprofileid);
                                ctx.Database.ExecuteSqlCommand(sql);
                                x += 1;
                            }
                        }
                        else if (string.IsNullOrEmpty(data.listTujuan) && old.Tujuan != null && old.Tujuan.Count > 0)
                        {
                            sql = string.Format(@"
                            DELETE {0}.TBLDRAFTSURATTUJUAN
                            WHERE DRAFTCODE = '{1}' AND TEMBUSAN = '0'
                            ", skema, data.DraftCode);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }


                        if (!string.IsNullOrEmpty(data.listTembusan))
                        {
                            string[] tembusan = data.listTembusan.Split('|');
                            int x = 1;
                            sql = string.Format(@"
                                DELETE {0}.TBLDRAFTSURATTUJUAN
                                WHERE DRAFTCODE = '{1}' AND TEMBUSAN = '1'
                                ", skema, data.DraftCode);
                            ctx.Database.ExecuteSqlCommand(sql);
                            foreach (var t in tembusan)
                            {
                                string fnama = t.Split('%')[0];
                                string fprofileid = t.Split('%')[1];
                                sql = string.Format(@"
                                INSERT INTO {0}.TBLDRAFTSURATTUJUAN (DRAFTCODE, URUTAN, NAMA, TEMBUSAN, PROFILID)
                                VALUES ('{1}',{2},'{3}','1','{4}')
                                ", skema, data.DraftCode, x, fnama, fprofileid);
                                ctx.Database.ExecuteSqlCommand(sql);
                                x += 1;
                            }
                        }
                        else if (string.IsNullOrEmpty(data.listTembusan) && old.Tembusan != null && old.Tembusan.Count > 0)
                        {
                            sql = string.Format(@"
                            DELETE {0}.TBLDRAFTSURATTUJUAN
                            WHERE DRAFTCODE = '{1}' AND TEMBUSAN = '1'
                            ", skema, data.DraftCode);
                            ctx.Database.ExecuteSqlCommand(sql);
                        }

                        if (!string.IsNullOrEmpty(data.LampiranId))
                        {
                            sql = string.Format("SELECT COUNT(1) FROM {0}.TBLLAMPIRANDRAFTSURAT WHERE LAMPIRANID = '{1}' AND STATUS = 'A'", skema, data.LampiranId);
                            int ctlampiranExist = ctx.Database.SqlQuery<int>(sql).FirstOrDefault();
                            string statuslampiran = "A";
                            string Hapus = data.LampiranId.Split('|')[0];
                            if (Hapus == "hapus")
                            {
                                ctlampiranExist = 1;
                                statuslampiran = "D";
                                data.LampiranId = data.LampiranId.Replace("hapus|", "");
                            }
                            if (ctlampiranExist == 0)
                            {
                                sql = string.Format(@"
                                    INSERT INTO {0}.TBLLAMPIRANDRAFTSURAT (LAMPIRANID, DRAFTCODE, STATUS, UPDTIME, UPDUSER)
                                    VALUES ('{1}','{2}','{4}',SYSDATE,'{3}')
                                    ", skema, data.LampiranId, data.DraftCode, data.UserPembuat, statuslampiran);
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                            else
                            {
                                sql = string.Format(@"
                                    UPDATE {0}.TBLLAMPIRANDRAFTSURAT SET 
                                        STATUS = '{4}', 
                                        UPDTIME = SYSDATE, 
                                        UPDUSER = '{3}'
                                    WHERE LAMPIRANID = '{1}' AND DRAFTCODE = '{2}'
                                    ", skema, data.LampiranId, data.DraftCode, data.UserPembuat, statuslampiran);
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                        }

                        if (data.TTE != null && data.TTE.Count > 0)
                        {
                            sql = string.Format(@"
                                DELETE {0}.TBLPENANDATANGANDRAFTSURAT
                                WHERE DRAFTCODE = '{1}'
                                ", skema, data.DraftCode);
                            ctx.Database.ExecuteSqlCommand(sql);
                            foreach (var tte in data.TTE)
                            {
                                sql = string.Format(@"
                                INSERT INTO {0}.TBLPENANDATANGANDRAFTSURAT (DRAFTCODE, USERID, PEGAWAIID, NAMA, PROFILEID, JABATAN, TIPE, URUT, STATUS, UPDTIME, UPDUSER)
                                SELECT 
	                                '{1}' AS DRAFTCODE, 
	                                PG.USERID, 
	                                PG.PEGAWAIID, 
	                                DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS NAMA,
	                                JB.PROFILEID, 
                                  JB.NAMA AS JABATAN, 
	                                '{4}' AS TIPE, 
	                                {5} AS URUT, 
	                                'P' AS STATUS, 
	                                SYSDATE AS UPDTIME, 
	                                '{6}' AS UPDUSER 
                                FROM PEGAWAI PG 
                                INNER JOIN JABATANPEGAWAI JP ON 
	                                JP.PEGAWAIID = PG.PEGAWAIID 
                                INNER JOIN JABATAN JB ON 
	                                JB.PROFILEID = JP.PROFILEID 
                                WHERE PG.USERID = '{2}' AND JB.PROFILEID = '{3}' AND ROWNUM = 1  
                                ", skema, data.DraftCode, tte.PenandatanganId, tte.ProfileId, tte.Tipe, tte.Urut, data.UserPembuat);
                                ctx.Database.ExecuteSqlCommand(sql);
                            }
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = data.DraftCode;
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

        public TransactionResult InsertSuratKeluar(Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            string sql = string.Empty;
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = mDataMaster.GetSatkerId(unitkerjaid);
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();
                        data.Arah = "Masuk"; // Keluar
                        data.PengirimSurat = mDataMaster.GetUnitKerjaFromProfileId(data.ProfileIdPengirim);

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.PenerimaSurat + " ";
                        metadata += data.IsiSingkatSurat + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata += data.PegawaiId + " ";
                        metadata += data.TipeSurat + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.Kategori + " ";
                        metadata = metadata.Trim();
                        #endregion

                        // Insert SURAT
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".surat ( " +
                            "       suratid, kantorid, tanggalsurat, tanggalproses, tanggalundangan, nomorsurat, nomoragenda, perihal, pengirim, penerima, arah, kategori, " +
                            "       tipesurat, sifatsurat, keterangansurat, jumlahlampiran, isisingkat, unitorganisasi, tipekegiatan, referensi, metadata) VALUES " +
                            "( " +
                            "       :Id, :SatkerId, TO_DATE(:TanggalSurat,'DD/MM/YYYY'), TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'), TO_DATE(:TanggalUndangan,'DD/MM/YYYY HH24:MI'), " +
                            "       :NomorSurat, :NomorAgenda, :Perihal, :PengirimSurat, :PenerimaSurat, :Arah, :Kategori, " +
                            "       :TipeSurat, :SifatSurat, :KeteranganSurat, :JumlahLampiran, :IsiSingkatSurat, :NamaSeksi, :KodeKlasifikasi, :Referensi, utl_raw.cast_to_raw(:Metadata))";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("Id", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("TanggalSurat", data.TanggalSurat));
                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                        aParams.Add(new OracleParameter("TanggalUndangan", data.TanggalUndangan));
                        aParams.Add(new OracleParameter("NomorSurat", data.NomorSurat));
                        aParams.Add(new OracleParameter("NomorAgenda", data.NomorAgenda));
                        aParams.Add(new OracleParameter("Perihal", data.Perihal));
                        aParams.Add(new OracleParameter("PengirimSurat", data.PengirimSurat));
                        aParams.Add(new OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        aParams.Add(new OracleParameter("Arah", data.Arah));
                        aParams.Add(new OracleParameter("Kategori", data.ArahSuratKeluar));
                        aParams.Add(new OracleParameter("TipeSurat", data.TipeSurat));
                        aParams.Add(new OracleParameter("SifatSurat", data.SifatSurat));
                        aParams.Add(new OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        aParams.Add(new OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        aParams.Add(new OracleParameter("IsiSingkatSurat", data.IsiSingkatSurat));
                        aParams.Add(new OracleParameter("NamaSeksi", data.NamaSeksi));
                        aParams.Add(new OracleParameter("KodeKlasifikasi", data.KodeKlasifikasi));
                        aParams.Add(new OracleParameter("Referensi", data.Referensi));
                        aParams.Add(new OracleParameter("Metadata", metadata));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        foreach (var file in data.ListFiles)
                        {
                            string path = string.Empty;
                            if (string.IsNullOrEmpty(file.Tipe))
                            {
                                path = "-";
                            }
                            else
                            {
                                path = string.Concat(file.Tipe, "|", file.FilesId);
                                file.FilesId = string.Empty;
                            }
                            string namafile = !string.IsNullOrEmpty(file.Tipe) && file.Tipe.Equals("DokumenTTE") ? string.Concat(file.PengenalFile, ".pdf") : file.PengenalFile;

                            sql = string.Format(@"
                            INSERT INTO {0}.LAMPIRANSURAT 
                                (LAMPIRANSURATID, SURATID, PATH, NAMAFILE, PROFILEID, KANTORID, KETERANGAN, NIP) VALUES
                                (:LampiranSuratId,:SuratId,:Path,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)", OtorisasiUser.NamaSkema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("LampiranSuratId", string.IsNullOrEmpty(file.FilesId)?GetUID():file.FilesId));
                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                            aParams.Add(new OracleParameter("Path", path));
                            aParams.Add(new OracleParameter("NamaFile", namafile));
                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.UserId));
                            aParams.Add(new OracleParameter("pKantKantorIdorId", kantorid));
                            aParams.Add(new OracleParameter("Keterangan", file.PengenalFile));
                            aParams.Add(new OracleParameter("Nip", nip));
                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                        }

                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("Nip", data.PegawaiId));
                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        string suratinboxid = "";
                        int urutan = 0;

                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "            statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "            0,0,1)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                        aParams.Add(new OracleParameter("Nip", nip)); // nip Pengirim
                        aParams.Add(new OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                        aParams.Add(new OracleParameter("Keterangan", data.CatatanAnda));
                        aParams.Add(new OracleParameter("Redaksi", ""));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            "            suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "            :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #endregion

                        #region Session Tujuan Surat

                        string profileidba = "";
                        bool belumkirimtuba = true;

                        foreach (SessionTujuanSurat tujuanSurat in data.ListTujuanSurat)
                        {
                            Pegawai pegawaiTujuan = mDataMaster.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            string profileidtu = mDataMaster.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            if (tujuanSurat.Redaksi.Equals("Asli"))
                            {
                                data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            }

                            int statusterkunci = 0;

                            bool IsKirimKeBA = true;

                            if (data.TipeSurat == "Nota Dinas" ||
                                data.TipeSurat == "Surat Pengantar")
                            {
                                IsKirimKeBA = false;
                            }


                            #region Kirim ke TU Persuratan (Pusat)

                            // Cek TU pengirim dan tujuan. Bila sama, tidak insert ke BA
                            if (profileidtu == myprofileidtu)
                            {
                                IsKirimKeBA = false;
                            }

                            if (IsKirimKeBA)
                            {
                                if (belumkirimtuba)
                                {
                                    profileidba = mDataMaster.GetProfileIdBAFromProfileId(myprofileidtu);

                                    if (!string.IsNullOrEmpty(profileidba))
                                    {
                                        statusterkunci = 1;

                                        string pegawaiidba = mDataMaster.GetPegawaiIdFromProfileId(profileidba, true);
                                        if (string.IsNullOrEmpty(pegawaiidba))
                                        {
                                            pegawaiidba = mDataMaster.GetPegawaiIdFromProfileId(profileidba);
                                        }
                                        Pegawai pegawaiBA = mDataMaster.GetPegawaiByPegawaiId(pegawaiidba);
                                        if (pegawaiBA == null)
                                        {
                                            string namaprofile = mDataMaster.GetProfileNameFromId(profileidba);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidba;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }


                                        #region Cek Duplikasi

                                        bool BisaKirimSurat = true;

                                        sql =
                                            "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimSurat = false;
                                        }

                                        #endregion

                                        if (BisaKirimSurat)
                                        {
                                            // Insert SURATINBOX
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                            sql =
                                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "            statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "            0,1,:Urutan)";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                            aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                            aParams.Add(new OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                                            aParams.Add(new OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                                            aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                            aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                            aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            aParams.Add(new OracleParameter("Urutan", urutan));
                                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                                            // Insert SURATOUTBOXRELASI
                                            sql =
                                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                            aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                            aParams.Add(new OracleParameter("StatusBaca", "D"));
                                            aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                        }
                                    }

                                    belumkirimtuba = false;
                                }
                            }

                            #endregion

                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                if (profileidtu != myprofileidtu)
                                {
                                    string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                    if (string.IsNullOrEmpty(pegawaiidtu))
                                    {
                                        pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                    }
                                    Pegawai pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                    if (pegawaiTU == null)
                                    {
                                        string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidtu;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }

                                    #region Cek Duplikasi

                                    bool BisaKirimKeTUPengolah = true;

                                    string query =
                                        "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                    aParams.Add(new OracleParameter("SatkerId", satkerid));
                                    aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                    aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                    int jumlahinbox = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimKeTUPengolah = false;
                                    }

                                    #endregion

                                    if (pegawaiidtu == tujuanSurat.NIP)
                                    {
                                        BisaKirimKeTUPengolah = false;
                                        statusterkunci = 0;
                                    }

                                    if (BisaKirimKeTUPengolah)
                                    {
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                        sql =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "       :StatusTerkunci,1,:Urutan)";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                        aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                        aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                        aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                        aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                        aParams.Add(new OracleParameter("Urutan", urutan));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                        // Insert SURATOUTBOXRELASI (ke Profile TU)
                                        sql =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                        statusterkunci = 1;
                                    }
                                }
                            }
                            else
                            {
                                statusterkunci = 0;
                            }

                            #endregion

                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql =
                                "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND NIP = :PegawaiIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            aParams.Add(new OracleParameter("PegawaiIdPenerima", tujuanSurat.NIP));
                            aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :StatusTerkunci,0,:Urutan)";
                                //sql = sWhitespace.Replace(sql, " ");
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                aParams.Add(new OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                                aParams.Add(new OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai)); // nama penerima surat
                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                aParams.Add(new OracleParameter("Urutan", urutan));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            }

                            #endregion

                        }

                        #endregion

                        #region Bila Eksternal dan tidak ada Tembusan Surat, kirim ke TU Persuratan (Pusat)

                        if (data.ArahSuratKeluar == "Eksternal")
                        {
                            if (data.ListTujuan.Count == 0)
                            {
                                profileidba = mDataMaster.GetProfileIdBAFromProfileId(myprofileidtu);
                                if (!string.IsNullOrEmpty(profileidba))
                                {
                                    string pegawaiidba = mDataMaster.GetPegawaiIdFromProfileId(profileidba, true);
                                    if (string.IsNullOrEmpty(pegawaiidba))
                                    {
                                        pegawaiidba = mDataMaster.GetPegawaiIdFromProfileId(profileidba);
                                    }
                                    Pegawai pegawaiBA = mDataMaster.GetPegawaiByPegawaiId(pegawaiidba);
                                    if (pegawaiBA == null)
                                    {
                                        string namaprofile = mDataMaster.GetProfileNameFromId(profileidba);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidba;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat Eksternal tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }

                                    #region Cek Duplikasi

                                    bool IsBisaKirim = true;

                                    sql =
                                        "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                    aParams.Add(new OracleParameter("SatkerId", satkerid));
                                    aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                    int jumlahdata = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                    if (jumlahdata > 0)
                                    {
                                        IsBisaKirim = false;
                                    }

                                    #endregion

                                    if (IsBisaKirim)
                                    {
                                        // Insert SURATINBOX
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                        sql =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "            SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "            NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "            statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "            :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "            :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "            0,1,:Urutan)";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                        aParams.Add(new OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                                        aParams.Add(new OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                        aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        aParams.Add(new OracleParameter("Redaksi", "Asli"));
                                        aParams.Add(new OracleParameter("Urutan", urutan));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                        // Insert SURATOUTBOXRELASI
                                        sql =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                    }
                                }
                            }
                        }

                        #endregion

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = string.Concat("Surat ",data.NomorSurat," berhasil dikirim");
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT"))
                        {
                            tr.Pesan = string.Concat("Nomor Surat ",data.NomorSurat," sudah ada.");
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

        public TransactionResult SimpanLampiranSurat(string kantorid, string suratid, string dokumenid, string namafile, string userid, string nip, string tipedokumen)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        string path = string.Concat(tipedokumen, "|", dokumenid);
                        var aParams = new ArrayList();

                        sql = string.Format(@"
                            INSERT INTO {0}.LAMPIRANSURAT 
                                (LAMPIRANSURATID, SURATID, PATH, NAMAFILE, PROFILEID, KANTORID, KETERANGAN, NIP) VALUES
                                (:LampiranSuratId,:SuratId,:Path,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)", OtorisasiUser.NamaSkema);
                        aParams.Add(new OracleParameter("LampiranSuratId", GetUID()));
                        aParams.Add(new OracleParameter("SuratId", suratid));
                        aParams.Add(new OracleParameter("Path", path));
                        aParams.Add(new OracleParameter("NamaFile", namafile));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", userid));
                        aParams.Add(new OracleParameter("pKantKantorIdorId", kantorid));
                        aParams.Add(new OracleParameter("Keterangan", namafile));
                        aParams.Add(new OracleParameter("Nip", nip));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

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

        public int GetMaxUrutanSuratInbox(BpnDbContext ctx, string suratid)
        {
            int result = 0;

            string query = "select max(urutan)+1 from " + OtorisasiUser.NamaSkema + ".suratinbox where suratid = :param1";
            var aParams = new ArrayList();
            aParams.Add(new OracleParameter("param1", suratid));
            result = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();

            return result;
        }

        public List<SessionTujuanSurat> getTujuanMassal(int tipekantorid)
        {
            var records = new List<SessionTujuanSurat>();
            var aParams = new ArrayList();
            string query = @"
                SELECT DISTINCT
                  JB.PROFILEID AS PROFILEID,
                  JB.NAMA AS NAMAJABATAN,
                  JP.PEGAWAIID AS NIP,
                  PG.NAMA_LENGKAP AS NAMAPEGAWAI,
                  'Asli' AS REDAKSI
                FROM UNITKERJA UK
                  INNER JOIN JABATAN JB ON
                    JB.UNITKERJAID = UK.UNITKERJAID AND
                    JB.PROFILEID = JB.PROFILEIDTU
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = JB.PROFILEID AND
                    NVL(JP.STATUSHAPUS,'0') = '0' AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                  INNER JOIN SIMPEG_2702.V_PEGAWAI_EOFFICE PG ON
                  	PG.NIPBARU = JP.PEGAWAIID
                WHERE
                  UK.TAMPIL = 1 AND
                  UK.TIPEKANTORID = :param1";
            aParams.Add(new OracleParameter("param1", tipekantorid));

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<SessionTujuanSurat>(query, aParams.ToArray()).ToList();
            }

            return records;
        }

        public List<Files> getListDokumenTTE(string userid, string profiletu, string kantorid, bool isTU, string metadata, int from, int to)
        {
            var records = new List<Files>();
            string skema = OtorisasiUser.NamaSkema;
            var aParams = new ArrayList();

            string query = string.Format(@"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY TDE.NOMORSURAT) AS RNUMBER, 
                    COUNT(1) OVER() TOTAL,
                    TDE.DOKUMENELEKTRONIKID AS FILESID,
                    TDE.PERIHAL AS KETERANGAN,
                    'DokumenTTE' AS TIPE,
                    TDE.NOMORSURAT AS PENGENALFILE,
                    TO_CHAR(TDE.TANGGALDIBUAT, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                    TDE.SIFATSURAT AS SIFATFILE,
                    TDE.KANTORID
                FROM {0}.TBLDOKUMENELEKTRONIK TDE WHERE TDE.USERPEMBUAT = :param1 AND NVL(TDE.STATUSHAPUS,'0') = '0' AND TDE.KODEFILE IS NOT NULL AND TDE.TANGGALTOLAK IS NULL AND TDE.KANTORID = :param2 AND LOWER(TDE.PERIHAL || TDE.NOMORSURAT) LIKE :param3", skema);
            aParams.Clear();
            aParams.Add(new OracleParameter("param1", userid));
            aParams.Add(new OracleParameter("param2", kantorid));
            aParams.Add(new OracleParameter("param3", string.Concat("%", metadata.ToLower(),"%")));
            if (isTU)
            {
                query = string.Format(@"
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY RST.TANGGALDIBUAT DESC, RST.NOMORSURAT DESC) AS RNUMBER,  
                        COUNT(1) OVER() TOTAL, 
                        RST.DOKUMENELEKTRONIKID AS FILESID,
                        RST.PERIHAL AS KETERANGAN,
                        'DokumenTTE' AS TIPE,
                        RST.NOMORSURAT AS PENGENALFILE,
                        TO_CHAR(RST.TANGGALDIBUAT, 'dd/mm/yyyy fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALDIBUAT,
                        RST.SIFATSURAT AS SIFATFILE,
                        RST.KANTORID
                    FROM 
                        (SELECT DISTINCT 
                            TDE.DOKUMENELEKTRONIKID, TDE.NOMORSURAT, 
                            TO_CHAR(TDE.TANGGALSURAT, 'dd/mm/yyyy', 'nls_date_language=INDONESIAN') AS TANGGALSURAT, 
                            TDE.TANGGALDIBUAT, 
                            TDE.PERIHAL, TDE.SIFATSURAT, TDE.KANTORID
                        FROM {0}.TBLDOKUMENELEKTRONIK TDE
                            LEFT JOIN PEGAWAI PG ON
                                PG.USERID = TDE.USERPEMBUAT AND
                                (PG.VALIDSAMPAI IS NULL OR TRUNC(CAST(PG.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE))
                            LEFT JOIN PPNPN PP ON
                                PP.USERID = TDE.USERPEMBUAT 
                            INNER JOIN JABATANPEGAWAI JP ON
                                (JP.PEGAWAIID = PG.PEGAWAIID OR JP.PEGAWAIID = PP.NIK) AND
                                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                NVL(JP.STATUSHAPUS,'0') = '0' AND
                                JP.KANTORID = :param1 
                            INNER JOIN JABATAN JB ON
                                JB.PROFILEID = JP.PROFILEID AND
                                (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) > TRUNC(SYSDATE)) AND
                                NVL(JB.SEKSIID,'XXX') <> 'A800' AND
                                (JB.PROFILEIDTU = :param2 OR JB.PROFILEIDBA = :param3) 
                            INNER JOIN {0}.TBLDOKUMENTTE TTE ON
                                TTE.DOKUMENELEKTRONIKID = TDE.DOKUMENELEKTRONIKID 
                        WHERE
                            NVL(TDE.STATUSHAPUS,'0') = '0' AND
                            TDE.KANTORID = :param4 AND
                            TDE.TANGGALTOLAK IS NULL AND
                            TDE.KODEFILE IS NOT NULL AND LOWER(TDE.PERIHAL || TDE.NOMORSURAT) LIKE :param5) RST", skema);
                aParams.Clear();
                aParams.Add(new OracleParameter("param1", kantorid));
                aParams.Add(new OracleParameter("param2", profiletu));
                aParams.Add(new OracleParameter("param3", profiletu));
                aParams.Add(new OracleParameter("param4", kantorid));
                aParams.Add(new OracleParameter("param5", string.Concat("%", metadata.ToLower(), "%")));
            }

            query = string.Concat("SELECT * FROM (", query, ") WHERE RNUMBER BETWEEN :pFrom AND :pTo");
            aParams.Add(new OracleParameter("pFrom", from));
            aParams.Add(new OracleParameter("pTo", to));

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Files>(query, aParams.ToArray()).ToList();
            }

            return records;
        }

        public string GetNamaJabatan(string profileid)
        {
            string txt = string.Empty;
            var aParams = new ArrayList();
            string query = @"
                SELECT
                  NAMA
                FROM JABATAN
                WHERE
                  PROFILEID = :param1";
            aParams.Add(new OracleParameter("param1", profileid));

            using (var ctx = new BpnDbContext())
            {
                txt = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).FirstOrDefault();
            }

            return txt;
        }

        public string GetNamaPegawai(string pegawaiid)
        {
            string txt = string.Empty;
            var aParams = new ArrayList();
            string query = @"
                SELECT
	                COALESCE(VPE.NAMA_LENGKAP, PG.NAMA, PP.NAMA)
                FROM JABATANPEGAWAI JP
                LEFT JOIN SIMPEG_2702.V_PEGAWAI_EOFFICE VPE ON
	                VPE.NIPBARU = JP.PEGAWAIID
                LEFT JOIN PEGAWAI PG ON
	                PG.PEGAWAIID = JP.PEGAWAIID
                LEFT JOIN PPNPN PP ON
	                PP.NIK = JP.PEGAWAIID
                WHERE
	                JP.PEGAWAIID = :param1 AND
	                NVL(JP.STATUSHAPUS,'0') = '0' AND
	                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                GROUP BY
	                COALESCE(VPE.NAMA_LENGKAP, PG.NAMA, PP.NAMA)";
            aParams.Add(new OracleParameter("param1", pegawaiid));

            using (var ctx = new BpnDbContext())
            {
                txt = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).FirstOrDefault();
            }

            return txt;
        }

        public Files getFileLampiran(string lampiranid)
        {
            var records = new Files();
            var aParams = new ArrayList();

            string query = string.Format(@"
               SELECT
                 LS.LAMPIRANSURATID AS FILESID,
                 LS.KETERANGAN,
                 LS.PATH,
                 LS.NAMAFILE AS PENGENALFILE,
                 LS.KANTORID
               FROM {0}.LAMPIRANSURAT LS
               WHERE
                 LS.LAMPIRANSURATID = :param1", OtorisasiUser.NamaSkema);
            aParams.Add(new OracleParameter("param1", lampiranid));

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Files>(query, aParams.ToArray()).FirstOrDefault();
            }

            return records;
        }

        public bool cekNomorSuratGanda(string nomorsurat, string pengirim, string unitkerjaid = null)
        {
            bool result = false;

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList aParams = new ArrayList();

            string query =
                 "SELECT COUNT(1) " +
                 " FROM " + OtorisasiUser.NamaSkema + ".surat " +
                 " WHERE UPPER(nomorsurat) = :NomorSurat AND UPPER(pengirim) = :PengirimSurat";

            aParams.Clear();
            aParams.Add(new OracleParameter("NomorSurat", nomorsurat.ToUpper()));
            aParams.Add(new OracleParameter("PengirimSurat", pengirim.ToUpper()));
            if (string.IsNullOrEmpty(unitkerjaid))
            {
                query += " AND KANTORID = :Kantorid";
                aParams.Add(new OracleParameter("KantorId", unitkerjaid));
            }

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).FirstOrDefault() > 0;
            }

            return result;
        }

        public TransactionResult KirimDisposisi(Entities.Surat data, string kantorid, string unitkerjaid, string myprofileidtu, string nip, List<SessionTujuanSurat> dataSessionTujuanSurat)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            string sql = "";
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = mDataMaster.GetSatkerId(unitkerjaid);
                        int tipekantorid = mDataMaster.GetTipeKantor(kantorid);

                        sql = string.Format(@"
                                UPDATE {0}.SURAT SET
                                    STATUSSURAT = 1
                                WHERE SURATID = :param1",skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", data.SuratId));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        sql = string.Format(@"
                                UPDATE {0}.SURATINBOX SET
                                    STATUSTERKIRIM = 1,
                                    KETERANGAN = :param1,
                                    PERINTAHDISPOSISI = :param2
                                WHERE SURATINBOXID = :param3", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", data.CatatanAnda));
                        aParams.Add(new OracleParameter("param2", data.PerintahDisposisi));
                        aParams.Add(new OracleParameter("param3", data.SuratInboxId));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql = string.Format(@"
                                INSERT INTO {0}.SURATOUTBOX 
                                    (SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, KETERANGAN, PERINTAHDISPOSISI)
                                VALUES
                                    (:param1, :param2, :param3, :param4, :param5, SYSDATE, :param6, :param7)", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", suratoutboxid));
                        aParams.Add(new OracleParameter("param2", data.SuratId));
                        aParams.Add(new OracleParameter("param3", satkerid));
                        aParams.Add(new OracleParameter("param4", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("param5", nip));
                        aParams.Add(new OracleParameter("param6", data.CatatanAnda));
                        aParams.Add(new OracleParameter("param7", data.PerintahDisposisi));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #region Session Tujuan Surat

                        string suratinboxid = "";
                        int urutan = 0;

                        foreach (var tujuanSurat in dataSessionTujuanSurat)
                        {
                            var pegawaiTujuan = mDataMaster.GetPegawaiByPegawaiId(tujuanSurat.NIP);
                            string profileidtu = mDataMaster.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            sql = @"
                                SELECT UNITKERJAID
                                FROM JABATAN
                                WHERE 
                                    PROFILEID = :param1";
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", tujuanSurat.ProfileId));
                            tujuanSurat.UnitKerjaId = ctx.Database.SqlQuery<string>(sql, aParams.ToArray()).FirstOrDefault();

                            sql = @"
                                SELECT KANTORID
                                FROM UNITKERJA
                                WHERE 
                                    UNITKERJAID = :param1";
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", tujuanSurat.UnitKerjaId));
                            tujuanSurat.KantorId = ctx.Database.SqlQuery<string>(sql, aParams.ToArray()).FirstOrDefault();

                            int statusterkunci = (myprofileidtu == profileidtu) ? 0 : 1;

                            if (mDataMaster.CheckIsTU(tujuanSurat.NIP,tujuanSurat.KantorId))
                            {
                                statusterkunci = 0;
                            }

                            #region Cek Duplikasi

                            bool BisaKirimSurat = true;

                            sql = string.Format(@"
                                SELECT COUNT(1) 
                                FROM {0}.SURATINBOX
                                WHERE 
                                    SURATID = :param1 AND
                                    PROFILEPENERIMA = :param2 AND
                                    NIP = :param3 AND
                                    STATUSTERKUNCI = :param4 AND
                                    STATUSTERKIRIM = 0", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", data.SuratId));
                            aParams.Add(new OracleParameter("param2", tujuanSurat.ProfileId));
                            aParams.Add(new OracleParameter("param3", tujuanSurat.NIP));
                            aParams.Add(new OracleParameter("param4", statusterkunci));
                            int jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                            if (jumlahinbox > 0)
                            {
                                BisaKirimSurat = false;
                            }

                            #endregion

                            if (BisaKirimSurat)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                sql = string.Format(@"
                                    INSERT INTO {0}.SURATINBOX 
                                        (SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, STATUSTERKUNCI, STATUSURGENT, REDAKSI, URUTAN)
                                    VALUES
                                        (:param1, :param2, :param3, :param4, :param5, :param6, :param7, :param8, SYSDATE, TO_DATE(:param9,'DD/MM/YYYY HH24:MI'), :param10, 0, :param11, :param12, :param13, :param14)", skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("param1", suratinboxid));
                                aParams.Add(new OracleParameter("param2", data.SuratId));
                                aParams.Add(new OracleParameter("param3", satkerid));
                                aParams.Add(new OracleParameter("param4", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("param5", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("param6", data.NamaPengirim));
                                aParams.Add(new OracleParameter("param7", tujuanSurat.NIP));
                                aParams.Add(new OracleParameter("param8", tujuanSurat.NamaPegawai));
                                aParams.Add(new OracleParameter("param9", data.TanggalTerima));
                                aParams.Add(new OracleParameter("param10", "Ekspedisi"));
                                aParams.Add(new OracleParameter("param11", statusterkunci));
                                aParams.Add(new OracleParameter("param12", tujuanSurat.StatusUrgent));
                                aParams.Add(new OracleParameter("param13", tujuanSurat.Redaksi));
                                aParams.Add(new OracleParameter("param14", urutan));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                sql = string.Format(@"
                                    INSERT INTO {0}.SURATOUTBOXRELASI 
                                        (SURATOUTBOXID, SURATINBOXID, PROFILEPENERIMA, STATUSBACA, KETERANGAN)
                                    VALUES
                                        (:param1, :param2, :param3, :param4, :param5)", skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("param1", suratoutboxid));
                                aParams.Add(new OracleParameter("param2", suratinboxid));
                                aParams.Add(new OracleParameter("param3", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("param4", "D"));
                                aParams.Add(new OracleParameter("param5", data.CatatanAnda));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                //#region Kirim Notifikasi Mobile
                                //if (statusterkunci == 0)
                                //{
                                //    try
                                //    {
                                //        new Mobile().KirimNotifikasi(tujuanSurat.NIP, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.CatatanAnda) ? (string.IsNullOrEmpty(data.PerintahDisposisi) ? "Disposisi Baru" : data.PerintahDisposisi.ToString()) : data.CatatanAnda.ToString()), "Kotak Masuk");
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        var msg = ex.Message;
                                //    }
                                //}
                                //#endregion

                                // Kirim Persetujuan Surat Ke TU Penerima Surat
                                if (statusterkunci == 1)
                                {
                                    string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                    if (string.IsNullOrEmpty(pegawaiidtu))
                                    {
                                        pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                    }
                                    var pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                    if (pegawaiTU == null)
                                    {
                                        string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidtu;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }


                                    #region Cek Duplikasi

                                    bool BisaKirimSuratTU = true;

                                    sql = string.Format(@"
                                        SELECT COUNT(1) 
                                        FROM {0}.SURATINBOX
                                        WHERE 
                                            SURATID = :param1 AND
                                            PROFILEPENERIMA = :param2 AND
                                            STATUSTERKUNCI = 0 AND
                                            STATUSFORWARDTU = 1 AND
                                            STATUSTERKIRIM = 0", skema);
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("param1", data.SuratId));
                                    aParams.Add(new OracleParameter("param2", profileidtu));
                                    jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimSuratTU = false;
                                    }

                                    #endregion

                                    if (BisaKirimSuratTU)
                                    {
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                        sql = string.Format(@"
                                            INSERT INTO {0}.SURATINBOX 
                                                (SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, STATUSTERKUNCI, STATUSFORWARDTU, REDAKSI, URUTAN)
                                            VALUES
                                                (:param1, :param2, :param3, :param4, :param5, :param6, :param7, :param8, SYSDATE, TO_DATE(:param9,'DD/MM/YYYY HH24:MI'), :param10, 0, 0, 1, :param11, :param12)", skema);
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("param1", suratinboxid));
                                        aParams.Add(new OracleParameter("param2", data.SuratId));
                                        aParams.Add(new OracleParameter("param3", satkerid));
                                        aParams.Add(new OracleParameter("param4", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("param5", profileidtu));
                                        aParams.Add(new OracleParameter("param6", data.NamaPengirim));
                                        aParams.Add(new OracleParameter("param7", pegawaiidtu)); 
                                        aParams.Add(new OracleParameter("param8", pegawaiTU.NamaLengkap));
                                        aParams.Add(new OracleParameter("param9", data.TanggalTerima));
                                        aParams.Add(new OracleParameter("param10", "Ekspedisi"));
                                        aParams.Add(new OracleParameter("param11", tujuanSurat.Redaksi));
                                        aParams.Add(new OracleParameter("param12", urutan));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                        sql = string.Format(@"
                                            INSERT INTO {0}.SURATOUTBOXRELASI 
                                                (SURATOUTBOXID, SURATINBOXID, PROFILEPENERIMA, STATUSBACA, KETERANGAN)
                                            VALUES
                                                (:param1, :param2, :param3, :param4, :param5)", skema);
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("param1", suratoutboxid));
                                        aParams.Add(new OracleParameter("param2", suratinboxid));
                                        aParams.Add(new OracleParameter("param3", profileidtu));
                                        aParams.Add(new OracleParameter("param4", "D"));
                                        aParams.Add(new OracleParameter("param5", data.CatatanAnda));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                    }
                                }
                            }
                        }

                        #endregion

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";
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

        public TransactionResult InsertSuratMasuk(Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim, List<SessionTujuanSurat> dataSessionTujuanSurat, List<SessionLampiranSurat> dataSessionLampiran)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };
            string skema = OtorisasiUser.NamaSkema;

            string sql = string.Empty;
            string query = string.Empty;
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = mDataMaster.GetSatkerId(unitkerjaid);
                        int tipekantorid = mDataMaster.GetTipeKantor(kantorid);
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        data.SuratId = string.IsNullOrEmpty(data.SuratId) ? GetUID() : data.SuratId;
                        data.Arah = "Masuk";
                        data.PenerimaSurat = data.PenerimaSurat.Length > 500? data.PenerimaSurat.Substring(0,500): data.PenerimaSurat; // ambil dari daftar session tujuan surat (asli)
                        int _ctRecord = 0;
                        decimal _ctAgenda = 1;
                        string _nmrAgenda = string.Empty;
                        string _strBulan = Functions.NomorRomawi(Convert.ToDateTime(GetServerDate(), theCultureInfo).Month);
                        string _kodeAgenda = "AG-";

                        //#region Buat Nomor Agenda

                        //// Cek Konter Agenda
                        //string query = string.Format(@"
                        //    SELECT COUNT(1)
                        //    FROM {0}.KONTERSURAT
                        //    WHERE
                        //      KANTORID = :param1 AND
                        //      TAHUN = :param2 AND
                        //      TIPESURAT = :param3", skema);
                        //aParams.Clear();
                        //aParams.Add(new OracleParameter("param1", satkerid));
                        //aParams.Add(new OracleParameter("param2", tahun));
                        //aParams.Add(new OracleParameter("param3", "Agenda"));
                        //int jumlahrecord = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();
                        //if (jumlahrecord == 0)
                        //{
                        //    query = string.Format(@"
                        //        INSERT INTO {0}.KONTERSURAT (KONTERSURATID, KANTORID, TIPESURAT, TAHUN, NILAIKONTER)
                        //        VALUES (SYS_GUID(), :param1, :param2, :param3, 0)", skema);
                        //    aParams.Clear();
                        //    aParams.Add(new OracleParameter("param1", satkerid));
                        //    aParams.Add(new OracleParameter("param2", "Agenda"));
                        //    aParams.Add(new OracleParameter("param3", tahun));
                        //    ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());
                        //}

                        //decimal nilainomoragenda = 1;

                        //query = string.Format(@"
                        //    SELECT NILAIKONTER+1
                        //    FROM {0}.KONTERSURAT
                        //    WHERE
                        //      KANTORID = :param1 AND
                        //      TAHUN = :param2 AND
                        //      TIPESURAT = :param3", skema);
                        //aParams.Clear();
                        //aParams.Add(new OracleParameter("param1", satkerid));
                        //aParams.Add(new OracleParameter("param2", tahun));
                        //aParams.Add(new OracleParameter("param3", "Agenda"));

                        //nilainomoragenda = ctx.Database.SqlQuery<decimal>(query, aParams.ToArray()).FirstOrDefault();

                        //sql = string.Format("UPDATE {0}.KONTERSURAT SET NILAIKONTER = :NilaiKonter WHERE KANTORID = :SatkerId AND TAHUN = :Tahun AND TIPESURAT = :Tipe", skema);
                        //aParams.Clear();
                        //aParams.Add(new OracleParameter("NilaiKonter", nilainomoragenda));
                        //aParams.Add(new OracleParameter("SatkerId", satkerid));
                        //aParams.Add(new OracleParameter("Tahun", tahun));
                        //aParams.Add(new OracleParameter("Tipe", "Agenda"));
                        //ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        //// Binding Nomor Agenda
                        //int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                        //string strBulan = Functions.NomorRomawi(bulan);
                        //string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
                        //string kodesurat = "AG-";

                        //string strNomorAgenda = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());
                        //data.NomorAgenda = strNomorAgenda;
                        //#endregion

                        //#region Set Metadata
                        //string metadata = "";
                        //metadata += data.NomorSurat + " ";
                        //metadata += data.NomorAgenda + " ";
                        //metadata += data.TanggalSurat + " ";
                        //metadata += data.Perihal + " ";
                        //metadata += data.PengirimSurat + " ";
                        //metadata += data.PenerimaSurat + " ";
                        //metadata += data.IsiSingkatSurat + " ";
                        //metadata += data.NamaPenerima + " ";
                        //metadata += data.UserId + " ";
                        //metadata += data.TipeSurat + " ";
                        //metadata += data.SifatSurat + " ";
                        //metadata += data.Kategori + " ";
                        //metadata += data.KeteranganSurat + " ";
                        //metadata += data.Sumber_Keterangan + " ";
                        //metadata = metadata.Trim();
                        //#endregion

                        // Insert SURAT
                        sql = string.Format( @"
                            INSERT INTO {0}.surat (
                                   suratid, kantorid, tanggalsurat, tanggalproses, tanggalundangan, nomorsurat, perihal, pengirim, penerima, arah, kategori,
                                   tipesurat, sifatsurat, keterangansurat, jumlahlampiran, isisingkat, tipekegiatan) VALUES
                            (
                                   :Id, :SatkerId, TO_DATE(:TanggalSurat,'DD/MM/YYYY'), TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'), TO_DATE(:TanggalUndangan,'DD/MM/YYYY HH24:MI'),
                                   :NomorSurat, :Perihal, :PengirimSurat, :PenerimaSurat, :Arah, 'Masuk', 
                                   :TipeSurat, :SifatSurat, :KeteranganSurat, :JumlahLampiran, :IsiSingkatSurat, :KodeKlasifikasi)", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("Id", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("TanggalSurat", data.TanggalSurat));
                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                        aParams.Add(new OracleParameter("TanggalUndangan", data.TanggalUndangan));
                        aParams.Add(new OracleParameter("NomorSurat", data.NomorSurat));
                        aParams.Add(new OracleParameter("Perihal", data.Perihal));
                        aParams.Add(new OracleParameter("PengirimSurat", data.PengirimSurat));
                        aParams.Add(new OracleParameter("PenerimaSurat", data.PenerimaSurat));
                        aParams.Add(new OracleParameter("Arah", data.Arah));
                        aParams.Add(new OracleParameter("TipeSurat", data.TipeSurat));
                        aParams.Add(new OracleParameter("SifatSurat", data.SifatSurat));
                        aParams.Add(new OracleParameter("KeteranganSurat", data.KeteranganSurat));
                        aParams.Add(new OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        aParams.Add(new OracleParameter("IsiSingkatSurat", data.IsiSingkatSurat));
                        aParams.Add(new OracleParameter("KodeKlasifikasi", data.KodeKlasifikasi));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        //#region AGENDA SURAT

                        //// Insert AGENDASURAT
                        //sql =
                        //    "INSERT INTO " + OtorisasiUser.NamaSkema + ".agendasurat ( " +
                        //    "       agendasuratid, suratid, nomoragenda, kantorid) VALUES " +
                        //    "( " +
                        //    "       SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
                        //aParams.Clear();
                        //aParams.Add(new OracleParameter("Id", data.SuratId));
                        //aParams.Add(new OracleParameter("NomorAgenda", data.NomorAgenda));
                        //aParams.Add(new OracleParameter("SatkerId", satkerid));
                        //ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        //#endregion

                        // Insert LAMPIRAN SURAT
                        foreach (SessionLampiranSurat lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".lampiransurat ( " +
                                    "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                                    "( " +
                                    "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip)";
                                aParams.Clear();
                                aParams.Add(new OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                aParams.Add(new OracleParameter("FolderFile", folderfile));
                                aParams.Add(new OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("KantorId", kantorid));
                                aParams.Add(new OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                aParams.Add(new OracleParameter("Nip", lampiranSurat.Nip));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            }
                        }

                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql = 
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("Nip", nip));
                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        string suratinboxid = GetUID();
                        int urutan = 0;

                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "       0,0,1)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                        aParams.Add(new OracleParameter("Nip", nip)); // nip Pengirim
                        aParams.Add(new OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                        aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                        aParams.Add(new OracleParameter("Redaksi", ""));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #endregion

                        #region Session Tujuan Surat

                        bool belumkirimtuba = true;

                        bool iskirimtuba = false;

                        int statusterkunci = 0;

                        foreach (SessionTujuanSurat tujuanSurat in dataSessionTujuanSurat)
                        {
                            var pegawaiTujuan = mDataMaster.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            if (tujuanSurat.Redaksi == "Asli")
                            {
                                data.PenerimaSurat = tujuanSurat.NamaJabatan;
                            }

                            // Get SatkerId Tujuan. Bila Pusat dari UnitkerjaId, bila kanwil/kantah dari KantorId

                            string satkeridtujuan = string.Empty;
                            string unitkerjaTujuan = ctx.Database.SqlQuery<string>($"SELECT UNITKERJAID FROM JABATAN WHERE PROFILEID = '{tujuanSurat.ProfileId}'").First();

                            if (tipekantorid == 1)
                            {
                                satkeridtujuan = unitkerjaTujuan;
                            }
                            else
                            {
                                satkeridtujuan = mDataMaster.GetKantorIdByUnitKerjaId(unitkerjaTujuan);
                            }

                            // GET JABATAN DAN PEGAWAI TUJUAN

                            // Cek Delegasi Surat ------------------------
                            string profileidtujuan = tujuanSurat.ProfileId;
                            string niptujuan = tujuanSurat.NIP;
                            string namapegawaitujuan = tujuanSurat.NamaPegawai;

                            var delegasiSurat = GetDelegasiSurat(tujuanSurat.ProfileId);
                            if (delegasiSurat != null)
                            {
                                profileidtujuan = delegasiSurat.ProfilePenerima;
                                niptujuan = delegasiSurat.NIPPenerima;
                                namapegawaitujuan = delegasiSurat.NamaPenerima;
                            }
                            // Eof Cek Delegasi Surat --------------------

                            // Eof // GET JABATAN DAN PEGAWAI TUJUAN------

                            int statusterkunciTujuan = 1;

                            #region Kirim ke TU Persuratan (Pusat)

                            if (belumkirimtuba)
                            {
                                string profileidba = mDataMaster.GetProfileIdBAFromProfileId(myprofileidtu);

                                if (!string.IsNullOrEmpty(profileidba))
                                {
                                    statusterkunci = 1;

                                    string pegawaiidba = mDataMaster.GetPegawaiIdFromProfileId(profileidba, true);
                                    if (string.IsNullOrEmpty(pegawaiidba))
                                    {
                                        pegawaiidba = mDataMaster.GetPegawaiIdFromProfileId(profileidba);
                                    }

                                    var pegawaiBA = mDataMaster.GetPegawaiByPegawaiId(pegawaiidba);
                                    if (pegawaiBA == null)
                                    {
                                        string namaprofile = mDataMaster.GetProfileNameFromId(profileidba);
                                        if (string.IsNullOrEmpty(namaprofile))
                                        {
                                            namaprofile = profileidba;
                                        }
                                        tc.Rollback();
                                        tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                        tc.Dispose();
                                        ctx.Dispose();

                                        return tr;
                                    }

                                    #region Cek Duplikasi

                                    bool BisaKirimSurat = true;

                                    sql =
                                        "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                        "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                        "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                    aParams.Add(new OracleParameter("SatkerId", satkerid));
                                    aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                    aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                    int jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                    if (jumlahinbox > 0)
                                    {
                                        BisaKirimSurat = false;
                                    }

                                    #endregion

                                    if (pegawaiidba == nip) // Arya :: 2020-08-07 :: Cek Pembuat Surat Masuk, bila MailRoom maka langsung ke TU/Tujuan.  
                                    {
                                        BisaKirimSurat = false;
                                        iskirimtuba = true;
                                        statusterkunci = 0;
                                    }

                                    if (BisaKirimSurat)
                                    {
                                        // Insert SURATINBOX
                                        suratinboxid = GetUID();
                                        urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                        sql =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                            "( " +
                                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                            "       0,1,:Urutan)";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                        aParams.Add(new OracleParameter("Nip", pegawaiidba)); // nip TU Biro
                                        aParams.Add(new OracleParameter("NamaPegawai", pegawaiBA.NamaLengkap)); // nama TU Biro
                                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                        aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                        aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                        aParams.Add(new OracleParameter("Urutan", urutan));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                        try
                                        {
                                            new Mobile().KirimNotifikasi(pegawaiidba, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.IsiSingkatSurat) ? "Persetujuan Baru" : data.IsiSingkatSurat.ToString()), "Persetujuan");
                                        }
                                        catch (Exception ex)
                                        {
                                            var str = ex.Message;
                                        }

                                        // Insert SURATOUTBOXRELASI
                                        sql =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                            "( " +
                                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidba));
                                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                        iskirimtuba = true;
                                    }
                                }

                                belumkirimtuba = false;
                            }

                            #endregion


                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            string profileidtu = mDataMaster.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);
                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                // Cek Apakah dari MailRoom (Pembuat Surat), langsung dikirim ke Pejabat Tujuan,
                                // atau dikirim ke TU, dimana nanti TU yang akan mendistribusikan (alias Kirim seperti Biasa)
                                // Kalau dikirim ke TU, tidak usah buat Persetujuan TU, tapi kirim Inbox TU biasa

                                bool JadiKirimkeTU = true;

                                if (profileidtu == profileidtujuan)
                                {
                                    // Bila tujuan surat ke TU, tidak usah buat Inbox Persetujuan, buat inbox biasa ke TU
                                    JadiKirimkeTU = false;
                                    statusterkunciTujuan = 0;
                                }

                                if (profileidtu == myprofileid) // Arya :: 2020-08-07 :: Cek Pengirim Surat, Bila TU dari tujuan surat maka tidak usah buat Inbox Persetujuan, buat inbox biasa ke Tujuan
                                {
                                    JadiKirimkeTU = false;
                                    statusterkunciTujuan = 0;
                                }

                                if (JadiKirimkeTU)
                                {
                                    if (profileidtu != myprofileidtu)
                                    {
                                        // bila dikirim ke TU Pengolah Kantor Pusat


                                        // Cek Delegasi Surat ------------------------
                                        var delegasiSuratTU = GetDelegasiSurat(profileidtu);
                                        if (delegasiSuratTU != null)
                                        {
                                            profileidtu = delegasiSuratTU.ProfilePenerima;
                                        }
                                        // Eof Cek Delegasi Surat --------------------


                                        string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                        if (string.IsNullOrEmpty(pegawaiidtu))
                                        {
                                            pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                        }
                                        var pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                        if (pegawaiTU == null)
                                        {
                                            string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidtu;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }


                                        #region Cek Duplikasi

                                        bool BisaKirimKeTUPengolah = true;

                                        query =
                                            "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                        aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                        aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimKeTUPengolah = false;
                                        }

                                        #endregion


                                        if (BisaKirimKeTUPengolah)
                                        {
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                            sql =
                                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "       :StatusTerkunci,1,:Urutan)";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                            aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                            aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                            aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                            aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                            aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                            aParams.Add(new OracleParameter("Urutan", urutan));
                                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                            // Insert SURATOUTBOXRELASI (ke Profile TU)
                                            sql =
                                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                            aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            aParams.Add(new OracleParameter("StatusBaca", "D"));
                                            aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                            try
                                            {
                                                new Mobile().KirimNotifikasi(pegawaiidtu, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.IsiSingkatSurat) ? "Persetujuan Baru" : data.IsiSingkatSurat.ToString()), "Persetujuan");
                                            }
                                            catch (Exception ex)
                                            {
                                                var str = ex.Message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!iskirimtuba)
                                        {
                                            // bila dikirim ke TU Kanwil/Kantah

                                            string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                            if (string.IsNullOrEmpty(pegawaiidtu))
                                            {
                                                pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                            }
                                            var pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                            if (pegawaiTU == null)
                                            {
                                                string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                                if (string.IsNullOrEmpty(namaprofile))
                                                {
                                                    namaprofile = profileidtu;
                                                }
                                                tc.Rollback();
                                                tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                                tc.Dispose();
                                                ctx.Dispose();

                                                return tr;
                                            }

                                            #region Cek Duplikasi

                                            bool BisaKirimKeTUPengolah = true;

                                            query =
                                                "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();
                                            if (jumlahinbox > 0)
                                            {
                                                BisaKirimKeTUPengolah = false;
                                            }

                                            #endregion

                                            if (BisaKirimKeTUPengolah)
                                            {
                                                suratinboxid = GetUID();
                                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                    "( " +
                                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                    "       0,1,:Urutan)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                                aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                                aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                                aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                                aParams.Add(new OracleParameter("Urutan", urutan));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                                                // Insert SURATOUTBOXRELASI (ke Profile TU)
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                    "    suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                    "( " +
                                                    "    :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                            }
                                        }
                                        else
                                        {
                                            // Bila Surat dikirim ke Biro Umum, dimana TU nya ada TU Mail-Room (Kantor Pusat)
                                            string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                            if (string.IsNullOrEmpty(pegawaiidtu))
                                            {
                                                pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                            }
                                            Models.Entities.Pegawai pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                            if (pegawaiTU == null)
                                            {
                                                string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                                if (string.IsNullOrEmpty(namaprofile))
                                                {
                                                    namaprofile = profileidtu;
                                                }
                                                tc.Rollback();
                                                tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                                tc.Dispose();
                                                ctx.Dispose();

                                                return tr;
                                            }

                                            #region Cek Duplikasi

                                            bool BisaKirimKeTUPengolah = true;

                                            query =
                                                "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).First();
                                            if (jumlahinbox > 0)
                                            {
                                                BisaKirimKeTUPengolah = false;
                                            }

                                            #endregion

                                            if (BisaKirimKeTUPengolah)
                                            {
                                                suratinboxid = GetUID();
                                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                    "( " +
                                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                    "       :StatusTerkunci,1,:Urutan)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                                aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                                aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                                aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                                aParams.Add(new OracleParameter("Urutan", urutan));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                                // Insert SURATOUTBOXRELASI (ke Profile TU)
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                    "( " +
                                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                            }
                                        }
                                    }
                                } // End of if (JadiKirimkeTU) ------
                            }

                            #endregion

                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            // detail Jabatan dan Pegawai Tujuan sudah disetting di atas, di awal Loop

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql =
                                "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 1 AND statusterkirim = 0";
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                            aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtujuan));
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :StatusTerkunciTujuan,0,:Urutan)";
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtujuan));
                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                aParams.Add(new OracleParameter("Nip", niptujuan)); // nip penerima surat
                                aParams.Add(new OracleParameter("NamaPegawai", namapegawaitujuan)); // nama penerima surat
                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                aParams.Add(new OracleParameter("StatusTerkunciTujuan", statusterkunciTujuan));
                                aParams.Add(new OracleParameter("Urutan", urutan));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                #region AGENDA SURAT DI TUJUAN SURAT

                                // Bila Satker Pengirim tidak sama dengan Satker Tujuan Surat
                                if (satkerid != satkeridtujuan)
                                {
                                    query = "select count(*) from " + OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                    aParams.Add(new OracleParameter("Tahun", tahun));
                                    aParams.Add(new OracleParameter("Tipe", "Agenda"));
                                    if (ctx.Database.SqlQuery<int>(query, aParams.ToArray()).FirstOrDefault() == 0)
                                    {
                                        // Bila tidak ada, Insert KONTERSURAT
                                        query =
                                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".kontersurat ( " +
                                            "       kontersuratid, kantorid, tipesurat, tahun, nilaikonter) VALUES " +
                                            "( " +
                                            "       SYS_GUID(), :SatkerId, :TipeSurat, :Tahun, 0)";
                                        //query = sWhitespace.Replace(query, " ");
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                        aParams.Add(new OracleParameter("TipeSurat", "Agenda"));
                                        aParams.Add(new OracleParameter("Tahun", tahun));
                                        ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());
                                    }

                                    query =
                                        "select nilaikonter+1 from " + OtorisasiUser.NamaSkema + ".kontersurat where kantorid = :SatkerId and tahun = :Tahun AND tipesurat = :Tipe " +
                                        "FOR UPDATE NOWAIT";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                    aParams.Add(new OracleParameter("Tahun", tahun));
                                    aParams.Add(new OracleParameter("Tipe", "Agenda"));

                                    _ctAgenda = ctx.Database.SqlQuery<decimal>(query, aParams.ToArray()).FirstOrDefault();

                                    sql = "UPDATE " + OtorisasiUser.NamaSkema + ".kontersurat SET nilaikonter = :NilaiKonter WHERE kantorid = :SatkerId AND tahun = :Tahun AND tipesurat = :Tipe";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("NilaiKonter", _ctAgenda));
                                    aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                    aParams.Add(new OracleParameter("Tahun", tahun));
                                    aParams.Add(new OracleParameter("Tipe", "Agenda"));
                                    ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                    // Binding Nomor Agenda
                                    //int bulan = Convert.ToDateTime(GetServerDate(), theCultureInfo).Month;
                                    //string strBulan = Functions.NomorRomawi(bulan);
                                    //string kodesurat = "AG-";

                                    _nmrAgenda = string.Concat(_ctAgenda.ToString(), "/", _kodeAgenda, GetKodeIdentifikasi(tujuanSurat.UnitKerjaId), "/", _strBulan, "/", tahun.ToString());

                                    // Insert AGENDASURAT
                                    sql =
                                        "INSERT INTO " + OtorisasiUser.NamaSkema + ".agendasurat ( " +
                                        "       agendasuratid, suratid, nomoragenda, kantorid) VALUES " +
                                        "( " +
                                        "       SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
                                    aParams.Clear();
                                    aParams.Add(new OracleParameter("Id", data.SuratId));
                                    aParams.Add(new OracleParameter("NomorAgenda", _nmrAgenda));
                                    aParams.Add(new OracleParameter("SatkerId", satkeridtujuan));
                                    ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                }
                                #endregion
                            }
                            #endregion
                        }
                        #endregion

                        #region Buat Nomor Agenda

                        // Cek Konter Agenda
                        query = string.Format(@"
                            SELECT COUNT(1)
                            FROM {0}.KONTERSURAT
                            WHERE
                              KANTORID = :param1 AND
                              TAHUN = :param2 AND
                              TIPESURAT = :param3", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", satkerid));
                        aParams.Add(new OracleParameter("param2", tahun));
                        aParams.Add(new OracleParameter("param3", "Agenda"));
                        _ctRecord = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).FirstOrDefault();
                        if (_ctRecord == 0)
                        {
                            query = string.Format(@"
                                INSERT INTO {0}.KONTERSURAT (KONTERSURATID, KANTORID, TIPESURAT, TAHUN, NILAIKONTER)
                                VALUES (SYS_GUID(), :param1, :param2, :param3, 0)", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", satkerid));
                            aParams.Add(new OracleParameter("param2", "Agenda"));
                            aParams.Add(new OracleParameter("param3", tahun));
                            ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());
                        }

                        query = string.Format(@"
                            SELECT NILAIKONTER+1
                            FROM {0}.KONTERSURAT
                            WHERE
                              KANTORID = :param1 AND
                              TAHUN = :param2 AND
                              TIPESURAT = :param3", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", satkerid));
                        aParams.Add(new OracleParameter("param2", tahun));
                        aParams.Add(new OracleParameter("param3", "Agenda"));

                        _ctAgenda = ctx.Database.SqlQuery<decimal>(query, aParams.ToArray()).FirstOrDefault();

                        sql = string.Format("UPDATE {0}.KONTERSURAT SET NILAIKONTER = :NilaiKonter WHERE KANTORID = :SatkerId AND TAHUN = :Tahun AND TIPESURAT = :Tipe", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("NilaiKonter", _ctAgenda));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("Tahun", tahun));
                        aParams.Add(new OracleParameter("Tipe", "Agenda"));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        // Binding Nomor Agenda
                        data.NomorAgenda = string.Concat(_ctAgenda.ToString(), "/", _kodeAgenda, GetKodeIdentifikasi(unitkerjaid), "/", _strBulan, "/", tahun.ToString());
                        #endregion

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.NomorAgenda + " ";
                        metadata += data.TanggalSurat + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.PenerimaSurat + " ";
                        metadata += data.IsiSingkatSurat + " ";
                        metadata += data.NamaPenerima + " ";
                        metadata += data.UserId + " ";
                        metadata += data.TipeSurat + " ";
                        metadata += data.SifatSurat + " ";
                        metadata += data.Kategori + " ";
                        metadata += data.KeteranganSurat + " ";
                        metadata += data.Sumber_Keterangan + " ";
                        metadata = metadata.Trim();
                        #endregion

                        // Insert SURAT
                        sql = string.Format(@"UPDATE {0}.SURAT SET NOMORAGENDA = :param1, METADATA = utl_raw.cast_to_raw(:param2) WHERE SURATID = :param3", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", data.NomorAgenda));
                        aParams.Add(new OracleParameter("param2", metadata));
                        aParams.Add(new OracleParameter("param3", data.SuratId));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #region AGENDA SURAT

                        // Insert AGENDASURAT
                        sql =
                            "INSERT INTO " + OtorisasiUser.NamaSkema + ".agendasurat ( " +
                            "       agendasuratid, suratid, nomoragenda, kantorid) VALUES " +
                            "( " +
                            "       SYS_GUID(), :Id, :NomorAgenda, :SatkerId)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("Id", data.SuratId));
                        aParams.Add(new OracleParameter("NomorAgenda", data.NomorAgenda));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #endregion

                        sql = $@"Insert into {skema}.Sumber_surat (sumber_id, surat_id, sumber_keterangan) values (:param1, :param2, :param3)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", GetUID()));
                        aParams.Add(new OracleParameter("param2", data.SuratId));
                        aParams.Add(new OracleParameter("param3", data.Sumber_Keterangan));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.NomorAgenda;
                        tr.ReturnValue2 = data.NomorSurat;
                        tr.Pesan = "Surat berhasil dikirim";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT") || tr.Pesan.ToUpper().Contains("I2_SURAT"))
                        {
                            tr.Pesan = string.Concat("Nomor Surat ", data.NomorSurat, " dari ", data.PengirimSurat, " sudah ada.");
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

        public TransactionResult InsertSuratInisiatif(Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim, List<SessionTujuanSurat> dataSessionTujuanSurat, List<SessionLampiranSurat> dataSessionLampiran)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };
            string skema = OtorisasiUser.NamaSkema;

            string sql = string.Empty;
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = mDataMaster.GetSatkerId(unitkerjaid);
                        int tipekantorid = mDataMaster.GetTipeKantor(kantorid);

                        string id = GetUID();
                        int tahun = ctx.Database.SqlQuery<int>("SELECT EXTRACT (YEAR FROM SYSDATE) FROM DUAL").FirstOrDefault<int>();

                        data.SuratId = id;
                        data.Arah = "Inisiatif";
                        data.PengirimSurat = data.NamaPengirim;

                        #region Set Metadata
                        string metadata = "";
                        metadata += data.NomorSurat + " ";
                        metadata += data.TargetSelesai + " ";
                        metadata += data.Perihal + " ";
                        metadata += data.PengirimSurat + " ";
                        metadata += data.UserId + " ";
                        metadata += data.CatatanAnda + " ";
                        metadata += data.Kategori + " ";
                        foreach(var _tujuan in dataSessionTujuanSurat)
                        {
                            metadata += string.Concat(_tujuan.NIP,_tujuan.NamaPegawai," ");
                        }
                        metadata = metadata.Trim();
                        #endregion

                        // Insert SURAT
                        sql = string.Format(@"
                                INSERT INTO {0}.surat(
                                      suratid, kantorid, targetselesai, perihal, pengirim, arah, kategori, 
                                      jumlahlampiran, isisingkat, metadata) VALUES
                                   ( :Id, :SatkerId, TO_DATE(:TargetSelesai,'DD/MM/YYYY'), :Perihal, :PengirimSurat, :Arah, :Kategori,
                                     :JumlahLampiran, :IsiSingkatSurat, utl_raw.cast_to_raw(:Metadata))",skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("Id", id));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("TargetSelesai", data.TargetSelesai));
                        aParams.Add(new OracleParameter("Perihal", data.Perihal));
                        aParams.Add(new OracleParameter("PengirimSurat", data.PengirimSurat));
                        aParams.Add(new OracleParameter("Arah", data.Arah));
                        aParams.Add(new OracleParameter("Kategori", data.ArahSuratKeluar));
                        aParams.Add(new OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        aParams.Add(new OracleParameter("IsiSingkatSurat", data.CatatanAnda));
                        aParams.Add(new OracleParameter("Metadata", metadata));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        // Insert LAMPIRAN SURAT
                        foreach (var lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql = string.Format(@"
                                    INSERT INTO {0}.LAMPIRANSURAT (
                                           LAMPIRANSURATID, SURATID, PATH, NAMAFILE, PROFILEID, KANTORID, KETERANGAN, NIP, EXT) VALUES 
                                    ( 
                                           :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:KantorId,:Keterangan,:Nip,:Ext)",skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                aParams.Add(new OracleParameter("SuratId", id));
                                aParams.Add(new OracleParameter("FolderFile", folderfile));
                                aParams.Add(new OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("KantorId", kantorid));
                                aParams.Add(new OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                aParams.Add(new OracleParameter("Nip", lampiranSurat.Nip));
                                aParams.Add(new OracleParameter("Ext", lampiranSurat.Ext));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            }
                        }

                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql = string.Format(@"
                            INSERT INTO {0}.suratoutbox (
                                SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES
                            ( 
                                :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)",skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratId", id));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("Nip", data.UserId));
                        aParams.Add(new OracleParameter("CatatanAnda", data.CatatanAnda));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        string suratinboxid = "";
                        int urutan = 0;

                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql = string.Format(@"
                            INSERT INTO {0}.suratinbox ( 
                                        SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM,
                                        NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, 
                                        statusterkunci, statusforwardtu, urutan) VALUES 
                            ( 
                                        :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, 
                                        :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, 
                                        0,0,1)",skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("SuratId", id));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                        aParams.Add(new OracleParameter("Nip", nip)); // nip Pengirim
                        aParams.Add(new OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                        aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                        aParams.Add(new OracleParameter("Redaksi", ""));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());


                        // Insert SURATOUTBOXRELASI
                        sql = string.Format(@"
                            INSERT INTO {0}.suratoutboxrelasi ( 
                                        suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES
                            ( 
                                        :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)",skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                        aParams.Add(new OracleParameter("CatatanAnda", data.CatatanAnda));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #endregion

                        #region Session Tujuan Surat

                        foreach (var tujuanSurat in dataSessionTujuanSurat)
                        {
                            var pegawaiTujuan = mDataMaster.GetPegawaiByPegawaiId(tujuanSurat.NIP);

                            string profileidtu = mDataMaster.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);

                            #region Kirim ke Tujuan Surat

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql = string.Format(@"
                                SELECT count(*) FROM {0}.suratinbox
                                WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim
                                AND profilepenerima = :ProfileIdPenerima AND statusterkunci = 0 AND statusterkirim = 0",skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SuratId", id));
                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, id);
                                sql = string.Format(@"
                                    INSERT INTO {0}.suratinbox ( 
                                           SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, 
                                           NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi,
                                           statusterkunci, statusforwardtu, statusurgent, urutan) VALUES
                                    ( 
                                           :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, 
                                           :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi,
                                           0,0,:StatusUrgent,:Urutan)",skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("SuratId", id));
                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                aParams.Add(new OracleParameter("Nip", tujuanSurat.NIP)); // nip penerima surat
                                aParams.Add(new OracleParameter("NamaPegawai", tujuanSurat.NamaPegawai)); // nama penerima surat
                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                aParams.Add(new OracleParameter("StatusUrgent", tujuanSurat.StatusUrgent));
                                aParams.Add(new OracleParameter("Urutan", urutan));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                // Insert SURATOUTBOXRELASI
                                sql = string.Format(@"
                                    INSERT INTO {0}.suratoutboxrelasi (
                                           suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES 
                                    ( 
                                           :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)",skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                try
                                {
                                    new Mobile().KirimNotifikasi(tujuanSurat.NIP, "asn", data.NamaPengirim, string.Concat("", string.IsNullOrEmpty(data.IsiSingkatSurat) ? "Surat Inisiatif Baru" : data.IsiSingkatSurat.ToString()), "Surat Inisiatif");
                                }
                                catch (Exception ex)
                                {
                                    var str = ex.Message;
                                }
                            }
                            #endregion
                        }
                        #endregion

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.SuratId;
                        tr.Pesan = "Surat berhasil dikirim";

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

        public string GetKodeIdentifikasi(string unitkerjaid)
        {
            string result = "";

            if (!string.IsNullOrEmpty(unitkerjaid))
            {
                string query = "SELECT kode FROM unitkerja WHERE unitkerjaid = :UnitKerjaId";

                var aParams = new ArrayList();
                aParams.Add(new OracleParameter("UnitKerjaId", unitkerjaid));

                using (var ctx = new BpnDbContext())
                {
                    result = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).FirstOrDefault();
                }
            }

            return result;
        }

        public DelegasiSurat GetDelegasiSurat(string profilepengirim)
        {
            var data = new DelegasiSurat();

            var aParams = new ArrayList();

            string query = string.Format(@"
                SELECT
                  DS.DELEGASISURATID, DS.PROFILEPENGIRIM, DS.PROFILEPENERIMA,
                  TO_CHAR(DS.TANGGAL, 'dd/mm/yyyy') AS tanggal,
                  DS.STATUS,
                  PG.PEGAWAIID AS NIPPenerima,
                  NVL(PG.NAMAALIAS, PG.NAMA) NamaPenerima
                FROM {0}.DELEGASISURAT DS
                  JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = DS.PROFILEPENERIMA AND
                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
                    NVL(JP.STATUSHAPUS,'0') = '0'
                  JOIN PEGAWAI PG ON
                    PG.PEGAWAIID = JP.PEGAWAIID
                WHERE
                  DS.PROFILEPENGIRIM = :ProfilePengirim AND
                  DS.STATUS = 1", OtorisasiUser.NamaSkema);

            aParams.Add(new OracleParameter("ProfilePengirim", profilepengirim));

            using (var ctx = new BpnDbContext())
            {
                data = ctx.Database.SqlQuery<DelegasiSurat>(query, aParams.ToArray()).FirstOrDefault();
            }

            return data;
        }

        public string GetSuratIdFromNomorSuratDanPengirim(string nomorsurat, string pengirim)
        {
            string result = "";
            var aParams = new ArrayList();

            string query =
                 "SELECT suratid " +
                 " FROM " + OtorisasiUser.NamaSkema + ".surat " +
                 " WHERE UPPER(nomorsurat) = :NomorSurat AND UPPER(pengirim) = :PengirimSurat";

            aParams.Clear();
            aParams.Add(new OracleParameter("NomorSurat", nomorsurat.ToUpper()));
            aParams.Add(new OracleParameter("PengirimSurat", pengirim.ToUpper()));

            using (var ctx = new BpnDbContext())
            {
                result = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).FirstOrDefault();
            }

            return result;
        }

        public TransactionResult MergeSuratMasuk(Entities.Surat data, string kantorid, string unitkerjaid, string myprofileid, string myprofileidtu, string nip, string namapegawaipengirim, List<SessionTujuanSurat> dataSessionTujuanSurat, List<SessionLampiranSurat> dataSessionLampiran)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string skema = OtorisasiUser.NamaSkema;

            string sql = "";
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string satkerid = kantorid;
                        int tipekantorid = mDataMaster.GetTipeKantor(kantorid);
                        if (tipekantorid == 1)
                        {
                            satkerid = unitkerjaid;
                        }

                        // Insert LAMPIRAN SURAT
                        foreach (var lampiranSurat in dataSessionLampiran)
                        {
                            if (lampiranSurat.ObjectFile.Length > 0)
                            {
                                string folderfile = "-";

                                sql =
                                    "INSERT INTO " + skema + ".lampiransurat ( " +
                                    "       lampiransuratid, suratid, path, namafile, profileid, KANTORID, KETERANGAN, nip) VALUES " +
                                    "( " +
                                    "       :LampiranSuratId,:SuratId,:FolderFile,:NamaFile,:ProfileIdPengirim,:SatkerId,:Keterangan,:Nip)";
                                aParams.Clear();
                                aParams.Add(new OracleParameter("LampiranSuratId", lampiranSurat.LampiranSuratId));
                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                aParams.Add(new OracleParameter("FolderFile", folderfile));
                                aParams.Add(new OracleParameter("NamaFile", lampiranSurat.NamaFile));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                aParams.Add(new OracleParameter("Keterangan", lampiranSurat.NamaFile));
                                aParams.Add(new OracleParameter("Nip", lampiranSurat.Nip));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            }
                        }

                        // Insert SURATOUTBOX
                        string suratoutboxid = GetUID();
                        sql =
                            "INSERT INTO " + skema + ".suratoutbox ( " +
                            "       SURATOUTBOXID, SURATID, KANTORID, PROFILEPENGIRIM, NIP, TANGGALKIRIM, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:Nip,SYSDATE,:CatatanAnda)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("Nip", data.UserId));
                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        string suratinboxid = "";
                        int urutan = 0;

                        #region Insert SURATINBOX dari Pengirim Pertama

                        string[] arrProfileId = myprofileid.Split(",".ToCharArray());
                        if (arrProfileId.Length > 0)
                        {
                            myprofileid = arrProfileId[0];
                        }

                        suratinboxid = GetUID();
                        sql =
                            "INSERT INTO " + skema + ".suratinbox ( " +
                            "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                            "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                            "       statusterkunci, statusforwardtu, urutan) VALUES " +
                            "( " +
                            "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                            "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,1,:Keterangan,:Redaksi, " +
                            "       0,0,1)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                        aParams.Add(new OracleParameter("Nip", nip)); // nip Pengirim
                        aParams.Add(new OracleParameter("NamaPegawai", namapegawaipengirim)); // nama Pegawai Pembuat Surat
                        aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                        aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                        aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                        aParams.Add(new OracleParameter("Redaksi", ""));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        // Insert SURATOUTBOXRELASI
                        sql =
                            "INSERT INTO " + skema + ".suratoutboxrelasi ( " +
                            "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                            "( " +
                            "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                        aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                        aParams.Add(new OracleParameter("ProfileIdPenerima", myprofileid));
                        aParams.Add(new OracleParameter("StatusBaca", "D"));
                        aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        #endregion

                        #region Session Tujuan Surat

                        //bool belumkirimtuba = true;

                        bool iskirimtuba = false; // bila merge surat, tidak perlu ke mailroom pusat

                        int statusterkunci = 0;

                        //List<Models.Entities.SessionTujuanSurat> dataSessionTujuanSurat = this.GetListSessionTujuanSurat(myClientId); // data.UserId

                        foreach (var tujuanSurat in dataSessionTujuanSurat)
                        {
                            #region Kirim ke Tujuan Surat, dengan status HOLD (terkunci)

                            // Cek Delegasi Surat ------------------------
                            string profileidtujuan = tujuanSurat.ProfileId;
                            string niptujuan = tujuanSurat.NIP;
                            string namapegawaitujuan = tujuanSurat.NamaPegawai;

                            Entities.DelegasiSurat delegasiSurat = GetDelegasiSurat(tujuanSurat.ProfileId);
                            if (delegasiSurat != null)
                            {
                                profileidtujuan = delegasiSurat.ProfilePenerima;
                                niptujuan = delegasiSurat.NIPPenerima;
                                namapegawaitujuan = delegasiSurat.NamaPenerima;
                            }

                            // Eof Cek Delegasi Surat --------------------
                            #region Kirim ke TU-nya Tujuan Surat, dengan status HOLD (terkunci) / Open (tergantung di Pusat/Daerah)

                            string profileidtu = mDataMaster.GetProfileIdTuFromProfileId(tujuanSurat.ProfileId);
                            if (!string.IsNullOrEmpty(profileidtu))
                            {
                                bool JadiKirimkeTU = true;

                                if (profileidtu == profileidtujuan)
                                {
                                    // Bila tujuan surat ke TU, tidak usah buat Inbox Persetujuan, buat inbox biasa ke TU
                                    JadiKirimkeTU = false;
                                    statusterkunci = 0;
                                }

                                if (profileidtu == myprofileid) // Arya :: 2020-08-07 :: Cek Pengirim Surat, Bila TU dari tujuan surat maka tidak usah buat Inbox Persetujuan, buat inbox biasa ke Tujuan
                                {
                                    JadiKirimkeTU = false;
                                    statusterkunci = 0;
                                }

                                if (JadiKirimkeTU)
                                {
                                    if (profileidtu != myprofileidtu)
                                    {
                                        string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                        if (string.IsNullOrEmpty(pegawaiidtu))
                                        {
                                            pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                        }
                                        Models.Entities.Pegawai pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                        if (pegawaiTU == null)
                                        {
                                            string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                            if (string.IsNullOrEmpty(namaprofile))
                                            {
                                                namaprofile = profileidtu;
                                            }
                                            tc.Rollback();
                                            tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                            tc.Dispose();
                                            ctx.Dispose();

                                            return tr;
                                        }

                                        #region Cek Duplikasi

                                        bool BisaKirimKeTUPengolah = true;

                                        sql =
                                            "SELECT count(*) FROM " + skema + ".suratinbox " +
                                            "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                            "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                        aParams.Clear();
                                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                        aParams.Add(new OracleParameter("SatkerId", satkerid));
                                        aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                        aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                        aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                        int jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                        if (jumlahinbox > 0)
                                        {
                                            BisaKirimKeTUPengolah = false;
                                        }

                                        #endregion

                                        if (BisaKirimKeTUPengolah)
                                        {
                                            suratinboxid = GetUID();
                                            urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                            sql =
                                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                "( " +
                                                "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                "       :StatusTerkunci,1,:Urutan)";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                            aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                            aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                            aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                            aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                            aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                            aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                            aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                            aParams.Add(new OracleParameter("Urutan", urutan));
                                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                            // Insert SURATOUTBOXRELASI (ke Profile TU)
                                            sql =
                                                "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                "( " +
                                                "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                            aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            aParams.Add(new OracleParameter("StatusBaca", "D"));
                                            aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                            statusterkunci = 1;
                                        }
                                    }
                                    else
                                    {
                                        if (!iskirimtuba)
                                        {
                                            // bila dikirim ke TU Kanwil/Kantah
                                            string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                            if (string.IsNullOrEmpty(pegawaiidtu))
                                            {
                                                pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                            }
                                            var pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                            if (pegawaiTU == null)
                                            {
                                                string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                                if (string.IsNullOrEmpty(namaprofile))
                                                {
                                                    namaprofile = profileidtu;
                                                }
                                                tc.Rollback();
                                                tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                                tc.Dispose();
                                                ctx.Dispose();

                                                return tr;
                                            }


                                            #region Cek Duplikasi

                                            bool BisaKirimKeTUPengolah = true;

                                            sql =
                                                "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                                "AND profilepenerima = :ProfileIdPenerima AND statusterkirim = 0";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                            if (jumlahinbox > 0)
                                            {
                                                BisaKirimKeTUPengolah = false;
                                            }

                                            #endregion

                                            if (BisaKirimKeTUPengolah)
                                            {
                                                suratinboxid = GetUID();
                                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                    "( " +
                                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                    "       0,1,:Urutan)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                                aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                                aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                                aParams.Add(new OracleParameter("Urutan", urutan));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                                // Insert SURATOUTBOXRELASI (ke Profile TU)
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                    "    suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                    "( " +
                                                    "    :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                                statusterkunci = 1;
                                            }
                                        }
                                        else
                                        {
                                            // Bila Surat dikirim ke Biro Umum, dimana TU nya ada TU Mail-Room (Kantor Pusat)
                                            string pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu, true);
                                            if (string.IsNullOrEmpty(pegawaiidtu))
                                            {
                                                pegawaiidtu = mDataMaster.GetPegawaiIdFromProfileId(profileidtu);
                                            }
                                            var pegawaiTU = mDataMaster.GetPegawaiByPegawaiId(pegawaiidtu);

                                            if (pegawaiTU == null)
                                            {
                                                string namaprofile = mDataMaster.GetProfileNameFromId(profileidtu);
                                                if (string.IsNullOrEmpty(namaprofile))
                                                {
                                                    namaprofile = profileidtu;
                                                }
                                                tc.Rollback();
                                                tr.Pesan = "Data Pegawai untuk jabatan " + namaprofile + " untuk tujuan surat ke " + tujuanSurat.NamaJabatan + " tidak ditemukan.";

                                                tc.Dispose();
                                                ctx.Dispose();

                                                return tr;
                                            }

                                            #region Cek Duplikasi

                                            bool BisaKirimKeTUPengolah = true;

                                            sql =
                                                "SELECT count(*) FROM " + OtorisasiUser.NamaSkema + ".suratinbox " +
                                                "WHERE suratid = :SuratId AND kantorid = :SatkerId AND profilepengirim = :ProfileIdPengirim " +
                                                "AND profilepenerima = :ProfileIdPenerima AND statusterkunci = :StatusTerkunci AND statusterkirim = 0";
                                            aParams.Clear();
                                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                            aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                            int jumlahinbox = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                                            if (jumlahinbox > 0)
                                            {
                                                BisaKirimKeTUPengolah = false;
                                            }

                                            #endregion

                                            if (BisaKirimKeTUPengolah)
                                            {
                                                suratinboxid = GetUID();
                                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                                    "( " +
                                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                                    "       :StatusTerkunci,1,:Urutan)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                                aParams.Add(new OracleParameter("Nip", pegawaiidtu)); // nip TU nya penerima surat
                                                aParams.Add(new OracleParameter("NamaPegawai", pegawaiTU.NamaLengkap)); // nama TU nya penerima surat
                                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                                aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                                aParams.Add(new OracleParameter("Urutan", urutan));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                                // Insert SURATOUTBOXRELASI (ke Profile TU)
                                                sql =
                                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                                    "( " +
                                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                                aParams.Clear();
                                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtu));
                                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                                                statusterkunci = 1;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region Cek Duplikasi

                            bool CanSendLetter = true;

                            sql = string.Format(@"
                                SELECT COUNT(1)
                                FROM {0}.SURATINBOX
                                WHERE
                                  SURATID = :SuratId AND
                                  KANTORID = :SatkerId AND
                                  PROFILEPENGIRIM = :ProfileIdPengirim AND
                                  PROFILEPENERIMA = :ProfileIdPenerima AND
                                  STATUSTERKIRIM = 0 AND
                                  STATUSFORWARDTU = 0", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SuratId", data.SuratId));
                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                            aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                            aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtujuan));
                            int jumlahdata = ctx.Database.SqlQuery<int>(sql, aParams.ToArray()).First();
                            if (jumlahdata > 0)
                            {
                                CanSendLetter = false;
                            }

                            #endregion

                            if (CanSendLetter)
                            {
                                suratinboxid = GetUID();
                                urutan = GetMaxUrutanSuratInbox(ctx, data.SuratId);
                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratinbox ( " +
                                    "       SURATINBOXID, SURATID, KANTORID, PROFILEPENGIRIM, PROFILEPENERIMA, NAMAPENGIRIM, " +
                                    "       NIP, NAMAPEGAWAI, TANGGALKIRIM, TANGGALTERIMA, TINDAKLANJUT, STATUSTERKIRIM, keterangan, redaksi, " +
                                    "       statusterkunci, statusforwardtu, urutan) VALUES " +
                                    "( " +
                                    "       :SuratInboxId,:SuratId,:SatkerId,:ProfileIdPengirim,:ProfileIdPenerima,:NamaPengirim, " +
                                    "       :Nip,:NamaPegawai,SYSDATE, TO_DATE(:TanggalTerima,'DD/MM/YYYY HH24:MI'),:TindakLanjut,0,:Keterangan,:Redaksi, " +
                                    "       :StatusTerkunci,0,:Urutan)";
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("SuratId", data.SuratId));
                                aParams.Add(new OracleParameter("SatkerId", satkerid));
                                aParams.Add(new OracleParameter("ProfileIdPengirim", data.ProfileIdPengirim));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", profileidtujuan));
                                aParams.Add(new OracleParameter("NamaPengirim", data.NamaPengirim));
                                aParams.Add(new OracleParameter("Nip", niptujuan)); // nip penerima surat
                                aParams.Add(new OracleParameter("NamaPegawai", namapegawaitujuan)); // nama penerima surat
                                aParams.Add(new OracleParameter("TanggalTerima", data.TanggalTerima));
                                aParams.Add(new OracleParameter("TindakLanjut", "Ekspedisi"));
                                aParams.Add(new OracleParameter("Keterangan", data.IsiSingkatSurat));
                                aParams.Add(new OracleParameter("Redaksi", tujuanSurat.Redaksi));
                                aParams.Add(new OracleParameter("StatusTerkunci", statusterkunci));
                                aParams.Add(new OracleParameter("Urutan", urutan));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                                // Insert SURATOUTBOXRELASI
                                sql =
                                    "INSERT INTO " + OtorisasiUser.NamaSkema + ".suratoutboxrelasi ( " +
                                    "       suratoutboxid, suratinboxid, profilepenerima, statusbaca, keterangan) VALUES " +
                                    "( " +
                                    "       :SuratOutboxId,:SuratInboxId,:ProfileIdPenerima,:StatusBaca,:CatatanAnda)";
                                //sql = sWhitespace.Replace(sql, " ");
                                aParams.Clear();
                                aParams.Add(new OracleParameter("SuratOutboxId", suratoutboxid));
                                aParams.Add(new OracleParameter("SuratInboxId", suratinboxid));
                                aParams.Add(new OracleParameter("ProfileIdPenerima", tujuanSurat.ProfileId));
                                aParams.Add(new OracleParameter("StatusBaca", "D"));
                                aParams.Add(new OracleParameter("CatatanAnda", data.IsiSingkatSurat));
                                ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());
                            }
                            #endregion

                        }

                        #endregion

                        sql = "SELECT NOMORAGENDA FROM " + OtorisasiUser.NamaSkema + ".SURAT WHERE SURATID = :SuratId";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        data.NomorAgenda = ctx.Database.SqlQuery<string>(sql, aParams.ToArray()).FirstOrDefault();

                        // Update Table SURAT
                        sql =
                             "UPDATE " + OtorisasiUser.NamaSkema + ".surat SET " +
                             "       jumlahlampiran = jumlahlampiran + :JumlahLampiran " +
                             "       WHERE suratid = :SuratId";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("JumlahLampiran", data.JumlahLampiran));
                        aParams.Add(new OracleParameter("SuratId", data.SuratId));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        sql = $@"Insert into {skema}.Sumber_surat (sumber_id, surat_id, sumber_keterangan) values (:param1, :param2, :param3)";
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", GetUID()));
                        aParams.Add(new OracleParameter("param2", data.SuratId));
                        aParams.Add(new OracleParameter("param3", data.Sumber_Keterangan));
                        ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.Status = true;
                        tr.ReturnValue = data.NomorAgenda;
                        tr.ReturnValue2 = data.NomorSurat;
                        tr.Pesan = string.Concat("Surat ", data.NomorSurat, " berhasil dikirim");
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                        if (tr.Pesan.ToUpper().Contains("I1_SURAT") || tr.Pesan.ToUpper().Contains("I2_SURAT"))
                        {
                            tr.Pesan = string.Concat("Nomor Surat ", data.NomorSurat, " dari ", data.PengirimSurat, " sudah ada.");
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

        public List<SuratInbox> GetDaftarSurat(string satkerid, string nip, string statussurat, string arah, string profileid, string metadata, string nomorsurat, string nomoragenda, string perihal, string tanggalsurat, string tipesurat, string sifatsurat, string sortby, string sorttype, string spesificprofileid, int from, int to, string sumber = null)
        {
            var records = new List<SuratInbox>();
            var aParams = new ArrayList();

            string orderby = "suratinbox.statusurgent DESC, sifatsurat, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
            if (!string.IsNullOrEmpty(sortby))
            {
                if (sortby == "SifatSurat")
                {
                    orderby = "suratinbox.statusurgent DESC, sifatsurat, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.statusurgent, sifatsurat DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalTerima")
                {
                    orderby = "suratinbox.tanggalterima, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalterima DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TanggalKirim")
                {
                    orderby = "suratinbox.tanggalkirim, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
                else if (sortby == "TargetSelesai")
                {
                    orderby = "surat.targetselesai, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                    if (!string.IsNullOrEmpty(sorttype))
                    {
                        if (sorttype == "DESC")
                        {
                            orderby = "surat.targetselesai DESC, suratinbox.tanggalkirim DESC, suratinbox.suratinboxid";
                        }
                    }
                }
            }

            string query =
                "SELECT * FROM ( " +
                "SELECT ROW_NUMBER() over (ORDER BY " + orderby.Replace("suratinbox.tanggalkirim", "tglkirim").Replace("surat.targetselesai", "tglselesai").Replace("suratinbox.tanggalterima", "tglterima").Replace("suratinbox.", "").Replace("surat.", "") + ") RNUMBER, COUNT(1) OVER() TOTAL, RST.* FROM (" +
                "    SELECT DISTINCT" +
                "        agendasurat.nomoragenda NomorAgendaSurat, " +
                "        suratinbox.suratinboxid, suratinbox.suratid, suratinbox.nip, suratinbox.profilepengirim, suratinbox.profilepenerima, " +
                "        suratinbox.tanggalkirim AS tglkirim, surat.targetselesai AS tglselesai, suratinbox.tanggalterima AS tglterima, " +
                "        to_char(suratinbox.tanggalkirim, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkirim, " +
                "        to_char(suratinbox.tanggalbuka, 'dd/mm/yyyy HH24:MI:SS') tanggalbuka, " +
                "        to_char(suratinbox.tanggalterima, 'fmDD Mon YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalterima, " +
                "        suratinbox.tindaklanjut, suratinbox.namapegawai, suratinbox.namapengirim, suratinbox.keterangan, " +
                "        suratinbox.statusterkirim, decode(suratinbox.tanggalbuka, null, 0, 1) AS STATUSBUKA, " +
                "        suratinbox.redaksi, suratinbox.statusterkunci, suratinbox.statusforwardtu, suratinbox.statusurgent, suratinbox.perintahdisposisi, " +
                "        to_char(surat.tanggalsurat, 'dd/mm/yyyy') tanggalsurat, " +
                "        to_char(surat.tanggalproses, 'dd/mm/yyyy') tanggalproses, " +
                "        to_char(surat.tanggalarsip, 'dd/mm/yyyy') tanggalarsip, " +
                "        to_char(surat.targetselesai, 'dd-mm-yyyy') TargetSelesai, " +
                "        to_char(surat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesai, " +
                "        to_char(agendasurat.targetselesai, 'dd-mm-yyyy') TargetSelesaiSuratMasuk, " +
                "        to_char(agendasurat.targetselesai, 'fmDD Month YYYY') InfoTargetSelesaiSuratMasuk, " +
                "        to_char(surat.tanggalundangan, 'dd/mm/yyyy') TanggalUndangan, " +
                "        to_char(surat.tanggalundangan, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') InfoTanggalUndangan, " +
                "        to_char(surat.tanggalinput, 'dd/mm/yyyy HH24:MI') TanggalInput, " +
                "        to_char(surat.tanggalinput, 'fmDD Month YYYY') InfoTanggalInput, " +
                "        to_char(suratkembali.tanggalkembali, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') tanggalkembali, " +
                "        surat.nomorsurat, surat.nomoragenda, surat.perihal, " +
                "        surat.pengirim AS pengirimsurat, surat.penerima AS penerimasurat, surat.kategori, surat.arah, surat.tipesurat, surat.sifatsurat, " +
                "        surat.jumlahlampiran, surat.statussurat, surat.isisingkat AS isisingkatsurat, surat.referensi AS referensisurat, surat.keterangansurat, " +
                "        profiledari.nama AS NAMAPROFILEPENGIRIM, profiletujuan.nama AS NAMAPROFILEPENERIMA, " +
                "        sumber_surat.sumber_keterangan as sumber_keterangan " +
                "    FROM " +
                "        " + OtorisasiUser.NamaSkema + ".suratinbox " +
                "        JOIN " + OtorisasiUser.NamaSkema + ".surat ON surat.suratid = suratinbox.suratid " +
                "        JOIN jabatan profiletujuan ON profiletujuan.profileid = suratinbox.profilepenerima " +
                "        JOIN UNITKERJA UK ON UK.UNITKERJAID = profiletujuan.UNITKERJAID " + // Arya :: 2020-10-02
                "        LEFT JOIN " + OtorisasiUser.NamaSkema + ".sifatsurat ON sifatsurat.nama = surat.sifatsurat " +
                "        LEFT JOIN " + OtorisasiUser.NamaSkema + ".agendasurat ON agendasurat.suratid = surat.suratid AND agendasurat.kantorid = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID) " + // Arya :: 2020-10-02
                "        LEFT JOIN jabatan profiledari ON profiledari.profileid = suratinbox.profilepengirim " +
                "        LEFT JOIN " + OtorisasiUser.NamaSkema + ".suratkembali ON suratkembali.suratinboxid = suratinbox.suratinboxid " +
                (!string.IsNullOrEmpty(sumber) && sumber != "-" ? " INNER JOIN " : " LEFT JOIN ") + OtorisasiUser.NamaSkema + ".SUMBER_SURAT ON SUMBER_SURAT.SURAT_ID = suratinbox.suratid " +
                (!string.IsNullOrEmpty(sumber) && sumber != "-" ? string.Format(" AND sumber_surat.sumber_keterangan = '{0}' ", sumber) : "") +
                "    WHERE " +
                "        suratinbox.statusterkirim = 0 AND suratinbox.statusterkunci = 0 AND suratinbox.statusforwardtu = 0 AND suratinbox.tindaklanjut <> 'Selesai' AND suratinbox.urutan > 1 " +
                "        AND (suratinbox.statushapus IS NULL OR suratinbox.statushapus = '0') " +
                "        AND NOT EXISTS (SELECT 1 FROM " + OtorisasiUser.NamaSkema + ".arsipsurat WHERE arsipsurat.suratid = surat.suratid AND arsipsurat.kantorid = DECODE(UK.TIPEKANTORID,1,UK.UNITKERJAID,UK.KANTORID)) " +  // Arya :: 2020-10-02
                (sumber == "-" ? " AND sumber_surat.sumber_keterangan is null " : "");
            if (!String.IsNullOrEmpty(nip))
            {
                aParams.Add(new OracleParameter("Nip", nip));
                query += " AND (suratinbox.nip IS NULL OR suratinbox.nip = :Nip) ";
            }
            if (!String.IsNullOrEmpty(arah))
            {
                if (arah.Equals("Inisiatif"))
                {
                    aParams.Add(new OracleParameter("Arah", arah));
                    aParams.Add(new OracleParameter("Kategori", arah));
                    query += " AND (surat.arah = :Arah OR surat.kategori = :Kategori)";
                }
                else
                {
                    aParams.Add(new OracleParameter("Arah", arah));
                    query += " AND surat.arah = :Arah AND surat.kategori <> 'Inisiatif' ";
                }
            }
            if (string.IsNullOrEmpty(spesificprofileid))
            {
                if (!string.IsNullOrEmpty(profileid))
                {
                    query += " AND suratinbox.profilepenerima IN (" + profileid + ") ";
                }
            }
            if (!string.IsNullOrEmpty(spesificprofileid))
            {
                aParams.Add(new OracleParameter("SpesificProfileId", spesificprofileid));
                query += " AND suratinbox.profilepenerima = :SpesificProfileId ";
            }
            if (!string.IsNullOrEmpty(metadata))
            {
                aParams.Add(new OracleParameter("Metadata", String.Concat("%", metadata.ToLower(), "%")));
                aParams.Add(new OracleParameter("CatatanAnda", String.Concat("%", metadata.ToLower(), "%")));
                query +=
                    " AND (LOWER(APEX_UTIL.URL_ENCODE(utl_raw.cast_to_varchar2(surat.metadata))) LIKE :Metadata " +
                    "      OR EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".suratinbox WHERE suratinbox.suratid = surat.suratid AND LOWER(APEX_UTIL.URL_ENCODE(suratinbox.keterangan)) LIKE :CatatanAnda)) ";
            }
            if (!string.IsNullOrEmpty(nomorsurat))
            {
                aParams.Add(new OracleParameter("NomorSurat", String.Concat("%", nomorsurat.ToLower(), "%")));
                query += " AND LOWER(surat.nomorsurat) LIKE :NomorSurat ";
            }
            if (!string.IsNullOrEmpty(nomoragenda))
            {
                aParams.Add(new OracleParameter("NomorAgenda", nomoragenda));
                query += " AND surat.nomoragenda = :NomorAgenda ";
            }
            if (!string.IsNullOrEmpty(perihal))
            {
                aParams.Add(new OracleParameter("Perihal", String.Concat("%", perihal.ToLower(), "%")));
                query += " AND LOWER(surat.perihal) LIKE :Perihal ";
            }
            if (!string.IsNullOrEmpty(tanggalsurat))
            {
                aParams.Add(new OracleParameter("Tanggal1", tanggalsurat + " 00:00:00"));
                aParams.Add(new OracleParameter("Tanggal2", tanggalsurat + " 23:59:59"));
                query += " AND (surat.tanggalsurat BETWEEN TO_DATE(:Tanggal1, 'DD/MM/YYYY HH24:MI:SS') AND TO_DATE(:Tanggal2, 'DD/MM/YYYY HH24:MI:SS')) ";
            }
            if (!string.IsNullOrEmpty(tipesurat))
            {
                aParams.Add(new OracleParameter("TipeSurat", tipesurat));
                query += " AND surat.tipesurat = :TipeSurat ";
            }
            if (!string.IsNullOrEmpty(sifatsurat))
            {
                aParams.Add(new OracleParameter("SifatSurat", sifatsurat));
                query += " AND surat.sifatsurat = :SifatSurat ";
            }

            query +=
                " )RST ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            aParams.Add(new OracleParameter("startCnt", from));
            aParams.Add(new OracleParameter("limitCnt", to));

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<SuratInbox>(query, aParams.ToArray()).ToList();
            }

            return records;
        }

        public List<LampiranSurat> GetListLampiranSurat(string suratid, string satkerid)
        {
            var records = new List<LampiranSurat>();

            var aParams = new ArrayList();

            string query = string.Format(@"
                SELECT
                  ROW_NUMBER() OVER (ORDER BY LS.TANGGAL, LS.NAMAFILE, LS.LAMPIRANSURATID) AS RNUMBER,
                  LS.LAMPIRANSURATID,
                  SUBSTR(LS.NAMAFILE, INSTR(LS.NAMAFILE, '|', -1, 1) +1) AS NAMAFILE,
                  LS.NIP,
                  NVL(PG.NAMA, PN.NAMA) AS NAMAPEGAWAI,
                  TO_CHAR(LS.TANGGAL, 'fmDD Month YYYY fmHH24:MI', 'nls_date_language=INDONESIAN') AS TANGGAL,
                  LS.KANTORID, 
                  COUNT(1) OVER() TOTAL
                FROM {0}.LAMPIRANSURAT LS
                INNER JOIN {0}.SURAT SR ON
	                SR.SURATID = LS.SURATID
                INNER JOIN UNITKERJA UK1 ON
	                DECODE(UK1.TIPEKANTORID,1,UK1.UNITKERJAID,UK1.KANTORID) = :param1
                INNER JOIN KANTOR KT ON
	                KT.KANTORID = UK1.KANTORID
                INNER JOIN  UNITKERJA UK0 ON
	                UK0.KANTORID = CASE WHEN KT.TIPEKANTORID > 2 THEN KT.INDUK ELSE KT.KANTORID END
                INNER JOIN UNITKERJA UK2 ON
	                DECODE(UK2.TIPEKANTORID,1,UK2.UNITKERJAID,UK2.KANTORID) = SR.KANTORID
                INNER JOIN JABATAN PR ON
                (PR.PROFILEID = LS.PROFILEID OR PR.PROFILEID IN (
    	            SELECT JP.PROFILEID
			            FROM JABATANPEGAWAI JP
			                INNER JOIN PEGAWAI PG ON
			  	            PG.USERID = LS.PROFILEID AND
			                PG.PEGAWAIID = JP.PEGAWAIID AND
			                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
			                NVL(JP.STATUSHAPUS,'0') = '0'
			            UNION ALL
			            SELECT JP.PROFILEID
			            FROM JABATANPEGAWAI JP
			                INNER JOIN PPNPN PG ON
			  	            PG.USERID = LS.PROFILEID AND
			                PG.NIK = JP.PEGAWAIID AND
			                (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
			                NVL(JP.STATUSHAPUS,'0') = '0')) AND
                PR.UNITKERJAID IN (UK1.UNITKERJAID,UK0.UNITKERJAID,UK2.UNITKERJAID)
                  LEFT JOIN PEGAWAI PG ON
                    PG.PEGAWAIID = LS.NIP
                  LEFT JOIN PPNPN PN ON
                    PN.NIK = LS.NIP
                WHERE
                  LS.SURATID = :param2
                GROUP BY
	                LS.LAMPIRANSURATID,
                  LS.PATH,
                  LS.NAMAFILE,
                  LS.KETERANGAN,
                  LS.PROFILEID,
                  LS.NIP,
                  NVL(PG.NAMA, PN.NAMA),
                  LS.TANGGAL,
                  LS.KANTORID", OtorisasiUser.NamaSkema);

            aParams.Add(new OracleParameter("param1", satkerid));
            aParams.Add(new OracleParameter("param2", suratid));
            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<LampiranSurat>(query, aParams.ToArray()).ToList();
            }

            return records;
        }

        public string GetCatatanSebelumnya(string suratinboxid, string satkerid)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("id-ID");

            string result = "";
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                string query = string.Format(@"
                    SELECT
                      SI1.URUTAN,
                      SO.TANGGALKIRIM,
                      DECODE(SI1.PROFILEPENERIMA,SI2.PROFILEPENGIRIM,1,0) AS INLINE,
                      SO.NIP AS PEGAWAIID,
                      DECODE(PG.PEGAWAIID,NULL,PN.NAMA,DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || decode(PG.NAMA, '', '', PG.NAMA) || decode(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG)) AS NAMALENGKAP,
                      SO.KETERANGAN
                    FROM {0}.SURATINBOX SI1
                      INNER JOIN {0}.SURATINBOX SI2 ON
                        SI2.SURATID = SI1.SURATID AND
                        SI2.SURATINBOXID = :param1 AND
                        SI1.URUTAN <= SI2.URUTAN
                      INNER JOIN {0}.SURAT SR ON
                        SR.SURATID = SI2.SURATID
                      INNER JOIN UNITKERJA UK1 ON
                        DECODE(UK1.TIPEKANTORID,1,UK1.UNITKERJAID,UK1.KANTORID) = :param2 AND
                        DECODE(UK1.TIPEKANTORID,1,UK1.UNITKERJAID,UK1.KANTORID) = SI1.KANTORID
                      INNER JOIN KANTOR KT ON
                        KT.KANTORID = UK1.KANTORID
                      INNER JOIN UNITKERJA UK0 ON
                        UK0.KANTORID = CASE WHEN KT.TIPEKANTORID > 2 THEN KT.INDUK ELSE KT.KANTORID END
                      INNER JOIN {0}.SURATOUTBOXRELASI SOR ON
  	                    SOR.SURATINBOXID = SI1.SURATINBOXID
                      INNER JOIN {0}.SURATOUTBOX SO ON
                        SO.SURATOUTBOXID = SOR.SURATOUTBOXID AND
                        SO.KANTORID = SI1.KANTORID AND
	                    NULLIF(SO.KETERANGAN,'') IS NOT NULL
                      LEFT JOIN PEGAWAI PG ON
                        PG.PEGAWAIID = SO.NIP
                      LEFT JOIN PPNPN PN ON
                        PN.NIK = SO.NIP
                    WHERE
                      SI1.KANTORID IN (DECODE(UK1.TIPEKANTORID,1,UK1.UNITKERJAID,UK1.KANTORID),DECODE(UK0.TIPEKANTORID,1,UK0.UNITKERJAID,UK0.KANTORID))
                    GROUP BY
	                  SI1.URUTAN,
                      SO.TANGGALKIRIM,
                      DECODE(SI1.PROFILEPENERIMA,SI2.PROFILEPENGIRIM,1,0),
                      SO.NIP,
                      DECODE(PG.PEGAWAIID,NULL,PN.NAMA,DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || decode(PG.NAMA, '', '', PG.NAMA) || decode(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG)),
                      SO.KETERANGAN
                    ORDER BY SI1.URUTAN DESC, DECODE(SI1.PROFILEPENERIMA,SI2.PROFILEPENGIRIM,1,0) DESC, SO.TANGGALKIRIM DESC", skema);
                ArrayList aParams = new ArrayList();
                aParams.Add(new OracleParameter("param1", suratinboxid));
                aParams.Add(new OracleParameter("param2", satkerid));
                var data = ctx.Database.SqlQuery<CatatanSurat>(query, aParams.ToArray()).FirstOrDefault();
                if (data != null)
                {
                    result = string.Format(@"<span style='font-size:9pt; color:Navy'><u>{0}</u></span><br /><div style='color:#dd6136'>{1}:</div>{2}", data.TanggalKirim.ToString("dddd, d MMMM yyyy"), data.NamaLengkap, data.Keterangan);
                }
            }

            return result;
        }

        public Entities.Surat GetSuratBySuratInboxId(string suratinboxid, string satkerid, string suratid, string pegawaiid, string sumber = null)
        {
            var records = new Entities.Surat();
            var aParams = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;

            string query = string.Format(@"
                SELECT      
                  SURAT.SURATID,
                  SURAT.NOMORSURAT,
                  SURAT.NOMORAGENDA,
                  SURAT.PERIHAL,
                  SURAT.PENGIRIM AS PENGIRIMSURAT,
                  SURAT.PENERIMA AS PENERIMASURAT,
                  AGENDASURAT.NOMORAGENDA AS NOMORAGENDASURAT,
                  TO_CHAR(SURAT.TANGGALSURAT, 'dd/mm/yyyy') AS TANGGALSURAT,
                  TO_CHAR(SURAT.TANGGALSURAT, 'fmDD Month YYYY', 'nls_date_language=INDONESIAN') AS INFoTANGGALSURAT,
                  TO_CHAR(SURAT.TANGGALINPUT, 'dd/mm/yyyy HH24:MI') AS TANGGALINPUT,
                  TO_CHAR(SURAT.TANGGALPROSES, 'dd/mm/yyyy') AS TANGGALPROSES,
                  TO_CHAR(SURAT.TANGGALPROSES, 'dd Month yyyy', 'nls_date_language=INDONESIAN') AS INFOTANGGALPROSES,
                  TO_CHAR(SURAT.TANGGALARSIP, 'dd/mm/yyyy') AS TANGGALARSIP,
                  TO_CHAR(SURAT.TARGETSELESAI, 'dd-mm-yyyy') AS TARGETSELESAI,
                  TO_CHAR(SURAT.TARGETSELESAI, 'fmDD Month YYYY') AS INFOTARGETSELESAI,
                  TO_CHAR(AGENDASURAT.TARGETSELESAI, 'dd-mm-yyyy') AS TARGETSELESAISURATMASUK,
                  TO_CHAR(AGENDASURAT.TARGETSELESAI, 'fmDD Month YYYY') AS INFOTARGETSELESAISURATMASUK,
                  TO_CHAR(SURAT.TANGGALUNDANGAN, 'dd/mm/yyyy') AS TANGGALUNDANGAN,
                  TO_CHAR(SURAT.TANGGALUNDANGAN, 'fmDD Month YYYY HH24:MI', 'nls_date_language=INDONESIAN') AS INFOTANGGALUNDANGAN,
                  TO_CHAR(SURATINBOX.TANGGALKIRIM, 'dd Mon yyyy HH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALKIRIM,
                  TO_CHAR(SURATINBOX.TANGGALBUKA, 'dd/mm/yyyy HH24:MI:SS') AS TANGGALBUKA,
                  TO_CHAR(SURATINBOX.TANGGALTERIMA, 'dd/mm/yyyy HH24:MI') AS TANGGALTERIMA,
                  TO_CHAR(SURATINBOX.TANGGALKIRIM, 'Day, dd Mon yyyy HH24:MI', 'nls_date_language=INDONESIAN') AS TANGGALINBOX,
                  SURAT.KATEGORI,
                  SURAT.ARAH,
                  'Surat ' || SURAT.ARAH AS ARAHSURAT,
                  SURAT.TIPESURAT,
                  SURAT.SIFATSURAT,
                  SURAT.JUMLAHLAMPIRAN,
                  SURAT.ISISINGKAT AS ISISINGKATSURAT,
                  SURAT.STATUSSURAT,
                  SURAT.REFERENSI AS REFERENSISURAT,
                  SURAT.KETERANGANSURAT,
                  DECODE(ARSIPSURAT.ARSIPSURATid, NULL, 0, 1) AS STATUSARSIP,
                  TO_CHAR(SURATINBOX.TANGGALTERIMA, 'dd/mm/yyyy') AS TANGGALTERIMAFISIK, 
                  DECODE(SURATINBOX.TANGGALTERIMA, NULL, 'TIDAK', 'YA') AS TERIMAFISIK,
                  PROFILEDARI.NAMA AS NAMAPROFILEPENGIRIM,
                  SURATINBOX.NAMAPEGAWAI AS NAMAPENERIMA,
                  SURATINBOX.NIP AS NIP,
                  SURAT.KETERANGANSURAT || ', ' || SURATINBOX.REDAKSI AS KETERANGANSURATREDAKSI,
                  SURATINBOX.REDAKSI,
                  SURATINBOX.STATUSTERKUNCI,
                  SURATINBOX.STATUSFORWARDTU,
                  SURATINBOX.STATUSURGENT,
                  SURATINBOX.PERINTAHDISPOSISI,
                  SURATINBOX.PROFILEPENERIMA,
                  SURATINBOX.SURATINBOXID,
                  SURATINBOX.KETERANGAN AS CATATANANDA,
                  SURATINBOX.URUTAN,
                  SUMBER_SURAT.SUMBER_KETERANGAN
                FROM {0}.SURATINBOX
                  JOIN {0}.SURAT ON
                    SURAT.SURATID = SURATINBOX.SURATID
                  LEFT JOIN {0}.ARSIPSURAT ON
                    ARSIPSURAT.SURATID = SURAT.SURATID AND
                    ARSIPSURAT.KANTORID = :param1
                  LEFT JOIN {0}.AGENDASURAT ON
                    AGENDASURAT.SURATID = SURAT.SURATID AND
                    AGENDASURAT.KANTORID = :param2
                  LEFT JOIN JABATAN PROFILEDARI ON
                    PROFILEDARI.PROFILEID = SURATINBOX.PROFILEPENGIRIM
                  {1} {0}.SUMBER_SURAT ON
                    SUMBER_SURAT.SURAT_ID = SURATINBOX.SURATID {2}
                  INNER JOIN JABATANPEGAWAI JP ON
                    JP.PROFILEID = SURATINBOX.PROFILEPENERIMA AND
                    JP.PEGAWAIID = :param3 AND
		            (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
		            NVL(JP.STATUSHAPUS, '0') = '0'
                WHERE
                  SURATINBOX.SURATINBOXID = :param4", skema, !string.IsNullOrEmpty(sumber) ? " INNER JOIN " : " LEFT JOIN ", !string.IsNullOrEmpty(sumber) ? string.Format(" AND sumber_surat.sumber_keterangan = '{0}' ", sumber) : "");
            aParams.Add(new OracleParameter("param1", satkerid));
            aParams.Add(new OracleParameter("param2", satkerid));
            aParams.Add(new OracleParameter("param3", pegawaiid));
            aParams.Add(new OracleParameter("param4", suratinboxid));


            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<Models.Entities.Surat>(query, aParams.ToArray()).FirstOrDefault();
            }

            return records;
        }

        public TransactionResult GetNomorAgenda(string suratid, string satkerid, string unitkerjaid, string suratinboxid = null)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };

            string skema = OtorisasiUser.NamaSkema;
            var aParams = new ArrayList();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string query = string.Format(@"
                            SELECT NOMORAGENDA
                            FROM {0}.AGENDASURAT
                            WHERE
                              SURATID = :param1 AND
                              KANTORID = :param2", skema);
                        aParams.Clear();
                        aParams.Add(new OracleParameter("param1", suratid));
                        aParams.Add(new OracleParameter("param2", satkerid));
                        string _nomorAgenda = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).FirstOrDefault();
                        if (string.IsNullOrEmpty(_nomorAgenda))
                        {
                            var _dt = ctx.Database.SqlQuery<DateTime>("SELECT SYSDATE FROM DUAL").FirstOrDefault();
                            //if (!string.IsNullOrEmpty(suratinboxid))
                            //{
                            //    query = string.Format(@"SELECT TANGGALKIRIM FROM {0}.SURATINBOX WHERE SURATINBOXID = :param1", skema);
                            //    aParams.Clear();
                            //    aParams.Add(new OracleParameter("param1", suratinboxid));
                            //    _dt = ctx.Database.SqlQuery<DateTime>(query, aParams.ToArray()).FirstOrDefault();
                            //}
                            int tahun = _dt.Year;
                            query = string.Format(@"
                                SELECT COUNT(1)
                                FROM {0}.KONTERSURAT
                                WHERE
                                  KANTORID = :param1 AND
                                  TAHUN = :param2 AND
                                  TIPESURAT = :param3", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", satkerid));
                            aParams.Add(new OracleParameter("param2", tahun));
                            aParams.Add(new OracleParameter("param3", "Agenda"));
                            int jumlahrecord = ctx.Database.SqlQuery<int>(query, aParams.ToArray()).FirstOrDefault();
                            if (jumlahrecord == 0)
                            {
                                query = string.Format(@"
                                INSERT INTO {0}.KONTERSURAT (KONTERSURATID, KANTORID, TIPESURAT, TAHUN, NILAIKONTER)
                                VALUES (SYS_GUID(), :param1, :param2, :param3, 0)", skema);
                                aParams.Clear();
                                aParams.Add(new OracleParameter("param1", satkerid));
                                aParams.Add(new OracleParameter("param2", "Agenda"));
                                aParams.Add(new OracleParameter("param3", tahun));
                                ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());
                            }

                            decimal nilainomoragenda = 1;

                            query = string.Format(@"
                            SELECT NILAIKONTER+1
                            FROM {0}.KONTERSURAT
                            WHERE
                              KANTORID = :param1 AND
                              TAHUN = :param2 AND
                              TIPESURAT = :param3", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", satkerid));
                            aParams.Add(new OracleParameter("param2", tahun));
                            aParams.Add(new OracleParameter("param3", "Agenda"));
                            nilainomoragenda = ctx.Database.SqlQuery<decimal>(query, aParams.ToArray()).FirstOrDefault();

                            query = string.Format("UPDATE {0}.KONTERSURAT SET NILAIKONTER = :NilaiKonter WHERE KANTORID = :SatkerId AND TAHUN = :Tahun AND TIPESURAT = :Tipe", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("NilaiKonter", nilainomoragenda));
                            aParams.Add(new OracleParameter("SatkerId", satkerid));
                            aParams.Add(new OracleParameter("Tahun", tahun));
                            aParams.Add(new OracleParameter("Tipe", "Agenda"));
                            ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());

                            // Binding Nomor Agenda
                            int bulan = _dt.Month;
                            string strBulan = Functions.NomorRomawi(bulan);
                            string kodeindentifikasi = GetKodeIdentifikasi(unitkerjaid);
                            string kodesurat = "AG-";

                            //_nomorAgenda = Convert.ToString(nilainomoragenda) + "/" + kodesurat + kodeindentifikasi + "/" + strBulan + "/" + Convert.ToString(GetServerYear());
                            _nomorAgenda = string.Concat(Convert.ToString(nilainomoragenda),"/",kodesurat,kodeindentifikasi,"/",strBulan,"/", tahun);

                            query = string.Format(@"
                                INSERT INTO {0}.AGENDASURAT (AGENDASURATID, SURATID, NOMORAGENDA, KANTORID)
                                VALUES (SYS_GUID(), :param1, :param2, :param3)", skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("param1", suratid));
                            aParams.Add(new OracleParameter("param2", _nomorAgenda));
                            aParams.Add(new OracleParameter("param3", satkerid));
                            ctx.Database.ExecuteSqlCommand(query, aParams.ToArray());
                        }
                        tr.ReturnValue = _nomorAgenda;
                        tr.Status = true;
                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message;
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

        public string GetExt(string lampiransuratid)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(lampiransuratid))
            {
                string query = string.Format("SELECT EXT FROM {0}.LAMPIRANSURAT WHERE LAMPIRANSURATID = :param1 AND STATUS = 'A'", OtorisasiUser.NamaSkema);

                var aParams = new ArrayList();
                aParams.Add(new OracleParameter("param1", lampiransuratid));

                using (var ctx = new BpnDbContext())
                {
                    result = ctx.Database.SqlQuery<string>(query, aParams.ToArray()).FirstOrDefault();
                }
            }

            return result;
        }

        public TransactionResult KembalikanSurat(SuratKembali data)
        {
            var tr = new TransactionResult() { Status = false, Pesan = "" };
            string sql = string.Empty;
            var aParams = new ArrayList();
            string skema = OtorisasiUser.NamaSkema;

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var suratasal = GetSuratInboxForSuratKembali(data.SuratInboxId);

                        if (suratasal != null)
                        {
                            sql =
                                "INSERT INTO " + skema + ".suratkembali ( " +
                                "       suratkembaliid, suratinboxid, suratoutboxid, tanggalkembali, keterangan, " +
                                "       profilepengirim, nippengirim, namapengirim, " +
                                "       profilepenerima, nippenerima, namapenerima) VALUES " +
                                "( " +
                                "       SYS_GUID(), :SuratInboxId, :SuratOutboxId, SYSDATE, :Keterangan, " +
                                "       :ProfilePengirim, :NipPengirim, :NamaPengirim, " +
                                "       :ProfilePenerima, :NipPenerima, :NamaPenerima)";
                            aParams.Clear();
                            aParams.Add(new OracleParameter("SuratInboxId", data.SuratInboxId)); // inbox pengirim
                            aParams.Add(new OracleParameter("SuratOutboxId", suratasal.SuratOutboxId));
                            aParams.Add(new OracleParameter("Keterangan", data.Keterangan));
                            aParams.Add(new OracleParameter("ProfilePengirim", suratasal.ProfilePengirim));
                            aParams.Add(new OracleParameter("NipPengirim", suratasal.NipPengirim));
                            aParams.Add(new OracleParameter("NamaPengirim", suratasal.NamaPengirim));
                            aParams.Add(new OracleParameter("ProfilePenerima", suratasal.ProfilePenerima));
                            aParams.Add(new OracleParameter("NipPenerima", suratasal.NipPenerima));
                            aParams.Add(new OracleParameter("NamaPenerima", suratasal.NamaPenerima));
                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                            sql = string.Format("UPDATE {0}.suratinbox SET STATUSHAPUS = '1', USERHAPUS = :UserId WHERE suratinboxid = :SuratInboxId",skema);
                            aParams.Clear();
                            aParams.Add(new OracleParameter("UserId", data.UserId));
                            aParams.Add(new OracleParameter("SuratInboxId", data.SuratInboxId));
                            ctx.Database.ExecuteSqlCommand(sql, aParams.ToArray());

                            tr.Status = true;
                            tr.ReturnValue = data.SuratId;
                            tr.Pesan = string.Concat("Surat berhasil dikembalikan ke ", suratasal.NamaPenerima, ".");
                        }
                        else
                        {
                            tr.Pesan = "";
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
                    }
                    catch (Exception ex)
                    {
                        string errmess = ex.Message.ToString();
                        if (errmess.Contains("ORA-01400"))
                        {
                            errmess = "Catatan wajib diisi";
                        }
                        tc.Rollback();
                        tr.Pesan = errmess;
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

        public SuratKembali GetSuratInboxForSuratKembali(string suratinboxid)
        {
            var records = new SuratKembali();
            var aParams = new ArrayList();

            string query = string.Format(@"
                SELECT
	                SI.SURATINBOXID,
	                SI.PROFILEPENERIMA AS PROFILEPENGIRIM,
	                SI.NIP AS NIPPENGIRIM,
	                SI.NAMAPEGAWAI AS NAMAPENGIRIM,
	                SO.SURATOUTBOXID,
	                SO.PROFILEPENGIRIM AS PROFILEPENERIMA,
	                SO.NIP AS NIPPENERIMA,
	                SI.NAMAPENGIRIM AS NAMAPENERIMA
                FROM {0}.SURATINBOX SI
                INNER JOIN {0}.SURATOUTBOXRELASI SOR ON
	                SOR.SURATINBOXID = SI.SURATINBOXID
                INNER JOIN {0}.SURATOUTBOX SO ON
	                SO.SURATOUTBOXID = SOR.SURATOUTBOXID
                WHERE
	                SI.SURATINBOXID = :param1", OtorisasiUser.NamaSkema);

            aParams.Add(new OracleParameter("param1", suratinboxid));

            using (var ctx = new BpnDbContext())
            {
                records = ctx.Database.SqlQuery<SuratKembali>(query, aParams.ToArray()).FirstOrDefault();
            }

            return records;
        }

        public List<Pegawai> GetPetugasSuratMasukByUnitKerja(string ukid)
        {
            List<Pegawai> list = new List<Pegawai>();
            var aParams = new ArrayList();
            string sql = $@" SELECT ROW_NUMBER() over(ORDER BY TBL.tipeuser, TBL.tipeeselonid, TBL.nama) RNumber,
                                    COUNT(1) OVER() TOTAL, TBL.pegawaiid, TBL.nama, TBL.profileid, TBL.jabatan, 
                                    TBL.namalengkap, TBL.tipeeselonid, TBL.nama || ', ' || TBL.jabatan AS NamaDanJabatan
                             FROM (
                                   SELECT 
			                             pegawai.pegawaiid, pegawai.nama, jabatan.nama || decode(jabatanpegawai.statusplt, 1, ' (PLT)', 2, ' (PLH)', '') AS jabatan, decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || decode(pegawai.nama, '', '', pegawai.nama) || decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang) AS NamaLengkap, jabatan.profileid, jabatan.tipeeselonid, 0 AS tipeuser 
			                             FROM
			                             pegawai
			                             JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = pegawai.pegawaiid AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND (jabatanpegawai.VALIDSAMPAI IS NULL OR CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0'
			                             JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid 
                                   JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid AND unitkerja.unitkerjaid = :parm1
			                             UNION
			                             SELECT
			                             ppnpn.nik AS pegawaiid, ppnpn.nama, 'PPNPN' AS jabatan, ppnpn.nama AS NamaLengkap, jabatan.profileid, jabatan.tipeeselonid, 1 AS tipeuser
			                             FROM
			                             ppnpn
			                             JOIN jabatanpegawai ON jabatanpegawai.pegawaiid = ppnpn.nik AND jabatanpegawai.profileid NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') 
			                             AND (jabatanpegawai.VALIDSAMPAI IS NULL OR CAST(jabatanpegawai.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0' 
			                             JOIN jabatan ON jabatan.profileid = jabatanpegawai.profileid
			                             JOIN unitkerja ON unitkerja.unitkerjaid = jabatan.unitkerjaid AND unitkerja.unitkerjaid = :parm2
			                    ) TBL
			                    INNER JOIN JABATANPEGAWAI JP ON JP.PEGAWAIID = TBL.PEGAWAIID AND JP.PROFILEID = 'A81001' AND (JP.VALIDSEJAK IS NOT NULL AND (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)))
			                    WHERE TBL.pegawaiid IS NOT NULL
                                ORDER BY TIPEESELONID DESC, NAMA ASC";
            aParams.Add(new OracleParameter("parm1", ukid));
            aParams.Add(new OracleParameter("parm2", ukid));

            using (var ctx = new BpnDbContext())
            {
                list = ctx.Database.SqlQuery<Pegawai>(sql, aParams.ToArray()).ToList<Pegawai>();
            }

            return list;
        }

        public PengantarSurat GetNewPengantarSurat(string psid)
        {
            PengantarSurat result = new PengantarSurat();
            using (var ctx = new BpnDbContext())
            {
                ArrayList arrayListParameters = new ArrayList();
                object[] parameters;
                string query = $@"SELECT 
                                    PENGANTARSURATID, 
                                    KANTORID AS UNITKERJAID, 
                                    NOMOR AS NOMORSURAT, 
                                    TUJUAN AS USERID, 
                                    PROFILEIDTUJUAN, 
                                    NAMAPENERIMA AS PENANDATANGAN, 
                                    TO_CHAR(TANGGALSURAT,'dd/mm/yyyy') AS TANGGALSURAT, 
                                    utl_raw.cast_to_varchar2(METADATA) AS LSTSURAT 
                                FROM {OtorisasiUser.NamaSkema}.PENGANTARSURAT WHERE PENGANTARSURATID = :psid";
                arrayListParameters.Add(new OracleParameter("psid", psid));
                parameters = arrayListParameters.OfType<object>().ToArray();
                if (!string.IsNullOrEmpty(psid))
                {
                    result = ctx.Database.SqlQuery<PengantarSurat>(query, parameters).FirstOrDefault();
                }
            }
            return result;
        }

        public string HapusSuratPengantar(string psid, string ukid)
        {
            string result = string.Empty;
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters;
                        string query = $@"UPDATE {OtorisasiUser.NamaSkema}.PENGANTARSURAT SET KANTORID = :ukid WHERE PENGANTARSURATID = :psid";
                        arrayListParameters.Add(new OracleParameter("ukid", "D" + ukid));
                        arrayListParameters.Add(new OracleParameter("psid", psid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(query, parameters);
                        tc.Commit();
                        result = "berhasil";
                    }
                    catch (Exception e)
                    {
                        tc.Rollback();
                        result = e.Message;
                    }
                }
            }
            return result;
        }

        public List<Models.Entities.PengantarSurat> GetListPengantar(
           int from, int to,
           string pengantarid = null,
           string unitkerjaid = null,
           string nomorpengantar = null,
           string tanggalpengantar = null,
           string tujuanpengantar = null,
           string srchkey = null
           )
        {
            List<Models.Entities.PengantarSurat> records = new List<Models.Entities.PengantarSurat>();
            ArrayList arrayListParameters = new ArrayList();
            object[] parameters;
            string sql = $@"SELECT  
                                COUNT(1) OVER() TOTAL, 
                                PT.PENGANTARSURATID, 
                                PT.NOMOR AS NOMORSURAT,
                                PT.TUJUAN AS USERID,
                                PT.NAMAPENERIMA AS PENANDATANGAN,
                                TO_CHAR(PT.TANGGALSURAT,'dd/mm/yyyy') AS TANGGALSURAT,
                                PT.PROFILEIDTUJUAN, 
                                utl_raw.cast_to_varchar2(PT.METADATA) AS LSTSURAT
                            FROM {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT PT
                            WHERE PT.NOMOR IS NOT NULL AND PT.TUJUAN IS NOT NULL AND PT.TANGGALSURAT IS NOT NULL AND PT.KANTORID = :unitkerjaid ";
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("unitkerjaid", unitkerjaid));

            if (!string.IsNullOrEmpty(pengantarid))
            {
                sql += $" AND PT.PENGANTARSURATID = :pengantarsuratid";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pengantarsuratid", pengantarid));
            }
            if (!string.IsNullOrEmpty(nomorpengantar))
            {
                sql += $" AND PT.NOMOR LIKE '%'||:nomorsurat||'%' ";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorsurat", nomorpengantar));
            }
            //sementara jadi tanggal surat
            if (!string.IsNullOrEmpty(tanggalpengantar))
            {
                sql += $" AND TO_CHAR(TANGGALSURAT,'dd/mm/yyyy') = :tanggal";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggal", tanggalpengantar));
            }
            //bukan jabatan tapi satker tujuan
            if (!string.IsNullOrEmpty(tujuanpengantar))
            {
                sql += $" AND PT.PROFILEIDTUJUAN = :profiletujuan";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("profiletujuan", tujuanpengantar));
            }

            using (var ctx = new BpnDbContext())
            {
                sql += " ORDER BY PT.TANGGALSURAT DESC, PT.NOMOR DESC";
                sql = $"SELECT ROWNUM AS RNUMBER, TB.* FROM ({sql}) TB";
                sql = $"SELECT * FROM ({sql}) WHERE RNUMBER BETWEEN {from} AND {to} ";
                parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Models.Entities.PengantarSurat>(sql, parameters).ToList<Models.Entities.PengantarSurat>();
            }
            return records;
        }

        public string GetProfileidTuFromUnitKerja(string ukid, bool isMenteri)
        {
            string result = "";
            ArrayList arrayListParameters = new ArrayList();
            string sql = $@"SELECT PROFILEIDTU 
                            FROM JABATAN 
                            WHERE UNITKERJAID = :ukid
                                AND TIPEESELONID <> 0 
                                AND ROWNUM = 1 
                            ORDER BY TIPEESELONID";
            if (isMenteri)
            {
                sql = $@"SELECT PROFILEIDTU FROM JABATAN WHERE PROFILEID = :ukid AND ROWNUM = 1 ORDER BY TIPEESELONID";
            }

            arrayListParameters.Add(new OracleParameter("ukid", ukid));

            using (var ctx = new BpnDbContext())
            {
                var parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(sql,parameters).FirstOrDefault();
            }

            return result;
        }

        public List<Entities.Surat> GetListSuratForPengantar(string namapegawai, string profileidtu, string tanggal)
        {
            List<Entities.Surat> result = new List<Entities.Surat>();
            ArrayList arrayListParameters = new ArrayList();
            string sql = $@"
                            SELECT SI.SURATID 
                            FROM {OtorisasiUser.NamaSkema}.SURATINBOX SI 
                            WHERE SI.NAMAPENGIRIM = :param1 
                                AND SI.PROFILEPENERIMA = :param2 
                                AND TRUNC(SI.TANGGALTERIMA) = TO_DATE(:param3, 'DD/MM/YYYY') 
                            GROUP BY SI.SURATID
                            ";
            arrayListParameters.Add(new OracleParameter("param1", namapegawai));
            arrayListParameters.Add(new OracleParameter("param2", profileidtu));
            arrayListParameters.Add(new OracleParameter("param3", tanggal));
            using (var ctx = new BpnDbContext())
            {
                var parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<Entities.Surat>(sql,parameters).ToList<Entities.Surat>();
            }

            return result;
        }

        public Entities.TransactionResult SimpanSuratPengantar(Models.Entities.PengantarSurat ps)
        {
            var result = new Entities.TransactionResult() { Status = false, Pesan = "Terjadi Kesalahan dalam menyimpan data" };
            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters;
                        if (string.IsNullOrEmpty(ps.PengantarSuratId))
                        {
                            if (!string.IsNullOrEmpty(ps.NomorSurat) && !string.IsNullOrEmpty(ps.TanggalSurat) && !string.IsNullOrEmpty(ps.LstSurat) && !string.IsNullOrEmpty(ps.Penandatangan))
                            {

                                if (ps.ProfileIdTujuan == "H0000001" || ps.ProfileIdTujuan == "H0000002")
                                {
                                    ps.TujuanSurat = GetNamaJabatan(ps.ProfileIdTujuan);
                                }
                                else
                                {
                                    var dm = new DataMasterModel();
                                    ps.TujuanSurat = dm.GetNamaUnitKerjaById(ps.ProfileIdTujuan);
                                }

                                var psid = new Models.SuratModel().GetUID();

                                string sql = $@"INSERT INTO {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT (PENGANTARSURATID, KANTORID, TANGGALDARI, NOMOR, TUJUAN, NAMAPENERIMA, METADATA, PROFILEIDTUJUAN, TANGGALSURAT)
                                        VALUES (:psid, :unitkerja, SYSDATE, :nomorsurat, :pembuat, :penandatangan, utl_raw.cast_to_raw(:listsurat), :tujuansurat, TO_DATE(:tanggalsurat,'DD/MM/YYYY'))";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("psid", psid));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("unitkerja", ps.UnitKerjaId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorsurat", ps.NomorSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pembuat", ps.UserId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("penandatangan", ps.Penandatangan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("listsurat", ps.LstSurat));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tujuansurat", ps.ProfileIdTujuan));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggalsurat", ps.TanggalSurat));

                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(sql, parameters);
                                tc.Commit();
                                result.Status = true;
                                result.Pesan = "Data Berhasil di tambahkan";
                                result.ReturnValue = psid;
                            }
                            else
                            {
                                result.Status = false;
                                result.Pesan = "Terdatap Kekurangan pada data isians";
                            }
                        }
                        else
                        {
                            var pengantar = GetNewPengantarSurat(ps.PengantarSuratId);
                            bool up = false;
                            string update = "";
                            if (ps.NomorSurat != pengantar.NomorSurat)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  NOMOR = :nomorsurat ";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorsurat", ps.NomorSurat));
                            }
                            if (ps.Penandatangan != pengantar.Penandatangan)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  NAMAPENERIMA = :penandatangan";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("penandatangan", ps.Penandatangan));
                            }
                            if (ps.ProfileIdTujuan != pengantar.ProfileIdTujuan)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  PROFILEIDTUJUAN = :tujuansurat";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tujuansurat", ps.ProfileIdTujuan));
                            }
                            if (ps.TanggalSurat != pengantar.TanggalSurat)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  TANGGALSURAT = TO_DATE(:tanggalsurat,'DD/MM/YYYY')";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggalsurat", ps.TanggalSurat));
                            }
                            if (ps.LstSurat != pengantar.LstSurat)
                            {
                                up = true;
                                update += $" {(string.IsNullOrEmpty(update) ? "" : ",")}  METADATA = utl_raw.cast_to_raw(:listsurat)";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("listsurat", ps.LstSurat));
                            }

                            if (up)
                            {
                                update = $@"UPDATE {System.Web.Mvc.OtorisasiUser.NamaSkema}.PENGANTARSURAT SET {update} WHERE PENGANTARSURATID = :psid";
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("psid", ps.PengantarSuratId));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                ctx.Database.ExecuteSqlCommand(update, parameters);
                                tc.Commit();
                                result.Status = true;
                                result.Pesan = "data Berhasil di update";
                                result.ReturnValue = ps.PengantarSuratId;
                            }
                            else
                            {
                                result.Status = true;
                                result.Pesan = "Tidak ada perubahan pada data";
                                result.ReturnValue = ps.PengantarSuratId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        result.Status = false;
                        result.Pesan = ex.Message;
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

    }
}