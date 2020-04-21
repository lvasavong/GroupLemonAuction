using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LemonAuction.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;


namespace LemonAuction.Middleware {
    public class ExceptionMiddelwareAttribute : ExceptionFilterAttribute
    {

        public HttpStatusCode ExceptionStatusCode(Exception e) {
            switch (e) {
                case LemonInvalidException _:
                    return HttpStatusCode.BadRequest;
                case LemonNotFoundException _:
                    return HttpStatusCode.NotFound;
                default:
                    return HttpStatusCode.InternalServerError;
            }
        }
        // public override void OnException(ExceptionContext context) {

        // }

        public override void OnException(ExceptionContext context) {
            var exception = context.Exception;

            var code = ExceptionStatusCode(exception);
            // var result = JsonConvert.SerializeObject(new { error = exception.Message });
            // context.HttpContext.Response.StatusCode = (int)code;
            var resultObject = new  ObjectResult(new { error = exception.Message });
            resultObject.ContentTypes = new MediaTypeCollection {"application/json"};
            resultObject.StatusCode = (int)code;
            context.Result = resultObject;
        }
    }
}