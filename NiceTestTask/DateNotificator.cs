public class DateNotificator
{
    private NotificatorConfiguration _config;

    public void SetConfiguration(NotificatorConfiguration config)
    {
        _config = config;
    }

    public void NotifyDate()
    {
        Console.WriteLine("New notification - {0}", DateTime.Now.ToString(_config.DataFormat));
        Task.Delay(TimeSpan.FromSeconds(_config.IntervalInSeconds)).Wait();
    }
}
