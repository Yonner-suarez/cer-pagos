namespace microPagos.API.Model.Response
{
    public class PedidoResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public PedidoData? Data { get; set; }
    }
    public class PedidoData
    {
        public int IdPedido { get; set; }
        public string Estado { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaPedido { get; set; }
        public string NroGuia { get; set; }
        public string EnlaceTransportadora { get; set; }
        public string EstadoPago { get; set; }
        public decimal Monto { get; set; }
        public List<ProductoPedido> productos { get; set; }

    }
    public class ProductoPedido
    {
        public string Marca { get; set; }
        public string Cateogira { get; set; }

        public string Descripcion { get; set; }
        public int Cantidad { get; set; }

        public int PrecioUnitario { get; set; }
        public byte[] Image { get; set; }
    }
}
