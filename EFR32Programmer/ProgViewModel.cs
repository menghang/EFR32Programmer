using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace EFR32Programmer
{
    class ProgViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProgProcess> ProcessList { get; private set; }
            = new ObservableCollection<ProgProcess>();
        private int currentStep = 0;
        public int CurrentStep { get { return this.currentStep; } set { this.currentStep = value; RaisePropertyChanged(nameof(CurrentStep)); } }
        private int totalSteps = 1;
        public int TotalSteps { get { return this.totalSteps; } set { this.totalSteps = value; RaisePropertyChanged(nameof(TotalSteps)); } }
        private bool buttonEnable = true;
        public bool ButtonEnable
        {
            get { return this.buttonEnable; }
            set { this.buttonEnable = value; RaisePropertyChanged(nameof(ButtonEnable)); }
        }

        public string[] StepTitles { get; } = {
            "Serial Number", "1 Create Image", "2 Reset Device",
            "3 Unlock Device", "4 Mass Erase", "5 Flash Firmware",
            "6 Flash Lockbits", "7 Verify Image", "8 Dump Tokens",
            "9 Reset Device", "Time Escape" };
        
        public static readonly int N_JLinkSN = 0;
        public static readonly int N_CreateImage = 1;
        public static readonly int N_ResetDevice1 = 2;
        public static readonly int N_UnlockDevice = 3;
        public static readonly int N_MassErase = 4;
        public static readonly int N_FlashFirmware = 5;
        public static readonly int N_FlashLockbits = 6;
        public static readonly int N_VerifyImage = 7;
        public static readonly int N_DumpTokens = 8;
        public static readonly int N_ResetDevice2 = 9;
        public static readonly int N_TimeEscape = 10;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class ProgProcess
        {
            public ProgStep[] Steps { get; set; } = {
                new ProgStep("-"), new ProgStep("-"), new ProgStep("-"),
                new ProgStep("-"), new ProgStep("-"), new ProgStep("-"),
                new ProgStep("-"), new ProgStep("-"), new ProgStep("-"),
                new ProgStep("-"), new ProgStep("-") };

            public void Reset()
            {
                this.Steps[0].CurrentStatus = ProgStep.Status.Unknown;
                this.Steps[0].DetailMsg = string.Empty;
                for (int ii = 1; ii <= 10; ii++)
                {
                    this.Steps[ii].Reset();
                }
            }

            public class ProgStep : INotifyPropertyChanged
            {
                private static readonly SolidColorBrush Blue = new SolidColorBrush(Colors.LightSkyBlue);
                private static readonly SolidColorBrush Green = new SolidColorBrush(Colors.LightGreen);
                private static readonly SolidColorBrush Red = new SolidColorBrush(Colors.LightPink);
                private static readonly SolidColorBrush Grey = new SolidColorBrush(Colors.LightGray);
                public enum Status { Unknown = 0, Success = 1, Failure = 2, NotApplicable = 3 };
                private string text = string.Empty;
                public string Text
                {
                    get { return this.text; }
                    set { this.text = value; RaisePropertyChanged(nameof(Text)); }
                }
                public string DetailMsg { get; set; } = string.Empty;
                private Status currentStatus = Status.Unknown;
                public Status CurrentStatus { set { this.currentStatus = value; RaisePropertyChanged(nameof(Color)); } }
                public ProgStep(string _text)
                {
                    this.text = _text;
                    this.currentStatus = Status.Unknown;
                }

                public void Reset()
                {
                    this.CurrentStatus = Status.Unknown;
                    this.Text = "-";
                    this.DetailMsg = string.Empty;
                }

                public SolidColorBrush Color
                {
                    get
                    {
                        switch (this.currentStatus)
                        {
                            case Status.Unknown: return Blue;
                            case Status.Success: return Green;
                            case Status.Failure: return Red;
                            case Status.NotApplicable:
                            default: return Grey;
                        }
                    }
                }

                public event PropertyChangedEventHandler PropertyChanged;
                protected virtual void RaisePropertyChanged(string propertyName)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
