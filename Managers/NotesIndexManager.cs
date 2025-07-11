using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace scrbl.Managers
{
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
            catch(Exception ex)
            {
                AnsiConsole.WriteException(ex);
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
            catch(Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }

        private static void DeleteIndex(string indexPath)
        {
            if (!File.Exists(indexPath)) return;
            try
            {
                File.Delete(indexPath);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }

        public static void CleanupOldIndexes(string notesDirectory)
        {
            try
            {
                string[] indexPatterns = ["*.index", "*.scrbl-index"];
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
                        catch { /* Ignore cleanup errors */ }
                    }
                }
            }
            catch { /* Ignore cleanup errors */ }
        }
    }

    internal record HeadingItem(int LineIndex, string Text, int Level)
    {
        public string Title => Text.TrimStart('#').Trim();
    }

    internal class HeadingNode(HeadingItem heading)
    {
        public HeadingItem Heading { get; } = heading;
        public HeadingNode? Parent { get; set; }
        public List<HeadingNode> Children { get; } = [];
    }

    internal class PersistentNotesIndex
    {
        public DateTime FileTimestamp { get; set; }
        public List<HeadingItem> Headings { get; set; } = [];
        public Dictionary<string, List<int>> WordIndex { get; set; } = new();
    }

    internal class NotesIndex
    {
        private readonly List<HeadingItem> _allHeadings;
        private readonly Dictionary<string, List<int>> _wordIndex;
        private readonly Dictionary<int, HeadingNode> _headingNodes;

        private static readonly Regex _headingRegex = new Regex(@"^(#+)\s*(.*)$");

        public NotesIndex(List<string> lines)
        {
            _allHeadings = [];
            _wordIndex = new Dictionary<string, List<int>>();
            _headingNodes = new Dictionary<int, HeadingNode>();
            
            BuildIndex(lines);
            BuildHeadingTree();
        }

        private NotesIndex(List<HeadingItem> headings, Dictionary<string, List<int>> wordIndex)
        {
            _allHeadings = headings;
            _wordIndex = wordIndex;
            _headingNodes = new Dictionary<int, HeadingNode>();
            
            BuildHeadingTree();
        }

        public static NotesIndex FromPersistent(PersistentNotesIndex persistent)
        {
            return new NotesIndex(persistent.Headings, persistent.WordIndex);
        }

        public HeadingItem? GetLastHeader(int level = 2)
        {
            return _allHeadings.LastOrDefault(h => h.Level == level);
        }

        public HeadingItem? FindHeadingUnderParent(HeadingItem parentHeading, string headingText, int headingLevel)
        {
            // Find the node for the parent heading
            if (!_headingNodes.TryGetValue(parentHeading.LineIndex, out var parentNode))
            {
                return null;
            }

            // Search among its direct children
            return parentNode.Children
                             .Select(node => node.Heading)
                             .FirstOrDefault(h => h.Title.Equals(headingText, StringComparison.OrdinalIgnoreCase) && h.Level == headingLevel);
        }

        public HeadingNode? GetHeadingNode(int lineIndex)
        {
            return _headingNodes.GetValueOrDefault(lineIndex);
        }

        public IEnumerable<HeadingItem> GetChildHeadings(HeadingItem parent)
        {
            return _headingNodes.TryGetValue(parent.LineIndex, out var parentNode)
                ? parentNode.Children.Select(childNode => childNode.Heading)
                : [];
        }

        public HeadingItem? GetParentHeading(HeadingItem child)
        {
            if (_headingNodes.TryGetValue(child.LineIndex, out var childNode) && childNode.Parent != null)
            {
                return childNode.Parent.Heading;
            }
            return null;
        }

        public void InsertLineAt(int lineIndex, string content)
        {
            // 1. Shift all existing headings after the insertion point
            for (var i = 0; i < _allHeadings.Count; i++)
            {
                if (_allHeadings[i].LineIndex >= lineIndex)
                {
                    _allHeadings[i] = _allHeadings[i] with { LineIndex = _allHeadings[i].LineIndex + 1 };
                }
            }

            // 2. Shift all word index line numbers
            foreach (var lines in _wordIndex.Values) // Iterate over values directly
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i] >= lineIndex)
                    {
                        lines[i] += 1;
                    }
                }
            }

            // 3. Index words in the new content
            IndexWordsInLine(content, lineIndex);

            // 4. If the new line is a heading, add it to the unified headings list
            var headingMatch = _headingRegex.Match(content);
            if (!headingMatch.Success) return;
            
            var level = headingMatch.Groups[1].Length;
            if (level is < 2 or > 6) return; 
            
            _allHeadings.Add(new HeadingItem(lineIndex, content.Trim(), level));
            _allHeadings.Sort((a, b) => a.LineIndex.CompareTo(b.LineIndex));
                    
            // Rebuild the heading tree after adding a new heading
            BuildHeadingTree();
        }

        public IEnumerable<HeadingItem> GetAllHeadings() => _allHeadings;
        
        public Dictionary<string, List<int>> GetWordIndex() => _wordIndex;
        public IEnumerable<int> FindLinesContaining(string word)
        {
            return _wordIndex.TryGetValue(word.ToLowerInvariant(), out var lines) ? lines : Enumerable.Empty<int>();
        }

        public IEnumerable<HeadingItem> GetHeadingsByDate(DateTime date)
        {
            var dateString = date.ToString("yyyy.MM.dd");
            return _allHeadings.Where(h => h.Text.Contains(dateString));
        }

        public IEnumerable<HeadingItem> GetHeadingsByName(string name)
        {
            return _allHeadings.Where(h => h.Title.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        private void BuildIndex(List<string> lines)
        {
            var startTime = DateTime.Now;
            
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var headingMatch = _headingRegex.Match(line);
                if (headingMatch.Success)
                {
                    var level = headingMatch.Groups[1].Length;
                    if (level is >= 2 and <= 6) // Only index ## to ######
                    {
                        _allHeadings.Add(new HeadingItem(i, line.Trim(), level));
                    }
                }
                
                IndexWordsInLine(line, i);
            }
            
            var duration = DateTime.Now - startTime;
            AnsiConsole.MarkupLine($"[dim]Indexed {lines.Count} lines, {_allHeadings.Count} headings in {duration.TotalMilliseconds:F0}ms[/]");
        }

        private void BuildHeadingTree()
        {
            _headingNodes.Clear();
            
            // Create nodes for all headings
            foreach (var heading in _allHeadings)
            {
                _headingNodes[heading.LineIndex] = new HeadingNode(heading);
            }
            
            // Sort headings by line index to process them in document order
            var sortedHeadings = _allHeadings.OrderBy(h => h.LineIndex).ToList();
            
            // Stack to keep track of the current parent for each heading level.
            // headingLevelParents[0] is not used, headingLevelParents[1] for #, headingLevelParents[2] for ##, etc.
            var headingLevelParents = new HeadingNode[7]; // Max 6 levels for Markdown headings

            foreach (var currentHeading in sortedHeadings)
            {
                var currentNode = _headingNodes[currentHeading.LineIndex];
                
                // Find the appropriate parent: look backwards for a heading with a lower level
                HeadingNode? parentNode = null;
                for (var j = currentHeading.Level - 1; j >= 1; j--)
                {
                    if (headingLevelParents[j] == null) continue;
                    parentNode = headingLevelParents[j];
                    break;
                }

                if (parentNode != null)
                {
                    parentNode.Children.Add(currentNode);
                    currentNode.Parent = parentNode;
                }
                // If no parent found, it's a top-level heading (e.g., the first ## in the document)
                // We don't explicitly store top-level nodes in a separate list here,
                // but they can be found by traversing the tree from nodes with Parent == null.

                // Update the current heading for this level
                headingLevelParents[currentHeading.Level] = currentNode;

                // Clear out any lower-level headings, as they are no longer in scope under the current heading
                for (var k = currentHeading.Level + 1; k <= 6; k++)
                {
                    headingLevelParents[k] = null;
                }
            }
        }

        private void IndexWordsInLine(string line, int lineIndex)
        {
            // Regex to remove punctuation and split words, then convert to lowercase
            var words = Regex.Matches(line, @"\b\w+\b")
                             .Select(m => m.Value.ToLowerInvariant())
                             .Where(w => w.Length > 2) // Filter out words with 2 or fewer characters
                             .ToList();

            foreach (var word in words.Where(word => !string.IsNullOrWhiteSpace(word)))
            {
                if (!_wordIndex.TryGetValue(word, out var value))
                {
                    value = [];
                    _wordIndex[word] = value;
                }

                value.Add(lineIndex);
            }
        }
    }
}
