using Spectre.Console;
using System.Text.Json;

namespace scrbl.Managers
{
    /// <summary>
    /// Manages the persistence and lifecycle of notes indexes.
    /// Handles saving/loading index data to/from disk as JSON files.
    /// </summary>
    internal static class NotesIndexManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static PersistentNotesIndex? LoadIndex(string indexPath)
        {
            if (!File.Exists(indexPath))
                return null;

            try
            {
                var json = File.ReadAllText(indexPath);
                return JsonSerializer.Deserialize<PersistentNotesIndex>(json, JsonOptions);
            }
            catch
            {
                DeleteIndex(indexPath);
                return null;
            }
        }

        public static void SaveIndex(string indexPath, PersistentNotesIndex index)
        {
            try
            {
                var json = JsonSerializer.Serialize(index, JsonOptions);
                File.WriteAllText(indexPath, json);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: Could not save index file: {ex.Message}[/]");
            }
        }

        public static void DeleteIndex(string indexPath)
        {
            if (!File.Exists(indexPath)) return;
            try { File.Delete(indexPath); }
            catch
            {
                // ignored
            }
        }

        public static void CleanupOldIndexes(string notesDirectory)
        {
            try
            {
                var indexPatterns = new[] { "*.index", "*.scrbl-index" };
                var cutoffTime = DateTime.Now.AddDays(-7);

                foreach (var pattern in indexPatterns)
                {
                    var indexFiles = Directory.GetFiles(notesDirectory, pattern);
                    foreach (var indexFile in indexFiles)
                    {
                        var lastWrite = File.GetLastWriteTime(indexFile);
                        if (lastWrite >= cutoffTime) continue;
                        try
                        {
                            File.Delete(indexFile);
                            AnsiConsole.MarkupLine($"[dim]Cleaned up old index: {Path.GetFileName(indexFile)}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Error deleting index file {Path.GetFileName(indexFile)}: {ex.Message}[/]");
                        }
                    }
                }
            }
            catch { /* Ignore cleanup errors */ }
        }
    }

    internal class NotesIndex
    {
        private readonly List<HeaderItem> _headers;
        private readonly List<SectionItem> _sections;
        private readonly Dictionary<string, List<int>> _wordIndex;

        public NotesIndex(List<string> lines)
        {
            _headers = [];
            _sections = [];
            _wordIndex = new Dictionary<string, List<int>>();

            BuildIndex(lines);
        }

        private NotesIndex(List<HeaderItem> headers, List<SectionItem> sections, Dictionary<string, List<int>> wordIndex)
        {
            _headers = headers;
            _sections = sections;
            _wordIndex = wordIndex;
        }

        /// <summary>
        /// Creates a NotesIndex from a saved PersistentNotesIndex.
        /// </summary>
        /// <param name="persistent">The persistent index loaded from disk</param>
        /// <returns>A working NotesIndex instance</returns>
        public static NotesIndex FromPersistent(PersistentNotesIndex persistent)
        {
            return new NotesIndex(persistent.Headers, persistent.Sections, persistent.WordIndex);
        }

        public HeaderItem? GetLastHeader()
        {
            return _headers.LastOrDefault(h => h.Level == 2);
        }

        public SectionItem? FindSectionUnderHeader(HeaderItem header, string sectionText)
        {
            var nextHeaderIndex = _headers.FindIndex(h => h.LineIndex > header.LineIndex && h.Level == 2);
            var searchEnd = nextHeaderIndex == -1 ? int.MaxValue : _headers[nextHeaderIndex].LineIndex;

            return _sections.FirstOrDefault(s =>
                s.LineIndex > header.LineIndex &&
                s.LineIndex < searchEnd &&
                s.Text == sectionText);
        }



        public void InsertLineAt(int lineIndex, string content)
        {
            ShiftHeadersAfterInsertion(lineIndex);
            ShiftSectionsAfterInsertion(lineIndex);
            ShiftWordIndexAfterInsertion(lineIndex);

            IndexWordsInLine(content, lineIndex);
            AddStructuralElementsIfNeeded(content, lineIndex);
        }

        // Helper methods for InsertLineAt to improve readability

        private void ShiftHeadersAfterInsertion(int insertionIndex)
        {
            for (var i = 0; i < _headers.Count; i++)
            {
                if (_headers[i].LineIndex >= insertionIndex)
                {
                    _headers[i] = _headers[i] with { LineIndex = _headers[i].LineIndex + 1 };
                }
            }
        }

        private void ShiftSectionsAfterInsertion(int insertionIndex)
        {
            for (var i = 0; i < _sections.Count; i++)
            {
                if (_sections[i].LineIndex >= insertionIndex)
                {
                    _sections[i] = _sections[i] with { LineIndex = _sections[i].LineIndex + 1 };
                }
            }
        }

        private void ShiftWordIndexAfterInsertion(int insertionIndex)
        {
            var keysToUpdate = _wordIndex.Keys.ToList();
            foreach (var key in keysToUpdate)
            {
                var lines = _wordIndex[key];
                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i] >= insertionIndex)
                    {
                        lines[i] += 1;
                    }
                }
            }
        }

        private void AddStructuralElementsIfNeeded(string content, int lineIndex)
        {
            if (content.StartsWith("## "))
            {
                _headers.Add(new HeaderItem(lineIndex, content.Trim(), 2));
                _headers.Sort((a, b) => a.LineIndex.CompareTo(b.LineIndex));
            }
            else if (content.StartsWith("### "))
            {
                _sections.Add(new SectionItem(lineIndex, content.Trim(), 3));
                _sections.Sort((a, b) => a.LineIndex.CompareTo(b.LineIndex));
            }
        }
        public IEnumerable<HeaderItem> GetAllHeaders() => _headers;
        public IEnumerable<SectionItem> GetAllSections() => _sections;

        public Dictionary<string, List<int>> GetWordIndex() => _wordIndex;
        public IEnumerable<int> FindLinesContaining(string word)
        {
            return _wordIndex.TryGetValue(word.ToLowerInvariant(), out var lines) ? lines : Enumerable.Empty<int>();
        }

        public IEnumerable<HeaderItem> GetHeadersByDate(DateTime date)
        {
            var dateString = date.ToString("yyyy.MM.dd");
            return _headers.Where(h => h.Text.Contains(dateString));
        }

        public IEnumerable<SectionItem> GetSectionsByName(string name)
        {
            return _sections.Where(s => s.Text.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        private void BuildIndex(List<string> lines)
        {
            var startTime = DateTime.Now;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.StartsWith("## "))
                {
                    _headers.Add(new HeaderItem(i, line.Trim(), 2));
                }
                else if (line.StartsWith("### "))
                {
                    _sections.Add(new SectionItem(i, line.Trim(), 3));
                }

                IndexWordsInLine(line, i);
            }

            var duration = DateTime.Now - startTime;
            AnsiConsole.MarkupLine($"[dim]Indexed {lines.Count} lines, {_headers.Count} headers, {_sections.Count} sections in {duration.TotalMilliseconds:F0}ms[/]");
        }

        private void IndexWordsInLine(string line, int lineIndex)
        {
            var words = line.Split([' ', '\t', '.', ',', '!', '?', ';', ':'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var cleanWord = word.ToLowerInvariant().Trim();
                if (!_wordIndex.ContainsKey(cleanWord))
                    _wordIndex[cleanWord] = [];

                _wordIndex[cleanWord].Add(lineIndex);
            }
        }
    }

    internal class PersistentNotesIndex
    {
        public DateTime FileTimestamp { get; set; }
        public List<HeaderItem> Headers { get; set; } = [];
        public List<SectionItem> Sections { get; set; } = [];
        public Dictionary<string, List<int>> WordIndex { get; set; } = new();
    }

    internal record HeaderItem(int LineIndex, string Text, int Level)
    {
        public string Title => Text.TrimStart('#').Trim();
    }

    internal record SectionItem(int LineIndex, string Text, int Level)
    {
        public string Title => Text.TrimStart('#').Trim();
    }
}