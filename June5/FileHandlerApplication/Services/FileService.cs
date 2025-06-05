using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Misc;
using FileHandlerApplication.Models;
using Microsoft.AspNetCore.SignalR;

namespace FileHandlerApplication.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly string _storagePath;
        private readonly IHubContext<NotificationHub> _hubContext;


        public FileService(IFileRepository fileRepository,
                            IHubContext<NotificationHub> hubContext,
                            string storagePath = "/Users/aditirajesh/Desktop/GenSpark-Training/June5/FileHandlerApplication/Files")
        {
            _fileRepository = fileRepository;
            _storagePath = storagePath;
            _hubContext = hubContext;

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<byte[]?> GetFileData(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<FileData> UploadFile(IFormFile file)
        {
            var filePath = Path.Combine(_storagePath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var metadata = new FileData
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            _fileRepository.SaveFile(metadata);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "System", $"New file uploaded: {file.FileName}");

            return metadata;
        }
    }
}