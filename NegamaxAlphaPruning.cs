using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class Negamax
    {
        double Max;
        double Min;
        public double MaxDepth;

        public Move NegaBest = null;

        public Negamax(double _Min, double _Max, int _MaxDepth)
        {
            Min = _Min;
            Max = _Max;
            MaxDepth = _MaxDepth;
        }


        public double NegaMaxSearchFunction(GameBoard GameBoard, int Depth, double AlphaValue, double BetaValue, PlayerWorldState WorldState)
        {
            List<Move> moves = GameBoard.GetMoves(GameBoard.MyAISnailID, GameBoard.MyAISnailTrailID);

            if (Depth == MaxDepth)
            {
                //returns estimated guess of best score for board
                return GameBoard.GetEstimatedResult(GameBoard, WorldState);
            }

            foreach (Move m in moves)
            {
                if (AlphaValue >= BetaValue)
                {
                    //cuts off search
                    return AlphaValue;
                }

                //do move m
                GameBoard.DoMove(m);

                //minus value is current players poin of view
                double Move = -NegaMaxSearchFunction(GameBoard, Depth + 1, -BetaValue, -AlphaValue, WorldState);
                if (Move > AlphaValue)
                {
                    AlphaValue = Move;
                    if (Depth == 0)
                    {
                        //save best move from the root
                        NegaBest = m;
                    }
                }
                //undo move m
                GameBoard.UndoMove(m);
            }
            //returns alpha value 
            return AlphaValue;
        }
    }
}
