using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using WebAPI.Entities;
using WebApiTests.Utilities;

namespace WebApiTests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class CommentsControllerTests: BaseTest
    {
        private readonly string url = "/api/v1/books/1/comments";
        private string nameDB = Guid.NewGuid().ToString();

        private async Task CreateTestData()
        {
            var context = BuildContext(nameDB);
            var author = new Author { Names = "Camila", LastNames = "Arenas" };

            context.Add(author);
            await context.SaveChangesAsync();

            var book = new Book { Title = "Titulo" };
            book.Authors.Add(new AuthorBook { Author = author });
            context.Add(book);
            await context.SaveChangesAsync();


        }

        [TestMethod]
        public async Task Delete_Return204_WhenUserDeleteOwnComment()
        {
            //Preparation
            await CreateTestData();

            var factory = BuiltWebApplicationFactory(nameDB, ignoreSecurity: false);

            var token = await CreateUser(nameDB, factory);

            var context = BuildContext(nameDB);
            var user = await context.Users.FirstAsync();

            var commet = new Comment { Body = "body", UserId = user!.Id, BookId = 1 }; 
            context.Add(commet);

            await context.SaveChangesAsync();

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            //Test
            var response = await client.DeleteAsync($"{url}/{commet.Id}");
            //Verification

            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: response.StatusCode);
        }

        public async Task Delete_Return403_WhenUserTryDeleteComment()
        {
            //Preparation
            await CreateTestData();

            var factory = BuiltWebApplicationFactory(nameDB, ignoreSecurity: false);

            var emailCreadorComentario = "comment-creator@hotmail.com";

            await CreateUser(nameDB, factory, [], emailCreadorComentario);

            var context = BuildContext(nameDB);
            var userCreatorComment = await context.Users.FirstAsync();

            var commet = new Comment { Body = "body", UserId = userCreatorComment!.Id, BookId = 1 };
            context.Add(commet);

            await context.SaveChangesAsync();

            var tokenDifferentUser = await CreateUser(nameDB, factory, [], "user-different@gmail.com");

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDifferentUser);


            //Test
            var response = await client.DeleteAsync($"{url}/{commet.Id}");
            //Verification

            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: response.StatusCode);
        }

    }
}
