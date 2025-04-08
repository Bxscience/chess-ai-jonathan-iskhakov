using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    private GameObject highlightCircle;
    private float highlightSize = 2.75f;
    private float highlightHeight = 0.01f;
    private ChessBase chess;

    private void Start()
    {
        chess = transform.parent.parent.GetComponent<ChessBase>();
    }

    private void OnMouseEnter()
    {
        if (HasPiece() && IsCurrentPlayerPiece())
        {
            highlightCircle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlightCircle.transform.position = transform.position + Vector3.up * highlightHeight;
            highlightCircle.transform.localScale = new Vector3(highlightSize, highlightHeight, highlightSize);
            highlightCircle.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.16f, 0.5f);
            Destroy(highlightCircle.GetComponent<Collider>());
        }
    }

    private void OnMouseExit()
    {
        if (highlightCircle != null)
        {
            Destroy(highlightCircle);
        }
    }

    private void OnMouseDown()
    {
        if (chess == null) return;

        if (HasPiece() && IsCurrentPlayerPiece())
        {
            chess.SelectCoordinate(gameObject.name.ToLower());
        }
        else
        {
            chess.TryMove(gameObject.name.ToLower());
        }
    }

    private bool HasPiece()
    {
        return transform.childCount > 0;
    }

    private bool IsCurrentPlayerPiece()
    {
        if (!HasPiece()) return false;
        string pieceColor = transform.GetChild(0).name.ToLower().StartsWith("white") ? "white" : "black";
        return pieceColor == chess.CurrentTurn();
    }
}