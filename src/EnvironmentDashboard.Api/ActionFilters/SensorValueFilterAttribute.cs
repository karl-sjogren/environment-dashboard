using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace EnvironmentDashboard.Api.ActionFilters {
    public class SensorValueFilter : ActionFilterAttribute {
        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var parameters = ((Controller)context.Controller).ControllerContext.ActionDescriptor.Parameters;
            var sensorParam = parameters.FirstOrDefault(p => p.ParameterType == typeof(string));
            var sensorValueParam = parameters.FirstOrDefault(p => p.ParameterType == typeof(SensorValue));

            var sensorId = context.ActionArguments[sensorParam.Name] as string;

            var sensorStore = (ISensorStore)context.HttpContext.RequestServices.GetService(typeof(ISensorStore));
            var sensor = await sensorStore.GetById(sensorId);

            switch(sensor.Type) {
                case "temperature":
                    sensorValueParam.ParameterType = typeof(SensorTemperatureValue);
                    break;
                default:
                    context.Result = new BadRequestObjectResult("Unknown sensor type: " + sensor.Type);
                    return;
            }

            // TODO Investigate if we can trigger the original modelbinding here
            // somehow, it would be way nicer.
            string json;
            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using(var sr = new StreamReader(context.HttpContext.Request.Body))
                json = await sr.ReadToEndAsync();

            var model = JsonConvert.DeserializeObject(json, sensorValueParam.ParameterType);
            context.ActionArguments[sensorValueParam.Name] = model;

            await next();
        }
    }
}