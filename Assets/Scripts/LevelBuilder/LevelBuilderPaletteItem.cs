using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class LevelBuilderPaletteItem :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Preview")]

    [SerializeField]
    private PieceView preview;


    [SerializeField]
    private TMP_Text nameText;


    private LevelBuilderManager manager;

    private PieceDefinition definition;


    private LevelBuilderPieceInstance
        draggedInstance;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        LevelBuilderManager newManager,
        PieceDefinition newDefinition
    )
    {
        manager =
            newManager;


        definition =
            newDefinition;


        if (preview != null)
        {
            preview.Initialize(
                definition
            );


            preview.SetLayout(
                30f,
                3f
            );
        }


        if (nameText != null)
        {
            nameText.text =
                definition != null
                    ? definition.PieceId
                    : "?";
        }
    }


    // =====================================================
    // DRAG
    // =====================================================

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        if (
            manager == null ||
            definition == null
        )
        {
            return;
        }


        draggedInstance =
            manager.CreatePieceFromPalette(
                definition
            );


        if (draggedInstance == null)
        {
            return;
        }


        draggedInstance
            .BeginExternalDrag(
                eventData
            );
    }


    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (draggedInstance == null)
        {
            return;
        }


        draggedInstance
            .ExternalDrag(
                eventData
            );
    }


    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (draggedInstance == null)
        {
            return;
        }


        draggedInstance
            .EndExternalDrag(
                eventData
            );


        draggedInstance =
            null;
    }
}