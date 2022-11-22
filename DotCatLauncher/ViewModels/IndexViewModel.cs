using Microsoft.Win32;
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

namespace DotCatLauncher.ViewModels
{
    public class IndexViewModel : BindableBase
    {
        //get player's name
        private string playerName;

        public string PlayerName
        {
            get { return playerName; }
            set { SetProperty(ref playerName, value); }
        }

        public IndexViewModel()
        {
            ChoseGamePathCommand = new DelegateCommand(ChoseGamePath);
            StartCommand = new DelegateCommand(StartGame);
            InitCore();
            //TODO create a class to replace VersionInfo
            //VersionInfos = ObservableCollection<VersionInfo>;
        }

        //init core
        Guid clientToken = Guid.NewGuid();
        private DefaultGameCore core;
        private void InitCore()
        {
            core = new DefaultGameCore()
            {
                ClientToken = this.clientToken, // 游戏客户端识别码，你可以设置成你喜欢的任何GUID，例如88888888-8888-8888-8888-888888888888，或者自己随机生成一个！
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
        private ObservableCollection<VersionInfo> versionInfos;
        public ObservableCollection<VersionInfo> VersionInfos
        {
            get { return versionInfos; }
            set { versionInfos = value; RaisePropertyChanged(); }
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

        private string debugLog;

        public string DebugLog { get => debugLog; set => SetProperty(ref debugLog, value); }
    }
}
