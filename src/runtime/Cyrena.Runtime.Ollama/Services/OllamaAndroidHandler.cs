using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Runtime.Ollama.Services
{
    public sealed class OllamaAndroidHandler : DelegatingHandler
    {
        public OllamaAndroidHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Remove("Origin");
            request.Headers.Remove("Referer");
            // Spoof User-Agent to match desktop
            request.Headers.Remove("User-Agent");
            request.Headers.TryAddWithoutValidation(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/124.0.0.0 Safari/537.36");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
