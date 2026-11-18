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


    private bool isApplyingLayout;


    // Запоминаем только внешнюю геометрию,
    // которая действительно должна вызвать
    // повторный layout.
    private Vector2 lastSafeAreaSize;

    private float lastPiecePanelTop;

    private float lastHeaderBottom;

    private bool hasLayoutSnapshot;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        gameplayRect =
            GetComponent<RectTransform>();
    }


    private void LateUpdate()
    {
        if (
            !initialized ||
            isApplyingLayout
        )
        {
            return;
        }


        if (
            !ReferencesAreValid()
        )
        {
            return;
        }


        // Перестраиваем интерфейс только тогда,
        // когда реально изменилась внешняя
        // геометрия экрана / панелей.
        if (
            HasExternalGeometryChanged()
        )
        {
            RebuildLayout();
        }
    }


    // =====================================================
    // PUBLIC APPLY
    // =====================================================

    public void ApplyLayout()
    {
        if (gameplayRect == null)
        {
            gameplayRect =
                GetComponent<RectTransform>();
        }


        if (
            !ReferencesAreValid()
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


        RebuildLayout();
    }


    // =====================================================
    // VALIDATION
    // =====================================================

    private bool ReferencesAreValid()
    {
        return
            safeArea != null &&
            headerPanel != null &&
            piecePanel != null &&
            boardArea != null &&
            hintButtonsArea != null;
    }


    // =====================================================
    // REBUILD
    // =====================================================

    private void RebuildLayout()
    {
        if (isApplyingLayout)
        {
            return;
        }


        isApplyingLayout =
            true;


        try
        {
            Canvas.ForceUpdateCanvases();


            ConfigureGameplayArea();


            ConfigureChildren();


            Canvas.ForceUpdateCanvases();


            SaveLayoutSnapshot();
        }
        finally
        {
            isApplyingLayout =
                false;
        }
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

        float pieceTop =
            GetPiecePanelTop();


        // =================================================
        // HEADER BOTTOM
        // =================================================

        float headerBottom =
            GetHeaderBottom();


        // =================================================
        // AVAILABLE AREA
        // =================================================

        float gameplayBottom =
            pieceTop +
            gapAbovePiecePanel;


        float gameplayTop =
            headerBottom -
            gapBelowHeader;


        // =================================================
        // SAFETY
        // =================================================

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
        // GAMEPLAY RECT
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
        // HINT BUTTONS AREA
        //
        // Всегда внизу GameplayArea.
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
        // Всё оставшееся место над кнопками.
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
    // EXTERNAL GEOMETRY
    // =====================================================

    private float GetPiecePanelTop()
    {
        Vector3[] corners =
            new Vector3[4];


        piecePanel.GetWorldCorners(
            corners
        );


        Vector3 local =
            safeArea.InverseTransformPoint(
                corners[1]
            );


        return local.y;
    }


    private float GetHeaderBottom()
    {
        Vector3[] corners =
            new Vector3[4];


        headerPanel.GetWorldCorners(
            corners
        );


        Vector3 local =
            safeArea.InverseTransformPoint(
                corners[0]
            );


        return local.y;
    }


    // =====================================================
    // SNAPSHOT
    // =====================================================

    private void SaveLayoutSnapshot()
    {
        lastSafeAreaSize =
            safeArea.rect.size;


        lastPiecePanelTop =
            GetPiecePanelTop();


        lastHeaderBottom =
            GetHeaderBottom();


        hasLayoutSnapshot =
            true;
    }


    private bool HasExternalGeometryChanged()
    {
        if (!hasLayoutSnapshot)
        {
            return true;
        }


        Vector2 currentSafeSize =
            safeArea.rect.size;


        float currentPieceTop =
            GetPiecePanelTop();


        float currentHeaderBottom =
            GetHeaderBottom();


        const float tolerance =
            0.5f;


        if (
            Mathf.Abs(
                currentSafeSize.x -
                lastSafeAreaSize.x
            )
            >
            tolerance
        )
        {
            return true;
        }


        if (
            Mathf.Abs(
                currentSafeSize.y -
                lastSafeAreaSize.y
            )
            >
            tolerance
        )
        {
            return true;
        }


        if (
            Mathf.Abs(
                currentPieceTop -
                lastPiecePanelTop
            )
            >
            tolerance
        )
        {
            return true;
        }


        if (
            Mathf.Abs(
                currentHeaderBottom -
                lastHeaderBottom
            )
            >
            tolerance
        )
        {
            return true;
        }


        return false;
    }
}