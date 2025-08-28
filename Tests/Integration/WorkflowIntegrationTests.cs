using scrbl.Managers;
using System;
using System.IO;
using System.Text.Json;

namespace scrbl.Tests.Integration
{
    public class WorkflowIntegrationTests
    {
        private string GetTempFile()
        {
            return Path.GetTempFileName();
        }

        public void CompleteWorkflow_WithConfigAndNotes_ExecutesSuccessfully()
        {
            var tempFile = GetTempFile();
            var tempConfigDir = Path.Combine(Path.GetTempPath(), "scrbl-integration-test");
            
            try
            {
                Directory.CreateDirectory(tempConfigDir);
                
                var config = new Config
                {
                    NotesFilePath = tempFile,
                    ServerUrl = "https://test.example.com",
                    ServerApiKey = "test-key"
                };

                var configPath = Path.Combine(tempConfigDir, "config.json");
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);

                var manager = new NotesFileManager(tempFile);

                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var dailyTemplate = $"---\n## {today}\n### Daily Summary\n\n---\n";
                manager.AppendContent(dailyTemplate);

                manager.AppendToTodaysSection("* Morning standup meeting");
                manager.AppendToTodaysSection("* Worked on feature implementation");
                manager.AppendToTodaysSection("* Code review session");

                manager.AppendContent("* End of day reflection\n");

                var lines = File.ReadAllLines(tempFile);
                var todayIndex = Array.FindIndex(lines, line => line.Contains($"## {today}"));
                var firstDashAfterToday = Array.FindIndex(lines, todayIndex + 1, line => line.Trim() == "---");
                var endReflectionIndex = Array.FindIndex(lines, line => line.Contains("End of day reflection"));

                if (todayIndex == -1)
                    throw new Exception("Today's section not found");

                if (firstDashAfterToday == -1)
                    throw new Exception("Closing dashes not found");

                if (endReflectionIndex <= firstDashAfterToday)
                    throw new Exception("End reflection should be after today's section");

                var standupIndex = Array.FindIndex(lines, line => line.Contains("Morning standup"));
                var featureIndex = Array.FindIndex(lines, line => line.Contains("feature implementation"));
                var reviewIndex = Array.FindIndex(lines, line => line.Contains("Code review"));

                if (standupIndex > firstDashAfterToday || featureIndex > firstDashAfterToday || reviewIndex > firstDashAfterToday)
                    throw new Exception("Appended items should be within today's section");

                Console.WriteLine("✓ CompleteWorkflow_WithConfigAndNotes_ExecutesSuccessfully test passed");
            }
            finally
            {
                File.Delete(tempFile);
                if (Directory.Exists(tempConfigDir))
                    Directory.Delete(tempConfigDir, true);
            }
        }

