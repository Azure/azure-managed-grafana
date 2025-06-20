using Azure.ResourceManager.Grafana;
using Azure.ResourceManager.Models;
using Azure.ResourceManager;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TestUtility;
using Xunit.Abstractions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Azure.ResourceManager.ManagedServiceIdentities;
using Azure.Core;

namespace ResourceManagementTests
{
    public class GrafanaCreateTest : AzureTestBase
    {
        public GrafanaCreateTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task StandardSku()
        {
            try
            {
                string grafanaName = RandomNameGenerator.GenerateGrafanaName();

                var grafanaData = new ManagedGrafanaData(TestLocation)
                {
                    // “Standard” is the only production SKU today
                    SkuName = "Standard",

                    // Enable System-Assigned Managed Identity (optional but typical)
                    Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)
                };

                ManagedGrafanaCollection grafanas = TestResourceGroup.GetManagedGrafanas();

                Logger.Information("Creating Managed Grafana with name '{GrafanaName}' in resource group '{ResourceGroupName}' in location '{Location}'.", grafanaName, TestResourceGroup.Data.Name, TestLocation);

                ArmOperation<ManagedGrafanaResource> op =
                    await grafanas.CreateOrUpdateAsync(
                            WaitUntil.Completed,
                            grafanaName,
                            grafanaData);

                ManagedGrafanaResource grafana = op.Value;

                Assert.Equal("Standard", grafana.Data.SkuName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create Managed Grafana.");
                throw;
            }
        }

        [Fact]
        public async Task StandardSkuWithUserAssignedManagedIdentity()
        {
            try
            {
                // Get all user-assigned managed identities in the subscription
                var userIdentityList = new List<UserAssignedIdentityResource>();

                Logger.Information("Listing user-assigned managed identities in subscription '{SubscriptionId}'.", SubscriptionId);

                await foreach (var identity in DefaultSubscription.GetUserAssignedIdentitiesAsync())
                {
                    userIdentityList.Add(identity);
                    Logger.Information("Found user-assigned managed identity: '{IdentityName}' with ID '{IdentityId}'.", identity.Data.Name, identity.Id);
                }

                // Skip test if no user-assigned managed identities are available
                if (userIdentityList.Count == 0)
                {
                    Logger.Warning("No user-assigned managed identities found in subscription. Skipping test.");
                    // Instead of SkipException, just return early or use Assert.True with a message
                    Logger.Information("Test skipped: No user-assigned managed identities available.");
                    return;
                }

                // Select the first user-assigned managed identity
                var selectedIdentity = userIdentityList.First();
                Logger.Information("Selected user-assigned managed identity: '{IdentityName}' with ID '{IdentityId}'.", selectedIdentity.Data.Name, selectedIdentity.Id);

                string grafanaName = RandomNameGenerator.GenerateGrafanaName();

                // Create user assigned identities dictionary for the Grafana instance
                var userAssignedIdentities = new Dictionary<ResourceIdentifier, UserAssignedIdentity>
                {
                    { selectedIdentity.Id, new UserAssignedIdentity() }
                };

                var managedIdentity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
                foreach (var kvp in userAssignedIdentities)
                {
                    managedIdentity.UserAssignedIdentities.Add(kvp.Key, kvp.Value);
                }

                var grafanaData = new ManagedGrafanaData(TestLocation)
                {
                    // "Standard" is the only production SKU today
                    SkuName = "Standard",

                    // Enable User-Assigned Managed Identity
                    Identity = managedIdentity
                };

                ManagedGrafanaCollection grafanas = TestResourceGroup.GetManagedGrafanas();

                Logger.Information("Creating Managed Grafana with name '{GrafanaName}' in resource group '{ResourceGroupName}' in location '{Location}' using user-assigned managed identity '{IdentityName}'.", 
                    grafanaName, TestResourceGroup.Data.Name, TestLocation, selectedIdentity.Data.Name);

                ArmOperation<ManagedGrafanaResource> op =
                    await grafanas.CreateOrUpdateAsync(
                            WaitUntil.Completed,
                            grafanaName,
                            grafanaData);

                ManagedGrafanaResource grafana = op.Value;

                // Verify the Grafana instance was created successfully
                Assert.Equal("Standard", grafana.Data.SkuName);
                Assert.NotNull(grafana.Data.Identity);
                Assert.Equal(ManagedServiceIdentityType.UserAssigned, grafana.Data.Identity.ManagedServiceIdentityType);
                Assert.True(grafana.Data.Identity.UserAssignedIdentities.ContainsKey(selectedIdentity.Id));

                Logger.Information("Successfully created Managed Grafana '{GrafanaName}' with user-assigned managed identity '{IdentityName}'.", 
                    grafanaName, selectedIdentity.Data.Name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create Managed Grafana with user-assigned managed identity.");
                throw;
            }
        }
    }
}
