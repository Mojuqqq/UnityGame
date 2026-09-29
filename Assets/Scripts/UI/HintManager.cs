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
    private float topHintHeight =
        110f;

    [SerializeField]
    private float rowHintWidth =
        130f;


    private LevelData currentLevel;


    private readonly List<HintGroupView>
        columnHintGroups =
            new List<HintGroupView>();


    private readonly List<HintGroupView>
        rowHintGroups =
            new List<HintGroupView>();


    // =====================================================
    // EVENTS
    // =====================================================

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


        Canvas.ForceUpdateCanvases();


        ConfigureContainers();

        BuildHintGroups();

        UpdateHints();
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
                "TopHintsContainer requires GridLayoutGroup."
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
                "RowHintsContainer requires GridLayoutGroup."
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
    // CREATE GROUPS
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
    // UPDATE
    // =====================================================

    public void UpdateHints()
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
                    x
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
                    y
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


        for (
            int x = 0;
            x < currentLevel.Columns;
            x++
        )
        {
            if (
                GetOccupiedCountInColumn(x) !=
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
                GetOccupiedCountInRow(y) !=
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
        int rowIndex
    )
    {
        int count =
            0;


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
        int count =
            0;


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