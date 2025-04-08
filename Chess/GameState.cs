using System;
using System.Collections.Generic;

public class GameState
{
    public string[,] Board { get; private set; }
    private Dictionary<char, Action<int, int, List<Move>>> moveFunctions;
    public bool WhiteToMove { get; private set; }
    public List<Move> MoveLog { get; private set; }
    private (int, int) whiteKingLocation;
    private (int, int) blackKingLocation;
    public bool Checkmate { get; private set; }
    public bool Stalemate { get; private set; }
    public bool inCheck;
    private List<(int, int, int, int)> pins;
    private List<(int, int, int, int)> checks;
    public (int, int) EnpassantPossible { get; private set; }
    private List<(int, int)> enpassantPossibleLog;
    public CastleRights CurrentCastleRights { get; private set; }
    private List<CastleRights> castleRightsLog;

    public GameState()
    {
        Board = new string[8, 8]
        {
            {"bR", "bN", "bB", "bQ", "bK", "bB", "bN", "bR"},
            {"bp", "bp", "bp", "bp", "bp", "bp", "bp", "bp"},
            {"--", "--", "--", "--", "--", "--", "--", "--"},
            {"--", "--", "--", "--", "--", "--", "--", "--"},
            {"--", "--", "--", "--", "--", "--", "--", "--"},
            {"--", "--", "--", "--", "--", "--", "--", "--"},
            {"wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp"},
            {"wR", "wN", "wB", "wQ", "wK", "wB", "wN", "wR"}
        };
        moveFunctions = new Dictionary<char, Action<int, int, List<Move>>>
        {
            {'p', GetPawnMoves},
            {'R', GetRookMoves},
            {'N', GetKnightMoves},
            {'B', GetBishopMoves},
            {'Q', GetQueenMoves},
            {'K', GetKingMoves}
        };
        WhiteToMove = true;
        MoveLog = new List<Move>();
        whiteKingLocation = (7, 4);
        blackKingLocation = (0, 4);
        Checkmate = false;
        Stalemate = false;
        EnpassantPossible = (-1, -1);
        enpassantPossibleLog = new List<(int, int)> { EnpassantPossible };
        CurrentCastleRights = new CastleRights(true, true, true, true);
        castleRightsLog = new List<CastleRights> { new CastleRights(CurrentCastleRights.WKS, CurrentCastleRights.BKS, CurrentCastleRights.WQS, CurrentCastleRights.BQS) };
    }

    public void MakeMove(Move move)
    {
        Board[move.StartRow, move.StartCol] = "--";
        Board[move.EndRow, move.EndCol] = move.PieceMoved;
        MoveLog.Add(move);
        WhiteToMove = !WhiteToMove;

        if (move.PieceMoved == "wK")
            whiteKingLocation = (move.EndRow, move.EndCol);
        else if (move.PieceMoved == "bK")
            blackKingLocation = (move.EndRow, move.EndCol);

        if (move.IsPawnPromotion)
            Board[move.EndRow, move.EndCol] = move.PieceMoved[0] + "Q";

        if (move.IsEnpassantMove)
            Board[move.StartRow, move.EndCol] = "--";

        if (move.PieceMoved[1] == 'p' && Math.Abs(move.StartRow - move.EndRow) == 2)
            EnpassantPossible = ((move.StartRow + move.EndRow) / 2, move.StartCol);
        else
            EnpassantPossible = (-1, -1);

        if (move.IsCastleMove)
        {
            if (move.EndCol - move.StartCol == 2)
            {
                Board[move.EndRow, move.EndCol - 1] = Board[move.EndRow, move.EndCol + 1];
                Board[move.EndRow, move.EndCol + 1] = "--";
            }
            else
            {
                Board[move.EndRow, move.EndCol + 1] = Board[move.EndRow, move.EndCol - 2];
                Board[move.EndRow, move.EndCol - 2] = "--";
            }
        }

        enpassantPossibleLog.Add(EnpassantPossible);
        UpdateCastleRights(move);
        castleRightsLog.Add(new CastleRights(CurrentCastleRights.WKS, CurrentCastleRights.BKS, CurrentCastleRights.WQS, CurrentCastleRights.BQS));
    }

