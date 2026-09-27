using UnityEngine;
using UnityEngine.UI;

public enum GridCellState
{
    Empty,
    Occupied,
    Blocked
}


[RequireComponent(typeof(Image))]
public class GridCell : MonoBehaviour
{
    public int X
    {
        get;
        private set;
    }


    public int Y
    {
        get;
        private set;
    }


    public GridCellState State
    {
        get;
        private set;
    }


    [Header("References")]

    [SerializeField]
    private GameObject blockedVisual;


    [Header("Cell Colors")]

    [SerializeField]
    private Color emptyColor =
        new Color(
            0.25f,
            0.25f,
            0.25f,
            1f
        );


    [SerializeField]
    private Color occupiedColor =
        new Color(
            0.35f,
            0.65f,
            0.35f,
            1f
        );


    [SerializeField]
    private Color blockedColor =
        new Color(
            0.15f,
            0.15f,
            0.15f,
            1f
        );


    private Image cellImage;

    private Color currentOccupiedColor;


    private void Awake()
    {
        cellImage =
            GetComponent<Image>();


        currentOccupiedColor =
            occupiedColor;


        UpdateBlockedVisual();
    }


    public void Initialize(
        int x,
        int y
    )
    {
        X =
            x;

        Y =
            y;


        gameObject.name =
            $"Cell_{x}_{y}";


        SetState(
            GridCellState.Empty
        );
    }


    // =====================================================
    // STATE
    // =====================================================

    public void SetState(
        GridCellState newState
    )
    {
        State =
            newState;


        ApplyStateVisuals();
    }


    public void SetOccupied(
        Color pieceColor
    )
    {
        State =
            GridCellState.Occupied;


        currentOccupiedColor =
            pieceColor;


        ApplyStateVisuals();
    }


    public void SetEmpty()
    {
        State =
            GridCellState.Empty;


        ApplyStateVisuals();
    }


    public void SetBlocked()
    {
        State =
            GridCellState.Blocked;


        ApplyStateVisuals();
    }


    // =====================================================
    // PREVIEW
    // =====================================================

    public void ShowPreview(
        Color previewColor
    )
    {
        EnsureImage();


        cellImage.color =
            previewColor;
    }


    public void ClearPreview()
    {
        ApplyStateVisuals();
    }


    // =====================================================
    // VISUALS
    // =====================================================

    private void ApplyStateVisuals()
    {
        EnsureImage();


        switch (State)
        {
            case GridCellState.Empty:

                cellImage.color =
                    emptyColor;

                break;


            case GridCellState.Occupied:

                cellImage.color =
                    currentOccupiedColor;

                break;


            case GridCellState.Blocked:

                cellImage.color =
                    blockedColor;

                break;
        }


        UpdateBlockedVisual();
    }


    private void UpdateBlockedVisual()
    {
        if (blockedVisual == null)
        {
            return;
        }


        blockedVisual.SetActive(
            State ==
            GridCellState.Blocked
        );
    }


    private void EnsureImage()
    {
        if (cellImage == null)
        {
            cellImage =
                GetComponent<Image>();
        }
    }


    // =====================================================
    // STATE CHECKS
    // =====================================================

    public bool IsEmpty()
    {
        return
            State ==
            GridCellState.Empty;
    }


    public bool IsOccupied()
    {
        return
            State ==
            GridCellState.Occupied;
    }


    public bool IsBlocked()
    {
        return
            State ==
            GridCellState.Blocked;
    }
}