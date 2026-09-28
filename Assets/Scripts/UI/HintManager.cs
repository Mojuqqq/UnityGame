using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private RectTransform
        topHintsContainer;

    [SerializeField]
    private RectTransform
        rowHintsContainer;

    [SerializeField]
    private HintGroupView
        hintGroupPrefab;


    [Header("Layout")]

    [SerializeField]
    private float topHintHeight =
        84f;

    [SerializeField]
    private float rowHintWidth =
        124f;


    private LevelData currentLevel;

    private readonly List<HintGroupView>
        columnHintGroups =
            new List<HintGroupView>();

    private readonly List<HintGroupView>
        rowHintGroups =
            new List<HintGroupView>();


    private void OnEnable()
    {
        if (gridManager != null)
        {
            gridManager.GridChanged +=
                UpdateHints;
        }
    }


    private void OnDisable()
    {
        if (gridManager != null)
        {
            gridManager.GridChanged -=
                UpdateHints;
        }
    }


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


        if (
            topHintsContainer == null ||
            rowHintsContainer == null
        )
        {
            Debug.LogError(
                "HintManager: hint containers are not assigned."
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


        ConfigureContainers();
        BuildHintGroups();
        UpdateHints();
    }


    public void UpdateHints()
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
            int occupiedCount =
                GetOccupiedCountInColumn(
                    x
                );

            columnHintGroups[x]
                .UpdateProgress(
                    occupiedCount
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
            int occupiedCount =
                GetOccupiedCountInRow(
                    y
                );

            rowHintGroups[y]
                .UpdateProgress(
                    occupiedCount
                );
        }
    }


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
            int occupiedCount =
                GetOccupiedCountInColumn(
                    x
                );

            if (
                occupiedCount !=
                currentLevel
                    .ColumnTargets[x]
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
            int occupiedCount =
                GetOccupiedCountInRow(
                    y
                );

            if (
                occupiedCount !=
                currentLevel
                    .RowTargets[y]
            )
            {
                return false;
            }
        }


        return true;
    }


    private void ConfigureContainers()
    {
        ConfigureTopHintsContainer();
        ConfigureRowHintsContainer();
    }


    private void ConfigureTopHintsContainer()
    {
        GridLayoutGroup layout =
            topHintsContainer
                .GetComponent<
                    GridLayoutGroup
                >();

        if (layout == null)
        {
            Debug.LogWarning(
                "HintManager: TopHintsContainer does not have GridLayoutGroup."
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


    private void ConfigureRowHintsContainer()
    {
        GridLayoutGroup layout =
            rowHintsContainer
                .GetComponent<
                    GridLayoutGroup
                >();

        if (layout == null)
        {
            Debug.LogWarning(
                "HintManager: RowHintsContainer does not have GridLayoutGroup."
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


    private void BuildHintGroups()
    {
        ClearHintGroups();

        // -----------------------------
        // Column hints
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
                currentLevel
                    .ColumnTargets[x],
                HintGroupVisualType.Column
            );

            columnHintGroups.Add(
                group
            );
        }


        // -----------------------------
        // Row hints
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
                currentLevel
                    .RowTargets[y],
                HintGroupVisualType.Row
            );

            rowHintGroups.Add(
                group
            );
        }
    }


    private int GetOccupiedCountInRow(
        int rowIndex
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

            if (
                cell != null &&
                cell.IsOccupied()
            )
            {
                count++;
            }
        }

        return count;
    }


    private int GetOccupiedCountInColumn(
        int columnIndex
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

            if (
                cell != null &&
                cell.IsOccupied()
            )
            {
                count++;
            }
        }

        return count;
    }


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

            child.SetActive(false);
            Destroy(child);
        }
    }
}