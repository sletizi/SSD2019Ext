using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SSD2019.Models
{
    public class Persistence
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["RemoteSQLConn"].ConnectionString;
        private string factory = ConfigurationManager.ConnectionStrings["RemoteSQLConn"].ProviderName;

        public List<Order> getOrders()
        {
            List<Order> lstOrders = new List<Order>();
            IDbConnection conn = new SqlConnection(connectionString);
            using (conn)
            {
             
                conn.Open();
                IDbCommand com = conn.CreateCommand();
                string queryText = "select id, customer, time, quant from ordini" /*order by quant"*/;
                com.CommandText = queryText;
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

       
        public List<Order> getCustomerOrders(string custId)
        {
            List<Order> lstOrders = new List<Order>();
            IDbConnection conn = new SqlConnection(connectionString);
            using (conn)
            {
                conn.Open();
                string queryText = "select id, customer, time, quant from ordini where customer=@custId" /*order by quant"*/;
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
            IDbConnection conn = new SqlConnection(connectionString);
            using (conn)
            {
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
            IDbConnection conn = new SqlConnection(connectionString);
            using (conn)
            {
                conn.Open();
                string queryText = "update ordini set quant=0 where customer=@custId";
                IDbCommand com = PrepareParametrizedStringQuery(conn, queryText, DbType.String, custId);
                return com.ExecuteNonQuery() > 0 ? true : false;
            }
        }

        public bool deleteCustomerOrders(string custId)
        {
            IDbConnection conn = new SqlConnection(connectionString);
            using (conn)
            {
                conn.Open();
                string queryText = "delete from ordini where customer=@custId";
                IDbCommand com = PrepareParametrizedStringQuery(conn, queryText, DbType.String, custId);
                return com.ExecuteNonQuery() > 0 ? true : false;
            }
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

    }



}