namespace GraphqlSampleApp.Api.Models.User
{
    public record LoginInput(string UserName, string Password);
    public record RenewTokenInput(string AccessToken, string RefreshToken);

    public record CreateUserInput(
        string UserName, string Password, string Bio, string ProfileImageUrl, string ProfileBannerUrl,
        string EmailAddress, string WalletAddress, string WalletType);

    public record UpdateUserInput(string UserName, string Password, string Bio, string ProfileImageUrl, string ProfileBannerUrl,
        string EmailAddress, string WalletAddress, string WalletType);
}
