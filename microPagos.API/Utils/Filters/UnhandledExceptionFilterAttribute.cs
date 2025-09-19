using microPagos.API.Model;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace microPagos.API.Utils.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]

    public class UnhandledExceptionFilterAttribute : ActionFilterAttribute
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<HttpGlobalExceptionFilter> logger;

        public UnhandledExceptionFilterAttribute(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(v => v.Errors.Count > 0)
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage)
                        .ToList();

                var messageGlobal = errors.Find(x => !String.IsNullOrEmpty(x));

                logger.LogError(errors.ToString());

                var json = new GeneralResponse
                {
                    message = messageGlobal,
                    status = StatusCodes.Status400BadRequest,
                    data = null
                };

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(json)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                //Helper.logSentryIO(new Exception(errors.ToString()));
            }
        }
    }
}
