using System;
using System.Collections.Generic;
using Xunit;
using test02.Models;

namespace DoNothing.Tests
{
    public class DataAccessLayerTests
    {
        /// <summary>
        /// ConvertTime takes a TimeSpan object and converts it into a string representation.
        /// Depending on the context we append a "0" to part of the string 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="expected"></param>
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
                new object[] { new TimeSpan(1,33,7), "1:33:07" },
                new object[] { new TimeSpan(0,0,8), "0:08" },
                new object[] { new TimeSpan(1,8,7), "1:08:07"}
            };

        [Trait("DataAccessLayer", "GetAlbumArt simple")]
        [Theory]
        [InlineData(@"testDir", null)]
        public void GetAlbumArtTest(string folder, string expected)
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folder);
            Assert.Equal(dir.Exists.ToString(), false.ToString());

            var comp = new DataAccessLayer();

            var result = comp.GetAlbumArt(folder);

            Assert.Equal(expected, result);
        }
    }
}