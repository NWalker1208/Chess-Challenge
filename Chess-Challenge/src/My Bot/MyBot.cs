using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    MinimaxBot tempBrain = new();

    public Move Think(Board board, Timer timer)
    {
        return tempBrain.Think(board, timer);
    }
}