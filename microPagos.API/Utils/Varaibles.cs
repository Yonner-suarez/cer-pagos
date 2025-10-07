namespace microPagos.API.Utils
{
    public class Variables
    {
        /// <summary>
        /// Se asigna en el startup
        /// </summary>
        public static string env = "appsettings.json";

        public static class Conexion
        {
            public static string cnx;

            static Conexion()
            {
                // Intentamos primero obtener variables del entorno (Render las pone ahí)
                var host = Environment.GetEnvironmentVariable("DB_HOST");
                var port = Environment.GetEnvironmentVariable("DB_PORT");
                var name = Environment.GetEnvironmentVariable("DB_NAME");
                var user = Environment.GetEnvironmentVariable("DB_USER");
                var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");

                // Si existen (Render), las usamos
                if (!string.IsNullOrEmpty(host))
                {
                    cnx = $"Server={host};Port={port};Database={name};User ID={user};Password={pass};";
                }
                else
                {
                    // Si estamos en local, leemos del appsettings.json
                    cnx = new ConfigurationBuilder()
                        .AddJsonFile(env)
                        .Build()
                        .GetSection("AppSettings")["conexion"];
                }
            }
        }
        public static class Token
        {
            private static readonly IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(env, optional: true)
                .Build();

            // TOKEN_KEY según tu appsettings.json
            public static readonly string Llave =
                Environment.GetEnvironmentVariable("TOKEN_KEY") ??
                config.GetSection("AppSettings").GetSection("Token")["Llave"];
        }

        public static class MercadoPago
        {
            private static readonly IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(env, optional: true)
                .Build()
                .GetSection("AppSettings").GetSection("MercadoPago");

            // Variables de entorno con prefijo MP_
            public static readonly string ACCESS_TOKEN =
                Environment.GetEnvironmentVariable("MP_ACCESS_TOKEN") ??
                config["ACCESS_TOKEN"];

            public static readonly string SuccessUrl =
                Environment.GetEnvironmentVariable("MP_SUCCESS_URL") ??
                config["SuccessUrl"];

            public static readonly string FailureUrl =
                Environment.GetEnvironmentVariable("MP_FAILURE_URL") ??
                config["FailureUrl"];

            public static readonly string PendingUrl =
                Environment.GetEnvironmentVariable("MP_PENDING_URL") ??
                config["PendingUrl"];

            public static readonly string CallbackUrl =
                Environment.GetEnvironmentVariable("MP_CALLBACK_URL") ??
                config["CallbackUrl"];
        }

        public static class PEDIDOSAPI
        {
            private static readonly IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(env, optional: true)
                .Build()
                .GetSection("AppSettings").GetSection("PEDIDOSAPI");

            // Variable de entorno PEDIDOS_API_URL
            public static readonly string Url =
                Environment.GetEnvironmentVariable("PEDIDOS_API_URL") ??
                config["url"];
        }

    public static class Response
        {
            public static int OK = StatusCodes.Status200OK;
            public static int ERROR = StatusCodes.Status500InternalServerError;
            public static int NotFound = StatusCodes.Status404NotFound;
            public static int BadRequest = StatusCodes.Status400BadRequest;
            public static int Inautorizado = StatusCodes.Status401Unauthorized;

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
