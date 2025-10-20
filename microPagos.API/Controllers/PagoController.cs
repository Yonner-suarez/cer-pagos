using microPagos.API.Logic;
using microPagos.API.Model;
using microPagos.API.Model.Request;
using microPagos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace microPagos.API.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PagoController : ControllerBase
    {
        private readonly BLPagos _blPagos;

        public PagoController(BLPagos blPagos)
        {
            _blPagos = blPagos;
        }
        [HttpPost]
        [Route("OrdenPagoWompi/{idPedido}")]
        public async Task<ActionResult> OrdenPagoWompi(List<OrdenPagoRequest> request, [Required] int idPedido)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
                return StatusCode(Variables.Response.Inautorizado, null);

            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (role != "Cliente")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse
                {
                    data = null,
                    status = Variables.Response.BadRequest,
                    message = "Solo los clientes pueden crear ordenes de pago"
                });
            }

            var idCliente = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var res =  _blPagos.GenerarOrdenPago(request, idPedido, idCliente, email);
            if (res.status != Variables.Response.OK)
            {
                return StatusCode(res.status, res);
            }

            return Ok(res);
        }


    }
}