    public void UndoMove()
    {
        if (MoveLog.Count == 0) return;

        Move move = MoveLog[MoveLog.Count - 1];
        MoveLog.RemoveAt(MoveLog.Count - 1);

        Board[move.StartRow, move.StartCol] = move.PieceMoved;
        Board[move.EndRow, move.EndCol] = move.PieceCaptured;
        WhiteToMove = !WhiteToMove;

        if (move.PieceMoved == "wK")
            whiteKingLocation = (move.StartRow, move.StartCol);
        else if (move.PieceMoved == "bK")
            blackKingLocation = (move.StartRow, move.StartCol);

        if (move.IsEnpassantMove)
            Board[move.EndRow, move.EndCol] = "--";

        enpassantPossibleLog.RemoveAt(enpassantPossibleLog.Count - 1);
        EnpassantPossible = enpassantPossibleLog[enpassantPossibleLog.Count - 1];

        castleRightsLog.RemoveAt(castleRightsLog.Count - 1);
        CurrentCastleRights = castleRightsLog[castleRightsLog.Count - 1];

        if (move.IsCastleMove)
        {
            if (move.EndCol - move.StartCol == 2)
            {
                Board[move.EndRow, move.EndCol + 1] = Board[move.EndRow, move.EndCol - 1];
                Board[move.EndRow, move.EndCol - 1] = "--";
            }
            else
            {
                Board[move.EndRow, move.EndCol - 2] = Board[move.EndRow, move.EndCol + 1];
                Board[move.EndRow, move.EndCol + 1] = "--";
            }
        }
    }

    private void UpdateCastleRights(Move move)
    {
        if (move.PieceMoved == "wK")
        {
            CurrentCastleRights.WKS = false;
            CurrentCastleRights.WQS = false;
        }
        else if (move.PieceMoved == "bK")
        {
            CurrentCastleRights.BKS = false;
            CurrentCastleRights.BQS = false;
        }
        else if (move.PieceMoved == "wR")
        {
            if (move.StartRow == 7)
            {
                if (move.StartCol == 0)
                    CurrentCastleRights.WQS = false;
                else if (move.StartCol == 7)
                    CurrentCastleRights.WKS = false;
            }
        }
        else if (move.PieceMoved == "bR")
        {
            if (move.StartRow == 0)
            {
                if (move.StartCol == 0)
                    CurrentCastleRights.BQS = false;
                else if (move.StartCol == 7)
                    CurrentCastleRights.BKS = false;
            }
        }

        if (move.PieceCaptured == "wR")
        {
            if (move.EndCol == 0)
                CurrentCastleRights.WQS = false;
            else if (move.EndCol == 7)
                CurrentCastleRights.WKS = false;
        }
        else if (move.PieceCaptured == "bR")
        {
            if (move.EndCol == 0)
                CurrentCastleRights.BQS = false;
            else if (move.EndCol == 7)
                CurrentCastleRights.BKS = false;
        }
    }

    public List<Move> GetValidMoves()
    {
        var tempCastleRights = new CastleRights(
            CurrentCastleRights.WKS, CurrentCastleRights.BKS,
            CurrentCastleRights.WQS, CurrentCastleRights.BQS
        );

        (inCheck, pins, checks) = CheckForPinsAndChecks();
        var moves = new List<Move>();

        (int kingRow, int kingCol) = WhiteToMove ? whiteKingLocation : blackKingLocation;

        if (inCheck)
        {
            if (checks.Count == 1)
            {
                moves = GetAllPossibleMoves();
                var check = checks[0];
                var validSquares = new List<(int, int)>();

                if (Board[check.Item1, check.Item2][1] == 'N')
                {
                    validSquares.Add((check.Item1, check.Item2));
                }
                else
                {
                    for (int i = 1; i < 8; i++)
                    {
                        int validRow = kingRow + check.Item3 * i;
                        int validCol = kingCol + check.Item4 * i;
                        validSquares.Add((validRow, validCol));
                        if (validRow == check.Item1 && validCol == check.Item2) break;
                    }
                }

                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    if (moves[i].PieceMoved[1] != 'K')
                    {
                        if (!validSquares.Contains((moves[i].EndRow, moves[i].EndCol)))
                            moves.RemoveAt(i);
                    }
                }
            }
            else
            {
                GetKingMoves(kingRow, kingCol, moves);
            }
        }
        else
        {
            moves = GetAllPossibleMoves();
            if (WhiteToMove)
                GetCastleMoves(whiteKingLocation.Item1, whiteKingLocation.Item2, moves);
            else
                GetCastleMoves(blackKingLocation.Item1, blackKingLocation.Item2, moves);
        }

