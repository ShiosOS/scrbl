using scrbl.Managers;
using System;
using System.IO;
using System.Text;

namespace scrbl.Tests.Unit
{
    public class EdgeCaseTests
    {
        private string GetTempFile()
        {
            return Path.GetTempFileName();
        }

        public void GetLastLines_WithEmptyFile_ReturnsNoLines()
        {
            var tempFile = GetTempFile();
            try
            {
                File.WriteAllText(tempFile, "");
                var manager = new NotesFileManager(tempFile);
                
                var lastLines = manager.GetLastLines(5);
                if (lastLines.Length != 0)
                    throw new Exception("Empty file should return no lines");

                Console.WriteLine("✓ GetLastLines_WithEmptyFile_ReturnsNoLines test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void GetLastLines_WithVeryLongLines_HandlesCorrectly()
        {
            var tempFile = GetTempFile();
            try
            {
                var longLine = new string('A', 5000);
                File.WriteAllText(tempFile, $"Short line\n{longLine}\nAnother short line");
                
                var manager = new NotesFileManager(tempFile);
                var lastLines = manager.GetLastLines(3);
                
                if (lastLines.Length != 3)
                    throw new Exception($"Expected 3 lines, got {lastLines.Length}");
                
                if (lastLines[1].Length != 5000)
                    throw new Exception("Long line not preserved correctly");

                Console.WriteLine("✓ GetLastLines_WithVeryLongLines_HandlesCorrectly test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void AppendContent_WithSpecialCharacters_PreservesEncoding()
        {
            var tempFile = GetTempFile();
            try
            {
                var specialContent = "Content with émojis 🎉, unicode ñáéíóú, and symbols @#$%^&*()";
                var manager = new NotesFileManager(tempFile);
                
                manager.AppendContent(specialContent);
                
                var fileContent = File.ReadAllText(tempFile, Encoding.UTF8);
                if (!fileContent.Contains("🎉"))
                    throw new Exception("Emoji not preserved");
                
                if (!fileContent.Contains("ñáéíóú"))
                    throw new Exception("Unicode characters not preserved");

                Console.WriteLine("✓ AppendContent_WithSpecialCharacters_PreservesEncoding test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void AppendToTodaysSection_WithMultipleTodaysSections_FindsFirstMatch()
        {
            var tempFile = GetTempFile();
            try
            {
                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var content = $@"## {today}
### Morning Notes

---
## {today}
### Evening Notes

---
## 2023.01.01
### Old Notes";
                
                File.WriteAllText(tempFile, content);
                var manager = new NotesFileManager(tempFile);
                
                var result = manager.AppendToTodaysSection("* New entry");
                
                if (!result)
                    throw new Exception("Should find today's section even with duplicates");
                
                var fileContent = File.ReadAllText(tempFile);
                if (!fileContent.Contains("New entry"))
                    throw new Exception("New entry not added");

                Console.WriteLine("✓ AppendToTodaysSection_WithMultipleTodaysSections_FindsFirstMatch test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void AppendToTodaysSection_WithFutureDate_Appends()
        {
            var tempFile = GetTempFile();
            try
            {
                var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy.MM.dd");
                var content = $@"## {tomorrow}
### Future Notes

---";
                
                File.WriteAllText(tempFile, content);
                var manager = new NotesFileManager(tempFile);
                
                var result = manager.AppendToTodaysSection("* Should go to end");
                
                if (result)
                    throw new Exception("Should not find today's section when only tomorrow exists");
                
                var fileContent = File.ReadAllText(tempFile);
                if (!fileContent.Contains("Should go to end"))
                    throw new Exception("Content should still be added to end of file");

                Console.WriteLine("✓ AppendToTodaysSection_WithFutureDate_Appends test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void FileOperations_WithCorruptedStructure_HandlesGracefully()
        {
            var tempFile = GetTempFile();
            try
            {
                var corruptedContent = @"## No date format
### Random content
---
## 2025.13.99  // Invalid date
### Bad section
Some content without proper structure
---
##   // Empty header
";
                
                File.WriteAllText(tempFile, corruptedContent);
                var manager = new NotesFileManager(tempFile);
                
                var result = manager.AppendToTodaysSection("* Test content");
                var lastLines = manager.GetLastLines(5);
                var fileInfo = manager.GetFileInfo();
                
                if (!fileInfo.Exists)
                    throw new Exception("File should still exist after operations");

                Console.WriteLine("✓ FileOperations_WithCorruptedStructure_HandlesGracefully test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void AppendContent_WithReadOnlyFile_ThrowsExpectedException()
        {
            var tempFile = GetTempFile();
            try
            {
                File.WriteAllText(tempFile, "Initial content");
                
                var fileInfo = new FileInfo(tempFile);
                fileInfo.IsReadOnly = true;
                
                var manager = new NotesFileManager(tempFile);
                
                try
                {
                    manager.AppendContent("Should fail");
                    fileInfo.IsReadOnly = false;
                    throw new Exception("Should have thrown exception for read-only file");
                }
                catch (UnauthorizedAccessException)
                {
                    fileInfo.IsReadOnly = false;
                    Console.WriteLine("✓ AppendContent_WithReadOnlyFile_ThrowsExpectedException test passed");
                }
            }
            finally
            {
                var fileInfo = new FileInfo(tempFile);
                if (fileInfo.Exists)
                {
                    fileInfo.IsReadOnly = false;
                    File.Delete(tempFile);
                }
            }
        }

        public void GetLastLines_WithMixedLineEndings_HandlesConsistently()
        {
            var tempFile = GetTempFile();
            try
            {
                var mixedContent = "Line 1\nLine 2\r\nLine 3\rLine 4\n";
                File.WriteAllText(tempFile, mixedContent);
                
                var manager = new NotesFileManager(tempFile);
                var lines = manager.GetLastLines(10);
                
                if (lines.Length < 3)
                    throw new Exception($"Expected at least 3 lines, got {lines.Length}");

                Console.WriteLine("✓ GetLastLines_WithMixedLineEndings_HandlesConsistently test passed");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public static void RunAllTests()
        {
            var tests = new EdgeCaseTests();
            
            Console.WriteLine("Running Edge Case unit tests...");
            tests.GetLastLines_WithEmptyFile_ReturnsNoLines();
            tests.GetLastLines_WithVeryLongLines_HandlesCorrectly();
            tests.AppendContent_WithSpecialCharacters_PreservesEncoding();
            tests.AppendToTodaysSection_WithMultipleTodaysSections_FindsFirstMatch();
            tests.AppendToTodaysSection_WithFutureDate_Appends();
            tests.FileOperations_WithCorruptedStructure_HandlesGracefully();
            tests.AppendContent_WithReadOnlyFile_ThrowsExpectedException();
            tests.GetLastLines_WithMixedLineEndings_HandlesConsistently();
            Console.WriteLine("All Edge Case unit tests passed! ✓");
        }
    }
}