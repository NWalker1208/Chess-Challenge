using ChessChallenge.API;
using System;
using System.Linq;

internal class MinimaxBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return BestMove(board, maxDepth: 3);
    }

    private delegate bool SimpleComparison(double a, double b);

    private static bool IsGreater(double a, double b) => a > b;
    private static bool IsLess(double a, double b) => a < b;

    private Move BestMove(Board board, int maxDepth = -1)
    {
        Move[] moves = board.GetLegalMoves();

        SimpleComparison isBetter = board.IsWhiteToMove ? IsGreater : IsLess;
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = MiniMax(board, maxDepth);
            if (isBetter(score, bestScore)) (bestScore, bestMove) = (score, move);
            board.UndoMove(move);
        }

        return bestMove;
    }

    /// <summary>
    /// Performs the minimax algorithm up to a maximum search depth.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <param name="maxDepth">Maximum search depth. Values less than 0 are treated as infinity.</param>
    /// <returns>Minimax value of board state.</returns>
    private double MiniMax(Board board, int maxDepth)
    {
        if (maxDepth == 0 || board.IsGameOver()) return Heuristic(board);
        
        if (maxDepth > 0) maxDepth--;

        SimpleComparison isBetter = board.IsWhiteToMove ? IsGreater : IsLess;
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;
        
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            double score = MiniMax(board, maxDepth);
            if (isBetter(score, bestScore)) bestScore = score;
            board.UndoMove(move);
        }

        return bestScore;
    }

    /// <summary>
    /// Heuristic for evaluating board states.
    /// Based on https://www.chess.com/terms/chess-piece-value.
    /// </summary>
    /// <param name="board">Board state.</param>
    /// <returns>Heuristic score.</returns>
    private double Heuristic(Board board)
    {
        if (board.IsInCheckmate()) return board.IsWhiteToMove ? -100 : 100;
        if (board.IsDraw()) return 0;

        PieceList[] pieces = board.GetAllPieceLists();
        double score = (pieces[0].Count - pieces[6].Count) + 
                       (pieces[1].Count - pieces[7].Count) * 3 + 
                       (pieces[2].Count - pieces[8].Count) * 3 + 
                       (pieces[3].Count - pieces[9].Count) * 5 + 
                       (pieces[4].Count - pieces[10].Count) * 9;

        return score;
    }
}

internal static class BoardExtensions
{
    public static bool IsGameOver(this Board board) => board.IsInCheckmate() || board.IsDraw();
}
