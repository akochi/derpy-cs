using System.Threading.Tasks;

namespace Derpy.Utils.Tumblr
{
    public interface ITumblrClient
    {
        Task<string[]> GetAllPostUrlsAsync(string blogIdentifier, string tag = null);
    }
}
