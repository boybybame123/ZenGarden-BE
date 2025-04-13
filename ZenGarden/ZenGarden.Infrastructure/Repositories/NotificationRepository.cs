using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public  class NotificationRepository(ZenGardenContext context) : GenericRepository<Notification>(context), INotificationRepository
{
}