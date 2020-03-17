using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Xunit;
using System.IO.Abstractions;
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

        /// <summary>
        /// GetAlbumArt takes a directory, looks for files with usable file extentions,
        /// and returns the full file path to the largest image file in the directory
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="expected"></param>
        [Trait("DataAccessLayer", "GetAlbumArt simple")]
        [Theory]
        [InlineData(@"testDir", null)]
        [InlineData(null, null)]
        public void GetAlbumArtTest(string folder, string expected)
        {
            if (!(folder is null))
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folder);
                Assert.Equal(dir.Exists.ToString(), false.ToString());
            }

            var comp = new DataAccessLayer();

            var result = comp.GetAlbumArt(folder);

            Assert.Equal(expected, result);
        }

        [Trait("DataAccessLayer", "GetAlbumArt ext")]
        [Theory]
        [InlineData("unused", null)]
        public void GetAlbumArtTestExt(string folder, string expected)
        {
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dirName = @"testDir";

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Path.Combine(path, dirName));
            if (dir.Exists)
            {
                dir.Delete();
            }

            dir.Create();

            var comp = new DataAccessLayer();

            var result = comp.GetAlbumArt(Path.Combine(path, dirName));
            dir.Delete();

            Assert.Equal(expected, result);
        }
    }
}