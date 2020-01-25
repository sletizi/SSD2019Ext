using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Web.Http.Cors;
using SSD2019.Models;

namespace SSD2019.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api")]
    public class OrdersController : ApiController
    {
        private Persistence persistence;

        private string pythonPath;
        private string pythonScriptsPath;
        private string dbPath;
        private PythonRunner python;

        public OrdersController()
        {
            pythonScriptsPath = ConfigurationManager.AppSettings["projectPath"]+"\\python_scripts";
            pythonPath = ConfigurationManager.AppSettings["pythonPath"];
            python = new PythonRunner(pythonPath, 20000);
            dbPath = ConfigurationManager.AppSettings["projectPath"] + "\\App_Data\\ordiniMI2019.sqlite";
            persistence = Persistence.Instance;
        }

        [HttpGet]
        [Route("orders")]
        [ActionName("GetAllOrders")]
        public IHttpActionResult GetAllOrders()
        {
            try
            {
                List<Order> result = persistence.getOrders();
                return Ok(JArray.FromObject(result));
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("customers/{id}/orders")]
        [ActionName("GetOrdersByCustomer")]
        public IHttpActionResult GetOrdersByCustomer(string id)
        {
            try
            {
                List<Order> result = persistence.getCustomerOrders(id);
                return Ok(JArray.FromObject(result));

            }
            catch (Exception e)
            {
                return InternalServerError();
            }

        }

        [HttpPost]
        [Route("customers/{id}/orders")]
        [ActionName("AddOrderForCustomer")]
        public IHttpActionResult AddOrderForCustomer(string id, Order order)
        {
            try
            {
                Order result = persistence.addCustomerOrder(order);
                JObject response = new JObject();
                response.Add("insertedOrder", JObject.FromObject(result));
                return Ok(response.ToString());
            }
            catch (Exception e)
            {
                return InternalServerError();
            }

        }

        [HttpPut]
        [Route("customers/{id}/orders")]
        [ActionName("ResetCustomerOrdersQuant")]
        public IHttpActionResult ResetCustomerOrdersQuant(string id)
        {
            try
            {
                bool result = persistence.resetCustomerQuant(id);
                return result ? Content(HttpStatusCode.NoContent, string.Empty) :
                    Content(HttpStatusCode.NotFound, string.Empty);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }

        }

        [HttpDelete]
        [Route("customers/{id}/orders")]
        [ActionName("DeleteAllCustomerOrders")]
        public IHttpActionResult DeleteAllCustomerOrders(string id)
        {
            try
            {
                bool result = persistence.deleteCustomerOrders(id);
                return result ? Content(HttpStatusCode.OK, string.Empty) :
                    Content(HttpStatusCode.NotFound, string.Empty);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }


        [HttpGet]
        [Route("ordersChart")]
        [ActionName("GetAllOrdersChart")]
        public IHttpActionResult GetAllOrdersChart()
        {
            try
            {
                string customersString = getCustomersStringList(persistence.getCustomersList());
                string bitmapString = python.getImage(pythonScriptsPath,
                                    "chartOrders.py",
                                     pythonScriptsPath,
                                     customersString,
                                     dbPath);
                return Content(HttpStatusCode.OK, getBitmapStringFromPythonResult(bitmapString));
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }
        public string getBitmapStringFromPythonResult(string bitmap)
        {
            return bitmap.Substring(2, bitmap.Length - 3);
        }
        [HttpGet]
        [Route("customers")]
        [ActionName("GetAllCustomers")]
        public IHttpActionResult GetAllCustomers()
        {
            try
            {
                return Ok(JArray.FromObject(persistence.getCustomersList()));
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }
        private string getCustomersStringList(List<String> customerList)
        {
            return string.Join(",", customerList.Select(s => "'"+s+"'"));
        }

    }
}
