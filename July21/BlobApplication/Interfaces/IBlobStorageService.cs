namespace BlobApplication.Interfaces
{
    public interface IBlobStorageService
    {
        Task UploadFile(Stream fileStream, string fileName);
        Task<Stream> DownloadFile(string fileName);
    }
}