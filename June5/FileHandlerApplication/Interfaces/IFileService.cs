using FileHandlerApplication.Models;

namespace FileHandlerApplication.Interfaces
{
    public interface IFileService
    {
        Task<FileData> UploadFile(IFormFile file);
        Task<byte[]?> GetFileData(string fileName);
    }
}