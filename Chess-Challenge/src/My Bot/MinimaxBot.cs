using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

internal delegate double Heuristic(Board board);

internal class MinimaxBot : IChessBot
{
    /// <summary>
    /// Board heuristic used during minimax search.
    /// </summary>
    public Heuristic Heuristic { get; set; } = DefaultHeuristic;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return BestMove(board, maxDepth: 5, maxBreadth: 12);
    }

    /// <summary>
    /// Performs a minimax search up to a maximum depth and returns the best move for the current board state.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <param name="maxDepth">Maximum search depth. Values less than 0 are treated as infinity.</param>
    /// <param name="maxBreadth">Maximum search breadth (number of moves considered per turn). Values less than 1 are treated as infinity.</param>\
    /// <returns>Best move found for the current board state.</returns>
    public Move BestMove(Board board, int maxDepth = -1, int maxBreadth = 0)
    {
        MiniMax(board, double.NegativeInfinity, double.PositiveInfinity, maxDepth, maxBreadth, out Move bestMove);
        return bestMove;
    }

    /// <summary>
    /// Performs a minimax search up to a maximum depth.
    /// Based on https://en.wikipedia.org/wiki/Minimax and https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <param name="alpha">Lower-bound for alpha-beta pruning.</param>
    /// <param name="alpha">Upper-bound for alpha-beta pruning.</param>
    /// <param name="maxDepth">Maximum search depth. Values less than 0 are treated as infinity.</param>
    /// <param name="maxBreadth">Maximum search breadth (number of moves considered per turn). Values less than 1 are treated as infinity.</param>
    /// <param name="bestMove">Best move found for the current board state.</param>
    /// <returns>Minimax value of board state.</returns>
    public double MiniMax(Board board, double alpha, double beta, int maxDepth, int maxBreadth, out Move bestMove)
    {
        if (maxDepth == 0 || board.IsGameOver())
        {
            bestMove = Move.NullMove;
            return Heuristic(board);
        }
        
        if (maxDepth > 0) maxDepth--;

        Comparison<double> scoreComparison = GetScoreComparison(board);
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;

        IEnumerable<Move> moves = GetSortedMoves(board);
        if (maxBreadth >= 1) moves = moves.Take(maxBreadth);
        bestMove = moves.First();

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = MiniMax(board, alpha, beta, maxDepth, maxBreadth, out _);
            board.UndoMove(move);

            if (scoreComparison(score, bestScore) > 0)
            {
                bestScore = score;

                if (board.IsWhiteToMove) alpha = Math.Max(alpha, bestScore);
                else beta = Math.Min(beta, bestScore);

                if (alpha > beta) break;
            }
        }

        return bestScore;
    }

    /// <summary>
    /// Calculates a heuristic for the given move based on the board state.
    /// Equivalent to the heuristic of the board after the move has been made.
    /// </summary>
    /// <param name="board">Board state in which move will be made.</param>
    /// <param name="move">Move to evaluate.</param>
    /// <returns>Heuristic score for move.</returns>
    private double MoveHeuristic(Board board, Move move)
    {
        board.MakeMove(move);
        double score = Heuristic(board);
        board.UndoMove(move);
        return score;
    }

    /// <summary>
    /// Gets the legal moves given the current board state and sorts them from best to worst (from the current player's perspective).
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <returns>Sorted array of legal moves.</returns>
    private Move[] GetSortedMoves(Board board)
    {
        Comparison<double> scoreComparison = GetScoreComparison(board);
        Move[] moves = board.GetLegalMoves();
        Array.Sort(moves.Select(m => MoveHeuristic(board, m)).Select(s => -s).ToArray(), moves, Comparer<double>.Create(scoreComparison));
        return moves;
    }

    /// <summary>
    /// Gets a <see cref="SimpleComparison"/> for comparing whether one score is better than another.
    /// On white's turn, moves with higher (more positive) scores are better.
    /// On black's turn, moves with lower (more negative) scores are better.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <returns>Score comparison function.</returns>
    private static Comparison<double> GetScoreComparison(Board board) => board.IsWhiteToMove ? (a, b) => a.CompareTo(b) : (a, b) => b.CompareTo(a);

    /// <summary>
    /// Calculates a simple heuristic for the board state based on piece values.
    /// Based on https://www.chess.com/terms/chess-piece-value.
    /// </summary>
    /// <param name="board">Board state to evaluate.</param>
    /// <returns>Heuristic score for board.</returns>
    public static double DefaultHeuristic(Board board)
    {
        if (board.IsInCheckmate()) return board.IsWhiteToMove ? -1000 : 1000;
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
    /// <summary>
    /// Determines whether the game has ended.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <returns>Whether the game has ended.</returns>
    public static bool IsGameOver(this Board board) => board.IsInCheckmate() || board.IsDraw();
}
