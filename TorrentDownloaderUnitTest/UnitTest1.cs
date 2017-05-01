using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TorrentDownloaderUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string s = "A.Few.Good.Men.1992.720p.BDRip.x264.Hindi.Audio - Lesnar 10.00 in Dubbed /Dual Audio, by EtLesnar".ToUpper();
            string key = "x men";

            Regex regex = new Regex("x men".ToUpper());
            var match = regex.IsMatch(s);
            Assert.IsTrue(match);
        }
    }
}
