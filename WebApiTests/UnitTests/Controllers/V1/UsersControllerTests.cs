using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Controllers.V1;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Services;
using WebApiTests.Utilities;

namespace WebApiTests.UnitTests.Controllers.V1
{
    [TestClass]
    public class UsersControllerTests:BaseTest
    {
        private string nameDB = Guid.NewGuid().ToString();
        private UserManager<User> userManager = null!;
        private SignInManager<User> signInManager = null!;
        private IUserService userService = null!;
        //private IMapper mapper = null!;
        private UsersController controller = null!;
        private IHttpContextAccessor contextAccessor = null!;
        private IUserClaimsPrincipalFactory<User> userClaimsFactory = null!;

        [TestInitialize]
        public void Setup()
        {
            var context = BuildContext(nameDB);
            userManager = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            var myConfig = new Dictionary<string, string>
            {
                {
                    "jwtKey", "dlkjfalsmxiovnasqdADPFJKLEMWQPOEWRQINMDAL"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfig!)
                .Build();

            contextAccessor = Substitute.For<IHttpContextAccessor>();

            userClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();

            signInManager = Substitute.For<SignInManager<User>>(userManager,
                contextAccessor, userClaimsFactory, null, null, null, null);

            userService = Substitute.For<IUserService>();

            var mapper = ConfigAutoMapper();

            controller = new UsersController(configuration, userManager, signInManager, userService, context ,mapper);
        }

        [TestMethod]
        public async Task Register_ReturnValidationProblem_WhenItIsNotSuccess()
        {
            //Preparation
            string errorMessage = "test";
            UserCredentialsDTO credentialsDTO = new UserCredentialsDTO
            {
                Email = "test@test.com",
                Password = "29342sdfa"
            };

            userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError
                {
                    Code = "test",
                    Description = errorMessage
                }));

            //test
            var response = await controller.Register(credentialsDTO);

            //verification
            var result = response.Result as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: errorMessage, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Register_ReturnToken_WhenItIsExist()
        {
            //Preparation
            string errorMessage = "test";
            UserCredentialsDTO credentialsDTO = new UserCredentialsDTO
            {
                Email = "test@test.com",
                Password = "29342sdfa"
            };

            userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //test
            var response = await controller.Register(credentialsDTO);

            //verification
            Assert.IsNotNull(response.Value);
            Assert.IsNotNull(response.Value.Token);
        }

        [TestMethod]
        public async Task Login_ReturnValidationProblem_WhenUserDoesNotExist()
        {
            //Preparation
            UserCredentialsDTO credentialsDTO = new UserCredentialsDTO
            {
                Email = "test@test.com",
                Password = "29342sdfa"
            };

            userManager.FindByEmailAsync(credentialsDTO.Email)!.Returns(Task.FromResult<User>(null!));
            //test
            var response = await controller.Login(credentialsDTO);

            //Validation
            var result = response.Result as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login Incorrecto", actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_ReturnValidationProblem_WhenLoginIsNotCorrrect()
        {
            //Preparation
            UserCredentialsDTO credentialsDTO = new UserCredentialsDTO
            {
                Email = "test@test.com",
                Password = "29342sdfa"
            };

            var user = new User
            { Email = credentialsDTO.Email };

            userManager.FindByEmailAsync(credentialsDTO.Email)!.Returns(Task.FromResult<User>(user));

            signInManager.CheckPasswordSignInAsync(user, credentialsDTO.Password, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);
            //test
            var response = await controller.Login(credentialsDTO);

            //Validation
            var result = response.Result as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login Incorrecto", actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_ReturnToken_WhenLoginIsCorrrect()
        {
            //Preparation
            UserCredentialsDTO credentialsDTO = new UserCredentialsDTO
            {
                Email = "test@test.com",
                Password = "29342sdfa"
            };

            var user = new User
            { Email = credentialsDTO.Email };

            userManager.FindByEmailAsync(credentialsDTO.Email)!.Returns(Task.FromResult<User>(user));

            signInManager.CheckPasswordSignInAsync(user, credentialsDTO.Password, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            //test
            var response = await controller.Login(credentialsDTO);

            //Validation
            Assert.IsNotNull(response.Value);
            Assert.IsNotNull(response.Value.Token);
        }
    }
}
