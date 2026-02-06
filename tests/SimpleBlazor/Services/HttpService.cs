using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SimpleBlazor.Contracts;

namespace SimpleBlazor.Services;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;

    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> PostJsonAsync<T>(string url, T payload, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
