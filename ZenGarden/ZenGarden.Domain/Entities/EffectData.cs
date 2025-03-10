using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.Entities
{
    public class EffectData
    {
        public double? XpMultiplier { get; set; } // Hệ số tăng XP (nếu có)
        public string ProtectionDuration { get; set; } // Thời gian bảo vệ XP (nếu có)
    }

}
