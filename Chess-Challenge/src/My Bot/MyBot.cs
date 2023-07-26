using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return BestMove(board, maxDepth: 16)?.Move ?? moves[0];
    }

    /// <summary>
    /// Calculates the (approximately) best move for the current player based on the given board state.
    /// </summary>
    /// <param name="board">Board to determine best move for.</param>
    /// <param name="alpha">Minimum score before pruning search branches.</param>
    /// <param name="beta">Maximum score before pruning search branches.</param>
    /// <param name="maxDepth">Maximum depth of min-max search. Negative values are treated as infinity.</param>
    /// <returns>Best move and its associated score, or null if search was terminated early.</returns>
    private (Move Move, double Score)? BestMove(Board board, double alpha = double.NegativeInfinity, double beta = double.PositiveInfinity, int maxDepth = -1)
    {
        Move? bestMove = null;
        double bestScore = board.IsWhiteToMove ? alpha : beta;
        int desiredSign = board.IsWhiteToMove ? 1 : -1;

        Move[] moves = board.GetLegalMoves();
        double[] moveScores = moves.Select(m => EvaluateMove(board, m) * -desiredSign).ToArray();
        Array.Sort(moveScores, moves);

        if (maxDepth == 0 || moveScores[0] == 100.0 * desiredSign) return (moves[0], moveScores[0]);
        if (maxDepth > 0) maxDepth--;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            if (BestMove(board, alpha, beta, maxDepth) is (Move, double) next) {
                if (next.Score * desiredSign > bestScore * desiredSign) {
                    (bestMove, bestScore) = next;
                    if (desiredSign == 1) alpha = bestScore;
                    else beta = bestScore;
                }
            }
            board.UndoMove(move);

            if (beta < alpha) return null;
        }

        return bestMove == null ? null : ((Move)bestMove, bestScore);
    }

    /// <summary>
    /// Evaluates the given board state and returns a positive or negative score.
    /// A positive score favors white and a negative score favors black.
    /// </summary>
    /// <param name="board">Board to evaluate.</param>
    /// <returns>Evaluation score.</returns>
    private double EvaluateBoard(Board board)
    {
        return board.IsInCheckmate() ? (board.IsWhiteToMove ? -100.0 : 100.0) : 0.0;
    }


    /// <summary>
    /// Evaluates the given move based on the given board state and returns a positive or negative score.
    /// A positive score favors white and a negative score favors black.
    /// </summary>
    /// <param name="board">Board state in which move will be made.</param>
    /// <param name="move">Move to evaluate.</param>
    /// <returns>Evaluation score.</returns>
    private double EvaluateMove(Board board, Move move)
    {
        board.MakeMove(move);
        double score = EvaluateBoard(board);
        board.UndoMove(move);
        return score;
    }
}