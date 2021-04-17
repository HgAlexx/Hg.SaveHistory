using System;
using Hg.SaveHistory.API;
using NUnit.Framework;

namespace Tests.API
{
    [TestFixture]
    public class HgConverterTests
    {
        [Test]
        public void DateTimeToUnixTest()
        {
            ulong value = 1578960000;
            DateTime input = new DateTime(2020, 1, 14, 0, 0, 0, DateTimeKind.Utc);
            long output = HgConverter.DateTimeToUnix(input);
            Assert.AreEqual(output, value);
        }

        [Test]
        public void StringToGuidTest()
        {
            string input = Guid.NewGuid().ToString();
            Guid? output = HgConverter.StringToGuid(input);
            Assert.AreEqual(input, output.ToString());
        }

        [Test]
        public void UnixToDateTimeTest()
        {
            DateTime value = new DateTime(2020, 1, 14, 0, 0, 0, DateTimeKind.Utc);
            long input = 1578960000;
            DateTime output = HgConverter.UnixToDateTime(input);
            Assert.AreEqual(value, output.ToUniversalTime());
        }
    }
}