using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using SSD2019.Models;
using System.Web.Http.Cors;

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OptimizationsController : ApiController
    {
        private GAPclass GAP = new GAPclass();
        private Persistence persistence = Persistence.Instance;
        private string gapReqPath = ConfigurationManager.AppSettings["projectPath"] + "\\previsions\\GAPreq.dat";
        private ForecastsController forecastController = new ForecastsController();
        [HttpGet]
        [Route("simpleConstruct")]
        [ActionName("SimpleConstruct")]
        public IHttpActionResult SimpleConstruct()
        {

            checkPrevisionFile();
            double result = GAP.simpleConstruct();
            return Ok(result);
        }

        [HttpGet]
        [Route("opt10")]
        [ActionName("Opt10")]
        public IHttpActionResult Opt10()
        {
            checkPrevisionFile();
            double result = GAP.opt10(GAP.c);
            return Ok(result);
        }

        [HttpGet]
        [Route("tabuSearch")]
        [ActionName("tabuSearch")]
        public IHttpActionResult TabuSearch()
        {
            checkPrevisionFile();
            double result = GAP.tabuSearch(30,1000);
            return Ok(result);
        }

        private void checkPrevisionFile()
        {
            persistence.ReadGAPinstance(GAP);
            if (!File.Exists(gapReqPath))
            {
                forecastController.GetAllForecasts();
            }
            string[] txtData = File.ReadAllLines(gapReqPath);
            GAP.req = Array.ConvertAll<string, int>(txtData, new Converter<string, int>(i => int.Parse(i)));
        }

    }
}
