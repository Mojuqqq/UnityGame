using System.Collections.Generic;
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

    private RectTransform rectTransform;

    private readonly List<Image> spawnedCells =
        new List<Image>();

    private Vector2Int[] currentCells;

    private int rotationSteps = 0;

    public PieceDefinition Definition => definition;

    public float CellSize => cellSize;
    public float Spacing => spacing;

    public Vector2Int[] CurrentCells => currentCells;

    public int RotationSteps => rotationSteps;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (definition != null)
        {
            ResetRotation();
        }
    }

    public void Initialize(
        PieceDefinition newDefinition
    )
    {
        definition = newDefinition;

        ResetRotation();
    }

    public void ResetRotation()
    {
        rotationSteps = 0;

        RecalculateCurrentCells();

        BuildPiece();
    }

    public void RotateClockwise()
{
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
        rotationSteps = 0;
    }

    RecalculateCurrentCells();

    BuildPiece();
}


public void SetRotationSteps(
    int newRotationSteps
)
{
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
                    RotateCellClockwise(rotated);
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
            Vector2Int cell in currentCells
        )
        {
            if (cell.x < minX)
            {
                minX = cell.x;
            }

            if (cell.y < minY)
            {
                minY = cell.y;
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

        int maxX = 0;
        int maxY = 0;

        foreach (
            Vector2Int cell in currentCells
        )
        {
            if (cell.x > maxX)
            {
                maxX = cell.x;
            }

            if (cell.y > maxY)
            {
                maxY = cell.y;
            }
        }

        int widthInCells =
            maxX + 1;

        int heightInCells =
            maxY + 1;

        float step =
            cellSize + spacing;

        float pieceWidth =
            widthInCells * cellSize
            +
            (widthInCells - 1) * spacing;

        float pieceHeight =
            heightInCells * cellSize
            +
            (heightInCells - 1) * spacing;

        rectTransform.sizeDelta =
            new Vector2(
                pieceWidth,
                pieceHeight
            );

        foreach (
            Vector2Int cell in currentCells
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

            RectTransform cellRect =
                newCell.GetComponent<
                    RectTransform
                >();

            cellRect.anchorMin =
                new Vector2(0f, 1f);

            cellRect.anchorMax =
                new Vector2(0f, 1f);

            cellRect.pivot =
                new Vector2(0f, 1f);

            cellRect.sizeDelta =
                new Vector2(
                    cellSize,
                    cellSize
                );

            cellRect.anchoredPosition =
                new Vector2(
                    cell.x * step,
                    -cell.y * step
                );

            newCell.color =
                definition.Color;

            newCell.name =
                $"PieceCell_{cell.x}_{cell.y}";
        }
    }

    private void ClearPiece()
    {
        foreach (
            Image cell in spawnedCells
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
        cellSize =
            newCellSize;

        BuildPiece();
    }

    public void SetLayout(
        float newCellSize,
        float newSpacing
    )
    {
        cellSize =
            newCellSize;

        spacing =
            newSpacing;

        BuildPiece();
    }
}