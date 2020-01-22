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
    [RoutePrefix("api")]
    public class OrdersController : ApiController
    {
        private Persistence persistence = new Persistence();

        string pythonPath;
        string pythonScriptsPath;

        PythonRunner python;

        public OrdersController()
        {
            pythonScriptsPath = ConfigurationManager.AppSettings["pyScripts"];
            pythonPath = ConfigurationManager.AppSettings["pythonPath"];
            python = new PythonRunner(pythonPath, 20000);
        }

        [HttpGet]
        [EnableCors(origins: "https://maluffa.github.io", headers:"*", methods:"*")]
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
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
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
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
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
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
        public IHttpActionResult ResetCustomerOrdersQuant(string id)
        {
            try
            {
                bool result = persistence.resetCustomerQuant(id);
                JObject response = new JObject();
                return result ? Content(HttpStatusCode.NoContent, string.Empty) :
                    Content(HttpStatusCode.NotFound, string.Empty);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
           
        }

        [HttpDelete ]
        [Route("customers/{id}/orders")]
        [ActionName("DeleteAllCustomerOrders")]
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
        public IHttpActionResult DeleteAllCustomerOrders(string id)
        {
            try
            {
                bool result = persistence.deleteCustomerOrders(id);
                JObject response = new JObject();
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
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
        public IHttpActionResult GetAllOrdersChart()
        {
            pythonScriptsPath = System.IO.Path.GetFullPath(pythonScriptsPath);
            try
            {
                string customersString = getCustomersStringList(persistence.getCustomersList());
                string bitmapString=  python.getImage(pythonScriptsPath,
                                    "chartOrders.py",
                                     pythonScriptsPath,

                                     customersString);
                return Content(HttpStatusCode.OK, bitmapString);
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
