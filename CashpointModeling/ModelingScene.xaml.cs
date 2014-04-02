using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ELW.Library.Math;
using ELW.Library.Math.Tools;

namespace CashpointModeling
{
    /// <summary>
    /// Логика взаимодействия для ModelingScene.xaml
    /// </summary>
    public partial class ModelingScene : UserControl
    {
        private readonly CashpointNetwork _cashpointNetwork;
        private readonly List<Client> _clients = new List<Client>();
        private ModelingConfiguration _modelingConfiguration;
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly Random _rand = new Random();
        private readonly StackPanel _clientsInfo;
        private readonly double _backstage;

        public ModelingScene(StackPanel clientsInfo, ModelingConfiguration configuration)
        {
            InitializeComponent();
            _clientsInfo = clientsInfo;
            _backstage = Math.Max(_modelingConfiguration.ClientSize.Width, _modelingConfiguration.ClientSize.Height) * 1.2;
            _dispatcherTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 40)
            };

            _modelingConfiguration = configuration;

            _cashpointNetwork = new CashpointNetwork(CModelingScene, _backstage,
                _modelingConfiguration.CashpointsSize, _modelingConfiguration.CashpointsNumber, true);

            for (int i = 0; i < configuration.ClientsCount; i++)
            {
                var client = new Client(_modelingConfiguration.StepRange, i)
                {
                    WaitingTime = GetRandomWaitingTime(_modelingConfiguration.ClientWaitingLimit.Lower,
                        _modelingConfiguration.ClientWaitingLimit.Upper),
                    Position = GetRandomBackstagePosition()
                };
                _clientsInfo.Children.Add(new TextBlock());
                _clients.Add(client);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            CModelingScene.Children.Clear();
            _cashpointNetwork.RenderCashpoints();
            PerformModelingIteration();
            RenderBackstage();
        }

        public void RenderBackstage()
        {
            CModelingScene.Children.Add(new Border
            {
                Width = CModelingScene.Width - _backstage*2,
                Height = CModelingScene.Height - _backstage * 2,
                BorderThickness =
                    new Thickness(Math.Max(_modelingConfiguration.ClientSize.Width,
                        _modelingConfiguration.ClientSize.Height) * 1.2),
                BorderBrush = Brushes.CornflowerBlue
            });
        }

        public void PerformModelingIteration()
        {
            _clientsInfo.Children.Clear();
            foreach (var client in _clients)
            {
                switch (client.ClientState)
                {
                    case ClientState.Inactive: 
                        break;
                    case ClientState.Waiting:
                        if (client.CurrentProgress >= client.WaitingTime)
                        {
                            client.StopProgress();
                            client.ClientState = ClientState.Incoming;
                            client.CashpointId = _rand.Next(_cashpointNetwork.Cashpoints.Count);
                            client.SetCurrectWaypoint(_cashpointNetwork.Cashpoints[client.CashpointId].QueuePosition);
                        }

                        break;
                    case ClientState.Incoming: 
                        client.Move();
                        if (client.RemainingDistance <= client.StepRange)
                        {
                            var queue = _cashpointNetwork.Cashpoints[client.CashpointId].Queue;
                            if (queue.Count > 0)
                            {
                                var position = _cashpointNetwork.Cashpoints[client.CashpointId].QueuePosition;
                                client.SetCurrectWaypoint(new Position(position.X,
                                    position.Y + _modelingConfiguration.QueueDistance*queue.Count));
                                client.ClientState = ClientState.InQueue;
                            }
                            else
                            {
                                StartProcessing(client);
                            }
                            _cashpointNetwork.Cashpoints[client.CashpointId].Queue.Add(client);
                        }
                        break;
                    case ClientState.InQueue:
                        if (client.RemainingDistance >= client.StepRange) client.Move(); break;
                    case ClientState.Processing:
                        if (client.RemainingDistance >= client.StepRange)
                        {
                            client.Move(); break;
                        }
                        var progress = (double)client.CurrentProgress.Ticks / (double)client.InteractionDuration.Ticks;
                        if (progress >= 1)
                        {
                            client.StopProgress();
                            client.ClientState = ClientState.Outgoing;
                            client.SetCurrectWaypoint(GetRandomBackstagePosition());

                            var queue = _cashpointNetwork.Cashpoints[client.CashpointId].Queue;
                            queue.RemoveAt(0);
                            
                            if (queue.Count > 0)
                            {
                                var position = _cashpointNetwork.Cashpoints[client.CashpointId].QueuePosition;
                                for (int i = 0; i < queue.Count; i++)
                                {
                                    queue[i].SetCurrectWaypoint(new Position(position.X,
                                        position.Y + _modelingConfiguration.QueueDistance * i));
                                }
                                StartProcessing(queue[0]);
                            }
                        }
                        else
                        {
                            _cashpointNetwork.RenderCashpointProgress(client.CashpointId, progress);
                        }
                        break;
                    case ClientState.Outgoing: 
                        client.Move();
                        if (client.RemainingDistance <= client.StepRange)
                        {
                            client.WaitingTime = GetRandomWaitingTime(_modelingConfiguration.ClientWaitingLimit.Lower,
                                _modelingConfiguration.ClientWaitingLimit.Upper);
                            client.ClientState = ClientState.Waiting;
                            client.StartProgress();
                        }
                        break;
                }
                RenderClient(client.Position);
            }
        }

