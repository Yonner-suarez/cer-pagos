using MercadoPago.Client.Order;

namespace microPagos.API.Model.Request
{
    public class OrdenPagoRequest
    {
        public decimal Monto { get; set; }
        public int Cantidad { get; set; }
    }
}
