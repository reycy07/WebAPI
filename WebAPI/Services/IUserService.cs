using Microsoft.AspNetCore.Identity;
using WebAPI.Entities;

namespace WebAPI.Services
{
    public interface IUserService
    {
        Task<User?> GetUser();
    }
}