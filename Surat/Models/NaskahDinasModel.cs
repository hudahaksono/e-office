﻿using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static Surat.Codes.Functions;
using Npgsql;

namespace Surat.Models
{
	public class NaskahDinasModel
	{
		Regex sWhitespace = new Regex(@"\s+");
		IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("id-ID", true);
		Functions functions = new Functions();
		public List<ListDraft> GetKonsepList(string userid, string status, string draftcode)
		{
			//status = status surat | statusAcc = stataus koordinasi
			var records = new List<ListDraft>();
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();

			string sql = $@"SELECT TBL.DRAFTCODE,
								   TBL.UNITKERJAID, 
								   NVL(TBL.PERIHAL,'-') AS PERIHAL, 
								   TBL.SIFATSURAT, 
								   TBL.TIPESURAT, 
								   TBL.USERBUAT, 
								   TBL.USERUBAH, TBL.TANGGALBUAT, TBL.TANGGALUBAH, TBL.STATUS, TBL.NAMABUAT, TBL.NAMAUBAH, TBL.KETERANGAN, TBL.PENANDATANGAN, TBL.NOTIFIKASI, TBL.FORORDER, CASE WHEN KDH.PESAN IS NOT NULL THEN 1 ELSE 0 END AS OPEN, 
							NVL(CASE TBL.KETERANGAN WHEN 'Pengolah Surat' THEN 'P' ELSE KRD.STATUS END,'P') as STATUSACC, KRD.TANGGAL
							FROM (
							SELECT TD.DRAFTCODE, TD.UNITKERJAID, TD.PERIHAL, TD.SIFATSURAT, TD.TIPESURAT, TD.USERBUAT, TD.USERUBAH, TO_CHAR(TD.TANGGALBUAT,'DD/MM/YYYY HH24:MI') TANGGALBUAT, TO_CHAR(TD.TANGGALUBAH,'DD/MM/YYYY HH24:MI') TANGGALUBAH, TD.STATUS, NVL(PN.NAMA, PG.NAMA) NAMABUAT, NVL(PN2.NAMA, PG2.NAMA) NAMAUBAH, TD.KETERANGAN, JB.NAMA AS PENANDATANGAN, NVL(KH.URUTAN, 0) AS NOTIFIKASI, TD.FORORDER
							FROM 
							(SELECT TDS.DRAFTCODE,  TDS.UNITKERJAID, TDS.PERIHAL, TDS.SIFATSURAT, TDS.TIPESURAT, TLD.USERID USERBUAT, TDS.UPDUSER USERUBAH, TLD.LOGTIME TANGGALBUAT, TDS.UPDTIME TANGGALUBAH, TDS.STATUS, 'Pengolah Surat' AS KETERANGAN, TDS.PROFILEPENGIRIM, TDS.UPDTIME AS FORORDER 
							FROM {skema}.TBLDRAFTSURAT TDS 
							INNER JOIN {skema}.TBLLOGDRAFT TLD ON TLD.DRAFTCODE = TDS.DRAFTCODE AND TLD.LOGTEXT = 'New' 
							WHERE TLD.USERID = :usrid AND TDS.STATUS = :status
							UNION ALL 
							SELECT TDS.DRAFTCODE, TDS.UNITKERJAID, TDS.PERIHAL, TDS.SIFATSURAT, TDS.TIPESURAT, TLD.USERID USERBUAT, TDS.UPDUSER USERUBAH, TLD.LOGTIME TANGGALBUAT, TDS.UPDTIME TANGGALUBAH, TDS.STATUS, 'Verifikator' AS KETERANGAN, TDS.PROFILEPENGIRIM, KD.TANGGAL AS FORORDER  
							FROM {skema}.KOORDINASIDRAFT KD 
							LEFT JOIN {skema}.TBLDRAFTSURAT TDS ON KD.DRAFTCODE = TDS.DRAFTCODE 
							INNER JOIN {skema}.TBLLOGDRAFT TLD ON TLD.DRAFTCODE = KD.DRAFTCODE AND TLD.LOGTEXT = 'New'
							WHERE TDS.STATUS = :status1 AND KD.USERID = :usrid1 AND KD.STATUS != 'D') TD 
							LEFT JOIN PPNPN PN ON PN.USERID = TD.USERBUAT LEFT JOIN PEGAWAI PG ON PG.USERID = TD.USERBUAT LEFT JOIN PPNPN PN2 ON PN2.USERID = TD.USERUBAH LEFT JOIN PEGAWAI PG2 ON PG2.USERID = TD.USERUBAH 
							LEFT JOIN JABATAN JB ON TD.PROFILEPENGIRIM = JB.PROFILEID 
							LEFT JOIN {skema}.KOORDINASIDRAFTHISTORY KH ON KH.KOR_ID = TD.DRAFTCODE AND KH.URUTAN = 1) TBL
							LEFT JOIN {skema}.KOORDINASIDRAFTHISTORY KDH ON TBL.DRAFTCODE = KDH.KOR_ID AND KDH.PSFROM = :usrid2 AND ROWNUM = 1
							LEFT JOIN {skema}.KOORDINASIDRAFT KRD ON TBL.DRAFTCODE = KRD.DRAFTCODE AND KRD.USERID = :usrid3 AND KRD.STATUS != 'D'
							";
			arrayListParameters.Add(new OracleParameter("usrid", userid));
			arrayListParameters.Add(new OracleParameter("status", status));
			arrayListParameters.Add(new OracleParameter("status1", status));
			arrayListParameters.Add(new OracleParameter("usrid1", userid));
			arrayListParameters.Add(new OracleParameter("usrid2", userid));
			arrayListParameters.Add(new OracleParameter("usrid3", userid));

			if (!string.IsNullOrEmpty(draftcode))
			{
				sql += $" WHERE TBL.DRAFTCODE = '{draftcode}' ";
			}

			sql += @"GROUP BY TBL.DRAFTCODE, TBL.UNITKERJAID, TBL.PERIHAL, TBL.SIFATSURAT, TBL.TIPESURAT, TBL.USERBUAT, TBL.USERUBAH, TBL.TANGGALBUAT, TBL.TANGGALUBAH, TBL.STATUS, TBL.NAMABUAT, TBL.NAMAUBAH, TBL.KETERANGAN, TBL.PENANDATANGAN, TBL.NOTIFIKASI, TBL.FORORDER, CASE WHEN KDH.PESAN IS NOT NULL THEN 1 ELSE 0 END, KRD.STATUS, KRD.TANGGAL
					ORDER BY TBL.FORORDER DESC
					";
			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				records = ctx.Database.SqlQuery<ListDraft>(sql,parameters).ToList();
			}
			return records;
		}

		public List<ListDraft> GetMyKonsep(string userid, string unitkerjaid, string status, string draftcode = null)
        {
            var datas = new List<ListDraft>();
            string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();

			string sql = $@"SELECT
								TD.DRAFTCODE,
								TD.TIPESURAT,
								TD.SIFATSURAT,
								NVL(TD.PERIHAL,'-') AS PERIHAL,
								TO_CHAR( TL.LOGTIME, 'DD/MM/YYYY HH24:MI' ) AS TANGGALBUAT,
								TO_CHAR( TD.UPDTIME, 'DD/MM/YYYY HH24:MI' ) AS TANGGALUBAH
							FROM
								{skema}.TBLDRAFTSURAT TD
								INNER JOIN {skema}.TBLLOGDRAFT TL ON TL.DRAFTCODE = TD.DRAFTCODE 
								AND TL.LOGTEXT = 'New' 
								AND TL.USERID = :usrid 
							WHERE
								TD.STATUS != 'D' AND TD.STATUS = :status AND TD.UNITKERJAID = :ukid";
			arrayListParameters.Add(new OracleParameter("usrid", userid));
			arrayListParameters.Add(new OracleParameter("status", status));
			arrayListParameters.Add(new OracleParameter("ukid", unitkerjaid));
            if (!string.IsNullOrEmpty(draftcode))
            {
				sql += " AND TD.DRAFTCODE = :draftcode ";
				arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
			}

			sql += " ORDER BY TL.LOGTIME DESC ";
			using (var ctx = new BpnDbContext())
            {
				var parameters = arrayListParameters.OfType<object>().ToArray();
				datas = ctx.Database.SqlQuery<ListDraft>(sql, parameters).ToList();
			}

			return datas;
		}

        public List<ListDraft> GetFinalDraft(string unitkerja, string draftcode, int from = 0, int to = 0)
		{
			var records = new List<ListDraft>();
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT
								ROW_NUMBER() OVER(ORDER BY TDS.STATUS ASC, TDS.UPDTIME DESC) RNUMBER,
								COUNT(1) OVER() TOTAL,
								TPS.DRAFTCODE, 
								TDS.PERIHAL, 
								TDS.SIFATSURAT, 
								TDS.TIPESURAT, 
								TDS.STATUS 
							FROM {skema}.TBLPENANDATANGANDRAFTSURAT TPS 
							INNER JOIN 
								(SELECT TB.* FROM 
									(SELECT DRAFTCODE, MAX(URUT) OVER (PARTITION BY DRAFTCODE) AS MAX FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE STATUS != 'D') TB 
								GROUP BY TB.DRAFTCODE, TB.MAX) TBB 
								ON TPS.URUT = TBB.MAX AND TPS.DRAFTCODE = TBB.DRAFTCODE AND TPS.STATUS != 'D'
							INNER JOIN {skema}.TBLDRAFTSURAT TDS 
								ON TPS.DRAFTCODE = TDS.DRAFTCODE 
								AND TDS.STATUS IN ('A', 'F')
							INNER JOIN JABATAN JB 
								ON TPS.PROFILEID = JB.PROFILEID
								AND JB.UNITKERJAID IN ({unitkerja})";

			if (!string.IsNullOrEmpty(draftcode))
            {
				sql += " WHERE AND TDS.DRAFTCODE = :draftcode ";
				arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
			}
			sql += @" ORDER BY TDS.STATUS ASC, TDS.UPDTIME DESC ";

            if (to > 0)
            {
				sql = $"SELECT TB.* FROM ({sql}) TB WHERE RNUMBER BETWEEN :fromthis AND :tothis";
				arrayListParameters.Add(new OracleParameter("fromthis", from));
				arrayListParameters.Add(new OracleParameter("tothis", to));
			}

			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				records = ctx.Database.SqlQuery<ListDraft>(sql,parameters).ToList();
			}

