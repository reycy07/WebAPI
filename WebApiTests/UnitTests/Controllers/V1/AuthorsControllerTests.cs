using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using WebAPI.Controllers.V1;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Routes;
using WebAPI.Services;
using WebApiTests.Utilities;
using WebApiTests.Utilities.Doubles;

namespace WebApiTests.UnitTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTests: BaseTest
    {
        IFileStorage fileStorage = null!;
        ILogger<AuthorsController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        private string nameDB = Guid.NewGuid().ToString();
        private AuthorsController controller = null!;
        private AuthorRoutes authorRoutes = null!;
        private const string container = "authors";
        private const string cache = "get-authors";

        [TestInitialize]
        public void Setup()
        {
            var context = BuildContext(nameDB);
            var mapper = ConfigAutoMapper();
            fileStorage = Substitute.For<IFileStorage>();
            logger = Substitute.For<ILogger<AuthorsController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            authorRoutes = Substitute.For<AuthorRoutes>();

            controller = new AuthorsController(
                    context, mapper, logger, fileStorage, outputCacheStore, authorRoutes
                );
        }
        [TestMethod]
        public async Task Get_Return404_WhenAuthorWithIdNoExist()
        {
            //Test
            var response = await controller.Get(1);

            //Verification

            var result = response.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: result!.StatusCode);
        }


        [TestMethod]
        public async Task Get_ReturnAuthor_WhenAuthorWhitIdExist()
        {
            //preparation
           
            var context = BuildContext(nameDB);

            context.Authors.Add(new Author { Names = "Cristian", LastNames = "Rojas" });
            context.Authors.Add(new Author { Names = "Camila", LastNames = "Arenas" });

            await context.SaveChangesAsync();

            //Test
            var response = await controller.Get(1);

            //Verification

            var result = response.Value;
            Assert.AreEqual(expected: 1, actual: result!.Id);
        }
        [TestMethod]
        public async Task Get_ReturnAuthorWithBooks_WhenAuthorHaveBooks()
        {
            //preparation
           
            var context = BuildContext(nameDB);

            var book1 = new Book { Title = "book 1" };
            var book2 = new Book { Title = "book 2" };

            var author = new Author()
            {
                Names = "Camila",
                LastNames = "Arenas",
                Books = new List<AuthorBook>()
                {
                    new AuthorBook{Book = book1},
                    new AuthorBook{Book = book2}
                }
            };

            context.Add(author);

            await context.SaveChangesAsync();

            //Test
            var response = await controller.Get(1);

            //Verification
            var result = response.Value;
            Assert.AreEqual(expected: 1, actual: result!.Id);
            Assert.AreEqual(expected: 2, actual: result.Books.Count);
        }
        [TestMethod]
        public async Task Post_MayCreateAuthor_WhenSendAuthor()
        {
            //preparation
            var context = BuildContext(nameDB);

            var newAuthor = new AuthorCreateDTO { Names  = "Camilita", LastNames = "Arenita", Identification = "10231"};

            //Test

            var response = await controller.Post(newAuthor);

            //Verification

            var result = response as CreatedAtRouteResult;
            Assert.IsNotNull(result);
            
            var context2 = BuildContext(nameDB);
            var qty = await context2.Authors.CountAsync();
            
            Assert.AreEqual(expected: 1, actual: qty);
        }
        [TestMethod]
        public async Task Put_Return404_WhenAuthorNoExist()
        {
            //Test
            var response = await controller.Put(1, authorCreateWithPictureDTO: null!);
            //Verification

            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result!.StatusCode);
        }

        [TestMethod]
        public async Task Put_UpdateAuthor_WhenSentAuthorWithoutPicture()
        {
            //preparation
            var context = BuildContext(nameDB);

            context.Authors.Add(new Author { Names = "Cristian", LastNames = "Rojas", Identification = "" });

            await context.SaveChangesAsync();

            var authorCreationDTO = new AuthorCreateWithPictureDTO
            {
                Names = "Camila",
                LastNames = "Arenas"
            };


            //Test
            var response = await controller.Put(1, authorCreateWithPictureDTO: authorCreationDTO);
            //Verification

            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result!.StatusCode);

            var context2 = BuildContext(nameDB);
            var updatedAuthor = await context2.Authors.SingleAsync();

            Assert.AreEqual(expected: "Camila", actual: updatedAuthor.Names);
            Assert.AreEqual(expected: "Arenas", actual: updatedAuthor.LastNames);
            await fileStorage.DidNotReceiveWithAnyArgs().Edit(default, default!, default!);
        }

        [TestMethod]
        public async Task Put_UpdateAuthor_WhenSentAuthorWithPicture()
        {
            //preparation
            var context = BuildContext(nameDB);

            string oldUrl = "URL-1";
            var newUrl = "URL-2";
            fileStorage.Edit(default, default!, default!).ReturnsForAnyArgs(newUrl);

            context.Authors.Add(new Author { 
                Names = "Cristian",
                LastNames = "Rojas",
                Identification = "23942",
                Picture = oldUrl 
            });

            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();
            var authorCreationDTO = new AuthorCreateWithPictureDTO
            {
                Names = "Camila",
                LastNames = "Arenas",
                Picture = formFile,
            };


            //Test
            var response = await controller.Put(1, authorCreateWithPictureDTO: authorCreationDTO);
            //Verification

            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result!.StatusCode);

            var context2 = BuildContext(nameDB);
            var updatedAuthor = await context2.Authors.SingleAsync();

            Assert.AreEqual(expected: "Camila", actual: updatedAuthor.Names);
            Assert.AreEqual(expected: "Arenas", actual: updatedAuthor.LastNames);
            Assert.AreEqual(expected: newUrl, actual: updatedAuthor.Picture);
            await fileStorage.Received(1).Edit(oldUrl, container, formFile);
        }

        [TestMethod]
        public async Task Patch_Return400_WhenPatchDocIsNull()
        {

            //Test
            var response = await controller.Patch(1, patchDoc: null!);

            //Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(expected: 400, actual: result!.StatusCode);


        }

        [TestMethod]
        public async Task Patch_Return404_WhenAuthorNoExist()
        {
            //preparation

            var patchDoc = Substitute.For<JsonPatchDocument<AuthorPatchDTO>>();

            //Test
            var response = await controller.Patch(1, patchDoc: patchDoc);

            //Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: result!.StatusCode);

        }

        [TestMethod]
        public async Task Patch_ReturnValidationProblem_WhenThereIsAValidation()
        {
            //preparation
            var context = BuildContext(nameDB);
            context.Authors.Add(new Author
            {
                Names = "Camila",
                LastNames = "Arenas",
                Identification = "349823"
            });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            string errorMessage = "Error message";
            controller.ModelState.AddModelError("", errorMessage);

            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();

            //Test
            var response = await controller.Patch(1, patchDoc: patchDoc);

            //Verification
            var result = response as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails; 
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(
                expected: errorMessage, 
                actual: problemDetails
                        .Errors.Values
                        .First().First());

        }

        [TestMethod]
        public async Task Patch_UpdateAFile_WhenSentOneOperation()
        {
            //preparation
            var context = BuildContext(nameDB);
            context.Authors.Add(new Author
            {
                Names = "Camila",
                LastNames = "Arenas",
                Identification = "349823",
                Picture = "URL-1"
            });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();
            patchDoc.Operations.Add(new Operation<AuthorPatchDTO>("replace", "/names", null, "Camilita2"));

            //Test
            var response = await controller.Patch(1, patchDoc: patchDoc);


            var result = response as StatusCodeResult;
            Assert.AreEqual(expected: 204, result!.StatusCode);

            //await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = BuildContext(nameDB);
            var authorDB = await context2.Authors.SingleAsync();

            Assert.AreEqual(expected: "Camilita2", actual: authorDB.Names);

        }

        [TestMethod]
        public async Task Delete_Return404_WhenAuthorNoExist()
        {
            //test
            var response = await controller.Delete(1);

            //validation
            var result = response as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: result!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Remove_WhenSentAuthorId()
        {
            //preparation
            string urlPicture = "URL-1";
            var context = BuildContext(nameDB);
            context.Authors.Add(new Author
            {
                Names = "Camila",
                LastNames = "Arenas",
                Identification = "349823",
                Picture = urlPicture
            });
            context.Authors.Add(new Author
            {
                Names = "Cristian",
                LastNames = "Rojas",
            });
            await context.SaveChangesAsync();

            //test
            var response = await controller.Delete(1);

            //validation
            var result = response as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: result!.StatusCode);

            var context2 = BuildContext(nameDB);
            var qtyAuthors = await context2.Authors.CountAsync();
            Assert.AreEqual(expected: 1, actual: qtyAuthors);

            var author2Exist = await context2.Authors.AnyAsync(x => x.Names == "Cristian");
            Assert.IsTrue(author2Exist);

            await fileStorage.Received(1).Delete(urlPicture, container);

        }
    }

}
