using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PieceRotationZone :
    MonoBehaviour,
    IPointerDownHandler
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
    private float inactiveAlpha = 0.25f;


    [SerializeField]
    [Range(0f, 1f)]
    private float activeAlpha = 0.65f;


    [SerializeField]
    [Range(0f, 1f)]
    private float hoverAlpha = 1f;


    [Header("Rotation")]

    [Tooltip(
        "Через сколько секунд нахождения " +
        "в зоне происходит следующий поворот."
    )]
    [SerializeField]
    [Range(0.5f, 0.8f)]
    private float hoverRotationInterval = 0.65f;


    private RectTransform rectTransform;


    private PieceDragHandler activePiece;


    private Vector2 trackedPointerPosition;

    private Camera trackedEventCamera;


    private bool pointerInside;

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
            hoverTimer =
                0f;


            pointerInside =
                false;


            UpdateVisual();


            return;
        }


        // Даже если курсор перестал двигаться,
        // таймер продолжает работать.
        UpdatePointerInsideState();


        if (!pointerInside)
        {
            hoverTimer =
                0f;

            return;
        }


        hoverTimer +=
            Time.unscaledDeltaTime;


        if (
            hoverTimer <
            hoverRotationInterval
        )
        {
            return;
        }


        hoverTimer =
            0f;


        activePiece
            .RotateFromExternalControl();
    }


    // =====================================================
    // TRACK DRAGGED PIECE
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


        // Только что вошли в зону —
        // начинаем новый интервал.
        if (
            pointerInside &&
            !wasInside
        )
        {
            hoverTimer =
                0f;
        }


        // Вышли из зоны —
        // тоже сбрасываем таймер.
        if (
            !pointerInside &&
            wasInside
        )
        {
            hoverTimer =
                0f;
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


        pointerInside =
            false;


        hoverTimer =
            0f;


        UpdateVisual();
    }


    // =====================================================
    // CLICK / TAP
    // =====================================================

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (
            activePiece == null ||
            !activePiece.IsCurrentlyDragging
        )
        {
            return;
        }


        // Нажатие = один моментальный поворот на 90°.
        activePiece
            .RotateFromExternalControl();


        // После ручного поворота начинаем
        // отсчёт hover заново.
        hoverTimer =
            0f;
    }


    // =====================================================
    // POINTER INSIDE
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