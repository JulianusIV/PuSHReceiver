using Contracts.DbContext;
using Contracts.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private const int _saltSize = 16; //128 bits
        private const int _keySize = 32; //256 bits
        private const int _iterations = 100000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;
        private const char _delimiter = ':';

        private readonly IDbContext _dbContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async void SignIn(HttpContext httpContext, string username, string password, bool isPersistent = false)
        {
            _semaphore.Wait();

            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == username);
                if (user is null)
                    return;

                if (!Verify(password, user.PasswordHash))
                    return;

                ClaimsIdentity identity = new(GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new(identity);

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async void SignOut(HttpContext httpContext)
            => await httpContext.SignOutAsync();

        private static IEnumerable<Claim> GetUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName)
            };
            claims.AddRange(GetUserRoleClaims(user));
            return claims;
        }

        private static IEnumerable<Claim> GetUserRoleClaims(User user)
        {
            foreach (var role in user.Roles)
                yield return new(ClaimTypes.Role, role.Name);
        }

        private static string Hash(string input)
        {
            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, _iterations, _algorithm, _keySize);
            return string.Join(_delimiter, Convert.ToHexString(hash), Convert.ToHexString(salt), _iterations, _algorithm);
        }

        private static bool Verify(string input, string hashString)
        {
            var segments = hashString.Split(_delimiter);
            var hash = Convert.FromHexString(segments[0]);
            var salt = Convert.FromHexString(segments[1]);
            var iterations = int.Parse(segments[2]);
            var algorithm = new HashAlgorithmName(segments[3]);
            var inputHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algorithm, hash.Length);
            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }
    }
}