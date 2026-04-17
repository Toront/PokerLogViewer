using System.Windows;
using PokerLogViewer.ViewModels;

namespace PokerLogViewer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}