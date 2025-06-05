
using System.Security.Cryptography;
using System.Text;
using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Models;

namespace FileHandlerApplication.Services
{
    public class EncryptionService : IEncryptionService
    {
        public virtual async Task<EncryptModel> EncryptData(EncryptModel data)
        {
            byte[] hashKey;
            HMACSHA256 hMACSHA256;

            if (data.HashKey != null)
            {
                hMACSHA256 = new HMACSHA256(data.HashKey);
                hashKey = data.HashKey;
            }
            else
            {
                hMACSHA256 = new HMACSHA256();
                hashKey = hMACSHA256.Key;
            }

            var hash = hMACSHA256.ComputeHash(Encoding.UTF8.GetBytes(data.Data));

            return new EncryptModel
            {
                Data = data.Data,
                EncryptedData = hash,
                HashKey = hashKey
            };
        }
    }
}
