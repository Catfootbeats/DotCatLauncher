using DotCatLauncher.Event;
using Prism.Events;
using System.Windows;

namespace DotCatLauncher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            eventAggregator.GetEvent<SnackBarMsg>().Subscribe(msg => { SnackBarIndex.MessageQueue?.Enqueue(msg); });
        }
    }
}
