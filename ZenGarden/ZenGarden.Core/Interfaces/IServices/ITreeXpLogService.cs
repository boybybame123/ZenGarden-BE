using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface ITreeXpLogService
    {
        Task<List<TreeXpLog>> GetAllTreeXpLog();
        Task<TreeXpLog> GetTreeXpLogByLogIdAsync(int logId);
        Task<List<TreeXpLog>> GetTreeXpLogByTaskIdAsync(int taskId);

    }
}
