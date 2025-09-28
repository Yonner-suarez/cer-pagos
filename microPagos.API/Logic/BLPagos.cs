using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using microPagos.API.Dao;
using microPagos.API.Model;
using microPagos.API.Model.Request;
using microPagos.API.Model.Response;
using microPagos.API.Utils;
using microPagos.API.Utils.ExternalAPI;
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

        public GeneralResponse GenerarOrdenPago(OrdenPagoRequest req, int idCliente, string correoCliente)
        {
            // Configurar credenciales de MercadoPago (PRODUCCIÓN)
            MercadoPagoConfig.AccessToken = Variables.MercadoPago.ACCESS_TOKEN;

            // Crear la preferencia de pago
            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = $"Pedido #{req.IdPedido}",
                        Quantity = 1,
                        CurrencyId = "COP",
                        UnitPrice = req.Monto
                    }
                },
                ExternalReference = $"pedido_{req.IdPedido}",
                Payer = new PreferencePayerRequest
                {
                    Email = correoCliente // o test user si es sandbox
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = Variables.MercadoPago.SuccessUrl.Replace("idPedidoParams", req.IdPedido.ToString()),
                    Failure = Variables.MercadoPago.FailureUrl.Replace("idPedidoParams", req.IdPedido.ToString()),
                    Pending = Variables.MercadoPago.PendingUrl.Replace("idPedidoParams", req.IdPedido.ToString()),
                    
                },
                //AutoReturn = "approved",
                NotificationUrl = Variables.MercadoPago.CallbackUrl // Webhook para notificaciones
            };

            var client = new PreferenceClient();
            Preference preference = client.Create(request);

            // URL de checkout para redirigir al cliente
            string checkoutUrl = preference.SandboxInitPoint;

            // Guardar en la BD usando tu DAO
            int idPasarela = DAPagos.CrearPasarela("MercadoPago", idCliente);
            bool ok = DAPagos.GuardarPago(req.Monto, req.IdPedido, idPasarela, idCliente);

            if (!ok)
            {
                return new GeneralResponse
                {
                    status = Variables.Response.BadRequest,
                    message = "No se pudo completar el guardado de pago",
                    data = null
                };
            }

            return new GeneralResponse
            {
                status = Variables.Response.OK,
                message = "Orden generada correctamente",
                data = checkoutUrl
            };
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
