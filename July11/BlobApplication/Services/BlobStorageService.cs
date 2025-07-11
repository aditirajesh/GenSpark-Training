using Azure.Storage.Blobs;

namespace BlobApplication.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClinet;
        public BlobStorageService(IConfiguration configuration)
        {
            var sasUrl = configuration["AzureBlob:ContainerSasUrl"];
            _containerClinet = new BlobContainerClient(new Uri(sasUrl));
        }

        public async Task UploadFile(Stream fileStream,string fileName)
        {
            var blobClient = _containerClinet.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream,overwrite:true);
        }

        public async Task<Stream> DownloadFile(string fileName)
        {
            var blobClient = _containerClinet?.GetBlobClient(fileName);
            if(await blobClient.ExistsAsync())
            {
                var downloadInfor = await blobClient.DownloadStreamingAsync();
                return downloadInfor.Value.Content;
            }
            return null;
        }
    }
}