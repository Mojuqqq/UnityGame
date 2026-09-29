using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Prefab")]

    [SerializeField]
    private PieceView pieceViewPrefab;


    [Header("Containers")]

    [SerializeField]
    private RectTransform piecesContainer;

    [SerializeField]
    private RectTransform piecePanel;

    [SerializeField]
    private RectTransform dragLayer;


    [Header("Gameplay")]

    [SerializeField]
    private GridManager gridManager;


    [Header("Mobile Layout")]

    [SerializeField]
    private float sidePadding =
        20f;

    [SerializeField]
    private float topPadding =
        25f;

    [SerializeField]
    private float horizontalSpacing =
        40f;

    [SerializeField]
    private float verticalSpacing =
        35f;


    private readonly List<PieceView>
        spawnedPieces =
            new List<PieceView>();


    public void BuildPieces(
        PieceDefinition[] definitions
    )
    {
        ClearPieces();


        if (
            definitions == null ||
            definitions.Length == 0
        )
        {
            Debug.LogError(
                "PieceSpawner: No pieces supplied."
            );

            return;
        }


        if (pieceViewPrefab == null)
        {
            Debug.LogError(
                "PieceSpawner: PieceView Prefab is not assigned."
            );

            return;
        }


        if (piecesContainer == null)
        {
            Debug.LogError(
                "PieceSpawner: PiecesContainer is not assigned."
            );

            return;
        }


        foreach (
            PieceDefinition definition
            in definitions
        )
        {
            if (definition == null)
            {
                continue;
            }


            PieceView piece =
                Instantiate(
                    pieceViewPrefab,
                    piecesContainer
                );


            piece.name =
                $"Piece_{spawnedPieces.Count}_{definition.PieceId}";


            piece.Initialize(
                definition
            );


            PieceDragHandler dragHandler =
                piece.GetComponent<
                    PieceDragHandler
                >();


            if (dragHandler != null)
            {
                dragHandler.Configure(
                    gridManager,
                    dragLayer,
                    piecePanel
                );
            }
            else
            {
                Debug.LogError(
                    $"{piece.name} has no PieceDragHandler."
                );
            }


            spawnedPieces.Add(
                piece
            );
        }


        Canvas.ForceUpdateCanvases();


        LayoutPieces();
    }


    private void LayoutPieces()
    {
        if (
            spawnedPieces.Count == 0 ||
            piecesContainer == null
        )
        {
            return;
        }


        float availableWidth =
            piecesContainer.rect.width -
            sidePadding * 2f;


        List<List<PieceView>> rows =
            new List<List<PieceView>>();


        List<PieceView> currentRow =
            new List<PieceView>();


        float currentRowWidth =
            0f;


        foreach (
            PieceView piece
            in spawnedPieces
        )
        {
            RectTransform rect =
                piece.GetComponent<
                    RectTransform
                >();


            float width =
                rect.rect.width;


            float requiredWidth =
                currentRow.Count == 0
                    ? width
                    : currentRowWidth +
                      horizontalSpacing +
                      width;


            if (
                currentRow.Count > 0 &&
                requiredWidth >
                availableWidth
            )
            {
                rows.Add(
                    currentRow
                );


                currentRow =
                    new List<PieceView>();


                currentRowWidth =
                    0f;
            }


            if (currentRow.Count > 0)
            {
                currentRowWidth +=
                    horizontalSpacing;
            }


            currentRow.Add(
                piece
            );


            currentRowWidth +=
                width;
        }


        if (currentRow.Count > 0)
        {
            rows.Add(
                currentRow
            );
        }


        float currentY =
            piecesContainer.rect.height *
            0.5f -
            topPadding;


        foreach (
            List<PieceView> row
            in rows
        )
        {
            float rowWidth =
                0f;

            float rowHeight =
                0f;


            for (
                int i = 0;
                i < row.Count;
                i++
            )
            {
                RectTransform rect =
                    row[i].GetComponent<
                        RectTransform
                    >();


                rowWidth +=
                    rect.rect.width;


                if (i > 0)
                {
                    rowWidth +=
                        horizontalSpacing;
                }


                rowHeight =
                    Mathf.Max(
                        rowHeight,
                        rect.rect.height
                    );
            }


            float currentX =
                -rowWidth * 0.5f;


            foreach (
                PieceView piece
                in row
            )
            {
                RectTransform rect =
                    piece.GetComponent<
                        RectTransform
                    >();


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
                        0.5f,
                        0.5f
                    );


                float x =
                    currentX +
                    rect.rect.width *
                    0.5f;


                float y =
                    currentY -
                    rowHeight *
                    0.5f;


                rect.anchoredPosition =
                    new Vector2(
                        x,
                        y
                    );


                currentX +=
                    rect.rect.width +
                    horizontalSpacing;
            }


            currentY -=
                rowHeight +
                verticalSpacing;
        }
    }


    public void ClearPieces()
    {
        spawnedPieces.Clear();


        if (piecesContainer == null)
        {
            return;
        }


        for (
            int i =
                piecesContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                piecesContainer
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
}