using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.Entities
{
    public class XPConfig
    {
       
        public int XPConfigId { get; set; }

        
        public int FocusMethodId { get; set; }

        
        public int TaskTypeId { get; set; }

        public double BaseXP { get; set; }
        public double Multiplier { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual required FocusMethod FocusMethod { get; set; }
        public virtual required TaskType TaskType { get; set; }
    }
}
