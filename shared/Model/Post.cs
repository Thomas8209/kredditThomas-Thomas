using System;
using System.Collections.Generic;

namespace shared.Model;

public class Post {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Url { get; set; }
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public List<Comment> Comments { get; set; } = new List<Comment>();

    public Post(User user, string title = "", string content = "", string url = "", int upvotes = 0, int downvotes = 0, DateTime? createdAt = null) {
        Title = title;
        Content = content;
        Url = url;
        Upvotes = upvotes;
        Downvotes = downvotes;
        User = user;
        UserId = user != null ? user.Id : 0;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public Post() {
        Id = 0;
        Title = "";
        Content = "";
        Url = "";
        Upvotes = 0;
        Downvotes = 0;
        CreatedAt = DateTime.UtcNow;
        UserId = 0;
        User = null;
    }

    public override string ToString()
    {
        return $"Id: {Id}, Title: {Title}, Content: {Content}, Url: {Url}, Upvotes: {Upvotes}, Downvotes: {Downvotes}, CreatedAt: {CreatedAt}, User: {User}";
    }
}
