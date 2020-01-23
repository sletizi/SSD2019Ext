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


//TODO change connection from remote to local. also change in python scripts
//TODO check ip in scripts if we can set 127.0.0.1
//TODO change policy to save results in gapReq.dat ?, anyway save the path in web.config
//TODO cancel json representation
//TODO add css flex to graphics

namespace SSD2019.Controllers
{
    [RoutePrefix("api")]
    public class ForecastsController : ApiController
    {
        string pythonScriptsPath;
        string pythonPath;
        private Persistence persistence;
        string pklDirectory;
        string dbPath;
        string connectionString;
        string factory;
        PythonRunner python;

        public ForecastsController(){
            pythonScriptsPath = ConfigurationManager.AppSettings["projectPath"]+"\\python_scripts";
            pythonPath = ConfigurationManager.AppSettings["pythonPath"];
            python = new PythonRunner(pythonPath, 20000);
            pklDirectory = ConfigurationManager.AppSettings["projectPath"]+"\\previsions";
            dbPath = ConfigurationManager.AppSettings["projectPath"] + "\\SQLite\\ordiniMI2019";
            connectionString = ConfigurationManager.ConnectionStrings["SQLiteConn"].ConnectionString;
            connectionString = connectionString.Replace("DBFILE", dbPath);
            factory = ConfigurationManager.ConnectionStrings["SQLiteConn"].ProviderName;
            persistence = new Persistence(connectionString, factory, dbPath);
        }

        [HttpGet]
        [Route("forecasts")]
        [ActionName("GetAllForecasts")]
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
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
        [EnableCors(origins: "https://maluffa.github.io", headers: "*", methods: "*")]
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
