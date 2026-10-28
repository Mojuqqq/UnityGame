using System.Collections.Generic;
using System.Text;
using UnityEngine;


public static class LevelSolver
{
    // =====================================================
    // INTERNAL PLACEMENT
    // =====================================================

    private class Placement
    {
        public Vector2Int[] Cells;

        public string Key;
    }


    // =====================================================
    // PUBLIC
    // =====================================================

    public static int CountUniqueSolutions(
        LevelData level,
        int maxSolutions = 2
    )
    {
        if (level == null)
        {
            return 0;
        }


        if (
            level.Pieces == null ||
            level.Pieces.Length == 0
        )
        {
            return 0;
        }


        int rows =
            level.Rows;


        int columns =
            level.Columns;


        HashSet<Vector2Int> blocked =
            new HashSet<Vector2Int>();


        if (level.BlockedCells != null)
        {
            foreach (
                Vector2Int cell
                in level.BlockedCells
            )
            {
                blocked.Add(
                    cell
                );
            }
        }


        // =================================================
        // GENERATE ALL POSSIBLE PLACEMENTS
        // =================================================

        List<Placement>[] placements =
            new List<Placement>[
                level.Pieces.Length
            ];


        for (
            int pieceIndex = 0;
            pieceIndex <
                level.Pieces.Length;
            pieceIndex++
        )
        {
            placements[pieceIndex] =
                GeneratePlacements(
                    level,
                    pieceIndex,
                    blocked
                );


            if (
                placements[pieceIndex].Count
                ==
                0
            )
            {
                return 0;
            }
        }


        // =================================================
        // CHOOSE SEARCH ORDER
        // =================================================

        List<int> pieceOrder =
            new List<int>();


        for (
            int i = 0;
            i < level.Pieces.Length;
            i++
        )
        {
            pieceOrder.Add(
                i
            );
        }


        // Фигуры с меньшим количеством
        // возможных размещений проверяем раньше.
        pieceOrder.Sort(
            (a, b) =>
                placements[a].Count.CompareTo(
                    placements[b].Count
                )
        );


        // =================================================
        // SEARCH STATE
        // =================================================

        bool[,] occupied =
            new bool[
                columns,
                rows
            ];


        int[] rowCounts =
            new int[rows];


        int[] columnCounts =
            new int[columns];


        HashSet<string> uniqueBoards =
            new HashSet<string>();


        Dictionary<
            PieceDefinition,
            int
        > lastPlacementByPieceType =
            new Dictionary<
                PieceDefinition,
                int
            >();


        Search(
            level,
            pieceOrder,
            placements,
            0,
            occupied,
            rowCounts,
            columnCounts,
            blocked,
            uniqueBoards,
            lastPlacementByPieceType,
            Mathf.Max(
                1,
                maxSolutions
            )
        );


        return uniqueBoards.Count;
    }


    // =====================================================
    // SEARCH
    // =====================================================

    private static void Search(
        LevelData level,
        List<int> pieceOrder,
        List<Placement>[] placements,
        int depth,
        bool[,] occupied,
        int[] rowCounts,
        int[] columnCounts,
        HashSet<Vector2Int> blocked,
        HashSet<string> solutions,
        Dictionary<
            PieceDefinition,
            int
        > lastPlacementByPieceType,
        int maxSolutions
    )
    {
        if (
            solutions.Count >=
            maxSolutions
        )
        {
            return;
        }


        // =================================================
        // ALL PIECES PLACED
        // =================================================

        if (
            depth >=
            pieceOrder.Count
        )
        {
            if (
                !TargetsMatch(
                    level,
                    rowCounts,
                    columnCounts
                )
            )
            {
                return;
            }


            solutions.Add(
                BuildBoardKey(
                    occupied,
                    level.Columns,
                    level.Rows
                )
            );


            return;
        }


        int pieceIndex =
            pieceOrder[depth];


        PieceDefinition piece =
            level.Pieces[
                pieceIndex
            ];


        List<Placement>
            currentPlacements =
                placements[
                    pieceIndex
                ];


        int minimumPlacementIndex =
            0;


        // Если используются одинаковые PieceDefinition,
        // не перебираем их перестановки как разные решения.
        if (
            piece != null &&
            lastPlacementByPieceType
                .TryGetValue(
                    piece,
                    out int previousPlacement
                )
        )
        {
            minimumPlacementIndex =
                previousPlacement + 1;
        }


        for (
            int placementIndex =
                minimumPlacementIndex;

            placementIndex <
                currentPlacements.Count;

            placementIndex++
        )
        {
            Placement placement =
                currentPlacements[
                    placementIndex
                ];


            if (
                !CanPlace(
                    level,
                    placement,
                    occupied,
                    rowCounts,
                    columnCounts
                )
            )
            {
                continue;
            }


            // ---------------------------------------------
            // APPLY
            // ---------------------------------------------

            ApplyPlacement(
                placement,
                occupied,
                rowCounts,
                columnCounts,
                true
            );


            int savedPrevious =
    -1;


bool hadPrevious =
    false;


if (piece != null)
{
    hadPrevious =
        lastPlacementByPieceType
            .TryGetValue(
                piece,
                out savedPrevious
            );


    lastPlacementByPieceType[
        piece
    ] =
        placementIndex;
}


            // ---------------------------------------------
            // PRUNE
            // ---------------------------------------------

            if (
                CanStillReachTargets(
                    level,
                    occupied,
                    rowCounts,
                    columnCounts,
                    blocked
                )
            )
            {
                Search(
                    level,
                    pieceOrder,
                    placements,
                    depth + 1,
                    occupied,
                    rowCounts,
                    columnCounts,
                    blocked,
                    solutions,
                    lastPlacementByPieceType,
                    maxSolutions
                );
            }


            // ---------------------------------------------
            // RESTORE SAME-PIECE STATE
            // ---------------------------------------------

            if (piece != null)
            {
                if (hadPrevious)
                {
                    lastPlacementByPieceType[
                        piece
                    ] =
                        savedPrevious;
                }
                else
                {
                    lastPlacementByPieceType
                        .Remove(
                            piece
                        );
                }
            }


            // ---------------------------------------------
            // REMOVE
            // ---------------------------------------------

            ApplyPlacement(
                placement,
                occupied,
                rowCounts,
                columnCounts,
                false
            );


            if (
                solutions.Count >=
                maxSolutions
            )
            {
                return;
            }
        }
    }


