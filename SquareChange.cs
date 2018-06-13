using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{

    //records the change on game board so the move can be un done
    class SquareChange
    {

        public int X;
        public int Y;
        public int oldSquare;
        public int newSquare;

        //square changes from old square to new square when we do the square change
        public SquareChange(int _x, int _y, int _oldSquare, int _newSquare)
        {
            oldSquare = _oldSquare;
            newSquare = _newSquare;

            X = _x;
            Y = _y;
        }
    }
}