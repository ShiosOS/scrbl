using Spectre.Console;
using System.Text.RegularExpressions;

namespace scrbl.Managers
{
    internal class NotesFileManager(string filePath)
    {
        private readonly string _indexPath = GetIndexPath(filePath);
        private List<string> _lines = File.Exists(filePath) 
            ? File.ReadAllText(filePath).Split(["\r\n", "\n"], StringSplitOptions.None).ToList()
            : [];
        private NotesIndex? _index;

        public bool AddToLastHeader(string content)
        {
            var index = GetIndex();
            var lastHeader = index.GetLastHeader(level: 2); 
            if (lastHeader == null)
                return false;

            var insertIndex = FindContentInsertionPoint(lastHeader.LineIndex);
            _lines.Insert(insertIndex, content);
            
            index.InsertLineAt(insertIndex, content);
            SaveUpdatedIndex();
            
            return true;
        }

        public bool AddToSection(string sectionName, string content, int targetLevel = 3)
        {
            var index = GetIndex();
            var lastHeader = index.GetLastHeader(level: 2); 
            if (lastHeader == null)
                return false;

            var formattedSectionHeader = FormatSectionHeader(sectionName, targetLevel);
            
            var section = index.FindHeadingUnderParent(lastHeader, formattedSectionHeader, targetLevel);
            
            if (section == null)
                return false;

            var insertIndex = FindContentInsertionPoint(section.LineIndex);
            _lines.Insert(insertIndex, content);
            
            index.InsertLineAt(insertIndex, content);
            SaveUpdatedIndex();
            
            return true;
        }

        public void AppendTemplate(string templateContent)
        {
            File.AppendAllText(filePath, templateContent);
            
            _lines = File.ReadAllText(filePath).Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
            
            InvalidateIndex();
        }

        public void Save()
        {
            File.WriteAllText(filePath, string.Join(Environment.NewLine, _lines));
            UpdateIndexTimestamp();
        }

        private void UpdateIndexTimestamp()
        {
            var persistentIndex = NotesIndexManager.LoadIndex(_indexPath);
            if (persistentIndex == null) return;
            
            persistentIndex.FileTimestamp = File.GetLastWriteTime(filePath);
            NotesIndexManager.SaveIndex(_indexPath, persistentIndex);
        }

        private NotesIndex GetIndex()
        {
            if (_index != null)
                return _index;

            var fileTimestamp = File.GetLastWriteTime(filePath);
            var persistentIndex = NotesIndexManager.LoadIndex(_indexPath);

            // Check if persistent index exists and is up-to-date
            if (persistentIndex != null && persistentIndex.FileTimestamp >= fileTimestamp)
            {
                AnsiConsole.MarkupLine("[dim]Using cached index...[/]");
                _index = NotesIndex.FromPersistent(persistentIndex);
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]Building index...[/]");
                _index = new NotesIndex(_lines);
                SaveUpdatedIndex(); 
            }

            return _index;
        }

        public void InvalidateIndex()
        {
            if (_index != null)
            {
                AnsiConsole.MarkupLine("[yellow]DEBUG: Index before invalidation:[/]");
                AnsiConsole.MarkupLine($"[dim]Headings: {string.Join(", ", _index.GetAllHeadings().Select(h => $"L{h.LineIndex}:'{h.Title}' (Level {h.Level})"))}[/]");
            }
            
            _index = null;
        }
        
        private void SaveUpdatedIndex()
        {
            if (_index == null) return;
            var fileTimestamp = DateTime.Now; 
            NotesIndexManager.SaveIndex(_indexPath, new PersistentNotesIndex
            {
                FileTimestamp = fileTimestamp,
                Headings = _index.GetAllHeadings().ToList(),
                WordIndex = _index.GetWordIndex()
            });
        }

        private int FindContentInsertionPoint(int headerIndex)
        {
            var insertIndex = headerIndex + 1;
            
            // Find the next line that starts with any heading (##, ###, ####, etc.)
            // This ensures content is inserted before the next structural element.
            while (insertIndex < _lines.Count)
            {
                // Check if the line matches any heading pattern (##, ###, ####, etc.)
                if (Regex.IsMatch(_lines[insertIndex], @"^#+\s")) 
                {
                    break; // Found the next heading, so insert before it
                }
                insertIndex++;
            }
            
            return insertIndex;
        }

        private static string FormatSectionHeader(string sectionName, int level)
        {
            if (level is < 1 or > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Heading level must be between 1 and 6.");
            }
            return $"{new string('#', level)} {sectionName.Trim()}";
        }

        private static string GetIndexPath(string notesPath)
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT 
                ? $"{notesPath}.scrbl-index"
                : $"{notesPath}.index";
        }
    }
}
