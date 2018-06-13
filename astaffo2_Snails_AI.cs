using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class astaffo2 : BasePlayer
    {
        //state of board from AI point of view
        static PlayerWorldState WorldState;

        //max deapth tree will search ( higher = slower)
        int MaxDepth = 4;

        GameBoard GameBoard;

        Move BestMove;

        public astaffo2() : base()
        {
            this.Name = "Skynet T-800";
        }

        public override ICommand GetTurnCommands(IPlayerWorldState igrid)
        {
            //the Board
            WorldState = (PlayerWorldState)igrid;

            // Search for the best move
            Negamax NM = new Negamax(-1000.0, 1000.0, MaxDepth);

            // Make a working copy of the board, with me about to move
            GameBoard = new GameBoard(WorldState);

            // No BestMove known yet - BestMove == null corresponds to passing the turn
            BestMove = null;

            double alpha = NM.NegaMaxSearchFunction(GameBoard, 0, -1000.0, 1000.0, WorldState);

            BestMove = NM.NegaBest;

            return BestMove.Com;
        }

    }
}
