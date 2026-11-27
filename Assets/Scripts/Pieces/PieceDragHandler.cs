using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CanvasGroup))]
public class PieceDragHandler :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
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


    [Header("Drag Visuals")]

    [SerializeField]
    [Range(0.1f, 1f)]
    private float dragAlpha = 0.68f;

    [SerializeField]
    [Min(0.01f)]
    private float pickupFadeDuration = 0.08f;


    [Header("Drop Animation")]

    [SerializeField]
    [Min(0.01f)]
    private float snapDuration = 0.12f;

    [SerializeField]
    private AnimationCurve snapCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    [SerializeField]
[Min(0.01f)]
private float gridDropDuration = 0.18f;

[SerializeField]
[Min(0f)]
private float gridDropLift = 8f;


    private PieceView pieceView;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private RectTransform gridRect;

    private PieceSpawner pieceSpawner;

    private Coroutine alphaFadeRoutine;


    // =====================================================
    // HOME
    // =====================================================

    private Transform homeParent;

    private int homeSiblingIndex;

    private Vector2 homeAnchoredPosition;

    private Vector2 homeAnchorMin;
    private Vector2 homeAnchorMax;
    private Vector2 homePivot;

    private float homeCellSize;
    private float homeSpacing;
    private float homeAlpha;

    private Vector3 homeTopLeftWorld;

    private bool homeCaptured;

    public bool IsBusyForPanelRotation =>
    isDragging || isSettling;


    // Достижение:
    // конкретную фигуру считаем только один раз.
    private bool countedForPlacementAchievement;


    // =====================================================
    // PLACED STATE
    // =====================================================

    private bool isPlaced;

    private Vector2Int placedOrigin;

    private readonly List<Vector2Int>
        placedCoordinates =
            new List<Vector2Int>();


    // =====================================================
    // PREVIOUS GRID PLACEMENT
    // =====================================================

    private bool wasPlacedAtDragStart;

    private Vector2Int previousOrigin;

    private int previousRotationSteps;


    // =====================================================
    // DRAG
    // =====================================================

    private bool isDragging;
    private bool isSettling;

    // Нужен, чтобы отпускание после drag
    // не воспринималось как обычный click.
    private bool draggedSincePointerDown;

    private bool hasValidPlacement;

    private Vector2Int currentOrigin;

    private Vector2 lastPointerPosition;

    private Camera lastEventCamera;


    // =====================================================
    // GRABBED CELL
    // =====================================================

    private int grabbedCellIndex;


    private Vector2 grabPointInsideCell =
        new Vector2(
            0.5f,
            0.5f
        );


    public bool IsCurrentlyDragging =>
        isDragging &&
        !isSettling;


    // =====================================================
    // UNITY
    // =====================================================

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


        if (
            gridRect == null &&
            gridManager != null
        )
        {
            gridRect =
                gridManager.GetComponent<RectTransform>();
        }
    }


    private void Update()
    {
        if (
            !isDragging ||
            isSettling
        )
        {
            return;
        }


        // Оставляем R для тестирования на ПК.
        if (
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame
        )
        {
            RotatePiece();
        }
    }


    // =====================================================
    // CONFIGURE
    // =====================================================

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


        if (gridManager != null)
        {
            gridRect =
                gridManager.GetComponent<RectTransform>();
        }
    }


    public void SetPieceSpawner(
        PieceSpawner newPieceSpawner
    )
    {
        pieceSpawner =
            newPieceSpawner;
    }


    // =====================================================
    // HOME
    // =====================================================

    private void CaptureHomePosition()
    {
        if (homeCaptured)
        {
            return;
        }


        if (
            pieceView == null ||
            rectTransform == null ||
            canvasGroup == null
        )
        {
            return;
        }


        homeParent =
            transform.parent;


        homeSiblingIndex =
            transform.GetSiblingIndex();


        homeAnchoredPosition =
            rectTransform.anchoredPosition;


        homeAnchorMin =
            rectTransform.anchorMin;


        homeAnchorMax =
            rectTransform.anchorMax;


        homePivot =
            rectTransform.pivot;


        homeCellSize =
            pieceView.CellSize;


        homeSpacing =
            pieceView.Spacing;


        homeAlpha =
            canvasGroup.alpha;


        homeTopLeftWorld =
            GetCurrentTopLeftWorld();


        // Если PiecePanel не был назначен напрямую,
        // пытаемся определить его через PiecesContainer.
        if (
            piecePanel == null &&
            homeParent != null &&
            homeParent.parent != null
        )
        {
            piecePanel =
                homeParent.parent
                    .GetComponent<RectTransform>();
        }


        homeCaptured =
            true;
    }


    public void RefreshPanelHome()
    {
        if (
            isDragging ||
            isSettling ||
            isPlaced
        )
        {
            return;
        }


        if (
            pieceView == null ||
            rectTransform == null ||
            canvasGroup == null
        )
        {
            return;
        }


        // Не перезаписываем Home,
        // если объект сейчас вообще не в PiecePanel.
        if (
            piecePanel != null &&
            !transform.IsChildOf(
                piecePanel
            )
        )
        {
            return;
        }


        homeParent =
            transform.parent;


        homeSiblingIndex =
            transform.GetSiblingIndex();


        homeAnchoredPosition =
            rectTransform.anchoredPosition;


        homeAnchorMin =
            rectTransform.anchorMin;


        homeAnchorMax =
            rectTransform.anchorMax;


        homePivot =
            rectTransform.pivot;


        homeCellSize =
            pieceView.CellSize;


        homeSpacing =
            pieceView.Spacing;


        homeAlpha =
            canvasGroup.alpha;


        homeTopLeftWorld =
            GetCurrentTopLeftWorld();


        homeCaptured =
            true;
    }


    // =====================================================
    // POINTER
    // =====================================================

    public void OnPointerDown(
        PointerEventData eventData
    )
    {

       if (
    pieceSpawner != null &&
    !pieceSpawner.CanStartPanelRotation()
)
{
    draggedSincePointerDown = true;

    return;
}

        if (pieceView != null && pieceView.IsRotating)
    return;

        if (isSettling)
        {
            return;
        }


        draggedSincePointerDown =
            false;


        CaptureHomePosition();


        StartAlphaFade(
            dragAlpha,
            pickupFadeDuration
        );
    }


    public void OnPointerUp(
        PointerEventData eventData
    )
    {
        if (isSettling)
        {
            return;
        }


        if (!isDragging)
        {
            StartAlphaFade(
                homeAlpha,
                pickupFadeDuration
            );
        }
    }


    public void OnPointerClick(
    PointerEventData eventData
)
{
    if (
        draggedSincePointerDown ||
        isDragging ||
        isSettling ||
        isPlaced ||
        pieceView == null ||
        pieceView.IsRotating
    )
    {
        return;
    }

    if (
        piecePanel != null &&
        !transform.IsChildOf(piecePanel)
    )
    {
        return;
    }

    if (
    pieceSpawner != null &&
    !pieceSpawner.CanStartPanelRotation()
)
{
    return;
}

    if (!pieceView.RotateClockwiseAnimated())
        return;

    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlaySfx(
            GameSfx.PieceRotate
        );
    }

    TutorialEvents.ReportPieceRotated();

}


    // =====================================================
    // BEGIN DRAG
    // =====================================================

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {

        if (
    pieceSpawner != null &&
    !pieceSpawner.CanStartPanelRotation()
)
{
    draggedSincePointerDown = true;

    return;
}

        if (pieceView != null && pieceView.IsRotating)
    return;

        if (isSettling)
        {
            return;
        }


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


        draggedSincePointerDown =
            true;

        TutorialEvents
    .ReportPieceDragStarted();

        if (AudioManager.Instance != null)
{
    AudioManager.Instance.PlaySfx(
        GameSfx.PiecePickup
    );
}


        CaptureHomePosition();


        CaptureGrabbedCell(
            eventData.position,
            eventData.pressEventCamera
        );


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


        // Если фигура уже стояла на Grid,
        // временно освобождаем её клетки.
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
            false
        );


        transform.SetAsLastSibling();


        rectTransform.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        rectTransform.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        rectTransform.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        canvasGroup.blocksRaycasts =
            false;


        UpdateDragVisualSize(
            eventData.position,
            eventData.pressEventCamera
        );


        MoveGrabbedCellToPointer(
            eventData.position,
            eventData.pressEventCamera
        );


        UpdatePlacementPreview();
    }


    // =====================================================
    // DRAG
    // =====================================================

    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (
            !isDragging ||
            isSettling
        )
        {
            return;
        }


        lastPointerPosition =
            eventData.position;


        lastEventCamera =
            eventData.pressEventCamera;


        UpdateDragVisualSize(
            eventData.position,
            eventData.pressEventCamera
        );


        MoveGrabbedCellToPointer(
            eventData.position,
            eventData.pressEventCamera
        );


        UpdatePlacementPreview();
    }


    // =====================================================
    // END DRAG
    // =====================================================

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (
            !isDragging ||
            isSettling
        )
        {
            return;
        }

        if (alphaFadeRoutine != null)
{
    StopCoroutine(alphaFadeRoutine);

    alphaFadeRoutine = null;
}


        isDragging =
            false;


        isSettling =
            true;


        canvasGroup.interactable =
            false;


        canvasGroup.blocksRaycasts =
            false;

        bool overPiecePanel =
    IsPointerOverPiecePanel(eventData);

