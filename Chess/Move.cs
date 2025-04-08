using System.Collections.Generic;

public class Move
{
    public static Dictionary<string, int> ranksToRows = new Dictionary<string, int>
    {
        {"1", 7}, {"2", 6}, {"3", 5}, {"4", 4},
        {"5", 3}, {"6", 2}, {"7", 1}, {"8", 0}
    };
    public static Dictionary<int, string> rowsToRanks = new Dictionary<int, string>();
    public static Dictionary<string, int> filesToCols = new Dictionary<string, int>
    {
        {"a", 0}, {"b", 1}, {"c", 2}, {"d", 3},
        {"e", 4}, {"f", 5}, {"g", 6}, {"h", 7}
    };
    public static Dictionary<int, string> colsToFiles = new Dictionary<int, string>();

    static Move()
    {
        foreach (var kvp in ranksToRows)
        {
            rowsToRanks[kvp.Value] = kvp.Key;
        }
        foreach (var kvp in filesToCols)
        {
            colsToFiles[kvp.Value] = kvp.Key;
        }
    }

    public int StartRow { get; private set; }
    public int StartCol { get; private set; }
    public int EndRow { get; private set; }
    public int EndCol { get; private set; }
    public string PieceMoved { get; private set; }
    public string PieceCaptured { get; private set; }
    public bool IsPawnPromotion { get; private set; }
    public bool IsEnpassantMove { get; private set; }
    public bool IsCastleMove { get; private set; }
    public bool IsCapture { get; private set; }
    public int MoveID { get; private set; }

    public Move((int, int) startSquare, (int, int) endSquare, string[,] board, bool isEnpassantMove = false, bool isCastleMove = false)
    {
        StartRow = startSquare.Item1;
        StartCol = startSquare.Item2;
        EndRow = endSquare.Item1;
        EndCol = endSquare.Item2;
        PieceMoved = board[StartRow, StartCol];
        PieceCaptured = board[EndRow, EndCol];

        IsPawnPromotion = (PieceMoved == "wp" && EndRow == 0) || (PieceMoved == "bp" && EndRow == 7);
        IsEnpassantMove = isEnpassantMove;
        if (IsEnpassantMove)
        {
            PieceCaptured = PieceMoved == "bp" ? "wp" : "bp";
        }
        IsCastleMove = isCastleMove;

        IsCapture = PieceCaptured != "--";
        MoveID = StartRow * 1000 + StartCol * 100 + EndRow * 10 + EndCol;
    }

    public override bool Equals(object obj)
    {
        if (obj is Move other)
        {
            return MoveID == other.MoveID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return MoveID.GetHashCode();
    }

    public static bool operator ==(Move left, Move right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.MoveID == right.MoveID;
    }

    public static bool operator !=(Move left, Move right)
    {
        return !(left == right);
    }

    public string GetChessNotation()
    {
        if (IsPawnPromotion)
        {
            return GetRankFile(EndRow, EndCol) + "Q";
        }
        if (IsCastleMove)
        {
            return EndCol == 1 ? "0-0-0" : "0-0";
        }
        if (IsEnpassantMove)
        {
            return $"{colsToFiles[StartCol]}x{GetRankFile(EndRow, EndCol)} e.p.";
        }
        if (PieceCaptured != "--")
        {
            if (PieceMoved[1] == 'p')
            {
                return $"{colsToFiles[StartCol]}x{GetRankFile(EndRow, EndCol)}";
            }
            else
            {
                return $"{PieceMoved[1]}x{GetRankFile(EndRow, EndCol)}";
            }
        }
        else
        {
            if (PieceMoved[1] == 'p')
            {
                return GetRankFile(EndRow, EndCol);
            }
            else
            {
                return $"{PieceMoved[1]}{GetRankFile(EndRow, EndCol)}";
            }
        }
    }

    public string GetRankFile(int row, int col)
    {
        return colsToFiles[col] + rowsToRanks[row];
    }

    public override string ToString()
    {
        if (IsCastleMove)
        {
            return EndCol == 6 ? "0-0" : "0-0-0";
        }

        string endSquare = GetRankFile(EndRow, EndCol);

        if (PieceMoved[1] == 'p')
        {
            if (IsCapture)
            {
                return $"{colsToFiles[StartCol]}x{endSquare}";
            }
            else
            {
                return endSquare + (IsPawnPromotion ? "Q" : "");
            }
        }

        string moveString = PieceMoved[1].ToString();
        if (IsCapture)
        {
            moveString += "x";
        }
        return moveString + endSquare;
    }
}