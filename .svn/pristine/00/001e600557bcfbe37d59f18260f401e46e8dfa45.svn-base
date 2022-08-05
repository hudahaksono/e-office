using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Surat.Models
{
    public class PostgresDbContext : DbContext
    {
        static PostgresDbContext()
        {
            Database.SetInitializer<PostgresDbContext>(null);
        }

        public PostgresDbContext()
            : base("PostgresConn")
        {
        }
    }
}