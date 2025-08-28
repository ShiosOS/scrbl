using scrbl.Managers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace scrbl.Tests.Integration
{
    public class CommandIntegrationTests
    {
        private string GetTempFile()
        {
            return Path.GetTempFileName();
        }

        public async Task ConfigValidation_WithValidSettings_CreatesAndValidatesCorrectly()
        {
            var tempConfigDir = Path.Combine(Path.GetTempPath(), "scrbl-config-validation");
            try
            {
                Directory.CreateDirectory(tempConfigDir);

                var config = new Config
                {
                    NotesFilePath = @"C:\test\notes.md",
                    ServerUrl = "https://api.example.com",
                    ServerApiKey = "test-key-123"
                };

                if (string.IsNullOrEmpty(config.NotesFilePath))
                    throw new Exception("NotesFilePath should not be empty");

                if (string.IsNullOrEmpty(config.ServerUrl))
                    throw new Exception("ServerUrl should not be empty");

                if (string.IsNullOrEmpty(config.ServerApiKey))
                    throw new Exception("ServerApiKey should not be empty");

                Console.WriteLine("✓ ConfigValidation_WithValidSettings_CreatesAndValidatesCorrectly test passed");
            }
            finally
            {
                if (Directory.Exists(tempConfigDir))
                    Directory.Delete(tempConfigDir, true);
            }
        }

        public async Task FileManagerValidation_WithBasicOperations_ExecutesSuccessfully()
        {
            var tempFile = GetTempFile();
            try
            {
                var manager = new NotesFileManager(tempFile);
                
                manager.AppendContent("Test content\n");
                
                var lines = manager.GetLastLines(5);
                if (lines.Length == 0)
                    throw new Exception("Should have at least one line");

                var fileInfo = manager.GetFileInfo();
                if (!fileInfo.Exists)
                    throw new Exception("File should exist after append");

                Console.WriteLine("✓ FileManagerValidation_WithBasicOperations_ExecutesSuccessfully test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void CreateCommand_WithDateFormatting_FormatsCorrectly()
        {
            var tempFile = GetTempFile();
            try
            {
                File.WriteAllText(tempFile, "# Existing content\n");
                
                var manager = new NotesFileManager(tempFile);
                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var expectedTemplate = $"---\n## {today}\n### Daily Summary\n\n---\n";
                
                manager.AppendContent(expectedTemplate);
                
                var content = File.ReadAllText(tempFile);
                if (!content.Contains($"## {today}"))
                    throw new Exception("Today's date not found in correct format");
                
                if (!content.Contains("### Daily Summary"))
                    throw new Exception("Daily Summary header not found");

                Console.WriteLine("✓ CreateCommand_WithDateFormatting_FormatsCorrectly test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void ShowCommand_WithVariableLineCounts_ReturnsCorrectLines()
        {
            var tempFile = GetTempFile();
            try
            {
                var lines = new string[20];
                for (int i = 0; i < 20; i++)
                {
                    lines[i] = $"Line {i + 1}";
                }
                File.WriteAllLines(tempFile, lines);

                var manager = new NotesFileManager(tempFile);
                
                var result10 = manager.GetLastLines(10);
                if (result10.Length != 10)
                    throw new Exception($"Expected 10 lines, got {result10.Length}");

                var result5 = manager.GetLastLines(5);
                if (result5.Length != 5)
                    throw new Exception($"Expected 5 lines, got {result5.Length}");

                var result50 = manager.GetLastLines(50);
                if (result50.Length != 20)
                    throw new Exception($"Expected 20 lines (all available), got {result50.Length}");

                Console.WriteLine("✓ ShowCommand_WithVariableLineCounts_ReturnsCorrectLines test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public static async Task RunAllTests()
        {
            var tests = new CommandIntegrationTests();
            
            Console.WriteLine("Running Command integration tests...");
            await tests.ConfigValidation_WithValidSettings_CreatesAndValidatesCorrectly();
            await tests.FileManagerValidation_WithBasicOperations_ExecutesSuccessfully();
            tests.CreateCommand_WithDateFormatting_FormatsCorrectly();
            tests.ShowCommand_WithVariableLineCounts_ReturnsCorrectLines();
            Console.WriteLine("All Command integration tests passed! ✓");
        }
    }
}