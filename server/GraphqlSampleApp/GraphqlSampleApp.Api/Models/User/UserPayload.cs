namespace GraphqlSampleApp.Api.Models.User
{
    public class UserPayload
    {
        public record CreateUserPayload(User user);
        public record DeleteUserPayload(bool isSuccessfull);
        public record UpdateUserPayload(bool isSuccessfull);
        public record UserTokenPayload(string Message, string AccessToken, string RefreshToken);
    }
}