StartCoroutine(
    CompleteDropAfterRotation(overPiecePanel)
);

}


private IEnumerator CompleteDropAfterRotation(
    bool overPiecePanel
)
{
    while (pieceView.IsRotating)
        yield return null;

    // Успешная установка на Grid.
    if (hasValidPlacement)
    {
        yield return AnimateToGridAndPlace(
            currentOrigin
        );

        yield break;
    }

    gridManager.ClearPlacementPreview();

    // Возвращение в PiecePanel.
    if (overPiecePanel)
    {
        yield return AnimateBackHome();
        yield break;
    }

    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlaySfx(
            GameSfx.InvalidDrop
        );
    }

    // Возвращение на предыдущую позицию Grid.
    if (wasPlacedAtDragStart)
    {
        pieceView.SetRotationSteps(
            previousRotationSteps
        );

        yield return AnimateBackToPreviousGrid();

        yield break;
    }

    // Возвращение в исходную панель.
    yield return AnimateBackHome();
}


    // =====================================================
    // DROP TO GRID
    // =====================================================

    private IEnumerator AnimateToGridAndPlace(
        Vector2Int targetOrigin
    )
    {
        if (
            !gridManager.TryGetCellTopLeftWorld(
                targetOrigin.x,
                targetOrigin.y,
                out Vector3 targetTopLeft
            )
        )
        {
            gridManager.ClearPlacementPreview();


            if (wasPlacedAtDragStart)
            {
                pieceView.SetRotationSteps(
                    previousRotationSteps
                );


                yield return
                    AnimateBackToPreviousGrid();
            }
            else
            {
                yield return
                    AnimateBackHome();
            }


            yield break;
        }


        yield return
            AnimateVisual(
                targetTopLeft,
                gridManager.CellSize,
                gridManager.Spacing,
                homeAlpha,
                true
            );


        bool placed =
            gridManager.PlacePiece(
                pieceView.CurrentCells,
                pieceView.Definition.Color,
                targetOrigin,
                out List<Vector2Int>
                    newCoordinates
            );


        if (!placed)
        {
            if (wasPlacedAtDragStart)
            {
                pieceView.SetRotationSteps(
                    previousRotationSteps
                );


                yield return
                    AnimateBackToPreviousGrid();
            }
            else
            {
                yield return
                    AnimateBackHome();
            }


            yield break;
        }


        placedOrigin =
            targetOrigin;


        placedCoordinates.Clear();


        placedCoordinates.AddRange(
            newCoordinates
        );


        isPlaced =
            true;

        if (AudioManager.Instance != null)
{
    AudioManager.Instance.PlaySfx(
        GameSfx.PiecePlace
    );
}


    HapticManager.Vibrate();

        TutorialEvents
    .ReportPiecePlaced();


        if (!countedForPlacementAchievement)
        {
            countedForPlacementAchievement =
                true;


            PlayerStats.RecordPiecePlaced();
        }


        FinishSettling();
    }


    // =====================================================
    // RETURN TO PREVIOUS GRID POSITION
    // =====================================================

    private IEnumerator AnimateBackToPreviousGrid()
    {
        if (
            !gridManager.TryGetCellTopLeftWorld(
                previousOrigin.x,
                previousOrigin.y,
                out Vector3 targetTopLeft
            )
        )
        {
            yield return
                AnimateBackHome();

            yield break;
        }


        yield return
            AnimateVisual(
                targetTopLeft,
                gridManager.CellSize,
                gridManager.Spacing,
                homeAlpha
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
                "PieceDragHandler: " +
                "could not restore previous placement."
            );


            yield return
                AnimateBackHome();

            yield break;
        }


        placedOrigin =
            previousOrigin;


        placedCoordinates.Clear();


        placedCoordinates.AddRange(
            restoredCoordinates
        );


        isPlaced =
            true;


        FinishSettling();
    }


    // =====================================================
    // RETURN TO PIECE PANEL
    // =====================================================

    private IEnumerator AnimateBackHome()
{
    gridManager.ClearPlacementPreview();

    if (pieceSpawner != null)
    {
        yield return pieceSpawner.PrepareReturnSlot(
            pieceView
        );

        if (pieceSpawner.TryGetReturnTarget(
            pieceView,
            out Vector2 targetPosition,
            out Vector3 targetTopLeft,
            out float targetCellSize,
            out float targetSpacing
        ))
        {
            homeAnchoredPosition = targetPosition;
            homeTopLeftWorld = targetTopLeft;

            homeCellSize = targetCellSize;
            homeSpacing = targetSpacing;

            homeAnchorMin = new Vector2(0.5f, 0.5f);
            homeAnchorMax = new Vector2(0.5f, 0.5f);
            homePivot = new Vector2(0.5f, 0.5f);
        }
    }

    yield return AnimateVisual(
        homeTopLeftWorld,
        homeCellSize,
        homeSpacing,
        homeAlpha
    );

    RestoreHomeTransform();

    placedCoordinates.Clear();

    isPlaced = false;

    FinishSettling();

    if (pieceSpawner != null)
    {
        pieceSpawner.EndPanelReturn(pieceView);
    }

    RefreshPanelHome();

    TutorialEvents.ReportPieceDragCancelled();
}

    // =====================================================
    // ANIMATION
    // =====================================================

    private IEnumerator AnimateVisual(
    Vector3 targetTopLeftWorld,
    float targetCellSize,
    float targetSpacing,
    float targetAlpha,
    bool isGridDrop = false
)
{
    Vector3 startPosition =
        GetCurrentTopLeftWorld();

    float startCellSize = pieceView.CellSize;

    float startSpacing = pieceView.Spacing;

    float startAlpha = canvasGroup.alpha;

    float duration = isGridDrop
        ? gridDropDuration
        : snapDuration;

    duration = Mathf.Max(0.01f, duration);

    Vector3 liftOffset =
        rectTransform.TransformVector(
            Vector3.up * gridDropLift
        );

    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;

        float rawT = Mathf.Clamp01(
            elapsed / duration
        );

        float t = Mathf.Clamp01(
            snapCurve.Evaluate(rawT)
        );

        float currentCellSize = Mathf.Lerp(
            startCellSize,
            targetCellSize,
            t
        );

        float currentSpacing = Mathf.Lerp(
            startSpacing,
            targetSpacing,
            t
        );

        pieceView.SetLayout(
            currentCellSize,
            currentSpacing
        );

        Vector3 position = Vector3.Lerp(
            startPosition,
            targetTopLeftWorld,
            t
        );

        // Небольшое движение по дуге
        // перед окончательным опусканием.
        if (isGridDrop)
        {
            float lift = Mathf.Sin(rawT * Mathf.PI);

            position += liftOffset * lift;
        }

        SetTopLeftWorld(position);

        canvasGroup.alpha = Mathf.Lerp(
            startAlpha,
            targetAlpha,
            t
        );

        yield return null;
    }

    pieceView.SetLayout(
        targetCellSize,
        targetSpacing
    );

    SetTopLeftWorld(targetTopLeftWorld);

    canvasGroup.alpha = targetAlpha;
}


    // =====================================================
    // WORLD POSITION
    // =====================================================

    private Vector3 GetCurrentTopLeftWorld()
    {
        rectTransform.ForceUpdateRectTransforms();


        Vector3 localTopLeft =
            new Vector3(
                rectTransform.rect.xMin,
                rectTransform.rect.yMax,
                0f
            );


        return
            rectTransform.TransformPoint(
                localTopLeft
            );
    }


    private void SetTopLeftWorld(
        Vector3 targetTopLeft
    )
    {
        rectTransform.ForceUpdateRectTransforms();


        Vector3 currentTopLeft =
            GetCurrentTopLeftWorld();


        rectTransform.position +=
            targetTopLeft -
            currentTopLeft;
    }


    // =====================================================
    // DRAG SIZE
    // =====================================================

    private void UpdateDragVisualSize(
        Vector2 screenPosition,
        Camera eventCamera
    )
    {
        float progress =
            CalculateGridProgress(
                screenPosition,
                eventCamera
            );


        float targetCellSize =
            Mathf.Lerp(
                homeCellSize,
                gridManager.CellSize,
                progress
            );


        float targetSpacing =
            Mathf.Lerp(
                homeSpacing,
                gridManager.Spacing,
                progress
            );


        pieceView.SetLayout(
            targetCellSize,
            targetSpacing
        );
    }


    private float CalculateGridProgress(
        Vector2 screenPosition,
        Camera eventCamera
    )
    {
        if (
            piecePanel != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                piecePanel,
                screenPosition,
                eventCamera
            )
        )
        {
            return 0f;
        }


        if (
            gridRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                gridRect,
                screenPosition,
                eventCamera
            )
        )
        {
            return 1f;
        }


        if (
            piecePanel == null ||
            gridRect == null
        )
        {
            return 1f;
        }


        float distanceToPanel =
            DistanceToRect(
                piecePanel,
                screenPosition,
                eventCamera
            );


        float distanceToGrid =
            DistanceToRect(
                gridRect,
                screenPosition,
                eventCamera
            );


        float total =
            distanceToPanel +
            distanceToGrid;


        if (total <= 0.01f)
        {
            return 0.5f;
        }


        float progress =
            distanceToPanel /
            total;


        return
            Mathf.SmoothStep(
                0f,
                1f,
                progress
            );
    }


    private float DistanceToRect(
        RectTransform target,
        Vector2 screenPosition,
        Camera eventCamera
    )
    {
        bool success =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                target,
                screenPosition,
                eventCamera,
                out Vector2 localPoint
            );


        if (!success)
        {
            return float.MaxValue;
        }


        Rect rect =
            target.rect;


        Vector2 closestLocal =
            new Vector2(
                Mathf.Clamp(
                    localPoint.x,
                    rect.xMin,
                    rect.xMax
                ),

                Mathf.Clamp(
                    localPoint.y,
                    rect.yMin,
                    rect.yMax
                )
            );


        Vector3 closestWorld =
            target.TransformPoint(
                closestLocal
            );


        Vector2 closestScreen =
            RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                closestWorld
            );


        return
            Vector2.Distance(
                screenPosition,
                closestScreen
            );
    }


    // =====================================================
    // GRABBED CELL
    // =====================================================

    private void CaptureGrabbedCell(
        Vector2 screenPosition,
        Camera eventCamera
    )
    {
        grabbedCellIndex =
            0;


        grabPointInsideCell =
            new Vector2(
                0.5f,
                0.5f
            );


        if (
            pieceView.CurrentCells == null ||
            pieceView.CurrentCells.Length == 0
        )
        {
            return;
        }


        bool success =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPosition,
                eventCamera,
                out Vector2 localPoint
            );


        if (!success)
        {
            return;
        }


        Rect pieceRect =
            rectTransform.rect;


        float cellSize =
            pieceView.CellSize;


        float step =
            pieceView.CellSize +
            pieceView.Spacing;


        int nearestIndex =
            0;


        float nearestDistance =
            float.MaxValue;


        for (
            int i = 0;
            i < pieceView.CurrentCells.Length;
            i++
        )
        {
            Vector2Int cell =
                pieceView.CurrentCells[i];


            float left =
                pieceRect.xMin +
                cell.x *
                step;


            float top =
                pieceRect.yMax -
                cell.y *
                step;


            float right =
                left +
                cellSize;


            float bottom =
                top -
                cellSize;


            bool inside =
                localPoint.x >= left &&
                localPoint.x <= right &&
                localPoint.y <= top &&
                localPoint.y >= bottom;


            if (inside)
            {
                grabbedCellIndex =
                    i;


                grabPointInsideCell =
                    new Vector2(
                        Mathf.InverseLerp(
                            left,
                            right,
                            localPoint.x
                        ),

                        Mathf.InverseLerp(
                            top,
                            bottom,
                            localPoint.y
                        )
                    );


                return;
            }


            Vector2 center =
                new Vector2(
                    left +
                    cellSize *
                    0.5f,

                    top -
                    cellSize *
                    0.5f
                );


            float distance =
                Vector2.SqrMagnitude(
                    localPoint -
                    center
                );


            if (
                distance <
                nearestDistance
            )
            {
                nearestDistance =
                    distance;


                nearestIndex =
                    i;
            }
        }


        grabbedCellIndex =
            nearestIndex;


        grabPointInsideCell =
            new Vector2(
                0.5f,
                0.5f
            );
    }


    // =====================================================
    // MOVE
    // =====================================================

    private void MoveGrabbedCellToPointer(
        Vector2 screenPosition,
        Camera eventCamera
    )
    {
        if (
            pieceView.CurrentCells == null ||
            pieceView.CurrentCells.Length == 0
        )
        {
            return;
        }


        bool success =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragLayer,
                screenPosition,
                eventCamera,
                out Vector2 pointerLocal
            );


        if (!success)
        {
            return;
        }


        grabbedCellIndex =
            Mathf.Clamp(
                grabbedCellIndex,
                0,
                pieceView.CurrentCells.Length - 1
            );


        Vector2Int grabbedCell =
            pieceView.CurrentCells[
                grabbedCellIndex
            ];


        Rect pieceRect =
            rectTransform.rect;


        float cellSize =
            pieceView.CellSize;


        float step =
            pieceView.CellSize +
            pieceView.Spacing;


        float cellLeft =
            pieceRect.xMin +
            grabbedCell.x *
            step;


        float cellTop =
            pieceRect.yMax -
            grabbedCell.y *
            step;


        Vector2 grabbedLocalPoint =
            new Vector2(
                cellLeft +
                grabPointInsideCell.x *
                cellSize,

                cellTop -
                grabPointInsideCell.y *
                cellSize
            );


        rectTransform.anchoredPosition =
            pointerLocal -
            grabbedLocalPoint;
    }


    // =====================================================
    // ROTATION
    // =====================================================

    // Оставлено для совместимости/возможного
    // внешнего управления в будущем.
    public void RotateFromExternalControl()
    {
        if (
            !isDragging ||
            isSettling
        )
        {
            return;
        }


        RotatePiece();
    }


    private void RotatePiece()
{

    if (
    pieceSpawner != null &&
    pieceSpawner.GameplayLocked
)
{
    return;
}

    if (
        pieceView == null ||
        pieceView.IsRotating ||
        isSettling
    )
    {
        return;
    }

    // Пересчитываем положение точки внутри
    // захваченной клетки при повороте на 90°.
    Vector2 rotatedGrabPoint = new Vector2(
        1f - grabPointInsideCell.y,
        grabPointInsideCell.x
    );

    bool rotated =
        pieceView.RotateClockwiseAnimated(
            grabbedCellIndex,
            rotatedGrabPoint
        );

    if (!rotated)
        return;

    grabPointInsideCell = rotatedGrabPoint;

    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlaySfx(
            GameSfx.PieceRotate
        );
    }

    TutorialEvents.ReportPieceRotated();

    UpdateDragVisualSize(
        lastPointerPosition,
        lastEventCamera
    );

    MoveGrabbedCellToPointer(
        lastPointerPosition,
        lastEventCamera
    );

    UpdatePlacementPreview();
}

    // =====================================================
    // PREVIEW
    // =====================================================

    private void UpdatePlacementPreview()
    {
        gridManager.ClearPlacementPreview();


        hasValidPlacement =
            false;


        if (
            pieceView.CurrentCells == null ||
            pieceView.CurrentCells.Length == 0
        )
        {
            return;
        }


        // ---------------------------------------------
        // Вариант 1:
        // курсор прямо над конкретной GridCell.
        // ---------------------------------------------

        bool cursorIsOverGrid =
            gridManager.TryGetCellUnderPointer(
                lastPointerPosition,
                lastEventCamera,
                out GridCell hoveredCell
            );


        if (cursorIsOverGrid)
        {
            grabbedCellIndex =
                Mathf.Clamp(
                    grabbedCellIndex,
                    0,
                    pieceView.CurrentCells.Length - 1
                );


            Vector2Int grabbedCell =
                pieceView.CurrentCells[
                    grabbedCellIndex
                ];


            currentOrigin =
                new Vector2Int(
                    hoveredCell.X -
                    grabbedCell.x,

                    hoveredCell.Y -
                    grabbedCell.y
                );
        }
        else
        {
            // -----------------------------------------
            // Вариант 2:
            // курсор снаружи Grid,
            // но сама фигура уже перекрывает поле.
            // -----------------------------------------

            bool pieceOverlapsGrid =
                TryGetPreviewOriginFromPieceOverlap(
                    out currentOrigin
                );


            if (!pieceOverlapsGrid)
            {
                return;
            }
        }


        hasValidPlacement =
            gridManager.ShowPlacementPreview(
                pieceView.CurrentCells,
                currentOrigin,
                pieceView.Definition.Color
            );
    }


    private bool TryGetPreviewOriginFromPieceOverlap(
        out Vector2Int origin
    )
    {
        origin =
            Vector2Int.zero;


        if (
            pieceView == null ||
            pieceView.CurrentCells == null ||
            pieceView.CurrentCells.Length == 0 ||
            gridManager == null
        )
        {
            return false;
        }


        Rect pieceRect =
            rectTransform.rect;


        float cellSize =
            pieceView.CellSize;


        float step =
            pieceView.CellSize +
            pieceView.Spacing;


        bool foundCandidate =
            false;


        float bestDistance =
            float.MaxValue;


        foreach (
            Vector2Int pieceCell
            in pieceView.CurrentCells
        )
        {
            Vector3 localCenter =
                new Vector3(
                    pieceRect.xMin +
                    pieceCell.x *
                    step +
                    cellSize *
                    0.5f,

                    pieceRect.yMax -
                    pieceCell.y *
                    step -
                    cellSize *
                    0.5f,

                    0f
                );


            Vector3 worldCenter =
                rectTransform.TransformPoint(
                    localCenter
                );


            Vector2 screenCenter =
                RectTransformUtility.WorldToScreenPoint(
                    lastEventCamera,
                    worldCenter
                );


            bool isOverGrid =
                gridManager.TryGetCellUnderPointer(
                    screenCenter,
                    lastEventCamera,
                    out GridCell gridCell
                );


            if (
                !isOverGrid ||
                gridCell == null
            )
            {
                continue;
            }


            RectTransform gridCellRect =
                gridCell.GetComponent<RectTransform>();


            if (gridCellRect == null)
            {
                continue;
            }


            Vector3 gridWorldCenter =
                gridCellRect.TransformPoint(
                    gridCellRect.rect.center
                );


            Vector2 gridScreenCenter =
                RectTransformUtility.WorldToScreenPoint(
                    lastEventCamera,
                    gridWorldCenter
                );


            float distance =
                Vector2.SqrMagnitude(
                    screenCenter -
                    gridScreenCenter
                );


            if (
                distance >=
                bestDistance
            )
            {
                continue;
            }


            bestDistance =
                distance;


            origin =
                new Vector2Int(
                    gridCell.X -
                    pieceCell.x,

                    gridCell.Y -
                    pieceCell.y
                );


            foundCandidate =
                true;
        }


        return foundCandidate;
    }


    // =====================================================
    // PANEL
    // =====================================================

    private bool IsPointerOverPiecePanel(
        PointerEventData eventData
    )
    {
        if (piecePanel == null)
        {
            return false;
        }


        return
            RectTransformUtility.RectangleContainsScreenPoint(
                piecePanel,
                eventData.position,
                eventData.pressEventCamera
            );
    }


    private void RestoreHomeTransform()
    {
        if (homeParent == null)
        {
            return;
        }


        transform.SetParent(
            homeParent,
            false
        );


        transform.SetSiblingIndex(
            Mathf.Clamp(
                homeSiblingIndex,
                0,
                transform.parent.childCount - 1
            )
        );


        rectTransform.anchorMin =
            homeAnchorMin;


        rectTransform.anchorMax =
            homeAnchorMax;


        rectTransform.pivot =
            homePivot;


        pieceView.SetLayout(
            homeCellSize,
            homeSpacing
        );


        rectTransform.anchoredPosition =
            homeAnchoredPosition;
    }


    // =====================================================
    // FINISH
    // =====================================================

    private void FinishSettling()
    {
        if (alphaFadeRoutine != null)
        {
            StopCoroutine(
                alphaFadeRoutine
            );


            alphaFadeRoutine =
                null;
        }


        if (gridManager != null)
        {
            gridManager.ClearPlacementPreview();
        }


        canvasGroup.alpha =
            homeAlpha;


        canvasGroup.interactable =
            true;


        canvasGroup.blocksRaycasts =
            true;


        hasValidPlacement =
            false;


        wasPlacedAtDragStart =
            false;


        isSettling =
            false;
    }


    // =====================================================
    // ALPHA
    // =====================================================

    private void StartAlphaFade(
        float targetAlpha,
        float duration
    )
    {
        if (alphaFadeRoutine != null)
        {
            StopCoroutine(
                alphaFadeRoutine
            );
        }


        alphaFadeRoutine =
            StartCoroutine(
                FadeAlpha(
                    targetAlpha,
                    duration
                )
            );
    }


    private IEnumerator FadeAlpha(
        float targetAlpha,
        float duration
    )
    {
        float startAlpha =
            canvasGroup.alpha;


        if (duration <= 0f)
        {
            canvasGroup.alpha =
                targetAlpha;


            alphaFadeRoutine =
                null;


            yield break;
        }


        float elapsed =
            0f;


        while (
            elapsed <
            duration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );


            yield return null;
        }


        canvasGroup.alpha =
            targetAlpha;


        alphaFadeRoutine =
            null;
    }
}