using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MVCApp.xUnit.Tests
{
    public class UnitTest1
    {
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
        [InlineData(6)]
        public void MyFirstTheory(int value)
        {
            Assert.True(IsOdd(value));
        }

        bool IsOdd(int value)
        {
            return value % 2 == 1;
        }

        #endregion
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

            IEnumerator<T> enumeratorX = leftList.GetEnumerator();
            IEnumerator<T> enumeratorY = rightList.GetEnumerator();

            while (true)
            {
                bool hasNextX = enumeratorX.MoveNext();
                bool hasNextY = enumeratorY.MoveNext();

                if (!hasNextX || !hasNextY)
                    return (hasNextX == hasNextY);

                if (!enumeratorX.Current.Equals(enumeratorY.Current))
                    return false;
            }
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            throw new NotImplementedException();
        }
    }
}
