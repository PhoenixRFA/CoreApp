using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MVCApp.Services;
using Xunit;
using Range = Moq.Range;

namespace MVCApp.Tests
{
    public class Moq
    {
        public interface IFoo
        {
            Bar Bar { get; set; }
            string Name { get; set; }
            int Value { get; set; }
            bool DoSomething(string value);
            bool DoSomething(int number, string value);
            Task<bool> DoSomethingAsync();
            string DoSomethingStringy(string value);
            bool TryParse(string value, out string outputValue);
            bool Submit(ref Bar bar);
            int GetCount();
            bool Add(int value);
        }

        public class Bar 
        {
            public virtual Baz Baz { get; set; }
            public virtual bool Submit() => false;
        }

        public class Baz
        {
            public virtual string Name { get; set; }
        }


        [Fact]
        public void Asserts()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(x => x.DoSomething("ping")).Returns(true);
            
            Assert.True(mock.Object.DoSomething("ping"));
            Assert.False(mock.Object.DoSomething("pong"));

            var stub = Mock.Of<IFoo>(x => x.DoSomething("ping") == true);
            Assert.True(stub.DoSomething("ping"));

            //out
            string outString = "ack";
            //TryParse will return true, and the out argument will return "ack", lazy evaluated
            mock.Setup(foo => foo.TryParse("ping", out outString)).Returns(true);
            Assert.True(mock.Object.TryParse("ping", out string out1));
            Assert.Equal("ack", out1);
            Assert.False(mock.Object.TryParse("pong", out string out2));
            Assert.NotEqual("ack", out2);

            // ref arguments
            var instance = new Bar();
            var instance2 = new Bar();
            //Only matches if the ref argument to the invocation is the same instance
            mock.Setup(foo => foo.Submit(ref instance)).Returns(true);
            Assert.True(mock.Object.Submit(ref instance));
            Assert.False(mock.Object.Submit(ref instance2));

            //access invocation arguments when returning a value
            mock.Setup(x => x.DoSomethingStringy(It.IsAny<string>()))
                .Returns((string s) => s.ToLower());
            //Multiple parameters overloads available
            Assert.Same("foo", mock.Object.DoSomethingStringy("foo"));
            Assert.Same("bar", mock.Object.DoSomethingStringy("bar"));

            //throwing when invoked with specific parameters
            mock.Setup(foo => foo.DoSomething("reset")).Throws<InvalidOperationException>();
            mock.Setup(foo => foo.DoSomething("")).Throws(new ArgumentException("", "command"));
            Assert.Throws<InvalidOperationException>(()=>mock.Object.DoSomething("reset"));
            Assert.Throws<ArgumentException>("command",() => mock.Object.DoSomething(""));

            //lazy evaluating return value
            int count = 1;
            mock.Setup(foo => foo.GetCount()).Returns(() => count);
            Assert.Equal(1, mock.Object.GetCount());

            //async methods (see below for more about async):
            mock.Setup(foo => foo.DoSomethingAsync().Result).Returns(true);
            Assert.True(mock.Object.DoSomethingAsync().Result);

            var stub1 = Mock.Of<IRequestStoreService>();
            Assert.Null(stub1.Get(""));

            var stub2 = Mock.Of<IRequestStoreService>(x => x.Get(It.IsAny<string>()) == "foo");
            Assert.Equal("foo", stub2.Get(""));


            //any value
            mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
            Assert.True(mock.Object.DoSomething("asd"));

            //any value passed in a `ref` parameter (requires Moq 4.8 or later):
            mock.Setup(foo => foo.Submit(ref It.Ref<Bar>.IsAny)).Returns(true);
            Assert.True(mock.Object.Submit(ref It.Ref<Bar>.IsAny));

            //matching Func<int>, lazy evaluated
            mock.Setup(foo => foo.Add(It.Is<int>(i => i % 2 == 0))).Returns(true);
            Assert.True(mock.Object.Add(2));
            Assert.False(mock.Object.Add(1));

            //matching ranges
            mock.Setup(foo => foo.Add(It.IsInRange<int>(0, 10, Range.Inclusive))).Returns(true);
            Assert.True(mock.Object.Add(1));
            Assert.False(mock.Object.Add(11));


            mock.Setup(foo => foo.Name).Returns("bar");
            Assert.Equal("bar", mock.Object.Name);

            //auto-mocking hierarchies (a.k.a. recursive mocks)
            mock.Setup(foo => foo.Bar.Baz.Name).Returns("baz");
            Assert.Equal("baz", mock.Object.Bar.Baz.Name);

            //expects an invocation to set the value to "foo"
            mock.SetupSet(foo => foo.Name = "foo");
            mock.Object.Name = "foo";
            //verify the setter directly
            mock.VerifySet(foo => foo.Name = "foo");


            //alternatively, provide a default value for the stubbed property
            mock.SetupProperty(f => f.Name, "foo");

            //Now you can do:
            IFoo foo = mock.Object;
            //Initial value was stored
            Assert.Equal("foo", foo.Name);

            //New value set which changes the initial value
            foo.Name = "bar";
            Assert.Equal("bar", foo.Name);
        }
    }
}
