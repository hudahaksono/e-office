using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Surat.Models.Entities;

namespace Surat.Models
{
    public class ExpiredTTEModel
    {
        public ExpiredTTENotif GetExpired(string email)
        {
            using (var ctx = new BpnDbContext())
            {

                ArrayList arrayListParameters = new ArrayList();
                object[] parameters = null;

                var sql = "SELECT ID, NAMA,nvl(NIP,0) AS NIP, EMAIL, JENISSERTIFIKAT, TANGGALKADALUARSA,CREATEDDATE, UPDATEDDATE  FROM ttekadaluarsa where email = :email and JENISSERTIFIKAT = 'INDIVIDU'";

                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("email", email));
                parameters = arrayListParameters.OfType<object>().ToArray();

                var data = ctx.Database.SqlQuery<Entities.ExpiredTTE>(sql, parameters).ToList();

                if(data.Count > 0)
                {
                    var show = (data[0].UpdatedDate == null);
                    return new ExpiredTTENotif { Tanggal = data[0].TanggalKadaluarsa.ToString("dd/MM/yyyy"), Show = show };
                }

                return null;

            }

        }

        public bool UpdateExpired(string email)
        {
            using (var ctx = new BpnDbContext())
            {
                ArrayList arrayListParameters = new ArrayList();
                object[] parameters = null;

                var sql = "UPDATE TTEKADALUARSA SET UPDATEDDATE = sysdate WHERE EMAIL = :email";

                arrayListParameters.Clear();
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("email", email));
                parameters = arrayListParameters.OfType<object>().ToArray();

                var rowAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);

                if(rowAffected > 0)
                {
                    return true;
                }

                return false;

            }
                

            
        }
    }

    
}