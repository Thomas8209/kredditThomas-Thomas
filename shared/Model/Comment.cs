using System;

namespace shared.Model;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public Comment(string content = "", int upvotes = 0, int downvotes = 0, User user = null, Post post = null, DateTime? createdAt = null)
    {
        Content = content;
        Upvotes = upvotes;
        Downvotes = downvotes;
        User = user;
        UserId = user != null ? user.Id : 0;
        Post = post;
        PostId = post != null ? post.Id : 0;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public Comment() {
        Id = 0;
        Content = "";
        Upvotes = 0;
        Downvotes = 0;
        CreatedAt = DateTime.UtcNow;
        PostId = 0;
        UserId = 0;
        Post = null;
        User = null;
    }
}
