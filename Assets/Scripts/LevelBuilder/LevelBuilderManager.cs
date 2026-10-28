using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class LevelBuilderManager :
    MonoBehaviour
{
    // =====================================================
    // DATABASE
    // =====================================================

    [Header("Database")]

    [SerializeField]
    private LevelDatabase levelDatabase;


    // =====================================================
    // INPUT
    // =====================================================

    [Header("Inputs")]

    [SerializeField]
    private TMP_InputField levelNumberInput;

    [SerializeField]
    private TMP_InputField columnsInput;

    [SerializeField]
    private TMP_InputField rowsInput;


    // =====================================================
    // BOARD
    // =====================================================

    [Header("Board")]

    [SerializeField]
    private RectTransform boardArea;

    [SerializeField]
    private RectTransform gridCellsContainer;

    [SerializeField]
    private RectTransform placedPiecesContainer;

    [SerializeField]
    private RectTransform dragLayer;


    [SerializeField]
    private LevelBuilderCell cellPrefab;


    [Header("Board Layout")]

    [SerializeField]
    [Min(1f)]
    private float maxCellSize =
        110f;


    [SerializeField]
    [Min(0f)]
    private float spacing =
        8f;


    // =====================================================
    // HINTS
    // =====================================================

    [Header("Hints")]

    [SerializeField]
    private RectTransform columnHintsContainer;

    [SerializeField]
    private RectTransform rowHintsContainer;

    [SerializeField]
    private TMP_Text hintTextPrefab;


    // =====================================================
    // PALETTE
    // =====================================================

    [Header("Palette")]

    [SerializeField]
    private Transform paletteContent;

    [SerializeField]
    private LevelBuilderPaletteItem
        paletteItemPrefab;

    [SerializeField]
    private LevelBuilderPieceInstance
        pieceInstancePrefab;

    [SerializeField]
    private PieceDefinition[]
        availablePieces;


    // =====================================================
    // STATUS
    // =====================================================

    [Header("Status")]

    [SerializeField]
    private TMP_Text statusText;


    [SerializeField]
    private TMP_Text blockedModeButtonText;


    // =====================================================
    // STATE
    // =====================================================

    private int columns =
        4;

    private int rows =
        4;


    private float cellSize;


    private bool blockedMode;


    private readonly HashSet<Vector2Int>
        blockedCells =
            new HashSet<Vector2Int>();


    private readonly Dictionary<
        Vector2Int,
        LevelBuilderCell
    > cellViews =
        new Dictionary<
            Vector2Int,
            LevelBuilderCell
        >();


    private readonly Dictionary<
        Vector2Int,
        LevelBuilderPieceInstance
    > occupiedCells =
        new Dictionary<
            Vector2Int,
            LevelBuilderPieceInstance
        >();


    private readonly List<
        LevelBuilderPieceInstance
    > placedPieces =
        new List<
            LevelBuilderPieceInstance
        >();


    private readonly List<TMP_Text>
        rowHintTexts =
            new List<TMP_Text>();


    private readonly List<TMP_Text>
        columnHintTexts =
            new List<TMP_Text>();


    private int[] rowTargets;

    private int[] columnTargets;


    // =====================================================
    // PUBLIC
    // =====================================================

    public RectTransform DragLayer =>
        dragLayer;


    public float CellSize =>
        cellSize;


    public float Spacing =>
        spacing;


    // =====================================================
    // START
    // =====================================================

    private IEnumerator Start()
{
    yield return null;


    Canvas.ForceUpdateCanvases();


    BuildNewGrid();


    BuildPalette();
}


    // =====================================================
    // PALETTE
    // =====================================================

    private void BuildPalette()
    {
        if (
            paletteContent == null ||
            paletteItemPrefab == null ||
            availablePieces == null
        )
        {
            return;
        }


        for (
            int i =
                paletteContent.childCount - 1;

            i >= 0;

            i--
        )
        {
            Destroy(
                paletteContent
                    .GetChild(i)
                    .gameObject
            );
        }


        foreach (
            PieceDefinition definition
            in availablePieces
        )
        {
            if (definition == null)
            {
                continue;
            }


            LevelBuilderPaletteItem item =
                Instantiate(
                    paletteItemPrefab,
                    paletteContent
                );


            item.Initialize(
                this,
                definition
            );
        }
    }


    // =====================================================
    // INPUT
    // =====================================================

    private int ReadInt(
        TMP_InputField input,
        int fallback
    )
    {
        if (
            input == null ||
            !int.TryParse(
                input.text,
                out int value
            )
        )
        {
            return fallback;
        }


        return value;
    }


    // =====================================================
    // NEW GRID
    // =====================================================

    public void BuildNewGrid()
    {
        columns =
            Mathf.Clamp(
                ReadInt(
                    columnsInput,
                    4
                ),
                1,
                12
            );


        rows =
            Mathf.Clamp(
                ReadInt(
                    rowsInput,
                    4
                ),
                1,
                12
            );


        ClearCurrentLevel();


        BuildGridVisual();


        GenerateHintsInternal();


        SetStatus(
            $"Создано поле " +
            $"{columns} × {rows}."
        );
    }


    // =====================================================
    // GRID VISUAL
    // =====================================================

    private void BuildGridVisual()
    {
        Canvas.ForceUpdateCanvases();


        ClearChildren(
            gridCellsContainer
        );


        ClearChildren(
            rowHintsContainer
        );


        ClearChildren(
            columnHintsContainer
        );


        cellViews.Clear();

        rowHintTexts.Clear();

        columnHintTexts.Clear();


        float widthAvailable =
            Mathf.Max(
                1f,
                boardArea.rect.width
            );


        float heightAvailable =
            Mathf.Max(
                1f,
                boardArea.rect.height
            );


        float byWidth =
            (
                widthAvailable -
                spacing *
                (columns - 1)
            )
            /
            columns;


        float byHeight =
            (
                heightAvailable -
                spacing *
                (rows - 1)
            )
            /
            rows;


        cellSize =
            Mathf.Min(
                maxCellSize,
                byWidth,
                byHeight
            );


        float gridWidth =
            columns * cellSize +
            (columns - 1) *
            spacing;


        float gridHeight =
            rows * cellSize +
            (rows - 1) *
            spacing;


        float gridLeft =
    -gridWidth * 0.5f;


float gridTop =
    gridHeight * 0.5f;


float hintSize =
    45f;


float hintGap =
    16f;


// =================================================
// GRID
// =================================================

ConfigureTopLeftContainer(
    gridCellsContainer,
    gridWidth,
    gridHeight,
    gridLeft,
    gridTop
);


ConfigureTopLeftContainer(
    placedPiecesContainer,
    gridWidth,
    gridHeight,
    gridLeft,
    gridTop
);


// =================================================
// COLUMN HINTS
// Над сеткой
// =================================================

ConfigureTopLeftContainer(
    columnHintsContainer,
    gridWidth,
    hintSize,

    gridLeft,

    gridTop +
    hintGap +
    hintSize
);


// =================================================
// ROW HINTS
// Слева от сетки
// =================================================

ConfigureTopLeftContainer(
    rowHintsContainer,
    hintSize,
    gridHeight,

    gridLeft -
    hintGap -
    hintSize,

    gridTop
);


        float step =
            cellSize +
            spacing;


        // =================================================
        // CELLS
        // =================================================

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
                Vector2Int coordinate =
                    new Vector2Int(
                        x,
                        y
                    );


                LevelBuilderCell cell =
                    Instantiate(
                        cellPrefab,
                        gridCellsContainer
                    );


                RectTransform rect =
                    cell.GetComponent<
                        RectTransform
                    >();


                SetupTopLeftChild(
                    rect
                );


                rect.sizeDelta =
                    new Vector2(
                        cellSize,
                        cellSize
                    );


                rect.anchoredPosition =
                    new Vector2(
                        x * step,
                        -y * step
                    );


                cell.Initialize(
                    this,
                    coordinate
                );


                cellViews[
                    coordinate
                ] =
                    cell;
            }
        }


        // =================================================
        // COLUMN HINTS
        // =================================================

        for (
            int x = 0;
            x < columns;
            x++
        )
        {
            TMP_Text hint =
                Instantiate(
                    hintTextPrefab,
                    columnHintsContainer
                );


            RectTransform rect =
                hint.rectTransform;


            SetupTopLeftChild(
                rect
            );


            rect.sizeDelta =
                new Vector2(
                    cellSize,
                    45f
                );


            rect.anchoredPosition =
                new Vector2(
                    x * step,
                    0f
                );


            hint.alignment =
                TextAlignmentOptions
                    .Center;


            columnHintTexts.Add(
                hint
            );
        }


        // =================================================
        // ROW HINTS
        // =================================================

        for (
            int y = 0;
            y < rows;
            y++
        )
        {
            TMP_Text hint =
                Instantiate(
                    hintTextPrefab,
                    rowHintsContainer
                );


            RectTransform rect =
                hint.rectTransform;


            SetupTopLeftChild(
                rect
            );


            rect.sizeDelta =
                new Vector2(
                    45f,
                    cellSize
                );


            rect.anchoredPosition =
                new Vector2(
                    0f,
                    -y * step
                );


            hint.alignment =
                TextAlignmentOptions
                    .Center;


            rowHintTexts.Add(
                hint
            );
        }
    }


    private void ConfigureTopLeftContainer(
    RectTransform rect,
    float width,
    float height,
    float x,
    float y
)
{
    if (rect == null)
    {
        return;
    }


    rect.anchorMin =
        new Vector2(
            0.5f,
            0.5f
        );


    rect.anchorMax =
        new Vector2(
            0.5f,
            0.5f
        );


    rect.pivot =
        new Vector2(
            0f,
            1f
        );


    rect.sizeDelta =
        new Vector2(
            width,
            height
        );


    rect.anchoredPosition =
        new Vector2(
            x,
            y
        );
}


    private void SetupTopLeftChild(
        RectTransform rect
    )
    {
        rect.anchorMin =
            new Vector2(
                0f,
                1f
            );


        rect.anchorMax =
            new Vector2(
                0f,
                1f
            );


        rect.pivot =
            new Vector2(
                0f,
                1f
            );
    }


    // =====================================================
    // PALETTE SPAWN
    // =====================================================

    public LevelBuilderPieceInstance
        CreatePieceFromPalette(
            PieceDefinition definition
        )
    {
        if (
            pieceInstancePrefab == null ||
            dragLayer == null
        )
        {
            return null;
        }


        LevelBuilderPieceInstance instance =
            Instantiate(
                pieceInstancePrefab,
                dragLayer
            );


        instance.Initialize(
            this,
            definition
        );


        return instance;
    }


    public void RefreshPieceLayout(
        LevelBuilderPieceInstance piece
    )
    {
        if (
            piece == null ||
            piece.PieceView == null
        )
        {
            return;
        }


        piece.PieceView.SetLayout(
            cellSize,
            spacing
        );
    }


    // =====================================================
    // DROP
    // =====================================================

    public bool TryDropPiece(
        LevelBuilderPieceInstance piece
    )
    {
        if (
            piece == null ||
            piece.RectTransform == null
        )
        {
            return false;
        }


        Vector3[] corners =
            new Vector3[4];


        piece.RectTransform
            .GetWorldCorners(
                corners
            );


        // index 1 = top-left.
        Vector3 localTopLeft =
            placedPiecesContainer
                .InverseTransformPoint(
                    corners[1]
                );


        float step =
            cellSize +
            spacing;


        int x =
            Mathf.RoundToInt(
                localTopLeft.x /
                step
            );


        int y =
            Mathf.RoundToInt(
                -localTopLeft.y /
                step
            );


        Vector2Int origin =
            new Vector2Int(
                x,
                y
            );


        if (
            !CanPlacePiece(
                piece,
                origin
            )
        )
        {
            return false;
        }


        PlacePieceAt(
            piece,
            origin
        );


        return true;
    }


    // =====================================================
    // CAN PLACE
    // =====================================================

    private bool CanPlacePiece(
        LevelBuilderPieceInstance piece,
        Vector2Int origin
    )
    {
        if (
            piece == null ||
            piece.PieceView == null ||
            piece.PieceView.CurrentCells ==
                null
        )
        {
            return false;
        }


        foreach (
            Vector2Int localCell
            in piece.PieceView
                .CurrentCells
        )
        {
            Vector2Int coordinate =
                origin +
                localCell;


            if (
                coordinate.x < 0 ||
                coordinate.x >= columns ||
                coordinate.y < 0 ||
                coordinate.y >= rows
            )
            {
                return false;
            }


            if (
                blockedCells.Contains(
                    coordinate
                )
            )
            {
                return false;
            }


            if (
                occupiedCells.ContainsKey(
                    coordinate
                )
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // PLACE
    // =====================================================

    public bool PlacePieceAt(
        LevelBuilderPieceInstance piece,
        Vector2Int origin
    )
    {
        if (
            !CanPlacePiece(
                piece,
                origin
            )
        )
        {
            return false;
        }


        piece.transform.SetParent(
            placedPiecesContainer,
            false
        );


        RectTransform rect =
            piece.RectTransform;


        SetupTopLeftChild(
            rect
        );


        float step =
            cellSize +
            spacing;


        rect.anchoredPosition =
            new Vector2(
                origin.x *
                step,

                -origin.y *
                step
            );


        RefreshPieceLayout(
            piece
        );


        foreach (
            Vector2Int localCell
            in piece.PieceView
                .CurrentCells
        )
        {
            Vector2Int coordinate =
                origin +
                localCell;


            occupiedCells[
                coordinate
            ] =
                piece;
        }


        if (
            !placedPieces.Contains(
                piece
            )
        )
        {
            placedPieces.Add(
                piece
            );
        }


        piece.SetPlaced(
            origin
        );


        GenerateHintsInternal();


        return true;
    }


    // =====================================================
    // RELEASE
    // =====================================================

    public void ReleasePiece(
        LevelBuilderPieceInstance piece
    )
    {
        if (
            piece == null ||
            !piece.IsPlaced
        )
        {
            return;
        }


        List<Vector2Int> remove =
            new List<Vector2Int>();


        foreach (
            KeyValuePair<
                Vector2Int,
                LevelBuilderPieceInstance
            > pair
            in occupiedCells
        )
        {
            if (pair.Value == piece)
            {
                remove.Add(
                    pair.Key
                );
            }
        }


        foreach (
            Vector2Int coordinate
            in remove
        )
        {
            occupiedCells.Remove(
                coordinate
            );
        }


        piece.SetReleased();


        GenerateHintsInternal();
    }


    // =====================================================
    // ROTATE
    // =====================================================

    public void TryRotatePlacedPiece(
        LevelBuilderPieceInstance piece
    )
    {
        if (
            piece == null ||
            !piece.IsPlaced
        )
        {
            return;
        }


        Vector2Int oldOrigin =
            piece.Origin;


        int oldRotation =
            piece.RotationSteps;


        ReleasePiece(
            piece
        );


        piece.PieceView
            .RotateClockwise();


        RefreshPieceLayout(
            piece
        );


        if (
            PlacePieceAt(
                piece,
                oldOrigin
            )
        )
        {
            return;
        }


        piece.SetRotationSteps(
            oldRotation
        );


        PlacePieceAt(
            piece,
            oldOrigin
        );


        SetStatus(
            "Поворот невозможен: " +
            "фигура вышла за поле, " +
            "пересеклась или попала " +
            "на Blocked Cell."
        );
    }


    // =====================================================
    // DELETE
    // =====================================================

    public void DeletePiece(
        LevelBuilderPieceInstance piece
    )
    {
        if (piece == null)
        {
            return;
        }


        ReleasePiece(
            piece
        );


        placedPieces.Remove(
            piece
        );


        Destroy(
            piece.gameObject
        );


        GenerateHintsInternal();


        SetStatus(
            "Фигура удалена."
        );
    }


    // =====================================================
    // BLOCKED MODE
    // =====================================================

    public void ToggleBlockedMode()
    {
        blockedMode =
            !blockedMode;


        if (
            blockedModeButtonText != null
        )
        {
            blockedModeButtonText.text =
                blockedMode
                    ? "BLOCKED: ON"
                    : "BLOCKED: OFF";
        }


        SetStatus(
            blockedMode
                ? "Режим Blocked включён. " +
                  "Кликай по клеткам."
                : "Режим Blocked выключен."
        );
    }


    public void HandleCellClick(
        Vector2Int coordinate
    )
    {
        if (!blockedMode)
        {
            return;
        }


        if (
            occupiedCells.ContainsKey(
                coordinate
            )
        )
        {
            SetStatus(
                "Эта клетка занята фигурой."
            );

            return;
        }


        if (
            blockedCells.Contains(
                coordinate
            )
        )
        {
            blockedCells.Remove(
                coordinate
            );
        }
        else
        {
            blockedCells.Add(
                coordinate
            );
        }


        if (
            cellViews.TryGetValue(
                coordinate,
                out LevelBuilderCell view
            )
        )
        {
            view.SetBlocked(
                blockedCells.Contains(
                    coordinate
                )
            );
        }


        GenerateHintsInternal();
    }


    // =====================================================
    // HINTS
    // =====================================================

    public void GenerateHints()
    {
        GenerateHintsInternal();


        SetStatus(
            "Hints пересчитаны."
        );
    }


    private void GenerateHintsInternal()
    {
        rowTargets =
            new int[
                rows
            ];


        columnTargets =
            new int[
                columns
            ];


        foreach (
            Vector2Int coordinate
            in occupiedCells.Keys
        )
        {
            rowTargets[
                coordinate.y
            ]++;


            columnTargets[
                coordinate.x
            ]++;
        }


        for (
            int y = 0;
            y < rowHintTexts.Count;
            y++
        )
        {
            rowHintTexts[y].text =
                rowTargets[y]
                    .ToString();
        }


        for (
            int x = 0;
            x <
                columnHintTexts.Count;
            x++
        )
        {
            columnHintTexts[x].text =
                columnTargets[x]
                    .ToString();
        }
    }


    // =====================================================
    // VALIDATE
    // =====================================================

    public void ValidateCurrentLevel()
    {
#if UNITY_EDITOR

        LevelData temporary =
            BuildTemporaryLevel();


        if (temporary == null)
        {
            return;
        }


        if (
            temporary.IsValid(
                out string error
            )
        )
        {
            SetStatus(
                "✓ Уровень корректен."
            );
        }
        else
        {
            SetStatus(
                "✕ " +
                error
            );
        }


        DestroyImmediate(
            temporary
        );

#else

        SetStatus(
            "LevelBuilder доступен " +
            "только в Unity Editor."
        );

#endif
    }


    // =====================================================
    // SOLUTION COUNT
    // =====================================================

    public void CountSolutions()
    {
#if UNITY_EDITOR

        LevelData temporary =
            BuildTemporaryLevel();


        if (temporary == null)
        {
            return;
        }


        if (
            !temporary.IsValid(
                out string error
            )
        )
        {
            SetStatus(
                "Сначала исправь уровень:\n" +
                error
            );


            DestroyImmediate(
                temporary
            );

            return;
        }


        int count =
            LevelSolver
                .CountUniqueSolutions(
                    temporary,
                    2
                );


        if (count == 0)
        {
            SetStatus(
                "✕ Решений нет."
            );
        }
        else if (count == 1)
        {
            SetStatus(
                "✓ Ровно одно решение."
            );
        }
        else
        {
            SetStatus(
                "⚠ Найдено несколько решений " +
                "(минимум 2)."
            );
        }


        DestroyImmediate(
            temporary
        );

#endif
    }


    // =====================================================
    // BUILD TEMP LEVEL
    // =====================================================

#if UNITY_EDITOR

    private LevelData BuildTemporaryLevel()
    {
        GenerateHintsInternal();


        if (placedPieces.Count == 0)
        {
            SetStatus(
                "На поле нет фигур."
            );

            return null;
        }


        int levelNumber =
            Mathf.Max(
                1,
                ReadInt(
                    levelNumberInput,
                    1
                )
            );


        LevelData level =
            ScriptableObject
                .CreateInstance<
                    LevelData
                >();


        PopulateLevelData(
            level,
            levelNumber
        );


        return level;
    }


    private void PopulateLevelData(
        LevelData target,
        int levelNumber
    )
    {
        SerializedObject serialized =
            new SerializedObject(
                target
            );


        serialized
            .FindProperty(
                "levelNumber"
            )
            .intValue =
                levelNumber;


        serialized
            .FindProperty(
                "displayName"
            )
            .stringValue =
                $"УРОВЕНЬ " +
                $"{levelNumber}";


        serialized
            .FindProperty(
                "columns"
            )
            .intValue =
                columns;


        serialized
            .FindProperty(
                "rows"
            )
            .intValue =
                rows;


        // =================================================
        // PIECES
        // =================================================

        SerializedProperty pieces =
            serialized.FindProperty(
                "pieces"
            );


        pieces.arraySize =
            placedPieces.Count;


        for (
            int i = 0;
            i < placedPieces.Count;
            i++
        )
        {
            pieces
                .GetArrayElementAtIndex(
                    i
                )
                .objectReferenceValue =
                    placedPieces[
                        i
                    ].Definition;
        }


        // =================================================
        // HINTS
        // =================================================

        SerializedProperty rowsProperty =
            serialized.FindProperty(
                "rowTargets"
            );


        rowsProperty.arraySize =
            rows;


        for (
            int y = 0;
            y < rows;
            y++
        )
        {
            rowsProperty
                .GetArrayElementAtIndex(
                    y
                )
                .intValue =
                    rowTargets[y];
        }


        SerializedProperty columnsProperty =
            serialized.FindProperty(
                "columnTargets"
            );


        columnsProperty.arraySize =
            columns;


        for (
            int x = 0;
            x < columns;
            x++
        )
        {
            columnsProperty
                .GetArrayElementAtIndex(
                    x
                )
                .intValue =
                    columnTargets[x];
        }


        // =================================================
        // BLOCKED
        // =================================================

        SerializedProperty blocked =
            serialized.FindProperty(
                "blockedCells"
            );


        blocked.arraySize =
            blockedCells.Count;


        int blockedIndex =
            0;


        foreach (
            Vector2Int coordinate
            in blockedCells
        )
        {
            blocked
                .GetArrayElementAtIndex(
                    blockedIndex
                )
                .vector2IntValue =
                    coordinate;


            blockedIndex++;
        }


        // =================================================
        // SOLUTION
        // =================================================

        SerializedProperty solution =
            serialized.FindProperty(
                "solution"
            );


        solution.arraySize =
            placedPieces.Count;


        for (
            int i = 0;
            i < placedPieces.Count;
            i++
        )
        {
            LevelBuilderPieceInstance piece =
                placedPieces[i];


            SerializedProperty entry =
                solution
                    .GetArrayElementAtIndex(
                        i
                    );


            entry
                .FindPropertyRelative(
                    "pieceIndex"
                )
                .intValue =
                    i;


            entry
                .FindPropertyRelative(
                    "origin"
                )
                .vector2IntValue =
                    piece.Origin;


            entry
                .FindPropertyRelative(
                    "rotationSteps"
                )
                .intValue =
                    piece.RotationSteps;
        }


        serialized
            .ApplyModifiedPropertiesWithoutUndo();
    }

#endif


    // =====================================================
    // SAVE
    // =====================================================

    public void SaveLevel()
    {
#if UNITY_EDITOR

        LevelData temporary =
            BuildTemporaryLevel();


        if (temporary == null)
        {
            return;
        }


        if (
            !temporary.IsValid(
                out string error
            )
        )
        {
            SetStatus(
                "Нельзя сохранить:\n" +
                error
            );


            DestroyImmediate(
                temporary
            );

            return;
        }


        int levelNumber =
            temporary.LevelNumber;


        string path =
            $"Assets/Data/Levels/" +
            $"Level_{levelNumber:000}.asset";


        LevelData asset =
            AssetDatabase
                .LoadAssetAtPath<
                    LevelData
                >(
                    path
                );


        if (asset == null)
        {
            asset =
                ScriptableObject
                    .CreateInstance<
                        LevelData
                    >();


            PopulateLevelData(
                asset,
                levelNumber
            );


            AssetDatabase.CreateAsset(
                asset,
                path
            );
        }
        else
        {
            bool overwrite =
                EditorUtility.DisplayDialog(
                    "Level already exists",
                    $"Level_{levelNumber:000} " +
                    "уже существует.\n" +
                    "Перезаписать?",
                    "Да",
                    "Нет"
                );


            if (!overwrite)
            {
                DestroyImmediate(
                    temporary
                );

                return;
            }


            PopulateLevelData(
                asset,
                levelNumber
            );


            EditorUtility.SetDirty(
                asset
            );
        }


        AddLevelToDatabase(
            asset,
            levelNumber
        );


        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();


        DestroyImmediate(
            temporary
        );


        SetStatus(
            $"✓ Level_{levelNumber:000} " +
            "сохранён."
        );

#endif
    }


    private void AddLevelToDatabase(
        LevelData level,
        int levelNumber
    )
    {
#if UNITY_EDITOR

        if (levelDatabase == null)
        {
            return;
        }


        SerializedObject databaseObject =
            new SerializedObject(
                levelDatabase
            );


        SerializedProperty levels =
            databaseObject
                .FindProperty(
                    "levels"
                );


        int targetIndex =
            levelNumber - 1;


        if (
            targetIndex >
            levels.arraySize
        )
        {
            SetStatus(
                "Нельзя добавить уровень " +
                "с пропуском номеров."
            );

            return;
        }


        if (
            targetIndex ==
            levels.arraySize
        )
        {
            levels.InsertArrayElementAtIndex(
                targetIndex
            );
        }


        levels
            .GetArrayElementAtIndex(
                targetIndex
            )
            .objectReferenceValue =
                level;


        databaseObject
            .ApplyModifiedProperties();


        EditorUtility.SetDirty(
            levelDatabase
        );

#endif
    }


    // =====================================================
    // LOAD
    // =====================================================

    public void LoadLevel()
    {
        if (levelDatabase == null)
        {
            SetStatus(
                "LevelDatabase не назначена."
            );

            return;
        }


        int levelNumber =
            Mathf.Max(
                1,
                ReadInt(
                    levelNumberInput,
                    1
                )
            );


        LevelData level =
            levelDatabase
                .GetLevelByNumber(
                    levelNumber
                );


        if (level == null)
        {
            SetStatus(
                $"Level {levelNumber} " +
                "не найден."
            );

            return;
        }


        columns =
            level.Columns;


        rows =
            level.Rows;


        if (columnsInput != null)
        {
            columnsInput.text =
                columns.ToString();
        }


        if (rowsInput != null)
        {
            rowsInput.text =
                rows.ToString();
        }


        ClearCurrentLevel();

        BuildGridVisual();


        if (level.BlockedCells != null)
        {
            foreach (
                Vector2Int blocked
                in level.BlockedCells
            )
            {
                blockedCells.Add(
                    blocked
                );


                if (
                    cellViews.TryGetValue(
                        blocked,
                        out LevelBuilderCell view
                    )
                )
                {
                    view.SetBlocked(
                        true
                    );
                }
            }
        }


        for (
            int pieceIndex = 0;
            pieceIndex <
                level.Pieces.Length;
            pieceIndex++
        )
        {
            LevelSolutionEntry entry =
                level
                    .GetSolutionEntryByPieceIndex(
                        pieceIndex
                    );


            if (entry == null)
            {
                continue;
            }


            LevelBuilderPieceInstance piece =
                Instantiate(
                    pieceInstancePrefab,
                    placedPiecesContainer
                );


            piece.Initialize(
                this,
                level.Pieces[
                    pieceIndex
                ]
            );


            piece.SetRotationSteps(
                entry.RotationSteps
            );


            PlacePieceAt(
                piece,
                entry.Origin
            );
        }


        GenerateHintsInternal();


        SetStatus(
            $"✓ Level {levelNumber} загружен."
        );
    }


    // =====================================================
    // CLEAR
    // =====================================================

    public void ClearLevel()
    {
        ClearCurrentLevel();


        BuildGridVisual();


        GenerateHintsInternal();


        SetStatus(
            "Поле очищено."
        );
    }


    private void ClearCurrentLevel()
    {
        occupiedCells.Clear();

        blockedCells.Clear();

        placedPieces.Clear();


        ClearChildren(
            placedPiecesContainer
        );
    }


    private void ClearChildren(
        Transform parent
    )
    {
        if (parent == null)
        {
            return;
        }


        for (
            int i =
                parent.childCount - 1;

            i >= 0;

            i--
        )
        {
            Destroy(
                parent
                    .GetChild(i)
                    .gameObject
            );
        }
    }


    // =====================================================
    // STATUS
    // =====================================================

    private void SetStatus(
        string message
    )
    {
        if (statusText != null)
        {
            statusText.text =
                message;
        }


        Debug.Log(
            "LevelBuilder: " +
            message
        );
    }
}