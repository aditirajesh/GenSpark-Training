using FileHandlerApplication.Models;

namespace FileHandlerApplication.Interfaces
{
    public interface IFileRepository
    {
        void SaveFile(FileData fileData);
        FileData GetFile(string fileName);
    }
}