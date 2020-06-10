using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace Derpy.Utils.Tumblr
{
    public class TumblrClient : ITumblrClient
    {
        private readonly IWebClient _webClient;
        private readonly string _apiKey;

        private static Uri apiUrl = new Uri("https://api.tumblr.com/v2/blog/");
        private static JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };

        public TumblrClient(IWebClient webClient, IKeyProvider keyProvider)
        {
            _webClient = webClient;
            _apiKey = keyProvider.TumblrApiKey;
        }

        // https://www.tumblr.com/docs/en/api/v2#posts--retrieve-published-posts
        // https://github.com/tumblr/docs/blob/master/api.md#posts--retrieve-published-posts
        public async Task<string[]> GetAllPostUrlsAsync(string blogIdentifier, string tag = null)
        {
            var @params = new Dictionary<string, string>
            {
                { "api_key", _apiKey }
            };

            if (!string.IsNullOrWhiteSpace(tag))
            {
                @params.Add("tag", tag);
            }

            var uri = BuildUri(blogIdentifier, "/posts", @params);
            var response = await _webClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Tumblr posts GET response returned {StatusCode}", response.StatusCode);
                return null;
            }

            TumblrResponse responseData;

            try
            {
                responseData = await DeserializeResponse<TumblrResponse>(response);
            }
            catch
            {
                return null;
            }

            if (responseData.Meta.Status != 200)
            {
                Log.Error("Tumblr posts meta field returned {StatusCode}", responseData.Meta.Status);
                return null;
            }

            return responseData.Response.Posts.Select(post => post.PostUrl).ToArray();
        }

        private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<T>(content, _serializerOptions);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error when deserializing {content}", content);
                throw;
            }
        }

        private static Uri BuildUri(string blogIdentifier, string endpoint, Dictionary<string, string> @params = null)
        {
            var uriBuilder = new UriBuilder(apiUrl);
            uriBuilder.Path += blogIdentifier + endpoint;

            if (@params != null)
            {
                uriBuilder.Query = BuildQuery(@params);
            }

            return uriBuilder.Uri;
        }

        private static string BuildQuery(Dictionary<string, string> @params)
        {
            return string.Join("&", @params.Select(_ => $"{_.Key}={WebUtility.UrlEncode(_.Value.Trim())}"));
        }
    }
}
