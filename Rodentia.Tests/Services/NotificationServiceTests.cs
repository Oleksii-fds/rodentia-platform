#nullable enable

using Moq;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Services;
using Xunit;

namespace Rodentia.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _service = new NotificationService(_repositoryMock.Object);
    }

    [Fact]
    public async Task NotifyAsync_ShouldFail_WhenUserIsEmpty()
    {
        var result = await _service.NotifyAsync(Guid.Empty, "Title", "Body");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task NotifyAsync_ShouldSave_WhenValid()
    {
        var result = await _service.NotifyAsync(Guid.NewGuid(), "Title", "Body", NotificationType.General);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUnreadAsync_ShouldReturnItems()
    {
        var userId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetUnreadByUserIdAsync(userId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, Title = "A", Message = "B", Type = NotificationType.General, CreatedAt = DateTime.UtcNow }
            });

        var result = await _service.GetUnreadAsync(userId, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldFail_WhenNotificationNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification)null!);

        var result = await _service.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldMarkRead_WhenOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsRead = false,
            Title = "t",
            Message = "m"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await _service.MarkAsReadAsync(userId, notification.Id);

        Assert.True(result.IsSuccess);
        Assert.True(notification.IsRead);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
