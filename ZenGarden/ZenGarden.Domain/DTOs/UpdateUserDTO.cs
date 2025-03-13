using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs
{
    public class UpdateUserDTO
    {
        public int UserId { get; set; }
        public int? RoleId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public UserStatus Status { get; set; }
    }
}
