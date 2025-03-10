using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public float moveDelay = 1.0f; // Artificial delay for AI "thinking"

    [SerializeField] private Player _aiPlayer;
     public Player AIPlayer => _aiPlayer;
    private bool isAITurn = false;
private bool IsAITurnValid()
{

    return _aiPlayer != null && GameManager.instance.currentPlayer == _aiPlayer;
}
    void Update()
    {
        if (isAITurn && !IsMoveInProgress())
        {
            StartCoroutine(MakeRandomMove());
        }
    }

    public void SetAIPlayer(Player player)
    {
        _aiPlayer = player;
    }

    private IEnumerator MakeRandomMove()
    {
        isAITurn = false;
        yield return new WaitForSeconds(moveDelay);

        // Get all possible moves
        List<AIMove> possibleMoves = GetAllPossibleMoves();
        
        if (possibleMoves.Count > 0)
        {
            // Select random move
            AIMove randomMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
            
            // Execute move
            ExecuteMove(randomMove);
        }

        GameManager.instance.NextPlayer();
    }

    private List<AIMove> GetAllPossibleMoves()
    {
        List<AIMove> moves = new List<AIMove>();

        foreach (GameObject piece in _aiPlayer.pieces)
        {
            Vector2Int currentPosition = GameManager.instance.GridForPiece(piece);
            List<Vector2Int> validMoves = GameManager.instance.MovesForPiece(piece);

            foreach (Vector2Int move in validMoves)
            {
                moves.Add(new AIMove {
                    piece = piece,
                    start = currentPosition,
                    end = move
                });
            }
        }
        return moves;
    }

    private void ExecuteMove(AIMove move)
    {
        GameObject targetPiece = GameManager.instance.PieceAtGrid(move.end);
        
        if (targetPiece != null)
        {
            GameManager.instance.CapturePieceAt(move.end);
        }
        
        GameManager.instance.Move(move.piece, move.end);
    }

    private bool IsMoveInProgress()
    {
        // Add any move animation checks here if needed
        return false;
    }

    public void StartAITurn()
    {
        isAITurn = true;
    }
}

public struct AIMove
{
    public GameObject piece;
    public Vector2Int start;
    public Vector2Int end;
}