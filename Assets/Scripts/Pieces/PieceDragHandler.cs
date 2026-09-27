using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class PieceDragHandler :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("References")]

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private RectTransform dragLayer;

    [SerializeField]
    private RectTransform piecePanel;


    private PieceView pieceView;

    private RectTransform rectTransform;

    private CanvasGroup canvasGroup;


    // Где фигура живёт,
    // когда находится в панели.

    private Transform homeParent;

    private int homeSiblingIndex;

    private Vector2
        homeAnchoredPosition;

    private float
        homeCellSize;

    private float
        homeSpacing;

    private bool
        homeCaptured;


    // Состояние на поле.

    private bool
        isPlaced;

    private Vector2Int
        placedOrigin;

    private List<Vector2Int>
        placedCoordinates =
            new List<Vector2Int>();


    // Запоминаем состояние перед
    // перемещением установленной фигуры.

    private bool
        wasPlacedAtDragStart;

    private Vector2Int
        previousOrigin;

    private int
        previousRotationSteps;


    // Текущее перетаскивание.

    private bool
        isDragging;

    private bool
        hasValidPlacement;

    private Vector2Int
        currentOrigin;

    private Vector2
        lastPointerPosition;

    private Camera
        lastEventCamera;

    public void Configure(
    GridManager newGridManager,
    RectTransform newDragLayer,
    RectTransform newPiecePanel
)
{
    gridManager =
        newGridManager;

    dragLayer =
        newDragLayer;

    piecePanel =
        newPiecePanel;
}

    private void Awake()
    {
        pieceView =
            GetComponent<PieceView>();

        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();
    }


    private void Start()
    {
        CaptureHomePosition();
    }


    private void Update()
    {
        if (!isDragging)
        {
            return;
        }

        if (
            Keyboard.current != null &&
            Keyboard
                .current
                .rKey
                .wasPressedThisFrame
        )
        {
            RotatePiece();
        }
    }


    private void CaptureHomePosition()
    {
        if (homeCaptured)
        {
            return;
        }

        homeParent =
            transform.parent;

        homeSiblingIndex =
            transform.GetSiblingIndex();

        homeAnchoredPosition =
            rectTransform
                .anchoredPosition;

        homeCellSize =
            pieceView.CellSize;

        homeSpacing =
            pieceView.Spacing;

        // Если PiecePanel забыли
        // назначить вручную,
        // попробуем найти его через
        // PiecesContainer.

        if (
            piecePanel == null &&
            homeParent != null &&
            homeParent.parent != null
        )
        {
            piecePanel =
                homeParent.parent
                    .GetComponent<
                        RectTransform
                    >();
        }

        homeCaptured =
            true;
    }


    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        if (
            pieceView == null ||
            pieceView.Definition == null ||
            gridManager == null ||
            dragLayer == null
        )
        {
            Debug.LogError(
                "PieceDragHandler: references are missing."
            );

            return;
        }

        CaptureHomePosition();

        isDragging =
            true;

        hasValidPlacement =
            false;


        lastPointerPosition =
            eventData.position;

        lastEventCamera =
            eventData.pressEventCamera;


        wasPlacedAtDragStart =
            isPlaced;


        if (isPlaced)
        {
            previousOrigin =
                placedOrigin;

            previousRotationSteps =
                pieceView.RotationSteps;

            gridManager.ClearCells(
                placedCoordinates
            );

            placedCoordinates.Clear();

            isPlaced =
                false;
        }


        transform.SetParent(
            dragLayer,
            true
        );

        transform.SetAsLastSibling();


        canvasGroup.blocksRaycasts =
            false;


        pieceView.SetLayout(
            gridManager.CellSize,
            gridManager.Spacing
        );


        MoveToPointer(
            eventData
        );


        UpdatePlacementPreview();
    }


    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (!isDragging)
        {
            return;
        }


        lastPointerPosition =
            eventData.position;

        lastEventCamera =
            eventData.pressEventCamera;


        MoveToPointer(
            eventData
        );


        UpdatePlacementPreview();
    }


    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (!isDragging)
        {
            return;
        }


        gridManager
            .ClearPlacementPreview();


        // 1. Если позиция на Grid допустима,
        // устанавливаем фигуру.

        if (hasValidPlacement)
        {
            bool placed =
                gridManager.PlacePiece(
                    pieceView.CurrentCells,
                    pieceView.Definition.Color,
                    currentOrigin,
                    out List<Vector2Int>
                        newCoordinates
                );


            if (placed)
            {
                placedOrigin =
                    currentOrigin;

                placedCoordinates =
                    newCoordinates;

                isPlaced =
                    true;


                SnapVisualToGrid(
                    placedOrigin
                );


                FinishDrag();

                return;
            }
        }


        // 2. Если отпустили над PiecePanel,
        // возвращаем фигуру в панель.

        if (
            IsPointerOverPiecePanel(
                eventData
            )
        )
        {
            ReturnToPiecePanel();

            FinishDrag();

            return;
        }


        // 3. Если фигура раньше уже
        // стояла на поле, а новое
        // размещение не удалось,
        // возвращаем старую позицию.

        if (wasPlacedAtDragStart)
        {
            RestorePreviousPlacement();

            FinishDrag();

            return;
        }


        // 4. Если фигура была взята
        // из PiecePanel и отпущена
        // где-то мимо — просто домой.

        ReturnToPiecePanel();

        FinishDrag();
    }


    private void RotatePiece()
    {
        if (
            !isDragging ||
            pieceView == null
        )
        {
            return;
        }

        pieceView
            .RotateClockwise();

        UpdatePlacementPreview();
    }


    private void MoveToPointer(
        PointerEventData eventData
    )
    {
        bool success =
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    dragLayer,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint
                );

        if (success)
        {
            rectTransform
                .anchoredPosition =
                    localPoint;
        }
    }


    private void UpdatePlacementPreview()
    {
        gridManager
            .ClearPlacementPreview();

        hasValidPlacement =
            false;


        bool hasCell =
            gridManager
                .TryGetCellUnderPointer(
                    lastPointerPosition,
                    lastEventCamera,
                    out GridCell hoveredCell
                );


        if (!hasCell)
        {
            return;
        }


        currentOrigin =
            new Vector2Int(
                hoveredCell.X,
                hoveredCell.Y
            );


        hasValidPlacement =
            gridManager
                .ShowPlacementPreview(
                    pieceView.CurrentCells,
                    currentOrigin
                );
    }


    private void SnapVisualToGrid(
        Vector2Int origin
    )
    {
        transform.SetParent(
            dragLayer,
            true
        );

        transform.SetAsLastSibling();


        pieceView.SetLayout(
            gridManager.CellSize,
            gridManager.Spacing
        );


        bool found =
            gridManager
                .TryGetCellTopLeftWorld(
                    origin.x,
                    origin.y,
                    out Vector3
                        targetTopLeft
                );


        if (!found)
        {
            return;
        }


        rectTransform
            .ForceUpdateRectTransforms();


        Vector3 currentTopLeft =
            rectTransform.TransformPoint(
                new Vector3(
                    rectTransform.rect.xMin,
                    rectTransform.rect.yMax,
                    0f
                )
            );


        Vector3 offset =
            targetTopLeft -
            currentTopLeft;


        rectTransform.position +=
            offset;
    }


    private bool IsPointerOverPiecePanel(
        PointerEventData eventData
    )
    {
        if (piecePanel == null)
        {
            return false;
        }


        return
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    piecePanel,
                    eventData.position,
                    eventData.pressEventCamera
                );
    }


    private void ReturnToPiecePanel()
    {
        transform.SetParent(
            homeParent,
            false
        );


        transform.SetSiblingIndex(
            homeSiblingIndex
        );


        pieceView.SetLayout(
            homeCellSize,
            homeSpacing
        );


        rectTransform
            .anchoredPosition =
                homeAnchoredPosition;


        placedCoordinates.Clear();

        isPlaced =
            false;
    }


    private void RestorePreviousPlacement()
    {
        pieceView.SetRotationSteps(
            previousRotationSteps
        );


        bool restored =
            gridManager.PlacePiece(
                pieceView.CurrentCells,
                pieceView.Definition.Color,
                previousOrigin,
                out List<Vector2Int>
                    restoredCoordinates
            );


        if (!restored)
        {
            Debug.LogError(
                "PieceDragHandler: could not restore previous placement."
            );

            ReturnToPiecePanel();

            return;
        }


        placedOrigin =
            previousOrigin;

        placedCoordinates =
            restoredCoordinates;

        isPlaced =
            true;


        SnapVisualToGrid(
            placedOrigin
        );
    }


    private void FinishDrag()
    {
        gridManager
            .ClearPlacementPreview();


        canvasGroup.blocksRaycasts =
            true;

        canvasGroup.interactable =
            true;


        hasValidPlacement =
            false;

        isDragging =
            false;

        wasPlacedAtDragStart =
            false;
    }
}