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

        // Канонический набор абсолютных клеток
        // этого конкретного размещения.
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
        // ALL POSSIBLE PLACEMENTS FOR EACH PIECE
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


            // Если хотя бы одну фигуру
            // невозможно поставить вообще,
            // решений быть не может.
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
        // SEARCH ORDER
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


        // Сначала рассматриваем фигуры,
        // у которых меньше вариантов размещения.
        // Это сильно ускоряет перебор.
        //
        // При одинаковом количестве вариантов
        // используем Piece Index,
        // чтобы порядок был стабильным.
        pieceOrder.Sort(
            (a, b) =>
            {
                int placementComparison =
                    placements[a]
                        .Count
                        .CompareTo(
                            placements[b].Count
                        );


                if (placementComparison != 0)
                {
                    return placementComparison;
                }


                return
                    a.CompareTo(
                        b
                    );
            }
        );


        // =================================================
        // CURRENT BOARD STATE
        // =================================================

        bool[,] occupied =
            new bool[
                columns,
                rows
            ];


        int[] rowCounts =
            new int[
                rows
            ];


        int[] columnCounts =
            new int[
                columns
            ];


        // Здесь храним конкретное размещение
        // каждого Piece Index для текущей ветки поиска.
        Placement[] selectedPlacements =
            new Placement[
                level.Pieces.Length
            ];


        // =================================================
        // UNIQUE SOLUTIONS
        // =================================================

        HashSet<string> uniqueSolutions =
            new HashSet<string>();


        // =================================================
        // IDENTICAL PIECES
        // =================================================

        // Для одинаковых PieceDefinition вводим
        // канонический порядок размещений.
        //
        // Например:
        //
        // L1 = A, L2 = B
        //
        // и
        //
        // L1 = B, L2 = A
        //
        // не будут перебраны как два разных решения.
        Dictionary<
            PieceDefinition,
            string
        > lastPlacementKeyByPieceType =
            new Dictionary<
                PieceDefinition,
                string
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
            uniqueSolutions,
            selectedPlacements,
            lastPlacementKeyByPieceType,
            Mathf.Max(
                1,
                maxSolutions
            )
        );


        return uniqueSolutions.Count;
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
        Placement[] selectedPlacements,
        Dictionary<
            PieceDefinition,
            string
        > lastPlacementKeyByPieceType,
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


            // ВАЖНО:
            //
            // раньше здесь был BuildBoardKey(),
            // который смотрел только на итоговую
            // занятость поля.
            //
            // Теперь ключ содержит реальные
            // размещения каждой фигуры.
            string solutionKey =
                BuildSolutionKey(
                    level,
                    selectedPlacements
                );


            if (
                !string.IsNullOrEmpty(
                    solutionKey
                )
            )
            {
                solutions.Add(
                    solutionKey
                );
            }


            return;
        }


        // =================================================
        // CURRENT PIECE
        // =================================================

        int pieceIndex =
            pieceOrder[
                depth
            ];


        PieceDefinition piece =
            level.Pieces[
                pieceIndex
            ];


        List<Placement> currentPlacements =
            placements[
                pieceIndex
            ];


        // =================================================
        // IDENTICAL PIECE SYMMETRY
        // =================================================

        string minimumPlacementKey =
            null;


        if (piece != null)
        {
            lastPlacementKeyByPieceType
                .TryGetValue(
                    piece,
                    out minimumPlacementKey
                );
        }


        // =================================================
        // TRY PLACEMENTS
        // =================================================

        foreach (
            Placement placement
            in currentPlacements
        )
        {
            // ---------------------------------------------
            // IDENTICAL PIECES
            // ---------------------------------------------
            //
            // Для одинаковых фигур разрешаем
            // только возрастающий порядок ключей.
            //
            // Благодаря этому перестановка
            // одинаковых экземпляров не создаёт
            // отдельную ветку решения.

            if (
                minimumPlacementKey != null &&
                string.CompareOrdinal(
                    placement.Key,
                    minimumPlacementKey
                )
                <=
                0
            )
            {
                continue;
            }


            // ---------------------------------------------
            // COLLISION / TARGET LIMIT
            // ---------------------------------------------

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


            selectedPlacements[
                pieceIndex
            ] =
                placement;


            // ---------------------------------------------
            // SAVE IDENTICAL-PIECE STATE
            // ---------------------------------------------

            string previousPlacementKey =
                null;


            bool hadPreviousPlacement =
                false;


            if (piece != null)
            {
                hadPreviousPlacement =
                    lastPlacementKeyByPieceType
                        .TryGetValue(
                            piece,
                            out previousPlacementKey
                        );


                lastPlacementKeyByPieceType[
                    piece
                ] =
                    placement.Key;
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
                    selectedPlacements,
                    lastPlacementKeyByPieceType,
                    maxSolutions
                );
            }


            // ---------------------------------------------
            // RESTORE IDENTICAL-PIECE STATE
            // ---------------------------------------------

            if (piece != null)
            {
                if (hadPreviousPlacement)
                {
                    lastPlacementKeyByPieceType[
                        piece
                    ] =
                        previousPlacementKey;
                }
                else
                {
                    lastPlacementKeyByPieceType
                        .Remove(
                            piece
                        );
                }
            }


            // ---------------------------------------------
            // RESTORE SELECTED PLACEMENT
            // ---------------------------------------------

            selectedPlacements[
                pieceIndex
            ] =
                null;


            // ---------------------------------------------
            // REMOVE FROM BOARD
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
    // GENERATE ALL PLACEMENTS
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


        // Разные Rotation Steps иногда дают
        // одинаковую форму:
        //
        // O: 0°, 90°, 180°, 270°
        // визуально всё равно один квадрат.
        //
        // Поэтому одинаковые формы
        // не генерируем повторно.
        HashSet<string> uniqueRotations =
            new HashSet<string>();


        // А это защищает уже от одинаковых
        // абсолютных размещений на поле.
        HashSet<string> uniquePlacements =
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


            if (
                !uniqueRotations.Add(
                    shapeKey
                )
            )
            {
                continue;
            }


            // ---------------------------------------------
            // SHAPE SIZE
            // ---------------------------------------------

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


            // ---------------------------------------------
            // ALL ORIGINS
            // ---------------------------------------------

            for (
                int y = 0;
                y <=
                    level.Rows -
                    height;
                y++
            )
            {
                for (
                    int x = 0;
                    x <=
                        level.Columns -
                        width;
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


                    string placementKey =
                        BuildShapeKey(
                            cells
                        );


                    if (
                        !uniquePlacements.Add(
                            placementKey
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
                                placementKey
                        }
                    );
                }
            }
        }


        // Очень важно для одинаковых фигур:
        // одинаковый набор placements должен
        // иметь один и тот же стабильный порядок.
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
        Dictionary<int, int> addedRows =
            new Dictionary<int, int>();


        Dictionary<int, int> addedColumns =
            new Dictionary<int, int>();


        foreach (
            Vector2Int cell
            in placement.Cells
        )
        {
            // ---------------------------------------------
            // COLLISION
            // ---------------------------------------------

            if (
                occupied[
                    cell.x,
                    cell.y
                ]
            )
            {
                return false;
            }


            // ---------------------------------------------
            // ROW ADDITIONS
            // ---------------------------------------------

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


            // ---------------------------------------------
            // COLUMN ADDITIONS
            // ---------------------------------------------

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


        // ---------------------------------------------
        // ROW TARGET LIMIT
        // ---------------------------------------------

        foreach (
            KeyValuePair<int, int> row
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


        // ---------------------------------------------
        // COLUMN TARGET LIMIT
        // ---------------------------------------------

        foreach (
            KeyValuePair<int, int> column
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
        // =================================================
        // ROWS
        // =================================================

        for (
            int y = 0;
            y < level.Rows;
            y++
        )
        {
            int needed =
                level.RowTargets[
                    y
                ]
                -
                rowCounts[
                    y
                ];


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
                    occupied[
                        x,
                        y
                    ]
                    ||
                    blocked.Contains(
                        cell
                    )
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


        // =================================================
        // COLUMNS
        // =================================================

        for (
            int x = 0;
            x < level.Columns;
            x++
        )
        {
            int needed =
                level.ColumnTargets[
                    x
                ]
                -
                columnCounts[
                    x
                ];


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
                    occupied[
                        x,
                        y
                    ]
                    ||
                    blocked.Contains(
                        cell
                    )
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
    // FINAL TARGET CHECK
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
                rowCounts[
                    y
                ]
                !=
                level.RowTargets[
                    y
                ]
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
                columnCounts[
                    x
                ]
                !=
                level.ColumnTargets[
                    x
                ]
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // SOLUTION KEY
    // =====================================================

    private static string BuildSolutionKey(
        LevelData level,
        Placement[] selectedPlacements
    )
    {
        if (
            level == null ||
            level.Pieces == null ||
            selectedPlacements == null ||
            selectedPlacements.Length !=
                level.Pieces.Length
        )
        {
            return "";
        }


        // =================================================
        // GROUP BY PIECE TYPE
        // =================================================
        //
        // Пример:
        //
        // Pieces:
        // L, T, L, L
        //
        // превратятся в:
        //
        // L -> [placement A,
        //       placement B,
        //       placement C]
        //
        // T -> [placement D]
        //
        // Размещения одинаковых L потом
        // сортируются.
        //
        // Поэтому перестановка L1/L2/L3
        // не меняет ключ решения.

        Dictionary<
            PieceDefinition,
            List<string>
        > placementsByType =
            new Dictionary<
                PieceDefinition,
                List<string>
            >();


        List<PieceDefinition> typeOrder =
            new List<PieceDefinition>();


        for (
            int pieceIndex = 0;
            pieceIndex <
                level.Pieces.Length;
            pieceIndex++
        )
        {
            PieceDefinition definition =
                level.Pieces[
                    pieceIndex
                ];


            Placement placement =
                selectedPlacements[
                    pieceIndex
                ];


            if (
                definition == null ||
                placement == null
            )
            {
                return "";
            }


            if (
                !placementsByType
                    .TryGetValue(
                        definition,
                        out List<string>
                            typePlacements
                    )
            )
            {
                typePlacements =
                    new List<string>();


                placementsByType[
                    definition
                ] =
                    typePlacements;


                // Порядок типов определяется
                // первым появлением в Pieces.
                // Он стабилен для данного уровня.
                typeOrder.Add(
                    definition
                );
            }


            typePlacements.Add(
                placement.Key
            );
        }


        // =================================================
        // BUILD CANONICAL KEY
        // =================================================

        StringBuilder builder =
            new StringBuilder();


        for (
            int typeIndex = 0;
            typeIndex <
                typeOrder.Count;
            typeIndex++
        )
        {
            PieceDefinition definition =
                typeOrder[
                    typeIndex
                ];


            List<string> typePlacements =
                placementsByType[
                    definition
                ];


            // Самый важный момент:
            //
            // одинаковые экземпляры фигуры
            // становятся неразличимыми.
            typePlacements.Sort(
                System.StringComparer
                    .Ordinal
            );


            builder
                .Append(
                    "TYPE_"
                )
                .Append(
                    typeIndex
                )
                .Append(
                    "{"
                );


            foreach (
                string placementKey
                in typePlacements
            )
            {
                builder
                    .Append(
                        "["
                    )
                    .Append(
                        placementKey
                    )
                    .Append(
                        "]"
                    );
            }


            builder.Append(
                "}"
            );
        }


        return builder.ToString();
    }


    // =====================================================
    // CELL SET KEY
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
                int yComparison =
                    a.y.CompareTo(
                        b.y
                    );


                if (yComparison != 0)
                {
                    return yComparison;
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
}