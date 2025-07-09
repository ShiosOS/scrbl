using Spectre.Console;

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
            var lastHeader = index.GetLastHeader();
            if (lastHeader == null)
                return false;

            var insertIndex = FindContentInsertionPoint(lastHeader.LineIndex);
            _lines.Insert(insertIndex, content);
            
            // Update the index incrementally
            index.InsertLineAt(insertIndex, content);
            SaveUpdatedIndex();
            
            return true;
        }

        public bool AddToSection(string sectionName, string content)
        {
            var index = GetIndex();
            var lastHeader = index.GetLastHeader();
            if (lastHeader == null)
                return false;

            var sectionHeader = FormatSectionHeader(sectionName);
            var section = index.FindSectionUnderHeader(lastHeader, sectionHeader);
            if (section == null)
                return false;

            var insertIndex = FindContentInsertionPoint(section.LineIndex);
            _lines.Insert(insertIndex, content);
            
            // Update the index incrementally
            index.InsertLineAt(insertIndex, content);
            SaveUpdatedIndex();
            
            return true;
        }

        public void AppendTemplate(string templateContent)
        {
            File.AppendAllText(filePath, templateContent);
            // Reload lines after template addition
            _lines = File.ReadAllText(filePath).Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
            
            // Template changes are structural - force rebuild
            InvalidateIndex();
        }

        public void Save()
        {
            File.WriteAllText(filePath, string.Join(Environment.NewLine, _lines));
            
            // Update the saved index timestamp to match file
            UpdateIndexTimestamp();
        }

        private void UpdateIndexTimestamp()
        {
            var persistentIndex = NotesIndexManager.LoadIndex(_indexPath);
            if (persistentIndex != null)
            {
                persistentIndex.FileTimestamp = File.GetLastWriteTime(filePath);
                NotesIndexManager.SaveIndex(_indexPath, persistentIndex);
            }
        }

        private NotesIndex GetIndex()
        {
            if (_index != null)
                return _index;

            var fileTimestamp = File.GetLastWriteTime(filePath);
            var persistentIndex = NotesIndexManager.LoadIndex(_indexPath);

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
                AnsiConsole.MarkupLine($"[dim]Headers: {string.Join(", ", _index.GetAllHeaders().Select(h => $"L{h.LineIndex}:'{h.Title}'"))}[/]");
                AnsiConsole.MarkupLine($"[dim]Sections: {string.Join(", ", _index.GetAllSections().Select(s => $"L{s.LineIndex}:'{s.Title}'"))}[/]");
            }
            
            // Just mark in-memory index as stale - don't delete the file
            _index = null;
            // Index file stays on disk, will be overwritten on next rebuild
        }
        
        private void SaveInitialIndex()
        {
            if (_index == null) return;
            var fileTimestamp = File.GetLastWriteTime(filePath);
            NotesIndexManager.SaveIndex(_indexPath, new PersistentNotesIndex
            {
                FileTimestamp = fileTimestamp,
                Headers = _index.GetAllHeaders().ToList(),
                Sections = _index.GetAllSections().ToList(),
                WordIndex = _index.GetWordIndex()
            });
        }
        
        private void SaveUpdatedIndex()
        {
            if (_index == null) return;
            // Save the updated index with current timestamp
            var fileTimestamp = DateTime.Now; // Will be updated when Save() is called
            NotesIndexManager.SaveIndex(_indexPath, new PersistentNotesIndex
            {
                FileTimestamp = fileTimestamp,
                Headers = _index.GetAllHeaders().ToList(),
                Sections = _index.GetAllSections().ToList(),
                WordIndex = _index.GetWordIndex()
            });
        }


        private int FindContentInsertionPoint(int headerIndex)
        {
            var insertIndex = headerIndex + 1;
            
            while (insertIndex < _lines.Count && 
                   !_lines[insertIndex].StartsWith("## ") && 
                   !_lines[insertIndex].StartsWith("### "))
            {
                insertIndex++;
            }
            
            return insertIndex;
        }

        private static string FormatSectionHeader(string sectionName)
        {
            var hashCount = 0;
            var cleanName = sectionName;
            
            while (cleanName.StartsWith("#"))
            {
                hashCount++;
                cleanName = cleanName.Substring(1);
            }
            
            cleanName = cleanName.Trim();
            var totalHashes = 3 + hashCount;
            var prefix = new string('#', totalHashes);
            
            return $"{prefix} {cleanName}";
        }

        private static string GetIndexPath(string notesPath)
        {
            // Use visible filename on Windows, hidden on Unix-like systems
            return Environment.OSVersion.Platform == PlatformID.Win32NT 
                ? $"{notesPath}.scrbl-index"
                : $"{notesPath}.index";
        }
    }
}