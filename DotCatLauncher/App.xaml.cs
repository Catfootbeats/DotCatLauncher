using DotCatLauncher.Common;
using DotCatLauncher.ViewModels;
using DotCatLauncher.Views;
using Prism.Ioc;
using Prism.Modularity;
using ProjBobcat.Class.Helper;
using System.Net;
using System.Windows;

namespace DotCatLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        protected override void OnInitialized()
        {
            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.Configure();
            base.OnInitialized();
            ServicePointManager.DefaultConnectionLimit = 512;
            ServiceHelper.Init();
            HttpClientHelper.Init();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
        }
    }
}
