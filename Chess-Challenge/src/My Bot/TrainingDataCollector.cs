using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TrainingDataCollector
{
    const int MAX_DEPTH = 5;
    const int MAX_BREADTH = 12;

    public void CollectTrainingData(int sampleSize = int.MaxValue)
    {
        MinimaxBot minimaxBot = new();
        string[] fenStrings = FileHelper.ReadResourceFile("Fens.txt").Split('\n').Where(fen => fen.Length > 0).ToArray();
        Board[] boards = fenStrings.Take(sampleSize).Select(Board.CreateBoardFromFEN).ToArray();
        
        ParallelQuery<int[]> inputs = boards.AsParallel().Select(board => MyBot.BoardToFenChars(board).Select(MyBot.FenCharToOneHotIndex).ToArray());
        ParallelQuery<float> scores = boards.AsParallel().Select(board => (float)minimaxBot.MiniMax(board, double.NegativeInfinity, double.PositiveInfinity, MAX_DEPTH, MAX_BREADTH, out _));
        IEnumerable<(int[], float)> samples = inputs.Zip(scores, (i, s) => (i, s));
        
        Stopwatch sw = new();
        sw.Restart();
        (int[], float)[] samplesArray = samples.ToArray();
        sw.Stop();
        Console.WriteLine($"Total time elapsed: {sw.Elapsed}");
        Console.WriteLine($"Time per sample: {sw.Elapsed.TotalSeconds / samplesArray.Length} seconds");
        // TODO: Save inputs and scores to a numpy or pandas compatible file
    }
}
