namespace SystemTimeUpdater.Services
    {
    public class Timers
        {
        private readonly TimeUpdate _timeUpdate;
        private readonly MainWindowViewModel _mainWindowViewModel;
        DispatcherTimer TimeUpdate = new();
        DispatcherTimer TimeUpdateServer = new();

        public Timers(TimeUpdate timeUpdate , MainWindowViewModel mainWindowViewModel)
            {
            _timeUpdate = timeUpdate;
            _mainWindowViewModel = mainWindowViewModel;

            TimeUpdate.Tick += TimeUpdateTick;
            TimeUpdate.Start( );

            TimeUpdateServer.Tick += TimeUpdateServerTick;
            TimeUpdateServer.Start( );
            }

        private async void TimeUpdateServerTick(Object? sender , EventArgs e)
            {
            TimeUpdateServer.Stop( );

            var Time = await _timeUpdate.GetTime(_mainWindowViewModel );

            await Dispatcher.UIThread.InvokeAsync(( )
                    => _mainWindowViewModel.NowServer = Time ,
                DispatcherPriority.Background);

            await Task.Delay(5000);

            TimeUpdateServer.Start( );
            }

        private async void TimeUpdateTick(System.Object? sender , System.EventArgs e)
            {
            TimeUpdate.Stop( );
            await Dispatcher.UIThread.InvokeAsync(( )
                    => _mainWindowViewModel.NowCmos = DateTime.Now ,
                DispatcherPriority.Background);

            await Task.Delay(50);
            TimeUpdate.Start( );
            }
        }
    }
