using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.ViewModels;

namespace FFXIVAPP.Plugin.TeastParse.UserControls
{
    public class ActionTimelineDetails : UserControl
    {
        private readonly Canvas _canvas;
        private readonly ScrollViewer _scrollView;
        private int _pixelPerSecond = 15;

        public ActionTimelineDetails()
        {
            InitializeComponent();

            _canvas = this.FindControl<Canvas>("Canvas");
            _scrollView = this.FindControl<ScrollViewer>("ScrollViewer");
            
            DataContextChanged += (_, __) => HandleNewDataContext();
            RedrawCanvas();
        }

        private void HandleNewDataContext()
        {
            if (DataContext == null)
                return;
            
            var data = DataContext as MainViewModel;
            if (data == null)
                return;

            data.PropertyChanged += (e, s) => {
                if (s.PropertyName == nameof(data.TimelineSelected)) RedrawCanvas();
                if (s.PropertyName == nameof(data.LoadingParse)) RedrawCanvas();
            };

            data.Alliance.CollectionChanged += (e, s) => HandleCollectionChange(s);
            data.Monster.CollectionChanged += (e, s) => HandleCollectionChange(s);
            data.Party.CollectionChanged += (e, s) => HandleCollectionChange(s);
        }

        private void HandleCollectionChange(NotifyCollectionChangedEventArgs change)
        {
            if (change.Action == NotifyCollectionChangedAction.Reset || change.Action == NotifyCollectionChangedAction.Add)
                RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            if (_canvas.Children.Count > 0)
                _canvas.Children.RemoveRange(0, _canvas.Children.Count);
            
            if (DataContext == null)
                return;
            
            var model = DataContext as MainViewModel;
            if (model == null)
                return;

            if (model.LoadingParse)
            {
                Console.WriteLine($"Waiting with redrawing canvas due to loading parse");
                return;
            }

            Console.WriteLine($"Now going to draw!");

            var (start, end) = GetTimelineDate(model);
            (start, end) = GetActualStartEnd(model, start, end);

            if (start >= end) 
                return;

            this.Height = model.Party.Count * (20*4);
            _canvas.Width = (end - start).TotalSeconds * _pixelPerSecond + 40;
            _canvas.Height = model.Party.Count * (20*4);
            _scrollView.Height = _canvas.Height;

            for(var x = _pixelPerSecond*10; x < _canvas.Width; x += _pixelPerSecond*10)
            {
                var line = new Line();
                line.StartPoint = new Point(x, 0);
                line.EndPoint = new Point(x, _canvas.Height);
                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 2;
                line[ToolTip.TipProperty] = start.AddSeconds(x / _pixelPerSecond);
                _canvas.Children.Add(line);
            }

            var baseY = 0;
            foreach(var player in model.Party)
            {

                var line = new Line();
                line.StartPoint = new Point(0, baseY);
                line.EndPoint = new Point(_canvas.Width, baseY);
                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 2;
                _canvas.Children.Add(line);

                DrawDashLine(baseY+20);
                DrawDashLine(baseY+40);
                DrawDashLine(baseY+60);


                var label = new TextBlock();
                label.Text = player.Name;
                label[Canvas.LeftProperty] = 5;
                label[Canvas.TopProperty] = baseY + 4;
                _canvas.Children.Add(label);

                baseY += 60;

                long totalDmg = 0;
                var lastX = new [] {-1, -1, -1};
                var curY = 0;
                foreach(var action in player.AllActions.OrderBy(x => x.OccurredUtc))
                {
                    if (action.OccurredUtc < start || action.OccurredUtc > end)
                        continue;

                    var image = new Image
                    {
                        Source = ResourceReader.GetActionIcon(action.Icon),
                        Width = 20
                    };

                    totalDmg += action.Damage;

                    var x = ((int)(action.OccurredUtc - start).TotalSeconds * _pixelPerSecond) - 10;
                    curY = 0;
                    if (lastX[curY] >= 0 && lastX[curY] + 20 >= x)
                    {
                        for(var i = 0; i < 3; i++)
                        {
                            curY++;
                            if (curY > 2) curY = 0;
                            if (lastX[curY] < 0 || lastX[curY] + 20 < x)
                            {
                                break;
                            }
                        }
                    }

                    lastX[curY] = x;

                    image[Canvas.LeftProperty] = x;
                    image[Canvas.TopProperty] = baseY - (curY * 20);
                    image[ToolTip.TipProperty] = $"{action.Name} (dmg: {action.Damage}, total: {totalDmg})";
                    _canvas.Children.Add(image);
                }

                baseY += 20;
            }
        }

