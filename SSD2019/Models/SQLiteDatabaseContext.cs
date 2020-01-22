using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;

namespace SSD2019.Models
{
    public class SQLiteDatabaseContext : DbContext
    {
        public SQLiteDatabaseContext(string databaseFile) :
            base(new SQLiteConnection
            {
                ConnectionString = new SQLiteConnectionStringBuilder
                {
                    DataSource = databaseFile,
                    ForeignKeys = true
                }
                        .ConnectionString
            },
                true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

    }
}