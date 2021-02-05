using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MVCApp.xUnit.Tests
{
    [Collection("Our Test Collection #1")]
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        #region AsyncExamples

        [Fact]
        public async void CodeThrowsAsync()
        {
            Func<Task> testCode = () => Task.Factory.StartNew(ThrowingMethod);

            var ex = await Assert.ThrowsAsync<NotImplementedException>(testCode);

            Assert.IsType<NotImplementedException>(ex);
        }

        [Fact]
        public async void RecordAsync()
        {
            Func<Task> testCode = () => Task.Factory.StartNew(ThrowingMethod);

            var ex = await Record.ExceptionAsync(testCode);

            Assert.IsType<NotImplementedException>(ex);
        }

        void ThrowingMethod()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region CollectionExamples

        [Fact]
        public void CollectionEquality()
        {
            List<int> left = new List<int>(new int[] { 4, 12, 16, 27 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.Equal(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void LeftCollectionSmallerThanRight()
        {
            List<int> left = new List<int>(new int[] { 4, 12, 16 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.NotEqual(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void LeftCollectionLargerThanRight()
        {
            List<int> left = new List<int>(new int[] { 4, 12, 16, 27, 42 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.NotEqual(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void SameValuesOutOfOrder()
        {
            List<int> left = new List<int>(new int[] { 4, 16, 12, 27 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.Equal(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void DuplicatedItemInOneListOnly()
        {
            List<int> left = new List<int>(new int[] { 4, 16, 12, 27, 4 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.NotEqual(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void DuplicatedItemInBothLists()
        {
            List<int> left = new List<int>(new int[] { 4, 16, 12, 27, 4 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 4, 27 });

            Assert.Equal(left, right, new CollectionEquivalenceComparer<int>());
        }

        #endregion

        #region EqualExample

        [Fact]
        public void EqualStringIgnoreCase()
        {
            string expected = "TestString";
            string actual = "teststring";

            Assert.False(actual == expected);
            Assert.NotEqual(expected, actual);
            Assert.Equal(expected, actual, StringComparer.CurrentCultureIgnoreCase);
        }

        class DateComparer : IEqualityComparer<DateTime>
        {
            public bool Equals(DateTime x, DateTime y)
            {
                return x.Date == y.Date;
            }

            public int GetHashCode(DateTime obj)
            {
                return obj.GetHashCode();
            }
        }

        [Fact]
        public void DateShouldBeEqualEvenThoughTimesAreDifferent()
        {
            DateTime firstTime = DateTime.Now.Date;
            DateTime later = firstTime.AddMinutes(90);

            Assert.NotEqual(firstTime, later);
            Assert.Equal(firstTime, later, new DateComparer());
        }

        #endregion

        #region Theory

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void MyFirstTheory(int value)
        {
            Assert.True(IsOdd(value));
        }

        bool IsOdd(int value)
        {
            return value % 2 == 1;
        }

        #endregion

        [Fact]
        public void CustomOutput()
        {
            output.WriteLine("This is output from {0}", nameof(CustomOutput));
        }

        [Fact]
        public void Asserts()
        {
            Assert.NotNull(new object());
            Assert.Null(null);
            Assert.Empty(new int[0]);

            var arr = new[] { 1, 2, 3, 4, 5 };
            var arr2 = new[] { 1, 2, 3, 4, 5 };
            Assert.Contains(2, arr);
            Assert.Contains(arr, i => i == 2);
            var dict = new Dictionary<int, string>
            {
                {1, "foo"},
                {2, "bar"},
                {3, "foobar"}
            };
            Assert.Contains(2, (IDictionary<int, string>)dict);
            var now = DateTime.Now;
            Assert.Equal(now, DateTime.Now, TimeSpan.FromSeconds(1));
            Assert.Equal(arr, arr2);
            Assert.Equal(3.1415, 3.1416, 2);
            Assert.Equal("foo", "Foo", true);

            Assert.IsType<DateTime>(now);
            Assert.StartsWith("foo", "foobar");

            Assert.All(arr, i => { });
            Assert.Collection(new[] { 1, 2 }, i => { }, j => { });
            Assert.DoesNotContain(0, arr);

            Assert.EndsWith("bar", "foobar");
            Assert.False(1 == 0);
            Assert.IsAssignableFrom<IDictionary>(dict);

            Assert.IsNotType<DateTime>(1);
            Assert.True(1 == 1);
            Assert.Throws<NotImplementedException>(() =>
            {
                throw new NotImplementedException();
                return 1;
            });
            Assert.Same(arr, arr);
        }
    }

    internal class CollectionEquivalenceComparer<T> : IEqualityComparer<IEnumerable<T>>
    where T : IEquatable<T>
    {
        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            List<T> leftList = new List<T>(x);
            List<T> rightList = new List<T>(y);
            leftList.Sort();
            rightList.Sort();

            using IEnumerator<T> enumeratorX = leftList.GetEnumerator();
            using IEnumerator<T> enumeratorY = rightList.GetEnumerator();

            while (true)
            {
                bool hasNextX = enumeratorX.MoveNext();
                bool hasNextY = enumeratorY.MoveNext();

                if (!hasNextX || !hasNextY) return hasNextX == hasNextY;

                if (!enumeratorX.Current.Equals(enumeratorY.Current)) return false;
            }
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            throw new NotImplementedException();
        }
    }
}
