using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookifyHotel.Model
{
    public class RolePermission
    {
        public string RoleId { get; set; } // ✅ string (من IdentityRole)
        public int PermissionId { get; set; }

        [ForeignKey("RoleId")]
        public virtual IdentityRole Role { get; set; } // ✅ IdentityRole

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }
    }
}
