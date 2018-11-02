using System.IO;
using Netpips.Core;
using NUnit.Framework;

namespace Netpips.Tests.Core
{
    [TestFixture]
    public class FilesystemHelperTests
    {
        [TestCase(".srt", null)]
        [TestCase(".en.srt", null)]
        [TestCase(".eng.srt", ".en.srt")]
        [TestCase(".fr.srt", null)]
        [TestCase(".fra.srt", ".fr.srt")]
        public void ConvertToTwoLetterIsoLanguageNameSubtitleTest(string subExt, string expectedResultEndsWith)
        {
            var srtFilename = "the.big.bang.theory.s10.e01";
            var srtBasePath = Path.Combine(Path.GetTempPath(), TestHelper.Uid(), srtFilename);

            var res = FilesystemHelper.ConvertToTwoLetterIsoLanguageNameSubtitle(srtBasePath + subExt);
            if (expectedResultEndsWith == null)
            {
                Assert.IsNull(res);
            }
            else
            {
                Assert.AreEqual(srtBasePath + expectedResultEndsWith, res);
            }
        }
    }
}