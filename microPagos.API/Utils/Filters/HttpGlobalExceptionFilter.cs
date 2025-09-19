using microPagos.API.Model;
using microPagos.API.Utils.ActionResults;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace microPagos.API.Utils.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<HttpGlobalExceptionFilter> logger;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            var messageGlobal = context.Exception.Message;

            if (env.EnvironmentName != "Development")
            {
                messageGlobal = "No se ha podido procesar su solicitud.";
            }

            var json = new GeneralResponse
            {
                message = messageGlobal,
                status = StatusCodes.Status500InternalServerError,
                data = null
            };

            context.Result = new InternalServerErrorObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            context.ExceptionHandled = true;
            //Helper.logSentryIO(context.Exception);
        }
    }
}