        private void DrawDashLine(int y)
        {
            var line = new Line();
            line.StartPoint = new Point(0, y);
            line.EndPoint = new Point(_canvas.Width, y);
            line.Stroke = new SolidColorBrush(Colors.Gray);
            line.StrokeThickness = 1;
            line.StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 5, 5};
            _canvas.Children.Add(line);
        }

        /// <summary>
        /// Because <see cref="GetTimelineDate"/> can have MinValue/MaxValue we want to fix this to first event's or last event's time
        /// or else the canvas would be big
        /// </summary>
        private (DateTime start, DateTime end) GetActualStartEnd(MainViewModel model, DateTime start, DateTime end)
        {
            if (start != DateTime.MinValue && end != DateTime.MaxValue) return (start, end);

            var newStart = start;
            var newEnd = end;

            if (start == DateTime.MinValue)
            {
                newStart = GetMinDateTime(model.Party.SelectMany(x => x.AllActions), DateTime.MaxValue);
                newStart = GetMinDateTime(model.Alliance.SelectMany(x => x.AllActions), newStart);
                newStart = GetMinDateTime(model.Monster.SelectMany(x => x.AllActions), newStart);
            }

            if (end == DateTime.MaxValue)
            {
                newEnd = GetMaxDateTime(model.Party.SelectMany(x => x.AllActions), DateTime.MinValue);
                newEnd = GetMaxDateTime(model.Alliance.SelectMany(x => x.AllActions), newEnd);
                newEnd = GetMaxDateTime(model.Monster.SelectMany(x => x.AllActions), newEnd);
            }

            return (newStart, newEnd);
        }

        private DateTime GetMinDateTime(IEnumerable<ActorActionModel> actions, DateTime current)
        {
            if (!actions.Any()) return current;

            var date = actions.Select(x => x.OccurredUtc).Min();
            //date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);
            return date < current ? date : current;
        }

        private DateTime GetMaxDateTime(IEnumerable<ActorActionModel> actions, DateTime current)
        {
            if (!actions.Any()) return current;

            var date = actions.Select(x => x.OccurredUtc).Max();
            //date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);
            return date > current ? date : current;
        }

        /// <summary>
        /// get start and end time for current timeline
        /// </summary>
        private (DateTime start, DateTime end) GetTimelineDate(MainViewModel model)
        {
            var index = model.TimelineSelected;
            if (index == -1) index = 0;

            if (model.Timeline.Count == 0)
                return (DateTime.MinValue, DateTime.MaxValue);
            
            if (index > model.Timeline.Count) index = model.Timeline.Count - 1;

            var timeline = model.Timeline[index];
            var start = timeline.StartUtc.ToUniversalTime();
            var end = timeline.EndUtc?.ToUniversalTime() ?? DateTime.MaxValue;
            return (start, end);
        }

        private void RedrawCanvas_old()
        {
            if (_canvas.Children.Count > 0)
                _canvas.Children.RemoveRange(0, _canvas.Children.Count);
            
            if (DataContext == null)
                return;
            
            var data = DataContext as IEnumerable<ActorActionModel>;
            if (data == null)
                return;

            var count = data.Count();
            var start = data.Min(x => x.OccurredUtc);
            //Width = count * 50;
            Width = (data.Max(x => x.OccurredUtc) - start).TotalSeconds * _pixelPerSecond + 50;
            Height = 50;

            var index = 0;
            var last = "";
            foreach(var action in data.OrderBy(x => x.OccurredUtc))
            {
                var image = new Image
                {
                    Source = ResourceReader.GetActionIcon(action.Icon),
                    Width = 20
                };

                //image[Canvas.LeftProperty] = index * 50;
                image[Canvas.LeftProperty] = (action.OccurredUtc - start).TotalSeconds * _pixelPerSecond;
                image[Canvas.TopProperty] = 0;
                image[ToolTip.TipProperty] = action.Name;
                _canvas.Children.Add(image);
                index++;
                last = action.Name;
            }

            System.Console.WriteLine($"Total action: {count}, last: '{last}' totalWidth: {Width}");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}