    // =====================================================
    // GENERATE PLACEMENTS
    // =====================================================

    private static List<Placement>
        GeneratePlacements(
            LevelData level,
            int pieceIndex,
            HashSet<Vector2Int> blocked
        )
    {
        List<Placement> result =
            new List<Placement>();


        HashSet<string> unique =
            new HashSet<string>();


        for (
            int rotation = 0;
            rotation < 4;
            rotation++
        )
        {
            Vector2Int[] shape =
                level.GetRotatedPieceCells(
                    pieceIndex,
                    rotation
                );


            if (
                shape == null ||
                shape.Length == 0
            )
            {
                continue;
            }


            string shapeKey =
                BuildShapeKey(
                    shape
                );


            // Например квадрат O после поворота
            // остаётся тем же квадратом.
            if (
                !unique.Add(
                    "ROT_" +
                    shapeKey
                )
            )
            {
                continue;
            }


            int width =
                0;


            int height =
                0;


            foreach (
                Vector2Int cell
                in shape
            )
            {
                width =
                    Mathf.Max(
                        width,
                        cell.x + 1
                    );


                height =
                    Mathf.Max(
                        height,
                        cell.y + 1
                    );
            }


            for (
                int y = 0;
                y <=
                    level.Rows - height;
                y++
            )
            {
                for (
                    int x = 0;
                    x <=
                        level.Columns - width;
                    x++
                )
                {
                    Vector2Int[] cells =
                        new Vector2Int[
                            shape.Length
                        ];


                    bool valid =
                        true;


                    for (
                        int i = 0;
                        i < shape.Length;
                        i++
                    )
                    {
                        Vector2Int cell =
                            new Vector2Int(
                                x +
                                shape[i].x,

                                y +
                                shape[i].y
                            );


                        if (
                            blocked.Contains(
                                cell
                            )
                        )
                        {
                            valid =
                                false;

                            break;
                        }


                        cells[i] =
                            cell;
                    }


                    if (!valid)
                    {
                        continue;
                    }


                    string key =
                        BuildShapeKey(
                            cells
                        );


                    if (
                        result.Exists(
                            placement =>
                                placement.Key ==
                                key
                        )
                    )
                    {
                        continue;
                    }


                    result.Add(
                        new Placement
                        {
                            Cells =
                                cells,

                            Key =
                                key
                        }
                    );
                }
            }
        }


        result.Sort(
            (a, b) =>
                string.CompareOrdinal(
                    a.Key,
                    b.Key
                )
        );


        return result;
    }


    // =====================================================
    // CAN PLACE
    // =====================================================

