#nullable enable

using Rodentia.Core.Entities;

namespace Rodentia.Core.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, int take, CancellationToken cancellationToken = default);

    Task<int> CountUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
