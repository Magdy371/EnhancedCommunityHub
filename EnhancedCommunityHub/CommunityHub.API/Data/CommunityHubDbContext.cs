using Microsoft.EntityFrameworkCore;
using CommunityHub.API.Models;

namespace CommunityHub.API.Data
{
    public class CommunityHubDbContext(DbContextOptions<CommunityHubDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Resource> Resources { get; set; }
    }

}
