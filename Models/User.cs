﻿namespace Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public User(string userName, string passwordHash)
        {
            UserName = userName;
            PasswordHash = passwordHash;
        }
    }
}
