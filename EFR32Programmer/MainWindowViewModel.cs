﻿namespace EFR32Programmer
{
    internal class MainWindowViewModel
    {
        public SettingsViewModel SettingsView { get; private set; } = new SettingsViewModel();
        public ProgViewModel ProgView { get; private set; } = new ProgViewModel();
        private bool running = true;
        public bool Running
        {
            get => this.running;
            set
            {
                this.running = value;
                this.SettingsView.Enable = !this.running;
                this.ProgView.ButtonEnable = !this.running;
            }
        }
        public bool Enable { get => !this.running; }
    }
}
