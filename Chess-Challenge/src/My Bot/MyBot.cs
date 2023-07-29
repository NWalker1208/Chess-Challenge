﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class MyBot : IChessBot
{
    private const int NUM_INPUTS = 832;
    private const int NUM_PARAMS = 16 * NUM_INPUTS + 16 + 512 + 32 + 512 + 16 + 16 + 1;
    // TODO: Replace with actual pre-trained parameters
    private static readonly float[] PRETRAINED_PARAMS = Enumerable.Range(0, NUM_PARAMS).Select(_ => (float)(Random.Shared.NextDouble() * 2 - 1)).ToArray();

    private NeuralNet neuralNet;

    public MyBot()
    {
        neuralNet = new(new float[][,] { new float[16, NUM_INPUTS], new float[32, 16], new float[16, 32], new float[1, 16] },
                        new float[][] { new float[16], new float[32], new float[16], new float[1] });
        neuralNet.LoadParameters(PRETRAINED_PARAMS);
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        float[] evals = moves.Select(m => EvalMove(board, m)).ToArray();
        return moves[Array.IndexOf(evals, board.IsWhiteToMove ? evals.Max() : evals.Min())];
    }

    private float EvalMove(Board board, Move move)
    {
        board.MakeMove(move);
        float[] inputs = BoardToFenChars(board).Select(FenCharToOneHotIndex).SelectMany(OneHotIndexToInputs).ToArray();
        float eval = neuralNet.GetOutputs(inputs)[0];
        board.UndoMove(move);
        return eval;
    }

    internal static IEnumerable<char> BoardToFenChars(Board board)
        => board.GetFenString().Split(' ')[0].Split('/').SelectMany(rank => rank)
            .SelectMany(c => char.IsNumber(c) ? Enumerable.Repeat('_', int.Parse(c.ToString())) : Enumerable.Repeat(c, 1));

    internal static int FenCharToOneHotIndex(char c)
        => char.ToLower(c) switch
        {
            '_' => 0,
            'p' => 1,
            'n' => 2,
            'b' => 3,
            'r' => 4,
            'q' => 5,
            'k' => 6,
            _ => throw new ArgumentException($"Invalid FEN character: {c}")
        } + (char.IsUpper(c) ? 6 : 0);

    internal static IEnumerable<float> OneHotIndexToInputs(int i)
        => Enumerable.Repeat(0.0f, i).Append(1.0f).Concat(Enumerable.Repeat(0.0f, 12 - i));

    private class NeuralNet
    {
        private readonly float[][,] weights;
        private readonly float[][] biases;

        public NeuralNet(float[][,] weights, float[][] biases)
        {
            this.weights = weights;
            this.biases = biases;
        }

        public void LoadParameters(IEnumerable<float> parameters)
        {
            var p = parameters.GetEnumerator();
            p.MoveNext();
            for (int l = 0; l < weights.Length; l++)
            {
                for (int o = 0; o < weights[l].GetLength(0); o++)
                {
                    for (int i = 0; i < weights[l].GetLength(1); i++)
                    {
                        weights[l][o, i] = p.Current;
                        p.MoveNext();
                    }
                    biases[l][o] = p.Current;
                    p.MoveNext();
                }
            }
        }

        public float[] GetOutputs(float[] inputs)
        {
            for (int l = 0; l < weights.Length; l++)
            {
                inputs = ApplyLayer(inputs, weights[l], biases[l]);
            }
            return inputs;
        }

        private float[] ApplyLayer(float[] inputs, float[,] weights, float[] biases)
        {
            float[] outputs = new float[weights.GetLength(0)];
            for (int o = 0; o < outputs.Length; o++)
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    outputs[o] += inputs[i] * weights[o, i];
                }
                outputs[o] += biases[o];
                outputs[o] = Math.Max(0, outputs[o]); // ReLU
            }
            return outputs;
        }
    }
}