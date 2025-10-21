using microPagos.API.Model.Response;
using microPagos.API.Utils;
using System.Net.Http.Headers;
using System.Text.Json;

public class PedidosClient
{
    private readonly HttpClient _httpClient;

    public PedidosClient(IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(Variables.PEDIDOSAPI.Url)
        };

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

    }

    public async Task PutAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.PutAsync(endpoint, null);

            // Si NO fue exitoso, lanza excepción con detalle del body
            response.EnsureSuccessStatusCode();

            Console.WriteLine($"✅ PUT {endpoint} - {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ Error en PUT {endpoint}: {ex.Message}");

            // Intentamos leer el contenido del body para ver el error del servidor
            if (ex.Data.Count == 0)
            {
                try
                {
                    // Intenta obtener el cuerpo de la respuesta (si existe)
                    var errorResponse = await _httpClient.GetAsync(endpoint);
                    var errorContent = await errorResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"🧾 Detalle del error: {errorContent}");
                }
                catch { /* ignorar errores secundarios */ }
            }
        }
    }


    public async Task<PedidoResponse> GetAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<PedidoResponse>(json, options)
               ?? throw new Exception("Error: Respuesta vacía");
    }
}
