using System.Net;
using System.Security.Claims;
using System.Text.Json;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebApiTests.Utilities;

namespace WebApiTests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTests: BaseTest
    {
        private static readonly string url = "/api/v1/authors";
        private string nameDB = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Get_Return404_WhenAuthorNoExsist()
        {
            //Preparation
            var factory = BuiltWebApplicationFactory(nameDB);

            var client = factory.CreateClient();

            //Test 

            var response = await client.GetAsync($"{url}/1");

            //Verification

            var statusCode = response.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: response.StatusCode);

        }
        [TestMethod]
        public async Task Get_ReturnAuthor_WhenAuthorExsist()
        {
            //Preparation
            var context = BuildContext(nameDB);
            context.Authors.Add(new Author() { Names = "Camila", LastNames = "Arenas" });
            context.Authors.Add(new Author() { Names = "Aberladina", LastNames = "Espriella" });
            await context.SaveChangesAsync();


            var factory = BuiltWebApplicationFactory(nameDB);
            var client = factory.CreateClient();

            //Test 

            var response = await client.GetAsync($"{url}/1");

            //Verification
            response.EnsureSuccessStatusCode();

            var author = JsonSerializer.Deserialize<AuthorWithBooksDTO>(
                await response.Content.ReadAsStringAsync(), jsonSerializerOptions)!;
            var statusCode = response.StatusCode;
            Assert.AreEqual(expected: 1, actual: author.Id);

        }

        [TestMethod]
        public async Task Post_Return401_WhenUserDoesNotAuthentication()
        {
            //Preparation
            var factory = BuiltWebApplicationFactory(nameDB, ignoreSecurity: false);

            var client = factory.CreateClient();
            var authorCreateDTO = new AuthorCreateDTO
            {
                Names = "Camilia",
                LastNames = "Arenas",
                Identification = "123"
            };

            //Test 
            var response = await client.PostAsJsonAsync(url, authorCreateDTO);

            //Verification
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: response.StatusCode);
        }

        [TestMethod]
        public async Task Post_Return403_WhenUserDoesNotBeAdmin()
        {
            //Preparation
            var factory = BuiltWebApplicationFactory(nameDB, ignoreSecurity: false);
            var token = await CreateUser(nameDB, factory);

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorCreateDTO = new AuthorCreateDTO
            {
                Names = "Camilia",
                LastNames = "Arenas",
                Identification = "123"
            };

            //Test 
            var response = await client.PostAsJsonAsync(url, authorCreateDTO);

            //Verification
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: response.StatusCode);
        }

        [TestMethod]
        public async Task Post_Return201_WhenUserDoesNotBeAdmin()
        {
            //Preparation
            var factory = BuiltWebApplicationFactory(nameDB, ignoreSecurity: false);

            var claims = new List<Claim> { adminClaim };

            var token = await CreateUser(nameDB, factory, claims);

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorCreateDTO = new AuthorCreateDTO
            {
                Names = "Camilia",
                LastNames = "Arenas",
                Identification = "123"
            };

            //Test 
            var response = await client.PostAsJsonAsync(url, authorCreateDTO);

            //Verification
            response.EnsureSuccessStatusCode();
            Assert.AreEqual(expected: HttpStatusCode.Created, actual: response.StatusCode);
        }
    }
}
