using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVCApp.MSTest.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var foo = new object();
            var foo2 = foo;
            var bar = new object();

            Assert.AreEqual(11, 11);
            Assert.AreNotEqual(11, 12);
            Assert.AreSame(foo, foo2);
            Assert.AreNotSame(foo, bar);
            Assert.IsFalse(1 == 0, "1 == 0");
            Assert.IsNull(null);
            Assert.IsTrue(1 == 1, "1 == 1");
            Assert.ThrowsException<NotImplementedException>(() => throw new NotImplementedException());
            Assert.IsNotNull(1);
        }

        [DataTestMethod]
        [DataRow(-2)]
        [DataRow(0)]
        [DataRow(2)]
        [DataRow(3)]
        public void IsOdd(int value)
        {
            bool result = value % 2 == 0;

            Assert.IsFalse(result, $"{value} should be odd");
        }
    }
}
