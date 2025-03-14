using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Domain.DTOs
{
    public class TreeXpConfigDto
    {
        public int LevelId { get; set; }
        public int XpThreshold { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserTreeDto> UserTrees { get; set; } = new List<UserTreeDto>();
    }
}
