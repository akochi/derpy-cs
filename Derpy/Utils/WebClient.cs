using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Derpy.Utils
{
    public class WebClient : IWebClient
    {
        private readonly HttpClient _httpClient;

        public WebClient()
        {
            _httpClient = new HttpClient();
        }

        public Task<HttpResponseMessage> GetAsync(Uri requestUri) => _httpClient.GetAsync(requestUri);
    }
}
