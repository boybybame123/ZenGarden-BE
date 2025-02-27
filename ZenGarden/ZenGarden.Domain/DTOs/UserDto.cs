using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs;
public record UserDto(int UserId = 0, string Email =null, string Phone = null);