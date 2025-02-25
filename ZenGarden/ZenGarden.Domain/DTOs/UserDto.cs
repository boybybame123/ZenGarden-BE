using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        
        // Các thuộc tính khác mà bạn muốn trả về cho client
        // Lưu ý: KHÔNG bao gồm password hoặc các thông tin nhạy cảm
    }
}
