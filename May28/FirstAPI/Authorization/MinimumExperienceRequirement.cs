using Microsoft.AspNetCore.Authorization;

namespace FirstAPI.Authorization
{
    public class MinimumExperienceRequirement : IAuthorizationRequirement
    {
        public int MinimumYOE { get; }

        public MinimumExperienceRequirement(int minYoe)
        {
            MinimumYOE = minYoe;
        }
    }

}