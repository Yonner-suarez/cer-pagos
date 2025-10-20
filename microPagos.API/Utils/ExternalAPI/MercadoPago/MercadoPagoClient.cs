using System;
using System.Threading.Tasks;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using MercadoPago.Config;

namespace microPagos.API.Utils.ExternalAPI
{
    public static class MercadoPagoClient
    {
        static MercadoPagoClient()
        {
            // Configura tu Access Token de MercadoPago
           // MercadoPagoConfig.AccessToken = Variables.MercadoPago.ACCESS_TOKEN;
        }

        /// <summary>
        /// Obtiene el estado actual del pago en MercadoPago.
        /// </summary>
        /// <param name="paymentId">ID del pago enviado en la notificación</param>
        /// <returns>Estado del pago ("approved", "pending", "rejected", etc.)</returns>
        public static async Task<string> GetPaymentStatus(long paymentId)
        {
            try
            {
                var client = new PaymentClient();
                Payment payment = await client.GetAsync(paymentId);

                if (payment != null && !string.IsNullOrEmpty(payment.Status))
                    return payment.Status;

                return "unknown";
            }
            catch (Exception ex)
            {
                // Loguear error
                Console.WriteLine($"Error al obtener estado del pago: {ex.Message}");
                return "error al interactuar con la api de mercado pago";
            }
        }
    }
}
