﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

internal class MinimaxBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return BestMove(board, maxDepth: 5, maxBreadth: 12);
    }

    private Move BestMove(Board board, int maxDepth = -1, int maxBreadth = 0)
    {
        double alpha = double.NegativeInfinity, beta = double.PositiveInfinity;

        Comparison<double> scoreComparison = board.GetScoreComparison();
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;

        IEnumerable<Move> moves = board.GetSortedMoves();
        if (maxBreadth >= 1) moves = moves.Take(maxBreadth);
        Move bestMove = moves.First();

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = MiniMax(board, alpha, beta, maxDepth, maxBreadth);
            board.UndoMove(move);

            if (scoreComparison(score, bestScore) > 0)
            {
                (bestScore, bestMove) = (score, move);

                if (board.IsWhiteToMove) alpha = Math.Max(alpha, bestScore);
                else beta = Math.Min(beta, bestScore);
            }
        }

        return bestMove;
    }

    /// <summary>
    /// Performs the minimax algorithm up to a maximum search depth.
    /// Based on https://en.wikipedia.org/wiki/Minimax and https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning.
    /// </summary>
    /// <param name="board">Current board state.</param>
    /// <param name="alpha">Lower-bound for alpha-beta pruning.</param>
    /// <param name="alpha">Upper-bound for alpha-beta pruning.</param>
    /// <param name="maxDepth">Maximum search depth. Values less than 0 are treated as infinity.</param>
    /// <param name="maxBreadth">Maximum search breadth (number of moves considered per turn). Values less than 1 are treated as infinity.</param>
    /// <returns>Minimax value of board state.</returns>
    private double MiniMax(Board board, double alpha, double beta, int maxDepth, int maxBreadth)
    {
        if (maxDepth == 0 || board.IsGameOver()) return board.GetHeuristic();
        
        if (maxDepth > 0) maxDepth--;

        Comparison<double> scoreComparison = board.GetScoreComparison();
        double bestScore = board.IsWhiteToMove ? double.NegativeInfinity : double.PositiveInfinity;

        IEnumerable<Move> moves = board.GetSortedMoves();
        if (maxBreadth >= 1) moves = moves.Take(maxBreadth);

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = MiniMax(board, alpha, beta, maxDepth, maxBreadth);
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