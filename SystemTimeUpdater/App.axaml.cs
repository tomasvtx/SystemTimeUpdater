namespace SystemTimeUpdater
    {
    public partial class App : Application
        {
      
        public static MainWindow Main;
        public override void Initialize( )
            {
            AvaloniaXamlLoader.Load(this);
            }

        public override async void OnFrameworkInitializationCompleted( )
            {
                try
                {
                    var services = await Task.Run(ServiceCollection);
                    var serviceProvider = services.BuildServiceProvider();

                    Main = serviceProvider.GetRequiredService<MainWindow>();

                    await serviceProvider.GetRequiredService<MainWindowViewModel>().Load();
                    await serviceProvider.GetRequiredService<Serializer>().Load();
                    serviceProvider.GetRequiredService<Timers>();

                    await Dispatcher.UIThread.InvokeAsync(( ) => Main.Show( ) , DispatcherPriority.Send);

                base.OnFrameworkInitializationCompleted();
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(async ( ) =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("UpdateTime",
                            "Chyba pøi spouštìní aplikace " + ex.Message, ButtonEnum.Ok);
                        await box.ShowWindowDialogAsync(App.Main);
                    } , DispatcherPriority.Background);
                }
            }

        private static IServiceCollection ServiceCollection( )
        {
            var services = new ServiceCollection()
                .AddSingleton<Serializer>()
                .AddSingleton<TimeUpdate>()
                .AddSingleton<Timers>()

                .AddSingleton<MainWindow>()
                .AddSingleton<MainWindowViewModel>();

            return services;
            }
        }
    }