using UnityEngine;
using UnityEngine.EventSystems;

public class PieceRaycastFilter :
    MonoBehaviour,
    ICanvasRaycastFilter
{
    [SerializeField]
    [Range(0.5f, 1f)]
    private float gapCoverage = 0.6f;


    private PieceView pieceView;
    private RectTransform rectTransform;


    private void Awake()
    {
        pieceView =
            GetComponent<PieceView>();

        rectTransform =
            GetComponent<RectTransform>();
    }


    public bool IsRaycastLocationValid(
        Vector2 screenPoint,
        Camera eventCamera
    )
    {
        if (
            pieceView == null ||
            pieceView.CurrentCells == null ||
            pieceView.CurrentCells.Length == 0
        )
        {
            return false;
        }


        bool success =
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    screenPoint,
                    eventCamera,
                    out Vector2 localPoint
                );


        if (!success)
        {
            return false;
        }


        Rect pieceRect =
            rectTransform.rect;


        float cellSize =
            pieceView.CellSize;

        float spacing =
            pieceView.Spacing;

        float step =
            cellSize +
            spacing;


        // Каждую клетку немного расширяем
        // в область промежутка.

        float padding =
            spacing *
            gapCoverage;


        foreach (
            Vector2Int cell
            in pieceView.CurrentCells
        )
        {
            float left =
                pieceRect.xMin +
                cell.x * step -
                padding;

            float right =
                pieceRect.xMin +
                cell.x * step +
                cellSize +
                padding;

            float top =
                pieceRect.yMax -
                cell.y * step +
                padding;

            float bottom =
                pieceRect.yMax -
                cell.y * step -
                cellSize -
                padding;


            bool inside =
                localPoint.x >= left &&
                localPoint.x <= right &&
                localPoint.y >= bottom &&
                localPoint.y <= top;


            if (inside)
            {
                return true;
            }
        }


        return false;
    }
}