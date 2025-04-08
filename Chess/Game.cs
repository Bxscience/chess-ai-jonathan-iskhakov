using System.Collections.Generic;

public class Game
{
    public GameState gameState = new GameState();
    
    public string CurrentTurn => gameState.WhiteToMove ? "white" : "black";
    
    public bool ValidateMove(string moveNotation)
    {
        if (moveNotation.Length < 4) return false;
        int startCol = moveNotation[0] - 'a';
        int startRow = 8 - (moveNotation[1] - '0');
        int endCol = moveNotation[2] - 'a';
        int endRow = 8 - (moveNotation[3] - '0');
        return gameState.GetValidMoves().Exists(m => 
            m.StartRow == startRow && m.StartCol == startCol &&
            m.EndRow == endRow && m.EndCol == endCol);
    }

    public Move PlayMove(string moveNotation)
    {
        int startCol = moveNotation[0] - 'a';
        int startRow = 8 - (moveNotation[1] - '0');
        int endCol = moveNotation[2] - 'a';
        int endRow = 8 - (moveNotation[3] - '0');
        
        Move move = gameState.GetValidMoves().Find(m => 
            m.StartRow == startRow && m.StartCol == startCol &&
            m.EndRow == endRow && m.EndCol == endCol);
        
        if (move != null) gameState.MakeMove(move);

        return move;
    }

    public string AIMove(int depth = 3)
    {
        List<Move> validMoves = gameState.GetValidMoves();
        Move aiMove = AI.FindBestMove(gameState, validMoves, depth);
        gameState.MakeMove(aiMove);
        return $"{GetPosition(aiMove.StartRow, aiMove.StartCol)}{GetPosition(aiMove.EndRow, aiMove.EndCol)}";
    }

    public List<Move> GetPossibleMovesForPiece(string coordinate)
    {
        int col = coordinate[0] - 'a';
        int row = 8 - int.Parse(coordinate[1].ToString());
        return gameState.GetValidMoves().FindAll(m => 
            m.StartRow == row && m.StartCol == col);
    }

    private string GetPosition(int row, int col)
    {
        return $"{(char)('a' + col)}{8 - row}";
    }
}