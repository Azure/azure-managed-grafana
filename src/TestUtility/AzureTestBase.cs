using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Resources;
using System;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;
using Azure.ResourceManager;
using System.Collections.Generic;

namespace TestUtility
{
    public class AzureTestBase : TestBase
    {
        private static readonly Random s_rand = new Random();

        private const string AzureUnitTestSubscriptionId = nameof(AzureUnitTestSubscriptionId);

        public AzureTestBase(ITestOutputHelper output, bool createResourceGroup = true, [CallerFilePath] string sourceFile = "")
            : base(output, useAppInsights: true, sourceFile)
        {
            SubscriptionId = GetUnitTestSubscriptionId();

            TokenCredential = LoadTestCredential();

            ArmClient = new ArmClient(TokenCredential);

            DefaultSubscription = ArmClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(SubscriptionId));

            ResourceGroupName = $"{TestClassName}-{TestLocation.Name}-{DateTimeStr}-{s_rand.Next(0, 999)}";

            if (createResourceGroup)
            {
                try
                {
                    ResourceGroupCollection resourceGroups = DefaultSubscription.GetResourceGroups();

                    ResourceGroupData resourceGroupData = new ResourceGroupData(TestLocation);
                    resourceGroupData.AppendTags(Tags);

                    var operation = resourceGroups.CreateOrUpdate(WaitUntil.Completed, ResourceGroupName, resourceGroupData);

                    TestResourceGroup = operation.Value;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "err_create_rg");
                    throw new InvalidOperationException($"Failed at creating testing resource group with name '{ResourceGroupName}' in subscription '{SubscriptionId}'. There might be testing credential issues. Please ensure you have done 'az login' before.", ex);
                }
            }
        }

        public ArmClient ArmClient { get; }

        public string SubscriptionId { get; }

        public SubscriptionResource DefaultSubscription { get; }

        public AzureLocation TestLocation => AzureLocation.WestUS2;

        public string ResourceGroupName { get; }

        public ResourceGroupResource TestResourceGroup { get; }

        public TokenCredential TokenCredential { get; }

        public Dictionary<string, string> Tags { get; } = new Dictionary<string, string>(TestCommon.Tags);

        public override void Dispose()
        {
            bool deleteResourceGroup = true;

            if (TestResourceGroup == null)
            {
                deleteResourceGroup = false;
            }
            else if (IsFailure == true)
            {
                // set this to false if we want to keep the resource group for failed test investigation.
                deleteResourceGroup = true;
            }

            if (deleteResourceGroup)
            {
                try
                {
                    TestResourceGroup.DeleteAsync(WaitUntil.Started).GetAwaiter().GetResult();
                }
                catch
                {
                }
            }

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        private TokenCredential LoadTestCredential()
        {
            Logger.Information("unit_test_auth_setup. Using az cli credential.");

            var tokenCredential = new AzureCliCredential();

            return tokenCredential;
        }

        private string GetUnitTestSubscriptionId()
        {
            string subscriptionIdStr = Environment.GetEnvironmentVariable(AzureUnitTestSubscriptionId);

            if (string.IsNullOrEmpty(subscriptionIdStr))
            {
                if (IsGithubAction)
                {
                    subscriptionIdStr = "d320f99c-3d38-41c8-89d6-021f326613b8";
                }
                else
                {
                    throw new InvalidOperationException($"Cannot find the subscription id for running the unit tests. It should be set in the environment variable with name {AzureUnitTestSubscriptionId}.");
                }
            }

            if (!Guid.TryParse(subscriptionIdStr, out _))
            {
                throw new InvalidOperationException($"The unit test default subscription id {subscriptionIdStr} is not a valid GUID.");
            }

            return subscriptionIdStr;
        }
    }
}
