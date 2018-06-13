using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class GameBoard
    {
        private const int IMPASSABLE = -100, EMPTY = 0;
        public int Impassable { get { return IMPASSABLE; } }
        public int Empty { get { return EMPTY; } }

        private int[,] boardStateArray;
        public int[,] BoardStateArray { get { return boardStateArray; } }

        public int MyAISnailID;
        public int MyAISnailTrailID { get { return -this.MyAISnailID; } }

        public int EnemyAiSnailID { get { return 3 - this.MyAISnailID; } }
        public int EnemyAiSnailTrailID { get { return MyAISnailTrailID - 3; } }

        private Vector2<int> playerStartPostition = new Vector2<int>(-9999, -9999);

        Random rnd = new Random();

        //creates copy of player world state
        public GameBoard(PlayerWorldState WorldState)
        {
            MyAISnailID = WorldState.ID;
            boardStateArray = InitialiseGameBoard(WorldState);

            if (WorldState.ID == 1)
            {
                playerStartPostition = new Vector2<int>(0, 0);
            }
            else
            {
                playerStartPostition = new Vector2<int>(WorldState.GridWidthInSquares - 1, WorldState.GridHeightInSquares - 1);
            }
        }

        //initiakise gameboard to specifed size
        public int[,] InitialiseGameBoard(PlayerWorldState WorldState)
        {
            int[,] newMap = new int[WorldState.GridWidthInSquares, WorldState.GridHeightInSquares];

            for (int x = 0; x < WorldState.GridWidthInSquares; x++)
            {
                for (int y = 0; y < WorldState.GridHeightInSquares; y++)
                {
                    switch (WorldState[x, y].Contents)
                    {
                        case GridSquare.ContentType.Snail:
                            newMap[x, y] = WorldState[x, y].Player;
                            break;
                        case GridSquare.ContentType.Trail:
                            newMap[x, y] = -WorldState[x, y].Player;
                            break;
                        case GridSquare.ContentType.Impassable:
                            newMap[x, y] = IMPASSABLE;
                            break;
                        default:
                            newMap[x, y] = EMPTY;
                            break;
                    }
                }
            }
            return newMap;
        }


        //gets board poistion after move m has been made
        public void DoMove(Move m)
        {
            if (m.Com != null)
            {
                foreach (SquareChange sc in m.Changes)
                {
                    Debug.Assert(boardStateArray[sc.X, sc.Y] == sc.oldSquare);
                    boardStateArray[sc.X, sc.Y] = sc.newSquare;
                }
            }
            // alternate between who is playing my ai or enemy ai
            MyAISnailID = 3 - MyAISnailID;
        }

        //gets board poistion after move m has been undone
        public void UndoMove(Move m)
        {
            foreach (SquareChange sc in m.Changes)
            {
                Debug.Assert(boardStateArray[sc.X, sc.Y] == sc.newSquare);
                boardStateArray[sc.X, sc.Y] = sc.oldSquare;
            }
            // alternate between who is playing my ai or enemy ai
            MyAISnailID = 3 - MyAISnailID;
        }

        //fill array with all possible moves for my ai, only consider moves which are not immediately blocked
        internal List<Move> GetMoves(int snailID, int snailTrailID)
        {
            List<Move> possibleAIMoves = new List<Move>();

            for (int x = 0; x < boardStateArray.GetLength(0); x++)
            {
                for (int y = 0; y < boardStateArray.GetLength(1); y++)
                {
                    if (boardStateArray[x, y] == snailID)
                    {
                        if (TileInBoard(x - 1, y))
                        {
                            if (boardStateArray[x - 1, y] == EMPTY || boardStateArray[x - 1, y] == snailTrailID)
                            {
                                possibleAIMoves.Add(GetMoves(new Command(x, y, Command.Direction.Left), snailID, snailTrailID));
                            }
                        }
                        if (TileInBoard(x + 1, y))
                        {
                            if (boardStateArray[x + 1, y] == EMPTY || boardStateArray[x + 1, y] == snailTrailID)
                            {
                                possibleAIMoves.Add(GetMoves(new Command(x, y, Command.Direction.Right), snailID, snailTrailID));
                            }
                        }
                        if (TileInBoard(x, y - 1))
                        {
                            if (boardStateArray[x, y - 1] == EMPTY || boardStateArray[x, y - 1] == snailTrailID)
                            {
                                possibleAIMoves.Add(GetMoves(new Command(x, y, Command.Direction.Down), snailID, snailTrailID));
                            }
                        }
                        if (TileInBoard(x, y + 1))
                        {
                            if (boardStateArray[x, y + 1] == EMPTY || boardStateArray[x, y + 1] == snailTrailID)
                            {
                                possibleAIMoves.Add(GetMoves(new Command(x, y, Command.Direction.Up), snailID, snailTrailID));
                            }
                        }
                    }
                }
            }
            return possibleAIMoves;
        }

        //if command c is carried out what is the resulting move
        internal Move GetMoves(Command c, int snailID, int snailTrailID)
        {

            Debug.Assert(boardStateArray[c.X, c.Y] == snailID);

            Move newMove = new Move(c);

            int dirX = 0;
            int dirY = 0;
            switch (c.DirectionToMove)
            {
                case Command.Direction.Up:
                    dirX = 0; dirY = +1;
                    break;
                case Command.Direction.Down:
                    dirX = 0; dirY = -1;
                    break;
                case Command.Direction.Right:
                    dirX = +1; dirY = 0;
                    break;
                case Command.Direction.Left:
                    dirX = -1; dirY = 0;
                    break;
            }

            //if the tile in dirX and dirY is empty that is the destination
            if (TileInBoard(c.X + dirX, c.Y + dirY))
            {
                if (boardStateArray[c.X + dirX, c.Y + dirY] == EMPTY)
                {
                    //change square from empty to my snail
                    newMove.AddSquareChange(new SquareChange(c.X, c.Y, snailID, snailTrailID));
                    //change square from my snail to empty
                    newMove.AddSquareChange(new SquareChange(c.X + dirX, c.Y + dirY, EMPTY, snailID));
                    return newMove;
                }
            }

            //slide aong my trail
            int distTraveled = 1;
            while (TileInBoard(c.X + distTraveled * dirX, c.Y + distTraveled * dirY) &&
                  boardStateArray[c.X + distTraveled * dirX, c.Y + distTraveled * dirY] == snailTrailID)
            {
                distTraveled++;
            }

            if (distTraveled > 1)
            {
                //change square from empty to my snail
                newMove.AddSquareChange(new SquareChange(c.X, c.Y, snailID, snailTrailID));
                newMove.AddSquareChange(new SquareChange(c.X + (distTraveled - 1) * dirX, c.Y + (distTraveled - 1) * dirY, snailTrailID, snailID));
                return newMove;
            }

            return newMove;
        }

        public bool TileInBoard(int x, int y)
        {
            if (0 <= x && x < boardStateArray.GetLength(0) && 0 <= y && y < boardStateArray.GetLength(1))
            {
                return true;
            }
            return false;
        }


        //returns estimated result of the board
        public double GetEstimatedResult(GameBoard lastBoard, PlayerWorldState WorldState)
        {
            Vector2<int> player = GetCoordinatesOfID(MyAISnailID);
            Vector2<int> enemy = GetCoordinatesOfID(EnemyAiSnailID);

            double TileScore = 0.0;
            TileScore += 1.0 * GetAdvantage();



            if (WorldState[enemy.x, enemy.y].Contents == EMPTY)
            {
                TileScore -= DistanceToEnemy(player.y, WorldState[enemy.x, enemy.y].Y);
                TileScore -= DistanceToEnemy(player.x, WorldState[enemy.x, enemy.y].X);
            }

            if (TileScore > 1000.0)
                TileScore = 1000.0;
            if (TileScore < -1000.0)
                TileScore = -1000.0;

            return TileScore;
        }


        //gets coordinates of the given id
        private Vector2<int> GetCoordinatesOfID(int id)
        {
            Vector2<int> bp = new Vector2<int>(0, 0);
            for (int x = 0; x < boardStateArray.GetLength(0); x++)
            {
                for (int y = 0; y < boardStateArray.GetLength(1); y++)
                {
                    if (boardStateArray[x, y] == id)
                    {
                        bp.x = x;
                        bp.y = y;
                        return bp;
                    }
                }
            }

            bp.x = -9999;
            bp.y = -9999;

            return bp;
        }

        //returns the tile advantage for depenign on what is in the square
        public double GetAdvantage()
        {
            double Advantage = 0.0;

            for (int x = 0; x < boardStateArray.GetLength(0); x++)
            {
                for (int y = 0; y < boardStateArray.GetLength(1); y++)
                {
                    if (boardStateArray[x, y] == MyAISnailID || boardStateArray[x, y] == MyAISnailTrailID)
                    {
                        //sets advantage to 1 plus a random number divided by 1 to produce a ranom difference between empty squares producng the random movment (e.g 1.25 or 1.22222)
                        Advantage = Advantage + 1 + 1 / rnd.Next(1, 9);
                    }
                    else if (boardStateArray[x, y] == EnemyAiSnailID || boardStateArray[x, y] == EnemyAiSnailID)
                    {
                        //sets advantage to -1 plus a random number divided by -1 to produce a ranom difference between empty squares producng the random movment (e.g -1.25 or -1.22222)
                        Advantage = Advantage - 1 - 1 / rnd.Next(1, 9);
                    }
                }
            }

            return Advantage;
        }

        private struct Vector2<T>
        {
            public T x;
            public T y;
            public Vector2(T _x, T _y)
            {
                x = _x;
                y = _y;
            }
        }

        private int DistanceToEnemy(int PointOne, int PointTwo)
        {

            return Math.Abs(PointOne - PointTwo);
        }
    }
}