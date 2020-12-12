using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FFXIVAPP.Plugin.TeastParse.ViewModels;

namespace FFXIVAPP.Plugin.TeastParse.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void LoadParse(object sender, RoutedEventArgs e)
        {
            var model = this.DataContext as MainViewModel;
            if (model == null)
                return;

            var window = (Window)this.VisualRoot;
            var dlg = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "parse file (*.db)", Extensions = new List<string> { "db" } } }
            };

            var file = await dlg.ShowAsync(window);
            if (file == null || file.Length != 1)
                return;

            model.LoadParse(file[0]);
        }
    }
}