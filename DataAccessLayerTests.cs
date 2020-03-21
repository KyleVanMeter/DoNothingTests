using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

using Xunit;
using TagLib;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using test02.Models;

namespace DoNothing.Tests
{
    public static class Utility
    {
        public static void Fail(string msg) =>
            throw new Xunit.Sdk.XunitException(msg);
    }
    public class MockFileInfo
    {
        public MockFileInfo(string fn, string mc)
        {
            fileName = fn;
            mockContents = mc;
        }
        public string fileName { get; set; }
        public string mockContents { get; set; }
    }

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
        [Trait("DataAccessLayer", "ConvertTime")]
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
        [Trait("DataAccessLayer", "GetAlbumArt")]
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

        /// <summary>
        /// This unit test is meant to test if GetAlbumArt is finding the right types of files
        /// with the right names, and prioritizing selecting files with larger file sizes for
        /// cover art
        /// </summary>
        /// <param name="fileInfos"></param>
        /// <param name="expected"></param>
        [Trait("DataAccessLayer", "GetAlbumArt")]
        [Theory]
        [MemberData(nameof(GetAlbumArtFileData))]
        public void GetAlbumArtParamExtTest(List<MockFileInfo> fileInfos, string expected)
        {
            string path = CurrentDir;
            string dirName = @"testDir";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Path.Combine(path, dirName));

            if(dir.Exists)
            {
                dir.Delete();
            }

            dir.Create();

            try
            {
                foreach(MockFileInfo mockFile in fileInfos)
                {
                    using(FileStream fs = System.IO.File.Create(dir.FullName + @"\" + mockFile.fileName))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes(mockFile.mockContents);
                        fs.Write(info, 0, info.Length);
                        _testOutput.WriteLine(mockFile.fileName + ": " + info.Length.ToString());
                    }
                }
            } catch (Exception ex)
            {
                _testOutput.WriteLine("File creation failed with expection: " + ex.ToString());
            }

            var comp = new DataAccessLayer();

            var result = comp.GetAlbumArt(dir.FullName);

            dir.Delete(true);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> GetAlbumArtFileData =>
            new List<object[]>
            {
                new object[] { new List<MockFileInfo>() { new MockFileInfo("cover.jpg", "aaaa") }, 
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\testDir\cover.jpg" },
                new object[] { new List<MockFileInfo>() { new MockFileInfo("no.txt", "notext"),
                                                          new MockFileInfo("no.jpg", "wrongfile"),
                                                          new MockFileInfo("cover.jpg", "a") },
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\testDir\cover.jpg" },
                new object[] { new List<MockFileInfo>() { new MockFileInfo("no.txt", "notext"),
                                                          new MockFileInfo("cover.png", "yesfileaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                                                          new MockFileInfo("cover.gif", "") },
                     System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\testDir\cover.png" },
                new object[] { new List<MockFileInfo>() { },
                    null }
            };

        /// <summary>
        /// This unit test is meant to see if we can find base64 encoded album art
        /// in a file.  No file mocking is done at the moment, so a valid .mp3 file
        /// must be present
        /// </summary>
        [Trait("DataAccessLayer", "GetEmbedAlbumArt")]
        [Fact]
        public void GetEmbedAlbumArtTest()
        {
            string fileName = @"\embeddedart.mp3";
            string path = CurrentDir + fileName;
            if (!System.IO.File.Exists(path))
            {
                Utility.Fail($"Cannot find file \"{fileName}\"");
            }

            TagLib.File file = TagLib.File.Create(path);

            TagLib.IPicture[] pics = file.Tag.Pictures;
            Assert.True(pics.Length > 0 ? true : false);
        }
    }
}