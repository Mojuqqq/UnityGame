using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


[RequireComponent(
    typeof(CanvasGroup)
)]
[RequireComponent(
    typeof(PieceView)
)]
public class LevelBuilderPieceInstance :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    
    private PieceView pieceView;


    private LevelBuilderManager manager;

    private RectTransform rectTransform;

    private CanvasGroup canvasGroup;


    // =====================================================
    // PLACEMENT
    // =====================================================

    private bool isPlaced;

    private Vector2Int origin;


    public bool IsPlaced =>
        isPlaced;


    public Vector2Int Origin =>
        origin;


    public PieceDefinition Definition =>
        pieceView != null
            ? pieceView.Definition
            : null;


    public int RotationSteps =>
        pieceView != null
            ? pieceView.RotationSteps
            : 0;


    public PieceView PieceView =>
        pieceView;


    public RectTransform RectTransform =>
        rectTransform;


    // =====================================================
    // DRAG
    // =====================================================

    private bool isDragging;


    private bool hadPlacementBeforeDrag;

    private Vector2Int previousOrigin;

    private int previousRotationSteps;


    private Vector3 pointerOffsetWorld;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
{
    rectTransform =
        GetComponent<RectTransform>();


    canvasGroup =
        GetComponent<CanvasGroup>();


    pieceView =
        GetComponent<PieceView>();


    if (pieceView == null)
    {
        Debug.LogError(
            "LevelBuilderPieceInstance: " +
            "PieceView component is missing."
        );
    }
}


    private void Update()
    {
        if (
            !isDragging ||
            pieceView == null
        )
        {
            return;
        }


        if (
            Keyboard.current != null &&
            Keyboard.current
                .rKey
                .wasPressedThisFrame
        )
        {
            pieceView
                .RotateClockwise();


            manager?
                .RefreshPieceLayout(
                    this
                );
        }
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
    LevelBuilderManager newManager,
    PieceDefinition definition
)
{
    manager =
        newManager;


    if (pieceView == null)
    {
        pieceView =
            GetComponent<PieceView>();
    }


    if (pieceView == null)
    {
        Debug.LogError(
            "LevelBuilderPieceInstance: " +
            "PieceView is missing."
        );

        return;
    }


    if (definition == null)
    {
        Debug.LogError(
            "LevelBuilderPieceInstance: " +
            "PieceDefinition is null."
        );

        return;
    }


    pieceView.Initialize(
        definition
    );


    manager?
        .RefreshPieceLayout(
            this
        );
}


    // =====================================================
    // ROTATION
    // =====================================================

    public void SetRotationSteps(
        int rotationSteps
    )
    {
        if (pieceView == null)
        {
            return;
        }


        pieceView.SetRotationSteps(
            rotationSteps
        );


        manager?
            .RefreshPieceLayout(
                this
            );
    }


    // =====================================================
    // PLACED STATE
    // =====================================================

    public void SetPlaced(
        Vector2Int newOrigin
    )
    {
        origin =
            newOrigin;


        isPlaced =
            true;


        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts =
                true;


            canvasGroup.alpha =
                1f;
        }
    }


    public void SetReleased()
    {
        isPlaced =
            false;
    }


    // =====================================================
    // POINTER CLICK
    // =====================================================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (
            manager == null ||
            isDragging
        )
        {
            return;
        }


        if (
            eventData.button ==
            PointerEventData
                .InputButton
                .Right
        )
        {
            manager.DeletePiece(
                this
            );

            return;
        }


        if (
            eventData.button ==
                PointerEventData
                    .InputButton
                    .Left
            &&
            isPlaced
        )
        {
            manager.TryRotatePlacedPiece(
                this
            );
        }
    }


    // =====================================================
    // NORMAL DRAG
    // =====================================================

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        BeginDragInternal(
            eventData
        );
    }


    public void OnDrag(
        PointerEventData eventData
    )
    {
        DragInternal(
            eventData
        );
    }


    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        EndDragInternal(
            eventData
        );
    }


    // =====================================================
    // PALETTE FORWARDED DRAG
    // =====================================================

    public void BeginExternalDrag(
        PointerEventData eventData
    )
    {
        BeginDragInternal(
            eventData,
            true
        );
    }


    public void ExternalDrag(
        PointerEventData eventData
    )
    {
        DragInternal(
            eventData
        );
    }


    public void EndExternalDrag(
        PointerEventData eventData
    )
    {
        EndDragInternal(
            eventData
        );
    }


    // =====================================================
    // BEGIN
    // =====================================================

    private void BeginDragInternal(
        PointerEventData eventData,
        bool fromPalette = false
    )
    {
        if (
            manager == null ||
            rectTransform == null
        )
        {
            return;
        }


        if (isDragging)
        {
            return;
        }


        hadPlacementBeforeDrag =
            isPlaced;


        if (isPlaced)
        {
            previousOrigin =
                origin;


            previousRotationSteps =
                RotationSteps;


            manager.ReleasePiece(
                this
            );
        }


        transform.SetParent(
            manager.DragLayer,
            true
        );


        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts =
                false;


            canvasGroup.alpha =
                0.75f;
        }


        isDragging =
            true;


        if (
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    manager.DragLayer,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector3 pointerWorld
                )
        )
        {
            if (fromPalette)
            {
                pointerOffsetWorld =
                    Vector3.zero;


                rectTransform.position =
                    pointerWorld;
            }
            else
            {
                pointerOffsetWorld =
                    rectTransform.position -
                    pointerWorld;
            }
        }
    }


    // =====================================================
    // DRAG
    // =====================================================

    private void DragInternal(
        PointerEventData eventData
    )
    {
        if (!isDragging)
        {
            return;
        }


        if (
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    manager.DragLayer,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector3 pointerWorld
                )
        )
        {
            rectTransform.position =
                pointerWorld +
                pointerOffsetWorld;
        }
    }


    // =====================================================
    // END
    // =====================================================

    private void EndDragInternal(
        PointerEventData eventData
    )
    {
        if (!isDragging)
        {
            return;
        }


        isDragging =
            false;


        bool placed =
            manager.TryDropPiece(
                this
            );


        if (placed)
        {
            return;
        }


        if (hadPlacementBeforeDrag)
        {
            SetRotationSteps(
                previousRotationSteps
            );


            manager.PlacePieceAt(
                this,
                previousOrigin
            );


            return;
        }


        Destroy(
            gameObject
        );
    }
}