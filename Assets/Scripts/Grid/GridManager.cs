using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]

    [SerializeField]
    private int columns = 5;

    [SerializeField]
    private int rows = 5;


    [Header("Cell")]

    [SerializeField]
    private GridCell cellPrefab;


    [Header("Fallback Layout")]

    [SerializeField]
    private float spacing = 8f;


    [Header("Preview Colors")]

    [SerializeField]
    private Color validPreviewColor =
        new Color(
            0.2f,
            0.9f,
            0.3f,
            0.75f
        );


    [SerializeField]
    private Color invalidPreviewColor =
        new Color(
            0.95f,
            0.2f,
            0.2f,
            0.75f
        );


    [Header("Ghost Preview")]

    [SerializeField]
    [Range(0f, 1f)]
    private float validGhostAlpha =
        0.42f;


    [SerializeField]
    [Range(0f, 1f)]
    private float invalidGhostAlpha =
        0.55f;


    private GridCell[,] cells;

    private GridLayoutGroup gridLayout;

    private RectTransform gridRect;


    private readonly List<GridCell>
        previewCells =
            new List<GridCell>();


    private readonly List<Vector2Int>
        previewCoordinates =
            new List<Vector2Int>();


    private bool initializedFromLevelData;

    private bool hasExternalLayoutMetrics;


    public int Columns =>
        columns;

    public int Rows =>
        rows;


    public float CellSize
    {
        get;
        private set;
    }


    public float Spacing =>
        spacing;


    public event Action GridChanged;


    public event Action<
        IReadOnlyList<Vector2Int>,
        bool
    > PlacementPreviewChanged;


    private void Start()
    {
        if (!initializedFromLevelData)
        {
            CreateGrid();
        }
    }


    // =====================================================
    // ADAPTIVE LAYOUT
    // =====================================================

    public void SetLayoutMetrics(
        float newCellSize,
        float newSpacing
    )
    {
        CellSize =
            Mathf.Max(
                1f,
                newCellSize
            );


        spacing =
            Mathf.Max(
                0f,
                newSpacing
            );


        hasExternalLayoutMetrics =
            true;


        if (gridLayout == null)
        {
            gridLayout =
                GetComponent<GridLayoutGroup>();
        }


        ApplyGridLayoutSettings();


        Canvas.ForceUpdateCanvases();
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        int newColumns,
        int newRows
    )
    {
        Initialize(
            newColumns,
            newRows,
            null
        );
    }


    public void Initialize(
        int newColumns,
        int newRows,
        Vector2Int[] blockedCells
    )
    {
        columns =
            Mathf.Max(
                1,
                newColumns
            );


        rows =
            Mathf.Max(
                1,
                newRows
            );


        initializedFromLevelData =
            true;


        CreateGrid();


        ApplyBlockedCells(
            blockedCells
        );
    }


    private void ApplyBlockedCells(
        Vector2Int[] blockedCells
    )
    {
        if (
            blockedCells == null ||
            blockedCells.Length == 0
        )
        {
            return;
        }


        foreach (
            Vector2Int coordinate
            in blockedCells
        )
        {
            GridCell cell =
                GetCell(
                    coordinate.x,
                    coordinate.y
                );


            if (cell == null)
            {
                Debug.LogWarning(
                    $"GridManager: Blocked cell " +
                    $"({coordinate.x}, {coordinate.y}) " +
                    "does not exist."
                );

                continue;
            }


            cell.SetBlocked();
        }
    }


    // =====================================================
    // CREATE GRID
    // =====================================================

    public void CreateGrid()
    {
        if (cellPrefab == null)
        {
            Debug.LogError(
                "GridManager: Cell Prefab is not assigned."
            );

            return;
        }


        gridLayout =
            GetComponent<GridLayoutGroup>();


        gridRect =
            GetComponent<RectTransform>();


        ClearGrid();


        ConfigureLayout();


        cells =
            new GridCell[
                columns,
                rows
            ];


        for (
            int y = 0;
            y < rows;
            y++
        )
        {
            for (
                int x = 0;
                x < columns;
                x++
            )
            {
                GridCell newCell =
                    Instantiate(
                        cellPrefab,
                        transform
                    );


                newCell.Initialize(
                    x,
                    y
                );


                cells[x, y] =
                    newCell;
            }
        }


        Canvas.ForceUpdateCanvases();
    }


    private void ConfigureLayout()
    {
        if (
            gridLayout == null ||
            gridRect == null
        )
        {
            return;
        }


        // Если AdaptiveBoardLayout уже передал
        // реальный размер клетки —
        // НЕ вычисляем его повторно.
        if (!hasExternalLayoutMetrics)
        {
            float containerWidth =
                gridRect.rect.width;


            float containerHeight =
                gridRect.rect.height;


            float horizontalSpacingTotal =
                spacing *
                (columns - 1);


            float verticalSpacingTotal =
                spacing *
                (rows - 1);


            float availableWidth =
                containerWidth -
                horizontalSpacingTotal;


            float availableHeight =
                containerHeight -
                verticalSpacingTotal;


            float cellWidth =
                availableWidth /
                columns;


            float cellHeight =
                availableHeight /
                rows;


            CellSize =
                Mathf.Min(
                    cellWidth,
                    cellHeight
                );
        }


        ApplyGridLayoutSettings();
    }


    private void ApplyGridLayoutSettings()
    {
        if (gridLayout == null)
        {
            return;
        }


        gridLayout.constraint =
            GridLayoutGroup
                .Constraint
                .FixedColumnCount;


        gridLayout.constraintCount =
            columns;


        gridLayout.cellSize =
            new Vector2(
                CellSize,
                CellSize
            );


        gridLayout.spacing =
            new Vector2(
                spacing,
                spacing
            );


        // Теперь контейнер и сетка имеют
        // одинаковый фактический размер.
        gridLayout.childAlignment =
            TextAnchor.UpperLeft;


        gridLayout.startCorner =
            GridLayoutGroup
                .Corner
                .UpperLeft;


        gridLayout.startAxis =
            GridLayoutGroup
                .Axis
                .Horizontal;
    }


    private void ClearGrid()
    {
        ClearPlacementPreview();


        cells =
            null;


        for (
            int i =
                transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                transform
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


    // =====================================================
    // CELL ACCESS
    // =====================================================

    public GridCell GetCell(
        int x,
        int y
    )
    {
        if (cells == null)
        {
            return null;
        }


        if (
            x < 0 ||
            x >= columns ||
            y < 0 ||
            y >= rows
        )
        {
            return null;
        }


        return cells[x, y];
    }


    public bool IsInsideGrid(
        int x,
        int y
    )
    {
        return
            x >= 0 &&
            x < columns &&
            y >= 0 &&
            y < rows;
    }


    public bool IsCellEmpty(
        int x,
        int y
    )
    {
        GridCell cell =
            GetCell(
                x,
                y
            );


        return
            cell != null &&
            cell.IsEmpty();
    }


    // =====================================================
    // POINTER
    // =====================================================

    public bool TryGetCellUnderPointer(
        Vector2 screenPosition,
        Camera eventCamera,
        out GridCell closestCell
    )
    {
        closestCell =
            null;


        if (
            gridRect == null ||
            cells == null
        )
        {
            return false;
        }


        if (
            !RectTransformUtility
                .RectangleContainsScreenPoint(
                    gridRect,
                    screenPosition,
                    eventCamera
                )
        )
        {
            return false;
        }


        float closestDistance =
            float.MaxValue;


        for (
            int y = 0;
            y < rows;
            y++
        )
        {
            for (
                int x = 0;
                x < columns;
                x++
            )
            {
                GridCell cell =
                    cells[x, y];


                if (cell == null)
                {
                    continue;
                }


                RectTransform cellRect =
                    cell.GetComponent<
                        RectTransform
                    >();


                Vector3 worldCenter =
                    cellRect.TransformPoint(
                        cellRect.rect.center
                    );


                Vector2 screenCenter =
                    RectTransformUtility
                        .WorldToScreenPoint(
                            eventCamera,
                            worldCenter
                        );


                float distance =
                    Vector2.SqrMagnitude(
                        screenPosition -
                        screenCenter
                    );


                if (
                    distance <
                    closestDistance
                )
                {
                    closestDistance =
                        distance;

                    closestCell =
                        cell;
                }
            }
        }


        return
            closestCell != null;
    }


    // =====================================================
    // PREVIEW
    // =====================================================

    public bool ShowPlacementPreview(
        Vector2Int[] pieceCells,
        Vector2Int origin
    )
    {
        ClearPreviewVisuals();


        bool valid =
            GetPlacementCells(
                pieceCells,
                origin,
                out List<GridCell> targetCells
            );


        Color previewColor =
            valid
                ? validPreviewColor
                : invalidPreviewColor;


        foreach (
            GridCell cell
            in targetCells
        )
        {
            if (cell == null)
            {
                continue;
            }


            cell.ShowPreview(
                previewColor
            );


            previewCells.Add(
                cell
            );
        }


        UpdatePreviewCoordinates(
            pieceCells,
            origin,
            valid
        );


        return valid;
    }


    public bool ShowPlacementPreview(
        Vector2Int[] pieceCells,
        Vector2Int origin,
        Color pieceColor
    )
    {
        ClearPreviewVisuals();


        bool valid =
            GetPlacementCells(
                pieceCells,
                origin,
                out List<GridCell> targetCells
            );


        Color previewColor;


        if (valid)
        {
            previewColor =
                pieceColor;


            previewColor.a =
                validGhostAlpha;
        }
        else
        {
            previewColor =
                invalidPreviewColor;


            previewColor.a =
                invalidGhostAlpha;
        }


        foreach (
            GridCell cell
            in targetCells
        )
        {
            if (cell == null)
            {
                continue;
            }


            cell.ShowPreview(
                previewColor
            );


            previewCells.Add(
                cell
            );
        }


        UpdatePreviewCoordinates(
            pieceCells,
            origin,
            valid
        );


        return valid;
    }


    private void UpdatePreviewCoordinates(
        Vector2Int[] pieceCells,
        Vector2Int origin,
        bool valid
    )
    {
        previewCoordinates.Clear();


        if (
            pieceCells != null &&
            pieceCells.Length > 0
        )
        {
            foreach (
                Vector2Int pieceCell
                in pieceCells
            )
            {
                Vector2Int coordinate =
                    new Vector2Int(
                        origin.x +
                        pieceCell.x,

                        origin.y +
                        pieceCell.y
                    );


                if (
                    !IsInsideGrid(
                        coordinate.x,
                        coordinate.y
                    )
                )
                {
                    continue;
                }


                if (
                    !previewCoordinates.Contains(
                        coordinate
                    )
                )
                {
                    previewCoordinates.Add(
                        coordinate
                    );
                }
            }
        }


        PlacementPreviewChanged?.Invoke(
            previewCoordinates,
            valid
        );
    }


    private void ClearPreviewVisuals()
    {
        foreach (
            GridCell cell
            in previewCells
        )
        {
            if (cell != null)
            {
                cell.ClearPreview();
            }
        }


        previewCells.Clear();
    }


    public void ClearPlacementPreview()
    {
        ClearPreviewVisuals();


        previewCoordinates.Clear();


        PlacementPreviewChanged?.Invoke(
            previewCoordinates,
            false
        );
    }


    // =====================================================
    // PLACEMENT
    // =====================================================

    public bool CanPlacePiece(
        Vector2Int[] pieceCells,
        Vector2Int origin
    )
    {
        return
            GetPlacementCells(
                pieceCells,
                origin,
                out _
            );
    }


    public bool PlacePiece(
        Vector2Int[] pieceCells,
        Color pieceColor,
        Vector2Int origin,
        out List<Vector2Int> placedCoordinates
    )
    {
        placedCoordinates =
            new List<Vector2Int>();


        bool valid =
            GetPlacementCells(
                pieceCells,
                origin,
                out List<GridCell> targetCells
            );


        if (!valid)
        {
            return false;
        }


        foreach (
            GridCell cell
            in targetCells
        )
        {
            cell.SetOccupied(
                pieceColor
            );


            placedCoordinates.Add(
                new Vector2Int(
                    cell.X,
                    cell.Y
                )
            );
        }


        ClearPlacementPreview();


        GridChanged?.Invoke();


        return true;
    }


    public bool PlacePiece(
        Vector2Int[] pieceCells,
        Color pieceColor,
        Vector2Int origin
    )
    {
        return
            PlacePiece(
                pieceCells,
                pieceColor,
                origin,
                out _
            );
    }


    public void ClearCells(
        List<Vector2Int> coordinates
    )
    {
        if (
            coordinates == null ||
            coordinates.Count == 0
        )
        {
            return;
        }


        foreach (
            Vector2Int coordinate
            in coordinates
        )
        {
            GridCell cell =
                GetCell(
                    coordinate.x,
                    coordinate.y
                );


            if (
                cell == null ||
                cell.IsBlocked()
            )
            {
                continue;
            }


            cell.SetEmpty();
        }


        ClearPlacementPreview();


        GridChanged?.Invoke();
    }


    // =====================================================
    // VISUAL POSITION
    // =====================================================

    public bool TryGetCellTopLeftWorld(
        int x,
        int y,
        out Vector3 worldPosition
    )
    {
        worldPosition =
            Vector3.zero;


        GridCell cell =
            GetCell(
                x,
                y
            );


        if (cell == null)
        {
            return false;
        }


        RectTransform cellRect =
            cell.GetComponent<
                RectTransform
            >();


        if (cellRect == null)
        {
            return false;
        }


        Vector3 localTopLeft =
            new Vector3(
                cellRect.rect.xMin,
                cellRect.rect.yMax,
                0f
            );


        worldPosition =
            cellRect.TransformPoint(
                localTopLeft
            );


        return true;
    }


    // =====================================================
    // INTERNAL
    // =====================================================

    private bool GetPlacementCells(
        Vector2Int[] pieceCells,
        Vector2Int origin,
        out List<GridCell> targetCells
    )
    {
        targetCells =
            new List<GridCell>();


        if (
            pieceCells == null ||
            pieceCells.Length == 0
        )
        {
            return false;
        }


        bool valid =
            true;


        foreach (
            Vector2Int pieceCell
            in pieceCells
        )
        {
            int x =
                origin.x +
                pieceCell.x;


            int y =
                origin.y +
                pieceCell.y;


            if (
                !IsInsideGrid(
                    x,
                    y
                )
            )
            {
                valid =
                    false;

                continue;
            }


            GridCell gridCell =
                GetCell(
                    x,
                    y
                );


            if (gridCell == null)
            {
                valid =
                    false;

                continue;
            }


            targetCells.Add(
                gridCell
            );


            if (!gridCell.IsEmpty())
            {
                valid =
                    false;
            }
        }


        return valid;
    }
}