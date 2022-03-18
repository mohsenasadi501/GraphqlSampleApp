using GraphqlSampleApp.Api.Models.User;
using GraphqlSampleApp.Api.Repositories;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;

namespace GraphqlSampleApp.Api.Types
{
    public class Query
    {
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseSorting]
        [UseFiltering]
        //[Authorize(Roles= new[] {"admin","super-admin"})]
        //[Authorize(Policy="roles-policy")]
        //[Authorize(Policy="claim-policy-1")]
        //[Authorize(Policy = "claim-policy-2")]
        [Authorize]
        public IExecutable<User> GetUsers([Service] IUserRepository userRepository) => userRepository.GetUser();

        [UseFirstOrDefault]
        public IExecutable<User> GetUserById([Service] IUserRepository userRepository, [ID] Guid id) => userRepository.GetUserById(id);
    }
}
