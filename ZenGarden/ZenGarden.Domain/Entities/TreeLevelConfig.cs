using System.ComponentModel.DataAnnotations;

namespace ZenGarden.Domain.Entities;

public class TreeLevelConfig
{
    [Key]
    public int Level { get; set; } 

    public int XpRequired { get; set; }
}