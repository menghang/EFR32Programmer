using System.Collections.Generic;

namespace EFR32Programmer
{
    class Config
    {
        public string ROM1 = string.Empty;
        public string ROM2 = string.Empty;
        public bool FlashROM1 = false;
        public bool FlashROM2 = false;
        public string Commander = string.Empty;
        public bool AutoDetectCommander = false;
        public bool RandomMAC = true;
        public bool RandomInstallCode = true;
        public List<string> J_Link = new List<string>();

        public Config()
        {
            this.ROM1 = string.Empty;
            this.ROM2 = string.Empty;
            this.FlashROM1 = false;
            this.FlashROM2 = false;
            this.Commander = string.Empty;
            this.AutoDetectCommander = false;
            this.RandomMAC = true;
            this.RandomInstallCode = true;
            this.J_Link = new List<string>();
        }

        public Config(SettingsViewModel settings)
        {
            this.ROM1 = settings.ROM1File;
            this.ROM2 = settings.ROM2File;
            this.FlashROM1 = settings.FlashROM1;
            this.FlashROM2 = settings.FlashROM2;
            this.Commander = settings.CommanderFile;
            this.AutoDetectCommander = settings.AutoDetectCommander;
            this.RandomMAC = settings.RandomMAC;
            this.RandomInstallCode = settings.RandomInstallCode;
            this.J_Link = new List<string>();
            foreach(var j in settings.JLinkList)
            {
                if (j.Selection)
                {
                    this.J_Link.Add(j.SerialNumber);
                }
            }
        }
    }
}
