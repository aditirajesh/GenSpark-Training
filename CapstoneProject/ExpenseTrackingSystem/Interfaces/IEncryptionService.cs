using ExpenseTrackingSystem.Models;

namespace ExpenseTrackingSystem.Interfaces
{
    public interface IEncryptionService
    {
        public Task<EncryptModel> EncryptData(EncryptModel data);
        public bool VerifyData(string user_ip, string password);
    }
}