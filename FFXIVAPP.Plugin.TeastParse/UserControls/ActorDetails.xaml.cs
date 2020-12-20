using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FFXIVAPP.Plugin.TeastParse.UserControls
{
    public class ActorDetails : UserControl
    {
        public ActorDetails()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}