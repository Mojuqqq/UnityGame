using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Prefab")]

    [SerializeField]
    private PieceView pieceViewPrefab;


    [Header("Containers")]

    [SerializeField]
    private RectTransform piecesContainer;

    [SerializeField]
    private RectTransform piecePanel;

    [SerializeField]
    private RectTransform dragLayer;


    [Header("Gameplay")]

    [SerializeField]
    private GridManager gridManager;


    [Header("Adaptive Piece Size")]

    [SerializeField]
    [Min(1f)]
    private float minPanelCellSize = 30f;

    [SerializeField]
    [Min(1f)]
    private float maxPanelCellSize = 70f;


    [Tooltip(
        "Максимальный размер клетки фигуры " +
        "относительно клетки игрового поля."
    )]
    [SerializeField]
    [Range(0.1f, 1f)]
    private float gridCellRatio = 0.38f;


    [Tooltip(
        "Отступ между клетками внутри фигуры " +
        "относительно размера клетки."
    )]
    [SerializeField]
    [Range(0f, 0.3f)]
    private float internalSpacingRatio = 0.10f;


    [Header("Piece Spacing")]

    [SerializeField]
    [Min(0f)]
    private float horizontalPieceSpacing = 36f;

    [SerializeField]
    [Min(0f)]
    private float verticalPieceSpacing = 30f;


    private readonly List<PieceView>
        spawnedPieces =
            new List<PieceView>();


    // =====================================================
    // INTERNAL ROW
    // =====================================================

    private class PieceRow
    {
        public readonly List<PieceView>
            pieces =
                new List<PieceView>();

        public float width;

        public float height;
    }


    // =====================================================
    // BUILD
    // =====================================================

    public void BuildPieces(
        PieceDefinition[] definitions
    )
    {
        ClearPieces();


        if (
            definitions == null ||
            definitions.Length == 0
        )
        {
            Debug.LogError(
                "PieceSpawner: No pieces supplied."
            );

            return;
        }


        if (pieceViewPrefab == null)
        {
            Debug.LogError(
                "PieceSpawner: PieceView Prefab is not assigned."
            );

            return;
        }


        if (piecesContainer == null)
        {
            Debug.LogError(
                "PieceSpawner: PiecesContainer is not assigned."
            );

            return;
        }


        foreach (
            PieceDefinition definition
            in definitions
        )
        {
            if (definition == null)
            {
                continue;
            }


            PieceView piece =
                Instantiate(
                    pieceViewPrefab,
                    piecesContainer
                );


            piece.name =
                $"Piece_{spawnedPieces.Count}_{definition.PieceId}";


            piece.Initialize(
                definition
            );


            PieceDragHandler dragHandler =
                piece.GetComponent<
                    PieceDragHandler
                >();


            if (dragHandler != null)
            {
                dragHandler.Configure(
                    gridManager,
                    dragLayer,
                    piecePanel
                );
            }
            else
            {
                Debug.LogError(
                    $"{piece.name} has no PieceDragHandler."
                );
            }


            spawnedPieces.Add(
                piece
            );
        }


        Canvas.ForceUpdateCanvases();


        RefreshLayout();
    }


    // =====================================================
    // PUBLIC REFRESH
    // =====================================================

    public void RefreshLayout()
    {
        if (
            spawnedPieces.Count == 0 ||
            piecesContainer == null
        )
        {
            return;
        }


        Canvas.ForceUpdateCanvases();


        float availableWidth =
            piecesContainer.rect.width;


        float availableHeight =
            piecesContainer.rect.height;


        if (
            availableWidth <= 0f ||
            availableHeight <= 0f
        )
        {
            return;
        }


        float effectiveMaxCellSize =
            maxPanelCellSize;


        // Связываем размер фигур в панели
        // с размером клеток игрового Grid.
        if (
            gridManager != null &&
            gridManager.CellSize > 0f
        )
        {
            effectiveMaxCellSize =
                Mathf.Min(
                    maxPanelCellSize,

                    gridManager.CellSize *
                    gridCellRatio
                );
        }


        effectiveMaxCellSize =
            Mathf.Max(
                minPanelCellSize,
                effectiveMaxCellSize
            );


        float selectedCellSize =
            FindBestCellSize(
                availableWidth,
                availableHeight,
                effectiveMaxCellSize
            );


        float selectedInternalSpacing =
            selectedCellSize *
            internalSpacingRatio;


        // =================================================
        // APPLY SIZE TO PIECES
        // =================================================

        foreach (
            PieceView piece
            in spawnedPieces
        )
        {
            piece.SetLayout(
                selectedCellSize,
                selectedInternalSpacing
            );
        }


        Canvas.ForceUpdateCanvases();


        // =================================================
        // BUILD FINAL ROWS
        // =================================================

        List<PieceRow> rows =
            BuildRows(
                availableWidth,
                selectedCellSize,
                selectedInternalSpacing
            );


        LayoutRows(
            rows
        );
    }


    // =====================================================
    // FIND BEST CELL SIZE
    // =====================================================

    private float FindBestCellSize(
        float availableWidth,
        float availableHeight,
        float maxCellSize
    )
    {
        float candidate =
            maxCellSize;


        while (
            candidate >=
            minPanelCellSize
        )
        {
            float internalSpacing =
                candidate *
                internalSpacingRatio;


            List<PieceRow> rows =
                BuildRows(
                    availableWidth,
                    candidate,
                    internalSpacing
                );


            float totalHeight =
                GetRowsTotalHeight(
                    rows
                );


            if (
                totalHeight <=
                availableHeight
            )
            {
                return candidate;
            }


            candidate -=
                1f;
        }


        Debug.LogWarning(
            "PieceSpawner: " +
            "pieces do not fully fit into PiecePanel. " +
            "Using minimum cell size."
        );


        return
            minPanelCellSize;
    }


    // =====================================================
    // BUILD ROWS
    // =====================================================

    private List<PieceRow> BuildRows(
        float availableWidth,
        float cellSize,
        float internalSpacing
    )
    {
        List<PieceRow> rows =
            new List<PieceRow>();


        PieceRow currentRow =
            new PieceRow();


        foreach (
            PieceView piece
            in spawnedPieces
        )
        {
            Vector2 pieceSize =
                CalculatePieceSize(
                    piece,
                    cellSize,
                    internalSpacing
                );


            float requiredWidth =
                currentRow.pieces.Count == 0
                    ? pieceSize.x
                    : currentRow.width +
                      horizontalPieceSpacing +
                      pieceSize.x;


            if (
                currentRow.pieces.Count > 0 &&
                requiredWidth >
                availableWidth
            )
            {
                rows.Add(
                    currentRow
                );


                currentRow =
                    new PieceRow();
            }


            if (
                currentRow.pieces.Count > 0
            )
            {
                currentRow.width +=
                    horizontalPieceSpacing;
            }


            currentRow.pieces.Add(
                piece
            );


            currentRow.width +=
                pieceSize.x;


            currentRow.height =
                Mathf.Max(
                    currentRow.height,
                    pieceSize.y
                );
        }


        if (
            currentRow.pieces.Count > 0
        )
        {
            rows.Add(
                currentRow
            );
        }


        return rows;
    }


    // =====================================================
    // PIECE SIZE
    // =====================================================

    private Vector2 CalculatePieceSize(
        PieceView piece,
        float cellSize,
        float internalSpacing
    )
    {
        if (
            piece == null ||
            piece.CurrentCells == null ||
            piece.CurrentCells.Length == 0
        )
        {
            return Vector2.zero;
        }


        int maxX =
            0;

        int maxY =
            0;


        foreach (
            Vector2Int cell
            in piece.CurrentCells
        )
        {
            maxX =
                Mathf.Max(
                    maxX,
                    cell.x
                );


            maxY =
                Mathf.Max(
                    maxY,
                    cell.y
                );
        }


        int widthInCells =
            maxX + 1;


        int heightInCells =
            maxY + 1;


        float width =
            widthInCells *
            cellSize
            +
            (widthInCells - 1) *
            internalSpacing;


        float height =
            heightInCells *
            cellSize
            +
            (heightInCells - 1) *
            internalSpacing;


        return
            new Vector2(
                width,
                height
            );
    }


    // =====================================================
    // ROW HEIGHT
    // =====================================================

    private float GetRowsTotalHeight(
        List<PieceRow> rows
    )
    {
        float height =
            0f;


        for (
            int i = 0;
            i < rows.Count;
            i++
        )
        {
            height +=
                rows[i].height;


            if (
                i <
                rows.Count - 1
            )
            {
                height +=
                    verticalPieceSpacing;
            }
        }


        return height;
    }


    // =====================================================
    // FINAL POSITIONING
    // =====================================================

    private void LayoutRows(
        List<PieceRow> rows
    )
    {
        if (
            rows == null ||
            rows.Count == 0
        )
        {
            return;
        }


        float totalHeight =
            GetRowsTotalHeight(
                rows
            );


        // Верх центрированного блока.
        float currentY =
            totalHeight *
            0.5f;


        foreach (
            PieceRow row
            in rows
        )
        {
            float currentX =
                -row.width *
                0.5f;


            float rowCenterY =
                currentY -
                row.height *
                0.5f;


            foreach (
                PieceView piece
                in row.pieces
            )
            {
                RectTransform rect =
                    piece.GetComponent<
                        RectTransform
                    >();


                rect.anchorMin =
                    new Vector2(
                        0.5f,
                        0.5f
                    );


                rect.anchorMax =
                    new Vector2(
                        0.5f,
                        0.5f
                    );


                rect.pivot =
                    new Vector2(
                        0.5f,
                        0.5f
                    );


                float x =
                    currentX +
                    rect.rect.width *
                    0.5f;


                rect.anchoredPosition =
                    new Vector2(
                        x,
                        rowCenterY
                    );


                currentX +=
                    rect.rect.width +
                    horizontalPieceSpacing;
            }


            currentY -=
                row.height +
                verticalPieceSpacing;
        }
    }


    // =====================================================
    // CLEAR
    // =====================================================

    public void ClearPieces()
    {
        spawnedPieces.Clear();


        if (piecesContainer == null)
        {
            return;
        }


        for (
            int i =
                piecesContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                piecesContainer
                    .GetChild(i)
                    .gameObject;


            child.SetActive(
                false
            );


            Destroy(
                child
            );
        }
    }
}