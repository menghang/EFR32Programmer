using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EFR32Programmer
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        private bool autoDetectCommander = false;
        public bool AutoDetectCommander
        {
            get { return this.autoDetectCommander; }
            set
            {
                this.autoDetectCommander = value;
                RaisePropertyChanged(nameof(AutoDetectCommander));
                RaisePropertyChanged(nameof(EnableEditCommander));
            }
        }
        public bool EnableEditCommander
        {
            get { return !this.autoDetectCommander; }
        }
        private string commanderFile = string.Empty;
        public string CommanderFile
        {
            get { return this.commanderFile; }
            set { this.commanderFile = value; RaisePropertyChanged(nameof(CommanderFile)); }
        }
        private bool flashROM1 = true;
        public bool FlashROM1
        {
            get { return this.flashROM1; }
            set { this.flashROM1 = value; RaisePropertyChanged(nameof(FlashROM1)); }
        }
        private string rom1File = string.Empty;
        public string ROM1File
        {
            get { return this.rom1File; }
            set { this.rom1File = value; RaisePropertyChanged(nameof(ROM1File)); }
        }
        private bool flashROM2 = false;
        public bool FlashROM2
        {
            get { return this.flashROM2; }
            set { this.flashROM2 = value; RaisePropertyChanged(nameof(FlashROM2)); }
        }
        private string rom2File = string.Empty;
        public string ROM2File
        {
            get { return this.rom2File; }
            set { this.rom2File = value; RaisePropertyChanged(nameof(ROM2File)); }
        }
        private bool randomMAC = true;
        public bool RandomMAC
        {
            get { return this.randomMAC; }
            set { this.randomMAC = value; RaisePropertyChanged(nameof(RandomMAC)); }
        }
        private bool randomInstallCode = true;
        public bool RandomInstallCode
        {
            get { return this.randomInstallCode; }
            set { this.randomInstallCode = value; RaisePropertyChanged(nameof(RandomInstallCode)); }
        }
        private bool enable = true;
        public bool Enable
        {
            get { return this.enable; }
            set { this.enable = value; RaisePropertyChanged(nameof(Enable)); }
        }
        public ObservableCollection<JLink> JLinkList { get; private set; } = new ObservableCollection<JLink>();
        private bool buttonReloadJlinkListEnable = true;
        public bool ButtonReloadJlinkListEnable
        {
            get { return this.buttonReloadJlinkListEnable; }
            set { this.buttonReloadJlinkListEnable = value; RaisePropertyChanged(nameof(ButtonReloadJlinkListEnable)); }
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
                get { return this.serialNumber; }
                set { this.serialNumber = value; RaisePropertyChanged(nameof(SerialNumber)); }
            }
            private bool selection = false;
            public bool Selection
            {
                get { return this.selection; }
                set { this.selection = value; RaisePropertyChanged(nameof(Selection)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
