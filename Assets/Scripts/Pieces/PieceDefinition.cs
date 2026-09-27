using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPiece",
    menuName = "Puzzle/Piece Definition"
)]
public class PieceDefinition : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string pieceId;
    [SerializeField] private string displayName;

    [Header("Appearance")]
    [SerializeField] private Color color = Color.white;

    [Header("Shape")]
    [SerializeField] private Vector2Int[] cells;

    public string PieceId => pieceId;
    public string DisplayName => displayName;
    public Color Color => color;
    public Vector2Int[] Cells => cells;
}