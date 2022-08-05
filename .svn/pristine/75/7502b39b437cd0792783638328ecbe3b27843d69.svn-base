using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Data.Entity.Infrastructure;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Surat.Models;

namespace Surat
{
    public class OracleMembershipProvider : MembershipProvider
    {

        private string pApplicationName;
        private bool pEnablePasswordReset;
        private bool pEnablePasswordRetrieval;
        private bool pRequiresQuestionAndAnswer;
        private bool pRequiresUniqueEmail;
        private int pMaxInvalidPasswordAttempts;
        private int pPasswordAttemptWindow;
        private MembershipPasswordFormat pPasswordFormat;

        private int newPasswordLength = 8;
        private string eventSource = "OracleMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string tableName = "Users";
        private string connectionString;

        private bool pWriteExceptionsToEventLog;

        private const string encryptionKey = "AE09F72B007CAAB5";

        private int pMinRequiredNonAlphanumericCharacters;
        private int pMinRequiredPasswordLength;
        private string pPasswordStrengthRegularExpression;

        /// <summary>
        /// System.Configuration.Provider.ProviderBase.Initialize Method
        /// </summary>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "OracleMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Oracle Membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    pPasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    pPasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    pPasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            // Initialize Oracle Connection.
            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;
        }

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return pEnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return pEnablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return pRequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return pRequiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return pMaxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return pPasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return pPasswordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return pMinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return pMinRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return pPasswordStrengthRegularExpression; }
        }


        #region System.Web.Security.MembershipProvider methods

        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
                return false;


            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            int rowsAffected = 0;

            try
            {
                OracleParameter p1 = new OracleParameter("param1", EncodePassword(newPwd));
                OracleParameter p2 = new OracleParameter("param2", username);
                //Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pApplicationName);
                //Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", DateTime.Now);

                object[] myParams = new object[2] { p1, p2 };

                using (var ctx = new BpnDbContext())
                {
                    string sql = "UPDATE " + tableName + " SET Password = :param1, LastPasswordChangedDate = sysdate WHERE username = :param2";
                    rowsAffected = ctx.Database.ExecuteSqlCommand(sql, myParams);
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPwdQuestion, string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            int rowsAffected = 0;

            try
            {
                OracleParameter p1 = new OracleParameter("param1", newPwdQuestion);
                OracleParameter p2 = new OracleParameter("param2", EncodePassword(newPwdAnswer));
                OracleParameter p3 = new OracleParameter("param3", username);
                OracleParameter p4 = new OracleParameter("param4", pApplicationName);
                object[] myParams = new object[4] { p1, p2, p3, p4 };

                using (var ctx = new BpnDbContext())
                {
                    string sql = "UPDATE " + tableName + " SET PasswordQuestion = :param1, PasswordAnswer = :param2, ApplicationName = :param4" + " WHERE Username = :param3";
                    rowsAffected = ctx.Database.ExecuteSqlCommand(sql, myParams);
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            //    return this.CreateUser(username, password, email,
            //                passwordQuestion, passwordAnswer,
            //                isApproved, providerUserKey, "","","",DateTime.Now,
            //                out status);
            //}

            //public MobileMembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, string nomorIndukKependudukan, string verifiedByUserId, string verifiedAtKantorId, DateTime verifiedDate, out MembershipCreateStatus status)
            //{
            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                int recAdded = 0;

                try
                {
                    OracleParameter p1 = new OracleParameter("param1", providerUserKey.ToString());
                    OracleParameter p2 = new OracleParameter("param2", username);
                    OracleParameter p3 = new OracleParameter("param3", EncodePassword(password));
                    OracleParameter p4 = new OracleParameter("param4", email);
                    OracleParameter p5 = new OracleParameter("param5", passwordQuestion);
                    OracleParameter p6 = new OracleParameter("param6", passwordAnswer == null ? "" : EncodePassword(passwordAnswer));
                    OracleParameter p7 = new OracleParameter("param7", isApproved ? 1 : 0);
                    OracleParameter p8 = new OracleParameter("param8", "Mobile Application User");
                    OracleParameter p9 = new OracleParameter("param9", createDate);
                    OracleParameter p10 = new OracleParameter("param10", pApplicationName);
                    object[] myParams = new object[8] { p1, p2, p3, p4, p5, p6, p7, p8 };

                    using (var ctx = new BpnDbContext())
                    {
                        string sql = "INSERT INTO " + tableName + " (USERID, Username, Password, Email, PasswordQuestion, PasswordAnswer, IsApproved, Commentar, CreationDate, LastPasswordChangedDate, LastActivityDate, LastLockedOutDate, FailedPwdAttemptWindowStart, FailedPwdAnswerAttWindowStart, LastLoginDate, FailedPwdAttemptCount, FailedPwdAnswerAttCount) Values (:param1, :param2, :param3, :param4, :param5, :param6, :param7, :param8, sysdate, sysdate, sysdate, sysdate, sysdate, sysdate, sysdate, 0, 0)";
                        recAdded = ctx.Database.ExecuteSqlCommand(sql, myParams);
                    }

                    if (recAdded > 0)
                    {
                        status = MembershipCreateStatus.Success;
                    }
                    else
                    {
                        status = MembershipCreateStatus.UserRejected;
                    }
                }
                catch (OracleException e)
                {
                    if (WriteExceptionsToEventLog)
                    {
                        throw e;
                    }

                    status = MembershipCreateStatus.ProviderError;
                }

                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }

            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            int rowsAffected = 0;

            try
            {
                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.
                }

                OracleParameter p1 = new OracleParameter("param1", username);
                object[] myParams = new object[1] { p1 };

                using (var ctx = new BpnDbContext())
                {
                    string sql = "DELETE FROM " + tableName + " WHERE Username = :param1";
                    rowsAffected = ctx.Database.ExecuteSqlCommand(sql, myParams);
                }

                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            if (rowsAffected > 0)
                return true;

            return false;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();
            MembershipUser user = null;

            try
            {
                OracleParameter p1 = new OracleParameter("param1", emailToMatch);
                object[] parameters = new object[1] { p1 };

                var memberList = new List<Member>();

                using (var ctx = new BpnDbContext())
                {
                    string end = (pageIndex * pageSize + pageSize).ToString();
                    string sql = "SELECT UserId, Username, Email, Password, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate FROM (SELECT USERID, Username, Email, PasswordQuestion,Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate,LastActivityDate, LastPasswordChangedDate, LastLockedOutDate, ROW_NUMBER() OVER (ORDER BY UserName ASC) ROWNUMS FROM " + tableName + ") WHERE ROWNUMS BETWEEN " + (pageIndex * pageSize).ToString() + " AND " + end;
                    memberList = ctx.MembershipData.SqlQuery(sql, parameters).ToList();


                    foreach (Member m in memberList)
                    {
                        user = new MembershipUser(Name, m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate);
                        users.Add(user);
                    }
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            return users;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();
            MembershipUser user = null;

            try
            {
                OracleParameter p1 = new OracleParameter("param1", usernameToMatch);
                object[] parameters = new object[1] { p1 };

                var memberList = new List<Member>();

                using (var ctx = new BpnDbContext())
                {
                    string end = (pageIndex * pageSize + pageSize).ToString();
                    string sql = "SELECT UserId, Username, Email, Password, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate FROM (SELECT USERID, Username, Email, PasswordQuestion,Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate,LastActivityDate, LastPasswordChangedDate, LastLockedOutDate, ROW_NUMBER() OVER (ORDER BY UserName ASC) ROWNUMS FROM " + tableName + ") WHERE ROWNUMS BETWEEN " + (pageIndex * pageSize).ToString() + " AND " + end;
                    memberList = ctx.MembershipData.SqlQuery(sql, parameters).ToList();

                    foreach (Member m in memberList)
                    {
                        user = new MembershipUser(Name, m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate);
                        users.Add(user);
                    }
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            return users;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();
            MembershipUser user = null;

            try
            {
                var memberList = new List<Member>();

                using (var ctx = new BpnDbContext())
                {
                    string end = (pageIndex * pageSize + pageSize).ToString();
                    string sql = "SELECT UserId, Username, Email, Password, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate FROM (SELECT USERID, Username, Email, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate, ROW_NUMBER() OVER (ORDER BY UserName ASC) ROWNUMS FROM " + tableName + ") WHERE ROWNUMS BETWEEN " + (pageIndex * pageSize).ToString() + " AND " + end;
                    memberList = ctx.MembershipData.SqlQuery(sql).ToList();

                    foreach (Member m in memberList)
                    {
                        user = new MembershipUser(Name, m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate);
                        users.Add(user);
                    }
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            int numOnline = 0;

            try
            {
                OracleParameter p1 = new OracleParameter("param1", compareTime);
                object[] parameters = new object[1] { p1 };

                using (var ctx = new BpnDbContext())
                {
                    string sql = "SELECT Count(1) FROM " + tableName + " WHERE LastActivityDate > :param1";
                    numOnline = ctx.Database.SqlQuery<Int32>(sql, parameters).SingleOrDefault();
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            return numOnline;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            string password = "";
            string passwordAnswer = "";

            try
            {
                Member m;
                using (var ctx = new BpnDbContext())
                {
                    OracleParameter p1 = new OracleParameter("param1", username);
                    object[] parameters = new object[1] { p1 };

                    string sql = "SELECT UserId, Username, Email, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate FROM " + tableName + " WHERE Username = :param1";
                    m = ctx.MembershipData.SqlQuery(sql).FirstOrDefault();
                }

                if (m != null)
                {
                    if (m.IsLockedOut == 1)
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }

                if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                {
                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                if (PasswordFormat == MembershipPasswordFormat.Encrypted)
                {
                    password = UnEncodePassword(password);
                }

                return password;
            }
            catch (Exception ex)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(ex.Message.ToString());
                }
                else
                {
                    throw ex;
                }
            }
        }

        public MembershipUser GetUser(string username)
        {
            MembershipUser u = null;
            //MembershipUser user = null;
            try
            {
                var memberList = new List<InternalMember>();
                using (var ctx = new BpnDbContext())
                {
                    OracleParameter p1 = new OracleParameter("param1", username);
                    object[] parameters = new object[1] { p1 };

                    string sql = "SELECT UserId, Username, Email, Password, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate, NomorIndukKependudukan, VerifiedByUserId, VerifiedAtKantorId, VerifiedDate FROM " + tableName + " WHERE Username = :param1";
                    memberList = ctx.InternalMembershipData.SqlQuery(sql, parameters).ToList();


                    if (memberList.Count > 0)
                    {
                        //if (userIsOnline)
                        //{
                        //Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", DateTime.Now);
                        //Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pApplicationName);
                        //Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param4", username);

                        //object[] parameters2 = new object[3] { p1, p2, p3 };
                        //string sql2 = "UPDATE " + tableName + " SET LastActivityDate = :param2, ApplicationName = :param3 WHERE Username = :param4";

                        //int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters2);
                        //}
                        InternalMember m = memberList.FirstOrDefault();
                        u = new MembershipUser(Name, m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate);

                        //u = new MobileMembershipUser(Name,
                        //    m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate,
                        //    m.NomorIndukKependudukan, m.VerifiedByUserId, m.VerifiedAtKantorId);
                        //MobMembershipUser u = new OdbcMembershipUser(this.Name,
                        //                username,
                        //                providerUserKey,
                        //                email,
                        //                passwordQuestion,
                        //                comment,
                        //                isApproved,
                        //                isLockedOut,
                        //                creationDate,
                        //                lastLoginDate,
                        //                lastActivityDate,
                        //                lastPasswordChangedDate,
                        //                lastLockedOutDate,
                        //                isSubscriber,
                        //                customerID);

                    }
                }
            }
            catch (Exception ex)
            {
                string tes = ex.Message.ToString();
            }

            return u;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            //MobileMembershipUser u = null;
            MembershipUser user = null;
            try
            {
                var memberList = new List<Member>();
                using (var ctx = new BpnDbContext())
                {
                    OracleParameter p1 = new OracleParameter("param1", username);
                    object[] parameters = new object[1] { p1 };

                    string sql = "SELECT UserId, Username, Email, Password, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate FROM " + tableName + " WHERE Username = :param1";
                    memberList = ctx.MembershipData.SqlQuery(sql, parameters).ToList();


                    if (memberList.Count > 0)
                    {
                        if (userIsOnline)
                        {
                            OracleParameter p2 = new OracleParameter("param2", DateTime.Now);
                            OracleParameter p3 = new OracleParameter("param3", pApplicationName);
                            OracleParameter p4 = new OracleParameter("param4", username);

                            object[] parameters2 = new object[3] { p1, p2, p3 };
                            string sql2 = "UPDATE " + tableName + " SET LastActivityDate = :param2, ApplicationName = :param3 WHERE Username = :param4";

                            int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters2);
                        }
                        Member m = memberList.FirstOrDefault();
                        user = new MembershipUser(Name, m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate);

                        //u = new MobileMembershipUser(Name,
                        //    m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate,
                        //    "NIK", "IKETUT", "GIANYAR", DateTime.Now);
                        //MobMembershipUser u = new OdbcMembershipUser(this.Name,
                        //                username,
                        //                providerUserKey,
                        //                email,
                        //                passwordQuestion,
                        //                comment,
                        //                isApproved,
                        //                isLockedOut,
                        //                creationDate,
                        //                lastLoginDate,
                        //                lastActivityDate,
                        //                lastPasswordChangedDate,
                        //                lastLockedOutDate,
                        //                isSubscriber,
                        //                customerID);

                    }
                }
            }
            catch (Exception ex)
            {
                string tes = ex.Message.ToString();
            }

            return user;
            //return u;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser user = null;
            var memberList = new List<Member>();
            Member m;

            using (var ctx = new BpnDbContext())
            {
                OracleParameter p1 = new OracleParameter("param1", providerUserKey);
                object[] parameters = new object[1] { p1 };

                string sql = "SELECT USERID, Username, Email, Password, PasswordQuestion, Commentar, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangedDate, LastLockedOutDate FROM " + tableName + " WHERE USERID = :USERID";
                memberList = ctx.MembershipData.SqlQuery(sql, parameters).ToList();



                if (memberList.Count > 0)
                {
                    if (userIsOnline)
                    {
                        OracleParameter p2 = new OracleParameter("param2", DateTime.Now);
                        OracleParameter p3 = new OracleParameter("param3", pApplicationName);
                        OracleParameter p4 = new OracleParameter("param4", providerUserKey);

                        object[] parameters2 = new object[3] { p2, p3, p4 };
                        string sql2 = "UPDATE " + tableName + " SET LastActivityDate = :param2, ApplicationName = :param3 WHERE UserId = :param4";

                        int rowsAffected = ctx.Database.ExecuteSqlCommand(sql2, parameters);

                    }

                    m = memberList.FirstOrDefault();
                    user = new MembershipUser(Name, m.Username, m.UserId, m.Email, m.PasswordQuestion, m.Commentar, m.IsApproved == 1, m.IsLockedOut == 1, m.CreationDate, m.LastLoginDate, m.LastActivityDate, m.LastPasswordChangedDate, m.LastLockedoutDate);

                }
            }

            return user;
        }

        public override string GetUserNameByEmail(string email)
        {
            try
            {
                OracleParameter p1 = new OracleParameter("param1", email);
                object[] parameters = new object[1] { p1 };

                var memberList = new List<Member>();

                using (var ctx = new BpnDbContext())
                {
                    string sql = "SELECT Username FROM " + tableName + " WHERE Email = :param1 ORDER BY Username Asc";
                    DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);
                    return (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && RequiresQuestionAndAnswer)
            {
                //UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword = System.Web.Security.Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");


            //OracleConnection conn = new OracleConnection(connectionString);
            //OracleCommand cmd = new OracleCommand("SELECT PasswordAnswer, IsLockedOut FROM " + tableName + " " +
            //      " WHERE Username = :Username", conn);

            //cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255).Value = username;

            int rowsAffected = 0;
            //string passwordAnswer = "";
            //OracleDataReader reader = null;

            try
            {
                //conn.Open();

                //using (reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                //{
                //    if (reader.HasRows)
                //    {
                //        reader.Read();

                //        if (reader.GetInt32(1) == 1)
                //            throw new MembershipPasswordException("The supplied user is locked out.");

                //        passwordAnswer = reader.GetString(0);
                //    }
                //    else
                //    {
                //        throw new MembershipPasswordException("The supplied user name is not found.");
                //    }
                //    reader.Close();
                //}

                //if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                //{
                //    //UpdateFailureCount(username, "passwordAnswer");

                //    throw new MembershipPasswordException("Incorrect password answer.");
                //}

                using (var ctx = new BpnDbContext())
                {
                    OracleParameter p0 = new OracleParameter("param0", username);
                    object[] parameters = new object[1] { p0 };
                    string sql = "SELECT PasswordAnswer FROM " + tableName + " WHERE Username = :param0 AND ISLOCKEDOUT = 0";
                    DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);

                    string passwordAnswer = (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                    if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                    {
                        throw new MembershipPasswordException("Incorrect password answer.");
                    }

                    OracleParameter p1 = new OracleParameter("param1", EncodePassword(newPassword));
                    //Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", DateTime.Now);
                    OracleParameter p2 = new OracleParameter("param2", pApplicationName);
                    OracleParameter p3 = new OracleParameter("param3", username);

                    parameters = new object[3] { p1, p2, p3 };
                    sql = "UPDATE " + tableName + " SET Password = :param1, LastPasswordChangedDate = SYSDATE, ApplicationName = :param2 WHERE username = :param3";

                    rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
                }
            }
            catch (OracleException e)
            {
                return e.Message.ToString();
                //if (WriteExceptionsToEventLog)
                //{
                //    throw new ProviderException(exceptionMessage);
                //}
                //else
                //{
                //    throw e;
                //}
            }

            if (rowsAffected > 0)
            {
                return newPassword;
            }
            else
            {
                return "User not found, or user is locked out. Password not Reset.";
                //throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }
        }

        public override bool UnlockUser(string username)
        {
            int rowsAffected = 0;

            using (var ctx = new BpnDbContext())
            {
                OracleParameter p1 = new OracleParameter("param1", DateTime.Now);
                OracleParameter p2 = new OracleParameter("param2", pApplicationName);
                OracleParameter p3 = new OracleParameter("param3", username);

                object[] parameters = new object[3] { p1, p2, p3 };
                string sql = "UPDATE " + tableName + " SET IsLockedOut = 0, LastLockedOutDate = :param1, ApplicationName = :param2 WHERE Username = :param3";

                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }

            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            using (var ctx = new BpnDbContext())
            {
                OracleParameter p1 = new OracleParameter("param1", user.UserName);
                //Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", user.Comment);
                //Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", user.IsApproved);
                //Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pApplicationName);
                //Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", user.UserName);

                object[] parameters = new object[1] { p1 };
                string sql = "UPDATE " + tableName + " SET IsApproved = 1 WHERE Username = :param1";

                int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            bool isApproved = false;
            string pwd = "";

            string namatable = "users";

            try
            {
                Member m;
                using (var ctx = new BpnDbContext())
                {
                    OracleParameter p1 = new OracleParameter("param1", username);
                    OracleParameter p2 = new OracleParameter("param2", EncodePassword(password));
                    object[] parameters = new object[2] { p1, p2 };

                    string sql = @"SELECT 
                                       UserId, Username, Email, Password, PasswordQuestion, 
                                       Commentar, IsApproved, IsLockedOut, 
                                       NVL(CreationDate,SYSDATE) CreationDate, 
                                       NVL(LastLoginDate,SYSDATE) LastLoginDate, 
                                       NVL(LastActivityDate,SYSDATE) LastActivityDate, 
                                       NVL(LastPasswordChangedDate,SYSDATE) LastPasswordChangedDate, 
                                       NVL(LastLockedOutDate,SYSDATE) LastLockedOutDate 
                                   FROM users 
                                   WHERE Username = :param1 AND password = :param2 AND IsLockedOut = 0 AND (VALIDSAMPAI IS NULL OR CAST(VALIDSAMPAI AS TIMESTAMP) > SYSDATE)";
                    if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
                    {
                        sql = sql.Replace(":param2", "NVL(password,:param2)");
                    }
                    m = ctx.MembershipData.SqlQuery(sql, parameters).FirstOrDefault();
                    if (m == null)
                    {
                        // Bila tidak ada di login Pegawai, cari di login PPNPN
                        sql = @"SELECT 
                                    UserId, Username, Email, Password, PasswordQuestion, 
                                    Commentar, IsApproved, IsLockedOut, 
                                    NVL(CreationDate,SYSDATE) CreationDate, 
                                    NVL(LastLoginDate,SYSDATE) LastLoginDate, 
                                    NVL(LastActivityDate,SYSDATE) LastActivityDate, 
                                    NVL(LastPasswordChangedDate,SYSDATE) LastPasswordChangedDate, 
                                    NVL(LastLockedOutDate,SYSDATE) LastLockedOutDate 
                                FROM userppnpn 
                                WHERE Username = :param1 AND password = :param2 AND IsLockedOut = 0 AND (VALIDSAMPAI IS NULL OR CAST(VALIDSAMPAI AS TIMESTAMP) > SYSDATE)";
                        if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin.Equals("surattrain"))
                        {
                            sql = sql.Replace(":param2", "NVL(password,:param2)");
                        }
                        m = ctx.MembershipData.SqlQuery(sql, parameters).FirstOrDefault();

                        namatable = "userppnpn";
                    }
                }

                if (m != null)
                {
                    pwd = m.Password;
                    isApproved = m.IsApproved == 1;
                }
                else
                {
                    return false;
                }

                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        isValid = true;
                        using (var ctx = new BpnDbContext())
                        {
                            OracleParameter p1 = new OracleParameter("param1", username);

                            object[] parameters = new object[1] { p1 };
                            string sql = "UPDATE " + namatable + " SET LastLoginDate = sysdate, LastActivityDate = sysdate WHERE Username = :param1";

                            int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                    }
                }
                else
                {
                    //using (var ctx = new BpnDbContext())
                    //{
                    //    OracleParameter p1 = new OracleParameter("param1", username);

                    //    object[] parameters = new object[1] { p1 };
                    //    string sql = "UPDATE " + namatable + " SET FAILEDPWDATTEMPTWINDOWSTART = sysdate WHERE Username = :param1";

                    //    int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
                    //}
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            return isValid;
        }

        public Models.Entities.TransactionResult ValidateLogin(string username, string password)
        {
            var tr = new Models.Entities.TransactionResult() { Status = false, Pesan = "", ReturnValue = "" };
            bool isApproved = false;
            var dateNow = DateTime.Today;
            string pwdDev = string.Concat("Eoffice",(dateNow.DayOfWeek+100).ToString().Substring(1), (dateNow.Month + 100).ToString().Substring(1));
            string namatable = "users";
            string tipe = "PEGAWAI";

            try
            {
                Member m;
                using (var ctx = new BpnDbContext())
                {
                    OracleParameter p1 = new OracleParameter("param1", username);
                    OracleParameter p2 = new OracleParameter("param2", EncodePassword(password));
                    object[] parameters = new object[2] { p1, p2 };

                    string sql = @"
                        SELECT
                          U.USERID, U.USERNAME, U.EMAIL, U.PASSWORD, U.PASSWORDQUESTION, U.COMMENTAR, U.ISAPPROVED,
                          U.ISLOCKEDOUT, NVL(U.CREATIONDATE,SYSDATE) CREATIONDATE,
                          NVL(U.LASTLOGINDATE,SYSDATE) LASTLOGINDATE,
                          NVL(U.LASTACTIVITYDATE,SYSDATE) LASTACTIVITYDATE,
                          NVL(U.LASTPASSWORDCHANGEDDATE,SYSDATE) LASTPASSWORDCHANGEDDATE,
                          NVL(U.LASTLOCKEDOUTDATE,SYSDATE) LASTLOCKEDOUTDATE,
                          TRUNC(NVL(U.VALIDSAMPAI, SYSDATE+1)) AS VALIDSAMPAI
                        FROM USERS U
                        INNER JOIN PEGAWAI P ON
                          P.USERID = U.USERID AND
                          (P.VALIDSAMPAI IS NULL OR TRUNC(CAST(P.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
                        WHERE
                          U.USERNAME = :param1 AND
                          U.PASSWORD = :param2 AND
                          U.ISLOCKEDOUT = 0";// AND
                          //(U.VALIDSAMPAI IS NULL OR TRUNC(CAST(U.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                    if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin.Equals("surattrain") && password.Equals(pwdDev))
                    {
                        sql = sql.Replace(":param2", "NVL(U.PASSWORD,:param2)");
                    }
                    m = ctx.MembershipData.SqlQuery(sql, parameters).FirstOrDefault();
                    if (m == null)
                    {
                        // Bila tidak ada di login Pegawai, cari di login PPNPN
                        sql = @"SELECT 
                                    UserId, Username, Email, Password, PasswordQuestion, 
                                    Commentar, IsApproved, IsLockedOut, 
                                    NVL(CreationDate,SYSDATE) CreationDate, 
                                    NVL(LastLoginDate,SYSDATE) LastLoginDate, 
                                    NVL(LastActivityDate,SYSDATE) LastActivityDate, 
                                    NVL(LastPasswordChangedDate,SYSDATE) LastPasswordChangedDate, 
                                    NVL(LastLockedOutDate,SYSDATE) LastLockedOutDate,
                                    TRUNC(NVL(VALIDSAMPAI, SYSDATE)) AS VALIDSAMPAI 
                                FROM userppnpn 
                                WHERE Username = :param1 AND password = :param2 AND IsLockedOut = 0";// AND (VALIDSAMPAI IS NULL OR TRUNC(CAST(VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))";
                        if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin.Equals("surattrain") && password.Equals(pwdDev))
                        {
                            sql = sql.Replace(":param2", "NVL(password,:param2)");
                        }
                        m = ctx.MembershipData.SqlQuery(sql, parameters).FirstOrDefault();

                        namatable = "userppnpn";
                        tipe = "PPNPN";
                    }
                }

                if (m != null)
                {
                    if(m.ValidSampai != null)
                    {
                        if(m.ValidSampai < DateTime.Now.Date)
                        {
                            tr.Pesan = string.Concat("Gagal login. Akun ", tipe, " sudah tidak valid, harap lakukan pengkinian data ", tipe);
                            return tr;
                        }
                    }
                    //pwd = m.Password;
                    //isApproved = m.IsApproved == 1;
                    tr.Status = true;
                    tr.Pesan = m.UserId;
                    tr.ReturnValue = tipe;
                    if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin.Equals("surattrain") && password.Equals(pwdDev))
                    {

                    }
                    else
                    {
                        using (var ctx = new BpnDbContext())
                        {
                            OracleParameter p1 = new OracleParameter("param1", username);

                            object[] parameters = new object[1] { p1 };
                            string sql = "UPDATE " + namatable + " SET LastLoginDate = sysdate, LastActivityDate = sysdate WHERE Username = :param1";

                            int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                    }
                }
                else
                {
                    tr.Pesan = "Gagal login. Nama pengguna atau Kata sandi yang Anda masukan salah.";
                    return tr;
                }
                /*
                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        tr.Status = true;
                        tr.Pesan = m.UserId;
                        tr.ReturnValue = tipe;
                        using (var ctx = new BpnDbContext())
                        {
                            //tr.Pesan = ctx.Database.SqlQuery<string>(string.Format("SELECT {0} FROM {1} WHERE USERID = '{2}'", tipe.Equals("PEGAWAI") ?"pegawaiid":"nik", tipe, m.UserId)).FirstOrDefault();
                            OracleParameter p1 = new OracleParameter("param1", username);

                            object[] parameters = new object[1] { p1 };
                            string sql = "UPDATE " + namatable + " SET LastLoginDate = sysdate, LastActivityDate = sysdate WHERE Username = :param1";

                            int rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                    }
                }
                else
                {
                    return tr;
                }*/
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }

            return tr;
        }

        #endregion


        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            // Bila skema training, passing passwordnya
            if (System.Web.Mvc.OtorisasiUser.NamaSkemaLogin == "surattrain")
            {
                return true;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

        public string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(encryptionKey);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private void UpdateFailureCount(string username, string failureType)
        {
            OracleConnection conn = new OracleConnection(connectionString);
            //OracleCommand cmd = new OracleCommand("SELECT FailedPwdAttemptCount, " +
            //                                  "  FailedPwdAttemptWindowStart, " +
            //                                  "  FailedPwdAnswerAttCount, " +
            //                                  "  FailedPwdAnswerAttWindowStart " +
            //                                  "  FROM " + tableName + " " +
            //                                  "  WHERE Username = :Username AND ApplicationName = :ApplicationName", conn);
            OracleCommand cmd = new OracleCommand("SELECT FailedPwdAttemptCount, " +
                                              "  FailedPwdAttemptWindowStart, " +
                                              "  FailedPwdAnswerAttCount, " +
                                              "  FailedPwdAnswerAttWindowStart " +
                                              "  FROM " + tableName + " " +
                                              "  WHERE Username = :Username", conn);

            cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255).Value = username;
            //cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = pApplicationName;

            OracleDataReader reader = null;
            DateTime windowStart = DateTime.Now;
            Decimal failureCount = 0;

            try
            {
                conn.Open();

                using (reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (failureType == "password")
                        {
                            failureCount = reader.GetDecimal(0);
                            if (!reader.IsDBNull(1))
                            {
                                windowStart = reader.GetDateTime(1);
                            }
                        }

                        if (failureType == "passwordAnswer")
                        {
                            failureCount = reader.GetDecimal(2);
                            if (!reader.IsDBNull(3))
                            {
                                windowStart = reader.GetDateTime(3);
                            }
                        }
                    }
                    reader.Close();
                }

                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.

                    if (failureType == "password")
                        //cmd.CommandText = "UPDATE " + tableName + " " +
                        //                  "  SET FailedPwdAttemptCount = :Count, " +
                        //                  "      FailedPwdAttemptWindowStart = :WindowStart " +
                        //                  "  WHERE Username = :Username AND ApplicationName = :ApplicationName";
                        cmd.CommandText = "UPDATE " + tableName + " " +
                                          "  SET FailedPwdAttemptCount = :Count, " +
                                          "      FailedPwdAttemptWindowStart = :WindowStart " +
                                          "  WHERE Username = :Username";

                    if (failureType == "passwordAnswer")
                        //cmd.CommandText = "UPDATE " + tableName + " " +
                        //                  "  SET FailedPwdAnswerAttCount = :Count, " +
                        //                  "      FailedPwdAnswerAttWindowStart = :WindowStart " +
                        //                  "  WHERE Username = :Username AND ApplicationName = :ApplicationName";
                        cmd.CommandText = "UPDATE " + tableName + " " +
                                          "  SET FailedPwdAnswerAttCount = :Count, " +
                                          "      FailedPwdAnswerAttWindowStart = :WindowStart " +
                                          "  WHERE Username = :Username";

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":Count", OracleDbType.Decimal).Value = failureCount + 1;
                    cmd.Parameters.Add(":WindowStart", OracleDbType.Date).Value = DateTime.Now;
                    cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255).Value = username;
                    //cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = pApplicationName;

                    if (cmd.ExecuteNonQuery() < 0)
                        throw new ProviderException("Unable to update failure count and window start.");
                }
                else
                {
                    if (failureCount++ >= MaxInvalidPasswordAttempts)
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.

                        //cmd.CommandText = "UPDATE " + tableName + " " +
                        //                  "  SET IsLockedOut = :IsLockedOut, LastLockedOutDate = :LastLockedOutDate " +
                        //                  "  WHERE Username = :Username AND ApplicationName = :ApplicationName";
                        cmd.CommandText = "UPDATE " + tableName + " " +
                                          "  SET IsLockedOut = :IsLockedOut, LastLockedOutDate = :LastLockedOutDate " +
                                          "  WHERE Username = :Username";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":IsLockedOut", OracleDbType.Int32).Value = true;
                        cmd.Parameters.Add(":LastLockedOutDate", OracleDbType.Date).Value = DateTime.Now;
                        cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255).Value = username;
                        //cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = pApplicationName;

                        if (cmd.ExecuteNonQuery() < 0)
                            throw new ProviderException("Unable to lock out user.");
                    }
                    else
                    {
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.

                        if (failureType == "password")
                            //cmd.CommandText = "UPDATE " + tableName + " " +
                            //                  "  SET FailedPwdAttemptCount = :Count" +
                            //                  "  WHERE Username = :Username AND ApplicationName = :ApplicationName";
                            cmd.CommandText = "UPDATE " + tableName + " " +
                                              "  SET FailedPwdAttemptCount = :Count" +
                                              "  WHERE Username = :Username";

                        if (failureType == "passwordAnswer")
                            //cmd.CommandText = "UPDATE " + tableName + " " +
                            //                  "  SET FailedPwdAnswerAttCount = :Count" +
                            //                  "  WHERE Username = :Username AND ApplicationName = :ApplicationName";
                            cmd.CommandText = "UPDATE " + tableName + " " +
                                              "  SET FailedPwdAnswerAttCount = :Count" +
                                              "  WHERE Username = :Username";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":Count", OracleDbType.Int32).Value = failureCount;
                        cmd.Parameters.Add(":Username", OracleDbType.Varchar2, 255).Value = username;
                        //cmd.Parameters.Add(":ApplicationName", OracleDbType.Varchar2, 255).Value = pApplicationName;

                        if (cmd.ExecuteNonQuery() < 0)
                            throw new ProviderException("Unable to update failure count.");
                    }
                }
            }
            catch (OracleException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    //WriteToEventLog(e, "UpdateFailureCount");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }
                conn.Close();
            }
        }

        private MembershipUser GetUserFromReader(OracleDataReader reader)
        {
            object providerUserKey = new Guid(reader.GetValue(0).ToString());
            string username = reader.IsDBNull(1) ? "" : reader.GetString(1);
            string email = reader.IsDBNull(2) ? "" : reader.GetString(2);
            string passwordQuestion = reader.IsDBNull(3) ? "" : reader.GetString(3);
            string commentar = reader.IsDBNull(4) ? "" : reader.GetString(4);
            bool isApproved = reader.IsDBNull(5) ? false : (reader.GetInt32(5) == 1 ? true : false);
            bool isLockedOut = reader.IsDBNull(6) ? false : (reader.GetInt32(6) == 1 ? true : false);
            DateTime creationDate = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7);
            DateTime lastLoginDate = reader.IsDBNull(8) ? DateTime.Now : reader.GetDateTime(8);
            DateTime lastActivityDate = reader.IsDBNull(9) ? DateTime.Now : reader.GetDateTime(9);
            DateTime lastPasswordChangedDate = reader.IsDBNull(10) ? DateTime.Now : reader.GetDateTime(10);
            DateTime lastLockedOutDate = reader.IsDBNull(11) ? DateTime.Now : reader.GetDateTime(11);

            MembershipUser u = new MembershipUser(this.Name,
                                                  username,
                                                  providerUserKey,
                                                  email,
                                                  passwordQuestion,
                                                  commentar,
                                                  isApproved,
                                                  isLockedOut,
                                                  creationDate,
                                                  lastLoginDate,
                                                  lastActivityDate,
                                                  lastPasswordChangedDate,
                                                  lastLockedOutDate);

            return u;
        }
    }
}