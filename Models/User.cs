using Models.Enums;

namespace Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string? NormalizedName { get; set; }
        public string? PasswordHash { get; set; }
        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public User(string userName)
        {
            UserName = userName;
        }
    }
}
