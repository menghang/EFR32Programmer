using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EFR32Programmer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel view;
        private ProgTask jlink;
        private ConfigUtils configUtil;
        private Dictionary<string, ProgTask> progTaskList;
        private List<string> progTaskStatusList;
        private Dictionary<string, ProgViewModel.ProgProcess> progProcessList;
        private static readonly string defaultCommanderPath = @"\SiliconLabs\SimplicityStudio\v4\developer\adapter_packs\commander";

        public MainWindow()
        {
            this.view = new MainWindowViewModel();
            this.DataContext = this.view;
            InitializeComponent();
            this.jlink = new ProgTask(null, true, true);
            this.configUtil = new ConfigUtils();
            this.progTaskList = new Dictionary<string, ProgTask>();
            this.progTaskStatusList = new List<string>();
            this.progProcessList = new Dictionary<string, ProgViewModel.ProgProcess>();
        }

        private async Task GetJlinkList()
        {
            var list = await this.jlink.GetJlinkList();
            this.view.SettingsView.JLinkList.Clear();
            foreach (var sn in list)
            {
                this.view.SettingsView.JLinkList.Add(new SettingsViewModel.JLink() { Selection = true, SerialNumber = sn });
            }
        }

        private async void MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await LoadConfig();
        }

        private async Task LoadConfig()
        {
            var config = await this.configUtil.LoadConfig();
            var settings = this.view.SettingsView;
            settings.AutoDetectCommander = config.AutoDetectCommander;
            settings.CommanderFile = config.Commander;
            settings.FlashROM1 = config.FlashROM1;
            settings.FlashROM2 = config.FlashROM2;
            settings.ROM1File = config.ROM1;
            settings.ROM2File = config.ROM2;
            settings.RandomMAC = true;//config.RandomMAC;
            settings.RandomInstallCode = true;//config.RandomInstallCode;
            settings.JLinkList.Clear();
            var list = await this.jlink.GetJlinkList();
            foreach (var j in list)
            {
                if (config.J_Link.Contains(j))
                {
                    settings.JLinkList.Add(new SettingsViewModel.JLink() { Selection = true, SerialNumber = j });
                }
                else
                {
                    settings.JLinkList.Add(new SettingsViewModel.JLink() { Selection = false, SerialNumber = j });
                }
            }
        }

        private async void ButtonStart_ClickAsync(object sender, RoutedEventArgs e)
        {
            this.view.Running = true;
            this.view.ProgView.CurrentStep = 0;
            GetProgTasks();
            this.view.ProgView.TotalSteps = this.progTaskList.Count * 10;
            if (this.progTaskList.Count == 0)
            {
                this.view.Running = false;
                return;
            }
            var resultList = new List<Task<bool>>();
            foreach (var t in this.progTaskList)
            {
                this.progTaskStatusList.Add(t.Key);
                resultList.Add(t.Value.Run());
            }
            await Task.WhenAll(resultList);
        }

        private void GetProgTasks()
        {
            var settings = this.view.SettingsView;
            ProgTask.CommanderPath = settings.CommanderFile;
            ProgTask.Rom1 = settings.FlashROM1 ? settings.ROM1File : string.Empty;
            ProgTask.Rom2 = settings.FlashROM2 ? settings.ROM2File : string.Empty;

            this.progTaskList.Clear();
            this.progTaskStatusList.Clear();
            this.progProcessList.Clear();
            this.view.ProgView.ProcessList.Clear();
            foreach (var jlink in settings.JLinkList)
            {
                if (jlink.Selection)
                {
                    var progProcess = new ProgViewModel.ProgProcess();
                    progProcess.Steps[ProgViewModel.N_JLinkSN].Text = jlink.SerialNumber;
                    this.view.ProgView.ProcessList.Add(progProcess);
                    this.progProcessList.Add(jlink.SerialNumber, progProcess);
                    var progTask = new ProgTask(jlink.SerialNumber, settings.RandomMAC, settings.RandomInstallCode);
                    progTask.TaskProgressUpdated += ProgTask_TaskUpdated;
                    progTask.StepUpdated += ProgTask_StepUpdated;
                    progTask.TaskCompleted += ProgTask_TaskCompleted;
                    this.progTaskList.Add(jlink.SerialNumber, progTask);
                }
            }
        }

        private void ProgTask_TaskCompleted(object sender, EventArgs e)
        {
            this.progTaskStatusList.Remove((sender as ProgTask).JlinkSn);
            if (this.progTaskStatusList.Count == 0)
            {
                this.view.Running = false;
            }
        }

        private void ProgTask_StepUpdated(object sender, ProgTask.ProgStepUpdatedEventArgs e)
        {
            var step = this.progProcessList[(sender as ProgTask).JlinkSn].Steps[e.Step];
            if (e.Status != null)
            {
                step.CurrentStatus = (ProgViewModel.ProgProcess.ProgStep.Status)e.Status;
            }
            if (e.Text != null)
            {
                step.Text = e.Text;
            }
            if (e.DetailMsg != null)
            {
                step.DetailMsg += e.DetailMsg;
            }
        }

        private void ProgTask_TaskUpdated(object sender, ProgTask.ProgTaskProgressUpdatedEventArgs e)
        {
            this.view.ProgView.CurrentStep += e.Inc;
        }

        private async void ButtonReloadJlinkList_ClickAsync(object sender, RoutedEventArgs e)
        {
            this.view.SettingsView.ButtonReloadJlinkListEnable = false;
            await GetJlinkList();
            this.view.SettingsView.ButtonReloadJlinkListEnable = true;
        }

        private async void ButtonSaveSettings_ClickAsync(object sender, RoutedEventArgs e)
        {
            var settings = this.view.SettingsView;
            var config = new Config(settings);
            await this.configUtil.SaveConfig(config);
        }

        private void TextBoxCommander_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Select Commander.exe";
            fileDialog.Filter = "commander.exe|commander.exe";

            if (fileDialog.ShowDialog() == true)
            {
                this.view.SettingsView.CommanderFile = fileDialog.FileName;
            }
            ProgTask.CommanderPath = this.view.SettingsView.CommanderFile;
        }

        private void TextBoxRom1_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Select ROM file";
            fileDialog.Filter = "Intel Hex File (*.hex)|*.hex";

            if (fileDialog.ShowDialog() == true)
            {
                this.view.SettingsView.ROM1File = fileDialog.FileName;
            }
        }

        private void TextBoxRom2_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "Select ROM file";
            fileDialog.Filter = "Intel Hex File (*.hex)|*.hex";

            if (fileDialog.ShowDialog() == true)
            {
                this.view.SettingsView.ROM2File = fileDialog.FileName;
            }
        }

        private void CheckBoxAutoDetectCommander_Checked(object sender, RoutedEventArgs e)
        {
            string path = Environment.GetEnvironmentVariable("Path");
            List<string> pathList = new List<string>(path.Split(';'))
            {
                "C:" + defaultCommanderPath,
                "D:" + defaultCommanderPath
            };
            bool commanderFound = false;
            foreach (var p in pathList)
            {
                string f = Path.Combine(p, "commander.exe");
                if (File.Exists(f))
                {
                    this.view.SettingsView.CommanderFile = f;
                    commanderFound = true;
                    break;
                }
            }
            if (!commanderFound)
            {
                MessageBox.Show("Can not find commander.exe in the windows search path. Please select the location manually.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.view.SettingsView.AutoDetectCommander = false;
            }
            ProgTask.CommanderPath = this.view.SettingsView.CommanderFile;
        }

        private async void DataGridProgProcess_MouseDoubleClickAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left)
            {
                return;
            }
            var cell = (sender as DataGrid).CurrentCell;
            if (cell.Column == null)
            {
                return;
            }
            if (cell.Column.DisplayIndex == ProgViewModel.N_JLinkSN || cell.Column.DisplayIndex == ProgViewModel.N_TimeEscape)
            {
                string jlink = (cell.Item as ProgViewModel.ProgProcess).Steps[ProgViewModel.N_JLinkSN].Text;
                if (!this.progTaskStatusList.Contains(jlink))
                {
                    this.view.Running = true;
                    this.progTaskStatusList.Add(jlink);
                    this.progProcessList[jlink].Reset();
                    await this.progTaskList[jlink].Run();
                }
            }
            else
            {
                var msgWindow = new MsgWindow();
                var step = (cell.Item as ProgViewModel.ProgProcess).Steps[cell.Column.DisplayIndex];
                msgWindow.SetResult(step.Text);
                msgWindow.SetMsg(step.DetailMsg);
                msgWindow.SetTitle("J-Link SN: " + (cell.Item as ProgViewModel.ProgProcess).Steps[ProgViewModel.N_JLinkSN].Text);
                msgWindow.SetStep(this.view.ProgView.StepTitles[cell.Column.DisplayIndex]);
                msgWindow.Show();
            }
        }
    }
}
