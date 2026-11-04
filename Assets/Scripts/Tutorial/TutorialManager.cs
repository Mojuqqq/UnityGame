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

    [Header("Visual Settings")]

    [SerializeField]
    private Vector2 focusPadding =
        new Vector2(
            30f,
            30f
        );


    [SerializeField]
    private float messageEdgeOffset =
        130f;


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
        TutorialEvents.PiecePlaced +=
            HandlePiecePlaced;


        TutorialEvents.PieceRotated +=
            HandlePieceRotated;
    }


    private void OnDisable()
    {
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
        currentStep =
            TutorialStep.DragPiece;


        RectTransform piece =
            GetFirstPiece();


        ShowTutorial(
            "ПЕРЕМЕЩЕНИЕ",
            "Перетащи фигуру с нижней панели " +
            "на игровое поле.",
            piece,
            false,
            ""
        );
    }


    // =====================================================
    // LEVEL 2
    // =====================================================

    private void BeginHintsTutorial(
        int subStep
    )
    {
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
    }


    // =====================================================
    // LEVEL 6
    // =====================================================

    private void BeginBlockedCellTutorial()
    {
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


        focusFrame.anchoredPosition =
            center;


        focusFrame.sizeDelta =
            size +
            focusPadding;


        PositionMessagePanel(
            center.y
        );
    }


    // =====================================================
    // MESSAGE POSITION
    // =====================================================

    private void PositionMessagePanel(
        float targetY
    )
    {
        if (messagePanel == null)
        {
            return;
        }


        // Если цель находится в нижней
        // половине экрана,
        // сообщение ставим наверх.
        if (targetY < 0f)
        {
            messagePanel.anchorMin =
                new Vector2(
                    0.5f,
                    1f
                );


            messagePanel.anchorMax =
                new Vector2(
                    0.5f,
                    1f
                );


            messagePanel.pivot =
                new Vector2(
                    0.5f,
                    1f
                );


            messagePanel.anchoredPosition =
                new Vector2(
                    0f,
                    -messageEdgeOffset
                );

            return;
        }


        // Если цель сверху,
        // сообщение ставим вниз.
        messagePanel.anchorMin =
            new Vector2(
                0.5f,
                0f
            );


        messagePanel.anchorMax =
            new Vector2(
                0.5f,
                0f
            );


        messagePanel.pivot =
            new Vector2(
                0.5f,
                0f
            );


        messagePanel.anchoredPosition =
            new Vector2(
                0f,
                messageEdgeOffset
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