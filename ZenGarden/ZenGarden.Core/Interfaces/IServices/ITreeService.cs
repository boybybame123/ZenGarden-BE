using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface ITreeService
    {
        Task<List<TreeDto>> GetAllTreeAsync();
    }
}
