using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface INotificationService
    {
        Task PushNotificationAsync(int userId, string title, string content);

    }
}
