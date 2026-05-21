using System.Windows;
using SmartEnvironmentMonitoringSystem.Wpf.ViewModels;

namespace SmartEnvironmentMonitoringSystem.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
