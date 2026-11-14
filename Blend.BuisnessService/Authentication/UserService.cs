using Blend.BuisnessEntities;

namespace Blend.BuisnessService.Authentication
{
    public sealed class UserService : IUserLookupService
    {
        public UserEntity FindByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            var mapper = new DataMapper<UserEntity>(
                "GetUserByUserName",
                new UserEntity { UserName = userName.Trim() });

            return mapper.Object();
        }
    }
}
