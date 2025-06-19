using Azure.Core;
using Azure.Identity;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit.Abstractions;

namespace TestUtility
{
    public class AzureTestBase : TestBase
    {
        private const string AzureUnitTestSubscriptionId = nameof(AzureUnitTestSubscriptionId);

        public AzureTestBase(ITestOutputHelper output, [CallerFilePath] string sourceFile = "")
            : base(output, useAppInsights: true, sourceFile)
        {
            SubscriptionId = GetUnitTestSubscriptionId();
            TokenCredential = LoadTestCredential();
        }

        public string SubscriptionId { get; }

        public TokenCredential TokenCredential { get; }

        private TokenCredential LoadTestCredential()
        {
            Logger.Information("unit_test_auth_setup. Using az cli credential.");

            var tokenCredential = new AzureCliCredential();

            return tokenCredential;
        }

        private static string GetUnitTestSubscriptionId()
        {
            string subscriptionIdStr = Environment.GetEnvironmentVariable(AzureUnitTestSubscriptionId);

            if (string.IsNullOrEmpty(subscriptionIdStr))
            {
                throw new InvalidOperationException($"Cannot find the subscription id for running the unit tests. It should be set in the environment variable with name {AzureUnitTestSubscriptionId}.");
            }

            if (!Guid.TryParse(subscriptionIdStr, out _))
            {
                throw new InvalidOperationException($"The unit test default subscription id {subscriptionIdStr} is not a valid GUID.");
            }

            return subscriptionIdStr;
        }
    }
}
