using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UpNet.Core.Test
{
    [TestClass]
    public class UpdateTest
    {
        [TestMethod]
        public void TestUpdateCreation()
        {
            try
            {
                new Update(
                    new[] { 
                        new Patch(
                            new[] { 
                                new Change(FileAction.AddOrReplace, "TestFile1", "/path/to/TestFile1", "abcdefghijklm"),
                                new Change(FileAction.AddOrReplace, "TestFile2", "/path/to/TestFile2", "asdfjklasdfja"),
                                new Change(FileAction.AddOrReplace, "TestFile3", "/path/to/TestFile3", "asdfjklasdfja")
                            },
                            new UserMeta(DateTime.Now, "This is a test release!"),
                            new Version(1, 0, 3, 3534)
                        ),
                        new Patch(
                            new[] { 
                                new Change(FileAction.Delete, "TestFile1", "/path/to/TestFile1", "abcdefghijklm"),
                                new Change(FileAction.AddOrReplace, "TestFile2", "/path/to/TestFile2", "asdfjklasdfja"),
                                new Change(FileAction.AddOrReplace, "TestFile3", "/path/to/TestFile3", "asdfjklasdfja")
                            },
                            new UserMeta(DateTime.Now, "This is a test release, too!"),
                            new Version(1, 0, 7, 6754)
                        ),
                    }
                );
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    String.Format(
                        "An exception occured: '{0}'",
                        ex.ToString()
                    ),
                    ex
                );
            }
        }

        [TestMethod]
        public void TestUpdateSerialization()
        {
            try
            {
                Update update = new Update(
                    new[] { 
                        new Patch(
                            new[] { 
                                new Change(FileAction.AddOrReplace, "TestFile1", "/path/to/TestFile1", "abcdefghijklm"),
                                new Change(FileAction.AddOrReplace, "TestFile2", "/path/to/TestFile2", "asdfjklasdfja"),
                                new Change(FileAction.AddOrReplace, "TestFile3", "/path/to/TestFile3", "asdfjklasdfja")
                            },
                            new UserMeta(DateTime.Now, "This is a test release!"),
                            new Version(1, 0, 3, 3534)
                        ),
                        new Patch(
                            new[] { 
                                new Change(FileAction.Delete, "TestFile1", "/path/to/TestFile1", "abcdefghijklm"),
                                new Change(FileAction.AddOrReplace, "TestFile2", "/path/to/TestFile2", "asdfjklasdfja"),
                                new Change(FileAction.AddOrReplace, "TestFile3", "/path/to/TestFile3", "asdfjklasdfja")
                            },
                            new UserMeta(DateTime.Now, "This is a test release, too!"),
                            new Version(1, 0, 7, 6754)
                        ),
                    }
                );

                String json = JsonConvert.SerializeObject(update, Formatting.Indented);
                try
                {
                    System.IO.File.WriteAllText(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Update.json"),
                        json
                    );
                }
                catch { }

                try
                {
                    JContainer.Parse(json);
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        "The json was probably malformatted! See: {0}.",
                        ex.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    String.Format(
                        "An exception occured: '{1}'",
                        ex.GetType().AssemblyQualifiedName,
                        ex.ToString()
                    ),
                    ex
                );
            }
        }
    }
}
