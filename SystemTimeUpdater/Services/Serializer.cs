namespace SystemTimeUpdater.Services
    {
    public class Serializer
    {
        List<NtpServer>Default = new( )
                {
                new NtpServer("tik.cesnet.cz", "195.113.144.201", "Praha 6"),
                new NtpServer("tak.cesnet.cz", "195.113.144.238", "Praha 6"),
                new NtpServer("stratum.eunet.cz", "193.85.3.51", "Praha"),
                new NtpServer("ntp.gts.cz", "193.85.3.51", "Praha")
            };

            private readonly MainWindowViewModel _mainWindowViewModel;

            public Serializer(MainWindowViewModel mainWindowViewModel)
            {
                _mainWindowViewModel = mainWindowViewModel;
            }

            public class NtpServer
            {
                public NtpServer(string serverName , string ipAddress , string location , string url = "")
                {
                    ServerName = serverName;
                    IPAddress = ipAddress;
                    Location = location;
                    Url = url;
                }

                public string ServerName { get; }
            public string IPAddress { get; }
            public string Location { get; }
            public string Url { get; }
            }

        public async Task Load( )
            {
            try
                {
                var fileContent = await Task.Run(()=>File.ReadAllText("ntpServers.json"));

                await Dispatcher.UIThread.InvokeAsync(
                    ( ) => _mainWindowViewModel.NtpServers =
                        JsonSerializer.Deserialize<List<NtpServer>>(fileContent) ?? Default ,
                    DispatcherPriority.Background);
                }
            catch
                {
                await Dispatcher.UIThread.InvokeAsync(( ) => _mainWindowViewModel.NtpServers = Default ,
                    DispatcherPriority.Background);
                }
            }
        }
    }
