using Contracts.DbContext;
using Contracts.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Models;
using System.Security.Claims;
using System.Security.Cryptography;

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

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;

            _semaphore.Wait();

            try
            {
                if (!_dbContext.Users.Any())
                {
                    var user = new User("Admin", "5F23E4F71C3C727AB02B49793EF10A9F2FBD98B62562D658AB585CB399BE23DB:87513C55EB8463A4FFC056E06F612013:100000:SHA256") { Id = 1 };
                    var role = new Role("Administrator") { Id = 1 };
                    _dbContext.Users.Add(user);
                    _dbContext.Roles.Add(role);
                    user.Roles.Add(role);
                    role.Users.Add(user);
                    dbContext.SaveChanges();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public ClaimsPrincipal? GetUserClaims(string username, string password)
        {
            _semaphore.Wait();

            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.UserName == username);
                if (user is null)
                    return null;

                if (string.IsNullOrEmpty(user.PasswordHash) || !Verify(password, user.PasswordHash))
                    return null;

                ClaimsIdentity identity = new(GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
                return new ClaimsPrincipal(identity);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public User GetUser(int id)
        {
            _semaphore.Wait();

            try
            {
                return _dbContext.Users.First(x => x.Id == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void CreateUser(string username, string password, List<Role>? roles = null)
        {
            _semaphore.Wait();

            try
            {
                var user = new User(username, Hash(password));

                if (roles is not null)
                    user.Roles = roles.Select(role => _dbContext.Roles.First(x => role.Id == x.Id)).ToList();

                _dbContext.Users.Add(user);

                _dbContext.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}