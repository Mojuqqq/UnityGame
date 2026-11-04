using UnityEngine;


[RequireComponent(
    typeof(RectTransform)
)]
public class AdaptiveGameplayLayout :
    MonoBehaviour
{
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("Main References")]

    [SerializeField]
    private RectTransform safeArea;


    [SerializeField]
    private RectTransform headerPanel;


    [SerializeField]
    private RectTransform piecePanel;


    [Header("Gameplay Children")]

    [SerializeField]
    private RectTransform boardArea;


    [SerializeField]
    private RectTransform hintButtonsArea;


    // =====================================================
    // OUTER LAYOUT
    // =====================================================

    [Header("Gameplay Area")]

    [SerializeField]
    [Min(0f)]
    private float horizontalMargin =
        24f;


    [SerializeField]
    [Min(0f)]
    private float gapBelowHeader =
        16f;


    [SerializeField]
    [Min(0f)]
    private float gapAbovePiecePanel =
        16f;


    // =====================================================
    // INTERNAL LAYOUT
    // =====================================================

    [Header("Hint Buttons Area")]

    [SerializeField]
    [Min(1f)]
    private float hintButtonsHeight =
        120f;


    [SerializeField]
    [Min(0f)]
    private float boardToHintButtonsGap =
        12f;


    // =====================================================
    // STATE
    // =====================================================

    private RectTransform gameplayRect;


    private bool initialized;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        gameplayRect =
            GetComponent<RectTransform>();
    }


    // =====================================================
    // APPLY
    // =====================================================

    public void ApplyLayout()
    {
        if (gameplayRect == null)
        {
            gameplayRect =
                GetComponent<RectTransform>();
        }


        if (
            safeArea == null ||
            headerPanel == null ||
            piecePanel == null ||
            boardArea == null ||
            hintButtonsArea == null
        )
        {
            Debug.LogError(
                "AdaptiveGameplayLayout: " +
                "references are missing."
            );

            return;
        }


        initialized =
            true;


        Canvas.ForceUpdateCanvases();


        ConfigureGameplayArea();


        Canvas.ForceUpdateCanvases();


        ConfigureChildren();


        Canvas.ForceUpdateCanvases();
    }


    // =====================================================
    // GAMEPLAY AREA
    // =====================================================

    private void ConfigureGameplayArea()
    {
        Rect safeRect =
            safeArea.rect;


        // =================================================
        // PIECE PANEL TOP
        // =================================================

        Vector3[] pieceCorners =
            new Vector3[4];


        piecePanel.GetWorldCorners(
            pieceCorners
        );


        Vector3 pieceTopLeft =
            safeArea.InverseTransformPoint(
                pieceCorners[1]
            );


        float pieceTop =
            pieceTopLeft.y;


        // =================================================
        // HEADER BOTTOM
        // =================================================

        Vector3[] headerCorners =
            new Vector3[4];


        headerPanel.GetWorldCorners(
            headerCorners
        );


        Vector3 headerBottomLeft =
            safeArea.InverseTransformPoint(
                headerCorners[0]
            );


        float headerBottom =
            headerBottomLeft.y;


        // =================================================
        // AVAILABLE AREA
        // =================================================

        float gameplayBottom =
            pieceTop +
            gapAbovePiecePanel;


        float gameplayTop =
            headerBottom -
            gapBelowHeader;


        // Защита от некорректного layout.
        if (
            gameplayTop <=
            gameplayBottom
        )
        {
            Debug.LogError(
                "AdaptiveGameplayLayout: " +
                "not enough vertical space " +
                "between HeaderPanel and PiecePanel."
            );

            return;
        }


        float bottomInset =
            gameplayBottom -
            safeRect.yMin;


        float topInset =
            safeRect.yMax -
            gameplayTop;


        // =================================================
        // STRETCH INSIDE SAFE AREA
        // =================================================

        gameplayRect.anchorMin =
            Vector2.zero;


        gameplayRect.anchorMax =
            Vector2.one;


        gameplayRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        gameplayRect.offsetMin =
            new Vector2(
                horizontalMargin,
                bottomInset
            );


        gameplayRect.offsetMax =
            new Vector2(
                -horizontalMargin,
                -topInset
            );


        gameplayRect.localScale =
            Vector3.one;
    }


    // =====================================================
    // CHILDREN
    // =====================================================

    private void ConfigureChildren()
    {
        // =================================================
        // HINT BUTTONS
        //
        // Всегда находятся внизу GameplayArea.
        // =================================================

        hintButtonsArea.anchorMin =
            new Vector2(
                0f,
                0f
            );


        hintButtonsArea.anchorMax =
            new Vector2(
                1f,
                0f
            );


        hintButtonsArea.pivot =
            new Vector2(
                0.5f,
                0f
            );


        hintButtonsArea.anchoredPosition =
            Vector2.zero;


        hintButtonsArea.sizeDelta =
            new Vector2(
                0f,
                hintButtonsHeight
            );


        hintButtonsArea.localScale =
            Vector3.one;


        // =================================================
        // BOARD AREA
        //
        // Заполняет всё пространство,
        // которое осталось НАД HintButtonsArea.
        // =================================================

        boardArea.anchorMin =
            Vector2.zero;


        boardArea.anchorMax =
            Vector2.one;


        boardArea.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        boardArea.offsetMin =
            new Vector2(
                0f,

                hintButtonsHeight +
                boardToHintButtonsGap
            );


        boardArea.offsetMax =
            Vector2.zero;


        boardArea.localScale =
            Vector3.one;
    }


    // =====================================================
    // RESPONSIVE
    // =====================================================

    private void OnRectTransformDimensionsChange()
    {
        if (
            !Application.isPlaying ||
            !initialized
        )
        {
            return;
        }


        ApplyLayout();
    }
}