﻿using ChessChallenge.API;
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
        
        ParallelQuery<float[]> inputs = boards.AsParallel().Select(MyBot.BoardToInputs);
        ParallelQuery<float> scores = boards.AsParallel().Select(board => (float)minimaxBot.MiniMax(board, double.NegativeInfinity, double.PositiveInfinity, MAX_DEPTH, MAX_BREADTH, out _));
        IEnumerable<(float[], float)> samples = inputs.Zip(scores, (i, s) => (i, s));
        
        Stopwatch sw = new();
        sw.Restart();
        samples.ToArray();
        sw.Stop();
        Console.WriteLine($"Time passed: {sw.Elapsed}");
        // TODO: Save inputs and scores to a numpy or pandas compatible file
    }
}