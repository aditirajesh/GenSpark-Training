using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Misc;
using FileHandlerApplication.Models;
using Microsoft.AspNetCore.SignalR;

namespace FileHandlerApplication.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly Dictionary<string, FileData> _filestorage = new();
        public void SaveFile(FileData fileData)
        {
            if (fileData == null)
            {
                throw new Exception("File is null");
            }
            _filestorage[fileData.FileName] = fileData;
        }

        public FileData GetFile(string name)
        {
            if (!_filestorage.ContainsKey(name))
            {
                throw new Exception("No file with that name found");
            }
            return _filestorage[name];
        }
    }
}