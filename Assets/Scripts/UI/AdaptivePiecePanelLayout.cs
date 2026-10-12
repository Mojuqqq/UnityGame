using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class AdaptivePiecePanelLayout :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RectTransform safeArea;

    [SerializeField]
    private RectTransform piecesContainer;

    [SerializeField]
    private RectTransform activePieceZone;

    [SerializeField]
    private PieceSpawner pieceSpawner;


    [Header("Bottom Area Height")]

    [SerializeField]
    [Range(0.1f, 0.35f)]
    private float heightRatio =
        0.20f;

    [SerializeField]
    [Min(1f)]
    private float minHeight =
        260f;

    [SerializeField]
    [Min(1f)]
    private float maxHeight =
        360f;


    [Header("Margins")]

    [SerializeField]
    [Min(0f)]
    private float leftMargin =
        40f;

    [SerializeField]
    [Min(0f)]
    private float rightMargin =
        40f;

    [SerializeField]
    [Min(0f)]
    private float bottomMargin =
        40f;

    [SerializeField]
    [Min(0f)]
    private float gap =
        24f;


    [Header("Active Piece Zone")]

    [SerializeField]
    [Min(100f)]
    private float activeZoneWidth =
        220f;


    [Header("Piece Panel Padding")]

    [SerializeField]
    [Min(0f)]
    private float horizontalPadding =
        24f;

    [SerializeField]
    [Min(0f)]
    private float verticalPadding =
        20f;


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


        float panelHeight =
            Mathf.Clamp(
                safeHeight *
                heightRatio,

                minHeight,
                maxHeight
            );


        float zoneWidth =
            Mathf.Clamp(
                activeZoneWidth,
                140f,
                safeWidth * 0.32f
            );


        float panelWidth =
            safeWidth
            -
            leftMargin
            -
            rightMargin
            -
            gap
            -
            zoneWidth;


        panelWidth =
            Mathf.Max(
                1f,
                panelWidth
            );


        // =================================================
        // PIECE PANEL
        // =================================================

        panelRect.anchorMin =
            new Vector2(
                0f,
                0f
            );

        panelRect.anchorMax =
            new Vector2(
                0f,
                0f
            );

        panelRect.pivot =
            new Vector2(
                0f,
                0f
            );


        panelRect.sizeDelta =
            new Vector2(
                panelWidth,
                panelHeight
            );


        panelRect.anchoredPosition =
            new Vector2(
                leftMargin,
                bottomMargin
            );


        panelRect.localScale =
            Vector3.one;


        // =================================================
        // ACTIVE PIECE ZONE
        // =================================================

        if (activePieceZone != null)
        {
            activePieceZone.anchorMin =
                new Vector2(
                    0f,
                    0f
                );

            activePieceZone.anchorMax =
                new Vector2(
                    0f,
                    0f
                );

            activePieceZone.pivot =
                new Vector2(
                    0f,
                    0f
                );


            activePieceZone.sizeDelta =
                new Vector2(
                    zoneWidth,
                    panelHeight
                );


            activePieceZone.anchoredPosition =
                new Vector2(
                    leftMargin
                    +
                    panelWidth
                    +
                    gap,

                    bottomMargin
                );


            activePieceZone.localScale =
                Vector3.one;
        }


        // =================================================
        // PIECES CONTAINER
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


        if (pieceSpawner != null)
        {
            pieceSpawner.RefreshLayout();
        }
    }
}