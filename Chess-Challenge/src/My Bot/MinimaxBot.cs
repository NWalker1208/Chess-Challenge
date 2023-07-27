using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

internal class MinimaxBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return BestMove(board, maxDepth: 3);
    }

    private Move BestMove(Board board, int maxDepth = -1)
    {
        Comparison<double> scoreComparison = board.GetScoreComparison();
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;

        Move[] moves = board.GetSortedMoves();
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = MiniMax(board, maxDepth);
            if (scoreComparison(score, bestScore) > 0) (bestScore, bestMove) = (score, move);
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
        if (maxDepth == 0 || board.IsGameOver()) return board.GetHeuristic();
        
        if (maxDepth > 0) maxDepth--;

        Comparison<double> scoreComparison = board.GetScoreComparison();
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;

        Move[] moves = board.GetSortedMoves();

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = MiniMax(board, maxDepth);
            if (scoreComparison(score, bestScore) > 0) bestScore = score;
            board.UndoMove(move);
        }

        return bestScore;
    }
}

internal static class BoardExtensions
{
    /// <summary>
    /// Gets a <see cref="SimpleComparison"/> for comparing whether one score is better than another.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <returns>Score comparison function.</returns>
    public static Comparison<double> GetScoreComparison(this Board board) => board.IsWhiteToMove ? (a, b) => a.CompareTo(b) : (a, b) => b.CompareTo(a);

    /// <summary>
    /// Determines whether the game has ended.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <returns>Whether the game has ended.</returns>
    public static bool IsGameOver(this Board board) => board.IsInCheckmate() || board.IsDraw();

    /// <summary>
    /// Calculates a heuristic for the board state.
    /// Based on https://www.chess.com/terms/chess-piece-value.
    /// </summary>
    /// <param name="board">Board state to evaluate.</param>
    /// <returns>Heuristic score for board.</returns>
    public static double GetHeuristic(this Board board)
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

    /// <summary>
    /// Calculates a heuristic for the given move based on the board state.
    /// Equivalent to the heuristic of the board after the move has been made.
    /// </summary>
    /// <param name="board">Board state in which move will be made.</param>
    /// <param name="move">Move to evaluate.</param>
    /// <returns>Heuristic score for move.</returns>
    public static double GetMoveHeuristic(this Board board, Move move)
    {
        board.MakeMove(move);
        double score = board.GetHeuristic();
        board.UndoMove(move);
        return score;
    }

    /// <summary>
    /// Gets the legal moves given the current board state sorted from best to worst.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <returns>Sorted array of legal moves.</returns>
    public static Move[] GetSortedMoves(this Board board)
    {
        Comparison<double> scoreComparison = board.GetScoreComparison();
        Move[] moves = board.GetLegalMoves();
        Array.Sort(moves.Select(board.GetMoveHeuristic).Select(s => -s).ToArray(), moves, Comparer<double>.Create(scoreComparison));
        return moves;
    }
}
