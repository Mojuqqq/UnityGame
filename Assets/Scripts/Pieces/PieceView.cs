using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PieceView : MonoBehaviour
{
    [Header("Piece")]
    [SerializeField]
    private PieceDefinition definition;

    [Header("Visuals")]
    [SerializeField]
    private Image cellPrefab;

    [Header("Cell Layout")]
    [SerializeField]
    private float cellSize = 70f;

    [SerializeField]
    private float spacing = 6f;

    [Header("Rotation Animation")]

[SerializeField]
[Min(0.01f)]
private float rotationDuration = 0.16f;

[SerializeField]
[Min(0.01f)]
private float panelRotationDuration = 0.25f;

[SerializeField]
private AnimationCurve rotationCurve =
    AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

private Coroutine rotationRoutine;

private float visualRotationAngle;

private int rotationPivotCellIndex = -1;

private Vector2 rotationPivotPoint;

public bool IsRotating => rotationRoutine != null;

    private RectTransform rectTransform;

    private readonly List<Image> spawnedCells =
        new List<Image>();

    private Vector2Int[] currentCells;

    private int rotationSteps;

    private bool initialized;


    public PieceDefinition Definition =>
        definition;

    public float CellSize =>
        cellSize;

    public float Spacing =>
        spacing;

    public Vector2Int[] CurrentCells =>
        currentCells;

    public int RotationSteps =>
        rotationSteps;


    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();
    }


    private void Start()
    {
        if (
            !initialized &&
            definition != null
        )
        {
            ResetRotation();
        }
    }


    public void Initialize(
        PieceDefinition newDefinition
    )
    {
        definition =
            newDefinition;

        initialized =
            true;

        ResetRotation();
    }


    public void ResetRotation()
    {
        if (IsRotating)
    FinishRotationAnimation();

        rotationSteps =
            0;

        RecalculateCurrentCells();

        BuildPiece();
    }


    public void RotateClockwise()
    {
        if (IsRotating)
    FinishRotationAnimation();

        if (
            definition == null ||
            definition.Cells == null ||
            definition.Cells.Length == 0
        )
        {
            return;
        }

        rotationSteps++;

        if (rotationSteps >= 4)
        {
            rotationSteps =
                0;
        }

        RecalculateCurrentCells();

        BuildPiece();
    }


    public bool RotateClockwiseAnimated(
    int pivotCellIndex = -1,
    Vector2 pivotPoint = default
)
{
    if (
        IsRotating ||
        definition == null ||
        definition.Cells == null ||
        definition.Cells.Length == 0
    )
    {
        return false;
    }

    rotationSteps = (rotationSteps + 1) % 4;

    RecalculateCurrentCells();

    rotationPivotCellIndex = pivotCellIndex;
    rotationPivotPoint = pivotPoint;

    // Логика уже соответствует новой ориентации,
    // визуально начинаем с предыдущей.
    visualRotationAngle = 90f;

    if (spawnedCells.Count == currentCells.Length)
        ApplyLayout();
    else
        BuildPiece();

    float duration = pivotCellIndex < 0
    ? panelRotationDuration
    : rotationDuration;

rotationRoutine = StartCoroutine(
    AnimateRotation(duration)
);

    return true;
}


private IEnumerator AnimateRotation(float duration)
{
    float elapsed = 0f;

    duration = Mathf.Max(0.01f, duration);

    while (elapsed < duration)
    {
        yield return null;

        elapsed += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(
            elapsed / duration
        );

        float easedT = rotationCurve.Evaluate(t);

        visualRotationAngle = Mathf.Lerp(
            90f,
            0f,
            easedT
        );

        ApplyLayout();
    }

    visualRotationAngle = 0f;
    rotationPivotCellIndex = -1;
    rotationRoutine = null;

    ApplyLayout();
}


private void FinishRotationAnimation()
{
    if (rotationRoutine != null)
    {
        StopCoroutine(rotationRoutine);
        rotationRoutine = null;
    }

    visualRotationAngle = 0f;
    rotationPivotCellIndex = -1;

    ApplyLayout();
}


