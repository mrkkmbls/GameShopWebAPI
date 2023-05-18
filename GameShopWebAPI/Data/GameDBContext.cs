using GameShopWebAPI.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameShopWebAPI.Data
{
    public class GameDBContext : IdentityDbContext<ApplicationUser>
    {
        public GameDBContext() 
        {

        }

        public GameDBContext(DbContextOptions<GameDBContext> options) : base(options)

        {
        
        }

        public DbSet<Game> Games => Set<Game>();
    }
}
