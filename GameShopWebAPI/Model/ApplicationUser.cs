using Microsoft.AspNetCore.Identity;

namespace GameShopWebAPI.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string firstName { get; set; }

        public string lastName { get; set; }
    }
}
