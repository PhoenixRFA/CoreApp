using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MVCApp.Services;
using Xunit;

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
            mock.Setup(foo => foo.DoSomething("")).Throws(new ArgumentException("command"));
            Assert.Throws<InvalidOperationException>(()=>mock.Object.DoSomething("reset"));
            //Assert.Throws<ArgumentException>("message",() => mock.Object.DoSomething(""));

            // lazy evaluating return value
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
        }
    }
}
