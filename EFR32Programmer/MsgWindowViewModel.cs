using System.ComponentModel;

namespace EFR32Programmer
{
    class MsgWindowViewModel : INotifyPropertyChanged
    {
        private string result;
        public string Result
        {
            get { return this.result; }
            set { this.result = value; RaisePropertyChanged(nameof(Result)); }
        }
        private string step;
        public string Step
        {
            get { return this.step; }
            set { this.step = value; RaisePropertyChanged(nameof(Step)); }
        }
        private string detailMsg;
        public string DetailMsg
        {
            get { return this.detailMsg; }
            set { this.detailMsg = value; RaisePropertyChanged(nameof(DetailMsg)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
