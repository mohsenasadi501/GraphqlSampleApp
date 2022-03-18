namespace GraphqlSampleApp.Api.Models.User
{
    [Node(
   IdField = nameof(Id),
   NodeResolverType = typeof(UserNodeResolver),
   NodeResolver = nameof(UserNodeResolver.ResolveAsync))]

    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ProfileBannerUrl { get; set; }
        public string EmailAddress { get; set; }
        public Dictionary<string, string> Links { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
