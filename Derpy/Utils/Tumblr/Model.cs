using System.Collections.Generic;

namespace Derpy.Utils.Tumblr
{
    public class Post
    {
        public long Id { get; set; }
        public string BlogName { get; set; }
        public string PostUrl { get; set; }
    }

    public class PostResponse
    {
        public long TotalPosts { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Meta
    {
        public int Status { get; set; }
        public string Msg { get; set; }
    }

    public class TumblrResponse
    {
        public Meta Meta { get; set; }
        public PostResponse Response { get; set; }
    }
}
