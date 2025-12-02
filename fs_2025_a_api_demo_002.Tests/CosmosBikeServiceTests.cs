using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using fs_2025_a_api_demo_002.Services;
using Xunit;

namespace fs_2025_a_api_demo_002.Tests
{
    public class CosmosBikeServiceTests
    {
        // -----------------------------------------------------
        // Helper: Build test configuration for CosmosBikeService
        // -----------------------------------------------------
        private IConfiguration BuildConfig(Dictionary<string, string?>? overrides = null)
        {
            var settings = new Dictionary<string, string?>
            {
                ["CosmosDb:AccountEndpoint"] = "https://localhost:8081",

                // IMPORTANT: must be valid Base64 or CosmosClient throws FormatException
                ["CosmosDb:AccountKey"] = "C2y6yLjf5/R+ob0N8A7Cgv30VRhFEMuyNEMi8J2p1Ek=",

                ["CosmosDb:DatabaseName"] = "DublinBikesDb",
                ["CosmosDb:ContainerName"] = "stations"
            };

            // Apply overrides when needed
            if (overrides != null)
            {
                foreach (var kvp in overrides)
                {
                    settings[kvp.Key] = kvp.Value;
                }
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        // -----------------------------------------------------
        // Test 1: Missing DatabaseName → should throw
        // -----------------------------------------------------
        [Fact]
        public void Constructor_ThrowsIfDatabaseNameMissing()
        {
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["CosmosDb:DatabaseName"] = null
            });

            var ex = Assert.Throws<InvalidOperationException>(
                () => new CosmosBikeService(config));

            Assert.Contains("CosmosDb:DatabaseName", ex.Message);
        }

        // -----------------------------------------------------
        // Test 2: Missing ContainerName → should throw
        // -----------------------------------------------------
        [Fact]
        public void Constructor_ThrowsIfContainerNameMissing()
        {
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["CosmosDb:ContainerName"] = null
            });

            var ex = Assert.Throws<InvalidOperationException>(
                () => new CosmosBikeService(config));

            Assert.Contains("CosmosDb:ContainerName", ex.Message);
        }

        // -----------------------------------------------------
        // Test 3: Valid configuration → constructor succeeds
        // -----------------------------------------------------
        [Fact]
        public void Constructor_SucceedsWithValidConfiguration()
        {
            var config = BuildConfig(); // all valid defaults

            var service = new CosmosBikeService(config);

            Assert.NotNull(service);
        }
    }
}
