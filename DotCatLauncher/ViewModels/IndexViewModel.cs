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
using ProjBobcat.Class.Model.LauncherProfile;
using System.Windows.Shapes;
using System.Diagnostics;
using ProjBobcat.DefaultComponent.Authenticator;

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
            ChoseVersionCommand = new DelegateCommand<GameVersionItem>(gameId => { versionId = gameId.Id; });
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
            if (core == null) { SnackMsg("未选择 .minecraft 目录"); return; }
            gameList = core.VersionLocator.GetAllGames().ToList();
            CreateList();
        }

        //SnackMsg
        private void SnackMsg(string msg)
        {
            eventAggregator.GetEvent<SnackBarMsg>().Publish(msg);
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
        public DelegateCommand<GameVersionItem> ChoseVersionCommand { get; private set; }
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
            if(core == null){ SnackMsg("未选择游戏目录"); return; }
            if (playerName == null || playerName == "") { SnackMsg("你没写名字"); return; }
            LaunchGame();
        }

        //Launch Setting
        private string versionId;
        private LaunchSettings LaunchSetting()
        {
            var launchSettings = new LaunchSettings
            {
                FallBackGameArguments = new GameArguments // 游戏启动参数缺省值，适用于以该启动设置启动的所有游戏，对于具体的某个游戏，可以设置（见下）具体的启动参数，如果所设置的具体参数出现缺失，将使用这个补全
                {
                    GcType = GcType.G1Gc, // GC类型
                    JavaExecutable = javaPath, // Java路径
                    Resolution = new ResolutionModel // 游戏窗口分辨率
                    {
                        Height = 600, // 高度
                        Width = 800 // 宽度
                    },
                    MinMemory = 512, // 最小内存
                    MaxMemory = 1024 // 最大内存
                },
                Version = versionId, // 需要启动的游戏ID，例如1.7.10或者1.15.2
                VersionInsulation = true, // 版本隔离
                GameResourcePath = core.RootPath, // 资源根目录
                GamePath = rootPath, // 游戏根目录
                VersionLocator = core.VersionLocator // 游戏定位器
            };
            return launchSettings;
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; }
        }

        //TODO launch game
        private async void LaunchGame()
        {
            var launchSettings = LaunchSetting();
            bool isNormal = true;
            eventAggregator.GetEvent<VerifyWay>().Subscribe(IsNoremal => { IsNoremal = isNormal; });
            if (isNormal) 
            { 
                launchSettings.Authenticator = new OfflineAuthenticator
                {
                    Username = playerName,
                    LauncherAccountParser = core.VersionLocator.LauncherAccountParser // launcher_profiles.json解析组件
                };
                isLoading= true;
                var result = await core.LaunchTaskAsync(launchSettings).ConfigureAwait(true);
                Debug.WriteLine(result.Error.ErrorMessage);
                isLoading= false;
            }
            else
            {
                //TODO Microsoft Verify
                return;
            }
        }
    }
}
