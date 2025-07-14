using BCrypt.Net;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Models;

namespace ExpenseTrackingSystem.Services
{
    public class EncryptionService : IEncryptionService
    {
        public virtual async Task<EncryptModel> EncryptData(EncryptModel data)
        {
            // Hash the data using BCrypt
            var hashedData = BCrypt.Net.BCrypt.HashPassword(data.Data);

            return new EncryptModel
            {
                Data = data.Data,
                EncryptedData = hashedData
            };
        }

        public bool VerifyData(string rawData, string hashedData)
        {
            return BCrypt.Net.BCrypt.Verify(rawData, hashedData);
        }
    }
}

