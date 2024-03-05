using Microsoft.VisualBasic;
using Moq;
using Newtonsoft.Json;
using System.Reflection;

namespace NiceTestTask.Tests
{
    public class NotificatorConfigHandlerTests
    {
        private NotificatorConfigHandler _configHandler;
        public NotificatorConfigHandlerTests()
        {
            _configHandler = new NotificatorConfigHandler();
            SetupLocalPath();
        }

        private void SetupLocalPath()
        {
            var workingDirField = _configHandler.GetType()
                .GetField("WORKING_DIRECTORY", BindingFlags.NonPublic | BindingFlags.Instance);
            workingDirField.SetValue(_configHandler, Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName);

            _configHandler.GetType().GetMethod("InitWatcher", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_configHandler, null);
        }

        private void SetupLocalConfigFile(NotificatorConfiguration configData)
        {
            string json = JsonConvert.SerializeObject(configData, Formatting.Indented);

            var workingDirField = _configHandler.GetType()
                .GetField("WORKING_DIRECTORY", BindingFlags.NonPublic | BindingFlags.Instance);
            var workingDirValue = workingDirField.GetValue(_configHandler).ToString();

            var fileNameField = _configHandler.GetType()
                .GetField("CONFIG_FILE_NAME", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileNameValue = (string)fileNameField.GetValue(_configHandler);

            string path = Path.Combine(workingDirValue, fileNameValue);
            File.WriteAllText(path, json);
        }

        [Fact]
        public void ConfigurationFileExists()
        {
            // Arrange
            var handler = new NotificatorConfigHandler();
            bool expected = true;

            // Act
            var workingDirField = handler.GetType()
                .GetField("WORKING_DIRECTORY", BindingFlags.NonPublic | BindingFlags.Instance);
            var workingDirValue = workingDirField.GetValue(handler).ToString();

            var fileNameField = handler.GetType()
                .GetField("CONFIG_FILE_NAME", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileNameValue = (string)fileNameField.GetValue(handler);

            string path = Path.Combine(workingDirValue, fileNameValue);
            bool exists = File.Exists(path);

            // Assert
            Assert.Equal(expected, exists);
        }

        [Theory]
        [InlineData("yyyy-MM-dd", 1)]
        [InlineData("dd/MM/yyyy hh:mm:ss", 2)]
        [InlineData("dd", 0)]
        public async Task GetConfiguration_ValidFile_ReturnsConfigData(string dateFormat, int interval)
        {
            // Arrange
            var expectedConfig = new NotificatorConfiguration { DataFormat = dateFormat, IntervalInSeconds = interval };

            // Act
            SetupLocalConfigFile(expectedConfig);
            var configData = await _configHandler.GetConfiguration();

            // Assert
            Assert.NotNull(configData);
            Assert.Equal(expectedConfig.DataFormat, configData.DataFormat);
            Assert.Equal(expectedConfig.IntervalInSeconds, configData.IntervalInSeconds);
        }

        [Theory]
        [InlineData(null, 1)]
        [InlineData("", 0)]
        [InlineData("dd", -1)]
        public async Task GetConfiguration_InvalidFile_ReturnsConfigData(string dateFormat, int interval)
        {
            // Arrange
            var expectedConfig = new NotificatorConfiguration { DataFormat = dateFormat, IntervalInSeconds = interval };

            // Act
            SetupLocalConfigFile(expectedConfig);

            // Assert
            await Assert.ThrowsAsync<Exception>(async () => await _configHandler.GetConfiguration());
        }

        [Fact]
        public void SubscribeToWatcher_EventHandlerSubscribed()
        {
            // Arrange
            var expectedConfig = new NotificatorConfiguration { DataFormat = "yyyy-MM-dd", IntervalInSeconds = 1 };
            bool eventHandlerCalled = false;

            // Act
            SetupLocalConfigFile(expectedConfig);
            _configHandler.SubscribeToWatcher((sender, e) => { eventHandlerCalled = true; });

            expectedConfig.IntervalInSeconds = 2;
            SetupLocalConfigFile(expectedConfig);

            // Assert
            Assert.True(eventHandlerCalled);
        }

        [Theory]
        [InlineData("yyyy-MM-dd", 1)]
        [InlineData("yyyy-MM-dd", 0)]
        [InlineData("dd", null)]
        public void ValidateConfigData_ValidConfig_ReturnsTrue(string dataFormat, int interval)
        {
            // Arrange
            var expectedConfig = new NotificatorConfiguration { DataFormat = dataFormat, IntervalInSeconds = interval };

            // Act

            var isValid = (bool)_configHandler.GetType().GetMethod("ValidateConfigData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_configHandler, new object[] { expectedConfig });

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(null, 1)]
        [InlineData("", 0)]
        [InlineData("dd", -1)]
        public void ValidateConfigData_InvalidConfig_ReturnsFalse(string dataFormat, int interval)
        {
            // Arrange
            var expectedConfig = new NotificatorConfiguration { DataFormat = dataFormat, IntervalInSeconds = interval };

            // Act

            var isValid = (bool)_configHandler.GetType().GetMethod("ValidateConfigData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_configHandler, new object[] { expectedConfig });

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateConfigData_NullConfig_ReturnsFalse()
        {
            // Arrange
            NotificatorConfiguration config = null;

            // Act
            var isValid = (bool)_configHandler.GetType().GetMethod("ValidateConfigData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_configHandler, new object[] { config });

            // Assert
            Assert.False(isValid);
        }
    }
}
