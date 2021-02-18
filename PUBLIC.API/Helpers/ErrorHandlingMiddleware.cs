using MassTransit.Dispatch.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MQDispatch.Client;
using Newtonsoft.Json;
using Platform.Framework;
using System;
using System.Net;
using System.Threading.Tasks;
using static PUBLIC.SERVICE.LIB.Helpers.MQErrors;

namespace PUBLIC.API.Helpers
{
    internal class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static ILogger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, DispatchClient dispatchClient)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, dispatchClient);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, DispatchClient dispatchClient)
        {
            if (!(exception is CecurityException cecurityException))
            {
                cecurityException = new CecurityException((int)MQMessages.APP_ERR_UNHANDLED, exception.Message, exception.InnerException);

                try
                {
                    await dispatchClient.SendApplicationErrorMessageAsync(cecurityException.Code(), cecurityException);
                }
                catch (Exception) { }
            }

            _logger.LogError(cecurityException.InnerException, cecurityException.Message);

            var result = JsonConvert.SerializeObject(new
            {
                error = cecurityException.Message,
                code = cecurityException.Code(),
                additionalInfo = cecurityException.AdditionalInfo()
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(result);
        }
    }
}