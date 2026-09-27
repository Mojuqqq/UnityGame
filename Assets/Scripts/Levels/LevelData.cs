using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Level_001",
    menuName = "Puzzle/Level Data"
)]
public class LevelData : ScriptableObject
{
    [Header("Information")]

    [SerializeField]
    private int levelNumber = 1;

    [SerializeField]
    private string displayName = "УРОВЕНЬ 1";


    [Header("Grid Size")]

    [SerializeField]
    [Min(1)]
    private int columns = 5;

    [SerializeField]
    [Min(1)]
    private int rows = 5;


    [Header("Pieces")]

    [SerializeField]
    private PieceDefinition[] pieces;


    [Header("Hints")]

    [Tooltip(
        "Количество занятых клеток в каждой строке сверху вниз."
    )]
    [SerializeField]
    private int[] rowTargets;

    [Tooltip(
        "Количество занятых клеток в каждой колонке слева направо."
    )]
    [SerializeField]
    private int[] columnTargets;


    [Header("Blocked Cells")]

    [Tooltip(
        "Клетки, в которые нельзя ставить фигуры. " +
        "X — слева направо, Y — сверху вниз."
    )]
    [SerializeField]
    private Vector2Int[] blockedCells;


    public int LevelNumber =>
        levelNumber;

    public string DisplayName =>
        displayName;

    public int Columns =>
        columns;

    public int Rows =>
        rows;

    public PieceDefinition[] Pieces =>
        pieces;

    public int[] RowTargets =>
        rowTargets;

    public int[] ColumnTargets =>
        columnTargets;

    public Vector2Int[] BlockedCells =>
        blockedCells;


    public bool IsValid(
        out string errorMessage
    )
    {
        // -----------------------------------------
        // GRID SIZE
        // -----------------------------------------

        if (columns <= 0)
        {
            errorMessage =
                "Columns must be greater than 0.";

            return false;
        }


        if (rows <= 0)
        {
            errorMessage =
                "Rows must be greater than 0.";

            return false;
        }


        // -----------------------------------------
        // HINT ARRAYS
        // -----------------------------------------

        if (
            rowTargets == null ||
            rowTargets.Length != rows
        )
        {
            errorMessage =
                $"Row Targets must contain exactly {rows} values.";

            return false;
        }


        if (
            columnTargets == null ||
            columnTargets.Length != columns
        )
        {
            errorMessage =
                $"Column Targets must contain exactly {columns} values.";

            return false;
        }


        // -----------------------------------------
        // HINT VALUES
        // -----------------------------------------

        for (
            int y = 0;
            y < rowTargets.Length;
            y++
        )
        {
            if (
                rowTargets[y] < 0 ||
                rowTargets[y] > columns
            )
            {
                errorMessage =
                    $"Row {y} has impossible target: {rowTargets[y]}.";

                return false;
            }
        }


        for (
            int x = 0;
            x < columnTargets.Length;
            x++
        )
        {
            if (
                columnTargets[x] < 0 ||
                columnTargets[x] > rows
            )
            {
                errorMessage =
                    $"Column {x} has impossible target: {columnTargets[x]}.";

                return false;
            }
        }


        // -----------------------------------------
        // HINT SUM
        // -----------------------------------------

        int rowSum = 0;

        foreach (
            int value
            in rowTargets
        )
        {
            rowSum += value;
        }


        int columnSum = 0;

        foreach (
            int value
            in columnTargets
        )
        {
            columnSum += value;
        }


        if (rowSum != columnSum)
        {
            errorMessage =
                $"Hint totals do not match. " +
                $"Rows = {rowSum}, Columns = {columnSum}.";

            return false;
        }


        // -----------------------------------------
        // PIECES
        // -----------------------------------------

        if (
            pieces == null ||
            pieces.Length == 0
        )
        {
            errorMessage =
                "The level has no pieces.";

            return false;
        }


        int totalPieceCells = 0;


        foreach (
            PieceDefinition piece
            in pieces
        )
        {
            if (piece == null)
            {
                errorMessage =
                    "The Pieces list contains an empty element.";

                return false;
            }


            if (
                piece.Cells == null ||
                piece.Cells.Length == 0
            )
            {
                errorMessage =
                    $"Piece '{piece.name}' has no cells.";

                return false;
            }


            totalPieceCells +=
                piece.Cells.Length;
        }


        if (rowSum != totalPieceCells)
        {
            errorMessage =
                $"Hint total does not match piece cells. " +
                $"Hints = {rowSum}, " +
                $"Piece cells = {totalPieceCells}.";

            return false;
        }


        // -----------------------------------------
        // BLOCKED CELLS
        // -----------------------------------------

        int[] blockedPerRow =
            new int[rows];

        int[] blockedPerColumn =
            new int[columns];


        HashSet<Vector2Int>
            uniqueBlockedCells =
                new HashSet<Vector2Int>();


        if (blockedCells != null)
        {
            foreach (
                Vector2Int blockedCell
                in blockedCells
            )
            {
                if (
                    blockedCell.x < 0 ||
                    blockedCell.x >= columns ||
                    blockedCell.y < 0 ||
                    blockedCell.y >= rows
                )
                {
                    errorMessage =
                        $"Blocked cell " +
                        $"({blockedCell.x}, {blockedCell.y}) " +
                        "is outside the grid.";

                    return false;
                }


                if (
                    !uniqueBlockedCells.Add(
                        blockedCell
                    )
                )
                {
                    errorMessage =
                        $"Blocked cell " +
                        $"({blockedCell.x}, {blockedCell.y}) " +
                        "is duplicated.";

                    return false;
                }


                blockedPerRow[
                    blockedCell.y
                ]++;


                blockedPerColumn[
                    blockedCell.x
                ]++;
            }
        }


        // -----------------------------------------
        // CHECK AVAILABLE CELLS
        // -----------------------------------------

        for (
            int y = 0;
            y < rows;
            y++
        )
        {
            int availableCells =
                columns -
                blockedPerRow[y];


            if (
                rowTargets[y] >
                availableCells
            )
            {
                errorMessage =
                    $"Row {y} requires " +
                    $"{rowTargets[y]} occupied cells, " +
                    $"but only {availableCells} are available " +
                    "because of blocked cells.";

                return false;
            }
        }


        for (
            int x = 0;
            x < columns;
            x++
        )
        {
            int availableCells =
                rows -
                blockedPerColumn[x];


            if (
                columnTargets[x] >
                availableCells
            )
            {
                errorMessage =
                    $"Column {x} requires " +
                    $"{columnTargets[x]} occupied cells, " +
                    $"but only {availableCells} are available " +
                    "because of blocked cells.";

                return false;
            }
        }


        errorMessage = "";

        return true;
    }
}