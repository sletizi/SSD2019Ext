using System.Collections.Generic;
using System;
using System.Linq;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Configuration;

namespace SSD2019.Models
{
    public class LocalPersistence
    {
        private string connectionString;
        private string factory;
        private string dbpath;
        private DbProviderFactory dbFactory;

        public LocalPersistence()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SQLiteConn"].ConnectionString;
            factory = ConfigurationManager.ConnectionStrings["SQLiteConn"].ProviderName;  
            dbpath = ConfigurationManager.AppSettings["projectPath"]+"\\SQLite\\ordiniMI2019.sqlite";
            dbFactory = DbProviderFactories.GetFactory(factory);
        }
        public void ReadGAPinstance(GAPclass G)
        {
            int i, j;
            List<int> lstCap;
            List<double> lstCosts;
            

            using (var ctx = new SQLiteDatabaseContext(dbpath))
            {
                lstCap = getListFromQuery("SELECT cap from capacita", "cap").Select(d => Convert.ToInt32(d)).ToList();
                G.m = lstCap.Count();
                G.cap = new int[G.m];
                for (i = 0; i < G.m; i++)
                    G.cap[i] = lstCap[i];

                lstCosts = getListFromQuery("SELECT cost from costi", "cost").ToList();
                G.n = lstCosts.Count / G.m;
                G.c = new double[G.m, G.n];
                G.req = new int[G.n];
                G.sol = new int[G.n];
                G.solbest = new int[G.n];
                G.zub = Double.MaxValue;
                G.zlb = Double.MinValue;

                for (i = 0; i < G.m; i++)
                    for (j = 0; j < G.n; j++)
                        G.c[i, j] = lstCosts[i * G.n + j];

                for (j = 0; j < G.n; j++)
                    G.req[j] = -1;          // placeholder
            }
           

        }
        private List<double> getListFromQuery (string query, string selectField)
        {
            List<double> result = new List<double>();
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();
                    DbCommand com = conn.CreateCommand();
                    com.CommandText = query;
                    DbDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(Convert.ToDouble(reader[selectField]));
                    }

                    reader.Close();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    return new List<double>();
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return result;
            }
        }
    }

}