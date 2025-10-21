using System.Collections.Generic;

namespace shared.Model;

public class User {
    public int Id { get; set; }
    public string Username { get; set; }
    public List<Post> Posts { get; set; } = new List<Post>();
    public List<Comment> Comments { get; set; } = new List<Comment>();

    public User(string username = "") {
        Username = username;
    }

    public User() {
        Id = 0;
        Username = "";
    }
}
