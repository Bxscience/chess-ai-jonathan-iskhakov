using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Chess : ChessBase
{
    public Game ChessGame;
    private string selectedCoordinate;
    private List<GameObject> highlightedSquares = new List<GameObject>();

    public string user = "white";
    public int depth = 3;

    public TextMeshProUGUI aiDepthText;
    public TextMeshProUGUI playerColorText;
    public TextMeshProUGUI aiMoveText;
    public TextMeshProUGUI moveHistoryText;

    private void Start()
    {
        depth = PlayerPrefs.GetInt("AIDepth", 3);
        user = PlayerPrefs.GetString("UserSide", "white");

        ChessGame = new Game();

        aiDepthText.text = "AI Depth: " + depth;
        playerColorText.text = "You Are: " + user;

        if (user == "black")
        {
            AIMove();
        }
    }

    public override string CurrentTurn()
    {
        return ChessGame.CurrentTurn;
    }
    
    public override void SelectCoordinate(string coordinate)
    {
        ClearHighlights();
        selectedCoordinate = coordinate;
        HighlightValidMoves(coordinate);
    }

    public void DeselectCoordinate()
    {
        ClearHighlights();
        selectedCoordinate = null;
    }

    public override void TryMove(string targetCoordinate)
    {
        if (string.IsNullOrEmpty(selectedCoordinate)) return;

        string moveNotation = selectedCoordinate.ToLower() + targetCoordinate.ToLower();
        if (ChessGame.ValidateMove(moveNotation))
        {
            Move move = ChessGame.PlayMove(moveNotation);
            if (move.IsCastleMove)
            {
                if (moveNotation == "e1g1")
                {
                    MovePiece("e1", "g1");
                    MovePiece("h1", "f1");
                }
                else if (moveNotation == "e1c1")
                {
                    MovePiece("e1", "c1");
                    MovePiece("a1", "d1");
                }
                else if (moveNotation == "e8g8")
                {
                    MovePiece("e8", "g8");
                    MovePiece("h8", "f8");
                }
                else if (moveNotation == "e8c8")
                {
                    MovePiece("e8", "c8");
                    MovePiece("a8", "d8");
                }
            }
            else
            {
                MovePiece(selectedCoordinate, targetCoordinate);
            }

            moveHistoryText.text += "\n" + user.Substring(0, 1).ToUpper() + user.Substring(1) + ": " + move.ToString();

            if (ChessGame.CurrentTurn != user)
            {
                AIMove();
            }
        }
        DeselectCoordinate();
    }

    private void MovePiece(string fromCoord, string toCoord)
    {
        Transform fromSquare = transform.Find("Positions/" + fromCoord.ToLower());
        Transform toSquare = transform.Find("Positions/" + toCoord.ToLower());

        if (fromSquare != null && toSquare != null && fromSquare.childCount > 0)
        {
            Transform piece = null;
            for (int i = 0; i < fromSquare.childCount; i++)
            {
                if (fromSquare.GetChild(i).CompareTag("Piece"))
                {
                    piece = fromSquare.GetChild(i);
                    break;
                }
            }

            for (int i = 0; i < fromSquare.childCount; i++)
            {
                if (fromSquare.GetChild(i) != piece)
                {
                    Destroy(fromSquare.GetChild(i).gameObject);
                }
            }

            if (toSquare.childCount > 0)
            {
                for (int i = 0; i < toSquare.childCount; i++)
                {
                    Destroy(toSquare.GetChild(i).gameObject);
                }
            }

            piece.SetParent(toSquare);
            piece.localPosition = Vector3.zero;
        }
    }

    private void HighlightValidMoves(string coordinate)
    {
        List<Move> validMoves = ChessGame.GetPossibleMovesForPiece(coordinate);
        foreach (Move move in validMoves)
        {
            char file = (char)('a' + move.EndCol);
            int rank = 8 - move.EndRow;
            string targetCoord = file.ToString() + rank.ToString();

            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            highlight.transform.position = transform.Find("Positions/" + targetCoord).position + Vector3.up * 0.01f;
            highlight.transform.localScale = new Vector3(2.5f, 0.01f, 2.5f);
            highlight.GetComponent<Renderer>().material.color = new Color(0f, 1f, 0f, 0.5f);
            Destroy(highlight.GetComponent<Collider>());

            highlightedSquares.Add(highlight);
        }
    }

    private void ClearHighlights()
    {
        foreach (GameObject highlight in highlightedSquares)
        {
            Destroy(highlight);
        }
        highlightedSquares.Clear();
    }

    private void AIMove()
    {
        try
        {
            string aiMove = ChessGame.AIMove(depth);
            if (!string.IsNullOrEmpty(aiMove))
            {
                ChessGame.PlayMove(aiMove);
                
                MovePiece(aiMove.Substring(0, 2), aiMove.Substring(2, 2));
                aiMoveText.text = "AI Played: " + aiMove;
                
                moveHistoryText.text += "\n" + (user == "white" ? "Black" : "White") + ": " + aiMove;
                if (ChessGame.gameState.inCheck)
                {
                    aiMoveText.text += "\nCheck!";
                }
            }
            else
            {
                aiMoveText.text = "AI Played: ";
                Debug.Log("Null move");
            }
        }
        catch
        {
            if (ChessGame.gameState.Checkmate)
            {
                Debug.Log(user == "white" ? "Black lost." : "White lost.");
            }
        }
    }

    void Update()
    {
        if (ChessGame.gameState.Stalemate)
        {
            PlayerPrefs.SetString("ResultText", "Stalemate");
            PlayerPrefs.SetString("PGN", GeneratePGN());
            SceneManager.LoadScene("EndScreen");
        }
        else if (ChessGame.gameState.Checkmate)
        {
            string result;
            if (user == "white")
            {
                result = ChessGame.gameState.WhiteToMove ? "You Lost Against AI" : "You Won Against AI";
            }
            else
            {
                result = ChessGame.gameState.WhiteToMove ? "You Won Against AI" : "You Lost Against AI";
            }

            PlayerPrefs.SetString("ResultText", result);
            PlayerPrefs.SetString("PGN", GeneratePGN());
            SceneManager.LoadScene("EndScreen");
        }
        else if (ChessGame.gameState.inCheck)
        {
            if (ChessGame.gameState.WhiteToMove)
            {
                // Debug.Log("White is in check.");
            }
            else
            {
                // Debug.Log("Black is in check.");
            }
        }
        else
        {
            if (aiMoveText.text.Contains("\nCheck!"))
            {
                aiMoveText.text = aiMoveText.text.Replace("\nCheck!", "");
            }
        }
    }
    
    public string GeneratePGN()
    {
        List<Move> moveLog = ChessGame.gameState.MoveLog;
        string pgn = "";
        int turn = 1;

        for (int i = 0; i < moveLog.Count; i++)
        {
            if (i % 2 == 0)
            {
                pgn += turn + ". ";
            }

            pgn += moveLog[i].GetChessNotation() + " ";

            if (i % 2 == 1)
            {
                turn++;
            }
        }

        return pgn.Trim();
    }
    
    public void Resign()
    {
        PlayerPrefs.SetString("ResultText", "AI Won by Resignation (you lose)");
        PlayerPrefs.SetString("PGN", GeneratePGN());
        SceneManager.LoadScene("EndScreen");
    }
}
