using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    public class ForecastsController : ApiController
    {
        [HttpGet]
        [Route("forecasts")]
        [ActionName("GetAllForecasts")]
        public IHttpActionResult GetAllForecasts()
        {
            return Ok("ok");
        }
        [HttpGet]
        [Route("customers/{id}/forecasts")]
        [ActionName("GetForecastsByCustomer")]
        public IHttpActionResult GetForecastsByCustomer()
        {
            return Ok("ok");
        }
    }
}
