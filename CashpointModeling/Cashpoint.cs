using System.Collections.Generic;

namespace CashpointModeling
{
    public class Cashpoint
    {
        public Position Position;
        public Position QueuePosition 
        {
            get
            {
                return new Position(Position.X + 5, Position.Y + 25);
            }
        }
        public int Index { get; set; }
        public List<Client> Queue;

        public Cashpoint(int index)
        {
            Index = index;
            Queue = new List<Client>();
        }
    }
}
