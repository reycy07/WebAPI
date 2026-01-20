using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Controllers.V1;
using WebAPI.DTOs;
using WebApiTests.Utilities;

namespace WebApiTests.UnitTests.Controllers.V1
{
    [TestClass]
    public class BooksControllerTests: BaseTest
    {
        [TestMethod]
        public async Task Get_ReturnZeroBooks_WhenThereAreNoBooks()
        {
            //preparation
            var nameDB = Guid.NewGuid().ToString();
            var context = BuildContext(nameDB);
            var mapper = ConfigAutoMapper();
            IOutputCacheStore outputCacheStore = null!;

            var controller = new BooksController(context, mapper);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var paginationDTO = new PaginationDTO(1, 1);


            //test

            var response = await controller.Get(paginationDTO);

            //validation

            Assert.AreEqual(expected: 0, actual: response.Count());
        }
    }
}
