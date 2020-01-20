using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    public class ForecastsController : ApiController
    {
        string pythonScriptsPath;
        string pythonPath;
        PythonRunner python;

        public ForecastsController(){
            pythonScriptsPath = ConfigurationManager.AppSettings["pyScripts"];
            pythonPath = ConfigurationManager.AppSettings["pythonPath"];
            python = new PythonRunner(pythonPath, 20000);
        }

        [HttpGet]
        [Route("forecasts")]
        [ActionName("GetAllForecasts")]
        public IHttpActionResult GetAllForecasts()
        {
            pythonScriptsPath = System.IO.Path.GetFullPath(pythonScriptsPath);//todo:check se non va
            double[] results = new double[52];
            string cust, outputString;
            JArray jarray = new JArray();
            for (int i = 0; i < 52; i++)
            {
                cust = $"'cust{i + 1}'";
                try
                {
                    string pythonResult = python.getStrings(
                        pythonScriptsPath,
                        "arima_forecast.py",
                        pythonScriptsPath,
                        cust);

                    if (!string.IsNullOrWhiteSpace(pythonResult))
                    {
                        outputString = pythonResult.Substring(pythonResult.IndexOf("Actual"));
                        outputString = outputString.Remove(outputString.IndexOf("b'"));
                        JObject json = JObject.Parse(@"
                            {
                                ""customer"" : "",
                                ""forecasts"" : ""
                            }");
                        json["customer"] = cust;
                        json["forecasts"] = outputString;
                        jarray.Add(json);
                    }
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
            return Ok(jarray);
     
        }
        [HttpGet]
        [Route("customers/{id}/forecasts")]
        [ActionName("GetForecastsByCustomer")]
        public IHttpActionResult GetForecastsByCustomer(string id)
        {
            pythonScriptsPath = System.IO.Path.GetFullPath(pythonScriptsPath);//todo:check se non va
            try
            {
                return Content(HttpStatusCode.OK, getBitmapStringForSpecifiedCustomer(id));
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        private string getBitmapStringForSpecifiedCustomer(string customer)
        {         
            return python.getImage(pythonScriptsPath,
                                    "arima_forecast.py",
                                     pythonScriptsPath,
                                     $"'{customer}'");
        }

    }
}
