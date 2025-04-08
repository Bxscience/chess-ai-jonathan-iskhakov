using UnityEngine;

public abstract class ChessBase : MonoBehaviour
{
    public abstract string CurrentTurn();
    public abstract void SelectCoordinate(string coordinate);
    public abstract void TryMove(string targetCoordinate);
}