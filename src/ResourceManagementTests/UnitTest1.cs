using TestUtility;
using Xunit.Abstractions;

namespace ResourceManagementTests
{
    public class UnitTest1 : TestBase
    {
        public UnitTest1(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void Test1()
        {
            string uniqueString = Guid.NewGuid().ToString();
            Logger.Information("This is a test log message. " + uniqueString);
            var logEvents = GetLogEvents();
            Assert.True(logEvents.Any(e => e.MessageTemplate.Text.Contains(uniqueString)), "Log message should contain the unique string.");
            Assert.True(true, "This test should always pass.");
        }
    }
}