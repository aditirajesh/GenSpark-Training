using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackingSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<string, User> _userRepository;
        private readonly UserMapper _userMapper;
        private readonly IAuditLogService _auditservice;
        private readonly IRepository<Guid, Expense> _expenseRepository;
        private readonly IReceiptService _receiptService;
        private readonly IRepository<Guid, Receipt> _receiptRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IEncryptionService encryptionService,
                            IRepository<string, User> userRepository,
                            IAuditLogService auditService,
                            IRepository<Guid, Expense> expenseRepository,
                            IReceiptService receiptService,
                            IRepository<Guid, Receipt> receiptRepository,
                            ILogger<UserService> logger) // Add logger parameter
        {
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _userMapper = new();
            _auditservice = auditService;
            _expenseRepository = expenseRepository;
            _receiptService = receiptService;
            _receiptRepository = receiptRepository;
            _logger = logger; // Assign logger
        }

        public async Task<User> CreateUser(UserAddRequestDto dto)
        {
            _logger.LogInformation("Starting user creation for username {Username} with role {Role}", dto?.Username, dto?.Role);

            ArgumentNullException.ThrowIfNull(dto, nameof(dto));

            _logger.LogDebug("Checking if user {Username} already exists", dto.Username);
            var existingUser = await _userRepository.GetByID(dto.Username);
            
            if (existingUser == null)
            {
                _logger.LogDebug("User {Username} does not exist, creating new user", dto.Username);
                
                var user = _userMapper.MapAddRequestUser(dto);
                
                _logger.LogDebug("Encrypting password for user {Username}", dto.Username);
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.Password //encrypting the password
                });
                user.Password = encryptedData.EncryptedData;
                user.CreatedBy = dto.Username;
                user.UpdatedBy = dto.Username;
                user.CreatedAt = DateTimeOffset.UtcNow;
                user.UpdatedAt = DateTimeOffset.UtcNow;

                _logger.LogDebug("Adding user {Username} to repository", dto.Username);
                try
                {
                    user = await _userRepository.Add(user);
                    _logger.LogDebug("Successfully added user {Username} to repository", dto.Username);
                    
                    await _auditservice.LogAction(new AuditAddRequestDto
                    {
                        Action = "Add",
                        EntityName = "User",
                        Details = "User added to database",
                        Username = dto.Username,
                        Timestamp  = DateTimeOffset.UtcNow
                    });
                    
                    _logger.LogInformation("Successfully created new user {Username} with role {Role}", dto.Username, dto.Role);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create user {Username} in repository", dto.Username);
                    throw new EntityCreationException($"User could not be created: {ex.Message}");
                }
                return user;
            }
            else if (existingUser != null && existingUser.IsDeleted)
            {
                _logger.LogInformation("User {Username} exists but is deleted, reactivating account", dto.Username);
                
                existingUser.IsDeleted = false;
                
                _logger.LogDebug("Encrypting new password for reactivated user {Username}", dto.Username);
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.Password
                });
                existingUser.Password = encryptedData.EncryptedData;
                existingUser.Role = dto.Role;
                existingUser.Phone = dto.Phone;
                existingUser.CreatedAt = DateTimeOffset.UtcNow;
                
                _logger.LogDebug("Updating reactivated user {Username} in repository", dto.Username);
                var added_user = await _userRepository.Update(existingUser.Username, existingUser);
                
                await _auditservice.LogAction(new AuditAddRequestDto
                {
                    Action = "Add",
                    EntityName = "User",
                    Details = $"User added to database: {dto.Username}",
                    Username = dto.Username,
                    Timestamp  = DateTimeOffset.UtcNow
                });
                
                _logger.LogInformation("Successfully reactivated user {Username} with new role {Role}", dto.Username, dto.Role);
                return added_user;
            }
            
            _logger.LogWarning("Failed to create user {Username} - username already taken", dto.Username);
            throw new DuplicateEntityException("Username already taken");
        }

        public async Task<User> DeleteUser(string username, string deletedBy)
        {
            _logger.LogInformation("Starting user deletion for {Username} by {DeletedBy}", username, deletedBy);

            var user = await _userRepository.GetByID(username);
            if (user == null)
            {
                _logger.LogWarning("User {Username} not found during deletion attempt", username);
                throw new EntityNotFoundException("User not found in database");
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("User {Username} is already deleted", username);
                throw new InvalidOperationException("User is already deleted");
            }

            var actualDeletedBy = deletedBy ?? username;
            var expenseCount = user.Expenses?.Count ?? 0;
            var receiptCount = user.Receipts?.Count ?? 0;

            _logger.LogInformation("Deleting user {Username} with {ExpenseCount} expenses and {ReceiptCount} receipts", 
                username, expenseCount, receiptCount);

            user.IsDeleted = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = actualDeletedBy;

            _logger.LogDebug("Updating user {Username} with deletion flag", username);
            await _userRepository.Update(username, user);

            await _auditservice.LogAction(new AuditAddRequestDto
            {
                Action = "Soft Delete",
                EntityName = "User",
                Details = $"Soft deleted user: {username}. Database automatically hard deleted {expenseCount} expenses and {receiptCount} receipts via cascade.",
                Username = actualDeletedBy,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully soft deleted user {Username} by {DeletedBy}, {ExpenseCount} expenses and {ReceiptCount} receipts will be cascade deleted", 
                username, actualDeletedBy, expenseCount, receiptCount);

            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            _logger.LogDebug("Retrieving user {Username}", username);

            var user = await _userRepository.GetByID(username);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("User {Username} not found or is deleted", username);
                throw new EntityNotFoundException("Could not find user");
            }
            
            _logger.LogDebug("Successfully retrieved user {Username} with role {Role}", username, user.Role);
            return user;
        }

        public async Task<ICollection<User>> GetAllUsers()
        {
            _logger.LogDebug("Retrieving all users from repository");

            var users = await _userRepository.GetAll();
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No users found in repository");
                throw new CollectionEmptyException("No users present");
            }
            
            var userList = users.ToList();
            _logger.LogInformation("Successfully retrieved {UserCount} users from repository", userList.Count);
            return userList;
        }

        public async Task<User> UpdateUserDetails(string username, UserUpdateRequestDto dto, bool isAdmin, bool isUpdatingSelf)
        {
            using var scope = _logger.BeginScope("UserUpdate for {Username} by {UpdateType}", username, isAdmin ? "Admin" : "User");
            
            _logger.LogInformation("Starting user update for {Username}, isAdmin: {IsAdmin}, isUpdatingSelf: {IsUpdatingSelf}", 
                username, isAdmin, isUpdatingSelf);

            var existingUser = await _userRepository.GetByID(username);
            
            if (existingUser == null || existingUser.IsDeleted == true)
            {
                _logger.LogWarning("User {Username} not found or is deleted during update attempt", username);
                throw new EntityNotFoundException("User not found");
            }

            if (string.IsNullOrWhiteSpace(dto.Password) && 
                string.IsNullOrWhiteSpace(dto.Phone) && 
                string.IsNullOrWhiteSpace(dto.Role))
            {
                _logger.LogWarning("No fields provided for update for user {Username}", username);
                throw new ArgumentException("At least one field (password, phone, or role) must be provided for update");
            }

            var fieldsToUpdate = new List<string>();

            // Role update logic
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                _logger.LogDebug("Role update requested for user {Username} from {OldRole} to {NewRole}", 
                    username, existingUser.Role, dto.Role);
                
                if (!isAdmin)
                {
                    _logger.LogWarning("Non-admin user attempted to change role for user {Username}", username);
                    throw new UnauthorizedAccessException("Only administrators can change user roles");
                }
                
                if (isUpdatingSelf)
                {
                    _logger.LogWarning("Admin {Username} attempted to change their own role", username);
                    throw new InvalidOperationException("Administrators cannot change their own role");
                }
                
                _logger.LogInformation("Admin updating role for user {Username} from {OldRole} to {NewRole}", 
                    username, existingUser.Role, dto.Role);
                existingUser.Role = dto.Role;
                fieldsToUpdate.Add("role");
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogDebug("Password update requested for user {Username}", username);
                var encrypted = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.Password
                });
                existingUser.Password = encrypted.EncryptedData;
                fieldsToUpdate.Add("password");
                _logger.LogDebug("Password successfully encrypted for user {Username}", username);
            }

            // Phone update
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                _logger.LogDebug("Phone update requested for user {Username} from {OldPhone} to {NewPhone}", 
                    username, existingUser.Phone, dto.Phone);
                existingUser.Phone = dto.Phone;
                fieldsToUpdate.Add("phone");
            }

            existingUser.UpdatedBy = username;
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;

            _logger.LogDebug("Updating user {Username} in repository with fields: {UpdatedFields}", 
                username, string.Join(", ", fieldsToUpdate));

            var updatedUser = await _userRepository.Update(username, existingUser);
            
            var updatedFields = new List<string>();
            if (!string.IsNullOrWhiteSpace(dto.Password)) updatedFields.Add("password");
            if (!string.IsNullOrWhiteSpace(dto.Phone)) updatedFields.Add("phone");
            if (!string.IsNullOrWhiteSpace(dto.Role)) updatedFields.Add("role");
            
            await _auditservice.LogAction(new AuditAddRequestDto
            {
                Action = "Update",
                EntityName = "User",
                Details = $"Updated {string.Join(", ", updatedFields)} for user {username}",
                Username = username,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Successfully updated user {Username} with fields: {UpdatedFields}", 
                username, string.Join(", ", updatedFields));

            return updatedUser;
        }

        public async Task<ICollection<User>> SearchUsers(UserSearchModel searchModel)
        {
            _logger.LogInformation("Starting user search with criteria: Username={Username}, Role={Role}, CreatedAtRange={CreatedAtRange}", 
                searchModel.Username, searchModel.Role,
                searchModel.CreatedAtRange != null ? $"{searchModel.CreatedAtRange.MinVal}-{searchModel.CreatedAtRange.MaxVal}" : "None");

            _logger.LogDebug("Retrieving all non-deleted users from repository");
            var users = (await _userRepository.GetAll()).Where(u => !u.IsDeleted).ToList();
            _logger.LogDebug("Retrieved {TotalUsers} non-deleted users from repository", users.Count);

            var originalCount = users.Count;

            users = FilterByUsername(users, searchModel.Username);
            _logger.LogDebug("After username filter: {Count} users (filtered {Removed})", 
                users.Count, originalCount - users.Count);

            users = FilterByRole(users, searchModel.Role);
            _logger.LogDebug("After role filter: {Count} users", users.Count);

            users = FilterByCreatedAt(users, searchModel.CreatedAtRange);
            _logger.LogDebug("After creation date filter: {Count} users", users.Count);

            var finalUsers = users.OrderByDescending(u => u.CreatedAt).ToList();
            _logger.LogInformation("Search completed, returning {ResultCount} users out of {TotalCount} total", 
                finalUsers.Count, originalCount);

            return finalUsers;
        }

        private List<User> FilterByUsername(List<User> users, string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogTrace("No username filter applied");
                return users;
            }
            
            _logger.LogTrace("Filtering users by username containing '{Username}'", username);
            return users.Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<User> FilterByRole(List<User> users, string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                _logger.LogTrace("No role filter applied");
                return users;
            }
            
            _logger.LogTrace("Filtering users by role containing '{Role}'", role);
            return users.Where(u => u.Role != null && u.Role.Contains(role, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<User> FilterByCreatedAt(List<User> users, Range<DateTimeOffset>? createdAtRange)
        {
            if (createdAtRange == null)
            {
                _logger.LogTrace("No creation date range filter applied");
                return users;
            }

            _logger.LogTrace("Filtering users by creation date range {StartDate} - {EndDate}", 
                createdAtRange.MinVal, createdAtRange.MaxVal);

            if (createdAtRange.MinVal.HasValue)
                users = users.Where(u => u.CreatedAt >= createdAtRange.MinVal.Value).ToList();

            if (createdAtRange.MaxVal.HasValue)
                users = users.Where(u => u.CreatedAt <= createdAtRange.MaxVal.Value).ToList();

            return users;
        }
    }
}