namespace SystemTimeUpdater.ViewModels
    {

    public class MainWindowViewModel : ReactiveObject
        {
        private Serializer.NtpServer selectedNtpServers;
        private DateTime nowCmos;
        private DateTimeOffset nowServer;
        private List<NtpServer> _ntpServers;
        private ReactiveCommand<Unit , Unit> writeToCmosCommand;
        private String syncError;
        private readonly TimeUpdate _timeUpdate;

        public MainWindowViewModel(TimeUpdate timeUpdate)
        {
            _timeUpdate = timeUpdate;
        }

        public Task Load()
            {
                WriteToCmosCommand = ReactiveCommand.CreateFromTask(_ => _timeUpdate.UpdateTime(this ));

                return Task.CompletedTask;
            }

            public List<NtpServer> NtpServers
        {
            get => _ntpServers;
            set => this.RaiseAndSetIfChanged(ref _ntpServers , value);
        }

        public NtpServer SelectedNtpServers
            {
            get => selectedNtpServers;
            set => this.RaiseAndSetIfChanged(ref selectedNtpServers , value);
            }

        public DateTime NowCmos
            {
            get => nowCmos;
            set => this.RaiseAndSetIfChanged(ref nowCmos , value);
            }
        public ReactiveCommand<Unit , Unit> WriteToCmosCommand
        {
            get => writeToCmosCommand;
            set => this.RaiseAndSetIfChanged(ref writeToCmosCommand , value);
        }

        public DateTimeOffset NowServer
            {
            get => nowServer;
            set => this.RaiseAndSetIfChanged(ref nowServer , value);
            }
        public String SyncError
        {
            get => syncError;
            set => this.RaiseAndSetIfChanged(ref syncError , value);
        }
        }
    }
