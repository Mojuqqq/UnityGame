using System.IO;
using UnityEditor;
using UnityEngine;


[CustomEditor(
    typeof(LevelDatabase)
)]
public class LevelDatabaseEditor :
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
            "LEVEL DATABASE TOOLS",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(
            5
        );


        LevelDatabase database =
            (LevelDatabase)target;


        // =================================================
        // CREATE
        // =================================================

        if (
            GUILayout.Button(
                "СОЗДАТЬ СЛЕДУЮЩИЙ УРОВЕНЬ",
                GUILayout.Height(38)
            )
        )
        {
            CreateNextLevel(
                database
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
                "ПРОВЕРИТЬ ВСЮ БАЗУ",
                GUILayout.Height(32)
            )
        )
        {
            ValidateDatabase(
                database
            );
        }


        // =================================================
        // SOLUTIONS
        // =================================================

        if (
            GUILayout.Button(
                "ПРОВЕРИТЬ РЕШЕНИЯ ВСЕХ УРОВНЕЙ",
                GUILayout.Height(32)
            )
        )
        {
            CheckAllSolutions(
                database
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
    // CREATE NEXT
    // =====================================================

    private void CreateNextLevel(
        LevelDatabase database
    )
    {
        serializedObject.Update();


        SerializedProperty levelsProperty =
            serializedObject
                .FindProperty(
                    "levels"
                );


        int nextLevelNumber =
            levelsProperty.arraySize + 1;


        int columns =
            3;


        int rows =
            3;


        // Используем размеры предыдущего уровня
        // как стартовую точку.
        if (
            database.Levels != null &&
            database.Levels.Length > 0
        )
        {
            LevelData lastLevel =
                database.Levels[
                    database.Levels.Length - 1
                ];


            if (lastLevel != null)
            {
                columns =
                    lastLevel.Columns;


                rows =
                    lastLevel.Rows;
            }
        }


        LevelData newLevel =
            ScriptableObject
                .CreateInstance<
                    LevelData
                >();


        SerializedObject levelObject =
            new SerializedObject(
                newLevel
            );


        levelObject
            .FindProperty(
                "levelNumber"
            )
            .intValue =
                nextLevelNumber;


        levelObject
            .FindProperty(
                "displayName"
            )
            .stringValue =
                $"УРОВЕНЬ " +
                $"{nextLevelNumber}";


        levelObject
            .FindProperty(
                "columns"
            )
            .intValue =
                columns;


        levelObject
            .FindProperty(
                "rows"
            )
            .intValue =
                rows;


        SerializedProperty pieces =
            levelObject
                .FindProperty(
                    "pieces"
                );


        pieces.arraySize =
            0;


        SerializedProperty rowTargets =
            levelObject
                .FindProperty(
                    "rowTargets"
                );


        rowTargets.arraySize =
            rows;


        SerializedProperty columnTargets =
            levelObject
                .FindProperty(
                    "columnTargets"
                );


        columnTargets.arraySize =
            columns;


        SerializedProperty blocked =
            levelObject
                .FindProperty(
                    "blockedCells"
                );


        blocked.arraySize =
            0;


        SerializedProperty solution =
            levelObject
                .FindProperty(
                    "solution"
                );


        solution.arraySize =
            0;


        levelObject
            .ApplyModifiedPropertiesWithoutUndo();


        // =================================================
        // CREATE ASSET
        // =================================================

        string folder =
            "Assets/Data/Levels";


        if (
            !AssetDatabase
                .IsValidFolder(
                    folder
                )
        )
        {
            Debug.LogError(
                $"Folder not found: " +
                $"{folder}"
            );

            return;
        }


        string fileName =
            $"Level_" +
            $"{nextLevelNumber:000}" +
            ".asset";


        string path =
            Path.Combine(
                folder,
                fileName
            )
            .Replace(
                "\\",
                "/"
            );


        path =
            AssetDatabase
                .GenerateUniqueAssetPath(
                    path
                );


        AssetDatabase.CreateAsset(
            newLevel,
            path
        );


        // =================================================
        // ADD TO DATABASE
        // =================================================

        serializedObject.Update();


        levelsProperty =
            serializedObject
                .FindProperty(
                    "levels"
                );


        int newIndex =
            levelsProperty.arraySize;


        levelsProperty
            .InsertArrayElementAtIndex(
                newIndex
            );


        levelsProperty
            .GetArrayElementAtIndex(
                newIndex
            )
            .objectReferenceValue =
                newLevel;


        serializedObject
            .ApplyModifiedProperties();


        EditorUtility.SetDirty(
            database
        );


        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();


        Selection.activeObject =
            newLevel;


        EditorGUIUtility.PingObject(
            newLevel
        );


        SetResult(
            $"Создан Level_" +
            $"{nextLevelNumber:000}.",
            MessageType.Info
        );
    }


    // =====================================================
    // VALIDATE
    // =====================================================

    private void ValidateDatabase(
        LevelDatabase database
    )
    {
        if (
            database.IsValid(
                out string error
            )
        )
        {
            SetResult(
                $"✓ База корректна. " +
                $"Уровней: " +
                $"{database.Count}.",
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
    // CHECK ALL SOLUTIONS
    // =====================================================

    private void CheckAllSolutions(
        LevelDatabase database
    )
    {
        if (
            database.Levels == null
        )
        {
            return;
        }


        int correct =
            0;


        int invalid =
            0;


        int unsolvable =
            0;


        int multiple =
            0;


        string details =
            "";


        foreach (
            LevelData level
            in database.Levels
        )
        {
            if (level == null)
            {
                invalid++;

                continue;
            }


            if (
                !level.IsValid(
                    out string validationError
                )
            )
            {
                invalid++;


                details +=
                    $"\n{level.name}: " +
                    $"INVALID — " +
                    $"{validationError}";


                continue;
            }


            int solutions =
                LevelSolver
                    .CountUniqueSolutions(
                        level,
                        2
                    );


            if (solutions == 0)
            {
                unsolvable++;


                details +=
                    $"\n{level.name}: " +
                    "NO SOLUTION";


                continue;
            }


            if (solutions > 1)
            {
                multiple++;


                details +=
                    $"\n{level.name}: " +
                    "MULTIPLE SOLUTIONS";


                continue;
            }


            correct++;
        }


        string summary =
            $"Проверка завершена.\n\n" +
            $"Уникальное решение: {correct}\n" +
            $"Ошибки данных: {invalid}\n" +
            $"Нет решения: {unsolvable}\n" +
            $"Несколько решений: {multiple}";


        if (
            !string.IsNullOrEmpty(
                details
            )
        )
        {
            summary +=
                "\n\nПроблемы:" +
                details;
        }


        MessageType type =
            (
                invalid == 0 &&
                unsolvable == 0 &&
                multiple == 0
            )
            ?
            MessageType.Info
            :
            MessageType.Warning;


        SetResult(
            summary,
            type
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