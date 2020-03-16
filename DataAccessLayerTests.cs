using System;
using System.Collections.Generic;
using Xunit;
using test02.Models;

namespace DoNothing.Tests
{
    public class DataAccessLayerTests
    {
        [Trait("DataAccessLayer", "ConvertTime Simple")]
        [Theory]
        [MemberData(nameof(TimeSpanData))]
        public void ConvertTimeTest(TimeSpan time, string expected)
        {
            var comp = new DataAccessLayer();

            var result = comp.ConvertTime(time);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> TimeSpanData =>
            new List<object[]>
            {
                new object[] { new TimeSpan(0,12,13), "12:13" },
                new object[] { new TimeSpan(0,3,10), "3:10" },
                new object[] { new TimeSpan(0,16,1), "16:01" },
                new object[] { new TimeSpan(1,33,7), "1:33:07" }
            };
    }
}