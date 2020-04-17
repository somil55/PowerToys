using Windows.UI.Xaml.Controls;

namespace PowerLauncher.UI
{
    public sealed partial class LauncherControl : UserControl
    {
        public TextBox SearchTextBox => this.TextBox;
        public LauncherControl()
        {
            InitializeComponent();
        }
    }
}