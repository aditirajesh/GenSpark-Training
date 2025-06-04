namespace FirstAPI.Models
{
    public class EncryptModel
    {
        public string? Data { get; set; }  //the password as a string
        public byte[]? EncryptedData { get; set; } //the password encrypted
        public byte[]? HashKey { get; set; } //salt for encryption
    }
}