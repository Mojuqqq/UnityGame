using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class PieceRotationZone :
    MonoBehaviour
{
    public static PieceRotationZone Instance
    {
        get;
        private set;
    }


    [Header("Visual")]

    [SerializeField]
    private CanvasGroup canvasGroup;


    [SerializeField]
    [Range(0f, 1f)]
    private float inactiveAlpha =
        0.35f;


    [SerializeField]
    [Range(0f, 1f)]
    private float activeAlpha =
        0.70f;


    [SerializeField]
    [Range(0f, 1f)]
    private float hoverAlpha =
        1f;


    [Header("Rotation")]

    [Tooltip(
        "Задержка перед первым автоматическим поворотом."
    )]
    [SerializeField]
    [Range(1f, 1.5f)]
    private float initialHoverDelay =
        1.2f;


    [Tooltip(
        "Интервал между следующими поворотами."
    )]
    [SerializeField]
    [Range(0.6f, 0.8f)]
    private float hoverRotationInterval =
        0.7f;


    private RectTransform rectTransform;

    private PieceDragHandler activePiece;


    private Vector2 trackedPointerPosition;

    private Camera trackedEventCamera;


    private bool pointerInside;

    private bool firstRotationDone;

    private float hoverTimer;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        Instance =
            this;


        rectTransform =
            GetComponent<RectTransform>();


        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }


        ResetHoverState();

        UpdateVisual();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }
    }


    private void Update()
    {
        if (
            activePiece == null ||
            !activePiece.IsCurrentlyDragging
        )
        {
            ResetHoverState();

            UpdateVisual();

            return;
        }


        UpdatePointerInsideState();


        if (!pointerInside)
        {
            hoverTimer =
                0f;

            firstRotationDone =
                false;

            return;
        }


        hoverTimer +=
            Time.unscaledDeltaTime;


        float requiredTime =
            firstRotationDone
                ? hoverRotationInterval
                : initialHoverDelay;


        if (
            hoverTimer <
            requiredTime
        )
        {
            return;
        }


        hoverTimer =
            0f;


        firstRotationDone =
            true;


        activePiece
            .RotateFromExternalControl();
    }


    // =====================================================
    // TRACK
    // =====================================================

    public void BeginTracking(
        PieceDragHandler piece,
        Vector2 pointerPosition,
        Camera eventCamera
    )
    {
        if (piece == null)
        {
            return;
        }


        activePiece =
            piece;


        trackedPointerPosition =
            pointerPosition;


        trackedEventCamera =
            eventCamera;


        hoverTimer =
            0f;

        firstRotationDone =
            false;


        UpdatePointerInsideState();

        UpdateVisual();
    }


    public void UpdateTrackingPointer(
        PieceDragHandler piece,
        Vector2 pointerPosition,
        Camera eventCamera
    )
    {
        if (
            piece == null ||
            piece != activePiece
        )
        {
            return;
        }


        trackedPointerPosition =
            pointerPosition;


        trackedEventCamera =
            eventCamera;


        bool wasInside =
            pointerInside;


        UpdatePointerInsideState();


        if (
            pointerInside &&
            !wasInside
        )
        {
            hoverTimer =
                0f;

            firstRotationDone =
                false;
        }


        if (
            !pointerInside &&
            wasInside
        )
        {
            hoverTimer =
                0f;

            firstRotationDone =
                false;
        }


        UpdateVisual();
    }


    public void EndTracking(
        PieceDragHandler piece
    )
    {
        if (
            piece != null &&
            activePiece != piece
        )
        {
            return;
        }


        activePiece =
            null;


        ResetHoverState();

        UpdateVisual();
    }


    // =====================================================
    // POINTER
    // =====================================================

    private void UpdatePointerInsideState()
    {
        if (rectTransform == null)
        {
            pointerInside =
                false;

            return;
        }


        pointerInside =
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    rectTransform,
                    trackedPointerPosition,
                    trackedEventCamera
                );


        UpdateVisual();
    }


    private void ResetHoverState()
    {
        pointerInside =
            false;

        hoverTimer =
            0f;

        firstRotationDone =
            false;
    }


    // =====================================================
    // VISUAL
    // =====================================================

    private void UpdateVisual()
    {
        if (canvasGroup == null)
        {
            return;
        }


        if (
            activePiece == null ||
            !activePiece.IsCurrentlyDragging
        )
        {
            canvasGroup.alpha =
                inactiveAlpha;

            return;
        }


        if (pointerInside)
        {
            canvasGroup.alpha =
                hoverAlpha;

            return;
        }


        canvasGroup.alpha =
            activeAlpha;
    }
}