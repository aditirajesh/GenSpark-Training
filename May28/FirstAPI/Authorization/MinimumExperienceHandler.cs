using System.Security.Claims;
using FirstAPI.Interfaces;
using FirstAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace FirstAPI.Authorization
{
    public class MinimumExperienceHandler : AuthorizationHandler<MinimumExperienceRequirement>
    {
        private readonly IRepository<string, User> _userrepository;

        public MinimumExperienceHandler(IRepository<string, User> userRepository)
        {
            _userrepository = userRepository;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumExperienceRequirement requirement)
        {
            var username = context.User.Identity?.Name;
            Console.WriteLine($"Username from token: {username}");


            if (username.IsNullOrEmpty())
            {
                Console.WriteLine("No username found in token");

                throw new Exception("User not found");
            }
            var user = await _userrepository.GetByID(username);
            Console.WriteLine($"User fetched: {user?.Username}, Role: {user?.Role}, YOE: {user?.Doctor?.YearsOfExperience}");

            if (user.Role == "Doctor" && user.Doctor?.YearsOfExperience >= requirement.MinimumYOE)
            {
                Console.WriteLine("Authorization success");
                context.Succeed(requirement);
            }
            else
            {
                Console.WriteLine("Authorization failed");
            }
        }
    }
}
