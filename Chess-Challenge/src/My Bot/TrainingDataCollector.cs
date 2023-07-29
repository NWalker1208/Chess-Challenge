using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        
        ParallelQuery<int[]> inputs = boards.AsParallel().Select(board => MyBot.BoardToFenChars(board).Select(MyBot.FenCharToClassIndex).ToArray());
        ParallelQuery<float> scores = boards.AsParallel().Select(board => (float)minimaxBot.MiniMax(board, double.NegativeInfinity, double.PositiveInfinity, MAX_DEPTH, MAX_BREADTH, out _));
        IEnumerable<(int[] Inputs, float Score)> samples = inputs.Zip(scores, (i, s) => (i, s));

        int sampleCount = 0;
        Stopwatch sw = new();
        sw.Restart();
        File.WriteAllLines("samples.csv", samples.Select(sample =>
        {
            sampleCount++;
            return $"{string.Join(',', sample.Inputs)},{sample.Score:e9}";
        }));
        sw.Stop();
        Console.WriteLine($"Total time elapsed: {sw.Elapsed}");
        Console.WriteLine($"Number of samples: {sampleCount}");
        Console.WriteLine($"Time per sample: {sw.Elapsed.TotalSeconds / sampleCount} seconds");
    }
}
