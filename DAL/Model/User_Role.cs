using BookifyHotel.Model;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class User_Role
{
    public string RoleId { get; set; } // ✅ string (من IdentityRole)
    public int UserProfileId { get; set; }

    [ForeignKey("RoleId")]
    public virtual IdentityRole Role { get; set; } // ✅ IdentityRole

    [ForeignKey("UserProfileId")]
    public virtual UserProfile UserProfile { get; set; }
}