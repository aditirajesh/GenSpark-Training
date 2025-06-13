using ExpenseTrackingSystem.Exceptions;
using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Mappers;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;

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

        public UserService(IEncryptionService encryptionService,
                            IRepository<string, User> userRepository,
                            IAuditLogService auditService,
                            IRepository<Guid, Expense> expenseRepository,
                            IReceiptService receiptService,
                            IRepository<Guid, Receipt> receiptRepository
                            )
        {
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _userMapper = new();
            _auditservice = auditService;
            _expenseRepository = expenseRepository;
            _receiptService = receiptService;
            _receiptRepository = receiptRepository;
        }

        public async Task<User> CreateUser(UserAddRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));

            var existingUser = await _userRepository.GetByID(dto.Username);
            if (existingUser == null)
            {
                var user = _userMapper.MapAddRequestUser(dto);
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.Password //encrypting the password
                });
                user.Password = encryptedData.EncryptedData;
                user.CreatedBy = dto.Username;
                user.UpdatedBy = dto.Username;
                user.CreatedAt = DateTimeOffset.UtcNow;
                user.UpdatedAt = DateTimeOffset.UtcNow;


                try
                {
                    user = await _userRepository.Add(user);
                    await _auditservice.LogAction(new AuditAddRequestDto
                    {
                        Action = "Add",
                        EntityName = "User",
                        Details = "User added to database",
                        Username = dto.Username,
                        Timestamp  = DateTimeOffset.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    throw new EntityCreationException($"User could not be created: {ex.Message}");
                }
                return user;
            }
            else if (existingUser != null && existingUser.IsDeleted)
            {
                existingUser.IsDeleted = false;
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.Password
                });
                existingUser.Password = encryptedData.EncryptedData;
                existingUser.Role = dto.Role;
                existingUser.Phone = dto.Phone;
                existingUser.CreatedAt = DateTimeOffset.UtcNow;
                var added_user = await _userRepository.Update(existingUser.Username, existingUser);
                await _auditservice.LogAction(new AuditAddRequestDto
                {
                    Action = "Add",
                    EntityName = "User",
                    Details = $"User added to database: {dto.Username}",
                    Username = dto.Username,
                    Timestamp  = DateTimeOffset.UtcNow
                });
                return added_user;
            }
            throw new DuplicateEntityException("Username already taken");

        }

        public async Task<User> DeleteUser(string username, string deletedBy)
        {
            var user = await _userRepository.GetByID(username)
                    ?? throw new EntityNotFoundException("User not found in database");

            if (user.IsDeleted)
                throw new InvalidOperationException("User is already deleted");

            var actualDeletedBy = deletedBy ?? username;

            var expenseCount = user.Expenses?.Count ?? 0;
            var receiptCount = user.Receipts?.Count ?? 0;


            user.IsDeleted = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = actualDeletedBy;

            await _userRepository.Update(username, user);

            await _auditservice.LogAction(new AuditAddRequestDto
            {
                Action = "Soft Delete",
                EntityName = "User",
                Details = $"Soft deleted user: {username}. Database automatically hard deleted {expenseCount} expenses and {receiptCount} receipts via cascade.",
                Username = actualDeletedBy,
                Timestamp = DateTimeOffset.UtcNow
            });

            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var user = await _userRepository.GetByID(username);
            if (user == null || user.IsDeleted)
            {
                throw new EntityNotFoundException("Could not find user");
            }
            return user;
        }

        public async Task<ICollection<User>> GetAllUsers()
        {
            var users = await _userRepository.GetAll();
            if (users == null || !users.Any())
            {
                throw new CollectionEmptyException("No users present");
            }
            return users.ToList();
        }

        public async Task<User> UpdateUserDetails(string username, UserUpdateRequestDto dto, bool isAdmin, bool isUpdatingSelf)
        {
            var existingUser = await _userRepository.GetByID(username);
            
            if (existingUser == null || existingUser.IsDeleted == true)
            {
                throw new EntityNotFoundException("User not found");
            }

            if (string.IsNullOrWhiteSpace(dto.Password) && 
                string.IsNullOrWhiteSpace(dto.Phone) && 
                string.IsNullOrWhiteSpace(dto.Role))
            {
                throw new ArgumentException("At least one field (password, phone, or role) must be provided for update");
            }

            // Role update logic
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("Only administrators can change user roles");
                }
                
                if (isUpdatingSelf)
                {
                    throw new InvalidOperationException("Administrators cannot change their own role");
                }
                
                existingUser.Role = dto.Role;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var encrypted = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.Password
                });
                existingUser.Password = encrypted.EncryptedData;
            }

            // Phone update
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                existingUser.Phone = dto.Phone;
            }

            existingUser.UpdatedBy = username;
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;

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

            return updatedUser;
        }

        
        public async Task<ICollection<User>> SearchUsers(UserSearchModel searchModel)
        {
            var users = (await _userRepository.GetAll()).Where(u => !u.IsDeleted).ToList();

            users = FilterByUsername(users, searchModel.Username);
            users = FilterByRole(users, searchModel.Role);
            users = FilterByCreatedAt(users, searchModel.CreatedAtRange);

            return users.OrderByDescending(u => u.CreatedAt).ToList();
        }

        private List<User> FilterByUsername(List<User> users, string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return users;
            return users.Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<User> FilterByRole(List<User> users, string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return users;
            return users.Where(u => u.Role != null && u.Role.Contains(role, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<User> FilterByCreatedAt(List<User> users, Range<DateTimeOffset>? createdAtRange)
        {
            if (createdAtRange == null)
                return users;

            if (createdAtRange.MinVal.HasValue)
                users = users.Where(u => u.CreatedAt >= createdAtRange.MinVal.Value).ToList();

            if (createdAtRange.MaxVal.HasValue)
                users = users.Where(u => u.CreatedAt <= createdAtRange.MaxVal.Value).ToList();

            return users;
        }
    }
}