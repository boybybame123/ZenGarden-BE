namespace ZenGarden.API.Models
{
    public class RegisterModel
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
