using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

using Xunit;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using test02.Models;

namespace DoNothing.Tests
{
    public class DataAccessLayerTests
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _testOutput;

        public DataAccessLayerTests(Xunit.Abstractions.ITestOutputHelper _testOutput)
        {
            this._testOutput = _testOutput;
        }

        public string CurrentDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
            string path = CurrentDir;
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

        [Trait("DataAccessLayer", "GetAblumArt Ext2")]
        [Fact]
        public void GetAlbumArtTestExt2()
        {
            string path = CurrentDir;
            string dirName = @"testDir";

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Path.Combine(path, dirName));
            string expected = dir.FullName + @"\cover.jpg";

            if (dir.Exists)
            {
                dir.Delete();
            }

            dir.Create();

            try
            {
                using (FileStream fs = File.Create(dir.FullName + @"\cover.jpg"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("test");
                    fs.Write(info, 0, info.Length);
                }
            } catch (Exception e)
            {
                _testOutput.WriteLine("File creation failed with exception: " + e.ToString());
            }

            var comp = new DataAccessLayer();

            var result = comp.GetAlbumArt(dir.FullName);

            dir.Delete(true);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> GetAlbumArtFileData =>
            new List<object[]>
            {
                new object[] { @"\cover.jpg", System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).EndsWith("cover.jpg") },
                new object[] { new TimeSpan(1,8,7), "1:08:07"}
            };
    }
}