        Checkmate = moves.Count == 0 && inCheck;
        Stalemate = moves.Count == 0 && !inCheck;

        CurrentCastleRights = tempCastleRights;
        return moves;
    }

    private bool InCheck()
    {
        return WhiteToMove ?
            SquareUnderAttack(whiteKingLocation.Item1, whiteKingLocation.Item2) :
            SquareUnderAttack(blackKingLocation.Item1, blackKingLocation.Item2);
    }

    private bool SquareUnderAttack(int row, int col)
    {
        WhiteToMove = !WhiteToMove;
        var opponentMoves = GetAllPossibleMoves();
        WhiteToMove = !WhiteToMove;

        foreach (var move in opponentMoves)
            if (move.EndRow == row && move.EndCol == col)
                return true;
        return false;
    }

    private List<Move> GetAllPossibleMoves()
    {
        var moves = new List<Move>();
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                char pieceColor = Board[row, col][0];
                if ((pieceColor == 'w' && WhiteToMove) || (pieceColor == 'b' && !WhiteToMove))
                {
                    char pieceType = Board[row, col][1];
                    moveFunctions[pieceType](row, col, moves);
                }
            }
        }
        return moves;
    }

    private (bool, List<(int, int, int, int)>, List<(int, int, int, int)>) CheckForPinsAndChecks()
    {
        List<(int, int, int, int)> pins = new List<(int, int, int, int)>();
        List<(int, int, int, int)> checks = new List<(int, int, int, int)>();
        bool inCheck = false;

        (int kingRow, int kingCol) = WhiteToMove ? whiteKingLocation : blackKingLocation;
        char allyColor = WhiteToMove ? 'w' : 'b';
        char enemyColor = WhiteToMove ? 'b' : 'w';
        var directions = new (int, int)[]
        {
            (-1, 0), (0, -1), (1, 0), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        for (int j = 0; j < directions.Length; j++)
        {
            (int dr, int dc) = directions[j];
            (int, int, int, int)? possiblePin = null;

            for (int i = 1; i < 8; i++)
            {
                int endRow = kingRow + dr * i;
                int endCol = kingCol + dc * i;

                if (endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8) break;

                string endPiece = Board[endRow, endCol];
                if (endPiece[0] == allyColor && endPiece[1] != 'K')
                {
                    if (!possiblePin.HasValue)
                        possiblePin = (endRow, endCol, dr, dc);
                    else
                        break;
                }
                else if (endPiece[0] == enemyColor)
                {
                    char pieceType = endPiece[1];
                    bool validCheck = false;

                    if ((j < 4 && pieceType == 'R') ||
                        (j >= 4 && pieceType == 'B') ||
                        (i == 1 && pieceType == 'p' && (
                            (enemyColor == 'b' && j >= 4 && j <= 5) ||
                            (enemyColor == 'w' && j >= 6 && j <= 7))) ||
                        pieceType == 'Q' ||
                        (i == 1 && pieceType == 'K'))
                    {
                        validCheck = true;
                    }

                    if (validCheck)
                    {
                        if (possiblePin.HasValue)
                            pins.Add(possiblePin.Value);
                        else
                        {
                            inCheck = true;
                            checks.Add((endRow, endCol, dr, dc));
                        }
                        break;
                    }
                    else
                        break;
                }
            }
        }

        var knightMoves = new (int, int)[]
        {
            (-2, -1), (-2, 1), (-1, 2), (1, 2),
            (2, -1), (2, 1), (-1, -2), (1, -2)
        };

        foreach (var (dr, dc) in knightMoves)
        {
            int endRow = kingRow + dr;
            int endCol = kingCol + dc;
            if (endRow >= 0 && endRow < 8 && endCol >= 0 && endCol < 8)
            {
                string endPiece = Board[endRow, endCol];
                if (endPiece[0] == enemyColor && endPiece[1] == 'N')
                {
                    inCheck = true;
                    checks.Add((endRow, endCol, dr, dc));
                }
            }
        }

        return (inCheck, pins, checks);
    }

    private void GetPawnMoves(int row, int col, List<Move> moves)
    {
        bool piecePinned = false;
        (int, int) pinDirection = (-1, -1);
        foreach (var pin in pins)
        {
            if (pin.Item1 == row && pin.Item2 == col)
            {
                piecePinned = true;
                pinDirection = (pin.Item3, pin.Item4);
                break;
            }
        }

        int moveAmount = WhiteToMove ? -1 : 1;
        int startRow = WhiteToMove ? 6 : 1;
        string enemyColor = WhiteToMove ? "b" : "w";

        if (Board[row + moveAmount, col] == "--")
        {
            if (!piecePinned || pinDirection == (moveAmount, 0))
            {
                moves.Add(new Move((row, col), (row + moveAmount, col), Board));
                if (row == startRow && Board[row + 2 * moveAmount, col] == "--")
                    moves.Add(new Move((row, col), (row + 2 * moveAmount, col), Board));
            }
        }

        for (int dCol = -1; dCol <= 1; dCol += 2)
        {
            if (col + dCol < 0 || col + dCol >= 8) continue;

            if (Board[row + moveAmount, col + dCol][0] == enemyColor[0])
            {
                if (!piecePinned || pinDirection == (moveAmount, dCol))
                    moves.Add(new Move((row, col), (row + moveAmount, col + dCol), Board));
            }
            else if ((row + moveAmount, col + dCol) == EnpassantPossible)
            {
                bool attackingPiece = false;
                bool blockingPiece = false;

                (int kingRow, int kingCol) = WhiteToMove ? whiteKingLocation : blackKingLocation;
                if (kingRow == row)
                {
                    int start = Math.Min(col, kingCol) + 1;
                    int end = Math.Max(col, kingCol);
                    for (int c = start; c < end; c++)
                        if (Board[row, c] != "--")
                            blockingPiece = true;

                    for (int c = (dCol == -1) ? col - 2 : col + 2; (dCol == -1) ? c >= 0 : c < 8; c += dCol)
                    {
                        string piece = Board[row, c];
                        if (piece[0] == enemyColor[0] && (piece[1] == 'R' || piece[1] == 'Q'))
                            attackingPiece = true;
                        else if (piece != "--")
                            break;
                    }
                }

                if (!attackingPiece || blockingPiece)
                    moves.Add(new Move((row, col), (row + moveAmount, col + dCol), Board, isEnpassantMove: true));
            }
        }
    }

    private void GetRookMoves(int row, int col, List<Move> moves)
    {
        bool piecePinned = false;
        (int, int) pinDirection = (-1, -1);
        foreach (var pin in pins)
        {
            if (pin.Item1 == row && pin.Item2 == col)
            {
                piecePinned = true;
                pinDirection = (pin.Item3, pin.Item4);
                if (Board[row, col][1] != 'Q') pins.Remove(pin);
                break;
            }
        }

        var directions = new (int, int)[] { (-1, 0), (0, -1), (1, 0), (0, 1) };
        string enemyColor = WhiteToMove ? "b" : "w";

        foreach (var (dr, dc) in directions)
        {
            for (int i = 1; i < 8; i++)
            {
                int endRow = row + dr * i;
                int endCol = col + dc * i;
                if (endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8) break;

                if (!piecePinned || pinDirection == (dr, dc) || pinDirection == (-dr, -dc))
                {
                    string endPiece = Board[endRow, endCol];
                    if (endPiece == "--")
                        moves.Add(new Move((row, col), (endRow, endCol), Board));
                    else if (endPiece[0] == enemyColor[0])
                    {
                        moves.Add(new Move((row, col), (endRow, endCol), Board));
                        break;
                    }
                    else
                        break;
                }
            }
        }
    }

    private void GetKnightMoves(int row, int col, List<Move> moves)
    {
        bool piecePinned = false;
        foreach (var pin in pins)
        {
            if (pin.Item1 == row && pin.Item2 == col)
            {
                piecePinned = true;
                break;
            }
        }
        if (piecePinned) return;

        var knightMoves = new (int, int)[]
        {
            (-2, -1), (-2, 1), (-1, 2), (1, 2),
            (2, -1), (2, 1), (-1, -2), (1, -2)
        };
        string allyColor = WhiteToMove ? "w" : "b";

        foreach (var (dr, dc) in knightMoves)
        {
            int endRow = row + dr;
            int endCol = col + dc;
            if (endRow >= 0 && endRow < 8 && endCol >= 0 && endCol < 8)
            {
                string endPiece = Board[endRow, endCol];
                if (endPiece[0] != allyColor[0])
                    moves.Add(new Move((row, col), (endRow, endCol), Board));
            }
        }
    }

    private void GetBishopMoves(int row, int col, List<Move> moves)
    {
        bool piecePinned = false;
        (int, int) pinDirection = (-1, -1);
        foreach (var pin in pins)
        {
            if (pin.Item1 == row && pin.Item2 == col)
            {
                piecePinned = true;
                pinDirection = (pin.Item3, pin.Item4);
                break;
            }
        }

        var directions = new (int, int)[] { (-1, -1), (-1, 1), (1, 1), (1, -1) };
        string enemyColor = WhiteToMove ? "b" : "w";

        foreach (var (dr, dc) in directions)
        {
            for (int i = 1; i < 8; i++)
            {
                int endRow = row + dr * i;
                int endCol = col + dc * i;
                if (endRow < 0 || endRow >= 8 || endCol < 0 || endCol >= 8) break;

                if (!piecePinned || pinDirection == (dr, dc) || pinDirection == (-dr, -dc))
                {
                    string endPiece = Board[endRow, endCol];
                    if (endPiece == "--")
                        moves.Add(new Move((row, col), (endRow, endCol), Board));
                    else if (endPiece[0] == enemyColor[0])
                    {
                        moves.Add(new Move((row, col), (endRow, endCol), Board));
                        break;
                    }
                    else
                        break;
                }
            }
        }
    }

    private void GetQueenMoves(int row, int col, List<Move> moves)
    {
        GetRookMoves(row, col, moves);
        GetBishopMoves(row, col, moves);
    }

    private void GetKingMoves(int row, int col, List<Move> moves)
    {
        var rowMoves = new[] { -1, -1, -1, 0, 0, 1, 1, 1 };
        var colMoves = new[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        string allyColor = WhiteToMove ? "w" : "b";

        for (int i = 0; i < 8; i++)
        {
            int endRow = row + rowMoves[i];
            int endCol = col + colMoves[i];
            if (endRow >= 0 && endRow < 8 && endCol >= 0 && endCol < 8)
            {
                string endPiece = Board[endRow, endCol];
                if (endPiece[0] != allyColor[0])
                {
                    (int, int) originalKingLocation = WhiteToMove ? whiteKingLocation : blackKingLocation;
                    if (WhiteToMove)
                        whiteKingLocation = (endRow, endCol);
                    else
                        blackKingLocation = (endRow, endCol);

                    (bool inCheck, _, _) = CheckForPinsAndChecks();
                    if (!inCheck)
                        moves.Add(new Move((row, col), (endRow, endCol), Board));

                    if (WhiteToMove)
                        whiteKingLocation = originalKingLocation;
                    else
                        blackKingLocation = originalKingLocation;
                }
            }
        }
    }

    private void GetCastleMoves(int row, int col, List<Move> moves)
    {
        if (SquareUnderAttack(row, col)) return;

        if ((WhiteToMove && CurrentCastleRights.WKS) || (!WhiteToMove && CurrentCastleRights.BKS))
            GetKingsideCastleMoves(row, col, moves);
        if ((WhiteToMove && CurrentCastleRights.WQS) || (!WhiteToMove && CurrentCastleRights.BQS))
            GetQueensideCastleMoves(row, col, moves);
    }

    private void GetKingsideCastleMoves(int row, int col, List<Move> moves)
    {
        if (Board[row, col + 1] == "--" && Board[row, col + 2] == "--")
        {
            if (!SquareUnderAttack(row, col + 1) && !SquareUnderAttack(row, col + 2))
                moves.Add(new Move((row, col), (row, col + 2), Board, isCastleMove: true));
        }
    }

    private void GetQueensideCastleMoves(int row, int col, List<Move> moves)
    {
        if (Board[row, col - 1] == "--" && Board[row, col - 2] == "--" && Board[row, col - 3] == "--")
        {
            if (!SquareUnderAttack(row, col - 1) && !SquareUnderAttack(row, col - 2))
                moves.Add(new Move((row, col), (row, col - 2), Board, isCastleMove: true));
        }
    }
}