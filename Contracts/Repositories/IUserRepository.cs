using Microsoft.AspNetCore.Http;
using Models;

namespace Contracts.Repositories
{
    public interface IUserRepository
    {
        public User GetUser(int id);
        public void SignIn(HttpContext httpContext, string username, string password, bool isPersistent = false);
        public void SignOut(HttpContext httpContent);
    }
}
