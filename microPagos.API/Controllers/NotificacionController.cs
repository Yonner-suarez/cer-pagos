using microPagos.API.Logic;
using microPagos.API.Model.Request;
using microPagos.API.Model;
using microPagos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MercadoPago.Client;
using microPagos.API.Utils.ExternalAPI;
using System.Text.Json;

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
                string concatenado = "";

                foreach (var prop in signature.properties)
                {
                    if (prop.StartsWith("transaction."))
                    {
                        var field = prop.Split('.')[1];
                        switch (field)
                        {
                            case "id":
                                concatenado += transaction.id;
                                break;
                            case "status":
                                concatenado += transaction.status;
                                break;
                            case "amount_in_cents":
                                concatenado += transaction.amount_in_cents.ToString();
                                break;
                        }
                    }
                }

                // Concatenar timestamp y secreto
                concatenado += timestamp.ToString();
                string secreto = Variables.Wompi.IntegritySecret; // 🔒 Tu llave de integridad de eventos Wompi
                concatenado += secreto;

                // 3️⃣ Calcular SHA256
                using var sha = System.Security.Cryptography.SHA256.Create();
                var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(concatenado));
                var calculado = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                // 4️⃣ Validar que la firma coincida
                if (signature.checksum.ToLowerInvariant() != calculado)
                {
                    Console.WriteLine($"⚠️ Firma inválida: esperado {calculado}, recibido {signature.checksum}");
                    return BadRequest(new { error = "Firma inválida" });
                }

                // 5️⃣ Procesar evento válido
                Console.WriteLine($"✅ Evento recibido: {eventType} - Estado: {status} - Pedido: {reference}");

                // 👉 Aquí puedes actualizar tu pedido en la BD:
                // await _blPagos.ActualizarEstado(reference, status);

                // 6️⃣ Responder JSON (Wompi lo requiere)
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
