namespace FileHandlerApplication.Models
{
    public class EncryptModel
    {
        public string Data { get; set; } = string.Empty;
        public byte[]? EncryptedData { get; set; }
        public byte[]? HashKey { get; set; }
    }
}