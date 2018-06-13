using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class Move
    {
        // The Command that causes the snail to move
        public Command Com;

        // The SquareChanges associated with this move 
        public List<SquareChange> Changes;

        public Move(Command c)
        {
            Com = c;
            Changes = new List<SquareChange>();
        }

        // Add a SquareChange associated with this moves Command
        public void AddSquareChange(SquareChange SquareChange)
        {
            Changes.Add(SquareChange);
        }

    }
}