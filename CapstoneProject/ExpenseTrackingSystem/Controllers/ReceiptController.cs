using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Misc;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using ExpenseTrackingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/receipts")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        private readonly ILogger<ReceiptController> _logger;
        private readonly IRepository<Guid, Receipt> _receiptRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<Guid, Expense> _expenseRepository;
        private readonly ReceiptMapper _receiptMapper;
        private readonly string _storagePath;

        public ReceiptController(IReceiptService receiptService, ILogger<ReceiptController> logger,
                                IRepository<Guid, Receipt> receiptRepository, IRepository<string, User> userRepository,
                                ReceiptMapper receiptMapper, IRepository<Guid, Expense> expenseRepository,
                                string storagePath = "/Users/aditirajesh/Desktop/GenSpark-Training/BackendProject/Receipts"
                                )
        {
            _receiptService = receiptService;
            _logger = logger;
            _receiptRepository = receiptRepository;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _receiptMapper = receiptMapper;
            _storagePath = storagePath;
        }

        // ✅ NEW: Download receipt file with proper headers
        [Authorize]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadReceipt(Guid id)
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("Download receipt request for receipt {ReceiptId} by user {Username}", id, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receiptResponse = await _receiptService.GetReceipt(id);
                
                // Security check
                if (!User.IsInRole("Admin") && receiptResponse.Username != username)
                {
                    _logger.LogWarning("Unauthorized receipt download attempt - user {Username} tried to download receipt {ReceiptId} owned by {ReceiptOwner}",
                        username, id, receiptResponse.Username);
                    return Unauthorized("You can only download your own receipts.");
                }

                var contentType = GetContentType(receiptResponse.ReceiptName);
                var fileName = receiptResponse.ReceiptName;

                _logger.LogInformation("Serving receipt file {FileName} ({FileSize} bytes) with content-type {ContentType}",
                    fileName, receiptResponse.fileData.Length, contentType);

                // Return file with proper download headers
                return File(receiptResponse.fileData, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Receipt file not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading receipt {ReceiptId} for user {Username}", id, username);
                return StatusCode(500, "An error occurred while downloading the receipt.");
            }
        }

        // ✅ NEW: Preview receipt in browser (inline viewing)
        [Authorize]
        [HttpGet("preview/{id}")]
        public async Task<IActionResult> PreviewReceipt(Guid id)
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("Preview receipt request for receipt {ReceiptId} by user {Username}", id, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receiptResponse = await _receiptService.GetReceipt(id);
                
                // Security check
                if (!User.IsInRole("Admin") && receiptResponse.Username != username)
                {
                    _logger.LogWarning("Unauthorized receipt preview attempt - user {Username} tried to preview receipt {ReceiptId} owned by {ReceiptOwner}",
                        username, id, receiptResponse.Username);
                    return Unauthorized("You can only preview your own receipts.");
                }

                var contentType = GetContentType(receiptResponse.ReceiptName);
                
                _logger.LogInformation("Serving receipt preview {FileName} ({FileSize} bytes) with content-type {ContentType}",
                    receiptResponse.ReceiptName, receiptResponse.fileData.Length, contentType);

                // Return file for inline viewing (no download prompt)
                Response.Headers.Add("Content-Disposition", "inline");
                return File(receiptResponse.fileData, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Receipt file not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing receipt {ReceiptId} for user {Username}", id, username);
                return StatusCode(500, "An error occurred while previewing the receipt.");
            }
        }

        // ✅ ENHANCED: Get receipt metadata only (no binary data)
        [Authorize]
        [HttpGet("info/{id}")]
        public async Task<ActionResult<ReceiptMetadataDto>> GetReceiptInfo(Guid id)
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("Get receipt info request for receipt {ReceiptId} by user {Username}", id, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receiptResponse = await _receiptService.GetReceipt(id);
                
                if (!User.IsInRole("Admin") && receiptResponse.Username != username)
                    return Unauthorized("You can only access your own receipts.");

                // Return metadata only (no file data)
                var metadata = new ReceiptMetadataDto
                {
                    Id = id,
                    ReceiptName = receiptResponse.ReceiptName,
                    Category = receiptResponse.Category,
                    CreatedAt = receiptResponse.CreatedAt,
                    Username = receiptResponse.Username,
                    ExpenseId = receiptResponse.ExpenseId,
                    FileSizeBytes = receiptResponse.fileData.Length,
                    ContentType = GetContentType(receiptResponse.ReceiptName)
                };

                return Ok(metadata);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Receipt not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt info {ReceiptId} for user {Username}", id, username);
                return StatusCode(500, "An error occurred while getting receipt information.");
            }
        }

        // ✅ YOUR EXISTING ENDPOINTS (kept as-is for backward compatibility)
        [Authorize]
        [HttpPost("create")]
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

            // ✅ FIXED: Preserve file extension in receipt name
            var fileExtension = Path.GetExtension(dto.ReceiptBill.FileName);
            var receiptNameWithExtension = $"{receipt.ReceiptName}{fileExtension}";
            var filePath = Path.Combine(_storagePath, receiptNameWithExtension);

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

            // ✅ FIXED: Store the filename WITH extension in the database
            receipt.ReceiptName = receiptNameWithExtension;
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

            // Update user if it exists
            if (user != null)
            {
                _logger.LogDebug("Updating user {Username} with new receipt association", user.Username);
                await _userRepository.Update(user.Username, user);
            }

            _logger.LogDebug("Logging audit action for receipt creation");
            _logger.LogInformation("Successfully created receipt {ReceiptId} '{ReceiptName}' for expense {ExpenseId} by user {Username}",
                receipt.Id, receipt.ReceiptName, receipt.ExpenseId, dto.Username);

            return receipt;
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Receipt>> Update(Guid id, [FromForm] ReceiptUpdateRequestDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receipt = await _receiptService.GetReceipt(id);
                if (receipt == null)
                    return NotFound("Receipt not found.");

                if (!User.IsInRole("Admin") && receipt.Username != username)
                    return Unauthorized("You are not authorized to update this receipt.");

                var result = await _receiptService.UpdateReceipt(dto, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<Receipt>> Delete(Guid id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receipt = await _receiptService.GetReceipt(id);
                if (receipt == null)
                    return NotFound("Receipt not found.");

                if (!User.IsInRole("Admin") && receipt.Username != username)
                    return Unauthorized("You are not authorized to delete this receipt.");

                var result = await _receiptService.DeleteReceipt(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ UPDATED: This now returns Receipt entities, not file data
        [Authorize]
        [HttpGet("get/{id}")]
        public async Task<ActionResult<Receipt>> Get(Guid id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                // This returns the Receipt entity from repository, not file data
                var receipt = await _receiptService.GetReceipt(id);
                if (receipt == null)
                    return NotFound("Receipt not found.");

                if (!User.IsInRole("Admin") && receipt.Username != username)
                    return Unauthorized("You are not authorized to view this receipt.");

                // Note: This returns ReceiptResponseDto which contains file data
                // Consider using GetReceiptInfo endpoint instead for metadata only
                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("search")]
        public async Task<ActionResult<ICollection<Receipt>>?> Search([FromBody] ReceiptSearchModel model)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var result = await _receiptService.SearchReceipts(model);
                if (!User.IsInRole("Admin"))
                    result = result?.Where(r => r.Username == username).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<ICollection<Receipt>>> GetAllReceipts(
            [FromQuery] string? username,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var requester = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(requester))
                    return Unauthorized("Username not found in token.");

                if (!string.IsNullOrWhiteSpace(username) && username != requester && !User.IsInRole("Admin"))
                    return Unauthorized("You are not authorized to access other users' receipts.");

                var userToQuery = string.IsNullOrWhiteSpace(username) ? requester : username;

                var receipts = await _receiptService.SearchReceipts(new ReceiptSearchModel());
                var filtered = receipts?
                                .Where(r => r.Username == userToQuery)
                                .OrderByDescending(r => r.CreatedAt)
                                .Paginate(pageNumber, pageSize)
                                .ToList();

                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ NEW: Helper method for content type detection
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }
    }
}