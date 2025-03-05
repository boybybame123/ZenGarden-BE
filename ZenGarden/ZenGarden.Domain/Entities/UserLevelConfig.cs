using System.ComponentModel.DataAnnotations;

namespace ZenGarden.Domain.Entities;

public class UserLevelConfig
{
    [Key] public int Level { get; set; }

    public int XpRequired { get; set; }
}