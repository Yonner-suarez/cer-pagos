using microPagos.API.Dao;
using microPagos.API.Model;
using microPagos.API.Model.Request;
using microPagos.API.Model.Response;
using microPagos.API.Utils;

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
        public async Task<int> ActualizarEstadoPago(int externalReference)
        {
            try
            {
                var endpoint = $"/api/v1/Pedido/EstadoPago/{externalReference}/1";
                Console.WriteLine(endpoint);

                await _pedidosClient.PutAsync(endpoint);

                return externalReference;
            }
            catch (HttpRequestException httpEx)
            {
                // Error en la llamada HTTP
                Console.WriteLine($"Error en la petición HTTP: {httpEx.Message}");
                return -1; 
            }
            catch (Exception ex)
            {
                // Otros errores inesperados
                Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
                return -1; 
            }
        }

    }
}
