using Microsoft.AspNetCore.Identity;

namespace WebAPI.Entities
{
    public class User:IdentityUser 
    {
        public DateTime BirthDate { get; set; }
    }
}
