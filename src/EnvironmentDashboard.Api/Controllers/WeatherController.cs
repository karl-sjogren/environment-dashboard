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
using EnvironmentDashboard.Api.Extensions;
using System.Linq;

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

            var doc = XDocument.Parse(xml);

            var model = new ForecastResponse {
                Location = doc.Descendants("name").FirstValueOrDefault(),
                Country = doc.Descendants("country").FirstValueOrDefault(),
                ForecastUrl = doc.Descendants("link").Attributes("url").FirstValueOrDefault(),

                Sunrise = doc.Descendants("sun").Attributes("rise").FirstValueOrDefault(),
                Sunset = doc.Descendants("sun").Attributes("set").FirstValueOrDefault()
            };

            model.Timeperiods.AddRange(doc.Descendants("time").Select(x => {
                var periodValue = x.Attribute("period").ValueOrDefault();
                var periodString = "Night";
                switch(periodValue) {
                    case "1":
                        periodString = "Morning";
                        break;
                    case "2":
                        periodString = "Day";
                        break;
                    case "3":
                        periodString = "Evening";
                        break;
                }
                
                var timeperiod = new ForecastTimeperiod {
                    From = x.Attribute("from").ValueOrDefault(),
                    To = x.Attribute("to").ValueOrDefault(),
                    Period = periodString,
                    YrIcon = x.Descendants("symbol").Attributes("var").FirstValueOrDefault(),
                    Precipitation = x.Descendants("precipitation").Attributes("value").FirstValueOrDefault().ToDouble(),
                    Temperature = x.Descendants("temperature").Attributes("value").FirstValueOrDefault().ToDouble(),
                    WindSpeed = x.Descendants("windSpeed").Attributes("mps").FirstValueOrDefault().ToDouble(),
                    WindDirectionDegrees = x.Descendants("windDirection").Attributes("deg").FirstValueOrDefault().ToDouble(),
                    WindDirectionCode = x.Descendants("windDirection").Attributes("code").FirstValueOrDefault()
                };

                return timeperiod;
            }));

            return Json(model);
        }

        #region Response models

        public class ForecastResponse {
            public ForecastResponse() {
                Timeperiods = new List<ForecastTimeperiod>();
            }

            public string Location { get; set; }
            public string Country { get; set; }
            public string ForecastUrl { get; set; }

            public string Sunrise { get; set; }
            public string Sunset { get; set; }

            public List<ForecastTimeperiod> Timeperiods { get; private set; }
        }

        public class ForecastTimeperiod {
            public string From { get; set; }
            public string To { get; set; }
            public string Period { get; set; }
            public string YrIcon { get; set; }
            public double Precipitation { get; set; }
            public double Temperature { get; set; }
            public double WindSpeed { get; set; }
            public double WindDirectionDegrees { get; set; }
            public string WindDirectionCode { get; set; }
        }

        #endregion
    }
}