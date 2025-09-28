using microPagos.API.Logic;
using microPagos.API.Model.Request;
using microPagos.API.Model;
using microPagos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MercadoPago.Client;
using microPagos.API.Utils.ExternalAPI;

namespace microPagos.API.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotificacionController : ControllerBase
    {
        private readonly BLPagos _blPagos;

        public NotificacionController(BLPagos blPagos)
        {
            _blPagos = blPagos;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> NotificacionMP([FromBody] MercadoPagoNotification notification)
        {
            // 1. Obtener ID de la preferencia
            var idPedido = _blPagos.ParseIdPedido(notification.Data.Id.ToString());

            // 2. Consultar el estado real del pago en MercadoPago
            var status = await MercadoPagoClient.GetPaymentStatus(notification.Data.Id);

            if (status == "approved")
            {
                // Marcar pedido como pagado
                _blPagos.ActualizarEstadoPago(idPedido);
                return Ok();
            }
            else
            {
                return BadRequest(status);
            }

        }
    }
}
