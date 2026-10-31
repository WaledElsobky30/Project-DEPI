using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required] public string FullName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MaxLength(15)] public string PhoneNumber { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; }
    [Required, DataType(DataType.Password), Compare("Password")] public string ConfirmPassword { get; set; }
}

