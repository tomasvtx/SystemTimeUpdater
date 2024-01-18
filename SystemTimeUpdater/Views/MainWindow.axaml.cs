namespace SystemTimeUpdater.Views
    {
    public partial class MainWindow : Window
        {
        public MainWindow( MainWindowViewModel mainWindow)
            {
            InitializeComponent( );

            DataContext = mainWindow;
            }
        }
    }