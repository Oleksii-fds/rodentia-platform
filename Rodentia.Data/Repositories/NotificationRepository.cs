#nullable enable

using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;

namespace Rodentia.Data.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly RodentiaDbContext _dbContext;

    public NotificationRepository(RodentiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
