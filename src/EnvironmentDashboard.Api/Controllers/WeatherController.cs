using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.IdGenerators;
using EnvironmentDashboard.Api.Options;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Xml;

namespace EnvironmentDashboard.Api.Controllers {
    [Authorize(Policy = "AdminUser")]
    [Route("admin/api/weather")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WeatherController : Controller {
        
        private readonly YrOptions _yrOptions;
        private readonly HttpClient _httpClient;
        
        public WeatherController(IOptions<YrOptions> yrOptionsAccessor) {
            _yrOptions = yrOptionsAccessor.Value;
            _httpClient = new HttpClient();
        }

        [HttpGet("forecast")]
        public async Task<IActionResult> GetCurrentForecast() {
            var uri = new Uri(new Uri("https://www.yr.no/place/"), _yrOptions.Path + "/forecast.xml");
            var xml = await _httpClient.GetStringAsync(uri);

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var json = JsonConvert.SerializeXmlNode(doc);
            return Content(json, "application/json");
        }

        #region Response models

        public class ForecastResponse {
            public ForecastResponse() {
                Timeperiods = new List<ForecastTimeperiod>();
            }

            public string Location { get; set; }
            public string Country { get; set; }
            public string ForecastUrl { get; set; }

            public DateTime Sunrise { get; set; }
            public DateTime Sunset { get; set; }

            public List<ForecastTimeperiod> Timeperiods { get; private set; }
        }

        public class ForecastTimeperiod {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public string YrIcon { get; set; }
            public double Precipitation { get; set; }
            public double Temperature { get; set; }
            public double WindSpeed { get; set; }
            public double WindDirectionDegrees { get; set; }
            public string WindDirectionCode { get; set; }
/*
<time from="2018-07-19T12:00:00" to="2018-07-19T18:00:00" period="2">
 <!--
 Valid from 2018-07-19T12:00:00 to 2018-07-19T18:00:00 
-->
<symbol number="2" numberEx="2" name="Lettskyet" var="02d"/>
<precipitation value="0"/>
 <!--  Valid at 2018-07-19T12:00:00  -->
<windDirection deg="109.5" code="ESE" name="Øst-sørøst"/>
<windSpeed mps="2.3" name="Svak vind"/>
<temperature unit="celsius" value="23"/>
<pressure unit="hPa" value="1015.7"/>
</time>

 */
        }

        #endregion
    }
}