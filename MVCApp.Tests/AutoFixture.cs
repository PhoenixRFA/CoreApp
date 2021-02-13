using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCApp.Controllers;
using MVCApp.Models;
using MVCApp.Services;
using Xunit;

namespace MVCApp.Tests
{
    public class AutoFixture
    {
        [Fact]
        public void Test1()
        {
            //Creates object with random values
            var simple = new Fixture().Create<Foo>();
            //Creates object with concrete data and all other random
            var difined = new Fixture().Build<Foo>()
                .With(x=>x.DecimalProp, 3.1415m)
                .Without(x=>x.BarProp)
                .Create();

            Assert.NotNull(simple);
        }

        //Autogetenerate params
        [Theory, AutoData]
        public void Test2(int val)
        {
            Assert.InRange(val, 1, int.MaxValue);
        }

        [Theory]
        [InlineAutoData("1")]//setup first parm and second is random
        [InlineAutoData("3", "5")]//setup first and second params
        public void Test4(int val, int varr)
        {
            Assert.InRange(val, 1, int.MaxValue);
        }

        [Theory, AutoData]//setup property but with defaul values
        public void Test3([NoAutoProperties]Foo val)
        {
            Assert.NotNull(val);
        }

        [Fact]
        public void Test5()
        {
            var fixture = new Fixture();

            fixture.Freeze<Foo>();

            var value1 = fixture.Create<Foo>();
            var value2 = fixture.Create<Foo>();

            Assert.Equal(value2, value1);
        }

        [Fact]
        public void Test6()
        {
            var fixture = new Fixture();

            fixture.Inject(42);

            int value = fixture.Create<int>();
            
            Assert.Equal(42, value);
        }

        [Theory, AutoData]
        public void Test8(Mock<IUsersService> mock)
        {
            // Arrange
            var valid = new Fixture().Build<User>()
                .With(x=>x.Id, 1)
                .Without(x => x.Company)
                .Without(x => x.LanguageUsers)
                .Without(x => x.UserData)
                .Without(x => x.Role)
                .Create();
            bool outVal = false;
            mock.Setup(x => x.GetUser(1, out outVal)).Returns(valid);
            var controller = new CacheController(mock.Object, null);

            // Act
            IActionResult res = controller.GetData(1);

            // Assert
            var viewRes = Assert.IsType<ViewResult>(res);
            Assert.NotNull(viewRes.Model);
            var model = Assert.IsType<CacheGetDataModel>(viewRes.Model);
            Assert.Equal(1, model.User?.Id);
        }

        [Theory, AutoMoqData]
        public void Test7(Mock<IUsersService> mock)
        {
            // Arrange
            var valid = new Fixture().Build<User>()
                .With(x=>x.Id, 1)
                .Without(x => x.Company)
                .Without(x => x.LanguageUsers)
                .Without(x => x.UserData)
                .Without(x => x.Role)
                .Create();
            bool outVal = false;
            mock.Setup(x => x.GetUser(1, out outVal)).Returns(valid);
            var controller = new CacheController(mock.Object, null);

            // Act
            IActionResult res = controller.GetData(1);

            // Assert
            var viewRes = Assert.IsType<ViewResult>(res);
            Assert.NotNull(viewRes.Model);
            var model = Assert.IsType<CacheGetDataModel>(viewRes.Model);
            Assert.Equal(1, model.User?.Id);
        }
    }

    public class Foo
    {
        public string SringProp { get; set; }
        public int IntProp { get; set; }
        public decimal DecimalProp { get; set; }
        public DateTime DateProp { get; set; }
        public Bar BarProp { get; set; }

        public string GetString(string s) => s;
        public int GetInt() => 42;
    }

    public class Bar
    {
        public string S { get; set; }
    }

    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().Customize(new AutoMoqCustomization())) { }
        /*
         * AutoMoqCustomization это одна из настроек, которая «учит» AutoFixture делать простую вещь:
         * если ни одним из способов не удалось создать требуемый экземпляр и тип объекта является
         * либо интерфейсом, либо абстрактным классом, то AutoFixture создает мок, используя Moq.
         * Звучит классно, создаем сервис через AutoFixture и все его зависимости уже проинициализированы.
         *
         * Иногда моки нужно настраивать и проверять, что там было вызвано. Где взять экземпляр мока собственно?
         * Использовать freeze - тогда всегда будет возвращаться один и тот же объект
         */
    }
}
