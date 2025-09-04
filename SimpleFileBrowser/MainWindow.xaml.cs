using System.Windows;

namespace SimpleFileBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(new FileSystemService(), new DialogService()); 
            // Improvement:Simple Dependency Injection - Needs to be replaced with proper DI container in a real app
        }
    }
}
