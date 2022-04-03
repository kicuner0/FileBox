namespace FileBox.Models
{
    public class FileItem
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; }
        public long? Size { get; set; }
    }
}
