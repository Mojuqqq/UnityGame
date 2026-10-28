using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LevelBuilderCell :
    MonoBehaviour,
    IPointerClickHandler
{
    [Header("Visual")]

    [SerializeField]
    private Image background;


    [SerializeField]
    private TMP_Text markerText;


    [SerializeField]
    private Color normalColor =
        new Color(
            0.12f,
            0.16f,
            0.25f,
            1f
        );


    [SerializeField]
    private Color blockedColor =
        new Color(
            0.45f,
            0.12f,
            0.16f,
            1f
        );


    private LevelBuilderManager manager;

    private Vector2Int coordinate;


    public Vector2Int Coordinate =>
        coordinate;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        LevelBuilderManager newManager,
        Vector2Int newCoordinate
    )
    {
        manager =
            newManager;


        coordinate =
            newCoordinate;


        SetBlocked(
            false
        );
    }


    // =====================================================
    // BLOCKED VISUAL
    // =====================================================

    public void SetBlocked(
        bool blocked
    )
    {
        if (background != null)
        {
            background.color =
                blocked
                    ? blockedColor
                    : normalColor;
        }


        if (markerText != null)
        {
            markerText.text =
                blocked
                    ? "×"
                    : "";
        }
    }


    // =====================================================
    // CLICK
    // =====================================================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (
            manager == null ||
            eventData.button !=
                PointerEventData
                    .InputButton
                    .Left
        )
        {
            return;
        }


        manager.HandleCellClick(
            coordinate
        );
    }
}