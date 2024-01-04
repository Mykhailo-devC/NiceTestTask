public class DateNotificator
{
    private string _dateFormat;
    private int _interval;

    public virtual void SetConfiguration(NotificatorConfigData configData)
    {
        _dateFormat = configData.DataFormat;
        _interval = configData.IntervalInSeconds;
    }

    public virtual void NotifyDate()
    {
        Console.WriteLine("New notification - {0}", DateTime.Now.ToString(_dateFormat));
        Task.Delay(TimeSpan.FromSeconds(_interval)).Wait();
    }
}
