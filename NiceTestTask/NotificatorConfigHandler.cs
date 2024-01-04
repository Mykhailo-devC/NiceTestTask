using Newtonsoft.Json;

public class NotificatorConfigHandler
{
    private string WORKING_DIRECTORY = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
    private string CONFIG_FILE_NAME = "configuration.json";

    private NotificatorConfigData _configData;
    private FileSystemWatcher _watcher;

    public NotificatorConfigHandler()
    {
        _configData = new NotificatorConfigData();
        InitWatcher();
    }

    public virtual async Task<NotificatorConfigData> GetConfiguration()
    {
        try
        {
            using (StreamReader stream = new StreamReader(Path.Combine(WORKING_DIRECTORY ,CONFIG_FILE_NAME)))
            {
                string json = await stream.ReadToEndAsync();
                var config = JsonConvert.DeserializeObject<NotificatorConfigData>(json);

                if(!ValidateConfigData(config))
                {
                    throw new Exception("Invalid configuration data.");
                }

                _configData.DataFormat = config.DataFormat;
                _configData.IntervalInSeconds = config.IntervalInSeconds;

                return _configData;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public virtual void SubscribeToWatcher(FileSystemEventHandler handler)
    {
        _watcher.Changed += handler;
    }

    private void InitWatcher()
    {
        try
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = WORKING_DIRECTORY;
            _watcher.EnableRaisingEvents = true;
            _watcher.Filter = CONFIG_FILE_NAME;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(-1);
        }
    }

    private bool ValidateConfigData(NotificatorConfigData config)
    {
        if (config == null)
        {
            Console.WriteLine("Error while reading configuration file");
            return false;
        }
        else if (string.IsNullOrEmpty(config.DataFormat))
        {
            Console.WriteLine("Date format is empty or not set. DataFormat: {0}", config.DataFormat);
            return false;
        }
        else if (config.IntervalInSeconds < 0)
        {
            Console.WriteLine("Invalid time interval. IntervalInSeconds:{0}", config.IntervalInSeconds);
            return false;
        }

        return true;
    }


}
