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
        var response = await _httpClient.PutAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
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
