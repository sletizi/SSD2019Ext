using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    public class OptimizationsController : ApiController
    {
        [HttpGet]
        [Route("simpleConstruct")]
        [ActionName("SimpleConstruct")]
        public IHttpActionResult SimpleConstruct()
        {
            return Ok("ok");
        }
        [HttpGet]
        [Route("opt10")]
        [ActionName("Opt10")]
        public IHttpActionResult Opt10()
        {
            return Ok("ok");
        }
        [HttpGet]
        [Route("tabuSearch")]
        [ActionName("tabuSearch")]
        public IHttpActionResult TabuSearch()
        {
            return Ok("ok");
        }

    }
}
