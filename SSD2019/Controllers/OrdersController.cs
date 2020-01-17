using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using SSD2019.Models;

namespace SSD2019.Controllers
{ 
    [RoutePrefix("api")]
    public class OrdersController : ApiController
    {
        private Persistence persistence = new Persistence();
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
    }
}
