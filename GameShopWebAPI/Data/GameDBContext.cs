using GameShopWebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace GameShopWebAPI.Data
{
    public class GameDBContext : DbContext
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
