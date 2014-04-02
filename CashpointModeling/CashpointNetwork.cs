using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CashpointModeling
{
    public class CashpointNetwork
    {
        readonly Random _rand = new Random();
        public readonly List<Cashpoint> Cashpoints = new List<Cashpoint>();
        private Size _cashpointSize;
        private readonly double _cashpointDiagonal;
        private const int ATTEMPT_LIMIT = 1000;
        private readonly Canvas _modelingField;
        private readonly double _backstage;

        public CashpointNetwork(Canvas field, double backstage, Size cashpointSize, int number, bool render = false)
        {
            _modelingField = field;
            _cashpointSize = cashpointSize;
            _backstage = backstage;
            _cashpointDiagonal = Math.Sqrt(Math.Pow(_cashpointSize.Width, 2) + Math.Pow(_cashpointSize.Height, 2));
            for (int i = 0; i < number; i++)
            {
                Cashpoints.Add(new Cashpoint(i));
                SetRandomPosition(i);
                int attempt = 0;
                for (int j = 0; j < i; j++)
                {
                    while (attempt < ATTEMPT_LIMIT && IsCashpoinsConflicting(Cashpoints[j].Position, Cashpoints[i].Position))
                    {
                        attempt++;
                        SetRandomPosition(i);
                    }
                }
            }
            if (render)
                RenderCashpoints();
        }

        public void SetRandomPosition(int index)
        {
            Cashpoints[index].Position = new Position
            {
                X = _rand.Next((int)(_modelingField.Width - _backstage)) + _backstage,
                Y = _rand.Next((int)(_modelingField.Height - _backstage - _cashpointSize.Height)) +_backstage 
            };
        }

        public bool IsCashpoinsConflicting(Position cashpoint1, Position cashpoint2)
        {
            return Math.Sqrt(Math.Pow(cashpoint1.X - cashpoint2.X, 2) +
                             Math.Pow(cashpoint1.Y - cashpoint2.Y, 2)) < _cashpointDiagonal;
        }

        public void RenderCashpoints()
        {
            foreach (var cashpoint in Cashpoints)
            {
                RenderCashpoint(cashpoint);
            }
        }

        public void RenderCashpointProgress(int index, double progress)
        {
            var body = new Rectangle
            {
                Fill = Brushes.Gold,
                Width = _cashpointSize.Width * 0.7 * progress,
                Height = _cashpointSize.Height * 0.35
            };
            Canvas.SetTop(body, Cashpoints[index].Position.Y + 2);
            Canvas.SetLeft(body, Cashpoints[index].Position.X + 2);

            _modelingField.Children.Add(body);
        }

        //public Shape[] CreateCashpoint()
        //{
        //    var pen = new Pen(Brushes.Black, 2);
        //    var rectDrawing1 = new GeometryDrawing(Brushes.Crimson, pen, new RectangleGeometry(new Rect(0, 0, _cashpointSize.Width, _cashpointSize.Height)));
        //    var rectDrawing2 = new GeometryDrawing(Brushes.LightBlue, pen, new RectangleGeometry(new Rect(5, 5, _cashpointSize.Width * 0.8, _cashpointSize.Height * 0.4)));
        //    var aDrawingGroup = new DrawingGroup
        //    {
        //        Children = new DrawingCollection
        //        {
        //            rectDrawing1,
        //            rectDrawing2
        //        }
        //    };
        //    return aDrawingGroup;
        //}

        public void RenderCashpoint(Cashpoint cashpoint)
        {
            var body = new Rectangle
            {
                Fill = Brushes.Crimson,
                Width = _cashpointSize.Width,
                Height = _cashpointSize.Height
            };
            Canvas.SetTop(body, cashpoint.Position.Y);
            Canvas.SetLeft(body, cashpoint.Position.X);

            var display = new Rectangle
            {
                Fill = Brushes.LightBlue,
                Width = _cashpointSize.Width * 0.9,
                Height = _cashpointSize.Height * 0.4
            };
            Canvas.SetTop(display, cashpoint.Position.Y + 1);
            Canvas.SetLeft(display, cashpoint.Position.X + 1);

            var text = new TextBlock
            {
                Text = cashpoint.Index.ToString(CultureInfo.InvariantCulture), 
                Foreground = Brushes.Chartreuse,
                FontSize = 8
            };
            Canvas.SetTop(text, cashpoint.Position.Y + 17);
            Canvas.SetLeft(text, cashpoint.Position.X + 1);

            _modelingField.Children.Add(body);
            _modelingField.Children.Add(display);
            _modelingField.Children.Add(text);
        }
    }
}
