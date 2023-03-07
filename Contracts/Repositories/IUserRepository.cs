using Models;
using System.Security.Claims;

namespace Contracts.Repositories
{
    public interface IUserRepository
    {
        public User GetUser(int id);
        public ClaimsPrincipal? GetUserClaims(string username, string password);
        public void CreateUser(string username, string password, List<Role>? roles = null);
    }
}