private void OnDisable()
{
    if (IsRotating)
        FinishRotationAnimation();
}


    public void SetRotationSteps(
        int newRotationSteps
    )
    {

        if (IsRotating)
    FinishRotationAnimation();

        rotationSteps =
            ((newRotationSteps % 4) + 4) % 4;

        RecalculateCurrentCells();

        BuildPiece();
    }


    private void RecalculateCurrentCells()
    {
        if (
            definition == null ||
            definition.Cells == null
        )
        {
            currentCells =
                new Vector2Int[0];

            return;
        }


        currentCells =
            new Vector2Int[
                definition.Cells.Length
            ];


        for (
            int i = 0;
            i < definition.Cells.Length;
            i++
        )
        {
            Vector2Int cell =
                definition.Cells[i];

            Vector2Int rotated =
                cell;


            for (
                int step = 0;
                step < rotationSteps;
                step++
            )
            {
                rotated =
                    RotateCellClockwise(
                        rotated
                    );
            }


            currentCells[i] =
                rotated;
        }


        NormalizeCells();
    }


    private Vector2Int RotateCellClockwise(
        Vector2Int cell
    )
    {
        return new Vector2Int(
            -cell.y,
            cell.x
        );
    }


    private void NormalizeCells()
    {
        if (
            currentCells == null ||
            currentCells.Length == 0
        )
        {
            return;
        }


        int minX =
            currentCells[0].x;

        int minY =
            currentCells[0].y;


        foreach (
            Vector2Int cell
            in currentCells
        )
        {
            if (cell.x < minX)
            {
                minX =
                    cell.x;
            }

            if (cell.y < minY)
            {
                minY =
                    cell.y;
            }
        }


        for (
            int i = 0;
            i < currentCells.Length;
            i++
        )
        {
            currentCells[i] =
                new Vector2Int(
                    currentCells[i].x - minX,
                    currentCells[i].y - minY
                );
        }
    }


    public void BuildPiece()
    {
        if (definition == null)
        {
            Debug.LogError(
                $"PieceView '{gameObject.name}': " +
                "PieceDefinition is not assigned."
            );

            return;
        }


        if (cellPrefab == null)
        {
            Debug.LogError(
                $"PieceView '{gameObject.name}': " +
                "Cell Prefab is not assigned."
            );

            return;
        }


        if (
            currentCells == null ||
            currentCells.Length == 0
        )
        {
            RecalculateCurrentCells();
        }


        ClearPiece();


        if (
            currentCells == null ||
            currentCells.Length == 0
        )
        {
            return;
        }


        foreach (
            Vector2Int cell
            in currentCells
        )
        {
            Image newCell =
                Instantiate(
                    cellPrefab,
                    transform
                );


            spawnedCells.Add(
                newCell
            );


            newCell.color =
                definition.Color;
        }


        ApplyLayout();
    }


    private void ApplyLayout()
    {
        if (
            currentCells == null ||
            currentCells.Length == 0 ||
            spawnedCells.Count !=
            currentCells.Length
        )
        {
            return;
        }


        int maxX =
            0;

        int maxY =
            0;


        foreach (
            Vector2Int cell
            in currentCells
        )
        {
            if (cell.x > maxX)
            {
                maxX =
                    cell.x;
            }

            if (cell.y > maxY)
            {
                maxY =
                    cell.y;
            }
        }


        int widthInCells =
            maxX + 1;

        int heightInCells =
            maxY + 1;


        float step =
            cellSize +
            spacing;


        float pieceWidth =
            widthInCells * cellSize +
            (widthInCells - 1) * spacing;


        float pieceHeight =
            heightInCells * cellSize +
            (heightInCells - 1) * spacing;


        rectTransform.sizeDelta =
            new Vector2(
                pieceWidth,
                pieceHeight
            );

            // По умолчанию вращаем вокруг центра фигуры.
Vector2 rotationPivot = new Vector2(
    pieceWidth * 0.5f,
    -pieceHeight * 0.5f
);

// Во время перетаскивания вращаем вокруг
// точки захвата фигуры.
if (
    rotationPivotCellIndex >= 0 &&
    rotationPivotCellIndex < currentCells.Length
)
{
    Vector2Int pivotCell =
        currentCells[rotationPivotCellIndex];

    rotationPivot = new Vector2(
        pivotCell.x * step +
        rotationPivotPoint.x * cellSize,

        -pivotCell.y * step -
        rotationPivotPoint.y * cellSize
    );
}

Quaternion visualRotation = Quaternion.Euler(
    0f,
    0f,
    visualRotationAngle
);


        for (
            int i = 0;
            i < currentCells.Length;
            i++
        )
        {
            Vector2Int cell =
                currentCells[i];


            Image image =
                spawnedCells[i];


            RectTransform cellRect =
                image.GetComponent<
                    RectTransform
                >();


            cellRect.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            cellRect.anchorMax =
                new Vector2(
                    0f,
                    1f
                );

            cellRect.pivot =
                new Vector2(
                    0f,
                    1f
                );


            cellRect.sizeDelta =
                new Vector2(
                    cellSize,
                    cellSize
                );


            Vector2 targetPosition = new Vector2(
    cell.x * step,
    -cell.y * step
);

Vector2 offset =
    targetPosition - rotationPivot;

Vector3 rotatedOffset =
    visualRotation * new Vector3(
        offset.x,
        offset.y,
        0f
    );

cellRect.anchoredPosition =
    rotationPivot + (Vector2)rotatedOffset;

cellRect.localRotation = visualRotation;


            image.color =
                definition.Color;


            image.name =
                $"PieceCell_{cell.x}_{cell.y}";
        }
    }


    private void ClearPiece()
    {
        foreach (
            Image cell
            in spawnedCells
        )
        {
            if (cell != null)
            {
                cell.gameObject
                    .SetActive(false);

                Destroy(
                    cell.gameObject
                );
            }
        }


        spawnedCells.Clear();
    }


    public void SetCellSize(
        float newCellSize
    )
    {
        SetLayout(
            newCellSize,
            spacing
        );
    }


    public void SetLayout(
        float newCellSize,
        float newSpacing
    )
    {
        cellSize =
            Mathf.Max(
                1f,
                newCellSize
            );

        spacing =
            Mathf.Max(
                0f,
                newSpacing
            );


        ApplyLayout();
    }

    public Vector2 GetPanelVisualSize()
{
    Vector2 size = rectTransform.rect.size;

    float radians =
        visualRotationAngle * Mathf.Deg2Rad;

    float cos = Mathf.Abs(Mathf.Cos(radians));
    float sin = Mathf.Abs(Mathf.Sin(radians));

    return new Vector2(
        size.x * cos + size.y * sin,
        size.x * sin + size.y * cos
    );
}
}