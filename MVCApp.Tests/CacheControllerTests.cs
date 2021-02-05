using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCApp.Controllers;
using MVCApp.Models;
using MVCApp.Services;
using Xunit;

namespace MVCApp.Tests
{
    public class CacheControllerTests
    {
        [Fact]
        public void GetTime1ReturnsNonNullResult()
        {
            // Arrange
            var controller = new CacheController(null, null);

            // Act
            IActionResult res = controller.GetTime1();

            // Assert
            Assert.NotNull(res);
            Assert.NotNull((res as ContentResult)?.Content);
        }


        [Fact]
        public void GetDataReturnUser()
        {
            // Arrange
            var mock = new Mock<IUsersService>();
            bool outVal = false;
            mock.Setup(x => x.GetUser(1, out outVal)).Returns(new User
            {
                Id = 1
            });
            var controller = new CacheController(mock.Object, null);

            // Act
            IActionResult res = controller.GetData(1);

            // Assert
            var viewRes = Assert.IsType<ViewResult>(res);
            Assert.NotNull(viewRes.Model);
            var model = Assert.IsType<CacheGetDataModel>(viewRes.Model);
            Assert.Equal(1, model.User?.Id);
        }

        [Fact]
        public void GetDataReturnBadRequestOnBadId()
        {
            //Arrange
            var controller = new CacheController(null, null);

            // Act
            IActionResult res = controller.GetData(-1);

            // Assert
            var concreteRes = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(concreteRes.Value);
        }

        [Fact]
        public void GetDataReturnNotFoundIfUserNotExists()
        {
            //Arrange
            var mock = new Mock<IUsersService>();
            bool outVal = false;
            mock.Setup(x => x.GetUser(1, out outVal)).Returns((User)null);
            var controller = new CacheController(mock.Object, null);

            // Act
            IActionResult res = controller.GetData(1);

            // Assert
            var concreteRes = Assert.IsType<NotFoundObjectResult>(res);
            Assert.NotNull(concreteRes.Value);
        }
    }
}
