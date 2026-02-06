using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleBlazor.Contracts;

public interface IHttpService
{
    Task<string> GetAsync(string url, CancellationToken cancellationToken = default);
    Task<string> PostJsonAsync<T>(string url, T payload, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
