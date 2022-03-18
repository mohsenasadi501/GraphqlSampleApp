using GraphqlSampleApp.Api.Models.User;

namespace GraphqlSampleApp.Api.Repositories
{
    public interface IUserRoleRepository
    {
        IList<UserRole> GetRoleById(Guid id);
    }
}
