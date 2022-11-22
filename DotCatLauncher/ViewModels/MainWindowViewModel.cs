using DotCatLauncher.Common;
using DotCatLauncher.Common.Modules;
using DotCatLauncher.Extension;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace DotCatLauncher.ViewModels
{
    public class MainWindowViewModel : BindableBase , IConfigureService
    {
        private string _title = "Dot Cat Launcher";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            MenuBars = new ObservableCollection<MenuBar>();
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            this.regionManager = regionManager;
        }
        //Navigation
        private IRegionManager regionManager;
        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }
        private void Navigate(MenuBar obj)
        {
            //通过MenuBar的标题导航
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;
            regionManager.Regions[PrismManager.MainWindowRegionName].RequestNavigate(obj.NameSpace);
        }
        //MenuBar
        private ObservableCollection<MenuBar> menuBars;
        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }
        void CreateMenuBar()
        {
            MenuBars.Add(new MenuBar() { Icon = "Home", Title = "启动", NameSpace = "IndexView" });
        }
        public void Configure()
        {
            CreateMenuBar();//创建menubar
            regionManager.Regions[PrismManager.MainWindowRegionName].RequestNavigate("IndexView");
        }
    }
}
