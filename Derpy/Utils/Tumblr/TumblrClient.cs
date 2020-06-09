using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Derpy.Utils.Tumblr
{
    public class TumblrClient
    {
        private static Uri apiUrl = new Uri("https://api.tumblr.com/v2/blog/");
        private static string apiKey = "hello";

        private static JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };

        private readonly HttpClient _httpClient;

        public TumblrClient(HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler);
        }

        // https://www.tumblr.com/docs/en/api/v2#posts--retrieve-published-posts
        // https://github.com/tumblr/docs/blob/master/api.md#posts--retrieve-published-posts
        public async Task<string[]> GetAllPostUrlsAsync(string blogIdentifier, string tag = null)
        {
            var @params = new Dictionary<string, string>
            {
                { "api_key", apiKey }
            };

            if (!string.IsNullOrWhiteSpace(tag))
            {
                @params.Add("tag", tag);
            }

            var uri = BuildUri(blogIdentifier, "/posts", @params);
            var response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                // do logging i guess
                return null;
            }

            var responseData = await DeserializeResponse<PostsResponse>(response);

            if (responseData.Meta.Status != 200)
            {
                // do logging i guess
                return null;
            }

            return responseData.Response.Posts.Select(post => post.PostUrl).ToArray();
        }

        private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _serializerOptions);
            }
            catch (Exception)
            {
                // do logging i guess
            }

            return default;
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
            return string.Join("&", @params.Select(_ => $"{_.Key}={WebUtility.UrlEncode(_.Value)}"));
        }
    }
}
