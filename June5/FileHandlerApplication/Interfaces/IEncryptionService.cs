using FileHandlerApplication.Models;

namespace FileHandlerApplication.Interfaces
{
    public interface IEncryptionService
    {
        public Task<EncryptModel> EncryptData(EncryptModel data);
    }
}