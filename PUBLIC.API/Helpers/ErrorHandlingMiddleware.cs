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

        public async Task Invoke(HttpContext context, IRabbitMqPersistentConnection rabbitMqPersistentConnection)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, rabbitMqPersistentConnection);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IRabbitMqPersistentConnection rabbitMqPersistentConnection)
        {
            if (!(exception is CecurityException cecurityException))
            {
                cecurityException = new CecurityException((int)MQMessages.APP_ERR_UNHANDLED, exception.Message, exception.InnerException);

                rabbitMqPersistentConnection.SendApplicationErrorMessage(cecurityException.Code(), cecurityException);
            }

            _logger.LogError(cecurityException.Code(), cecurityException.Message, cecurityException.InnerException);

            var result = JsonConvert.SerializeObject(new
            {
                error = cecurityException.Message,
                code = cecurityException.Code(),
                additionalInfo = cecurityException.AdditionalInfo()
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return context.Response.WriteAsync(result);
        }
    }
}