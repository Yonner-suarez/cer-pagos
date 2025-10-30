namespace microPagos.API.Model.Response
{
    public class Municipio
    {
        public int IdMunicipio { get; set; }
        public int IdDepartamento { get; set; }
        public string Nombre { get; set; }
        public bool EsCapital { get; set; }
        public double Longitud { get; set; }
        public double Latitud { get; set; }
    }
}
