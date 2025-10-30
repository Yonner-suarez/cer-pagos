namespace microPagos.API.Model.Response
{
    public class ResultadoEnvio
    {
        public string Origen { get; set; }
        public string Destino { get; set; }
        public double DistanciaKm { get; set; }
        public string Categoria { get; set; }
        public decimal PesoKg { get; set; }
        public decimal Tarifa { get; set; }
    }
}
