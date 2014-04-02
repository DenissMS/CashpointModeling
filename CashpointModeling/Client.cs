using System;
using System.Diagnostics;

namespace CashpointModeling
{
    public class Client
    {
        public Position Position;

        public TimeSpan CurrentProgress
        {
            get
            {
                return _currentWaiting.Elapsed;
            }
        }

        public Position CurrentWaypoint;

        public TimeSpan WaitingTime;
        public TimeSpan InteractionDuration;
        public double CurrentDuration;
        private readonly Stopwatch _currentWaiting = new Stopwatch();
        public int CashpointId;
        public bool IsMoving;
        public bool IsContractActive;
        public double StepRange;
        public double RemainingDistance;
        public Offset Offset;
        public int Index;
        public ClientState ClientState;

        public Client(double range, int index)
        {
            StepRange = range;
            Index = index;
        }

        public void Move()
        {
            RemainingDistance -= StepRange;
            Position.X += Offset.Width;
            Position.Y += Offset.Height;
        }

        public void SetCurrectWaypoint(Position position)
        {
            CurrentWaypoint = position;
            Offset = GetOffset();
            RemainingDistance = GetDistance(Position, CurrentWaypoint);
        }

        public void StartProgress()
        {
            _currentWaiting.Start();
        }

        public void StopProgress()
        {
            _currentWaiting.Reset();
        }

        public void PauseProgress()
        {
            _currentWaiting.Stop();
        }

        private Offset GetOffset()
        {
            var distance = GetDistance(CurrentWaypoint, Position);
            var ratio = distance/StepRange;
            return new Offset((CurrentWaypoint.X - Position.X) / ratio,
                (CurrentWaypoint.Y - Position.Y) / ratio);
        }

        private double GetDistance(Position p1, Position p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
