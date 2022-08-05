using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Surat.Models
{
    public class AccountModel
    {
        [Required(ErrorMessage = "Nama pengguna harus diisi")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Kata sandi harus diisi")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        bool? rememberMe;
        [Display(Name = "Tetap login?")]
        public bool RememberMe
        {
            get { return rememberMe ?? false; }
            set { rememberMe = value; }
        }
        public string Token { get; set; }
        public string TokenEx { get; set; }

        public List<Entities.Pegawai> ListPegawaiByKantorProfile(string kantorid, string profileid)
        {
            List<Entities.Pegawai> listpegawai = new List<Entities.Pegawai>();

            using (var ctx = new BpnDbContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    ArrayList arrayListParameters = new ArrayList();
                    object[] parameters = null;

                    string sql =
                        "SELECT " +
                        "    pegawai.pegawaiid, UPPER(jabatan.nama) AS JABATAN, UPPER(pegawai.nama) AS NAMA, " +
                        "    UPPER(decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                        "        decode(pegawai.nama, '', '', pegawai.nama) || " +
                        "        decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang)) AS NAMALENGKAP " +
                        "FROM " +
                        "    pegawai, jabatan, jabatanpegawai " +
                        "WHERE " +
                        "    jabatan.profileid = jabatanpegawai.profileid " +
                        "    AND lower(pegawai.nama) not like '%pusdatin%' " +
                        "    AND (pegawai.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(pegawai.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) " +
                        "    AND (jabatanpegawai.VALIDSAMPAI IS NULL OR TO_DATE(TRIM(jabatanpegawai.VALIDSAMPAI),'DD/MM/YYYY') > TO_DATE(TRIM(SYSDATE),'DD/MM/YYYY')) " +
                        "    AND NVL(jabatanpegawai.STATUSHAPUS,'0') = '0' " +
                        "    AND pegawai.pegawaiid = jabatanpegawai.pegawaiid " +
                        "    AND jabatanpegawai.kantorid = :KantorId " +
                        "    AND jabatanpegawai.profileid IN (" + profileid + ") " +
                        "ORDER BY jabatan.tipeeselonid, jabatan.profileid, pegawai.nama";
                    arrayListParameters.Clear();
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                    parameters = arrayListParameters.OfType<object>().ToArray();
                    listpegawai = ctx.Database.SqlQuery<Entities.Pegawai>(sql, parameters).ToList();
                }
            }

            return listpegawai;
        }

        public string GetFotoPegawai(string nip)
        {
            string result = "";

            Regex sWhitespace = new Regex(@"\s+");
            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT foto
                    FROM SIAP_VW_PEGAWAI
                    WHERE NIPBARU = :Nip
                UNION ALL
                SELECT URLPROFILE AS FOTO
                 FROM PPNPN
                 WHERE NIK = :Nip";
            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Clear();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nip", nip));

            using (var ctx = new BpnDbContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }
    }
}