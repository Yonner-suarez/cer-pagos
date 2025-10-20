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
        public async Task<IActionResult> Eventos([FromBody] JsonElement evento)
        {
            try
            {
                // 1️⃣ Extraer la info principal
                var eventType = evento.GetProperty("event").GetString();
                var environment = evento.GetProperty("environment").GetString();
                var data = evento.GetProperty("data").GetProperty("transaction");
                var signature = evento.GetProperty("signature");
                var timestamp = evento.GetProperty("timestamp").GetInt64();

                var transactionId = data.GetProperty("id").GetString();
                var status = data.GetProperty("status").GetString();
                var amount = data.GetProperty("amount_in_cents").GetInt32();
                var reference = data.GetProperty("reference").GetString();

                // 2️⃣ Validar el checksum (firma del evento)
                var properties = signature.GetProperty("properties").EnumerateArray()
                    .Select(x => x.GetString()).ToList();

                // Concatenar los valores indicados en "properties"
                string concatenado = "";
                foreach (var prop in properties)
                {
                    // Ejemplo: prop = "transaction.id"
                    var parts = prop.Split('.');
                    if (parts.Length == 2 && parts[0] == "transaction")
                    {
                        concatenado += data.GetProperty(parts[1]).GetRawText().Trim('"');
                    }
                }

                // Concatenar timestamp
                concatenado += timestamp.ToString();

                // Concatenar tu SECRETO DE EVENTOS de Wompi Sandbox
                string secreto = "test_events_xxxxxxxxxxxxxxxxxxxxxxx"; // 🔒 pon aquí tu clave sandbox
                concatenado += secreto;

                // Calcular SHA256
                using var sha = System.Security.Cryptography.SHA256.Create();
                var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(concatenado));
                var calculado = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();

                var checksum = signature.GetProperty("checksum").GetString();

                // 3️⃣ Validar que la firma coincida
                if (checksum != calculado)
                {
                    Console.WriteLine("⚠️ Firma inválida del evento");
                    return BadRequest("Firma inválida");
                }

                // 4️⃣ Procesar evento válido
                Console.WriteLine($"✅ Evento recibido: {eventType} - {status} - Pedido {reference}");

                // 👉 Aquí puedes actualizar tu base de datos:
                // await _blPagos.ActualizarEstado(reference, status);

                // 5️⃣ Responder 200 OK (Wompi lo necesita)
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error manejando evento: {ex.Message}");
                return StatusCode(500, "Error procesando evento");
            }
        }
    }

}
