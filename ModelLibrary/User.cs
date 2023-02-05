using Models.Enums;

namespace Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public PermissionsEnum Permissions { get; set; }

        public User(string name, string passwordHash, string salt)
        {
            Name = name;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
