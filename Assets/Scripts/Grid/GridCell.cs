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


    private Image cellImage;


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


    private Color currentOccupiedColor;


    private void Awake()
    {
        cellImage =
            GetComponent<Image>();

        currentOccupiedColor =
            occupiedColor;
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


    public void SetState(
        GridCellState newState
    )
    {
        State =
            newState;

        ApplyStateColor();
    }


    public void SetOccupied(
        Color pieceColor
    )
    {
        State =
            GridCellState.Occupied;


        currentOccupiedColor =
            pieceColor;


        ApplyStateColor();
    }


    public void SetEmpty()
    {
        State =
            GridCellState.Empty;

        ApplyStateColor();
    }


    public void SetBlocked()
    {
        State =
            GridCellState.Blocked;

        ApplyStateColor();
    }


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
        ApplyStateColor();
    }


    private void ApplyStateColor()
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
    }


    private void EnsureImage()
    {
        if (cellImage == null)
        {
            cellImage =
                GetComponent<Image>();
        }
    }


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
}