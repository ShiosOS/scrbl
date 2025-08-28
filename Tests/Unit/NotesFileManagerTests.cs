using scrbl.Managers;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace scrbl.Tests.Unit
{
    public class NotesFileManagerTests
    {
        private string GetTempFile()
        {
            return Path.GetTempFileName();
        }

        [Fact]
        public void AppendContent_WithValidContent_AppendsSuccessfully()
        {
            var tempFile = GetTempFile();
            try
            {
                var manager = new NotesFileManager(tempFile);
                var content = "Test content\n";

                manager.AppendContent(content);

                var fileContent = File.ReadAllText(tempFile);
                Assert.Equal(content, fileContent);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetLastLines_WithValidCount_ReturnsCorrectLines()
        {
            var tempFile = GetTempFile();
            try
            {
                var lines = new[] { "Line 1", "Line 2", "Line 3", "Line 4", "Line 5" };
                File.WriteAllLines(tempFile, lines);

                var manager = new NotesFileManager(tempFile);
                var result = manager.GetLastLines(3);

                Assert.Equal(3, result.Length);
                Assert.Equal("Line 3", result[0]);
                Assert.Equal("Line 4", result[1]);
                Assert.Equal("Line 5", result[2]);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void AppendToTodaysSection_WithExistingSection_AppendsSuccessfully()
        {
            var tempFile = GetTempFile();
            try
            {
                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var initialContent = $"## {today}\n### Daily Summary\n\n## 2023.01.01\n* Old entry\n";
                File.WriteAllText(tempFile, initialContent);

                var manager = new NotesFileManager(tempFile);
                var newEntry = "* New entry";
                var result = manager.AppendToTodaysSection(newEntry);

                Assert.True(result);
                var fileContent = File.ReadAllText(tempFile);
                Assert.Contains(newEntry, fileContent);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void AppendToTodaysSection_WithClosingDashes_InsertsAtCorrectPosition()
        {
            var tempFile = GetTempFile();
            try
            {
                var today = DateTime.Now.ToString("yyyy.MM.dd");
                var initialContent = $"---\n## {today}\n### Daily Summary\n\n---\n## 2023.01.01\n* Old entry\n";
                File.WriteAllText(tempFile, initialContent);

                var manager = new NotesFileManager(tempFile);
                var newEntry = "* New entry before closing dashes";
                var result = manager.AppendToTodaysSection(newEntry);

                Assert.True(result);
                
                var lines = File.ReadAllLines(tempFile);
                var newEntryIndex = Array.FindIndex(lines, line => line.Contains("New entry before closing dashes"));
                var closingDashIndex = Array.FindIndex(lines, newEntryIndex, line => line.Trim() == "---");

                Assert.True(newEntryIndex >= 0, "New entry not found in file");
                Assert.True(closingDashIndex >= 0 && newEntryIndex < closingDashIndex, "New entry should be before closing ---");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetFileInfo_WithNonExistentFile_ReturnsNonExistentFileInfo()
        {
            var nonExistentFile = "nonexistent.txt";
            var manager = new NotesFileManager(nonExistentFile);
            
            var fileInfo = manager.GetFileInfo();
            
            Assert.False(fileInfo.Exists);
        }

        [Fact]
        public void GetLastLines_RequestingMoreLinesThanExist_ReturnsAllAvailableLines()
        {
            var tempFile = GetTempFile();
            try
            {
                var lines = new[] { "Line 1", "Line 2" };
                File.WriteAllLines(tempFile, lines);

                var manager = new NotesFileManager(tempFile);
                var result = manager.GetLastLines(10); // Request more lines than exist

                Assert.Equal(2, result.Length);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void GetLastLines_WithEmptyFile_ReturnsEmptyArray()
        {
            var tempFile = GetTempFile();
            try
            {
                File.WriteAllText(tempFile, "");
                var manager = new NotesFileManager(tempFile);
                
                var lastLines = manager.GetLastLines(5);
                Assert.Empty(lastLines);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

    }
}