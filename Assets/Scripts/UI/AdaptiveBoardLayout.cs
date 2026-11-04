using UnityEngine;


[RequireComponent(
    typeof(RectTransform)
)]
public class AdaptiveBoardLayout :
    MonoBehaviour
{
    // =====================================================
    // ROW HINTS SIDE
    // =====================================================

    public enum RowHintsSide
    {
        Left,
        Right
    }


    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("Layout Area")]

    [Tooltip(
        "BoardRoot внутри BoardArea. " +
        "Если BoardRoot не используется, " +
        "можно оставить пустым."
    )]
    [SerializeField]
    private RectTransform layoutRoot;


    [Header("Board Containers")]

    [SerializeField]
    private RectTransform gridContainer;


    [SerializeField]
    private RectTransform topHintsContainer;


    [SerializeField]
    private RectTransform rowHintsContainer;


    [Header("Managers")]

    [SerializeField]
    private GridManager gridManager;


    [SerializeField]
    private HintManager hintManager;


    // =====================================================
    // GRID
    // =====================================================

    [Header("Grid")]

    [SerializeField]
    [Min(1f)]
    private float maxCellSize =
        200f;


    [SerializeField]
    [Min(1f)]
    private float minCellSize =
        20f;


    [SerializeField]
    [Min(0f)]
    private float cellSpacing =
        8f;


    // =====================================================
    // HINTS
    // =====================================================

    [Header("Hints")]

    [SerializeField]
    [Min(1f)]
    private float topHintHeight =
        90f;


    [SerializeField]
    [Min(1f)]
    private float rowHintWidth =
        90f;


    [SerializeField]
    [Min(0f)]
    private float hintGap =
        12f;


    [SerializeField]
    private RowHintsSide rowHintsSide =
        RowHintsSide.Right;


    // =====================================================
    // BOARD AREA
    // =====================================================

    [Header("Board Area Padding")]

    [SerializeField]
    [Min(0f)]
    private float horizontalPadding =
        20f;


    [SerializeField]
    [Min(0f)]
    private float verticalPadding =
        20f;


    [Header("Composition Position")]

    [Tooltip(
        "Дополнительный сдвиг всей композиции " +
        "Grid + Hints внутри BoardArea."
    )]
    [SerializeField]
    private Vector2 compositionOffset =
        Vector2.zero;


    // =====================================================
    // STATE
    // =====================================================

    private RectTransform boardRect;


    private int currentColumns =
        1;


    private int currentRows =
        1;


    private bool initialized;


    private bool layoutDirty;


    // =====================================================
    // PUBLIC
    // =====================================================

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


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        boardRect =
            GetComponent<RectTransform>();


        PrepareLayoutRoot();
    }


    private void LateUpdate()
    {
        if (
            !initialized ||
            !layoutDirty
        )
        {
            return;
        }


        layoutDirty =
            false;


        RebuildLayout();
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


        PrepareLayoutRoot();


        Canvas.ForceUpdateCanvases();


        RebuildLayout();
    }


    // =====================================================
    // RESPONSIVE
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


        // Не пересчитываем layout прямо внутри
        // OnRectTransformDimensionsChange,
        // чтобы не получить цепочку повторных вызовов.
        layoutDirty =
            true;
    }


    // =====================================================
    // LAYOUT ROOT
    // =====================================================

    private void PrepareLayoutRoot()
    {
        if (boardRect == null)
        {
            boardRect =
                GetComponent<RectTransform>();
        }


        // Если BoardRoot не используется,
        // сам BoardArea становится layout root.
        if (layoutRoot == null)
        {
            layoutRoot =
                boardRect;

            return;
        }


        if (layoutRoot == boardRect)
        {
            return;
        }


        // Если BoardRoot является прямым ребёнком
        // BoardArea — автоматически растягиваем
        // его на всю доступную область.
        if (
            layoutRoot.parent ==
            boardRect
        )
        {
            layoutRoot.anchorMin =
                Vector2.zero;


            layoutRoot.anchorMax =
                Vector2.one;


            layoutRoot.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );


            layoutRoot.offsetMin =
                Vector2.zero;


            layoutRoot.offsetMax =
                Vector2.zero;


            layoutRoot.localScale =
                Vector3.one;
        }
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


        PrepareLayoutRoot();


        if (
            layoutRoot == null ||
            gridContainer == null ||
            topHintsContainer == null ||
            rowHintsContainer == null
        )
        {
            Debug.LogError(
                "AdaptiveBoardLayout: " +
                "Layout Root, GridContainer, " +
                "TopHintsContainer или " +
                "RowHintsContainer не назначены."
            );

            return;
        }


        // Контейнеры должны жить
        // в одной системе координат.
        if (
            gridContainer.parent !=
                layoutRoot ||
            topHintsContainer.parent !=
                layoutRoot ||
            rowHintsContainer.parent !=
                layoutRoot
        )
        {
            Debug.LogError(
                "AdaptiveBoardLayout: " +
                "GridContainer, TopHintsContainer " +
                "и RowHintsContainer должны быть " +
                "прямыми детьми Layout Root."
            );

            return;
        }


        Canvas.ForceUpdateCanvases();


        float boardWidth =
            layoutRoot.rect.width;


        float boardHeight =
            layoutRoot.rect.height;


        if (
            boardWidth <= 1f ||
            boardHeight <= 1f
        )
        {
            return;
        }


        // =================================================
        // AVAILABLE SPACE
        //
        // В BoardArea должны поместиться:
        //
        //        TopHints
        //          gap
        // Grid + gap + RowHints
        //
        // HintButtons и PiecePanel здесь
        // вообще не учитываются.
        // =================================================

        float availableWidth =
            boardWidth
            -
            horizontalPadding * 2f;


        float availableHeight =
            boardHeight
            -
            verticalPadding * 2f;


        float availableGridWidth =
            availableWidth
            -
            rowHintWidth
            -
            hintGap;


        float availableGridHeight =
            availableHeight
            -
            topHintHeight
            -
            hintGap;


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
        // SPACING
        // =================================================

        float horizontalSpacingTotal =
            cellSpacing *
            Mathf.Max(
                0,
                currentColumns - 1
            );


        float verticalSpacingTotal =
            cellSpacing *
            Mathf.Max(
                0,
                currentRows - 1
            );


        // =================================================
        // CELL SIZE
        // =================================================

        float cellByWidth =
            (
                availableGridWidth
                -
                horizontalSpacingTotal
            )
            /
            currentColumns;


        float cellByHeight =
            (
                availableGridHeight
                -
                verticalSpacingTotal
            )
            /
            currentRows;


        float calculatedCellSize =
            Mathf.Min(
                maxCellSize,
                cellByWidth,
                cellByHeight
            );


        // Целое значение даёт более стабильную
        // UI-геометрию и меньше размытия.
        calculatedCellSize =
            Mathf.Floor(
                calculatedCellSize
            );


        CellSize =
            Mathf.Max(
                1f,
                calculatedCellSize
            );


        if (
            CellSize <
            minCellSize
        )
        {
            Debug.LogWarning(
                $"AdaptiveBoardLayout: " +
                $"CellSize получился " +
                $"{CellSize}px. " +
                $"BoardArea слишком маленький " +
                $"для сетки " +
                $"{currentColumns}x{currentRows}."
            );
        }


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
        // COMPLETE COMPOSITION
        // =================================================

        float compositionWidth =
            rowHintWidth
            +
            hintGap
            +
            gridWidth;


        float compositionHeight =
            topHintHeight
            +
            hintGap
            +
            gridHeight;


        float compositionLeft =
            -compositionWidth * 0.5f
            +
            compositionOffset.x;


        float compositionBottom =
            -compositionHeight * 0.5f
            +
            compositionOffset.y;


        // =================================================
        // GRID POSITION
        // =================================================

        float gridX;


        if (
            rowHintsSide ==
            RowHintsSide.Left
        )
        {
            gridX =
                compositionLeft
                +
                rowHintWidth
                +
                hintGap
                +
                gridWidth * 0.5f;
        }
        else
        {
            gridX =
                compositionLeft
                +
                gridWidth * 0.5f;
        }


        float gridY =
            compositionBottom
            +
            gridHeight * 0.5f;


        ConfigureRect(
            gridContainer,
            new Vector2(
                gridWidth,
                gridHeight
            ),
            new Vector2(
                gridX,
                gridY
            )
        );


        // =================================================
        // TOP HINTS
        // =================================================

        float topHintsY =
            compositionBottom
            +
            gridHeight
            +
            hintGap
            +
            topHintHeight * 0.5f;


        ConfigureRect(
            topHintsContainer,
            new Vector2(
                gridWidth,
                topHintHeight
            ),
            new Vector2(
                gridX,
                topHintsY
            )
        );


        // =================================================
        // ROW HINTS
        // =================================================

        float rowHintsX;


        if (
            rowHintsSide ==
            RowHintsSide.Left
        )
        {
            rowHintsX =
                compositionLeft
                +
                rowHintWidth * 0.5f;
        }
        else
        {
            rowHintsX =
                compositionLeft
                +
                gridWidth
                +
                hintGap
                +
                rowHintWidth * 0.5f;
        }


        ConfigureRect(
            rowHintsContainer,
            new Vector2(
                rowHintWidth,
                gridHeight
            ),
            new Vector2(
                rowHintsX,
                gridY
            )
        );


        // =================================================
        // GRID MANAGER
        // =================================================

        if (gridManager != null)
        {
            gridManager
                .SetLayoutMetrics(
                    CellSize,
                    cellSpacing
                );
        }


        Canvas.ForceUpdateCanvases();


        // =================================================
        // HINT MANAGER
        // =================================================

        if (hintManager != null)
        {
            hintManager
                .RefreshLayout();
        }
    }


    // =====================================================
    // RECT
    // =====================================================

    private void ConfigureRect(
        RectTransform target,
        Vector2 size,
        Vector2 position
    )
    {
        if (target == null)
        {
            return;
        }


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