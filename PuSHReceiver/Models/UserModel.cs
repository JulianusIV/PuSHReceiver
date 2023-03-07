namespace PuSHReceiver.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        [Obsolete("Use ctor with Username and Password args instead")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public UserModel() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public UserModel(string username)
        {
            Username = username;
        }
    }
}
