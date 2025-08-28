namespace scrbl.Managers
{
    internal class NotesFileManager(string filePath)
    {
        public void AppendContent(string content)
        {
            File.AppendAllText(filePath, content);
        }

        public string[] GetLastLines(int count = 10)
        {
            if (!File.Exists(filePath))
                return Array.Empty<string>();

            var lines = File.ReadAllLines(filePath);
            return lines.TakeLast(count).ToArray();
        }

        public bool AppendToTodaysSection(string content)
        {
            if (!File.Exists(filePath))
                return false;

            var lines = File.ReadAllLines(filePath).ToList();
            var today = DateTime.Now.ToString("yyyy.MM.dd");
            var todayHeader = $"## {today}";

            var todayIndex = lines.FindIndex(line => line.Contains(todayHeader));
            
            if (todayIndex == -1)
            {
                AppendContent(content);
                return false;
            }

            var insertIndex = lines.Count;
            for (int i = todayIndex + 1; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "---" || lines[i].StartsWith("## "))
                {
                    insertIndex = i;
                    break;
                }
            }

            lines.Insert(insertIndex, content.TrimEnd());
            File.WriteAllLines(filePath, lines);
            return true;
        }

        public FileInfo GetFileInfo()
        {
            return new FileInfo(filePath);
        }

        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();
            
            if (!File.Exists(filePath))
            {
                stats["TotalLines"] = 0;
                stats["TotalWords"] = 0;
                stats["TotalCharacters"] = 0;
                stats["NotesToday"] = 0;
                stats["NotesThisWeek"] = 0;
                stats["NotesThisMonth"] = 0;
                return stats;
            }

            var lines = File.ReadAllLines(filePath);
            var content = File.ReadAllText(filePath);
            
            stats["TotalLines"] = lines.Length;
            stats["TotalWords"] = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            stats["TotalCharacters"] = content.Length;

            var today = DateTime.Now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var dateHeaders = lines
                .Where(line => line.StartsWith("## ") && line.Length > 3)
                .Select(line => line.Substring(3).Trim())
                .Where(dateStr => DateTime.TryParseExact(dateStr, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out _))
                .Select(dateStr => DateTime.ParseExact(dateStr, "yyyy.MM.dd", null))
                .ToList();

            stats["NotesToday"] = dateHeaders.Count(date => date.Date == today);
            stats["NotesThisWeek"] = dateHeaders.Count(date => date.Date >= startOfWeek);
            stats["NotesThisMonth"] = dateHeaders.Count(date => date.Date >= startOfMonth);

            return stats;
        }
    }
}
