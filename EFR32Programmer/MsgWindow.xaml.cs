using System.Windows;

namespace EFR32Programmer
{
    /// <summary>
    /// Interaction logic for MsgWindow.xaml
    /// </summary>
     public partial class MsgWindow : Window
    {
        private MsgWindowViewModel view;
        public MsgWindow()
        {
            this.view = new MsgWindowViewModel();
            this.DataContext = this.view;
            InitializeComponent();
        }

        public void SetTitle(string title)
        {
            this.Title = title;
        }

        public void SetResult(string result)
        {
            this.view.Result = result;
        }

        public void SetStep(string step)
        {
            this.view.Step = step;
        }

        public void SetMsg(string msg)
        {
            this.view.DetailMsg = msg;
        }
    }
}
