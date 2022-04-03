namespace FileBoxUI.Models
{
    internal class FileItem
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public long Size { get; set; }
    }
}
