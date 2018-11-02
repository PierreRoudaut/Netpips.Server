using System;
using System.IO;
using Netpips.Core;
using NUnit.Framework;

namespace Netpips.Tests.Core
{
    [TestFixture]
    public class OsHelperTests
    {

        [Test]
        [Category(TestCategory.Filesystem)]
        public void DirectoryWritable()
        {
            Assert.IsTrue(FilesystemHelper.IsDirectoryWritable(Path.GetTempPath()));
            Assert.IsFalse(FilesystemHelper.IsDirectoryWritable(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        }
    }
}