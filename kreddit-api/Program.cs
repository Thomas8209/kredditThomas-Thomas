using System;
using System.Text.Json.Serialization;
using kreddit_api.Data;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using shared.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<KredditContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KredditContext>();
    db.Database.Migrate();
    SeedSampleData(db);
}

app.UseCors("AllowAll");

app.MapGet("/", () => "Kreddit API er oppe");

app.MapGet("/api/posts", async (KredditContext db) =>
    await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments).ThenInclude(c => c.User)
        .OrderByDescending(p => p.CreatedAt)
        .Take(50)
        .ToListAsync());

app.MapGet("/api/posts/{id:int}", async (int id, KredditContext db) =>
{
    var post = await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments).ThenInclude(c => c.User)
        .FirstOrDefaultAsync(p => p.Id == id);

    return post is null ? Results.NotFound() : Results.Ok(post);
});

app.MapPost("/api/posts", async (CreatePostRequest request, KredditContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title is required.");
    }

    var hasContent = !string.IsNullOrWhiteSpace(request.Content);
    var hasUrl = !string.IsNullOrWhiteSpace(request.Url);

    if (!hasContent && !hasUrl)
    {
        return Results.BadRequest("Write some text or provide a url.");
    }

    if (hasContent && hasUrl)
    {
        return Results.BadRequest("Use either text or url, not both.");
    }

    if (string.IsNullOrWhiteSpace(request.Username))
    {
        return Results.BadRequest("Username is required.");
    }

    var user = await FindOrCreateUserAsync(db, request.Username!);

    var post = new Post(user)
    {
        Title = request.Title!.Trim(),
        Content = request.Content?.Trim() ?? string.Empty,
        Url = request.Url?.Trim() ?? string.Empty
    };

    db.Posts.Add(post);
    await db.SaveChangesAsync();

    await db.Entry(post).Reference(p => p.User).LoadAsync();
    await db.Entry(post).Collection(p => p.Comments).LoadAsync();

    return Results.Created($"/api/posts/{post.Id}", post);
});

app.MapPost("/api/posts/{id:int}/comments", async (int id, CreateCommentRequest request, KredditContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest("Content is required.");
    }

    var post = await db.Posts.FindAsync(id);
    if (post == null)
    {
        return Results.NotFound();
    }

    if (request.UserId <= 0)
    {
        return Results.BadRequest("UserId is required.");
    }

    var user = await db.Users.FindAsync(request.UserId);
    if (user == null)
    {
        return Results.BadRequest("User not found.");
    }

    var comment = new Comment(request.Content!.Trim(), user: user, post: post);

    db.Comments.Add(comment);
    await db.SaveChangesAsync();

    await db.Entry(comment).Reference(c => c.User).LoadAsync();

    return Results.Created($"/api/posts/{id}/comments/{comment.Id}", comment);
});

app.MapPut("/api/posts/{id:int}/upvote", async (int id, KredditContext db) =>
{
    var post = await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments).ThenInclude(c => c.User)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (post == null)
    {
        return Results.NotFound();
    }

    post.Upvotes += 1;
    await db.SaveChangesAsync();

    return Results.Ok(post);
});

app.MapPut("/api/posts/{id:int}/downvote", async (int id, KredditContext db) =>
{
    var post = await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments).ThenInclude(c => c.User)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (post == null)
    {
        return Results.NotFound();
    }

    post.Downvotes += 1;
    await db.SaveChangesAsync();

    return Results.Ok(post);
});

app.MapPut("/api/posts/{postId:int}/comments/{commentId:int}/upvote", async (int postId, int commentId, KredditContext db) =>
{
    var comment = await db.Comments
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

    if (comment == null)
    {
        return Results.NotFound();
    }

    comment.Upvotes += 1;
    await db.SaveChangesAsync();

    return Results.Ok(comment);
});

app.MapPut("/api/posts/{postId:int}/comments/{commentId:int}/downvote", async (int postId, int commentId, KredditContext db) =>
{
    var comment = await db.Comments
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

    if (comment == null)
    {
        return Results.NotFound();
    }

    comment.Downvotes += 1;
    await db.SaveChangesAsync();

    return Results.Ok(comment);
});

app.Run();

static async Task<User> FindOrCreateUserAsync(KredditContext db, string username)
{
    var trimmed = username.Trim();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == trimmed);
    if (user != null)
    {
        return user;
    }

    user = new User(trimmed);
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return user;
}

static void SeedSampleData(KredditContext db)
{
    if (db.Posts.Any())
    {
        Console.WriteLine("Seed: Data findes allerede, ingen ændringer.");
        return;
    }

    var user = new User("Thomas Den Store");
    db.Users.Add(user);
    db.SaveChanges();

    var post = new Post(user, "Velkommen til Kreddit", "Forumer er bare det bedste");
    db.Posts.Add(post);
    db.SaveChanges();

    db.Comments.Add(new Comment("Spændende projekt!", user: user, post: post));
    db.SaveChanges();

    Console.WriteLine("Seed: Oprettede 1 bruger, 1 post og 1 kommentar.");
}

public record CreatePostRequest(string? Title, string? Content, string? Url, string? Username);
public record CreateCommentRequest(string? Content, int UserId);
