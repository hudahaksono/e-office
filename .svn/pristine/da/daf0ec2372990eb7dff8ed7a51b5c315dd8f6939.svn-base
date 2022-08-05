using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class HakAksesModel
    {
        public string[] GetRolesForUser(string pegawaiid, string kantorid)
        {
            var userRoles = new List<string>();

            try
            {
                var arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pegawaiid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", kantorid));
                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                string sql = @"
                    SELECT
	                    JB.ROLENAME
                    FROM JABATANPEGAWAI JP
                    INNER JOIN JABATAN JB ON
	                    JB.PROFILEID = JP.PROFILEID AND
	                    (JB.VALIDSAMPAI IS NULL OR TRUNC(CAST(JB.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE)) AND
	                    JB.SEKSIID = 'A800'
                    WHERE
	                    JP.PEGAWAIID = :param1 AND
	                    JP.KANTORID = :param2 AND
	                    NVL(JP.STATUSHAPUS,'0') = '0' AND
	                    (JP.VALIDSAMPAI IS NULL OR TRUNC(CAST(JP.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                    GROUP BY
	                    JB.ROLENAME";

                using (var ctx = new BpnDbContext())
                {
                    userRoles = ctx.Database.SqlQuery<string>(sql, parameters).ToList();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return userRoles.ToArray();
        }
        

        public string[] GetProfileIdForUser(string pegawaiid, string kantorid)
        {
            Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

            var userRoles = new List<string>();

            try
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", kantorid);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pegawaiid);
                object[] myParams = new object[2] { p1, p2 };

                string sql =
                    "SELECT distinct profileid FROM jabatanpegawai JB  " +
                    "WHERE (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND NVL(STATUSHAPUS,'0') = '0' " +
                    "AND JB.KANTORID = :param1 " +
                    "AND JB.pegawaiid = :param2 " +
                    "AND JB.profileid not in ('A81001','A81002','A81003','A81004','A80100','A80300')";

                //int tipekantorid = dataMasterModel.GetTipeKantor(kantorid);
                //if (tipekantorid == 1)
                //{
                //    sql +=
                //        "AND (jabatanpegawai.profileid like 'C%' " +
                //        "  OR jabatanpegawai.profileid like 'D%' " +
                //        "  OR jabatanpegawai.profileid like 'E%' " +
                //        "  OR jabatanpegawai.profileid like 'F%' " +
                //        "  OR jabatanpegawai.profileid like 'G%' " +
                //        "  OR jabatanpegawai.profileid like 'H%') ";
                //}
                //else if (tipekantorid == 2)
                //{
                //    sql += "AND jabatanpegawai.profileid like 'R%' ";
                //}
                //else if (tipekantorid == 3 || tipekantorid == 4)
                //{
                //    sql += "AND jabatanpegawai.profileid like 'N%' ";
                //}

                using (var ctx = new BpnDbContext())
                {
                    userRoles = ctx.Database.SqlQuery<string>(sql, myParams).ToList();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return userRoles.ToArray();
        }

        public bool isPriv(string pegawaiid, string unitkerjaid, string kode)
        {
            var rst = false;

            try
            {
                string sql = string.Format(@"
                    SELECT COUNT(1)
                    FROM {0}.TBLHAKAKSES THA
                    INNER JOIN JABATANPEGAWAI JB ON
	                    JB.PROFILEID = THA.PROFILEID AND
	                    (JB.VALIDSAMPAI IS NULL OR CAST(JB.VALIDSAMPAI AS TIMESTAMP) > SYSDATE) AND
	                    NVL(JB.STATUSHAPUS,'0') = '0' AND
	                    JB.PEGAWAIID = :param1
                    WHERE
	                    THA.KODE = :param2 AND
	                    THA.UNITKERJAID = :param3 AND
	                    NVL(THA.STATUS,'A') <> 'D'", System.Web.Mvc.OtorisasiUser.NamaSkema);
                var arrayListParameters = new ArrayList();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pegawaiid));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", kode));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", unitkerjaid));
                using (var ctx = new BpnDbContext())
                {
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    rst = ctx.Database.SqlQuery<int>(sql,parameters).FirstOrDefault() > 0;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return rst;
        }
    }
}