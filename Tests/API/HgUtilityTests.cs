using System;
using System.IO;
using Hg.SaveHistory.Utilities;
using NUnit.Framework;

namespace Hg.SaveHistory.API.Tests
{
    [TestFixture]
    public class HgUtilityTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Utilities.Logger.Level = LogLevel.None;
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data", @"HgUtility");

            Directory.Delete(path, true);
        }

        [Test]
        public void HashFileTest()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Data", @"HgUtility", @"HashFile.txt");
            string hash = HgUtility.HashFile(path);

            Assert.AreEqual(@"2e99758548972a8e8822ad47fa1017ff72f06f3ff6a016851f45c398732bc50c", hash);
        }

        [Test]
        public void HashStringTest()
        {
            string hash = HgUtility.HashString(@"this is a test");

            Assert.AreEqual(@"2e99758548972a8e8822ad47fa1017ff72f06f3ff6a016851f45c398732bc50c", hash);
        }

        [Test]
        public void IsValidFileNameTest()
        {
            //Assert.Fail();
        }

        [Test]
        public void IsValidPathTest()
        {
            //Assert.Fail();
        }

        [Test]
        public void SafeFileNameTest()
        {
            //Assert.Fail();
        }

        [Test]
        public void SafePathTest()
        {
            //Assert.Fail();
        }

        [Test]
        public void SleepTest()
        {
            //Assert.Fail();
        }

        [Test]
        public void StringEndsWithTest()
        {
            string input = @"this is a test";

            Assert.IsTrue(HgUtility.StringEndsWith(input, @"test"));
            Assert.IsFalse(HgUtility.StringEndsWith(input, @"plop"));
        }

        [Test]
        public void StringSplitTest()
        {
            string input = @"this is a test";

            string[] values = HgUtility.StringSplit(input, " ", StringSplitOptions.None);
            Assert.AreEqual(4, values.Length);

            input = @"this,is,another,test";
            values = HgUtility.StringSplit(input, ",", StringSplitOptions.None);
            Assert.AreEqual(4, values.Length);

            input = @"this,is,,another,test";
            values = HgUtility.StringSplit(input, ",", StringSplitOptions.None);
            Assert.AreEqual(5, values.Length);

            input = @"this,is,,another,test";
            values = HgUtility.StringSplit(input, ",", StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(4, values.Length);

            input = @"this is again, a test";
            values = HgUtility.StringSplit(input, " ,", StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(5, values.Length);
        }

        [Test]
        public void StringStartsWithTest()
        {
            string input = @"this is a test";

            Assert.IsTrue(HgUtility.StringStartsWith(input, @"this"));
            Assert.IsFalse(HgUtility.StringStartsWith(input, @"plop"));
        }

        [Test]
        public void StringTrimTest()
        {
            string input = @" this is a test ";
            string value = HgUtility.StringTrim(input);

            Assert.AreEqual(@"this is a test", value);

            input = @"   this is a test   ";
            value = HgUtility.StringTrim(input);

            Assert.AreEqual(@"this is a test", value);
        }
    }
}