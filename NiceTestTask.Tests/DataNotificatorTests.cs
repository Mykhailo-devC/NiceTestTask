using System.Reflection;

namespace NiceTestTask.Tests
{
    public class DataNotificatorTests
    {
        private DateNotificator _notificator;
        public DataNotificatorTests()
        {
            _notificator = new DateNotificator();
        }

        [Theory]
        [InlineData("yyyy-MM-dd", 1)]
        [InlineData("yyyy-MM-dd", null)]
        [InlineData(null, 5)]
        [InlineData(null, null)]
        public void SetConfiguration_UpdatePrivateFieldsValues_Success(string dateFormat, int interval)
        {
            // Arrange
            var configData = new NotificatorConfigData { DataFormat = dateFormat, IntervalInSeconds = interval };
            _notificator.SetConfiguration(configData);

            // Act
            var dateFormatField = typeof(DateNotificator).GetField("_dateFormat", BindingFlags.NonPublic | BindingFlags.Instance);
            var dateFormatValue = dateFormatField.GetValue(_notificator);

            var intervalField = typeof(DateNotificator).GetField("_interval", BindingFlags.NonPublic | BindingFlags.Instance);
            var intervalValue = intervalField.GetValue(_notificator);

            // Assert
            Assert.Equal(configData.DataFormat, dateFormatValue);
            Assert.Equal(configData.IntervalInSeconds, intervalValue);
        }

        [Fact]
        public void SetConfiguration_UpdatePrivateFieldsValuesWithNull_ThrowsNullReferenceException()
        {
            // Arrange
            NotificatorConfigData configData = null;

            // Assert
            Assert.Throws<NullReferenceException>(() => _notificator.SetConfiguration(configData));
        }

        [Theory]
        [InlineData("yyyy-MM-dd", 1)]
        [InlineData(null, 0)]
        public void NotifyDate_NotificationGeneratedWithValidData_Success(string dateFormat, int interval)
        {
            // Arrange
            var configData = new NotificatorConfigData { DataFormat = dateFormat, IntervalInSeconds = interval };
            _notificator.SetConfiguration(configData);

            // Act
            string notification = ExecuteAndWaitForNotification(_notificator);

            // Assert
            Assert.NotNull(notification);
        }

        [Theory]
        [InlineData(null, -1)]
        [InlineData(null, -10)]
        public void NotifyDate_NotificationGeneratedWithInvalidInterval_Success(string dateFormat, int interval)
        {
            // Arrange
            var configData = new NotificatorConfigData { DataFormat = dateFormat, IntervalInSeconds = interval };
            _notificator.SetConfiguration(configData);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ExecuteAndWaitForNotification(_notificator)); ;
        }

        private string ExecuteAndWaitForNotification(DateNotificator notificator)
        {
            string notification = null;
            var consoleOut = new StringWriter();
            Console.SetOut(consoleOut);

            notificator.NotifyDate();

            consoleOut.Flush();
            var output = consoleOut.ToString().Trim();
            if (output.StartsWith("New notification - "))
            {
                notification = output.Substring("New notification - ".Length);
            }

            return notification;
        }
    }
}