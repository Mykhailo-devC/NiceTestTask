using Moq;
using System.Reflection;

namespace NiceTestTask.Tests
{
    public class DateNotificationHandlerTests
    {
        private DateNotificationHandler _notificationHandler;
        private Mock<NotificatorConfigHandler> _mockConfigHandler;
        private Mock<DateNotificator> _mockNotificator;

        public DateNotificationHandlerTests()
        {
            _notificationHandler = new DateNotificationHandler();


            _mockConfigHandler = new Mock<NotificatorConfigHandler>();
            _mockConfigHandler.Setup(x => x.GetConfiguration()).ReturnsAsync(new NotificatorConfigData()).Verifiable();
            _mockConfigHandler.Setup(x => x.SubscribeToWatcher(new Mock<FileSystemEventHandler>().Object)).Verifiable();

            _mockNotificator = new Mock<DateNotificator>();
            _mockNotificator.Setup(x => x.SetConfiguration(new NotificatorConfigData())).Verifiable();
            _mockNotificator.Setup(x => x.NotifyDate()).Verifiable();


            _notificationHandler.GetType().GetField("_configHandler", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_notificationHandler, _mockConfigHandler.Object);
            _notificationHandler.GetType().GetField("_notificator", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_notificationHandler, _mockNotificator.Object);

        }

        [Fact]
        public void UpdateNotificator_ValidData_Success()
        {
            // Arrange
            var not = new DateNotificationHandler();
            _notificationHandler = not;
            var updateMethod = _notificationHandler.GetType().GetMethod("UpdateNotificator", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            updateMethod.Invoke(_notificationHandler, null);
            Task.Delay(1000).Wait();

            // Assert
            _mockConfigHandler.Verify(x => x.GetConfiguration(), Times.Once());
        }

        [Fact]
        public void Start_Stop_NotificatorAfterCancellation()
        {
            // Arrange

            // Act
            Task.Run(() =>
            {
                Task.Delay(1000).Wait();
                _notificationHandler.Stop();
            });

            _notificationHandler.Start();

            // Assert
            _mockNotificator.Verify(x => x.NotifyDate());
        }
    }
}
