using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class LevelSolutionEntry
{
    [SerializeField]
    private int pieceIndex;


    [SerializeField]
    private Vector2Int origin;


    [SerializeField]
    [Range(0, 3)]
    private int rotationSteps;


    public int PieceIndex =>
        pieceIndex;


    public Vector2Int Origin =>
        origin;


    public int RotationSteps =>
        rotationSteps;
}


[CreateAssetMenu(
    fileName = "Level_001",
    menuName = "Puzzle/Level Data"
)]
public class LevelData :
    ScriptableObject
{
    [Header("Information")]

    [SerializeField]
    private int levelNumber = 1;

    [SerializeField]
    private string displayName =
        "УРОВЕНЬ 1";


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
        "Количество занятых клеток " +
        "в каждой строке сверху вниз."
    )]
    [SerializeField]
    private int[] rowTargets;


    [Tooltip(
        "Количество занятых клеток " +
        "в каждой колонке слева направо."
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


    [Header("Solution")]

    [Tooltip(
        "Эталонное решение уровня. " +
        "Каждая запись соответствует одной фигуре " +
        "из массива Pieces."
    )]
    [SerializeField]
    private LevelSolutionEntry[] solution;


    // =====================================================
    // PROPERTIES
    // =====================================================

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


    public LevelSolutionEntry[] Solution =>
        solution;


    // =====================================================
    // SOLUTION ACCESS
    // =====================================================

    public LevelSolutionEntry
        GetSolutionEntryByPieceIndex(
            int pieceIndex
        )
    {
        if (solution == null)
        {
            return null;
        }


        foreach (
            LevelSolutionEntry entry
            in solution
        )
        {
            if (
                entry != null &&
                entry.PieceIndex ==
                pieceIndex
            )
            {
                return entry;
            }
        }


        return null;
    }


    public Vector2Int[] GetSolutionCells(
        LevelSolutionEntry entry
    )
    {
        if (entry == null)
        {
            return new Vector2Int[0];
        }


        Vector2Int[] rotatedCells =
            GetRotatedPieceCells(
                entry.PieceIndex,
                entry.RotationSteps
            );


        Vector2Int[] result =
            new Vector2Int[
                rotatedCells.Length
            ];


        for (
            int i = 0;
            i < rotatedCells.Length;
            i++
        )
        {
            result[i] =
                entry.Origin +
                rotatedCells[i];
        }


        return result;
    }


    public Vector2Int[] GetRotatedPieceCells(
        int pieceIndex,
        int rotationSteps
    )
    {
        if (
            pieces == null ||
            pieceIndex < 0 ||
            pieceIndex >= pieces.Length ||
            pieces[pieceIndex] == null ||
            pieces[pieceIndex].Cells == null
        )
        {
            return new Vector2Int[0];
        }


        Vector2Int[] source =
            pieces[pieceIndex].Cells;


        Vector2Int[] result =
            new Vector2Int[source.Length];


        int normalizedSteps =
            (
                (rotationSteps % 4)
                +
                4
            )
            %
            4;


        for (
            int i = 0;
            i < source.Length;
            i++
        )
        {
            Vector2Int rotated =
                source[i];


            for (
                int step = 0;
                step < normalizedSteps;
                step++
            )
            {
                // Та же формула,
                // которую использует PieceView.
                rotated =
                    new Vector2Int(
                        -rotated.y,
                        rotated.x
                    );
            }


            result[i] =
                rotated;
        }


        NormalizeCells(
            result
        );


        return result;
    }


    private void NormalizeCells(
        Vector2Int[] cells
    )
    {
        if (
            cells == null ||
            cells.Length == 0
        )
        {
            return;
        }


        int minX =
            cells[0].x;

        int minY =
            cells[0].y;


        foreach (
            Vector2Int cell
            in cells
        )
        {
            minX =
                Mathf.Min(
                    minX,
                    cell.x
                );


            minY =
                Mathf.Min(
                    minY,
                    cell.y
                );
        }


        for (
            int i = 0;
            i < cells.Length;
            i++
        )
        {
            cells[i] =
                new Vector2Int(
                    cells[i].x -
                    minX,

                    cells[i].y -
                    minY
                );
        }
    }


    // =====================================================
    // VALIDATION
    // =====================================================

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
                $"Row Targets must contain " +
                $"exactly {rows} values.";

            return false;
        }


        if (
            columnTargets == null ||
            columnTargets.Length != columns
        )
        {
            errorMessage =
                $"Column Targets must contain " +
                $"exactly {columns} values.";

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
                    $"Row {y} has impossible target: " +
                    $"{rowTargets[y]}.";

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
                    $"Column {x} has impossible target: " +
                    $"{columnTargets[x]}.";

                return false;
            }
        }


        // -----------------------------------------
        // HINT SUM
        // -----------------------------------------

        int rowSum =
            0;


        foreach (
            int value
            in rowTargets
        )
        {
            rowSum +=
                value;
        }


        int columnSum =
            0;


        foreach (
            int value
            in columnTargets
        )
        {
            columnSum +=
                value;
        }


        if (rowSum != columnSum)
        {
            errorMessage =
                $"Hint totals do not match. " +
                $"Rows = {rowSum}, " +
                $"Columns = {columnSum}.";

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


        int totalPieceCells =
            0;


        foreach (
            PieceDefinition piece
            in pieces
        )
        {
            if (piece == null)
            {
                errorMessage =
                    "The Pieces list contains " +
                    "an empty element.";

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


        if (
            rowSum !=
            totalPieceCells
        )
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
        // AVAILABLE CELLS
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
                    $"but only {availableCells} are available.";

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
                    $"but only {availableCells} are available.";

                return false;
            }
        }


        // -----------------------------------------
        // SOLUTION EXISTS
        // -----------------------------------------

        if (
            solution == null ||
            solution.Length !=
            pieces.Length
        )
        {
            errorMessage =
                $"Solution must contain exactly " +
                $"{pieces.Length} entries.";

            return false;
        }


        HashSet<int>
            usedPieceIndexes =
                new HashSet<int>();


        HashSet<Vector2Int>
            solutionOccupiedCells =
                new HashSet<Vector2Int>();


        // -----------------------------------------
        // SOLUTION ENTRIES
        // -----------------------------------------

        foreach (
            LevelSolutionEntry entry
            in solution
        )
        {
            if (entry == null)
            {
                errorMessage =
                    "Solution contains " +
                    "an empty entry.";

                return false;
            }


            int pieceIndex =
                entry.PieceIndex;


            if (
                pieceIndex < 0 ||
                pieceIndex >= pieces.Length
            )
            {
                errorMessage =
                    $"Solution Piece Index " +
                    $"{pieceIndex} is invalid.";

                return false;
            }


            if (
                !usedPieceIndexes.Add(
                    pieceIndex
                )
            )
            {
                errorMessage =
                    $"Piece Index {pieceIndex} " +
                    "appears more than once in Solution.";

                return false;
            }


            Vector2Int[] solutionCells =
                GetSolutionCells(
                    entry
                );


            foreach (
                Vector2Int coordinate
                in solutionCells
            )
            {
                if (
                    coordinate.x < 0 ||
                    coordinate.x >= columns ||
                    coordinate.y < 0 ||
                    coordinate.y >= rows
                )
                {
                    errorMessage =
                        $"Solution piece {pieceIndex} " +
                        $"has cell " +
                        $"({coordinate.x}, {coordinate.y}) " +
                        "outside the grid.";

                    return false;
                }


                if (
                    uniqueBlockedCells.Contains(
                        coordinate
                    )
                )
                {
                    errorMessage =
                        $"Solution piece {pieceIndex} " +
                        $"occupies blocked cell " +
                        $"({coordinate.x}, {coordinate.y}).";

                    return false;
                }


                if (
                    !solutionOccupiedCells.Add(
                        coordinate
                    )
                )
                {
                    errorMessage =
                        $"Solution contains overlap at " +
                        $"({coordinate.x}, {coordinate.y}).";

                    return false;
                }
            }
        }


        // -----------------------------------------
        // CHECK SOLUTION AGAINST TARGETS
        // -----------------------------------------

        int[] solutionRows =
            new int[rows];


        int[] solutionColumns =
            new int[columns];


        foreach (
            Vector2Int coordinate
            in solutionOccupiedCells
        )
        {
            solutionRows[
                coordinate.y
            ]++;


            solutionColumns[
                coordinate.x
            ]++;
        }


        for (
            int y = 0;
            y < rows;
            y++
        )
        {
            if (
                solutionRows[y] !=
                rowTargets[y]
            )
            {
                errorMessage =
                    $"Solution row {y} has " +
                    $"{solutionRows[y]} occupied cells, " +
                    $"but Row Target requires " +
                    $"{rowTargets[y]}.";

                return false;
            }
        }


        for (
            int x = 0;
            x < columns;
            x++
        )
        {
            if (
                solutionColumns[x] !=
                columnTargets[x]
            )
            {
                errorMessage =
                    $"Solution column {x} has " +
                    $"{solutionColumns[x]} occupied cells, " +
                    $"but Column Target requires " +
                    $"{columnTargets[x]}.";

                return false;
            }
        }


        errorMessage =
            "";

        return true;
    }
}