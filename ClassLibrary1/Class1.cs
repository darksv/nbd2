using System;
using Xunit;

namespace ClassLibrary1
{
    public class Class1
    {
        [Fact]
        public void Test()
        {
            Assert.Equal(4, 2 + 2);
        }
    }
}