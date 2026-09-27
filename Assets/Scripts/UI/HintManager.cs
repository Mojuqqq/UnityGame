using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private RectTransform
        columnHintsContainer;

    [SerializeField]
    private RectTransform
        rowHintsContainer;

    [SerializeField]
    private TMP_Text hintPrefab;


    [Header("Colors")]

    [SerializeField]
    private Color incompleteColor =
        Color.white;

    [SerializeField]
    private Color completeColor =
        new Color(
            0.3f,
            0.9f,
            0.4f,
            1f
        );

    [SerializeField]
    private Color overflowColor =
        new Color(
            1f,
            0.25f,
            0.25f,
            1f
        );


    private int[] rowTargets;

    private int[] columnTargets;


    private readonly List<TMP_Text>
        rowHintTexts =
            new List<TMP_Text>();

    private readonly List<TMP_Text>
        columnHintTexts =
            new List<TMP_Text>();


    private bool initialized;


    private void OnEnable()
    {
        if (gridManager != null)
        {
            gridManager.GridChanged +=
                RefreshHints;
        }
    }


    private void OnDisable()
    {
        if (gridManager != null)
        {
            gridManager.GridChanged -=
                RefreshHints;
        }
    }


    public void Initialize(
        LevelData levelData
    )
    {
        if (levelData == null)
        {
            Debug.LogError(
                "HintManager: LevelData is null."
            );

            return;
        }


        rowTargets =
            (int[])
            levelData
                .RowTargets
                .Clone();


        columnTargets =
            (int[])
            levelData
                .ColumnTargets
                .Clone();


        ConfigureLayoutSpacing();

        BuildHints();

        initialized =
            true;

        RefreshHints();
    }


    private void ConfigureLayoutSpacing()
    {
        HorizontalLayoutGroup
            columnLayout =
                columnHintsContainer
                    .GetComponent<
                        HorizontalLayoutGroup
                    >();


        if (columnLayout != null)
        {
            columnLayout.spacing =
                gridManager.Spacing;
        }


        VerticalLayoutGroup
            rowLayout =
                rowHintsContainer
                    .GetComponent<
                        VerticalLayoutGroup
                    >();


        if (rowLayout != null)
        {
            rowLayout.spacing =
                gridManager.Spacing;
        }
    }


    private void BuildHints()
    {
        ClearContainer(
            columnHintsContainer
        );

        ClearContainer(
            rowHintsContainer
        );


        rowHintTexts.Clear();

        columnHintTexts.Clear();


        if (
            rowTargets == null ||
            rowTargets.Length !=
            gridManager.Rows
        )
        {
            Debug.LogError(
                "HintManager: Row Targets count does not match Grid rows."
            );

            return;
        }


        if (
            columnTargets == null ||
            columnTargets.Length !=
            gridManager.Columns
        )
        {
            Debug.LogError(
                "HintManager: Column Targets count does not match Grid columns."
            );

            return;
        }


        for (
            int x = 0;
            x < gridManager.Columns;
            x++
        )
        {
            TMP_Text hint =
                Instantiate(
                    hintPrefab,
                    columnHintsContainer
                );


            hint.name =
                $"ColumnHint_{x}";


            hint.text =
                columnTargets[x]
                    .ToString();


            columnHintTexts.Add(
                hint
            );
        }


        for (
            int y = 0;
            y < gridManager.Rows;
            y++
        )
        {
            TMP_Text hint =
                Instantiate(
                    hintPrefab,
                    rowHintsContainer
                );


            hint.name =
                $"RowHint_{y}";


            hint.text =
                rowTargets[y]
                    .ToString();


            rowHintTexts.Add(
                hint
            );
        }
    }


    private void ClearContainer(
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


    public void RefreshHints()
    {
        if (!initialized)
        {
            return;
        }


        RefreshRowHints();

        RefreshColumnHints();
    }


    private void RefreshRowHints()
    {
        for (
            int y = 0;
            y < gridManager.Rows;
            y++
        )
        {
            int occupiedCount =
                0;


            for (
                int x = 0;
                x < gridManager.Columns;
                x++
            )
            {
                GridCell cell =
                    gridManager
                        .GetCell(
                            x,
                            y
                        );


                if (
                    cell != null &&
                    cell.State ==
                    GridCellState.Occupied
                )
                {
                    occupiedCount++;
                }
            }


            ApplyHintState(
                rowHintTexts[y],
                occupiedCount,
                rowTargets[y]
            );
        }
    }


    private void RefreshColumnHints()
    {
        for (
            int x = 0;
            x < gridManager.Columns;
            x++
        )
        {
            int occupiedCount =
                0;


            for (
                int y = 0;
                y < gridManager.Rows;
                y++
            )
            {
                GridCell cell =
                    gridManager
                        .GetCell(
                            x,
                            y
                        );


                if (
                    cell != null &&
                    cell.State ==
                    GridCellState.Occupied
                )
                {
                    occupiedCount++;
                }
            }


            ApplyHintState(
                columnHintTexts[x],
                occupiedCount,
                columnTargets[x]
            );
        }
    }


    private void ApplyHintState(
        TMP_Text hintText,
        int current,
        int target
    )
    {
        hintText.text =
            target.ToString();


        if (current > target)
        {
            hintText.color =
                overflowColor;

            return;
        }


        if (current == target)
        {
            hintText.color =
                completeColor;

            return;
        }


        hintText.color =
            incompleteColor;
    }


    public bool IsSolved()
    {
        if (!initialized)
        {
            return false;
        }


        for (
            int y = 0;
            y < gridManager.Rows;
            y++
        )
        {
            int occupiedCount =
                0;


            for (
                int x = 0;
                x < gridManager.Columns;
                x++
            )
            {
                GridCell cell =
                    gridManager
                        .GetCell(
                            x,
                            y
                        );


                if (
                    cell != null &&
                    cell.State ==
                    GridCellState.Occupied
                )
                {
                    occupiedCount++;
                }
            }


            if (
                occupiedCount !=
                rowTargets[y]
            )
            {
                return false;
            }
        }


        for (
            int x = 0;
            x < gridManager.Columns;
            x++
        )
        {
            int occupiedCount =
                0;


            for (
                int y = 0;
                y < gridManager.Rows;
                y++
            )
            {
                GridCell cell =
                    gridManager
                        .GetCell(
                            x,
                            y
                        );


                if (
                    cell != null &&
                    cell.State ==
                    GridCellState.Occupied
                )
                {
                    occupiedCount++;
                }
            }


            if (
                occupiedCount !=
                columnTargets[x]
            )
            {
                return false;
            }
        }


        return true;
    }
}