        public void MultipleDaysWorkflow_WithSequentialDays_HandlesCorrectly()
        {
            var tempFile = GetTempFile();
            try
            {
                var manager = new NotesFileManager(tempFile);
                
                var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy.MM.dd");
                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy.MM.dd");

                var yesterdayTemplate = $"---\n## {yesterday}\n### Daily Summary\n* Yesterday's work\n* Completed tasks\n---\n";
                manager.AppendContent(yesterdayTemplate);

                var todayTemplate = $"---\n## {today}\n### Daily Summary\n\n---\n";
                manager.AppendContent(todayTemplate);

                var todayResult = manager.AppendToTodaysSection("* Today's first task");
                if (!todayResult)
                    throw new Exception("Should have found today's section");

                var file2 = GetTempFile();
                var tomorrowTemplate = $"---\n## {tomorrow}\n### Daily Summary\n\n---\n";
                File.WriteAllText(file2, tomorrowTemplate);
                var manager2 = new NotesFileManager(file2);
                var tomorrowResult = manager2.AppendToTodaysSection("* Should go to end");
                
                if (tomorrowResult)
                    throw new Exception("Should not have found today's section in tomorrow's file");

                File.Delete(file2);
                Console.WriteLine("✓ MultipleDaysWorkflow_WithSequentialDays_HandlesCorrectly test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void ConfigurationPersistence_WithValidConfig_PersistsCorrectly()
        {
            var tempConfigDir = Path.Combine(Path.GetTempPath(), "scrbl-config-test");
            
            try
            {
                Directory.CreateDirectory(tempConfigDir);
                var configPath = Path.Combine(tempConfigDir, "config.json");

                var originalConfig = new Config
                {
                    NotesFilePath = @"C:\test\notes.md",
                    ServerUrl = "https://api.example.com",
                    ServerApiKey = "secret-key-123"
                };

                var json = JsonSerializer.Serialize(originalConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);

                var loadedJson = File.ReadAllText(configPath);
                var loadedConfig = JsonSerializer.Deserialize<Config>(loadedJson);

                if (loadedConfig == null)
                    throw new Exception("Config deserialization failed");

                if (loadedConfig.NotesFilePath != originalConfig.NotesFilePath)
                    throw new Exception("NotesFilePath not persisted correctly");

                if (loadedConfig.ServerUrl != originalConfig.ServerUrl)
                    throw new Exception("ServerUrl not persisted correctly");

                if (loadedConfig.ServerApiKey != originalConfig.ServerApiKey)
                    throw new Exception("ServerApiKey not persisted correctly");

                Console.WriteLine("✓ ConfigurationPersistence_WithValidConfig_PersistsCorrectly test passed");
            }
            finally
            {
                if (Directory.Exists(tempConfigDir))
                    Directory.Delete(tempConfigDir, true);
            }
        }

        public void FileOperationSequence_WithComplexOperations_ExecutesInOrder()
        {
            var tempFile = GetTempFile();
            try
            {
                var manager = new NotesFileManager(tempFile);

                var initialLines = manager.GetLastLines(5);
                if (initialLines.Length != 0)
                    throw new Exception("New file should be empty");

                manager.AppendContent("# My Notes\n\nInitial content\n");

                var afterFirstAdd = manager.GetLastLines(5);
                if (afterFirstAdd.Length < 3)
                    throw new Exception("Content not added correctly");

                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var template = $"---\n## {today}\n### Daily Summary\n\n---\n";
                manager.AppendContent(template);

                var appendResult = manager.AppendToTodaysSection("* First task");
                if (!appendResult)
                    throw new Exception("Failed to append to today's section");

                manager.AppendToTodaysSection("* Second task");
                manager.AppendToTodaysSection("* Third task");

                var finalContent = File.ReadAllText(tempFile);
                if (!finalContent.Contains("# My Notes"))
                    throw new Exception("Initial content lost");

                if (!finalContent.Contains($"## {today}"))
                    throw new Exception("Today's section not found");

                if (!finalContent.Contains("First task") || !finalContent.Contains("Second task") || !finalContent.Contains("Third task"))
                    throw new Exception("Tasks not all present");

                var fileInfo = manager.GetFileInfo();
                if (!fileInfo.Exists)
                    throw new Exception("File should exist");

                if (fileInfo.Length == 0)
                    throw new Exception("File should not be empty");

                Console.WriteLine("✓ FileOperationSequence_WithComplexOperations_ExecutesInOrder test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public static void RunAllTests()
        {
            var tests = new WorkflowIntegrationTests();
            
            Console.WriteLine("Running Workflow integration tests...");
            tests.CompleteWorkflow_WithConfigAndNotes_ExecutesSuccessfully();
            tests.MultipleDaysWorkflow_WithSequentialDays_HandlesCorrectly();
            tests.ConfigurationPersistence_WithValidConfig_PersistsCorrectly();
            tests.FileOperationSequence_WithComplexOperations_ExecutesInOrder();
            Console.WriteLine("All Workflow integration tests passed! ✓");
        }
    }
}