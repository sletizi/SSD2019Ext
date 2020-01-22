using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using SSD2019.Models;

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    public class ForecastsController : ApiController
    {
        string pythonScriptsPath;
        string pythonPath;
        private Persistence persistence = new Persistence();
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
            List<String> customersList = persistence.getCustomersList();
            pythonScriptsPath = System.IO.Path.GetFullPath(pythonScriptsPath);//todo:check se non va
            double[] results = new double[customersList.Count];
            int i = 0;
            string customer, outputString;
            JArray jarray = new JArray();
            foreach (string cust in customersList)
            {
                customer = $"'{cust}'";
                try
                {
                    string pythonResult = python.getStrings(
                        pythonScriptsPath,
                        "arimaForecast.py",
                        pythonScriptsPath,
                        customer);

                    if (!string.IsNullOrWhiteSpace(pythonResult))
                    {
                        outputString = pythonResult.Substring(pythonResult.IndexOf("Actual"));
                        outputString = outputString.Remove(outputString.IndexOf("b'"));
                        JObject json = JObject.Parse(@"
                            {
                                'customer' : '',
                                'forecasts' : ''
                            }");
                        json["customer"] = customer;
                        json["forecasts"] = outputString;
                        jarray.Add(json);
                        results[i] = PrepareFileLoading(outputString);
                        i++;
                    }


                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
            File.WriteAllLines("C:\\Users\\Mark Studio\\Desktop\\Universita\\Magistrale\\SsD\\estensioneProgetto\\SSD2019\\SSD2019\\previsions\\GAPreq.dat", results.Select(x => x.ToString()));
            return Ok(jarray);
     
        }

        private double PrepareFileLoading(string outputString)
        {
            
            string[] lastPrevLine = outputString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string lastPrev = lastPrevLine[2].Substring(lastPrevLine[2].IndexOf("forecast"))
                .Split(new[] { "forecast " }, StringSplitOptions.None)[1];
            return Double.Parse(lastPrev.Replace('.', ','));//TODO check se era qui l'errore che non modificava results
        }

        [HttpGet]
        [Route("customers/{id}/forecasts")]
        [ActionName("GetForecastsByCustomer")]
        public IHttpActionResult GetForecastsByCustomer(string id)
        {
            pythonScriptsPath = System.IO.Path.GetFullPath(pythonScriptsPath);//todo:check se non va
            try
            {
                if(!persistence.getCustomersList().Contains(id))
                {
                    return Content(HttpStatusCode.NotFound, string.Empty);
                }
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
                                    "arimaForecast.py",
                                     pythonScriptsPath,
                                     $"'{customer}'");
        }

    }
}
