using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebApiTests.UnitTests.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private UserManager<User> userManager = null!;
        private IHttpContextAccessor contextAccessor = null!;
        private UserService userService = null!;

        [TestInitialize]
        public void SetUp()
        {
            userManager = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(), null,null,null,null,null,null,null,null);

            contextAccessor = Substitute.For<IHttpContextAccessor>();
            userService = new UserService(userManager, contextAccessor);
        }

        [TestMethod]
        public async Task GetUser_ReturnNull_WhenDoesntHaveEmailClaim()
        {
            //preparation
            var httpContext = new DefaultHttpContext();
            contextAccessor.HttpContext.Returns(httpContext);

            //test
            var user = await userService.GetUser();

            //validation
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task GetUser_ReturnUser_WhenExistEmailClaim()
        {
            //preparation
            var email = "test@gmail.com";
            var userExpect = new User {Email = email};

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(userExpect));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims};
            contextAccessor.HttpContext.Returns(httpContext);

            //test
            var user = await userService.GetUser();

            //validation
            Assert.IsNotNull(user);
            Assert.AreEqual(expected: email, actual: user.Email);
        }

        [TestMethod]
        public async Task GetUser_ReturnNull_WhenExistUserNoExist()
        {
            //preparation
            var email = "test@gmail.com";
            var userExpect = new User { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult<User>(null!));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
            contextAccessor.HttpContext.Returns(httpContext);

            //test
            var user = await userService.GetUser();

            //validation
            Assert.IsNull(user);
        }
    }
}
