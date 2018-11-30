using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EFR32Programmer
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private bool autoDetectCommander = false;
        public bool AutoDetectCommander
        {
            get => this.autoDetectCommander;
            set
            {
                this.autoDetectCommander = value;
                RaisePropertyChanged(nameof(this.AutoDetectCommander));
                RaisePropertyChanged(nameof(this.EnableEditCommander));
            }
        }
        public bool EnableEditCommander
        {
            get => !this.autoDetectCommander;
        }
        private string commanderFile = string.Empty;
        public string CommanderFile
        {
            get => this.commanderFile;
            set { this.commanderFile = value; RaisePropertyChanged(nameof(this.CommanderFile)); }
        }
        private bool flashROM1 = true;
        public bool FlashROM1
        {
            get => this.flashROM1;
            set { this.flashROM1 = value; RaisePropertyChanged(nameof(this.FlashROM1)); }
        }
        private string rom1File = string.Empty;
        public string ROM1File
        {
            get => this.rom1File;
            set { this.rom1File = value; RaisePropertyChanged(nameof(this.ROM1File)); }
        }
        private bool flashROM2 = false;
        public bool FlashROM2
        {
            get => this.flashROM2;
            set { this.flashROM2 = value; RaisePropertyChanged(nameof(this.FlashROM2)); }
        }
        private string rom2File = string.Empty;
        public string ROM2File
        {
            get => this.rom2File;
            set { this.rom2File = value; RaisePropertyChanged(nameof(this.ROM2File)); }
        }
        private bool randomMAC = true;
        public bool RandomMAC
        {
            get => this.randomMAC;
            set { this.randomMAC = value; RaisePropertyChanged(nameof(this.RandomMAC)); }
        }
        private bool randomInstallCode = true;
        public bool RandomInstallCode
        {
            get => this.randomInstallCode;
            set { this.randomInstallCode = value; RaisePropertyChanged(nameof(this.RandomInstallCode)); }
        }
        private bool enable = true;
        public bool Enable
        {
            get => this.enable;
            set { this.enable = value; RaisePropertyChanged(nameof(this.Enable)); }
        }
        public ObservableCollection<JLink> JLinkList { get; private set; } = new ObservableCollection<JLink>();
        private bool buttonReloadJlinkListEnable = true;
        public bool ButtonReloadJlinkListEnable
        {
            get => this.buttonReloadJlinkListEnable;
            set { this.buttonReloadJlinkListEnable = value; RaisePropertyChanged(nameof(this.ButtonReloadJlinkListEnable)); }
        }
        private string commanderVersion = string.Empty;
        public string CommanderVersion
        {
            get => this.commanderVersion;
            set { this.commanderVersion = value; RaisePropertyChanged(nameof(this.CommanderVersion)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class JLink : INotifyPropertyChanged
        {
            private string serialNumber = string.Empty;
            public string SerialNumber
            {
                get => this.serialNumber;
                set { this.serialNumber = value; RaisePropertyChanged(nameof(this.SerialNumber)); }
            }
            private bool selection = false;
            public bool Selection
            {
                get => this.selection;
                set { this.selection = value; RaisePropertyChanged(nameof(this.Selection)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
