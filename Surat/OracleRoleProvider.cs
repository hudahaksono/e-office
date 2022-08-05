using System;
using System.Data;
using System.Configuration.Provider;
using System.Collections.Specialized;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Collections.Generic;
using System.Linq;
using Surat.Models;

namespace Surat
{
    public class OracleRoleProvider : RoleProvider
    {

        //
        // Global connection string, generic exception message, event log info.
        //

        private string rolesTable = "Roles";
        private string usersInRolesTable = "UsersInRoles";

        private string eventSource = "OracleRoleProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";

        private ConnectionStringSettings pConnectionStringSettings;
        private string connectionString;

        private bool pWriteExceptionsToEventLog = false;


        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }


        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "OracleRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Sample Oracle Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);


            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }


            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                {
                    pWriteExceptionsToEventLog = true;
                }
            }


            // Initialize OracleConnection.

            pConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (pConnectionStringSettings == null || pConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = pConnectionStringSettings.ConnectionString;
        }



        //
        // System.Web.Security.RoleProvider properties.
        //


        private string pApplicationName;


        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        //
        // System.Web.Security.RoleProvider methods.
        //

        //
        // RoleProvider.AddUsersToRoles
        //

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                if (username.IndexOf(',') > 0)
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }


            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("INSERT INTO " + usersInRolesTable + " " +
                    " (Username, Rolename, ApplicationName) " +
                    " Values(:Username, :Rolename, :ApplicationName)", conn);

            OracleParameter userParm = cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255);
            OracleParameter roleParm = cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255);
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            OracleTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        userParm.Value = username;
                        roleParm.Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch (OracleException e)
            {
                try
                {
                    tran.Rollback();
                }
                catch { }


                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "AddUsersToRoles");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                conn.Close();
            }
        }


        //
        // RoleProvider.CreateRole
        //

        public override void CreateRole(string rolename)
        {
            if (rolename.IndexOf(',') > 0)
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(rolename))
            {
                throw new ProviderException("Role name already exists.");
            }

            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("INSERT INTO " + rolesTable + " " +
                    " (Rolename, ApplicationName) " +
                    " Values(:Rolename, :ApplicationName)", conn);

            cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255).Value = rolename;
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "CreateRole");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                conn.Close();
            }
        }


        //
        // RoleProvider.DeleteRole
        //

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("DELETE FROM " + rolesTable + " " +
                    " WHERE Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);

            cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255).Value = rolename;
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;


            OracleCommand cmd2 = new OracleCommand("DELETE FROM " + usersInRolesTable + " " +
                    " WHERE Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);

            cmd2.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255).Value = rolename;
            cmd2.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            OracleTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                cmd2.Transaction = tran;

                cmd2.ExecuteNonQuery();
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (OracleException e)
            {
                try
                {
                    tran.Rollback();
                }
                catch { }


                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "DeleteRole");

                //    return false;
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                conn.Close();
            }

            return true;
        }


        //
        // RoleProvider.GetAllRoles
        //

        public override string[] GetAllRoles()
        {
            string tmpRoleNames = "";

            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("SELECT Rolename FROM " + rolesTable + " " +
                      " WHERE ApplicationName = :ApplicationName", conn);

            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            OracleDataReader reader = null;

            try
            {
                conn.Open();

                using (reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tmpRoleNames += reader.GetString(0) + ",";
                    }
                    reader.Close();
                }
            }
            catch (OracleException e)
            {
                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "GetAllRoles");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }

            if (tmpRoleNames.Length > 0)
            {
                // Remove trailing comma.
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);
                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }


        //
        // RoleProvider.GetRolesForUser
        //

        public override string[] GetRolesForUser(string username)
        {
            var userRoles = new List<string>();

            try
            {
                OracleParameter p1 = new OracleParameter("param1", username);
                OracleParameter p2 = new OracleParameter("param2", ApplicationName);
                object[] myParams = new object[2] { p1, p2 };

                string sql = "SELECT Rolename FROM " + usersInRolesTable + " WHERE Username = :param1 AND ApplicationName = :param2";
                using (var ctx = new BpnDbContext())
                {
                    userRoles = ctx.Database.SqlQuery<string>(sql, myParams).ToList();
                }
            }
            catch (OracleException e)
            {
                throw e;
            }

            return userRoles.ToArray();
        }


        //
        // RoleProvider.GetUsersInRole
        //

        public override string[] GetUsersInRole(string rolename)
        {
            string tmpUserNames = "";

            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("SELECT Username FROM " + usersInRolesTable + " " +
                      " WHERE Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);

            cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255).Value = rolename;
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            OracleDataReader reader = null;

            try
            {
                conn.Open();

                using (reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tmpUserNames += reader.GetString(0) + ",";
                    }
                    reader.Close();
                }
            }
            catch (OracleException e)
            {
                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "GetUsersInRole");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }


        //
        // RoleProvider.IsUserInRole
        //

        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;

            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM " + usersInRolesTable + " " +
                    " WHERE Username = :Username AND Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);

            cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255).Value = username;
            cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255).Value = rolename;
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            try
            {
                conn.Open();

                long numRecs = Convert.ToInt64(cmd.ExecuteScalar());

                if (numRecs > 0)
                {
                    userIsInRole = true;
                }
            }
            catch (OracleException e)
            {
                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "IsUserInRole");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                conn.Close();
            }

            return userIsInRole;
        }


        //
        // RoleProvider.RemoveUsersFromRoles
        //

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }


            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("DELETE FROM " + usersInRolesTable + " " +
                    " WHERE Username = :Username AND Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);

            OracleParameter userParm = cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255);
            OracleParameter roleParm = cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255);
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            OracleTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        userParm.Value = username;
                        roleParm.Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch (OracleException e)
            {
                try
                {
                    tran.Rollback();
                }
                catch { }


                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "RemoveUsersFromRoles");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                conn.Close();
            }
        }


        //
        // RoleProvider.RoleExists
        //

        public override bool RoleExists(string rolename)
        {
            bool exists = false;

            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM " + rolesTable + " " +
                      " WHERE Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);

            cmd.Parameters.Add(":Rolename", OracleDbType.Varchar2, 255).Value = rolename;
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = ApplicationName;

            try
            {
                conn.Open();

                long numRecs = Convert.ToInt64(cmd.ExecuteScalar());

                if (numRecs > 0)
                {
                    exists = true;
                }
            }
            catch (OracleException e)
            {
                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "RoleExists");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                conn.Close();
            }

            return exists;
        }

        //
        // RoleProvider.FindUsersInRole
        //

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            OracleConnection conn = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand("SELECT Username FROM " + usersInRolesTable + " " +
                      "WHERE Username LIKE :UsernameSearch AND Rolename = :Rolename AND ApplicationName = :ApplicationName", conn);
            cmd.Parameters.Add(":UsernameSearch", OracleDbType.Varchar2, 255).Value = usernameToMatch;
            cmd.Parameters.Add(":RoleName", OracleDbType.Varchar2, 255).Value = rolename;
            cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = pApplicationName;

            string tmpUserNames = "";
            OracleDataReader reader = null;

            try
            {
                conn.Open();

                using (reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tmpUserNames += reader.GetString(0) + ",";
                    }
                    reader.Close();
                }
            }
            catch (OracleException e)
            {
                //if (WriteExceptionsToEventLog)
                //{
                //    WriteToEventLog(e, "FindUsersInRole");
                //}
                //else
                //{
                throw e;
                //}
            }
            finally
            {
                if (reader != null) { reader.Close(); }

                conn.Close();
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }

        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //

        private void WriteToEventLog(OracleException e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = exceptionMessage + "\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }
    }
}