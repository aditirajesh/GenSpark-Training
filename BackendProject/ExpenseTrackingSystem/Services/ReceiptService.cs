using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

namespace ExpenseTrackingSystem.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IRepository<Guid, Receipt> _receiptRepository;
        private readonly IRepository<Guid, Expense> _expenseRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly ReceiptMapper _receiptMapper;
        private readonly string _storagePath;
        private readonly IAuditLogService _auditLogService;

        public ReceiptService(IRepository<Guid, Receipt> receiptRepository,
                              IRepository<Guid, Expense> expenseRepository,
                              IRepository<string, User> userRepository,
                              IAuditLogService auditLogService,
                              string storagePath = "/Users/aditirajesh/Desktop/GenSpark-Training/BackendProject/Receipts")
        {
            _receiptRepository = receiptRepository;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _receiptMapper = new();
            _storagePath = storagePath;
            _auditLogService = auditLogService;

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }
        }

        public async Task<Receipt> CreateReceipt(ReceiptAddRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto), "Receipt cannot be null");

            var receipt = _receiptMapper.MapAddRequestReceipt(dto);
            if (receipt == null) throw new EntityCreationException("Receipt mapping failed");

            var fileExtension = Path.GetExtension(dto.ReceiptBill.FileName);
            var safeFileName = $"{receipt.ReceiptName}{fileExtension}";
            var filePath = Path.Combine(_storagePath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ReceiptBill.CopyToAsync(stream);
            }

            receipt.FilePath = filePath;
            receipt.CreatedAt = DateTimeOffset.UtcNow;

            var expense = await _expenseRepository.GetByID(receipt.ExpenseId)
                        ?? throw new EntityNotFoundException("Expense not found");
            
            // Check if user exists (handle nullable username)
            User? user = null;
            if (!string.IsNullOrEmpty(receipt.Username))
            {
                user = await _userRepository.GetByID(receipt.Username);
                if (user != null)
                {
                    user.Receipts ??= new List<Receipt>();
                    user.Receipts.Add(receipt);
                    receipt.User = user;
                }
            }

            // Set up the relationships
            expense.Receipt = receipt;
            receipt.Expense = expense;
            receipt.CreatedBy = dto.Username;
            receipt.UpdatedBy = dto.Username;
            receipt.UpdatedAt = DateTimeOffset.UtcNow;

            // Create the receipt first
            receipt = await _receiptRepository.Add(receipt)
                    ?? throw new EntityCreationException("Could not create receipt");

            // ✅ REMOVED: No longer need to update expense.ReceiptId since it doesn't exist
            // The relationship is managed through EF navigation properties

            // Update user if it exists
            if (user != null)
            {
                await _userRepository.Update(user.Username, user);
            }

            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = dto.Username,
                Action = "Create",
                EntityName = "Receipt",
                Details = $"Created receipt '{receipt.ReceiptName}' for expense ID {receipt.ExpenseId}",
                Timestamp = DateTimeOffset.UtcNow
            });

            return receipt;
        }

        public async Task<Receipt> DeleteReceipt(Guid receipt_id)
        {
            var receipt = await _receiptRepository.GetByID(receipt_id)
                        ?? throw new EntityNotFoundException("Could not find receipt");

            var originalUsername = receipt.Username;
            var originalReceiptName = receipt.ReceiptName;

            if (File.Exists(receipt.FilePath))
            {
                try
                {
                    File.Delete(receipt.FilePath);
                    Console.WriteLine($"Deleted physical file: {receipt.FilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete physical receipt file: {ex.Message}");
                }
            }

            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = originalUsername,
                Action = "Delete",
                EntityName = "Receipt",
                Details = $"Hard deleted receipt '{originalReceiptName}' (ID: {receipt_id})",
                Timestamp = DateTimeOffset.UtcNow
            });

            var deletedReceipt = await _receiptRepository.Delete(receipt_id);
            Console.WriteLine($"Hard deleted Receipt: {deletedReceipt.Id} - {deletedReceipt.ReceiptName}");
            
            return deletedReceipt;
        }

        public async Task<ReceiptResponseDto> GetReceipt(Guid receipt_id)
        {
            var receipt = await _receiptRepository.GetByID(receipt_id)
                           ?? throw new EntityNotFoundException("Receipt not found");

            if (!File.Exists(receipt.FilePath))
            {
                throw new FileNotFoundException("No receipt file found");
            }

            // ✅ FIXED: This should be "Read" action, not "Delete"
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = receipt.Username ?? "System",
                Action = "Read",
                EntityName = "Receipt",
                Details = $"Retrieved receipt '{receipt.ReceiptName}' (ID: {receipt.Id})",
                Timestamp = DateTimeOffset.UtcNow
            });

            return new ReceiptResponseDto
            {
                fileData = await File.ReadAllBytesAsync(receipt.FilePath),
                ReceiptName = receipt.ReceiptName,
                ExpenseId = receipt.ExpenseId,
                Category = receipt.Category,
                CreatedAt = receipt.CreatedAt,
                Username = receipt.Username
            };
        }

        public async Task<Receipt> UpdateReceipt(ReceiptUpdateRequestDto dto, Guid receipt_id)
        {
            var receipt = await _receiptRepository.GetByID(receipt_id)
                           ?? throw new EntityNotFoundException("Receipt not found");

            if (!string.IsNullOrWhiteSpace(dto.ReceiptName))
                receipt.ReceiptName = dto.ReceiptName;

            if (!string.IsNullOrWhiteSpace(dto.Category))
                receipt.Category = dto.Category;

            if (dto.Receipt != null)
            {
                // Store the original data before deletion
                var originalExpenseId = receipt.ExpenseId;
                var originalUsername = receipt.Username;
                var originalCategory = receipt.Category;
                var originalReceiptName = receipt.ReceiptName;

                // Delete the old receipt
                await DeleteReceipt(receipt_id);
                
                // Create new receipt with updated file
                var receipt_add_dto = new ReceiptAddRequestDto
                {
                    ReceiptBill = dto.Receipt,
                    ReceiptName = !string.IsNullOrWhiteSpace(dto.ReceiptName) ? dto.ReceiptName : originalReceiptName,
                    Username = originalUsername,
                    Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : originalCategory,
                    ExpenseId = originalExpenseId
                };
                receipt = await CreateReceipt(receipt_add_dto);
            }
            else
            {
                // Update without changing file
                receipt.UpdatedAt = DateTimeOffset.UtcNow;
                receipt.UpdatedBy = receipt.Username;

                receipt = await _receiptRepository.Update(receipt_id, receipt)
                            ?? throw new EntityUpdateException("Could not update receipt");
            }

            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = receipt.Username ?? "System",
                Action = "Update",
                EntityName = "Receipt",
                Details = $"Updated receipt '{receipt.ReceiptName}' (ID: {receipt.Id})",
                Timestamp = DateTimeOffset.UtcNow
            });

            return receipt;
        }

        public async Task<ICollection<Receipt>?> SearchReceipts(ReceiptSearchModel searchModel)
        {
            try
            {
                var receiptsList = (await _receiptRepository.GetAll()).ToList();

                receiptsList = FilterByReceiptName(receiptsList, searchModel.ReceiptName);
                receiptsList = FilterByCategory(receiptsList, searchModel.Category);
                receiptsList = FilterByUploadDate(receiptsList, searchModel.UploadDate);

                return receiptsList.OrderByDescending(r => r.CreatedAt).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private List<Receipt> FilterByReceiptName(List<Receipt> receipts, string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return receipts;
            return receipts.Where(r => r.ReceiptName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        private List<Receipt> FilterByCategory(List<Receipt> receipts, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return receipts;
            return receipts.Where(r => r.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        private List<Receipt> FilterByUploadDate(List<Receipt> receipts, DateRange? dateRange)
        {
            if (dateRange == null) return receipts;

            if (dateRange.MinDate.HasValue)
                receipts = receipts.Where(r => r.CreatedAt >= dateRange.MinDate.Value).ToList();

            if (dateRange.MaxDate.HasValue)
                receipts = receipts.Where(r => r.CreatedAt <= dateRange.MaxDate.Value).ToList();

            return receipts;
        }
    }
}