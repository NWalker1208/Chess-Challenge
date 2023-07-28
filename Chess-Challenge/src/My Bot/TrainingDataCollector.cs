using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TrainingDataCollector
{
    const int MAX_DEPTH = 5;
    const int MAX_BREADTH = 12;

    public void CollectTrainingData()
    {
        MinimaxBot minimaxBot = new();
        string[] fenStrings = FileHelper.ReadResourceFile("Fens.txt").Split('\n').Where(fen => fen.Length > 0).ToArray();
        Board[] boards = fenStrings.Select(Board.CreateBoardFromFEN).ToArray();
        float[][] inputs = boards.Select(MyBot.BoardToInputs).ToArray(); 
        float[] scores = boards.Select(board => (float)minimaxBot.MiniMax(board, double.NegativeInfinity, double.PositiveInfinity, MAX_DEPTH, MAX_BREADTH, out _)).ToArray();
        
        // TODO: Save inputs and scores to a numpy or pandas compatible file
    }
}
