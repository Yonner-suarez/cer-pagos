using microPagos.API.Dao;
using microPagos.API.Logic;
using microPagos.API.Model;
using microPagos.API.Model.Request;
using microPagos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace microPagos.API.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PagoController: ControllerBase
    {
        private readonly BLPagos _blPagos;

        public PagoController(BLPagos blPagos)
        {
            _blPagos = blPagos;
        }

        [HttpPost]
        [Route("[action]")]
        public ActionResult ordenCompra(OrdenPagoRequest request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


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


            GeneralResponse res = _blPagos.GenerarOrdenPago(request, idCliente, email);
            if (res.status == Variables.Response.OK)
            {
                return Ok(res);
            }
            else
            {
                return StatusCode(res.status, res);
            }
        }
        [HttpGet]
        [Route("[action]/{idPedido}")]
        public async Task<IActionResult> ValidarPago(int idPedido)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Cliente")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse
                {
                    data = null,
                    status = Variables.Response.BadRequest,
                    message = "Solo los clientes pueden VALIDAR sus pagos"
                });
            }
            GeneralResponse res = await _blPagos.ObtenerPedido(idPedido);

            if (res.data == null || res.data is not true)
            {
                return StatusCode(res.status, res);
            }

            return Ok(res);
        }
    }
}
