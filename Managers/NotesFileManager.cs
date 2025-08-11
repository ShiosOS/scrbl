namespace scrbl.Managers
{
    internal class NotesFileManager(string filePath)
    {
        public void AppendContent(string content)
        {
            File.AppendAllText(filePath, content);
        }
    }
}
