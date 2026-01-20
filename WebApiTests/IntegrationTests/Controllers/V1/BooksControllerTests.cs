using System.Net;
using WebAPI.DTOs;
using WebApiTests.Utilities;

namespace WebApiTests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class BooksControllerTests : BaseTest
    {
        private readonly string url = "/api/v1/books";
        private string nameDB = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Post_Return400_WhenAuthorsIDsIfbeEmpty()
        {
            //Preparation
            var factory = BuiltWebApplicationFactory(nameDB);
            var client = factory.CreateClient();
            var bookCreationDTO = new BookCreateDTO { Title = "title" };

            //Test
            var response = await client.PostAsJsonAsync(url, bookCreationDTO);

            //Verification
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, response.StatusCode);
                
        }
    }
}
