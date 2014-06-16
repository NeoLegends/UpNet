using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UpNet.Core.DataSource;

namespace UpNet.Core.Test
{
    [TestClass]
    public class UpdateTest
    {
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public Task TestApplying()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestDataSourceSetter()
        {
            Update update = this.CreateUpdate();
            Uri serverUri = new Uri("http://test.com/");

            try
            {
                update.DataSource = null;
            }
            catch (ArgumentNullException)
            {

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }

            try
            {
                update.DataSource = new HttpDataSource(serverUri, "test.xml");
                update.DataSource = new HttpDataSource(serverUri, "test2.xml");
            }
            catch (ArgumentNullException)
            {

            }
            catch (InvalidOperationException)
            {

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }

            Assert.IsNotNull(update.DataSource);
            Assert.AreEqual(((HttpDataSource)update.DataSource).ServerUri, serverUri);
            Assert.AreEqual(((HttpDataSource)update.DataSource).UpdateFileName, "test.xml");
        }

        [TestMethod]
        public void TestSerialization()
        {
            Update update = this.CreateUpdate();
            using (FileStream fs = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Update.xml")))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Update));
                serializer.WriteObject(fs, update);

                fs.Position = 0;

                Update deserializedUpdate = (Update)serializer.ReadObject(fs);
                Assert.AreEqual(deserializedUpdate.Patches.Count(), update.Patches.Count());
                Assert.AreEqual(deserializedUpdate.LatestVersion, update.LatestVersion);
                for (int i = 0; i < update.Patches.Count(); i++)
                {
                    Assert.AreEqual(deserializedUpdate.ElementAt(i).Count, update.ElementAt(i).Count);
                }
            }
        }

        private Update CreateUpdate()
        {
            return new Update(
                new[] { 
                    new Patch(
                        new Change[] { 
                            new AddOrReplaceChange("TestFile1", "/path/to/TestFile1", "asdfjklasdfja"),
                            new AddOrReplaceChange("TestFile2", "/path/to/TestFile2", "asdfjklasdfja"),
                            new DeleteChange("/path/to/TestFile3")
                        },
                        new UserMeta(DateTime.Now, "This is a test release!"),
                        new Version(1, 0, 3, 3534)
                    ),
                    new Patch(
                        new Change[] {
                            new DeleteChange("/path/to/TestFile1"),
                            new AddOrReplaceChange("TestFile4", "/path/to/TestFile4", "abcdefghijklmnop"),
                            new AddOrReplaceChange("TestFile5", "/path/to/TestFile5", "abcdefghijklmnop"),
                            new AddOrReplaceChange("TestFile6", "/path/to/TestFile6", "abcdefghijklmnop"),
                            new AddOrReplaceChange("TestFile7", "/path/to/TestFile7", "abcdefghijklmnop")
                        },
                        new UserMeta(DateTime.Now, "This is a test release, too!"),
                        new Version(1, 1, 0, 2344)
                    )
                }
            );
        }
    }
}
