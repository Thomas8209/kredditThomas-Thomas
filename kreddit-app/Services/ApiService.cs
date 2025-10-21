using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using shared.Model;

namespace kreddit_app.Data;

public class ApiService
{
    private readonly HttpClient http;
    private readonly IConfiguration configuration;
    private readonly string baseAPI = "";

    public ApiService(HttpClient http, IConfiguration configuration)
    {
        this.http = http;
        this.configuration = configuration;
        this.baseAPI = configuration["base_api"]!;
    }

    public async Task<Post[]> GetPosts()
    {
        string url = $"{baseAPI}posts/";
        return (await http.GetFromJsonAsync<Post[]>(url))!;
    }

    public async Task<Post> GetPost(int id)
    {
        string url = $"{baseAPI}posts/{id}/";
        return (await http.GetFromJsonAsync<Post>(url))!;
    }

    public async Task<Post> CreatePost(string title, string content, string url, string username)
    {
        string requestUrl = $"{baseAPI}posts/";

        var body = new { title, content, url, username };
        HttpResponseMessage msg = await http.PostAsJsonAsync(requestUrl, body);
        string json = msg.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task<Comment> CreateComment(string content, int postId, int userId)
    {
        string url = $"{baseAPI}posts/{postId}/comments";
     
        HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { content, userId });
        string json = msg.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task<Post> UpvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/upvote/";
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");
        string json = msg.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task<Post> DownvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/downvote/";
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");
        string json = msg.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task<Comment> UpvoteComment(int postId, int commentId)
    {
        string url = $"{baseAPI}posts/{postId}/comments/{commentId}/upvote/";
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");
        string json = msg.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task<Comment> DownvoteComment(int postId, int commentId)
    {
        string url = $"{baseAPI}posts/{postId}/comments/{commentId}/downvote/";
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");
        string json = msg.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        })!;
    }
}
