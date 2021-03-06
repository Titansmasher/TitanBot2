﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TitanBotBaseTest.Helpers;

namespace TitanBotBaseTest.Tests.UtilTests
{
    [TestClass]
    public class FileUtilTests
    {
        [TestMethod]
        public void EnsureDirectoryCreate()
        {
            var path = FilesAndFolders.TestFolderPath;
            var pathDir = Path.GetDirectoryName(path);

            // Ensure that the file does not exist
            Assert.IsFalse(Directory.Exists(pathDir));

            FileUtil.EnsureDirectory(path);
            Assert.IsTrue(Directory.Exists(pathDir));

            // Cleanup
            Assert.IsTrue(FilesAndFolders.DeleteTestFolder());
        }
    }
}
