namespace GraphqlSampleApp.Api.Utilities
{
    public class JwtSettings
    {
        public string Secret { set; get; }
        /// <summary>
        /// ExpDate is in Minutes
        /// </summary>
        public int AccessTokenExpMinute { set; get; }
        public int RefreshTokenExpMinute { set; get; }
        public string Issuer { set; get; }
        public string Audience { set; get; }
    }
}
