using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using ProjBobcat.DefaultComponent.Launch.GameCore;
using ProjBobcat.DefaultComponent.Launch;
using ProjBobcat.DefaultComponent.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DotCatLauncher.Common.Modules;
using ProjBobcat.Class.Model;
using System.Collections.ObjectModel;
using Prism.Events;
using DotCatLauncher.Event;
using System.Windows.Documents;

namespace DotCatLauncher.ViewModels
{
    public class IndexViewModel : BindableBase
    {
        private readonly IEventAggregator eventAggregator;
        //get player's name
        private string playerName;

        public string PlayerName
        {
            get { return playerName; }
            set { SetProperty(ref playerName, value); }
        }

        public IndexViewModel(IEventAggregator eventAggregator)
        {
            ChoseGamePathCommand = new DelegateCommand(ChoseGamePath);
            StartCommand = new DelegateCommand(StartGame);
            ScanGameCommand = new DelegateCommand(ScanGame);
            this.eventAggregator= eventAggregator;
            //InitCore();
            gameVersionItems = new ObservableCollection<GameVersionItem>();
        }
        //add version in list
        private void CreateList()
        {
            (from e in gameList.Select(o => o.Id).Distinct() select new GameVersionItem { Id = e }).ToList().ForEach(l => gameVersionItems.Add(l));
        }

        //Scan Game
        public DelegateCommand ScanGameCommand { get; private set; }
        private List<VersionInfo> gameList =new();
        private void ScanGame()
        {
            if (core == null) { eventAggregator.GetEvent<SnackBarMsg>().Publish("未选择游戏文件夹"); return; }
            gameList = core.VersionLocator.GetAllGames().ToList();
            CreateList();
        }

        //init core
        readonly Guid clientToken = Guid.NewGuid();
        private DefaultGameCore core;
        private void InitCore(string rootPath)
        {
            core = new DefaultGameCore()
            {
                ClientToken = clientToken, // 游戏客户端识别码，你可以设置成你喜欢的任何GUID，例如88888888-8888-8888-8888-888888888888，或者自己随机生成一个！
                RootPath = rootPath, // .minecraft\的路径
                VersionLocator = new DefaultVersionLocator(rootPath, clientToken)
                {
                    LauncherProfileParser = new DefaultLauncherProfileParser(rootPath, clientToken),
                    LauncherAccountParser = new DefaultLauncherAccountParser(rootPath, clientToken)
                },
                GameLogResolver = new DefaultGameLogResolver()
            };
        }

        //chose game version by listbox and add game version
        public DelegateCommand<VersionInfo> ChoseVersionCommand { get; private set; }
        private ObservableCollection<GameVersionItem> gameVersionItems;
        public ObservableCollection<GameVersionItem> GameVersionItems
        {
            get { return gameVersionItems; }
            set { gameVersionItems = value; RaisePropertyChanged(); }
        }
        //chose game folder path and game version
        //TODO change click button
        public DelegateCommand ChoseGamePathCommand { get; private set; }
        private string rootPath;
        private void ChoseGamePath()
        {
            CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true
            };
            dialog.ShowDialog();
            try
            {
                if (dialog.FileName == null || dialog.FileName == String.Empty)
                    return;
                rootPath = dialog.FileName;
                InitCore(rootPath);
                ScanGame();
            }
            catch (Exception)
            {
                return;
            }
        }
        //TODO this is for test, wait to create a java setting in settings view
        private readonly string javaPath = Environment.GetEnvironmentVariable("JAVA_HOME")+"bin\\javaw.exe";
        public DelegateCommand StartCommand { get; private set; }
        private void StartGame()
        {
            LaunchGame(rootPath, javaPath, playerName);
        }
        //TODO launch game
        private async void LaunchGame(string rootPath,string javaPath,string playerName)
        {

        }
    }
}
