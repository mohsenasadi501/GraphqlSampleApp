using GraphqlSampleApp.Api.Models.User;
using GraphqlSampleApp.Api.Utilities;
using HotChocolate.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static GraphqlSampleApp.Api.Models.User.UserPayload;

namespace GraphqlSampleApp.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _user;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRepository(IMongoClient client, IOptions<JwtSettings> jwtSettings, IUserRoleRepository userRoleRepository)
        {
            var database = client.GetDatabase("NFTDB");
            var collection = database.GetCollection<User>(nameof(User));
            _user = collection;
            _userRoleRepository = userRoleRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public IExecutable<User> GetUserById([ID(null)] Guid id)
        {
            return _user.Find(x => x.Id == id).AsExecutable();
        }
        public IExecutable<User> GetUser()
        {
            return _user.AsExecutable();
        }
        public User CreateUser(CreateUserInput createUserSettingInput)
        {
            var item = new User
            {
                Bio = createUserSettingInput.Bio,
                EmailAddress = createUserSettingInput.EmailAddress,
                ProfileBannerUrl = createUserSettingInput?.ProfileBannerUrl,
                ProfileImageUrl = createUserSettingInput?.ProfileImageUrl,
                UserName = createUserSettingInput?.UserName,
                Password = HashPassword(createUserSettingInput.Password),
            };
            _user.InsertOne(item);
            return item;
        }
        public bool DeleteUser(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(c => c.Id, id);
            var result = _user.DeleteOne(filter);

            return result.DeletedCount == 1;
        }
        public bool UpdateUser(Guid id, UpdateUserInput updateUserSettingInput)
        {
            var filter = Builders<User>.Filter.Eq(c => c.Id, id);
            var update = Builders<User>.Update
                .Set(c => c.ProfileBannerUrl, updateUserSettingInput.ProfileBannerUrl)
                .Set(c => c.Bio, updateUserSettingInput.Bio)
                .Set(c => c.EmailAddress, updateUserSettingInput.EmailAddress)
                .Set(c => c.UserName, updateUserSettingInput.UserName)
                .Set(c => c.ProfileImageUrl, updateUserSettingInput.ProfileImageUrl);

            var result = _user.UpdateOne(filter, update);
            return result.ModifiedCount == 1;
        }
        public UserTokenPayload Login(LoginInput loginInput)
        {
            string Message = "Success";
            if (string.IsNullOrEmpty(loginInput.UserName)
            || string.IsNullOrEmpty(loginInput.Password))
            {
                Message = "Invalid Credentials";
                return new UserTokenPayload(Message, "", "");
            }

            var user = _user.Find(x => x.UserName == loginInput.UserName).FirstOrDefault();
            if (user == null)
            {
                Message = "Invalid Credentials";
                return new UserTokenPayload(Message, "", "");
            }

            if (!ValidatePasswordHash(loginInput.Password, user.Password))
            {
                Message = "Invalid Credentials";
                return new UserTokenPayload(Message, "", "");
            }
            var roles = _userRoleRepository.GetRoleById(user.Id);

            var userTokenPayload = new UserTokenPayload(Message, GenerateToken(user, roles), GenerateRefreshToken());

            user.RefreshToken = userTokenPayload.RefreshToken;
            user.RefreshTokenExpiration = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpMinute);

            UpdateRefreshToken(user);

            return userTokenPayload;
        }
        public UserTokenPayload RenewAccessToken(RenewTokenInput renewTokenInput)
        {
            string Message = "Success";
            if (string.IsNullOrEmpty(renewTokenInput.AccessToken)
            || string.IsNullOrEmpty(renewTokenInput.RefreshToken))
            {
                Message = "Invalid Token";
                return new UserTokenPayload(Message, "", "");
            }

            ClaimsPrincipal principal = GetClaimsFromExpiredToken(renewTokenInput.AccessToken);

            if (principal == null)
            {
                Message = "Invalid Token";
                return new UserTokenPayload(Message, "", "");
            }

            string userName = principal.Claims.Where(_ => _.Type == "UserName").Select(_ => _.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(userName))
            {
                Message = "Invalid Token";
                return new UserTokenPayload(Message, "", "");
            }

            var user = _user.Find(x => x.UserName == userName && x.RefreshToken == renewTokenInput.RefreshToken && x.RefreshTokenExpiration > DateTime.Now).FirstOrDefault();
            if (user == null)
            {
                Message = "Invalid Token";
                return new UserTokenPayload(Message, "", "");
            }

            var userRoles = _userRoleRepository.GetRoleById(user.Id);

            var userTokenPayload = new UserTokenPayload(Message, GenerateToken(user, userRoles), GenerateRefreshToken());

            user.RefreshToken = userTokenPayload.RefreshToken;
            user.RefreshTokenExpiration = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpMinute);

            UpdateRefreshToken(user);

            return userTokenPayload;
        }

        private bool UpdateRefreshToken(User user)
        {
            var filter = Builders<User>.Filter.Eq(c => c.Id, user.Id);

            var update = Builders<User>.Update
                .Set(c => c.RefreshToken, user.RefreshToken)
                .Set(c => c.RefreshTokenExpiration, user.RefreshTokenExpiration);

            var result = _user.UpdateOne(filter, update);
            return result.ModifiedCount == 1;
        }
        private bool ValidatePasswordHash(string password, string dbPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(dbPassword);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
        private string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }
        private string GenerateToken(User user, IList<UserRole> roles)
        {
            var securtityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(securtityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("UserName", user.UserName),
                new Claim("Email", user.EmailAddress)
            };
            if ((roles?.Count ?? 0) > 0 && roles != null)
            {
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpMinute),
                signingCredentials: credentials,
                claims: claims
            );
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        private ClaimsPrincipal GetClaimsFromExpiredToken(string accessToken)
        {
            var tokenValidationParameter = new TokenValidationParameters
            {
                ValidIssuer = _jwtSettings.Issuer,
                ValidateIssuer = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // ignore expiration
            };

            var jwtHandler = new JwtSecurityTokenHandler();
            var principal = jwtHandler.ValidateToken(accessToken, tokenValidationParameter, out SecurityToken securityToken);

            var jwtScurityToken = securityToken as JwtSecurityToken;
            if (jwtScurityToken == null)
            {
                return null;
            }

            return principal;
        }
    }
}
