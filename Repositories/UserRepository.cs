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
            //add id and name claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName)
            };
            //add role claims
            claims.AddRange(GetUserRoleClaims(user));
            return claims;
        }

        private static IEnumerable<Claim> GetUserRoleClaims(User user)
            => user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name));

        private static string Hash(string input)
        {
            //create a random salt
            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            //hash the password
            var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, _iterations, _algorithm, _keySize);
            //create a string for saving in db
            return string.Join(_delimiter, Convert.ToHexString(hash), Convert.ToHexString(salt), _iterations, _algorithm);
        }

        private static bool Verify(string input, string hashString)
        {
            //read arguments from dbstring
            var segments = hashString.Split(_delimiter);
            var hash = Convert.FromHexString(segments[0]);
            var salt = Convert.FromHexString(segments[1]);
            var iterations = int.Parse(segments[2]);
            var algorithm = new HashAlgorithmName(segments[3]);
            //hash the given password
            var inputHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algorithm, hash.Length);
            //compare hashes
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
                    //seed data
                    var user = new User("Admin", "85A26076BE2B2A20BE53E548F4B0B1C19E1CFD4682F7922F5A5921BA7C840EF1:A6F1247D796006FBCD1C86052B218709:100000:SHA256") { Id = 1 };
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

                //check given password - no login possible w/o password
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
                //hash given password
                var user = new User(username, Hash(password));

                //fetch and add roles
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

        public void UpdateUser(int id, string username = "", string password = "", List<Role>? roles = null)
        {
            _semaphore.Wait();

            try
            {
                var user = _dbContext.Users.First(x => x.Id == id);

                if (!string.IsNullOrEmpty(username))
                    user.UserName = username;
                if (!string.IsNullOrEmpty(password))
                    user.PasswordHash = Hash(password);

                _dbContext.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}