			return records;
		}

		public List<ListDraft> GetFinalDraftV2(bool isTU, userIdentity usr, string stage, int from = 0, int to = 0, string srchkey = null)
        {
			var records = new List<ListDraft>();
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			string instr = string.Empty;


			arrayListParameters.Add(new OracleParameter("stage", stage));
			if (isTU)
			{
				List<string> units = GetUnitKerjaFromTU(usr.PegawaiId);
				int i = 1;
				foreach (var u in units)
				{
					instr += string.IsNullOrEmpty(instr) ? $" AND JB.UNITKERJAID IN (:param{i}" : $", :param{i}";
					arrayListParameters.Add(new OracleParameter($"param{i}", u));
					i++;
				}
                if (string.IsNullOrEmpty(instr))
                {
					instr = " AND TL.USERID = :usr ";
					arrayListParameters.Add(new OracleParameter("usr", usr.UserId));
				} else
                {
					instr += ")";
				}
			}
			else
			{
				instr = " AND TL.USERID = :usr ";
				arrayListParameters.Add(new OracleParameter("usr", usr.UserId));
			}

            if (!string.IsNullOrEmpty(srchkey))
            {
				instr += " AND  UPPER(PERIHAL) LIKE '%'|| UPPER(APEX_UTIL.URL_ENCODE(:srchkey)) ||'%'";
				arrayListParameters.Add(new OracleParameter("srchkey", srchkey));
			}

			string sql = $@"SELECT ROW_NUMBER() OVER(ORDER BY TDS.STATUS ASC, TDS.UPDTIME DESC) RNUMBER,
								COUNT(1) OVER() TOTAL,
								TDS.DRAFTCODE, 
								UTL_URL.UNESCAPE(TDS.PERIHAL) AS PERIHAL, 
								TDS.SIFATSURAT, 
								TDS.TIPESURAT, 
								TDS.STATUS,
								TL.USERID,
								TP.PROFILEID,
								JB.UNITKERJAID,
								FLOOR(TO_NUMBER(NVL(SM.ESELONID,'99'), '999')/10) AS ESFILTER
							FROM {skema}.TBLDRAFTSURAT TDS
							LEFT JOIN {skema}.TBLLOGDRAFT TL ON TL.DRAFTCODE = TDS.DRAFTCODE AND TL.LOGTEXT = 'New'
							LEFT JOIN {skema}.TBLPENANDATANGANDRAFTSURAT TP ON TP.DRAFTCODE = TDS.DRAFTCODE AND TP.TIPE = '1' AND TP.STATUS != 'D'
							LEFT JOIN SIMPEG_2702.SIAP_VW_PEGAWAI SM ON SM.NIPBARU = TP.PEGAWAIID
							LEFT JOIN JABATAN JB ON TP.PROFILEID = JB.PROFILEID
							WHERE TDS.STATUS = :stage AND TP.PROFILEID IS NOT NULL {instr} 
							ORDER BY TDS.STATUS ASC, TDS.UPDTIME DESC";
			if (to > 0)
			{
				sql = $"SELECT TB.* FROM ({sql}) TB WHERE RNUMBER BETWEEN :fromthis AND :tothis";
				arrayListParameters.Add(new OracleParameter("fromthis", from));
				arrayListParameters.Add(new OracleParameter("tothis", to));
			}

			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				records = ctx.Database.SqlQuery<ListDraft>(sql, parameters).ToList();
			}
			return records;
		}

		//cari unitkerja dari tu
		public List<string> GetUnitKerjaFromTU(string pegawaiid)
        {
			string skema = OtorisasiUser.NamaSkema;
			var list = new List<string>();
			if (!string.IsNullOrEmpty(pegawaiid))
            {
				var arrayListParameters = new ArrayList();
				string sql = @"SELECT DISTINCT
									JB.UNITKERJAID 
								FROM
									JABATAN JB 
								WHERE
									JB.PROFILEIDTU IN (
									SELECT
										JP.PROFILEID 
									FROM
										JABATANPEGAWAI JP
										LEFT JOIN JABATAN JB ON JP.PROFILEID = JB.PROFILEID 
										AND NVL( JB.SEKSIID, 'X' ) <> 'A800' 
									WHERE
										JP.PEGAWAIID = :pegawaiid1 
										AND JP.PROFILEID NOT IN ( 'A81001', 'A81002', 'A81003', 'A81004', 'A80100', 'A80300', 'A80400', 'A80500', 'B80100' ) 
										AND JB.PROFILEID = JB.PROFILEIDTU 
										AND ( JP.VALIDSAMPAI IS NULL OR TRUNC( CAST( JP.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
									) 
									AND JB.PROFILEID NOT IN (
									SELECT
										JP.PROFILEID 
									FROM
										JABATANPEGAWAI JP
										LEFT JOIN JABATAN JB ON JP.PROFILEID = JB.PROFILEID 
										AND NVL( JB.SEKSIID, 'X' ) <> 'A800' 
									WHERE
										JP.PEGAWAIID = :pegawaiid2 
										AND JP.PROFILEID NOT IN ( 'A81001', 'A81002', 'A81003', 'A81004', 'A80100', 'A80300', 'A80400', 'A80500', 'B80100' ) 
										AND JB.PROFILEID = JB.PROFILEIDTU 
									AND ( JP.VALIDSAMPAI IS NULL OR TRUNC( CAST( JP.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
									)";
				arrayListParameters.Add(new OracleParameter("pegawaiid1", pegawaiid));
				arrayListParameters.Add(new OracleParameter("pegawaiid2", pegawaiid));
				using (var ctx = new BpnDbContext())
				{
					var parameters = arrayListParameters.OfType<object>().ToArray();
					list = ctx.Database.SqlQuery<string>(sql,parameters).ToList();
				}
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
					var arrayListParameters = new ArrayList();
					var query = $@"SELECT COUNT(1) FROM {skema}.TBLDRAFTSURAT WHERE DRAFTCODE = :result";
					arrayListParameters.Add(new OracleParameter("result", _result));
					var parameters = arrayListParameters.OfType<object>().ToArray();
					check = ctx.Database.SqlQuery<int>(query, parameters).FirstOrDefault() > 0;
				} while (check);
			}

			return _result;
		}

		public TransactionResult SimpanDraftNaskahDinas(DraftSurat data, string nama, string kantorid, userIdentity user)
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
						var arrayListParameters = new ArrayList();
						var parameters = arrayListParameters.OfType<object>().ToArray();
						DraftSurat old = new DraftSurat();
						if (string.IsNullOrEmpty(data.DraftCode))
						{
							//get new draft code
							data.DraftCode = NewDraftCode();

							//insert new data
							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO {skema}.TBLDRAFTSURAT (DRAFTCODE, UNITKERJAID, PERIHAL, KOPSURAT, KODEARSIP, SIFATSURAT, TIPESURAT, HALAMANTTE, ISISURAT, UPDUSER, PROFILEPENGIRIM, STATUS, UPDTIME)
                                VALUES (:draftcode, :unitkerja, :perihal, :kopsurat, :kodearsip, :sifatsurat, :tipesurat, :halamantte, :isisurat, :upduser, :profilepengirim, 'P', SYSDATE)";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("unitkerja", data.UnitKerjaId));
							arrayListParameters.Add(new OracleParameter("perihal", data.Perihal));
							arrayListParameters.Add(new OracleParameter("kopsurat", data.KopSurat));
							arrayListParameters.Add(new OracleParameter("kodearsip", data.KodeArsip));
							arrayListParameters.Add(new OracleParameter("sifatsurat", data.SifatSurat));
							arrayListParameters.Add(new OracleParameter("tipesurat", data.TipeSurat));
							arrayListParameters.Add(new OracleParameter("halamantte", data.PosisiTTE));
							arrayListParameters.Add(new OracleParameter("isisurat", data.IsiSurat));
							arrayListParameters.Add(new OracleParameter("upduser", user.UserId));
							arrayListParameters.Add(new OracleParameter("profilepengirim", data.ProfilePengirim));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							//insert LOG
							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO {skema}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT)
									 VALUES (RAWTOHEX(SYS_GUID()), :draftcode, :userid, SYSDATE, 'New')";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("userid", user.UserId));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}
						else
						{
							string log = string.Empty;
							string update = string.Empty;
							old = GetDraftSurat(data.DraftCode, data.UnitKerjaId);
							var participant = GetKonsepParticipant(data.DraftCode);
							arrayListParameters = new ArrayList();
							if (string.IsNullOrEmpty(old.Perihal) || !old.Perihal.Equals(data.Perihal))
							{
								log = string.Concat(log, "PERIHAL|!", old.Perihal, "||");
								update = string.Concat(update, "PERIHAL = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.Perihal));
							}
							if (string.IsNullOrEmpty(old.KopSurat) || !old.KopSurat.Equals(data.KopSurat))
							{
								log = string.Concat(log, "KOPSURAT|!", old.KopSurat, "||");
								update = string.Concat(update, "KOPSURAT = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.KopSurat));
							}
							if (string.IsNullOrEmpty(old.KodeArsip) || !old.KodeArsip.Equals(data.KodeArsip))
							{
								log = string.Concat(log, "KODEARSIP|!", old.KodeArsip, "||");
								update = string.Concat(update, "KODEARSIP = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.KodeArsip));
							}
							if (string.IsNullOrEmpty(old.SifatSurat) || !old.SifatSurat.Equals(data.SifatSurat))
							{
								log = string.Concat(log, "SIFATSURAT|!", old.SifatSurat, "||");
								update = string.Concat(update, "SIFATSURAT = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.SifatSurat));
							}
							if (string.IsNullOrEmpty(old.TipeSurat) || !old.TipeSurat.Equals(data.TipeSurat))
							{
								log = string.Concat(log, "TIPESURAT|!", old.TipeSurat, "||");
								update = string.Concat(update, "TIPESURAT = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.TipeSurat));
							}
							if (string.IsNullOrEmpty(old.PosisiTTE) || !old.PosisiTTE.Equals(data.PosisiTTE))
							{
								log = string.Concat(log, "HALAMANTTE|!", old.PosisiTTE, "||");
								update = string.Concat(update, "HALAMANTTE = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.PosisiTTE));
							}
							if (string.IsNullOrEmpty(old.IsiSurat) || !old.IsiSurat.Equals(data.IsiSurat))
							{
								log = string.Concat(log, "ISISURAT|!", old.IsiSurat, "||");
								update = string.Concat(update, "ISISURAT = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.IsiSurat));
							}
							if (string.IsNullOrEmpty(old.ProfilePengirim) || !old.ProfilePengirim.Equals(data.ProfilePengirim))
							{
								log = string.Concat(log, "PROFILEPENGIRIM|!", old.ProfilePengirim, "||");
								update = string.Concat(update, "PROFILEPENGIRIM = :data,");
								arrayListParameters.Add(new OracleParameter("data", data.ProfilePengirim));
							}
							if (!string.IsNullOrEmpty(update))
							{
								//update
								sql = $@"UPDATE {skema}.TBLDRAFTSURAT SET
                                      {update} UPDTIME = SYSDATE, UPDUSER = :upduser
									  WHERE DRAFTCODE = :draftcode AND UNITKERJAID = :unitkerja ";
								arrayListParameters.Add(new OracleParameter("upduser", user.UserId));
								arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
                                if (participant.Contains(user.UserId))
                                {
									arrayListParameters.Add(new OracleParameter("unitkerja", old.UnitKerjaId));
								} 
								else
                                {
									arrayListParameters.Add(new OracleParameter("unitkerja", user.UnitKerjaId));
								}								
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);

								if (old.Status != "P")
								{
									arrayListParameters = new ArrayList();
									sql = $@"INSERT INTO {skema}.TBLLOGDRAFT (LOGDRAFTID, DRAFTCODE, USERID, LOGTIME, LOGTEXT, LOGDETAIL)
											 VALUES (RAWTOHEX(SYS_GUID()), :draftcode, :upduser, SYSDATE, 'Update', :log)";
									arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
									arrayListParameters.Add(new OracleParameter("upduser", user.UserId));
									arrayListParameters.Add(new OracleParameter("log", log));
									parameters = arrayListParameters.OfType<object>().ToArray();
									ctx.Database.ExecuteSqlCommand(sql, parameters);

                                    if (participant.Contains(user.UserId) && (user.UserId != data.UserPembuat))
                                    {
										arrayListParameters = new ArrayList();
										sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL)
												 VALUES (RAWTOHEX(SYS_GUID()), :draftcode, '!RESUBMIT!', :upduser, SYSDATE)";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("upduser", user.UserId));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
									}
								}
							}
						}


						//check DraftSuratDetail
						arrayListParameters = new ArrayList();
						sql = $@"SELECT DETAILTEXT AS TEXT , DETAILVALUE AS VALUE FROM {skema}.TBLDRAFTSURATDETAIL WHERE DRAFTCODE = :draftcode";
						arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						var details = ctx.Database.SqlQuery<DraftSuratDetail>(sql, parameters).ToList();


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
						var cOpttj = true;
						var cOadhoc = true;

						//check and update
						if (details.Count > 0)
						{
							string updateDetails = $"UPDATE {skema}.TBLDRAFTSURATDETAIL SET DETAILVALUE = :value WHERE DRAFTCODE = :draftcode AND DETAILTEXT = :text";
							foreach (var detail in details)
							{
								if (data.TipeSurat.Equals("Surat Undangan"))
								{
									if (detail.Text.Equals("TanggalUndangan") && !string.IsNullOrEmpty(data.TanggalUndangan))
									{
										arrayListParameters = new ArrayList();
										arrayListParameters.Add(new OracleParameter("value", data.TanggalUndangan));
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "TanggalUndangan"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(updateDetails, parameters);
										cTanggalUnd = false;
									}
									if (detail.Text.Equals("TempatUndangan") && !string.IsNullOrEmpty(data.TempatUndangan))
									{
										arrayListParameters = new ArrayList();
										arrayListParameters.Add(new OracleParameter("value", data.TempatUndangan));
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "TempatUndangan"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(updateDetails, parameters);
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
										arrayListParameters = new ArrayList();
										arrayListParameters.Add(new OracleParameter("value", "Tanpa Gelar"));
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "OptionTTD"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(updateDetails, parameters);
										cOptTTD = false;
									}
									else
									{
										arrayListParameters = new ArrayList();
										sql = $@"UPDATE {skema}.TBLDRAFTSURATDETAIL 
												 SET DETAILVALUE = NULL WHERE DRAFTCODE =:draftcode AND DETAILTEXT = :text";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "OptionTTD"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
										cOptTTD = false;
									}
								}
								if (detail.Text.Equals("OptionAn"))
								{
									if (!string.IsNullOrEmpty(data.AtasNama))
									{
										arrayListParameters = new ArrayList();
										arrayListParameters.Add(new OracleParameter("value", data.AtasNama));
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "OptionAn"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(updateDetails, parameters);
										cOptAn = false;
									}
									else
									{
										arrayListParameters = new ArrayList();
										sql = $@"UPDATE {skema}.TBLDRAFTSURATDETAIL 
												 SET DETAILVALUE = NULL WHERE DRAFTCODE =:draftcode AND DETAILTEXT = :text";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "OptionAn"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
										cOptAn = false;
									}
								}
								if (detail.Text.Equals("OptionTujuan"))
								{
									if (data.TujuanTerlampir)
									{
										arrayListParameters = new ArrayList();
										arrayListParameters.Add(new OracleParameter("value", "Terlampir"));
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "OptionTujuan"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(updateDetails, parameters);
										cOpttj = false;
									}
									else
									{
										arrayListParameters = new ArrayList();
										sql = $@"UPDATE {skema}.TBLDRAFTSURATDETAIL 
												 SET DETAILVALUE = NULL WHERE DRAFTCODE =:draftcode AND DETAILTEXT = :text";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "OptionTujuan"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
										cOpttj = false;
									}
								}
								if (detail.Text.Equals("JabatanAdhoc"))
								{
									if (!string.IsNullOrEmpty(data.JabatanAdhoc))
									{
										arrayListParameters = new ArrayList();
										arrayListParameters.Add(new OracleParameter("value", data.JabatanAdhoc));
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "JabatanAdhoc"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(updateDetails, parameters);
										cOadhoc = false;
									}
									else
									{
										arrayListParameters = new ArrayList();
										sql = $@"UPDATE {skema}.TBLDRAFTSURATDETAIL 
												 SET DETAILVALUE = NULL WHERE DRAFTCODE =:draftcode AND DETAILTEXT = :text";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("text", "JabatanAdhoc"));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
										cOadhoc = false;
									}
								}
							}
						}

						//if new then create
						string createInsert = $@"INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
												 VALUES(:draftcode, :text, :value)";
						if (data.TipeSurat.Equals("Surat Undangan"))
						{
							if (cTanggalUnd && !string.IsNullOrEmpty(data.TanggalUndangan))
							{
								arrayListParameters = new ArrayList();
								arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
								arrayListParameters.Add(new OracleParameter("text", "TanggalUndangan"));
								arrayListParameters.Add(new OracleParameter("value", data.TanggalUndangan));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(createInsert, parameters);
							}
							if (cTempatlUnd && !string.IsNullOrEmpty(data.TempatUndangan))
							{
								arrayListParameters = new ArrayList();
								arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
								arrayListParameters.Add(new OracleParameter("text", "TempatUndangan"));
								arrayListParameters.Add(new OracleParameter("value", data.TempatUndangan));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(createInsert, parameters);
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
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("text", "OptionTTD"));
							arrayListParameters.Add(new OracleParameter("value", "Tanpa Gelar"));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(createInsert, parameters);
						}
						if (cOptAn && !string.IsNullOrEmpty(data.AtasNama))
						{
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("text", "OptionAn"));
							arrayListParameters.Add(new OracleParameter("value", data.AtasNama));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(createInsert, parameters);
						}
						if (cOpttj && data.TujuanTerlampir)
						{
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("text", "OptionTujuan"));
							arrayListParameters.Add(new OracleParameter("value", "Terlampir"));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(createInsert, parameters);
						}
						if (cOadhoc && !string.IsNullOrEmpty(data.JabatanAdhoc))
						{
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("text", "JabatanAdhoc"));
							arrayListParameters.Add(new OracleParameter("value", data.JabatanAdhoc));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(createInsert, parameters);
						}


						if (!string.IsNullOrEmpty(data.listTujuan))
						{
							string[] tujuan = data.listTujuan.Split('|');
							int x = 1;

							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.TBLDRAFTSURATTUJUAN
									 SET URUTAN = 0 WHERE DRAFTCODE = :draftcode AND TEMBUSAN = :tembusan ";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("tembusan", '0'));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql,parameters);

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

								arrayListParameters = new ArrayList();
								sql = $@"INSERT INTO {skema}.TBLDRAFTSURATTUJUAN (DRAFTCODE, URUTAN, NAMA, TEMBUSAN, PROFILID)
										 VALUES (:draftcode, :urutan, :nama, :tembusan, :profileid)";
								arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
								arrayListParameters.Add(new OracleParameter("urutan", x));
								arrayListParameters.Add(new OracleParameter("nama", fnama));
								arrayListParameters.Add(new OracleParameter("tembusan", '0'));
								arrayListParameters.Add(new OracleParameter("profileid", fprofileid));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql,parameters);
								x += 1;
							}
						}
						else if (string.IsNullOrEmpty(data.listTujuan) && old.Tujuan != null && old.Tujuan.Count > 0)
						{
							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.TBLDRAFTSURATTUJUAN
									 SET URUTAN = 0 WHERE DRAFTCODE = :draftcode AND TEMBUSAN = :tembusan ";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("tembusan", '0'));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}


						if (!string.IsNullOrEmpty(data.listTembusan))
						{
							string[] tembusan = data.listTembusan.Split('|');
							int x = 1;

							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.TBLDRAFTSURATTUJUAN
									 SET URUTAN = 0 WHERE DRAFTCODE = :draftcode AND TEMBUSAN = :tembusan ";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("tembusan", '1'));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							foreach (var t in tembusan)
							{
								string fnama = t.Split('%')[0];
								string fprofileid = t.Split('%')[1];
								arrayListParameters = new ArrayList();
								sql = $@"INSERT INTO {skema}.TBLDRAFTSURATTUJUAN (DRAFTCODE, URUTAN, NAMA, TEMBUSAN, PROFILID)
										 VALUES (:draftcode, :urutan, :nama, :tembusan, :profileid)";
								arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
								arrayListParameters.Add(new OracleParameter("urutan", x));
								arrayListParameters.Add(new OracleParameter("nama", fnama));
								arrayListParameters.Add(new OracleParameter("tembusan", '1'));
								arrayListParameters.Add(new OracleParameter("profileid", fprofileid));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql,parameters);
								x += 1;
							}
						}
						else if (string.IsNullOrEmpty(data.listTembusan) && old.Tembusan != null && old.Tembusan.Count > 0)
						{
							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.TBLDRAFTSURATTUJUAN
									 SET URUTAN = 0 WHERE DRAFTCODE = :draftcode AND TEMBUSAN = :tembusan ";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							arrayListParameters.Add(new OracleParameter("tembusan", '1'));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}


						if (data.isHaveLampiran || old.isHaveLampiran)
						{
							//set status delete semua
							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.TBLLAMPIRANDRAFTSURAT
									 SET STATUS = 'D' WHERE DRAFTCODE = :draftcode";
							arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							if (data.lstFileLampiran != null)
                            {
								//cari yang baru
								foreach (var id in data.lstFileLampiran)
								{
									if (id.save)
									{
										arrayListParameters = new ArrayList();
										sql = $@"UPDATE {skema}.TBLLAMPIRANDRAFTSURAT
											 SET STATUS = 'A' WHERE DRAFTCODE = :draftcode AND LAMPIRANID = :lampiran";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("lampiran", id.namaFile));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
									}
									else
									{
										//simpen file fisik dulu

										int versi = 0;
										string lampiranid = NewGuID();

										Stream stream = new MemoryStream(id.ObjectFile);

										var reqmessage = new HttpRequestMessage();
										var content = new MultipartFormDataContent();
										var tipe = "DokumenTTE";

										content.Add(new StringContent(kantorid), "kantorId");
										content.Add(new StringContent(tipe), "tipeDokumen");
										content.Add(new StringContent(lampiranid), "dokumenId");
										content.Add(new StringContent(".pdf"), "fileExtension");
										content.Add(new StringContent(versi.ToString()), "versionNumber");
										content.Add(new StreamContent(stream), "file", id.namaFile);

										reqmessage.Method = HttpMethod.Post;
										reqmessage.Content = content;
										reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

										using (var client = new HttpClient())
										{
											var reqresult = client.SendAsync(reqmessage).Result;
											if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK)
											{
												arrayListParameters = new ArrayList();
												sql = $@"INSERT INTO {skema}.TBLLAMPIRANDRAFTSURAT (LAMPIRANID, DRAFTCODE, STATUS, UPDTIME, UPDUSER)
													 VALUES (:lampiranid, :draftcode, :status, SYSDATE, :upduser)";
												arrayListParameters.Add(new OracleParameter("lampiranid", lampiranid));
												arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
												arrayListParameters.Add(new OracleParameter("status", "A"));
												arrayListParameters.Add(new OracleParameter("upduser", user.UserId));
												parameters = arrayListParameters.OfType<object>().ToArray();
												ctx.Database.ExecuteSqlCommand(sql, parameters);
											}
											else
											{
												tr.Status = false;
												tr.Pesan = "Gagal Membuat Surat, ada file lampiran yang bermasalah\nHarap cek ulang lampiran anda.";
												throw new Exception(tr.Pesan);
											}
										}
									}
								}
							}
						}

						bool isDelete = false;
						if (data.TTE != null && data.TTE.Count > 0)
						{
							
							var ForUrut = 1;
                            if (string.IsNullOrEmpty(old.DraftCode))
                            {
								foreach (var tte in data.TTE)
								{
									if (tte.Tipe == "1")
									{
										var jumlahTTE = data.TTE.Count();
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
									", skema, data.DraftCode, tte.PenandatanganId, tte.ProfileId, tte.Tipe, jumlahTTE, data.UserPembuat);
										ctx.Database.ExecuteSqlCommand(sql);
									}
									else
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
									", skema, data.DraftCode, tte.PenandatanganId, tte.ProfileId, tte.Tipe, ForUrut, data.UserPembuat);
										ctx.Database.ExecuteSqlCommand(sql);
										ForUrut++;
									}

								}
							}						
							else
                            {
								//cek data lama
								foreach (var t in old.TTE)
                                {
									bool found = false;
									foreach(var d in data.TTE)
                                    {
										if(d.PenandatanganId == t.PenandatanganId)
                                        {
											string namajabtan = new SuratModel().GetNamaJabatan(d.ProfileId);
											sql = $@"UPDATE {skema}.TBLPENANDATANGANDRAFTSURAT 
													 SET PROFILEID = '{d.ProfileId}', JABATAN = '{namajabtan}',
													 TIPE = '{d.Tipe}', URUT = {d.Urut}, UPDTIME = SYSDATE, STATUS = 'P'
													 WHERE DRAFTCODE = '{data.DraftCode}' AND USERID = '{d.PenandatanganId}' ";
											ctx.Database.ExecuteSqlCommand(sql);
											found = true;
                                        }
                                        else
                                        {
											found = (found) ? true : false;
                                        }
                                    }

                                    if (!found)
                                    {
										sql = $@"UPDATE {skema}.TBLPENANDATANGANDRAFTSURAT 
												SET STATUS = 'D' WHERE DRAFTCODE = '{data.DraftCode}' AND USERID = '{t.PenandatanganId}'";
										ctx.Database.ExecuteSqlCommand(sql);
									}
								}

								//cek ada input baru
								foreach (var d in data.TTE)
                                {
									bool check = old.TTE.Any(x => x.PenandatanganId == d.PenandatanganId);
                                    if (!check)
                                    {
										arrayListParameters = new ArrayList();
										sql = $@"SELECT COUNT(1) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE = :draftcode AND USERID = :userid ";
										arrayListParameters.Add(new OracleParameter("draftcode", data.DraftCode));
										arrayListParameters.Add(new OracleParameter("userid", d.PenandatanganId));
										parameters = arrayListParameters.OfType<object>().ToArray();
										var checkttd = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();

										if(checkttd > 0)
                                        {
											string namajabtan = new SuratModel().GetNamaJabatan(d.ProfileId);
											sql = $@"UPDATE {skema}.TBLPENANDATANGANDRAFTSURAT 
													 SET PROFILEID = '{d.ProfileId}', JABATAN = '{namajabtan}',
													 TIPE = '{d.Tipe}', URUT = {d.Urut}, UPDTIME = SYSDATE, STATUS = 'P'
													 WHERE DRAFTCODE = '{data.DraftCode}' AND USERID = '{d.PenandatanganId}' ";
											ctx.Database.ExecuteSqlCommand(sql);

										} else
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
											", skema, data.DraftCode, d.PenandatanganId, d.ProfileId, d.Tipe, d.Urut, data.UserPembuat);
												ctx.Database.ExecuteSqlCommand(sql);
										}										
									}
								}
							}
							if (old.Status == "W")
                            {
								//var KoorDraft = ctx.Database.SqlQuery<KoordinasiDraft>($"SELECT KOR_ID,	DRAFTCODE, VERIFIKATOR, USERID, V_RANK AS RANK, V_MAX AS MAX, STATUS AS STATUSKOORDINASI FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE='{data.DraftCode}'").ToList();
								UserTTE penandatangan = data.TTE.Find(item => item.Tipe.Equals("1"));
								foreach (var kor in data.TTE)
                                {
									bool check = old.TTE.Any(x => x.PenandatanganId == kor.PenandatanganId);
									if (!check)
                                    {
										isDelete = true;
										break;
                                    }
								}

								if (isDelete)
                                {
									sql = string.Format(@"
									UPDATE {0}.KOORDINASIDRAFT 
									SET STATUS = 'D'
									WHERE DRAFTCODE = '{1}'
									", skema, data.DraftCode);
									ctx.Database.ExecuteSqlCommand(sql);
								}
							}
						}

						tc.Commit();
                        if (isDelete && old.Status == "W")
                        {
							var pengajuanUlang = PengajuanDraft(data.DraftCode, user.UnitKerjaId, user.UserId, user.ProfileIdTU);
						}
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

		public DraftSurat GetDraftSurat(string draftcode, string unitkerjaid)
		{
			var result = new DraftSurat();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			object[] parameters = arrayListParameters.OfType<object>().ToArray();


			string query = $@"SELECT
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
							FROM {skema}.TBLDRAFTSURAT TDS
							WHERE
								TDS.DRAFTCODE = :draftcode AND TDS.STATUS <> 'D' ";
			arrayListParameters.Add(new OracleParameter("draftcode", draftcode));

			using (var ctx = new BpnDbContext())
			{
				try
				{
					parameters = arrayListParameters.OfType<object>().ToArray();
					result = ctx.Database.SqlQuery<DraftSurat>(query,parameters).First();
					if (!string.IsNullOrEmpty(result.DraftCode))
					{
						//get user data
						var userdata = new DraftSuratDetail();
						arrayListParameters = new ArrayList();
						query = $@"SELECT USERID AS TEXT, to_char(LOGTIME,'DD/MM/YYYY HH24:MI') as VALUE FROM {skema}.TBLLOGDRAFT WHERE DRAFTCODE = :draftcode AND LOGTEXT = 'New' ORDER BY LOGTIME";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						userdata = ctx.Database.SqlQuery<DraftSuratDetail>(query, parameters).FirstOrDefault();
						result.UserPembuat = userdata.Text;
						result.TanggalDibuat = userdata.Value;

						//get from table detail
						arrayListParameters = new ArrayList();
						query = $@"SELECT DETAILTEXT AS TEXT , DETAILVALUE AS VALUE FROM {skema}.TBLDRAFTSURATDETAIL WHERE DRAFTCODE = :draftcode";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						var details = ctx.Database.SqlQuery<DraftSuratDetail>(query, parameters).ToList();

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
							if (detail.Text.Equals("JabatanAdhoc"))
							{
								result.JabatanAdhoc = string.IsNullOrEmpty(detail.Value) ? null : detail.Value;
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

						//tte
						arrayListParameters = new ArrayList();
						query = $@"SELECT TPS.DRAFTCODE, TPS.USERID AS PENANDATANGANID, TPS.PEGAWAIID, TPS.NAMA, TPS.PROFILEID, TPS.JABATAN, TPS.URUT, TPS.TIPE, TPS.STATUS 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TPS 
									WHERE TPS.DRAFTCODE = :draftcode AND TPS.STATUS != 'D' ORDER BY URUT";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						result.TTE = ctx.Database.SqlQuery<UserTTE>(query, parameters).ToList();


						//tujuan dan tembusan
						var tujuantembusan = new List<DraftSuratDetail>();
						arrayListParameters = new ArrayList();
						query = $@" SELECT
                                TDS.NAMA||(CASE WHEN TDS.PROFILID IS NULL THEN '' ELSE CONCAT('%', TDS.PROFILID) END) AS TEXT,
								TDS.TEMBUSAN AS VALUE
                            FROM {skema}.TBLDRAFTSURATTUJUAN TDS
                            WHERE
	                            TDS.DRAFTCODE = :draftcode AND TDS.URUTAN != 0 ORDER BY URUTAN";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						tujuantembusan = ctx.Database.SqlQuery<DraftSuratDetail>(query, parameters).ToList();
						result.Tujuan = new List<string>();
						result.Tembusan = new List<string>();
						foreach(var t in tujuantembusan)
                        {
							if(t.Value == "1")
                            {
								result.Tembusan.Add(t.Text);
							}
							else
                            {
								result.Tujuan.Add(t.Text);
                            }
                        }


						arrayListParameters = new ArrayList();
						query = $@"SELECT LAMPIRANID FROM {skema}.TBLLAMPIRANDRAFTSURAT WHERE DRAFTCODE = :draftcode AND STATUS = 'A' ";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						result.lstLampiranId = ctx.Database.SqlQuery<string>(query, parameters).ToList();
						result.isHaveLampiran = result.lstLampiranId.Count > 0 ? true : false;
                        if (result.isHaveLampiran)
                        {
							//versilama biar masih bisa jalan
							result.LampiranId = result.lstLampiranId[0];
						}
					}
				}
				catch (Exception ex)
				{
					string msg = ex.Message;
				}
			}

			return result;
		}


		public KopSurat getKopDetail(string id)
		{
			var result = new KopSurat();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			string query = $@"SELECT
								UNITKERJAID, NAMAKANTOR_L1||' '||NAMAKANTOR_L2 AS UNITKERJANAME,  NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL
							  FROM
								{skema}.TBLKANTOR
							  WHERE
								UNITKERJAID = :Id";
			arrayListParameters.Add(new NpgsqlParameter("Id", id));
			using (var ctx = new PostgresDbContext())
			{
				try
				{
					object[] parameters = arrayListParameters.OfType<object>().ToArray();
					result = ctx.Database.SqlQuery<KopSurat>(query, parameters).First();
				}
				catch (Exception ex)
				{
					string msg = ex.Message;
				}
			}

			return result;
		}

		public List<KopSurat> getListAdhoc(string unitkerja = null)
        {
			List<KopSurat> result = new List<KopSurat>();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			using (var ctx = new PostgresDbContext())
			{
				string query = string.Empty;

				if (string.IsNullOrEmpty(unitkerja))
                {
					query =		$@"SELECT
									UNITKERJAID, NAMAKANTOR_L1||' '||NAMAKANTOR_L2 AS UNITKERJANAME,  NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL
								  FROM
									{skema}.TBLKANTOR
								  WHERE
									SUBSTR(UNITKERJAID,0,1) = 'A'
								  ORDER BY
									UNITKERJAID ASC";
				} else
                {
					query = $@"SELECT
									UNITKERJAID, NAMAKANTOR_L1||' '||NAMAKANTOR_L2 AS UNITKERJANAME,  NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL
								  FROM
									{skema}.TBLKANTOR
								  WHERE
									SUBSTR(UNITKERJAID,0,1) = 'A' AND  SUBSTR(UNITKERJAID,4,:lengthunitkerja) = :unitkerja
								  ORDER BY
									UNITKERJAID ASC";
					arrayListParameters.Add(new NpgsqlParameter("lengthunitkerja", unitkerja.Length));
					arrayListParameters.Add(new NpgsqlParameter("unitkerja", unitkerja));
				}
				object[] parameters = arrayListParameters.OfType<object>().ToArray();
				result = ctx.Database.SqlQuery<KopSurat>(query, parameters).ToList();
			}
			return result;
		}
		public TransactionResult deleteKopSurat(string kopsuratUkid)
        {
			TransactionResult tr = new TransactionResult() { Status = false, Pesan = "Id belum dipilih" };
			string query = string.Empty;
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			object[] parameters = arrayListParameters.OfType<object>().ToArray();
			if (!string.IsNullOrEmpty(kopsuratUkid))
            {				
				using (var ctx = new PostgresDbContext())
                {
					query = $"SELECT COUNT(UNITKERJAID) FROM {skema}.TBLKANTOR WHERE UNITKERJAID = :KopsuratUkid";
					arrayListParameters.Add(new OracleParameter("KopsuratUkid", kopsuratUkid));
					parameters = arrayListParameters.OfType<object>().ToArray();
					int count = ctx.Database.SqlQuery<int>(query,parameters).FirstOrDefault();

					arrayListParameters = new ArrayList();
					query = $@"UPDATE {skema}.TBLKANTOR SET UNITKERJAID = :deletedKop WHERE UNITKERJAID = :KopsuratUkid";
					arrayListParameters.Add(new NpgsqlParameter("deletedKop", "D"+count.ToString()+kopsuratUkid));
					arrayListParameters.Add(new NpgsqlParameter("KopsuratUkid", kopsuratUkid));
					using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                    {
						try
						{
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(query, parameters);
							tc.Commit();
							tr.Status = true;
							tr.Pesan = "Data berhasil di hapus";
						}
						catch (Exception e)
						{
							tc.Rollback();
							tr.Pesan = e.Message;
						}
						finally
						{
							tc.Dispose();
							ctx.Dispose();
						}
					}
				}               
			}
			return tr;
		}

		public TransactionResult simpanKopAdhoc(KopSurat kopSurat)
        {
			TransactionResult tr = new TransactionResult() { Status = false, Pesan = "Data tidak lengkap" };
			string query = string.Empty;
			string skema = OtorisasiUser.NamaSkema;
			string unitkerjadhoc = string.Empty;
			ArrayList arrayListParameters = new ArrayList();
			if(!string.IsNullOrEmpty(kopSurat.UnitKerjaId ?? kopSurat.NamaKantor_L1 ?? kopSurat.Alamat))
            {
				using (var ctx = new PostgresDbContext())
				{
					using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
					{
						if (kopSurat.UnitKerjaId.Substring(0, 1) == "A")
						{
							query = $@"	UPDATE {skema}.TBLKANTOR
									SET NAMAKANTOR_L1 = :namakantor_l1,
										NAMAKANTOR_L2 = :namakantor_l2,
										ALAMAT = :alamat,
										TELEPON = :telepon,
										EMAIL = :email
									WHERE UNITKERJAID = :unitkerjaid
							";
							tr.Pesan = "Data berhasil di perbaharui";
							arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", kopSurat.UnitKerjaId));
						}
						else
						{
							query = $@"	INSERT INTO {skema}.TBLKANTOR
									(UNITKERJAID, NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL, UPDTIME)
									VALUES
									(:unitkerjaid, :namakantor_l1, :namakantor_l2, :alamat, :telepon, :email, SYSDATE)						
							";
							tr.Pesan = "Data berhasil ditambahkan";
							unitkerjadhoc = getUnitKerjaIdAdhoc(kopSurat.UnitKerjaId);
                            if (string.IsNullOrEmpty(unitkerjadhoc))
                            {
								tr.Pesan = "Gagal Mendapatkan ID Kop";
								return tr;
                            } else
                            {
								arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", unitkerjadhoc));
							}							
						}
						arrayListParameters.Add(new NpgsqlParameter("namakantor_l1", kopSurat.NamaKantor_L1));
						arrayListParameters.Add(new NpgsqlParameter("namakantor_l2", kopSurat.NamaKantor_L2));
						arrayListParameters.Add(new NpgsqlParameter("alamat", kopSurat.Alamat));
						arrayListParameters.Add(new NpgsqlParameter("telepon", kopSurat.Telepon));
						arrayListParameters.Add(new NpgsqlParameter("email", kopSurat.Email));

						try
						{
							object[] parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(query, parameters);
							tc.Commit();
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
			}
			return tr;
		}

        public string getUnitKerjaIdAdhoc(string unitkerjaid)
        {
            string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			string query = $@"SELECT SUBSTR(UNITKERJAID,2,2) FROM {skema}.TBLKANTOR WHERE SUBSTR(UNITKERJAID,0,1) = 'A' AND SUBSTR(UNITKERJAID,4,LENGTH(UNITKERJAID)-3) = :unitkerjaid ORDER BY SUBSTR(UNITKERJAID,2,2) DESC ";
			arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", unitkerjaid));
			string result = string.Empty;
			using (var ctx = new PostgresDbContext())
			{
				object[] parameters = arrayListParameters.OfType<object>().ToArray();
				result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
                if (!string.IsNullOrEmpty(result))
                {
					int reslutInt = int.Parse(result);
					if (reslutInt <= 9 && reslutInt > 0)
                    {
						result = $"A0{reslutInt + 1}{unitkerjaid}";
                    } else if (reslutInt > 9 && reslutInt < 100)
                    {
						result = $"A{reslutInt + 1}{unitkerjaid}";
					} else 
                    {
						result = string.Empty;
                    }
                } else
                {
					result = $"A01{unitkerjaid}";
				}
			}
			return result;
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


						string checknull = ctx.Database.SqlQuery<string>($@" SELECT USERID FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE = '{id}' AND STATUS != 'D' AND ROWNUM = 1 ").FirstOrDefault();

						if (string.IsNullOrEmpty(checknull))
						{
							var penandatnaganId = ctx.Database.SqlQuery<string>($@"SELECT USERID FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{id}' AND TIPE = 1 AND STATUS != 'D'").FirstOrDefault();
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
								var korid = NewGuID();
								var max = ctx.Database.SqlQuery<decimal>($@"SELECT MAX(URUT) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{id}' AND STATUS != 'D' GROUP BY DRAFTCODE ").FirstOrDefault();
								var min = ctx.Database.SqlQuery<decimal>($@"SELECT MIN(URUT) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{id}' AND STATUS != 'D' AND USERID != '{userid}' GROUP BY DRAFTCODE ").FirstOrDefault();

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
                                WHERE TP.DRAFTCODE = '{id}' AND TP.URUT = '{min}' AND TP.STATUS != 'D'";
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

		public List<ListDraft> GetKonsepDibuat(string userid, string unitkerjaid, string status, string searchKey, int from, int to)
		{
			var records = new List<ListDraft>();
			string skema = OtorisasiUser.NamaSkema;

			string sql = $@"SELECT
							ROW_NUMBER() OVER(ORDER BY TLD.LOGTIME DESC) RNUMBER,
							COUNT(1) OVER() TOTAL,
							TDS.DRAFTCODE,
							TDS.UNITKERJAID,
							TDS.PERIHAL,
							TDS.SIFATSURAT,
							TDS.TIPESURAT,
							TLD.USERID USERBUAT,
							TDS.UPDUSER USERUBAH,
							TO_CHAR(TLD.LOGTIME,'DD/MM/YYYY HH24:MI') TANGGALBUAT,
							TO_CHAR(TDS.UPDTIME,'DD/MM/YYYY HH24:MI') TANGGALUBAH,
							TDS.STATUS,
							TDS.PROFILEPENGIRIM
						FROM
							{skema}.TBLDRAFTSURAT TDS
							INNER JOIN {skema}.TBLLOGDRAFT TLD ON TLD.DRAFTCODE = TDS.DRAFTCODE 
							AND TLD.LOGTEXT = 'New' AND TLD.USERID = '{userid}' 
						WHERE
							TDS.STATUS <> 'D' AND TDS.STATUS <> 'P' 
							AND TDS.UNITKERJAID = '{unitkerjaid}' 
							{(!string.IsNullOrEmpty(searchKey) ? $" AND LOWER(TDS.PERIHAL) LIKE '%{searchKey.ToLower()}%' " : " ")}
			";

			using (var ctx = new BpnDbContext())
			{
				records = ctx.Database.SqlQuery<ListDraft>($"SELECT * FROM ({sql}) WHERE RNUMBER BETWEEN {from} AND {to} ").ToList();
			}

			return records;
		}

		public string GetDokumenElektronikIdFromDraftCode(string dc)
		{
			string skema = System.Web.Mvc.OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			string query = $@"SELECT 
							 DOKUMENELEKTRONIKID
						   FROM {skema}.TBLDOKUMENELEKTRONIK
						   WHERE
							 NAMAFILE = :namafile AND NVL(STATUSHAPUS,'0') = '0' AND KODEFILE != :dc ORDER BY TANGGALDIBUAT DESC";
			arrayListParameters.Add(new OracleParameter("namafile", $"Naskah_Dinas_[{dc}].pdf"));
			arrayListParameters.Add(new OracleParameter("dc", dc));
			try
			{
				using (var ctx = new BpnDbContext())
                {
					object[] parameters = arrayListParameters.OfType<object>().ToArray();
					string dokid = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
					return string.IsNullOrEmpty(dokid) ? "" : dokid;
				}					
			}
			catch
			{
				return "";
			}
		}

		public List<KoordinasiDraft> GetKoordinasiDraft(string draftcode, string status)
		{
			var records = new List<KoordinasiDraft>();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();

			if (!string.IsNullOrEmpty(draftcode))
			{
				using (var ctx = new BpnDbContext())
				{
					string query = $@"SELECT KD.KOR_ID, KD.DRAFTCODE, KD.VERIFIKATOR, KD.USERID, KD.V_RANK AS RANK, 
									  KD.V_MAX AS MAX, KD.STATUS AS STATUSKOORDINASI, 
									  DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS NAMA 
									  FROM {skema}.KOORDINASIDRAFT KD LEFT JOIN PEGAWAI PG ON PG.USERID = KD.USERID WHERE KD.DRAFTCODE = :draftcode AND KD.STATUS != 'D' ";
					arrayListParameters.Add(new OracleParameter("draftcode", draftcode));


					if (!string.IsNullOrEmpty(status))
					{
						query += $" AND KD.STATUS = :status";
						arrayListParameters.Add(new OracleParameter("status", status));
					}
					object[] parameters = arrayListParameters.OfType<object>().ToArray();
					records = ctx.Database.SqlQuery<KoordinasiDraft>(query,parameters).ToList();
				}
			}
			return records;
		}

		public List<UserTTE> GetUserTtd(string draftcode)
		{
			var records = new List<UserTTE>();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(draftcode))
			{
				using (var ctx = new BpnDbContext())
				{
					var sql = $@"SELECT TP.DRAFTCODE, TP.PEGAWAIID, TP.NAMA, TP.PROFILEID, TP.JABATAN, TP.URUT, TP.TIPE, TP.STATUS FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
								WHERE TP.DRAFTCODE = :draftcode AND TP.STATUS != 'D' ORDER BY TO_NUMBER(TP.URUT,'99') ";
					arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
					object[] parameters = arrayListParameters.OfType<object>().ToArray();
					records = ctx.Database.SqlQuery<UserTTE>(sql,parameters).ToList();
				}
			}
			return records;
		}


		public string GetLampiranIdByDraftcode(string draftcode)
		{
			string lampiraind = string.Empty;
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(draftcode))
			{
				using (var ctx = new BpnDbContext())
				{
					var sql = $@"SELECT LAMPIRANID FROM {skema}.TBLLAMPIRANDRAFTSURAT WHERE DRAFTCODE = :draftcode AND STATUS = 'A'";
					arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
					object[] parameters = arrayListParameters.OfType<object>().ToArray();
					lampiraind = ctx.Database.SqlQuery<string>(sql,parameters).FirstOrDefault();
				}
			}
			return lampiraind;
		}


		public List<KoordinasiHist> GetHistroyKoordinasi(string draftcode, string userid)
		{
			var records = new List<KoordinasiHist>();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(draftcode))
			{
				using (var ctx = new BpnDbContext())
				{
					var sql = $@"SELECT KH.KOR_ID AS DRAFTCODE,  KH.KORHIST_ID, KH.PESAN, KH.PSFROM, TO_CHAR(KH.TANGGAL,'HH24:MI DD-MM-YYYY') TANGGAL,  CASE KH.PSFROM WHEN :userid THEN 1 ELSE 0 END isUser, 
								NVL(DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG), PP.NAMA) AS PSFROMNAMA 
								FROM {skema}.KOORDINASIDRAFTHISTORY KH  
								LEFT JOIN PEGAWAI PG ON KH.PSFROM = PG.USERID 
								LEFT JOIN PPNPN PP ON KH.PSFROM = PP.USERID     
								WHERE KH.KOR_ID = :draftcode
								ORDER BY KH.TANGGAL DESC ";
					arrayListParameters.Add(new OracleParameter("userid", userid));
					arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
					object[] parameters = arrayListParameters.OfType<object>().ToArray();
					records = ctx.Database.SqlQuery<KoordinasiHist>(sql,parameters).ToList();
				}
			}
			return records;
		}

		public TransactionResult InputTextHistroy(string text, string userid, string draftcode)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			ArrayList arrayListParameters = new ArrayList();
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), :draftcode, :text, :userid, SYSDATE)";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						arrayListParameters.Add(new OracleParameter("text", text));
						arrayListParameters.Add(new OracleParameter("userid", userid));
						object[] parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql,parameters);

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Data Berhasil ditambahkan";
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


		public List<KopSurat> GetListKop(string kantorid, string tipe = null)
		{
			var records = new List<KopSurat>();
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			string sql = $@"SELECT UNITKERJAID, NAMAUNITKERJA UnitKerjaName, NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL 
							FROM {skema}.TBLKANTOR";
			if (!string.IsNullOrEmpty(kantorid))
			{
				if (tipe == "adhoc")
				{
					sql += $"WHERE SUBSTR(UNITKERJAID,0,1) = 'A' AND SUBSTR(UNITKERJAID,3,LENGTH(KANTORID)) = :kantorid";
					arrayListParameters.Add(new NpgsqlParameter("kantorid", kantorid));
				} else
				{
					sql += $"WHERE KANTORID = :kantorid";
					arrayListParameters.Add(new NpgsqlParameter("kantorid", kantorid));
				}
			} else if (string.IsNullOrEmpty(kantorid) && tipe == "adhoc")
            {
				sql += $"WHERE SUBSTR(UNITKERJAID,0,1) = 'A'";
			}
			using (var ctx = new PostgresDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				records = ctx.Database.SqlQuery<KopSurat>(sql, parameters).ToList();
			}

			return records;
		}

		public List<KopSurat> GetListKopSurat(string unitkerja)
		{
			if (unitkerja.Length >= 6)
			{
				unitkerja = unitkerja.Substring(0, 6);
			}

			var records = new List<KopSurat>();
			string skema = OtorisasiUser.NamaSkema;
			string sql = $@"SELECT UNITKERJAID, NAMAUNITKERJA UNITKERJANAME, NAMAKANTOR_L1, NAMAKANTOR_L2, ALAMAT, TELEPON, EMAIL
					 FROM SURAT.TBLKANTOR
					 WHERE UNITKERJAID = '{unitkerja}'";
			using (var ctx = new PostgresDbContext())
			{
				records = ctx.Database.SqlQuery<KopSurat>(sql).ToList();
			}
			return records;
		}

		public List<Profile> GetProfileE1()
		{
			var records = new List<Profile>();


			string query = $@"
				SELECT
					JB.PROFILEID,
					JB.NAMA AS NAMAPROFILE, JB.*
				FROM JABATAN JB
				WHERE
					PROFILEID = 'H0000001' OR (TIPEESELONID = '1' AND LENGTH(PROFILEID) = 8) 
				ORDER BY NVL(TIPEESELONID,99)
					";


			using (var ctx = new BpnDbContext())
			{
				records = ctx.Database.SqlQuery<Profile>(query).ToList();
			}

			return records;
		}

		public List<UnitKerja> GetUnitKerjaE1()
		{
			var records = new List<UnitKerja>();
			string skema = OtorisasiUser.NamaSkema;

			string query = $@"
				SELECT 
					UK.unitkerjaid, UK.induk, UK.namaunitkerja, UK.eselon, UK.tipekantorid 
				FROM 
					UNITKERJA UK LEFT JOIN {skema}.TBLKANTOR TK ON TK.UNITKERJAID != UK.UNITKERJAID 
				WHERE 
					UK.ESELON = '1' GROUP BY UK.unitkerjaid, UK.induk, UK.namaunitkerja, UK.eselon, UK.tipekantorid
					";


			using (var ctx = new PostgresDbContext())
			{
				records = ctx.Database.SqlQuery<UnitKerja>(query).ToList();
			}

			return records;
		}

		public string GetIndukUnitKerjaId(string unitkerjaid)
        {
			string induk = string.Empty;
			string query = $@"
				SELECT 
					INDUK 
				FROM 
					UNITKERJA
				WHERE 
					UNITKERJAID = '{unitkerjaid}'";
			using (var ctx = new BpnDbContext())
			{
				induk = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                if (string.IsNullOrEmpty(induk) && unitkerjaid.Length > 6)
                {
					induk = unitkerjaid.Substring(0, 6);
                }
			}
			return induk;
		}

		public List<string> GetChildUnitKerjaId(string unitkerjaid)
		{
			var child = new List<string>();
			string query = $@"
				SELECT 
					UNITKERJAID 
				FROM 
					UNITKERJA
				WHERE 
					INDUK = '{unitkerjaid}' OR SUBSTR(UNITKERJAID,0,6) = '{unitkerjaid}'";
			using (var ctx = new BpnDbContext())
			{
				child = ctx.Database.SqlQuery<string>(query).ToList();
			}
			return child;
		}

		public List<Pegawai> GetPenandatanganNaskahDinas(string unitkerja, string profileidtu, string namajabatan, string namapegawai, string metadata, string userid, int from, int to, userIdentity user, bool isTU = false)
		{
			var records = new List<Pegawai>();
			using (var ctx = new BpnDbContext())
			{
				ArrayList arrayListParameters = new ArrayList();
				string UnitInduk = string.Empty;

				if (user != null)
				{
					if (new DataMasterModel().GetTipeKantor(user.KantorId) == 1)
					{
						UnitInduk = $"'{ctx.Database.SqlQuery<string>($"SELECT NVL(INDUK,SUBSTR('{unitkerja}',0,6)) FROM UNITKERJA WHERE UNITKERJAID='{unitkerja}'").FirstOrDefault()}'";
						if (UnitInduk.Length == 8)
						{
							UnitInduk += ",'02'";
						}
					}
				}



				string query = $@"
					SELECT
				  ROW_NUMBER() OVER(ORDER BY TIPEUSER, TIPEESELONID, RS.NAMA) RNUMBER,
				  COUNT(1) OVER() TOTAL, RS.PEGAWAIID, RS.NAMA, RS.PROFILEID, RS.JABATAN, RS.USERID
				FROM
				  (SELECT DISTINCT
					 PG.PEGAWAIID, JP.PROFILEID, JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '') AS JABATAN, PG.USERID,
					 DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS NAMA,
					 JB.TIPEESELONID, 0 AS TIPEUSER
				   FROM PEGAWAI PG
					 JOIN JABATANPEGAWAI JP ON
					   JP.PEGAWAIID = PG.PEGAWAIID AND
					   JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND
					   (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
					   NVL(JP.STATUSHAPUS,'0') = '0'
					 JOIN JABATAN JB ON
					   JB.PROFILEID = JP.PROFILEID AND
					   (JB.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JB.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
					   (JB.PROFILEIDTU = '{profileidtu}' OR JB.UNITKERJAID = '{unitkerja}' {(string.IsNullOrEmpty(UnitInduk) ? "" : $" OR JB.UNITKERJAID IN ({UnitInduk}) ")})
				   WHERE
					 (PG.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(PG.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
				   UNION ALL
				   SELECT DISTINCT
					 PPNPN.NIK AS PEGAWAIID, JP.PROFILEID, 'PPNPN' AS JABATAN, PPNPN.USERID, PPNPN.NAMA, JB.TIPEESELONID,
					 1 AS TIPEUSER
				   FROM PPNPN
					 JOIN JABATANPEGAWAI JP ON
					   JP.PEGAWAIID = PPNPN.NIK AND
					   JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND
					   (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
					   NVL(JP.STATUSHAPUS,'0') = '0'
					 JOIN JABATAN JB ON
					   JB.PROFILEID = JP.PROFILEID AND
					   (JB.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JB.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))  AND
					   (JB.PROFILEIDTU = '{profileidtu}' OR JB.UNITKERJAID = '{unitkerja}' {(string.IsNullOrEmpty(UnitInduk) ? "" : $" OR JB.UNITKERJAID IN ({UnitInduk}) ")} )  ) RS
				  INNER JOIN REGISTERUSERTTDDIGITAL TTE ON
					TTE.NIP = RS.PEGAWAIID";

				if (!string.IsNullOrEmpty(metadata))
				{
					string[] str = metadata.Split('|');
					metadata = "";
					foreach (var s in str)
					{
						if (!string.IsNullOrEmpty(s))
						{
							metadata += string.Concat("'", s, "',");
						}
					}
					metadata = metadata.Remove(metadata.Length - 1, 1);
					query += string.Format(" AND APEX_UTIL.URL_ENCODE(RS.PEGAWAIID) NOT IN ({0}) ", metadata);
				}

				if (!String.IsNullOrEmpty(userid))
				{
					query += string.Format(" AND RS.USERID <> '{0}' ", userid);
				}

				if (!String.IsNullOrEmpty(namajabatan))
				{
					arrayListParameters.Add(new OracleParameter("NamaJabatan", String.Concat("%", namajabatan.ToLower(), "%")));
					query += " AND LOWER(APEX_UTIL.URL_ENCODE(RS.JABATAN)) LIKE :NamaJabatan ";
				}
				if (!String.IsNullOrEmpty(namapegawai))
				{
					arrayListParameters.Add(new OracleParameter("NamaPegawai", String.Concat("%", namapegawai.ToLower(), "%")));
					query += " AND LOWER(APEX_UTIL.URL_ENCODE(RS.NAMA)) LIKE :NamaPegawai ";
				}

				if (to > 0)
				{
					query = string.Format(string.Concat("SELECT * FROM (" + query + ") WHERE RNumber BETWEEN {0} AND {1}"), from, to);
				}

				object[] parameters = arrayListParameters.OfType<object>().ToArray();
				try
				{
					records = ctx.Database.SqlQuery<Pegawai>(query, parameters).ToList<Pegawai>();
				}
				catch (Exception ex)
				{
					string msg = ex.Message;
				}
			}

			return records;
		}


		public List<Pegawai> GetPegawaiForTTE(string unitkerja, string profileidtu, string namajabatan, string namapegawai, string metadata, int from, int to)
		{
			var records = new List<Pegawai>();
			using (var ctx = new BpnDbContext())
			{
				ArrayList arrayListParameters = new ArrayList();
				object[] parameters;					

				string sql = $@"SELECT
									ROW_NUMBER ( ) OVER ( ORDER BY TIPEUSER, TIPEESELONID, RS.NAMA ) RNUMBER,
									COUNT( 1 ) OVER ( ) TOTAL,
									RS.PEGAWAIID,
									RS.NAMA,
									RS.PROFILEID,
									RS.JABATAN,
									RS.USERID 
								FROM
									(SELECT DISTINCT
											PG.PEGAWAIID,
											JP.PROFILEID,
											JB.NAMA || DECODE( JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '' ) AS JABATAN,
											PG.USERID,
											DECODE( PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ' ) || DECODE( PG.NAMA, '', '', PG.NAMA ) || DECODE( PG.GELARBELAKANG, NULL, '', ', ' || PG.GELARBELAKANG ) AS NAMA,
											JB.TIPEESELONID,
											0 AS TIPEUSER 
										FROM
											PEGAWAI PG
											JOIN JABATANPEGAWAI JP ON JP.PEGAWAIID = PG.PEGAWAIID 
											AND ( JP.VALIDSAMPAI IS NULL OR TRUNC( CAST( JP.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
											AND NVL( JP.STATUSHAPUS, '0' ) = '0'
											JOIN JABATAN JB ON JB.PROFILEID = JP.PROFILEID 
											AND NVL( JB.SEKSIID, 'X' ) <> 'A800' 
											AND ( JB.VALIDSAMPAI IS NULL OR TRUNC( CAST( JB.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
											AND (JB.PROFILEIDTU = :profileidtu1 {(!string.IsNullOrEmpty(unitkerja) ? " OR JB.UNITKERJAID = :unitkerja1 " : "")})
										WHERE
											( PG.VALIDSAMPAI IS NULL OR TRUNC( CAST( PG.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) UNION ALL
										SELECT DISTINCT
											PPNPN.NIK AS PEGAWAIID,
											JP.PROFILEID,
											'PPNPN' AS JABATAN,
											PPNPN.USERID,
											PPNPN.NAMA,
											JB.TIPEESELONID,
											1 AS TIPEUSER 
										FROM
											PPNPN
											JOIN JABATANPEGAWAI JP ON JP.PEGAWAIID = PPNPN.NIK 
											AND JP.PROFILEID NOT IN ( 'A81001', 'A81002', 'A81003', 'A81004', 'A80100', 'A80300', 'A80400', 'A80500', 'B80100' ) 
											AND ( JP.VALIDSAMPAI IS NULL OR TRUNC( CAST( JP.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
											AND NVL( JP.STATUSHAPUS, '0' ) = '0'
											JOIN JABATAN JB ON JB.PROFILEID = JP.PROFILEID 
											AND NVL( JB.SEKSIID, 'X' ) <> 'A800' 
											AND ( JB.VALIDSAMPAI IS NULL OR TRUNC( CAST( JB.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
											AND (JB.PROFILEIDTU = :profileidtu2  {(!string.IsNullOrEmpty(unitkerja) ? " OR JB.UNITKERJAID = :unitkerja2 " : "")}) ) RS
									WHERE RS.PEGAWAIID IN (SELECT NIP FROM REGISTERUSERTTDDIGITAL) ";
				arrayListParameters = new ArrayList();
				arrayListParameters.Add(new OracleParameter("profileidtu1", profileidtu));
				if (!string.IsNullOrEmpty(unitkerja)) { arrayListParameters.Add(new OracleParameter("unitkerja1", unitkerja));} 				
				arrayListParameters.Add(new OracleParameter("profileidtu2", profileidtu));
				if (!string.IsNullOrEmpty(unitkerja)) { arrayListParameters.Add(new OracleParameter("unitkerja2", unitkerja));}
				if (!string.IsNullOrEmpty(metadata))
				{
					string[] str = metadata.Split('|');
					metadata = "";
					foreach (var s in str)
					{
						if (!string.IsNullOrEmpty(s))
						{
							metadata += string.Concat("'", s, "',");
						}
					}
					metadata = metadata.Remove(metadata.Length - 1, 1);
					sql += $" AND APEX_UTIL.URL_ENCODE(RS.PEGAWAIID) NOT IN ({metadata}) ";
				}

				if (!String.IsNullOrEmpty(namajabatan))
				{
					arrayListParameters.Add(new OracleParameter("namajabatan", namajabatan.ToLower()));
					sql += " AND LOWER(APEX_UTIL.URL_ENCODE(RS.JABATAN)) LIKE '%'||:namajabatan||'%' ";
				}
				if (!String.IsNullOrEmpty(namapegawai))
				{
					arrayListParameters.Add(new OracleParameter("NamaPegawai", namapegawai.ToLower()));
					sql += " AND LOWER(APEX_UTIL.URL_ENCODE(RS.NAMA)) LIKE '%'||:NamaPegawai||'%' ";
				}

				sql = $"SELECT * FROM  ({sql}) WHERE RNumber BETWEEN :fromthis AND :tothis";
				arrayListParameters.Add(new OracleParameter("fromthis", from));
				arrayListParameters.Add(new OracleParameter("tothis", to));

				parameters = arrayListParameters.OfType<object>().ToArray();
				try
				{
					records = ctx.Database.SqlQuery<Pegawai>(sql, parameters).ToList<Pegawai>();
				}
				catch (Exception ex)
				{
					string msg = ex.Message;
				}
			}

			return records;
		}

		public TransactionResult HapusNotif(string draftcode)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string sql = $@"UPDATE {skema}.KOORDINASIDRAFTHISTORY SET URUTAN = NULL WHERE KOR_ID = '{draftcode}' ";
						ctx.Database.ExecuteSqlCommand(sql);

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Data Berhasil ditambahkan";
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

		public TransactionResult RevisiNotification(string draftcode, string userid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string update = $@"UPDATE {skema}.KOORDINASIDRAFT SET STATUS = 'X' WHERE DRAFTCODE = '{draftcode}' AND USERID= '{userid}'";
						ctx.Database.ExecuteSqlCommand(update);
						string sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, URUTAN, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}', 1 ,'!REVISINOTIFICATION!','{userid}',SYSDATE)";
						ctx.Database.ExecuteSqlCommand(sql);

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Data Berhasil ditambahkan";
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

		public TransactionResult infokanPerubahan(string draftcode, string userid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string update = $@"UPDATE {skema}.KOORDINASIDRAFT SET STATUS = 'W', TANGGAL = SYSDATE WHERE DRAFTCODE = '{draftcode}' AND STATUS = 'X'";
						ctx.Database.ExecuteSqlCommand(update);
						string sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, URUTAN, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}', 0 ,'!RESUBMIT!','{userid}',SYSDATE)";
						ctx.Database.ExecuteSqlCommand(sql);

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Konsep Berhasil Diajukan Kembali";
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

		public TransactionResult infokanPerubahanVerifikator(string draftcode, string userid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string update = $@"UPDATE {skema}.KOORDINASIDRAFT SET STATUS = 'W', TANGGAL = SYSDATE WHERE DRAFTCODE = '{draftcode}' AND STATUS = 'X'";
						ctx.Database.ExecuteSqlCommand(update);
						string sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, URUTAN, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}', 0 ,'!RESUBMITVERIFIKATOR!','{userid}',SYSDATE)";
						ctx.Database.ExecuteSqlCommand(sql);

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Konsep Berhasil Diajukan Kembali";
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

		public TransactionResult BatalkanPersetujuan(string draftcode, userIdentity usr)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" }; 
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			string sql = string.Empty;
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;

						arrayListParameters = new ArrayList();
						sql = $@"UPDATE {skema}.KOORDINASIDRAFT 
									 SET STATUS = 'W' WHERE DRAFTCODE = :draftcode AND USERID = :userid ";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						arrayListParameters = new ArrayList();
						sql = $@"SELECT 
								DRAFTCODE, 
								USERID AS PENANDATANGANID, 
								PEGAWAIID, 
								NAMA, 
								PROFILEID, 
								JABATAN,
								TIPE,
								URUT
							FROM {skema}.TBLPENANDATANGANDRAFTSURAT
							WHERE DRAFTCODE = :draftcode AND USERID = :userid AND STATUS != 'D'";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
						parameters = arrayListParameters.OfType<object>().ToArray();
						var thisVerfikator = ctx.Database.SqlQuery<UserTTE>(sql, parameters).FirstOrDefault();

						if (thisVerfikator != null)
                        {				
							//update status delete untuk semua verifikator setelahnya
							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.KOORDINASIDRAFT 
									 SET STATUS = 'D' WHERE DRAFTCODE = :draftcode AND USERID != :userid AND V_RANK > :urut ";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
							arrayListParameters.Add(new OracleParameter("urut", thisVerfikator.Urut));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}
                        else
                        {
							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.KOORDINASIDRAFT 
									 SET STATUS = 'D' WHERE DRAFTCODE = :draftcode AND USERID != :userid AND V_RANK = V_MAX ";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}

						arrayListParameters = new ArrayList();
						sql = $@"UPDATE {skema}.KOORDINASIDRAFTHISTORY 
									 SET PESAN = 'Persetujuan dibatalkan' WHERE KOR_ID = :draftcode AND PSFROM = :userid AND PESAN = '!ACCNOTIFICATION!'";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);


						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Persetujuan Dibatalkan";
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

		public TransactionResult SetHalamanTTE(string page, string draftcode)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string sql = $@"UPDATE {skema}.TBLDRAFTSURAT SET HALAMANTTE = '{page}' WHERE DRAFTCODE = '{draftcode}'";
						ctx.Database.ExecuteSqlCommand(sql);

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Halaman TTE Berhasil Diubah";
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



		public TransactionResult SetujuiKonsepV2(string draftcode, userIdentity usr)
        {
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			string sql = string.Empty;

			using (var ctx = new BpnDbContext())
            {
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
						//1 ubah status menjadi acc
						sql = $@"UPDATE {skema}.KOORDINASIDRAFT 
							 SET STATUS = 'A', TANGGAL = SYSDATE
							 WHERE DRAFTCODE = :draftcode AND USERID= :userid";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//get pengolah 
						arrayListParameters = new ArrayList();
						sql = $@"SELECT NVL(PG.PEGAWAIID,PN.NIK)
								 FROM {skema}.TBLLOGDRAFT LOG 
								 LEFT JOIN PEGAWAI PG ON LOG.USERID = PG.USERID
								 LEFT JOIN PPNPN PN ON LOG.USERID = PN.USERID
								 WHERE LOG.DRAFTCODE = :draftcode AND LOG.LOGTEXT = 'New'";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						parameters = arrayListParameters.OfType<object>().ToArray();
						var pengolah = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();

						//get this verifikator data
						arrayListParameters = new ArrayList();
						sql = $@"SELECT 
								DRAFTCODE, 
								USERID AS PENANDATANGANID, 
								PEGAWAIID, 
								NAMA, 
								PROFILEID, 
								JABATAN,
								TIPE,
								URUT
							FROM {skema}.TBLPENANDATANGANDRAFTSURAT
							WHERE DRAFTCODE = :draftcode AND USERID = :userid AND STATUS != 'D'";
						arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
						arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
						parameters = arrayListParameters.OfType<object>().ToArray();
						var thisVerfikator = ctx.Database.SqlQuery<UserTTE>(sql, parameters).FirstOrDefault();

						if (thisVerfikator == null)
                        {
							//acc dari tu yang ditambahkan
							var korid = NewGuID();

							arrayListParameters = new ArrayList();
							sql = $@"SELECT MAX(URUT) 
								 FROM {skema}.TBLPENANDATANGANDRAFTSURAT 
								 WHERE DRAFTCODE= :draftcode AND STATUS != 'D' 
								 GROUP BY DRAFTCODE ";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							parameters = arrayListParameters.OfType<object>().ToArray();
							int max = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();

							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT 
									 (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
									 SELECT 
										 :korid AS KOR_ID, 
										 TP.DRAFTCODE,
										 TP.PROFILEID AS VERIFIKATOR,
										 TP.USERID,
										 :urut AS V_RANK,
										 :max AS V_MAX, 
										 'W' AS STATUS,
										  SYSDATE AS TANGGAL 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
									WHERE TP.DRAFTCODE = :draftcode AND TP.URUT = :urut AND TP.STATUS != 'D'";
							arrayListParameters.Add(new OracleParameter("korid", korid));
							arrayListParameters.Add(new OracleParameter("urut", max));
							arrayListParameters.Add(new OracleParameter("max", max));
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							arrayListParameters.Add(new OracleParameter("urut", max));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), :draftcode,'!ACCNOTIFICATION!', :userid, SYSDATE)";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
							tr.Pesan = "Dokumen Berhasil disetujui";
						}
						else if (thisVerfikator.Tipe == "1")
						{
							//acc dari penandatangans

							arrayListParameters = new ArrayList();
							sql = $@"UPDATE {skema}.TBLDRAFTSURAT SET STATUS = 'A' WHERE DRAFTCODE = :draftcode";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
								 VALUES (RAWTOHEX(SYS_GUID()), :draftcode,'!FINALDRAFT!', :userid, SYSDATE)";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);


							//notifikasi selesai
							try
							{
								arrayListParameters = new ArrayList();
								sql = $@"SELECT PERIHAL FROM {skema}.TBLDRAFTSURAT WHERE DRAFTCODE = :draftcode";
								arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
								parameters = arrayListParameters.OfType<object>().ToArray();
								var perihal = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
								perihal = HttpUtility.UrlDecode(perihal);
								perihal = Regex.Replace(perihal, "<.*?>", String.Empty);

								var pg = new DataMasterModel().GetPegawaiByPegawaiId(pengolah);

								if (pg != null)
								{
									new Mobile().KirimNotifikasi(pg.PegawaiId, "asn", pg.Nama, $"Disetujui Oleh : {usr.NamaPegawai} | perihal : {perihal.Substring(0, 97)}{(perihal.Length > 97 ? "..." : "")}", "Konsep Disetujui (Pengolah Surat)");
								}

								var koordinasilist = this.GetListProsesKoordinasi(draftcode);

								foreach (var kor in koordinasilist)
								{
									if (kor.Tipe == "0")
									{
										new Mobile().KirimNotifikasi(kor.PegawaiId, "asn", kor.Nama, $"Disetujui Oleh : {usr.NamaPegawai} | perihal : {perihal.Substring(0, 97)}{(perihal.Length > 97 ? "..." : "")}", "Konsep Disetujui (Verifikator)");
									}
									else if (kor.Tipe == "TU")
									{
										new Mobile().KirimNotifikasi(kor.PegawaiId, "asn", kor.Nama, $"Disetujui Oleh : {usr.NamaPegawai} | perihal : {perihal.Substring(0, 97)}{(perihal.Length > 97 ? "..." : "")}", "Konsep Baru Perlu Diberikan Nomor");
									}
								}

								tr.Pesan = "Dokumen Berhasil disetujui";

							}
							catch
							{
								//gagal notifikasi
								tr.Pesan = "Dokumen Berhasil disetujui, Namun gagal mengirimkan notifikasi";
							}


						}
						else
						{
							var korid = NewGuID();
							arrayListParameters = new ArrayList();
							sql = $@"SELECT 
									DRAFTCODE, 
									USERID AS PENANDATANGANID, 
									PEGAWAIID, 
									NAMA, 
									PROFILEID, 
									JABATAN,
									TIPE,
									URUT
								FROM {skema}.TBLPENANDATANGANDRAFTSURAT
								WHERE DRAFTCODE = :draftcode AND TIPE = '1' AND STATUS != 'D'";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							parameters = arrayListParameters.OfType<object>().ToArray();
							var penandatangan = ctx.Database.SqlQuery<UserTTE>(sql, parameters).FirstOrDefault();

							arrayListParameters = new ArrayList();
							sql = $@"SELECT MAX(URUT) 
								 FROM {skema}.TBLPENANDATANGANDRAFTSURAT 
								 WHERE DRAFTCODE= :draftcode AND STATUS != 'D' 
								 GROUP BY DRAFTCODE ";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							parameters = arrayListParameters.OfType<object>().ToArray();
							int max = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
							bool addNextVerifikator = true;

							if ((thisVerfikator.Urut + 1) == max)
							{
								//cek penandatangan eselon
								arrayListParameters = new ArrayList();
								//sql = $@"SELECT JB.TIPEESELONID 
								//	 FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
								//	 INNER JOIN JABATAN JB ON TP.PROFILEID = JB.PROFILEID 
								//	 WHERE TP.DRAFTCODE = :draftcode AND TP.TIPE = '1' AND TP.STATUS != 'D'";
								sql = $@"SELECT NVL(TO_NUMBER(SM.ESELONID), 0) AS ESELONID 
										 FROM SIMPEG_2702.SIAP_VW_PEGAWAI SM
										 INNER JOIN {skema}.TBLPENANDATANGANDRAFTSURAT TP ON TP.PEGAWAIID = SM.NIPBARU
											AND TP.DRAFTCODE = :draftcode 
											AND TP.TIPE = '1'
											AND TP.STATUS != 'D'";
								arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
								parameters = arrayListParameters.OfType<object>().ToArray();
								decimal? eselonPenandatangan = ctx.Database.SqlQuery<decimal?>(sql, parameters).FirstOrDefault();

								if(eselonPenandatangan == null || eselonPenandatangan == 0)
                                {
									eselonPenandatangan = 2;
                                }else { eselonPenandatangan = Math.Floor((decimal)(eselonPenandatangan / 10)); }

								if (eselonPenandatangan < 3)
								{
									//TU Penandatangan
									arrayListParameters = new ArrayList();
									sql = $@" SELECT PROFILEIDTU FROM JABATAN WHERE PROFILEID = :profileid AND (VALIDSAMPAI IS NULL OR TO_DATE( TRIM( VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )";
									arrayListParameters.Add(new OracleParameter("profileid", penandatangan.ProfileId));
									parameters = arrayListParameters.OfType<object>().ToArray();
									var profileIdTUPenandatangan = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();

									//cek TU udah ada atau belum
									arrayListParameters = new ArrayList();
									sql = $@" SELECT COUNT(1) FROM  {skema}.TBLPENANDATANGANDRAFTSURAT WHERE PROFILEID = :profileTU AND DRAFTCODE = :draftcode  ";
									arrayListParameters.Add(new OracleParameter("profileTU", profileIdTUPenandatangan));
									arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
									parameters = arrayListParameters.OfType<object>().ToArray();
									bool isTuExist = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault() > 0;

									if (!isTuExist)
									{
										arrayListParameters = new ArrayList();
										sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT 
												 (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
												 SELECT 
													 :korid AS KOR_ID, 
													 TP.DRAFTCODE,
													 JB.PROFILEIDTU AS VERIFIKATOR,
													 PG.USERID,
													 :urut AS V_RANK,
													 :max AS V_MAX, 
													 'W' AS STATUS,
													  SYSDATE AS TANGGAL 
												FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
												LEFT JOIN JABATAN JB 
													ON TP.PROFILEID = JB.PROFILEID 
													AND (JB.VALIDSAMPAI IS NULL OR TO_DATE( TRIM( JB.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )
												LEFT JOIN JABATANPEGAWAI JP 
													ON JB.PROFILEIDTU = JP.PROFILEID 
													AND (JP.VALIDSAMPAI IS NULL OR TO_DATE( TRIM( JP.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )
													AND (JP.STATUSHAPUS = 0 OR JP.STATUSHAPUS IS NULL)
												LEFT JOIN PEGAWAI PG 
													ON JP.PEGAWAIID = PG.PEGAWAIID AND (PG.VALIDSAMPAI IS NULL OR TO_DATE( TRIM( PG.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )
												WHERE TP.DRAFTCODE = :draftcode AND TP.URUT = :urut AND TP.STATUS != 'D'";
										arrayListParameters.Add(new OracleParameter("korid", korid));
										arrayListParameters.Add(new OracleParameter("urut", max));
										arrayListParameters.Add(new OracleParameter("max", max));
										arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
										arrayListParameters.Add(new OracleParameter("urut", max));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
										addNextVerifikator = false;
									}
								}
							}

							if (addNextVerifikator)
							{
								arrayListParameters = new ArrayList();
								sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT 
									 (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
									 SELECT 
										 :korid AS KOR_ID, 
										 TP.DRAFTCODE,
										 TP.PROFILEID AS VERIFIKATOR,
										 TP.USERID,
										 :urut AS V_RANK,
										 :max AS V_MAX, 
										 'W' AS STATUS,
										  SYSDATE AS TANGGAL 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
									WHERE TP.DRAFTCODE = :draftcode AND TP.URUT = :urut AND TP.STATUS != 'D'";
								arrayListParameters.Add(new OracleParameter("korid", korid));
								arrayListParameters.Add(new OracleParameter("urut", thisVerfikator.Urut + 1));
								arrayListParameters.Add(new OracleParameter("max", max));
								arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
								arrayListParameters.Add(new OracleParameter("urut", thisVerfikator.Urut + 1));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}

							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), :draftcode,'!ACCNOTIFICATION!', :userid, SYSDATE)";
							arrayListParameters.Add(new OracleParameter("draftcode", draftcode));
							arrayListParameters.Add(new OracleParameter("userid", usr.UserId));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
							tr.Pesan = "Dokumen Berhasil disetujui";
						}

						tc.Commit();
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

		public TransactionResult SetujuiKonsep(string draftcode, userIdentity usr)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string skema = OtorisasiUser.NamaSkema;
						string sql = $@"UPDATE {skema}.KOORDINASIDRAFT SET STATUS = 'A',TANGGAL = SYSDATE WHERE DRAFTCODE = '{draftcode}' AND USERID= '{usr.UserId}' ";
						ctx.Database.ExecuteSqlCommand(sql);
						decimal rank = 0;

						//notifikasi Data
						bool isAccFinal = false;
						string pengolah = string.Empty;
						string perihal = string.Empty;
						var pg = new Pegawai();
						var koordinasilist = new List<UserTTE>();


						//get next verifikator
						var urut = ctx.Database.SqlQuery<decimal>($@"SELECT URUT FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE = '{draftcode}' AND USERID = '{usr.UserId}' AND STATUS != 'D'").FirstOrDefault();
						var max = ctx.Database.SqlQuery<decimal>($@"SELECT MAX(URUT) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE= '{draftcode}' AND STATUS != 'D' GROUP BY DRAFTCODE ").FirstOrDefault();
						bool TuDanVerifikator = false;
						//var isTuOrVerifakator = ctx.Database.SqlQuery<string>($@"SELECT USERID FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE = '{draftcode}' AND USERID = '{usr.UserId}' AND V_RANK = V_MAX ").FirstOrDefault();

						if (OtorisasiUser.isTU())
                        {
							bool isVerifakator = string.IsNullOrEmpty(ctx.Database.SqlQuery<string>($@"SELECT USERID FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE = '{draftcode}' AND USERID = '{usr.UserId}' AND STATUS != 'D'").FirstOrDefault()) ? false : true;
							TuDanVerifikator = isVerifakator;
							if (urut == 0)
                            {
								rank = decimal.Parse(ctx.Database.SqlQuery<string>($@"SELECT V_RANK FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE = '{draftcode}' AND USERID = '{usr.UserId}' AND STATUS != 'D' ").FirstOrDefault());
							}
							else if (isVerifakator)
                            {
								//cek tu sebagai verifikator bukan sebagai filter penandatangan
								var maxurutTU = decimal.Parse(ctx.Database.SqlQuery<string>($@"SELECT MAX(V_RANK) FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE = '{draftcode}' AND USERID = '{usr.UserId}' AND STATUS != 'D'").FirstOrDefault());
								string tubukanpenandatangan = ctx.Database.SqlQuery<string>($@"SELECT TIPE FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE DRAFTCODE = '{draftcode}' AND USERID = '{usr.UserId}' AND STATUS != 'D'  ").FirstOrDefault();
								if (urut != max && maxurutTU == max && tubukanpenandatangan == "0")
                                {
									urut = 0;
									rank = max;
								} 
							}
                        }

						var korid = NewGuID();


						if ((urut + 1) < max && urut != 0)
						{
							string isExist = ctx.Database.SqlQuery<string>($"SELECT KOR_ID FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE='{draftcode}' AND V_RANK='{urut + 1}'  AND STATUS != 'D'").FirstOrDefault();
							if (string.IsNullOrEmpty(isExist))
							{
								sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
									 SELECT 
										'{korid}' AS KOR_ID, 
										 TP.DRAFTCODE,
										 TP.PROFILEID AS VERIFIKATOR,
										 TP.USERID,
										 {urut + 1} AS V_RANK,
										 {max} AS V_MAX, 
										 'W' AS STATUS,
										  SYSDATE AS TANGGAL 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
									WHERE TP.DRAFTCODE = '{draftcode}' AND TP.URUT = {urut + 1} AND TP.STATUS != 'D'";

								ctx.Database.ExecuteSqlCommand(sql);
							} 

							sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}','!ACCNOTIFICATION!','{usr.UserId}',SYSDATE)";
							ctx.Database.ExecuteSqlCommand(sql);
						}
						else if (urut != 0 && (urut + 1) == max)
						{

							var eselonjabatan = ctx.Database.SqlQuery<decimal>($@"SELECT JB.TIPEESELONID FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP LEFT JOIN JABATAN JB ON TP.PROFILEID = JB.PROFILEID WHERE TP.DRAFTCODE = '{draftcode}' AND TP.TIPE = 1 AND TP.STATUS != 'D'").FirstOrDefault();
							
							//cek tu sudah dalam verifkator
							var penanadatanganProfileid = ctx.Database.SqlQuery<string>($@"SELECT PROFILEID FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE TIPE = '1' AND DRAFTCODE = '{draftcode}' AND STATUS != 'D'").FirstOrDefault();

							var stringsql = $"SELECT PROFILEIDTU FROM JABATAN WHERE PROFILEID='{penanadatanganProfileid}'";
							string profileIdTuPenandatangan = ctx.Database.SqlQuery<string>(stringsql).FirstOrDefault();

							bool TUisPresent = false;

							if (!string.IsNullOrEmpty(profileIdTuPenandatangan))
                            {
								decimal isPresent = ctx.Database.SqlQuery<decimal>($"SELECT COUNT(1) FROM {skema}.TBLPENANDATANGANDRAFTSURAT WHERE PROFILEID = '{profileIdTuPenandatangan}' AND DRAFTCODE = '{draftcode}' AND STATUS != 'D' ").FirstOrDefault();
								TUisPresent = isPresent > 0;
							}

							//cek tu bukan pengolah

							bool TUisPengolah = false;

							pengolah = ctx.Database.SqlQuery<string>($"SELECT USERID FROM {skema}.TBLLOGDRAFT WHERE LOGTEXT = 'New' AND DRAFTCODE = '{draftcode}'").FirstOrDefault();
							var pengolahPegawaiid = ctx.Database.SqlQuery<string>($@"SELECT PEGAWAIID FROM (
														SELECT PEGAWAIID FROM PEGAWAI WHERE USERID = '{pengolah}'
														UNION
														SELECT NIK AS PEGAWAIID FROM PPNPN WHERE USERID = '{pengolah}'
														)").FirstOrDefault();
							var PegawaiidTu = new DataMasterModel().GetPegawaiIdFromProfileId(profileIdTuPenandatangan);
							if(pengolahPegawaiid == PegawaiidTu)
                            {
								TUisPengolah = true;
							}


							if (eselonjabatan < 3 &&  (!TUisPresent && !TUisPengolah))
							{
								//tambahkan tu untuk eselon 1 dan 2
								string isExist = ctx.Database.SqlQuery<string>($"SELECT NVL(KOR_ID,'') KOR_ID FROM {skema}.KOORDINASIDRAFT WHERE DRAFTCODE='{draftcode}' AND V_RANK='{max}' AND STATUS != 'D'").FirstOrDefault();
								if (string.IsNullOrEmpty(isExist))
								{
									sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
									 SELECT 
										'{korid}' AS KOR_ID, 
										 TP.DRAFTCODE,
										 JB.PROFILEIDTU AS VERIFIKATOR,
										 PG.USERID,
										 {urut + 1} AS V_RANK,
										 {max} AS V_MAX, 
										 'W' AS STATUS,
										  SYSDATE AS TANGGAL 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
									LEFT JOIN JABATAN JB ON TP.PROFILEID = JB.PROFILEID AND (JB.VALIDSAMPAI IS NULL 
									OR TO_DATE( TRIM( JB.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )
									LEFT JOIN JABATANPEGAWAI JP ON JB.PROFILEIDTU = JP.PROFILEID AND (JP.VALIDSAMPAI IS NULL 
									OR TO_DATE( TRIM( JP.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )
									AND (JP.STATUSHAPUS = 0 OR JP.STATUSHAPUS IS NULL)
									LEFT JOIN PEGAWAI PG ON JP.PEGAWAIID = PG.PEGAWAIID AND (PG.VALIDSAMPAI IS NULL 
									OR TO_DATE( TRIM( PG.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) )
									WHERE TP.DRAFTCODE = '{draftcode}' AND TP.URUT = {urut + 1} AND TP.STATUS != 'D'";

									ctx.Database.ExecuteSqlCommand(sql);
								}
							} else
							{
								// eselon > 2 tidak perlu tambah tu
								string isExist = ctx.Database.SqlQuery<string>($@"SELECT KOR_ID FROM {skema}.KOORDINASIDRAFT KD 
																				  LEFT JOIN {skema}.TBLPENANDATANGANDRAFTSURAT TPS ON KD.DRAFTCODE = TPS.DRAFTCODE AND KD.USERID = TPS.USERID AND TPS.URUT='{max}'
																				  WHERE KD.DRAFTCODE ='{draftcode}' AND KD.STATUS != 'D' AND TPS.STATUS != 'D' ").FirstOrDefault();
								if (string.IsNullOrEmpty(isExist))
								{
									sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
									 SELECT 
										'{korid}' AS KOR_ID, 
										 TP.DRAFTCODE,
										 TP.PROFILEID AS VERIFIKATOR,
										 TP.USERID,
										 {urut + 1} AS V_RANK,
										 {max} AS V_MAX, 
										 'W' AS STATUS,
										  SYSDATE AS TANGGAL 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
									WHERE TP.DRAFTCODE = '{draftcode}' AND TP.URUT = {urut + 1}  AND TP.STATUS != 'D'";
									ctx.Database.ExecuteSqlCommand(sql);
								}
							}
							sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}','!ACCNOTIFICATION!','{usr.UserId}',SYSDATE)";
							ctx.Database.ExecuteSqlCommand(sql);

						}
						else if (urut == max || rank == max)
						{
							//jika acc dari tu
							if (OtorisasiUser.isTU() && urut == 0)
							{
								string query = $@"SELECT KOR_ID FROM {skema}.KOORDINASIDRAFT KD 
												LEFT JOIN {skema}.TBLPENANDATANGANDRAFTSURAT TPS ON KD.DRAFTCODE = TPS.DRAFTCODE AND KD.USERID = TPS.USERID
												WHERE KD.DRAFTCODE ='{draftcode}' AND TPS.URUT='{max}' AND TPS.STATUS != 'D' AND KD.STATUS != 'D' ";
								string isExist = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
								if (string.IsNullOrEmpty(isExist))
								{
									sql = $@"INSERT INTO {skema}.KOORDINASIDRAFT (KOR_ID, DRAFTCODE, VERIFIKATOR, USERID, V_RANK, V_MAX, STATUS, TANGGAL) 
									 SELECT 
										'{korid}' AS KOR_ID, 
										 TP.DRAFTCODE,
										 TP.PROFILEID AS VERIFIKATOR,
										 TP.USERID,
										 {rank} AS V_RANK,
										 {max} AS V_MAX, 
										 'W' AS STATUS,
										  SYSDATE AS TANGGAL 
									FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP 
									WHERE TP.DRAFTCODE = '{draftcode}' AND TP.URUT = {rank} AND TP.STATUS != 'D'";
									ctx.Database.ExecuteSqlCommand(sql);
								}

								sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}','!ACCNOTIFICATION!','{usr.UserId}',SYSDATE)";
								ctx.Database.ExecuteSqlCommand(sql);
							}
							// jika penandatangan
							else
							{
								sql = $@"UPDATE {skema}.TBLDRAFTSURAT SET STATUS = 'A' WHERE DRAFTCODE = '{draftcode}'";
								ctx.Database.ExecuteSqlCommand(sql);
								sql = $@"INSERT INTO {skema}.KOORDINASIDRAFTHISTORY (KORHIST_ID, KOR_ID, PESAN, PSFROM, TANGGAL) 
										VALUES (RAWTOHEX(SYS_GUID()), '{draftcode}','!FINALDRAFT!','{usr.UserId}',SYSDATE)";
								ctx.Database.ExecuteSqlCommand(sql);

								isAccFinal = true;
								var arrayListParameters = new ArrayList();
								sql = $@"SELECT PG.PEGAWAIID
										 FROM {skema}.TBLLOGDRAFT LOG 
										 INNER JOIN PEGAWAI PG ON LOG.USERID = PG.USERID 
										 WHERE LOG.DRAFTCODE =  :dc AND LOG.LOGTEXT = 'New'";
								arrayListParameters.Add(new OracleParameter("dc", draftcode));
								var parameters = arrayListParameters.OfType<object>().ToArray();
								pengolah = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();

								arrayListParameters = new ArrayList();
								sql = $@"SELECT PERIHAL FROM {skema}.TBLDRAFTSURAT WHERE DRAFTCODE = :dc";
								arrayListParameters.Add(new OracleParameter("dc", draftcode));
								parameters = arrayListParameters.OfType<object>().ToArray();
								perihal = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
								perihal = HttpUtility.UrlDecode(perihal);
								perihal = Regex.Replace(perihal, "<.*?>", String.Empty);
								if (!string.IsNullOrEmpty(pengolah))
								{
									pg = new DataMasterModel().GetPegawaiByPegawaiId(pengolah);								
								}
								koordinasilist = this.GetListProsesKoordinasi(draftcode);
							}								
						}


						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Dokumen Berhasil Disetujui";
						if (isAccFinal && OtorisasiUser.NamaSkemaLogin != "surattrain")
                        {
                            if (!string.IsNullOrEmpty(pengolah))
                            {
								new Mobile().KirimNotifikasi(pg.PegawaiId, "asn", pg.Nama, $"Disetujui Oleh : {usr.NamaPegawai} | perihal : {perihal.Substring(0, 97)}{(perihal.Length > 97 ? "..." : "")}", "Konsep Disetujui (Pengolah Surat)");
							}

							foreach (var kor in koordinasilist)
                            {
								if (kor.Tipe == "0")
                                {
									new Mobile().KirimNotifikasi(kor.PegawaiId, "asn", kor.Nama, $"Disetujui Oleh : {usr.NamaPegawai} | perihal : {perihal.Substring(0, 97)}{(perihal.Length > 97 ? "..." : "")}", "Konsep Disetujui (Verifikator)");
								}
								else if (kor.Tipe == "TU")
                                {
									new Mobile().KirimNotifikasi(kor.PegawaiId, "asn", kor.Nama, $"Disetujui Oleh : {usr.NamaPegawai} | perihal : {perihal.Substring(0, 97)}{(perihal.Length > 97 ? "..." : "")}", "Konsep Baru Perlu Diberikan Nomor");
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

		internal List<UserTTE> GetListProsesKoordinasi(string dc)
        {
			string skema = OtorisasiUser.NamaSkema;
			string sql = $@"SELECT * FROM (
							SELECT
								TP.USERID AS PENANDATANGANID,
								TP.PEGAWAIID,
								TP.NAMA,
								TP.PROFILEID,
								TP.JABATAN,
								TP.URUT,
								TP.TIPE,
								NVL(KD.STATUS,'Y') AS STATUS
							FROM
								{skema}.TBLPENANDATANGANDRAFTSURAT TP
								LEFT JOIN {skema}.KOORDINASIDRAFT KD ON TP.DRAFTCODE = KD.DRAFTCODE AND KD.STATUS != 'D'
								AND TP.USERID = KD.USERID 
							WHERE
								TP.DRAFTCODE = '{dc}' 
								AND TP.STATUS != 'D'
							UNION
							SELECT
								KD.USERID AS PENANDATANGANID,
								PG.PEGAWAIID AS PEGAWAIID,
								DECODE( PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ' ) || DECODE( PG.NAMA, '', '', PG.NAMA ) || DECODE( PG.GELARBELAKANG, NULL, '', ', ' || PG.GELARBELAKANG ) AS NAMA,
								KD.VERIFIKATOR AS PROFILEID,
								JB.NAMA AS JABATAN,
								NVL(CAST (KD.V_RANK AS INTEGER),0) AS URUT,
								'TU',
								KD.STATUS AS STATUS 
							FROM
								{skema}.KOORDINASIDRAFT KD
								LEFT JOIN PEGAWAI PG ON PG.USERID = KD.USERID  AND KD.STATUS != 'D'
								AND (
									PG.VALIDSAMPAI IS NULL 
									OR TO_DATE( TRIM( PG.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) 
								)
								LEFT JOIN JABATAN JB ON KD.VERIFIKATOR = JB.PROFILEID 
								AND (
									JB.VALIDSAMPAI IS NULL 
									OR TO_DATE( TRIM( JB.VALIDSAMPAI ), 'DD/MM/YY' ) > TO_DATE( TRIM( SYSDATE ), 'DD/MM/YY' ) 
								) 
							WHERE
								KD.DRAFTCODE = '{dc}' 
								AND KD.USERID NOT IN ( SELECT TP2.USERID FROM {skema}.TBLPENANDATANGANDRAFTSURAT TP2 WHERE TP2.DRAFTCODE = '{dc}' AND TP2.STATUS != 'D' )
							) ORDER BY URUT DESC, TIPE ASC";
			using (var ctx = new BpnDbContext())
			{
				return ctx.Database.SqlQuery<UserTTE>(sql).ToList();
			}
		}

        public string NewGuID()
		{
			string _result = "";
			using (var ctx = new BpnDbContext())
			{
				_result = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
			}

			return _result;
		}

		public bool CekDraftTU(string id, string pegawaiid, string max)
		{
			string result = "";
			string skema = OtorisasiUser.NamaSkema;
			using (var ctx = new BpnDbContext())
			{
				result = ctx.Database.SqlQuery<string>($@"SELECT JP.PEGAWAIID FROM {skema}.TBLPENANDATANGANDRAFTSURAT TPS  
															LEFT JOIN JABATAN JB ON TPS.PROFILEID = JB.PROFILEID
															LEFT JOIN JABATANPEGAWAI JP ON JB.PROFILEIDTU = JP.PROFILEID
															WHERE TPS.DRAFTCODE = '{id}' AND TPS.URUT = {max} AND TPS.STATUS != 'D'").FirstOrDefault<string>();
			}

			return result == pegawaiid ? true : false;
		}
		public bool SetDraftStatus(string draftcode, string status)
		{
			bool result = false;
			string skema = OtorisasiUser.NamaSkema;
			using (var ctx = new BpnDbContext())
			{
				try
				{
					string sql = $@"UPDATE {skema}.TBLDRAFTSURAT SET STATUS = '{status}' WHERE DRAFTCODE = '{draftcode}'";
					ctx.Database.ExecuteSqlCommand(sql);
					result = true;
				}
				catch
				{
					result = false;
				}
			}
			return result;
		}

		//public bool HapusKop(string unitkerjaid)
		//{
		//	bool result = false;
		//	string skema = OtorisasiUser.NamaSkema;
		//	using (var ctx = new BpnDbContext())
		//	{
		//		try
		//		{
		//			string sql = $@"DELETE FROM {skema}.TBLKOPSURAT WHERE UNITKERJAID = '{unitkerjaid}'";
		//			ctx.Database.ExecuteSqlCommand(sql);
		//			result = true;
		//		}
		//		catch
		//		{
		//			result = false;
		//		}
		//	}
		//	return result;
		//}


		public string getLokasiKantor(string profileId)
		{
			string kantorid = "";
			string result = "";
			var arrayListParameters = new ArrayList();
			using (var ctx = new BpnDbContext())
			{
				string sql = $@"SELECT CASE WHEN UK.UNITKERJAID IN ('020116','020118','020115') THEN 'Bogor' ELSE UK.KANTORID END
								FROM JABATAN JB
								INNER JOIN UNITKERJA UK ON JB.UNITKERJAID = UK.UNITKERJAID 
								WHERE JB.PROFILEID = :profileid";
				arrayListParameters.Add(new OracleParameter("profileid", profileId));
				var parameters = arrayListParameters.OfType<object>().ToArray();
				kantorid = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault<string>();
			}

			if(kantorid != "Bogor")
            {
				using (var ctx = new PostgresDbContext())
				{
					arrayListParameters = new ArrayList();
					string sql = $@"SELECT KOTA 
								FROM SURAT.TBLKANTOR 
								WHERE KANTORID = :kantorid";
					arrayListParameters.Add(new NpgsqlParameter("kantorid", kantorid));
					var parameters = arrayListParameters.OfType<object>().ToArray();
                    try
                    {
						result = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault<string>();
					}
                    catch
                    {
						result = string.Empty;
					}
					
				}
			} else { result = kantorid; }           

            return result;
		}

		public string getLokasiKantorByPegawaiid(string pegawaiid)
		{
			string result = "";
			string skema = OtorisasiUser.NamaSkema;
			using (var ctx = new BpnDbContext())
			{
				result = ctx.Database.SqlQuery<string>($@"SELECT KT.KOTA FROM JABATAN JB
														INNER JOIN JABATANPEGAWAI JP ON JP.PROFILEID = JB.PROFILEID
														INNER JOIN KANTOR KT ON JP.KANTORID = KT.KANTORID
														WHERE JP.PEGAWAIID = '{pegawaiid}' AND JP.VALIDSEJAK IS NOT NULL 
														AND (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
														AND JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100')").FirstOrDefault<string>();
			}

			return result;
		}


		public List<UnitKerja> NDGetUnitKerja(){
			var lst = new List<UnitKerja>();
			string sql = "SELECT unitkerjaid, induk, namaunitkerja, eselon, tipekantorid FROM unitkerja WHERE unitkerjaid IS NOT NULL AND tampil = 1 ORDER BY TIPEKANTORID, LENGTH(unitkerjaid), ESELON,UNITKERJAID";
			using (var ctx = new BpnDbContext())
			{
				lst = ctx.Database.SqlQuery<UnitKerja>(sql).ToList<UnitKerja>();
			}
			return lst;
		}

		public bool setNomordanTanggal(string draftcode, string nomor, string tanggal)
		{
			var result = false;
			string skema = OtorisasiUser.NamaSkema;
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string sql;
						//check
						var details = ctx.Database.SqlQuery<DraftSuratDetail>($"SELECT DETAILTEXT AS TEXT , DETAILVALUE AS VALUE FROM {skema}.TBLDRAFTSURATDETAIL WHERE DRAFTCODE='{draftcode}' AND (DETAILTEXT = 'NomorSurat' OR DETAILTEXT = 'TanggalSurat') ").ToList();
						if (details.Count() == 0)
						{
							sql = $@"
								INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
								VALUES ('{draftcode}','NomorSurat','{nomor}')";
							ctx.Database.ExecuteSqlCommand(sql);

							sql = $@"
								INSERT INTO {skema}.TBLDRAFTSURATDETAIL (DRAFTCODE, DETAILTEXT, DETAILVALUE) 
								VALUES ('{draftcode}','TanggalSurat','{tanggal}')";
							ctx.Database.ExecuteSqlCommand(sql);
						}
						else if (details.Count() > 0)
						{
							sql = $@"UPDATE {skema}.TBLDRAFTSURATDETAIL 
									 SET DETAILVALUE ='{nomor}' WHERE DRAFTCODE ='{draftcode}' AND DETAILTEXT = 'NomorSurat'";
							ctx.Database.ExecuteSqlCommand(sql);

							sql = $@"UPDATE {skema}.TBLDRAFTSURATDETAIL 
									 SET DETAILVALUE ='{tanggal}' WHERE DRAFTCODE ='{draftcode}' AND DETAILTEXT = 'TanggalSurat'";
							ctx.Database.ExecuteSqlCommand(sql);
						}

						tc.Commit();
						result = true;
					}
					catch
					{
						tc.Rollback();
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

		public List<Profile> GetIndukByUnitKerja(string unitkerjaid, bool x)
		{
			var records = new List<Profile>();
			string query;
			//if (unitkerjaid.Length >= 6)
			//{
			//	unitkerjaid = unitkerjaid.Substring(0, 6);
			//}

			if (x)
			{
				query = $@"SELECT PROFILEID, NAMA AS NAMAPROFILE, JABATAN.* FROM JABATAN WHERE PROFILEID = 'H0000001' OR PROFILEID = 'H0000002'";

				using (var ctx = new BpnDbContext())
				{
					records.AddRange(ctx.Database.SqlQuery<Profile>(query).ToList());
				}
			}

            #region Mendapatkan Jabatan tertinggi di Unit Induk
            string unitinduk = string.Empty;
			query = $"SELECT INDUK FROM UNITKERJA WHERE UNITKERJAID = '{unitkerjaid}'";
			using (var ctx = new BpnDbContext())
			{
				unitinduk = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
                if (string.IsNullOrEmpty(unitinduk))
                {
                    if (unitkerjaid.Length > 6)
                    {
						unitinduk = unitkerjaid.Substring(0, 6);
					}
                }
			}
            if (!string.IsNullOrEmpty(unitinduk))
            {
				query = $@"SELECT JB.PROFILEID, JB.NAMA AS NAMAPROFILE, JB.* FROM 
				(SELECT * FROM JABATAN WHERE JABATAN.UNITKERJAID = '{unitinduk}' AND NVL(JABATAN.SEKSIID,'XX') != 'A800' AND (JABATAN.VALIDSAMPAI IS NULL OR CAST(JABATAN.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) ORDER BY JABATAN.TIPEESELONID) JB 
				WHERE ROWNUM <= 1 ";

				using (var ctx = new BpnDbContext())
				{
					records.AddRange(ctx.Database.SqlQuery<Profile>(query).ToList());
				}
			}
            #endregion // :: EOL

            //query = $@"SELECT JB.PROFILEID, JB.NAMA AS NAMAPROFILE, JB.* FROM 
            //	(SELECT * FROM JABATAN WHERE JABATAN.UNITKERJAID = '{unitkerjaid}' AND NVL(JABATAN.SEKSIID,'XX') != 'A800' AND (JABATAN.VALIDSAMPAI IS NULL OR CAST(JABATAN.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) ORDER BY JABATAN.TIPEESELONID) JB 
            //	WHERE ROWNUM <= 1 ";

            //using (var ctx = new BpnDbContext())
            //{
            //    records.AddRange(ctx.Database.SqlQuery<Profile>(query).ToList());
            //}
            return records;
		}

		public List<UnitKerja> GetUnitkerjaKop(int tipekantor = 0, bool tampil = false ,int eselon = 0, string namelike = null)
		{
			string skema = OtorisasiUser.NamaSkema;
			List<UnitKerja> units = new List<UnitKerja>();
			var arrayListParameters = new ArrayList();
			string query = $@"SELECT UNITKERJAID, INDUK, NAMAUNITKERJA, ESELON, TIPEKANTORID, KANTORID, ROW_NUMBER() OVER (ORDER BY {1}) AS RNUMBER, CASE WHEN UNITKERJAID IS NULL THEN 0 ELSE 1 END TOTAL  
							  FROM {skema}.TBLKANTOR 
							  WHERE UNITKERJAID IS NOT NULL";

			if(tipekantor > 0)
            {
				query += " AND TIPEKANTORID = :tipekantor ";
				arrayListParameters.Add(new NpgsqlParameter("tipekantor", tipekantor));
			}
		   // if (tampil)
		   // {
			//	query += " AND UK.TAMPIL = 1";
			//}
			if(eselon > 0)
            {
				query += " AND ESELON = :eselon ";
				arrayListParameters.Add(new NpgsqlParameter("eselon", eselon));
			}
            if (!string.IsNullOrEmpty(namelike))
            {
				query += " AND NAMAUNITKERJA LIKE :namelike ";
				arrayListParameters.Add(new NpgsqlParameter("namelike", $"%{namelike}%"));
			}

			using (var ctx = new PostgresDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				units = ctx.Database.SqlQuery<UnitKerja>(query, parameters).ToList();
			}

			return units;
		}

		public KopSurat GetKopDetailbyUnitKerja(string kantorid)
		{
			KopSurat kop = new KopSurat();
			string query = $@"SELECT NAMA AS UNITKERJANAME, NAMA AS NAMAKANTOR_L1, ALAMAT, TELEPON, EMAIL, 11 AS FONTSIZE FROM KANTOR WHERE KANTORID = '{kantorid}'";

			using (var ctx = new BpnDbContext())
			{
				kop = ctx.Database.SqlQuery<KopSurat>(query).FirstOrDefault();
			}
			return kop;
		}

		public string GetNamaJabatanByPegawaiId(string pegawaiid)
		{
			string JabatanNama;
			string query = $@"SELECT JB.NAMA FROM JABATANPEGAWAI
							  INNER JOIN JABATAN JB ON JABATANPEGAWAI.PROFILEID = JB.PROFILEID
							  WHERE JABATANPEGAWAI.PEGAWAIID = '{pegawaiid}' AND (JABATANPEGAWAI.VALIDSAMPAI IS NULL OR 
							  TO_DATE(TRIM(JABATANPEGAWAI.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
							  AND JABATANPEGAWAI.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100')";

			using (var ctx = new BpnDbContext())
			{
				JabatanNama = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
			}
			return JabatanNama;
		}

		public TransactionResult checklampiraninfo(string lampiranid, string draftcode)
		{
			string skema = OtorisasiUser.NamaSkema;
			var tr = new TransactionResult() { Status = false, Pesan = "" , ReturnValue = ""};
			using (var ctx = new BpnDbContext())
			{
				var lampId = ctx.Database.SqlQuery<string>($"SELECT LAMPIRANID FROM {skema}.TBLLAMPIRANDRAFTSURAT WHERE STATUS = 'A' {(string.IsNullOrEmpty(draftcode)? "" : $" AND DRAFTCODE='{draftcode}' ")} {(string.IsNullOrEmpty(lampiranid) ? "" : $" AND LAMPIRANID = '{lampiranid}'" )} ").FirstOrDefault();

				if (string.IsNullOrEmpty(lampId))
				{
					using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
					{
						try
						{
							string sql = $@"
								INSERT INTO {skema}.TBLLAMPIRANDRAFTSURAT (LAMPIRANID, DRAFTCODE, STATUS, UPDTIME) 
								VALUES ('{lampiranid}','{draftcode}','A', SYSDATE)";
							ctx.Database.ExecuteSqlCommand(sql);
							tc.Commit();
							tr.Status = true;
							tr.Pesan = "Berhasil Menyimpan Data";
							tr.ReturnValue = lampiranid;
						}
						catch
						{
							tc.Rollback();
							tr.Status = false;
							tr.Pesan = "Gagal Menyimpan Data";
						}
						finally
						{
							tc.Dispose();
							ctx.Dispose();
						}
					}

					return tr;
				}
				else
				{
					tr.Status = false;
					tr.Pesan = "Lampiranid Sudah Terdaftar";
					tr.ReturnValue = lampId;
					return tr;
				}
			}
		}

		public List<Entities.Petugas> GetPetugasSatker(string unitkerjaid)
		{
			var list = new List<Entities.Petugas>();

			string query = $@"
						SELECT DISTINCT
							DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) AS DATA, 
							DECODE(PG.GELARDEPAN, '', '', PG.GELARDEPAN || ' ') || DECODE(PG.NAMA, '', '', PG.NAMA) || DECODE(PG.GELARBELAKANG, null, '', ', ' || PG.GELARBELAKANG) || ' ' || '|' || ' ' || (JB.NAMA) AS VALUE
						FROM
							PEGAWAI PG
							INNER JOIN JABATANPEGAWAI JP ON
							   JP.PEGAWAIID = PG.PEGAWAIID AND
							   JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND
							   (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
							   NVL(JP.STATUSHAPUS,'0') = '0'
							INNER JOIN JABATAN JB ON
							   JB.PROFILEID = JP.PROFILEID AND
							   (JB.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JB.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
							   (JB.UNITKERJAID = '{unitkerjaid}')
						WHERE
							PG.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(PG.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')
						UNION ALL
						SELECT DISTINCT
							INITCAP(PPNPN.NAMA) AS DATA, INITCAP(PPNPN.NAMA) || ' ' || '|' || ' ' ||'PPNPN' AS VALUE
						FROM PPNPN
							JOIN JABATANPEGAWAI JP ON
							   JP.PEGAWAIID = PPNPN.NIK AND
							   JP.PROFILEID NOT IN ('A81001','A81002','A81003','A81004','A80100','A80300','A80400','A80500','B80100') AND
							   (JP.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JP.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY')) AND
							   NVL(JP.STATUSHAPUS,'0') = '0'
							JOIN JABATAN JB ON
							   JB.PROFILEID = JP.PROFILEID AND
							   (JB.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(JB.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))  AND
							   (JB.UNITKERJAID = '{unitkerjaid}')";

			

			using (var ctx = new BpnDbContext())
			{
				list = ctx.Database.SqlQuery<Entities.Petugas>(query).ToList();
			}

			return list;
		}

		public List<UnitKerja> GetUnitKerjaForSelect(string unitkerjaid)
        {
			//get unitkerja yang didalamnya terdapat unitkerja user, dan induk unitkerja
			//fungsi untuk memilih pejabat dalam konsep surat

			var result = new List<UnitKerja>();
			string sql = $@"
						SELECT
							UNITKERJAID,
							NAMAUNITKERJA,
							INDUK,
							KODE,
							KANTORID 
						FROM
							UNITKERJA 
						WHERE
							UNITKERJAID = '{unitkerjaid}'
					";
			var firstrow = new BpnDbContext().Database.SqlQuery<UnitKerja>(sql).FirstOrDefault();
			result.Add(firstrow);
			int tipeKantor = new DataMasterModel().GetTipeKantor(firstrow.KantorId);
			if (tipeKantor > 1)
            {
				return result;
            }
			if (!string.IsNullOrEmpty(firstrow.Induk))
            {
				sql = $@"
						SELECT
							UNITKERJAID,
							NAMAUNITKERJA,
							INDUK,
							KODE,
							KANTORID 
						FROM
							UNITKERJA 
						WHERE
							UNITKERJAID = '{firstrow.Induk}'
					";
				var secondrow = new BpnDbContext().Database.SqlQuery<UnitKerja>(sql).FirstOrDefault();
				result.Add(secondrow);
			} else
            {
				sql = $@"
						SELECT
							UNITKERJAID,
							NAMAUNITKERJA,
							INDUK,
							KODE,
							KANTORID 
						FROM
							UNITKERJA 
						WHERE
							UNITKERJAID = '{unitkerjaid.Substring(0,6)}'
					";
				var secondrow = new BpnDbContext().Database.SqlQuery<UnitKerja>(sql).FirstOrDefault();
                if (!string.IsNullOrEmpty(secondrow.UnitKerjaId))
                {
					result.Add(secondrow);
                }
			}
			return result;
		}

		public bool CheckIsPLT(string pegawaiid, string profileid)
		{
			bool result = false;
			var arrayListParameters = new ArrayList();
			string query = @"
                SELECT COUNT(1)
                FROM JABATANPEGAWAI
                WHERE
	                PROFILEID = :profileid AND
	                PEGAWAIID = :pegawaiid AND
	                (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(STATUSHAPUS,'0') = '0' AND
	                STATUSPLT = '1'";
			arrayListParameters.Add(new OracleParameter("profileid", profileid));
			arrayListParameters.Add(new OracleParameter("pegawaiid", pegawaiid));
			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				result = (ctx.Database.SqlQuery<int>(query, parameters).First() > 0);
			}

			return result;
		}
		public bool CheckIsPLH(string pegawaiid, string profileid)
		{
			bool result = false;
			var arrayListParameters = new ArrayList();
			string query = @"
                SELECT COUNT(1)
                FROM JABATANPEGAWAI
                WHERE
	                PROFILEID = :profileid AND
	                PEGAWAIID = :pegawaiid AND
	                (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                NVL(STATUSHAPUS,'0') = '0' AND
	                STATUSPLT = '2'";
			arrayListParameters.Add(new OracleParameter("profileid", profileid));
			arrayListParameters.Add(new OracleParameter("pegawaiid", pegawaiid));
			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				result = (ctx.Database.SqlQuery<int>(query, parameters).First() > 0);
			}

			return result;
		}

		public List<PegawaiDetail> GetPegawaiDetailByUnitKerja(string UnitKerja)
        {
			var arrayListParameters = new ArrayList();
			string sql = $@"
				SELECT 
					PG.PEGAWAIID, 
					JB.PROFILEID, 
					JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', '') AS NAMAJABATAN, 
					SM.NAMA_LENGKAP AS NAMA, 
					SM.ESELON, 
					SM.PANGKAT, 
					SM.GOLONGAN
				FROM PEGAWAI PG
				INNER JOIN JABATANPEGAWAI JP
					ON JP.PEGAWAIID = PG.PEGAWAIID
					AND JP.PROFILEID NOT IN ( 'A81001', 'A81002', 'A81003', 'A81004', 'A80100', 'A80300', 'A80400', 'A80500', 'B80100' ) 
					AND ( JP.VALIDSAMPAI IS NULL OR CAST( JP.VALIDSAMPAI AS TIMESTAMP ) > SYSDATE ) 
				INNER JOIN JABATAN JB 
					ON JB.PROFILEID = JP.PROFILEID
					AND JB.UNITKERJAID = :unitkerja
				INNER JOIN SIMPEG_2702.SIAP_VW_PEGAWAI SM
					ON JP.PEGAWAIID = SM.NIPBARU
				WHERE
					(PG.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(PG.VALIDSAMPAI),'DD/MM/YY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YY'))
				GROUP BY 
					PG.PEGAWAIID,  
					JB.NAMA || DECODE(JP.STATUSPLT, 1, ' (PLT)', 2, ' (PLH)', ''),  
					JB.PROFILEID,  
					SM.NAMA_LENGKAP, 
					SM.ESELON,  
					SM.PANGKAT,  
					SM.GOLONGAN
				UNION ALL
				SELECT
					PN.NIK, JB.PROFILEID,
					'PPNPN' AS NAMAJABATAN,
					INITCAP(PN.NAMA) AS NAMA, NULL AS ESELON, NULL AS PANGKAT, NULL AS GOLONGAN
				FROM PPNPN PN 
				INNER JOIN JABATANPEGAWAI JP
					ON JP.PEGAWAIID = PN.NIK
					AND JP.PROFILEID NOT IN ( 'A81001', 'A81002', 'A81003', 'A81004', 'A80100', 'A80300', 'A80400', 'A80500', 'B80100' ) 
					AND ( JP.VALIDSAMPAI IS NULL OR CAST( JP.VALIDSAMPAI AS TIMESTAMP ) > SYSDATE ) 
				INNER JOIN JABATAN JB 
					ON JB.PROFILEID = JP.PROFILEID
					AND JB.UNITKERJAID = :unitkerja
				ORDER BY ESELON";

			arrayListParameters.Add(new OracleParameter("unitkerja", UnitKerja));
			try
            {
				var parameters = arrayListParameters.OfType<object>().ToArray();
				return new BpnDbContext().Database.SqlQuery<PegawaiDetail>(sql, parameters).ToList();
			} 
			catch (Exception e)
            {
				return new List<PegawaiDetail>();

			}
		}

		public List<string> GetKonsepParticipant(string DraftCode)
        {
			string skema = OtorisasiUser.NamaSkema;
			string sql = $@"SELECT
								TBLLOGDRAFT.USERID 
							FROM
								{skema}.TBLLOGDRAFT 
							WHERE
								DRAFTCODE = '{DraftCode}' 
								AND TBLLOGDRAFT.LOGTEXT = 'New' 
							UNION
							SELECT
								PDS.USERID 
							FROM
								{skema}.KOORDINASIDRAFT PDS 
							WHERE
								PDS.DRAFTCODE = '{DraftCode}'  AND PDS.STATUS != 'D'";
			try
			{
				return new BpnDbContext().Database.SqlQuery<string>(sql).ToList();
			}
			catch
			{
				return new List<string>();

			}
		}

		public bool RevisiCheck(string DraftCode)
        {
			string skema = OtorisasiUser.NamaSkema;
			string sql = $@"SELECT COUNT(1) FROM {skema}.KOORDINASIDRAFT WHERE STATUS = 'X' AND DRAFTCODE = '{DraftCode}' AND STATUS != 'D'";
			try
			{
				return new BpnDbContext().Database.SqlQuery<decimal>(sql).FirstOrDefault() > 0;
			}
			catch
			{
				return false;

			}
		}

		public string GetIdDokumenTTE(string dc)
		{
			string skema = OtorisasiUser.NamaSkema;
			string sql = $@"SELECT DOKUMENELEKTRONIKID FROM {skema}.TBLDOKUMENELEKTRONIK WHERE KODEFILE = '{dc}'";
			try
			{
				return new BpnDbContext().Database.SqlQuery<string>(sql).FirstOrDefault();
			}
			catch
			{
				return string.Empty;

			}
		}

		public decimal GetJumlahPersetujuanKonsep(string userid)
        {
			string skema = OtorisasiUser.NamaSkema;
			string sql = $@"SELECT
								COUNT(1) 
							FROM
								(
								SELECT DISTINCT
									KD.DRAFTCODE 
								FROM
									{skema}.KOORDINASIDRAFT KD
									INNER JOIN {skema}.TBLDRAFTSURAT TDS ON KD.DRAFTCODE = TDS.DRAFTCODE 
									AND TDS.STATUS = 'W' 
									AND KD.STATUS != 'D'
								WHERE
									KD.USERID = '{userid}' 
									AND KD.STATUS = 'W' 
								)";
			try
			{
				return new BpnDbContext().Database.SqlQuery<decimal>(sql).FirstOrDefault();
			}
			catch
			{
				return 0;

			}
		}

		public bool ProfileIdIsTU(string profileId)
        {
			string sql = $@"SELECT PROFILEID FROM JABATAN WHERE PROFILEID = PROFILEIDTU AND PROFILEID = '{profileId}'";
			return new BpnDbContext().Database.SqlQuery<string>(sql).FirstOrDefault() != null;
		}

		public string TuUnitKerja(string unitKerja)
		{
			 string sql = $@"SELECT
							JB.PROFILEID 
						FROM
							JABATAN JB
							LEFT JOIN UNITKERJA UK ON JB.UNITKERJAID = UK.UNITKERJAID 
						WHERE
							JB.PROFILEID = (
							SELECT
								PROFILEIDTU 
							FROM
								JABATAN 
							WHERE
								UNITKERJAID = '{unitKerja}' 
								AND TIPEESELONID = ( SELECT MIN( TIPEESELONID ) FROM JABATAN WHERE UNITKERJAID = '{unitKerja}' AND TIPEESELONID IS NOT NULL AND TIPEESELONID <> 0 ) 
								AND ( JB.VALIDSAMPAI IS NULL OR TRUNC( CAST( JB.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
							)";

			return new BpnDbContext().Database.SqlQuery<string>(sql).FirstOrDefault();
		}

		public string NdUnitkerjaNama(string profileId, string pegawaiid)
		{
			string sql = $@"SELECT UK.NAMAUNITKERJA FROM JABATAN JB 
							LEFT JOIN JABATANPEGAWAI JP ON JB.PROFILEID = JP.PROFILEID AND ( JP.VALIDSAMPAI IS NULL OR TRUNC( CAST( JP.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) 
							AND JP.PEGAWAIID = '{pegawaiid}'
							LEFT JOIN UNITKERJA UK ON JB.UNITKERJAID = UK.UNITKERJAID
							WHERE JB.PROFILEID =  '{profileId}'
							AND ( JB.VALIDSAMPAI IS NULL OR TRUNC( CAST( JB.VALIDSAMPAI AS TIMESTAMP ) ) >= TRUNC( SYSDATE ) ) ";
			return new BpnDbContext().Database.SqlQuery<string>(sql).FirstOrDefault();
		}

		public List<TipeSurat> GetFormatPenomoran()
		{
			var list = new List<Models.Entities.TipeSurat>();

			string query =
				"SELECT " +
				"   tipesurat.nama NamaTipeSurat, tipesurat.formatnomor, TO_CHAR(Durasi) as ValueTipeSurat " +
				"FROM surat.tipesurat " +
				"WHERE tipesurat.aktif = 9";

			using (var ctx = new BpnDbContext())
			{
				list = ctx.Database.SqlQuery<TipeSurat>(query).ToList<TipeSurat>();
			}

			return list;
		}
		
		public bool SimpanFormatNomor(string action, string namaTipeSurat, string FormatNomor, string KodeJenis)
        {
			string skema = OtorisasiUser.NamaSkema;
			string sql = string.Empty;
			bool status = false;
            if (OtorisasiUser.NamaSkema == "surattrain")
            {
                return status;
            }
            using (var ctx = new BpnDbContext())
            {
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					if (action.Equals("Insert"))
					{
						string kodeJenis = string.IsNullOrEmpty(KodeJenis) ? "TN" : KodeJenis;
						sql = $@"INSERT INTO {skema}.TIPESURAT (NAMA, INDUK, AKTIF, URUTAN, FORMATNOMOR, DURASI)
								VALUES ('{kodeJenis}|{namaTipeSurat}','Tata Naskah', 9, 0, '{FormatNomor}', 0)";
						try
						{
							ctx.Database.ExecuteSqlCommand(sql);
							tc.Commit();
							status = true;
						}
						catch (Exception e)
						{
							tc.Rollback();
						}
						finally
						{
							tc.Dispose();
							ctx.Dispose();
						}
					}
					else if (action.Equals("Delete")) {
						sql = $@"UPDATE {skema}.TIPESURAT SET AKTIF = 0  WHERE NAMA = '{namaTipeSurat}' AND FORMATNOMOR = '{FormatNomor}' AND AKTIF = 9";
						try
						{
							ctx.Database.ExecuteSqlCommand(sql);
							tc.Commit();
							status = true;
						}
						catch
						{
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

			return status;
		}

		public string PenomoranBuilder(DraftSurat data)
        {
			string skema = OtorisasiUser.NamaSkema;
			BpnDbContext db = new BpnDbContext();
			try
            {
				int eselon = db.Database.SqlQuery<int>($"SELECT NVL(TIPEESELONID,0) FROM JABATAN WHERE PROFILEID = '{data.ProfilePengirim}'").FirstOrDefault();
				if(eselon == 0 && eselon > 2)
                {
					return "";
                }
				TipeSurat TS = db.Database.SqlQuery<TipeSurat>($"SELECT NAMA AS NAMATIPESURAT, FORMATNOMOR FROM {skema}.TIPESURAT WHERE AKTIF = 9 AND SUBSTR(NAMA, 4, LENGTH(NAMA)) = '{data.TipeSurat}' ").FirstOrDefault();
				if(TS == null)
                {
					return "";
                }
				var format = TS.FormatNomor;
                if (!string.IsNullOrEmpty(format))
                {
					format = HttpUtility.UrlDecode(format);
					string kodeTTD = db.Database.SqlQuery<string>($"SELECT UK.KODE FROM UNITKERJA UK WHERE UNITKERJAID = (SELECT UNITKERJAID FROM JABATAN WHERE PROFILEID = '{data.ProfilePengirim}')").FirstOrDefault();
					int countKode = kodeTTD.Count(f => f == '.');
					if(countKode < 2 && !string.IsNullOrEmpty(kodeTTD))
                    {
						format = format.Replace("<Penandatangan>", kodeTTD);
						format = format.Replace("<Arsip>", data.KodeArsip);
						format = format.Replace("<Kode>", TS.NamaTipeSurat.Split('|')[0]);
						format = format.Replace("<Bulan>", ToRoman(DateTime.Now.Month));
						format = format.Replace("<Tahun>", DateTime.Now.Year.ToString());
					}
				}
				return format;
			} catch (Exception e)
            {
				return "";
            }
        }

		public string ToRoman(int number)
		{
			if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
			if (number < 1) return string.Empty;
			if (number >= 1000) return "M" + ToRoman(number - 1000);
			if (number >= 900) return "CM" + ToRoman(number - 900);
			if (number >= 500) return "D" + ToRoman(number - 500);
			if (number >= 400) return "CD" + ToRoman(number - 400);
			if (number >= 100) return "C" + ToRoman(number - 100);
			if (number >= 90) return "XC" + ToRoman(number - 90);
			if (number >= 50) return "L" + ToRoman(number - 50);
			if (number >= 40) return "XL" + ToRoman(number - 40);
			if (number >= 10) return "X" + ToRoman(number - 10);
			if (number >= 9) return "IX" + ToRoman(number - 9);
			if (number >= 5) return "V" + ToRoman(number - 5);
			if (number >= 4) return "IV" + ToRoman(number - 4);
			if (number >= 1) return "I" + ToRoman(number - 1);
			throw new ArgumentOutOfRangeException("something bad happened");
		}

		public int RomanToInt(string s)
		{
			int sum = 0;
			Dictionary<char, int> romanNumbersDictionary = new Dictionary<char, int>()
			{
				{ 'I', 1 },
				{ 'V', 5 },
				{ 'X', 10 },
				{ 'L', 50 },
				{ 'C', 100 },
				{ 'D', 500 },
				{ 'M', 1000 }
			};
			for (int i = 0; i < s.Length; i++)
			{
				char currentRomanChar = s[i];
				romanNumbersDictionary.TryGetValue(currentRomanChar, out int num);
				if (i + 1 < s.Length && romanNumbersDictionary[s[i + 1]] > romanNumbersDictionary[currentRomanChar])
				{
					sum -= num;
				}
				else
				{
					sum += num;
				}
			}
			return sum;
		}

		public DokumenTTE fDokTTEPengantarSuratMasuk(string psid)
		{
			string skema = OtorisasiUser.NamaSkema;
			string dokTTE = string.Empty;
			var data = new DokumenTTE();
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT T2.DOKUMENELEKTRONIKID, T2.STATUS
							FROM {skema}.TBLDOKUMENELEKTRONIK T1
							INNER JOIN {skema}.TBLDOKUMENTTE T2 
								ON T1.DOKUMENELEKTRONIKID = T2.DOKUMENELEKTRONIKID 
								AND T2.TIPE = 1
							WHERE T1.NAMAFILE = 'Pengantar_['||:psid||'].pdf'
							ORDER BY T1.TANGGALDIBUAT DESC";
			arrayListParameters.Add(new OracleParameter("psid", psid));
			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				data = ctx.Database.SqlQuery<DokumenTTE>(sql, parameters).FirstOrDefault();
			}

			return data;
		}

		public TransactionResult setEditorAktif(string param, string tipe)
        {
			var tr = new TransactionResult() {Status = false };
			var arrayListParameters = new ArrayList();
			string sql = $@"UPDATE SURAT.TIPESURAT
							SET DURASI = :param
							WHERE NAMA = :tipe AND INDUK = 'Tata Naskah'";
			arrayListParameters.Add(new OracleParameter("param", int.Parse(param)));
			arrayListParameters.Add(new OracleParameter("tipe", tipe));
			using (var ctx = new BpnDbContext())
            {
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
						var parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql,parameters);
						tc.Commit();
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

        public int nomorAkhirPengantar(string unitkerja)
        {
			string skema = OtorisasiUser.NamaSkema;
			ArrayList arrayListParameters = new ArrayList();
			int num = 0;
			string sql = $@"SELECT
                                SUBSTR(PT.NOMOR,0, INSTR(PT.NOMOR, '/', 1, 1) -1)
							FROM {skema}.PENGANTARSURAT PT
							WHERE PT.NOMOR IS NOT NULL AND PT.TUJUAN IS NOT NULL AND PT.TANGGALSURAT IS NOT NULL AND PT.KANTORID = :unitkerja AND EXTRACT(YEAR FROM PT.TANGGALSURAT) = :nowyear
							ORDER BY PT.TANGGALSURAT DESC, PT.NOMOR DESC";
			arrayListParameters.Add(new OracleParameter("unitkerja", unitkerja));
			arrayListParameters.Add(new OracleParameter("nowyear", DateTime.Now.Year.ToString()));
			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				string numStr = ctx.Database.SqlQuery<string>(sql,parameters).First();
                if (!string.IsNullOrEmpty(numStr))
                {
					bool success = int.TryParse(numStr, out int number);
					if (success)
                    {
						num = int.Parse(numStr);
					}					
				}
			}

			return num;
        }
    }
}