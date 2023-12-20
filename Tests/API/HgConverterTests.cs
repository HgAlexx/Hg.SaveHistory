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
            long value = 1578960000;
            DateTime input = new DateTime(2020, 1, 14, 0, 0, 0, DateTimeKind.Utc);
            long output = HgConverter.DateTimeToUnix(input);
            Assert.That(output == value);
        }

        [Test]
        public void StringToGuidTest()
        {
            string input = Guid.NewGuid().ToString();
            Guid? output = HgConverter.StringToGuid(input);
            Assert.That(input == output.ToString());
        }

        [Test]
        public void UnixToDateTimeTest()
        {
            DateTime value = new DateTime(2020, 1, 14, 0, 0, 0, DateTimeKind.Utc);
            long input = 1578960000;
            DateTime output = HgConverter.UnixToDateTime(input);
            Assert.That(value == output.ToUniversalTime());
        }
    }
}