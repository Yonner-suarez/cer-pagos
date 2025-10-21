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
            var content = await response.Content.ReadAsStringAsync(); // 👈 leemos SIEMPRE el contenido

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error en PUT {endpoint}");
                Console.WriteLine($"Status: {(int)response.StatusCode} - {response.ReasonPhrase}");
                Console.WriteLine($"🧾 Detalle del error: {content}");
            }
            else
            {
                Console.WriteLine($"✅ PUT {endpoint} OK - {(int)response.StatusCode}");
            }

            response.EnsureSuccessStatusCode(); // 👈 mantiene el comportamiento de lanzar si no fue 2xx
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"⚠️ Excepción HTTP en {endpoint}: {ex.Message}");
            throw; // opcional: vuelve a lanzar para manejo externo
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Error inesperado en {endpoint}: {ex.Message}");
            throw;
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
