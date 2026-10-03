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


    private LevelData currentLevel;


    private readonly List<HintGroupView>
        columnHintGroups =
            new List<HintGroupView>();


    private readonly List<HintGroupView>
        rowHintGroups =
            new List<HintGroupView>();


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


        if (
            gridManager == null ||
            topHintsContainer == null ||
            rowHintsContainer == null ||
            hintGroupPrefab == null
        )
        {
            Debug.LogError(
                "HintManager: references are missing."
            );

            return;
        }


        previewCells.Clear();

        previewActive =
            false;


        Canvas.ForceUpdateCanvases();


        BuildHintGroups();

        RefreshLayout();

        UpdateHints();
    }


    // =====================================================
    // RESPONSIVE REFRESH
    // =====================================================

    public void RefreshLayout()
    {
        if (
            currentLevel == null ||
            gridManager == null
        )
        {
            return;
        }


        ConfigureTopHintsLayout();

        ConfigureRowHintsLayout();


        Canvas.ForceUpdateCanvases();
    }


    private void ConfigureTopHintsLayout()
    {
        GridLayoutGroup layout =
            topHintsContainer
                .GetComponent<GridLayoutGroup>();


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


        // Ширина одной hint-ячейки
        // теперь ТОЧНО такая же, как GridCell.
        layout.cellSize =
            new Vector2(
                gridManager.CellSize,
                topHintsContainer.rect.height
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
                .GetComponent<GridLayoutGroup>();


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
                rowHintsContainer.rect.width,
                gridManager.CellSize
            );


        layout.spacing =
            new Vector2(
                0f,
                gridManager.Spacing
            );
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
    // BUILD
    // =====================================================

    private void BuildHintGroups()
    {
        ClearHintGroups();


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
    // REAL UPDATE
    // =====================================================

    public void UpdateHints()
    {
        previewCells.Clear();

        previewActive =
            false;


        UpdateHintVisuals(
            false
        );
    }


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
    // WIN
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


        for (
            int x = 0;
            x < currentLevel.Columns;
            x++
        )
        {
            if (
                GetOccupiedCountInColumn(
                    x,
                    false
                )
                !=
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
            if (
                GetOccupiedCountInRow(
                    y,
                    false
                )
                !=
                currentLevel.RowTargets[y]
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // COUNTS
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


            bool isPreview =
                includePreview &&
                previewActive &&
                previewCells.Contains(
                    new Vector2Int(
                        x,
                        rowIndex
                    )
                );


            if (
                occupied ||
                isPreview
            )
            {
                count++;
            }
        }


        return count;
    }


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


            bool isPreview =
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
                isPreview
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