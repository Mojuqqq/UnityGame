using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Prefab")]

    [SerializeField]
    private PieceView pieceViewPrefab;


    [Header("Containers")]

    [SerializeField]
    private RectTransform
        piecesContainer;

    [SerializeField]
    private RectTransform
        piecePanel;

    [SerializeField]
    private RectTransform
        dragLayer;


    [Header("Gameplay")]

    [SerializeField]
    private GridManager gridManager;


    [Header("Layout")]

    [SerializeField]
    private float topPadding = 20f;

    [SerializeField]
    private float verticalSpacing = 35f;


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


        float currentY =
            topPadding;


        for (
            int i = 0;
            i < definitions.Length;
            i++
        )
        {
            PieceDefinition definition =
                definitions[i];


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
                $"Piece_{i}_{definition.PieceId}";


            piece.Initialize(
                definition
            );


            PieceDragHandler
                dragHandler =
                    piece.GetComponent<
                        PieceDragHandler
                    >();


            if (dragHandler == null)
            {
                Debug.LogError(
                    $"{piece.name} has no PieceDragHandler."
                );
            }
            else
            {
                dragHandler.Configure(
                    gridManager,
                    dragLayer,
                    piecePanel
                );
            }


            RectTransform pieceRect =
                piece.GetComponent<
                    RectTransform
                >();


            pieceRect.anchorMin =
                new Vector2(
                    0.5f,
                    1f
                );

            pieceRect.anchorMax =
                new Vector2(
                    0.5f,
                    1f
                );

            pieceRect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );


            pieceRect.anchoredPosition =
                new Vector2(
                    0f,
                    -currentY
                );


            currentY +=
                pieceRect.rect.height +
                verticalSpacing;


            spawnedPieces.Add(
                piece
            );
        }
    }


    public void ClearPieces()
    {
        foreach (
            PieceView piece
            in spawnedPieces
        )
        {
            if (piece != null)
            {
                Destroy(
                    piece.gameObject
                );
            }
        }


        spawnedPieces.Clear();


        if (piecesContainer == null)
        {
            return;
        }


        for (
            int i =
                piecesContainer
                    .childCount - 1;
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