using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.IO;
using SSD2019.Models;


//TODO check ip in scripts if we can set 127.0.0.1
//TODO check optimization results
//TODO check if it works on GitHUB Pages

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    public class ForecastsController : ApiController
    {
        string pythonScriptsPath;
        string pythonPath;
        string pklDirectory, dbPath, gapReqPath;
        private Persistence persistence;

        PythonRunner python;

        public ForecastsController(){
            pythonScriptsPath = ConfigurationManager.AppSettings["projectPath"]+"\\python_scripts";
            pythonPath = ConfigurationManager.AppSettings["pythonPath"];
            python = new PythonRunner(pythonPath, 20000);
            pklDirectory = ConfigurationManager.AppSettings["projectPath"]+"\\previsions";
            dbPath = ConfigurationManager.AppSettings["projectPath"] + "\\App_Data\\ordiniMI2019.sqlite";
            gapReqPath = ConfigurationManager.AppSettings["projectPath"] + "\\previsions\\GAPreq.dat";
            persistence = Persistence.Instance;
        }

        [HttpGet]
        [Route("forecasts")]
        [ActionName("GetAllForecasts")]
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
        public IHttpActionResult GetAllForecasts()
        {
            List<String> customersList = persistence.getCustomersList();
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
                        customer,
                        pklDirectory,
                        dbPath);

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
            
            File.WriteAllLines(gapReqPath, results.Select(x => (int)Math.Round(x)).Select(x => x.ToString()));
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
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
        [ActionName("GetForecastsByCustomer")]
        public IHttpActionResult GetForecastsByCustomer(string id)
        {
            
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
            string pythonRes = python.getImage(pythonScriptsPath,
                                    "arimaForecast.py",
                                     pythonScriptsPath,
                                     $"'{customer}'",
                                     pklDirectory,
                                     dbPath);
            return pythonRes.Substring(2, pythonRes.Length - 3);
        }

    }
}
