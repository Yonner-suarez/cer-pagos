using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace microPagos.API.Utils.ActionResults
{
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object error)
            : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
            //Value = error.ToString();
        }
    }
}
