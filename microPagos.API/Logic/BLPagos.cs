using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using microPagos.API.Dao;
using microPagos.API.Model;
using microPagos.API.Model.Request;
using microPagos.API.Model.Response;
using microPagos.API.Utils;
using microPagos.API.Utils.ExternalAPI;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.Json;

namespace microPagos.API.Logic
{
    public class BLPagos
    {
        private readonly PedidosClient _pedidosClient;

        public BLPagos(IHttpContextAccessor httpContextAccessor)
        {
            _pedidosClient = new PedidosClient(httpContextAccessor);
        }

        public GeneralResponse GenerarOrdenPago(List<OrdenPagoRequest> request,int idPedido, int idCliente, string correoCliente)
        {
            try
            {

                // 🔹 Calcular el total del pedido
                var total = request.Sum(x => x.Monto * x.Cantidad) + Variables.ENVIO.Monto;
                var totalCentavos = (int)(total * 100);

                // 🔹 Crear referencia única (por ejemplo: PEDIDO-1234)
                var referencia = $"PEDIDO_{idPedido}";

                // 🔹 Generar la firma de integridad
                var cadena = $"{referencia}{totalCentavos}COP{Variables.Wompi.IntegritySecret}";
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(cadena));
                var firma = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // 🔹 Guardar información si es necesario (bitácora o DB)
                DAPagos.RegistrarIntentoPago(total,idPedido, 1, idCliente);

                // 🔹 Retornar datos al frontend
                var data = new
                {
                    referencia,
                    montoEnPesos = totalCentavos,
                    firma,
                    Variables.Wompi.RedirectUrl,
                    Variables.Wompi.PublicKey,
                };

                return new GeneralResponse
                {
                    data = data,
                    status = Variables.Response.OK,
                    message = "Orden de pago Wompi generada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    data = null,
                    status = Variables.Response.ERROR,
                    message = $"Error generando orden de pago Wompi: {ex.Message}"
                };
            }
        }
        public int ParseIdPedido(string externalReference)
        {
            return int.Parse(externalReference.Replace("pedido_", ""));
        }

        public async Task<int> ActualizarEstadoPago(int externalReference)
        {
            var endpoint = $"/api/v1/Notificacion/EstadoPago/{externalReference}";

            await _pedidosClient.PutAsync(endpoint);

            return externalReference; // o retornar algo más relevante según la respuesta
        }

        public async Task<GeneralResponse> ObtenerPedido(int idPedido)
        {
            try
            {
                var endpoint = $"/api/v1/Pedido/PedidoDetalle/{idPedido}";

                // Ejecuta GET
                PedidoResponse? pedido = await _pedidosClient.GetAsync(endpoint);

                var confirmed = pedido.Data.EstadoPago != "Pendiente de pago" ? true : false;

                if(confirmed && pedido.Data.EstadoPago is not null) return new GeneralResponse { data = confirmed, message = "Pago Confirmado", status = Variables.Response.OK };
                else
                {
                    return new GeneralResponse { data = confirmed, message = "Pago Pendiente", status = Variables.Response.BadRequest };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener pedido: {ex.Message}");
                return null;
            }
        }
    }
}
