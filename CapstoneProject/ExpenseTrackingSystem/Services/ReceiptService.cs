using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.Extensions.Logging; // Add this using statement

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
        private readonly ILogger<ReceiptService> _logger; // Add this field

        public ReceiptService(IRepository<Guid, Receipt> receiptRepository,
                              IRepository<Guid, Expense> expenseRepository,
                              IRepository<string, User> userRepository,
                              IAuditLogService auditLogService,
                              ILogger<ReceiptService> logger, // Add logger parameter
                              string storagePath = "/Users/aditirajesh/Desktop/GenSpark-Training/CapstoneProject/Receipts")
        {
            _receiptRepository = receiptRepository;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _receiptMapper = new();
            _storagePath = storagePath;
            _auditLogService = auditLogService;
            _logger = logger; // Assign logger

            _logger.LogDebug("Initializing ReceiptService with storage path: {StoragePath}", storagePath);

            if (!Directory.Exists(storagePath))
            {
                _logger.LogInformation("Creating storage directory: {StoragePath}", storagePath);
                Directory.CreateDirectory(storagePath);
            }
            else
            {
                _logger.LogDebug("Storage directory already exists: {StoragePath}", storagePath);
            }
        }

        public async Task<Receipt> CreateReceipt(ReceiptAddRequestDto dto)
        {
            _logger.LogInformation("Starting receipt creation for expense {ExpenseId} by user {Username}", 
                dto?.ExpenseId, dto?.Username);

            if (dto == null) 
            {
                _logger.LogWarning("Receipt DTO is null during creation attempt");
                throw new ArgumentNullException(nameof(dto), "Receipt cannot be null");
            }

            _logger.LogDebug("Mapping receipt DTO for expense {ExpenseId}", dto.ExpenseId);
            var receipt = _receiptMapper.MapAddRequestReceipt(dto);
            if (receipt == null) 
            {
                _logger.LogError("Receipt mapping failed for expense {ExpenseId}", dto.ExpenseId);
                throw new EntityCreationException("Receipt mapping failed");
            }

            var fileExtension = Path.GetExtension(dto.ReceiptBill.FileName);
            var safeFileName = $"{receipt.ReceiptName}{fileExtension}";
            var filePath = Path.Combine(_storagePath, safeFileName);

            _logger.LogDebug("Saving receipt file to: {FilePath} (Original: {OriginalFileName})", 
                filePath, dto.ReceiptBill.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ReceiptBill.CopyToAsync(stream);
                }
                _logger.LogDebug("Successfully saved receipt file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save receipt file to: {FilePath}", filePath);
                throw new EntityCreationException($"Failed to save receipt file: {ex.Message}");
            }

            receipt.FilePath = filePath;
            receipt.CreatedAt = DateTimeOffset.UtcNow;

            _logger.LogDebug("Looking up expense {ExpenseId} for receipt association", receipt.ExpenseId);
            var expense = await _expenseRepository.GetByID(receipt.ExpenseId);
            if (expense == null)
            {
                _logger.LogWarning("Expense {ExpenseId} not found during receipt creation", receipt.ExpenseId);
                throw new EntityNotFoundException("Expense not found");
            }
            
            // Check if user exists (handle nullable username)
            User? user = null;
            if (!string.IsNullOrEmpty(receipt.Username))
            {
                _logger.LogDebug("Looking up user {Username} for receipt association", receipt.Username);
                user = await _userRepository.GetByID(receipt.Username);
                if (user != null)
                {
                    _logger.LogDebug("Associating receipt with user {Username}", receipt.Username);
                    user.Receipts ??= new List<Receipt>();
                    user.Receipts.Add(receipt);
                    receipt.User = user;
                }
                else
                {
                    _logger.LogWarning("User {Username} not found during receipt creation", receipt.Username);
                }
            }
            else
            {
                _logger.LogDebug("No username provided for receipt association");
            }

            // Set up the relationships
            _logger.LogDebug("Setting up entity relationships for receipt and expense {ExpenseId}", receipt.ExpenseId);
            expense.Receipt = receipt;
            receipt.Expense = expense;
            receipt.CreatedBy = dto.Username;
            receipt.UpdatedBy = dto.Username;
            receipt.UpdatedAt = DateTimeOffset.UtcNow;

            // Create the receipt first
            _logger.LogDebug("Adding receipt to repository");
            receipt = await _receiptRepository.Add(receipt);
            if (receipt == null)
            {
                _logger.LogError("Failed to add receipt to repository for expense {ExpenseId}", dto.ExpenseId);
                throw new EntityCreationException("Could not create receipt");
            }

            // ✅ REMOVED: No longer need to update expense.ReceiptId since it doesn't exist
            // The relationship is managed through EF navigation properties

            // Update user if it exists
            if (user != null)
            {
                _logger.LogDebug("Updating user {Username} with new receipt association", user.Username);
                await _userRepository.Update(user.Username, user);
            }

            _logger.LogDebug("Logging audit action for receipt creation");
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = dto.Username,
                Action = "Create",
                EntityName = "Receipt",
                Details = $"Created receipt '{receipt.ReceiptName}' for expense ID {receipt.ExpenseId}",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully created receipt {ReceiptId} '{ReceiptName}' for expense {ExpenseId} by user {Username}", 
                receipt.Id, receipt.ReceiptName, receipt.ExpenseId, dto.Username);

            return receipt;
        }

        public async Task<Receipt> DeleteReceipt(Guid receipt_id)
        {
            _logger.LogInformation("Starting receipt deletion for receipt {ReceiptId}", receipt_id);

            var receipt = await _receiptRepository.GetByID(receipt_id);
            if (receipt == null)
            {
                _logger.LogWarning("Receipt {ReceiptId} not found during deletion attempt", receipt_id);
                throw new EntityNotFoundException("Could not find receipt");
            }

            var originalUsername = receipt.Username;
            var originalReceiptName = receipt.ReceiptName;
            var filePath = receipt.FilePath;

            _logger.LogInformation("Found receipt {ReceiptId} '{ReceiptName}' owned by {Username}, proceeding with deletion", 
                receipt_id, originalReceiptName, originalUsername);

            if (File.Exists(receipt.FilePath))
            {
                _logger.LogDebug("Attempting to delete physical file: {FilePath}", receipt.FilePath);
                try
                {
                    File.Delete(receipt.FilePath);
                    Console.WriteLine($"Deleted physical file: {receipt.FilePath}");
                    _logger.LogInformation("Successfully deleted physical receipt file: {FilePath}", receipt.FilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete physical receipt file: {ex.Message}");
                    _logger.LogWarning(ex, "Could not delete physical receipt file: {FilePath}", receipt.FilePath);
                }
            }
            else
            {
                _logger.LogWarning("Physical receipt file not found: {FilePath}", receipt.FilePath);
            }

            _logger.LogDebug("Logging audit action for receipt deletion");
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = originalUsername,
                Action = "Delete",
                EntityName = "Receipt",
                Details = $"Hard deleted receipt '{originalReceiptName}' (ID: {receipt_id})",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogDebug("Performing hard delete of receipt {ReceiptId} from repository", receipt_id);
            var deletedReceipt = await _receiptRepository.Delete(receipt_id);
            Console.WriteLine($"Hard deleted Receipt: {deletedReceipt.Id} - {deletedReceipt.ReceiptName}");

            _logger.LogInformation("Successfully deleted receipt {ReceiptId} '{ReceiptName}' and its physical file", 
                receipt_id, originalReceiptName);
            
            return deletedReceipt;
        }

        public async Task<ReceiptResponseDto> GetReceipt(Guid receipt_id)
        {
            _logger.LogDebug("Retrieving receipt {ReceiptId}", receipt_id);

            var receipt = await _receiptRepository.GetByID(receipt_id);
            if (receipt == null)
            {
                _logger.LogWarning("Receipt {ReceiptId} not found during retrieval", receipt_id);
                throw new EntityNotFoundException("Receipt not found");
            }

            _logger.LogDebug("Found receipt {ReceiptId} '{ReceiptName}', checking file existence: {FilePath}", 
                receipt_id, receipt.ReceiptName, receipt.FilePath);

            if (!File.Exists(receipt.FilePath))
            {
                _logger.LogError("Physical receipt file not found: {FilePath} for receipt {ReceiptId}", 
                    receipt.FilePath, receipt_id);
                throw new FileNotFoundException("No receipt file found");
            }

            _logger.LogDebug("Reading receipt file data from: {FilePath}", receipt.FilePath);

            byte[] fileData;
            try
            {
                fileData = await File.ReadAllBytesAsync(receipt.FilePath);
                _logger.LogDebug("Successfully read {FileSize} bytes from receipt file", fileData.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read receipt file: {FilePath}", receipt.FilePath);
                throw new FileNotFoundException($"Could not read receipt file: {ex.Message}");
            }

            // ✅ FIXED: This should be "Read" action, not "Delete"
            _logger.LogDebug("Logging audit action for receipt retrieval");
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = receipt.Username ?? "System",
                Action = "Read",
                EntityName = "Receipt",
                Details = $"Retrieved receipt '{receipt.ReceiptName}' (ID: {receipt.Id})",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully retrieved receipt {ReceiptId} '{ReceiptName}' ({FileSize} bytes) for user {Username}", 
                receipt_id, receipt.ReceiptName, fileData.Length, receipt.Username);

            return new ReceiptResponseDto
            {
                fileData = fileData,
                ReceiptName = receipt.ReceiptName,
                ExpenseId = receipt.ExpenseId,
                Category = receipt.Category,
                CreatedAt = receipt.CreatedAt,
                Username = receipt.Username
            };
        }

        public async Task<Receipt> UpdateReceipt(ReceiptUpdateRequestDto dto, Guid receipt_id)
        {
            _logger.LogInformation("Starting receipt update for receipt {ReceiptId}", receipt_id);

            var receipt = await _receiptRepository.GetByID(receipt_id);
            if (receipt == null)
            {
                _logger.LogWarning("Receipt {ReceiptId} not found during update attempt", receipt_id);
                throw new EntityNotFoundException("Receipt not found");
            }

            _logger.LogDebug("Found receipt {ReceiptId} '{ReceiptName}', processing updates", 
                receipt_id, receipt.ReceiptName);

            if (!string.IsNullOrWhiteSpace(dto.ReceiptName))
            {
                _logger.LogDebug("Updating receipt name from '{OldName}' to '{NewName}'", 
                    receipt.ReceiptName, dto.ReceiptName);
                receipt.ReceiptName = dto.ReceiptName;
            }

            if (!string.IsNullOrWhiteSpace(dto.Category))
            {
                _logger.LogDebug("Updating receipt category from '{OldCategory}' to '{NewCategory}'", 
                    receipt.Category, dto.Category);
                receipt.Category = dto.Category;
            }

            if (dto.Receipt != null)
            {
                _logger.LogInformation("Receipt file update requested for receipt {ReceiptId}, performing delete and recreate", receipt_id);
                
                // Store the original data before deletion
                var originalExpenseId = receipt.ExpenseId;
                var originalUsername = receipt.Username;
                var originalCategory = receipt.Category;
                var originalReceiptName = receipt.ReceiptName;

                _logger.LogDebug("Storing original receipt data: ExpenseId={ExpenseId}, Username={Username}", 
                    originalExpenseId, originalUsername);

                // Delete the old receipt
                _logger.LogDebug("Deleting old receipt {ReceiptId} before creating new one", receipt_id);
                await DeleteReceipt(receipt_id);
                
                // ✅ FIXED: Create new receipt with updated file and proper extension
                var fileExtension = Path.GetExtension(dto.Receipt.FileName);
                var baseReceiptName = !string.IsNullOrWhiteSpace(dto.ReceiptName) ? dto.ReceiptName : originalReceiptName;
                
                // Remove extension from base name if it exists, then add the new one
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseReceiptName);
                var newReceiptName = $"{nameWithoutExtension}{fileExtension}";

                _logger.LogDebug("Creating new receipt with updated file for expense {ExpenseId}", originalExpenseId);
                var receipt_add_dto = new ReceiptAddRequestDto
                {
                    ReceiptBill = dto.Receipt,
                    ReceiptName = nameWithoutExtension, // Will get extension added in CreateReceipt
                    Username = originalUsername,
                    Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : originalCategory,
                    ExpenseId = originalExpenseId
                };
                receipt = await CreateReceipt(receipt_add_dto);
                _logger.LogInformation("Successfully recreated receipt with new file as {NewReceiptId}", receipt.Id);
            }
            else
            {
                _logger.LogDebug("No file update requested, updating metadata only for receipt {ReceiptId}", receipt_id);
                // Update without changing file
                receipt.UpdatedAt = DateTimeOffset.UtcNow;
                receipt.UpdatedBy = receipt.Username;

                receipt = await _receiptRepository.Update(receipt_id, receipt);
                if (receipt == null)
                {
                    _logger.LogError("Failed to update receipt {ReceiptId} in repository", receipt_id);
                    throw new EntityUpdateException("Could not update receipt");
                }
                _logger.LogDebug("Successfully updated receipt metadata for receipt {ReceiptId}", receipt_id);
            }

            _logger.LogDebug("Logging audit action for receipt update");
            await _auditLogService.LogAction(new AuditAddRequestDto
            {
                Username = receipt.Username ?? "System",
                Action = "Update",
                EntityName = "Receipt",
                Details = $"Updated receipt '{receipt.ReceiptName}' (ID: {receipt.Id})",
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully updated receipt {ReceiptId} '{ReceiptName}'", 
                receipt.Id, receipt.ReceiptName);

            return receipt;
        }
        public async Task<ICollection<Receipt>?> SearchReceipts(ReceiptSearchModel searchModel)
        {
            _logger.LogInformation("Starting receipt search with criteria: Name={Name}, Category={Category}, DateRange={DateRange}", 
                searchModel.ReceiptName, searchModel.Category,
                searchModel.UploadDate != null ? $"{searchModel.UploadDate.MinDate}-{searchModel.UploadDate.MaxDate}" : "None");

            try
            {
                _logger.LogDebug("Retrieving all receipts from repository");
                var receiptsList = (await _receiptRepository.GetAll()).ToList();
                _logger.LogDebug("Retrieved {TotalReceipts} receipts from repository", receiptsList.Count);

                var originalCount = receiptsList.Count;

                receiptsList = FilterByReceiptName(receiptsList, searchModel.ReceiptName);
                _logger.LogDebug("After name filter: {Count} receipts (filtered {Removed})", 
                    receiptsList.Count, originalCount - receiptsList.Count);

                receiptsList = FilterByCategory(receiptsList, searchModel.Category);
                _logger.LogDebug("After category filter: {Count} receipts", receiptsList.Count);

                receiptsList = FilterByUploadDate(receiptsList, searchModel.UploadDate);
                _logger.LogDebug("After date filter: {Count} receipts", receiptsList.Count);

                var finalReceipts = receiptsList.OrderByDescending(r => r.CreatedAt).ToList();
                _logger.LogInformation("Search completed, returning {ResultCount} receipts out of {TotalCount} total", 
                    finalReceipts.Count, originalCount);

                return finalReceipts;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError(ex, "Error occurred during receipt search");
                return null;
            }
        }

        private List<Receipt> FilterByReceiptName(List<Receipt> receipts, string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) 
            {
                _logger.LogTrace("No receipt name filter applied");
                return receipts;
            }
            
            _logger.LogTrace("Filtering receipts by name containing '{Name}'", name);
            return receipts.Where(r => r.ReceiptName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        private List<Receipt> FilterByCategory(List<Receipt> receipts, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) 
            {
                _logger.LogTrace("No category filter applied");
                return receipts;
            }
            
            _logger.LogTrace("Filtering receipts by category containing '{Category}'", category);
            return receipts.Where(r => r.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        private List<Receipt> FilterByUploadDate(List<Receipt> receipts, DateRange? dateRange)
        {
            if (dateRange == null) 
            {
                _logger.LogTrace("No date range filter applied");
                return receipts;
            }

            _logger.LogTrace("Filtering receipts by upload date range {StartDate} - {EndDate}", 
                dateRange.MinDate, dateRange.MaxDate);

            if (dateRange.MinDate.HasValue)
                receipts = receipts.Where(r => r.CreatedAt >= dateRange.MinDate.Value).ToList();

            if (dateRange.MaxDate.HasValue)
                receipts = receipts.Where(r => r.CreatedAt <= dateRange.MaxDate.Value).ToList();

            return receipts;
        }
    }
}