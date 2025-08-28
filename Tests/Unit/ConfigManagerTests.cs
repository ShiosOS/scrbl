using scrbl.Managers;
using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace scrbl.Tests.Unit
{
    public class ConfigManagerTests
    {
        [Fact]
        public void ConfigSerialization_WithValidConfig_SerializesAndDeserializesCorrectly()
        {
            var config = new Config
            {
                NotesFilePath = @"C:\test\notes.md",
                ServerUrl = "https://example.com",
                ServerApiKey = "test-key"
            };

            var tempFile = Path.GetTempFileName();
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(tempFile, json);

                var loadedJson = File.ReadAllText(tempFile);
                var loadedConfig = JsonSerializer.Deserialize<Config>(loadedJson);

                Assert.NotNull(loadedConfig);
                Assert.Equal(config.NotesFilePath, loadedConfig.NotesFilePath);
                Assert.Equal(config.ServerUrl, loadedConfig.ServerUrl);
                Assert.Equal(config.ServerApiKey, loadedConfig.ServerApiKey);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void Config_WithDefaultConstructor_HasExpectedDefaults()
        {
            var config = new Config();

            Assert.Equal(string.Empty, config.NotesFilePath);
            Assert.Equal(string.Empty, config.ServerUrl);
            Assert.Equal(string.Empty, config.ServerApiKey);
        }

        [Fact]
        public void LoadConfig_WhenNoFileExists_ReturnsValidConfig()
        {
            var config = ConfigManager.LoadConfig();
            Assert.NotNull(config);
        }

    }
}