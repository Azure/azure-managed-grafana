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
        public async Task Test1()
        {
            string uniqueString = Guid.NewGuid().ToString();
            Logger.Information("This is a test log message. " + uniqueString);

            try
            {
                HttpClient client = new HttpClient();
                await client.GetAsync($"https://invalid-domain-value-{uniqueString}.com");
            }
            catch { }

            var logEvents = GetLogEvents();
            Assert.True(logEvents.Any(e => e.MessageTemplate.Text.Contains(uniqueString)), "Log message should contain the unique string.");
            Assert.True(true, "This test should always pass.");
        }
    }
}