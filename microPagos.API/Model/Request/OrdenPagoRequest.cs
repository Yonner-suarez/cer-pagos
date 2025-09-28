using MercadoPago.Client.Order;

namespace microPagos.API.Model.Request
{
    public class OrdenPagoRequest
    {
        public int IdPedido { get; set; }
        public decimal Monto { get; set; }
    }
}
