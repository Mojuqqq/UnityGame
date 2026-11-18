using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TutorialManager :
    MonoBehaviour
{
    // =====================================================
    // GAMEPLAY
    // =====================================================

    [Header("Gameplay")]

    [SerializeField]
    private LevelManager levelManager;


    [SerializeField]
    private GridManager gridManager;


    // =====================================================
    // TARGETS
    // =====================================================

    [Header("Tutorial Targets")]

    [SerializeField]
    private RectTransform piecesContainer;


    [SerializeField]
    private RectTransform topHintsContainer;


    [SerializeField]
    private RectTransform rowHintsContainer;


    [SerializeField]
    private RectTransform gridContainer;


    [SerializeField]
    private RectTransform hintButtons;



    // =====================================================
// INTERACTION
// =====================================================

[Header("Gameplay Interaction")]

[SerializeField]
private CanvasGroup headerGroup;


[SerializeField]
private CanvasGroup boardGroup;


[SerializeField]
private CanvasGroup hintButtonsGroup;


[SerializeField]
private CanvasGroup piecePanelGroup;


    // =====================================================
    // UI
    // =====================================================

    [Header("Tutorial UI")]

    [SerializeField]
    private GameObject tutorialRoot;


    [SerializeField]
    private RectTransform tutorialRootRect;


    [SerializeField]
    private RectTransform focusFrame;

    [SerializeField]
private TutorialPointerAnimator
    pointerAnimator;

    [SerializeField]
private RectTransform dimmerTop;


[SerializeField]
private RectTransform dimmerBottom;


[SerializeField]
private RectTransform dimmerLeft;


[SerializeField]
private RectTransform dimmerRight;


    [SerializeField]
    private RectTransform messagePanel;


    [SerializeField]
    private TMP_Text titleText;


    [SerializeField]
    private TMP_Text bodyText;


    [SerializeField]
    private Button continueButton;


    [SerializeField]
    private TMP_Text continueButtonText;


    // =====================================================
    // VISUAL SETTINGS
    // =====================================================

    [Header("Focus Padding")]

[SerializeField]
private Vector2 dragFocusPadding =
    new Vector2(
        70f,
        70f
    );


[SerializeField]
private Vector2 hintsFocusPadding =
    new Vector2(
        30f,
        24f
    );


[SerializeField]
private Vector2 rotationFocusPadding =
    new Vector2(
        70f,
        70f
    );


[SerializeField]
private Vector2 blockedFocusPadding =
    new Vector2(
        18f,
        18f
    );


[SerializeField]
private Vector2 hintButtonsFocusPadding =
    new Vector2(
        40f,
        30f
    );


[Header("Message Position")]

[SerializeField]
[Min(0f)]
private float messageTargetGap =
    40f;


[SerializeField]
[Min(0f)]
private float messageEdgePadding =
    45f;


    // =====================================================
    // STATE
    // =====================================================

    private TutorialStep
        currentStep;


    private bool tutorialActive;


    // Для Level 2:
    // 0 = columns
    // 1 = rows
    private int hintsSubStep;


    // =====================================================
    // UNITY
    // =====================================================

    private void OnEnable()
{
    TutorialEvents.PieceDragStarted +=
        HandlePieceDragStarted;


    TutorialEvents.PieceDragCancelled +=
        HandlePieceDragCancelled;


    TutorialEvents.PiecePlaced +=
        HandlePiecePlaced;


    TutorialEvents.PieceRotated +=
        HandlePieceRotated;
}


    private void OnDisable()
{
    TutorialEvents.PieceDragStarted -=
        HandlePieceDragStarted;


    TutorialEvents.PieceDragCancelled -=
        HandlePieceDragCancelled;


    TutorialEvents.PiecePlaced -=
        HandlePiecePlaced;


    TutorialEvents.PieceRotated -=
        HandlePieceRotated;
}


    private IEnumerator Start()
    {
        HideTutorial();


        // LevelManager создаёт Grid,
        // Pieces и Hints в своём Start().
        // Ждём один кадр,
        // чтобы UI уже существовал.
        yield return null;


        yield return
            new WaitUntil(
                () =>
                    levelManager != null
                    &&
                    levelManager.CurrentLevel != null
            );


        Canvas.ForceUpdateCanvases();


        TryStartTutorialForCurrentLevel();
    }


    // =====================================================
    // START BY LEVEL
    // =====================================================

    private void TryStartTutorialForCurrentLevel()
    {
        if (
            TutorialProgress
                .IsTutorialCompleted()
        )
        {
            HideTutorial();

            return;
        }


        if (
            levelManager == null ||
            levelManager.CurrentLevel == null
        )
        {
            return;
        }


        int levelNumber =
            levelManager
                .CurrentLevel
                .LevelNumber;


        switch (levelNumber)
        {
            // ---------------------------------------------
            // LEVEL 1
            // DRAG
            // ---------------------------------------------

            case 1:

                if (
                    !TutorialProgress
                        .IsStepCompleted(
                            TutorialStep
                                .DragPiece
                        )
                )
                {
                    BeginDragTutorial();
                }

                break;


            // ---------------------------------------------
            // LEVEL 2
            // HINTS
            // ---------------------------------------------

            case 2:

                if (
                    !TutorialProgress
                        .IsStepCompleted(
                            TutorialStep
                                .ReadHints
                        )
                )
                {
                    BeginHintsTutorial(
                        0
                    );
                }

                break;


            // ---------------------------------------------
            // LEVEL 3
            // ROTATION
            // ---------------------------------------------

            case 3:

                if (
                    !TutorialProgress
                        .IsStepCompleted(
                            TutorialStep
                                .RotatePiece
                        )
                )
                {
                    BeginRotationTutorial();
                }

                break;


            // ---------------------------------------------
            // LEVEL 6
            // BLOCKED CELL
            // ---------------------------------------------

            case 6:

                if (
                    !TutorialProgress
                        .IsStepCompleted(
                            TutorialStep
                                .BlockedCell
                        )
                )
                {
                    BeginBlockedCellTutorial();
                }

                break;


            // ---------------------------------------------
            // LEVEL 7
            // GAMEPLAY HINT BUTTONS
            // ---------------------------------------------

            case 7:

                if (
                    !TutorialProgress
                        .IsStepCompleted(
                            TutorialStep
                                .HintButtons
                        )
                )
                {
                    BeginHintButtonsTutorial();
                }

                break;
        }
    }


    // =====================================================
    // LEVEL 1
    // =====================================================

   private void BeginDragTutorial()
{

    SetTutorialInteraction(
    false,  // Header
    true,   // Board
    false,  // Hint buttons
    true    // Pieces
);

    currentStep =
        TutorialStep.DragPiece;


    RectTransform piece =
        GetFirstPiece();


    RectTransform destination =
        GetFirstSolutionTargetCell();


    ShowTutorial(
        "ПЕРЕМЕЩЕНИЕ",
        "Перетащи фигуру с нижней панели " +
        "на игровое поле.",
        piece,
        false,
        ""
    );


    if (pointerAnimator != null)
    {
        pointerAnimator.ShowDrag(
            piece,
            destination
        );
    }
}


    // =====================================================
    // LEVEL 2
    // =====================================================

   private void BeginHintsTutorial(
    int subStep
)
{   

    SetTutorialInteraction(
    false,
    false,
    false,
    false
);

    if (pointerAnimator != null)
    {
        pointerAnimator.HidePointer();
    }


    currentStep =
        TutorialStep.ReadHints;


    hintsSubStep =
        subStep;


    if (hintsSubStep == 0)
    {
        ShowTutorial(
            "ПОДСКАЗКИ",
            "Подсказки над полем показывают, " +
            "сколько клеток должно быть занято " +
            "в каждой колонке.",
            topHintsContainer,
            true,
            "ДАЛЕЕ"
        );

        return;
    }


    ShowTutorial(
        "ПОДСКАЗКИ",
        "Подсказки сбоку показывают, " +
        "сколько клеток должно быть занято " +
        "в каждой строке.",
        rowHintsContainer,
        true,
        "ПОНЯТНО"
    );
}

    // =====================================================
    // LEVEL 3
    // =====================================================

   private void BeginRotationTutorial()
{

    SetTutorialInteraction(
    false,  // Header
    false,  // Board
    false,  // Hints
    true    // PiecePanel
);

    currentStep =
        TutorialStep.RotatePiece;


    RectTransform piece =
        GetFirstPiece();


    ShowTutorial(
        "ПОВОРОТ",
        "Нажми на фигуру в нижней панели, " +
        "чтобы повернуть её на 90°.",
        piece,
        false,
        ""
    );


    if (pointerAnimator != null)
    {
        pointerAnimator.ShowTap(
            piece
        );
    }
}


    // =====================================================
    // LEVEL 6
    // =====================================================

    private void BeginBlockedCellTutorial()
{

    SetTutorialInteraction(
    false,
    false,
    false,
    false
);

    if (pointerAnimator != null)
    {
        pointerAnimator.HidePointer();
    }


    currentStep =
        TutorialStep.BlockedCell;


    RectTransform blockedCell =
        GetFirstBlockedCell();


    ShowTutorial(
        "ЗАБЛОКИРОВАННАЯ КЛЕТКА",
        "На эту клетку нельзя ставить фигуры. " +
        "Учитывай препятствия при поиске решения.",
        blockedCell,
        true,
        "ПОНЯТНО"
    );
}


    // =====================================================
    // LEVEL 7
    // =====================================================

    private void BeginHintButtonsTutorial()
{

    SetTutorialInteraction(
    false,
    false,
    false,
    false
);

    if (pointerAnimator != null)
    {
        pointerAnimator.HidePointer();
    }


    currentStep =
        TutorialStep.HintButtons;


    ShowTutorial(
        "ПОДСКАЗКИ",
        "Если застряла, можно воспользоваться " +
        "подсказкой. Первая показывает положение " +
        "одной фигуры, вторая отмечает клетки, " +
        "которые должны остаться пустыми.",
        hintButtons,
        true,
        "ПОНЯТНО"
    );
}

    // =====================================================
    // CONTINUE BUTTON
    // =====================================================

    public void ContinueTutorial()
    {
        if (!tutorialActive)
        {
            return;
        }


        switch (currentStep)
        {
            // ---------------------------------------------
            // LEVEL 2
            // ---------------------------------------------

            case TutorialStep.ReadHints:

                if (hintsSubStep == 0)
                {
                    BeginHintsTutorial(
                        1
                    );

                    return;
                }


                CompleteCurrentStep();

                return;


            // ---------------------------------------------
            // BLOCKED
            // ---------------------------------------------

            case TutorialStep.BlockedCell:

                CompleteCurrentStep();

                return;


            // ---------------------------------------------
            // HINT BUTTONS
            // ---------------------------------------------

            case TutorialStep.HintButtons:

                CompleteCurrentStep();

                return;
        }
    }


    // =====================================================
    // GAMEPLAY EVENTS
    // =====================================================

    private void HandlePieceDragStarted()
{
    if (
        !tutorialActive ||
        currentStep !=
            TutorialStep.DragPiece
    )
    {
        return;
    }


    if (pointerAnimator != null)
    {
        pointerAnimator.HidePointer();
    }
}


private void HandlePieceDragCancelled()
{
    if (
        !tutorialActive ||
        currentStep !=
            TutorialStep.DragPiece
    )
    {
        return;
    }


    if (pointerAnimator == null)
    {
        return;
    }


    RectTransform piece =
        GetFirstPiece();


    RectTransform destination =
        GetFirstSolutionTargetCell();


    if (
        piece == null ||
        destination == null
    )
    {
        return;
    }


    pointerAnimator.ShowDrag(
        piece,
        destination
    );
}


    private void HandlePiecePlaced()
    {
        if (
            !tutorialActive ||
            currentStep !=
                TutorialStep.DragPiece
        )
        {
            return;
        }


        CompleteCurrentStep();
    }


    private void HandlePieceRotated()
    {
        if (
            !tutorialActive ||
            currentStep !=
                TutorialStep.RotatePiece
        )
        {
            return;
        }


        CompleteCurrentStep();
    }


    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompleteCurrentStep()
    {
        TutorialProgress
            .MarkStepCompleted(
                currentStep
            );


        HideTutorial();
    }


    // =====================================================
    // SHOW
    // =====================================================

    private void ShowTutorial(
        string title,
        string body,
        RectTransform target,
        bool showContinue,
        string buttonText
    )
    {
        tutorialActive =
            true;


        if (tutorialRoot != null)
        {
            tutorialRoot.SetActive(
                true
            );
        }


        if (titleText != null)
        {
            titleText.text =
                title;
        }


        if (bodyText != null)
        {
            bodyText.text =
                body;
        }


        if (continueButton != null)
        {
            continueButton
                .gameObject
                .SetActive(
                    showContinue
                );
        }


        if (
            continueButtonText != null &&
            showContinue
        )
        {
            continueButtonText.text =
                buttonText;
        }


        Canvas.ForceUpdateCanvases();


        FocusTarget(
            target
        );
    }


    // =====================================================
    // HIDE
    // =====================================================

   private void HideTutorial()
{
    tutorialActive =
        false;


    if (pointerAnimator != null)
    {
        pointerAnimator.HidePointer();
    }


    RestoreGameplayInteraction();


    if (tutorialRoot != null)
    {
        tutorialRoot.SetActive(
            false
        );
    }
}

    // =====================================================
    // FOCUS
    // =====================================================

    private Vector2 GetCurrentFocusPadding()
{
    switch (currentStep)
    {
        case TutorialStep.DragPiece:

            return
                dragFocusPadding;


        case TutorialStep.ReadHints:

            return
                hintsFocusPadding;


        case TutorialStep.RotatePiece:

            return
                rotationFocusPadding;


        case TutorialStep.BlockedCell:

            return
                blockedFocusPadding;


        case TutorialStep.HintButtons:

            return
                hintButtonsFocusPadding;
    }


    return
        new Vector2(
            30f,
            30f
        );
}

    private void FocusTarget(
        RectTransform target
    )
    {
        if (
            focusFrame == null ||
            tutorialRootRect == null
        )
        {
            return;
        }


        if (target == null)
        {
            focusFrame
                .gameObject
                .SetActive(
                    false
                );

            return;
        }


        focusFrame
            .gameObject
            .SetActive(
                true
            );


        Vector3[] corners =
            new Vector3[4];


        target.GetWorldCorners(
            corners
        );


        Vector3 bottomLeft =
            tutorialRootRect
                .InverseTransformPoint(
                    corners[0]
                );


        Vector3 topRight =
            tutorialRootRect
                .InverseTransformPoint(
                    corners[2]
                );


        Vector2 size =
            new Vector2(
                topRight.x -
                bottomLeft.x,

                topRight.y -
                bottomLeft.y
            );


        Vector2 center =
            new Vector2(
                (
                    bottomLeft.x +
                    topRight.x
                )
                *
                0.5f,

                (
                    bottomLeft.y +
                    topRight.y
                )
                *
                0.5f
            );


        focusFrame.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        focusFrame.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        focusFrame.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

    Vector2 padding =
    GetCurrentFocusPadding();


Vector2 holeSize =
    size +
    padding;


focusFrame.anchoredPosition =
    center;


focusFrame.sizeDelta =
    holeSize;


UpdateDimmerHole(
    center,
    holeSize
);


PositionMessagePanel(
    center,
    holeSize
);

    }



    private void UpdateDimmerHole(
    Vector2 holeCenter,
    Vector2 holeSize
)
{
    if (
        tutorialRootRect == null ||
        dimmerTop == null ||
        dimmerBottom == null ||
        dimmerLeft == null ||
        dimmerRight == null
    )
    {
        return;
    }


    float rootWidth =
        tutorialRootRect.rect.width;


    Rect rootRect =
    tutorialRootRect.rect;


float rootLeft =
    rootRect.xMin;


float rootRight =
    rootRect.xMax;


float rootBottom =
    rootRect.yMin;


float rootTop =
    rootRect.yMax;


    float holeLeft =
        holeCenter.x -
        holeSize.x * 0.5f;


    float holeRight =
        holeCenter.x +
        holeSize.x * 0.5f;


    float holeBottom =
        holeCenter.y -
        holeSize.y * 0.5f;


    float holeTop =
        holeCenter.y +
        holeSize.y * 0.5f;


    // Не даём отверстию выходить
    // за пределы экрана.
    holeLeft =
        Mathf.Clamp(
            holeLeft,
            rootLeft,
            rootRight
        );


    holeRight =
        Mathf.Clamp(
            holeRight,
            rootLeft,
            rootRight
        );


    holeBottom =
        Mathf.Clamp(
            holeBottom,
            rootBottom,
            rootTop
        );


    holeTop =
        Mathf.Clamp(
            holeTop,
            rootBottom,
            rootTop
        );


    // =================================================
    // TOP
    // =================================================

    SetDimmerRect(
        dimmerTop,

        rootLeft,
        holeTop,

        rootRight,
        rootTop
    );


    // =================================================
    // BOTTOM
    // =================================================

    SetDimmerRect(
        dimmerBottom,

        rootLeft,
        rootBottom,

        rootRight,
        holeBottom
    );


    // =================================================
    // LEFT
    // =================================================

    SetDimmerRect(
        dimmerLeft,

        rootLeft,
        holeBottom,

        holeLeft,
        holeTop
    );


    // =================================================
    // RIGHT
    // =================================================

    SetDimmerRect(
        dimmerRight,

        holeRight,
        holeBottom,

        rootRight,
        holeTop
    );
}


private void SetDimmerRect(
    RectTransform rect,
    float left,
    float bottom,
    float right,
    float top
)
{
    if (rect == null)
    {
        return;
    }


    float width =
        Mathf.Max(
            0f,
            right - left
        );


    float height =
        Mathf.Max(
            0f,
            top - bottom
        );


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


    rect.sizeDelta =
        new Vector2(
            width,
            height
        );


    rect.anchoredPosition =
        new Vector2(
            (
                left +
                right
            )
            *
            0.5f,

            (
                bottom +
                top
            )
            *
            0.5f
        );
}

    // =====================================================
    // MESSAGE POSITION
    // =====================================================

    private void PositionMessagePanel(
    Vector2 targetCenter,
    Vector2 targetSize
)
{
    if (
        messagePanel == null ||
        tutorialRootRect == null
    )
    {
        return;
    }


    Canvas.ForceUpdateCanvases();


    Rect rootRect =
        tutorialRootRect.rect;


    float panelHeight =
        Mathf.Max(
            1f,
            messagePanel.rect.height
        );


    float panelWidth =
        Mathf.Max(
            1f,
            messagePanel.rect.width
        );


    float targetTop =
        targetCenter.y +
        targetSize.y * 0.5f;


    float targetBottom =
        targetCenter.y -
        targetSize.y * 0.5f;


    // =================================================
    // AVAILABLE SPACE
    // =================================================

    float spaceAbove =
        rootRect.yMax
        -
        messageEdgePadding
        -
        targetTop
        -
        messageTargetGap;


    float spaceBelow =
        targetBottom
        -
        messageTargetGap
        -
        (
            rootRect.yMin +
            messageEdgePadding
        );


    bool fitsAbove =
        spaceAbove >=
        panelHeight;


    bool fitsBelow =
        spaceBelow >=
        panelHeight;


    bool placeAbove;


    // Если помещается только сверху.
    if (
        fitsAbove &&
        !fitsBelow
    )
    {
        placeAbove =
            true;
    }

    // Если помещается только снизу.
    else if (
        fitsBelow &&
        !fitsAbove
    )
    {
        placeAbove =
            false;
    }

    // Если помещается с обеих сторон
    // или не помещается полностью нигде —
    // выбираем сторону, где больше места.
    else
    {
        placeAbove =
            spaceAbove >=
            spaceBelow;
    }


    // =================================================
    // TARGET POSITION
    // =================================================

    float wantedY;


    if (placeAbove)
    {
        wantedY =
            targetTop
            +
            messageTargetGap
            +
            panelHeight * 0.5f;
    }
    else
    {
        wantedY =
            targetBottom
            -
            messageTargetGap
            -
            panelHeight * 0.5f;
    }


    // =================================================
    // KEEP INSIDE SCREEN
    // =================================================

    float minY =
        rootRect.yMin
        +
        messageEdgePadding
        +
        panelHeight * 0.5f;


    float maxY =
        rootRect.yMax
        -
        messageEdgePadding
        -
        panelHeight * 0.5f;


    wantedY =
        Mathf.Clamp(
            wantedY,
            minY,
            maxY
        );


    // По X сообщение пока всегда центрируем.
    float minX =
        rootRect.xMin
        +
        messageEdgePadding
        +
        panelWidth * 0.5f;


    float maxX =
        rootRect.xMax
        -
        messageEdgePadding
        -
        panelWidth * 0.5f;


    float wantedX =
        Mathf.Clamp(
            0f,
            minX,
            maxX
        );


    // =================================================
    // APPLY
    // =================================================

    messagePanel.anchorMin =
        new Vector2(
            0.5f,
            0.5f
        );


    messagePanel.anchorMax =
        new Vector2(
            0.5f,
            0.5f
        );


    messagePanel.pivot =
        new Vector2(
            0.5f,
            0.5f
        );


    messagePanel.anchoredPosition =
        new Vector2(
            wantedX,
            wantedY
        );
}


    // =====================================================
    // DYNAMIC TARGETS
    // =====================================================

    private RectTransform GetFirstPiece()
    {
        if (
            piecesContainer == null ||
            piecesContainer.childCount == 0
        )
        {
            return
                piecesContainer;
        }


        return
            piecesContainer
                .GetChild(0)
                as RectTransform;
    }


    private RectTransform GetFirstBlockedCell()
    {
        if (
            levelManager == null ||
            levelManager.CurrentLevel == null ||
            gridManager == null
        )
        {
            return
                gridContainer;
        }


        Vector2Int[] blocked =
            levelManager
                .CurrentLevel
                .BlockedCells;


        if (
            blocked == null ||
            blocked.Length == 0
        )
        {
            return
                gridContainer;
        }


        Vector2Int coordinate =
            blocked[0];


        GridCell cell =
            gridManager.GetCell(
                coordinate.x,
                coordinate.y
            );


        if (cell == null)
        {
            return
                gridContainer;
        }


        return
            cell.GetComponent<
                RectTransform
            >();
    }


private RectTransform
    GetFirstSolutionTargetCell()
{
    if (
        levelManager == null ||
        levelManager.CurrentLevel == null ||
        gridManager == null
    )
    {
        return
            gridContainer;
    }


    LevelData level =
        levelManager
            .CurrentLevel;


    if (
        level.Pieces == null ||
        level.Pieces.Length == 0
    )
    {
        return
            gridContainer;
    }


    LevelSolutionEntry entry =
        level
            .GetSolutionEntryByPieceIndex(
                0
            );


    if (entry == null)
    {
        return
            gridContainer;
    }


    Vector2Int[] solutionCells =
        level.GetSolutionCells(
            entry
        );


    if (
        solutionCells == null ||
        solutionCells.Length == 0
    )
    {
        return
            gridContainer;
    }


    // Берём первую клетку,
    // которую должна занять первая фигура.
    Vector2Int coordinate =
        solutionCells[0];


    GridCell cell =
        gridManager.GetCell(
            coordinate.x,
            coordinate.y
        );


    if (cell == null)
    {
        return
            gridContainer;
    }


    return
        cell.GetComponent<
            RectTransform
        >();
}



// =====================================================
// INTERACTION CONTROL
// =====================================================

private void SetTutorialInteraction(
    bool allowHeader,
    bool allowBoard,
    bool allowHintButtons,
    bool allowPieces
)
{
    SetGroupInteraction(
        headerGroup,
        allowHeader
    );


    SetGroupInteraction(
        boardGroup,
        allowBoard
    );


    SetGroupInteraction(
        hintButtonsGroup,
        allowHintButtons
    );


    SetGroupInteraction(
        piecePanelGroup,
        allowPieces
    );
}


private void SetGroupInteraction(
    CanvasGroup group,
    bool allowed
)
{
    if (group == null)
    {
        return;
    }


    // Alpha не трогаем:
    // заблокированный UI остаётся видимым.
    group.interactable =
        allowed;


    group.blocksRaycasts =
        allowed;
}


private void RestoreGameplayInteraction()
{
    SetTutorialInteraction(
        true,   // Header
        true,   // Board
        true,   // Hint buttons
        true    // Pieces
    );
}

    // =====================================================
    // TESTING
    // =====================================================

    [ContextMenu(
        "Reset Tutorial For Testing"
    )]
    private void ResetTutorialForTesting()
    {
        TutorialProgress
            .ResetForTesting();


        Debug.Log(
            "Tutorial progress reset."
        );
    }
}