using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookifyHotel.Model
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public virtual ICollection<RolePermission>? RolePermissions { get; set; } 
    }
}