        public void StartProcessing(Client client)
        {
            client.ClientState = ClientState.Processing;
            client.InteractionDuration = GetRandomInteractionTime(_modelingConfiguration.ClientInteractionLimit.Lower,
                _modelingConfiguration.ClientInteractionLimit.Upper);
            client.StartProgress();
        }

        public Position GetRandomBackstagePosition()
        {
            var width = 2*_backstage + CModelingScene.Width;
            var height = 2*_backstage + CModelingScene.Height;
            switch (_rand.Next(4))
            {
                case 0:
                    return new Position(_rand.Next((int) width), -_backstage*2);
                case 1:
                    return new Position(width + _backstage*2, _rand.Next((int) height));
                case 2:
                    return new Position(_rand.Next((int) width), height - _backstage*2);
                case 3:
                    return new Position(-_backstage*2, _rand.Next((int) height));
                default:
                    return new Position(0, 0);
            }
        }

        public void RenderClient(Position position)
        {
            var body = new Ellipse
            {
                Fill = Brushes.CornflowerBlue,
                Width =  _modelingConfiguration.ClientSize.Width,
                Height = _modelingConfiguration.ClientSize.Height
            };
            Canvas.SetTop(body, position.Y);
            Canvas.SetLeft(body, position.X);

            CModelingScene.Children.Add(body);
        }

        public TimeSpan GetRandomWaitingTime(TimeSpan from, TimeSpan to)
        {
            var range = to - from;
            var variable = new VariableValue((long) (_rand.NextDouble()*range.Ticks), "x");
            return new TimeSpan((long)ToolsHelper.Calculator.Calculate(_modelingConfiguration.WaitingTimeFormula, variable));
        }

        public TimeSpan GetRandomInteractionTime(TimeSpan from, TimeSpan to)
        {
            var range = to - from;
            var variable = new VariableValue((long)(_rand.NextDouble() * range.Ticks), "x");
            return new TimeSpan((long)ToolsHelper.Calculator.Calculate(_modelingConfiguration.InteractionTimeFormula, variable));
        }

        //public TimeSpan GetRandomWaitingTime(TimeSpan from, TimeSpan to)
        //{
        //    var range = to - from;
        //    return new TimeSpan((long)(_rand.NextDouble() * range.Ticks));   
        //}

        //public TimeSpan GetRandomInteractionTime(TimeSpan from, TimeSpan to)
        //{
        //    var range = to - from;
        //    return new TimeSpan((long)(_rand.NextDouble() * range.Ticks));
        //}

        public void StartModeling()
        {
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Start();
            foreach (var client in _clients)
            {
                client.StartProgress();
            }
        }

        public void PauseModeling()
        {
            _dispatcherTimer.Stop();
            foreach (var client in _clients)
            {
                client.PauseProgress();
            }
        }
        
        public void StopModeling()
        {
            _dispatcherTimer.Tick -= dispatcherTimer_Tick;
            _dispatcherTimer.Stop();
            _clients.Clear();
            _cashpointNetwork.Cashpoints.Clear();
            CModelingScene.Children.Clear();
        }
    }

    public struct Position
    {
        public double X;
        public double Y;

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
