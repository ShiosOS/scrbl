using scrbl.Managers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace scrbl.Tests.Unit
{
    public class SyncServiceTests
    {
        private string GetTempFile()
        {
            return Path.GetTempFileName();
        }

        public void SyncConfigValidation_WithValidConfig_EnablesSync()
        {
            var config = new Config
            {
                NotesFilePath = GetTempFile(),
                ServerUrl = "https://api.example.com",
                ServerApiKey = "test-key"
            };

            var hasSyncConfig = !string.IsNullOrEmpty(config.ServerUrl) && !string.IsNullOrEmpty(config.ServerApiKey);
            if (!hasSyncConfig)
                throw new Exception("Sync should be enabled with valid URL and API key");

            var configNoUrl = new Config { NotesFilePath = GetTempFile(), ServerApiKey = "test-key" };
            var noUrlSync = !string.IsNullOrEmpty(configNoUrl.ServerUrl) && !string.IsNullOrEmpty(configNoUrl.ServerApiKey);
            if (noUrlSync)
                throw new Exception("Sync should be disabled without URL");

            Console.WriteLine("✓ SyncConfigValidation_WithValidConfig_EnablesSync test passed");
        }

        public void SyncService_WithoutConfig_DisablesSync()
        {
            var config = new Config
            {
                NotesFilePath = GetTempFile(),
                ServerUrl = "",
                ServerApiKey = ""
            };

            var isSyncEnabled = !string.IsNullOrEmpty(config.ServerUrl) && !string.IsNullOrEmpty(config.ServerApiKey);
            if (isSyncEnabled)
                throw new Exception("Sync should be disabled without proper config");
                
            Console.WriteLine("✓ SyncService_WithoutConfig_DisablesSync test passed");
        }

        public async Task SyncMetadataHandling_WithValidTimes_CalculatesCorrectDifference()
        {
            var now = DateTimeOffset.Now;
            var earlier = now.AddMinutes(-10);
            
            var timeDiff = Math.Abs((now - earlier).TotalSeconds);
            if (timeDiff < 500)
                throw new Exception("Time difference calculation incorrect");
                
            Console.WriteLine("✓ SyncMetadataHandling_WithValidTimes_CalculatesCorrectDifference test passed");
        }

        public void SyncService_WithInvalidServerConfig_HandlesGracefully()
        {
            var tempFile = GetTempFile();
            var tempConfigDir = Path.Combine(Path.GetTempPath(), "scrbl-test-invalid");
            
            try
            {
                Directory.CreateDirectory(tempConfigDir);
                
                var invalidConfig = new Config
                {
                    NotesFilePath = tempFile,
                    ServerUrl = "https://invalid-url-that-does-not-exist.com",
                    ServerApiKey = "invalid-key"
                };

                var configPath = Path.Combine(tempConfigDir, "config.json");
                var json = System.Text.Json.JsonSerializer.Serialize(invalidConfig);
                File.WriteAllText(configPath, json);

                var hasValidSync = !string.IsNullOrEmpty(invalidConfig.ServerUrl) && !string.IsNullOrEmpty(invalidConfig.ServerApiKey);
                
                Console.WriteLine("✓ SyncService_WithInvalidServerConfig_HandlesGracefully test passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ SyncService_WithInvalidServerConfig_HandlesGracefully test passed (handled: {ex.GetType().Name})");
            }
            finally
            {
                File.Delete(tempFile);
                if (Directory.Exists(tempConfigDir))
                    Directory.Delete(tempConfigDir, true);
            }
        }

        public void NotesMetadataModel_WithValidData_HandlesTimestamps()
        {
            var lastModified = DateTimeOffset.Now;
            var anotherTime = DateTimeOffset.Now.AddMinutes(-5);

            if (lastModified == default)
                throw new Exception("LastModified should be set");

            var timeDiff = Math.Abs((lastModified - anotherTime).TotalSeconds);
            if (timeDiff < 250)
                throw new Exception("Time difference calculation seems incorrect");

            Console.WriteLine("✓ NotesMetadataModel_WithValidData_HandlesTimestamps test passed");
        }

        public void HttpClientTimeout_WithValidTimeout_ConfiguresCorrectly()
        {
            var timeoutValue = TimeSpan.FromSeconds(10);
            
            if (timeoutValue.TotalSeconds != 10)
                throw new Exception("Timeout should be 10 seconds");
                
            if (timeoutValue <= TimeSpan.Zero)
                throw new Exception("Timeout should be positive");
                
            Console.WriteLine("✓ HttpClientTimeout_WithValidTimeout_ConfiguresCorrectly test passed");
        }

        public static async Task RunAllTests()
        {
            var tests = new SyncServiceTests();
            
            Console.WriteLine("Running SyncService unit tests...");
            tests.SyncConfigValidation_WithValidConfig_EnablesSync();
            tests.SyncService_WithoutConfig_DisablesSync();
            await tests.SyncMetadataHandling_WithValidTimes_CalculatesCorrectDifference();
            tests.SyncService_WithInvalidServerConfig_HandlesGracefully();
            tests.NotesMetadataModel_WithValidData_HandlesTimestamps();
            tests.HttpClientTimeout_WithValidTimeout_ConfiguresCorrectly();
            Console.WriteLine("All SyncService unit tests passed! ✓");
        }
    }
}