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
using ProjBobcat.DefaultComponent.ResourceInfoResolver;
using ProjBobcat.DefaultComponent;
using ProjBobcat.Interface;
using ProjBobcat.Class.Helper;
using ProjBobcat.Event;

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
            ChoseVersionCommand = new DelegateCommand<GameVersionItem>(gameVersion => { versionId = gameVersion.Id; });
            ChoseGamePathCommand = new DelegateCommand(ChoseGamePath);
            StartCommand = new DelegateCommand(StartGame);
            ScanGameCommand = new DelegateCommand(ScanGame);
            this.eventAggregator = eventAggregator;
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
        private List<VersionInfo> gameList = new();
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
        private DefaultGameCore core;
        private void InitCore()
        {
            var clientToken = new Guid("88888888-8888-8888-8888-888888888888");
            var path = rootPath;
            core = new DefaultGameCore()
            {
                ClientToken = clientToken, // 游戏客户端识别码，你可以设置成你喜欢的任何GUID，例如88888888-8888-8888-8888-888888888888，或者自己随机生成一个！
                RootPath = path, // .minecraft\的路径
                VersionLocator = new DefaultVersionLocator(path, clientToken)
                {
                    LauncherProfileParser = new DefaultLauncherProfileParser(path, clientToken),
                    LauncherAccountParser = new DefaultLauncherAccountParser(path, clientToken)
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
                InitCore();
                ScanGame();
            }
            catch (Exception)
            {
                return;
            }
        }
        //TODO this is for test, wait to create a java setting in settings view
        private readonly string javaPath = Environment.GetEnvironmentVariable("JAVA_HOME") + "bin\\javaw.exe";
        public DelegateCommand StartCommand { get; private set; }
        private void StartGame()
        {
            if (core == null) { SnackMsg("未选择游戏目录"); return; }
            if (playerName == null || playerName == "") { SnackMsg("你没写名字"); return; }
            LaunchGame();
        }

        //Completion Asset
        //TODO gamelist index
        private async void Competion()
        {
            int index = 0;
            eventAggregator.GetEvent<GameVersionIndex>().Subscribe(i => index = i); 
            var drc = new DefaultResourceCompleter
            {
                ResourceInfoResolvers = new List<IResourceInfoResolver>(2)
                {
                    new AssetInfoResolver
                    {
                        AssetIndexUriRoot = "https://download.mcbbs.net/",
                        AssetUriRoot = "https://download.mcbbs.net/assets/",
                        BasePath = core.RootPath,
                        VersionInfo = gameList[index]
                    },
                    new LibraryInfoResolver
                    {
                        BasePath = core.RootPath,
                        LibraryUriRoot = "https://download.mcbbs.net/maven/",
                        VersionInfo = gameList[index]
                    }
                }
            };

            await drc.CheckAndDownloadTaskAsync().ConfigureAwait(false);
        }

        //Launch Setting
        private string versionId;
        private LaunchSettings LaunchSetting()
        {
            var launchSettings = new LaunchSettings
            {
                Version = versionId, // 需要启动的游戏ID
                VersionInsulation = true, // 版本隔离
                GameResourcePath = core.RootPath, // 资源根目录
                GamePath = GamePathHelper.GetGamePath(versionId), // 游戏根目录，如果有版本隔离则应该改为GamePathHelper.GetGamePath(Core.RootPath, versionId)
                VersionLocator = core.VersionLocator, // 游戏定位器
                GameName = gameList.Find(e => e.Id==versionId).Name,
                GameArguments = new GameArguments // （可选）具体游戏启动参数
                {
                    AdvanceArguments = "", // GC类型
                    JavaExecutable = javaPath, // JAVA路径
                    Resolution = new ResolutionModel { Height = 600, Width = 800 }, // 游戏窗口分辨率
                    MinMemory = 512, // 最小内存
                    MaxMemory = 1024, // 最大内存
                    GcType = GcType.G1Gc, // GC类型
                },
                Authenticator = new OfflineAuthenticator
                {
                    Username = playerName,
                    LauncherAccountParser = core.VersionLocator.LauncherAccountParser // launcher_profiles.json解析组件
                }
        };
            return launchSettings;
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; RaisePropertyChanged(); }
        }

        //TODO launch game
        private void LaunchGame()
        {
            var launchSettings = LaunchSetting();
            bool isNormal = true;
            eventAggregator.GetEvent<VerifyWay>().Subscribe(IsNoremal => { IsNoremal = isNormal; });
            if (isNormal)
            {
                isLoading = true;
                core.GameLogEventDelegate += Core_GameLogEventDelegate;
                core.LaunchLogEventDelegate += Core_LaunchLogEventDelegate;
                core.GameExitEventDelegate += Core_GameExitEventDelegate;
                var task = Task.Run(() => { 
                    var result = core.LaunchTaskAsync(launchSettings).ConfigureAwait(true); 
                    isLoading = false;
                }); 
            }
            else
            {
                //TODO Microsoft Verify
            }
        }

        private static void Core_GameExitEventDelegate(object sender, GameExitEventArgs e)
        {
            Debug.WriteLine("DONE");
        }

        private static void Core_LaunchLogEventDelegate(object sender, LaunchLogEventArgs e)
        {
            Debug.WriteLine($"[启动 LOG] - {e.Item}");
        }

        private static void Core_GameLogEventDelegate(object sender, GameLogEventArgs e)
        {
            Debug.WriteLine($"[游戏 LOG] - {e.Content}");
        }
    }
}
