public class DateNotificationHandler
{
    private DateNotificator _notificator;
    private NotificatorConfigHandler _configHandler;

    private CancellationTokenSource _tokenSource;
    private bool UpdateRunningState = false;

    public  DateNotificationHandler()
    {
        _configHandler = new NotificatorConfigHandler();
        _notificator = new DateNotificator();

        UpdateNotificator().Wait();
        SubscribeToWatcher();
    }

    public async void Start()
    {
        InitCancelationToken();

        await Task.Run(() =>
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                if (!UpdateRunningState)
                {
                    _notificator.NotifyDate();
                }
            }
        });
    }

    public void Stop()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }

    private async Task UpdateNotificator()
    {
        try
        {
            UpdateRunningState = true;

            var configData = await _configHandler.GetConfiguration();
            _notificator.SetConfiguration(configData);

            UpdateRunningState = false;
        }
        catch
        {
            Environment.Exit(-1);
        }
    }

    private void SubscribeToWatcher()
    {
        _configHandler.SubscribeToWatcher(async (sender, e) => await UpdateNotificator());
    }

    private void InitCancelationToken()
    {
        _tokenSource = new CancellationTokenSource();
    }
}
