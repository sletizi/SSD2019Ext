using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace SSD2019.Models
{
    public class Persistence
    {
        private DbProviderFactory dbFactory;
        private string dbPath = ConfigurationManager.AppSettings["projectPath"] + "\\App_Data\\ordiniMI2019.sqlite";
        private string connectionString = ConfigurationManager.ConnectionStrings["SQLiteConn"].ConnectionString;
        private string factory = ConfigurationManager.ConnectionStrings["SQLiteConn"].ProviderName;
        private static Persistence instance = null;

        private Persistence()
        {

            this.connectionString = this.connectionString.Replace("DBFILE", dbPath);
            this.dbFactory = DbProviderFactories.GetFactory(factory);

        }
        public static Persistence Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Persistence();
                }
                return instance;
            }
        }
        public List<Order> getOrders()
        {
            List<Order> lstOrders = new List<Order>();
            using (DbConnection conn = dbFactory.CreateConnection())       
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                IDbCommand com = conn.CreateCommand();
                string queryText = "select id, customer, time, quant from ordini";
                com.CommandText = queryText;
                using (IDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstOrders.Add(new Order(Convert.ToInt64(reader["id"]), Convert.ToString(reader["customer"]), Convert.ToInt32(reader["time"]), Convert.ToInt32(reader["quant"])));
                    }
                }
                
            }
            return lstOrders.Where(order => order.customer != "default").ToList(); ;
        }

       
        public List<Order> getCustomerOrders(string custId)
        {
            List<Order> lstOrders = new List<Order>();
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                string queryText = "select id, customer, time, quant from ordini where customer=@custId";
                IDbCommand com = PrepareParametrizedStringQuery(conn, queryText, DbType.String, custId);
                using (IDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstOrders.Add(new Order(Convert.ToInt64(reader["id"]), Convert.ToString(reader["customer"]), Convert.ToInt32(reader["time"]), Convert.ToInt32(reader["quant"])));
                    }
                }
            } 
            return lstOrders;
        }

        public Order addCustomerOrder(Order order)
        {
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                IDbCommand com = conn.CreateCommand();
                string queryText = "insert into ordini ('id', 'customer', 'time', 'quant') values ('" + order.id + "','" + order.customer + "','" + order.time + "','" + order.quant + "')";
                com.CommandText = queryText;
                if (com.ExecuteNonQuery() != -1)
                {
                    return order;
                }
                else
                {
                    throw new Exception("inserimento non riuscito");
                }    
               
            }
        }

        public bool resetCustomerQuant(string custId)
        {
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                string queryText = "update ordini set quant=0 where customer=@custId";
                IDbCommand com = PrepareParametrizedStringQuery(conn, queryText, DbType.String, custId);
                return com.ExecuteNonQuery() > 0 ? true : false;
            }
        }

        public bool deleteCustomerOrders(string custId)
        {
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                string queryText = "delete from ordini where customer=@custId";
                IDbCommand com = PrepareParametrizedStringQuery(conn, queryText, DbType.String, custId);
                return com.ExecuteNonQuery() > 0 ? true : false;
            }
        }

        public List<String> getCustomersList()
        {
            List<String> lstCustomer = new List<String>();
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                IDbCommand com = conn.CreateCommand();
                string queryText = "select distinct customer from ordini";
                com.CommandText = queryText;
                using (IDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstCustomer.Add(Convert.ToString(reader["customer"]));
                    }
                }
            }
            return lstCustomer.Where(cust => cust != "default").ToList();
        }



        private IDbCommand PrepareParametrizedStringQuery(IDbConnection conn, string queryText, DbType paramType, string paramValue)
        {
            IDbCommand com = conn.CreateCommand();
            com.CommandText = queryText;
            addParamToQuery(com, paramType, paramValue);
            return com;
        }

        private void addParamToQuery(IDbCommand com, DbType paramType, string paramValue)
        {
            if (com.CommandText.Contains('@'))
            {
                IDbDataParameter param = com.CreateParameter();
                param.DbType = DbType.String;
                string[] query = com.CommandText.Split('@');
                param.ParameterName = "@" + com.CommandText.Split('@')[1];
                param.Value = paramValue;
                com.Parameters.Add(param);
            }
        }


        public void ReadGAPinstance(GAPclass G)
        {
            int i, j;
            List<int> lstCap;
            List<double> lstCosts;
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
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
        private List<double> getListFromQuery(string query, string selectField)
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