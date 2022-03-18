using MongoDB.Driver;

namespace GraphqlSampleApp.Api.Models.User
{
    public class UserNodeResolver
    {
        public Task<User> ResolveAsync([Service] IMongoCollection<User> collection, Guid id)
        {
            return collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
    }
}
