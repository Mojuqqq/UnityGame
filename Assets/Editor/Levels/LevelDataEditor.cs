using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(
    typeof(LevelData)
)]
public class LevelDataEditor :
    Editor
{
    private string resultMessage =
        "";


    private MessageType resultType =
        MessageType.Info;


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        EditorGUILayout.Space(
            20
        );


        EditorGUILayout.LabelField(
            "LEVEL TOOLS",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(
            5
        );


        LevelData level =
            (LevelData)target;


        // =================================================
        // GENERATE TARGETS
        // =================================================

        if (
            GUILayout.Button(
                "СГЕНЕРИРОВАТЬ HINTS ИЗ SOLUTION",
                GUILayout.Height(36)
            )
        )
        {
            GenerateTargets(
                level
            );
        }


        EditorGUILayout.Space(
            5
        );


        // =================================================
        // VALIDATE
        // =================================================

        if (
            GUILayout.Button(
                "ПРОВЕРИТЬ УРОВЕНЬ",
                GUILayout.Height(32)
            )
        )
        {
            ValidateLevel(
                level
            );
        }


        // =================================================
        // SOLVER
        // =================================================

        if (
            GUILayout.Button(
                "ПРОВЕРИТЬ КОЛИЧЕСТВО РЕШЕНИЙ",
                GUILayout.Height(32)
            )
        )
        {
            CheckSolutions(
                level
            );
        }


        if (
            !string.IsNullOrWhiteSpace(
                resultMessage
            )
        )
        {
            EditorGUILayout.Space(
                10
            );


            EditorGUILayout.HelpBox(
                resultMessage,
                resultType
            );
        }
    }


    // =====================================================
    // GENERATE TARGETS
    // =====================================================

    private void GenerateTargets(
        LevelData level
    )
    {
        if (level == null)
        {
            return;
        }


        if (
            level.Solution == null ||
            level.Pieces == null ||
            level.Solution.Length !=
                level.Pieces.Length
        )
        {
            SetResult(
                "Solution должен содержать " +
                "ровно столько записей, " +
                "сколько фигур в Pieces.",
                MessageType.Error
            );

            return;
        }


        int[] rows =
            new int[
                level.Rows
            ];


        int[] columns =
            new int[
                level.Columns
            ];


        HashSet<Vector2Int> occupied =
            new HashSet<Vector2Int>();


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


        HashSet<int> usedPieces =
            new HashSet<int>();


        foreach (
            LevelSolutionEntry entry
            in level.Solution
        )
        {
            if (entry == null)
            {
                SetResult(
                    "Solution содержит " +
                        "пустую запись.",
                    MessageType.Error
                );

                return;
            }


            if (
                entry.PieceIndex < 0 ||
                entry.PieceIndex >=
                    level.Pieces.Length
            )
            {
                SetResult(
                    "В Solution указан " +
                        "неверный Piece Index.",
                    MessageType.Error
                );

                return;
            }


            if (
                !usedPieces.Add(
                    entry.PieceIndex
                )
            )
            {
                SetResult(
                    $"Piece Index " +
                    $"{entry.PieceIndex} " +
                    "использован дважды.",
                    MessageType.Error
                );

                return;
            }


            Vector2Int[] cells =
                level.GetSolutionCells(
                    entry
                );


            foreach (
                Vector2Int cell
                in cells
            )
            {
                if (
                    cell.x < 0 ||
                    cell.x >= level.Columns ||
                    cell.y < 0 ||
                    cell.y >= level.Rows
                )
                {
                    SetResult(
                        $"Клетка " +
                        $"({cell.x}, {cell.y}) " +
                        "выходит за поле.",
                        MessageType.Error
                    );

                    return;
                }


                if (
                    blocked.Contains(
                        cell
                    )
                )
                {
                    SetResult(
                        $"Solution занимает " +
                        $"Blocked Cell " +
                        $"({cell.x}, {cell.y}).",
                        MessageType.Error
                    );

                    return;
                }


                if (
                    !occupied.Add(
                        cell
                    )
                )
                {
                    SetResult(
                        $"Фигуры пересекаются " +
                        $"в клетке " +
                        $"({cell.x}, {cell.y}).",
                        MessageType.Error
                    );

                    return;
                }


                rows[
                    cell.y
                ]++;


                columns[
                    cell.x
                ]++;
            }
        }


        // =================================================
        // WRITE SERIALIZED ARRAYS
        // =================================================

        serializedObject.Update();


        SerializedProperty rowTargets =
            serializedObject
                .FindProperty(
                    "rowTargets"
                );


        SerializedProperty columnTargets =
            serializedObject
                .FindProperty(
                    "columnTargets"
                );


        rowTargets.arraySize =
            level.Rows;


        for (
            int i = 0;
            i < rows.Length;
            i++
        )
        {
            rowTargets
                .GetArrayElementAtIndex(
                    i
                )
                .intValue =
                    rows[i];
        }


        columnTargets.arraySize =
            level.Columns;


        for (
            int i = 0;
            i < columns.Length;
            i++
        )
        {
            columnTargets
                .GetArrayElementAtIndex(
                    i
                )
                .intValue =
                    columns[i];
        }


        serializedObject
            .ApplyModifiedProperties();


        EditorUtility.SetDirty(
            level
        );


        AssetDatabase.SaveAssets();


        SetResult(
            "Hints успешно рассчитаны " +
            "из Solution.",
            MessageType.Info
        );
    }


    // =====================================================
    // VALIDATE
    // =====================================================

    private void ValidateLevel(
        LevelData level
    )
    {
        if (
            level.IsValid(
                out string error
            )
        )
        {
            SetResult(
                "✓ Уровень корректен.",
                MessageType.Info
            );
        }
        else
        {
            SetResult(
                "✕ Ошибка:\n" +
                error,
                MessageType.Error
            );
        }
    }


    // =====================================================
    // SOLUTIONS
    // =====================================================

    private void CheckSolutions(
        LevelData level
    )
    {
        if (
            !level.IsValid(
                out string error
            )
        )
        {
            SetResult(
                "Сначала исправь уровень:\n" +
                error,
                MessageType.Error
            );

            return;
        }


        int solutions =
            LevelSolver
                .CountUniqueSolutions(
                    level,
                    2
                );


        if (solutions == 0)
        {
            SetResult(
                "✕ Решений не найдено.",
                MessageType.Error
            );

            return;
        }


        if (solutions == 1)
        {
            SetResult(
                "✓ Найдено ровно одно решение.",
                MessageType.Info
            );

            return;
        }


        SetResult(
            "⚠ Уровень имеет несколько решений.",
            MessageType.Warning
        );
    }


    // =====================================================
    // MESSAGE
    // =====================================================

    private void SetResult(
        string message,
        MessageType type
    )
    {
        resultMessage =
            message;


        resultType =
            type;


        Repaint();
    }
}