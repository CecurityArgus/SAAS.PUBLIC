using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PUBLIC.CONTROLLER.Helpers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PUBLIC.API.Helpers
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            /* 
                    if      (exception is MyNotFoundException)     code = HttpStatusCode.NotFound;
                    else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
                    else if (exception is MyException)             code = HttpStatusCode.BadRequest;
            */

            string result;
            if (exception is CecurityException cecurityException)
                result = JsonConvert.SerializeObject(new CecurityError() { Message = cecurityException.Message, Code = cecurityException.Code, AdditionalInfo = cecurityException.AdditionalInfo });
            else
                result = JsonConvert.SerializeObject(new CecurityError() { Message = exception.Message, Code = "PUBLIC_API_99999" });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}