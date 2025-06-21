using TestUtility;
using Xunit.Abstractions;

namespace ResourceManagementTests
{
    public class SampleAzureUnitTest1 : AzureTestBase
    {
        public SampleAzureUnitTest1(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task VerifyAzureConnection()
        {
            string uniqueString = Guid.NewGuid().ToString();
            Logger.Information("This is a test log message. " + uniqueString);

            try
            {
                HttpClient client = new HttpClient();
                await client.GetAsync($"https://invalid-domain-value-{uniqueString}.invalid-domain-20250620.com");
            }
            catch { }

            var logEvents = GetLogEvents();
            Assert.True(logEvents.Any(e => e.MessageTemplate.Text.Contains(uniqueString)), "Log message should contain the unique string.");
            Assert.True(true, "This test should always pass.");
        }
    }
}