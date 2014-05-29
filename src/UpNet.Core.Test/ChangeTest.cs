using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UpNet.Core.Test
{
    [TestClass]
    public class ChangeTest
    {
        [TestMethod]
        public void ChangeCreationTest()
        {
            const FileAction testFileAction = FileAction.Delete;
            const String testDataSource = "TestDataSource";
            const String testLocalPath = "TestLocalPath";
            const String testHash = "abcdefg";

            Change change = new Change(testFileAction, testDataSource, testLocalPath, testHash);

            Assert.AreEqual(testFileAction, change.Action);
            Assert.AreEqual(testDataSource, change.DataSourcePath);
            Assert.AreEqual(testLocalPath, change.RelativePath);
            Assert.AreEqual(testHash, change.Sha1);
        }
    }
}
