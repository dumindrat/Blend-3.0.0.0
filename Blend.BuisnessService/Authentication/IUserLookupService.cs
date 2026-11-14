using Blend.BuisnessEntities;

namespace Blend.BuisnessService.Authentication
{
    public interface IUserLookupService
    {
        UserEntity FindByUserName(string userName);
    }
}
