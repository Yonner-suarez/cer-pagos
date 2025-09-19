namespace microPagos.API.Utils
{
    public class Variables
    {
        /// <summary>
        /// Se asigna en el startup
        /// </summary>
        public static string env = "appsettings.json";
        public static int ambiente = int.Parse(new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings")["ambiente"]);

        public static class Conexion
        {
            //Local
            public static string cnx = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings")["conexion"];
        }



        public static class Token
        {
            public static string PasswordHash = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Token")["PasswordHash"];
            public static string SaltKey = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Token")["SaltKey"];
            public static string VIKey = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Token")["VIKey"];
            public static string Bearer = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Token")["Bearer"];
        }

        //public static class Smtp
        //{
        //    public static string BCC = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["BCC"];
        //    public static string SMTP = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["SMTP"];
        //    public static string PUERTO = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["PUERTO"];
        //    public static string USUARIO = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["USUARIO"];
        //    public static string PASSWORD = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["PASSWORD"];
        //    public static string ENVIAR = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["ENVIAR"];
        //    public static string TLS = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["TLS"];
        //    // pruebas
        //    public static string PRUEBA = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["PRUEBA"];
        //    public static string CORREO_PRUEBA = new ConfigurationBuilder().AddJsonFile(env).Build().GetSection("AppSettings").GetSection("Smtp")["CORREO_PRUEBA"];
        //}

    }
}
