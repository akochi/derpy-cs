using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Derpy.Utils
{
    public interface IWebClient
    {
        Task<HttpResponseMessage> GetAsync(Uri requestUri);
    }
}
