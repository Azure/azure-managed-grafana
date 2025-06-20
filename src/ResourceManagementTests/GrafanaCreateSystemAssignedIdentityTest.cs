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

namespace ResourceManagementTests
{
    public class GrafanaCreateSystemAssignedIdentityTest : AzureTestBase
    {
        public GrafanaCreateSystemAssignedIdentityTest(ITestOutputHelper output)
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
                    // "Standard" is the only production SKU today
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
    }
}