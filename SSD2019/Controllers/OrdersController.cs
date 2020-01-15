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
                JObject response = new JObject();
                response.Add("orders", JArray.FromObject(result));
                return Ok(response.ToString());
            }
            catch (Exception e)
            {
                if(e is DBErrorException)
                {
                    return InternalServerError();
                } 
            }
            return InternalServerError();
        }

        [HttpGet]
        [Route("customers/{id:int}/orders")]
        [ActionName("GetOrdersByCustomer")]
        public IHttpActionResult GetOrdersByCustomer(int id)
        {
            List<Order> result = persistence.getCustomerOrders(id);
            JObject response = new JObject();
            response.Add("customerOrders", JArray.FromObject(result));
            return Ok(response.ToString());
        }

        [HttpPost]
        [Route("customers/{id:int}/orders")]
        [ActionName("AddOrderForCustomer")]
        public IHttpActionResult AddOrderForCustomer(int id, Order order)
        {
            Order result = persistence.addCustomerOrder(order);
            JObject response = new JObject();
            response.Add("insertedOrder", JObject.FromObject(result));
            return Ok(response.ToString());
        }

        [HttpPut]
        [Route("customers/{id:int}/orders")]
        [ActionName("ResetCustomerOrdersQuant")]
        public IHttpActionResult ResetCustomerOrdersQuant(int id)
        {
            bool result = persistence.resetCustomerQuant(id);
            JObject response = new JObject();
            
            return result ? Content(HttpStatusCode.NoContent, string.Empty) :
                Content(HttpStatusCode.NotFound, string.Empty);
        }

        [HttpDelete ]
        [Route("customers/{id:int}/orders")]
        [ActionName("ResetCustomerOrdersQuant")]
        public IHttpActionResult DeleteAllCustomerOrders(int id)
        {
            bool result = persistence.deleteCustomerOrders(id);
            JObject response = new JObject();
            return result ? Content(HttpStatusCode.NoContent, string.Empty) :
                Content(HttpStatusCode.NotFound, string.Empty);
        }
    }
}
