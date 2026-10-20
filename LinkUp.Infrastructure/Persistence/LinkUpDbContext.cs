using LinkUp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LinkUp.Infrastructure.Persistence;

public class LinkUpDbContext : IdentityDbContext<ApplicationUser>
{
    public LinkUpDbContext(
        DbContextOptions<LinkUpDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}