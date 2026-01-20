using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Utilities;

namespace WebApiTests.Utilities
{
    public class BaseTest
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions = 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        protected readonly Claim adminClaim = new Claim("isAdmin", "1");

        protected ApplicationDbContext BuildContext(string nambeDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nambeDB).Options;

            var dbContext = new ApplicationDbContext(options);
            return dbContext;
        }

        protected IMapper ConfigAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                options.AddProfile(new AutoMapperProfiles());
            });

            return config.CreateMapper();
        }



        protected WebApplicationFactory<Program> BuiltWebApplicationFactory( string nameDB, bool ignoreSecurity = true)
        {
            var factory = new WebApplicationFactory<Program>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor descriptorDBContext = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;

                    if(descriptorDBContext is not null)
                    {
                        services.Remove(descriptorDBContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameDB));

                    if (ignoreSecurity)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new FakeUserFilter());
                        });
                    }
                });
            });

            return factory;
        }

        protected async Task<string> CreateUser(string nameDB, WebApplicationFactory<Program> factory) =>
           await CreateUser(nameDB, factory, [], "ejemplo@ejemplo.com");
        protected async Task<string> CreateUser(string nameDB, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims) =>
           await CreateUser(nameDB, factory, claims, "ejemplo@ejemplo.com");
        protected async Task<string> CreateUser(
            string nameDb, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims, string email)
        {
            var urlRegister = "/api/v1/users";
            string token = string.Empty;
            token = await GetToken(email, urlRegister, factory);

            if (claims.Any())
            {
                var context = BuildContext(nameDb);
                var user = await context.Users.Where(x => x.Email == email).FirstAsync();
                Assert.IsNotNull(user);

                var userClaims = claims.Select(x => new IdentityUserClaim<string>
                {
                    UserId = user.Id,
                    ClaimType = x.Type,
                    ClaimValue = x.Value
                });

                context.UserClaims.AddRange(userClaims);
                await context.SaveChangesAsync();
                var urlLogin = "api/v1/users/login";
                token = await GetToken(email, urlLogin, factory);
            }

            return token;
        }

        private async Task<string> GetToken(string email, string url, WebApplicationFactory<Program> factory)
        {
            //Preparation
            var password = "aA2349S1#";
            var credentials =  new UserCredentialsDTO { Email = email, Password = password };
            var client = factory.CreateClient();

            //Test
            var response = await client.PostAsJsonAsync(url, credentials);
            response.EnsureSuccessStatusCode();

            //Validation
            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(content, jsonSerializerOptions)!;

            Assert.IsNotNull(authResponse.Token);

            return authResponse.Token;

        }
    }
}
