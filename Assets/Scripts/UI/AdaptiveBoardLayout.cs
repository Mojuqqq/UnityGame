using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AdaptiveBoardLayout : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RectTransform gridContainer;

    [SerializeField]
    private RectTransform topHintsContainer;

    [SerializeField]
    private RectTransform rowHintsContainer;

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private HintManager hintManager;


    [Header("Grid")]

    [SerializeField]
    [Min(1f)]
    private float maxCellSize = 200f;

    [SerializeField]
    [Min(0f)]
    private float cellSpacing = 8f;


    [Header("Hints")]

    [SerializeField]
    [Min(1f)]
    private float topHintHeight = 110f;

    [SerializeField]
    [Min(1f)]
    private float rowHintWidth = 130f;

    [SerializeField]
    [Min(0f)]
    private float hintGap = 12f;


    [Header("Board Padding")]

    [SerializeField]
    [Min(0f)]
    private float horizontalPadding = 20f;

    [SerializeField]
    [Min(0f)]
    private float verticalPadding = 20f;


    private RectTransform boardRect;

    private int currentColumns;
    private int currentRows;

    private bool initialized;


    public float CellSize
    {
        get;
        private set;
    }

    public float CellSpacing =>
        cellSpacing;

    public float TopHintHeight =>
        topHintHeight;

    public float RowHintWidth =>
        rowHintWidth;


    private void Awake()
    {
        boardRect =
            GetComponent<RectTransform>();
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void ApplyLayout(
        int columns,
        int rows
    )
    {
        currentColumns =
            Mathf.Max(
                1,
                columns
            );

        currentRows =
            Mathf.Max(
                1,
                rows
            );


        initialized =
            true;


        Canvas.ForceUpdateCanvases();


        RebuildLayout();
    }


    // =====================================================
    // RESPONSIVE UPDATE
    // =====================================================

    private void OnRectTransformDimensionsChange()
    {
        if (
            !Application.isPlaying ||
            !initialized
        )
        {
            return;
        }


        RebuildLayout();
    }


    // =====================================================
    // BUILD
    // =====================================================

    private void RebuildLayout()
    {
        if (boardRect == null)
        {
            boardRect =
                GetComponent<RectTransform>();
        }


        if (
            gridContainer == null ||
            topHintsContainer == null ||
            rowHintsContainer == null
        )
        {
            Debug.LogError(
                "AdaptiveBoardLayout: " +
                "Grid/Hint containers are not assigned."
            );

            return;
        }


        float boardWidth =
            boardRect.rect.width;


        float boardHeight =
            boardRect.rect.height;


        if (
            boardWidth <= 0f ||
            boardHeight <= 0f
        )
        {
            return;
        }


        // =================================================
        // AVAILABLE GRID AREA
        //
        // Из общей BoardArea вычитаем:
        //
        // справа:
        // RowHints + gap
        //
        // сверху:
        // TopHints + gap
        // =================================================

        float availableGridWidth =
            boardWidth
            - horizontalPadding * 2f
            - rowHintWidth
            - hintGap;


        float availableGridHeight =
            boardHeight
            - verticalPadding * 2f
            - topHintHeight
            - hintGap;


        availableGridWidth =
            Mathf.Max(
                1f,
                availableGridWidth
            );


        availableGridHeight =
            Mathf.Max(
                1f,
                availableGridHeight
            );


        // =================================================
        // CELL SIZE
        // =================================================

        float horizontalSpacingTotal =
            cellSpacing *
            (currentColumns - 1);


        float verticalSpacingTotal =
            cellSpacing *
            (currentRows - 1);


        float cellByWidth =
            (
                availableGridWidth -
                horizontalSpacingTotal
            )
            /
            currentColumns;


        float cellByHeight =
            (
                availableGridHeight -
                verticalSpacingTotal
            )
            /
            currentRows;


        CellSize =
            Mathf.Max(
                1f,
                Mathf.Min(
                    maxCellSize,
                    cellByWidth,
                    cellByHeight
                )
            );


        // =================================================
        // EXACT GRID SIZE
        // =================================================

        float gridWidth =
            currentColumns *
            CellSize
            +
            horizontalSpacingTotal;


        float gridHeight =
            currentRows *
            CellSize
            +
            verticalSpacingTotal;


        // =================================================
        // FULL COMPOSITION SIZE
        //
        //        TopHints
        //
        //        Grid | RowHints
        //
        // Центрируем ВСЮ композицию,
        // а не только Grid.
        // =================================================

        float compositionWidth =
            gridWidth
            +
            hintGap
            +
            rowHintWidth;


        float compositionHeight =
            topHintHeight
            +
            hintGap
            +
            gridHeight;


        float left =
            -compositionWidth * 0.5f;


        float bottom =
            -compositionHeight * 0.5f;


        // =================================================
        // GRID
        // =================================================

        Vector2 gridPosition =
            new Vector2(
                left +
                gridWidth * 0.5f,

                bottom +
                gridHeight * 0.5f
            );


        ConfigureRect(
            gridContainer,
            new Vector2(
                gridWidth,
                gridHeight
            ),
            gridPosition
        );


        // =================================================
        // TOP HINTS
        //
        // Ровно над фактическим Grid.
        // =================================================

        Vector2 topHintsPosition =
            new Vector2(
                gridPosition.x,

                bottom
                +
                gridHeight
                +
                hintGap
                +
                topHintHeight * 0.5f
            );


        ConfigureRect(
            topHintsContainer,
            new Vector2(
                gridWidth,
                topHintHeight
            ),
            topHintsPosition
        );


        // =================================================
        // ROW HINTS
        //
        // Ровно справа от фактического Grid.
        // =================================================

        Vector2 rowHintsPosition =
            new Vector2(
                left
                +
                gridWidth
                +
                hintGap
                +
                rowHintWidth * 0.5f,

                gridPosition.y
            );


        ConfigureRect(
            rowHintsContainer,
            new Vector2(
                rowHintWidth,
                gridHeight
            ),
            rowHintsPosition
        );


        // =================================================
        // INFORM OTHER SYSTEMS
        // =================================================

        if (gridManager != null)
        {
            gridManager.SetLayoutMetrics(
                CellSize,
                cellSpacing
            );
        }


        Canvas.ForceUpdateCanvases();


        if (hintManager != null)
        {
            hintManager.RefreshLayout();
        }
    }


    private void ConfigureRect(
        RectTransform target,
        Vector2 size,
        Vector2 position
    )
    {
        target.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        target.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        target.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        target.sizeDelta =
            size;


        target.anchoredPosition =
            position;


        target.localScale =
            Vector3.one;
    }
}