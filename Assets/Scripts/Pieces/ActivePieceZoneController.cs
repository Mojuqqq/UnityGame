using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(RectTransform))]
public class ActivePieceZoneController :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RectTransform pieceContainer;

    [SerializeField]
    private PieceSpawner pieceSpawner;


    [Header("Piece Size")]

    [SerializeField]
    [Min(0f)]
    private float innerPadding =
        18f;

    [SerializeField]
    [Min(1f)]
    private float minCellSize =
        28f;

    [SerializeField]
    [Min(1f)]
    private float maxCellSize =
        72f;

    [SerializeField]
    [Range(0f, 0.3f)]
    private float spacingRatio =
        0.10f;


    private RectTransform zoneRect;

    private PieceDragHandler activePiece;


    public PieceDragHandler ActivePiece =>
        activePiece;


    private void Awake()
    {
        zoneRect =
            GetComponent<RectTransform>();


        if (pieceContainer == null)
        {
            pieceContainer =
                zoneRect;
        }
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        PieceSpawner spawner
    )
    {
        if (spawner != null)
        {
            pieceSpawner =
                spawner;
        }


        if (pieceSpawner == null)
        {
            Debug.LogError(
                "ActivePieceZoneController: " +
                "PieceSpawner is not assigned."
            );

            return;
        }


        Canvas.ForceUpdateCanvases();


        // ВАЖНО:
        // сохраняем исходное место каждой фигуры
        // в PiecePanel ДО того, как вынесем одну из них.
        foreach (
            PieceView pieceView
            in pieceSpawner.SpawnedPieces
        )
        {
            if (pieceView == null)
            {
                continue;
            }


            PieceDragHandler handler =
                pieceView.GetComponent<
                    PieceDragHandler
                >();


            if (handler == null)
            {
                continue;
            }


            handler
                .CaptureCurrentAsPanelHome();


            handler
                .SetActivePieceZoneController(
                    this
                );
        }


        SelectRandomPanelPiece();
    }


    // =====================================================
    // STATE
    // =====================================================

    public bool IsActivePiece(
        PieceDragHandler piece
    )
    {
        return
            piece != null
            &&
            activePiece == piece;
    }


    public bool ContainsScreenPoint(
        Vector2 screenPosition,
        Camera eventCamera
    )
    {
        if (zoneRect == null)
        {
            return false;
        }


        return
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    zoneRect,
                    screenPosition,
                    eventCamera
                );
    }


    public bool IsPointerOverZone(
        PointerEventData eventData
    )
    {
        if (eventData == null)
        {
            return false;
        }


        return
            ContainsScreenPoint(
                eventData.position,
                eventData.pressEventCamera
            );
    }


    // =====================================================
    // CLICK
    // =====================================================

    public void HandlePieceClick(
        PieceDragHandler piece
    )
    {
        if (piece == null)
        {
            return;
        }


        // Активная фигура:
        // обычный click = повернуть на 90°.
        if (piece == activePiece)
        {
            piece.RotateWhileResting();

            FitActivePiece();

            return;
        }


        // Нажали фигуру из PiecePanel:
        // меняем местами с активной.
        if (IsPieceInPanel(piece))
        {
            ActivatePiece(
                piece
            );
        }
    }


    // =====================================================
    // DRAG DROP INTO ACTIVE ZONE
    // =====================================================

    public void AcceptDraggedPiece(
        PieceDragHandler piece
    )
    {
        if (piece == null)
        {
            return;
        }


        if (
            activePiece != null
            &&
            activePiece != piece
        )
        {
            activePiece
                .ReturnToPanelHomeImmediate();
        }


        activePiece =
            piece;


        PlaceInZone(
            activePiece
        );
    }


    // =====================================================
    // ACTIVE -> PANEL
    // =====================================================

    public void ReturnActiveToPanelAndSelectNext(
        PieceDragHandler piece
    )
    {
        if (piece == null)
        {
            return;
        }


        bool wasActive =
            piece == activePiece;


        piece.ReturnToPanelHomeImmediate();


        if (!wasActive)
        {
            return;
        }


        activePiece =
            null;


        // Не выбираем только что убранную фигуру,
        // если в панели есть другие.
        SelectRandomPanelPiece(
            piece
        );
    }


    // =====================================================
    // ACTIVE -> GRID
    // =====================================================

    public void NotifyPiecePlacedOnGrid(
        PieceDragHandler piece
    )
    {
        if (
            piece == null ||
            piece != activePiece
        )
        {
            return;
        }


        activePiece =
            null;


        // После успешного размещения
        // автоматически появляется следующая.
        SelectRandomPanelPiece();
    }


    // =====================================================
    // SWAP
    // =====================================================

    private void ActivatePiece(
        PieceDragHandler newPiece
    )
    {
        if (newPiece == null)
        {
            return;
        }


        if (newPiece == activePiece)
        {
            return;
        }


        if (activePiece != null)
        {
            activePiece
                .ReturnToPanelHomeImmediate();
        }


        activePiece =
            newPiece;


        PlaceInZone(
            activePiece
        );
    }


    // =====================================================
    // RANDOM
    // =====================================================

    private void SelectRandomPanelPiece(
        PieceDragHandler excludedPiece = null
    )
    {
        if (pieceSpawner == null)
        {
            return;
        }


        List<PieceDragHandler> candidates =
            new List<PieceDragHandler>();


        foreach (
            PieceView pieceView
            in pieceSpawner.SpawnedPieces
        )
        {
            if (pieceView == null)
            {
                continue;
            }


            PieceDragHandler handler =
                pieceView.GetComponent<
                    PieceDragHandler
                >();


            if (handler == null)
            {
                continue;
            }


            if (
                handler ==
                excludedPiece
            )
            {
                continue;
            }


            if (!IsPieceInPanel(handler))
            {
                continue;
            }


            candidates.Add(
                handler
            );
        }


        // Если других фигур нет,
        // разрешаем вернуть только что убранную.
        if (
            candidates.Count == 0
            &&
            excludedPiece != null
            &&
            IsPieceInPanel(
                excludedPiece
            )
        )
        {
            candidates.Add(
                excludedPiece
            );
        }


        if (candidates.Count == 0)
        {
            activePiece =
                null;

            return;
        }


        int index =
            Random.Range(
                0,
                candidates.Count
            );


        activePiece =
            candidates[index];


        PlaceInZone(
            activePiece
        );
    }


    private bool IsPieceInPanel(
        PieceDragHandler piece
    )
    {
        if (
            piece == null ||
            pieceSpawner == null ||
            pieceSpawner.PiecesContainer == null
        )
        {
            return false;
        }


        return
            piece.transform.parent
            ==
            pieceSpawner.PiecesContainer;
    }


    // =====================================================
    // ZONE LAYOUT
    // =====================================================

    public void FitActivePiece()
    {
        if (activePiece == null)
        {
            return;
        }


        PlaceInZone(
            activePiece
        );
    }


    private void PlaceInZone(
        PieceDragHandler piece
    )
    {
        if (
            piece == null ||
            pieceContainer == null
        )
        {
            return;
        }


        Canvas.ForceUpdateCanvases();


        PieceView pieceView =
            piece.GetComponent<
                PieceView
            >();


        if (
            pieceView == null ||
            pieceView.CurrentCells == null ||
            pieceView.CurrentCells.Length == 0
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
            in pieceView.CurrentCells
        )
        {
            maxX =
                Mathf.Max(
                    maxX,
                    cell.x
                );

            maxY =
                Mathf.Max(
                    maxY,
                    cell.y
                );
        }


        int widthInCells =
            maxX + 1;

        int heightInCells =
            maxY + 1;


        float availableWidth =
            Mathf.Max(
                1f,
                pieceContainer.rect.width
                -
                innerPadding * 2f
            );


        float availableHeight =
            Mathf.Max(
                1f,
                pieceContainer.rect.height
                -
                innerPadding * 2f
            );


        float initialCellSize =
            Mathf.Min(
                availableWidth /
                widthInCells,

                availableHeight /
                heightInCells,

                maxCellSize
            );


        float spacing =
            initialCellSize *
            spacingRatio;


        float widthCellSize =
            (
                availableWidth
                -
                spacing *
                (widthInCells - 1)
            )
            /
            widthInCells;


        float heightCellSize =
            (
                availableHeight
                -
                spacing *
                (heightInCells - 1)
            )
            /
            heightInCells;


        float finalCellSize =
            Mathf.Min(
                initialCellSize,
                widthCellSize,
                heightCellSize
            );


        finalCellSize =
            Mathf.Clamp(
                finalCellSize,
                minCellSize,
                maxCellSize
            );


        spacing =
            finalCellSize *
            spacingRatio;


        piece.MoveToActiveZone(
            pieceContainer,
            finalCellSize,
            spacing
        );
    }
}