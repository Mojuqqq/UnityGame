using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private RectTransform topHintsContainer;

    [SerializeField]
    private RectTransform rowHintsContainer;

    [SerializeField]
    private HintGroupView hintGroupPrefab;


    [Header("Layout")]

    [SerializeField]
    private float topHintHeight = 110f;

    [SerializeField]
    private float rowHintWidth = 130f;


    private LevelData currentLevel;


    private readonly List<HintGroupView>
        columnHintGroups =
            new List<HintGroupView>();


    private readonly List<HintGroupView>
        rowHintGroups =
            new List<HintGroupView>();


    // Координаты клеток,
    // которые сейчас занимает ghost-preview.
    private readonly HashSet<Vector2Int>
        previewCells =
            new HashSet<Vector2Int>();


    private bool previewActive;


    // =====================================================
    // EVENTS
    // =====================================================

    private void OnEnable()
    {
        if (gridManager == null)
        {
            return;
        }


        gridManager.GridChanged +=
            UpdateHints;


        gridManager.PlacementPreviewChanged +=
            OnPlacementPreviewChanged;
    }


    private void OnDisable()
    {
        if (gridManager == null)
        {
            return;
        }


        gridManager.GridChanged -=
            UpdateHints;


        gridManager.PlacementPreviewChanged -=
            OnPlacementPreviewChanged;
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        LevelData levelData
    )
    {
        currentLevel =
            levelData;


        if (currentLevel == null)
        {
            Debug.LogError(
                "HintManager: LevelData is null."
            );

            return;
        }


        if (gridManager == null)
        {
            Debug.LogError(
                "HintManager: GridManager is not assigned."
            );

            return;
        }


        if (topHintsContainer == null)
        {
            Debug.LogError(
                "HintManager: TopHintsContainer is not assigned."
            );

            return;
        }


        if (rowHintsContainer == null)
        {
            Debug.LogError(
                "HintManager: RowHintsContainer is not assigned."
            );

            return;
        }


        if (hintGroupPrefab == null)
        {
            Debug.LogError(
                "HintManager: HintGroup prefab is not assigned."
            );

            return;
        }


        previewCells.Clear();

        previewActive =
            false;


        Canvas.ForceUpdateCanvases();


        ConfigureContainers();

        BuildHintGroups();

        UpdateHints();
    }


    // =====================================================
    // GHOST PREVIEW
    // =====================================================

    private void OnPlacementPreviewChanged(
        IReadOnlyList<Vector2Int> coordinates,
        bool placementValid
    )
    {
        previewCells.Clear();


        // Если курсор ушёл с Grid
        // и ghost-клеток больше нет —
        // возвращаем реальные показатели.
        if (
            coordinates == null ||
            coordinates.Count == 0
        )
        {
            previewActive =
                false;


            UpdateHintVisuals(
                false
            );


            return;
        }


        // ВАЖНО:
        // placementValid здесь намеренно НЕ проверяем.
        //
        // Нам нужно показывать будущую заполненность
        // даже если фигуру поставить нельзя:
        //
        // - пересечение с другой фигурой;
        // - Blocked Cell;
        // - часть фигуры выходит за Grid.
        //
        // GridManager передаёт нам все части ghost,
        // которые находятся внутри Grid.

        foreach (
            Vector2Int coordinate
            in coordinates
        )
        {
            previewCells.Add(
                coordinate
            );
        }


        previewActive =
            previewCells.Count > 0;


        UpdateHintVisuals(
            true
        );
    }


    // =====================================================
    // CONTAINER LAYOUT
    // =====================================================

    private void ConfigureContainers()
    {
        RectTransform gridRect =
            gridManager.GetComponent<
                RectTransform
            >();


        if (gridRect == null)
        {
            Debug.LogError(
                "HintManager: GridManager has no RectTransform."
            );

            return;
        }


        topHintsContainer.sizeDelta =
            new Vector2(
                gridRect.rect.width,
                topHintHeight
            );


        rowHintsContainer.sizeDelta =
            new Vector2(
                rowHintWidth,
                gridRect.rect.height
            );


        ConfigureTopHintsLayout();

        ConfigureRowHintsLayout();
    }


    private void ConfigureTopHintsLayout()
    {
        GridLayoutGroup layout =
            topHintsContainer
                .GetComponent<
                    GridLayoutGroup
                >();


        if (layout == null)
        {
            Debug.LogError(
                "HintManager: TopHintsContainer requires GridLayoutGroup."
            );

            return;
        }


        layout.constraint =
            GridLayoutGroup
                .Constraint
                .FixedColumnCount;


        layout.constraintCount =
            currentLevel.Columns;


        layout.startCorner =
            GridLayoutGroup
                .Corner
                .UpperLeft;


        layout.startAxis =
            GridLayoutGroup
                .Axis
                .Horizontal;


        layout.childAlignment =
            TextAnchor.MiddleCenter;


        layout.cellSize =
            new Vector2(
                gridManager.CellSize,
                topHintHeight
            );


        layout.spacing =
            new Vector2(
                gridManager.Spacing,
                0f
            );
    }


    private void ConfigureRowHintsLayout()
    {
        GridLayoutGroup layout =
            rowHintsContainer
                .GetComponent<
                    GridLayoutGroup
                >();


        if (layout == null)
        {
            Debug.LogError(
                "HintManager: RowHintsContainer requires GridLayoutGroup."
            );

            return;
        }


        layout.constraint =
            GridLayoutGroup
                .Constraint
                .FixedColumnCount;


        layout.constraintCount =
            1;


        layout.startCorner =
            GridLayoutGroup
                .Corner
                .UpperLeft;


        layout.startAxis =
            GridLayoutGroup
                .Axis
                .Horizontal;


        layout.childAlignment =
            TextAnchor.MiddleCenter;


        layout.cellSize =
            new Vector2(
                rowHintWidth,
                gridManager.CellSize
            );


        layout.spacing =
            new Vector2(
                0f,
                gridManager.Spacing
            );
    }


    // =====================================================
    // BUILD HINT GROUPS
    // =====================================================

    private void BuildHintGroups()
    {
        ClearHintGroups();


        // -----------------------------
        // Top / Column hints
        // -----------------------------

        for (
            int x = 0;
            x < currentLevel.Columns;
            x++
        )
        {
            HintGroupView group =
                Instantiate(
                    hintGroupPrefab,
                    topHintsContainer
                );


            group.name =
                $"ColumnHint_{x}";


            group.Initialize(
                currentLevel.ColumnTargets[x],
                HintGroupVisualType.Column
            );


            columnHintGroups.Add(
                group
            );
        }


        // -----------------------------
        // Right / Row hints
        // -----------------------------

        for (
            int y = 0;
            y < currentLevel.Rows;
            y++
        )
        {
            HintGroupView group =
                Instantiate(
                    hintGroupPrefab,
                    rowHintsContainer
                );


            group.name =
                $"RowHint_{y}";


            group.Initialize(
                currentLevel.RowTargets[y],
                HintGroupVisualType.Row
            );


            rowHintGroups.Add(
                group
            );
        }
    }


    // =====================================================
    // REAL GRID UPDATE
    // =====================================================

    public void UpdateHints()
    {
        // Когда реальное поле изменилось,
        // сначала считаем его без старого preview.
        previewCells.Clear();

        previewActive =
            false;


        UpdateHintVisuals(
            false
        );
    }


    // =====================================================
    // VISUAL UPDATE
    // =====================================================

    private void UpdateHintVisuals(
        bool includePreview
    )
    {
        if (
            currentLevel == null ||
            gridManager == null
        )
        {
            return;
        }


        // -----------------------------
        // Columns
        // -----------------------------

        for (
            int x = 0;
            x < currentLevel.Columns &&
            x < columnHintGroups.Count;
            x++
        )
        {
            int occupied =
                GetOccupiedCountInColumn(
                    x,
                    includePreview
                );


            columnHintGroups[x]
                .UpdateProgress(
                    occupied
                );
        }


        // -----------------------------
        // Rows
        // -----------------------------

        for (
            int y = 0;
            y < currentLevel.Rows &&
            y < rowHintGroups.Count;
            y++
        )
        {
            int occupied =
                GetOccupiedCountInRow(
                    y,
                    includePreview
                );


            rowHintGroups[y]
                .UpdateProgress(
                    occupied
                );
        }
    }


    // =====================================================
    // WIN CHECK
    // =====================================================

    public bool IsSolved()
    {
        if (
            currentLevel == null ||
            gridManager == null
        )
        {
            return false;
        }


        // ВАЖНО:
        // здесь preview НЕ учитываем.
        // Победа возможна только после реального drop.

        for (
            int x = 0;
            x < currentLevel.Columns;
            x++
        )
        {
            int occupied =
                GetOccupiedCountInColumn(
                    x,
                    false
                );


            if (
                occupied !=
                currentLevel.ColumnTargets[x]
            )
            {
                return false;
            }
        }


        for (
            int y = 0;
            y < currentLevel.Rows;
            y++
        )
        {
            int occupied =
                GetOccupiedCountInRow(
                    y,
                    false
                );


            if (
                occupied !=
                currentLevel.RowTargets[y]
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // ROW COUNT
    // =====================================================

    private int GetOccupiedCountInRow(
        int rowIndex,
        bool includePreview
    )
    {
        int count = 0;


        for (
            int x = 0;
            x < currentLevel.Columns;
            x++
        )
        {
            GridCell cell =
                gridManager.GetCell(
                    x,
                    rowIndex
                );


            bool occupied =
                cell != null &&
                cell.IsOccupied();


            bool isPreviewCell =
                includePreview &&
                previewActive &&
                previewCells.Contains(
                    new Vector2Int(
                        x,
                        rowIndex
                    )
                );


            // occupied || preview,
            // поэтому пересечение с уже занятой клеткой
            // не считается дважды.
            if (
                occupied ||
                isPreviewCell
            )
            {
                count++;
            }
        }


        return count;
    }


    // =====================================================
    // COLUMN COUNT
    // =====================================================

    private int GetOccupiedCountInColumn(
        int columnIndex,
        bool includePreview
    )
    {
        int count = 0;


        for (
            int y = 0;
            y < currentLevel.Rows;
            y++
        )
        {
            GridCell cell =
                gridManager.GetCell(
                    columnIndex,
                    y
                );


            bool occupied =
                cell != null &&
                cell.IsOccupied();


            bool isPreviewCell =
                includePreview &&
                previewActive &&
                previewCells.Contains(
                    new Vector2Int(
                        columnIndex,
                        y
                    )
                );


            if (
                occupied ||
                isPreviewCell
            )
            {
                count++;
            }
        }


        return count;
    }


    // =====================================================
    // CLEAR
    // =====================================================

    private void ClearHintGroups()
    {
        columnHintGroups.Clear();

        rowHintGroups.Clear();


        ClearChildren(
            topHintsContainer
        );


        ClearChildren(
            rowHintsContainer
        );
    }


    private void ClearChildren(
        RectTransform container
    )
    {
        if (container == null)
        {
            return;
        }


        for (
            int i =
                container.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                container
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