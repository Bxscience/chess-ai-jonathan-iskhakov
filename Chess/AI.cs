using System;
using System.Collections.Generic;
using System.Linq;

public static class AI
{
    private static readonly Dictionary<char, int> pieceScore = new Dictionary<char, int>
    {
        {'K', 0}, {'Q', 9}, {'R', 5}, {'B', 3}, {'N', 3}, {'p', 1}
    };

    private static readonly double[][] knightScores = new double[][]
    {
        new double[] {0.0, 0.1, 0.2, 0.2, 0.2, 0.2, 0.1, 0.0},
        new double[] {0.1, 0.3, 0.5, 0.5, 0.5, 0.5, 0.3, 0.1},
        new double[] {0.2, 0.5, 0.6, 0.65, 0.65, 0.6, 0.5, 0.2},
        new double[] {0.2, 0.55, 0.65, 0.7, 0.7, 0.65, 0.55, 0.2},
        new double[] {0.2, 0.5, 0.65, 0.7, 0.7, 0.65, 0.5, 0.2},
        new double[] {0.2, 0.55, 0.6, 0.65, 0.65, 0.6, 0.55, 0.2},
        new double[] {0.1, 0.3, 0.5, 0.55, 0.55, 0.5, 0.3, 0.1},
        new double[] {0.0, 0.1, 0.2, 0.2, 0.2, 0.2, 0.1, 0.0}
    };

    private static readonly double[][] bishopScores = new double[][]
    {
        new double[] {0.0, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.0},
        new double[] {0.2, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.2},
        new double[] {0.2, 0.4, 0.5, 0.6, 0.6, 0.5, 0.4, 0.2},
        new double[] {0.2, 0.5, 0.5, 0.6, 0.6, 0.5, 0.5, 0.2},
        new double[] {0.2, 0.4, 0.6, 0.6, 0.6, 0.6, 0.4, 0.2},
        new double[] {0.2, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.2},
        new double[] {0.2, 0.5, 0.4, 0.4, 0.4, 0.4, 0.5, 0.2},
        new double[] {0.0, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.0}
    };

    private static readonly double[][] rookScores = new double[][]
    {
        new double[] {0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25},
        new double[] {0.5, 0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.5},
        new double[] {0.0, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.0},
        new double[] {0.0, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.0},
        new double[] {0.0, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.0},
        new double[] {0.0, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.0},
        new double[] {0.0, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.0},
        new double[] {0.25, 0.25, 0.25, 0.5, 0.5, 0.25, 0.25, 0.25}
    };

    private static readonly double[][] queenScores = new double[][]
    {
        new double[] {0.0, 0.2, 0.2, 0.3, 0.3, 0.2, 0.2, 0.0},
        new double[] {0.2, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.2},
        new double[] {0.2, 0.4, 0.5, 0.5, 0.5, 0.5, 0.4, 0.2},
        new double[] {0.3, 0.4, 0.5, 0.5, 0.5, 0.5, 0.4, 0.3},
        new double[] {0.4, 0.4, 0.5, 0.5, 0.5, 0.5, 0.4, 0.3},
        new double[] {0.2, 0.5, 0.5, 0.5, 0.5, 0.5, 0.4, 0.2},
        new double[] {0.2, 0.4, 0.5, 0.4, 0.4, 0.4, 0.4, 0.2},
        new double[] {0.0, 0.2, 0.2, 0.3, 0.3, 0.2, 0.2, 0.0}
    };

    private static readonly double[][] pawnScores = new double[][]
    {
        new double[] {0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8},
        new double[] {0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7},
        new double[] {0.3, 0.3, 0.4, 0.5, 0.5, 0.4, 0.3, 0.3},
        new double[] {0.25, 0.25, 0.3, 0.45, 0.45, 0.3, 0.25, 0.25},
        new double[] {0.2, 0.2, 0.2, 0.4, 0.4, 0.2, 0.2, 0.2},
        new double[] {0.25, 0.15, 0.1, 0.2, 0.2, 0.1, 0.15, 0.25},
        new double[] {0.25, 0.3, 0.3, 0.0, 0.0, 0.3, 0.3, 0.25},
        new double[] {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2}
    };

    private static readonly Dictionary<string, double[][]> piecePositionScores = new Dictionary<string, double[][]>
    {
        {"wN", knightScores},
        {"bN", knightScores.Reverse().ToArray()},
        {"wB", bishopScores},
        {"bB", bishopScores.Reverse().ToArray()},
        {"wQ", queenScores},
        {"bQ", queenScores.Reverse().ToArray()},
        {"wR", rookScores},
        {"bR", rookScores.Reverse().ToArray()},
        {"wp", pawnScores},
        {"bp", pawnScores.Reverse().ToArray()}
    };

    private const int CHECKMATE = 1000;
    private const int STALEMATE = 0;

    private static Move nextMove;

    public static Move FindBestMove(GameState gameState, List<Move> validMoves, int depth = 3)
    {
        nextMove = null;
        Shuffle(validMoves);
        FindMoveNegaMaxAlphaBeta(gameState, validMoves, depth, -CHECKMATE, CHECKMATE, gameState.WhiteToMove ? 1 : -1);
        return nextMove ?? FindRandomMove(validMoves);
    }

    private static int FindMoveNegaMaxAlphaBeta(GameState gameState, List<Move> validMoves, int depth, int alpha, int beta, int turnMultiplier, int toDepth = 3)
    {
        if (depth == 0)
        {
            return turnMultiplier * ScoreBoard(gameState);
        }

        int maxScore = -CHECKMATE;
        foreach (Move move in validMoves)
        {
            gameState.MakeMove(move);
            List<Move> nextMoves = gameState.GetValidMoves();
            int score = -FindMoveNegaMaxAlphaBeta(gameState, nextMoves, depth - 1, -beta, -alpha, -turnMultiplier);
            gameState.UndoMove();

            if (score > maxScore)
            {
                maxScore = score;
                if (depth == toDepth)
                {
                    nextMove = move;
                }
            }

            if (maxScore > alpha)
            {
                alpha = maxScore;
            }

            if (alpha >= beta)
            {
                break;
            }
        }
        return maxScore;
    }

    private static int ScoreBoard(GameState gameState)
    {
        if (gameState.Checkmate)
        {
            return gameState.WhiteToMove ? -CHECKMATE : CHECKMATE;
        }
        if (gameState.Stalemate)
        {
            return STALEMATE;
        }

        int score = 0;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                string piece = gameState.Board[row, col];
                if (piece != "--")
                {
                    double positionScore = 0;
                    if (piece[1] != 'K')
                    {
                        if (piecePositionScores.TryGetValue(piece, out double[][] scoreTable))
                        {
                            positionScore = scoreTable[row][col];
                        }
                    }

                    if (piece[0] == 'w')
                    {
                        score += pieceScore[piece[1]] + (int)positionScore;
                    }
                    else
                    {
                        score -= pieceScore[piece[1]] + (int)positionScore;
                    }
                }
            }
        }
        return score;
    }

    public static Move FindRandomMove(List<Move> validMoves)
    {
        Random rand = new Random();
        return validMoves.Count > 0 ? validMoves[rand.Next(validMoves.Count)] : null;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        Random rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}