using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AdaptivePiecePanelLayout : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RectTransform safeArea;

    [SerializeField]
    private RectTransform piecesContainer;

    [SerializeField]
    private PieceSpawner pieceSpawner;


    [Header("Panel Size")]

    [SerializeField]
    [Range(0.1f, 0.4f)]
    private float heightRatio = 0.22f;

    [SerializeField]
    [Min(1f)]
    private float minHeight = 280f;

    [SerializeField]
    [Min(1f)]
    private float maxHeight = 420f;


    [Header("Outer Margins")]

    [SerializeField]
    [Min(0f)]
    private float horizontalMargin = 40f;

    [SerializeField]
    [Min(0f)]
    private float bottomMargin = 40f;


    [Header("Inner Padding")]

    [SerializeField]
    [Min(0f)]
    private float horizontalPadding = 28f;

    [SerializeField]
    [Min(0f)]
    private float verticalPadding = 24f;


    private RectTransform panelRect;


    private void Awake()
    {
        panelRect =
            GetComponent<RectTransform>();
    }


    public void ApplyLayout()
    {
        if (panelRect == null)
        {
            panelRect =
                GetComponent<RectTransform>();
        }


        if (safeArea == null)
        {
            Debug.LogError(
                "AdaptivePiecePanelLayout: SafeArea is not assigned."
            );

            return;
        }


        if (piecesContainer == null)
        {
            Debug.LogError(
                "AdaptivePiecePanelLayout: PiecesContainer is not assigned."
            );

            return;
        }


        Canvas.ForceUpdateCanvases();


        float safeWidth =
            safeArea.rect.width;


        float safeHeight =
            safeArea.rect.height;


        float panelWidth =
            Mathf.Max(
                1f,
                safeWidth -
                horizontalMargin * 2f
            );


        float panelHeight =
            Mathf.Clamp(
                safeHeight *
                heightRatio,

                minHeight,
                maxHeight
            );


        // =================================================
        // PANEL
        //
        // Прижимаем его к нижней части SafeArea.
        // =================================================

        panelRect.anchorMin =
            new Vector2(
                0.5f,
                0f
            );


        panelRect.anchorMax =
            new Vector2(
                0.5f,
                0f
            );


        panelRect.pivot =
            new Vector2(
                0.5f,
                0f
            );


        panelRect.sizeDelta =
            new Vector2(
                panelWidth,
                panelHeight
            );


        panelRect.anchoredPosition =
            new Vector2(
                0f,
                bottomMargin
            );


        panelRect.localScale =
            Vector3.one;


        // =================================================
        // PIECES CONTAINER
        //
        // Растягиваем внутри Panel,
        // оставляя внутренние отступы.
        // =================================================

        piecesContainer.anchorMin =
            Vector2.zero;


        piecesContainer.anchorMax =
            Vector2.one;


        piecesContainer.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        piecesContainer.offsetMin =
            new Vector2(
                horizontalPadding,
                verticalPadding
            );


        piecesContainer.offsetMax =
            new Vector2(
                -horizontalPadding,
                -verticalPadding
            );


        piecesContainer.localScale =
            Vector3.one;


        Canvas.ForceUpdateCanvases();


        // Если фигуры уже созданы —
        // сразу пересобираем их layout.
        if (pieceSpawner != null)
        {
            pieceSpawner.RefreshLayout();
        }
    }
}