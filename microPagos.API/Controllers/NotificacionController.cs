using microPagos.API.Logic;
using microPagos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace microPagos.API.Controllers
{
    [AllowAnonymous]
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
        [Route("Eventos")]
        public async Task<IActionResult> Eventos([FromBody] WompiEventoRequest evento)
        {
            try
            {
                // 1️⃣ Extraer la información principal
                var eventType = evento.@event;
                var environment = evento.environment;
                var transaction = evento.data.transaction;
                var signature = evento.signature;
                var timestamp = evento.timestamp;

                string transactionId = transaction.id;
                string status = transaction.status;
                int amount = transaction.amount_in_cents;
                string reference = transaction.reference;

                // 2️⃣ Concatenar los valores indicados en "properties"
                string concatenado = $"{transaction.id}{transaction.status}{transaction.amount_in_cents}{timestamp}{Variables.Wompi.eventos}";
                using var sha = System.Security.Cryptography.SHA256.Create();
                var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(concatenado));
                var calculado = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();


                // 4️⃣ Validar que la firma coincida
                if (signature.checksum.ToLowerInvariant() != calculado)
                {
                    return BadRequest(new { error = "Firma inválida" });
                }
                string idPedido = reference.Replace("PEDIDO_", ""); // "13"

                if(status== "APPROVED")
                {
                    await _blPagos.ActualizarEstadoPago(int.Parse(idPedido));
                }

                return Ok(new
                {
                    received = true,
                    message = "Evento procesado correctamente",
                    reference,
                    status
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error manejando evento: {ex.Message}");
                return StatusCode(Variables.Response.ERROR, new { error = "Error procesando evento", detail = ex.Message });
            }
        }

    }

}
