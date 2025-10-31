using microPagos.API.Dao;
using microPagos.API.Model;
using microPagos.API.Model.Request;
using microPagos.API.Model.Response;
using microPagos.API.Utils;
using Org.BouncyCastle.Ocsp;
using static microPagos.API.Utils.Variables;

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
                decimal envio = request.Select(x => x.TarifaEnvio).FirstOrDefault();
                var total = request.Sum(x => x.Monto * x.Cantidad) + envio;
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

        public GeneralResponse ObtenerMunicipios()
        {
            try
            {
                var data = DAPagos.ObtenerMunicipios();

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
                    message = $"Error al obtener municipios: {ex.Message}"
                };
            }
        }
        public GeneralResponse CalcularEnvio(int idDestino, decimal pesoKg)
        {
            try
            {
                // 1️⃣ Obtener Tunja como origen fijo (ID 7)
                Municipio origen = DAPagos.ObtenerMunicipioPorId(Variables.ID_ORIGEN_ENVIO);
                if (origen == null)
                {
                    return new GeneralResponse
                    {
                        data = null,
                        message = "No se encontró el municipio de origen (Tunja).",
                        status = Variables.Response.ERROR
                    };
                }

                // 2️⃣ Obtener destino por ID
                Municipio destino = DAPagos.ObtenerMunicipioPorId(idDestino);
                if (destino == null)
                {
                    return new GeneralResponse
                    {
                        data = null,
                        message = $"No se encontró el municipio destino con ID {idDestino}.",
                        status = Variables.Response.ERROR
                    };
                }

                // 3️⃣ Calcular distancia con Haversine
                double distancia = Haversine(origen.Latitud, origen.Longitud, destino.Latitud, destino.Longitud);

                // 4️⃣ Categorizar según distancia
                string categoria;
                if (distancia <= 50)
                    categoria = "CERCANO";
                else if (distancia <= 300)
                    categoria = "INTERMEDIO";
                else
                    categoria = "LEJANO";

                // 5️⃣ Calcular tarifa base
                decimal baseTarifa = 8000m;
                decimal costoKm = categoria == "CERCANO" ? 90m :
                                  categoria == "INTERMEDIO" ? 92m : 95m;

                decimal tarifaBase = baseTarifa + (decimal)distancia * costoKm;

                // 6️⃣ Ajuste si no es capital
                if (!destino.EsCapital)
                    tarifaBase += 15000m;

                // 7️⃣ Límite máximo
                if (tarifaBase > 150000m)
                    tarifaBase = 150000m;

                // 8️⃣ Ajustar por peso (cada kg adicional cuesta 500 pesos)
                decimal tarifaFinal = tarifaBase;

                if (pesoKg > 1)
                {
                    decimal kilosExtra = pesoKg - 1;
                    tarifaFinal += kilosExtra * 5000m;
                }

                var data = new ResultadoEnvio
                {
                    Origen = origen.Nombre,
                    Destino = destino.Nombre,
                    DistanciaKm = Math.Round(distancia, 1),
                    Categoria = categoria,
                    PesoKg = pesoKg,
                    Tarifa = Math.Round(tarifaFinal, 2)
                };

                return new GeneralResponse
                {
                    data = data,
                    message = "Se calculó el envío correctamente.",
                    status = Variables.Response.OK
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    data = null,
                    message = $"Error al calcular el envío: {ex.Message}",
                    status = Variables.Response.ERROR
                };
            }
        }

        #region Private methods
        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = GradosARadianes(lat2 - lat1);
            double dLon = GradosARadianes(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(GradosARadianes(lat1)) * Math.Cos(GradosARadianes(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Variables.RADIO_TIERRA * c;

        }
        private double GradosARadianes(double grados) => grados * Math.PI / 180.0;
        #endregion


    }
}
