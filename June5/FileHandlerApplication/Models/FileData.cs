namespace FileHandlerApplication.Models
{
    public class FileData
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}