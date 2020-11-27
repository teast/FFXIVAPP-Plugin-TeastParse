using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace FFXIVAPP.Plugin.TeastParse.Windows
{
    public class RealTimeWidget : Window
    {
        public RealTimeWidget()
        {
            ShowInTaskbar = false;
            InitializeComponent();

            this.FindControl<Panel>("CustChrome").PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = this.MinWidth, height = this.MinHeight;
            for (var i = 0; i < this.VisualChildren.Count; i++)
            {
                IVisual visual = this.VisualChildren[i];
                if (visual is ILayoutable layoutable)
                {
                    layoutable.Measure(Size.Infinity);
                    width = Math.Max(layoutable.DesiredSize.Width, width);
                    height = Math.Max(layoutable.DesiredSize.Height, height);
                }
            }

            return new Size(width, height);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}