    private static bool CanPlace(
        LevelData level,
        Placement placement,
        bool[,] occupied,
        int[] rowCounts,
        int[] columnCounts
    )
    {
        Dictionary<int, int>
            addedRows =
                new Dictionary<int, int>();


        Dictionary<int, int>
            addedColumns =
                new Dictionary<int, int>();


        foreach (
            Vector2Int cell
            in placement.Cells
        )
        {
            if (
                occupied[
                    cell.x,
                    cell.y
                ]
            )
            {
                return false;
            }


            if (
                !addedRows.ContainsKey(
                    cell.y
                )
            )
            {
                addedRows[
                    cell.y
                ] =
                    0;
            }


            addedRows[
                cell.y
            ]++;


            if (
                !addedColumns.ContainsKey(
                    cell.x
                )
            )
            {
                addedColumns[
                    cell.x
                ] =
                    0;
            }


            addedColumns[
                cell.x
            ]++;
        }


        foreach (
            KeyValuePair<int, int>
            row
            in addedRows
        )
        {
            if (
                rowCounts[
                    row.Key
                ]
                +
                row.Value
                >
                level.RowTargets[
                    row.Key
                ]
            )
            {
                return false;
            }
        }


        foreach (
            KeyValuePair<int, int>
            column
            in addedColumns
        )
        {
            if (
                columnCounts[
                    column.Key
                ]
                +
                column.Value
                >
                level.ColumnTargets[
                    column.Key
                ]
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // APPLY / REMOVE
    // =====================================================

    private static void ApplyPlacement(
        Placement placement,
        bool[,] occupied,
        int[] rowCounts,
        int[] columnCounts,
        bool add
    )
    {
        int value =
            add
                ? 1
                : -1;


        foreach (
            Vector2Int cell
            in placement.Cells
        )
        {
            occupied[
                cell.x,
                cell.y
            ] =
                add;


            rowCounts[
                cell.y
            ] +=
                value;


            columnCounts[
                cell.x
            ] +=
                value;
        }
    }


    // =====================================================
    // PRUNING
    // =====================================================

    private static bool CanStillReachTargets(
        LevelData level,
        bool[,] occupied,
        int[] rowCounts,
        int[] columnCounts,
        HashSet<Vector2Int> blocked
    )
    {
        // ---------------------------------------------
        // ROWS
        // ---------------------------------------------

        for (
            int y = 0;
            y < level.Rows;
            y++
        )
        {
            int needed =
                level.RowTargets[y]
                -
                rowCounts[y];


            if (needed < 0)
            {
                return false;
            }


            int free =
                0;


            for (
                int x = 0;
                x < level.Columns;
                x++
            )
            {
                Vector2Int cell =
                    new Vector2Int(
                        x,
                        y
                    );


                if (
                    occupied[x, y] ||
                    blocked.Contains(cell)
                )
                {
                    continue;
                }


                free++;
            }


            if (needed > free)
            {
                return false;
            }
        }


        // ---------------------------------------------
        // COLUMNS
        // ---------------------------------------------

        for (
            int x = 0;
            x < level.Columns;
            x++
        )
        {
            int needed =
                level.ColumnTargets[x]
                -
                columnCounts[x];


            if (needed < 0)
            {
                return false;
            }


            int free =
                0;


            for (
                int y = 0;
                y < level.Rows;
                y++
            )
            {
                Vector2Int cell =
                    new Vector2Int(
                        x,
                        y
                    );


                if (
                    occupied[x, y] ||
                    blocked.Contains(cell)
                )
                {
                    continue;
                }


                free++;
            }


            if (needed > free)
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // TARGET MATCH
    // =====================================================

    private static bool TargetsMatch(
        LevelData level,
        int[] rowCounts,
        int[] columnCounts
    )
    {
        for (
            int y = 0;
            y < level.Rows;
            y++
        )
        {
            if (
                rowCounts[y] !=
                level.RowTargets[y]
            )
            {
                return false;
            }
        }


        for (
            int x = 0;
            x < level.Columns;
            x++
        )
        {
            if (
                columnCounts[x] !=
                level.ColumnTargets[x]
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // KEYS
    // =====================================================

    private static string BuildShapeKey(
        Vector2Int[] cells
    )
    {
        List<Vector2Int> sorted =
            new List<Vector2Int>(
                cells
            );


        sorted.Sort(
            (a, b) =>
            {
                int y =
                    a.y.CompareTo(
                        b.y
                    );


                if (y != 0)
                {
                    return y;
                }


                return
                    a.x.CompareTo(
                        b.x
                    );
            }
        );


        StringBuilder builder =
            new StringBuilder();


        foreach (
            Vector2Int cell
            in sorted
        )
        {
            builder
                .Append(
                    cell.x
                )
                .Append(
                    ","
                )
                .Append(
                    cell.y
                )
                .Append(
                    ";"
                );
        }


        return builder.ToString();
    }


    private static string BuildBoardKey(
        bool[,] occupied,
        int columns,
        int rows
    )
    {
        StringBuilder builder =
            new StringBuilder(
                columns *
                rows
            );


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
                builder.Append(
                    occupied[x, y]
                        ? "1"
                        : "0"
                );
            }
        }


        return builder.ToString();
    }
}