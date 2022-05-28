using GraphqlSampleApp.Api.Models.User;

namespace GraphqlSampleApp.Api.Types
{
    public class Subscription
    {
        [Topic]
        [Subscribe]
        public User SubscribeUser([EventMessage] User user)
        {
            return user;
        }
    }
}
