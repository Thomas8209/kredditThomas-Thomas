using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace kreddit_api.Data;

public class KredditContext : DbContext
{
    public KredditContext(DbContextOptions<KredditContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<User> Users => Set<User>();
}
